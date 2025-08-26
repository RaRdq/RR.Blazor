using RR.Blazor.Enums;
using System.Collections.Generic;

namespace RR.Blazor.Utilities
{
    /// <summary>
    /// Density helper for consistent density application across all R components
    /// </summary>
    public static class DensityHelper
    {
        private static readonly Dictionary<DensityType, double> DensityScales = new()
        {
            { DensityType.Compact, 0.5 },
            { DensityType.Dense, 0.75 },
            { DensityType.Normal, 1.0 },
            { DensityType.Spacious, 1.5 }
        };

        private static readonly Dictionary<DensityType, Dictionary<string, string>> TextSizeMap = new Dictionary<DensityType, Dictionary<string, string>>
        {
            { DensityType.Compact, new Dictionary<string, string> { ["sm"] = "xs", ["base"] = "sm", ["lg"] = "base", ["xl"] = "lg" } },
            { DensityType.Dense, new Dictionary<string, string> { ["sm"] = "sm", ["base"] = "base", ["lg"] = "lg", ["xl"] = "xl" } },
            { DensityType.Normal, new Dictionary<string, string> { ["sm"] = "sm", ["base"] = "base", ["lg"] = "lg", ["xl"] = "xl" } },
            { DensityType.Spacious, new Dictionary<string, string> { ["sm"] = "base", ["base"] = "lg", ["lg"] = "xl", ["xl"] = "2xl" } }
        };

        /// <summary>
        /// Gets the density class for any component
        /// </summary>
        public static string GetDensityClass(string componentName, DensityType density)
        {
            if (density == DensityType.Normal && !ShouldAlwaysApplyDensity(componentName))
                return string.Empty;

            return $"{componentName}-density-{density.ToString().ToLower()}";
        }

        /// <summary>
        /// Determines if a component should always apply density classes
        /// </summary>
        private static bool ShouldAlwaysApplyDensity(string componentName)
        {
            // Components that should always have explicit density classes
            var alwaysApply = new[] { "card", "table", "list", "form", "modal" };
            return Array.Exists(alwaysApply, c => c == componentName.ToLower());
        }

        /// <summary>
        /// Gets spacing value based on density
        /// </summary>
        public static int GetSpacing(DensityType density, int baseValue)
        {
            var scale = DensityScales[density];
            var calculated = baseValue * scale;
            
            if (calculated < 1) return 1;
            if (calculated < 2) return (int)Math.Round(calculated * 2) / 2; // Round to 0.5
            return (int)Math.Round(calculated);
        }

        /// <summary>
        /// Gets padding CSS variable based on density
        /// </summary>
        public static string GetPaddingVar(DensityType density, ComponentSize size = ComponentSize.Medium)
        {
            var baseValue = size switch
            {
                ComponentSize.Small => 2,
                ComponentSize.Medium => 3,
                ComponentSize.Large => 4,
                ComponentSize.ExtraLarge => 6,
                _ => 3
            };

            var spacing = GetSpacing(density, baseValue);
            return GetSpaceVar(spacing);
        }

        /// <summary>
        /// Gets gap CSS variable based on density
        /// </summary>
        public static string GetGapVar(DensityType density, ComponentSize size = ComponentSize.Medium)
        {
            var baseValue = size switch
            {
                ComponentSize.Small => 1,
                ComponentSize.Medium => 2,
                ComponentSize.Large => 3,
                _ => 2
            };

            var spacing = GetSpacing(density, baseValue);
            return GetSpaceVar(spacing);
        }

        /// <summary>
        /// Gets text size based on density
        /// </summary>
        public static string GetTextSize(DensityType density, string baseSize = "base")
        {
            if (TextSizeMap.TryGetValue(density, out var sizeMap) && 
                sizeMap.TryGetValue(baseSize, out var size))
            {
                return $"text-{size}";
            }
            return $"text-{baseSize}";
        }

