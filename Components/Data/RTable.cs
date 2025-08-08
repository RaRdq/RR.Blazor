using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using RR.Blazor.Models;
using RR.Blazor.Components.Base;
using System.Reflection;

namespace RR.Blazor.Components.Data;

/// <summary>
/// Smart table component with automatic type detection and intelligent defaults.
/// Supports any domain (CRM, ERP, eCommerce, etc.) with context-aware behavior.
/// </summary>
public abstract class RTableBase : RInteractiveComponentBase
{
    #region Core Configuration
    [Parameter] public string AdditionalClass { get; set; } = "";
    [Parameter] public string Title { get; set; } = "";
    [Parameter] public string Subtitle { get; set; } = "";
    [Parameter] public string? StartIcon { get; set; }
    [Parameter] public string? EndIcon { get; set; }
    [Parameter] public RenderFragment HeaderContent { get; set; }
    #endregion

    #region Data & State Parameters
    [Parameter] public object Items { get; set; }
    [Parameter] public bool EnableRowHighlight { get; set; } = true;
    [Parameter] public bool FilterEnabled { get; set; }
    [Parameter] public bool SearchEnabled { get; set; } = true;
    [Parameter] public bool Hoverable { get; set; } = true;
    [Parameter] public Func<object, string> GetRowClass { get; set; }
    #endregion

    #region Smart Features
    [Parameter] public bool BulkOperationsEnabled { get; set; }
    [Parameter] public bool ExportEnabled { get; set; }
    [Parameter] public RenderFragment BulkOperations { get; set; }
    #endregion

    #region Pagination
    [Parameter] public bool ShowPagination { get; set; } = true;
    [Parameter] public int CurrentPage { get; set; } = 1;
    [Parameter] public int PageSize { get; set; } = 25;
    [Parameter] public int TotalItems { get; set; }
    [Parameter] public EventCallback<int> OnPageChange { get; set; }
    [Parameter] public List<int> PageSizeOptions { get; set; } = new() { 10, 25, 50, 100, 250 };
    #endregion

    #region Selection
    [Parameter] public bool MultiSelection { get; set; }
    [Parameter] public bool SingleSelection { get; set; }
    [Parameter] public EventCallback<object> SelectedItemsChanged { get; set; }
    [Parameter] public EventCallback<object> SelectedItemChanged { get; set; }
    [Parameter] public object SelectedItemsChangedTyped { get; set; }
    [Parameter] public object SelectedItemChangedTyped { get; set; }
    #endregion

    #region Interaction
    [Parameter] public bool RowClickable { get; set; }
    [Parameter] public EventCallback<object> OnRowClick { get; set; }
    [Parameter] public object OnRowClickTyped { get; set; }
    [Parameter] public Func<object, string> RowClassFunc { get; set; }
    [Parameter] public object RowClassFuncTyped { get; set; }
    #endregion

    #region Sorting & Filtering
    [Parameter] public string SortColumn { get; set; } = "";
    [Parameter] public bool SortDescending { get; set; }
    [Parameter] public EventCallback<string> OnSort { get; set; }
    [Parameter] public List<TableSortState> SortStates { get; set; } = new();
    [Parameter] public EventCallback<List<TableSortState>> SortStatesChanged { get; set; }
    [Parameter] public EventCallback<SortEventArgs> OnSortChanged { get; set; }
    [Parameter] public bool MultiColumnSort { get; set; } = true;
    [Parameter] public int MaxSortLevels { get; set; } = 3;
    [Parameter] public Dictionary<string, string> ColumnFilters { get; set; } = new();
    [Parameter] public EventCallback<Dictionary<string, string>> ColumnFiltersChanged { get; set; }
    [Parameter] public EventCallback<ColumnFilterEventArgs> OnColumnFilter { get; set; }
    #endregion

