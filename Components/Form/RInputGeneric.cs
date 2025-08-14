using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using RR.Blazor.Attributes;
using RR.Blazor.Enums;

namespace RR.Blazor.Components.Form
{
    /// <summary>
    /// Smart input component wrapper that automatically detects value type and renders appropriate input component
    /// </summary>
    [Component("RInputGeneric", Category = "Form", Complexity = ComponentComplexity.Advanced)]
    [AIOptimized(
        Prompt = "Smart input wrapper that automatically detects value type and renders appropriate component",
        CommonUse = "Forms with mixed data types, automatic type detection, payroll data entry",
        AvoidUsage = "When you need specific input behavior, use specialized components like RTextInput, RDateInput instead"
    )]
    public class RInputGeneric<T> : RInputBase
    {
        #region Core Parameters
        
        [Parameter]
        [AIParameter("Input value of any supported type", Example = "\"Hello World\" or 42 or DateTime.Now")]
        public T? Value { get; set; }
        
        [Parameter]
        [AIParameter("Value changed callback")]
        public EventCallback<T> ValueChanged { get; set; }
        
        [Parameter]
        [AIParameter("Explicit field type override", Example = "FieldType.Email")]
        public FieldType? InputType { get; set; }
        
        [Parameter]
        [AIParameter("Enable automatic type conversion")]
        public bool AutoConvert { get; set; } = true;
        
        #endregion
        
        #region Date-specific Parameters
        
        [Parameter]
        [AIParameter("Date format for display", Example = "yyyy-MM-dd")]
        public string DateFormat { get; set; } = "";
        
        [Parameter]
        [AIParameter("Minimum allowed date")]
        public DateTime? MinDate { get; set; }
        
        [Parameter]
        [AIParameter("Show time picker for date inputs")]
        public bool ShowTime { get; set; }
        
        #endregion
        
        #region Number-specific Parameters
        
        [Parameter]
        [AIParameter("Number format for display", Example = "N2")]
        public string NumberFormat { get; set; } = "";
        
        [Parameter]
        [AIParameter("Culture for number formatting")]
        public string Culture { get; set; } = "";
        
        #endregion
        
        #region Text-specific Parameters
        
        [Parameter]
        [AIParameter("Enable multiline text input")]
        public bool IsMultiLine { get; set; }
        
        #endregion
        
        #region Smart Type Detection
        
        /// <summary>
        /// Detect the appropriate component type based on value and parameters
        /// </summary>
        private ComponentType DetectComponentType()
        {
            // 1. Use explicit type if provided
            if (InputType.HasValue)
                return GetComponentFromFieldType(InputType.Value);
            
            // 2. Detect from value type
            if (Value != null)
            {
                return Value switch
                {
                    DateTime => ComponentType.Date,
                    DateTimeOffset => ComponentType.Date,
                    TimeSpan => ComponentType.Time,
                    int => ComponentType.Number,
                    long => ComponentType.Number,
                    decimal => ComponentType.Number,
                    double => ComponentType.Number,
                    float => ComponentType.Number,
                    bool => ComponentType.Checkbox,
                    string str when IsEmailFormat(str) => ComponentType.Email,
                    string str when IsUrlFormat(str) => ComponentType.Url,
                    string str when IsTelFormat(str) => ComponentType.Tel,
                    _ => ComponentType.Text
                };
            }
            
            // 3. Detect from label hints
            if (!string.IsNullOrEmpty(Label))
            {
                var labelLower = Label.ToLower();
                if (labelLower.Contains("email")) return ComponentType.Email;
                if (labelLower.Contains("password")) return ComponentType.Password;
                if (labelLower.Contains("phone") || labelLower.Contains("tel")) return ComponentType.Tel;
                if (labelLower.Contains("url") || labelLower.Contains("website")) return ComponentType.Url;
                if (labelLower.Contains("search")) return ComponentType.Search;
                if (labelLower.Contains("date") || labelLower.Contains("time")) return ComponentType.Date;
                if (labelLower.Contains("number") || labelLower.Contains("amount") || labelLower.Contains("price")) return ComponentType.Number;
            }
            
            return ComponentType.Text;
        }
        
