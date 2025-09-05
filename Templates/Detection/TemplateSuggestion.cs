namespace RR.Blazor.Templates.Detection;

/// <summary>
/// Represents a template suggestion for a specific property
/// </summary>
/// <typeparam name="T">Type of data the template will handle</typeparam>
public class TemplateSuggestion<T> where T : class
{
    /// <summary>
    /// Name of the property this suggestion is for
    /// </summary>
    public string PropertyName { get; set; }
    
    /// <summary>
    /// Suggested template type
    /// </summary>
    public TemplateType TemplateType { get; set; }
    
    /// <summary>
    /// Confidence score (0.0 to 1.0)
    /// </summary>
    public double Confidence { get; set; }
    
    /// <summary>
    /// Human-readable reason for the suggestion
    /// </summary>
    public string Reason { get; set; }
    
    /// <summary>
    /// Configuration hint for the template
    /// </summary>
    public string ConfigurationHint { get; set; }
    
    /// <summary>
    /// Whether this suggestion should be automatically applied
    /// </summary>
    public bool AutoApply => Confidence >= 0.8;
    
    /// <summary>
    /// Whether this suggestion requires user confirmation
    /// </summary>
    public bool RequiresConfirmation => Confidence >= 0.6 && Confidence < 0.8;
}

/// <summary>
/// Types of templates available in the system
/// </summary>
public enum TemplateType
{
    None = 0,
    Currency = 2,
    Stack = 3,
    Group = 4,
    Avatar = 5,
    Progress = 6,
    Rating = 7
}