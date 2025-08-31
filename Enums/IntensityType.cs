namespace RR.Blazor.Enums
{
    /// <summary>
    /// Visual intensity/prominence levels for components.
    /// Controls how prominent or attention-grabbing a component appears.
    /// </summary>
    public enum IntensityType
    {
        /// <summary>Standard visual prominence - default appearance</summary>
        Normal = 0,
        
        /// <summary>Reduced visual prominence - less attention-grabbing</summary>
        Muted,
        
        /// <summary>Increased visual prominence - bold, attention-grabbing</summary>
        Bold,
        
        /// <summary>Very subtle appearance - minimal visual presence</summary>
        Subtle
    }
}