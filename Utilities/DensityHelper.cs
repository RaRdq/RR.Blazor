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
        public static string GetPadding(DensityType density, string baseSize = "3")
        {
            return density switch
            {
                DensityType.Compact => $"p-{AdjustSize(baseSize, -1)}",
                DensityType.Dense => $"p-{AdjustSize(baseSize, -0.5)}",
                DensityType.Normal => $"p-{baseSize}",
                DensityType.Spacious => $"p-{AdjustSize(baseSize, 1)}",
                _ => $"p-{baseSize}"
            };
        }
        
        /// <summary>
        /// Gets horizontal padding classes based on density
        /// </summary>
        public static string GetHorizontalPadding(DensityType density, string baseSize = "4")
        {
            return density switch
            {
                DensityType.Compact => $"px-{AdjustSize(baseSize, -1)}",
                DensityType.Dense => $"px-{AdjustSize(baseSize, -0.5)}",
                DensityType.Normal => $"px-{baseSize}",
                DensityType.Spacious => $"px-{AdjustSize(baseSize, 1)}",
                _ => $"px-{baseSize}"
            };
        }
        
        /// <summary>
        /// Gets vertical padding classes based on density
        /// </summary>
        public static string GetVerticalPadding(DensityType density, string baseSize = "3")
        {
            return density switch
            {
                DensityType.Compact => $"py-{AdjustSize(baseSize, -1)}",
                DensityType.Dense => $"py-{AdjustSize(baseSize, -0.5)}",
                DensityType.Normal => $"py-{baseSize}",
                DensityType.Spacious => $"py-{AdjustSize(baseSize, 1)}",
                _ => $"py-{baseSize}"
            };
        }
        
        #endregion
        
        #region Gap Utilities
        
        /// <summary>
        /// Gets gap classes based on density
        /// </summary>
        public static string GetGap(DensityType density)
        {
            return density switch
            {
                DensityType.Compact => "gap-1",
                DensityType.Dense => "gap-2",
                DensityType.Normal => "gap-3",
                DensityType.Spacious => "gap-4",
                _ => "gap-3"
            };
        }
        
        /// <summary>
        /// Gets horizontal gap classes based on density
        /// </summary>
        public static string GetHorizontalGap(DensityType density)
        {
            return density switch
            {
                DensityType.Compact => "gap-x-1",
                DensityType.Dense => "gap-x-2",
                DensityType.Normal => "gap-x-3",
                DensityType.Spacious => "gap-x-4",
                _ => "gap-x-3"
            };
        }
        
        /// <summary>
        /// Gets vertical gap classes based on density
        /// </summary>
        public static string GetVerticalGap(DensityType density)
        {
            return density switch
            {
                DensityType.Compact => "gap-y-1",
                DensityType.Dense => "gap-y-2",
                DensityType.Normal => "gap-y-3",
                DensityType.Spacious => "gap-y-4",
                _ => "gap-y-3"
            };
        }
        
        #endregion
        
        #region Spacing Utilities
        
        /// <summary>
        /// Gets margin classes based on density
        /// </summary>
        public static string GetMargin(DensityType density, string baseSize = "2")
        {
            return density switch
            {
                DensityType.Compact => $"m-{AdjustSize(baseSize, -1)}",
                DensityType.Dense => $"m-{AdjustSize(baseSize, -0.5)}",
                DensityType.Normal => $"m-{baseSize}",
                DensityType.Spacious => $"m-{AdjustSize(baseSize, 1)}",
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
        public static string GetButtonDensityClasses(DensityType density)
        {
            return density switch
            {
                DensityType.Compact => "px-2 py-1",
                DensityType.Dense => "px-3 py-1.5",
                DensityType.Normal => "px-4 py-2",
                DensityType.Spacious => "px-6 py-3",
                _ => "px-4 py-2"
            };
        }
        
        /// <summary>
        /// Gets input-specific density classes
        /// </summary>
        public static string GetInputDensityClasses(DensityType density)
        {
            return density switch
            {
                DensityType.Compact => "px-2 py-1 text-sm",
                DensityType.Dense => "px-2 py-1.5 text-sm",
                DensityType.Normal => "px-2 py-2 text-base",
                DensityType.Spacious => "px-4 py-3 text-base",
                _ => "px-2 py-2 text-base"
            };
        }
        
        /// <summary>
        /// Gets card-specific density classes
        /// </summary>
        public static string GetCardDensityClasses(DensityType density)
        {
            return density switch
            {
                DensityType.Compact => "p-3",
                DensityType.Dense => "p-4",
                DensityType.Normal => "p-6",
                DensityType.Spacious => "p-8",
                _ => "p-6"
            };
        }
        
        #endregion
    }
}