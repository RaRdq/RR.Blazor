using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using RR.Blazor.Attributes;
using RR.Blazor.Enums;

namespace RR.Blazor.Components.Form;

/// <summary>
/// Base class for choice components with shared parameters
/// </summary>
public abstract class RChoiceBase : ComponentBase
{
    [Parameter] public EventCallback<object> SelectedValueChanged { get; set; }
    [Parameter] public Func<object, string> ItemLabelSelector { get; set; }
    [Parameter] public Func<object, string> ItemIconSelector { get; set; }
    [Parameter] public Func<object, string> ItemTitleSelector { get; set; }
    [Parameter] public Func<object, string> ItemAriaLabelSelector { get; set; }
    [Parameter] public Func<object, bool> ItemDisabledSelector { get; set; }
    [Parameter] public Func<object, bool> ItemLoadingSelector { get; set; }
    [Parameter] public bool ShowLabels { get; set; } = true;
    [Parameter] public bool ShowActiveIndicator { get; set; } = false;
    [Parameter] public ChoiceSize Size { get; set; } = ChoiceSize.Medium;
    [Parameter] public ChoiceDirection Direction { get; set; } = ChoiceDirection.Horizontal;
    [Parameter] public string AdditionalClass { get; set; }
    [Parameter] public bool Disabled { get; set; } = false;
    [Parameter] public bool CloseOnSelect { get; set; } = true;
    [Parameter] public string AriaLabel { get; set; }
    [Parameter] public int? MaxItemsInline { get; set; } = 5;
    [Parameter] public int? MaxLabelLength { get; set; } = 20;
    [Parameter] public string Class { get; set; }
    [Parameter] public RenderFragment ChildContent { get; set; }
}

/// <summary>
/// Smart choice component that automatically detects whether to use inline or dropdown mode
/// Based on item count, content length, screen size, and other factors
/// </summary>
[Component("RChoice", Category = "Form")]
[AIOptimized(Prompt = "Create smart choice component that auto-detects inline vs dropdown")]
public class RChoice : RChoiceBase
{
    [Parameter, AIParameter("Collection of items to choose between", "new[] { \"Option1\", \"Option2\" }")]
    public object Items { get; set; }
    
    [Parameter, AIParameter("Currently selected value", "selectedOption")]
    public object SelectedValue { get; set; }
    
    [Parameter, AIParameter("Force specific variant", "ChoiceVariant.Auto for smart detection")]
    public ChoiceVariant Variant { get; set; } = ChoiceVariant.Auto;
    
    [Parameter, AIParameter("Style variant for inline mode", "ChoiceStyle.Standard")]
    public ChoiceStyle Style { get; set; } = ChoiceStyle.Standard;

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        if (Items == null) return;
        
        var itemsCollection = Items as System.Collections.IEnumerable ?? new[] { Items };
        var itemsList = itemsCollection.Cast<object>().ToList();
        
        // Smart detection logic
        var effectiveVariant = GetEffectiveVariant(itemsList);
        var itemType = Items.GetType().IsGenericType 
            ? Items.GetType().GetGenericArguments()[0] 
            : typeof(object);
        
        var genericChoiceType = typeof(RChoiceGeneric<>).MakeGenericType(itemType);
        
        builder.OpenComponent(0, genericChoiceType);
        
        // Forward all base parameters
        ForwardBaseParameters(builder, itemType);
        
        // Set computed variant
        builder.AddAttribute(1, "EffectiveVariant", effectiveVariant);
        builder.AddAttribute(2, "Items", Items);
        builder.AddAttribute(3, "SelectedValue", SelectedValue);
        
        builder.CloseComponent();
    }
    
    private ChoiceVariant GetEffectiveVariant(List<object> items)
    {
        // Explicit variant takes precedence
        if (Variant != ChoiceVariant.Auto)
            return Variant;
        
        // Smart detection rules
        
        // Rule 1: Too many items? Use dropdown
        if (MaxItemsInline.HasValue && items.Count > MaxItemsInline.Value)
            return ChoiceVariant.Dropdown;
        
        // Rule 2: Long text content? Use dropdown  
        if (MaxLabelLength.HasValue && HasLongLabels(items))
            return ChoiceVariant.Dropdown;
        
        // Rule 3: Vertical direction with many items? Use dropdown
        if (Direction == ChoiceDirection.Vertical && items.Count > 3)
            return ChoiceVariant.Dropdown;
        
        // Rule 4: Default to inline for simple cases
        return ChoiceVariant.Inline;
    }
    
    private bool HasLongLabels(List<object> items)
    {
        if (!MaxLabelLength.HasValue) return false;
        
        return items.Any(item =>
        {
            var label = ItemLabelSelector?.Invoke(item) ?? item?.ToString() ?? "";
            return label.Length > MaxLabelLength.Value;
        });
    }
    
    private void ForwardBaseParameters(RenderTreeBuilder builder, Type itemType)
    {
        // Forward events using object-based parameters - only add if they have delegates
        if (SelectedValueChanged.HasDelegate)
            builder.AddAttribute(100, "SelectedValueChangedObject", SelectedValueChanged);
        builder.AddAttribute(101, "ItemLabelSelectorTyped", ItemLabelSelector);
        builder.AddAttribute(102, "ItemIconSelectorTyped", ItemIconSelector);
        builder.AddAttribute(103, "ItemTitleSelectorTyped", ItemTitleSelector);
        builder.AddAttribute(104, "ItemAriaLabelSelectorTyped", ItemAriaLabelSelector);
        builder.AddAttribute(105, "ItemDisabledSelectorTyped", ItemDisabledSelector);
        builder.AddAttribute(106, "ItemLoadingSelectorTyped", ItemLoadingSelector);
        builder.AddAttribute(107, "ShowLabels", ShowLabels);
        builder.AddAttribute(108, "ShowActiveIndicator", ShowActiveIndicator);
        builder.AddAttribute(109, "Size", Size);
        builder.AddAttribute(110, "Direction", Direction);
        builder.AddAttribute(111, "AdditionalClass", AdditionalClass);
        builder.AddAttribute(112, "Disabled", Disabled);
        builder.AddAttribute(113, "CloseOnSelect", CloseOnSelect);
        builder.AddAttribute(114, "AriaLabel", AriaLabel);
        builder.AddAttribute(115, "Style", Style);
        builder.AddAttribute(116, "Class", Class);
        builder.AddAttribute(117, "ChildContent", ChildContent);
    }
}

public enum ChoiceVariant
{
    Auto,       // Smart detection based on content and context
    Inline,     // Always show as inline switcher
    Dropdown    // Always show as dropdown
}

public enum ChoiceStyle
{
    Standard,   // Default styling
    Compact,    // Compact layout with no gaps
    Pills,      // Pill-shaped items
    Tabs,       // Tab-style items
    Buttons     // Button-style items
}

public enum ChoiceSize
{
    Small,
    Medium,
    Large
}

public enum ChoiceDirection
{
    Horizontal,
    Vertical
}