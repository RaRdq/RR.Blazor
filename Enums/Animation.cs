namespace RR.Blazor.Enums
{
    /// <summary>
    /// Generic animation types for all RR.Blazor components
    /// Maps to keyframes defined in _animations.scss
    /// </summary>
    public enum AnimationType
    {
        /// <summary>No animation</summary>
        None,
        
        // Basic Transitions
        /// <summary>Simple fade in/out</summary>
        Fade,
        /// <summary>Fade in with upward movement</summary>
        FadeInUp,
        /// <summary>Fade out with downward movement</summary>
        FadeOutDown,
        
        // Scale Animations
        /// <summary>Scale from 0.9 to 1 with fade</summary>
        Scale,
        /// <summary>Scale in from 0</summary>
        ZoomIn,
        /// <summary>Bounce scale effect</summary>
        Bounce,
        /// <summary>Bounce in animation</summary>
        BounceIn,
        
        // Slide Animations
        /// <summary>Slide from top</summary>
        SlideDown,
        /// <summary>Slide from bottom</summary>
        SlideUp,
        /// <summary>Slide from left</summary>
        SlideLeft,
        /// <summary>Slide from right</summary>
        SlideRight,
        /// <summary>Smooth slide up with fade</summary>
        SlideUpSmooth,
        /// <summary>Slide in from top with subtle movement</summary>
        SlideInDown,
        
        // Advanced Animations
        /// <summary>3D flip effect</summary>
        Flip,
        /// <summary>Swing down from top</summary>
        Swing,
        /// <summary>Dropdown-style entrance</summary>
        DropdownSlideIn,
        /// <summary>Shake horizontally</summary>
        Shake,
        
        // Loading/Progress Animations
        /// <summary>Continuous rotation</summary>
        Spin,
        /// <summary>Pulsing opacity</summary>
        Pulse,
        /// <summary>Expanding ring effect</summary>
        Ping,
        /// <summary>Gentle scale breathing</summary>
        Breathe,
        /// <summary>Shimmer effect for loading</summary>
        Shimmer,
        
        // State Animations
        /// <summary>Error state pulse</summary>
        ErrorPulse,
        /// <summary>Error glow effect</summary>
        ErrorGlow,
        /// <summary>Success fade in with scale</summary>
        SuccessFadeIn,
        /// <summary>Glow pulsing effect</summary>
        GlowPulse,
        
        // Directional Slide with Scale
        /// <summary>Slide in from left with scale</summary>
        SlideInLeftScale,
        /// <summary>Slide in from right with scale</summary>
        SlideInRightScale,
        /// <summary>Slide in from top with scale</summary>
        SlideInTopScale,
        /// <summary>Slide in from bottom with scale</summary>
        SlideInBottomScale,
        
        // Special Effects
        /// <summary>Floating up and down</summary>
        Float,
        /// <summary>Line drawing effect</summary>
        LineDrawIn,
        /// <summary>Width expansion</summary>
        ExpandWidth,
        /// <summary>Hover lift effect</summary>
        HoverLift,
        /// <summary>Scale morphing</summary>
        ScaleMorph,
        
        // Blazor-specific
        /// <summary>Blazor error slide in</summary>
        BlazorErrorSlideIn
    }
    
    /// <summary>
    /// Animation trigger conditions
    /// </summary>
    public enum AnimationTrigger
    {
        /// <summary>On element mount/show</summary>
        OnMount,
        /// <summary>On element unmount/hide</summary>
        OnUnmount,
        /// <summary>On hover</summary>
        OnHover,
        /// <summary>On focus</summary>
        OnFocus,
        /// <summary>On click/active</summary>
        OnActive,
        /// <summary>On error state</summary>
        OnError,
        /// <summary>On success state</summary>
        OnSuccess,
        /// <summary>Always active (infinite)</summary>
        Always,
        /// <summary>Manual trigger via code</summary>
        Manual
    }
    
    /// <summary>
    /// Animation fill modes
    /// </summary>
    public enum AnimationFillMode
    {
        /// <summary>Default - no fill</summary>
        None,
        /// <summary>Retain final state</summary>
        Forwards,
        /// <summary>Apply initial state before start</summary>
        Backwards,
        /// <summary>Both forwards and backwards</summary>
        Both
    }
    
    /// <summary>
    /// Animation direction
    /// </summary>
    public enum AnimationDirection
    {
        /// <summary>Normal direction</summary>
        Normal,
        /// <summary>Reverse direction</summary>
        Reverse,
        /// <summary>Alternate between normal and reverse</summary>
        Alternate,
        /// <summary>Alternate starting with reverse</summary>
        AlternateReverse
    }
    
    /// <summary>
    /// Animation iteration count
    /// </summary>
    public enum AnimationIteration
    {
        /// <summary>Play once</summary>
        Once = 1,
        /// <summary>Play twice</summary>
        Twice = 2,
        /// <summary>Play three times</summary>
        Thrice = 3,
        /// <summary>Play infinitely</summary>
        Infinite = -1
    }
}