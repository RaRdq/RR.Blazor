using Microsoft.AspNetCore.Components;
using System.Linq.Expressions;
using RR.Blazor.Enums;

namespace RR.Blazor.Templates.Currency;

/// <summary>
/// Currency template for monetary values with proper formatting and colors
/// </summary>
/// <typeparam name="T">Type of data the template handles</typeparam>
public class CurrencyTemplate<T> where T : class
{
    /// <summary>
    /// Unique identifier for this template
    /// </summary>
    public string Id { get; set; } = Guid.NewGuid().ToString();
    
    /// <summary>
    /// Display name for the template
    /// </summary>
    public string Name { get; set; } = "Currency Template";
    
    /// <summary>
    /// Property selector for extracting data from the item
    /// </summary>
    public Expression<Func<T, object>> PropertySelector { get; set; }
    
    /// <summary>
    /// CSS classes to apply to the rendered template
    /// </summary>
    public string CssClass { get; set; }
    
    /// <summary>
    /// Size variant
    /// </summary>
    public SizeType Size { get; set; } = SizeType.Medium;
    
    /// <summary>
    /// Density variant
    /// </summary>
    public DensityType Density { get; set; } = DensityType.Normal;

    /// <summary>
    /// Currency code (USD, EUR, GBP, etc.)
    /// </summary>
    public string CurrencyCode { get; set; } = "USD";
    
    /// <summary>
    /// Number format string
    /// </summary>
    public string Format { get; set; } = "C";
    
    /// <summary>
    /// Whether to show compact format (1.2K instead of 1,200)
    /// </summary>
    public bool Compact { get; set; }
    
    /// <summary>
    /// Whether to show positive/negative color coding
    /// </summary>
    public bool ShowValueColors { get; set; } = true;
    
    /// <summary>
    /// Currency code selector for dynamic currency
    /// </summary>
    public Expression<Func<T, string>> CurrencyCodeSelector { get; set; }

    /// <summary>
    /// Gets the value from the item using the property selector
    /// </summary>
    public virtual object GetValue(T item)
    {
        if (item == null) return null;
        
        var compiledSelector = PropertySelector?.Compile();
        return compiledSelector?.Invoke(item);
    }

    /// <summary>
    /// Renders the template for the given item
    /// </summary>
    public RenderFragment Render(T item)
    {
        var context = CreateContext(item);
        var renderer = new CurrencyRenderer<T>();
        return renderer.Render(context);
    }

    private CurrencyContext<T> CreateContext(T item)
    {
        var context = new CurrencyContext<T>(item)
        {
            CurrencyCode = CurrencyCode,
            Format = Format,
            Compact = Compact,
            ShowValueColors = ShowValueColors,
            Size = Size,
            Density = Density,
            CssClass = CssClass
        };
        
        // Get value
        var value = GetValue(item);
        if (value != null && decimal.TryParse(value.ToString(), out var decimalValue))
        {
            context.Value = decimalValue;
        }
        
        // Get currency code if selector is provided
        if (CurrencyCodeSelector != null)
        {
            var codeGetter = CurrencyCodeSelector.Compile();
            var code = codeGetter(item);
            if (!string.IsNullOrEmpty(code))
            {
                context.CurrencyCode = code;
            }
        }
        
        return context;
    }
}