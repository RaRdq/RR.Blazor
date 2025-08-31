using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.Web;
using RR.Blazor.Attributes;
using RR.Blazor.Components.Base;
using RR.Blazor.Enums;

namespace RR.Blazor.Components.Core
{
    /// <summary>
    /// Professional avatar component for user representation with status indicators and interactive features.
    /// Supports images, initials, icons, status badges, and notification counts with responsive sizing.
    /// </summary>
    [Component("RAvatar", Category = "Core", Complexity = ComponentComplexity.Simple)]
    [AIOptimized(Prompt = "Create a professional avatar for user representation", 
                 CommonUse = "user profiles, team members, contact lists", 
                 AvoidUsage = "Don't use for decorative icons - use Icon component instead")]
    public class RAvatar : RForwardingComponentBase
    {
        [Parameter]
        [AIParameter(Hint = "Medium is default, Large for prominent display, Small for compact layouts, 2XL for hero sections", 
                     SuggestedValues = new[] { "ExtraSmall", "Small", "Medium", "Large", "ExtraLarge", "ExtraLarge2X" })]
        public SizeType Size { get; set; } = SizeType.Medium;
        
        [Parameter]
        [AIParameter(Hint = "Default for neutral, Primary for branded, Success/Warning/Error for status", 
                     SuggestedValues = new[] { "Default", "Primary", "Success", "Warning", "Error", "Gradient" }, 
                     IsRequired = false)]
        public AvatarVariant Variant { get; set; } = AvatarVariant.Default;
        
        [Parameter]
        [AIParameter(Hint = "Use high-quality square images. Falls back to text/icon if not provided", IsRequired = false)]
        public string ImageSrc { get; set; }
        
        [Parameter]
        [AIParameter(Hint = "Use descriptive text like 'John Doe profile picture' for screen readers", IsRequired = false)]
        public string Alt { get; set; }
        
        [Parameter]
        [AIParameter(Hint = "Use 1-2 character initials like JD, AB. Automatically uppercase", IsRequired = false)]
        public string Text { get; set; }
        
        [Parameter]
        [AIParameter(Hint = "Common icons: person, account_circle, face, group. Defaults to person", 
                     SuggestedValues = new[] { "person", "account_circle", "face", "group" })]
        public string Icon { get; set; }
        
        [Parameter]
        [AIParameter(Hint = "Use for presence indicators in chat, team lists, user directories")]
        public bool ShowStatus { get; set; }
        
        [Parameter]
        [AIParameter(Hint = "Online for active, Away for inactive, Busy for do-not-disturb, Offline for unavailable", 
                     SuggestedValues = new[] { "Online", "Away", "Busy", "Offline" })]
        public AvatarStatus Status { get; set; } = AvatarStatus.None;
        
        [Parameter]
        [AIParameter(Hint = "Use for unread message counts, notification indicators, pending items")]
        public bool ShowBadge { get; set; }
        
        [Parameter]
        [AIParameter(Hint = "Shows actual count, displays 99+ for values over 99")]
        public int BadgeCount { get; set; }
        
        [Parameter]
        [AIParameter(Hint = "Set to true for interactive avatars that open profiles or menus")]
        public bool IsClickable { get; set; }
        
        [Parameter] public EventCallback<MouseEventArgs> OnClick { get; set; }

        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            var sequence = 0;
            
            builder.OpenElement(++sequence, "div");
            builder.AddAttribute(++sequence, "class", GetAvatarClasses());
            
            // Forward additional attributes except our specific parameters
            ForwardParametersExcept(builder, ref sequence, 
                nameof(Size), nameof(Variant), nameof(ImageSrc), nameof(Alt), 
                nameof(Text), nameof(Icon), nameof(ShowStatus), nameof(Status),
                nameof(ShowBadge), nameof(BadgeCount), nameof(IsClickable), 
                nameof(Class), nameof(OnClick));

            if (IsClickable)
            {
                builder.AddAttribute(++sequence, "role", "button");
                builder.AddAttribute(++sequence, "onclick", EventCallback.Factory.Create<MouseEventArgs>(this, HandleClick));
            }
            
            // Avatar content
            if (!string.IsNullOrEmpty(ImageSrc))
            {
                builder.OpenElement(++sequence, "img");
                builder.AddAttribute(++sequence, "src", ImageSrc);
                builder.AddAttribute(++sequence, "alt", Alt);
                builder.AddAttribute(++sequence, "class", GetImageClasses());
                builder.AddAttribute(++sequence, "loading", "lazy");
                builder.CloseElement();
            }
            else if (!string.IsNullOrEmpty(Icon))
            {
                builder.OpenElement(++sequence, "i");
                builder.AddAttribute(++sequence, "class", GetIconClasses());
                builder.AddContent(++sequence, Icon);
                builder.CloseElement();
            }
            else if (!string.IsNullOrEmpty(Text))
            {
                builder.OpenElement(++sequence, "span");
                builder.AddAttribute(++sequence, "class", GetTextClasses());
                builder.AddContent(++sequence, Text);
                builder.CloseElement();
            }
            else
            {
                builder.OpenElement(++sequence, "i");
                builder.AddAttribute(++sequence, "class", GetIconClasses());
                builder.AddContent(++sequence, "person");
                builder.CloseElement();
            }
            
