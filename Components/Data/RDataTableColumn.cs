using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using RR.Blazor.Enums;
using RR.Blazor.Models;

namespace RR.Blazor.Components.Data;

/// <summary>
/// Smart data table column component that automatically inherits item type from parent RDataTable.
/// This eliminates the need for explicit TItem specification on every column.
/// </summary>
public class RDataTableColumn<TItem> : ComponentBase
{
    [Parameter] public string Key { get; set; }
    [Parameter] public string Header { get; set; }
    [Parameter] public bool Sortable { get; set; } = true;
    [Parameter] public RenderFragment<TItem> CellTemplate { get; set; }
    [Parameter] public RenderFragment HeaderTemplate { get; set; }
    [Parameter] public string Width { get; set; }
    [Parameter] public string MinWidth { get; set; }
    [Parameter] public string MaxWidth { get; set; }
    [Parameter] public string CellClass { get; set; } = "";
    [Parameter] public string HeaderClass { get; set; } = "";
    
    // TItem is now a proper generic type parameter
    
    // New filtering parameters
    [Parameter] public bool Filterable { get; set; }
    [Parameter] public FilterType FilterType { get; set; } = FilterType.Search;
    [Parameter] public List<FilterOption> FilterOptions { get; set; } = new();
    [Parameter] public string FilterValue { get; set; } = "";
    [Parameter] public EventCallback<ColumnFilterEventArgs> OnFilterChanged { get; set; }

    [CascadingParameter] private RDataTable<TItem> ParentTable { get; set; }


    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        // Create the generic component directly
        builder.OpenComponent<RDataTableColumnGeneric<TItem>>(0);
        
        // Add all parameters
        builder.AddAttribute(1, "Key", Key);
        builder.AddAttribute(2, "Header", Header);
        builder.AddAttribute(3, "Sortable", Sortable);
        builder.AddAttribute(4, "CellTemplate", CellTemplate);
        builder.AddAttribute(5, "HeaderTemplate", HeaderTemplate);
        builder.AddAttribute(6, "Width", Width);
        builder.AddAttribute(7, "MinWidth", MinWidth);
        builder.AddAttribute(8, "MaxWidth", MaxWidth);
        builder.AddAttribute(9, "CellClass", CellClass);
        builder.AddAttribute(10, "HeaderClass", HeaderClass);
        builder.AddAttribute(11, "Filterable", Filterable);
        builder.AddAttribute(12, "FilterType", FilterType);
        builder.AddAttribute(13, "FilterOptions", FilterOptions);
        builder.AddAttribute(14, "FilterValue", FilterValue);
        builder.AddAttribute(15, "OnFilterChanged", OnFilterChanged);
        
        builder.CloseComponent();
    }

}