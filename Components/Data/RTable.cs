using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using RR.Blazor.Models;

namespace RR.Blazor.Components.Data;

/// <summary>
/// Smart table component with automatic type detection and intelligent defaults.
/// Supports any domain (CRM, ERP, eCommerce, etc.) with context-aware behavior.
/// </summary>
public abstract class RTableBase : ComponentBase
{
    #region Core Configuration
    [Parameter] public string Class { get; set; } = "";
    [Parameter] public string AdditionalClass { get; set; } = "";
    [Parameter] public bool Loading { get; set; }
    [Parameter] public string Title { get; set; } = "";
    [Parameter] public string Subtitle { get; set; } = "";
    [Parameter] public string? StartIcon { get; set; }
    [Parameter] public string? EndIcon { get; set; }
    [Parameter] public RenderFragment HeaderContent { get; set; }
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
    [Parameter] public Dictionary<string, string> ColumnFilters { get; set; } = new();
    [Parameter] public EventCallback<Dictionary<string, string>> ColumnFiltersChanged { get; set; }
    [Parameter] public EventCallback<ColumnFilterEventArgs> OnColumnFilter { get; set; }
    #endregion

    #region Advanced Features
    [Parameter] public string Height { get; set; } = "";
    [Parameter] public bool Virtualize { get; set; }
    [Parameter] public bool ResizableColumns { get; set; }
    [Parameter] public List<string> StickyColumns { get; set; } = new();
    [Parameter] public RR.Blazor.Enums.ComponentDensity Density { get; set; } = RR.Blazor.Enums.ComponentDensity.Normal;
    #endregion

    #region Content Areas
    [Parameter] public RenderFragment ColumnsContent { get; set; }
    [Parameter] public string EmptyMessage { get; set; }
    [Parameter] public string LoadingMessage { get; set; }
    [Parameter] public RenderFragment FooterContent { get; set; }
    #endregion

    #region Export Configuration
    [Parameter] public string ExportFormats { get; set; } = "csv,excel,json";
    [Parameter] public string ExportFileName { get; set; } = "";
    [Parameter] public Dictionary<string, string> ExportMetadata { get; set; } = new();
    #endregion

    protected void ForwardBaseParameters(RenderTreeBuilder builder)
    {
        builder.AddAttribute(1, nameof(AdditionalClass), AdditionalClass);
        builder.AddAttribute(2, nameof(Loading), Loading);
        builder.AddAttribute(3, nameof(Title), Title);
        builder.AddAttribute(4, nameof(Subtitle), Subtitle);
        builder.AddAttribute(5, nameof(StartIcon), StartIcon);
        builder.AddAttribute(6, nameof(EndIcon), EndIcon);
        builder.AddAttribute(7, nameof(HeaderContent), HeaderContent);
        
        builder.AddAttribute(8, nameof(BulkOperationsEnabled), BulkOperationsEnabled);
        builder.AddAttribute(9, nameof(ExportEnabled), ExportEnabled);
        builder.AddAttribute(10, nameof(BulkOperations), BulkOperations);
        
        builder.AddAttribute(11, nameof(ShowPagination), ShowPagination);
        builder.AddAttribute(12, nameof(CurrentPage), CurrentPage);
        builder.AddAttribute(13, nameof(PageSize), PageSize);
        builder.AddAttribute(14, nameof(TotalItems), TotalItems);
        builder.AddAttribute(15, nameof(OnPageChange), OnPageChange);
        builder.AddAttribute(16, nameof(PageSizeOptions), PageSizeOptions);
        
        builder.AddAttribute(17, nameof(MultiSelection), MultiSelection);
        builder.AddAttribute(18, nameof(SingleSelection), SingleSelection);
        builder.AddAttribute(19, nameof(SelectedItemsChanged), SelectedItemsChanged);
        builder.AddAttribute(20, nameof(SelectedItemChanged), SelectedItemChanged);
        
        builder.AddAttribute(21, nameof(RowClickable), RowClickable);
        builder.AddAttribute(22, nameof(OnRowClick), OnRowClick);
        builder.AddAttribute(23, nameof(RowClassFunc), RowClassFunc);
        
        builder.AddAttribute(28, nameof(SortColumn), SortColumn);
        builder.AddAttribute(29, nameof(SortDescending), SortDescending);
        builder.AddAttribute(30, nameof(OnSort), OnSort);
        builder.AddAttribute(31, nameof(ColumnFilters), ColumnFilters);
        builder.AddAttribute(32, nameof(ColumnFiltersChanged), ColumnFiltersChanged);
        builder.AddAttribute(33, nameof(OnColumnFilter), OnColumnFilter);
        
        builder.AddAttribute(34, nameof(Height), Height);
        builder.AddAttribute(35, nameof(Virtualize), Virtualize);
        builder.AddAttribute(36, nameof(ResizableColumns), ResizableColumns);
        builder.AddAttribute(37, nameof(StickyColumns), StickyColumns);
        
        builder.AddAttribute(38, nameof(ColumnsContent), ColumnsContent);
        builder.AddAttribute(39, nameof(EmptyMessage), EmptyMessage);
        builder.AddAttribute(40, nameof(LoadingMessage), LoadingMessage);
        builder.AddAttribute(41, nameof(FooterContent), FooterContent);
        
        builder.AddAttribute(42, nameof(ExportFormats), ExportFormats);
        builder.AddAttribute(43, nameof(ExportFileName), ExportFileName);
        builder.AddAttribute(44, nameof(ExportMetadata), ExportMetadata);
        builder.AddAttribute(45, nameof(Density), Density);
    }

