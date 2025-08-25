using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace RR.Blazor.Services.Export;

/// <summary>
/// Core export service implementation with provider plugin system
/// </summary>
public class CoreExportService : ICoreExportService
{
    private readonly List<IExportProvider> providers = new();
    private readonly ILogger<CoreExportService> logger;
    private readonly SemaphoreSlim exportSemaphore = new(1, 1);
    
    public event Action<ExportProgress> ExportProgressChanged;
    public event Action<ExportResult> ExportCompleted;
    
    public CoreExportService(ILogger<CoreExportService> logger)
    {
        this.logger = logger;
    }
    
    public void RegisterExportProvider(IExportProvider provider)
    {
        if (providers.All(p => p.Name != provider.Name))
        {
            providers.Add(provider);
            provider.ProgressChanged += OnProviderProgressChanged;
            logger.LogInformation($"Registered export provider: {provider.Name}");
        }
    }
    
    public void UnregisterExportProvider(string providerName)
    {
        var provider = providers.FirstOrDefault(p => p.Name == providerName);
        if (provider != null)
        {
            provider.ProgressChanged -= OnProviderProgressChanged;
            providers.Remove(provider);
            logger.LogInformation($"Unregistered export provider: {providerName}");
        }
    }
    
    public async Task<ExportResult> ExportAsync<T>(IEnumerable<T> data, ExportOptions options, CancellationToken cancellationToken = default)
    {
        var startTime = DateTime.UtcNow;
        
        try
        {
            await exportSemaphore.WaitAsync(cancellationToken);
            
            // Validate input
            if (data == null)
            {
                return CreateErrorResult("Data cannot be null", options.Format);
            }
            
            // Find best provider for format
            var provider = FindBestProvider(data, options.Format);
            if (provider == null)
            {
                return CreateErrorResult($"No provider found for format: {options.Format}", options.Format);
            }
            
            // Validate options
            var validation = provider.ValidateOptions(data, options);
            if (!validation.IsValid)
            {
                return CreateErrorResult(string.Join("; ", validation.Errors), options.Format);
            }
            
            // Execute export
            logger.LogInformation($"Starting export with provider: {provider.Name}, Format: {options.Format}");
            var result = await provider.ExportAsync(data, options, cancellationToken);
            
            // Enhance result with timing
            result = result with 
            { 
                ProcessingTime = DateTime.UtcNow - startTime,
                Metadata = result.Metadata ?? new Dictionary<string, object>()
            };
            result.Metadata["ExportProvider"] = provider.Name;
            result.Metadata["ExportDate"] = DateTime.UtcNow;
            
            // Notify completion
            ExportCompleted?.Invoke(result);
            
            if (result.Success)
            {
                logger.LogInformation($"Export completed successfully. Rows: {result.RowCount}, Size: {result.FileSize} bytes, Time: {result.ProcessingTime.TotalMilliseconds}ms");
            }
            else
            {
                logger.LogWarning($"Export failed: {result.ErrorMessage}");
            }
            
            return result;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Export operation failed");
            return CreateErrorResult($"Export failed: {ex.Message}", options.Format);
        }
        finally
        {
            exportSemaphore.Release();
        }
    }
    
    public Task<ExportResult> ExportAsync<T>(IEnumerable<T> data, ExportOptions options, string providerName, CancellationToken cancellationToken = default)
    {
        var provider = GetProvider(providerName);
        if (provider == null)
        {
            return Task.FromResult(CreateErrorResult($"Provider not found: {providerName}", options.Format));
        }
        
        if (!provider.CanExport(data, options.Format))
        {
            return Task.FromResult(CreateErrorResult($"Provider {providerName} cannot export format: {options.Format}", options.Format));
        }
        
        return provider.ExportAsync(data, options, cancellationToken);
    }
    
    public async Task<ExportResult> ExportAsync<T>(IEnumerable<T> data, ExportOptions options, Expression<Func<T, object>>[] includeColumns, CancellationToken cancellationToken = default)
    {
        // Extract column names from expressions
        var columnNames = includeColumns.Select(expr => GetPropertyName(expr)).ToList();
        options.IncludeColumns = columnNames;
        
        return await ExportAsync(data, options, cancellationToken);
    }
    
    public async Task<ExportResult> ExportBatchAsync(BatchExportRequest request, CancellationToken cancellationToken = default)
    {
        // Find provider that supports batch export
        var batchProvider = providers.OfType<IBatchExportProvider>()
            .Where(p => p.CanExportBatch(request))
            .OrderByDescending(p => p.Priority)
            .FirstOrDefault();
        
        if (batchProvider != null)
        {
            return await batchProvider.ExportBatchAsync(request, cancellationToken);
        }
        
        // Fallback to sequential export
        logger.LogInformation("No batch provider found, falling back to sequential export");
        
        var results = new List<ExportResult>();
        foreach (var dataset in request.Datasets)
        {
            var dataType = dataset.Data.GetType();
            var exportMethod = GetType().GetMethod(nameof(ExportAsync), new[] { dataType, typeof(ExportOptions), typeof(CancellationToken) });
            
            if (exportMethod != null)
            {
                var task = (Task<ExportResult>)exportMethod.Invoke(this, new[] { dataset.Data, dataset.Options ?? new ExportOptions { Format = request.Format }, cancellationToken });
                var result = await task;
                results.Add(result);
            }
        }
        
        // Combine results
        return CombineResults(results, request);
    }
    
