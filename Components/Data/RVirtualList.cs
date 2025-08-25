using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using RR.Blazor.Enums;
using RR.Blazor.Components.Base;
using System.Collections;

namespace RR.Blazor.Components.Data;

/// <summary>
/// Smart virtual list component that automatically infers item type from Items collection.
/// This eliminates the need for explicit T specification for virtualized lists.
/// </summary>
public class RVirtualList<TItem> : RComponentBase
{
    [Parameter] public IEnumerable<TItem> Items { get; set; }
    [Parameter] public RenderFragment<TItem> ItemTemplate { get; set; }
    [Parameter] public RenderFragment<TItem> LoadingTemplate { get; set; }
    [Parameter] public RenderFragment EmptyTemplate { get; set; }
    [Parameter] public string Height { get; set; } = "400px";
    [Parameter] public string ItemHeight { get; set; } = "auto";
    [Parameter] public int OverscanCount { get; set; } = 5;
    [Parameter] public bool AutoLoad { get; set; } = false;
    [Parameter] public EventCallback OnEndReached { get; set; }
    [Parameter] public EventCallback<TItem> OnItemClick { get; set; }
    [Parameter] public bool ShowScrollbar { get; set; } = true;
    [Parameter] public string ItemClass { get; set; }


    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        // Create the generic component directly
        builder.OpenComponent<RVirtualListGeneric<TItem>>(0);
        
        // Add all parameters
        builder.AddAttribute(1, "Items", Items);
        builder.AddAttribute(2, "ItemTemplate", ItemTemplate);
        builder.AddAttribute(3, "LoadingTemplate", LoadingTemplate);
        builder.AddAttribute(4, "EmptyTemplate", EmptyTemplate);
        builder.AddAttribute(5, "Height", Height);
        builder.AddAttribute(6, "ItemHeight", ItemHeight);
        builder.AddAttribute(7, "OverscanCount", OverscanCount);
        builder.AddAttribute(8, "AutoLoad", AutoLoad);
        builder.AddAttribute(9, "OnEndReached", OnEndReached);
        builder.AddAttribute(10, "OnItemClick", OnItemClick);
        builder.AddAttribute(11, "ShowScrollbar", ShowScrollbar);
        builder.AddAttribute(12, "Class", Class);
        builder.AddAttribute(13, "ItemClass", ItemClass);
        
        builder.CloseComponent();
    }
}