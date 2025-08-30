using RR.Blazor.Enums;

namespace RR.Blazor.Utilities
{
    /// <summary>
    /// Generic animation utility for all RR.Blazor components
    /// Maps animation enums to CSS values from _animations.scss
    /// </summary>
    public static class AnimationHelper
    {
        /// <summary>
        /// Maps AnimationType enum to CSS keyframe name
        /// </summary>
        public static string GetKeyframeName(AnimationType animation)
        {
            return animation switch
            {
                AnimationType.None => "none",
                
                // Basic Transitions
                AnimationType.Fade => "fadeIn",
                AnimationType.FadeInUp => "fadeInUp",
                AnimationType.FadeOutDown => "fadeOutDown",
                
                // Scale Animations
                AnimationType.Scale => "scaleIn",
                AnimationType.ZoomIn => "zoomIn",
                AnimationType.Bounce => "bounce",
                AnimationType.BounceIn => "bounceIn",
                
                // Slide Animations
                AnimationType.SlideDown => "slideDown",
                AnimationType.SlideUp => "slideUp",
                AnimationType.SlideLeft => "slideLeft",
                AnimationType.SlideRight => "slideRight",
                AnimationType.SlideUpSmooth => "slideUpSmooth",
                AnimationType.SlideInDown => "slideInDown",
                
                // Advanced Animations
                AnimationType.Flip => "flipIn",
                AnimationType.Swing => "swingIn",
                AnimationType.DropdownSlideIn => "dropdownSlideIn",
                AnimationType.Shake => "shake",
                
                // Loading/Progress Animations
                AnimationType.Spin => "spin",
                AnimationType.Pulse => "pulse",
                AnimationType.Ping => "ping",
                AnimationType.Breathe => "breathe",
                AnimationType.Shimmer => "shimmer",
                
                // State Animations
                AnimationType.ErrorPulse => "errorPulse",
                AnimationType.ErrorGlow => "errorGlow",
                AnimationType.SuccessFadeIn => "successFadeIn",
                AnimationType.GlowPulse => "glow-pulse",
                
                // Directional Slide with Scale
                AnimationType.SlideInLeftScale => "slideInLeftScale",
                AnimationType.SlideInRightScale => "slideInRightScale",
                AnimationType.SlideInTopScale => "slideInTopScale",
                AnimationType.SlideInBottomScale => "slideInBottomScale",
                
                // Special Effects
                AnimationType.Float => "float",
                AnimationType.LineDrawIn => "lineDrawIn",
                AnimationType.ExpandWidth => "expand-width",
                AnimationType.HoverLift => "hover-lift",
                AnimationType.ScaleMorph => "scale-morph",
                
                // Blazor-specific
                AnimationType.BlazorErrorSlideIn => "blazorErrorSlideIn",
                
                _ => "fadeIn"
            };
        }
        
        /// <summary>
        /// Maps AnimationType enum to corresponding exit keyframe name
        /// </summary>
        public static string GetExitKeyframeName(AnimationType animation)
        {
            return animation switch
            {
                AnimationType.None => "none",
                
                // Basic Transitions
                AnimationType.Fade => "fadeOut",
                AnimationType.FadeInUp => "fadeOutDown",
                AnimationType.FadeOutDown => "fadeInUp",
                
                // Scale Animations
                AnimationType.Scale => "scaleOut",
                AnimationType.ZoomIn => "fadeOut",
                AnimationType.Bounce => "fadeOut",
                AnimationType.BounceIn => "fadeOut",
                
                // Slide Animations - reverse directions for exit
                AnimationType.SlideDown => "slideUp",
                AnimationType.SlideUp => "slideDown",
                AnimationType.SlideLeft => "slideRight",
                AnimationType.SlideRight => "slideLeft",
                AnimationType.SlideUpSmooth => "fadeOutDown",
                AnimationType.SlideInDown => "fadeInUp",
                
                // Advanced Animations - use simple fade for complex exits
                AnimationType.Flip => "fadeOut",
                AnimationType.Swing => "fadeOut",
                AnimationType.DropdownSlideIn => "fadeOutDown",
                AnimationType.Shake => "fadeOut",
                
                // Loading/Progress Animations - no exit needed (infinite)
                AnimationType.Spin => "none",
                AnimationType.Pulse => "none",
                AnimationType.Ping => "none",
                AnimationType.Breathe => "none",
                AnimationType.Shimmer => "none",
                
                // State Animations
                AnimationType.ErrorPulse => "fadeOut",
                AnimationType.ErrorGlow => "fadeOut",
                AnimationType.SuccessFadeIn => "fadeOut",
                AnimationType.GlowPulse => "fadeOut",
                
                // Directional Slide with Scale - reverse with fade
                AnimationType.SlideInLeftScale => "fadeOut",
                AnimationType.SlideInRightScale => "fadeOut",
                AnimationType.SlideInTopScale => "fadeOut",
                AnimationType.SlideInBottomScale => "fadeOut",
                
                // Special Effects
                AnimationType.Float => "none",
                AnimationType.LineDrawIn => "fadeOut",
                AnimationType.ExpandWidth => "widthShrink",
                AnimationType.HoverLift => "none",
                AnimationType.ScaleMorph => "none",
                
                // Blazor-specific
                AnimationType.BlazorErrorSlideIn => "fadeOut",
                
                _ => "fadeOut"
            };
        }
        
