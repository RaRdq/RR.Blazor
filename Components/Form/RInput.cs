using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.Web;
using RR.Blazor.Attributes;
using RR.Blazor.Enums;
using RR.Blazor.Models;

namespace RR.Blazor.Components.Form
{
    /// <summary>
    /// Smart input component with automatic type detection and conversion
    /// </summary>
    [Component("RInput", Category = "Form", Complexity = ComponentComplexity.Advanced)]
    [AIOptimized(
        Prompt = "Smart input that automatically detects type and renders appropriate component",
        CommonUse = "Forms with mixed data types, automatic type conversion, payroll data entry",
        AvoidUsage = "When you need specific input behavior, use specialized components instead"
    )]
    public class RInput : ComponentBase
    {
        #region Core Parameters
        
        [Parameter]
        [AIParameter("Input value of any supported type", Example = "\"Hello World\" or 42 or DateTime.Now")]
        public object? Value { get; set; }
        
        [Parameter]
        [AIParameter("Value changed callback")]
        public EventCallback<object> ValueChanged { get; set; }
        
        [Parameter]
        [AIParameter("Explicit field type override", Example = "FieldType.Email")]
        public FieldType? InputType { get; set; }
        
        [Parameter]
        [AIParameter("Enable automatic type conversion")]
        public bool AutoConvert { get; set; } = true;
        
        #endregion
        
        #region Numeric Parameters
        
        [Parameter]
        [AIParameter("Minimum value for numeric inputs", Example = "0")]
        public decimal? Min { get; set; }
        
        [Parameter]
        [AIParameter("Maximum value for numeric inputs", Example = "100")]
        public decimal? Max { get; set; }
        
        [Parameter]
        [AIParameter("Step value for numeric inputs", Example = "0.01")]
        public decimal? Step { get; set; }
        
        [Parameter]
        [AIParameter("Number format for display", Example = "\"C2\" or \"P2\"")]
        public string? NumberFormat { get; set; }
        
        #endregion
        
        #region DateTime Parameters
        
        [Parameter]
        [AIParameter("Date format for display", Example = "\"yyyy-MM-dd\"")]
        public string? DateFormat { get; set; }
        
        [Parameter]
        [AIParameter("Show time picker for date inputs")]
        public bool ShowTime { get; set; } = false;
        
        #endregion
        
        #region Inherited Parameters from RInputBase
        
        [Parameter] public string? Label { get; set; }
        [Parameter] public string? Placeholder { get; set; }
        [Parameter] public string? HelpText { get; set; }
        [Parameter] public string? FieldName { get; set; }
        [Parameter] public bool Required { get; set; }
        [Parameter] public bool Disabled { get; set; }
        [Parameter] public bool ReadOnly { get; set; }
        [Parameter] public bool Loading { get; set; }
        [Parameter] public TextInputVariant Variant { get; set; } = TextInputVariant.Default;
        [Parameter] public TextInputSize Size { get; set; } = TextInputSize.Medium;
        [Parameter] public ComponentDensity Density { get; set; } = ComponentDensity.Normal;
        [Parameter] public string? StartIcon { get; set; }
        [Parameter] public string? EndIcon { get; set; }
        [Parameter] public string? Class { get; set; }
        [Parameter] public string? Style { get; set; }
        [Parameter] public bool HasError { get; set; }
        [Parameter] public string? ErrorMessage { get; set; }
        [Parameter] public int? MaxLength { get; set; }
        [Parameter] public EventCallback<FocusEventArgs> OnFocus { get; set; }
        [Parameter] public EventCallback<FocusEventArgs> OnBlur { get; set; }
        [Parameter] public EventCallback<KeyboardEventArgs> OnKeyPress { get; set; }
        [Parameter] public EventCallback<KeyboardEventArgs> OnKeyDown { get; set; }
        [Parameter] public EventCallback<MouseEventArgs> OnStartIconClick { get; set; }
        [Parameter] public EventCallback<MouseEventArgs> OnEndIconClick { get; set; }
        
        #endregion
        
        #region Type Detection
        
