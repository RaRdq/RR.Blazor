using Microsoft.AspNetCore.Components;
using System.Linq.Expressions;
using RR.Blazor.Enums;

namespace RR.Blazor.Models;

/// <summary>
/// Base template definition for universal templates
/// </summary>
/// <typeparam name="T">Type of data the template handles</typeparam>
public abstract class TemplateDefinition<T> where T : class
{
    /// <summary>
    /// Unique identifier for this template
    /// </summary>
    public string Id { get; set; } = Guid.NewGuid().ToString();
    
    /// <summary>
    /// Display name for the template
    /// </summary>
    public string Name { get; set; }
    
    /// <summary>
    /// Description of what the template does
    /// </summary>
    public string Description { get; set; }
    
    /// <summary>
    /// Property selector for extracting data from the item
    /// </summary>
    public Expression<Func<T, object>> PropertySelector { get; set; }
    
    /// <summary>
    /// Compiled property selector for performance
    /// </summary>
    public Func<T, object> CompiledSelector { get; set; }
    
    /// <summary>
    /// Optional formatter function
    /// </summary>
    public Func<object, string> Formatter { get; set; }
    
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
    /// Gets the value from the item using the property selector
    /// </summary>
    /// <param name="item">Item to extract value from</param>
    /// <returns>Extracted value</returns>
    public virtual object GetValue(T item)
    {
        if (item == null) return null;
        
        CompiledSelector ??= PropertySelector?.Compile();
        return CompiledSelector?.Invoke(item);
    }
    
    /// <summary>
    /// Formats the value using the formatter or default formatting
    /// </summary>
    /// <param name="value">Value to format</param>
    /// <returns>Formatted string</returns>
    public virtual string FormatValue(object value)
    {
        if (value == null) return string.Empty;
        
        if (Formatter != null) return Formatter(value);
        
        return value.ToString();
    }
    
    /// <summary>
    /// Renders the template for the given item
    /// </summary>
    /// <param name="item">Item to render</param>
    /// <returns>Rendered fragment</returns>
    public abstract RenderFragment Render(T item);
}


/// <summary>
/// Template for currency rendering
/// </summary>
/// <typeparam name="T">Type of data the template handles</typeparam>
public class CurrencyTemplate<T> : TemplateDefinition<T> where T : class
{
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

    public override RenderFragment Render(T item)
    {
        // Convert Models.CurrencyTemplate to Templates.Currency.CurrencyTemplate
        var templateCurrency = new RR.Blazor.Templates.Currency.CurrencyTemplate<T>
        {
            Id = this.Id,
            Name = this.Name,
            PropertySelector = this.PropertySelector,
            Class = this.Class,
            Size = this.Size,
            Density = this.Density,
            CurrencyCode = this.CurrencyCode,
            Format = this.Format,
            Compact = this.Compact,
            ShowValueColors = this.ShowValueColors,
            CurrencyCodeSelector = this.CurrencyCodeSelector
        };
        return RR.Blazor.Templates.RTemplates.CurrencyTemplate(templateCurrency, item);
    }
}

/// <summary>
/// Template for stacked (multi-line) text rendering
/// </summary>
/// <typeparam name="T">Type of data the template handles</typeparam>
public class StackTemplate<T> : TemplateDefinition<T> where T : class
{
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
    public Models.StackOrientation Orientation { get; set; } = Models.StackOrientation.Vertical;
    
    /// <summary>
    /// Whether to truncate long text
    /// </summary>
    public bool TruncateText { get; set; } = true;
    
    /// <summary>
    /// Maximum length for truncation
    /// </summary>
    public int MaxLength { get; set; } = 50;

    public override RenderFragment Render(T item)
    {
        // Convert Models.StackTemplate to Templates.Stack.StackTemplate
        var templateStack = new RR.Blazor.Templates.Stack.StackTemplate<T>
        {
            Id = this.Id,
            Name = this.Name,
            Class = this.Class,
            Size = this.Size,
            Density = this.Density,
            PrimaryTextSelector = this.PrimaryTextSelector,
            SecondaryTextSelector = this.SecondaryTextSelector,
            TertiaryTextSelector = this.TertiaryTextSelector,
            IconSelector = this.IconSelector,
            Orientation = (RR.Blazor.Templates.Stack.StackOrientation)(int)this.Orientation,
            TruncateText = this.TruncateText,
            MaxLength = this.MaxLength
        };
        return RR.Blazor.Templates.RTemplates.StackTemplate(templateStack, item);
    }
}