        /// <summary>
        /// Maps AnimationSpeed enum to CSS duration variable
        /// </summary>
        public static string GetDuration(AnimationSpeed speed)
        {
            return speed switch
            {
                AnimationSpeed.UltraFast => "var(--duration-ultra-fast)",
                AnimationSpeed.Fast => "var(--duration-fast)",
                AnimationSpeed.Normal => "var(--duration-normal)",
                AnimationSpeed.Slow => "var(--duration-slow)",
                AnimationSpeed.VerySlow => "var(--duration-very-slow)",
                _ => "var(--duration-normal)"
            };
        }
        
        /// <summary>
        /// Maps AnimationSpeed enum to CSS timing function
        /// </summary>
        public static string GetTimingFunction(AnimationSpeed speed)
        {
            return speed switch
            {
                AnimationSpeed.UltraFast => "var(--ease-out)",
                AnimationSpeed.Fast => "var(--ease-out)",
                AnimationSpeed.Normal => "var(--ease-out)",
                AnimationSpeed.Slow => "var(--ease-in-out)",
                AnimationSpeed.VerySlow => "var(--ease-in-out)",
                _ => "var(--ease-out)"
            };
        }
        
        /// <summary>
        /// Maps AnimationFillMode enum to CSS value
        /// </summary>
        public static string GetFillMode(AnimationFillMode fillMode)
        {
            return fillMode switch
            {
                AnimationFillMode.None => "none",
                AnimationFillMode.Forwards => "forwards",
                AnimationFillMode.Backwards => "backwards",
                AnimationFillMode.Both => "both",
                _ => "both"
            };
        }
        
        /// <summary>
        /// Maps AnimationDirection enum to CSS value
        /// </summary>
        public static string GetDirection(AnimationDirection direction)
        {
            return direction switch
            {
                AnimationDirection.Normal => "normal",
                AnimationDirection.Reverse => "reverse",
                AnimationDirection.Alternate => "alternate",
                AnimationDirection.AlternateReverse => "alternate-reverse",
                _ => "normal"
            };
        }
        
        /// <summary>
        /// Maps AnimationIteration enum to CSS value
        /// </summary>
        public static string GetIterationCount(AnimationIteration iteration)
        {
            return iteration switch
            {
                AnimationIteration.Infinite => "infinite",
                _ => ((int)iteration).ToString()
            };
        }
        
        /// <summary>
        /// Gets complete CSS animation property value for entry animations
        /// </summary>
        public static string GetAnimationProperty(AnimationType animation, AnimationSpeed speed = AnimationSpeed.Normal, 
            AnimationFillMode fillMode = AnimationFillMode.Both, AnimationDirection direction = AnimationDirection.Normal,
            AnimationIteration iteration = AnimationIteration.Once)
        {
            if (animation == AnimationType.None)
                return "none";
                
            var keyframe = GetKeyframeName(animation);
            var duration = GetDuration(speed);
            var timing = GetTimingFunction(speed);
            var fill = GetFillMode(fillMode);
            var dir = GetDirection(direction);
            var count = GetIterationCount(iteration);
            
            return $"{keyframe} {duration} {timing} {fill} {dir} {count}".Trim();
        }
        
        /// <summary>
        /// Gets complete CSS animation property value for exit animations
        /// </summary>
        public static string GetExitAnimationProperty(AnimationType animation, AnimationSpeed speed = AnimationSpeed.Normal,
            AnimationFillMode fillMode = AnimationFillMode.Both)
        {
            if (animation == AnimationType.None)
                return "none";
                
            var keyframe = GetExitKeyframeName(animation);
            if (keyframe == "none")
                return "none";
                
            var duration = GetDuration(speed);
            var timing = GetTimingFunction(speed);
            var fill = GetFillMode(fillMode);
            
            return $"{keyframe} {duration} {timing} {fill}".Trim();
        }
        
        /// <summary>
        /// Gets the corresponding CSS utility class name
        /// </summary>
        public static string GetUtilityClass(AnimationType animation)
        {
            return animation switch
            {
                AnimationType.None => "",
                AnimationType.Fade => "animate-fade-in",
                AnimationType.Scale => "animate-scale-in",
                AnimationType.SlideUp => "animate-slide-up",
                AnimationType.SlideDown => "animate-slide-in-down",
                AnimationType.Bounce => "animate-bounce",
                AnimationType.Spin => "animate-spin",
                AnimationType.Pulse => "animate-pulse",
                AnimationType.Ping => "animate-ping",
                AnimationType.Shake => "animate-shake",
                AnimationType.ErrorPulse => "animate-error-pulse",
                AnimationType.SuccessFadeIn => "animate-success-fade-in",
                _ => "animate-fade-in"
            };
        }
        
        /// <summary>
        /// Determines if animation is infinite (for loading states)
        /// </summary>
        public static bool IsInfiniteAnimation(AnimationType animation)
        {
            return animation switch
            {
                AnimationType.Spin or AnimationType.Pulse or AnimationType.Ping or 
                AnimationType.Breathe or AnimationType.Shimmer or AnimationType.Float => true,
                _ => false
            };
        }
        
        /// <summary>
        /// Gets default animation for component entrance
        /// </summary>
        public static AnimationType GetDefaultEntranceAnimation() => AnimationType.Scale;
        
        /// <summary>
        /// Gets default animation for component exit
        /// </summary>
        public static AnimationType GetDefaultExitAnimation() => AnimationType.Fade;
    }
}