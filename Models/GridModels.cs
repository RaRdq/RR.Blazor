using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using RR.Blazor.Enums;
using RR.Blazor.Templates;
using System.Linq.Expressions;

using RR.Blazor.Services.Export;

namespace RR.Blazor.Models;

/// <summary>
/// Enterprise grid configuration with comprehensive features
/// </summary>
public class GridConfiguration<TItem> where TItem : class
{
    // Core Configuration
    public string GridId { get; set; } = Guid.NewGuid().ToString();
    public string Title { get; set; } = string.Empty;
    public string Subtitle { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public bool ShowHeader { get; set; } = true;
    
    // Data Management
    public bool EnablePaging { get; set; } = true;
    public int PageSize { get; set; } = 50;
    public int[] PageSizeOptions { get; set; } = { 10, 25, 50, 100, 250 };
    public bool EnableSorting { get; set; } = true;
    public bool EnableFiltering { get; set; } = true;
    public bool EnableColumnFiltering { get; set; } = false;
    public bool EnableSearch { get; set; } = true;
    
    // Enterprise Features
    public bool EnableGrouping { get; set; } = false;
    public bool EnableMasterDetail { get; set; } = false;
    public bool EnableExport { get; set; } = true;
    public bool EnableColumnReordering { get; set; } = true;
    public bool EnableColumnResizing { get; set; } = true;
    public bool EnableColumnFreeze { get; set; } = true;
    
    // Real-time Features
    public bool EnableRealTimeUpdates { get; set; } = false;
    public string SignalRHubUrl { get; set; } = string.Empty;
    public string SignalRGroup { get; set; } = string.Empty;
    
    // Performance
    public bool EnableVirtualization { get; set; } = false;
    public int VirtualizationThreshold { get; set; } = 1000;
    public bool EnableCaching { get; set; } = true;
    public TimeSpan CacheExpiration { get; set; } = TimeSpan.FromMinutes(5);
    
    // Selection
    public GridSelectionMode SelectionMode { get; set; } = GridSelectionMode.Single;
    public bool EnableKeyboardNavigation { get; set; } = true;
    
    // Styling
    public DensityType Density { get; set; } = DensityType.Normal;
    public bool Striped { get; set; } = true;
    public bool Bordered { get; set; } = false;
    public bool Hover { get; set; } = true;
    
    // Accessibility
    public string AriaLabel { get; set; } = string.Empty;
    public bool EnableHighContrast { get; set; } = false;
    public bool ReducedMotion { get; set; } = false;
    
    // Filter Persistence
    public bool EnableFilterPersistence { get; set; } = false;
}

/// <summary>
/// Grid selection mode options
/// </summary>
public enum GridSelectionMode
{
    None,
    Single,
    Multiple,
    Cell,
    Row,
    Column
}

/// <summary>
/// Column data type for filter operators
/// </summary>
public enum ColumnDataType
{
    Text,
    Number,
    Date,
    Boolean,
    Custom
}

/// <summary>
/// Grid column definition with enterprise features
/// </summary>
public class GridColumnDefinition<TItem> : ColumnDefinition<TItem> where TItem : class
{
    // Data Type
    public ColumnDataType? DataType { get; set; }
    
    // Grouping
    public bool CanGroup { get; set; } = true;
    public int GroupOrder { get; set; } = 0;
    public Func<TItem, object> GroupSelector { get; set; }
    
    // Master-Detail
    public bool IsMasterColumn { get; set; } = false;
    public Expression<Func<TItem, IEnumerable<object>>> DetailSelector { get; set; }
    public Type DetailType { get; set; }
    
    // Advanced Filtering
    public bool EnableAdvancedFilter { get; set; } = true;
    public List<GridFilterOperator> AllowedOperators { get; set; } = new();
    public Func<TItem, object> FilterSelector { get; set; }
    