        private ComponentType GetComponentFromFieldType(FieldType fieldType)
        {
            return fieldType switch
            {
                FieldType.Email => ComponentType.Email,
                FieldType.Password => ComponentType.Password,
                FieldType.Tel => ComponentType.Tel,
                FieldType.Url => ComponentType.Url,
                FieldType.Search => ComponentType.Search,
                FieldType.Number => ComponentType.Number,
                FieldType.Date => ComponentType.Date,
                FieldType.DateTime => ComponentType.Date,
                FieldType.Time => ComponentType.Time,
                FieldType.Checkbox => ComponentType.Checkbox,
                _ => ComponentType.Text
            };
        }
        
        private bool IsEmailFormat(string str) => str.Contains("@") && str.Contains(".");
        private bool IsUrlFormat(string str) => str.StartsWith("http://") || str.StartsWith("https://");
        private bool IsTelFormat(string str) => str.All(c => char.IsDigit(c) || c == '-' || c == '(' || c == ')' || c == ' ' || c == '+');
        
        #endregion
        
        #region Rendering
        
        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            var componentType = DetectComponentType();
            
            switch (componentType)
            {
                case ComponentType.Text:
                case ComponentType.Email:
                case ComponentType.Password:
                case ComponentType.Tel:
                case ComponentType.Url:
                case ComponentType.Search:
                case ComponentType.Number:
                    RenderTextInput(builder, GetFieldTypeFromComponent(componentType));
                    break;
                case ComponentType.Date:
                case ComponentType.Time:
                    RenderDateInput(builder);
                    break;
                case ComponentType.Checkbox:
                    RenderCheckboxInput(builder);
                    break;
                default:
                    RenderTextInput(builder, FieldType.Text);
                    break;
            }
        }
        
        private void RenderTextInput(RenderTreeBuilder builder, FieldType fieldType)
        {
            builder.OpenComponent<RTextInput>(0);
            
            // Value binding with conversion
            builder.AddAttribute(1, "Value", ConvertToString(Value));
            builder.AddAttribute(2, "ValueChanged", EventCallback.Factory.Create<string>(this, OnTextValueChanged));
            
            // Type and field-specific parameters
            builder.AddAttribute(3, "Type", fieldType);
            
            // Forward all base parameters
            ForwardBaseParameters(builder, 10);
            
            builder.CloseComponent();
        }
        
        private void RenderDateInput(RenderTreeBuilder builder)
        {
            // For now, use RTextInput with date type - later can be RDateInput when available
            builder.OpenComponent<RTextInput>(0);
            
            builder.AddAttribute(1, "Value", ConvertDateTimeToString(Value));
            builder.AddAttribute(2, "ValueChanged", EventCallback.Factory.Create<string>(this, OnDateValueChanged));
            builder.AddAttribute(3, "Type", FieldType.Date);
            
            ForwardBaseParameters(builder, 10);
            
            builder.CloseComponent();
        }
        
        private void RenderCheckboxInput(RenderTreeBuilder builder)
        {
            // For now, use RTextInput - later can be RCheckbox when needed
            builder.OpenComponent<RTextInput>(0);
            
            builder.AddAttribute(1, "Value", ConvertBoolToString(Value));
            builder.AddAttribute(2, "ValueChanged", EventCallback.Factory.Create<string>(this, OnBoolValueChanged));
            builder.AddAttribute(3, "Type", FieldType.Checkbox);
            
            ForwardBaseParameters(builder, 10);
            
            builder.CloseComponent();
        }
        
        private void ForwardBaseParameters(RenderTreeBuilder builder, int startSequence)
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
            if (Size != SizeType.Medium) builder.AddAttribute(seq++, "Size", Size);
            if (Density != DensityType.Normal) builder.AddAttribute(seq++, "Density", Density);
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
            
