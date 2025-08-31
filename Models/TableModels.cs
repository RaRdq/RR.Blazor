using Microsoft.AspNetCore.Components;
using RR.Blazor.Enums;
using System.Text.Json.Serialization;

namespace RR.Blazor.Models;

/// <summary>
/// Configuration for table features inspired by Radzen DataGrid
/// </summary>
public class TableConfiguration
{
    /// <summary>Column resizing configuration</summary>
    public ColumnResizeConfig ResizeConfig { get; set; } = new();
    
    /// <summary>Sticky columns configuration</summary>
    public StickyColumnsConfig StickyConfig { get; set; } = new();
    
    /// <summary>Selection configuration</summary>
    public SelectionConfig SelectionConfig { get; set; } = new();
    
    /// <summary>Animation and visual effects configuration</summary>
    public AnimationConfig AnimationConfig { get; set; } = new();
}

/// <summary>
/// Configuration for column resizing functionality
/// </summary>
public class ColumnResizeConfig
{
    /// <summary>Enable column resizing globally</summary>
    public bool Enabled { get; set; } = true;
    
    /// <summary>Show resize handles on hover</summary>
    public bool ShowHandlesOnHover { get; set; } = true;
    
    /// <summary>Minimum column width in pixels</summary>
    public int MinColumnWidth { get; set; } = 50;
    
    /// <summary>Maximum column width in pixels</summary>
    public int MaxColumnWidth { get; set; } = 500;
    
    /// <summary>Handle width in pixels</summary>
    public int HandleWidth { get; set; } = 4;
    
    /// <summary>Enable live resize preview</summary>
    public bool LiveResize { get; set; } = true;
    
    /// <summary>Remember column widths in localStorage</summary>
    public bool PersistWidths { get; set; } = false;
    
    /// <summary>Storage key for persisting widths</summary>
    public string StorageKey { get; set; } = "table-column-widths";
}

/// <summary>
/// Configuration for sticky columns functionality
/// </summary>
public class StickyColumnsConfig
{
    /// <summary>Enable sticky columns globally</summary>
    public bool Enabled { get; set; } = true;
    
    /// <summary>Enable sticky header</summary>
    public bool StickyHeader { get; set; } = true;
    
    /// <summary>Maximum number of sticky left columns</summary>
    public int MaxLeftSticky { get; set; } = 3;
    
    /// <summary>Maximum number of sticky right columns</summary>
    public int MaxRightSticky { get; set; } = 2;
    
    /// <summary>Show shadow on sticky columns</summary>
    public bool ShowShadows { get; set; } = true;
    
    /// <summary>Shadow intensity (0-1)</summary>
    public double ShadowIntensity { get; set; } = 0.1;
}

/// <summary>
/// Configuration for row selection
/// </summary>
public class SelectionConfig
{
    /// <summary>Enable enhanced selection features</summary>
    public bool Enabled { get; set; } = true;
    
    /// <summary>Show selection checkboxes</summary>
    public bool ShowCheckboxes { get; set; } = true;
    
    /// <summary>Enable row click selection</summary>
    public bool RowClickSelection { get; set; } = true;
    
    /// <summary>Enable keyboard navigation</summary>
    public bool KeyboardNavigation { get; set; } = true;
    
    /// <summary>Selection mode</summary>
    public TableSelectionMode Mode { get; set; } = TableSelectionMode.Multiple;
    
    /// <summary>Show selection count</summary>
    public bool ShowSelectionCount { get; set; } = true;
    
    /// <summary>Enable select all functionality</summary>
    public bool EnableSelectAll { get; set; } = true;
    
    /// <summary>Preserve selection across page changes</summary>
    public bool PreserveSelectionAcrossPages { get; set; } = true;
}

/// <summary>
/// Configuration for animations and visual effects
/// </summary>
public class AnimationConfig
{
    /// <summary>Enable animations globally</summary>
    public bool Enabled { get; set; } = true;
    
    /// <summary>Hover animation duration in milliseconds</summary>
    public int HoverDuration { get; set; } = 200;
    
