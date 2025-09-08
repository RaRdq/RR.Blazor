using Microsoft.AspNetCore.Components;
using RR.Blazor.Components.Base;
using RR.Blazor.Enums;

namespace RR.Blazor.Components.Data;

public abstract class RTableBase : RComponentBase
{
    [Parameter] public string Title { get; set; }
    [Parameter] public string Subtitle { get; set; }
    [Parameter] public bool Loading { get; set; }
    [Parameter] public string LoadingText { get; set; } = "Loading...";
    [Parameter] public string EmptyText { get; set; } = "No data available";
    [Parameter] public bool ShowTitle { get; set; } = true;
    [Parameter] public bool ShowFooter { get; set; }
    [Parameter] public bool Striped { get; set; } = true;
    [Parameter] public bool Hover { get; set; } = true;
    [Parameter] public bool Bordered { get; set; }
    [Parameter] public bool Compact { get; set; }
    [Parameter] public bool FixedHeader { get; set; }
    [Parameter] public bool Virtualize { get; set; }
    [Parameter] public bool StickyHeader { get; set; }
    [Parameter] public string Height { get; set; }
    [Parameter] public string MaxHeight { get; set; }
    [Parameter] public string RowHeight { get; set; }
    [Parameter] public string HeaderHeight { get; set; }
    [Parameter] public string FooterHeight { get; set; }
    [Parameter] public string CssClass { get; set; }
    [Parameter] public bool AutoGenerateColumns { get; set; } = true;
    [Parameter] public bool MultiSelection { get; set; }
    [Parameter] public bool SearchEnabled { get; set; }
    [Parameter] public bool FilterEnabled { get; set; }
    [Parameter] public bool ExportEnabled { get; set; }
    [Parameter] public bool RefreshEnabled { get; set; }
    [Parameter] public bool BulkOperationsEnabled { get; set; }
    [Parameter] public bool ShowPagination { get; set; } = true;
    [Parameter] public bool ShowSearch { get; set; } = true;
    [Parameter] public bool ShowToolbar { get; set; } = true;
    [Parameter] public bool ShowChartButton { get; set; }
    [Parameter] public bool ShowExportButton { get; set; } = true;
    [Parameter] public bool ShowColumnManager { get; set; }
    [Parameter] public bool EnableColumnReordering { get; set; }
    [Parameter] public bool EnableStickyColumns { get; set; }
    [Parameter] public bool EnableHorizontalScroll { get; set; }
    [Parameter] public bool EnableSorting { get; set; }
    [Parameter] public bool EnableFiltering { get; set; }
    [Parameter] public bool EnableSelection { get; set; }
    [Parameter] public bool EnableExport { get; set; }
    [Parameter] public bool EnablePaging { get; set; }
    [Parameter] public bool ShowFilters { get; set; }
    [Parameter] public bool Selectable { get; set; }
    [Parameter] public bool MultiSelect { get; set; }
    [Parameter] public bool RowClickable { get; set; }
    [Parameter] public int PageSize { get; set; } = 50;
    [Parameter] public int CurrentPage { get; set; } = 1;
    [Parameter] public string ChartButtonText { get; set; } = "Show as Chart";
    [Parameter] public ChartType? DefaultChartType { get; set; }
    [Parameter] public RenderFragment ColumnsContent { get; set; }
    [Parameter] public RenderFragment BulkOperations { get; set; }
    [Parameter] public RenderFragment TableActions { get; set; }
    [Parameter] public RenderFragment EmptyContent { get; set; }
    [Parameter] public RenderFragment LoadingContent { get; set; }
    [Parameter] public RenderFragment HeaderTemplate { get; set; }
    [Parameter] public RenderFragment FooterTemplate { get; set; }
    [Parameter] public EventCallback<int> PageSizeChanged { get; set; }
    [Parameter] public EventCallback<int> CurrentPageChanged { get; set; }
    [Parameter] public EventCallback<string> SortByChanged { get; set; }
    [Parameter] public EventCallback<bool> SortDescendingChanged { get; set; }
}