        /// <summary>
        /// Gets icon size based on density
        /// </summary>
        public static string GetIconSize(DensityType density, string baseSize = "lg")
        {
            return GetTextSize(density, baseSize);
        }

        /// <summary>
        /// Gets height based on density
        /// </summary>
        public static string GetHeightVar(DensityType density, int baseHeight = 10)
        {
            var height = GetSpacing(density, baseHeight);
            return GetSpaceVar(height);
        }

        /// <summary>
        /// Converts spacing value to CSS variable
        /// </summary>
        private static string GetSpaceVar(double value)
        {
            return value switch
            {
                0.5 => "var(--space-0-5)",
                1.5 => "var(--space-1-5)",
                2.5 => "var(--space-2-5)",
                _ => $"var(--space-{(int)Math.Round(value)})"
            };
        }

        /// <summary>
        /// Gets gap CSS class for component spacing
        /// </summary>
        public static string GetGap(DensityType density)
        {
            var gapValue = GetSpacing(density, 2);
            return GetSpaceVar(gapValue);
        }

        /// <summary>
        /// Gets margin CSS variable based on density
        /// </summary>
        public static string GetMargin(DensityType density, string baseValue = "1")
        {
            var baseInt = int.TryParse(baseValue, out var parsed) ? parsed : 1;
            var marginValue = GetSpacing(density, baseInt);
            return GetSpaceVar(marginValue);
        }

        /// <summary>
        /// Gets density classes specific to input components
        /// </summary>
        public static string GetInputDensityClasses(DensityType density)
        {
            var classes = new List<string>();
            var densityClass = density.ToString().ToLower();
            
            classes.Add($"input-density-{densityClass}");
            classes.Add($"density-{densityClass}");
            
            return string.Join(" ", classes);
        }

        /// <summary>
        /// Gets density classes specific to progress components
        /// </summary>
        public static string GetProgressDensityClasses(DensityType density)
        {
            var classes = new List<string>();
            var densityClass = density.ToString().ToLower();
            
            classes.Add($"progress-density-{densityClass}");
            classes.Add($"density-{densityClass}");
            
            return string.Join(" ", classes);
        }

        /// <summary>
        /// Gets height CSS variable based on density and component type
        /// </summary>
        public static string GetProgressHeightVar(DensityType density, SizeType size = SizeType.Medium)
        {
            var baseHeight = size switch
            {
                SizeType.Small => 2,
                SizeType.Large => 4,
                SizeType.ExtraLarge => 5,
                _ => 3
            };

            var height = GetSpacing(density, baseHeight);
            return GetSpaceVar(height);
        }

        /// <summary>
        /// Gets horizontal padding CSS variable
        /// </summary>
        public static string GetHorizontalPadding(DensityType density, string baseValue = "3")
        {
            var baseInt = int.TryParse(baseValue, out var parsed) ? parsed : 3;
            var paddingValue = GetSpacing(density, baseInt);
            return GetSpaceVar(paddingValue);
        }

        /// <summary>
        /// Gets vertical padding CSS variable
        /// </summary>
        public static string GetVerticalPadding(DensityType density, string baseValue = "2")
        {
            var baseInt = int.TryParse(baseValue, out var parsed) ? parsed : 2;
            var paddingValue = GetSpacing(density, baseInt);
            return GetSpaceVar(paddingValue);
        }

        /// <summary>
        /// Builds complete density classes for a component
        /// </summary>
        public static string BuildDensityClasses(string componentName, DensityType density, params string[] additionalClasses)
        {
            var classes = new List<string>();
            
            var densityClass = GetDensityClass(componentName, density);
            if (!string.IsNullOrEmpty(densityClass))
                classes.Add(densityClass);

            classes.AddRange(additionalClasses.Where(c => !string.IsNullOrEmpty(c)));
            
            return string.Join(" ", classes);
        }
    }

    /// <summary>
    /// Component size for density calculations
    /// </summary>
    public enum ComponentSize
    {
        Small,
        Medium,
        Large,
        ExtraLarge
    }
}