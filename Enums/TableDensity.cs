namespace RR.Blazor.Enums
{
    /// <summary>
    /// Enhanced table density options for professional table styling
    /// Provides granular control over spacing, typography, and layout density
    /// </summary>
    public enum TableDensity
    {
        /// <summary>Ultra-compact (32px rows) - Maximum data density for power users</summary>
        UltraCompact,
        
        /// <summary>Compact (36px rows) - High density for data-heavy applications</summary>
        Compact,
        
        /// <summary>Dense (42px rows) - Above normal density with good readability</summary>
        Dense,
        
        /// <summary>Normal (48px rows) - Standard comfortable spacing</summary>
        Normal,
        
        /// <summary>Comfortable (52px rows) - Relaxed spacing for better readability</summary>
        Comfortable,
        
        /// <summary>Spacious (56px rows) - Touch-friendly interfaces</summary>
        Spacious,
        
        /// <summary>Extra spacious (64px rows) - Maximum comfort and accessibility</summary>
        ExtraSpacious,
        
        /// <summary>Custom height - User-defined row heights</summary>
        Custom
    }
}