    #region Advanced Features
    [Parameter] public string Height { get; set; } = "";
    [Parameter] public bool Virtualize { get; set; }
    [Parameter] public bool ResizableColumns { get; set; }
    [Parameter] public List<string> StickyColumns { get; set; } = new();
    [Parameter] public Enums.ComponentDensity Density { get; set; } = Enums.ComponentDensity.Normal;
    [Parameter] public bool Striped { get; set; }
    [Parameter] public bool Hover { get; set; } = true;
    [Parameter] public bool Sortable { get; set; } = true;
    [Parameter] public bool AllowUnsorted { get; set; } = true;
    #endregion

    #region Professional Styling Parameters  
    [Parameter] public Enums.TableVariant TableVariant { get; set; } = Enums.TableVariant.Standard;
    [Parameter] public Enums.TableDensity TableDensity { get; set; } = Enums.TableDensity.Normal;
    [Parameter] public int CustomRowHeight { get; set; } = 48;
    [Parameter] public int TableElevation { get; set; } = 2;
    [Parameter] public bool EnableGlassmorphism { get; set; }
    [Parameter] public bool ShowBorders { get; set; } = true;
    [Parameter] public bool EnableHoverEffects { get; set; } = true;
    [Parameter] public bool EnableAnimations { get; set; } = true;
    [Parameter] public string TableTheme { get; set; } = "auto";
    [Parameter] public Dictionary<string, string> CustomCssVariables { get; set; } = new();
    #endregion

    #region Column Management
    [Parameter] public bool ShowColumnManager { get; set; } = true;
    [Parameter] public Dictionary<string, ColumnPreferences> ColumnPreferences { get; set; } = new();
    [Parameter] public EventCallback<Dictionary<string, ColumnPreferences>> ColumnPreferencesChanged { get; set; }
    [Parameter] public EventCallback<ColumnManagementEventArgs> OnColumnManagement { get; set; }
    [Parameter] public EventCallback<ColumnResizeEventArgs> OnColumnResize { get; set; }
    [Parameter] public bool EnableColumnReordering { get; set; }
    [Parameter] public bool PersistColumnPreferences { get; set; } = true;
    [Parameter] public string TableId { get; set; } = "";
    #endregion

    #region Content Areas
    [Parameter] public RenderFragment ColumnsContent { get; set; }
    [Parameter] public string EmptyText { get; set; } = "No data available";
    [Parameter] public string EmptyDescription { get; set; }
    [Parameter] public string EmptyIcon { get; set; } = "inbox";
    [Parameter] public RenderFragment FooterContent { get; set; }
    #endregion

    #region Export Configuration
    [Parameter] public string ExportFormats { get; set; } = "csv,excel,json";
    [Parameter] public string ExportFileName { get; set; } = "";
    [Parameter] public Dictionary<string, string> ExportMetadata { get; set; } = new();
    #endregion

    protected void ForwardBaseParameters(RenderTreeBuilder builder)
    {
        int seq = 0;
        // Exclude Blazor-specific parameters that contain @ symbols and component-specific ones
        builder.ForwardParameters(ref seq, this, 
            "Items", "SelectedItems", "SelectedItem", "Columns", "ChildContent",
            "OnRowClick", "OnRowClickTyped", "SelectedItemsChanged", "SelectedItemChanged",
            "SelectedItemsChangedTyped", "SelectedItemChangedTyped", "OnPageChange",
            "Loading", "LoadingText", "GetRowClass", "EnableRowHighlight", "Hoverable",
            "SearchEnabled", "FilterEnabled", "Elevation");
    }

    protected void ForwardBaseParametersExceptChildContent(RenderTreeBuilder builder)
    {
        int seq = 0;
        // Exclude ChildContent and Blazor-specific parameters that contain @ symbols
        builder.ForwardParameters(ref seq, this, 
            "Items", "SelectedItems", "SelectedItem", "Columns", "ChildContent",
            "OnRowClick", "OnRowClickTyped", "SelectedItemsChanged", "SelectedItemChanged",
            "SelectedItemsChangedTyped", "SelectedItemChangedTyped", "OnPageChange",
            "Loading", "LoadingText", "GetRowClass", "EnableRowHighlight", "Hoverable",
            "SearchEnabled", "FilterEnabled", "Elevation");
    }

