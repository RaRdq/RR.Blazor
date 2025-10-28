using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using RR.Blazor.Enums;

namespace RR.Blazor.Templates.Rating;

/// <summary>
/// Renders rating templates using HTML output
/// Supports stars, hearts, thumbs, numeric, and custom rating displays
/// </summary>
/// <typeparam name="T">Type of data being rendered</typeparam>
public class RatingRenderer<T> where T : class
{
    /// <summary>
    /// Renders the rating template
    /// </summary>
    public RenderFragment Render(RatingContext<T> context)
    {
        return builder =>
        {
            if (context?.Item == null) return;
            
            builder.OpenElement(0, "div");
            builder.AddAttribute(1, "class", $"rating-container d-flex align-center gap-1 {context.Class}".Trim());
            builder.AddAttribute(2, "data-template", "rating");
            builder.AddAttribute(3, "data-type", context.Type.ToString().ToLower());
            
            if (context.Disabled)
                builder.AddAttribute(4, "disabled", true);
            
            // Render based on type
            switch (context.Type)
            {
                case RatingType.Stars:
                case RatingType.Hearts:
                case RatingType.Custom:
                    RenderIconRating(builder, context, 100);
                    break;
                case RatingType.Thumbs:
                    RenderThumbsRating(builder, context, 200);
                    break;
                case RatingType.Numeric:
                    RenderNumericRating(builder, context, 300);
                    break;
                case RatingType.Bar:
                    RenderBarRating(builder, context, 400);
                    break;
                case RatingType.Emoji:
                    RenderEmojiRating(builder, context, 500);
                    break;
            }
            
            // Additional info (value, count, label)
            RenderAdditionalInfo(builder, context, 1000);
            
            builder.CloseElement(); // rating-container
        };
    }
    
    private static void RenderIconRating(RenderTreeBuilder builder, RatingContext<T> context, int sequence)
    {
        builder.OpenElement(sequence, "div");
        builder.AddAttribute(sequence + 1, "class", "rating-icons");
        
        for (int i = 1; i <= context.MaxRating; i++)
        {
            var filled = i <= Math.Floor(context.Value);
            var halfFilled = context.AllowHalf && !filled && (context.Value - (i - 1)) >= 0.5;
            var hovered = context.Interactive && context.HoverValue.HasValue && i <= context.HoverValue.Value;
            
            builder.OpenElement(sequence + i * 10, context.Interactive ? "button" : "span");
            builder.AddAttribute(sequence + i * 10 + 1, "class", GetIconClass(context, filled || hovered, halfFilled));
            builder.AddAttribute(sequence + i * 10 + 2, "data-rating", i);
            
            if (context.Interactive)
            {
                var rating = i;
                builder.AddAttribute(sequence + i * 10 + 3, "type", "button");
                builder.AddAttribute(sequence + i * 10 + 4, "onclick", EventCallback.Factory.Create(context.Item, () => HandleRatingClick(context, rating)));
                builder.AddAttribute(sequence + i * 10 + 5, "onmouseover", EventCallback.Factory.Create(context.Item, () => context.HoverValue = rating));
                builder.AddAttribute(sequence + i * 10 + 6, "onmouseout", EventCallback.Factory.Create(context.Item, () => context.HoverValue = null));
            }
            
            if (context.Tooltips.TryGetValue(i, out var tooltip))
            {
                builder.AddAttribute(sequence + i * 10 + 7, "title", tooltip);
            }
            
            // Icon
            builder.OpenElement(sequence + i * 10 + 10, "i");
            builder.AddAttribute(sequence + i * 10 + 11, "class", "icon");
            
            var icon = GetIconForState(context, i, filled, halfFilled);
            builder.AddContent(sequence + i * 10 + 12, icon);
            
            builder.CloseElement(); // icon
            builder.CloseElement(); // button/span
        }
        
        builder.CloseElement(); // rating-icons
    }
    
