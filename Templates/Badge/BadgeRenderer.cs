using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using RR.Blazor.Components;
using RR.Blazor.Enums;

namespace RR.Blazor.Templates.Badge;

/// <summary>
/// Renders badge templates using RBadge component
/// </summary>
/// <typeparam name="T">Type of data being rendered</typeparam>
public class BadgeRenderer<T> where T : class
{
    /// <summary>
    /// Renders the badge template using RBadge component
    /// </summary>
    public RenderFragment Render(BadgeContext<T> context)
    {
        return builder =>
        {
            if (context?.Item == null) return;
            
            var badgeType = typeof(RR.Blazor.Components.RBadge);
            builder.OpenComponent(0, badgeType);
            builder.AddAttribute(1, "Text", context.Text);
            builder.AddAttribute(2, "Variant", context.Variant);
            builder.AddAttribute(3, "Size", context.Size);
            
            if (!string.IsNullOrEmpty(context.Icon))
                builder.AddAttribute(4, "Icon", context.Icon);
            
            if (context.Disabled)
                builder.AddAttribute(5, "Disabled", true);
            
            if (context.Selected)
                builder.AddAttribute(6, "Selected", true);
            
            if (context.Clickable && context.OnClick.HasDelegate)
            {
                builder.AddAttribute(7, "Clickable", true);
                builder.AddAttribute(8, "OnClick", EventCallback.Factory.Create(context.Item, () => context.OnClick.InvokeAsync(context.Item)));
            }
            
            if (!string.IsNullOrEmpty(context.CssClass))
                builder.AddAttribute(9, "Class", context.CssClass);
            
            builder.CloseComponent();
        };
    }
    
    /// <summary>
    /// Create fluent badge with simplified API using RBadge component
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
            
            var badgeType = typeof(RR.Blazor.Components.RBadge);
            builder.OpenComponent(0, badgeType);
            builder.AddAttribute(1, "Text", text);
            builder.AddAttribute(2, "Variant", variant);
            
            if (!string.IsNullOrEmpty(icon))
                builder.AddAttribute(3, "Icon", icon);
            
            if (clickable && onClick.HasDelegate)
            {
                builder.AddAttribute(4, "Clickable", true);
                builder.AddAttribute(5, "OnClick", EventCallback.Factory.Create(item, () => onClick.InvokeAsync(item)));
            }
            
            builder.CloseComponent();
        };
    }
}