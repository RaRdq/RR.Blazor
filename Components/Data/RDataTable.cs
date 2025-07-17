using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using RR.Blazor.Enums;
using RR.Blazor.Models;
using System.Collections;

namespace RR.Blazor.Components.Data;

/// <summary>
/// Smart data table component that automatically infers item type from Items collection.
/// This eliminates the need for explicit TItem specification in most use cases.
/// </summary>
public class RDataTable<TItem> : ComponentBase
{
    // Core Configuration
    [Parameter] public IEnumerable<TItem> Items { get; set; }
    [Parameter] public bool Loading { get; set; }
    [Parameter] public int Elevation { get; set; } = 2;
    [Parameter] public string Class { get; set; }

    // Header Configuration  
    [Parameter] public bool ShowHeader { get; set; } = true;
    [Parameter] public string Title { get; set; }
    [Parameter] public string Subtitle { get; set; }
    [Parameter] public string Icon { get; set; }
    [Parameter] public RenderFragment HeaderContent { get; set; }

    // Filters
    [Parameter] public bool ShowFilters { get; set; }
    [Parameter] public RenderFragment Filters { get; set; }

    // Table Configuration
    [Parameter] public bool ShowTableHeader { get; set; } = true;
    [Parameter] public RenderFragment TableHeader { get; set; }
    [Parameter] public bool Striped { get; set; }
    [Parameter] public bool Hoverable { get; set; } = true;
    [Parameter] public bool Dense { get; set; }
    [Parameter] public string RowClass { get; set; } = "";
    
    // Child Content Support
    [Parameter] public RenderFragment ColumnsContent { get; set; }
    [Parameter] public RenderFragment FooterContent { get; set; }
    [Parameter] public RenderFragment ChildContent { get; set; }

    // Row Interaction
    [Parameter] public bool RowClickable { get; set; }
    [Parameter] public EventCallback<TItem> OnRowClick { get; set; }
    [Parameter] public Func<TItem, string> RowClassFunc { get; set; }

    // Selection
    [Parameter] public bool MultiSelection { get; set; }
    [Parameter] public bool SingleSelection { get; set; }
    [Parameter] public IList<TItem> SelectedItems { get; set; } = new List<TItem>();
    [Parameter] public EventCallback<IList<TItem>> SelectedItemsChanged { get; set; }
    [Parameter] public TItem SelectedItem { get; set; }
    [Parameter] public EventCallback<TItem> SelectedItemChanged { get; set; }

    // Sorting
    [Parameter] public string SortColumn { get; set; }
    [Parameter] public bool SortDescending { get; set; }
    [Parameter] public EventCallback<string> OnSort { get; set; }
    [Parameter] public bool AllowUnsorted { get; set; } = true;

    // Pagination
    [Parameter] public bool ShowPagination { get; set; } = true;
    [Parameter] public int CurrentPage { get; set; } = 1;
    [Parameter] public int PageSize { get; set; } = 10;
    [Parameter] public int TotalItems { get; set; }
    [Parameter] public EventCallback<int> OnPageChange { get; set; }
    [Parameter] public List<int> PageSizeOptions { get; set; } = new() { 5, 10, 25, 50, 100 };

    // Empty State
    [Parameter] public string EmptyMessage { get; set; }
    [Parameter] public RenderFragment EmptyStateContent { get; set; }
    [Parameter] public string LoadingMessage { get; set; }

    // Advanced Features
    [Parameter] public bool FixedHeader { get; set; }
    [Parameter] public string Height { get; set; }
    [Parameter] public bool Virtualize { get; set; }
    [Parameter] public float ItemSize { get; set; } = 50f;
    [Parameter] public bool ResizableColumns { get; set; }
    [Parameter] public List<string> StickyColumns { get; set; } = new();

    // Column Filtering
    [Parameter] public Dictionary<string, string> ColumnFilters { get; set; } = new();
    [Parameter] public EventCallback<Dictionary<string, string>> ColumnFiltersChanged { get; set; }
    [Parameter] public EventCallback<ColumnFilterEventArgs> OnColumnFilter { get; set; }
    
