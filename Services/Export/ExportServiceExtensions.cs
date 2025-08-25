using System;
using System.Linq.Expressions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RR.Blazor.Services.Export.Providers;

namespace RR.Blazor.Services.Export;

/// <summary>
/// Extension methods for configuring export services
/// </summary>
public static class ExportServiceExtensions
{
    /// <summary>
    /// Add export services with default providers (internal use)
    /// </summary>
    internal static IServiceCollection AddExportServices(this IServiceCollection services)
    {
        // Register core services
        services.AddSingleton<ICoreExportService, CoreExportService>();
        services.AddSingleton<IExportService, ExportService>();
        
        // Register default providers
        services.AddSingleton<IExportProvider, CsvExportProvider>();
        // ExcelExportProvider removed - not part of UI component library
        services.AddSingleton<IExportProvider, JsonExportProvider>();
        
        // Configure service with providers
        services.AddSingleton(provider =>
        {
            var exportService = provider.GetRequiredService<ICoreExportService>();
            var logger = provider.GetRequiredService<ILogger<CoreExportService>>();
            
            // Auto-register all providers
            foreach (var exportProvider in provider.GetServices<IExportProvider>())
            {
                exportService.RegisterExportProvider(exportProvider);
            }
            
            return exportService;
        });
        
        return services;
    }
    
    /// <summary>
    /// Add export services with configuration (internal use)
    /// </summary>
    internal static IServiceCollection AddExportServices(this IServiceCollection services, Action<ExportServiceBuilder> configure)
    {
        var builder = new ExportServiceBuilder(services);
        configure(builder);
        
        // Register core service
        services.AddSingleton<ICoreExportService>(provider =>
        {
            var logger = provider.GetRequiredService<ILogger<CoreExportService>>();
            var exportService = new CoreExportService(logger);
            
            // Register configured providers
            foreach (var providerType in builder.ProviderTypes)
            {
                var exportProvider = (IExportProvider)ActivatorUtilities.CreateInstance(provider, providerType);
                exportService.RegisterExportProvider(exportProvider);
            }
            
            return exportService;
        });
        
        return services;
    }
    
    /// <summary>
    /// Add a custom export provider
    /// </summary>
    public static IServiceCollection AddExportProvider<TProvider>(this IServiceCollection services) 
        where TProvider : class, IExportProvider
    {
        services.AddSingleton<IExportProvider, TProvider>();
        return services;
    }
    
    /// <summary>
    /// Add cached export service wrapper
    /// </summary>
    public static IServiceCollection AddCachedExportService(this IServiceCollection services)
    {
        services.AddSingleton<ICachedExportService, CachedExportService>();
        return services;
    }
    
    /// <summary>
    /// Add template export service wrapper
    /// </summary>
    public static IServiceCollection AddTemplateExportService(this IServiceCollection services)
    {
        services.AddSingleton<ITemplateExportService, TemplateExportService>();
        return services;
    }
}

/// <summary>
/// Builder for configuring export services
/// </summary>
public class ExportServiceBuilder
{
    private readonly IServiceCollection services;
    internal readonly List<Type> ProviderTypes = new();
    
    public ExportServiceBuilder(IServiceCollection services)
    {
        this.services = services;
    }
    
    /// <summary>
    /// Add CSV export provider
    /// </summary>
    public ExportServiceBuilder AddCsvProvider()
    {
        ProviderTypes.Add(typeof(CsvExportProvider));
        services.AddSingleton<CsvExportProvider>();
        return this;
    }
    
    /// <summary>
    /// Add Excel export provider - REMOVED (not part of UI component library)
    /// </summary>
    public ExportServiceBuilder AddExcelProvider()
    {
        // ExcelExportProvider removed - not part of UI component library
        return this;
    }
    
    /// <summary>
    /// Add JSON export provider
    /// </summary>
    public ExportServiceBuilder AddJsonProvider()
    {
        ProviderTypes.Add(typeof(JsonExportProvider));
        services.AddSingleton<JsonExportProvider>();
        return this;
    }
    
    /// <summary>
    /// Add all default providers
    /// </summary>
    public ExportServiceBuilder AddDefaultProviders()
    {
        AddCsvProvider();
        // AddExcelProvider(); // Removed - not part of UI component library
        AddJsonProvider();
        return this;
    }
    
    /// <summary>
    /// Add custom provider
    /// </summary>
    public ExportServiceBuilder AddProvider<TProvider>() where TProvider : class, IExportProvider
    {
        ProviderTypes.Add(typeof(TProvider));
        services.AddSingleton<TProvider>();
        return this;
    }
    
