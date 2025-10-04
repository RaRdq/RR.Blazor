using Microsoft.AspNetCore.Components;
using RR.Blazor.Enums;
using RR.Blazor.Templates.Avatar;
using RR.Blazor.Templates.Chip;
using RR.Blazor.Templates.Currency;
using RR.Blazor.Templates.Progress;
using RR.Blazor.Templates.Rating;
using RR.Blazor.Templates.Stack;

namespace RR.Blazor.Templates;

/// <summary>
/// Universal template system for RR.Blazor components
/// Provides reusable, strongly-typed templates for common data display patterns
/// REFACTORED: Now uses folder-based architecture with focused classes
/// </summary>
public static class RTemplates
{
    #region Chip Templates
    
    /// <summary>
    /// Creates a chip template with fluent API
    /// Delegates to ChipRenderer for actual rendering
    /// </summary>
    public static RenderFragment<T> Chip<T>(
        Func<T, string> textSelector,
        Func<T, VariantType> variantSelector = null,
        Func<T, string> iconSelector = null,
        ChipStyle style = ChipStyle.Chip,
        bool closeable = false,
        EventCallback<T> onClick = default,
        EventCallback<T> onClose = default) where T : class
    {
        return TemplateRegistry.Chip(textSelector, variantSelector, iconSelector, style, closeable, onClick, onClose);
    }
    
    /// <summary>
    /// Creates a filter chip template
    /// </summary>
    public static RenderFragment<T> FilterChip<T>(
        Func<T, string> textSelector,
        Func<T, bool> selectedSelector = null,
        EventCallback<T> onToggle = default) where T : class
    {
        return TemplateRegistry.FilterChip(textSelector, selectedSelector, onToggle);
    }
    
    #endregion
    
    #region Currency Templates - Delegated to Currency folder structure
    
    /// <summary>
    /// Creates a simple currency template with fluent API
    /// Delegates to CurrencyRenderer for actual rendering
    /// </summary>
    public static RenderFragment<T> Currency<T>(
        Func<T, decimal> valueSelector,
        string currencyCode = "USD",
        bool compact = false,
        bool showColors = true) where T : class
    {
        return TemplateRegistry.Currency(valueSelector, currencyCode, compact, showColors);
    }
    
    #endregion
    
    #region Stack Templates - Delegated to Stack folder structure
    
    /// <summary>
    /// Creates a simple stack template with fluent API
    /// Delegates to StackRenderer for actual rendering
    /// </summary>
    public static RenderFragment<T> Stack<T>(
        Func<T, string> primarySelector,
        Func<T, string> secondarySelector = null,
        Func<T, string> iconSelector = null,
        StackOrientation orientation = StackOrientation.Vertical) where T : class
    {
        return TemplateRegistry.Stack(primarySelector, secondarySelector, iconSelector, orientation);
    }
    
    #endregion
    
    #region Smart Templates - Auto-detection based
    
    /// <summary>
    /// Creates template with smart detection based on data analysis
    /// </summary>
    public static RenderFragment<T> Auto<T>(string propertyName) where T : class
    {
        return TemplateRegistry.Auto<T>(propertyName);
    }
    
    #endregion

    #region Avatar Templates - Delegated to Avatar folder structure
    
    /// <summary>
    /// Creates a simple avatar template with fluent API
    /// Delegates to AvatarRenderer for actual rendering
    /// </summary>
    public static RenderFragment<T> Avatar<T>(
        Func<T, string> nameSelector,
        Func<T, string> imageSelector = null,
        SizeType size = SizeType.Medium,
        AvatarShape shape = AvatarShape.Circle,
        Func<T, AvatarStatus> statusSelector = null,
        bool clickable = false,
        EventCallback<T> onClick = default) where T : class
    {
        return TemplateRegistry.Avatar(nameSelector, imageSelector, size, shape, statusSelector, clickable, onClick);
    }
    
    /// <summary>
    /// Creates an avatar group/stack for multiple avatars
    /// </summary>
    public static RenderFragment<IEnumerable<T>> AvatarGroup<T>(
        Func<T, string> nameSelector,
        Func<T, string> imageSelector = null,
        SizeType size = SizeType.Medium,
        int maxDisplay = 5) where T : class
    {
        return TemplateRegistry.AvatarGroup(nameSelector, imageSelector, size, maxDisplay);
    }
    
