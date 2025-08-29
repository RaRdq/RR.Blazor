using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using RR.Blazor.Attributes;
using RR.Blazor.Components.Base;
using RR.Blazor.Enums;

namespace RR.Blazor.Components.Core
{
    /// <summary>
    /// Professional divider component for visual separation with text support and multiple styles.
    /// Supports horizontal/vertical orientation, customizable styling, and automatic menu integration.
    /// </summary>
    [Component("RDivider", Category = "Core", Complexity = ComponentComplexity.Simple)]
    [AIOptimized(Prompt = "Create a professional divider for visual separation", 
                 CommonUse = "section separation, menu dividers, form sections", 
                 AvoidUsage = "Don't overuse - only for logical content separation")]
    public class RDivider : RForwardingComponentBase
    {
        [Parameter]
        [AIParameter(Hint = "Use for section labels like Settings, Advanced Options, OR. Keep short", IsRequired = false)]
        public string Text { get; set; }
        
        [Parameter]
        [AIParameter(Hint = "Material icon name for decorative divider (e.g. 'star', 'circle', 'diamond')", IsRequired = false)]
        public string Icon { get; set; }
        
        [Parameter]
        [AIParameter(Hint = "Icon size using standard size types", 
                     IsRequired = false)]
        public SizeType IconSize { get; set; } = SizeType.Medium;
        
        [Parameter]
        [AIParameter(Hint = "Icon style: outlined or filled", 
                     SuggestedValues = new[] { "outlined", "filled" }, 
                     IsRequired = false)]
        public string IconStyle { get; set; } = "outlined";
        
        [Parameter]
        [AIParameter(Hint = "Horizontal for section breaks, Vertical for inline separation", 
                     SuggestedValues = new[] { "Horizontal", "Vertical" }, 
                     IsRequired = false)]
        public DividerVariant Variant { get; set; } = DividerVariant.Horizontal;
        
        [Parameter]
        [AIParameter(Hint = "Solid for standard, Dashed for softer, Dotted for subtle separation", 
                     SuggestedValues = new[] { "Solid", "Dashed", "Dotted" }, 
                     IsRequired = false)]
        public DividerStyle DividerStyle { get; set; } = DividerStyle.Solid;
        
        [Parameter]
        [AIParameter(Hint = "Center for balanced labels, Left for section headers, Right for special cases", 
                     SuggestedValues = new[] { "Left", "Center", "Right" }, 
                     IsRequired = false)]
        public DividerTextAlign TextAlign { get; set; } = DividerTextAlign.Center;
        
        [Parameter]
        [AIParameter(Hint = "Semantic color variant for theming", 
                     SuggestedValues = new[] { "Default", "Primary", "Success", "Warning", "Error", "Info" }, 
                     IsRequired = false)]
        public string SemanticVariant { get; set; }
        
        [Parameter] public RenderFragment IconContent { get; set; }
        
        [Parameter] public string Subtitle { get; set; }
        
        [Parameter] public bool ShowLine { get; set; } = true;
        
        [Parameter] public SizeType Size { get; set; } = SizeType.Medium;
        
        [CascadingParameter(Name = "ParentListVariant")] private ListVariant? ParentListVariant { get; set; }

        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            var sequence = 0;
            var elementTag = Variant == DividerVariant.Vertical ? "span" : "div";
            
            builder.OpenElement(++sequence, elementTag);
            builder.AddAttribute(++sequence, "class", GetDividerClasses());
            
            ForwardParametersExcept(builder, ref sequence, 
                nameof(Text), nameof(Variant), nameof(DividerStyle), nameof(TextAlign), 
                nameof(Variant), nameof(Class), nameof(ChildContent),
                nameof(Icon), nameof(IconSize), nameof(IconStyle), nameof(IconContent), nameof(Size), nameof(Subtitle), nameof(ShowLine));
            
