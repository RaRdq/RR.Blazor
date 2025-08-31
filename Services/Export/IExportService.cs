using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using RR.Blazor.Models;

namespace RR.Blazor.Services.Export;

/// <summary>
/// High-level export service interface for RR.Blazor components
/// Provides a simplified API for Grid and Pivot exports
/// </summary>
public interface IExportService
{
    /// <summary>
    /// Export grid data with column definitions
    /// </summary>
    Task<ExportResult> ExportGridDataAsync<TItem>(
        IEnumerable<TItem> data,
        List<GridColumnDefinition<TItem>> columns,
        ExportOptions options,
        CancellationToken cancellationToken = default) where TItem : class;
    
    /// <summary>
    /// Export pivot table data
    /// </summary>
    Task<ExportResult> ExportPivotDataAsync<TItem>(
        PivotResult<TItem> pivotResult,
        ExportOptions options,
        CancellationToken cancellationToken = default) where TItem : class;
    
    /// <summary>
    /// Register a custom export provider
    /// </summary>
    void RegisterProvider(IExportProvider provider);
    
    /// <summary>
    /// Unregister an export provider
    /// </summary>
    void UnregisterProvider(string providerName);
    
    /// <summary>
    /// Get available export formats for a data type
    /// </summary>
    List<ExportFormat> GetAvailableFormats<TItem>() where TItem : class;
    
    /// <summary>
    /// Get default export options for a format
    /// </summary>
    ExportOptions GetDefaultOptions(ExportFormat format);
    
    /// <summary>
    /// Export progress event
    /// </summary>
    event Action<ExportProgress> ExportProgressChanged;
    
    /// <summary>
    /// Get supported formats for data type
    /// </summary>
    List<ExportFormat> GetSupportedFormats<TItem>(IEnumerable<TItem> data) where TItem : class;
    
    /// <summary>
    /// Export data using core service
    /// </summary>
    Task<ExportResult> ExportAsync<TItem>(IEnumerable<TItem> data, ExportOptions options, CancellationToken cancellationToken = default) where TItem : class;
}