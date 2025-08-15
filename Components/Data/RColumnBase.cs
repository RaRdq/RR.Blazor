using Microsoft.AspNetCore.Components;
using RR.Blazor.Enums;
using RR.Blazor.Models;

namespace RR.Blazor.Components.Data;

public abstract class RColumnBase : ComponentBase
{
    [Parameter] public string Header { get; set; }
    [Parameter] public string Title { get; set; }
    [Parameter] public string Format { get; set; }
    [Parameter] public string EmptyText { get; set; } = "-";
    [Parameter] public ColumnTemplate Template { get; set; } = ColumnTemplate.Auto;
    [Parameter] public RenderFragment HeaderTemplate { get; set; }
    [Parameter] public bool? Sortable { get; set; }
    [Parameter] public bool? Filterable { get; set; }
    [Parameter] public bool? Searchable { get; set; }
    [Parameter] public FilterType FilterType { get; set; } = FilterType.Auto;
    [Parameter] public string Width { get; set; }
    [Parameter] public string MinWidth { get; set; }
    [Parameter] public string MaxWidth { get; set; }
    [Parameter] public ColumnAlign Align { get; set; } = ColumnAlign.Auto;
    [Parameter] public string HeaderClass { get; set; }
    [Parameter] public string CellClass { get; set; }
    [Parameter] public string Class { get; set; }
    [Parameter] public bool Visible { get; set; } = true;
    [Parameter] public bool Exportable { get; set; } = true;
    [Parameter] public string ExportHeader { get; set; }
    [Parameter] public int Order { get; set; }
    
    protected string GetEffectiveTitle() => Header ?? Title;
}