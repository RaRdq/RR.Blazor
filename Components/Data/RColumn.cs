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
    [Parameter] public string? Header { get; set; }
    [Parameter] public string? Format { get; set; }
    [Parameter] public bool Sortable { get; set; }
    [Parameter] public bool Filterable { get; set; }
    [Parameter] public FilterType FilterType { get; set; } = FilterType.Text;
    [Parameter] public string? Width { get; set; }
    [Parameter] public string? HeaderClass { get; set; }
    [Parameter] public string? CellClass { get; set; }
    [Parameter] public bool Sticky { get; set; }
    [Parameter] public bool Visible { get; set; } = true;
    [Parameter] public RenderFragment? HeaderTemplate { get; set; }
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
        builder.AddAttribute(10, nameof(Sticky), Sticky);
        builder.AddAttribute(11, nameof(Visible), Visible);
        builder.AddAttribute(12, nameof(HeaderTemplate), HeaderTemplate);
    }
}

/// <summary>
/// Smart RColumn wrapper with automatic type detection via cascading context.
/// Usage: <RColumn Property="@(p => p.Name)" Header="Product Name" />
/// </summary>
public class RColumn : RColumnBase
{
    [CascadingParameter] public TableContext? TableContext { get; set; }
    [CascadingParameter] public ITableParent? ParentTable { get; set; }
    [Inject] private ILogger<RColumn>? Logger { get; set; }
    
    [Parameter] public object? Property { get; set; }
    [Parameter] public object? Template { get; set; }

    private bool _isRegistered = false;
    
    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        // RColumn renders as th element directly for table header
        builder.OpenElement(0, "th");
        builder.AddAttribute(1, "class", $"table-header-cell {HeaderClass}");
        
        if (!string.IsNullOrEmpty(Width))
        {
            builder.AddAttribute(2, "style", $"width: {Width}");
        }
        
        // Render header content
        if (HeaderTemplate != null)
        {
            builder.AddContent(3, HeaderTemplate);
        }
        else
        {
            builder.AddContent(4, Header ?? GetPropertyName());
        }
        
        builder.CloseElement();
        
        // Register this column with parent table only once
        if (!_isRegistered)
        {
            RegisterWithParentTable();
            _isRegistered = true;
        }
    }

    private void RegisterWithParentTable()
    {
        if (ParentTable == null) return;
        
        // Simple approach: Create generic dictionary with column info
        var columnInfo = new Dictionary<string, object>
        {
            ["Key"] = Key ?? GetPropertyName(),
            ["Header"] = Header ?? GetPropertyName(),
            ["Format"] = Format,
            ["Sortable"] = Sortable,
            ["Filterable"] = Filterable,
            ["Width"] = Width,
            ["HeaderClass"] = HeaderClass,
            ["CellClass"] = CellClass,
            ["Property"] = Property,
            ["Template"] = Template
        };
        
        // Let the table handle the complex type creation
        ParentTable.AddColumn(columnInfo);
        
        Console.WriteLine($"RColumn: Registered column {Key ?? GetPropertyName()} with simple approach");
    }

    protected override void OnInitialized()
    {
        base.OnInitialized();
        
        // Debug logging for smart detection
        if (TableContext != null)
        {
            Console.WriteLine($"RColumn: Smart detection active. Table: {TableContext.TableId}, ItemType: {TableContext.ItemType.Name}, IsSmartTable: {TableContext.IsSmartTable}");
        }
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
    }

    private string GetPropertyName()
    {
        if (Property == null) return "";
        
        // Extract property name from expression
        if (Property is LambdaExpression expression)
        {
            if (expression.Body is MemberExpression memberExpression)
            {
                return memberExpression.Member.Name;
            }
            
            if (expression.Body is UnaryExpression unaryExpression && 
                unaryExpression.Operand is MemberExpression memberExpr)
            {
                return memberExpr.Member.Name;
            }
        }
        
        return "";
    }
}