            // Status indicator
            if (ShowStatus)
            {
                builder.OpenElement(++sequence, "div");
                builder.AddAttribute(++sequence, "class", GetStatusClasses());
                builder.CloseElement();
            }
            
            // Badge/Count indicator
            if (ShowBadge && BadgeCount > 0)
            {
                builder.OpenElement(++sequence, "div");
                builder.AddAttribute(++sequence, "class", GetBadgeClasses());
                builder.AddContent(++sequence, BadgeCount > 99 ? "99+" : BadgeCount.ToString());
                builder.CloseElement();
            }
            
            builder.CloseElement();
        }
        
        private string GetAvatarClasses()
        {
            var classes = new List<string>();
            
            // Check if this is a group-more indicator
            var isGroupMore = !string.IsNullOrEmpty(Class) && Class.Contains("avatar-group-more");
            
            // Size modifier - single semantic class
            var sizeClass = Size switch
            {
                SizeType.ExtraSmall => "avatar-xs",
                SizeType.Small => "avatar-sm",
                SizeType.Medium => "avatar-md",
                SizeType.Large => "avatar-lg",
                SizeType.ExtraLarge => "avatar-xl",
                SizeType.ExtraLarge2X => "avatar-2xl",
                _ => "avatar-md"
            };
            classes.Add(sizeClass);
            
            // Only add variant modifier if not a group-more indicator
            if (!isGroupMore)
            {
                var variantClass = Variant switch
                {
                    AvatarVariant.Default => "avatar-default",
                    AvatarVariant.Primary => "avatar-primary",
                    AvatarVariant.Success => "avatar-success",
                    AvatarVariant.Warning => "avatar-warning",
                    AvatarVariant.Error => "avatar-error",
                    AvatarVariant.Gradient => "avatar-gradient",
                    _ => "avatar-default"
                };
                classes.Add(variantClass);
            }
            
            // Interactive modifier
            if (IsClickable)
            {
                classes.Add("avatar-interactive");
            }
            
            if (!string.IsNullOrEmpty(Class))
            {
                classes.Add(Class);
            }
            
            return string.Join(" ", classes);
        }
        
        private string GetImageClasses()
        {
            return "avatar-image w-full h-full object-cover rounded-full";
        }
        
        private string GetIconClasses()
        {
            var iconSizeClass = Size switch
            {
                SizeType.ExtraSmall => "text-xs",
                SizeType.Small => "text-sm",
                SizeType.Medium => "text-lg",
                SizeType.Large => "text-2xl",
                SizeType.ExtraLarge => "text-3xl",
                SizeType.ExtraLarge2X => "text-4xl",
                _ => "text-lg"
            };
            
            return $"icon {iconSizeClass} text-center";
        }
        
        private string GetTextClasses()
        {
            var textSizeClass = Size switch
            {
                SizeType.ExtraSmall => "text-2xs",
                SizeType.Small => "text-xs",
                SizeType.Medium => "text-sm",
                SizeType.Large => "text-base",
                SizeType.ExtraLarge => "text-lg",
                SizeType.ExtraLarge2X => "text-xl",
                _ => "text-sm"
            };
            
            return $"avatar-text font-bold text-transform-uppercase {textSizeClass} text-center";
        }
        
        private string GetStatusClasses()
        {
            var statusClass = Status switch
            {
                AvatarStatus.Online => "avatar-status-online bg-success",
                AvatarStatus.Away => "avatar-status-away bg-warning",
                AvatarStatus.Busy => "avatar-status-away bg-warning",
                AvatarStatus.Offline => "avatar-status-offline bg-error",
                _ => "avatar-status-offline bg-error"
            };
            
            return $"avatar-status absolute bottom-0 right-0 w-3 h-3 rounded-full border-2 border-surface {statusClass}";
        }
        
        private string GetBadgeClasses()
        {
            var badgeSizeClass = Size switch
            {
                SizeType.ExtraSmall => "text-2xs min-w-4 h-4",
                SizeType.Small => "text-xs min-w-5 h-5",
                SizeType.Medium => "text-xs min-w-6 h-6",
                SizeType.Large => "text-sm min-w-7 h-7",
                SizeType.ExtraLarge => "text-sm min-w-8 h-8",
                SizeType.ExtraLarge2X => "text-base min-w-10 h-10",
                _ => "text-xs min-w-6 h-6"
            };
            
            return $"avatar-badge absolute -top-1 -right-1 bg-error text-on-error rounded-full {badgeSizeClass} d-flex align-center justify-center font-bold";
        }
        
        private async Task HandleClick(MouseEventArgs e)
        {
            if (IsClickable)
            {
                await OnClick.InvokeAsync(e);
            }
        }
    }
}