    public async IAsyncEnumerable<byte[]> ExportStreamAsync<T>(IAsyncEnumerable<T> data, ExportOptions options, CancellationToken cancellationToken = default)
    {
        var streamingProvider = providers.OfType<IStreamingExportProvider>()
            .Where(p => p.SupportsStreaming && p.CanExport(Enumerable.Empty<T>(), options.Format))
            .OrderByDescending(p => p.Priority)
            .FirstOrDefault();
        
        if (streamingProvider == null)
        {
            throw new NotSupportedException($"No streaming provider found for format: {options.Format}");
        }
        
        await foreach (var chunk in streamingProvider.ExportStreamAsync(data, options, cancellationToken))
        {
            yield return chunk;
        }
    }
    
    public List<ExportFormat> GetSupportedFormats<T>(IEnumerable<T> data)
    {
        return providers
            .SelectMany(p => p.SupportedFormats)
            .Where(format => providers.Any(p => p.CanExport(data, format)))
            .Distinct()
            .OrderBy(f => f)
            .ToList();
    }
    
    public List<ExportFormat> GetAllSupportedFormats()
    {
        return providers
            .SelectMany(p => p.SupportedFormats)
            .Distinct()
            .OrderBy(f => f)
            .ToList();
    }
    
    public IReadOnlyList<IExportProvider> GetProviders()
    {
        return providers.AsReadOnly();
    }
    
    public IExportProvider GetProvider(string name)
    {
        return providers.FirstOrDefault(p => p.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
    }
    
    public bool IsFormatSupported(ExportFormat format)
    {
        return providers.Any(p => p.SupportedFormats.Contains(format));
    }
    
    public ExportOptions GetDefaultOptions(ExportFormat format)
    {
        var provider = providers
            .Where(p => p.SupportedFormats.Contains(format))
            .OrderByDescending(p => p.Priority)
            .FirstOrDefault();
        
        return provider?.GetDefaultOptions(format) ?? new ExportOptions { Format = format };
    }
    
    public ExportValidationResult ValidateExport<T>(IEnumerable<T> data, ExportOptions options)
    {
        var provider = FindBestProvider(data, options.Format);
        if (provider == null)
        {
            return new ExportValidationResult
            {
                IsValid = false,
                Errors = { $"No provider found for format: {options.Format}" }
            };
        }
        
        return provider.ValidateOptions(data, options);
    }
    
    private IExportProvider FindBestProvider<T>(IEnumerable<T> data, ExportFormat format)
    {
        return providers
            .Where(p => p.SupportedFormats.Contains(format) && p.CanExport(data, format))
            .OrderByDescending(p => p.Priority)
            .FirstOrDefault();
    }
    
    private void OnProviderProgressChanged(ExportProgress progress)
    {
        ExportProgressChanged?.Invoke(progress);
    }
    
    private static ExportResult CreateErrorResult(string errorMessage, ExportFormat format)
    {
        return new ExportResult
        {
            Success = false,
            ErrorMessage = errorMessage,
            Format = format,
            ProcessingTime = TimeSpan.Zero
        };
    }
    
    private static string GetPropertyName<T>(Expression<Func<T, object>> expression)
    {
        if (expression.Body is MemberExpression memberExpression)
        {
            return memberExpression.Member.Name;
        }
        
        if (expression.Body is UnaryExpression unaryExpression && 
            unaryExpression.Operand is MemberExpression operandMember)
        {
            return operandMember.Member.Name;
        }
        
        throw new ArgumentException("Invalid expression");
    }
    
    private ExportResult CombineResults(List<ExportResult> results, BatchExportRequest request)
    {
        if (!results.Any())
        {
            return CreateErrorResult("No results to combine", request.Format);
        }
        
        var allSuccess = results.All(r => r.Success);
        var totalRows = results.Sum(r => r.RowCount);
        var totalSize = results.Sum(r => r.FileSize);
        var totalTime = TimeSpan.FromMilliseconds(results.Sum(r => r.ProcessingTime.TotalMilliseconds));
        
        return new ExportResult
        {
            Success = allSuccess,
            FileName = request.FileName ?? $"batch_export_{DateTime.Now:yyyyMMddHHmmss}",
            Format = request.Format,
            RowCount = totalRows,
            FileSize = totalSize,
            ProcessingTime = totalTime,
            ErrorMessage = allSuccess ? null : string.Join("; ", results.Where(r => !r.Success).Select(r => r.ErrorMessage)),
            Metadata = new Dictionary<string, object>
            {
                ["BatchCount"] = results.Count,
                ["SuccessCount"] = results.Count(r => r.Success),
                ["FailureCount"] = results.Count(r => !r.Success)
            }
        };
    }
}