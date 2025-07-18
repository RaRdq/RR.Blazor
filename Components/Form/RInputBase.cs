using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using RR.Blazor.Attributes;
using RR.Blazor.Enums;
using RR.Blazor.Components.Base;

namespace RR.Blazor.Components.Form
{
    /// <summary>
    /// Abstract base class for all RInput components providing shared parameters and functionality
    /// </summary>
    public abstract class RInputBase : RComponentBase
    {
        #region Core Parameters
        
        [Parameter]
        [AIParameter("Field label", Example = "\"Email Address\"")]
        public string? Label { get; set; }
        
        [Parameter]
        [AIParameter("Placeholder text", Example = "\"Enter your email...\"")]
        public string? Placeholder { get; set; }
        
        [Parameter]
        [AIParameter("Help text displayed below the input", Example = "\"We'll never share your email\"")]
        public string? HelpText { get; set; }
        
        [Parameter]
        [AIParameter("Field name for form submission", Example = "\"email\"")]
        public string? FieldName { get; set; }
        
        [Parameter]
        [AIParameter("Whether the field is required")]
        public bool Required { get; set; }
        
        [Parameter]
        [AIParameter("Whether the field is read-only")]
        public bool ReadOnly { get; set; }
        
        [Parameter]
        [AIParameter("Whether the field is in loading state")]
        public bool Loading { get; set; }
        
        #endregion
        
        #region Styling Parameters
        
        [Parameter]
        [AIParameter("Input variant", SuggestedValues = new[] { "Default", "Clean", "Filled", "Outlined", "Glass" })]
        public TextInputVariant Variant { get; set; } = TextInputVariant.Default;
        
        [Parameter]
        [AIParameter("Input size", SuggestedValues = new[] { "Small", "Medium", "Large" })]
        public TextInputSize Size { get; set; } = TextInputSize.Medium;
        
        
        [Parameter]
        [AIParameter("Start icon name", Example = "\"search\"")]
        public string? StartIcon { get; set; }
        
        [Parameter]
        [AIParameter("End icon name", Example = "\"visibility\"")]
        public string? EndIcon { get; set; }
        
        
        #endregion
        
        #region Validation Parameters
        
        [Parameter]
        [AIParameter("Whether the field has validation error")]
        public bool HasError { get; set; }
        
        [Parameter]
        [AIParameter("Error message to display")]
        public string? ErrorMessage { get; set; }
        
        [Parameter]
        [AIParameter("Maximum character length")]
        public int? MaxLength { get; set; }
        
        #endregion
        
        #region Events
        
        [Parameter] public EventCallback<FocusEventArgs> OnFocus { get; set; }
        [Parameter] public EventCallback<FocusEventArgs> OnBlur { get; set; }
        [Parameter] public EventCallback<KeyboardEventArgs> OnKeyPress { get; set; }
        [Parameter] public EventCallback<KeyboardEventArgs> OnKeyDown { get; set; }
        [Parameter] public EventCallback<MouseEventArgs> OnStartIconClick { get; set; }
        [Parameter] public EventCallback<MouseEventArgs> OnEndIconClick { get; set; }
        
        #endregion
        
        #region Shared Methods
        
        /// <summary>
        /// Get the effective field name for the input
        /// </summary>
        protected virtual string GetEffectiveFieldName()
        {
            if (!string.IsNullOrEmpty(FieldName))
                return FieldName;
                
            // Auto-derive field name from Label by removing spaces and making camelCase
            if (!string.IsNullOrEmpty(Label))
            {
                var fieldName = Label.Replace(" ", "").Replace(":", "");
                if (!string.IsNullOrEmpty(fieldName))
                {
                    return char.ToLowerInvariant(fieldName[0]) + fieldName[1..];
                }
            }
            
            return "";
        }
        
        /// <summary>
        /// Get the effective error message to display
        /// </summary>
        protected virtual string GetEffectiveErrorMessage()
        {
            return ErrorMessage ?? "";
        }
        
        /// <summary>
        /// Check if the input is in error state
        /// </summary>
        protected virtual bool IsInErrorState => HasError || !string.IsNullOrEmpty(ErrorMessage);
        
        /// <summary>
        /// Handle focus event with base functionality
        /// </summary>
        protected async Task HandleFocusEvent(FocusEventArgs e, EventCallback<FocusEventArgs> callback)
        {
            if (callback.HasDelegate)
                await callback.InvokeAsync(e);
        }
        
        /// <summary>
        /// Handle blur event with base functionality
        /// </summary>
        protected async Task HandleBlurEvent(FocusEventArgs e, EventCallback<FocusEventArgs> callback)
        {
            if (callback.HasDelegate)
                await callback.InvokeAsync(e);
        }
        
        /// <summary>
        /// Handle user interaction for validation and state management
        /// </summary>
        protected async Task HandleUserInteraction(bool hasValue = true)
        {
            // Override in derived classes for specific behavior
            await Task.CompletedTask;
        }
        
        #endregion
    }
}