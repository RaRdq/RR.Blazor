using Microsoft.AspNetCore.Components;
using RR.Blazor.Enums;
using RR.Blazor.Models;
using System.Linq.Expressions;

namespace RR.Blazor.Extensions;

/// <summary>
/// Extension methods for integrating Universal Template System with Choice components
/// </summary>
public static class ChoiceTemplateExtensions
{
    /// <summary>
    /// Create a group item template using badge rendering
    /// </summary>
    public static RenderFragment<IChoiceItem> UseBadgeTemplate<T>(
        this IChoiceGroup group,
        Expression<Func<T, object>> propertySelector,
        VariantType variant = VariantType.Secondary,
        bool clickable = false) where T : class
    {
        var badgeTemplate = new BadgeTemplate<T>
        {
            PropertySelector = propertySelector,
            Variant = variant,
            Clickable = clickable
        };

        return item => builder =>
        {
            if (item.Value is T typedValue)
            {
                builder.AddContent(0, badgeTemplate.Render(typedValue));
            }
            else
            {
                builder.AddContent(0, item.Label);
            }
        };
    }

    /// <summary>
    /// Create a group item template using currency rendering
    /// </summary>
    public static RenderFragment<IChoiceItem> UseCurrencyTemplate<T>(
        this IChoiceGroup group,
        Expression<Func<T, object>> valueSelector,
        string currencyCode = "USD",
        bool compact = false,
        bool showColors = true) where T : class
    {
        var currencyTemplate = new CurrencyTemplate<T>
        {
            PropertySelector = valueSelector,
            CurrencyCode = currencyCode,
            Compact = compact,
            ShowValueColors = showColors
        };

        return item => builder =>
        {
            if (item.Value is T typedValue)
            {
                builder.AddContent(0, currencyTemplate.Render(typedValue));
            }
            else
            {
                builder.AddContent(0, item.Label);
            }
        };
    }

    /// <summary>
    /// Create a group item template using stack rendering for multi-line content
    /// </summary>
    public static RenderFragment<IChoiceItem> UseStackTemplate<T>(
        this IChoiceGroup group,
        Expression<Func<T, string>> primarySelector,
        Expression<Func<T, string>> secondarySelector = null,
        Expression<Func<T, string>> iconSelector = null,
        StackOrientation orientation = StackOrientation.Vertical) where T : class
    {
        var stackTemplate = new StackTemplate<T>
        {
            PrimaryTextSelector = primarySelector,
            SecondaryTextSelector = secondarySelector,
            IconSelector = iconSelector,
            Orientation = orientation
        };

        return item => builder =>
        {
            if (item.Value is T typedValue)
            {
                builder.AddContent(0, stackTemplate.Render(typedValue));
            }
            else
            {
                // Fallback rendering
                builder.OpenElement(0, "div");
                builder.AddAttribute(1, "class", "choice-item-stack-fallback");
                
                if (!string.IsNullOrEmpty(item.Icon))
                {
                    builder.OpenElement(2, "i");
                    builder.AddAttribute(3, "class", "icon choice-item-icon");
                    builder.AddContent(4, item.Icon);
                    builder.CloseElement();
                }
                
                builder.OpenElement(5, "div");
                builder.AddAttribute(6, "class", "choice-item-content");
                
                builder.OpenElement(7, "span");
                builder.AddAttribute(8, "class", "choice-item-primary");
                builder.AddContent(9, item.Label);
                builder.CloseElement();
                
                if (!string.IsNullOrEmpty(item.Description))
                {
                    builder.OpenElement(10, "span");
                    builder.AddAttribute(11, "class", "choice-item-secondary");
                    builder.AddContent(12, item.Description);
                    builder.CloseElement();
                }
                
                builder.CloseElement(); // content
                builder.CloseElement(); // wrapper
            }
        };
    }

