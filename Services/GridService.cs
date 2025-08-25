using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using RR.Blazor.Models;
using RR.Blazor.Enums;
using RR.Blazor.Services.Export;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.Json;
using System.Globalization;
using System.Text;
using System.Collections.Concurrent;

namespace RR.Blazor.Services;

/// <summary>
/// Enterprise Grid Service for comprehensive data processing and grid functionality
/// Implements core grid operations without external dependencies (SignalR, Excel)
/// </summary>
public class GridService : IAsyncDisposable
{
    private readonly IJSRuntime _jsRuntime;
    private readonly Dictionary<string, GridPerformanceMetrics> _performanceMetrics = new();
    private readonly ConcurrentDictionary<string, object> _gridCache = new();
    
    public GridService(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }
    
    /// <summary>
    /// Core data processing with filtering, sorting, paging, and grouping
    /// </summary>
    public GridDataResult<T> ProcessData<T>(IEnumerable<T> data, GridState<T> state, GridConfiguration<T> config) where T : class
    {
        var startTime = DateTime.UtcNow;
        var result = new GridDataResult<T>();
        
        try
        {
            var processedData = data?.ToList() ?? new List<T>();
            var originalCount = processedData.Count;
            
            // Apply search if enabled (search term comes from FilterCriteria.GlobalSearch)
            if (!string.IsNullOrEmpty(state.FilterCriteria?.GlobalSearch) && config.EnableSearch)
            {
                processedData = ApplySearch(processedData, state.FilterCriteria.GlobalSearch).ToList();
            }
            
            // Apply filters if available
            if (state.FilterCriteria?.HasActiveFilters == true && config.EnableFiltering)
            {
                processedData = ApplyFilters(processedData, state.FilterCriteria).ToList();
            }
            
            var filteredCount = processedData.Count;
            
            // Apply sorting
            if (state.SortDescriptors?.Any() == true && config.EnableSorting)
            {
                processedData = ApplySorting(processedData, state.SortDescriptors).ToList();
            }
            
            // Apply grouping
            if (state.GroupDescriptors?.Any() == true && config.EnableGrouping)
            {
                // TODO: Implement grouping logic
                result.Metadata["HasGroups"] = true;
            }
            
            // Apply paging
            var totalCount = processedData.Count;
            if (config.EnablePaging && state.PageSize > 0)
            {
                var skip = (state.CurrentPage - 1) * state.PageSize;
                processedData = processedData.Skip(skip).Take(state.PageSize).ToList();
            }
            
            // Build result
            result.Data = processedData;
            result.TotalCount = totalCount;
            result.CurrentPage = state.CurrentPage;
            result.PageSize = state.PageSize;
            result.Success = true;
            result.ProcessingTime = DateTime.UtcNow - startTime;
            result.Metadata["OriginalCount"] = originalCount;
            result.Metadata["FilteredCount"] = filteredCount;
            result.Metadata["ProcessedAt"] = DateTime.UtcNow;
            
            // Track performance
            UpdatePerformanceMetrics(config.GridId, result.ProcessingTime, totalCount, processedData.Count);
        }
        catch (Exception ex)
        {
            result.Success = false;
            result.Error = ex.Message;
            result.ProcessingTime = DateTime.UtcNow - startTime;
        }
        
        return result;
    }
    
    /// <summary>
    /// Export data with basic CSV/JSON support (Excel removed from UI library)
    /// </summary>
    public Task<byte[]> ExportDataAsync<T>(IEnumerable<T> data, ExportConfiguration config, IEnumerable<ColumnDefinition<T>> columns) where T : class
    {
        try
        {
            var exportData = data?.ToList() ?? new List<T>();
            
            // Apply column filtering if specified
            if (config.ColumnsToExport?.Any() == true)
            {
                // TODO: Filter data by selected columns
                // This would require creating new objects with only selected properties
            }
            
            // Convert to CSV as basic export format
            var csv = ConvertToCsv(exportData, columns, config.IncludeHeaders);
            return Task.FromResult(System.Text.Encoding.UTF8.GetBytes(csv));
        }
        catch
        {
            return Task.FromResult(new byte[0]);
        }
    }
    
