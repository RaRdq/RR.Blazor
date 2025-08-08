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
        [AIParameter(Hint = "Icon size variant: xs, sm, base, lg, xl", 
                     SuggestedValues = new[] { "xs", "sm", "base", "lg", "xl" }, 
                     IsRequired = false)]
        public string IconSize { get; set; } = "base";
        
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
        public DividerStyle Style { get; set; } = DividerStyle.Solid;
        
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
        
        [Parameter] public string Class { get; set; }
        
        [Parameter] public RenderFragment ChildContent { get; set; }
        
        [Parameter] public RenderFragment IconContent { get; set; }
        
        [CascadingParameter(Name = "ParentListVariant")] private ListVariant? ParentListVariant { get; set; }

        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            var sequence = 0;
            var elementTag = Variant == DividerVariant.Vertical ? "span" : "div";
            
            builder.OpenElement(++sequence, elementTag);
            builder.AddAttribute(++sequence, "class", GetDividerClasses());
            
            // Forward additional attributes except our specific parameters
            ForwardParametersExcept(builder, ref sequence, 
                nameof(Text), nameof(Variant), nameof(Style), nameof(TextAlign), 
                nameof(SemanticVariant), nameof(Class), nameof(ChildContent),
                nameof(Icon), nameof(IconSize), nameof(IconStyle), nameof(IconContent));
            
            if (!string.IsNullOrEmpty(Text) || ChildContent != null || !string.IsNullOrEmpty(Icon) || IconContent != null)
            {
                builder.OpenElement(++sequence, "span");
                builder.AddAttribute(++sequence, "class", "content");
                
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
                
                if (!string.IsNullOrEmpty(Text))
                {
                    builder.AddContent(++sequence, Text);
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
                
                if (Variant == DividerVariant.Vertical)
                {
                    classes.Add("divider-vertical");
                }
                
                if (Style != DividerStyle.Solid)
                {
                    classes.Add($"divider-{Style.ToString().ToLower()}");
                }
                
                if (!string.IsNullOrEmpty(Text) || ChildContent != null || !string.IsNullOrEmpty(Icon) || IconContent != null)
                {
                    classes.Add($"divider-text-{TextAlign.ToString().ToLower()}");
                    
                    if (!string.IsNullOrEmpty(Icon) || IconContent != null)
                    {
                        classes.Add("divider-icon");
                    }
                }
                else
                {
                    classes.Add("divider-empty");
                }
                
                if (!string.IsNullOrEmpty(SemanticVariant) && SemanticVariant != "Default")
                {
                    classes.Add($"divider-{SemanticVariant.ToLower()}");
                }
            }
            
            if (!string.IsNullOrEmpty(Class))
            {
                classes.Add(Class);
            }
            
            return string.Join(" ", classes);
        }
        
        private string GetIconClasses()
        {
            var classes = new List<string>();
            
            // Use material-symbols-rounded as the base class
            classes.Add("material-symbols-rounded");
            
            // Add size class
            classes.Add($"icon-{IconSize}");
            
            // Add semantic color if specified
            if (!string.IsNullOrEmpty(SemanticVariant) && SemanticVariant != "Default")
            {
                classes.Add($"icon-{SemanticVariant.ToLower()}");
            }
            
            // Apply filled style through font-variation-settings
            if (IconStyle == "filled")
            {
                classes.Add("material-symbols-rounded-filled");
            }
            
            return string.Join(" ", classes);
        }
    }
}