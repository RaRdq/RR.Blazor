using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using RR.Blazor.Components.Base;
using RR.Blazor.Enums;

namespace RR.Blazor.Components.Form;

/// <summary>
/// Smart toggle component that defaults to boolean values for most common use cases.
/// This eliminates the need for explicit TValue specification for boolean toggles.
/// </summary>
public class RToggle : RComponentBase
{
    [Parameter] public bool Value { get; set; }
    [Parameter] public EventCallback<bool> ValueChanged { get; set; }
    [Parameter] public string Text { get; set; }
    [Parameter] public string TrueText { get; set; }
    [Parameter] public string FalseText { get; set; }
    [Parameter] public string TrueIcon { get; set; }
    [Parameter] public string FalseIcon { get; set; }
    [Parameter] public ToggleVariant Variant { get; set; } = ToggleVariant.Standard;
    [Parameter] public SizeType Size { get; set; } = SizeType.Medium;
    [Parameter] public bool Loading { get; set; }
    [Parameter] public string AriaLabel { get; set; }
    [Parameter] public EventCallback<bool> OnToggle { get; set; }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        // Use the generic component with bool type
        builder.OpenComponent<RToggleGeneric<bool>>(0);
        
        // Forward Value and ValueChanged properly for @bind-Value support
        builder.AddAttribute(1, "Value", Value);
        builder.AddAttribute(2, "ValueChanged", ValueChanged);
        
        // Forward all other parameters manually to ensure compatibility
        builder.AddAttribute(3, "Text", Text);
        builder.AddAttribute(4, "TrueText", TrueText);
        builder.AddAttribute(5, "FalseText", FalseText);
        builder.AddAttribute(6, "TrueIcon", TrueIcon);
        builder.AddAttribute(7, "FalseIcon", FalseIcon);
        builder.AddAttribute(8, "Variant", Variant);
        builder.AddAttribute(9, "Size", Size);
        builder.AddAttribute(10, "Disabled", Disabled);
        builder.AddAttribute(11, "Loading", Loading);
        builder.AddAttribute(12, "Class", Class);
        builder.AddAttribute(13, "AriaLabel", AriaLabel);
        builder.AddAttribute(14, "OnToggle", OnToggle);
        
        builder.CloseComponent();
    }
}