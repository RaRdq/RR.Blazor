using Microsoft.AspNetCore.Components;
using RR.Blazor.Attributes;
using RR.Blazor.Enums;
using RR.Blazor.Utilities;

namespace RR.Blazor.Components.Base
{
    /// <summary>
    /// Base class for components that have size variants (Small, Medium, Large)
    /// </summary>
    public abstract class RSizedComponentBase<TSize> : RTextComponentBase where TSize : Enum
    {
        #region Size Properties
        
        /// <summary>
        /// Size variant of the component
        /// </summary>
        [Parameter] 
        [AIParameter("Component size variant", SuggestedValues = new[] { "Small", "Medium", "Large" })]
        public TSize Size { get; set; }
        
        #endregion
        
        #region Abstract Methods
        
        /// <summary>
        /// Override this method to provide component-specific size classes
        /// </summary>
        protected abstract string GetSizeClasses();
        
        /// <summary>
        /// Override this method to provide the default size for the component
        /// </summary>
        protected abstract TSize GetDefaultSize();
        
        #endregion
        
        #region Size Calculation Methods
        
        /// <summary>
        /// Gets text size classes based on component size and density
        /// </summary>
        protected override string GetTextSizeClasses()
        {
            return SizeHelper.GetTextSize(Size, Density);
        }
        
        /// <summary>
        /// Gets icon size classes based on component size and density
        /// </summary>
        protected override string GetIconSizeClasses()
        {
            return SizeHelper.GetIconSize(Size, Density);
        }
        
        /// <summary>
        /// Gets size classes with density adjustments applied
        /// </summary>
        protected virtual string GetSizeClassesWithDensity()
        {
            var baseClasses = GetSizeClasses();
            return ApplyDensityToSizeClasses(baseClasses);
        }
        
        /// <summary>
        /// Applies density adjustments to size classes
        /// </summary>
        protected virtual string ApplyDensityToSizeClasses(string baseClasses)
        {
            var classes = baseClasses.Split(' ');
            var adjustedClasses = new List<string>();
            
            foreach (var cls in classes)
            {
                if (cls.StartsWith("px-") || cls.StartsWith("py-") || cls.StartsWith("p-"))
                {
                    adjustedClasses.Add(AdjustSpacingForDensity(cls));
                }
                else if (cls.StartsWith("w-") || cls.StartsWith("h-"))
                {
                    adjustedClasses.Add(AdjustSizeForDensity(cls));
                }
                else
                {
                    adjustedClasses.Add(cls);
                }
            }
            
            return string.Join(" ", adjustedClasses);
        }
        
        /// <summary>
        /// Adjusts spacing classes based on density
        /// </summary>
        protected virtual string AdjustSpacingForDensity(string spacingClass)
        {
            var parts = spacingClass.Split('-');
            if (parts.Length >= 2 && double.TryParse(parts[1], out var value))
            {
                var adjustment = Density switch
                {
                    ComponentDensity.Compact => -0.5,
                    ComponentDensity.Dense => -0.25,
                    ComponentDensity.Normal => 0,
                    ComponentDensity.Spacious => 0.5,
                    _ => 0
                };
                
                var adjustedValue = Math.Max(0, value + adjustment);
                var adjustedValueStr = adjustedValue % 1 == 0 ? ((int)adjustedValue).ToString() : adjustedValue.ToString();
                
                return $"{parts[0]}-{adjustedValueStr}";
            }
            
            return spacingClass;
        }
        
        /// <summary>
        /// Adjusts width/height classes based on density
        /// </summary>
        protected virtual string AdjustSizeForDensity(string sizeClass)
        {
            var parts = sizeClass.Split('-');
            if (parts.Length >= 2 && double.TryParse(parts[1], out var value))
            {
                var adjustment = Density switch
                {
                    ComponentDensity.Compact => -1,
                    ComponentDensity.Dense => -0.5,
                    ComponentDensity.Normal => 0,
                    ComponentDensity.Spacious => 1,
                    _ => 0
                };
                
                var adjustedValue = Math.Max(1, value + adjustment);
                var adjustedValueStr = adjustedValue % 1 == 0 ? ((int)adjustedValue).ToString() : adjustedValue.ToString();
                
                return $"{parts[0]}-{adjustedValueStr}";
            }
            
            return sizeClass;
        }
        
        #endregion
        
        #region Styling Methods
        
        /// <summary>
        /// Gets CSS classes with size and density applied
        /// </summary>
        protected override IEnumerable<string> GetBaseCssClasses()
        {
            var classes = base.GetBaseCssClasses().ToList();
            
            // Add size classes
            var sizeClasses = GetSizeClassesWithDensity();
            if (!string.IsNullOrEmpty(sizeClasses))
            {
                classes.AddRange(sizeClasses.Split(' ').Where(c => !string.IsNullOrEmpty(c)));
            }
            
            return classes;
        }
        
        #endregion
        
        #region Helper Methods
        
        /// <summary>
        /// Initializes the size property with the default value
        /// </summary>
        protected override void OnInitialized()
        {
            base.OnInitialized();
            
            // Initialize size if not already set
            if (Size.Equals(default(TSize)))
            {
                Size = GetDefaultSize();
            }
        }
        
        #endregion
    }
}