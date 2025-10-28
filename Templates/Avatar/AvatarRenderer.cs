using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using RR.Blazor.Enums;

namespace RR.Blazor.Templates.Avatar;

/// <summary>
/// Renders avatar templates using HTML output
/// Supports images, initials, status indicators, and avatar groups
/// </summary>
/// <typeparam name="T">Type of data being rendered</typeparam>
public class AvatarRenderer<T> where T : class
{
    /// <summary>
    /// Renders the avatar template
    /// </summary>
    public RenderFragment Render(AvatarContext<T> context)
    {
        return builder =>
        {
            if (context?.Item == null) return;
            
            var sizeClass = GetSizeClass(context.Size);
            var shapeClass = GetShapeClass(context.Shape);
            var colorClass = GetColorClass(context.ColorVariant);
            var avatarClass = $"avatar {sizeClass} {shapeClass} {colorClass} {context.Class}".Trim();
            
            // Main avatar container
            builder.OpenElement(0, "div");
            builder.AddAttribute(1, "class", "avatar-wrapper");
            builder.AddAttribute(2, "data-template", "avatar");
            
            // Avatar element
            builder.OpenElement(3, "div");
            builder.AddAttribute(4, "class", avatarClass);
            
            if (context.Disabled)
                builder.AddAttribute(5, "disabled", true);
                
            if (context.Selected)
                builder.AddAttribute(6, "data-selected", true);
            
            if (context.ShowBorder)
                builder.AddAttribute(7, "data-border", true);
            
            if (context.Clickable && context.OnClick.HasDelegate)
            {
                builder.AddAttribute(8, "onclick", EventCallback.Factory.Create(context.Item, () => context.OnClick.InvokeAsync(context.Item)));
                builder.AddAttribute(9, "style", "cursor: pointer;");
                builder.AddAttribute(10, "data-clickable", true);
            }
            
            if (!string.IsNullOrEmpty(context.BackgroundColor))
            {
                builder.AddAttribute(11, "style", $"background-color: {context.BackgroundColor};");
            }
            
            // Render avatar content
            if (!string.IsNullOrEmpty(context.ImageUrl))
            {
                // Image avatar
                builder.OpenElement(12, "img");
                builder.AddAttribute(13, "src", context.ImageUrl);
                builder.AddAttribute(14, "alt", context.Name ?? "Avatar");
                builder.AddAttribute(15, "class", "avatar-image");
                builder.AddAttribute(16, "loading", "lazy");
                builder.CloseElement();
            }
            else if (!string.IsNullOrEmpty(context.Initials))
            {
                // Initials avatar
                builder.OpenElement(17, "span");
                builder.AddAttribute(18, "class", "avatar-initials");
                builder.AddContent(19, context.Initials);
                builder.CloseElement();
            }
            else
            {
                // Default avatar icon
                builder.OpenElement(20, "i");
                builder.AddAttribute(21, "class", "icon avatar-icon");
                builder.AddContent(22, "person");
                builder.CloseElement();
            }
            
            builder.CloseElement(); // avatar
            
            // Status indicator
            if (context.Status != AvatarStatus.None)
            {
                RenderStatusIndicator(builder, context.Status, 100);
            }
            
            // Badge indicator
            if (!string.IsNullOrEmpty(context.Badge))
            {
                RenderBadge(builder, context.Badge, 200);
            }
            
            builder.CloseElement(); // avatar-wrapper
        };
    }
    
    private static void RenderStatusIndicator(RenderTreeBuilder builder, AvatarStatus status, int sequence)
    {
        var statusClass = GetStatusClass(status);
        var statusColor = GetStatusColor(status);
        
        builder.OpenElement(sequence, "span");
        builder.AddAttribute(sequence + 1, "class", $"avatar-status {statusClass}");
        builder.AddAttribute(sequence + 2, "data-status", status.ToString().ToLower());
        builder.AddAttribute(sequence + 3, "style", $"background-color: {statusColor};");
        builder.AddAttribute(sequence + 4, "title", GetStatusTooltip(status));
        builder.CloseElement();
    }
    
    private static void RenderBadge(RenderTreeBuilder builder, string badge, int sequence)
    {
        builder.OpenElement(sequence, "span");
        builder.AddAttribute(sequence + 1, "class", "avatar-badge");
        builder.AddContent(sequence + 2, badge);
        builder.CloseElement();
    }
    
    private static string GetSizeClass(SizeType size) => size switch
    {
        SizeType.ExtraSmall => "avatar-xs",
        SizeType.Small => "avatar-sm",
        SizeType.Medium => "avatar-md",
        SizeType.Large => "avatar-lg",
        SizeType.ExtraLarge => "avatar-xl",
        _ => "avatar-md"
    };
    
    private static string GetShapeClass(AvatarShape shape) => shape switch
    {
        AvatarShape.Circle => "avatar-circle",
        AvatarShape.Square => "avatar-square",
        AvatarShape.Rounded => "avatar-rounded",
        _ => "avatar-circle"
    };
    
    private static string GetColorClass(VariantType variant) => $"avatar-{variant.ToString().ToLower()}";
    
    private static string GetStatusClass(AvatarStatus status) => $"status-{status.ToString().ToLower()}";
    
    private static string GetStatusColor(AvatarStatus status) => status switch
    {
        AvatarStatus.Online => "var(--bs-success)",
        AvatarStatus.Away => "var(--bs-warning)",
        AvatarStatus.Busy => "var(--bs-danger)",
        AvatarStatus.Offline => "var(--bs-secondary)",
        _ => "var(--bs-secondary)"
    };
    
