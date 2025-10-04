using Microsoft.AspNetCore.Components;
using RR.Blazor.Components.Base;
using RR.Blazor.Enums;

namespace RR.Blazor.Components.Data;

/// <summary>
/// Base class for RGrid components - consolidates all parameters in single location
/// following the proven RTable pattern: RTable → RTableBase → RTableGeneric&lt;T&gt;
/// </summary>
public abstract class RGridBase : RComponentBase
{
    #region Core Parameters

    /// <summary>Grid display mode</summary>
    [Parameter] public GridMode Mode { get; set; } = GridMode.Auto;

    /// <summary>Grid title</summary>
    [Parameter] public string Title { get; set; }

    /// <summary>Grid subtitle</summary>
    [Parameter] public string Subtitle { get; set; }

    /// <summary>Grid icon</summary>
    [Parameter] public string Icon { get; set; }

    /// <summary>Show grid title</summary>
    [Parameter] public bool ShowTitle { get; set; } = true;

    /// <summary>Header content</summary>
    [Parameter] public RenderFragment HeaderContent { get; set; }

    #endregion

    #region Responsive Configuration

    /// <summary>Default grid columns</summary>
    [Parameter] public int Columns { get; set; } = 4;

    /// <summary>Grid columns for extra small screens (mobile)</summary>
    [Parameter] public int ColumnsXs { get; set; } = 1;

    /// <summary>Grid columns for small screens</summary>
    [Parameter] public int ColumnsSm { get; set; } = 2;

    /// <summary>Grid columns for medium screens</summary>
    [Parameter] public int ColumnsMd { get; set; } = 3;

    /// <summary>Grid columns for large screens</summary>
    [Parameter] public int ColumnsLg { get; set; } = 4;

    /// <summary>Grid columns for extra large screens</summary>
    [Parameter] public int ColumnsXl { get; set; } = 6;

    /// <summary>Override mode for extra small screens</summary>
    [Parameter] public GridMode? ModeXs { get; set; }

    /// <summary>Override mode for small screens</summary>
    [Parameter] public GridMode? ModeSm { get; set; }

    /// <summary>Override mode for medium screens</summary>
    [Parameter] public GridMode? ModeMd { get; set; }

    /// <summary>Override mode for large screens</summary>
    [Parameter] public GridMode? ModeLg { get; set; }

    /// <summary>Override mode for extra large screens</summary>
    [Parameter] public GridMode? ModeXl { get; set; }

    /// <summary>Gap between grid items</summary>
    [Parameter] public string Gap { get; set; } = "var(--space-4)";

    #endregion

    #region Toolbar Parameters

    /// <summary>Show toolbar</summary>
    [Parameter] public bool ShowToolbar { get; set; } = true;

    /// <summary>Show search in toolbar</summary>
    [Parameter] public bool ShowSearch { get; set; } = true;

    /// <summary>Search placeholder text</summary>
    [Parameter] public string SearchPlaceholder { get; set; } = "Search...";

    /// <summary>Show filters in toolbar</summary>
    [Parameter] public bool ShowFilters { get; set; }

    /// <summary>Show export button in toolbar</summary>
    [Parameter] public bool ShowExportButton { get; set; }

    /// <summary>Toolbar content</summary>
    [Parameter] public RenderFragment ToolbarContent { get; set; }

    #endregion

    #region Filter Integration

    /// <summary>External filter reference for Case 1 integration</summary>
    [Parameter] public object Filter { get; set; }

    /// <summary>Enable built-in toolbar filter for Case 2 integration</summary>
    [Parameter] public bool EnableFilter { get; set; }

    /// <summary>Show quick search in toolbar</summary>
    [Parameter] public bool ShowQuickSearch { get; set; } = true;

    #endregion

    #region Pagination

    /// <summary>Enable pagination for the grid</summary>
    [Parameter] public bool EnablePagination { get; set; } = true;

    /// <summary>Number of items per page</summary>
    [Parameter] public int PageSize { get; set; } = 24;

    /// <summary>Current page index (1-based)</summary>
    [Parameter] public int CurrentPage { get; set; } = 1;

    /// <summary>Page size options</summary>
    [Parameter] public int[] PageSizeOptions { get; set; }

    /// <summary>Callback when page changes</summary>
    [Parameter] public EventCallback<int> OnPageChanged { get; set; }

    /// <summary>Callback when page size changes</summary>
    [Parameter] public EventCallback<int> PageSizeChanged { get; set; }

    /// <summary>Callback when current page changes</summary>
    [Parameter] public EventCallback<int> CurrentPageChanged { get; set; }

    #endregion

    #region Performance Features

    /// <summary>Enable virtualization for large datasets</summary>
    [Parameter] public bool EnableVirtualization { get; set; }

    /// <summary>Threshold for automatic virtualization</summary>
    [Parameter] public int VirtualizationThreshold { get; set; } = 1000;

    /// <summary>Virtual item height for virtualization</summary>
    [Parameter] public float VirtualItemHeight { get; set; } = 200f;

    /// <summary>Grid height (for virtualization)</summary>
    [Parameter] public string Height { get; set; }

    /// <summary>Maximum grid height</summary>
    [Parameter] public string MaxHeight { get; set; }

    #endregion

    #region Content Parameters

    /// <summary>Loading state</summary>
    [Parameter] public bool Loading { get; set; }

    /// <summary>Loading text</summary>
    [Parameter] public string LoadingText { get; set; } = "Loading...";

    /// <summary>Empty message</summary>
    [Parameter] public string EmptyMessage { get; set; } = "No items to display";

    /// <summary>Content to show when no items</summary>
    [Parameter] public RenderFragment EmptyContent { get; set; }

    #endregion

    #region Selection

    /// <summary>Enable item selection</summary>
    [Parameter] public bool Selectable { get; set; }

    /// <summary>Enable multi-item selection</summary>
    [Parameter] public bool MultiSelect { get; set; }

    #endregion

    #region Events

    /// <summary>Event fired when responsive mode changes</summary>
    [Parameter] public EventCallback<GridMode> OnModeChanged { get; set; }

    /// <summary>Event fired when more items should be loaded (infinite scroll)</summary>
    [Parameter] public EventCallback OnLoadMore { get; set; }

    #endregion
}