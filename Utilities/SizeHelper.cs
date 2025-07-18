using RR.Blazor.Enums;

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
        public static string GetTextSize<TSize>(TSize size, ComponentDensity density) where TSize : Enum
        {
            var sizeMap = GetSizeMap<TSize>();
            var baseSize = sizeMap.GetValueOrDefault(size.ToString(), "text-base");
            
            return density switch
            {
                ComponentDensity.Compact => AdjustTextSize(baseSize, -1),
                ComponentDensity.Dense => AdjustTextSize(baseSize, -0.5),
                ComponentDensity.Normal => baseSize,
                ComponentDensity.Spacious => AdjustTextSize(baseSize, 1),
                _ => baseSize
            };
        }
        
        /// <summary>
        /// Gets icon size classes based on generic size enum and density
        /// </summary>
        public static string GetIconSize<TSize>(TSize size, ComponentDensity density) where TSize : Enum
        {
            var sizeMap = GetIconSizeMap<TSize>();
            var baseSize = sizeMap.GetValueOrDefault(size.ToString(), "text-lg");
            
            return density switch
            {
                ComponentDensity.Compact => AdjustIconSize(baseSize, -1),
                ComponentDensity.Dense => AdjustIconSize(baseSize, -0.5),
                ComponentDensity.Normal => baseSize,
                ComponentDensity.Spacious => AdjustIconSize(baseSize, 1),
                _ => baseSize
            };
        }
        
        #endregion
        
        #region Button Size Utilities
        
        /// <summary>
        /// Gets button size classes based on ButtonSize enum and density
        /// </summary>
        public static string GetButtonSize(ButtonSize size, ComponentDensity density)
        {
            var baseClasses = size switch
            {
                ButtonSize.Small => "px-3 py-1.5 text-sm",
                ButtonSize.Medium => "px-4 py-2 text-base",
                ButtonSize.Large => "px-6 py-3 text-lg",
                _ => "px-4 py-2 text-base"
            };
            
            return ApplyDensityToClasses(baseClasses, density);
        }
        
        /// <summary>
        /// Gets button icon size classes
        /// </summary>
        public static string GetButtonIconSize(ButtonSize size, ComponentDensity density)
        {
            var baseSize = size switch
            {
                ButtonSize.Small => "text-sm",
                ButtonSize.Medium => "text-base",
                ButtonSize.Large => "text-lg",
                _ => "text-base"
            };
            
            return ApplyDensityToTextSize(baseSize, density);
        }
        
        #endregion
        
        #region Badge Size Utilities
        
        /// <summary>
        /// Gets badge size classes based on BadgeSize enum and density
        /// </summary>
        public static string GetBadgeSize(BadgeSize size, ComponentDensity density)
        {
            var baseClasses = size switch
            {
                BadgeSize.Small => "px-2 py-0.5 text-xs",
                BadgeSize.Medium => "px-2.5 py-1 text-sm",
                BadgeSize.Large => "px-3 py-1.5 text-base",
                _ => "px-2.5 py-1 text-sm"
            };
            
            return ApplyDensityToClasses(baseClasses, density);
        }
        
        #endregion
        
        #region Chip Size Utilities
        
        /// <summary>
        /// Gets chip size classes based on ChipSize enum and density
        /// </summary>
        public static string GetChipSize(ChipSize size, ComponentDensity density)
        {
            var baseClasses = size switch
            {
                ChipSize.Small => "px-2 py-1 text-xs",
                ChipSize.Medium => "px-3 py-1.5 text-sm",
                ChipSize.Large => "px-4 py-2 text-base",
                _ => "px-3 py-1.5 text-sm"
            };
            
            return ApplyDensityToClasses(baseClasses, density);
        }
        
        #endregion
        
        #region Avatar Size Utilities
        
        /// <summary>
        /// Gets avatar size classes based on AvatarSize enum and density
        /// </summary>
        public static string GetAvatarSize(AvatarSize size, ComponentDensity density)
        {
            var baseClasses = size switch
            {
                AvatarSize.Small => "w-8 h-8 text-sm",
                AvatarSize.Medium => "w-10 h-10 text-base",
                AvatarSize.Large => "w-12 h-12 text-lg",
                AvatarSize.ExtraLarge => "w-16 h-16 text-xl",
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
                "ButtonSize" => new Dictionary<string, string>
                {
                    { "Small", "text-sm" },
                    { "Medium", "text-base" },
                    { "Large", "text-lg" }
                },
                "BadgeSize" => new Dictionary<string, string>
                {
                    { "Small", "text-xs" },
                    { "Medium", "text-sm" },
                    { "Large", "text-base" }
                },
                "ChipSize" => new Dictionary<string, string>
                {
                    { "Small", "text-xs" },
                    { "Medium", "text-sm" },
                    { "Large", "text-base" }
                },
                "TextInputSize" => new Dictionary<string, string>
                {
                    { "Small", "text-sm" },
                    { "Medium", "text-base" },
                    { "Large", "text-lg" }
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
                "ButtonSize" => new Dictionary<string, string>
                {
                    { "Small", "text-base" },
                    { "Medium", "text-lg" },
                    { "Large", "text-xl" }
                },
                "BadgeSize" => new Dictionary<string, string>
                {
                    { "Small", "text-sm" },
                    { "Medium", "text-base" },
                    { "Large", "text-lg" }
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
        private static string ApplyDensityToClasses(string baseClasses, ComponentDensity density)
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
        private static string ApplyDensityToTextSize(string textSize, ComponentDensity density)
        {
            return density switch
            {
                ComponentDensity.Compact => AdjustTextSize(textSize, -1),
                ComponentDensity.Dense => AdjustTextSize(textSize, -0.5),
                ComponentDensity.Normal => textSize,
                ComponentDensity.Spacious => AdjustTextSize(textSize, 1),
                _ => textSize
            };
        }
        
        /// <summary>
        /// Adjusts spacing classes based on density
        /// </summary>
        private static string AdjustSpacingClass(string spacingClass, ComponentDensity density)
        {
            var parts = spacingClass.Split('-');
            if (parts.Length >= 2 && double.TryParse(parts[1], out var value))
            {
                var adjustment = density switch
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
        
        #endregion
    }
}