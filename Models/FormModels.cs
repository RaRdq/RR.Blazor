using RR.Blazor.Enums;

namespace RR.Blazor.Models;

/// <summary>
/// Form validation result containing field-level and form-level errors
/// </summary>
public class FormValidationResult
{
    public bool IsValid { get; set; } = true;
    public Dictionary<string, List<string>> FieldErrors { get; set; } = new();
    public List<string> FormErrors { get; set; } = new();
    public string? GeneralError { get; set; }
    
    public void AddFieldError(string fieldName, string error)
    {
        if (!FieldErrors.ContainsKey(fieldName))
            FieldErrors[fieldName] = new List<string>();
        FieldErrors[fieldName].Add(error);
        IsValid = false;
    }
    
    public void AddFormError(string error)
    {
        FormErrors.Add(error);
        IsValid = false;
    }
    
    public List<string> GetFieldErrors(string fieldName)
    {
        return FieldErrors.GetValueOrDefault(fieldName, new List<string>());
    }
    
    public bool HasFieldErrors(string fieldName)
    {
        return FieldErrors.ContainsKey(fieldName) && FieldErrors[fieldName].Any();
    }
    
    public void Clear()
    {
        IsValid = true;
        FieldErrors.Clear();
        FormErrors.Clear();
        GeneralError = null;
    }
}

/// <summary>
/// Custom validation function delegate
/// </summary>
public delegate Task<FormValidationResult> FormValidationDelegate<T>(T model) where T : class;

/// <summary>
/// Form submission event arguments
/// </summary>
public class FormSubmissionEventArgs<T> where T : class
{
    public T Model { get; set; }
    public FormValidationResult ValidationResult { get; set; }
    public CancellationToken CancellationToken { get; set; }
    public bool Cancel { get; set; } = false;
    
    public FormSubmissionEventArgs(T model, FormValidationResult validationResult, CancellationToken cancellationToken = default)
    {
        Model = model;
        ValidationResult = validationResult;
        CancellationToken = cancellationToken;
    }
}

/// <summary>
/// Form state change event arguments
/// </summary>
public class FormStateChangedEventArgs
{
    public FormState PreviousState { get; set; }
    public FormState CurrentState { get; set; }
    public string? Message { get; set; }
    
    public FormStateChangedEventArgs(FormState previousState, FormState currentState, string? message = null)
    {
        PreviousState = previousState;
        CurrentState = currentState;
        Message = message;
    }
}

/// <summary>
/// Form field validation event arguments
/// </summary>
public class FieldValidationEventArgs
{
    public string FieldName { get; set; }
    public object? Value { get; set; }
    public List<string> Errors { get; set; } = new();
    public bool IsValid => !Errors.Any();
    
    public FieldValidationEventArgs(string fieldName, object? value)
    {
        FieldName = fieldName;
        Value = value;
    }
    
    public void AddError(string error)
    {
        Errors.Add(error);
    }
}

/// <summary>
/// Form configuration options
/// </summary>
public class FormOptions
{
    /// <summary>
    /// Whether to automatically disable form during submission
    /// </summary>
    public bool AutoDisableOnSubmit { get; set; } = true;
    
    /// <summary>
    /// Whether to reset form after successful submission
    /// </summary>
    public bool ResetAfterSubmit { get; set; } = false;
    
    /// <summary>
    /// Whether to show validation summary at form level
    /// </summary>
    public bool ShowValidationSummary { get; set; } = true;
    
    /// <summary>
    /// Whether to validate on field blur
    /// </summary>
    public bool ValidateOnBlur { get; set; } = true;
    
    /// <summary>
    /// Whether to validate on field change
    /// </summary>
    public bool ValidateOnChange { get; set; } = false;
    
    /// <summary>
    /// Debounce delay in milliseconds for change validation
    /// </summary>
    public int ValidationDebounceMs { get; set; } = 300;
    
    /// <summary>
    /// Whether to focus first invalid field on validation failure
    /// </summary>
    public bool FocusFirstInvalidField { get; set; } = true;
    
    /// <summary>
    /// Whether to prevent form submission on Enter key
    /// </summary>
    public bool PreventEnterSubmit { get; set; } = false;
    
    /// <summary>
    /// Success message to show after successful submission
    /// </summary>
    public string? SuccessMessage { get; set; }
    
    /// <summary>
    /// Duration in milliseconds to show success message
    /// </summary>
    public int SuccessMessageDuration { get; set; } = 3000;
}

/// <summary>
/// Form section configuration
/// </summary>
public class FormSectionConfig
{
    public string? Title { get; set; }
    public string? Description { get; set; }
    public string? Icon { get; set; }
    public SectionElevation Elevation { get; set; } = SectionElevation.None;
    public bool IsCollapsible { get; set; } = false;
    public bool IsExpanded { get; set; } = true;
    public string? CssClass { get; set; }
}