    private string ConvertToCsv<T>(List<T> data, IEnumerable<ColumnDefinition<T>> columns, bool includeHeaders) where T : class
    {
        var sb = new StringBuilder();
        var columnList = columns?.ToList() ?? new List<ColumnDefinition<T>>();
        
        // Add headers if requested
        if (includeHeaders && columnList.Any())
        {
            var headers = columnList.Select(c => c.Title ?? c.Key).ToArray();
            sb.AppendLine(string.Join(",", headers.Select(h => $"\"{h}\"")));
        }
        
        // Add data rows
        foreach (var item in data)
        {
            var values = new List<string>();
            foreach (var column in columnList)
            {
                var property = typeof(T).GetProperty(column.Key);
                var value = property?.GetValue(item)?.ToString() ?? "";
                values.Add($"\"{value.Replace("\"", "\"\"")}\"");
            }
            sb.AppendLine(string.Join(",", values));
        }
        
        return sb.ToString();
    }
    
    /// <summary>
    /// Get comprehensive performance metrics for a grid
    /// </summary>
    public GridPerformanceMetrics GetPerformanceMetrics(string gridId)
    {
        return _performanceMetrics.TryGetValue(gridId, out var metrics) 
            ? metrics 
            : new GridPerformanceMetrics 
            { 
                TotalRows = 0,
                RenderTime = TimeSpan.Zero,
                LastMeasured = DateTime.UtcNow
            };
    }
    
    /// <summary>
    /// Initialize real-time updates (placeholder - SignalR functionality moved to PayrollAI.Server)
    /// </summary>
    public Task InitializeRealTimeUpdatesAsync<T>(string gridId, GridConfiguration<T> config, Func<object, Task> onUpdate) where T : class
    {
        // Real-time functionality should be implemented in PayrollAI.Server/Client
        // This is a placeholder to maintain interface compatibility
        return Task.CompletedTask;
    }
    
    /// <summary>
    /// Clear grid cache for specific grid or all grids
    /// </summary>
    public Task ClearCacheAsync(string gridId = null)
    {
        if (gridId != null)
        {
            _gridCache.TryRemove(gridId, out _);
            _performanceMetrics.Remove(gridId);
        }
        else
        {
            _gridCache.Clear();
            _performanceMetrics.Clear();
        }
        
        return Task.CompletedTask;
    }
    
    private IEnumerable<T> ApplySearch<T>(IEnumerable<T> data, string searchTerm)
    {
        if (string.IsNullOrEmpty(searchTerm) || !data.Any())
            return data;
            
        var searchLower = searchTerm.ToLowerInvariant();
        var type = typeof(T);
        var stringProperties = type.GetProperties()
            .Where(p => p.PropertyType == typeof(string) && p.CanRead)
            .ToList();
            
        return data.Where(item =>
        {
            return stringProperties.Any(prop =>
            {
                var value = prop.GetValue(item)?.ToString();
                return !string.IsNullOrEmpty(value) && value.ToLowerInvariant().Contains(searchLower);
            });
        });
    }
    
    private IEnumerable<T> ApplyFilters<T>(IEnumerable<T> data, FilterCriteria<T> filterCriteria) where T : class
    {
        var result = data;
        
        // Apply global search
        if (!string.IsNullOrEmpty(filterCriteria.GlobalSearch))
        {
            result = ApplySearch(result, filterCriteria.GlobalSearch);
        }
        
        // Apply column filters
        if (filterCriteria.ColumnFilters?.Any(f => f.IsActive) == true)
        {
            result = ApplyColumnFilters(result, filterCriteria.ColumnFilters);
        }
        
        // Apply custom filter expression
        if (filterCriteria.CustomFilter != null)
        {
            result = result.Where(filterCriteria.CustomFilter.Compile());
        }
        
        return result;
    }
    
    private IEnumerable<T> ApplyColumnFilters<T>(IEnumerable<T> data, List<ColumnFilter> columnFilters)
    {
        var result = data;
        
        foreach (var filter in columnFilters.Where(f => f.IsActive))
        {
            var property = typeof(T).GetProperty(filter.ColumnKey);
            if (property == null) continue;
            
            result = result.Where(item =>
            {
                var value = property.GetValue(item);
                return ApplyColumnFilter(value, filter);
            });
        }
        
        return result;
    }
    
    private bool ApplyColumnFilter(object value, ColumnFilter filter)
    {
        if (value == null && filter.Value == null)
            return true;
            
        if (value == null || filter.Value == null)
            return false;
            
        var stringValue = value.ToString();
        var filterValue = filter.Value.ToString();
        var comparison = filter.IsCaseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;
        
        return filter.Operator switch
        {
            GridFilterOperator.Equals => string.Equals(stringValue, filterValue, comparison),
            GridFilterOperator.NotEquals => !string.Equals(stringValue, filterValue, comparison),
            GridFilterOperator.Contains => stringValue.Contains(filterValue, comparison),
            GridFilterOperator.StartsWith => stringValue.StartsWith(filterValue, comparison),
            GridFilterOperator.EndsWith => stringValue.EndsWith(filterValue, comparison),
            GridFilterOperator.IsEmpty => string.IsNullOrEmpty(stringValue),
            GridFilterOperator.IsNotEmpty => !string.IsNullOrEmpty(stringValue),
            _ => true
        };
    }
    
    private IEnumerable<T> ApplySorting<T>(IEnumerable<T> data, List<SortDescriptor> sortDescriptors)
    {
        if (!sortDescriptors.Any() || !data.Any())
            return data;
            
        IOrderedEnumerable<T> orderedData = null;
        
        // Sort by order to handle multiple sort columns correctly
        var orderedSorts = sortDescriptors.OrderBy(s => s.Order).ToList();
        
        for (int i = 0; i < orderedSorts.Count; i++)
        {
            var sort = orderedSorts[i];
            var property = typeof(T).GetProperty(sort.ColumnKey);
            if (property == null) continue;
            
            if (i == 0)
            {
                orderedData = sort.Direction == GridSortDirection.Ascending
                    ? data.OrderBy(x => property.GetValue(x))
                    : data.OrderByDescending(x => property.GetValue(x));
            }
            else
            {
                orderedData = sort.Direction == GridSortDirection.Ascending
                    ? orderedData.ThenBy(x => property.GetValue(x))
                    : orderedData.ThenByDescending(x => property.GetValue(x));
            }
        }
        
        return orderedData ?? data;
    }
    
    private void UpdatePerformanceMetrics(string gridId, TimeSpan processingTime, int totalRows, int visibleRows)
    {
        if (!_performanceMetrics.ContainsKey(gridId))
        {
            _performanceMetrics[gridId] = new GridPerformanceMetrics();
        }
        
        var metrics = _performanceMetrics[gridId];
        metrics.RenderTime = processingTime;
        metrics.TotalRows = totalRows;
        metrics.VisibleRows = visibleRows;
        metrics.LastMeasured = DateTime.UtcNow;
    }
    
    public async ValueTask DisposeAsync()
    {
        _performanceMetrics.Clear();
        _gridCache.Clear();
        await Task.CompletedTask;
    }
}