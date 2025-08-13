using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using System.Linq.Expressions;

namespace RR.Blazor.Services;

/// <summary>
/// Type-safe template builders for common column scenarios
/// </summary>
public static class ColumnTemplateBuilder
{
    /// <summary>
    /// Create a simple text template with optional formatting
    /// </summary>
    public static RenderFragment<TItem> Text<TItem>(Expression<Func<TItem, object>> property, string format = null) where TItem : class
    {
        var getter = property.Compile();
        return item =>
        {
            var value = getter(item);
            var text = value?.ToString() ?? "-";
            if (!string.IsNullOrEmpty(format) && value is IFormattable formattable)
            {
                text = formattable.ToString(format, null);
            }
            return builder => builder.AddContent(0, text);
        };
    }

    /// <summary>
    /// Create a badge template for status values
    /// </summary>
    public static RenderFragment<TItem> StatusBadge<TItem>(Expression<Func<TItem, object>> property, 
        Dictionary<string, string> statusClasses = null) where TItem : class
    {
        var getter = property.Compile();
        statusClasses ??= GetDefaultStatusClasses();
        
        return item =>
        {
            var value = getter(item)?.ToString()?.ToLowerInvariant() ?? "";
            var badgeClass = statusClasses.ContainsKey(value) ? statusClasses[value] : "badge-secondary";
            
            return builder =>
            {
                builder.OpenElement(0, "span");
                builder.AddAttribute(1, "class", $"badge {badgeClass}");
                builder.AddContent(2, value);
                builder.CloseElement();
            };
        };
    }

    /// <summary>
    /// Create action buttons template
    /// </summary>
    public static RenderFragment<TItem> ActionButtons<TItem>(params (string icon, string title, Action<TItem> onClick)[] actions) where TItem : class
    {
        return item => builder =>
        {
            builder.OpenElement(0, "div");
            builder.AddAttribute(1, "class", "d-flex items-center gap-1");
            
            var seq = 2;
            foreach (var (icon, title, onClick) in actions)
            {
                builder.OpenElement(seq++, "button");
                builder.AddAttribute(seq++, "type", "button");
                builder.AddAttribute(seq++, "class", "btn btn-ghost btn-sm");
                builder.AddAttribute(seq++, "title", title);
                builder.AddAttribute(seq++, "onclick", EventCallback.Factory.Create<MouseEventArgs>(null, () => onClick(item)));
                
                builder.OpenElement(seq++, "i");
                builder.AddAttribute(seq++, "class", "material-symbols-rounded text-base");
                builder.AddContent(seq++, icon);
                builder.CloseElement();
                
                builder.CloseElement();
            }
            
            builder.CloseElement();
        };
    }

    private static Dictionary<string, string> GetDefaultStatusClasses()
    {
        return new Dictionary<string, string>
        {
            { "active", "badge-success" },
            { "enabled", "badge-success" },
            { "success", "badge-success" },
            { "completed", "badge-success" },
            { "approved", "badge-success" },
            { "inactive", "badge-danger" },
            { "disabled", "badge-danger" },
            { "failed", "badge-danger" },
            { "error", "badge-danger" },
            { "rejected", "badge-danger" },
            { "pending", "badge-warning" },
            { "processing", "badge-warning" },
            { "in progress", "badge-warning" },
            { "awaiting", "badge-warning" },
            { "draft", "badge-info" },
            { "new", "badge-info" },
            { "created", "badge-info" },
            { "cancelled", "badge-secondary" },
            { "canceled", "badge-secondary" }
        };
    }
}
