using Microsoft.AspNetCore.Components;
using RR.Blazor.Enums;
using RR.Blazor.Templates.Badge;
using RR.Blazor.Templates.Currency;
using RR.Blazor.Templates.Stack;
using RR.Blazor.Templates.Avatar;
using RR.Blazor.Templates.Progress;
using RR.Blazor.Templates.Rating;
using RR.Blazor.Templates.Configuration;
using RR.Blazor.Components.Core;
using RR.Blazor.Components;

namespace RR.Blazor.Templates;

/// <summary>
/// Central registry for all template types and configurations
/// Provides unified access to template functionality
/// </summary>
public static class TemplateRegistry
{
    private static TemplateConfiguration _configuration = new();

    /// <summary>
    /// Configure global template settings
    /// </summary>
    public static void Configure(TemplateConfiguration configuration)
    {
        _configuration = configuration ?? new();
    }

    /// <summary>
    /// Get current template configuration
    /// </summary>
    public static TemplateConfiguration GetConfiguration() => _configuration;

    #region Badge Templates

    /// <summary>
    /// Create a badge template with current defaults
    /// </summary>
    public static RenderFragment<T> Badge<T>(
        Func<T, string> textSelector,
        Func<T, VariantType> variantSelector = null,
        Func<T, string> iconSelector = null,
        bool? clickable = null,
        EventCallback<T> onClick = default) where T : class
    {
        return BadgeRenderer<T>.Create(
            textSelector,
            variantSelector,
            iconSelector,
            clickable ?? _configuration.Badge.DefaultClickable,
            onClick);
    }

    /// <summary>
    /// Create a badge template from configured BadgeTemplate
    /// </summary>
    public static RenderFragment<T> Badge<T>(BadgeTemplate<T> template) where T : class
    {
        // Apply global defaults if not set
        ApplyBadgeDefaults(template);
        return item => template.Render(item);
    }

    private static void ApplyBadgeDefaults<T>(BadgeTemplate<T> template) where T : class
    {
        template.Size = template.Size == default ? _configuration.Badge.DefaultSize : template.Size;
        template.Density = template.Density == default ? _configuration.Badge.DefaultDensity : template.Density;
        template.Variant = template.Variant == default ? _configuration.Badge.DefaultVariant : template.Variant;
        
        if (!template.StatusMapping.Any())
        {
            foreach (var mapping in _configuration.Badge.GlobalStatusMapping)
            {
                template.StatusMapping[mapping.Key] = mapping.Value;
            }
        }
    }

    #endregion

    #region Currency Templates

    /// <summary>
    /// Create a currency template with current defaults
    /// </summary>
    public static RenderFragment<T> Currency<T>(
        Func<T, decimal> valueSelector,
        string currencyCode = null,
        bool? compact = null,
        bool? showColors = null) where T : class
    {
        return CurrencyRenderer<T>.Create(
            valueSelector,
            currencyCode ?? _configuration.Currency.DefaultCurrencyCode,
            compact ?? _configuration.Currency.DefaultCompact,
            showColors ?? _configuration.Currency.DefaultShowColors);
    }

    /// <summary>
    /// Create a currency template from configured CurrencyTemplate
    /// </summary>
    public static RenderFragment<T> Currency<T>(CurrencyTemplate<T> template) where T : class
    {
        // Apply global defaults if not set
        ApplyCurrencyDefaults(template);
        return item => template.Render(item);
    }

    private static void ApplyCurrencyDefaults<T>(CurrencyTemplate<T> template) where T : class
    {
        template.Size = template.Size == default ? SizeType.Medium : template.Size;
        template.Density = template.Density == default ? DensityType.Normal : template.Density;
        template.CurrencyCode = string.IsNullOrEmpty(template.CurrencyCode) ? _configuration.Currency.DefaultCurrencyCode : template.CurrencyCode;
        template.Format = string.IsNullOrEmpty(template.Format) ? _configuration.Currency.DefaultFormat : template.Format;
    }

    #endregion

    #region Stack Templates

    /// <summary>
    /// Create a stack template with current defaults
    /// </summary>
    public static RenderFragment<T> Stack<T>(
        Func<T, string> primarySelector,
        Func<T, string> secondarySelector = null,
        Func<T, string> iconSelector = null,
        StackOrientation? orientation = null) where T : class
    {
        return StackRenderer<T>.Create(
            primarySelector,
            secondarySelector,
            iconSelector,
            orientation ?? _configuration.Stack.DefaultOrientation);
    }

    /// <summary>
    /// Create a stack template from configured StackTemplate
    /// </summary>
    public static RenderFragment<T> Stack<T>(StackTemplate<T> template) where T : class
    {
        // Apply global defaults if not set
        ApplyStackDefaults(template);
        return item => template.Render(item);
    }

