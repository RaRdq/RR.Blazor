using Microsoft.AspNetCore.Components;
using RR.Blazor.Attributes;
using RR.Blazor.Enums;

namespace RR.Blazor.Components.Base
{
    /// <summary>
    /// Base class for components that support both semantic variants and design modes
    /// Extends RVariantComponentBase to add orthogonal design mode styling
    /// </summary>
    public abstract class RDesignableComponentBase<TSize, TVariant> : RVariantComponentBase<TSize, TVariant>
        where TSize : Enum
        where TVariant : Enum
    {
        #region Design Mode Properties
        
        /// <summary>
        /// Visual design mode applied to the component (glass, neumorphism, etc.)
        /// </summary>
        [Parameter]
        [AIParameter("Visual design mode", SuggestedValues = new[] { "Material", "Glass", "Neumorphism", "Gradient", "Outlined", "Ghost", "Neon", "Shimmer", "Enterprise", "Elevated", "Flat", "Frosted" })]
        public DesignMode DesignMode { get; set; } = DesignMode.Material;
        
        /// <summary>
        /// Intensity of the design effect (light, medium, heavy)
        /// </summary>
        [Parameter]
        [AIParameter("Design effect intensity", SuggestedValues = new[] { "light", "medium", "heavy" })]
        public string DesignIntensity { get; set; } = "medium";
        
        /// <summary>
        /// Enable hover animations for the design mode
        /// </summary>
        [Parameter]
        public bool EnableDesignHover { get; set; } = true;
        
        #endregion
        
        #region Design Mode Methods
        
        /// <summary>
        /// Gets the CSS classes for the current design mode
        /// </summary>
        protected virtual string GetDesignModeClasses()
        {
            // Material is our default - no extra classes needed, existing styles already use Material Design
            if (DesignMode == DesignMode.Material)
                return string.Empty;
            
            var classes = new List<string>();
            var designName = DesignMode.ToString().ToLower();
            
            // Add base design class
            classes.Add($"design-{designName}");
            
            // Add intensity modifier
            if (!string.IsNullOrEmpty(DesignIntensity) && DesignIntensity != "medium")
            {
                classes.Add($"design-{designName}-{DesignIntensity}");
            }
            
            // Add hover modifier
            if (EnableDesignHover)
            {
                classes.Add($"design-{designName}-hover");
            }
            
            // Add variant-specific design combination
            var variantName = Variant.ToString().ToLower();
            classes.Add($"design-{designName}-{variantName}");
            
            return string.Join(" ", classes);
        }
        
        /// <summary>
        /// Determines if the current design mode requires special handling
        /// </summary>
        protected virtual bool RequiresSpecialDesignHandling()
        {
            return DesignMode is DesignMode.Glass or DesignMode.Frosted or DesignMode.Neumorphism;
        }
        
        /// <summary>
        /// Gets additional attributes required by the design mode
        /// </summary>
        protected virtual Dictionary<string, object> GetDesignModeAttributes()
        {
            var attributes = new Dictionary<string, object>();
            
            if (DesignMode == DesignMode.Shimmer)
            {
                attributes["data-shimmer"] = "true";
            }
            
            if (DesignMode == DesignMode.Material)
            {
                attributes["data-ripple"] = "true";
            }
            
            if (RequiresSpecialDesignHandling())
            {
                attributes["data-design-mode"] = DesignMode.ToString().ToLower();
            }
            
            return attributes;
        }
        
        #endregion
        
        #region Styling Override
        
        /// <summary>
        /// Override to include design mode classes
        /// </summary>
        protected override IEnumerable<string> GetBaseCssClasses()
        {
            var classes = base.GetBaseCssClasses().ToList();
            
            // Add design mode classes
            var designClasses = GetDesignModeClasses();
            if (!string.IsNullOrEmpty(designClasses))
            {
                classes.AddRange(designClasses.Split(' ').Where(c => !string.IsNullOrEmpty(c)));
            }
            
            return classes;
        }
        
        /// <summary>
        /// Override variant classes to work with design modes
        /// </summary>
        protected override string GetVariantClasses()
        {
            return GetCompleteVariantClasses();
        }
        
        #endregion
        
        #region Helper Methods
        
        /// <summary>
        /// Checks if the component should use inverted text colors
        /// </summary>
        protected bool ShouldUseInvertedText()
        {
            if (DesignMode is DesignMode.Glass)
            {
                return false;
            }
            
            // Material design uses inverted text
            if (DesignMode is DesignMode.Material)
            {
                return true;
            }
            
            return false;
        }
        
        /// <summary>
        /// Gets the composite style key for caching
        /// </summary>
        protected string GetStyleKey()
        {
            return $"{Variant}_{DesignMode}_{DesignIntensity}";
        }
        
        #endregion
    }
}