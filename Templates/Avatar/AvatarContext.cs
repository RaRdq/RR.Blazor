using Microsoft.AspNetCore.Components;
using RR.Blazor.Enums;

namespace RR.Blazor.Templates.Avatar;

/// <summary>
/// Context for avatar templates
/// </summary>
/// <typeparam name="T">The type of data being rendered</typeparam>
public class AvatarContext<T> where T : class
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
    /// Name or display text for avatar
    /// </summary>
    public string Name { get; set; }
    
    /// <summary>
    /// Image URL for avatar
    /// </summary>
    public string ImageUrl { get; set; }
    
    /// <summary>
    /// Generated or provided initials
    /// </summary>
    public string Initials { get; set; }
    
    /// <summary>
    /// Avatar shape variant
    /// </summary>
    public AvatarShape Shape { get; set; } = AvatarShape.Circle;
    
    /// <summary>
    /// Status indicator for avatar
    /// </summary>
    public AvatarStatus Status { get; set; } = AvatarStatus.None;
    
    /// <summary>
    /// Background color variant
    /// </summary>
    public VariantType ColorVariant { get; set; } = VariantType.Primary;
    
    /// <summary>
    /// Whether avatar is clickable
    /// </summary>
    public bool Clickable { get; set; }
    
    /// <summary>
    /// Click event handler
    /// </summary>
    public EventCallback<T> OnClick { get; set; }
    
    /// <summary>
    /// Badge content for avatar (e.g., notification count)
    /// </summary>
    public string Badge { get; set; }
    
    /// <summary>
    /// Whether to show border
    /// </summary>
    public bool ShowBorder { get; set; }
    
    /// <summary>
    /// Custom background color (overrides ColorVariant)
    /// </summary>
    public string BackgroundColor { get; set; }

    public AvatarContext(T item)
    {
        Item = item;
    }
}

/// <summary>
/// Avatar shape variants
/// </summary>
public enum AvatarShape
{
    Circle,
    Square,
    Rounded
}

