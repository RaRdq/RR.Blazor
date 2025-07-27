using RR.Blazor.Enums;

namespace RR.Blazor.Utilities
{
    /// <summary>
    /// Utility class for mapping animation enums to CSS values
    /// </summary>
    public static class AnimationHelper
    {
        /// <summary>
        /// Maps ModalAnimation enum to CSS keyframe name
        /// </summary>
        public static string GetKeyframeName(ModalAnimation animation)
        {
            return animation switch
            {
                ModalAnimation.Fade => "fadeIn",
                ModalAnimation.Scale => "scaleIn",
                ModalAnimation.SlideDown => "slideDown",
                ModalAnimation.SlideUp => "slideUp",
                ModalAnimation.SlideLeft => "slideLeft",
                ModalAnimation.SlideRight => "slideRight",
                ModalAnimation.Bounce => "bounceIn",
                ModalAnimation.Zoom => "zoomIn",
                ModalAnimation.Flip => "flipIn",
                ModalAnimation.Swing => "swingIn",
                ModalAnimation.SlideUpSmooth => "slideUpSmooth",
                ModalAnimation.DropdownSlideIn => "dropdownSlideIn",
                ModalAnimation.None => "none",
                _ => "fadeIn"
            };
        }
        
        /// <summary>
        /// Maps ModalAnimation enum to CSS exit keyframe name
        /// </summary>
        public static string GetExitKeyframeName(ModalAnimation animation)
        {
            return animation switch
            {
                ModalAnimation.Fade => "fadeOut",
                ModalAnimation.Scale => "scaleOut",
                ModalAnimation.SlideDown => "slideUp", // Reverse direction for exit
                ModalAnimation.SlideUp => "slideDown", // Reverse direction for exit
                ModalAnimation.SlideLeft => "slideRight", // Reverse direction for exit
                ModalAnimation.SlideRight => "slideLeft", // Reverse direction for exit
                ModalAnimation.Bounce => "fadeOut", // Simple fade for exit
                ModalAnimation.Zoom => "fadeOut", // Simple fade for exit
                ModalAnimation.Flip => "fadeOut", // Simple fade for exit
                ModalAnimation.Swing => "fadeOut", // Simple fade for exit
                ModalAnimation.SlideUpSmooth => "fadeOutDown",
                ModalAnimation.DropdownSlideIn => "fadeOutDown",
                ModalAnimation.None => "none",
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
        /// Gets complete CSS animation property value
        /// </summary>
        public static string GetAnimationProperty(ModalAnimation animation, AnimationSpeed speed)
        {
            if (animation == ModalAnimation.None)
                return "none";
                
            var keyframe = GetKeyframeName(animation);
            var duration = GetDuration(speed);
            var timing = GetTimingFunction(speed);
            
            return $"{keyframe} {duration} {timing} both";
        }
        
        /// <summary>
        /// Gets complete CSS exit animation property value
        /// </summary>
        public static string GetExitAnimationProperty(ModalAnimation animation, AnimationSpeed speed)
        {
            if (animation == ModalAnimation.None)
                return "none";
                
            var keyframe = GetExitKeyframeName(animation);
            var duration = GetDuration(speed);
            var timing = GetTimingFunction(speed);
            
            return $"{keyframe} {duration} {timing} both";
        }
    }
}