namespace RR.Blazor.Models;

/// <summary>
/// Marker interface for strongly-typed modal parameters.
/// Implement this interface on your parameter classes for type-safe modal communication.
/// </summary>
public interface IModalParameters 
{
    // Marker interface - no members required
    // Users can add their own properties in implementing classes
}

/// <summary>
/// Base class for modal parameters with common properties
/// </summary>
public abstract class ModalParametersBase : IModalParameters
{
    /// <summary>
    /// Optional modal ID for tracking
    /// </summary>
    public string ModalId { get; set; }
    
    /// <summary>
    /// Optional parent component reference
    /// </summary>
    public object ParentComponent { get; set; }
}