    /// <summary>
    /// Configure export options defaults
    /// </summary>
    public ExportServiceBuilder ConfigureDefaults(Action<ExportOptions> configure)
    {
        var options = new ExportOptions();
        configure(options);
        services.AddSingleton(options);
        return this;
    }
}

/// <summary>
/// Cached export service implementation
/// </summary>
internal class CachedExportService : ICachedExportService
{
    private readonly ICoreExportService exportService;
    private readonly Dictionary<string, (ExportResult Result, DateTime Expiry)> cache = new();
    private readonly SemaphoreSlim cacheLock = new(1, 1);
    
    public event Action<ExportProgress> ExportProgressChanged
    {
        add => exportService.ExportProgressChanged += value;
        remove => exportService.ExportProgressChanged -= value;
    }
    
    public event Action<ExportResult> ExportCompleted
    {
        add => exportService.ExportCompleted += value;
        remove => exportService.ExportCompleted -= value;
    }
    
    public CachedExportService(ICoreExportService exportService)
    {
        this.exportService = exportService;
    }
    
    public async Task<ExportResult> ExportWithCacheAsync<T>(IEnumerable<T> data, ExportOptions options, string cacheKey, TimeSpan? cacheDuration = null, CancellationToken cancellationToken = default)
    {
        await cacheLock.WaitAsync(cancellationToken);
        try
        {
            // Check cache
            if (cache.TryGetValue(cacheKey, out var cached) && cached.Expiry > DateTime.UtcNow)
            {
                return cached.Result;
            }
            
            // Perform export
            var result = await exportService.ExportAsync(data, options, cancellationToken);
            
            // Cache result
            if (result.Success)
            {
                var expiry = DateTime.UtcNow.Add(cacheDuration ?? TimeSpan.FromMinutes(5));
                cache[cacheKey] = (result, expiry);
            }
            
            return result;
        }
        finally
        {
            cacheLock.Release();
        }
    }
    
    public async Task ClearCacheAsync(string cacheKey = null)
    {
        await cacheLock.WaitAsync();
        try
        {
            if (cacheKey != null)
            {
                cache.Remove(cacheKey);
            }
            else
            {
                cache.Clear();
            }
        }
        finally
        {
            cacheLock.Release();
        }
    }
    
    public async Task<ExportResult> GetCachedExportAsync(string cacheKey)
    {
        await cacheLock.WaitAsync();
        try
        {
            if (cache.TryGetValue(cacheKey, out var cached) && cached.Expiry > DateTime.UtcNow)
            {
                return cached.Result;
            }
            return null;
        }
        finally
        {
            cacheLock.Release();
        }
    }
    
    // Delegate other methods to base service
    public void RegisterExportProvider(IExportProvider provider) => exportService.RegisterExportProvider(provider);
    public void UnregisterExportProvider(string providerName) => exportService.UnregisterExportProvider(providerName);
    public Task<ExportResult> ExportAsync<T>(IEnumerable<T> data, ExportOptions options, CancellationToken cancellationToken = default) 
        => exportService.ExportAsync(data, options, cancellationToken);
    public Task<ExportResult> ExportAsync<T>(IEnumerable<T> data, ExportOptions options, string providerName, CancellationToken cancellationToken = default) 
        => exportService.ExportAsync(data, options, providerName, cancellationToken);
    public Task<ExportResult> ExportAsync<T>(IEnumerable<T> data, ExportOptions options, Expression<Func<T, object>>[] includeColumns, CancellationToken cancellationToken = default) 
        => exportService.ExportAsync(data, options, includeColumns, cancellationToken);
    public Task<ExportResult> ExportBatchAsync(BatchExportRequest request, CancellationToken cancellationToken = default) 
        => exportService.ExportBatchAsync(request, cancellationToken);
    public IAsyncEnumerable<byte[]> ExportStreamAsync<T>(IAsyncEnumerable<T> data, ExportOptions options, CancellationToken cancellationToken = default) 
        => exportService.ExportStreamAsync(data, options, cancellationToken);
    public List<ExportFormat> GetSupportedFormats<T>(IEnumerable<T> data) => exportService.GetSupportedFormats(data);
    public List<ExportFormat> GetAllSupportedFormats() => exportService.GetAllSupportedFormats();
    public IReadOnlyList<IExportProvider> GetProviders() => exportService.GetProviders();
    public IExportProvider GetProvider(string name) => exportService.GetProvider(name);
    public bool IsFormatSupported(ExportFormat format) => exportService.IsFormatSupported(format);
    public ExportOptions GetDefaultOptions(ExportFormat format) => exportService.GetDefaultOptions(format);
    public ExportValidationResult ValidateExport<T>(IEnumerable<T> data, ExportOptions options) => exportService.ValidateExport(data, options);
}

