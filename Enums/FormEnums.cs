namespace RR.Blazor.Enums;

/// <summary>
/// Validation modes supported by RForm component
/// </summary>
public enum ValidationMode
{
    /// <summary>
    /// No automatic validation - manual validation only
    /// </summary>
    Manual = 0,
    
    /// <summary>
    /// Use DataAnnotations attributes for validation
    /// </summary>
    DataAnnotations = 1,
    
    /// <summary>
    /// Use custom validation functions
    /// </summary>
    Custom = 2,
    
    /// <summary>
    /// Combine DataAnnotations with custom validation
    /// </summary>
    Hybrid = 3
}

/// <summary>
/// Form layout modes for different use cases
/// </summary>
public enum FormLayout
{
    /// <summary>
    /// Standard vertical form layout
    /// </summary>
    Default = 0,
    
    /// <summary>
    /// Compact horizontal layout for simple forms
    /// </summary>
    Compact = 1,
    
    /// <summary>
    /// Wide layout with 2-column field arrangement
    /// </summary>
    Wide = 2,
    
    /// <summary>
    /// Executive dashboard style with elevated sections
    /// </summary>
    Executive = 3,
    
    /// <summary>
    /// Modal-optimized layout with proper spacing
    /// </summary>
    Modal = 4
}

/// <summary>
/// Form submission states
/// </summary>
public enum FormState
{
    /// <summary>
    /// Form is ready for input
    /// </summary>
    Ready = 0,
    
    /// <summary>
    /// Form is being validated
    /// </summary>
    Validating = 1,
    
    /// <summary>
    /// Form is being submitted
    /// </summary>
    Submitting = 2,
    
    /// <summary>
    /// Form submission succeeded
    /// </summary>
    Success = 3,
    
    /// <summary>
    /// Form submission failed
    /// </summary>
    Error = 4,
    
    /// <summary>
    /// Form is disabled
    /// </summary>
    Disabled = 5
}

/// <summary>
/// Form section elevation levels for visual hierarchy
/// </summary>
public enum SectionElevation
{
    /// <summary>
    /// No elevation - flat appearance
    /// </summary>
    None = 0,
    
    /// <summary>
    /// Subtle elevation for grouping
    /// </summary>
    Subtle = 1,
    
    /// <summary>
    /// Medium elevation for emphasis
    /// </summary>
    Medium = 2,
    
    /// <summary>
    /// High elevation for executive interfaces
    /// </summary>
    High = 3
}


/// <summary>
/// Spacing size between form elements
/// </summary>
public enum SpacingType
{
    /// <summary>
    /// No spacing
    /// </summary>
    None = 0,
    
    /// <summary>
    /// Small spacing
    /// </summary>
    Small = 1,
    
    /// <summary>
    /// Medium spacing (default)
    /// </summary>
    Medium = 2,
    
    /// <summary>
    /// Large spacing
    /// </summary>
    Large = 3
}