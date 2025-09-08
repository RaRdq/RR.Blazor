using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using RR.Blazor.Models;
using RR.Blazor.Components.Base;

namespace RR.Blazor.Components.Data;

[CascadingTypeParameter(nameof(TItem))]
public class RGridForwarder<TItem> : RComponentBase where TItem : class
{
    #region Core Parameters
    [Parameter] public IEnumerable<TItem> Items { get; set; }
    [Parameter] public GridMode Mode { get; set; } = GridMode.Auto;
    [Parameter] public RenderFragment<TItem> ItemTemplate { get; set; }
    
    #endregion
    
    #region Filter Parameters
    [Parameter] public object Filter { get; set; }
    [Parameter] public bool EnableFilter { get; set; }
    [Parameter] public bool ShowQuickSearch { get; set; } = true;
    
    #endregion
    
    #region Responsive Configuration
    [Parameter] public int Columns { get; set; } = 4;
    [Parameter] public int ColumnsXs { get; set; } = 1;
    [Parameter] public int ColumnsSm { get; set; } = 2;
    [Parameter] public int ColumnsMd { get; set; } = 3;
    [Parameter] public int ColumnsLg { get; set; } = 4;
    [Parameter] public int ColumnsXl { get; set; } = 6;
    [Parameter] public GridMode? ModeXs { get; set; }
    [Parameter] public GridMode? ModeSm { get; set; }
    [Parameter] public GridMode? ModeMd { get; set; }
    [Parameter] public GridMode? ModeLg { get; set; }
    [Parameter] public GridMode? ModeXl { get; set; }
    
    #endregion
    
    #region Performance Features
    [Parameter] public bool EnableVirtualization { get; set; }
    [Parameter] public int VirtualizationThreshold { get; set; } = 1000;
    [Parameter] public string Gap { get; set; } = "var(--space-4)";
    
    #endregion
    
    #region Pagination
    [Parameter] public bool EnablePagination { get; set; }
    [Parameter] public int PageSize { get; set; } = 24;
    [Parameter] public int CurrentPage { get; set; } = 1;
    [Parameter] public EventCallback<int> OnPageChanged { get; set; }
    [Parameter] public EventCallback<int> PageSizeChanged { get; set; }
    
    #endregion
    
    #region Content Parameters
    [Parameter] public string Title { get; set; }
    [Parameter] public string Subtitle { get; set; }
    [Parameter] public RenderFragment EmptyContent { get; set; }
    [Parameter] public RenderFragment ToolbarActions { get; set; }
    [Parameter] public EventCallback<GridMode> OnModeChanged { get; set; }
    [Parameter] public EventCallback OnLoadMore { get; set; }
    [Parameter] public string Height { get; set; }
    [Parameter] public string MaxHeight { get; set; }
    
    #endregion
    
    #region Event Parameters
    [Parameter] public EventCallback<TItem> OnItemClicked { get; set; }
    [Parameter] public Func<TItem, string> ItemClass { get; set; }
    
    #endregion
    
    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        var gridContext = new GridContext(typeof(TItem), $"grid-{GetHashCode()}", true);
        