        /// <summary>
        /// Detect the appropriate field type based on value and parameters
        /// </summary>
        private FieldType DetectFieldType()
        {
            // Use explicit type if provided
            if (InputType.HasValue)
                return InputType.Value;
            
            // Detect from value type
            if (Value != null)
            {
                return Value switch
                {
                    DateTime => FieldType.DateTime,
                    DateTimeOffset => FieldType.DateTime,
                    TimeRange => FieldType.TimeRange,
                    int => FieldType.Number,
                    long => FieldType.Number,
                    decimal => FieldType.Number,
                    double => FieldType.Number,
                    float => FieldType.Number,
                    bool => FieldType.Boolean,
                    string str when IsEmailFormat(str) => FieldType.Email,
                    string str when IsUrlFormat(str) => FieldType.Url,
                    string str when IsTelFormat(str) => FieldType.Tel,
                    _ => FieldType.Text
                };
            }
            
            // Detect from label hints
            if (!string.IsNullOrEmpty(Label))
            {
                var labelLower = Label.ToLower();
                if (labelLower.Contains("email")) return FieldType.Email;
                if (labelLower.Contains("password")) return FieldType.Password;
                if (labelLower.Contains("phone") || labelLower.Contains("tel")) return FieldType.Tel;
                if (labelLower.Contains("url") || labelLower.Contains("website")) return FieldType.Url;
                if (labelLower.Contains("search")) return FieldType.Search;
                if (labelLower.Contains("date") || labelLower.Contains("time")) return FieldType.DateTime;
                if (labelLower.Contains("number") || labelLower.Contains("amount") || labelLower.Contains("price")) return FieldType.Number;
            }
            
            return FieldType.Text;
        }
        
        private bool IsEmailFormat(string str) => str.Contains("@") && str.Contains(".");
        private bool IsUrlFormat(string str) => str.StartsWith("http://") || str.StartsWith("https://");
        private bool IsTelFormat(string str) => str.All(c => char.IsDigit(c) || c == '-' || c == '(' || c == ')' || c == ' ' || c == '+');
        
        #endregion
        
        #region Rendering
        
        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            var fieldType = DetectFieldType();
            
            switch (fieldType)
            {
                case FieldType.Number:
                    RenderNumericInput(builder);
                    break;
                case FieldType.DateTime:
                    RenderDateTimeInput(builder);
                    break;
                case FieldType.TimeRange:
                    RenderTimeRangeInput(builder);
                    break;
                case FieldType.Boolean:
                    RenderBooleanInput(builder);
                    break;
                default:
                    RenderTextInput(builder, fieldType);
                    break;
            }
        }
        
        private void RenderTextInput(RenderTreeBuilder builder, FieldType fieldType)
        {
            builder.OpenComponent<RTextInput>(0);
            
            // Value binding
            builder.AddAttribute(1, "Value", ConvertToString(Value));
            builder.AddAttribute(2, "ValueChanged", EventCallback.Factory.Create<string>(this, OnTextValueChanged));
            
            // Type and validation
            builder.AddAttribute(3, "Type", fieldType);
            if (Min.HasValue) builder.AddAttribute(4, "Min", Min.Value);
            if (Max.HasValue) builder.AddAttribute(5, "Max", Max.Value);
            if (Step.HasValue) builder.AddAttribute(6, "Step", Step.Value);
            
            // Add all inherited parameters
            AddInheritedParameters(builder, 10);
            
            builder.CloseComponent();
        }
        
        private void RenderNumericInput(RenderTreeBuilder builder)
        {
            builder.OpenComponent<RTextInput>(0);
            
            // Value binding with numeric conversion
            builder.AddAttribute(1, "Value", ConvertToString(Value));
            builder.AddAttribute(2, "ValueChanged", EventCallback.Factory.Create<string>(this, OnNumericValueChanged));
            
            // Numeric parameters
            builder.AddAttribute(3, "Type", FieldType.Number);
            if (Min.HasValue) builder.AddAttribute(4, "Min", Min.Value);
            if (Max.HasValue) builder.AddAttribute(5, "Max", Max.Value);
            if (Step.HasValue) builder.AddAttribute(6, "Step", Step.Value);
            
            // Add all inherited parameters
            AddInheritedParameters(builder, 10);
            
            builder.CloseComponent();
        }
        