/// <summary>
/// Template for group rendering (lists, arrays, etc.)
/// </summary>
/// <typeparam name="T">Type of data the template handles</typeparam>
public class GroupTemplate<T> : TemplateDefinition<T> where T : class
{
    /// <summary>
    /// Items selector for extracting collection
    /// </summary>
    public Expression<Func<T, IEnumerable<object>>> ItemsSelector { get; set; }
    
    /// <summary>
    /// Template for individual items
    /// </summary>
    public RenderFragment<object> ItemTemplate { get; set; }
    
    /// <summary>
    /// Separator between items
    /// </summary>
    public string Separator { get; set; } = ", ";
    
    /// <summary>
    /// Maximum number of items to show
    /// </summary>
    public int MaxItems { get; set; } = 5;
    
    /// <summary>
    /// Text to show when items are truncated
    /// </summary>
    public string MoreText { get; set; } = "and {0} more...";
    
    /// <summary>
    /// Whether to show items as badges
    /// </summary>
    public bool RenderAsBadges { get; set; }
    
    /// <summary>
    /// Badge variant when rendering as badges
    /// </summary>
    public VariantType BadgeVariant { get; set; } = VariantType.Secondary;

    public override RenderFragment Render(T item)
    {
        return RR.Blazor.Templates.RTemplates.GroupTemplate(this, item);
    }
}

/// <summary>
/// Template for avatar rendering
/// </summary>
/// <typeparam name="T">Type of data the template handles</typeparam>
public class AvatarTemplate<T> : TemplateDefinition<T> where T : class
{
    /// <summary>
    /// Name selector for generating initials
    /// </summary>
    public Expression<Func<T, string>> NameSelector { get; set; }
    
    /// <summary>
    /// Image URL selector
    /// </summary>
    public Expression<Func<T, string>> ImageUrlSelector { get; set; }
    
    /// <summary>
    /// Email selector for Gravatar support
    /// </summary>
    public Expression<Func<T, string>> EmailSelector { get; set; }
    
    /// <summary>
    /// Custom initials selector
    /// </summary>
    public Expression<Func<T, string>> InitialsSelector { get; set; }
    
    /// <summary>
    /// Color variant for avatar background
    /// </summary>
    public VariantType Variant { get; set; } = VariantType.Primary;
    
    /// <summary>
    /// Variant selector for dynamic coloring
    /// </summary>
    public Expression<Func<T, VariantType>> VariantSelector { get; set; }
    
    /// <summary>
    /// Whether avatars are clickable
    /// </summary>
    public bool Clickable { get; set; }
    
    /// <summary>
    /// Click event handler
    /// </summary>
    public EventCallback<T> OnClick { get; set; }
    
    /// <summary>
    /// Whether to use Gravatar for images
    /// </summary>
    public bool UseGravatar { get; set; } = true;

    public override RenderFragment Render(T item)
    {
        var template = new RR.Blazor.Templates.Avatar.AvatarTemplate<T>
        {
            PropertySelector = NameSelector != null ? (t => (object)NameSelector.Compile()(t)) : (t => (object)t.ToString()),
            Class = Class
        };
        return RR.Blazor.Templates.RTemplates.AvatarTemplate(template, item);
    }
}

/// <summary>
/// Template for progress bar rendering
/// </summary>
/// <typeparam name="T">Type of data the template handles</typeparam>
public class ProgressTemplate<T> : TemplateDefinition<T> where T : class
{
    /// <summary>
    /// Progress value selector (should return 0-100 or 0-Max)
    /// </summary>
    public Expression<Func<T, double>> ValueSelector { get; set; }
    
    /// <summary>
    /// Maximum value selector
    /// </summary>
    public Expression<Func<T, double>> MaxSelector { get; set; }
    
    /// <summary>
    /// Label text selector
    /// </summary>
    public Expression<Func<T, string>> LabelSelector { get; set; }
    
