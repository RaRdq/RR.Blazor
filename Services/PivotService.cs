using Microsoft.Extensions.Logging;
using RR.Blazor.Models;
using RR.Blazor.Services.Export;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq.Expressions;

namespace RR.Blazor.Services;

/// <summary>
/// Core service for pivot table data processing and OLAP operations
/// </summary>
public interface IPivotService
{
    /// <summary>
    /// Processes source data into pivot table structure
    /// </summary>
    Task<PivotResult<TItem>> ProcessDataAsync<TItem>(
        IEnumerable<TItem> sourceData,
        PivotConfiguration<TItem> configuration,
        CancellationToken cancellationToken = default) where TItem : class;
    
    /// <summary>
    /// Calculates aggregated values for pivot cells
    /// </summary>
    Task<object> CalculateAggregateAsync<TItem>(
        IEnumerable<TItem> data,
        PivotField<TItem> field,
        AggregationType aggregationType,
        CancellationToken cancellationToken = default) where TItem : class;
    
    /// <summary>
    /// Exports pivot data to specified format
    /// </summary>
    Task<byte[]> ExportAsync<TItem>(
        PivotResult<TItem> pivotResult,
        PivotExportConfiguration exportConfig,
        CancellationToken cancellationToken = default) where TItem : class;
    
    /// <summary>
    /// Gets unique values for a field (for filter dropdowns)
    /// </summary>
    Task<List<object>> GetDistinctValuesAsync<TItem>(
        IEnumerable<TItem> sourceData,
        PivotField<TItem> field,
        CancellationToken cancellationToken = default) where TItem : class;
    
    /// <summary>
    /// Validates pivot configuration
    /// </summary>
    ValidationResult ValidateConfiguration<TItem>(PivotConfiguration<TItem> configuration) where TItem : class;
    
    /// <summary>
    /// Gets performance metrics for the last operation
    /// </summary>
    PivotPerformanceMetrics GetPerformanceMetrics();
}

/// <summary>
/// Implementation of pivot service with caching and performance optimization
/// </summary>
public class PivotService : IPivotService
{
    private readonly ILogger<PivotService> _logger;
    private readonly ConcurrentDictionary<string, object> _cache;
    private readonly TimeSpan _cacheExpiration = TimeSpan.FromMinutes(10);
    private PivotPerformanceMetrics _lastMetrics = new();

    public PivotService(ILogger<PivotService> logger)
    {
        _logger = logger;
        _cache = new ConcurrentDictionary<string, object>();
    }

    public async Task<PivotResult<TItem>> ProcessDataAsync<TItem>(
        IEnumerable<TItem> sourceData,
        PivotConfiguration<TItem> configuration,
        CancellationToken cancellationToken = default) where TItem : class
    {
        var stopwatch = Stopwatch.StartNew();
        var metrics = new PivotPerformanceMetrics();
        
        try
        {
            _logger.LogDebug("Starting pivot data processing for {DataCount} items", sourceData?.Count() ?? 0);
            
            var sourceList = sourceData?.ToList() ?? new List<TItem>();
            metrics.SourceDataCount = sourceList.Count;
            
            // Validate configuration
            var validation = ValidateConfiguration(configuration);
            if (!validation.IsValid)
            {
                throw new InvalidOperationException($"Invalid pivot configuration: {string.Join(", ", validation.Errors)}");
            }
            
            // Check cache
            var cacheKey = GenerateCacheKey(sourceList, configuration);
            if (_cache.TryGetValue(cacheKey, out var cached) && cached is PivotResult<TItem> cachedResult)
            {
                metrics.CacheHits = 1;
                _logger.LogDebug("Using cached pivot result");
                return cachedResult;
            }
            
            metrics.CacheMisses = 1;
            
            // Process data phase
            var dataStopwatch = Stopwatch.StartNew();
            var filteredData = await ApplyFiltersAsync(sourceList, configuration.FilterFields, cancellationToken);
            metrics.DataProcessingTime = dataStopwatch.Elapsed;
            
            // Create pivot structure
            var aggregationStopwatch = Stopwatch.StartNew();
            var pivotResult = await CreatePivotStructureAsync(filteredData, configuration, cancellationToken);
            metrics.AggregationTime = aggregationStopwatch.Elapsed;
            
            // Calculate metrics
            metrics.ProcessedCells = CalculateTotalCells(pivotResult);
            metrics.RenderedCells = metrics.ProcessedCells; // Simplified for now
            metrics.TotalTime = stopwatch.Elapsed;
            metrics.MemoryUsageBytes = GC.GetTotalMemory(false);
            
            _lastMetrics = metrics;
            
            // Cache result
            _cache.TryAdd(cacheKey, pivotResult);
            
            _logger.LogInformation("Pivot processing completed in {ElapsedMs}ms for {Cells} cells", 
                stopwatch.ElapsedMilliseconds, metrics.ProcessedCells);
            
            return pivotResult;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing pivot data");
            throw;
        }
    }