        builder.OpenComponent<CascadingValue<GridContext>>(0);
        builder.AddAttribute(1, "Value", gridContext);
        builder.AddAttribute(2, "ChildContent", (RenderFragment)(contextBuilder =>
        {
            contextBuilder.OpenComponent<RGridGeneric<TItem>>(0);
            
            // Core parameters
            contextBuilder.AddAttribute(1, "Items", Items);
            contextBuilder.AddAttribute(2, "Mode", Mode);
            contextBuilder.AddAttribute(3, "ItemTemplate", ItemTemplate);
            
            // Toolbar parameters (matching RGridGeneric's parameters)
            contextBuilder.AddAttribute(4, "ShowFilters", EnableFilter);
            contextBuilder.AddAttribute(5, "ShowSearch", ShowQuickSearch);
            
            // Responsive configuration
            contextBuilder.AddAttribute(6, "Columns", Columns);
            contextBuilder.AddAttribute(7, "ColumnsXs", ColumnsXs);
            contextBuilder.AddAttribute(8, "ColumnsSm", ColumnsSm);
            contextBuilder.AddAttribute(9, "ColumnsMd", ColumnsMd);
            contextBuilder.AddAttribute(10, "ColumnsLg", ColumnsLg);
            contextBuilder.AddAttribute(11, "ColumnsXl", ColumnsXl);
            contextBuilder.AddAttribute(12, "Gap", Gap);
            
            // Performance features
            contextBuilder.AddAttribute(13, "EnableVirtualization", EnableVirtualization);
            contextBuilder.AddAttribute(14, "VirtualizationThreshold", VirtualizationThreshold);
            
            // Pagination
            contextBuilder.AddAttribute(15, "EnablePagination", EnablePagination);
            contextBuilder.AddAttribute(16, "PageSize", PageSize);
            contextBuilder.AddAttribute(17, "CurrentPage", CurrentPage);
            contextBuilder.AddAttribute(18, "CurrentPageChanged", OnPageChanged);
            contextBuilder.AddAttribute(19, "PageSizeChanged", PageSizeChanged);
            
            // Content parameters
            contextBuilder.AddAttribute(20, "Title", Title);
            contextBuilder.AddAttribute(21, "Subtitle", Subtitle);
            contextBuilder.AddAttribute(22, "EmptyContent", EmptyContent);
            contextBuilder.AddAttribute(23, "ToolbarContent", ToolbarActions);
            contextBuilder.AddAttribute(24, "Height", Height);
            contextBuilder.AddAttribute(25, "MaxHeight", MaxHeight);
            
            // Event parameters
            contextBuilder.AddAttribute(26, "OnItemClicked", OnItemClicked);
            contextBuilder.AddAttribute(27, "OnModeChanged", OnModeChanged);
            
            // Forward any remaining attributes from AdditionalAttributes (excluding handled parameters)
            if (AdditionalAttributes != null && AdditionalAttributes.Count > 0)
            {
                var handledParams = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
                {
                    "Items", "Mode", "ItemTemplate", "ShowFilters", "ShowSearch",
                    "Columns", "ColumnsXs", "ColumnsSm", "ColumnsMd", "ColumnsLg", "ColumnsXl", "Gap",
                    "EnableVirtualization", "VirtualizationThreshold", "EnablePagination", "PageSize",
                    "CurrentPage", "CurrentPageChanged", "PageSizeChanged", "Title", "Subtitle", "EmptyContent",
                    "ToolbarContent", "Height", "MaxHeight", "OnItemClicked", "OnModeChanged"
                };
                
                var remainingAttributes = AdditionalAttributes
                    .Where(attr => !handledParams.Contains(attr.Key))
                    .ToDictionary(attr => attr.Key, attr => attr.Value);
                
                if (remainingAttributes.Any())
                {
                    contextBuilder.AddMultipleAttributes(28, remainingAttributes);
                }
            }
            
            contextBuilder.AddAttribute(29, "ChildContent", ChildContent);
            contextBuilder.CloseComponent();
        }));
        builder.CloseComponent();
    }
}

/// <summary>
/// Extension to simplify grid creation with compile-time type resolution
/// </summary>
public static class RGridExtensions
{
    /// <summary>
    /// Creates a typed grid forwarder at compile time
    /// </summary>
    public static RenderFragment CreateGrid<TItem>(
        IEnumerable<TItem> items,
        RenderFragment itemTemplate = null,
        Dictionary<string, object> attributes = null) where TItem : class
    {
        return builder =>
        {
            builder.OpenComponent<RGridForwarder<TItem>>(0);
            builder.AddAttribute(1, "Items", items);
            if (itemTemplate != null)
            {
                builder.AddAttribute(2, "ItemTemplate", itemTemplate);
            }
            if (attributes != null)
            {
                builder.AddMultipleAttributes(3, attributes);
            }
            builder.CloseComponent();
        };
    }
}