using Microsoft.AspNetCore.Components;
using RR.Blazor.Enums;
using RR.Blazor.Templates;
using System.Linq.Expressions;

using RR.Blazor.Services.Export;

namespace RR.Blazor.Models;

/// <summary>
/// Core configuration for RPivotGrid OLAP-style pivot table
/// </summary>
public class PivotConfiguration<TItem> where TItem : class
{
    // Core Configuration
    public string PivotId { get; set; } = Guid.NewGuid().ToString();
    public string Title { get; set; } = string.Empty;
    public string Subtitle { get; set; } = string.Empty;
    public string Icon { get; set; } = "pivot_table_chart";
    public bool ShowHeader { get; set; } = true;
    
    // Field Configuration
    public List<PivotField<TItem>> RowFields { get; set; } = new();
    public List<PivotField<TItem>> ColumnFields { get; set; } = new();
    public List<PivotField<TItem>> DataFields { get; set; } = new();
    public List<PivotField<TItem>> FilterFields { get; set; } = new();
    public List<PivotField<TItem>> AvailableFields { get; set; } = new();
    
    // Drag and Drop
    public bool EnableDragDrop { get; set; } = true;
    public bool AllowFieldReordering { get; set; } = true;
    public bool AllowFieldMovement { get; set; } = true;
    
    // Data Processing
    public bool EnableSubtotals { get; set; } = true;
    public bool EnableGrandTotals { get; set; } = true;
    public bool ShowEmptyRows { get; set; } = false;
    public bool ShowEmptyColumns { get; set; } = false;
    
    // Export and Performance
    public bool EnableExport { get; set; } = true;
    public ExportFormat[] SupportedExportFormats { get; set; } = { ExportFormat.Excel, ExportFormat.CSV, ExportFormat.PDF };
    public int MaxCells { get; set; } = 100000;
    public bool EnableLazyLoading { get; set; } = true;
    public bool PerformanceMode { get; set; } = false;
    public bool EnableVirtualization { get; set; } = true;
    public bool OptimisticUpdates { get; set; } = false;
    public bool BatchUpdates { get; set; } = true;
    
    // Drill-down
    public bool EnableDrillDown { get; set; } = true;
    public bool EnableDrillThrough { get; set; } = true;
    public int MaxDrillDepth { get; set; } = 5;
    
    // Styling
    public DensityType Density { get; set; } = DensityType.Normal;
    public bool ShowGridLines { get; set; } = true;
    public bool AlternateRowColors { get; set; } = true;
    public bool HighlightTotals { get; set; } = true;
    
    // Advanced Features
    public bool EnableCalculatedFields { get; set; } = true;
    public bool EnableConditionalFormatting { get; set; } = true;
    public bool EnableSorting { get; set; } = true;
    public bool EnableFiltering { get; set; } = true;
    
    // Real-time Updates
    public bool EnableRealTimeUpdates { get; set; } = false;
    public string SignalRHubUrl { get; set; } = string.Empty;
    public string SignalRGroup { get; set; } = string.Empty;
}