    protected static object CreateTypedRowClassDelegate(Func<object, string> objectRowClass, Type itemType)
    {
        // Create a delegate of type Func<TItem, string> that wraps the Func<object, string>
        var delegateType = typeof(Func<,>).MakeGenericType(itemType, typeof(string));
        var method = typeof(RTableBase).GetMethod(nameof(WrapRowClassDelegate), BindingFlags.NonPublic | BindingFlags.Static)
                                       ?.MakeGenericMethod(itemType);
        return method?.Invoke(null, new object[] { objectRowClass });
    }

    private static Func<TItem, string> WrapRowClassDelegate<TItem>(Func<object, string> objectRowClass)
    {
        return item => objectRowClass(item);
    }
}

/// <summary>
/// Smart table component that automatically detects item type from Items parameter
/// Usage: &lt;RTable Items="@employees"&gt;
///           &lt;RColumn For="@(e => e.Name)" /&gt;
///        &lt;/RTable&gt;
/// </summary>
public class RTable : RTableBase
{
    [Parameter] public object SelectedItems { get; set; }
    [Parameter] public object SelectedItem { get; set; }
    [Parameter] public object Columns { get; set; }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        if (Items == null)
        {
            // Render empty state
            builder.OpenElement(0, "div");
            builder.AddAttribute(1, "class", "table-empty-state pa-6 text-center");
            builder.OpenElement(2, "div");
            builder.AddAttribute(3, "class", "text-secondary");
            builder.AddContent(4, EmptyText);
            builder.CloseElement();
            builder.CloseElement();
            return;
        }

        // Auto-detect item type from Items collection
        var itemsType = Items.GetType();
        Type itemType;

        if (itemsType.IsGenericType && itemsType.GetGenericTypeDefinition() == typeof(List<>))
        {
            itemType = itemsType.GetGenericArguments()[0];
        }
        else if (itemsType.IsArray)
        {
            itemType = itemsType.GetElementType();
        }
        else if (itemsType.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEnumerable<>)))
        {
            itemType = itemsType.GetInterfaces()
                .First(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                .GetGenericArguments()[0];
        }
        else
        {
            itemType = typeof(object);
        }

        // Create table context for child columns
        var tableContext = new TableContext(itemType, $"smart-table-{GetHashCode()}", true);

        // Use ColumnsContent for table columns
        var effectiveColumnsContent = ColumnsContent;

        // Provide context to child components
        builder.OpenComponent<CascadingValue<TableContext>>(0);
        builder.AddAttribute(1, "Value", tableContext);
        builder.AddAttribute(2, "ChildContent", (RenderFragment)(childBuilder =>
        {
            // Create RTableGeneric<T> dynamically
            var genericTableType = typeof(RTableGeneric<>).MakeGenericType(itemType);

            childBuilder.OpenComponent(0, genericTableType);
            
            // Forward all base parameters EXCEPT ChildContent (RTableGeneric doesn't support it)
            ForwardBaseParametersExceptChildContent(childBuilder);
            
            // Override ColumnsContent with effective content
            childBuilder.AddAttribute(38, nameof(ColumnsContent), effectiveColumnsContent);
            
            // Forward specific parameters with type conversion
            childBuilder.AddAttribute(50, "Items", Items);
            childBuilder.AddAttribute(51, "SelectedItems", SelectedItems);
            childBuilder.AddAttribute(52, "SelectedItem", SelectedItem);
            childBuilder.AddAttribute(53, "Columns", Columns);
            
            // Forward new parameters
            childBuilder.AddAttribute(54, "Loading", Loading);
            childBuilder.AddAttribute(55, "LoadingText", LoadingText);
            childBuilder.AddAttribute(56, "Hover", Hoverable);
            childBuilder.AddAttribute(57, "ShowSearch", SearchEnabled);
            
            // Forward GetRowClass with type conversion if provided
            if (GetRowClass != null)
            {
                // Create a typed delegate that converts object to TItem
                var typedRowClassDelegate = CreateTypedRowClassDelegate(GetRowClass, itemType);
                childBuilder.AddAttribute(58, "RowClass", typedRowClassDelegate);
            }
            
            // Note: EnableRowHighlight, FilterEnabled, Elevation may need custom handling
            
            childBuilder.CloseComponent();
        }));
        builder.CloseComponent();
    }
}

