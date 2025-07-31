using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.Extensions.Logging;
using RR.Blazor.Enums;
using RR.Blazor.Models;
using System.Linq.Expressions;
using System.Reflection;

namespace RR.Blazor.Components.Data;


/// <summary>
/// Base class for RColumn components with shared parameters
/// </summary>
public abstract class RColumnBase : ComponentBase
{
    #region Core Configuration
    [Parameter] public string Key { get; set; } = "";
    [Parameter] public string Header { get; set; }
    [Parameter] public string Format { get; set; }
    [Parameter] public bool Sortable { get; set; }
    [Parameter] public bool Filterable { get; set; }
    [Parameter] public FilterType FilterType { get; set; } = FilterType.Auto;
    [Parameter] public string Width { get; set; }
    [Parameter] public string HeaderClass { get; set; }
    [Parameter] public string CellClass { get; set; }
    [Parameter] public string Class { get; set; }
    [Parameter] public bool Sticky { get; set; }
    [Parameter] public bool Visible { get; set; } = true;
    [Parameter] public RenderFragment HeaderTemplate { get; set; }
    #endregion

    #region Advanced Filtering Configuration
    [Parameter] public List<FilterOperator> FilterOperators { get; set; } = new();
    [Parameter] public object FilterMinValue { get; set; }
    [Parameter] public object FilterMaxValue { get; set; }
    [Parameter] public IEnumerable<object> FilterOptions { get; set; }
    [Parameter] public string FilterPlaceholder { get; set; }
    [Parameter] public bool FilterShowOperatorSelection { get; set; } = true;
    [Parameter] public bool FilterShowClearButton { get; set; } = true;
    [Parameter] public Func<object, string> FilterDisplaySelector { get; set; }
    [Parameter] public Func<object, string> FilterValueSelector { get; set; }
    [Parameter] public RenderFragment<object> FilterTemplate { get; set; }
    #endregion

    #region Column Management Features
    [Parameter] public bool Resizable { get; set; } = true;
    [Parameter] public string MinWidth { get; set; } = "50px";
    [Parameter] public string MaxWidth { get; set; } = "1000px";
    [Parameter] public StickyPosition StickyPosition { get; set; } = StickyPosition.Left;
    [Parameter] public int Order { get; set; }
    [Parameter] public bool Hideable { get; set; } = true;
    #endregion

    protected void ForwardBaseParameters(RenderTreeBuilder builder)
    {
        builder.AddAttribute(1, nameof(Key), Key);
        builder.AddAttribute(2, nameof(Header), Header);
        builder.AddAttribute(3, nameof(Format), Format);
        builder.AddAttribute(4, nameof(Sortable), Sortable);
        builder.AddAttribute(5, nameof(Filterable), Filterable);
        builder.AddAttribute(6, nameof(FilterType), FilterType);
        builder.AddAttribute(7, nameof(Width), Width);
        builder.AddAttribute(8, nameof(HeaderClass), HeaderClass);
        builder.AddAttribute(9, nameof(CellClass), CellClass);
        builder.AddAttribute(10, nameof(Class), Class);
        builder.AddAttribute(11, nameof(Sticky), Sticky);
        builder.AddAttribute(12, nameof(Visible), Visible);
        builder.AddAttribute(13, nameof(HeaderTemplate), HeaderTemplate);
        
        // Column management parameters
        builder.AddAttribute(14, nameof(Resizable), Resizable);
        builder.AddAttribute(15, nameof(MinWidth), MinWidth);
        builder.AddAttribute(16, nameof(MaxWidth), MaxWidth);
        builder.AddAttribute(17, nameof(StickyPosition), StickyPosition);
        builder.AddAttribute(18, nameof(Order), Order);
        builder.AddAttribute(19, nameof(Hideable), Hideable);
    }
}

/// <summary>
/// Smart RColumn wrapper with automatic type detection via cascading context.
/// Usage: <RColumn Property="@(p => p.Name)" Header="Product Name" />
/// </summary>
public class RColumn : RColumnBase
{
    [CascadingParameter] public TableContext TableContext { get; set; }
    [CascadingParameter] public ITableParent ParentTable { get; set; }
    [Inject] private ILogger<RColumn> Logger { get; set; }
    
    [Parameter] public LambdaExpression Property { get; set; }
    [Parameter] public object Template { get; set; }

    private bool _isRegistered = false;
    
    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        // When RColumn is used inside ColumnsContent, it should render a header cell
        // When used elsewhere, it just registers with the parent table
        if (ParentTable != null)
        {
            // Check if we should render as a header cell (when used in ColumnsContent)
            // This is determined by context - if we're in a table header context, render
            var shouldRenderHeader = TableContext?.IsHeaderContext ?? false;
            
            if (shouldRenderHeader)
            {
                // Render as table header cell
                builder.OpenElement(0, "th");
                builder.AddAttribute(1, "class", $"table-header-cell {HeaderClass} {(Sortable ? "table-header-sortable cursor-pointer hover:text-interactive transition-all" : "")}");
                
                if (HeaderTemplate != null)
                {
                    builder.AddContent(2, HeaderTemplate);
                }
                else
                {
                    builder.OpenElement(3, "div");
                    builder.AddAttribute(4, "class", "d-flex items-center justify-between gap-2");
                    
                    builder.OpenElement(5, "div");
                    builder.AddAttribute(6, "class", "d-flex items-center gap-2");
                    builder.AddContent(7, Header ?? GetPropertyName());
                    builder.CloseElement(); // inner div
                    
                    builder.CloseElement(); // outer div
                }
                
                builder.CloseElement(); // th
            }
        }
        
