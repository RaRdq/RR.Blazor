namespace RR.Blazor.Enums
{
    /// <summary>
    /// Defines density levels for components to support ultra-dense layouts
    /// </summary>
    public enum ComponentDensity
    {
        /// <summary>Ultra-compact for mobile and dense interfaces</summary>
        Compact,
        
        /// <summary>Dense layout for data-heavy interfaces</summary>
        Dense,
        
        /// <summary>Standard spacing for general use</summary>
        Normal,
        
        /// <summary>Spacious layout for accessibility and comfort</summary>
        Spacious
    }
}