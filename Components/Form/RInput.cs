using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using RR.Blazor.Attributes;
using RR.Blazor.Enums;
using RR.Blazor.Models;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace RR.Blazor.Components.Form
{
    /// <summary>
    /// Smart input component with automatic type detection and conversion.
    /// Supports string, numeric, DateTime, TimeRange, and other value types.
    /// </summary>
    [Component("RInput", Category = "Form", Complexity = ComponentComplexity.Advanced)]
    [AIOptimized(
        Prompt = "Use for any input field with automatic type detection and conversion",
        CommonUse = "Form inputs with automatic type inference, mixed-type forms, dynamic forms",
        AvoidUsage = "When you need explicit control over input type (use RTextInput, RDatePicker, etc.)"
    )]
    public class RInput : RInputBase
    {
        #region Smart Parameters
        
        [Parameter]
        [AIParameter("Input value of any supported type (string, int, decimal, DateTime, TimeRange, etc.)", Example = "\"Hello World\" or 42 or DateTime.Now")]
        public object? Value { get; set; }
        
        [Parameter]
        [AIParameter("Callback when value changes")]
        public EventCallback<object> ValueChanged { get; set; }
        
        [Parameter]
        [AIParameter("Force specific input type instead of auto-detection")]
        public FieldType? InputType { get; set; }
        
        [Parameter]
        [AIParameter("Auto-convert between related formats (e.g., DateTime ↔ Unix ↔ String)")]
        public bool AutoConvert { get; set; } = true;
        
        [Parameter]
        [AIParameter("Date format for DateTime conversion", Example = "\"yyyy-MM-dd\"")]
        public string? DateFormat { get; set; } = "yyyy-MM-dd";
        
        [Parameter]
        [AIParameter("Number format for numeric conversion", Example = "\"F2\"")]
        public string? NumberFormat { get; set; }
        
        [Parameter]
        [AIParameter("Culture for formatting conversions")]
        public CultureInfo? Culture { get; set; }
        
        #endregion
        
        #region Numeric-Specific Parameters
        
        [Parameter]
        [AIParameter("Minimum value for numeric inputs")]
        public decimal? Min { get; set; }
        
        [Parameter]
        [AIParameter("Maximum value for numeric inputs")]
        public decimal? Max { get; set; }
        
        [Parameter]
        [AIParameter("Step value for numeric inputs")]
        public decimal? Step { get; set; }
        
        #endregion
        
        #region DateTime-Specific Parameters
        
        [Parameter]
        [AIParameter("Minimum date for DateTime inputs")]
        public DateTime? MinDate { get; set; }
        
        [Parameter]
        [AIParameter("Maximum date for DateTime inputs")]
        public DateTime? MaxDate { get; set; }
        
        [Parameter]
        [AIParameter("Show time picker for DateTime inputs")]
        public bool ShowTime { get; set; }
        
        [Parameter]
        [AIParameter("Use 24-hour format for time display")]
        public bool Use24HourFormat { get; set; } = true;
        
        #endregion
        
        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            var detectedType = DetectInputType();
            
            switch (detectedType)
            {
                case FieldType.Text:
                case FieldType.Email:
                case FieldType.Password:
                case FieldType.Tel:
                case FieldType.Url:
                case FieldType.Search:
                    RenderStringInput(builder, detectedType);
                    break;
                    
                case FieldType.Number:
                    RenderNumericInput(builder);
                    break;
                    
                case FieldType.Date:
                case FieldType.DateTime:
                case FieldType.Time:
                    RenderDateTimeInput(builder, detectedType);
                    break;
                    
                case FieldType.Custom:
                    RenderCustomInput(builder);
                    break;
                    
                default:
                    RenderStringInput(builder, FieldType.Text);
                    break;
            }
        }
        
        private FieldType DetectInputType()
        {
            // Explicit type override
            if (InputType.HasValue)
                return InputType.Value;
                
            // Auto-detect from value type
            if (Value != null)
            {
                var valueType = Value.GetType();
                
                // Handle nullable types
                if (valueType.IsGenericType && valueType.GetGenericTypeDefinition() == typeof(Nullable<>))
                {
                    valueType = Nullable.GetUnderlyingType(valueType) ?? valueType;
                }
                
                return valueType switch
                {
                    Type t when t == typeof(string) => DetectStringType((string)Value),
                    Type t when t == typeof(int) || t == typeof(long) || t == typeof(short) || t == typeof(byte) => FieldType.Number,
                    Type t when t == typeof(decimal) || t == typeof(double) || t == typeof(float) => FieldType.Number,
                    Type t when t == typeof(DateTime) => ShowTime ? FieldType.DateTime : FieldType.Date,
                    Type t when t == typeof(TimeSpan) => FieldType.Time,
                    Type t when t == typeof(TimeRange) => FieldType.Custom,
                    _ => FieldType.Text
                };
            }
            
            // Default to text for null values
            return FieldType.Text;
        }
        
        private FieldType DetectStringType(string value)
        {
            if (string.IsNullOrEmpty(value))
                return FieldType.Text;
                
            // Email detection
            if (value.Contains('@') && value.Contains('.'))
                return FieldType.Email;
                
            // URL detection
            if (value.StartsWith("http://") || value.StartsWith("https://"))
                return FieldType.Url;
                
            // Phone number detection (basic)
            if (value.Replace(" ", "").Replace("-", "").Replace("(", "").Replace(")", "").All(char.IsDigit))
                return FieldType.Tel;
                
            // Date detection
            if (DateTime.TryParse(value, out _))
                return FieldType.Date;
                
            // Number detection
            if (decimal.TryParse(value, out _))
                return FieldType.Number;
                
            return FieldType.Text;
        }
        
        private void RenderStringInput(RenderTreeBuilder builder, FieldType fieldType)
        {
            builder.OpenComponent<RTextInput>(0);
            
            // Forward all base parameters
            ForwardBaseParameters(builder);
            
            // String-specific parameters
            builder.AddAttribute(1, "Value", ConvertToString(Value));
            builder.AddAttribute(2, "ValueChanged", EventCallback.Factory.Create<string>(this, HandleStringValueChanged));
            builder.AddAttribute(3, "Type", fieldType);
            
            if (MaxLength.HasValue)
                builder.AddAttribute(4, "maxlength", MaxLength.Value);
                
            builder.CloseComponent();
        }
        
        private void RenderNumericInput(RenderTreeBuilder builder)
        {
            builder.OpenComponent<RTextInput>(0);
            
            // Forward all base parameters
            ForwardBaseParameters(builder);
            
            // Numeric-specific parameters
            builder.AddAttribute(1, "Value", ConvertToString(Value));
            builder.AddAttribute(2, "ValueChanged", EventCallback.Factory.Create<string>(this, HandleNumericValueChanged));
            builder.AddAttribute(3, "Type", FieldType.Number);
            
            if (Min.HasValue)
                builder.AddAttribute(4, "Min", Min.Value);
            if (Max.HasValue)
                builder.AddAttribute(5, "Max", Max.Value);
            if (Step.HasValue)
                builder.AddAttribute(6, "Step", Step.Value);
                
            builder.CloseComponent();
        }
        
        private void RenderDateTimeInput(RenderTreeBuilder builder, FieldType fieldType)
        {
            builder.OpenComponent<RDatePicker>(0);
            
            // Forward all base parameters
            ForwardBaseParameters(builder);
            
            // DateTime-specific parameters
            builder.AddAttribute(1, "Value", ConvertToDateTime(Value));
            builder.AddAttribute(2, "ValueChanged", EventCallback.Factory.Create<DateTime?>(this, HandleDateTimeValueChanged));
            builder.AddAttribute(3, "ShowTime", ShowTime || fieldType == FieldType.DateTime);
            builder.AddAttribute(4, "Use24HourFormat", Use24HourFormat);
            
            if (MinDate.HasValue)
                builder.AddAttribute(5, "MinDate", MinDate.Value);
            if (MaxDate.HasValue)
                builder.AddAttribute(6, "MaxDate", MaxDate.Value);
            if (!string.IsNullOrEmpty(DateFormat))
                builder.AddAttribute(7, "Format", DateFormat);
                
            builder.CloseComponent();
        }
        
        private void RenderCustomInput(RenderTreeBuilder builder)
        {
            // Handle TimeRange and other custom types
            if (Value is TimeRange timeRange)
            {
                RenderTimeRangeInput(builder, timeRange);
            }
            else
            {
                // Fallback to string input for unknown custom types
                RenderStringInput(builder, FieldType.Text);
            }
        }
        
        private void RenderTimeRangeInput(RenderTreeBuilder builder, TimeRange timeRange)
        {
            // For now, render as text input with time range format
            builder.OpenComponent<RTextInput>(0);
            
            ForwardBaseParameters(builder);
            
            builder.AddAttribute(1, "Value", timeRange.ToTimeString());
            builder.AddAttribute(2, "ValueChanged", EventCallback.Factory.Create<string>(this, HandleTimeRangeValueChanged));
            builder.AddAttribute(3, "Type", FieldType.Text);
            builder.AddAttribute(4, "Placeholder", Placeholder ?? "HH:mm-HH:mm");
            
            builder.CloseComponent();
        }
        
        private void ForwardBaseParameters(RenderTreeBuilder builder)
        {
            var sequence = 100;
            
            if (!string.IsNullOrEmpty(Label))
                builder.AddAttribute(sequence++, "Label", Label);
            if (!string.IsNullOrEmpty(Placeholder))
                builder.AddAttribute(sequence++, "Placeholder", Placeholder);
            if (!string.IsNullOrEmpty(HelpText))
                builder.AddAttribute(sequence++, "HelpText", HelpText);
            if (!string.IsNullOrEmpty(FieldName))
                builder.AddAttribute(sequence++, "FieldName", FieldName);
            if (Required)
                builder.AddAttribute(sequence++, "Required", Required);
            if (Disabled)
                builder.AddAttribute(sequence++, "Disabled", Disabled);
            if (ReadOnly)
                builder.AddAttribute(sequence++, "ReadOnly", ReadOnly);
            if (Loading)
                builder.AddAttribute(sequence++, "Loading", Loading);
            if (HasError)
                builder.AddAttribute(sequence++, "HasError", HasError);
            if (!string.IsNullOrEmpty(ErrorMessage))
                builder.AddAttribute(sequence++, "ErrorMessage", ErrorMessage);
            if (!string.IsNullOrEmpty(StartIcon))
                builder.AddAttribute(sequence++, "StartIcon", StartIcon);
            if (!string.IsNullOrEmpty(EndIcon))
                builder.AddAttribute(sequence++, "EndIcon", EndIcon);
            if (Variant != TextInputVariant.Default)
                builder.AddAttribute(sequence++, "Variant", Variant);
            if (Size != TextInputSize.Medium)
                builder.AddAttribute(sequence++, "Size", Size);
            if (Density != ComponentDensity.Normal)
                builder.AddAttribute(sequence++, "Density", Density);
            if (!string.IsNullOrEmpty(Class))
                builder.AddAttribute(sequence++, "Class", Class);
            if (!string.IsNullOrEmpty(Style))
                builder.AddAttribute(sequence++, "Style", Style);
                
            // Forward events
            if (OnFocus.HasDelegate)
                builder.AddAttribute(sequence++, "OnFocus", OnFocus);
            if (OnBlur.HasDelegate)
                builder.AddAttribute(sequence++, "OnBlur", OnBlur);
            if (OnKeyPress.HasDelegate)
                builder.AddAttribute(sequence++, "OnKeyPress", OnKeyPress);
            if (OnKeyDown.HasDelegate)
                builder.AddAttribute(sequence++, "OnKeyDown", OnKeyDown);
            if (OnStartIconClick.HasDelegate)
                builder.AddAttribute(sequence++, "OnStartIconClick", OnStartIconClick);
            if (OnEndIconClick.HasDelegate)
                builder.AddAttribute(sequence++, "OnEndIconClick", OnEndIconClick);
        }
        
        #region Value Conversion Methods
        
        private string ConvertToString(object? value)
        {
            if (value == null)
                return "";
                
            var culture = Culture ?? CultureInfo.CurrentCulture;
            
            return value switch
            {
                string s => s,
                DateTime dt => dt.ToString(DateFormat ?? "yyyy-MM-dd", culture),
                decimal d => d.ToString(NumberFormat ?? "F2", culture),
                double d => d.ToString(NumberFormat ?? "F2", culture),
                float f => f.ToString(NumberFormat ?? "F2", culture),
                int i => i.ToString(culture),
                long l => l.ToString(culture),
                TimeRange tr => tr.ToTimeString(),
                _ => value.ToString() ?? ""
            };
        }
        
        private DateTime? ConvertToDateTime(object? value)
        {
            if (value == null)
                return null;
                
            return value switch
            {
                DateTime dt => dt,
                string s when DateTime.TryParse(s, out var parsed) => parsed,
                long unix => DateTimeOffset.FromUnixTimeSeconds(unix).DateTime,
                _ => null
            };
        }
        
        private object? ConvertFromString(string value, Type targetType)
        {
            if (string.IsNullOrEmpty(value))
                return null;
                
            var culture = Culture ?? CultureInfo.CurrentCulture;
            
            try
            {
                if (targetType == typeof(string))
                    return value;
                    
                if (targetType == typeof(int) || targetType == typeof(int?))
                    return int.Parse(value, culture);
                    
                if (targetType == typeof(long) || targetType == typeof(long?))
                    return long.Parse(value, culture);
                    
                if (targetType == typeof(decimal) || targetType == typeof(decimal?))
                    return decimal.Parse(value, culture);
                    
                if (targetType == typeof(double) || targetType == typeof(double?))
                    return double.Parse(value, culture);
                    
                if (targetType == typeof(float) || targetType == typeof(float?))
                    return float.Parse(value, culture);
                    
                if (targetType == typeof(DateTime) || targetType == typeof(DateTime?))
                    return DateTime.Parse(value, culture);
                    
                if (targetType == typeof(TimeRange))
                {
                    if (TimeRange.TryParse(value, out var timeRange))
                        return timeRange;
                }
                
                return value;
            }
            catch
            {
                return value;
            }
        }
        
        #endregion
        
        #region Event Handlers
        
        private async Task HandleStringValueChanged(string newValue)
        {
            var convertedValue = AutoConvert && Value != null 
                ? ConvertFromString(newValue, Value.GetType()) 
                : newValue;
                
            Value = convertedValue;
            await ValueChanged.InvokeAsync(convertedValue);
        }
        
        private async Task HandleNumericValueChanged(string newValue)
        {
            var targetType = Value?.GetType() ?? typeof(decimal);
            var convertedValue = ConvertFromString(newValue, targetType);
            
            Value = convertedValue;
            await ValueChanged.InvokeAsync(convertedValue);
        }
        
        private async Task HandleDateTimeValueChanged(DateTime? newValue)
        {
            object? convertedValue = newValue;
            
            if (AutoConvert && Value != null)
            {
                var originalType = Value.GetType();
                if (originalType == typeof(long) || originalType == typeof(long?))
                {
                    convertedValue = newValue.HasValue 
                        ? ((DateTimeOffset)newValue.Value).ToUnixTimeSeconds() 
                        : (long?)null;
                }
                else if (originalType == typeof(string))
                {
                    convertedValue = newValue?.ToString(DateFormat ?? "yyyy-MM-dd");
                }
            }
            
            Value = convertedValue;
            await ValueChanged.InvokeAsync(convertedValue);
        }
        
        private async Task HandleTimeRangeValueChanged(string newValue)
        {
            if (TimeRange.TryParse(newValue, out var timeRange))
            {
                Value = timeRange;
                await ValueChanged.InvokeAsync(timeRange);
            }
            else
            {
                Value = newValue;
                await ValueChanged.InvokeAsync(newValue);
            }
        }
        
        #endregion
    }
}