/// <summary>
/// Pivot field definition for OLAP operations
/// </summary>
public class PivotField<TItem> where TItem : class
{
    // Basic Properties
    public string Key { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public PivotFieldType FieldType { get; set; } = PivotFieldType.Dimension;
    public PivotDataType DataType { get; set; } = PivotDataType.Text;
    
    // Data Access
    public Expression<Func<TItem, object>> Property { get; set; }
    public Func<TItem, object> CompiledProperty { get; set; }
    
    // Aggregation
    public AggregationType DefaultAggregation { get; set; } = AggregationType.Sum;
    public List<AggregationType> SupportedAggregations { get; set; } = new();
    public Expression<Func<IEnumerable<TItem>, object>> CustomAggregation { get; set; }
    
    // Formatting
    public string Format { get; set; } = string.Empty;
    public string NullDisplayText { get; set; } = "(Empty)";
    public Func<object, string> CustomFormatter { get; set; }
    
    // Sorting and Grouping
    public bool AllowSorting { get; set; } = true;
    public bool AllowGrouping { get; set; } = true;
    public PivotSortDirection DefaultSortDirection { get; set; } = PivotSortDirection.Ascending;
    public bool SortByValue { get; set; } = false; // Sort by aggregated values instead of labels
    
    // Filtering
    public bool AllowFiltering { get; set; } = true;
    public List<object> FilterValues { get; set; } = new();
    public List<object> ExcludedValues { get; set; } = new();
    public string SearchFilter { get; set; } = string.Empty;
    
    // Calculated Fields
    public bool IsCalculated { get; set; } = false;
    public Expression<Func<PivotCalculationContext<TItem>, object>> CalculationExpression { get; set; }
    public List<string> DependentFields { get; set; } = new();
    
    // Visual Formatting
    public ConditionalFormatRule[] ConditionalFormats { get; set; } = Array.Empty<ConditionalFormatRule>();
    public string CellClass { get; set; } = string.Empty;
    public Func<object, string> CellClassFunction { get; set; }
    
    // Templates
    public RenderFragment<PivotCellContext<TItem>> CellTemplate { get; set; }
    public RenderFragment<PivotHeaderContext> HeaderTemplate { get; set; }
    
    // Universal Templates
    public BadgeTemplate<PivotCellContext<TItem>> BadgeTemplate { get; set; }
    public CurrencyTemplate<PivotCellContext<TItem>> CurrencyTemplate { get; set; }
    public ProgressTemplate<PivotCellContext<TItem>> ProgressTemplate { get; set; }
    
    // Subtotals and Totals
    public bool ShowSubtotals { get; set; } = true;
    public bool ShowGrandTotal { get; set; } = true;
    public SubtotalPosition SubtotalPosition { get; set; } = SubtotalPosition.Bottom;
    public string SubtotalLabel { get; set; } = "Subtotal";
    
    // Drill-down
    public bool AllowDrillDown { get; set; } = true;
    public bool AllowDrillThrough { get; set; } = true;
    public EventCallback<PivotDrillEventArgs<TItem>> OnDrillDown { get; set; }
    public EventCallback<PivotDrillEventArgs<TItem>> OnDrillThrough { get; set; }
    
    /// <summary>
    /// Gets the value from an item using the property expression
    /// </summary>
    public object GetValue(TItem item)
    {
        if (item == null) return null;
        
        CompiledProperty ??= Property?.Compile();
        return CompiledProperty?.Invoke(item);
    }
    
    /// <summary>
    /// Formats a value for display
    /// </summary>
    public string FormatValue(object value)
    {
        if (value == null) return NullDisplayText;
        
        if (CustomFormatter != null) return CustomFormatter(value);
        
        if (!string.IsNullOrEmpty(Format) && value is IFormattable formattable)
            return formattable.ToString(Format, null);
        
        return value.ToString();
    }
    
    /// <summary>
    /// Creates a copy of this field for different pivot areas
    /// </summary>
    public PivotField<TItem> Clone()
    {
        return new PivotField<TItem>
        {
            Key = Key,
            DisplayName = DisplayName,
            Description = Description,
            FieldType = FieldType,
            DataType = DataType,
            Property = Property,
            CompiledProperty = CompiledProperty,
            DefaultAggregation = DefaultAggregation,
            SupportedAggregations = new List<AggregationType>(SupportedAggregations),
            CustomAggregation = CustomAggregation,
            Format = Format,
            NullDisplayText = NullDisplayText,
            CustomFormatter = CustomFormatter,
            AllowSorting = AllowSorting,
            AllowGrouping = AllowGrouping,
            DefaultSortDirection = DefaultSortDirection,
            SortByValue = SortByValue,
            AllowFiltering = AllowFiltering,
            FilterValues = new List<object>(FilterValues),
            ExcludedValues = new List<object>(ExcludedValues),
            SearchFilter = SearchFilter,
            IsCalculated = IsCalculated,
            CalculationExpression = CalculationExpression,
            DependentFields = new List<string>(DependentFields),
            ConditionalFormats = ConditionalFormats.ToArray(),
            CellClass = CellClass,
            CellClassFunction = CellClassFunction,
            CellTemplate = CellTemplate,
            HeaderTemplate = HeaderTemplate,
            BadgeTemplate = BadgeTemplate,
            CurrencyTemplate = CurrencyTemplate,
            ProgressTemplate = ProgressTemplate,
            ShowSubtotals = ShowSubtotals,
            ShowGrandTotal = ShowGrandTotal,
            SubtotalPosition = SubtotalPosition,
            SubtotalLabel = SubtotalLabel,
            AllowDrillDown = AllowDrillDown,
            AllowDrillThrough = AllowDrillThrough,
            OnDrillDown = OnDrillDown,
            OnDrillThrough = OnDrillThrough
        };
    }
}

/// <summary>
/// Type of pivot field
/// </summary>
public enum PivotFieldType
{
    Dimension,  // Used for grouping and categorization
    Measure,    // Numeric fields for aggregation
    Calculated, // Calculated/computed fields
    Filter      // Fields used only for filtering
}

/// <summary>
/// Data type for pivot field operations
/// </summary>
public enum PivotDataType
{
    Text,
    Number,
    Currency,
    Percentage,
    Date,
    DateTime,
    Time,
    Boolean,
    Custom
}

/// <summary>
/// Sort direction for pivot fields
/// </summary>
public enum PivotSortDirection
{
    Ascending,
    Descending,
    None
}

/// <summary>
/// Position for subtotal display
/// </summary>
public enum SubtotalPosition
{
    Top,
    Bottom,
    Both
}

/// <summary>
/// Context for pivot cell rendering
/// </summary>
public class PivotCellContext<TItem> where TItem : class
{
    public object Value { get; set; }
    public string FormattedValue { get; set; }
    public PivotField<TItem> Field { get; set; }
    public PivotCellType CellType { get; set; }
    public Dictionary<string, object> RowValues { get; set; } = new();
    public Dictionary<string, object> ColumnValues { get; set; } = new();
    public IEnumerable<TItem> SourceData { get; set; } = Enumerable.Empty<TItem>();
    public int RowIndex { get; set; }
    public int ColumnIndex { get; set; }
    public bool IsSubtotal { get; set; }
    public bool IsGrandTotal { get; set; }
    public bool IsEmpty { get; set; }
    public int Level { get; set; } // Hierarchy level for grouping
}

/// <summary>
/// Context for pivot header rendering
/// </summary>
public class PivotHeaderContext
{
    public string Text { get; set; }
    public object Field { get; set; }
    public PivotHeaderType HeaderType { get; set; }
    public int Level { get; set; }
    public int Span { get; set; } = 1;
    public bool IsSorted { get; set; }
    public PivotSortDirection SortDirection { get; set; }
    public bool IsExpanded { get; set; } = true;
    public bool CanDrop { get; set; } = true;
    public string DropZoneClass { get; set; } = string.Empty;
}

/// <summary>
/// Type of pivot cell
/// </summary>
public enum PivotCellType
{
    Data,
    RowHeader,
    ColumnHeader,
    CornerCell,
    Subtotal,
    GrandTotal,
    Empty
}

/// <summary>
/// Type of pivot header
/// </summary>
public enum PivotHeaderType
{
    Row,
    Column,
    Filter,
    Data
}

/// <summary>
/// Calculation context for computed fields
/// </summary>
public class PivotCalculationContext<TItem> where TItem : class
{
    public IEnumerable<TItem> SourceData { get; set; } = Enumerable.Empty<TItem>();
    public Dictionary<string, object> RowValues { get; set; } = new();
    public Dictionary<string, object> ColumnValues { get; set; } = new();
    public Dictionary<string, object> FilterValues { get; set; } = new();
    public Dictionary<string, object> AggregatedValues { get; set; } = new();
    public PivotField<TItem> CurrentField { get; set; }
}

/// <summary>
/// Drill-down/drill-through event arguments
/// </summary>
public class PivotDrillEventArgs<TItem> where TItem : class
{
    public PivotCellContext<TItem> CellContext { get; set; }
    public PivotDrillAction Action { get; set; }
    public Dictionary<string, object> FilterContext { get; set; } = new();
    public bool Cancel { get; set; } = false;
}

/// <summary>
/// Drill action type
/// </summary>
public enum PivotDrillAction
{
    DrillDown,   // Expand hierarchy level
    DrillUp,     // Collapse hierarchy level
    DrillThrough // Navigate to detailed data
}

/// <summary>
/// Conditional formatting rule for pivot cells
/// </summary>
public class ConditionalFormatRule
{
    public string Name { get; set; } = string.Empty;
    public Expression<Func<object, bool>> Condition { get; set; }
    public Func<object, bool> CompiledCondition { get; set; }
    public string CssClass { get; set; } = string.Empty;
    public string BackgroundColor { get; set; } = string.Empty;
    public string TextColor { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public FormatType FormatType { get; set; } = FormatType.BackgroundColor;
    public double? MinValue { get; set; }
    public double? MaxValue { get; set; }
    public string ColorScale { get; set; } = string.Empty; // e.g., "red-yellow-green"
    
    /// <summary>
    /// Evaluates if the rule applies to the given value
    /// </summary>
    public bool Evaluate(object value)
    {
        CompiledCondition ??= Condition?.Compile();
        return CompiledCondition?.Invoke(value) ?? false;
    }
}

/// <summary>
/// Type of conditional formatting
/// </summary>
public enum FormatType
{
    BackgroundColor,
    TextColor,
    Icon,
    DataBar,
    ColorScale,
    IconSet,
    CustomClass
}

/// <summary>
/// Pivot table state for persistence and real-time updates
/// </summary>
public class PivotState<TItem> where TItem : class
{
    // Field Configuration
    public List<PivotFieldState> RowFieldStates { get; set; } = new();
    public List<PivotFieldState> ColumnFieldStates { get; set; } = new();
    public List<PivotFieldState> DataFieldStates { get; set; } = new();
    public List<PivotFieldState> FilterFieldStates { get; set; } = new();
    
