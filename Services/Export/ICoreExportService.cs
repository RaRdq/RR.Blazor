using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace RR.Blazor.Services.Export;

/// <summary>
/// Core export service interface for all data export operations
/// </summary>
public interface ICoreExportService
{
    /// <summary>
    /// Register an export provider
    /// </summary>
    void RegisterExportProvider(IExportProvider provider);
    
    /// <summary>
    /// Unregister an export provider
    /// </summary>
    void UnregisterExportProvider(string providerName);
    
    /// <summary>
    /// Export data using the best available provider
    /// </summary>
    Task<ExportResult> ExportAsync<T>(IEnumerable<T> data, ExportOptions options, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Export data using a specific provider
    /// </summary>
    Task<ExportResult> ExportAsync<T>(IEnumerable<T> data, ExportOptions options, string providerName, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Export data with column selection
    /// </summary>
    Task<ExportResult> ExportAsync<T>(IEnumerable<T> data, ExportOptions options, Expression<Func<T, object>>[] includeColumns, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Batch export multiple datasets
    /// </summary>
    Task<ExportResult> ExportBatchAsync(BatchExportRequest request, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Stream export for large datasets
    /// </summary>
    IAsyncEnumerable<byte[]> ExportStreamAsync<T>(IAsyncEnumerable<T> data, ExportOptions options, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Get supported formats for data type
    /// </summary>
    List<ExportFormat> GetSupportedFormats<T>(IEnumerable<T> data);
    
    /// <summary>
    /// Get supported formats across all providers
    /// </summary>
    List<ExportFormat> GetAllSupportedFormats();
    
    /// <summary>
    /// Get registered providers
    /// </summary>
    IReadOnlyList<IExportProvider> GetProviders();
    
    /// <summary>
    /// Get provider by name
    /// </summary>
    IExportProvider GetProvider(string name);
    
    /// <summary>
    /// Check if format is supported
    /// </summary>
    bool IsFormatSupported(ExportFormat format);
    
    /// <summary>
    /// Get default export options for format
    /// </summary>
    ExportOptions GetDefaultOptions(ExportFormat format);
    
    /// <summary>
    /// Validate export request
    /// </summary>
    ExportValidationResult ValidateExport<T>(IEnumerable<T> data, ExportOptions options);
    
    /// <summary>
    /// Export progress event
    /// </summary>
    event Action<ExportProgress> ExportProgressChanged;
    
    /// <summary>
    /// Export completed event
    /// </summary>
    event Action<ExportResult> ExportCompleted;
}

/// <summary>
/// Extended export service with caching capabilities
/// </summary>
public interface ICachedExportService : ICoreExportService
{
    /// <summary>
    /// Export with caching support
    /// </summary>
    Task<ExportResult> ExportWithCacheAsync<T>(IEnumerable<T> data, ExportOptions options, string cacheKey, TimeSpan? cacheDuration = null, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Clear export cache
    /// </summary>
    Task ClearCacheAsync(string cacheKey = null);
    
    /// <summary>
    /// Get cached export if available
    /// </summary>
    Task<ExportResult> GetCachedExportAsync(string cacheKey);
}

/// <summary>
/// Export service with template support
/// </summary>
public interface ITemplateExportService : ICoreExportService
{
    /// <summary>
    /// Export using template
    /// </summary>
    Task<ExportResult> ExportWithTemplateAsync<T>(IEnumerable<T> data, string templateName, ExportOptions options = null, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Register export template
    /// </summary>
    Task RegisterTemplateAsync(string templateName, ExportTemplate template);
    
    /// <summary>
    /// Get available templates
    /// </summary>
    Task<List<ExportTemplate>> GetTemplatesAsync();
}

/// <summary>
/// Export template definition
/// </summary>
public class ExportTemplate
{
    public string Name { get; set; }
    public string Description { get; set; }
    public ExportFormat Format { get; set; }
    public ExportOptions DefaultOptions { get; set; }
    public List<ExportFieldConfig> Fields { get; set; } = new();
    public Dictionary<string, object> Metadata { get; set; } = new();
    public byte[] TemplateData { get; set; }
}