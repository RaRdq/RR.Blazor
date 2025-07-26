using Microsoft.AspNetCore.Components;
using RR.Blazor.Attributes;
using RR.Blazor.Enums;

namespace RR.Blazor.Components.Base
{
    /// <summary>
    /// Base class for components that have both size and variant styling
    /// </summary>
    public abstract class RVariantComponentBase<TSize, TVariant> : RSizedComponentBase<TSize> 
        where TSize : Enum 
        where TVariant : Enum
    {
        #region Variant Properties
        
        /// <summary>
        /// Visual style variant of the component
        /// </summary>
        [Parameter] 
        [AIParameter("Component style variant", SuggestedValues = new[] { "Primary", "Secondary", "Success", "Warning", "Danger", "Info" })]
        public TVariant Variant { get; set; } = default(TVariant)!;
        
        #endregion
        
        #region Abstract Methods
        
        /// <summary>
        /// Override this method to provide component-specific variant classes
        /// </summary>
        protected abstract string GetVariantClasses();
        
        /// <summary>
        /// Override this method to provide the default variant for the component
        /// </summary>
        protected abstract TVariant GetDefaultVariant();
        
        #endregion
        
        #region Variant Calculation Methods
        
        /// <summary>
        /// Gets variant classes with theme-aware styling
        /// </summary>
        protected virtual string GetVariantClassesWithTheme()
        {
            var baseClasses = GetVariantClasses();
            return ApplyThemeToVariantClasses(baseClasses);
        }
        
        /// <summary>
        /// Applies theme-aware styling to variant classes
        /// </summary>
        protected virtual string ApplyThemeToVariantClasses(string baseClasses)
        {
            // This can be enhanced to support theme-aware color adjustments
            return baseClasses;
        }
        
        /// <summary>
        /// Gets background color classes based on variant
        /// </summary>
        protected virtual string GetVariantBackgroundClasses()
        {
            var variantName = Variant.ToString();
            
            return variantName.ToLower() switch
            {
                "primary" => "bg-primary text-primary-foreground",
                "secondary" => "bg-secondary text-secondary-foreground",
                "success" => "bg-success text-success-foreground",
                "warning" => "bg-warning text-warning-foreground",
                "danger" or "error" => "bg-error text-error-foreground",
                "info" => "bg-info text-info-foreground",
                "outline" => "border border-primary text-primary bg-transparent",
                "ghost" => "text-primary bg-transparent hover:bg-primary/10",
                "link" => "text-primary underline-offset-4 hover:underline",
                "muted" => "bg-muted text-muted-foreground",
                _ => "bg-primary text-primary-foreground"
            };
        }
        
        /// <summary>
        /// Gets border color classes based on variant
        /// </summary>
        protected virtual string GetVariantBorderClasses()
        {
            var variantName = Variant.ToString();
            
            return variantName.ToLower() switch
            {
                "primary" => "border-primary",
                "secondary" => "border-secondary",
                "success" => "border-success",
                "warning" => "border-warning",
                "danger" or "error" => "border-error",
                "info" => "border-info",
                "outline" => "border-primary",
                "muted" => "border-muted",
                _ => "border-primary"
            };
        }
        
        /// <summary>
        /// Gets text color classes based on variant
        /// </summary>
        protected virtual string GetVariantTextClasses()
        {
            var variantName = Variant.ToString();
            
            return variantName.ToLower() switch
            {
                "primary" => "text-primary",
                "secondary" => "text-secondary",
                "success" => "text-success",
                "warning" => "text-warning",
                "danger" or "error" => "text-error",
                "info" => "text-info",
                "muted" => "text-muted",
                _ => "text-primary"
            };
        }
        
        /// <summary>
        /// Gets hover state classes based on variant
        /// </summary>
        protected virtual string GetVariantHoverClasses()
        {
            var variantName = Variant.ToString();
            
            return variantName.ToLower() switch
            {
                "primary" => "hover:bg-primary/90",
                "secondary" => "hover:bg-secondary/90",
                "success" => "hover:bg-success/90",
                "warning" => "hover:bg-warning/90",
                "danger" or "error" => "hover:bg-error/90",
                "info" => "hover:bg-info/90",
                "outline" => "hover:bg-primary hover:text-primary-foreground",
                "ghost" => "hover:bg-primary/10",
                "link" => "hover:underline",
                "muted" => "hover:bg-muted/90",
                _ => "hover:bg-primary/90"
            };
        }
        
        #endregion
        
        #region Styling Methods
        
        /// <summary>
        /// Gets CSS classes with variant styling applied
        /// </summary>
        protected override IEnumerable<string> GetBaseCssClasses()
        {
            var classes = base.GetBaseCssClasses().ToList();
            
            // Add variant classes
            var variantClasses = GetVariantClassesWithTheme();
            if (!string.IsNullOrEmpty(variantClasses))
            {
                classes.AddRange(variantClasses.Split(' ').Where(c => !string.IsNullOrEmpty(c)));
            }
            
            return classes;
        }
        
        /// <summary>
        /// Gets comprehensive variant styling including background, border, text, and hover
        /// </summary>
        protected virtual string GetCompleteVariantClasses()
        {
            var classes = new List<string>();
            
            // Add background classes
            var bgClasses = GetVariantBackgroundClasses();
            if (!string.IsNullOrEmpty(bgClasses))
                classes.AddRange(bgClasses.Split(' '));
            
            // Add border classes
            var borderClasses = GetVariantBorderClasses();
            if (!string.IsNullOrEmpty(borderClasses))
                classes.AddRange(borderClasses.Split(' '));
            
            // Add hover classes
            var hoverClasses = GetVariantHoverClasses();
            if (!string.IsNullOrEmpty(hoverClasses))
                classes.AddRange(hoverClasses.Split(' '));
            
            return string.Join(" ", classes.Where(c => !string.IsNullOrEmpty(c)));
        }
        
        #endregion
        
        /// <summary>
        /// Initializes the variant property with the default value
        /// </summary>
        protected override void OnInitialized()
        {
            base.OnInitialized();
            
            // Initialize variant if not already set
            if (Variant.Equals(default(TVariant)))
            {
                Variant = GetDefaultVariant();
            }
        }
        
        #region Helper Methods
        
        /// <summary>
        /// Checks if the current variant is a destructive action
        /// </summary>
        protected bool IsDestructiveVariant()
        {
            var variantName = Variant.ToString();
            return variantName.ToLower() is "danger" or "error" or "destructive";
        }
        
        /// <summary>
        /// Checks if the current variant is a subtle/muted variant
        /// </summary>
        protected bool IsSubtleVariant()
        {
            var variantName = Variant.ToString();
            return variantName.ToLower() is "ghost" or "link" or "muted" or "subtle";
        }
        
        #endregion
    }
}