    #endregion
    
    #region Progress Templates - Delegated to Progress folder structure
    
    /// <summary>
    /// Creates a simple progress template with fluent API
    /// Delegates to ProgressRenderer for actual rendering
    /// </summary>
    public static RenderFragment<T> Progress<T>(
        Func<T, double> valueSelector,
        double max = 100,
        ProgressType type = ProgressType.Linear,
        bool showPercentage = true,
        VariantType variant = VariantType.Primary) where T : class
    {
        return TemplateRegistry.Progress(valueSelector, max, type, showPercentage, variant);
    }
    
    /// <summary>
    /// Creates a step progress template
    /// </summary>
    public static RenderFragment<T> StepProgress<T>(
        Func<T, List<ProgressStep>> stepsSelector,
        Func<T, int> currentStepSelector) where T : class
    {
        return TemplateRegistry.StepProgress(stepsSelector, currentStepSelector);
    }
    
    /// <summary>
    /// Creates a multi-segment progress template
    /// </summary>
    public static RenderFragment<T> MultiSegmentProgress<T>(
        Func<T, List<ProgressSegment>> segmentsSelector) where T : class
    {
        return TemplateRegistry.MultiSegmentProgress(segmentsSelector);
    }
    
    #endregion
    
    #region Rating Templates - Delegated to Rating folder structure
    
    /// <summary>
    /// Creates a simple rating template with fluent API
    /// Delegates to RatingRenderer for actual rendering
    /// </summary>
    public static RenderFragment<T> Rating<T>(
        Func<T, double> valueSelector,
        int maxRating = 5,
        RatingType type = RatingType.Stars,
        bool showValue = false,
        SizeType size = SizeType.Medium,
        bool interactive = false,
        EventCallback<double> onValueChanged = default) where T : class
    {
        return TemplateRegistry.Rating(valueSelector, maxRating, type, showValue, size, interactive, onValueChanged);
    }
    
    /// <summary>
    /// Creates a thumbs up/down rating template
    /// </summary>
    public static RenderFragment<T> ThumbsRating<T>(
        Func<T, bool> valueSelector,
        bool interactive = false,
        EventCallback<bool> onValueChanged = default) where T : class
    {
        return TemplateRegistry.ThumbsRating(valueSelector, interactive, onValueChanged);
    }
    
    /// <summary>
    /// Creates an emoji rating template
    /// </summary>
    public static RenderFragment<T> EmojiRating<T>(
        Func<T, double> valueSelector,
        int maxRating = 5,
        bool interactive = false,
        EventCallback<double> onValueChanged = default) where T : class
    {
        return TemplateRegistry.EmojiRating(valueSelector, maxRating, interactive, onValueChanged);
    }
    
    #endregion
    
    #region Filter Display Templates - Delegated to Registry
    
    /// <summary>
    /// Creates a filter chip template for displaying active filters
    /// </summary>
    public static RenderFragment FilterChip(
        string text,
        EventCallback onClose,
        VariantType variant = VariantType.Info,
        string icon = null,
        bool closeable = true,
        SizeType size = SizeType.Small)
    {
        return TemplateRegistry.FilterChip(text, onClose, variant, icon, closeable, size);
    }
    
    /// <summary>
    /// Creates a filter summary badge template with total count
    /// </summary>
    public static RenderFragment FilterSummaryWithTotal(
        int activeCount,
        int totalCount = 0,
        VariantType variant = VariantType.Info,
        bool showTotal = false)
    {
        return TemplateRegistry.FilterSummary(activeCount, totalCount, variant, showTotal);
    }
    
    #endregion
    
    #region Template Support Methods - for backward compatibility with TemplateModels
    
    
    /// <summary>
    /// Renders a CurrencyTemplate instance - used by template models
    /// </summary>
    public static RenderFragment CurrencyTemplate<T>(CurrencyTemplate<T> template, T item) where T : class
    {
        return TemplateRegistry.Currency(template)(item);
    }
    
    /// <summary>
    /// Renders a StackTemplate instance - used by template models
    /// </summary>
    public static RenderFragment StackTemplate<T>(StackTemplate<T> template, T item) where T : class
    {
        return TemplateRegistry.Stack(template)(item);
    }
    
