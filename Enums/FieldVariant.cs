namespace RR.Blazor.Enums;

/// <summary>
/// Field style variants for form inputs
/// </summary>
public enum FieldVariant
{
    /// <summary>Default field style with visible borders</summary>
    Default = 0,
    
    /// <summary>Clean style with subtle background and no borders</summary>
    Clean = 1,
    
    /// <summary>Outlined style with prominent borders</summary>
    Outlined = 2,
    
    /// <summary>Filled style with solid background</summary>
    Filled = 3,
    
    /// <summary>Floating label style with animated label positioning</summary>
    FloatingLabel = 4
}