    public async Task<object> CalculateAggregateAsync<TItem>(
        IEnumerable<TItem> data,
        PivotField<TItem> field,
        AggregationType aggregationType,
        CancellationToken cancellationToken = default) where TItem : class
    {
        if (!data.Any()) return null;
        
        try
        {
            var values = data.Select(field.GetValue).Where(v => v != null);
            
            return aggregationType switch
            {
                AggregationType.Count => values.Count(),
                AggregationType.Sum => CalculateSum(values),
                AggregationType.Average => CalculateAverage(values),
                AggregationType.Min => CalculateMin(values),
                AggregationType.Max => CalculateMax(values),
                AggregationType.Custom => await CalculateCustomAggregateAsync(data, field, cancellationToken),
                _ => values.Count()
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating aggregate for field {FieldKey}", field.Key);
            return null;
        }
    }

    public async Task<byte[]> ExportAsync<TItem>(
        PivotResult<TItem> pivotResult,
        PivotExportConfiguration exportConfig,
        CancellationToken cancellationToken = default) where TItem : class
    {
        try
        {
            _logger.LogDebug("Exporting pivot data to {Format}", exportConfig.Format);
            
            return exportConfig.Format switch
            {
                RR.Blazor.Services.Export.ExportFormat.Excel => await ExportToExcelAsync(pivotResult, exportConfig, cancellationToken),
                RR.Blazor.Services.Export.ExportFormat.CSV => await ExportToCsvAsync(pivotResult, exportConfig, cancellationToken),
                RR.Blazor.Services.Export.ExportFormat.PDF => await ExportToPdfAsync(pivotResult, exportConfig, cancellationToken),
                RR.Blazor.Services.Export.ExportFormat.JSON => await ExportToJsonAsync(pivotResult, exportConfig, cancellationToken),
                _ => throw new NotSupportedException($"Export format {exportConfig.Format} is not supported")
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting pivot data");
            throw;
        }
    }

    public async Task<List<object>> GetDistinctValuesAsync<TItem>(
        IEnumerable<TItem> sourceData,
        PivotField<TItem> field,
        CancellationToken cancellationToken = default) where TItem : class
    {
        try
        {
            var distinctValues = sourceData
                .Select(field.GetValue)
                .Where(v => v != null)
                .Distinct()
                .OrderBy(v => v)
                .ToList();
            
            return distinctValues;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting distinct values for field {FieldKey}", field.Key);
            return new List<object>();
        }
    }

    public ValidationResult ValidateConfiguration<TItem>(PivotConfiguration<TItem> configuration) where TItem : class
    {
        var result = new ValidationResult();
        
        try
        {
            // Basic validation
            if (configuration == null)
            {
                result.Errors.Add("Configuration cannot be null");
                return result;
            }
            
            // Must have at least one data field
            if (!configuration.DataFields.Any())
            {
                result.Errors.Add("At least one data field must be specified");
            }
            
            // Check for duplicate field keys
            var allFields = configuration.RowFields
                .Concat(configuration.ColumnFields)
                .Concat(configuration.DataFields)
                .Concat(configuration.FilterFields);
            
            var duplicates = allFields
                .GroupBy(f => f.Key)
                .Where(g => g.Count() > 1)
                .Select(g => g.Key);
            
            if (duplicates.Any())
            {
                result.Errors.Add($"Duplicate field keys found: {string.Join(", ", duplicates)}");
            }
            
            // Validate field configurations
            foreach (var field in allFields)
            {
                if (string.IsNullOrEmpty(field.Key))
                {
                    result.Errors.Add("All fields must have a key");
                }
                
                if (field.Property == null && !field.IsCalculated)
                {
                    result.Errors.Add($"Field {field.Key} must have a property or be marked as calculated");
                }
                
                if (field.IsCalculated && field.CalculationExpression == null)
                {
                    result.Errors.Add($"Calculated field {field.Key} must have a calculation expression");
                }
            }
            
            // Check cell limit
            var estimatedCells = EstimateCellCount(configuration);
            if (estimatedCells > configuration.MaxCells)
            {
                result.Warnings.Add($"Estimated cell count ({estimatedCells:N0}) exceeds maximum ({configuration.MaxCells:N0})");
            }
            
            result.IsValid = !result.Errors.Any();
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating pivot configuration");
            result.Errors.Add($"Validation error: {ex.Message}");
            return result;
        }
    }

    public PivotPerformanceMetrics GetPerformanceMetrics() => _lastMetrics;

    #region Private Methods

    private async Task<List<TItem>> ApplyFiltersAsync<TItem>(
        List<TItem> sourceData,
        List<PivotField<TItem>> filterFields,
        CancellationToken cancellationToken) where TItem : class
    {
        if (!filterFields.Any()) return sourceData;
        
        var filteredData = sourceData.AsEnumerable();
        
        foreach (var filterField in filterFields)
        {
            if (filterField.FilterValues.Any())
            {
                filteredData = filteredData.Where(item =>
                {
                    var value = filterField.GetValue(item);
                    return filterField.FilterValues.Contains(value);
                });
            }
            
            if (filterField.ExcludedValues.Any())
            {
                filteredData = filteredData.Where(item =>
                {
                    var value = filterField.GetValue(item);
                    return !filterField.ExcludedValues.Contains(value);
                });
            }
            
            if (!string.IsNullOrEmpty(filterField.SearchFilter))
            {
                filteredData = filteredData.Where(item =>
                {
                    var value = filterField.GetValue(item)?.ToString() ?? "";
                    return value.Contains(filterField.SearchFilter, StringComparison.OrdinalIgnoreCase);
                });
            }
        }
        
        return filteredData.ToList();
    }

    private async Task<PivotResult<TItem>> CreatePivotStructureAsync<TItem>(
        List<TItem> filteredData,
        PivotConfiguration<TItem> configuration,
        CancellationToken cancellationToken) where TItem : class
    {
        var result = new PivotResult<TItem>
        {
            Configuration = configuration,
            SourceDataCount = filteredData.Count,
            GeneratedAt = DateTime.UtcNow
        };
        
        // Build row hierarchy
        result.RowHeaders = await BuildHierarchyAsync(filteredData, configuration.RowFields, true, cancellationToken);
        
        // Build column hierarchy
        result.ColumnHeaders = await BuildHierarchyAsync(filteredData, configuration.ColumnFields, false, cancellationToken);
        
        // Calculate data cells
        result.DataCells = await CalculateDataCellsAsync(filteredData, result.RowHeaders, result.ColumnHeaders, configuration.DataFields, cancellationToken);
        
        return result;
    }

    private async Task<List<PivotHeader<TItem>>> BuildHierarchyAsync<TItem>(
        List<TItem> data,
        List<PivotField<TItem>> fields,
        bool isRowHeader,
        CancellationToken cancellationToken) where TItem : class
    {
        if (!fields.Any()) return new List<PivotHeader<TItem>> { new() { Value = "Total", Level = 0, IsTotal = true } };
        
        var rootHeaders = new List<PivotHeader<TItem>>();
        
        await BuildHierarchyRecursive(data, fields, 0, null, rootHeaders, isRowHeader, cancellationToken);
        
        if (rootHeaders.Any() && fields.Any(f => f.ShowSubtotals))
        {
            AddSubtotalHeaders(rootHeaders, fields, isRowHeader);
        }
        
        if (fields.Any(f => f.ShowGrandTotal))
        {
            rootHeaders.Add(new PivotHeader<TItem>
            {
                Value = "Grand Total",
                Level = 0,
                IsTotal = true,
                IsRowHeader = isRowHeader,
                DataCount = data.Count
            });
        }
        
        return FlattenHierarchy(rootHeaders);
    }
    
    private async Task BuildHierarchyRecursive<TItem>(
        List<TItem> data,
        List<PivotField<TItem>> fields,
        int level,
        PivotHeader<TItem> parent,
        List<PivotHeader<TItem>> headers,
        bool isRowHeader,
        CancellationToken cancellationToken) where TItem : class
    {
        if (level >= fields.Count || !data.Any()) return;
        
        var field = fields[level];
        var groups = data.GroupBy(item => field.GetValue(item) ?? "(Empty)");
        
        foreach (var group in groups.OrderBy(g => g.Key))
        {
            var header = new PivotHeader<TItem>
            {
                Value = group.Key,
                Field = field,
                Level = level,
                IsRowHeader = isRowHeader,
                DataCount = group.Count(),
                Parent = parent,
                Children = new List<PivotHeader<TItem>>(),
                FormattedValue = field.FormatValue(group.Key)
            };
            
            headers.Add(header);
            
            if (parent != null)
            {
                parent.Children.Add(header);
            }
            
            if (level < fields.Count - 1)
            {
                await BuildHierarchyRecursive(
                    group.ToList(),
                    fields,
                    level + 1,
                    header,
                    header.Children,
                    isRowHeader,
                    cancellationToken);
            }
        }
    }
    
    private void AddSubtotalHeaders<TItem>(List<PivotHeader<TItem>> headers, List<PivotField<TItem>> fields, bool isRowHeader) where TItem : class
    {
        foreach (var header in headers.ToList())
        {
            if (header.Children.Any() && header.Level < fields.Count - 1 && fields[header.Level].ShowSubtotals)
            {
                var subtotalHeader = new PivotHeader<TItem>
                {
                    Value = $"{header.Value} Total",
                    Field = header.Field,
                    Level = header.Level + 1,
                    IsRowHeader = isRowHeader,
                    IsSubtotal = true,
                    Parent = header,
                    DataCount = header.DataCount
                };
                
                header.Children.Add(subtotalHeader);
            }
            
            if (header.Children.Any())
            {
                AddSubtotalHeaders(header.Children, fields, isRowHeader);
            }
        }
    }
    
    private List<PivotHeader<TItem>> FlattenHierarchy<TItem>(List<PivotHeader<TItem>> headers) where TItem : class
    {
        var flattened = new List<PivotHeader<TItem>>();
        
        void Flatten(List<PivotHeader<TItem>> currentHeaders)
        {
            foreach (var header in currentHeaders)
            {
                flattened.Add(header);
                if (header.Children.Any())
                {
                    Flatten(header.Children);
                }
            }
        }
        
        Flatten(headers);
        return flattened;
    }

    private async Task<Dictionary<string, PivotDataCell<TItem>>> CalculateDataCellsAsync<TItem>(
        List<TItem> data,
        List<PivotHeader<TItem>> rowHeaders,
        List<PivotHeader<TItem>> columnHeaders,
        List<PivotField<TItem>> dataFields,
        CancellationToken cancellationToken) where TItem : class
    {
        var cells = new Dictionary<string, PivotDataCell<TItem>>();
        
        foreach (var rowHeader in rowHeaders)
        {
            foreach (var columnHeader in columnHeaders)
            {
                foreach (var dataField in dataFields)
                {
                    var cellKey = GenerateCellKey(rowHeader, columnHeader, dataField);
                    
                    var cellData = FilterDataForCell(data, rowHeader, columnHeader);
                    
                    if (!cellData.Any() && !rowHeader.IsTotal && !rowHeader.IsSubtotal && 
                        !columnHeader.IsTotal && !columnHeader.IsSubtotal)
                    {
                        continue;
                    }
                    
                    var cellValue = await CalculateAggregateAsync(cellData, dataField, dataField.DefaultAggregation, cancellationToken);
                    
                    cells[cellKey] = new PivotDataCell<TItem>
                    {
                        Value = cellValue,
                        FormattedValue = dataField.FormatValue(cellValue),
                        Field = dataField,
                        RowHeader = rowHeader,
                        ColumnHeader = columnHeader,
                        SourceDataCount = cellData.Count(),
                        IsEmpty = cellValue == null || (cellValue is double d && d == 0),
                        IsRowTotal = rowHeader.IsTotal || rowHeader.IsSubtotal,
                        IsColumnTotal = columnHeader.IsTotal || columnHeader.IsSubtotal,
                        IsGrandTotal = rowHeader.IsTotal && columnHeader.IsTotal
                    };
                }
            }
        }
        
        return cells;
    }
    
    private string GenerateCellKey<TItem>(PivotHeader<TItem> rowHeader, PivotHeader<TItem> columnHeader, PivotField<TItem> dataField) where TItem : class
    {
        var rowPath = GetHeaderPath(rowHeader);
        var colPath = GetHeaderPath(columnHeader);
        return $"{rowPath}|{colPath}|{dataField.Key}";
    }
    
    private string GetHeaderPath<TItem>(PivotHeader<TItem> header) where TItem : class
    {
        var path = new List<string>();
        var current = header;
        
        while (current != null)
        {
            path.Insert(0, current.Value?.ToString() ?? "null");
            current = current.Parent;
        }
        
        return string.Join("/", path);
    }

    private List<TItem> FilterDataForCell<TItem>(List<TItem> data, PivotHeader<TItem> rowHeader, PivotHeader<TItem> columnHeader) where TItem : class
    {
        return data.Where(item =>
        {
            var rowMatch = MatchesHeaderHierarchy(item, rowHeader);
            var columnMatch = MatchesHeaderHierarchy(item, columnHeader);
            return rowMatch && columnMatch;
        }).ToList();
    }
    
    private bool MatchesHeaderHierarchy<TItem>(TItem item, PivotHeader<TItem> header) where TItem : class
    {
        if (header.IsTotal) return true;
        
        if (header.IsSubtotal)
        {
            return MatchesHeaderUpToLevel(item, header, header.Level);
        }
        
        var currentHeader = header;
        while (currentHeader != null)
        {
            if (currentHeader.Field != null)
            {
                var itemValue = currentHeader.Field.GetValue(item);
                var headerValue = currentHeader.Value;
                
                if (headerValue == null && itemValue == null)
                {
                    currentHeader = currentHeader.Parent;
                    continue;
                }
                
                if (headerValue == null || itemValue == null)
                    return false;
                
                var itemValueStr = itemValue.ToString();
                var headerValueStr = headerValue.ToString();
                
                if (headerValueStr == "(Empty)" && string.IsNullOrEmpty(itemValueStr))
                {
                    currentHeader = currentHeader.Parent;
                    continue;
                }
                
                if (!itemValue.Equals(headerValue) && itemValueStr != headerValueStr)
                    return false;
            }
            
            currentHeader = currentHeader.Parent;
        }
        
        return true;
    }
    
    private bool MatchesHeaderUpToLevel<TItem>(TItem item, PivotHeader<TItem> header, int maxLevel) where TItem : class
    {
        var currentHeader = header;
        var level = 0;
        
        while (currentHeader != null && level <= maxLevel)
        {
            if (currentHeader.Field != null)
            {
                var itemValue = currentHeader.Field.GetValue(item);
                var headerValue = currentHeader.Value;
                
                if (headerValue != null && itemValue != null)
                {
                    if (!itemValue.Equals(headerValue) && itemValue.ToString() != headerValue.ToString())
                        return false;
                }
                else if (headerValue != itemValue)
                {
                    return false;
                }
            }
            
            currentHeader = currentHeader.Parent;
            level++;
        }
        
        return true;
    }

    private async Task<object> CalculateSubtotalAsync<TItem>(List<TItem> data, PivotField<TItem> field, CancellationToken cancellationToken) where TItem : class
    {
        return await CalculateAggregateAsync(data, field, field.DefaultAggregation, cancellationToken);
    }

    private async Task<object> CalculateCustomAggregateAsync<TItem>(IEnumerable<TItem> data, PivotField<TItem> field, CancellationToken cancellationToken) where TItem : class
    {
        if (field.CustomAggregation == null) return null;
        
        try
        {
            var compiled = field.CustomAggregation.Compile();
            return compiled(data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating custom aggregate for field {FieldKey}", field.Key);
            return null;
        }
    }

    private object CalculateSum(IEnumerable<object> values)
    {
        try
        {
            return values.Sum(v => Convert.ToDouble(v));
        }
        catch
        {
            return 0;
        }
    }

    private object CalculateAverage(IEnumerable<object> values)
    {
        try
        {
            var numericValues = values.Select(v => Convert.ToDouble(v)).ToList();
            return numericValues.Any() ? numericValues.Average() : 0;
        }
        catch
        {
            return 0;
        }
    }

    private object CalculateMin(IEnumerable<object> values)
    {
        try
        {
            return values.Min(v => Convert.ToDouble(v));
        }
        catch
        {
            return null;
        }
    }

    private object CalculateMax(IEnumerable<object> values)
    {
        try
        {
            return values.Max(v => Convert.ToDouble(v));
        }
        catch
        {
            return null;
        }
    }

    private string GenerateCacheKey<TItem>(List<TItem> data, PivotConfiguration<TItem> configuration) where TItem : class
    {
        // Simple cache key based on data hash and configuration
        var dataHash = data.Count.GetHashCode();
        var configHash = configuration.PivotId.GetHashCode();
        return $"pivot_{dataHash}_{configHash}";
    }

    private int CalculateTotalCells<TItem>(PivotResult<TItem> result) where TItem : class
    {
        return result.RowHeaders.Count * result.ColumnHeaders.Count * result.Configuration.DataFields.Count;
    }

    private int EstimateCellCount<TItem>(PivotConfiguration<TItem> configuration) where TItem : class
    {
        // Rough estimation - in real implementation, analyze data cardinality
        var rowEstimate = Math.Max(1, configuration.RowFields.Count * 10);
        var colEstimate = Math.Max(1, configuration.ColumnFields.Count * 10);
        var dataEstimate = Math.Max(1, configuration.DataFields.Count);
        
        return rowEstimate * colEstimate * dataEstimate;
    }

    #endregion

    #region Export Methods

    private async Task<byte[]> ExportToExcelAsync<TItem>(PivotResult<TItem> result, PivotExportConfiguration config, CancellationToken cancellationToken) where TItem : class
    {
        // Note: For full Excel export functionality, use IExportService.ExportPivotDataAsync
        // This method is maintained for backward compatibility only
        _logger.LogWarning("Using legacy Excel export. Consider using IExportService.ExportPivotDataAsync for full provider support.");
        
        // Fallback to CSV format
        return await ExportToCsvAsync(result, config, cancellationToken);
    }

    private async Task<byte[]> ExportToCsvAsync<TItem>(PivotResult<TItem> result, PivotExportConfiguration config, CancellationToken cancellationToken) where TItem : class
    {
        var csv = new System.Text.StringBuilder();
        
        // Add headers
        csv.Append("Row,Column,Value");
        csv.AppendLine();
        
        // Add data
        foreach (var cell in result.DataCells)
        {
            csv.Append($"{cell.Value.RowHeader.Value},{cell.Value.ColumnHeader.Value},{cell.Value.FormattedValue}");
            csv.AppendLine();
        }
        
        return System.Text.Encoding.UTF8.GetBytes(csv.ToString());
    }

    private async Task<byte[]> ExportToPdfAsync<TItem>(PivotResult<TItem> result, PivotExportConfiguration config, CancellationToken cancellationToken) where TItem : class
    {
        // Note: For PDF export functionality, use IExportService.ExportPivotDataAsync with a PDF provider
        _logger.LogWarning("PDF export not available in legacy method. Use IExportService.ExportPivotDataAsync with a PDF provider.");
        throw new NotSupportedException("PDF export requires IExportService with a PDF provider. Use IExportService.ExportPivotDataAsync instead.");
    }

    private async Task<byte[]> ExportToJsonAsync<TItem>(PivotResult<TItem> result, PivotExportConfiguration config, CancellationToken cancellationToken) where TItem : class
    {
        var json = System.Text.Json.JsonSerializer.Serialize(result, new System.Text.Json.JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase
        });
        
        return System.Text.Encoding.UTF8.GetBytes(json);
    }

    #endregion
}