namespace RR.Blazor.Enums;

/// <summary>
/// Visual design modes that can be applied orthogonally to semantic variants
/// These control the visual treatment (glass, neumorphism, etc.) independent of semantic meaning
/// </summary>
public enum DesignMode
{
    /// <summary>
    /// Material Design with gradients (default style)
    /// </summary>
    Material = 0,
    
    /// <summary>
    /// Glassmorphism effect with blur and transparency
    /// </summary>
    Glass = 1,
    
    /// <summary>
    /// Soft UI design with subtle shadows and highlights
    /// </summary>
    Neumorphism = 2,
    
    /// <summary>
    /// Neon glow effect with bright borders
    /// </summary>
    Neon = 3,
    
    /// <summary>
    /// Shimmer animation effect
    /// </summary>
    Shimmer = 4,
    
    /// <summary>
    /// Enterprise/professional styling with subtle gradients
    /// </summary>
    Enterprise = 5,
    
    /// <summary>
    /// Elevated design with prominent shadows
    /// </summary>
    Elevated = 6,
    
    /// <summary>
    /// Flat design with no depth or shadows
    /// </summary>
    Flat = 7,
    
    /// <summary>
    /// Frosted glass effect with heavy blur
    /// </summary>
    Frosted = 8
}