    private static void RenderThumbsRating(RenderTreeBuilder builder, RatingContext<T> context, int sequence)
    {
        var isPositive = context.Value > 0;
        
        // Thumbs up
        builder.OpenElement(sequence, context.Interactive ? "button" : "span");
        builder.AddAttribute(sequence + 1, "class", GetThumbClass(context, true, isPositive));
        builder.AddAttribute(sequence + 2, "data-rating", "up");
        
        if (context.Interactive)
        {
            builder.AddAttribute(sequence + 3, "type", "button");
            builder.AddAttribute(sequence + 4, "onclick", EventCallback.Factory.Create(context.Item, () => HandleRatingClick(context, 1)));
        }
        
        builder.OpenElement(sequence + 10, "i");
        builder.AddAttribute(sequence + 11, "class", "icon");
        builder.AddContent(sequence + 12, isPositive ? RatingIcons.Thumbs.Up : RatingIcons.Thumbs.UpOutline);
        builder.CloseElement();
        
        builder.CloseElement(); // button/span
        
        // Thumbs down
        builder.OpenElement(sequence + 20, context.Interactive ? "button" : "span");
        builder.AddAttribute(sequence + 21, "class", GetThumbClass(context, false, !isPositive && context.Value != 0));
        builder.AddAttribute(sequence + 22, "data-rating", "down");
        
        if (context.Interactive)
        {
            builder.AddAttribute(sequence + 23, "type", "button");
            builder.AddAttribute(sequence + 24, "onclick", EventCallback.Factory.Create(context.Item, () => HandleRatingClick(context, 0)));
        }
        
        builder.OpenElement(sequence + 30, "i");
        builder.AddAttribute(sequence + 31, "class", "icon");
        builder.AddContent(sequence + 32, !isPositive && context.Value != 0 ? RatingIcons.Thumbs.Down : RatingIcons.Thumbs.DownOutline);
        builder.CloseElement();
        
        builder.CloseElement(); // button/span
    }
    
    private static void RenderNumericRating(RenderTreeBuilder builder, RatingContext<T> context, int sequence)
    {
        var displayValue = context.Value.ToString(context.ShowValue ? "0.#" : "0");
        var percentage = (context.Value / context.MaxRating) * 100;
        
        builder.OpenElement(sequence, "div");
        builder.AddAttribute(sequence + 1, "class", "rating-numeric");
        
        // Numeric display
        builder.OpenElement(sequence + 10, "span");
        builder.AddAttribute(sequence + 11, "class", $"rating-numeric-value text-{context.ColorVariant.ToString().ToLower()}");
        builder.AddContent(sequence + 12, displayValue);
        builder.CloseElement();
        
        // Separator
        builder.OpenElement(sequence + 20, "span");
        builder.AddAttribute(sequence + 21, "class", "rating-numeric-separator text-muted");
        builder.AddContent(sequence + 22, " / ");
        builder.CloseElement();
        
        // Max value
        builder.OpenElement(sequence + 30, "span");
        builder.AddAttribute(sequence + 31, "class", "rating-numeric-max text-muted");
        builder.AddContent(sequence + 32, context.MaxRating);
        builder.CloseElement();
        
        builder.CloseElement(); // rating-numeric
    }
    
    private static void RenderBarRating(RenderTreeBuilder builder, RatingContext<T> context, int sequence)
    {
        var percentage = (context.Value / context.MaxRating) * 100;
        
        builder.OpenElement(sequence, "div");
        builder.AddAttribute(sequence + 1, "class", "rating-bar-wrapper");
        
        // Bar background
        builder.OpenElement(sequence + 10, "div");
        builder.AddAttribute(sequence + 11, "class", "rating-bar");
        
        // Bar fill
        builder.OpenElement(sequence + 20, "div");
        builder.AddAttribute(sequence + 21, "class", $"rating-bar-fill bg-{context.ColorVariant.ToString().ToLower()}");
        builder.AddAttribute(sequence + 22, "style", $"width: {percentage:0.##}%;");
        builder.CloseElement();
        
        builder.CloseElement(); // rating-bar
        
        // Value overlay
        if (context.ShowValue)
        {
            builder.OpenElement(sequence + 30, "span");
            builder.AddAttribute(sequence + 31, "class", "rating-bar-value");
            builder.AddContent(sequence + 32, $"{context.Value:0.#}/{context.MaxRating}");
            builder.CloseElement();
        }
        
        builder.CloseElement(); // rating-bar-wrapper
    }
    
