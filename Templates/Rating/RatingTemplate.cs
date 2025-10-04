using Microsoft.AspNetCore.Components;
using System.Linq.Expressions;
using RR.Blazor.Enums;

namespace RR.Blazor.Templates.Rating;

/// <summary>
/// Rating template for displaying and collecting ratings
/// </summary>
/// <typeparam name="T">Type of data the template handles</typeparam>
public class RatingTemplate<T> where T : class
{
    /// <summary>
    /// Unique identifier for this template
    /// </summary>
    public string Id { get; set; } = Guid.NewGuid().ToString();
    
    /// <summary>
    /// Display name for the template
    /// </summary>
    public string Name { get; set; } = "Rating Template";
    
    /// <summary>
    /// Property selector for extracting data from the item
    /// </summary>
    public Expression<Func<T, object>> PropertySelector { get; set; }
    
    /// <summary>
    /// CSS classes to apply to the rendered template
    /// </summary>
    public string Class { get; set; }
    
    /// <summary>
    /// Size variant
    /// </summary>
    public SizeType Size { get; set; } = SizeType.Medium;
    
    /// <summary>
    /// Density variant
    /// </summary>
    public DensityType Density { get; set; } = DensityType.Normal;

    /// <summary>
    /// Rating display type
    /// </summary>
    public RatingType Type { get; set; } = RatingType.Stars;
    
    /// <summary>
    /// Maximum rating value
    /// </summary>
    public int MaxRating { get; set; } = 5;
    
    /// <summary>
    /// Default color variant
    /// </summary>
    public VariantType ColorVariant { get; set; } = VariantType.Warning;
    
    /// <summary>
    /// Value selector for rating value
    /// </summary>
    public Expression<Func<T, double>> ValueSelector { get; set; }
    
    /// <summary>
    /// Count selector for review count
    /// </summary>
    public Expression<Func<T, int>> CountSelector { get; set; }
    
    /// <summary>
    /// Label selector for custom labels
    /// </summary>
    public Expression<Func<T, string>> LabelSelector { get; set; }
    
    /// <summary>
    /// Whether to allow half ratings
    /// </summary>
    public bool AllowHalf { get; set; }
    
    /// <summary>
    /// Whether rating is interactive
    /// </summary>
    public bool Interactive { get; set; }
    
    /// <summary>
    /// Whether to show numeric value
    /// </summary>
    public bool ShowValue { get; set; }
    
    /// <summary>
    /// Whether to show review count
    /// </summary>
    public bool ShowCount { get; set; }
    
    /// <summary>
    /// Value changed event handler
    /// </summary>
    public EventCallback<double> OnValueChanged { get; set; }
    
    /// <summary>
    /// Icon configuration for star type
    /// </summary>
    public string FilledIcon { get; set; } = RatingIcons.Stars.Filled;
    public string EmptyIcon { get; set; } = RatingIcons.Stars.Empty;
    public string HalfIcon { get; set; } = RatingIcons.Stars.Half;
    
    /// <summary>
    /// Custom colors
    /// </summary>
    public string FilledColor { get; set; }
    public string EmptyColor { get; set; }
    
    /// <summary>
    /// Tooltips for rating levels
    /// </summary>
    public Dictionary<int, string> Tooltips { get; set; } = new()
    {
        { 1, "Poor" },
        { 2, "Fair" },
        { 3, "Good" },
        { 4, "Very Good" },
        { 5, "Excellent" }
    };
    
    /// <summary>
    /// Emoji mapping for emoji type ratings
    /// </summary>
    public Dictionary<int, string> EmojiMapping { get; set; } = new()
    {
        { 1, RatingIcons.Emoji.VeryBad },
        { 2, RatingIcons.Emoji.Bad },
        { 3, RatingIcons.Emoji.Neutral },
        { 4, RatingIcons.Emoji.Good },
        { 5, RatingIcons.Emoji.VeryGood }
    };
    
    /// <summary>
    /// Value format string
    /// </summary>
    public string ValueFormat { get; set; } = "0.#";
    
    /// <summary>
    /// Count format string
    /// </summary>
    public string CountFormat { get; set; } = "({0} reviews)";

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
    /// Configures icons based on rating type
    /// </summary>
    public void ConfigureIconsForType(RatingType type)
    {
        switch (type)
        {
            case RatingType.Stars:
                FilledIcon = RatingIcons.Stars.Filled;
                EmptyIcon = RatingIcons.Stars.Empty;
                HalfIcon = RatingIcons.Stars.Half;
                break;
            case RatingType.Hearts:
                FilledIcon = RatingIcons.Hearts.Filled;
                EmptyIcon = RatingIcons.Hearts.Empty;
                HalfIcon = RatingIcons.Hearts.Filled;
                break;
            case RatingType.Thumbs:
                FilledIcon = RatingIcons.Thumbs.Up;
                EmptyIcon = RatingIcons.Thumbs.UpOutline;
                MaxRating = 2; // Thumbs up/down is binary
                break;
        }
    }
    
    /// <summary>
    /// Gets color based on rating value percentage
    /// </summary>
    public static VariantType GetColorByValue(double value, int maxRating)
    {
        var percentage = (value / maxRating) * 100;
        
        return percentage switch
        {
            < 20 => VariantType.Error,
            < 40 => VariantType.Warning,
            < 60 => VariantType.Info,
            < 80 => VariantType.Primary,
            _ => VariantType.Success
        };
    }

    /// <summary>
    /// Renders the template for the given item
    /// </summary>
    public RenderFragment Render(T item)
    {
        var context = CreateContext(item);
        var renderer = new RatingRenderer<T>();
        return renderer.Render(context);
    }

    private RatingContext<T> CreateContext(T item)
    {
        var context = new RatingContext<T>(item)
        {
            Size = Size,
            Density = Density,
            Class = Class,
            Type = Type,
            MaxRating = MaxRating,
            AllowHalf = AllowHalf,
            Interactive = Interactive,
            ShowValue = ShowValue,
            ShowCount = ShowCount,
            OnValueChanged = OnValueChanged,
            FilledIcon = FilledIcon,
            EmptyIcon = EmptyIcon,
            HalfIcon = HalfIcon,
            FilledColor = FilledColor,
            EmptyColor = EmptyColor,
            Tooltips = Tooltips
        };
        
        // Get value
        if (ValueSelector != null)
        {
            var valueGetter = ValueSelector.Compile();
            context.Value = valueGetter(item);
        }
        else
        {
            var value = GetValue(item);
            context.Value = Convert.ToDouble(value ?? 0);
        }
        
        // Get count
        if (CountSelector != null)
        {
            var countGetter = CountSelector.Compile();
            context.Count = countGetter(item);
        }
        
        // Get label
        if (LabelSelector != null)
        {
            var labelGetter = LabelSelector.Compile();
            context.Label = labelGetter(item);
        }
        
        // Auto-detect color based on value if not set
        if (ColorVariant == VariantType.Warning && Type != RatingType.Stars)
        {
            context.ColorVariant = GetColorByValue(context.Value, MaxRating);
        }
        else
        {
            context.ColorVariant = ColorVariant;
        }
        
        // Configure emoji icons if using emoji type
        if (Type == RatingType.Emoji)
        {
            context.CustomIcons = EmojiMapping;
        }
        
        return context;
    }
}