    /// <summary>
    /// Create a group item template using avatar rendering
    /// </summary>
    public static RenderFragment<IChoiceItem> UseAvatarTemplate<T>(
        this IChoiceGroup group,
        Expression<Func<T, string>> nameSelector,
        Expression<Func<T, string>> imageSelector = null,
        SizeType size = SizeType.Medium,
        bool clickable = false) where T : class
    {
        var avatarTemplate = new AvatarTemplate<T>
        {
            NameSelector = nameSelector,
            ImageUrlSelector = imageSelector,
            Size = size,
            Clickable = clickable
        };

        return item => builder =>
        {
            if (item.Value is T typedValue)
            {
                builder.AddContent(0, avatarTemplate.Render(typedValue));
            }
            else
            {
                // Fallback rendering
                builder.OpenElement(0, "div");
                builder.AddAttribute(1, "class", "choice-item-avatar-fallback d-flex align-center gap-2");
                
                if (!string.IsNullOrEmpty(item.AvatarUrl))
                {
                    builder.OpenElement(2, "img");
                    builder.AddAttribute(3, "src", item.AvatarUrl);
                    builder.AddAttribute(4, "alt", item.Label);
                    builder.AddAttribute(5, "class", "choice-avatar-img");
                    builder.CloseElement();
                }
                else
                {
                    // Generate initials
                    var initials = GenerateInitials(item.Label);
                    builder.OpenElement(6, "div");
                    builder.AddAttribute(7, "class", "choice-avatar-initials");
                    builder.AddContent(8, initials);
                    builder.CloseElement();
                }
                
                builder.OpenElement(9, "span");
                builder.AddAttribute(10, "class", "choice-avatar-name");
                builder.AddContent(11, item.Label);
                builder.CloseElement();
                
                builder.CloseElement();
            }
        };
    }

    /// <summary>
    /// Create a tree item template using progress rendering
    /// </summary>
    public static RenderFragment<IChoiceTreeItem> UseProgressTemplate<T>(
        this IChoiceTreeItem treeItem,
        Expression<Func<T, double>> valueSelector,
        double max = 100,
        bool showPercentage = true,
        VariantType variant = VariantType.Primary) where T : class
    {
        var progressTemplate = new ProgressTemplate<T>
        {
            ValueSelector = valueSelector,
            Max = max,
            ShowPercentage = showPercentage,
            Variant = variant
        };

        return item => builder =>
        {
            if (item.Value is T typedValue)
            {
                builder.AddContent(0, progressTemplate.Render(typedValue));
            }
            else
            {
                builder.AddContent(0, item.Label);
            }
        };
    }

    /// <summary>
    /// Create a tree item template using rating rendering
    /// </summary>
    public static RenderFragment<IChoiceTreeItem> UseRatingTemplate<T>(
        this IChoiceTreeItem treeItem,
        Expression<Func<T, double>> valueSelector,
        int maxRating = 5,
        string icon = "star",
        bool showValue = false,
        bool readOnly = true) where T : class
    {
        var ratingTemplate = new RatingTemplate<T>
        {
            ValueSelector = valueSelector,
            MaxRating = maxRating,
            Icon = icon,
            ShowValue = showValue,
            ReadOnly = readOnly
        };

        return item => builder =>
        {
            if (item.Value is T typedValue)
            {
                builder.AddContent(0, ratingTemplate.Render(typedValue));
            }
            else
            {
                builder.AddContent(0, item.Label);
            }
        };
    }

    /// <summary>
    /// Create a group template for nested collections
    /// </summary>
    public static RenderFragment<IChoiceGroup> UseGroupTemplate<T>(
        this IChoiceGroup group,
        Expression<Func<T, IEnumerable<object>>> itemsSelector,
        string separator = ", ",
        int maxItems = 5,
        bool renderAsBadges = false,
        VariantType badgeVariant = VariantType.Secondary) where T : class
    {
        var groupTemplate = new GroupTemplate<T>
        {
            ItemsSelector = itemsSelector,
            Separator = separator,
            MaxItems = maxItems,
            RenderAsBadges = renderAsBadges,
            BadgeVariant = badgeVariant
        };

        return groupItem => builder =>
        {
            if (groupItem.Value is T typedValue)
            {
                builder.AddContent(0, groupTemplate.Render(typedValue));
            }
            else
            {
                // Fallback rendering of group items
                builder.OpenElement(0, "div");
                builder.AddAttribute(1, "class", "choice-group-items-fallback");
                
                var items = groupItem.Items.Take(maxItems).ToList();
                for (int i = 0; i < items.Count; i++)
                {
                    var item = items[i];
                    
                    if (renderAsBadges)
                    {
                        builder.OpenElement(i * 2, "span");
                        builder.AddAttribute(i * 2 + 1, "class", $"badge-{badgeVariant.ToString().ToLowerInvariant()}");
                        builder.AddContent(i * 2 + 2, item.Label);
                        builder.CloseElement();
                    }
                    else
                    {
                        builder.AddContent(i * 2, item.Label);
                        if (i < items.Count - 1)
                        {
                            builder.AddContent(i * 2 + 1, separator);
                        }
                    }
                }
                
                var remainingCount = groupItem.Items.Count() - maxItems;
                if (remainingCount > 0)
                {
                    builder.OpenElement(items.Count * 2, "span");
                    builder.AddAttribute(items.Count * 2 + 1, "class", "choice-group-more");
                    builder.AddContent(items.Count * 2 + 2, $" and {remainingCount} more...");
                    builder.CloseElement();
                }
                
                builder.CloseElement();
            }
        };
    }