    private static void RenderEmojiRating(RenderTreeBuilder builder, RatingContext<T> context, int sequence)
    {
        builder.OpenElement(sequence, "div");
        builder.AddAttribute(sequence + 1, "class", "rating-emoji");
        
        for (int i = 1; i <= context.MaxRating; i++)
        {
            var isSelected = Math.Round(context.Value) == i;
            var isHovered = context.Interactive && context.HoverValue.HasValue && context.HoverValue.Value == i;
            
            builder.OpenElement(sequence + i * 10, context.Interactive ? "button" : "span");
            builder.AddAttribute(sequence + i * 10 + 1, "class", GetEmojiClass(context, isSelected || isHovered));
            builder.AddAttribute(sequence + i * 10 + 2, "data-rating", i);
            
            if (context.Interactive)
            {
                var rating = i;
                builder.AddAttribute(sequence + i * 10 + 3, "type", "button");
                builder.AddAttribute(sequence + i * 10 + 4, "onclick", EventCallback.Factory.Create(context.Item, () => HandleRatingClick(context, rating)));
                builder.AddAttribute(sequence + i * 10 + 5, "onmouseover", EventCallback.Factory.Create(context.Item, () => context.HoverValue = rating));
                builder.AddAttribute(sequence + i * 10 + 6, "onmouseout", EventCallback.Factory.Create(context.Item, () => context.HoverValue = null));
            }
            
            if (context.Tooltips.TryGetValue(i, out var tooltip))
            {
                builder.AddAttribute(sequence + i * 10 + 7, "title", tooltip);
            }
            
            // Emoji icon
            builder.OpenElement(sequence + i * 10 + 10, "i");
            builder.AddAttribute(sequence + i * 10 + 11, "class", "icon");
            
            var emojiIcon = context.CustomIcons.TryGetValue(i, out var customIcon) 
                ? customIcon 
                : GetDefaultEmojiIcon(i, context.MaxRating);
            
            builder.AddContent(sequence + i * 10 + 12, emojiIcon);
            builder.CloseElement(); // icon
            
            builder.CloseElement(); // button/span
        }
        
        builder.CloseElement(); // rating-emoji
    }
    
    private static void RenderAdditionalInfo(RenderTreeBuilder builder, RatingContext<T> context, int sequence)
    {
        if (!context.ShowValue && !context.ShowCount && string.IsNullOrEmpty(context.Label))
            return;
        
        builder.OpenElement(sequence, "div");
        builder.AddAttribute(sequence + 1, "class", "rating-info ms-2");
        
        // Value
        if (context.ShowValue)
        {
            builder.OpenElement(sequence + 10, "span");
            builder.AddAttribute(sequence + 11, "class", "rating-value text-muted");
            builder.AddContent(sequence + 12, context.Value.ToString("0.#"));
            builder.CloseElement();
        }
        
        // Count
        if (context.ShowCount && context.Count > 0)
        {
            builder.OpenElement(sequence + 20, "span");
            builder.AddAttribute(sequence + 21, "class", "rating-count text-muted ms-1");
            builder.AddContent(sequence + 22, $"({context.Count})");
            builder.CloseElement();
        }
        
        // Label
        if (!string.IsNullOrEmpty(context.Label))
        {
            builder.OpenElement(sequence + 30, "span");
            builder.AddAttribute(sequence + 31, "class", "rating-label text-muted ms-1");
            builder.AddContent(sequence + 32, context.Label);
            builder.CloseElement();
        }
        
        builder.CloseElement(); // rating-info
    }
    
    private static string GetIconClass(RatingContext<T> context, bool filled, bool halfFilled)
    {
        var sizeClass = GetSizeClass(context.Size);
        var colorClass = filled || halfFilled 
            ? $"text-{context.ColorVariant.ToString().ToLower()}" 
            : "text-muted opacity-25";
        var interactiveClass = context.Interactive ? "rating-icon-interactive" : "";
        
        return $"rating-icon {sizeClass} {colorClass} {interactiveClass}".Trim();
    }
    
