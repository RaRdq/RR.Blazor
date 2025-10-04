namespace RR.Blazor.Enums
{
    /// <summary>
    /// Visual presentation mode for buttons - how the button appears structurally.
    /// Controls the fundamental visual treatment (filled, outline, or minimal).
    /// </summary>
    public enum ButtonMode
    {
        /// <summary>Solid filled background - default appearance</summary>
        Filled = 0,
        
        /// <summary>Transparent background with colored border</summary>
        Outline,
        
        /// <summary>Minimal appearance with subtle background</summary>
        Ghost
    }
}