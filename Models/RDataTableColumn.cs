using Microsoft.AspNetCore.Components;
using RR.Blazor.Enums;

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
    public bool Resizable { get; set; } = true;
    public bool Hideable { get; set; } = true;
    public bool Hidden { get; set; }
    public StickyPosition StickyPosition { get; set; } = StickyPosition.Left;
    public int Order { get; set; }
    public string CellClass { get; set; } = "";
    public string HeaderClass { get; set; } = "";
    public Func<T, object> ValueFunc { get; set; }
    public string Format { get; set; }
    public bool Filterable { get; set; }
    public FilterType FilterType { get; set; } = FilterType.Text;
    public List<FilterOption> FilterOptions { get; set; } = new();
    
    // Filter UI configuration
    public string FilterPlaceholder { get; set; } = "";
    public bool FilterShowOperatorSelection { get; set; } = true;
    public bool FilterShowClearButton { get; set; } = true;
    public object FilterMinValue { get; set; }
    public object FilterMaxValue { get; set; }
    public bool FilterAllowNull { get; set; } = true;
    public bool FilterMultipleSelection { get; set; } = false;
    public string FilterDateFormat { get; set; } = "yyyy-MM-dd";
    public bool FilterCaseSensitive { get; set; } = false;
    public int FilterDebounceMs { get; set; } = 300;
    
    // Sorting enhancement
    public Func<T, IComparable> SortKeySelector { get; set; }
    public IComparer<T> CustomComparer { get; set; }
}