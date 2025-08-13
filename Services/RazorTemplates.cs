using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using System.Linq.Expressions;

namespace RR.Blazor.Services;

/// <summary>
/// Simple Razor template helpers that eliminate RenderTreeBuilder complexity
/// </summary>
public static class RazorTemplates
{
    /// <summary>
    /// Create a simple text display template
    /// </summary>
    public static RenderFragment<T> Text<T>(Func<T, string> getValue) where T : class
    {
        return item => builder =>
        {
            builder.AddContent(0, getValue(item));
        };
    }
    
    /// <summary>
    /// Create a formatted text template
    /// </summary>
    public static RenderFragment<T> Format<T>(Func<T, object> getValue, string format) where T : class
    {
        return item => builder =>
        {
            var value = getValue(item);
            var text = value is IFormattable f ? f.ToString(format, null) : value?.ToString() ?? "-";
            builder.AddContent(0, text);
        };
    }
    
    /// <summary>
    /// Create a badge template
    /// </summary>
    public static RenderFragment<T> Badge<T>(Func<T, string> getValue, Func<T, string> getClass = null) where T : class
    {
        return item => builder =>
        {
            var value = getValue(item);
            var cssClass = getClass?.Invoke(item) ?? "badge-primary";
            
            builder.OpenElement(0, "span");
            builder.AddAttribute(1, "class", $"badge {cssClass}");
            builder.AddContent(2, value);
            builder.CloseElement();
        };
    }
    
    /// <summary>
    /// Create a link template
    /// </summary>
    public static RenderFragment<T> Link<T>(Func<T, string> getText, Func<T, string> getHref) where T : class
    {
        return item => builder =>
        {
            builder.OpenElement(0, "a");
            builder.AddAttribute(1, "href", getHref(item));
            builder.AddAttribute(2, "class", "link");
            builder.AddContent(3, getText(item));
            builder.CloseElement();
        };
    }
    
    /// <summary>
    /// Create an icon button template
    /// </summary>
    public static RenderFragment<T> IconButton<T>(string icon, Action<T> onClick, string tooltip = null) where T : class
    {
        return item => builder =>
        {
            builder.OpenElement(0, "button");
            builder.AddAttribute(1, "type", "button");
            builder.AddAttribute(2, "class", "btn btn-ghost btn-sm");
            if (!string.IsNullOrEmpty(tooltip))
                builder.AddAttribute(3, "title", tooltip);
            builder.AddAttribute(4, "onclick", EventCallback.Factory.Create(null, () => onClick(item)));
            
            builder.OpenElement(5, "i");
            builder.AddAttribute(6, "class", "material-symbols-rounded");
            builder.AddContent(7, icon);
            builder.CloseElement();
            
            builder.CloseElement();
        };
    }
    
    /// <summary>
    /// Create multiple action buttons
    /// </summary>
    public static RenderFragment<T> Actions<T>(params (string icon, string tooltip, Action<T> onClick)[] actions) where T : class
    {
        return item => builder =>
        {
            builder.OpenElement(0, "div");
            builder.AddAttribute(1, "class", "d-flex items-center gap-1");
            
            var seq = 2;
            foreach (var (icon, tooltip, onClick) in actions)
            {
                builder.OpenElement(seq++, "button");
                builder.AddAttribute(seq++, "type", "button");
                builder.AddAttribute(seq++, "class", "btn btn-ghost btn-sm");
                builder.AddAttribute(seq++, "title", tooltip);
                builder.AddAttribute(seq++, "onclick", EventCallback.Factory.Create(null, () => onClick(item)));
                
                builder.OpenElement(seq++, "i");
                builder.AddAttribute(seq++, "class", "material-symbols-rounded");
                builder.AddContent(seq++, icon);
                builder.CloseElement();
                
                builder.CloseElement();
            }
            
            builder.CloseElement();
        };
    }
    
    /// <summary>
    /// Create a checkbox template
    /// </summary>
    public static RenderFragment<T> Checkbox<T>(Func<T, bool> isChecked, Action<T, bool> onChange) where T : class
    {
        return item => builder =>
        {
            builder.OpenElement(0, "input");
            builder.AddAttribute(1, "type", "checkbox");
            builder.AddAttribute(2, "checked", isChecked(item));
            builder.AddAttribute(3, "onchange", EventCallback.Factory.Create<ChangeEventArgs>(null, 
                e => onChange(item, (bool)e.Value)));
            builder.CloseElement();
        };
    }
    
    /// <summary>
    /// Create a currency display template
    /// </summary>
    public static RenderFragment<T> Currency<T>(Func<T, decimal> getValue) where T : class
    {
        return item => builder =>
        {
            var value = getValue(item);
            builder.OpenElement(0, "span");
            builder.AddAttribute(1, "class", "font-mono");
            builder.AddContent(2, value.ToString("C"));
            builder.CloseElement();
        };
    }
    
    /// <summary>
    /// Create a date display template
    /// </summary>
    public static RenderFragment<T> Date<T>(Func<T, DateTime?> getValue, string format = "d") where T : class
    {
        return item => builder =>
        {
            var value = getValue(item);
            var text = value?.ToString(format) ?? "-";
            builder.AddContent(0, text);
        };
    }
    
    /// <summary>
    /// Create a status indicator template
    /// </summary>
    public static RenderFragment<T> Status<T>(Func<T, string> getStatus) where T : class
    {
        return item => builder =>
        {
            var status = getStatus(item)?.ToLowerInvariant() ?? "";
            var (icon, cssClass) = status switch
            {
                "active" or "success" or "completed" => ("check_circle", "text-success"),
                "inactive" or "error" or "failed" => ("cancel", "text-danger"),
                "pending" or "processing" => ("schedule", "text-warning"),
                _ => ("info", "text-secondary")
            };
            
            builder.OpenElement(0, "div");
            builder.AddAttribute(1, "class", "d-flex items-center gap-1");
            
            builder.OpenElement(2, "i");
            builder.AddAttribute(3, "class", $"material-symbols-rounded {cssClass}");
            builder.AddContent(4, icon);
            builder.CloseElement();
            
            builder.OpenElement(5, "span");
            builder.AddContent(6, status);
            builder.CloseElement();
            
            builder.CloseElement();
        };
    }
}