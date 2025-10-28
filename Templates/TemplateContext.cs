using Microsoft.AspNetCore.Components;
using RR.Blazor.Enums;
using RR.Blazor.Models;

namespace RR.Blazor.Templates;

/// <summary>
/// Context passed to universal templates for rendering data
/// </summary>
/// <typeparam name="T">The type of data being rendered</typeparam>
public class TemplateContext<T> where T : class
{
    /// <summary>
    /// The data item being rendered
    /// </summary>
    public T Item { get; set; }
    
    /// <summary>
    /// Optional label or description for the data
    /// </summary>
    public string Label { get; set; }
    
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
    /// Custom attributes for the rendered element
    /// </summary>
    public Dictionary<string, object> Attributes { get; set; } = new();
    
    /// <summary>
    /// Whether the item is in a disabled state
    /// </summary>
    public bool Disabled { get; set; }
    
    /// <summary>
    /// Whether the item is in a loading state
    /// </summary>
    public bool Loading { get; set; }
    
    /// <summary>
    /// Whether the item is selected/active
    /// </summary>
    public bool Selected { get; set; }
    
    /// <summary>
    /// Additional data that can be used by templates
    /// </summary>
    public Dictionary<string, object> AdditionalData { get; set; } = new();

    public TemplateContext(T item)
    {
        Item = item;
    }
}


/// <summary>
/// Context for currency templates
/// </summary>
/// <typeparam name="T">The type of data being rendered</typeparam>
public class CurrencyTemplateContext<T> : TemplateContext<T> where T : class
{
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

    public CurrencyTemplateContext(T item) : base(item) { }
}

/// <summary>
/// Context for stack (multi-line) templates
/// </summary>
/// <typeparam name="T">The type of data being rendered</typeparam>
public class StackTemplateContext<T> : TemplateContext<T> where T : class
{
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

    public StackTemplateContext(T item) : base(item) { }
}

/// <summary>
/// Context for group templates
/// </summary>
/// <typeparam name="T">The type of data being rendered</typeparam>
public class GroupTemplateContext<T> : TemplateContext<T> where T : class
{
    /// <summary>
    /// Items in the group
    /// </summary>
    public IEnumerable<T> Items { get; set; } = Enumerable.Empty<T>();
    
    /// <summary>
    /// Template for individual items
    /// </summary>
    public RenderFragment<T> ItemTemplate { get; set; }
    
    /// <summary>
    /// Separator between items
    /// </summary>
    public string Separator { get; set; } = ", ";
    
    /// <summary>
    /// Maximum number of items to show
    /// </summary>
    public int MaxItems { get; set; } = int.MaxValue;
    
    /// <summary>
    /// Text to show when items are truncated
    /// </summary>
    public string MoreText { get; set; } = "...";

    public GroupTemplateContext(T item) : base(item) { }
}

/// <summary>
/// Context for avatar templates
/// </summary>
/// <typeparam name="T">The type of data being rendered</typeparam>
public class AvatarTemplateContext<T> : TemplateContext<T> where T : class
{
    /// <summary>
    /// Display name for initials
    /// </summary>
    public string Name { get; set; }
    
    /// <summary>
    /// Image URL for avatar
    /// </summary>
    public string ImageUrl { get; set; }
    
    /// <summary>
    /// Email for Gravatar support
    /// </summary>
    public string Email { get; set; }
    
    /// <summary>
    /// Custom initials (overrides name-based initials)
    /// </summary>
    public string Initials { get; set; }
    
    /// <summary>
    /// Color variant for background
    /// </summary>
    public VariantType Variant { get; set; } = VariantType.Primary;
    
    /// <summary>
    /// Whether avatar is clickable
    /// </summary>
    public bool Clickable { get; set; }
    
    /// <summary>
    /// Click event handler
    /// </summary>
    public EventCallback<T> OnClick { get; set; }

    public AvatarTemplateContext(T item) : base(item) { }
}

/// <summary>
/// Context for progress templates
/// </summary>
/// <typeparam name="T">The type of data being rendered</typeparam>
public class ProgressTemplateContext<T> : TemplateContext<T> where T : class
{
    /// <summary>
    /// Progress value (0-100)
    /// </summary>
    public double Value { get; set; }
    
    /// <summary>
    /// Maximum value for progress
    /// </summary>
    public double Max { get; set; } = 100;
    
    /// <summary>
    /// Label text to display
    /// </summary>
    public string Text { get; set; }
    
    /// <summary>
    /// Whether to show percentage text
    /// </summary>
    public bool ShowPercentage { get; set; } = true;
    
    /// <summary>
    /// Variant for progress styling
    /// </summary>
    public VariantType Variant { get; set; } = VariantType.Primary;
    
    /// <summary>
    /// Whether progress is indeterminate
    /// </summary>
    public bool Indeterminate { get; set; }

    public ProgressTemplateContext(T item) : base(item) { }
}

/// <summary>
/// Context for rating templates
/// </summary>
/// <typeparam name="T">The type of data being rendered</typeparam>
public class RatingTemplateContext<T> : TemplateContext<T> where T : class
{
    /// <summary>
    /// Rating value (0-MaxRating)
    /// </summary>
    public double Value { get; set; }
    
    /// <summary>
    /// Maximum rating value
    /// </summary>
    public int MaxRating { get; set; } = 5;
    
    /// <summary>
    /// Icon to use for rating (default: star)
    /// </summary>
    public string Icon { get; set; } = "star";
    
    /// <summary>
    /// Whether rating is readonly
    /// </summary>
    public bool ReadOnly { get; set; } = true;
    
    /// <summary>
    /// Whether to show rating value as text
    /// </summary>
    public bool ShowValue { get; set; }
    
    /// <summary>
    /// Rating changed event
    /// </summary>
    public EventCallback<double> OnRatingChanged { get; set; }

    public RatingTemplateContext(T item) : base(item) { }
}