            // Forward specialized parameters
            if (!string.IsNullOrEmpty(DateFormat)) builder.AddAttribute(seq++, "DateFormat", DateFormat);
            if (MinDate.HasValue) builder.AddAttribute(seq++, "MinDate", MinDate.Value);
            if (ShowTime) builder.AddAttribute(seq++, "ShowTime", ShowTime);
            if (!string.IsNullOrEmpty(NumberFormat)) builder.AddAttribute(seq++, "NumberFormat", NumberFormat);
            if (!string.IsNullOrEmpty(Culture)) builder.AddAttribute(seq++, "Culture", Culture);
            if (IsMultiLine) builder.AddAttribute(seq++, "IsMultiLine", IsMultiLine);
        }
        
        private FieldType GetFieldTypeFromComponent(ComponentType componentType)
        {
            return componentType switch
            {
                ComponentType.Email => FieldType.Email,
                ComponentType.Password => FieldType.Password,
                ComponentType.Tel => FieldType.Tel,
                ComponentType.Url => FieldType.Url,
                ComponentType.Search => FieldType.Search,
                ComponentType.Number => FieldType.Number,
                _ => FieldType.Text
            };
        }
        
        #endregion
        
        #region Value Conversion
        
        private string? ConvertToString(T? value)
        {
            if (value == null) return null;
            
            return value switch
            {
                string str => str,
                DateTime dt => dt.ToString("yyyy-MM-dd"),
                DateTimeOffset dto => dto.ToString("yyyy-MM-dd"),
                TimeSpan ts => ts.ToString(@"hh\:mm"),
                decimal dec => dec.ToString(),
                double dbl => dbl.ToString(),
                float flt => flt.ToString(),
                int i => i.ToString(),
                long l => l.ToString(),
                bool b => b.ToString(),
                _ => value.ToString()
            };
        }
        
        private string? ConvertDateTimeToString(T? value)
        {
            if (value == null) return null;
            
            return value switch
            {
                DateTime dt => dt.ToString("yyyy-MM-dd"),
                DateTimeOffset dto => dto.ToString("yyyy-MM-dd"),
                string str => str,
                _ => value.ToString()
            };
        }
        
        private string? ConvertBoolToString(T? value)
        {
            if (value == null) return null;
            
            return value switch
            {
                bool b => b.ToString().ToLower(),
                string str => str,
                _ => value.ToString()
            };
        }
        
        #endregion
        
        #region Type Conversion
        
        private T? ConvertToType(string? value)
        {
            if (string.IsNullOrEmpty(value))
                return default(T);
                
            var targetType = typeof(T);
            var nullableType = Nullable.GetUnderlyingType(targetType);
            var actualType = nullableType ?? targetType;
            
            try
            {
                if (actualType == typeof(string))
                    return (T)(object)value;
                    
                if (actualType == typeof(DateTime))
                {
                    if (DateTime.TryParse(value, out var dateValue))
                        return (T)(object)dateValue;
                    return default(T);
                }
                
                if (actualType == typeof(DateTimeOffset))
                {
                    if (DateTimeOffset.TryParse(value, out var dateOffsetValue))
                        return (T)(object)dateOffsetValue;
                    return default(T);
                }
                
                if (actualType == typeof(TimeSpan))
                {
                    if (TimeSpan.TryParse(value, out var timeValue))
                        return (T)(object)timeValue;
                    return default(T);
                }
                
                if (actualType == typeof(bool))
                {
                    if (bool.TryParse(value, out var boolValue))
                        return (T)(object)boolValue;
                    return default(T);
                }
                
                if (actualType == typeof(int))
                {
                    if (int.TryParse(value, out var intValue))
                        return (T)(object)intValue;
                    return default(T);
                }
                
                if (actualType == typeof(long))
                {
                    if (long.TryParse(value, out var longValue))
                        return (T)(object)longValue;
                    return default(T);
                }
                
                if (actualType == typeof(decimal))
                {
                    if (decimal.TryParse(value, out var decimalValue))
                        return (T)(object)decimalValue;
                    return default(T);
                }
                
                if (actualType == typeof(double))
                {
                    if (double.TryParse(value, out var doubleValue))
                        return (T)(object)doubleValue;
                    return default(T);
                }
                
                if (actualType == typeof(float))
                {
                    if (float.TryParse(value, out var floatValue))
                        return (T)(object)floatValue;
                    return default(T);
                }
                
                // Fallback for other types
                return (T)Convert.ChangeType(value, actualType);
            }
            catch
            {
                return default(T);
            }
        }
        
        #endregion
        
        #region Event Handlers
        
        private async Task OnTextValueChanged(string? newValue)
        {
            Value = ConvertToType(newValue);
            await ValueChanged.InvokeAsync(Value);
        }
        
        private async Task OnDateValueChanged(string? newValue)
        {
            Value = ConvertToType(newValue);
            await ValueChanged.InvokeAsync(Value);
        }
        
        private async Task OnBoolValueChanged(string? newValue)
        {
            Value = ConvertToType(newValue);
            await ValueChanged.InvokeAsync(Value);
        }
        
        #endregion
        
        #region Component Types
        
        private enum ComponentType
        {
            Text,
            Email,
            Password,
            Tel,
            Url,
            Search,
            Number,
            Date,
            Time,
            Checkbox
        }
        
        #endregion
    }
}