    /// <summary>
    /// Create a choice item from template builder
    /// </summary>
    public static ChoiceItem WithTemplate<T>(this ChoiceItem item, TemplateDefinition<T> template) where T : class
    {
        if (item.Value is T typedValue)
        {
            item.Template = choiceItem => template.Render(typedValue);
        }
        return item;
    }

    /// <summary>
    /// Create a choice group with header template
    /// </summary>
    public static ChoiceGroup WithHeaderTemplate(this ChoiceGroup group, RenderFragment<IChoiceGroup> headerTemplate)
    {
        group.HeaderTemplate = headerTemplate;
        return group;
    }

    /// <summary>
    /// Apply universal template patterns to choice groups
    /// </summary>
    public static class ChoiceTemplatePatterns
    {
        /// <summary>
        /// Create a status-based group with colored badges
        /// </summary>
        public static RenderFragment<IChoiceGroup> StatusGroupHeader => group => builder =>
        {
            var statusColor = GetStatusColor(group.Label);
            
            builder.OpenElement(0, "div");
            builder.AddAttribute(1, "class", "choice-group-status-header");
            
            // Status indicator
            builder.OpenElement(2, "div");
            builder.AddAttribute(3, "class", $"choice-group-status-indicator bg-{statusColor}");
            builder.CloseElement();
            
            // Group content
            builder.OpenElement(4, "div");
            builder.AddAttribute(5, "class", "choice-group-status-content");
            
            builder.OpenElement(6, "span");
            builder.AddAttribute(7, "class", "choice-group-status-title");
            builder.AddContent(8, group.Label);
            builder.CloseElement();
            
            if (!string.IsNullOrEmpty(group.Count))
            {
                builder.OpenElement(9, "span");
                builder.AddAttribute(10, "class", $"choice-group-status-count badge-{statusColor}");
                builder.AddContent(11, group.Count);
                builder.CloseElement();
            }
            
            builder.CloseElement();
            builder.CloseElement();
        };

        /// <summary>
        /// Create a category-based group with icons
        /// </summary>
        public static RenderFragment<IChoiceGroup> CategoryGroupHeader => group => builder =>
        {
            builder.OpenElement(0, "div");
            builder.AddAttribute(1, "class", "choice-group-category-header");
            
            if (!string.IsNullOrEmpty(group.Icon))
            {
                builder.OpenElement(2, "i");
                builder.AddAttribute(3, "class", "icon choice-group-category-icon");
                builder.AddContent(4, group.Icon);
                builder.CloseElement();
            }
            
            builder.OpenElement(5, "div");
            builder.AddAttribute(6, "class", "choice-group-category-content");
            
            builder.OpenElement(7, "h4");
            builder.AddAttribute(8, "class", "choice-group-category-title");
            builder.AddContent(9, group.Label);
            builder.CloseElement();
            
            if (!string.IsNullOrEmpty(group.Count))
            {
                builder.OpenElement(10, "span");
                builder.AddAttribute(11, "class", "choice-group-category-count");
                builder.AddContent(12, group.Count);
                builder.CloseElement();
            }
            
            builder.CloseElement();
            builder.CloseElement();
        };

        /// <summary>
        /// Create a compact badge-style group header
        /// </summary>
        public static RenderFragment<IChoiceGroup> CompactGroupHeader => group => builder =>
        {
            builder.OpenElement(0, "div");
            builder.AddAttribute(1, "class", "choice-group-compact-header");
            
            builder.OpenElement(2, "span");
            builder.AddAttribute(3, "class", "choice-group-compact-badge badge-primary");
            builder.AddContent(4, group.Label);
            
            if (!string.IsNullOrEmpty(group.Count))
            {
                builder.AddContent(5, " ");
                builder.AddContent(6, group.Count);
            }
            
            builder.CloseElement();
            builder.CloseElement();
        };

        private static string GetStatusColor(string status)
        {
            return status.ToLowerInvariant() switch
            {
                var s when s.Contains("active") || s.Contains("success") => "success",
                var s when s.Contains("pending") || s.Contains("warning") => "warning", 
                var s when s.Contains("error") || s.Contains("danger") => "error",
                var s when s.Contains("info") => "info",
                _ => "secondary"
            };
        }
    }

    private static string GenerateInitials(string name)
    {
        if (string.IsNullOrEmpty(name)) return "?";
        
        var parts = name.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 0) return "?";
        
        if (parts.Length == 1)
        {
            return parts[0].Substring(0, Math.Min(2, parts[0].Length)).ToUpperInvariant();
        }
        
        return (parts[0][0].ToString() + parts[parts.Length - 1][0].ToString()).ToUpperInvariant();
    }
}