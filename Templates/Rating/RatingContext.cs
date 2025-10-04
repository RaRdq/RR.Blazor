using Microsoft.AspNetCore.Components;
using RR.Blazor.Enums;

namespace RR.Blazor.Templates.Rating;

/// <summary>
/// Context for rating templates
/// </summary>
/// <typeparam name="T">The type of data being rendered</typeparam>
public class RatingContext<T> where T : class
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
    /// Current rating value
    /// </summary>
    public double Value { get; set; }
    
    /// <summary>
    /// Maximum rating value
    /// </summary>
    public int MaxRating { get; set; } = 5;
    
    /// <summary>
    /// Rating display type
    /// </summary>
    public RatingType Type { get; set; } = RatingType.Stars;
    
    /// <summary>
    /// Icon to use for rating (for star type)
    /// </summary>
    public string Icon { get; set; } = "star";
    
    /// <summary>
    /// Icon for filled state
    /// </summary>
    public string FilledIcon { get; set; } = "star";
    
    /// <summary>
    /// Icon for empty state
    /// </summary>
    public string EmptyIcon { get; set; } = "star_outline";
    
    /// <summary>
    /// Icon for half-filled state
    /// </summary>
    public string HalfIcon { get; set; } = "star_half";
    
    /// <summary>
    /// Whether to allow half ratings
    /// </summary>
    public bool AllowHalf { get; set; }
    
    /// <summary>
    /// Whether rating is interactive (can be changed)
    /// </summary>
    public bool Interactive { get; set; }
    
    /// <summary>
    /// Whether to show the numeric value
    /// </summary>
    public bool ShowValue { get; set; }
    
    /// <summary>
    /// Whether to show count (e.g., "4.5 (123 reviews)")
    /// </summary>
    public bool ShowCount { get; set; }
    
    /// <summary>
    /// Count/reviews number to display
    /// </summary>
    public int Count { get; set; }
    
    /// <summary>
    /// Custom label text
    /// </summary>
    public string Label { get; set; }
    
    /// <summary>
    /// Color variant for rating
    /// </summary>
    public VariantType ColorVariant { get; set; } = VariantType.Warning;
    
    /// <summary>
    /// Custom color for filled items
    /// </summary>
    public string FilledColor { get; set; }
    
    /// <summary>
    /// Custom color for empty items
    /// </summary>
    public string EmptyColor { get; set; }
    
    /// <summary>
    /// Value changed callback for interactive ratings
    /// </summary>
    public EventCallback<double> OnValueChanged { get; set; }
    
    /// <summary>
    /// Hover value for interactive ratings
    /// </summary>
    public double? HoverValue { get; set; }
    
    /// <summary>
    /// Tooltips for each rating level
    /// </summary>
    public Dictionary<int, string> Tooltips { get; set; } = new();
    
    /// <summary>
    /// Custom icons for different rating levels
    /// </summary>
    public Dictionary<int, string> CustomIcons { get; set; } = new();

    public RatingContext(T item)
    {
        Item = item;
    }
}

/// <summary>
/// Rating display types
/// </summary>
public enum RatingType
{
    Stars,
    Hearts,
    Thumbs,
    Numeric,
    Custom,
    Bar,
    Emoji
}

/// <summary>
/// Predefined rating icon sets
/// </summary>
public static class RatingIcons
{
    public static class Stars
    {
        public const string Filled = "star";
        public const string Empty = "star_outline";
        public const string Half = "star_half";
    }
    
    public static class Hearts
    {
        public const string Filled = "favorite";
        public const string Empty = "favorite_border";
        public const string Half = "favorite";
    }
    
    public static class Thumbs
    {
        public const string Up = "thumb_up";
        public const string Down = "thumb_down";
        public const string UpOutline = "thumb_up_off_alt";
        public const string DownOutline = "thumb_down_off_alt";
    }
    
    public static class Emoji
    {
        public const string VeryBad = "sentiment_very_dissatisfied";
        public const string Bad = "sentiment_dissatisfied";
        public const string Neutral = "sentiment_neutral";
        public const string Good = "sentiment_satisfied";
        public const string VeryGood = "sentiment_very_satisfied";
    }
}