        private void RenderDateTimeInput(RenderTreeBuilder builder)
        {
            builder.OpenComponent<RTextInput>(0);
            
            // Value binding with date conversion
            builder.AddAttribute(1, "Value", ConvertDateTimeToString(Value));
            builder.AddAttribute(2, "ValueChanged", EventCallback.Factory.Create<string>(this, OnDateTimeValueChanged));
            
            // Date parameters
            builder.AddAttribute(3, "Type", ShowTime ? FieldType.DateTimeLocal : FieldType.Date);
            
            // Add all inherited parameters
            AddInheritedParameters(builder, 10);
            
            builder.CloseComponent();
        }
        
        private void RenderTimeRangeInput(RenderTreeBuilder builder)
        {
            builder.OpenComponent<RTextInput>(0);
            
            // Value binding with time range conversion
            builder.AddAttribute(1, "Value", ConvertTimeRangeToString(Value));
            builder.AddAttribute(2, "ValueChanged", EventCallback.Factory.Create<string>(this, OnTimeRangeValueChanged));
            
            // Time range parameters
            builder.AddAttribute(3, "Type", FieldType.Text);
            builder.AddAttribute(4, "Placeholder", Placeholder ?? "HH:mm-HH:mm");
            
            // Add all inherited parameters
            AddInheritedParameters(builder, 10);
            
            builder.CloseComponent();
        }
        
        private void RenderBooleanInput(RenderTreeBuilder builder)
        {
            builder.OpenComponent<RCheckbox>(0);
            
            // Value binding
            builder.AddAttribute(1, "Checked", ConvertToBool(Value));
            builder.AddAttribute(2, "CheckedChanged", EventCallback.Factory.Create<bool>(this, OnBooleanValueChanged));
            
            // Add common parameters
            if (!string.IsNullOrEmpty(Label)) builder.AddAttribute(3, "Label", Label);
            if (Disabled) builder.AddAttribute(4, "Disabled", Disabled);
            if (Required) builder.AddAttribute(5, "Required", Required);
            if (HasError) builder.AddAttribute(6, "HasError", HasError);
            if (!string.IsNullOrEmpty(ErrorMessage)) builder.AddAttribute(7, "ErrorMessage", ErrorMessage);
            
            builder.CloseComponent();
        }
        
        private void AddInheritedParameters(RenderTreeBuilder builder, int startSequence)
        {
            var seq = startSequence;
            
            if (!string.IsNullOrEmpty(Label)) builder.AddAttribute(seq++, "Label", Label);
            if (!string.IsNullOrEmpty(Placeholder)) builder.AddAttribute(seq++, "Placeholder", Placeholder);
            if (!string.IsNullOrEmpty(HelpText)) builder.AddAttribute(seq++, "HelpText", HelpText);
            if (!string.IsNullOrEmpty(FieldName)) builder.AddAttribute(seq++, "FieldName", FieldName);
            if (Required) builder.AddAttribute(seq++, "Required", Required);
            if (Disabled) builder.AddAttribute(seq++, "Disabled", Disabled);
            if (ReadOnly) builder.AddAttribute(seq++, "ReadOnly", ReadOnly);
            if (Loading) builder.AddAttribute(seq++, "Loading", Loading);
            if (Variant != TextInputVariant.Default) builder.AddAttribute(seq++, "Variant", Variant);
            if (Size != TextInputSize.Medium) builder.AddAttribute(seq++, "Size", Size);
            if (Density != ComponentDensity.Normal) builder.AddAttribute(seq++, "Density", Density);
            if (!string.IsNullOrEmpty(StartIcon)) builder.AddAttribute(seq++, "StartIcon", StartIcon);
            if (!string.IsNullOrEmpty(EndIcon)) builder.AddAttribute(seq++, "EndIcon", EndIcon);
            if (!string.IsNullOrEmpty(Class)) builder.AddAttribute(seq++, "Class", Class);
            if (!string.IsNullOrEmpty(Style)) builder.AddAttribute(seq++, "Style", Style);
            if (HasError) builder.AddAttribute(seq++, "HasError", HasError);
            if (!string.IsNullOrEmpty(ErrorMessage)) builder.AddAttribute(seq++, "ErrorMessage", ErrorMessage);
            if (MaxLength.HasValue) builder.AddAttribute(seq++, "MaxLength", MaxLength.Value);
            if (OnFocus.HasDelegate) builder.AddAttribute(seq++, "OnFocus", OnFocus);
            if (OnBlur.HasDelegate) builder.AddAttribute(seq++, "OnBlur", OnBlur);
            if (OnKeyPress.HasDelegate) builder.AddAttribute(seq++, "OnKeyPress", OnKeyPress);
            if (OnKeyDown.HasDelegate) builder.AddAttribute(seq++, "OnKeyDown", OnKeyDown);
            if (OnStartIconClick.HasDelegate) builder.AddAttribute(seq++, "OnStartIconClick", OnStartIconClick);
            if (OnEndIconClick.HasDelegate) builder.AddAttribute(seq++, "OnEndIconClick", OnEndIconClick);
        }
        