    protected void ForwardBaseParametersExceptChildContent(RenderTreeBuilder builder)
    {
        builder.AddAttribute(1, nameof(AdditionalClass), AdditionalClass);
        builder.AddAttribute(2, nameof(Loading), Loading);
        builder.AddAttribute(3, nameof(Title), Title);
        builder.AddAttribute(4, nameof(Subtitle), Subtitle);
        builder.AddAttribute(5, nameof(StartIcon), StartIcon);
        builder.AddAttribute(6, nameof(EndIcon), EndIcon);
        builder.AddAttribute(7, nameof(HeaderContent), HeaderContent);
        
        builder.AddAttribute(8, nameof(BulkOperationsEnabled), BulkOperationsEnabled);
        builder.AddAttribute(9, nameof(ExportEnabled), ExportEnabled);
        builder.AddAttribute(10, nameof(BulkOperations), BulkOperations);
        
        builder.AddAttribute(11, nameof(ShowPagination), ShowPagination);
        builder.AddAttribute(12, nameof(CurrentPage), CurrentPage);
        builder.AddAttribute(13, nameof(PageSize), PageSize);
        builder.AddAttribute(14, nameof(TotalItems), TotalItems);
        builder.AddAttribute(15, nameof(OnPageChange), OnPageChange);
        builder.AddAttribute(16, nameof(PageSizeOptions), PageSizeOptions);
        
        builder.AddAttribute(17, nameof(MultiSelection), MultiSelection);
        builder.AddAttribute(18, nameof(SingleSelection), SingleSelection);
        builder.AddAttribute(19, nameof(SelectedItemsChanged), SelectedItemsChanged);
        builder.AddAttribute(20, nameof(SelectedItemChanged), SelectedItemChanged);
        
        builder.AddAttribute(21, nameof(RowClickable), RowClickable);
        builder.AddAttribute(22, nameof(OnRowClick), OnRowClick);
        builder.AddAttribute(23, nameof(RowClassFunc), RowClassFunc);
        
        builder.AddAttribute(28, nameof(SortColumn), SortColumn);
        builder.AddAttribute(29, nameof(SortDescending), SortDescending);
        builder.AddAttribute(30, nameof(OnSort), OnSort);
        builder.AddAttribute(31, nameof(ColumnFilters), ColumnFilters);
        builder.AddAttribute(32, nameof(ColumnFiltersChanged), ColumnFiltersChanged);
        builder.AddAttribute(33, nameof(OnColumnFilter), OnColumnFilter);
        
        builder.AddAttribute(34, nameof(Height), Height);
        builder.AddAttribute(35, nameof(Virtualize), Virtualize);
        builder.AddAttribute(36, nameof(ResizableColumns), ResizableColumns);
        builder.AddAttribute(37, nameof(StickyColumns), StickyColumns);
        
        builder.AddAttribute(38, nameof(ColumnsContent), ColumnsContent);
        // NOTE: Intentionally NOT forwarding ChildContent - RTableGeneric doesn't support it
        builder.AddAttribute(39, nameof(EmptyMessage), EmptyMessage);
        builder.AddAttribute(40, nameof(LoadingMessage), LoadingMessage);
        builder.AddAttribute(41, nameof(FooterContent), FooterContent);
        
        builder.AddAttribute(42, nameof(ExportFormats), ExportFormats);
        builder.AddAttribute(43, nameof(ExportFileName), ExportFileName);
        builder.AddAttribute(44, nameof(ExportMetadata), ExportMetadata);
        builder.AddAttribute(45, nameof(Density), Density);
    }
}

/// <summary>
/// Smart table wrapper with automatic type detection.
/// Usage: &lt;RTable Items="@products" Title="Product Catalog" Domain="TableDomain.Ecommerce" /&gt;
/// </summary>
public class RTable : RTableBase
{
    [Parameter] public object Items { get; set; }
    [Parameter] public object SelectedItems { get; set; }
    [Parameter] public object SelectedItem { get; set; }
    [Parameter] public object Columns { get; set; }
    [Parameter] public RenderFragment ChildContent { get; set; }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        if (Items == null)
        {
            // Render empty state
            builder.OpenElement(0, "div");
            builder.AddAttribute(1, "class", "table-empty-state pa-6 text-center");
            builder.OpenElement(2, "div");
            builder.AddAttribute(3, "class", "text-secondary");
            builder.AddContent(4, EmptyMessage ?? "No data available");
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

        // Use ChildContent as ColumnsContent if provided (for compatibility)
        var effectiveColumnsContent = ColumnsContent ?? ChildContent;

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
            
            childBuilder.CloseComponent();
        }));
        builder.CloseComponent();
    }
}