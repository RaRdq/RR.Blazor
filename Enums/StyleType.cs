namespace RR.Blazor.Enums
{
    /// <summary>
    /// Visual style/aesthetic for components - the overall design language.
    /// Controls the aesthetic treatment and visual effects applied to components.
    /// </summary>
    public enum StyleType
    {
        /// <summary>Standard Material Design appearance - default</summary>
        Material = 0,
        
        /// <summary>Glassmorphism with blur and transparency effects</summary>
        Glass,
        
        /// <summary>Neumorphism with soft shadows and highlights</summary>
        Neumorphism,
        
        /// <summary>Neon glow effects with bright borders</summary>
        Neon,
        
        /// <summary>Elevated design with prominent shadows</summary>
        Elevated,
        
        /// <summary>Flat design with no depth or shadows</summary>
        Flat,
        
        /// <summary>Frosted glass effect with heavy blur</summary>
        Frosted
    }
}