        #endregion
        
        #region Value Conversion
        
        private string? ConvertToString(object? value)
        {
            if (value == null) return null;
            
            return value switch
            {
                string str => str,
                DateTime dt => ConvertDateTimeToString(dt),
                DateTimeOffset dto => ConvertDateTimeToString(dto.DateTime),
                TimeRange tr => ConvertTimeRangeToString(tr),
                decimal dec => NumberFormat != null ? dec.ToString(NumberFormat) : dec.ToString(),
                double dbl => NumberFormat != null ? dbl.ToString(NumberFormat) : dbl.ToString(),
                float flt => NumberFormat != null ? flt.ToString(NumberFormat) : flt.ToString(),
                bool bl => bl.ToString(),
                _ => value.ToString()
            };
        }
        
        private string? ConvertDateTimeToString(object? value)
        {
            if (value == null) return null;
            
            return value switch
            {
                DateTime dt => DateFormat != null ? dt.ToString(DateFormat) : 
                              ShowTime ? dt.ToString("yyyy-MM-ddTHH:mm") : dt.ToString("yyyy-MM-dd"),
                DateTimeOffset dto => DateFormat != null ? dto.ToString(DateFormat) : 
                                     ShowTime ? dto.ToString("yyyy-MM-ddTHH:mm") : dto.ToString("yyyy-MM-dd"),
                _ => value.ToString()
            };
        }
        
        private string? ConvertTimeRangeToString(object? value)
        {
            if (value is TimeRange tr)
            {
                return tr.ToString();
            }
            return value?.ToString();
        }
        
        private bool ConvertToBool(object? value)
        {
            return value switch
            {
                bool b => b,
                string str => bool.TryParse(str, out var result) && result,
                _ => false
            };
        }
        
        #endregion
        
        #region Event Handlers
        
        private async Task OnTextValueChanged(string? newValue)
        {
            Value = newValue;
            await ValueChanged.InvokeAsync(Value);
        }
        
        private async Task OnNumericValueChanged(string? newValue)
        {
            if (string.IsNullOrEmpty(newValue))
            {
                Value = null;
            }
            else if (decimal.TryParse(newValue, out var decimalValue))
            {
                // Convert to original type if possible
                Value = Value switch
                {
                    int => (int)decimalValue,
                    long => (long)decimalValue,
                    double => (double)decimalValue,
                    float => (float)decimalValue,
                    _ => decimalValue
                };
            }
            else
            {
                Value = newValue; // Keep as string if conversion fails
            }
            
            await ValueChanged.InvokeAsync(Value);
        }
        
        private async Task OnDateTimeValueChanged(string? newValue)
        {
            if (string.IsNullOrEmpty(newValue))
            {
                Value = null;
            }
            else if (DateTime.TryParse(newValue, out var dateValue))
            {
                Value = Value switch
                {
                    DateTimeOffset => new DateTimeOffset(dateValue),
                    _ => dateValue
                };
            }
            else
            {
                Value = newValue; // Keep as string if conversion fails
            }
            
            await ValueChanged.InvokeAsync(Value);
        }
        
        private async Task OnTimeRangeValueChanged(string? newValue)
        {
            if (string.IsNullOrEmpty(newValue))
            {
                Value = null;
            }
            else if (TimeRange.TryParse(newValue, out var timeRange))
            {
                Value = timeRange;
            }
            else
            {
                Value = newValue; // Keep as string if conversion fails
            }
            
            await ValueChanged.InvokeAsync(Value);
        }
        
        private async Task OnBooleanValueChanged(bool newValue)
        {
            Value = newValue;
            await ValueChanged.InvokeAsync(Value);
        }
        
        #endregion
    }
}