    // Data State
    public bool IsLoading { get; set; } = false;
    public string LoadingMessage { get; set; } = "Calculating...";
    public string ErrorMessage { get; set; } = string.Empty;
    public DateTime LastCalculated { get; set; } = DateTime.UtcNow;
    public TimeSpan CalculationTime { get; set; }
    
    // View State
    public Dictionary<string, bool> ExpandedNodes { get; set; } = new();
    public Dictionary<string, PivotSortDirection> SortStates { get; set; } = new();
    public Dictionary<string, List<object>> FilterStates { get; set; } = new();
    
    // Performance Metrics
    public int TotalCells { get; set; }
    public int VisibleCells { get; set; }
    public int DataPoints { get; set; }
    public long MemoryUsage { get; set; }
    
    // Export State
    public ExportConfiguration LastExportConfig { get; set; }
    public DateTime LastExported { get; set; }
}

/// <summary>
/// State for individual pivot fields
/// </summary>
public class PivotFieldState
{
    public string FieldKey { get; set; } = string.Empty;
    public int Order { get; set; }
    public AggregationType Aggregation { get; set; } = AggregationType.Sum;
    public PivotSortDirection SortDirection { get; set; } = PivotSortDirection.None;
    public bool SortByValue { get; set; } = false;
    public List<object> FilterValues { get; set; } = new();
    public List<object> ExcludedValues { get; set; } = new();
    public string SearchFilter { get; set; } = string.Empty;
    public bool ShowSubtotals { get; set; } = true;
    public SubtotalPosition SubtotalPosition { get; set; } = SubtotalPosition.Bottom;
    public bool IsExpanded { get; set; } = true;
}


/// <summary>
/// Event arguments for pivot field operations
/// </summary>
public class PivotFieldEventArgs<TItem> where TItem : class
{
    public PivotField<TItem> Field { get; set; }
    public string SourceArea { get; set; } = string.Empty; // "row", "column", "data", "filter", "available"
    public string TargetArea { get; set; } = string.Empty;
    public int SourceIndex { get; set; } = -1;
    public int TargetIndex { get; set; } = -1;
    public PivotFieldOperation Operation { get; set; }
    public bool Cancel { get; set; } = false;
}

/// <summary>
/// Pivot field operation types
/// </summary>
public enum PivotFieldOperation
{
    Add,
    Remove,
    Move,
    Reorder,
    Configure,
    Sort,
    Filter,
    Aggregate
}

/// <summary>
/// Performance metrics for pivot calculations
/// </summary>
public class PivotPerformanceMetrics
{
    public TimeSpan DataProcessingTime { get; set; }
    public TimeSpan AggregationTime { get; set; }
    public TimeSpan RenderTime { get; set; }
    public TimeSpan TotalTime { get; set; }
    public int SourceDataCount { get; set; }
    public int ProcessedCells { get; set; }
    public int RenderedCells { get; set; }
    public long MemoryUsageBytes { get; set; }
    public int CacheHits { get; set; }
    public int CacheMisses { get; set; }
    public DateTime MeasuredAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Fluent builder for pivot field configuration
/// </summary>
public class PivotFieldBuilder<TItem> where TItem : class
{
    private readonly PivotField<TItem> _field = new();
    