    /// <summary>
    /// Gets the item type for use by child columns
    /// </summary>
    public Type InferredItemType => typeof(TItem);


    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        // Provide this instance as cascading value for columns
        builder.OpenComponent<CascadingValue<RDataTable<TItem>>>(0);
        builder.AddAttribute(1, "Value", this);
        builder.AddAttribute(2, "ChildContent", (RenderFragment)((builder2) =>
        {
            // Create the generic component directly
            builder2.OpenComponent<RDataTableGeneric<TItem>>(0);
        
            // Add all parameters - Items is already properly typed
            var itemsList = Items?.ToList();
            
            // Core Configuration
            builder2.AddAttribute(1, "Items", itemsList);
            builder2.AddAttribute(2, "Loading", Loading);
            builder2.AddAttribute(3, "Elevation", Elevation);
            builder2.AddAttribute(4, "Class", Class);
            
            // Header Configuration  
            builder2.AddAttribute(5, "ShowHeader", ShowHeader);
            builder2.AddAttribute(6, "Title", Title);
            builder2.AddAttribute(7, "Subtitle", Subtitle);
            builder2.AddAttribute(8, "Icon", Icon);
            builder2.AddAttribute(9, "HeaderContent", HeaderContent);
            
            // Filters
            builder2.AddAttribute(10, "ShowFilters", ShowFilters);
            builder2.AddAttribute(11, "Filters", Filters);
            
            // Table Configuration
            builder2.AddAttribute(12, "ShowTableHeader", ShowTableHeader);
            builder2.AddAttribute(13, "TableHeader", TableHeader);
            builder2.AddAttribute(14, "Striped", Striped);
            builder2.AddAttribute(15, "Hoverable", Hoverable);
            builder2.AddAttribute(16, "Dense", Dense);
            builder2.AddAttribute(17, "RowClass", RowClass);
            
            // Child Content Support
            builder2.AddAttribute(18, "ColumnsContent", ColumnsContent ?? ChildContent);
            builder2.AddAttribute(19, "FooterContent", FooterContent);
            builder2.AddAttribute(20, "ChildContent", ChildContent);
            
            // Row Interaction
            builder2.AddAttribute(21, "RowClickable", RowClickable);
            builder2.AddAttribute(22, "OnRowClick", OnRowClick);
            builder2.AddAttribute(23, "RowClassFunc", RowClassFunc);
            
            // Selection
            builder2.AddAttribute(24, "MultiSelection", MultiSelection);
            builder2.AddAttribute(25, "SingleSelection", SingleSelection);
            builder2.AddAttribute(26, "SelectedItems", ConvertSelectedItems());
            builder2.AddAttribute(27, "SelectedItemsChanged", SelectedItemsChanged);
            builder2.AddAttribute(28, "SelectedItem", SelectedItem);
            builder2.AddAttribute(29, "SelectedItemChanged", SelectedItemChanged);
            
            // Sorting
            builder2.AddAttribute(30, "SortColumn", SortColumn);
            builder2.AddAttribute(31, "SortDescending", SortDescending);
            builder2.AddAttribute(32, "OnSort", OnSort);
            builder2.AddAttribute(33, "AllowUnsorted", AllowUnsorted);
            
            // Pagination
            builder2.AddAttribute(34, "ShowPagination", ShowPagination);
            builder2.AddAttribute(35, "CurrentPage", CurrentPage);
            builder2.AddAttribute(36, "PageSize", PageSize);
            builder2.AddAttribute(37, "TotalItems", TotalItems);
            builder2.AddAttribute(38, "OnPageChange", OnPageChange);
            builder2.AddAttribute(39, "PageSizeOptions", PageSizeOptions);
            
            // Empty State
            builder2.AddAttribute(40, "EmptyMessage", EmptyMessage);
            builder2.AddAttribute(41, "EmptyStateContent", EmptyStateContent);
            builder2.AddAttribute(42, "LoadingMessage", LoadingMessage);
            
            // Advanced Features
            builder2.AddAttribute(43, "FixedHeader", FixedHeader);
            builder2.AddAttribute(44, "Height", Height);
            builder2.AddAttribute(45, "Virtualize", Virtualize);
            builder2.AddAttribute(46, "ItemSize", ItemSize);
            builder2.AddAttribute(47, "ResizableColumns", ResizableColumns);
            builder2.AddAttribute(48, "StickyColumns", StickyColumns);
            
            // Column Filtering
            builder2.AddAttribute(49, "ColumnFilters", ColumnFilters);
            builder2.AddAttribute(50, "ColumnFiltersChanged", ColumnFiltersChanged);
            builder2.AddAttribute(51, "OnColumnFilter", OnColumnFilter);
        
            builder2.CloseComponent();
        }));
        builder.CloseComponent();
    }
    
    private List<TItem> ConvertSelectedItems()
    {
        return SelectedItems?.ToList() ?? new List<TItem>();
    }
}