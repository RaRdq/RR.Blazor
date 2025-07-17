using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using System.Collections;

namespace RR.Blazor.Components.Feedback;

/// <summary>
/// Smart select modal component that automatically infers item type from Items collection.
/// This eliminates the need for explicit T specification for item selection modals.
/// </summary>
public class RSelectModal<TItem> : ComponentBase
{
    [Parameter] public bool IsOpen { get; set; }
    [Parameter] public EventCallback<bool> IsOpenChanged { get; set; }
    [Parameter] public string Title { get; set; }
    [Parameter] public string SearchPlaceholder { get; set; } = "Search...";
    [Parameter] public bool ShowSearch { get; set; } = true;
    [Parameter] public bool MultiSelect { get; set; } = false;
    [Parameter] public IEnumerable<TItem> Items { get; set; }
    [Parameter] public TItem SelectedItem { get; set; }
    [Parameter] public EventCallback<TItem> SelectedItemChanged { get; set; }
    [Parameter] public IEnumerable<TItem> SelectedItems { get; set; }
    [Parameter] public EventCallback<IEnumerable<TItem>> SelectedItemsChanged { get; set; }
    [Parameter] public RenderFragment<TItem> ItemTemplate { get; set; }
    [Parameter] public Func<TItem, string> ItemLabelSelector { get; set; }
    [Parameter] public Func<TItem, string> ItemSearchSelector { get; set; }
    [Parameter] public Func<TItem, bool> ItemDisabledSelector { get; set; }
    [Parameter] public string EmptyText { get; set; } = "No items found";
    [Parameter] public string LoadingText { get; set; } = "Loading...";
    [Parameter] public bool Loading { get; set; } = false;
    [Parameter] public string Class { get; set; }
    [Parameter] public EventCallback<TItem> OnItemClick { get; set; }
    [Parameter] public EventCallback OnCancel { get; set; }
    [Parameter] public EventCallback OnConfirm { get; set; }


    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        // Create the generic component directly
        builder.OpenComponent<RSelectModalGeneric<TItem>>(0);
        
        // Add all parameters
        builder.AddAttribute(1, "IsOpen", IsOpen);
        builder.AddAttribute(2, "IsOpenChanged", IsOpenChanged);
        builder.AddAttribute(3, "Title", Title);
        builder.AddAttribute(4, "SearchPlaceholder", SearchPlaceholder);
        builder.AddAttribute(5, "ShowSearch", ShowSearch);
        builder.AddAttribute(6, "MultiSelect", MultiSelect);
        builder.AddAttribute(7, "Items", Items);
        builder.AddAttribute(8, "SelectedItem", SelectedItem);
        builder.AddAttribute(9, "SelectedItemChanged", SelectedItemChanged);
        builder.AddAttribute(10, "SelectedItems", SelectedItems);
        builder.AddAttribute(11, "SelectedItemsChanged", SelectedItemsChanged);
        builder.AddAttribute(12, "ItemTemplate", ItemTemplate);
        builder.AddAttribute(13, "ItemLabelSelector", ItemLabelSelector);
        builder.AddAttribute(14, "ItemSearchSelector", ItemSearchSelector);
        builder.AddAttribute(15, "ItemDisabledSelector", ItemDisabledSelector);
        builder.AddAttribute(16, "EmptyText", EmptyText);
        builder.AddAttribute(17, "LoadingText", LoadingText);
        builder.AddAttribute(18, "Loading", Loading);
        builder.AddAttribute(19, "Class", Class);
        builder.AddAttribute(20, "OnItemClick", OnItemClick);
        builder.AddAttribute(21, "OnCancel", OnCancel);
        builder.AddAttribute(22, "OnConfirm", OnConfirm);
        
        builder.CloseComponent();
    }
}