    private static void ApplyStackDefaults<T>(StackTemplate<T> template) where T : class
    {
        template.Size = template.Size == default ? _configuration.Stack.DefaultSize : template.Size;
        template.Density = template.Density == default ? _configuration.Stack.DefaultDensity : template.Density;
        template.Orientation = template.Orientation == default ? _configuration.Stack.DefaultOrientation : template.Orientation;
        template.TruncateText = template.MaxLength == 50 ? _configuration.Stack.DefaultTruncateText : template.TruncateText;
        template.MaxLength = template.MaxLength == 50 ? _configuration.Stack.DefaultMaxLength : template.MaxLength;
    }

    #endregion

    #region Avatar Templates

    /// <summary>
    /// Create an avatar template with current defaults
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
        return AvatarRenderer<T>.Create(
            nameSelector,
            imageSelector,
            size,
            shape,
            statusSelector,
            clickable,
            onClick);
    }

    /// <summary>
    /// Create an avatar group template
    /// </summary>
    public static RenderFragment<IEnumerable<T>> AvatarGroup<T>(
        Func<T, string> nameSelector,
        Func<T, string> imageSelector = null,
        SizeType size = SizeType.Medium,
        int maxDisplay = 5) where T : class
    {
        return AvatarRenderer<T>.CreateGroup(
            nameSelector,
            imageSelector,
            size,
            maxDisplay);
    }

    /// <summary>
    /// Create an avatar template from configured AvatarTemplate
    /// </summary>
    public static RenderFragment<T> Avatar<T>(AvatarTemplate<T> template) where T : class
    {
        return item => builder =>
        {
            if (item == null) return;
            // Implementation for AvatarTemplate - placeholder for now
            builder.AddContent(0, "Avatar Template");
        };
    }

    #endregion

    #region Progress Templates

    /// <summary>
    /// Create a progress template with current defaults
    /// </summary>
    public static RenderFragment<T> Progress<T>(
        Func<T, double> valueSelector,
        double max = 100,
        ProgressType type = ProgressType.Linear,
        bool showPercentage = true,
        VariantType variant = VariantType.Primary) where T : class
    {
        return ProgressRenderer<T>.Create(
            valueSelector,
            max,
            type,
            showPercentage,
            variant);
    }

    /// <summary>
    /// Create a step progress template
    /// </summary>
    public static RenderFragment<T> StepProgress<T>(
        Func<T, List<ProgressStep>> stepsSelector,
        Func<T, int> currentStepSelector) where T : class
    {
        return item => builder =>
        {
            if (item == null) return;
            
            var context = new ProgressContext<T>(item)
            {
                Type = ProgressType.Steps,
                Steps = stepsSelector?.Invoke(item) ?? new List<ProgressStep>(),
                CurrentStep = currentStepSelector?.Invoke(item) ?? 0
            };
            
            var renderer = new ProgressRenderer<T>();
            renderer.Render(context)(builder);
        };
    }

    /// <summary>
    /// Create a multi-segment progress template
    /// </summary>
    public static RenderFragment<T> MultiSegmentProgress<T>(
        Func<T, List<ProgressSegment>> segmentsSelector) where T : class
    {
        return item => builder =>
        {
            if (item == null) return;
            
            var context = new ProgressContext<T>(item)
            {
                Type = ProgressType.MultiSegment,
                Segments = segmentsSelector?.Invoke(item) ?? new List<ProgressSegment>()
            };
            
            var renderer = new ProgressRenderer<T>();
            renderer.Render(context)(builder);
        };
    }

    /// <summary>
    /// Create a progress template from configured ProgressTemplate
    /// </summary>
    public static RenderFragment<T> Progress<T>(ProgressTemplate<T> template) where T : class
    {
        return item => builder =>
        {
            if (item == null) return;
            // Implementation for ProgressTemplate - placeholder for now
            builder.AddContent(0, "Progress Template");
        };
    }

    #endregion

    #region Rating Templates

    /// <summary>
    /// Create a rating template with current defaults
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
        return item => builder =>
        {
            if (item == null) return;
            
            var context = new RatingContext<T>(item)
            {
                Value = valueSelector?.Invoke(item) ?? 0,
                MaxRating = maxRating,
                Type = type,
                ShowValue = showValue,
                Size = size,
                Interactive = interactive,
                OnValueChanged = onValueChanged
            };
            
            var renderer = new RatingRenderer<T>();
            renderer.Render(context)(builder);
        };
    }

    /// <summary>
    /// Create a thumbs rating template
    /// </summary>
    public static RenderFragment<T> ThumbsRating<T>(
        Func<T, bool> valueSelector,
        bool interactive = false,
        EventCallback<bool> onValueChanged = default) where T : class
    {
        return item => builder =>
        {
            if (item == null) return;
            
            var value = valueSelector?.Invoke(item) ?? false;
            var context = new RatingContext<T>(item)
            {
                Value = value ? 1 : 0,
                MaxRating = 1,
                Type = RatingType.Thumbs,
                Interactive = interactive
            };
            
            if (interactive && onValueChanged.HasDelegate)
            {
                context.OnValueChanged = EventCallback.Factory.Create<double>(
                    item, async (val) => await onValueChanged.InvokeAsync(val > 0));
            }
            
            var renderer = new RatingRenderer<T>();
            renderer.Render(context)(builder);
        };
    }

    /// <summary>
    /// Create an emoji rating template
    /// </summary>
    public static RenderFragment<T> EmojiRating<T>(
        Func<T, double> valueSelector,
        int maxRating = 5,
        bool interactive = false,
        EventCallback<double> onValueChanged = default) where T : class
    {
        return item => builder =>
        {
            if (item == null) return;
            
            var context = new RatingContext<T>(item)
            {
                Value = valueSelector?.Invoke(item) ?? 0,
                MaxRating = maxRating,
                Type = RatingType.Emoji,
                Interactive = interactive,
                OnValueChanged = onValueChanged
            };
            
            var renderer = new RatingRenderer<T>();
            renderer.Render(context)(builder);
        };
    }

    /// <summary>
    /// Create a rating template from configured RatingTemplate
    /// </summary>
    public static RenderFragment<T> Rating<T>(RatingTemplate<T> template) where T : class
    {
        return item => builder =>
        {
            if (item == null) return;
            // Implementation for RatingTemplate - placeholder for now
            builder.AddContent(0, "Rating Template");
        };
    }

    #endregion

    #region Group Templates

    /// <summary>
    /// Create a group template from configured GroupTemplate
    /// </summary>
    public static RenderFragment<T> Group<T>(RR.Blazor.Models.GroupTemplate<T> template) where T : class
    {
        return item => builder =>
        {
            if (item == null) return;
            // Implementation for GroupTemplate - placeholder for now
            builder.AddContent(0, "Group Template");
        };
    }

    #endregion

    #region Template Detection

    /// <summary>
    /// Create template with smart detection
    /// </summary>
    public static RenderFragment<T> Auto<T>(string propertyName) where T : class
    {
        return item => builder =>
        {
            if (!_configuration.SmartDetection.Enabled || item == null)
                return;

            var property = typeof(T).GetProperty(propertyName);
            if (property == null)
                return;

            var suggestion = Detection.TemplateDetector.CreateSuggestion(property, new[] { item });
            
            if (suggestion.Confidence < _configuration.SmartDetection.SuggestionThreshold)
                return;

            RenderFragment fragment = suggestion.TemplateType switch
            {
                Detection.TemplateType.Badge => Badge<T>(t => property.GetValue(t)?.ToString() ?? string.Empty)(item),
                Detection.TemplateType.Currency => Currency<T>(t => Convert.ToDecimal(property.GetValue(t) ?? 0))(item),
                Detection.TemplateType.Stack => Stack<T>(t => property.GetValue(t)?.ToString() ?? string.Empty)(item),
                Detection.TemplateType.Avatar => Avatar<T>(t => property.GetValue(t)?.ToString() ?? string.Empty)(item),
                Detection.TemplateType.Progress => Progress<T>(t => Convert.ToDouble(property.GetValue(t) ?? 0))(item),
                Detection.TemplateType.Rating => Rating<T>(t => Convert.ToDouble(property.GetValue(t) ?? 0))(item),
                _ => b => b.AddContent(0, property.GetValue(item)?.ToString() ?? string.Empty)
            };
            
            fragment(builder);
        };
    }

    #endregion

    #region Filter Display Templates

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
        return builder =>
        {
            if (string.IsNullOrWhiteSpace(text)) return;
            
            builder.OpenComponent<RChip>(0);
            builder.AddAttribute(1, nameof(RChip.Text), text);
            builder.AddAttribute(2, nameof(RChip.Variant), variant);
            builder.AddAttribute(3, nameof(RChip.Size), size);
            builder.AddAttribute(4, nameof(RChip.Closeable), closeable);
            
            if (!string.IsNullOrEmpty(icon))
                builder.AddAttribute(5, nameof(RChip.Icon), icon);
            
            if (closeable && onClose.HasDelegate)
                builder.AddAttribute(6, nameof(RChip.OnClose), onClose);
                
            builder.CloseComponent();
        };
    }

    /// <summary>
    /// Creates a filter summary badge template
    /// </summary>
    public static RenderFragment FilterSummary(
        int activeCount,
        int totalCount = 0,
        VariantType variant = VariantType.Info,
        bool showTotal = false)
    {
        return builder =>
        {
            if (activeCount <= 0) return;
            
            var text = showTotal && totalCount > 0 
                ? $"{activeCount}/{totalCount} filters"
                : $"{activeCount} active";
                
            builder.OpenComponent<RBadge>(0);
            builder.AddAttribute(1, nameof(RBadge.Text), text);
            builder.AddAttribute(2, nameof(RBadge.Variant), variant);
            builder.AddAttribute(3, nameof(RBadge.Size), SizeType.Small);
            builder.CloseComponent();
        };
    }

    #endregion
}