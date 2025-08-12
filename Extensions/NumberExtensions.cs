using System;

namespace RR.Blazor.Extensions
{
    /// <summary>
    /// Number formatting extensions for human-readable display in charts
    /// </summary>
    public static class NumberExtensions
    {
        /// <summary>
        /// Formats a number with K/M/B suffixes for human readability
        /// Examples: 1500 -> 1.5K, 2500000 -> 2.5M, 1200000000 -> 1.2B
        /// </summary>
        public static string ToHumanReadable(this double value, int decimals = 1)
        {
            if (Math.Abs(value) < 1000)
                return value.ToString("N0");

            var absValue = Math.Abs(value);
            var sign = value < 0 ? "-" : "";

            if (absValue >= 1_000_000_000)
                return $"{sign}{(value / 1_000_000_000).ToString($"N{decimals}")}B";
            
            if (absValue >= 1_000_000)
                return $"{sign}{(value / 1_000_000).ToString($"N{decimals}")}M";
            
            return $"{sign}{(value / 1_000).ToString($"N{decimals}")}K";
        }

        /// <summary>
        /// Formats a decimal with K/M/B suffixes for human readability
        /// </summary>
        public static string ToHumanReadable(this decimal value, int decimals = 1)
            => ToHumanReadable((double)value, decimals);

        /// <summary>
        /// Formats an integer with K/M/B suffixes for human readability
        /// </summary>
        public static string ToHumanReadable(this int value, int decimals = 1)
            => ToHumanReadable((double)value, decimals);

        /// <summary>
        /// Formats a long with K/M/B suffixes for human readability
        /// </summary>
        public static string ToHumanReadable(this long value, int decimals = 1)
            => ToHumanReadable((double)value, decimals);

        /// <summary>
        /// Formats a float with K/M/B suffixes for human readability
        /// </summary>
        public static string ToHumanReadable(this float value, int decimals = 1)
            => ToHumanReadable((double)value, decimals);
    }
}