using RR.Blazor.Enums;

namespace RR.Blazor.Templates.Stack;

/// <summary>
/// Context for stack (multi-line) templates
/// </summary>
/// <typeparam name="T">The type of data being rendered</typeparam>
public class StackContext<T> where T : class
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
    /// Primary text line
    /// </summary>
    public string PrimaryText { get; set; }
    
    /// <summary>
    /// Secondary text line
    /// </summary>
    public string SecondaryText { get; set; }
    
    /// <summary>
    /// Tertiary text line (optional)
    /// </summary>
    public string TertiaryText { get; set; }
    
    /// <summary>
    /// Icon to display with stack
    /// </summary>
    public string Icon { get; set; }
    
    /// <summary>
    /// Layout orientation
    /// </summary>
    public StackOrientation Orientation { get; set; } = StackOrientation.Vertical;

    public StackContext(T item)
    {
        Item = item;
    }
}