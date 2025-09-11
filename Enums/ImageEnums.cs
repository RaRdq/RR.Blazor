namespace RR.Blazor.Enums;

/// <summary>
/// Visual style variants for image components
/// </summary>
public enum ImageVariant
{
    /// <summary>
    /// Default image presentation without special styling
    /// </summary>
    Default,
    
    /// <summary>
    /// Circular image presentation with rounded borders
    /// </summary>
    Circle,
    
    /// <summary>
    /// Image with rounded corners
    /// </summary>
    Rounded,
    
    /// <summary>
    /// Square image presentation with sharp corners
    /// </summary>
    Square,
    
    /// <summary>
    /// Thumbnail style with border and padding
    /// </summary>
    Thumbnail
}

/// <summary>
/// Defines how an image should be resized to fit its container
/// </summary>
public enum ObjectFit
{
    /// <summary>
    /// Scales the image to cover the entire container while maintaining aspect ratio
    /// </summary>
    Cover,
    
    /// <summary>
    /// Scales the image to fit within the container while maintaining aspect ratio
    /// </summary>
    Contain,
    
    /// <summary>
    /// Stretches the image to fill the container, ignoring aspect ratio
    /// </summary>
    Fill,
    
    /// <summary>
    /// Scales down the image to fit if it's larger than the container
    /// </summary>
    ScaleDown,
    
    /// <summary>
    /// Displays the image at its original size
    /// </summary>
    None,
    
    /// <summary>
    /// Inherits the object-fit value from the parent element
    /// </summary>
    Inherit
}