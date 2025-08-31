using Microsoft.AspNetCore.Components;
using RR.Blazor.Enums;
using RR.Blazor.Models;
using RR.Blazor.Templates;
using RR.Blazor.Templates.Stack;
using RR.Blazor.Templates.Avatar;
using RR.Blazor.Templates.Progress;
using RR.Blazor.Templates.Rating;
using System.Linq.Expressions;

namespace RR.Blazor.Extensions;

/// <summary>
/// Extension methods for universal template system
/// </summary>
public static class TemplateExtensions
{
    #region ColumnDefinition Extensions
    
    /// <summary>
    /// Configure a badge template for this column
    /// </summary>
    public static ColumnDefinition<T> UseBadgeTemplate<T>(
        this ColumnDefinition<T> column,
        Action<BadgeTemplate<T>> configure = null) where T : class
    {
        var template = new BadgeTemplate<T>
        {
            PropertySelector = column.Property,
            Name = $"{column.Title} Badge Template",
            Size = SizeType.Small,
            Density = DensityType.Compact
        };
        
        configure?.Invoke(template);
        column.BadgeTemplate = template;
        
        return column;
    }
    
    /// <summary>
    /// Configure a currency template for this column
    /// </summary>
    public static ColumnDefinition<T> UseCurrencyTemplate<T>(
        this ColumnDefinition<T> column,
        string currencyCode = "USD",
        bool compact = false,
        bool showColors = true,
        Action<CurrencyTemplate<T>> configure = null) where T : class
    {
        var template = new CurrencyTemplate<T>
        {
            PropertySelector = column.Property,
            Name = $"{column.Title} Currency Template",
            CurrencyCode = currencyCode,
            Compact = compact,
            ShowValueColors = showColors
        };
        
        configure?.Invoke(template);
        column.CurrencyTemplate = template;
        
        return column;
    }
    
    /// <summary>
    /// Configure a stack template for this column
    /// </summary>
    public static ColumnDefinition<T> UseStackTemplate<T>(
        this ColumnDefinition<T> column,
        Expression<Func<T, string>> primarySelector,
        Expression<Func<T, string>> secondarySelector = null,
        Models.StackOrientation orientation = Models.StackOrientation.Vertical,
        Action<Models.StackTemplate<T>> configure = null) where T : class
    {
        var template = new Models.StackTemplate<T>
        {
            PrimaryTextSelector = primarySelector,
            SecondaryTextSelector = secondarySelector,
            Orientation = orientation,
            Name = $"{column.Title} Stack Template"
        };
        
        configure?.Invoke(template);
        column.StackTemplate = template;
        
        return column;
    }
    
    /// <summary>
    /// Configure a group template for this column
    /// </summary>
    public static ColumnDefinition<T> UseGroupTemplate<T>(
        this ColumnDefinition<T> column,
        Expression<Func<T, IEnumerable<object>>> itemsSelector,
        int maxItems = 5,
        string separator = ", ",
        Action<GroupTemplate<T>> configure = null) where T : class
    {
        var template = new GroupTemplate<T>
        {
            ItemsSelector = itemsSelector,
            MaxItems = maxItems,
            Separator = separator,
            Name = $"{column.Title} Group Template"
        };
        
        configure?.Invoke(template);
        column.GroupTemplate = template;
        
        return column;
    }
    
    /// <summary>
    /// Configure an avatar template for this column
    /// </summary>
    public static ColumnDefinition<T> UseAvatarTemplate<T>(
        this ColumnDefinition<T> column,
        Expression<Func<T, string>> nameSelector,
        Expression<Func<T, string>> imageSelector = null,
        SizeType size = SizeType.Medium,
        Action<Models.AvatarTemplate<T>> configure = null) where T : class
    {
        var template = new Models.AvatarTemplate<T>
        {
            NameSelector = nameSelector,
            ImageUrlSelector = imageSelector,
            Size = size,
            Name = $"{column.Title} Avatar Template"
        };
        
        configure?.Invoke(template);
        column.AvatarTemplate = template;
        
        return column;
    }
    
    /// <summary>
    /// Configure a progress template for this column
    /// </summary>
    public static ColumnDefinition<T> UseProgressTemplate<T>(
        this ColumnDefinition<T> column,
        Expression<Func<T, double>> valueSelector,
        double max = 100,
        bool showPercentage = true,
        Action<Models.ProgressTemplate<T>> configure = null) where T : class
    {
        var template = new Models.ProgressTemplate<T>
        {
            ValueSelector = valueSelector,
            Max = max,
            ShowPercentage = showPercentage,
            Name = $"{column.Title} Progress Template"
        };
        
        configure?.Invoke(template);
        column.ProgressTemplate = template;
        
        return column;
    }
    
    /// <summary>
    /// Configure a rating template for this column
    /// </summary>
    public static ColumnDefinition<T> UseRatingTemplate<T>(
        this ColumnDefinition<T> column,
        Expression<Func<T, double>> valueSelector,
        int maxRating = 5,
        bool showValue = false,
        Action<Models.RatingTemplate<T>> configure = null) where T : class
    {
        var template = new Models.RatingTemplate<T>
        {
            ValueSelector = valueSelector,
            MaxRating = maxRating,
            ShowValue = showValue,
            Name = $"{column.Title} Rating Template"
        };
        
        configure?.Invoke(template);
        column.RatingTemplate = template;
        
        return column;
    }
    