    private static string GetThumbClass(RatingContext<T> context, bool isUp, bool active)
    {
        var sizeClass = GetSizeClass(context.Size);
        var colorClass = active 
            ? isUp ? "text-success" : "text-danger"
            : "text-muted opacity-50";
        var interactiveClass = context.Interactive ? "rating-thumb-interactive" : "";
        
        return $"rating-thumb {sizeClass} {colorClass} {interactiveClass}".Trim();
    }
    
    private static string GetEmojiClass(RatingContext<T> context, bool selected)
    {
        var sizeClass = GetSizeClass(context.Size);
        var colorClass = selected ? "text-primary" : "text-muted opacity-25";
        var interactiveClass = context.Interactive ? "rating-emoji-interactive" : "";
        
        return $"rating-emoji-icon {sizeClass} {colorClass} {interactiveClass}".Trim();
    }
    
    private static string GetSizeClass(SizeType size) => size switch
    {
        SizeType.ExtraSmall => "text-xs",
        SizeType.Small => "text-sm",
        SizeType.Medium => "text-base",
        SizeType.Large => "text-lg",
        SizeType.ExtraLarge => "text-xl",
        _ => "text-base"
    };
    
    private static string GetIconForState(RatingContext<T> context, int position, bool filled, bool halfFilled)
    {
        if (context.CustomIcons.TryGetValue(position, out var customIcon))
            return customIcon;
        
        if (halfFilled)
            return context.HalfIcon;
        
        return filled ? context.FilledIcon : context.EmptyIcon;
    }
    
    private static string GetDefaultEmojiIcon(int rating, int maxRating)
    {
        var percentage = ((double)rating / maxRating) * 100;
        
        return percentage switch
        {
            <= 20 => RatingIcons.Emoji.VeryBad,
            <= 40 => RatingIcons.Emoji.Bad,
            <= 60 => RatingIcons.Emoji.Neutral,
            <= 80 => RatingIcons.Emoji.Good,
            _ => RatingIcons.Emoji.VeryGood
        };
    }
    
    private static async Task HandleRatingClick(RatingContext<T> context, double rating)
    {
        if (context.Interactive && context.OnValueChanged.HasDelegate)
        {
            await context.OnValueChanged.InvokeAsync(rating);
        }
    }
    
    /// <summary>
    /// Create fluent rating with simplified API
    /// </summary>
    public static RenderFragment<T> Create(
        Func<T, double> valueSelector,
        int maxRating = 5,
        RatingType type = RatingType.Stars,
        bool showValue = false,
        SizeType size = SizeType.Medium)
    {
        return item => builder =>
        {
            if (item == null) return;
            
            var value = valueSelector?.Invoke(item) ?? 0;
            var clampedValue = Math.Max(0, Math.Min(maxRating, value));
            
            builder.OpenElement(0, "div");
            builder.AddAttribute(1, "class", "rating-container d-flex align-center gap-1");
            builder.AddAttribute(2, "data-template", "rating");
            
            if (type == RatingType.Stars)
            {
                // Render stars
                for (int i = 1; i <= maxRating; i++)
                {
                    var filled = i <= Math.Floor(clampedValue);
                    var halfFilled = !filled && (clampedValue - (i - 1)) >= 0.5;
                    
                    builder.OpenElement(i * 10, "i");
                    builder.AddAttribute(i * 10 + 1, "class", GetStarClass(filled, halfFilled, size));
                    builder.AddContent(i * 10 + 2, GetStarIcon(filled, halfFilled));
                    builder.CloseElement();
                }
            }
            
            // Show value if requested
            if (showValue)
            {
                builder.OpenElement(1000, "span");
                builder.AddAttribute(1001, "class", "rating-value text-sm text-muted ms-2");
                builder.AddContent(1002, $"{value:0.#}");
                builder.CloseElement();
            }
            
            builder.CloseElement();
        };
    }
    
    private static string GetStarClass(bool filled, bool halfFilled, SizeType size)
    {
        var sizeClass = GetSizeClass(size);
        var colorClass = filled || halfFilled ? "text-warning" : "text-muted opacity-25";
        return $"icon rating-star {sizeClass} {colorClass}";
    }
    
    private static string GetStarIcon(bool filled, bool halfFilled)
    {
        if (halfFilled) return RatingIcons.Stars.Half;
        return filled ? RatingIcons.Stars.Filled : RatingIcons.Stars.Empty;
    }
}