    private static string GetStatusTooltip(AvatarStatus status) => status switch
    {
        AvatarStatus.Online => "Online",
        AvatarStatus.Away => "Away",
        AvatarStatus.Busy => "Busy",
        AvatarStatus.Offline => "Offline",
        _ => ""
    };
    
    /// <summary>
    /// Create fluent avatar with simplified API
    /// </summary>
    public static RenderFragment<T> Create(
        Func<T, string> nameSelector,
        Func<T, string> imageSelector = null,
        SizeType size = SizeType.Medium,
        AvatarShape shape = AvatarShape.Circle,
        Func<T, AvatarStatus> statusSelector = null,
        bool clickable = false,
        EventCallback<T> onClick = default)
    {
        return item => builder =>
        {
            if (item == null) return;
            
            var name = nameSelector?.Invoke(item) ?? string.Empty;
            var imageUrl = imageSelector?.Invoke(item);
            var status = statusSelector?.Invoke(item) ?? AvatarStatus.None;
            var initials = AvatarTemplate<T>.GenerateInitials(name);
            var color = AvatarTemplate<T>.GetConsistentColor(name);
            
            var sizeClass = GetSizeClass(size);
            var shapeClass = GetShapeClass(shape);
            var colorClass = GetColorClass(color);
            var avatarClass = $"avatar {sizeClass} {shapeClass} {colorClass}".Trim();
            
            // Main container
            builder.OpenElement(0, "div");
            builder.AddAttribute(1, "class", "avatar-wrapper");
            builder.AddAttribute(2, "data-template", "avatar");
            
            // Avatar
            builder.OpenElement(3, "div");
            builder.AddAttribute(4, "class", avatarClass);
            
            if (clickable && onClick.HasDelegate)
            {
                builder.AddAttribute(5, "onclick", EventCallback.Factory.Create(item, () => onClick.InvokeAsync(item)));
                builder.AddAttribute(6, "style", "cursor: pointer;");
                builder.AddAttribute(7, "data-clickable", true);
            }
            
            // Content
            if (!string.IsNullOrEmpty(imageUrl))
            {
                builder.OpenElement(8, "img");
                builder.AddAttribute(9, "src", imageUrl);
                builder.AddAttribute(10, "alt", name ?? "Avatar");
                builder.AddAttribute(11, "class", "avatar-image");
                builder.AddAttribute(12, "loading", "lazy");
                builder.CloseElement();
            }
            else if (!string.IsNullOrEmpty(initials))
            {
                builder.OpenElement(13, "span");
                builder.AddAttribute(14, "class", "avatar-initials");
                builder.AddContent(15, initials);
                builder.CloseElement();
            }
            else
            {
                builder.OpenElement(16, "i");
                builder.AddAttribute(17, "class", "icon avatar-icon");
                builder.AddContent(18, "person");
                builder.CloseElement();
            }
            
            builder.CloseElement(); // avatar
            
            // Status
            if (status != AvatarStatus.None)
            {
                RenderStatusIndicator(builder, status, 100);
            }
            
            builder.CloseElement(); // wrapper
        };
    }
    
    /// <summary>
    /// Creates an avatar group/stack for multiple avatars
    /// </summary>
    public static RenderFragment<IEnumerable<T>> CreateGroup(
        Func<T, string> nameSelector,
        Func<T, string> imageSelector = null,
        SizeType size = SizeType.Medium,
        int maxDisplay = 5,
        bool showOverflow = true)
    {
        return items => builder =>
        {
            if (items == null || !items.Any()) return;
            
            var itemList = items.Take(maxDisplay).ToList();
            var overflow = items.Count() - maxDisplay;
            
            builder.OpenElement(0, "div");
            builder.AddAttribute(1, "class", "avatar-group");
            builder.AddAttribute(2, "data-template", "avatar-group");
            
            var index = 0;
            foreach (var item in itemList)
            {
                var name = nameSelector?.Invoke(item) ?? string.Empty;
                var imageUrl = imageSelector?.Invoke(item);
                var initials = AvatarTemplate<T>.GenerateInitials(name);
                var color = AvatarTemplate<T>.GetConsistentColor(name);
                
                var sizeClass = GetSizeClass(size);
                var colorClass = GetColorClass(color);
                var avatarClass = $"avatar avatar-circle {sizeClass} {colorClass} avatar-stacked".Trim();
                
                builder.OpenElement(index * 10 + 3, "div");
                builder.AddAttribute(index * 10 + 4, "class", avatarClass);
                builder.AddAttribute(index * 10 + 6, "title", name);
                
                if (!string.IsNullOrEmpty(imageUrl))
                {
                    builder.OpenElement(index * 10 + 7, "img");
                    builder.AddAttribute(index * 10 + 8, "src", imageUrl);
                    builder.AddAttribute(index * 10 + 9, "alt", name ?? "Avatar");
                    builder.AddAttribute(index * 10 + 10, "class", "avatar-image");
                    builder.CloseElement();
                }
                else
                {
                    builder.OpenElement(index * 10 + 11, "span");
                    builder.AddAttribute(index * 10 + 12, "class", "avatar-initials");
                    builder.AddContent(index * 10 + 13, initials);
                    builder.CloseElement();
                }
                
                builder.CloseElement();
                index++;
            }
            
            // Overflow indicator
            if (showOverflow && overflow > 0)
            {
                var sizeClass = GetSizeClass(size);
                builder.OpenElement(1000, "div");
                builder.AddAttribute(1001, "class", $"avatar avatar-circle {sizeClass} avatar-secondary avatar-stacked");
                builder.OpenElement(1003, "span");
                builder.AddAttribute(1004, "class", "avatar-initials");
                builder.AddContent(1005, $"+{overflow}");
                builder.CloseElement();
                builder.CloseElement();
            }
            
            builder.CloseElement(); // avatar-group
        };
    }
}