    #endregion
    
    #region RenderFragment Extensions for Choice Components
    
    /// <summary>
    /// Create a badge template for choice items
    /// </summary>
    public static RenderFragment<T> AsBadgeTemplate<T>(
        this IEnumerable<T> source,
        Func<T, string> textSelector,
        Func<T, VariantType> variantSelector = null,
        Func<T, string> iconSelector = null,
        bool clickable = false) where T : class
    {
        return RTemplates.Badge(textSelector, variantSelector, iconSelector, clickable);
    }
    
    /// <summary>
    /// Create a currency template for choice items
    /// </summary>
    public static RenderFragment<T> AsCurrencyTemplate<T>(
        this IEnumerable<T> source,
        Func<T, decimal> valueSelector,
        string currencyCode = "USD",
        bool compact = false,
        bool showColors = true) where T : class
    {
        return RTemplates.Currency(valueSelector, currencyCode, compact, showColors);
    }
    
    /// <summary>
    /// Create a stack template for choice items
    /// </summary>
    public static RenderFragment<T> AsStackTemplate<T>(
        this IEnumerable<T> source,
        Func<T, string> primarySelector,
        Func<T, string> secondarySelector = null,
        Func<T, string> iconSelector = null,
        Templates.Stack.StackOrientation orientation = Templates.Stack.StackOrientation.Vertical) where T : class
    {
        return RTemplates.Stack(primarySelector, secondarySelector, iconSelector, orientation);
    }
    
    
    /// <summary>
    /// Create an avatar template for choice items
    /// </summary>
    public static RenderFragment<T> AsAvatarTemplate<T>(
        this IEnumerable<T> source,
        Func<T, string> nameSelector,
        Func<T, string> imageSelector = null,
        SizeType size = SizeType.Medium,
        bool clickable = false) where T : class
    {
        return RTemplates.Avatar(nameSelector, imageSelector, size, AvatarShape.Circle, null, clickable, default);
    }
    
    /// <summary>
    /// Create a progress template for choice items
    /// </summary>
    public static RenderFragment<T> AsProgressTemplate<T>(
        this IEnumerable<T> source,
        Func<T, double> valueSelector,
        double max = 100,
        bool showPercentage = true,
        VariantType variant = VariantType.Primary) where T : class
    {
        return RTemplates.Progress(valueSelector, max, ProgressType.Linear, showPercentage, variant);
    }
    
    /// <summary>
    /// Create a rating template for choice items
    /// </summary>
    public static RenderFragment<T> AsRatingTemplate<T>(
        this IEnumerable<T> source,
        Func<T, double> valueSelector,
        int maxRating = 5,
        string icon = "star",
        bool showValue = false,
        SizeType size = SizeType.Medium) where T : class
    {
        return RTemplates.Rating(valueSelector, maxRating, RatingType.Stars, showValue, size, false, default);
    }
    
    #endregion
    
    #region Template Builder Fluent API
    
    /// <summary>
    /// Start building a column with badge template
    /// </summary>
    public static ColumnDefinition<T> Column<T>(string key, string title) where T : class
    {
        return new ColumnDefinition<T>
        {
            Key = key,
            Title = title
        };
    }
    
    /// <summary>
    /// Set property selector for the column
    /// </summary>
    public static ColumnDefinition<T> WithProperty<T>(
        this ColumnDefinition<T> column,
        Expression<Func<T, object>> propertySelector) where T : class
    {
        column.Property = propertySelector;
        return column;
    }
    
    /// <summary>
    /// Set column width
    /// </summary>
    public static ColumnDefinition<T> WithWidth<T>(
        this ColumnDefinition<T> column,
        string width) where T : class
    {
        column.Width = width;
        return column;
    }
    
    /// <summary>
    /// Set column alignment
    /// </summary>
    public static ColumnDefinition<T> WithAlignment<T>(
        this ColumnDefinition<T> column,
        ColumnAlign align) where T : class
    {
        column.Align = align;
        return column;
    }
    
    /// <summary>
    /// Make column sortable
    /// </summary>
    public static ColumnDefinition<T> Sortable<T>(
        this ColumnDefinition<T> column,
        bool sortable = true) where T : class
    {
        column.Sortable = sortable;
        return column;
    }
    
    /// <summary>
    /// Make column filterable
    /// </summary>
    public static ColumnDefinition<T> Filterable<T>(
        this ColumnDefinition<T> column,
        bool filterable = true) where T : class
    {
        column.Filterable = filterable;
        return column;
    }
    
    /// <summary>
    /// Set custom formatter
    /// </summary>
    public static ColumnDefinition<T> WithFormatter<T>(
        this ColumnDefinition<T> column,
        Func<T, string> formatter) where T : class
    {
        column.Formatter = formatter;
        return column;
    }
    
    #endregion
}