            if (!string.IsNullOrEmpty(Text) || ChildContent != null || !string.IsNullOrEmpty(Icon) || IconContent != null)
            {
                // Check if we should use stacked layout (centered with subtitle, icon, or child content)
                var useStackedLayout = TextAlign == DividerTextAlign.Center && 
                                     (!string.IsNullOrEmpty(Subtitle) || !string.IsNullOrEmpty(Icon) || IconContent != null || ChildContent != null);
                
                builder.OpenElement(++sequence, "span");
                builder.AddAttribute(++sequence, "class", useStackedLayout ? "divider-content divider-content-stacked" : "divider-content");
                
                if (!string.IsNullOrEmpty(Icon))
                {
                    builder.OpenElement(++sequence, "i");
                    builder.AddAttribute(++sequence, "class", GetIconClasses());
                    builder.AddContent(++sequence, Icon);
                    builder.CloseElement();
                }
                else if (IconContent != null)
                {
                    builder.AddContent(++sequence, IconContent);
                }
                
                if (!string.IsNullOrEmpty(Text) || !string.IsNullOrEmpty(Subtitle))
                {
                    builder.OpenElement(++sequence, "div");
                    builder.AddAttribute(++sequence, "class", "divider-text-group");
                    
                    if (!string.IsNullOrEmpty(Text))
                    {
                        builder.OpenElement(++sequence, "span");
                        builder.AddAttribute(++sequence, "class", "font-semibold");
                        builder.AddContent(++sequence, Text);
                        builder.CloseElement();
                    }
                    
                    if (!string.IsNullOrEmpty(Subtitle))
                    {
                        builder.OpenElement(++sequence, "span");
                        builder.AddAttribute(++sequence, "class", "divider-subtitle");
                        builder.AddContent(++sequence, Subtitle);
                        builder.CloseElement();
                    }
                    
                    builder.CloseElement();
                }
                
                if (ChildContent != null)
                {
                    builder.AddContent(++sequence, ChildContent);
                }
                
                builder.CloseElement();
            }
            
            builder.CloseElement();
        }
        
        private string GetDividerClasses()
        {
            var classes = new List<string>();
            
            if (ParentListVariant == ListVariant.Menu)
            {
                classes.Add("menu-list-divider");
            }
            else
            {
                classes.Add("divider");
                
                if (Variant == DividerVariant.Vertical) classes.Add("divider-vertical");
                
                // Build style-variant combination classes like semantic-variants mixin
                var styleVariant = GetStyleVariantClass();
                if (!string.IsNullOrEmpty(styleVariant)) classes.Add(styleVariant);
                
                if (!string.IsNullOrEmpty(Text) || ChildContent != null || !string.IsNullOrEmpty(Icon) || IconContent != null || !string.IsNullOrEmpty(Subtitle))
                {
                    classes.Add($"divider-text-{TextAlign.ToString().ToLower()}");
                }
                else
                {
                    classes.Add("divider-empty");
                }
                
                if (!string.IsNullOrEmpty(SemanticVariant)) classes.Add($"divider-{SemanticVariant.ToLower()}");
                if (!ShowLine) classes.Add("divider-no-line");
                
                // Always apply a size class - normal is the default
                var sizeClass = Size switch
                {
                    SizeType.Small => "divider-compact",
                    SizeType.Large => "divider-spacious",
                    _ => "divider-normal" // Default for Medium and any other size
                };
                classes.Add(sizeClass);
            }
            
            if (!string.IsNullOrEmpty(Class)) classes.Add(Class);
            
            return string.Join(" ", classes);
        }

        private string GetStyleVariantClass()
        {
            var style = DividerStyle.ToString().ToLower();
            if (style == "solid") return string.Empty;
            
            var baseClass = $"divider-{style}";
            
            if (!string.IsNullOrEmpty(SemanticVariant))
            {
                return $"{baseClass}-{SemanticVariant.ToLower()}";
            }
            
            return baseClass;
        }
        
        private string GetIconClasses()
        {
            var classes = new List<string>();
            
            classes.Add("icon");
            
            // Map SizeType to icon size classes
            var sizeClass = IconSize switch
            {
                SizeType.ExtraSmall => "icon-xs",
                SizeType.Small => "icon-sm",
                SizeType.Medium or SizeType.Default => "icon-base",
                SizeType.Large => "icon-lg",
                SizeType.ExtraLarge or SizeType.XLarge => "icon-xl",
                SizeType.ExtraLarge2X => "icon-2xl",
                _ => "icon-base"
            };
            classes.Add(sizeClass);
            
            if (IconStyle == "filled") classes.Add("icon-filled");
            
            return string.Join(" ", classes);
        }
    }
}