    /// <summary>
    /// Maximum value for progress (if not using selector)
    /// </summary>
    public double Max { get; set; } = 100;
    
    /// <summary>
    /// Whether to show percentage text
    /// </summary>
    public bool ShowPercentage { get; set; } = true;
    
    /// <summary>
    /// Variant for progress styling
    /// </summary>
    public VariantType Variant { get; set; } = VariantType.Primary;
    
    /// <summary>
    /// Variant selector for dynamic styling
    /// </summary>
    public Expression<Func<T, VariantType>> VariantSelector { get; set; }
    
    /// <summary>
    /// Whether progress is indeterminate
    /// </summary>
    public bool Indeterminate { get; set; }

    public override RenderFragment Render(T item)
    {
        var template = new RR.Blazor.Templates.Progress.ProgressTemplate<T>
        {
            PropertySelector = ValueSelector != null ? (t => (object)ValueSelector.Compile()(t)) : (t => (object)0),
            Class = Class
        };
        return RR.Blazor.Templates.RTemplates.ProgressTemplate(template, item);
    }
}

/// <summary>
/// Template for rating/star rendering
/// </summary>
/// <typeparam name="T">Type of data the template handles</typeparam>
public class RatingTemplate<T> : TemplateDefinition<T> where T : class
{
    /// <summary>
    /// Rating value selector (0-MaxRating)
    /// </summary>
    public Expression<Func<T, double>> ValueSelector { get; set; }
    
    /// <summary>
    /// Maximum rating value
    /// </summary>
    public int MaxRating { get; set; } = 5;
    
    /// <summary>
    /// Icon to use for rating
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
    
    /// <summary>
    /// Allow half-star ratings
    /// </summary>
    public bool AllowHalfStars { get; set; } = true;

    public override RenderFragment Render(T item)
    {
        var template = new RR.Blazor.Templates.Rating.RatingTemplate<T>
        {
            PropertySelector = ValueSelector != null ? (t => (object)ValueSelector.Compile()(t)) : (t => (object)0),
            Class = Class
        };
        return RR.Blazor.Templates.RTemplates.RatingTemplate(template, item);
    }
}

/// <summary>
/// Template builder for fluent API creation
/// </summary>
/// <typeparam name="T">Type of data the template handles</typeparam>
public class TemplateBuilder<T> where T : class
{
    
    /// <summary>
    /// Create a currency template
    /// </summary>
    public static CurrencyTemplate<T> Currency(Expression<Func<T, object>> propertySelector)
    {
        return new CurrencyTemplate<T>
        {
            PropertySelector = propertySelector,
            Name = "Currency Template"
        };
    }
    
    /// <summary>
    /// Create a stack template
    /// </summary>
    public static StackTemplate<T> Stack(
        Expression<Func<T, string>> primarySelector,
        Expression<Func<T, string>> secondarySelector = null)
    {
        return new StackTemplate<T>
        {
            PrimaryTextSelector = primarySelector,
            SecondaryTextSelector = secondarySelector,
            Name = "Stack Template"
        };
    }
    
    /// <summary>
    /// Create a group template
    /// </summary>
    public static GroupTemplate<T> Group(Expression<Func<T, IEnumerable<object>>> itemsSelector)
    {
        return new GroupTemplate<T>
        {
            ItemsSelector = itemsSelector,
            Name = "Group Template"
        };
    }
    
    /// <summary>
    /// Create an avatar template
    /// </summary>
    public static AvatarTemplate<T> Avatar(Expression<Func<T, string>> nameSelector)
    {
        return new AvatarTemplate<T>
        {
            NameSelector = nameSelector,
            Name = "Avatar Template"
        };
    }
    
    /// <summary>
    /// Create a progress template
    /// </summary>
    public static ProgressTemplate<T> Progress(Expression<Func<T, double>> valueSelector)
    {
        return new ProgressTemplate<T>
        {
            ValueSelector = valueSelector,
            Name = "Progress Template"
        };
    }
    
    /// <summary>
    /// Create a rating template
    /// </summary>
    public static RatingTemplate<T> Rating(Expression<Func<T, double>> valueSelector)
    {
        return new RatingTemplate<T>
        {
            ValueSelector = valueSelector,
            Name = "Rating Template"
        };
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