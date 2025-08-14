namespace RR.Blazor.Enums
{
    /// <summary>
    /// Unified density type for all components - controls spacing and padding
    /// </summary>
    public enum DensityType
    {
        Normal = 0,     // Default density - standard spacing
        Compact = 1,    // Ultra-compact for mobile and dense interfaces
        Dense = 2,      // Dense layout for data-heavy interfaces
        Spacious = 3    // Spacious layout for accessibility and comfort
    }
}