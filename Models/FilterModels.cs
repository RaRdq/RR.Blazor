using RR.Blazor.Enums;

namespace RR.Blazor.Models;

// =============================================================================
// MODERN FILTER SYSTEM - Primary filter models and definitions
// Legacy event args are in FilterEventArgs.cs for backward compatibility
// =============================================================================

/// <summary>
/// Represents a single filter condition
/// </summary>
public record FilterState(
    string ColumnKey,
    FilterType Type,
    FilterOperator Operator,
    object? Value,
    object? SecondValue = null,
    bool IsActive = true,
    string? DisplayName = null
)
{
    /// <summary>
    /// Gets a display-friendly representation of the filter
    /// </summary>
    public string GetDisplayText()
    {
        var columnName = DisplayName ?? ColumnKey;
        var operatorText = GetOperatorText();
        var valueText = GetValueText();
        
        return $"{columnName} {operatorText} {valueText}";
    }
    
    private string GetOperatorText() => Operator switch
    {
        FilterOperator.Contains => "contains",
        FilterOperator.StartsWith => "starts with",
        FilterOperator.EndsWith => "ends with",
        FilterOperator.Equals => "equals",
        FilterOperator.NotEquals => "does not equal",
        FilterOperator.GreaterThan => ">",
        FilterOperator.LessThan => "<",
        FilterOperator.GreaterThanOrEqual => ">=",
        FilterOperator.LessThanOrEqual => "<=",
        FilterOperator.Between => "between",
        FilterOperator.On => "on",
        FilterOperator.Before => "before",
        FilterOperator.After => "after",
        FilterOperator.IsTrue => "is true",
        FilterOperator.IsFalse => "is false",
        FilterOperator.In => "in",
        FilterOperator.IsEmpty => "is empty",
        FilterOperator.IsNotEmpty => "is not empty",
        _ => Operator.ToString().ToLower()
    };
    
    private string GetValueText()
    {
        if (Value == null) return "null";
        
        return Operator switch
        {
            FilterOperator.Between or FilterOperator.NotBetween when SecondValue != null => 
                $"{Value} and {SecondValue}",
            FilterOperator.IsEmpty or FilterOperator.IsNotEmpty or 
            FilterOperator.IsTrue or FilterOperator.IsFalse => "",
            _ => Value.ToString() ?? ""
        };
    }
}

/// <summary>
/// Represents a group of filter conditions with logical operators
/// </summary>
public record FilterGroup(
    List<FilterState> Filters,
    FilterLogic Logic = FilterLogic.And,
    bool IsActive = true
)
{
    /// <summary>
    /// Gets all active filters in the group
    /// </summary>
    public IEnumerable<FilterState> ActiveFilters => Filters.Where(f => f.IsActive);
    
    /// <summary>
    /// Checks if the group has any active filters
    /// </summary>
    public bool HasActiveFilters => ActiveFilters.Any();
}

/// <summary>
/// Represents a saved filter template
/// </summary>
public record FilterTemplate(
    string Id,
    string Name,
    string? Description,
    FilterGroup FilterGroup,
    DateTime Created,
    string CreatedBy,
    bool IsGlobal = false,
    Dictionary<string, object>? Metadata = null
);

/// <summary>
/// Event arguments for filter changes
/// </summary>
public record FilterEventArgs(
    string ColumnKey,
    FilterState? OldFilter,
    FilterState? NewFilter,
    FilterGroup CurrentFilters,
    bool IsCleared = false
);

/// <summary>
/// Event arguments for filter template operations
/// </summary>
public record FilterTemplateEventArgs(
    string TemplateId,
    FilterTemplate Template,
    string Action // "Save", "Load", "Delete"
);

/// <summary>
/// Configuration for smart filter detection
/// </summary>
public record SmartFilterConfig(
    int SampleSize = 100,
    double TextThreshold = 0.7,
    double NumberThreshold = 0.8,
    double DateThreshold = 0.6,
    double BooleanThreshold = 0.9,
    int MaxSelectOptions = 50,
    bool EnableAutoSuggestions = true,
    Dictionary<string, FilterType>? ColumnTypeOverrides = null
);

/// <summary>
/// Result of smart filter analysis
/// </summary>
public record SmartFilterAnalysis(
    string ColumnKey,
    FilterType RecommendedType,
    List<FilterOperator> SuggestedOperators,
    IEnumerable<object>? SuggestedValues = null,
    double Confidence = 0.0,
    string? Reasoning = null,
    Dictionary<string, object>? Metadata = null
);

/// <summary>
/// Configuration for filter UI behavior
/// </summary>
public record FilterUIConfig(
    bool ShowOperatorSelection = true,
    bool ShowClearButton = true,
    bool ShowQuickFilters = true,
    bool EnableFilterHistory = true,
    bool EnableFilterTemplates = true,
    int MaxRecentFilters = 10,
    string? Placeholder = null,
    Dictionary<FilterOperator, string>? CustomOperatorLabels = null
);

/// <summary>
/// Represents filter options for Select/MultiSelect types
/// </summary>
public record FilterOption(
    object Value,
    string Label,
    bool IsSelected = false,
    string? Description = null,
    string? Group = null,
    bool IsDisabled = false
);

/// <summary>
/// Performance metrics for filtering operations
/// </summary>
public record FilterPerformanceMetrics(
    TimeSpan FilterTime,
    int TotalRows,
    int FilteredRows,
    string ColumnKey,
    FilterOperator Operator,
    DateTime Timestamp
);