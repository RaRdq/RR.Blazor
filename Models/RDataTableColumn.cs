using Microsoft.AspNetCore.Components;

namespace RR.Blazor.Models;

/// <summary>
/// Data table column definition for RTable components
/// </summary>
/// <typeparam name="T">The item type for the table</typeparam>
public class RDataTableColumn<T>
{
    public string Key { get; set; } = "";
    public string Header { get; set; } = "";
    public bool Sortable { get; set; } = true;
    public RenderFragment<T> CellTemplate { get; set; }
    public RenderFragment HeaderTemplate { get; set; }
    public string Width { get; set; }
    public string MinWidth { get; set; }
    public string MaxWidth { get; set; }
    public bool Resizable { get; set; }
    public bool Hideable { get; set; } = true;
    public bool Hidden { get; set; }
    public string CellClass { get; set; } = "";
    public string HeaderClass { get; set; } = "";
    public Func<T, object> ValueFunc { get; set; }
    public string Format { get; set; }
    public bool Filterable { get; set; }
    public RR.Blazor.Enums.FilterType FilterType { get; set; } = RR.Blazor.Enums.FilterType.Search;
    public List<RR.Blazor.Models.FilterOption> FilterOptions { get; set; } = new();
    
    // Sorting enhancement
    public Func<T, IComparable> SortKeySelector { get; set; }
    public IComparer<T> CustomComparer { get; set; }
}