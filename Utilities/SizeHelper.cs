using RR.Blazor.Enums;
using System.Linq;

namespace RR.Blazor.Utilities
{
    /// <summary>
    /// Utility class for size-based calculations and CSS class generation
    /// </summary>
    public static class SizeHelper
    {
        #region Generic Size Utilities
        
        /// <summary>
        /// Gets text size classes based on generic size enum and density
        /// </summary>
        public static string GetTextSize<TSize>(TSize size, DensityType density) where TSize : Enum
        {
            var sizeMap = GetSizeMap<TSize>();
            var baseSize = sizeMap.GetValueOrDefault(size.ToString(), "text-base");
            
            return density switch
            {
                DensityType.Compact => AdjustTextSize(baseSize, -1),
                DensityType.Dense => AdjustTextSize(baseSize, -0.5),
                DensityType.Normal => baseSize,
                DensityType.Spacious => AdjustTextSize(baseSize, 1),
                _ => baseSize
            };
        }
        
        /// <summary>
        /// Gets icon size classes based on generic size enum and density
        /// </summary>
        public static string GetIconSize<TSize>(TSize size, DensityType density) where TSize : Enum
        {
            var sizeMap = GetIconSizeMap<TSize>();
            var baseSize = sizeMap.GetValueOrDefault(size.ToString(), "text-lg");
            
            return density switch
            {
                DensityType.Compact => AdjustIconSize(baseSize, -1),
                DensityType.Dense => AdjustIconSize(baseSize, -0.5),
                DensityType.Normal => baseSize,
                DensityType.Spacious => AdjustIconSize(baseSize, 1),
                _ => baseSize
            };
        }
        
        #endregion
        
        #region Button Size Utilities
        
        /// <summary>
        /// Gets button size classes based on SizeType enum and density
        /// </summary>
        public static string GetButtonSize(SizeType size, DensityType density)
        {
            var sizeClass = size switch
            {
                SizeType.ExtraSmall => "button-xs",
                SizeType.Small => "button-sm",
                SizeType.Medium => "", // Default size
                SizeType.Large => "button-lg",
                SizeType.ExtraLarge => "button-xl",
                _ => ""
            };
            
            // Apply density modifiers
            var densityClass = density switch
            {
                DensityType.Compact => "button-compact",
                DensityType.Dense => "button-dense",
                DensityType.Normal => "",
                DensityType.Spacious => "button-spacious",
                _ => ""
            };
            
            return string.Join(" ", new[] { sizeClass, densityClass }.Where(c => !string.IsNullOrEmpty(c)));
        }
        
        /// <summary>
        /// Gets button icon size classes
        /// </summary>
        public static string GetButtonIconSize(SizeType size, DensityType density)
        {
            var baseSize = size switch
            {
                SizeType.ExtraSmall => "text-sm",
                SizeType.Small => "text-base",
                SizeType.Medium => "text-lg",
                SizeType.Large => "text-xl",
                SizeType.ExtraLarge => "text-2xl",
                _ => "text-lg"
            };
            
            return ApplyDensityToTextSize(baseSize, density);
        }
        
        #endregion
        
        #region Badge Size Utilities
        
        /// <summary>
        /// Gets badge size classes based on SizeType enum and density
        /// </summary>
        public static string GetBadgeSize(SizeType size, DensityType density)
        {
            var sizeClass = size switch
            {
                SizeType.ExtraSmall => "badge-xs",
                SizeType.Small => "badge-sm",
                SizeType.Medium => "", // Default badge size
                SizeType.Large => "badge-lg",
                SizeType.ExtraLarge => "badge-xl",
                _ => ""
            };
            
            var densityClass = density switch
            {
                DensityType.Compact => "badge-compact",
                DensityType.Dense => "badge-dense",
                DensityType.Normal => "",
                DensityType.Spacious => "badge-spacious",
                _ => ""
            };
            
            return string.Join(" ", new[] { sizeClass, densityClass }.Where(c => !string.IsNullOrEmpty(c)));
        }
        
        #endregion
        
        #region Chip Size Utilities
        
        /// <summary>
        /// Gets chip size classes based on SizeType enum and density
        /// </summary>
        public static string GetChipSize(SizeType size, DensityType density)
        {
            var baseClasses = size switch
            {
                SizeType.Small => "px-2 py-1 text-xs",
                SizeType.Medium => "px-3 py-1.5 text-sm",
                SizeType.Large => "px-4 py-2 text-base",
                _ => "px-3 py-1.5 text-sm"
            };
            
            return ApplyDensityToClasses(baseClasses, density);
        }
        
        #endregion
        
        #region Avatar Size Utilities
        