/// <summary>
/// Smart table wrapper with automatic type detection.
/// Usage: &lt;RTableAuto Items="@products" Title="Product Catalog" Domain="TableDomain.Ecommerce" /&gt;
/// </summary>
public class RTableAuto : RTableBase
{
    [Parameter] public object SelectedItems { get; set; }
    [Parameter] public object SelectedItem { get; set; }
    [Parameter] public object Columns { get; set; }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        if (Items == null)
        {
            // Render empty state
            builder.OpenElement(0, "div");
            builder.AddAttribute(1, "class", "table-empty-state pa-6 text-center");
            builder.OpenElement(2, "div");
            builder.AddAttribute(3, "class", "text-secondary");
            builder.AddContent(4, EmptyText);
            builder.CloseElement();
            builder.CloseElement();
            return;
        }

        // Auto-detect item type from Items collection
        var itemsType = Items.GetType();
        Type itemType;

        if (itemsType.IsGenericType && itemsType.GetGenericTypeDefinition() == typeof(List<>))
        {
            itemType = itemsType.GetGenericArguments()[0];
        }
        else if (itemsType.IsArray)
        {
            itemType = itemsType.GetElementType();
        }
        else if (itemsType.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEnumerable<>)))
        {
            itemType = itemsType.GetInterfaces()
                .First(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                .GetGenericArguments()[0];
        }
        else
        {
            itemType = typeof(object);
        }

        // Create table context for child columns
        var tableContext = new TableContext(itemType, $"smart-table-{GetHashCode()}", true);

        // Use ColumnsContent for table columns
        var effectiveColumnsContent = ColumnsContent;

        // Provide context to child components
        builder.OpenComponent<CascadingValue<TableContext>>(0);
        builder.AddAttribute(1, "Value", tableContext);
        builder.AddAttribute(2, "ChildContent", (RenderFragment)(childBuilder =>
        {
            // Create RTableGeneric<T> dynamically
            var genericTableType = typeof(RTableGeneric<>).MakeGenericType(itemType);

            childBuilder.OpenComponent(0, genericTableType);
            
            // Forward all base parameters EXCEPT ChildContent (RTableGeneric doesn't support it)
            ForwardBaseParametersExceptChildContent(childBuilder);
            
            // Override ColumnsContent with effective content
            childBuilder.AddAttribute(38, nameof(ColumnsContent), effectiveColumnsContent);
            
            // Forward specific parameters with type conversion
            childBuilder.AddAttribute(50, "Items", Items);
            childBuilder.AddAttribute(51, "SelectedItems", SelectedItems);
            childBuilder.AddAttribute(52, "SelectedItem", SelectedItem);
            childBuilder.AddAttribute(53, "Columns", Columns);
            
            // Forward new parameters
            childBuilder.AddAttribute(54, "Loading", Loading);
            childBuilder.AddAttribute(55, "LoadingText", LoadingText);
            childBuilder.AddAttribute(56, "Hover", Hoverable);
            childBuilder.AddAttribute(57, "ShowSearch", SearchEnabled);
            
            // Forward GetRowClass with type conversion if provided
            if (GetRowClass != null)
            {
                // Create a typed delegate that converts object to TItem
                var typedRowClassDelegate = CreateTypedRowClassDelegate(GetRowClass, itemType);
                childBuilder.AddAttribute(58, "RowClass", typedRowClassDelegate);
            }
            
            // Note: EnableRowHighlight, FilterEnabled, Elevation may need custom handling
            
            childBuilder.CloseComponent();
        }));
        builder.CloseComponent();
    }
}