    /// <summary>
    /// Renders an AvatarTemplate instance - used by template models
    /// </summary>
    public static RenderFragment AvatarTemplate<T>(AvatarTemplate<T> template, T item) where T : class
    {
        return TemplateRegistry.Avatar(template)(item);
    }
    
    /// <summary>
    /// Renders a ProgressTemplate instance - used by template models
    /// </summary>
    public static RenderFragment ProgressTemplate<T>(ProgressTemplate<T> template, T item) where T : class
    {
        return TemplateRegistry.Progress(template)(item);
    }
    
    /// <summary>
    /// Renders a RatingTemplate instance - used by template models
    /// </summary>
    public static RenderFragment RatingTemplate<T>(RatingTemplate<T> template, T item) where T : class
    {
        return TemplateRegistry.Rating(template)(item);
    }
    
    /// <summary>
    /// Renders a GroupTemplate instance - used by template models
    /// </summary>
    public static RenderFragment GroupTemplate<T>(RR.Blazor.Models.GroupTemplate<T> template, T item) where T : class
    {
        return TemplateRegistry.Group(template)(item);
    }
    
    /// <summary>
    /// Creates a quick filter template for filter components
    /// </summary>
    public static RenderFragment<T> QuickFilter<T>(
        Func<T, string> textSelector,
        EventCallback<T> onToggle = default,
        Func<T, bool> activeSelector = null) where T : class
    {
        return item => builder =>
        {
            if (item == null) return;
            
            var text = textSelector?.Invoke(item) ?? string.Empty;
            var isActive = activeSelector?.Invoke(item) ?? false;
            
            builder.OpenElement(0, "span");
            builder.AddAttribute(1, "class", $"quick-filter {(isActive ? "active" : "")}");
            builder.AddAttribute(2, "onclick", EventCallback.Factory.Create(item, () => onToggle.InvokeAsync(item)));
            builder.AddContent(3, text);
            builder.CloseElement();
        };
    }

    /// <summary>
    /// Creates a quick filter button for filter components - static version
    /// </summary>
    public static RenderFragment QuickFilter(
        string text,
        bool isActive,
        EventCallback onClick,
        string icon = null,
        SizeType size = SizeType.Medium)
    {
        return builder =>
        {
            builder.OpenComponent<RR.Blazor.Components.Core.RButton>(0);
            builder.AddAttribute(1, "Text", text);
            builder.AddAttribute(2, "Variant", isActive ? VariantType.Primary : VariantType.Secondary);
            builder.AddAttribute(3, "Size", size);
            builder.AddAttribute(4, "OnClick", onClick);
            if (!string.IsNullOrEmpty(icon))
            {
                builder.AddAttribute(5, "Icon", icon);
                builder.AddAttribute(6, "IconPosition", IconPosition.Start);
            }
            builder.AddAttribute(7, "Class", "quick-filter-btn");
            builder.CloseComponent();
        };
    }

    /// <summary>
    /// Creates a filter summary badge
    /// </summary>
    public static RenderFragment FilterSummary(
        int count,
        VariantType variant = VariantType.Primary,
        SizeType size = SizeType.Small)
    {
        return builder =>
        {
            builder.OpenComponent<RR.Blazor.Components.RChip>(0);
            builder.AddAttribute(1, "StyleVariant", ChipStyle.Badge);
            builder.AddAttribute(2, "Text", $"{count} filter{(count != 1 ? "s" : "")}");
            builder.AddAttribute(3, "Variant", variant);
            builder.AddAttribute(4, "Size", size);
            builder.CloseComponent();
        };
    }

    /// <summary>
    /// Creates a filter chip with remove capability
    /// </summary>
    public static RenderFragment FilterChip(
        string text,
        EventCallback onRemove,
        VariantType variant = VariantType.Info,
        bool closeable = true,
        SizeType size = SizeType.Small)
    {
        return builder =>
        {
            builder.OpenComponent<RR.Blazor.Components.RChip>(0);
            builder.AddAttribute(1, "Text", text);
            builder.AddAttribute(2, "Variant", variant);
            builder.AddAttribute(3, "Size", size);
            builder.AddAttribute(4, "Closeable", closeable);
            if (closeable)
            {
                builder.AddAttribute(5, "OnClose", onRemove);
            }
            builder.CloseComponent();
        };
    }
    
    #endregion
}