        /// <summary>
        /// Gets avatar size classes based on SizeType enum and density
        /// </summary>
        public static string GetAvatarSize(SizeType size, DensityType density)
        {
            var baseClasses = size switch
            {
                SizeType.Small => "w-8 h-8 text-sm",
                SizeType.Medium => "w-10 h-10 text-base",
                SizeType.Large => "w-12 h-12 text-lg",
                SizeType.ExtraLarge => "w-16 h-16 text-xl",
                _ => "w-10 h-10 text-base"
            };
            
            return ApplyDensityToClasses(baseClasses, density);
        }
        
        #endregion
        
        #region Helper Methods
        
        /// <summary>
        /// Gets size mappings for different enums
        /// </summary>
        private static Dictionary<string, string> GetSizeMap<TSize>() where TSize : Enum
        {
            return typeof(TSize).Name switch
            {
                "SizeType" => new Dictionary<string, string>
                {
                    { "ExtraSmall", "text-xs" },
                    { "Small", "text-sm" },
                    { "Medium", "text-base" },
                    { "Large", "text-lg" },
                    { "ExtraLarge", "text-xl" }
                },
                _ => new Dictionary<string, string>
                {
                    { "Small", "text-sm" },
                    { "Medium", "text-base" },
                    { "Large", "text-lg" }
                }
            };
        }
        
        /// <summary>
        /// Gets icon size mappings for different enums
        /// </summary>
        private static Dictionary<string, string> GetIconSizeMap<TSize>() where TSize : Enum
        {
            return typeof(TSize).Name switch
            {
                "SizeType" => new Dictionary<string, string>
                {
                    { "ExtraSmall", "text-sm" },
                    { "Small", "text-base" },
                    { "Medium", "text-lg" },
                    { "Large", "text-xl" },
                    { "ExtraLarge", "text-2xl" }
                },
                _ => new Dictionary<string, string>
                {
                    { "Small", "text-base" },
                    { "Medium", "text-lg" },
                    { "Large", "text-xl" }
                }
            };
        }
        
        /// <summary>
        /// Adjusts text size based on density offset
        /// </summary>
        private static string AdjustTextSize(string baseSize, double offset)
        {
            var sizeOrder = new[] { "text-xs", "text-sm", "text-base", "text-lg", "text-xl", "text-2xl" };
            var currentIndex = Array.IndexOf(sizeOrder, baseSize);
            
            if (currentIndex >= 0)
            {
                var newIndex = Math.Max(0, Math.Min(sizeOrder.Length - 1, currentIndex + (int)offset));
                return sizeOrder[newIndex];
            }
            
            return baseSize;
        }
        
        /// <summary>
        /// Adjusts icon size based on density offset
        /// </summary>
        private static string AdjustIconSize(string baseSize, double offset)
        {
            return AdjustTextSize(baseSize, offset);
        }
        
        /// <summary>
        /// Applies density adjustments to a set of CSS classes
        /// </summary>
        private static string ApplyDensityToClasses(string baseClasses, DensityType density)
        {
            var classes = baseClasses.Split(' ');
            var adjustedClasses = new List<string>();
            
            foreach (var cls in classes)
            {
                if (cls.StartsWith("px-") || cls.StartsWith("py-") || cls.StartsWith("p-"))
                {
                    adjustedClasses.Add(AdjustSpacingClass(cls, density));
                }
                else if (cls.StartsWith("text-"))
                {
                    adjustedClasses.Add(ApplyDensityToTextSize(cls, density));
                }
                else
                {
                    adjustedClasses.Add(cls);
                }
            }
            
            return string.Join(" ", adjustedClasses);
        }
        
        /// <summary>
        /// Applies density to text size classes
        /// </summary>
        private static string ApplyDensityToTextSize(string textSize, DensityType density)
        {
            return density switch
            {
                DensityType.Compact => AdjustTextSize(textSize, -1),
                DensityType.Dense => AdjustTextSize(textSize, -0.5),
                DensityType.Normal => textSize,
                DensityType.Spacious => AdjustTextSize(textSize, 1),
                _ => textSize
            };
        }
        
        /// <summary>
        /// Adjusts spacing classes based on density
        /// </summary>
        private static string AdjustSpacingClass(string spacingClass, DensityType density)
        {
            var parts = spacingClass.Split('-');
            if (parts.Length >= 2 && double.TryParse(parts[1], out var value))
            {
                var adjustment = density switch
                {
                    DensityType.Compact => -0.5,
                    DensityType.Dense => -0.25,
                    DensityType.Normal => 0,
                    DensityType.Spacious => 0.5,
                    _ => 0
                };
                
                var adjustedValue = Math.Max(0, value + adjustment);
                var adjustedValueStr = adjustedValue % 1 == 0 ? ((int)adjustedValue).ToString() : adjustedValue.ToString();
                
                return $"{parts[0]}-{adjustedValueStr}";
            }
            
            return spacingClass;
        }
        
        #endregion
    }
}