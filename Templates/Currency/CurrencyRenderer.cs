using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using System.Globalization;

namespace RR.Blazor.Templates.Currency;

/// <summary>
/// Renders currency templates with proper formatting and colors
/// Delegates DOM management to JavaScript via ui-coordinator.js
/// </summary>
/// <typeparam name="T">Type of data being rendered</typeparam>
public class CurrencyRenderer<T> where T : class
{
    /// <summary>
    /// Renders the currency template
    /// </summary>
    public RenderFragment Render(CurrencyContext<T> context)
    {
        return builder =>
        {
            if (context?.Item == null) return;
            
            var formattedValue = FormatCurrency(context.Value, context.CurrencyCode, context.Compact);
            var cssClass = context.ShowValueColors ? GetCurrencyColorClass(context.Value) : string.Empty;
            
            builder.OpenElement(0, "span");
            builder.AddAttribute(1, "class", $"currency-value {cssClass} {context.CssClass}".Trim());
            builder.AddAttribute(2, "data-template", "currency");
            builder.AddAttribute(3, "data-value", context.Value.ToString(CultureInfo.InvariantCulture));
            builder.AddAttribute(4, "data-currency", context.CurrencyCode);
            
            if (context.Compact)
                builder.AddAttribute(5, "data-compact", true);
                
            if (context.Disabled)
                builder.AddAttribute(6, "disabled", true);
                
            if (context.Selected)
                builder.AddAttribute(7, "data-selected", true);
            
            builder.AddContent(8, formattedValue);
            builder.CloseElement();
        };
    }
    
    /// <summary>
    /// Create fluent currency with simplified API
    /// </summary>
    public static RenderFragment<T> Create(
        Func<T, decimal> valueSelector,
        string currencyCode = "USD",
        bool compact = false,
        bool showColors = true)
    {
        return item => builder =>
        {
            if (item == null) return;
            
            var value = valueSelector?.Invoke(item) ?? 0;
            var formattedValue = FormatCurrency(value, currencyCode, compact);
            var cssClass = showColors ? GetCurrencyColorClass(value) : string.Empty;
            
            builder.OpenElement(0, "span");
            builder.AddAttribute(1, "class", $"currency-value {cssClass}".Trim());
            builder.AddAttribute(2, "data-template", "currency");
            builder.AddAttribute(3, "data-value", value.ToString(CultureInfo.InvariantCulture));
            builder.AddAttribute(4, "data-currency", currencyCode);
            
            if (compact)
                builder.AddAttribute(5, "data-compact", true);
            
            builder.AddContent(6, formattedValue);
            builder.CloseElement();
        };
    }

    private static string FormatCurrency(decimal value, string currencyCode, bool compact)
    {
        var culture = GetCultureForCurrency(currencyCode);
        
        if (compact)
        {
            return FormatCompactCurrency(value, culture);
        }
        
        return value.ToString("C", culture);
    }
    
    private static string FormatCompactCurrency(decimal value, CultureInfo culture)
    {
        var absValue = Math.Abs(value);
        var sign = value < 0 ? "-" : "";
        var symbol = culture.NumberFormat.CurrencySymbol;
        
        return absValue switch
        {
            >= 1_000_000_000 => $"{sign}{symbol}{absValue / 1_000_000_000:0.#}B",
            >= 1_000_000 => $"{sign}{symbol}{absValue / 1_000_000:0.#}M",
            >= 1_000 => $"{sign}{symbol}{absValue / 1_000:0.#}K",
            _ => value.ToString("C", culture)
        };
    }
    
    private static CultureInfo GetCultureForCurrency(string currencyCode)
    {
        return currencyCode?.ToUpperInvariant() switch
        {
            "USD" => new CultureInfo("en-US"),
            "EUR" => new CultureInfo("en-DE"),
            "GBP" => new CultureInfo("en-GB"),
            "JPY" => new CultureInfo("ja-JP"),
            "CAD" => new CultureInfo("en-CA"),
            "AUD" => new CultureInfo("en-AU"),
            _ => CultureInfo.CurrentCulture
        };
    }
    
    private static string GetCurrencyColorClass(decimal value)
    {
        return value switch
        {
            > 0 => "text-success",
            < 0 => "text-danger",
            _ => "text-muted"
        };
    }
}