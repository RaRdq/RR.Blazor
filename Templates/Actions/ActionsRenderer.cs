using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using RR.Blazor.Enums;

namespace RR.Blazor.Templates.Actions;

/// <summary>
/// Renderer for actions template
/// </summary>
public class ActionsRenderer<T> where T : class
{
    public RenderFragment Render(ActionsContext<T> context)
    {
        return builder => RenderActions(builder, context, 0);
    }
    
    private void RenderActions(RenderTreeBuilder builder, ActionsContext<T> context, int sequence)
    {
        var visibleActions = GetVisibleActions(context);
        var overflowActions = GetOverflowActions(context, visibleActions);
        
        var alignmentClass = context.Alignment switch
        {
            ActionsAlignment.Start => "justify-start",
            ActionsAlignment.Center => "justify-center",
            ActionsAlignment.End => "justify-end",
            _ => "justify-end"
        };
        
        var densityClass = context.Density switch
        {
            DensityType.Compact => "gap-1",
            DensityType.Dense => "gap-1",
            DensityType.Normal => "gap-2",
            DensityType.Spacious => "gap-3",
            _ => "gap-2"
        };
        
        // Container
        builder.OpenElement(sequence++, "div");
        builder.AddAttribute(sequence++, "class", $"d-flex align-items-center {alignmentClass} {densityClass} {context.CssClass}");
        
        // Render inline actions
        foreach (var action in visibleActions)
        {
            if (ShouldShowAction(action, context.Item))
            {
                RenderActionButton(builder, action, context, ref sequence);
            }
        }
        
        // Render dropdown if needed
        if (overflowActions.Any() && context.DisplayStyle != ActionsDisplayStyle.Inline)
        {
            RenderDropdown(builder, overflowActions, context, ref sequence);
        }
        
        builder.CloseElement();
    }
    
    private void RenderActionButton(RenderTreeBuilder builder, ActionButton<T> action, ActionsContext<T> context, ref int sequence)
    {
        var isEnabled = action.IsEnabled?.Invoke(context.Item) ?? true;
        var buttonStyle = GetButtonStyle(action.Style);
        var variantClass = GetVariantClass(action.Variant, action.Style);
        var sizeClass = GetSizeClass(context.Size);
        
        RenderButton(builder, action, context, isEnabled, buttonStyle, variantClass, sizeClass, sequence);
        sequence += 20;
    }
    
    private void RenderButton(RenderTreeBuilder builder, ActionButton<T> action, ActionsContext<T> context,
        bool isEnabled, string buttonStyle, string variantClass, string sizeClass, int baseSequence)
    {
        builder.OpenElement(baseSequence++, "button");
        builder.AddAttribute(baseSequence++, "type", "button");
        builder.AddAttribute(baseSequence++, "class", $"btn {buttonStyle} {variantClass} {sizeClass} {action.CssClass}");
        builder.AddAttribute(baseSequence++, "disabled", !isEnabled);
        
        // Add tooltip as title attribute
        if (context.ShowTooltips && !string.IsNullOrEmpty(action.Text))
        {
            builder.AddAttribute(baseSequence++, "title", action.Text);
        }
        
        if (isEnabled && action.OnClick.HasDelegate)
        {
            builder.AddAttribute(baseSequence++, "onclick", EventCallback.Factory.Create(this, async () =>
            {
                if (action.RequiresConfirmation)
                {
                    await action.OnClick.InvokeAsync(context.Item);
                }
                else
                {
                    await action.OnClick.InvokeAsync(context.Item);
                }
            }));
        }
        
        // Icon
        if (!string.IsNullOrEmpty(action.Icon))
        {
            builder.OpenElement(baseSequence++, "i");
            builder.AddAttribute(baseSequence++, "class", $"icon {GetIconSizeClass(context.Size)}");
            builder.AddContent(baseSequence++, action.Icon);
            builder.CloseElement();
        }
        
        // Text (if not icon-only)
        if (!action.IconOnly && !string.IsNullOrEmpty(action.Text))
        {
            if (!string.IsNullOrEmpty(action.Icon))
            {
                builder.AddContent(baseSequence++, " ");
            }
            builder.AddContent(baseSequence++, action.Text);
        }
        
        builder.CloseElement();
    }
    
