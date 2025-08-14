namespace RR.Blazor.Enums
{
    /// <summary>
    /// Unified variant type enum for all RR.Blazor components.
    /// Provides consistent theming across the entire component library.
    /// </summary>
    public enum VariantType
    {
        /// <summary>Default styling, typically uses neutral colors</summary>
        Default = 0,
        
        /// <summary>Primary brand color for emphasis and main actions</summary>
        Primary,
        
        /// <summary>Secondary neutral color for supporting elements</summary>
        Secondary,
        
        /// <summary>Green color variant indicating positive states, completion, approval</summary>
        Success,
        
        /// <summary>Orange/yellow color variant for warnings, attention, pending states</summary>
        Warning,
        
        /// <summary>Red color variant for errors, failures, critical states (alias for Danger)</summary>
        Error,
        
        /// <summary>Red color variant for destructive actions (alias for Error)</summary>
        Danger,
        
        /// <summary>Blue color variant for information, tips, neutral notifications</summary>
        Info,
        
        /// <summary>Muted/subtle styling for less prominent elements</summary>
        Muted,
        
        /// <summary>Outline style with transparent background</summary>
        Outline,
        
        /// <summary>Ghost style with minimal visual presence</summary>
        Ghost
    }
}