    public PivotFieldBuilder<TItem> WithKey(string key)
    {
        _field.Key = key;
        return this;
    }
    
    public PivotFieldBuilder<TItem> WithDisplayName(string displayName)
    {
        _field.DisplayName = displayName;
        return this;
    }
    
    public PivotFieldBuilder<TItem> WithProperty(Expression<Func<TItem, object>> property)
    {
        _field.Property = property;
        return this;
    }
    
    public PivotFieldBuilder<TItem> AsMeasure(AggregationType defaultAggregation = AggregationType.Sum)
    {
        _field.FieldType = PivotFieldType.Measure;
        _field.DefaultAggregation = defaultAggregation;
        return this;
    }
    
    public PivotFieldBuilder<TItem> AsDimension()
    {
        _field.FieldType = PivotFieldType.Dimension;
        return this;
    }
    
    public PivotFieldBuilder<TItem> WithFormat(string format)
    {
        _field.Format = format;
        return this;
    }
    
    public PivotFieldBuilder<TItem> WithDataType(PivotDataType dataType)
    {
        _field.DataType = dataType;
        return this;
    }
    
    public PivotFieldBuilder<TItem> WithAggregations(params AggregationType[] aggregations)
    {
        _field.SupportedAggregations = aggregations.ToList();
        return this;
    }
    
