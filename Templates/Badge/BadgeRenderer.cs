using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using RR.Blazor.Enums;

namespace RR.Blazor.Templates.Badge;

/// <summary>
/// Renders badge templates using HTML output
/// Delegates DOM management to JavaScript via ui-coordinator.js
/// </summary>
/// <typeparam name="T">Type of data being rendered</typeparam>
public class BadgeRenderer<T> where T : class
{
    /// <summary>
    /// Renders the badge template
    /// </summary>
    public RenderFragment Render(BadgeContext<T> context)
    {
        return builder =>
        {
            if (context?.Item == null) return;
            
            var badgeClass = $"badge-{context.Variant.ToString().ToLower()} {context.CssClass}".Trim();
            
            builder.OpenElement(0, "span");
            builder.AddAttribute(1, "class", badgeClass);
            builder.AddAttribute(2, "data-template", "badge");
            
            if (context.Disabled)
                builder.AddAttribute(3, "disabled", true);
                
            if (context.Selected)
                builder.AddAttribute(4, "data-selected", true);
            
            if (context.Clickable && context.OnClick.HasDelegate)
            {
                builder.AddAttribute(5, "onclick", EventCallback.Factory.Create(context.Item, () => context.OnClick.InvokeAsync(context.Item)));
                builder.AddAttribute(6, "style", "cursor: pointer;");
                builder.AddAttribute(7, "data-clickable", true);
            }
            
            if (!string.IsNullOrEmpty(context.Icon))
            {
                builder.OpenElement(8, "i");
                builder.AddAttribute(9, "class", "icon mr-1");
                builder.AddAttribute(10, "data-icon", context.Icon);
                builder.AddContent(11, context.Icon);
                builder.CloseElement();
            }
            
            builder.AddContent(12, context.Text);
            builder.CloseElement();
        };
    }
    
    /// <summary>
    /// Create fluent badge with simplified API
    /// </summary>
    public static RenderFragment<T> Create(
        Func<T, string> textSelector,
        Func<T, VariantType> variantSelector = null,
        Func<T, string> iconSelector = null,
        bool clickable = false,
        EventCallback<T> onClick = default)
    {
        return item => builder =>
        {
            if (item == null) return;
            
            var text = textSelector?.Invoke(item) ?? string.Empty;
            var variant = variantSelector?.Invoke(item) ?? VariantType.Primary;
            var icon = iconSelector?.Invoke(item);
            
            var badgeClass = $"badge-{variant.ToString().ToLower()}";
            builder.OpenElement(0, "span");
            builder.AddAttribute(1, "class", badgeClass);
            builder.AddAttribute(2, "data-template", "badge");
            
            if (clickable && onClick.HasDelegate)
            {
                builder.AddAttribute(3, "onclick", EventCallback.Factory.Create(item, () => onClick.InvokeAsync(item)));
                builder.AddAttribute(4, "style", "cursor: pointer;");
                builder.AddAttribute(5, "data-clickable", true);
            }
            
            if (!string.IsNullOrEmpty(icon))
            {
                builder.OpenElement(6, "i");
                builder.AddAttribute(7, "class", "icon mr-1");
                builder.AddAttribute(8, "data-icon", icon);
                builder.AddContent(9, icon);
                builder.CloseElement();
            }
            
            builder.AddContent(10, text);
            builder.CloseElement();
        };
    }
}