namespace RR.Blazor.Enums
{
    /// <summary>
    /// Defines modal animation types that map to SCSS keyframes
    /// </summary>
    public enum ModalAnimation
    {
        /// <summary>Standard fade in/out animation</summary>
        Fade,
        
        /// <summary>Scale in/out animation with fade</summary>
        Scale,
        
        /// <summary>Slide down from top</summary>
        SlideDown,
        
        /// <summary>Slide up from bottom</summary>
        SlideUp,
        
        /// <summary>Slide in from left</summary>
        SlideLeft,
        
        /// <summary>Slide in from right</summary>
        SlideRight,
        
        /// <summary>Bounce in animation</summary>
        Bounce,
        
        /// <summary>Zoom in animation</summary>
        Zoom,
        
        /// <summary>Flip in animation</summary>
        Flip,
        
        /// <summary>Swing in from top</summary>
        Swing,
        
        /// <summary>Smooth slide up animation</summary>
        SlideUpSmooth,
        
        /// <summary>Dropdown style slide in</summary>
        DropdownSlideIn,
        
        /// <summary>No animation</summary>
        None
    }
}