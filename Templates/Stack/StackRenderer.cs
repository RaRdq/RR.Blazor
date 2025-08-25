using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace RR.Blazor.Templates.Stack;

/// <summary>
/// Renders stack templates for multi-line content
/// Delegates DOM management to JavaScript via ui-coordinator.js
/// </summary>
/// <typeparam name="T">Type of data being rendered</typeparam>
public class StackRenderer<T> where T : class
{
    /// <summary>
    /// Renders the stack template
    /// </summary>
    public RenderFragment Render(StackContext<T> context)
    {
        return builder =>
        {
            if (context?.Item == null) return;
            
            var containerClass = context.Orientation == StackOrientation.Vertical 
                ? "d-flex flex-col" 
                : "d-flex align-center gap-2";
            
            builder.OpenElement(0, "div");
            builder.AddAttribute(1, "class", $"{containerClass} {context.CssClass}".Trim());
            builder.AddAttribute(2, "data-template", "stack");
            builder.AddAttribute(3, "data-orientation", context.Orientation.ToString().ToLowerInvariant());
            
            if (context.Disabled)
                builder.AddAttribute(4, "disabled", true);
                
            if (context.Selected)
                builder.AddAttribute(5, "data-selected", true);
            
            if (!string.IsNullOrEmpty(context.Icon))
            {
                builder.OpenElement(6, "i");
                builder.AddAttribute(7, "class", "icon mr-2");
                builder.AddAttribute(8, "data-icon", context.Icon);
                builder.AddContent(9, context.Icon);
                builder.CloseElement();
            }
            
            builder.OpenElement(10, "div");
            builder.AddAttribute(11, "class", "stack-content");
            
            // Primary text
            if (!string.IsNullOrEmpty(context.PrimaryText))
            {
                builder.OpenElement(12, "div");
                builder.AddAttribute(13, "class", "stack-primary font-medium");
                builder.AddAttribute(14, "data-level", "primary");
                builder.AddContent(15, context.PrimaryText);
                builder.CloseElement();
            }
            
            // Secondary text
            if (!string.IsNullOrEmpty(context.SecondaryText))
            {
                builder.OpenElement(16, "div");
                builder.AddAttribute(17, "class", "stack-secondary text-sm text-muted");
                builder.AddAttribute(18, "data-level", "secondary");
                builder.AddContent(19, context.SecondaryText);
                builder.CloseElement();
            }
            
            // Tertiary text
            if (!string.IsNullOrEmpty(context.TertiaryText))
            {
                builder.OpenElement(20, "div");
                builder.AddAttribute(21, "class", "stack-tertiary text-xs text-muted");
                builder.AddAttribute(22, "data-level", "tertiary");
                builder.AddContent(23, context.TertiaryText);
                builder.CloseElement();
            }
            
            builder.CloseElement(); // stack-content
            builder.CloseElement(); // container
        };
    }
    
    /// <summary>
    /// Create fluent stack with simplified API
    /// </summary>
    public static RenderFragment<T> Create(
        Func<T, string> primarySelector,
        Func<T, string> secondarySelector = null,
        Func<T, string> iconSelector = null,
        StackOrientation orientation = StackOrientation.Vertical)
    {
        return item => builder =>
        {
            if (item == null) return;
            
            var primaryText = primarySelector?.Invoke(item) ?? string.Empty;
            var secondaryText = secondarySelector?.Invoke(item);
            var icon = iconSelector?.Invoke(item);
            
            var containerClass = orientation == StackOrientation.Vertical 
                ? "d-flex flex-col" 
                : "d-flex align-center gap-2";
            
            builder.OpenElement(0, "div");
            builder.AddAttribute(1, "class", containerClass);
            builder.AddAttribute(2, "data-template", "stack");
            builder.AddAttribute(3, "data-orientation", orientation.ToString().ToLowerInvariant());
            
            if (!string.IsNullOrEmpty(icon))
            {
                builder.OpenElement(4, "i");
                builder.AddAttribute(5, "class", "icon mr-2");
                builder.AddAttribute(6, "data-icon", icon);
                builder.AddContent(7, icon);
                builder.CloseElement();
            }
            
            builder.OpenElement(8, "div");
            builder.AddAttribute(9, "class", "stack-content");
            
            // Primary text
            builder.OpenElement(10, "div");
            builder.AddAttribute(11, "class", "stack-primary font-medium");
            builder.AddAttribute(12, "data-level", "primary");
            builder.AddContent(13, primaryText);
            builder.CloseElement();
            
            // Secondary text
            if (!string.IsNullOrEmpty(secondaryText))
            {
                builder.OpenElement(14, "div");
                builder.AddAttribute(15, "class", "stack-secondary text-sm text-muted");
                builder.AddAttribute(16, "data-level", "secondary");
                builder.AddContent(17, secondaryText);
                builder.CloseElement();
            }
            
            builder.CloseElement(); // stack-content
            builder.CloseElement(); // container
        };
    }
}