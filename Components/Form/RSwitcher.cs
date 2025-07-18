using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using RR.Blazor.Enums;
using System.Collections;
using System.Reflection;

namespace RR.Blazor.Components.Form;

/// <summary>
/// Smart switcher component that automatically infers value type from SelectedValue or Items.
/// This eliminates the need for explicit TValue specification.
/// </summary>
public class RSwitcher : ComponentBase
{
    [Parameter] public IEnumerable Items { get; set; }
    [Parameter] public object SelectedValue { get; set; }
    [Parameter] public EventCallback<object> SelectedValueChanged { get; set; }
    [Parameter] public Func<object, string> ItemLabelSelector { get; set; }
    [Parameter] public Func<object, string> ItemIconSelector { get; set; }
    [Parameter] public Func<object, bool> ItemDisabledSelector { get; set; }
    [Parameter] public SwitcherVariant Variant { get; set; } = SwitcherVariant.Tabs;
    [Parameter] public ButtonSize Size { get; set; } = ButtonSize.Medium;
    [Parameter] public bool Disabled { get; set; }
    [Parameter] public bool Loading { get; set; }
    [Parameter] public string LoadingText { get; set; } = "Loading...";
    [Parameter] public string Class { get; set; }
    [Parameter] public string AriaLabel { get; set; }
    [Parameter] public EventCallback<object> OnSelectionChanged { get; set; }

    private Type _valueType;
    private bool _valueTypeResolved = false;

    protected override void OnParametersSet()
    {
        if (!_valueTypeResolved)
        {
            _valueType = GetValueType();
            _valueTypeResolved = true;
        }
    }

    private Type GetValueType()
    {
        // Try to infer from SelectedValue first
        if (SelectedValue != null)
        {
            return SelectedValue.GetType();
        }

        // Try to infer from Items collection
        if (Items != null)
        {
            var enumerableType = Items.GetType();
            
            // Check for generic IEnumerable<T>
            var genericInterface = enumerableType.GetInterfaces()
                .FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEnumerable<>));
            
            if (genericInterface != null)
            {
                return genericInterface.GetGenericArguments()[0];
            }

            // Fallback: check first item
            var firstItem = Items.Cast<object>().FirstOrDefault();
            if (firstItem != null)
            {
                return firstItem.GetType();
            }
        }

        // Default to object
        return typeof(object);
    }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        if (_valueType == null)
        {
            _valueType = typeof(object);
        }

        // Use reflection to create the generic component
        var genericSwitcherType = typeof(RSwitcherGeneric<>).MakeGenericType(_valueType);
        
        builder.OpenComponent(0, genericSwitcherType);
        
        // Add all parameters
        builder.AddAttribute(1, "Items", Items);
        builder.AddAttribute(2, "SelectedValue", SelectedValue);
        builder.AddAttribute(3, "SelectedValueChanged", SelectedValueChanged);
        builder.AddAttribute(4, "ItemLabelSelector", ItemLabelSelector);
        builder.AddAttribute(5, "ItemIconSelector", ItemIconSelector);
        builder.AddAttribute(6, "ItemDisabledSelector", ItemDisabledSelector);
        builder.AddAttribute(7, "Variant", Variant);
        builder.AddAttribute(8, "Size", Size);
        builder.AddAttribute(9, "Disabled", Disabled);
        builder.AddAttribute(10, "Loading", Loading);
        builder.AddAttribute(11, "LoadingText", LoadingText);
        builder.AddAttribute(12, "Class", Class);
        builder.AddAttribute(13, "AriaLabel", AriaLabel);
        builder.AddAttribute(14, "OnSelectionChanged", OnSelectionChanged);
        
        builder.CloseComponent();
    }
}