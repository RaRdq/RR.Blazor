using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using RR.Blazor.Models;

namespace RR.Blazor.Services.Export;

/// <summary>
/// High-level export service for RR.Blazor components
/// Wraps core export service with component-specific features
/// </summary>
public class ExportService : IExportService
{
    private readonly ICoreExportService coreExportService;
    private readonly ILogger<ExportService> logger;
    
    public event Action<ExportProgress> ExportProgressChanged;
    
    public ExportService(ICoreExportService coreExportService, ILogger<ExportService> logger)
    {
        this.coreExportService = coreExportService;
        this.logger = logger;
        
        // Forward progress events
        coreExportService.ExportProgressChanged += OnProgressChanged;
    }
    
    public async Task<ExportResult> ExportGridDataAsync<TItem>(
        IEnumerable<TItem> data,
        List<GridColumnDefinition<TItem>> columns,
        ExportOptions options,
        CancellationToken cancellationToken = default) where TItem : class
    {
        try
        {
            // Filter and order columns based on visibility and order
            var visibleColumns = columns
                .Where(c => c.Visible)
                .OrderBy(c => c.Order)
                .ToList();
            
            if (!visibleColumns.Any())
            {
                return new ExportResult
                {
                    Success = false,
                    ErrorMessage = "No visible columns to export",
                    Format = options.Format
                };
            }
            
            // Transform data to dynamic objects with only visible columns
            var transformedData = TransformGridData(data, visibleColumns, options);
            
            // Set column mappings for headers
            options.ColumnMappings = visibleColumns.ToDictionary(
                c => c.Key,
                c => c.Title
            );
            
            // Set include columns
            options.IncludeColumns = visibleColumns.Select(c => c.Key).ToList();
            
            // Add custom formatters if columns have them
            options.CustomFormatters = BuildGridFormatters(visibleColumns);
            
            // Set filename if not provided
            if (string.IsNullOrEmpty(options.FileName))
            {
                var extension = GetFileExtension(options.Format);
                options.FileName = $"grid_export_{DateTime.Now:yyyyMMddHHmmss}.{extension}";
            }
            
            logger.LogInformation($"Exporting grid data: {data.Count()} rows, {visibleColumns.Count} columns, format: {options.Format}");
            
            // Use the core export service
            return await coreExportService.ExportAsync(transformedData, options, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Grid export failed");
            return new ExportResult
            {
                Success = false,
                ErrorMessage = $"Grid export failed: {ex.Message}",
                Format = options.Format
            };
        }
    }
    
    public async Task<ExportResult> ExportPivotDataAsync<TItem>(
        PivotResult<TItem> pivotResult,
        ExportOptions options,
        CancellationToken cancellationToken = default) where TItem : class
    {
        try
        {
            // Transform pivot data to flat structure
            var flatData = FlattenPivotData(pivotResult);
            
            // Set filename if not provided
            if (string.IsNullOrEmpty(options.FileName))
            {
                var extension = GetFileExtension(options.Format);
                options.FileName = $"pivot_export_{DateTime.Now:yyyyMMddHHmmss}.{extension}";
            }
            
            logger.LogInformation($"Exporting pivot data: {flatData.Count()} cells, format: {options.Format}");
            
            // Use the core export service
            return await coreExportService.ExportAsync(flatData, options, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Pivot export failed");
            return new ExportResult
            {
                Success = false,
                ErrorMessage = $"Pivot export failed: {ex.Message}",
                Format = options.Format
            };
        }
    }
    
    public void RegisterProvider(IExportProvider provider)
    {
        coreExportService.RegisterExportProvider(provider);
        logger.LogInformation($"Registered export provider: {provider.Name}");
    }
    
    public void UnregisterProvider(string providerName)
    {
        coreExportService.UnregisterExportProvider(providerName);
        logger.LogInformation($"Unregistered export provider: {providerName}");
    }
    
    public List<ExportFormat> GetAvailableFormats<TItem>() where TItem : class
    {
        return coreExportService.GetAllSupportedFormats();
    }
    
    public ExportOptions GetDefaultOptions(ExportFormat format)
    {
        return coreExportService.GetDefaultOptions(format);
    }
    
    public List<ExportFormat> GetSupportedFormats<TItem>(IEnumerable<TItem> data) where TItem : class
    {
        return coreExportService.GetSupportedFormats(data);
    }
    
    public async Task<ExportResult> ExportAsync<TItem>(IEnumerable<TItem> data, ExportOptions options, CancellationToken cancellationToken = default) where TItem : class
    {
        return await coreExportService.ExportAsync(data, options, cancellationToken);
    }
    
    private void OnProgressChanged(ExportProgress progress)
    {
        ExportProgressChanged?.Invoke(progress);
    }
    
    private IEnumerable<dynamic> TransformGridData<TItem>(
        IEnumerable<TItem> data,
        List<GridColumnDefinition<TItem>> columns,
        ExportOptions options) where TItem : class
    {
        var result = new List<dynamic>();
        
        foreach (var item in data.Take(options.MaxRows))
        {
            var expando = new ExpandoObject() as IDictionary<string, object>;
            
            foreach (var column in columns)
            {
                var value = column.GetValue(item);
                
                // Apply column formatting if available
                if (column.Format != null)
                {
                    value = FormatValue(value, column.Format);
                }
                
                expando[column.Key] = value;
            }
            
            result.Add(expando);
        }
        
        return result;
    }
    
    private Dictionary<string, Func<object, string>> BuildGridFormatters<TItem>(
        List<GridColumnDefinition<TItem>> columns) where TItem : class
    {
        var formatters = new Dictionary<string, Func<object, string>>();
        
        foreach (var column in columns)
        {
            if (!string.IsNullOrEmpty(column.Format))
            {
                formatters[column.Key] = value => FormatValue(value, column.Format);
            }
        }
        
        return formatters;
    }
    
    private string FormatValue(object value, string format)
    {
        if (value == null) return string.Empty;
        
        return value switch
        {
            DateTime dt => dt.ToString(format),
            DateTimeOffset dto => dto.ToString(format),
            decimal d => d.ToString(format),
            double dbl => dbl.ToString(format),
            float f => f.ToString(format),
            int i => i.ToString(format),
            long l => l.ToString(format),
            _ => value.ToString()
        };
    }
    
    
    private string GetFileExtension(ExportFormat format)
    {
        return format switch
        {
            ExportFormat.CSV => "csv",
            ExportFormat.Excel => "xlsx",
            ExportFormat.JSON => "json",
            ExportFormat.PDF => "pdf",
            ExportFormat.XML => "xml",
            ExportFormat.TSV => "tsv",
            _ => "txt"
        };
    }
    
    private IEnumerable<dynamic> FlattenPivotData<TItem>(PivotResult<TItem> pivotResult) where TItem : class
    {
        var result = new List<dynamic>();
        
        // Create header row with row headers and column headers
        var rowFieldNames = pivotResult.Configuration.RowFields.Select(f => f.DisplayName ?? f.Key).ToList();
        var columnFieldNames = pivotResult.Configuration.ColumnFields.Select(f => f.DisplayName ?? f.Key).ToList();
        var dataFieldNames = pivotResult.Configuration.DataFields.Select(f => f.DisplayName ?? f.Key).ToList();
        
        // Flatten pivot cells into rows
        foreach (var cell in pivotResult.DataCells.Values)
        {
            var expando = new ExpandoObject() as IDictionary<string, object>;
            
            // Add row header values
            var rowHeader = cell.RowHeader;
            var rowPath = GetHeaderPath(rowHeader);
            for (int i = 0; i < rowFieldNames.Count && i < rowPath.Count; i++)
            {
                expando[rowFieldNames[i]] = rowPath[i];
            }
            
            // Add column header values
            var columnHeader = cell.ColumnHeader;
            var columnPath = GetHeaderPath(columnHeader);
            for (int i = 0; i < columnFieldNames.Count && i < columnPath.Count; i++)
            {
                expando[columnFieldNames[i]] = columnPath[i];
            }
            
            // Add data values
            if (dataFieldNames.Count > 0)
            {
                expando[dataFieldNames[0]] = cell.Value;
            }
            
            result.Add(expando);
        }
        
        return result;
    }
    
    private List<object> GetHeaderPath<TItem>(PivotHeader<TItem> header) where TItem : class
    {
        var path = new List<object>();
        var current = header;
        
        while (current != null && !current.IsTotal && !current.IsSubtotal)
        {
            path.Insert(0, current.Value ?? "(Empty)");
            current = current.Parent;
        }
        
        return path;
    }
}