    public PivotFieldBuilder<TItem> WithConditionalFormat(ConditionalFormatRule rule)
    {
        var formats = _field.ConditionalFormats.ToList();
        formats.Add(rule);
        _field.ConditionalFormats = formats.ToArray();
        return this;
    }
    
    public PivotFieldBuilder<TItem> DisableSubtotals()
    {
        _field.ShowSubtotals = false;
        return this;
    }
    
    public PivotFieldBuilder<TItem> DisableDrillDown()
    {
        _field.AllowDrillDown = false;
        return this;
    }
    
    public PivotField<TItem> Build()
    {
        // Set defaults based on property name if key not set
        if (string.IsNullOrEmpty(_field.Key) && _field.Property != null)
        {
            _field.Key = ExpressionHelper.GetPropertyName(_field.Property);
        }
        
        // Set display name if not set
        if (string.IsNullOrEmpty(_field.DisplayName))
        {
            _field.DisplayName = _field.Key;
        }
        
        return _field;
    }
}

/// <summary>
/// Helper for expression analysis
/// </summary>
internal static class ExpressionHelper
{
    public static string GetPropertyName<T>(Expression<Func<T, object>> expression)
    {
        if (expression.Body is MemberExpression member)
            return member.Member.Name;
        
        if (expression.Body is UnaryExpression unary && unary.Operand is MemberExpression unaryMember)
            return unaryMember.Member.Name;
        
        throw new ArgumentException("Expression must be a property access", nameof(expression));
    }
}

/// <summary>
/// Result of pivot data processing
/// </summary>
public class PivotResult<TItem> where TItem : class
{
    public PivotConfiguration<TItem> Configuration { get; set; } = new();
    public List<PivotHeader<TItem>> RowHeaders { get; set; } = new();
    public List<PivotHeader<TItem>> ColumnHeaders { get; set; } = new();
    public Dictionary<string, PivotDataCell<TItem>> DataCells { get; set; } = new();
    public int SourceDataCount { get; set; }
    public TimeSpan ProcessingTime { get; set; }
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
    public PivotPerformanceMetrics Performance { get; set; } = new();
    public List<string> Warnings { get; set; } = new();
    public List<string> Errors { get; set; } = new();
}

/// <summary>
/// Pivot header for rows and columns
/// </summary>
public class PivotHeader<TItem> where TItem : class
{
    public object Value { get; set; }
    public string FormattedValue { get; set; } = string.Empty;
    public PivotField<TItem> Field { get; set; }
    public int Level { get; set; }
    public bool IsSubtotal { get; set; }
    public bool IsTotal { get; set; }
    public bool IsExpanded { get; set; } = true;
    public bool IsRowHeader { get; set; }
    public int DataCount { get; set; }
    public object SubtotalValue { get; set; }
    public List<PivotHeader<TItem>> Children { get; set; } = new();
    public PivotHeader<TItem> Parent { get; set; }
}

/// <summary>
/// Individual data cell in pivot table
/// </summary>
public class PivotDataCell<TItem> where TItem : class
{
    public object Value { get; set; }
    public string FormattedValue { get; set; } = string.Empty;
    public PivotField<TItem> Field { get; set; }
    public PivotHeader<TItem> RowHeader { get; set; }
    public PivotHeader<TItem> ColumnHeader { get; set; }
    public bool IsEmpty { get; set; }
    public bool IsSubtotal { get; set; }
    public bool IsGrandTotal { get; set; }
    public bool IsRowTotal { get; set; }
    public bool IsColumnTotal { get; set; }
    public int SourceDataCount { get; set; }
    public IEnumerable<TItem> SourceData { get; set; } = Enumerable.Empty<TItem>();
}

/// <summary>
/// Validation result for pivot configuration
/// </summary>
public class ValidationResult
{
    public bool IsValid { get; set; } = true;
    public List<string> Errors { get; set; } = new();
    public List<string> Warnings { get; set; } = new();
}