    // Performance
    public bool IsVirtualized { get; set; } = false;
    public bool IsCalculated { get; set; } = false;
    public Expression<Func<TItem, object>> CalculationExpression { get; set; }
    
    // Real-time Updates
    public bool EnableRealTimeUpdates { get; set; } = true;
    public string UpdateChannel { get; set; } = string.Empty;
    
    // Cell Editing
    public bool IsEditable { get; set; } = false;
    public CellEditMode EditMode { get; set; } = CellEditMode.Inline;
    public RenderFragment<TItem> EditTemplate { get; set; }
    public Func<TItem, object, Task<bool>> OnCellEdit { get; set; }
    
    // Aggregation
    public bool ShowAggregation { get; set; } = false;
    public AggregationType AggregationType { get; set; } = AggregationType.None;
    public RenderFragment<GridAggregationContext<TItem>> AggregationTemplate { get; set; }
}

/// <summary>
/// Filter operators for advanced filtering
/// </summary>
public enum GridFilterOperator
{
    Equals,
    NotEquals,
    Contains,
    NotContains,
    StartsWith,
    EndsWith,
    GreaterThan,
    GreaterThanOrEqual,
    LessThan,
    LessThanOrEqual,
    Between,
    In,
    NotIn,
    IsNull,
    IsNotNull,
    IsEmpty,
    IsNotEmpty
}

/// <summary>
/// Cell edit modes
/// </summary>
public enum CellEditMode
{
    None,
    Inline,
    Modal,
    Popup
}

/// <summary>
/// Aggregation types for column summaries
/// </summary>
public enum AggregationType
{
    None,
    Count,
    Sum,
    Average,
    Min,
    Max,
    Custom
}

/// <summary>
/// Grid state management
/// </summary>
public class GridState<TItem> where TItem : class
{
    // Pagination
    public int CurrentPage { get; set; } = 1;
    public int PageSize { get; set; } = 50;
    public int TotalItems { get; set; } = 0;
    public int TotalPages => (int)Math.Ceiling((double)TotalItems / PageSize);
    
    // Sorting
    public List<SortDescriptor> SortDescriptors { get; set; } = new();
    
    // Filtering
    public FilterCriteria<TItem> FilterCriteria { get; set; } = new();
    
    // Grouping
    public List<GroupDescriptor> GroupDescriptors { get; set; } = new();
    
    // Selection
    public List<TItem> SelectedItems { get; set; } = new();
    public TItem SelectedItem { get; set; }
    
    // Column State
    public Dictionary<string, ColumnState> ColumnStates { get; set; } = new();
    
    // Expanded Details
    public HashSet<object> ExpandedMasterRows { get; set; } = new();
    
    // View State
    public bool IsLoading { get; set; } = false;
    public string LoadingMessage { get; set; } = "Loading...";
    public string ErrorMessage { get; set; } = string.Empty;
    
    // Real-time State
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
    public bool HasPendingUpdates { get; set; } = false;
}

/// <summary>
/// Sort descriptor for multi-column sorting
/// </summary>
public class SortDescriptor
{
    public string ColumnKey { get; set; } = string.Empty;
    public GridSortDirection Direction { get; set; } = GridSortDirection.Ascending;
    public int Order { get; set; } = 0;
}

/// <summary>
/// Grid sort direction
/// </summary>
public enum GridSortDirection
{
    Ascending,
    Descending
}

/// <summary>
/// Group descriptor for data grouping
/// </summary>
public class GroupDescriptor
{
    public string ColumnKey { get; set; } = string.Empty;
    public GridSortDirection Direction { get; set; } = GridSortDirection.Ascending;
    public bool ShowGroupHeader { get; set; } = true;
    public bool ShowGroupFooter { get; set; } = false;
    public bool IsExpanded { get; set; } = true;
    public AggregationType Aggregation { get; set; } = AggregationType.Count;
}

/// <summary>
/// Column state for persistence
/// </summary>
public class ColumnState
{
    public bool IsVisible { get; set; } = true;
    public int Order { get; set; } = 0;
    public string Width { get; set; } = string.Empty;
    public bool IsFrozen { get; set; } = false;
    public FreezeDirection FreezeDirection { get; set; } = FreezeDirection.Left;
}

/// <summary>
/// Freeze direction for column freezing
/// </summary>
public enum FreezeDirection
{
    Left,
    Right
}

/// <summary>
/// Filter criteria with advanced options
/// </summary>
public class FilterCriteria<TItem> where TItem : class
{
    public string GlobalSearch { get; set; } = string.Empty;
    public List<ColumnFilter> ColumnFilters { get; set; } = new();
    public List<GridQuickFilter> QuickFilters { get; set; } = new();
    public GridDateRange DateRange { get; set; }
    public Expression<Func<TItem, bool>> CustomFilter { get; set; }
    
