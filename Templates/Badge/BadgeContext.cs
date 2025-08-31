using Microsoft.AspNetCore.Components;
using RR.Blazor.Enums;

namespace RR.Blazor.Templates.Badge;

/// <summary>
/// Context for badge templates
/// </summary>
/// <typeparam name="T">The type of data being rendered</typeparam>
public class BadgeContext<T> where T : class
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
    public string CssClass { get; set; }
    
    /// <summary>
    /// Whether the item is in a disabled state
    /// </summary>
    public bool Disabled { get; set; }
    
    /// <summary>
    /// Whether the item is selected/active
    /// </summary>
    public bool Selected { get; set; }

    /// <summary>
    /// Variant for badge styling
    /// </summary>
    public VariantType Variant { get; set; } = VariantType.Default;
    
    /// <summary>
    /// Text to display in badge
    /// </summary>
    public string Text { get; set; }
    
    /// <summary>
    /// Icon to display in badge
    /// </summary>
    public string Icon { get; set; }
    
    /// <summary>
    /// Whether badge is clickable
    /// </summary>
    public bool Clickable { get; set; }
    
    /// <summary>
    /// Click event handler
    /// </summary>
    public EventCallback<T> OnClick { get; set; }

    public BadgeContext(T item)
    {
        Item = item;
    }
}