using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace RR.Blazor.Services.Export;

/// <summary>
/// Base interface for export providers
/// </summary>
public interface IExportProvider
{
    /// <summary>
    /// Unique name of the export provider
    /// </summary>
    string Name { get; }
    
    /// <summary>
    /// Priority for provider selection (higher = preferred)
    /// </summary>
    int Priority { get; }
    
    /// <summary>
    /// Supported export formats by this provider
    /// </summary>
    List<ExportFormat> SupportedFormats { get; }
    
    /// <summary>
    /// Export data to specified format
    /// </summary>
    Task<ExportResult> ExportAsync<T>(IEnumerable<T> data, ExportOptions options, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Check if provider can export the given data type and format
    /// </summary>
    bool CanExport<T>(IEnumerable<T> data, ExportFormat format);
    
    /// <summary>
    /// Validate export options before processing
    /// </summary>
    ExportValidationResult ValidateOptions<T>(IEnumerable<T> data, ExportOptions options);
    
    /// <summary>
    /// Get default options for the provider
    /// </summary>
    ExportOptions GetDefaultOptions(ExportFormat format);
    
    /// <summary>
    /// Progress callback for long-running exports
    /// </summary>
    event Action<ExportProgress> ProgressChanged;
}

/// <summary>
/// Extended export provider with batch capabilities
/// </summary>
public interface IBatchExportProvider : IExportProvider
{
    /// <summary>
    /// Export multiple datasets in a single operation
    /// </summary>
    Task<ExportResult> ExportBatchAsync(BatchExportRequest request, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Check if provider supports batch export
    /// </summary>
    bool CanExportBatch(BatchExportRequest request);
}

/// <summary>
/// Stream-based export provider for large datasets
/// </summary>
public interface IStreamingExportProvider : IExportProvider
{
    /// <summary>
    /// Export data as a stream for memory-efficient processing
    /// </summary>
    IAsyncEnumerable<byte[]> ExportStreamAsync<T>(IAsyncEnumerable<T> data, ExportOptions options, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Check if provider supports streaming
    /// </summary>
    bool SupportsStreaming { get; }
}

/// <summary>
/// Template-based export provider
/// </summary>
public interface ITemplateExportProvider : IExportProvider
{
    /// <summary>
    /// Export using a predefined template
    /// </summary>
    Task<ExportResult> ExportWithTemplateAsync<T>(IEnumerable<T> data, string templateName, ExportOptions options, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Get available templates
    /// </summary>
    Task<List<string>> GetAvailableTemplatesAsync();
    
    /// <summary>
    /// Register a new template
    /// </summary>
    Task RegisterTemplateAsync(string templateName, byte[] templateData);
}