    public bool HasActiveFilters =>
        !string.IsNullOrEmpty(GlobalSearch) ||
        ColumnFilters.Any(f => f.IsActive) ||
        QuickFilters.Any(f => f.IsActive) ||
        DateRange != null ||
        CustomFilter != null;
}

/// <summary>
/// Column-specific filter
/// </summary>
public class ColumnFilter
{
    public string ColumnKey { get; set; } = string.Empty;
    public GridFilterOperator Operator { get; set; } = GridFilterOperator.Equals;
    public object Value { get; set; }
    public object SecondValue { get; set; } // For between operations
    public bool IsActive { get; set; } = false;
    public bool IsCaseSensitive { get; set; } = false;
}

/// <summary>
/// Quick filter for common scenarios - Grid specific
/// </summary>
public class GridQuickFilter
{
    public string Key { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public bool IsActive { get; set; } = false;
    public Expression<Func<object, bool>> FilterExpression { get; set; }
}

/// <summary>
/// Date range filter - Grid specific
/// </summary>
public class GridDateRange
{
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    
    public bool IsValid => StartDate.HasValue || EndDate.HasValue;
    public bool IsComplete => StartDate.HasValue && EndDate.HasValue;
}

/// <summary>
/// Export configuration
/// </summary>
public class ExportConfiguration
{
    public ExportFormat Format { get; set; } = ExportFormat.Excel;
    public string FileName { get; set; } = string.Empty;
    public bool IncludeHeaders { get; set; } = true;
    public bool IncludeFilters { get; set; } = false;
    public bool IncludeGrouping { get; set; } = false;
    public List<string> ColumnsToExport { get; set; } = new();
    public int MaxRows { get; set; } = 100000;
    public bool ExportAllPages { get; set; } = false;
    public string DateFormat { get; set; } = "yyyy-MM-dd";
    public string NumberFormat { get; set; } = "N2";
    public bool CompressOutput { get; set; } = false;
}


/// <summary>
/// Grid aggregation context
/// </summary>
public class GridAggregationContext<TItem> where TItem : class
{
    public IEnumerable<TItem> Items { get; set; } = Enumerable.Empty<TItem>();
    public string ColumnKey { get; set; } = string.Empty;
    public AggregationType AggregationType { get; set; } = AggregationType.None;
    public object AggregatedValue { get; set; }
    public int Count => Items.Count();
}

/// <summary>
/// Real-time update event args
/// </summary>
public class GridUpdateEventArgs<TItem> where TItem : class
{
    public GridUpdateType UpdateType { get; set; }
    public TItem Item { get; set; }
    public TItem OldItem { get; set; }
    public string ColumnKey { get; set; } = string.Empty;
    public object NewValue { get; set; }
    public object OldValue { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Real-time update types
/// </summary>
public enum GridUpdateType
{
    ItemAdded,
    ItemUpdated,
    ItemRemoved,
    CellUpdated,
    BulkUpdate,
    DataRefresh
}

/// <summary>
/// Master-detail configuration
/// </summary>
public class MasterDetailConfiguration<TMaster, TDetail> 
    where TMaster : class 
    where TDetail : class
{
    public Expression<Func<TMaster, IEnumerable<TDetail>>> DetailSelector { get; set; }
    public RenderFragment<TDetail> DetailTemplate { get; set; }
    public GridConfiguration<TDetail> DetailGridConfiguration { get; set; }
    public bool LazyLoadDetails { get; set; } = true;
    public Func<TMaster, Task<IEnumerable<TDetail>>> DetailLoader { get; set; }
    public int MaxDetailRows { get; set; } = 100;
    public bool EnableDetailPaging { get; set; } = true;
}

/// <summary>
/// Grid performance metrics
/// </summary>
public class GridPerformanceMetrics
{
    public TimeSpan RenderTime { get; set; }
    public TimeSpan DataLoadTime { get; set; }
    public TimeSpan FilterTime { get; set; }
    public TimeSpan SortTime { get; set; }
    public int TotalRows { get; set; }
    public int VisibleRows { get; set; }
    public int ColumnCount { get; set; }
    public long MemoryUsage { get; set; }
    public DateTime LastMeasured { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Grid validation context
/// </summary>
public class GridValidationContext<TItem> where TItem : class
{
    public TItem Item { get; set; }
    public string ColumnKey { get; set; } = string.Empty;
    public object Value { get; set; }
    public object OldValue { get; set; }
    public Dictionary<string, string> ValidationErrors { get; set; } = new();
    public bool IsValid => !ValidationErrors.Any();
}

/// <summary>
/// Grid event args for various grid events
/// </summary>
public class GridEventArgs<TItem> where TItem : class
{
    public string GridId { get; set; } = string.Empty;
    public TItem Item { get; set; }
    public string ColumnKey { get; set; } = string.Empty;
    public object Value { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public bool Cancel { get; set; } = false;
}

/// <summary>
/// Grid row event args
/// </summary>
public class GridRowEventArgs<TItem> : GridEventArgs<TItem> where TItem : class
{
    public int RowIndex { get; set; }
    public bool IsSelected { get; set; }
    public MouseEventArgs MouseEventArgs { get; set; }
}

/// <summary>
/// Grid cell event args
/// </summary>
public class GridCellEventArgs<TItem> : GridRowEventArgs<TItem> where TItem : class
{
    public int ColumnIndex { get; set; }
    public bool IsEditing { get; set; }
    public object CellValue { get; set; }
}

/// <summary>
/// Grid SignalR connection statistics
/// </summary>
public class GridConnectionStatistics
{
    public int TotalConnections { get; set; }
    public int ConnectedCount { get; set; }
    public int ConnectingCount { get; set; }
    public int DisconnectedCount { get; set; }
    public int ReconnectingCount { get; set; }
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
    
    public double ConnectedPercentage => TotalConnections > 0 ? (double)ConnectedCount / TotalConnections * 100 : 0;
    public bool HasHealthyConnections => ConnectedCount > 0;
    public bool HasReconnectingConnections => ReconnectingCount > 0;
}

/// <summary>
/// Grid data processing result with comprehensive metadata
/// </summary>
public class GridDataResult<T> where T : class
{
    public List<T> Data { get; set; } = new();
    public int TotalCount { get; set; } = 0;
    public bool Success { get; set; } = true;
    public string Error { get; set; } = string.Empty;
    public TimeSpan ProcessingTime { get; set; }
    public int CurrentPage { get; set; } = 1;
    public int PageSize { get; set; } = 50;
    public int TotalPages => PageSize > 0 ? (int)Math.Ceiling((double)TotalCount / PageSize) : 1;
    public bool HasNextPage => CurrentPage < TotalPages;
    public bool HasPreviousPage => CurrentPage > 1;
    public Dictionary<string, object> Metadata { get; set; } = new();
}