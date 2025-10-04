using RR.Blazor.Enums;

namespace RR.Blazor.Templates.Currency;

/// <summary>
/// Context for currency templates
/// </summary>
/// <typeparam name="T">The type of data being rendered</typeparam>
public class CurrencyContext<T> where T : class
{
    /// <summary>
    /// The data item being rendered
    /// </summary>
    public T Item { get; set; }
    
    /// <summary>
    /// Size for rendering
    /// </summary>
    public SizeType Size { get; set; } = SizeType.Medium;
    
    /// <summary>
    /// Density for spacing and layout
    /// </summary>
    public DensityType Density { get; set; } = DensityType.Normal;
    
    /// <summary>
    /// Additional CSS classes to apply
    /// </summary>
    public string Class { get; set; }
    
    /// <summary>
    /// Whether the item is in a disabled state
    /// </summary>
    public bool Disabled { get; set; }
    
    /// <summary>
    /// Whether the item is selected/active
    /// </summary>
    public bool Selected { get; set; }

    /// <summary>
    /// Currency value to display
    /// </summary>
    public decimal Value { get; set; }
    
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

    public CurrencyContext(T item)
    {
        Item = item;
    }
}