using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using RR.Blazor.Components.Base;
using RR.Blazor.Enums;

namespace RR.Blazor.Components.Form;

/// <summary>
/// Smart toggle component that defaults to boolean values for most common use cases.
/// This eliminates the need for explicit TValue specification for boolean toggles.
/// </summary>
public class RToggle : ComponentBase
{
    [Parameter] public bool Value { get; set; }
    [Parameter] public EventCallback<bool> ValueChanged { get; set; }
    [Parameter] public string Text { get; set; }
    [Parameter] public string TrueText { get; set; }
    [Parameter] public string FalseText { get; set; }
    [Parameter] public string TrueIcon { get; set; }
    [Parameter] public string FalseIcon { get; set; }
    [Parameter] public ToggleVariant Variant { get; set; } = ToggleVariant.Standard;
    [Parameter] public ButtonSize Size { get; set; } = ButtonSize.Medium;
    [Parameter] public bool Disabled { get; set; }
    [Parameter] public bool Loading { get; set; }
    [Parameter] public string Class { get; set; }
    [Parameter] public string AriaLabel { get; set; }
    [Parameter] public EventCallback<bool> OnToggle { get; set; }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        // Use the generic component with bool type
        builder.OpenComponent<RToggleGeneric<bool>>(0);
        
        // Explicitly forward the Value and ValueChanged parameters
        builder.AddAttribute(1, "Value", Value);
        builder.AddAttribute(2, "ValueChanged", ValueChanged);
        
        // Forward all other parameters except Value and ValueChanged to prevent @bind issues
        var seq = 10;
        builder.ForwardParameters(ref seq, this, "Value", "ValueChanged");
        
        builder.CloseComponent();
    }
}