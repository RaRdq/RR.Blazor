using Microsoft.AspNetCore.Components;
using System.Linq.Expressions;
using RR.Blazor.Enums;

namespace RR.Blazor.Templates.Stack;

/// <summary>
/// Stack template for multi-line content rendering
/// </summary>
/// <typeparam name="T">Type of data the template handles</typeparam>
public class StackTemplate<T> where T : class
{
    /// <summary>
    /// Unique identifier for this template
    /// </summary>
    public string Id { get; set; } = Guid.NewGuid().ToString();
    
    /// <summary>
    /// Display name for the template
    /// </summary>
    public string Name { get; set; } = "Stack Template";
    
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
    /// Primary text selector
    /// </summary>
    public Expression<Func<T, string>> PrimaryTextSelector { get; set; }
    
    /// <summary>
    /// Secondary text selector
    /// </summary>
    public Expression<Func<T, string>> SecondaryTextSelector { get; set; }
    
    /// <summary>
    /// Tertiary text selector (optional)
    /// </summary>
    public Expression<Func<T, string>> TertiaryTextSelector { get; set; }
    
    /// <summary>
    /// Icon selector
    /// </summary>
    public Expression<Func<T, string>> IconSelector { get; set; }
    
    /// <summary>
    /// Layout orientation
    /// </summary>
    public StackOrientation Orientation { get; set; } = StackOrientation.Vertical;
    
    /// <summary>
    /// Whether to truncate long text
    /// </summary>
    public bool TruncateText { get; set; } = true;
    
    /// <summary>
    /// Maximum length for truncation
    /// </summary>
    public int MaxLength { get; set; } = 50;

    /// <summary>
    /// Renders the template for the given item
    /// </summary>
    public RenderFragment Render(T item)
    {
        var context = CreateContext(item);
        var renderer = new StackRenderer<T>();
        return renderer.Render(context);
    }

    private StackContext<T> CreateContext(T item)
    {
        var context = new StackContext<T>(item)
        {
            Orientation = Orientation,
            Size = Size,
            Density = Density,
            CssClass = CssClass
        };
        
        // Get primary text
        if (PrimaryTextSelector != null)
        {
            var getter = PrimaryTextSelector.Compile();
            context.PrimaryText = TruncateTextIfNeeded(getter(item), TruncateText, MaxLength);
        }
        
        // Get secondary text
        if (SecondaryTextSelector != null)
        {
            var getter = SecondaryTextSelector.Compile();
            context.SecondaryText = TruncateTextIfNeeded(getter(item), TruncateText, MaxLength);
        }
        
        // Get tertiary text
        if (TertiaryTextSelector != null)
        {
            var getter = TertiaryTextSelector.Compile();
            context.TertiaryText = TruncateTextIfNeeded(getter(item), TruncateText, MaxLength);
        }
        
        // Get icon
        if (IconSelector != null)
        {
            var getter = IconSelector.Compile();
            context.Icon = getter(item);
        }
        
        return context;
    }
    
    private static string TruncateTextIfNeeded(string text, bool truncate, int maxLength)
    {
        if (!truncate || string.IsNullOrEmpty(text) || text.Length <= maxLength)
            return text;
        
        return text.Substring(0, maxLength - 3) + "...";
    }
}

/// <summary>
/// Stack orientation for multi-line templates
/// </summary>
public enum StackOrientation
{
    Vertical,
    Horizontal
}