    private void RenderDropdown(RenderTreeBuilder builder, List<ActionButton<T>> actions, ActionsContext<T> context, ref int sequence)
    {
        var sizeClass = GetSizeClass(context.Size);
        
        // Dropdown container
        builder.OpenElement(sequence++, "div");
        builder.AddAttribute(sequence++, "class", "dropdown");
        
        // Dropdown toggle button
        builder.OpenElement(sequence++, "button");
        builder.AddAttribute(sequence++, "type", "button");
        builder.AddAttribute(sequence++, "class", $"btn btn-ghost btn-secondary {sizeClass} dropdown-toggle");
        builder.AddAttribute(sequence++, "data-bs-toggle", "dropdown");
        builder.AddAttribute(sequence++, "aria-expanded", "false");
        
        builder.OpenElement(sequence++, "i");
        builder.AddAttribute(sequence++, "class", $"icon {GetIconSizeClass(context.Size)}");
        builder.AddContent(sequence++, context.DropdownIcon);
        builder.CloseElement();
        
        builder.CloseElement();
        
        // Dropdown menu
        builder.OpenElement(sequence++, "ul");
        builder.AddAttribute(sequence++, "class", "dropdown-menu dropdown-menu-end");
        
        foreach (var action in actions)
        {
            if (ShouldShowAction(action, context.Item))
            {
                var isEnabled = action.IsEnabled?.Invoke(context.Item) ?? true;
                
                builder.OpenElement(sequence++, "li");
                
                builder.OpenElement(sequence++, "button");
                builder.AddAttribute(sequence++, "type", "button");
                builder.AddAttribute(sequence++, "class", $"dropdown-item {(!isEnabled ? "disabled" : "")} {(action.Variant == VariantType.Danger ? "text-danger" : "")}");
                builder.AddAttribute(sequence++, "disabled", !isEnabled);
                
                if (isEnabled && action.OnClick.HasDelegate)
                {
                    builder.AddAttribute(sequence++, "onclick", EventCallback.Factory.Create(this, async () =>
                    {
                        if (action.RequiresConfirmation)
                        {
                            await action.OnClick.InvokeAsync(context.Item);
                        }
                        else
                        {
                            await action.OnClick.InvokeAsync(context.Item);
                        }
                    }));
                }
                
                // Icon
                if (!string.IsNullOrEmpty(action.Icon))
                {
                    builder.OpenElement(sequence++, "i");
                    builder.AddAttribute(sequence++, "class", "icon me-2 text-base");
                    builder.AddContent(sequence++, action.Icon);
                    builder.CloseElement();
                }
                
                // Text
                builder.AddContent(sequence++, action.Text ?? action.Id);
                
                builder.CloseElement();
                builder.CloseElement();
            }
        }
        
        builder.CloseElement();
        builder.CloseElement();
    }
    
    private List<ActionButton<T>> GetVisibleActions(ActionsContext<T> context)
    {
        if (context.DisplayStyle == ActionsDisplayStyle.Dropdown)
            return new List<ActionButton<T>>();
            
        return context.Actions
            .Where(a => ShouldShowAction(a, context.Item))
            .Take(context.MaxVisibleActions)
            .ToList();
    }
    
    private List<ActionButton<T>> GetOverflowActions(ActionsContext<T> context, List<ActionButton<T>> visibleActions)
    {
        if (context.DisplayStyle == ActionsDisplayStyle.Inline)
            return new List<ActionButton<T>>();
            
        if (context.DisplayStyle == ActionsDisplayStyle.Dropdown)
            return context.Actions.Where(a => ShouldShowAction(a, context.Item)).ToList();
            
        // Mixed mode
        return context.Actions
            .Where(a => ShouldShowAction(a, context.Item) && !visibleActions.Contains(a))
            .ToList();
    }
    
    private bool ShouldShowAction(ActionButton<T> action, T item)
    {
        if (action.IsVisible != null)
            return action.IsVisible(item);
            
        return true;
    }
    
    private string GetButtonStyle(ButtonStyle style) => style switch
    {
        ButtonStyle.Filled => "btn-filled",
        ButtonStyle.Outlined => "btn-outlined",
        ButtonStyle.Ghost => "btn-ghost",
        ButtonStyle.Text => "btn-text",
        _ => "btn-ghost"
    };
    
    private string GetVariantClass(VariantType variant, ButtonStyle style) => variant switch
    {
        VariantType.Primary => "btn-primary",
        VariantType.Secondary => "btn-secondary",
        VariantType.Success => "btn-success",
        VariantType.Warning => "btn-warning",
        VariantType.Danger => "btn-danger",
        VariantType.Info => "btn-info",
        _ => "btn-secondary"
    };
    
    private string GetSizeClass(SizeType size) => size switch
    {
        SizeType.ExtraSmall => "btn-xs",
        SizeType.Small => "btn-sm",
        SizeType.Medium => "btn-md",
        SizeType.Large => "btn-lg",
        SizeType.ExtraLarge => "btn-xl",
        _ => "btn-sm"
    };
    
    private string GetIconSizeClass(SizeType size) => size switch
    {
        SizeType.ExtraSmall => "text-xs",
        SizeType.Small => "text-base",
        SizeType.Medium => "text-lg",
        SizeType.Large => "text-xl",
        SizeType.ExtraLarge => "text-2xl",
        _ => "text-base"
    };
}