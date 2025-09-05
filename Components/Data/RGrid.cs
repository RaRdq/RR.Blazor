using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.JSInterop;
using RR.Blazor.Attributes;
using RR.Blazor.Components.Base;
using RR.Blazor.Components.Core;
using RR.Blazor.Components.Display;
using RR.Blazor.Enums;
using RR.Blazor.Models;
using System.Collections;
using System.Reflection;

namespace RR.Blazor.Components.Data;

/// <summary>
/// Grid display modes for intelligent layout selection
/// </summary>
public enum GridMode
{
    Auto,     // Smart detection based on data properties
    Table,    // Use RTable for tabular data (edge case)
    Cards,    // Card-based responsive layouts
    List,     // Single/multi-column lists
    Tiles,    // Uniform tile layouts (TODO: implement RTile)
    Gallery,  // Image-focused grids (TODO: implement RGallery)
    Masonry   // Pinterest-style variable heights (TODO: implement RMasonry)
}

/// <summary>
/// Smart grid component with type inference that forwards to RGridForwarder&lt;T&gt;.
/// Uses CSS Grid for responsive layouts with proper pagination integration.
/// 
/// Architecture:
/// - RGrid → Type inference → RGridForwarder&lt;T&gt; → RGridGeneric&lt;T&gt;
/// - Follows RTable pattern: RTable → RTableForwarder&lt;T&gt; → RTableGeneric&lt;T&gt;
/// - Integrates with RPaginationFooter using 1-based pagination
/// - Supports virtualization via Blazor's Virtualize component
/// </summary>
[Component("RGrid", Category = "Data")]
[AIOptimized(
    Prompt = "Smart grid layout with auto mode detection and responsive design", 
    CommonUse = "responsive grids, card layouts, image galleries, data displays", 
    AvoidUsage = "Don't use for tabular data (use RTable), simple lists (use RList)"
)]
public class RGrid : RComponentBase
{
    private object _items;
    private Type _itemType;
    
    #region Core Parameters
    
    /// <summary>
    /// The data items to display in the grid
    /// </summary>
    [Parameter, AIParameter("Collection of items to display", "@employees or @products")]
    public object Items 
    { 
        get => _items;
        set
        {
            _items = value;
            _itemType = null; // Reset type inference when data changes
        }
    }
    
    /// <summary>
    /// Explicit grid mode. If Auto, mode will be detected from data properties.
    /// </summary>
    [Parameter, AIParameter("Grid layout mode", "GridMode.Auto for smart detection")]
    public GridMode Mode { get; set; } = GridMode.Auto;
    
    /// <summary>
    /// Custom template for rendering individual items
    /// </summary>
    [Parameter]
    public RenderFragment<object> ItemTemplate { get; set; }
    
    #endregion
    
    #region Filter Integration
    
    /// <summary>
    /// External filter reference for Case 1 integration
    /// </summary>
    [Parameter, AIParameter("External filter reference", "@myFilter")]
    public object Filter { get; set; }
    
    /// <summary>
    /// Enable built-in toolbar filter for Case 2 integration
    /// </summary>
    [Parameter, AIParameter("Enable built-in filter in toolbar", "true")]
    public bool EnableFilter { get; set; }
    
    /// <summary>
    /// Show quick search in toolbar
    /// </summary>
    [Parameter] public bool ShowQuickSearch { get; set; } = true;
    
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
    
    #endregion
    
    #region Performance Features
    
    /// <summary>
    /// Enable virtualization for large datasets
    /// </summary>
    [Parameter] public bool EnableVirtualization { get; set; }
    
    /// <summary>
    /// Threshold for automatic virtualization
    /// </summary>
    [Parameter] public int VirtualizationThreshold { get; set; } = 1000;
    
    /// <summary>
    /// Gap between grid items
    /// </summary>
    [Parameter] public string Gap { get; set; } = "var(--space-4)";
    
    #endregion
    
    #region Pagination
    
    /// <summary>
    /// Enable pagination for the grid
    /// </summary>
    [Parameter] public bool EnablePagination { get; set; }
    
    /// <summary>
    /// Number of items per page
    /// </summary>
    [Parameter] public int PageSize { get; set; } = 24;
    
    /// <summary>
    /// Current page index (0-based)
    /// </summary>
    [Parameter] public int CurrentPage { get; set; }
    
    /// <summary>
    /// Callback when page changes
    /// </summary>
    [Parameter] public EventCallback<int> OnPageChanged { get; set; }
    
    #endregion
    
    #region Content
    
    /// <summary>Grid title</summary>
    [Parameter] public string Title { get; set; }
    
    /// <summary>Grid subtitle</summary>
    [Parameter] public string Subtitle { get; set; }
    
    /// <summary>Content to show when no items</summary>
    [Parameter] public RenderFragment EmptyContent { get; set; }
    
    /// <summary>Toolbar actions</summary>
    [Parameter] public RenderFragment ToolbarActions { get; set; }
    
    /// <summary>Event fired when responsive mode changes</summary>
    [Parameter] public EventCallback<GridMode> OnModeChanged { get; set; }
    
    /// <summary>Event fired when more items should be loaded (infinite scroll)</summary>
    [Parameter] public EventCallback OnLoadMore { get; set; }
    
    /// <summary>Grid height (for virtualization)</summary>
    [Parameter] public string Height { get; set; }
    
    /// <summary>Maximum grid height</summary>
    [Parameter] public string MaxHeight { get; set; }
    
    #endregion
    
    protected override void OnParametersSet()
    {
        // Reset type inference when parameters change
        if (_items != Items)
        {
            _itemType = null;
        }
        base.OnParametersSet();
    }
    
    #region Type Inference Helper
    
    private Type InferItemType(object items)
    {
        if (items == null) return typeof(object);
        
        var itemsType = items.GetType();
        
        // Check for generic collection types
        if (itemsType.IsGenericType)
        {
            var genericArgs = itemsType.GetGenericArguments();
            if (genericArgs.Length == 1)
                return genericArgs[0];
        }
        
        // Check for array types
        if (itemsType.IsArray)
            return itemsType.GetElementType();
        
        // Check interfaces for IEnumerable<T>
        foreach (var interfaceType in itemsType.GetInterfaces())
        {
            if (interfaceType.IsGenericType && 
                interfaceType.GetGenericTypeDefinition() == typeof(IEnumerable<>))
            {
                var genericArgs = interfaceType.GetGenericArguments();
                if (genericArgs.Length == 1)
                    return genericArgs[0];
            }
        }
        
        // Examine first item
        if (items is IEnumerable enumerable)
        {
            foreach (var item in enumerable)
            {
                return item?.GetType() ?? typeof(object);
            }
        }
        
        return typeof(object);
    }
    
    #endregion
}