/// <summary>
/// Template export service implementation
/// </summary>
internal class TemplateExportService : ITemplateExportService
{
    private readonly ICoreExportService exportService;
    private readonly Dictionary<string, ExportTemplate> templates = new();
    
    public event Action<ExportProgress> ExportProgressChanged
    {
        add => exportService.ExportProgressChanged += value;
        remove => exportService.ExportProgressChanged -= value;
    }
    
    public event Action<ExportResult> ExportCompleted
    {
        add => exportService.ExportCompleted += value;
        remove => exportService.ExportCompleted -= value;
    }
    
    public TemplateExportService(ICoreExportService exportService)
    {
        this.exportService = exportService;
    }
    
    public async Task<ExportResult> ExportWithTemplateAsync<T>(IEnumerable<T> data, string templateName, ExportOptions options = null, CancellationToken cancellationToken = default)
    {
        if (!templates.TryGetValue(templateName, out var template))
        {
            return new ExportResult
            {
                Success = false,
                ErrorMessage = $"Template not found: {templateName}"
            };
        }
        
        var mergedOptions = options ?? template.DefaultOptions ?? new ExportOptions();
        mergedOptions.Format = template.Format;
        
        // Apply template field configurations
        if (template.Fields?.Any() == true)
        {
            mergedOptions.IncludeColumns = template.Fields.Where(f => f.IsExportable).Select(f => f.FieldName).ToList();
            mergedOptions.ColumnMappings = template.Fields.ToDictionary(f => f.FieldName, f => f.Header ?? f.FieldName);
        }
        
        return await exportService.ExportAsync(data, mergedOptions, cancellationToken);
    }
    
    public Task RegisterTemplateAsync(string templateName, ExportTemplate template)
    {
        templates[templateName] = template;
        return Task.CompletedTask;
    }
    
    public Task<List<ExportTemplate>> GetTemplatesAsync()
    {
        return Task.FromResult(templates.Values.ToList());
    }
    
    // Delegate other methods to base service
    public void RegisterExportProvider(IExportProvider provider) => exportService.RegisterExportProvider(provider);
    public void UnregisterExportProvider(string providerName) => exportService.UnregisterExportProvider(providerName);
    public Task<ExportResult> ExportAsync<T>(IEnumerable<T> data, ExportOptions options, CancellationToken cancellationToken = default) 
        => exportService.ExportAsync(data, options, cancellationToken);
    public Task<ExportResult> ExportAsync<T>(IEnumerable<T> data, ExportOptions options, string providerName, CancellationToken cancellationToken = default) 
        => exportService.ExportAsync(data, options, providerName, cancellationToken);
    public Task<ExportResult> ExportAsync<T>(IEnumerable<T> data, ExportOptions options, Expression<Func<T, object>>[] includeColumns, CancellationToken cancellationToken = default) 
        => exportService.ExportAsync(data, options, includeColumns, cancellationToken);
    public Task<ExportResult> ExportBatchAsync(BatchExportRequest request, CancellationToken cancellationToken = default) 
        => exportService.ExportBatchAsync(request, cancellationToken);
    public IAsyncEnumerable<byte[]> ExportStreamAsync<T>(IAsyncEnumerable<T> data, ExportOptions options, CancellationToken cancellationToken = default) 
        => exportService.ExportStreamAsync(data, options, cancellationToken);
    public List<ExportFormat> GetSupportedFormats<T>(IEnumerable<T> data) => exportService.GetSupportedFormats(data);
    public List<ExportFormat> GetAllSupportedFormats() => exportService.GetAllSupportedFormats();
    public IReadOnlyList<IExportProvider> GetProviders() => exportService.GetProviders();
    public IExportProvider GetProvider(string name) => exportService.GetProvider(name);
    public bool IsFormatSupported(ExportFormat format) => exportService.IsFormatSupported(format);
    public ExportOptions GetDefaultOptions(ExportFormat format) => exportService.GetDefaultOptions(format);
    public ExportValidationResult ValidateExport<T>(IEnumerable<T> data, ExportOptions options) => exportService.ValidateExport(data, options);
}