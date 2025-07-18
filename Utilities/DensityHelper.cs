using RR.Blazor.Enums;

namespace RR.Blazor.Utilities
{
    /// <summary>
    /// Utility class for density-based calculations and CSS class generation
    /// </summary>
    public static class DensityHelper
    {
        #region Padding Utilities
        
        /// <summary>
        /// Gets padding classes based on density and base size
        /// </summary>
        public static string GetPadding(ComponentDensity density, string baseSize = "3")
        {
            return density switch
            {
                ComponentDensity.Compact => $"p-{AdjustSize(baseSize, -1)}",
                ComponentDensity.Dense => $"p-{AdjustSize(baseSize, -0.5)}",
                ComponentDensity.Normal => $"p-{baseSize}",
                ComponentDensity.Spacious => $"p-{AdjustSize(baseSize, 1)}",
                _ => $"p-{baseSize}"
            };
        }
        
        /// <summary>
        /// Gets horizontal padding classes based on density
        /// </summary>
        public static string GetHorizontalPadding(ComponentDensity density, string baseSize = "4")
        {
            return density switch
            {
                ComponentDensity.Compact => $"px-{AdjustSize(baseSize, -1)}",
                ComponentDensity.Dense => $"px-{AdjustSize(baseSize, -0.5)}",
                ComponentDensity.Normal => $"px-{baseSize}",
                ComponentDensity.Spacious => $"px-{AdjustSize(baseSize, 1)}",
                _ => $"px-{baseSize}"
            };
        }
        
        /// <summary>
        /// Gets vertical padding classes based on density
        /// </summary>
        public static string GetVerticalPadding(ComponentDensity density, string baseSize = "3")
        {
            return density switch
            {
                ComponentDensity.Compact => $"py-{AdjustSize(baseSize, -1)}",
                ComponentDensity.Dense => $"py-{AdjustSize(baseSize, -0.5)}",
                ComponentDensity.Normal => $"py-{baseSize}",
                ComponentDensity.Spacious => $"py-{AdjustSize(baseSize, 1)}",
                _ => $"py-{baseSize}"
            };
        }
        
        #endregion
        
        #region Gap Utilities
        
        /// <summary>
        /// Gets gap classes based on density
        /// </summary>
        public static string GetGap(ComponentDensity density)
        {
            return density switch
            {
                ComponentDensity.Compact => "gap-1",
                ComponentDensity.Dense => "gap-2",
                ComponentDensity.Normal => "gap-3",
                ComponentDensity.Spacious => "gap-4",
                _ => "gap-3"
            };
        }
        
        /// <summary>
        /// Gets horizontal gap classes based on density
        /// </summary>
        public static string GetHorizontalGap(ComponentDensity density)
        {
            return density switch
            {
                ComponentDensity.Compact => "gap-x-1",
                ComponentDensity.Dense => "gap-x-2",
                ComponentDensity.Normal => "gap-x-3",
                ComponentDensity.Spacious => "gap-x-4",
                _ => "gap-x-3"
            };
        }
        
        /// <summary>
        /// Gets vertical gap classes based on density
        /// </summary>
        public static string GetVerticalGap(ComponentDensity density)
        {
            return density switch
            {
                ComponentDensity.Compact => "gap-y-1",
                ComponentDensity.Dense => "gap-y-2",
                ComponentDensity.Normal => "gap-y-3",
                ComponentDensity.Spacious => "gap-y-4",
                _ => "gap-y-3"
            };
        }
        
        #endregion
        
        #region Spacing Utilities
        
        /// <summary>
        /// Gets margin classes based on density
        /// </summary>
        public static string GetMargin(ComponentDensity density, string baseSize = "2")
        {
            return density switch
            {
                ComponentDensity.Compact => $"m-{AdjustSize(baseSize, -1)}",
                ComponentDensity.Dense => $"m-{AdjustSize(baseSize, -0.5)}",
                ComponentDensity.Normal => $"m-{baseSize}",
                ComponentDensity.Spacious => $"m-{AdjustSize(baseSize, 1)}",
                _ => $"m-{baseSize}"
            };
        }
        
        #endregion
        
        #region Size Adjustments
        
        /// <summary>
        /// Adjusts a size value based on density offset
        /// </summary>
        public static string AdjustSize(string baseSize, double offset)
        {
            if (int.TryParse(baseSize, out var size))
            {
                var adjustedSize = Math.Max(0, size + (int)offset);
                return adjustedSize.ToString();
            }
            
            // Handle fractional sizes like "1.5", "2.5", etc.
            if (double.TryParse(baseSize, out var fractionalSize))
            {
                var adjustedSize = Math.Max(0, fractionalSize + offset);
                return adjustedSize % 1 == 0 ? ((int)adjustedSize).ToString() : adjustedSize.ToString();
            }
            
            return baseSize;
        }
        
        #endregion
        
        #region Component-Specific Utilities
        
        /// <summary>
        /// Gets button-specific density classes
        /// </summary>
        public static string GetButtonDensityClasses(ComponentDensity density)
        {
            return density switch
            {
                ComponentDensity.Compact => "px-2 py-1",
                ComponentDensity.Dense => "px-3 py-1.5",
                ComponentDensity.Normal => "px-4 py-2",
                ComponentDensity.Spacious => "px-6 py-3",
                _ => "px-4 py-2"
            };
        }
        
        /// <summary>
        /// Gets input-specific density classes
        /// </summary>
        public static string GetInputDensityClasses(ComponentDensity density)
        {
            return density switch
            {
                ComponentDensity.Compact => "px-2 py-1 text-sm",
                ComponentDensity.Dense => "px-3 py-1.5 text-sm",
                ComponentDensity.Normal => "px-3 py-2 text-base",
                ComponentDensity.Spacious => "px-4 py-3 text-base",
                _ => "px-3 py-2 text-base"
            };
        }
        
        /// <summary>
        /// Gets card-specific density classes
        /// </summary>
        public static string GetCardDensityClasses(ComponentDensity density)
        {
            return density switch
            {
                ComponentDensity.Compact => "p-3",
                ComponentDensity.Dense => "p-4",
                ComponentDensity.Normal => "p-6",
                ComponentDensity.Spacious => "p-8",
                _ => "p-6"
            };
        }
        
        #endregion
    }
}