        // Registration happens in OnParametersSet after cascading parameters are available
    }

    private void RegisterWithParentTable()
    {
        if (ParentTable == null) return;
        
        // Create a proper RDataTableColumn with reflection handling
        var columnInfo = new Dictionary<string, object>();
        
        // Only add non-null values to avoid issues
        var keyValue = !string.IsNullOrEmpty(Key) ? Key : GetPropertyName();
        if (!string.IsNullOrEmpty(keyValue))
            columnInfo["Key"] = keyValue;
            
        var headerValue = !string.IsNullOrEmpty(Header) ? Header : GetPropertyName();
        if (!string.IsNullOrEmpty(headerValue))
            columnInfo["Header"] = headerValue;
        if (!string.IsNullOrEmpty(Format))
            columnInfo["Format"] = Format;
        columnInfo["Sortable"] = Sortable;
        columnInfo["Filterable"] = Filterable;
        columnInfo["FilterType"] = FilterType;
        if (FilterOperators?.Any() == true)
            columnInfo["FilterOperators"] = FilterOperators;
        if (FilterMinValue != null)
            columnInfo["FilterMinValue"] = FilterMinValue;
        if (FilterMaxValue != null)
            columnInfo["FilterMaxValue"] = FilterMaxValue;
        if (FilterOptions != null)
            columnInfo["FilterOptions"] = FilterOptions;
        if (!string.IsNullOrEmpty(FilterPlaceholder))
            columnInfo["FilterPlaceholder"] = FilterPlaceholder;
        columnInfo["FilterShowOperatorSelection"] = FilterShowOperatorSelection;
        columnInfo["FilterShowClearButton"] = FilterShowClearButton;
        if (FilterDisplaySelector != null)
            columnInfo["FilterDisplaySelector"] = FilterDisplaySelector;
        if (FilterValueSelector != null)
            columnInfo["FilterValueSelector"] = FilterValueSelector;
        if (FilterTemplate != null)
            columnInfo["FilterTemplate"] = FilterTemplate;
        if (!string.IsNullOrEmpty(Width))
            columnInfo["Width"] = Width;
        if (!string.IsNullOrEmpty(HeaderClass))
            columnInfo["HeaderClass"] = HeaderClass;
        if (!string.IsNullOrEmpty(CellClass))
            columnInfo["CellClass"] = CellClass;
        if (!string.IsNullOrEmpty(Class))
            columnInfo["Class"] = Class;
        if (Property != null)
            columnInfo["Property"] = Property;
        if (Template != null)
            columnInfo["Template"] = Template;
        
        // Column management properties
        columnInfo["Resizable"] = Resizable;
        if (!string.IsNullOrEmpty(MinWidth))
            columnInfo["MinWidth"] = MinWidth;
        if (!string.IsNullOrEmpty(MaxWidth))
            columnInfo["MaxWidth"] = MaxWidth;
        columnInfo["Sticky"] = Sticky;
        columnInfo["StickyPosition"] = StickyPosition;
        columnInfo["Order"] = Order;
        columnInfo["Visible"] = Visible;
        columnInfo["Hideable"] = Hideable;
        
        var propertyName = GetPropertyName();
        if (!string.IsNullOrEmpty(propertyName))
            columnInfo["PropertyName"] = propertyName;
        
        // Let the table handle the complex type creation
        ParentTable.AddColumn(columnInfo);
    }

    protected override void OnInitialized()
    {
        base.OnInitialized();
        
        // Smart detection context available if needed
    }

    protected override void OnParametersSet()
    {
        base.OnParametersSet();
        
        // Auto-generate Key from Property if not provided
        if (string.IsNullOrEmpty(Key) && Property != null)
        {
            Key = GetPropertyName();
        }
        
        // Auto-generate Header from Property if not provided
        if (string.IsNullOrEmpty(Header) && Property != null)
        {
            Header = GetPropertyName();
        }
        
        // Register with parent table after parameters are set and cascading values are available
        if (!_isRegistered && ParentTable != null)
        {
            RegisterWithParentTable();
            _isRegistered = true;
        }
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender && !_isRegistered && ParentTable != null)
        {
            // Last chance registration for late-binding scenarios
            RegisterWithParentTable();
            _isRegistered = true;
        }
    }

    private string GetPropertyName()
    {
        if (Property == null) return "";
        
        // Extract property name from lambda expression
        if (Property.Body is MemberExpression memberExpression)
        {
            return memberExpression.Member.Name;
        }
        
        if (Property.Body is UnaryExpression unaryExpression && 
            unaryExpression.Operand is MemberExpression memberExpr)
        {
            return memberExpr.Member.Name;
        }
        return "";
    }
}