    /// <summary>Selection animation duration in milliseconds</summary>
    public int SelectionDuration { get; set; } = 150;
    
    /// <summary>Column resize animation duration in milliseconds</summary>
    public int ResizeDuration { get; set; } = 100;
    
    /// <summary>Row transition duration in milliseconds</summary>
    public int RowTransitionDuration { get; set; } = 300;
    
    /// <summary>Animation easing function</summary>
    public string EasingFunction { get; set; } = "cubic-bezier(0.4, 0, 0.2, 1)";
    
    /// <summary>Respect user's prefers-reduced-motion setting</summary>
    public bool RespectReducedMotion { get; set; } = true;
}

/// <summary>
/// Enhanced selection mode options
/// </summary>
public enum TableSelectionMode
{
    None,
    Single,
    Multiple,
    Extended  // Shift+click for range selection
}

/// <summary>
/// Column resizing state and event data
/// </summary>
public class ColumnResizeState
{
    public string ColumnKey { get; set; }
    public bool IsResizing { get; set; }
    public double StartX { get; set; }
    public double StartWidth { get; set; }
    public double CurrentWidth { get; set; }
    public double MinWidth { get; set; }
    public double MaxWidth { get; set; }
}

/// <summary>
/// Sticky column positioning data
/// </summary>
public class StickyColumnPosition
{
    public string ColumnKey { get; set; }
    public StickyColumnType Type { get; set; }
    public int Order { get; set; }
    public double Left { get; set; }
    public double Right { get; set; }
    public double Width { get; set; }
    public bool ShowShadow { get; set; }
}

/// <summary>
/// Row selection state
/// </summary>
public class RowSelectionState<TItem> where TItem : class
{
    public HashSet<TItem> SelectedItems { get; set; } = new();
    public TItem LastSelectedItem { get; set; }
    public bool IsSelectAllChecked { get; set; }
    public bool IsSelectAllIndeterminate { get; set; }
    public int TotalSelectableItems { get; set; }
    public DateTime LastSelectionTime { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Filter integration context for RFilter component
/// </summary>
public class TableFilterContext<TItem> where TItem : class
{
    public IQueryable<TItem> FilteredQuery { get; set; }
    public Dictionary<string, object> ActiveFilters { get; set; } = new();
    public string SearchTerm { get; set; } = "";
    public bool HasActiveFilters => !string.IsNullOrEmpty(SearchTerm) || ActiveFilters.Any();
    public int FilteredCount { get; set; }
    public int TotalCount { get; set; }
}

/// <summary>
/// Column preferences with advanced features
/// </summary>
public record ColumnPreferencesAdvanced : ColumnPreferences
{
    /// <summary>Column width preference</summary>
    public new string Width { get; set; }
    
    /// <summary>Sticky positioning preference</summary>
    public StickyColumnType StickyPosition { get; set; } = StickyColumnType.None;
    
    /// <summary>Sticky order preference</summary>
    public int StickyOrder { get; set; }
    
    /// <summary>Last resize timestamp</summary>
    public DateTime? LastResized { get; set; }
    
    /// <summary>User-customized alignment</summary>
    public ColumnAlign? CustomAlign { get; set; }
}

/// <summary>
/// Universal template context for column rendering
/// </summary>
public class UniversalTemplateContext<TItem> where TItem : class
{
    public TItem Item { get; set; }
    public ColumnDefinition<TItem> Column { get; set; }
    public int RowIndex { get; set; }
    public bool IsSelected { get; set; }
    public bool IsHovered { get; set; }
    public Dictionary<string, object> CustomData { get; set; } = new();
}

/// <summary>
/// Row click event arguments
/// </summary>
public class RowClickEventArgs<TItem> where TItem : class
{
    public TItem Item { get; set; }
    public int Index { get; set; }
    public bool IsSelected { get; set; }
    public DateTime Timestamp { get; set; }
    public Dictionary<string, object> CustomData { get; set; } = new();
}