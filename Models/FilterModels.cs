using RR.Blazor.Enums;
using System.Linq.Expressions;

namespace RR.Blazor.Models;

/// <summary>
/// Universal filter configuration for smart components
/// </summary>
public class UniversalFilterConfig
{
    // Basic Display
    public bool ShowSearch { get; set; } = true;
    public string SearchPlaceholder { get; set; } = "Search...";
    public bool ShowQuickFilters { get; set; } = true;
    public bool ShowAdvancedPanel { get; set; } = true;
    public bool ShowClearButton { get; set; } = true;
    public bool ShowFilterCount { get; set; } = true;
    public bool ShowFilterChips { get; set; } = true;
    public bool ShowDateRange { get; set; } = false;
    
    // Layout & Density
    public DensityType Density { get; set; } = DensityType.Normal;
    public bool Minimal { get; set; } = false;
    public bool Compact { get; set; } = false;
    
    // Behavior
    public bool EnableRealTime { get; set; } = true;
    public bool EnableRealTimeFiltering => EnableRealTime; // Alias for compatibility
    public int DebounceMs { get; set; } = 300;
    public bool AllowMultipleFilters { get; set; } = true;
    public FilterLogic DefaultLogic { get; set; } = FilterLogic.And;
    
    // Performance
    public int VirtualScrollThreshold { get; set; } = 1000;
    public bool EnableVirtualScrolling { get; set; } = true;
    
    // Persistence
    public bool EnablePersistence { get; set; } = false;
    public string PersistenceKey { get; set; } = string.Empty;
}

/// <summary>
/// Universal filter criteria that works with any data type
/// </summary>
public class UniversalFilterCriteria<TItem> where TItem : class
{
    public string SearchTerm { get; set; } = string.Empty;
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
    public List<FilterCondition> Filters { get; set; } = new();
    public FilterLogic Logic { get; set; } = FilterLogic.And;
    
    // Compatibility properties
    public (DateTime? From, DateTime? To) DateRange 
    { 
        get => (DateFrom, DateTo); 
        set => (DateFrom, DateTo) = value; 
    }
    public List<QuickFilterState> QuickFilters { get; set; } = new();
}

/// <summary>
/// Individual filter condition
/// </summary>
public class FilterCondition
{
    public string ColumnKey { get; set; } = string.Empty;
    public FilterType Type { get; set; } = FilterType.Text;
    public FilterOperator Operator { get; set; } = FilterOperator.Contains;
    public object? Value { get; set; }
    public object? SecondValue { get; set; } // For range operators
    public bool IsActive { get; set; } = false;
    public bool IsCaseSensitive { get; set; } = false;
    
    public FilterCondition() { }
    
    public FilterCondition(string columnKey, FilterType type, FilterOperator op, object? value, object? secondValue = null, bool isActive = true)
    {
        ColumnKey = columnKey;
        Type = type;
        Operator = op;
        Value = value;
        SecondValue = secondValue;
        IsActive = isActive;
    }
}

/// <summary>
/// Column definition for filters
/// </summary>
public record FilterColumnDefinition(
    string Key,
    string DisplayName,
    FilterType Type,
    bool IsFilterable,
    bool IsSearchable,
    bool IsSortable,
    List<FilterOperator> SupportedOperators,
    List<object>? UniqueValues = null,
    object? MinValue = null,
    object? MaxValue = null,
    string? Format = null
);

/// <summary>
/// Quick filter state for toggle buttons
/// </summary>
public class QuickFilterState
{
    public string Key { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public bool IsActive { get; set; } = false;
    public Expression<Func<object, bool>>? FilterExpression { get; set; }
    public string Description { get; set; } = string.Empty;
}

/// <summary>
/// Filter state change event arguments
/// </summary>
public class FilterStateChangedEventArgs
{
    public string FilterId { get; set; } = string.Empty;
    public FilterChangeType ChangeType { get; set; }
    public bool HasActiveFilters { get; set; }
    public int ActiveFilterCount { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public Dictionary<string, object> Filters { get; set; } = new();
    public Expression<Func<object, bool>> Predicate { get; set; }
}

/// <summary>
/// Types of filter changes
/// </summary>
public enum FilterChangeType
{
    ConditionsChanged,
    QuickFilterToggled,
    SearchChanged,
    DateRangeChanged,
    FiltersCleared,
    TemplateLoaded
}


/// <summary>
/// Filter result containing filtered data and metadata
/// </summary>
public class FilterResult<T>
{
    public IEnumerable<T> FilteredData { get; set; } = Enumerable.Empty<T>();
    public IEnumerable<T> Data => FilteredData; // Alias for compatibility
    public int TotalCount { get; set; }
    public int FilteredCount { get; set; }
    public bool HasActiveFilters { get; set; }
    public Dictionary<string, object> Metadata { get; set; } = new();
}

/// <summary>
/// Column filter state for individual columns
/// </summary>
public class RColumnFilterState
{
    public string ColumnKey { get; set; } = string.Empty;
    public FilterOperator Operator { get; set; } = FilterOperator.Contains;
    public object? Value { get; set; }
    public object? SecondValue { get; set; } // For range operators
    public bool IsActive { get; set; } = false;
    public FilterType FilterType { get; set; } = FilterType.Text;
    public string QuickFilterValue { get; set; } = string.Empty;
}

/// <summary>
/// Advanced filter state for complex filtering scenarios
/// </summary>
public class AdvancedFilterState
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string ColumnKey { get; set; } = string.Empty;
    public FilterOperator Operator { get; set; } = FilterOperator.Contains;
    public object? Value { get; set; }
    public object? SecondValue { get; set; }
    public bool IsActive { get; set; } = false;
    public FilterType FilterType { get; set; } = FilterType.Text;
    public FilterType Type => FilterType; // Alias for compatibility
    public string Label { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Grid filter state for comprehensive grid filtering
/// </summary>
public class GridFilterState
{
    public string SearchTerm { get; set; } = string.Empty;
    public List<AdvancedFilterState> Filters { get; set; } = new();
    public Dictionary<string, RColumnFilterState> ColumnFilters { get; set; } = new();
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
    public FilterLogic Logic { get; set; } = FilterLogic.And;
    public string GridId { get; set; } = string.Empty;
    public DateTime LastSaved { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Saved filter configuration
/// </summary>
public class FilterConfiguration
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string ColumnKey { get; set; } = string.Empty; // For single-column configurations
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime LastModified { get; set; } = DateTime.UtcNow;
    public Dictionary<string, RColumnFilterState> ColumnFilters { get; set; } = new();
    public List<AdvancedFilterState> Filters { get; set; } = new();
    public string SearchTerm { get; set; } = string.Empty;
    public bool IsDefault { get; set; } = false;
}