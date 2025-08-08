using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using RR.Blazor.Attributes;
using RR.Blazor.Components.Base;
using RR.Blazor.Enums;

namespace RR.Blazor.Components.Form
{
    /// <summary>
    /// Non-generic smart wrapper for RInput that auto-detects types and creates appropriate generic instance
    /// </summary>
    [Component("RInput", Category = "Form", Complexity = ComponentComplexity.Advanced)]
    [AIOptimized(
        Prompt = "Smart input wrapper with automatic type detection - use when you want automatic type inference",
        CommonUse = "Forms with mixed data types, when you don't want to specify generic types",
        AvoidUsage = "When you need explicit type control, use RInput<T> directly"
    )]
    public class RInput : RInputBase
    {
        [Parameter]
        [AIParameter("Input value of any supported type", Example = "\"Hello World\" or 42 or DateTime.Now")]
        public object Value { get; set; }
        
        [Parameter]
        [AIParameter("Value changed callback")]
        public EventCallback<object> ValueChanged { get; set; }
        
        [Parameter]
        [AIParameter("Explicit field type override", Example = "FieldType.Email")]
        public FieldType InputType { get; set; }
        
        [Parameter]
        [AIParameter("Field type (alias for InputType)", Example = "FieldType.Email")]
        public FieldType Type { get; set; }
        
        [Parameter]
        [AIParameter("Enable automatic type conversion")]
        public bool AutoConvert { get; set; } = true;
        
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
        

        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            // Resolve effective input type - Type parameter takes precedence over InputType
            var effectiveInputType = Type != FieldType.Text ? Type : InputType;
            
            if (Value == null)
            {
                // Default to string type when value is null
                RenderGenericInput<string>(builder, effectiveInputType);
                return;
            }

            var valueType = Value.GetType();
            
            // Handle nullable types
            var nullableType = Nullable.GetUnderlyingType(valueType);
            var actualType = nullableType ?? valueType;
            
            // Create appropriate generic RInput based on type
            if (actualType == typeof(string))
                RenderGenericInput<string>(builder, effectiveInputType);
            else if (actualType == typeof(DateTime))
                RenderGenericInput<DateTime>(builder, effectiveInputType);
            else if (actualType == typeof(DateTimeOffset))
                RenderGenericInput<DateTimeOffset>(builder, effectiveInputType);
            else if (actualType == typeof(TimeSpan))
                RenderGenericInput<TimeSpan>(builder, effectiveInputType);
            else if (actualType == typeof(bool))
                RenderGenericInput<bool>(builder, effectiveInputType);
            else if (actualType == typeof(int))
                RenderGenericInput<int>(builder, effectiveInputType);
            else if (actualType == typeof(long))
                RenderGenericInput<long>(builder, effectiveInputType);
            else if (actualType == typeof(decimal))
                RenderGenericInput<decimal>(builder, effectiveInputType);
            else if (actualType == typeof(double))
                RenderGenericInput<double>(builder, effectiveInputType);
            else if (actualType == typeof(float))
                RenderGenericInput<float>(builder, effectiveInputType);
            else
                // Fallback to string for unknown types
                RenderGenericInput<string>(builder, effectiveInputType);
        }
        
        private void RenderGenericInput<T>(RenderTreeBuilder builder, FieldType effectiveInputType = FieldType.Text)
        {
            var genericInputType = typeof(RInputGeneric<>).MakeGenericType(typeof(T));
            
            builder.OpenComponent(0, genericInputType);
            
            // Forward value with proper type conversion
            if (Value != null && Value is T typedValue)
                builder.AddAttribute(1, "Value", typedValue);
            else if (Value != null)
                builder.AddAttribute(1, "Value", ConvertValue<T>(Value));
            else
                builder.AddAttribute(1, "Value", default(T));
            
            // Forward value changed callback with type conversion
            builder.AddAttribute(2, "ValueChanged", EventCallback.Factory.Create<T>(this, OnTypedValueChanged<T>));
            
            // Forward InputType - prefer the effective type resolved from Type parameter
            if (effectiveInputType != FieldType.Text)
                builder.AddAttribute(3, "InputType", effectiveInputType);
            else if (InputType != FieldType.Text)
                builder.AddAttribute(3, "InputType", InputType);
            
            // Forward AutoConvert
            builder.AddAttribute(4, "AutoConvert", AutoConvert);
            
            // Forward date-specific parameters if relevant
            if (!string.IsNullOrEmpty(DateFormat))
                builder.AddAttribute(5, "DateFormat", DateFormat);
            if (MinDate.HasValue)
                builder.AddAttribute(6, "MinDate", MinDate.Value);
            if (ShowTime)
                builder.AddAttribute(7, "ShowTime", ShowTime);
            
            // Forward number-specific parameters if relevant
            if (!string.IsNullOrEmpty(NumberFormat))
                builder.AddAttribute(8, "NumberFormat", NumberFormat);
            if (!string.IsNullOrEmpty(Culture))
                builder.AddAttribute(9, "Culture", Culture);
            
            // Forward text-specific parameters
            if (IsMultiLine)
                builder.AddAttribute(10, "IsMultiLine", IsMultiLine);
            
            // Forward all remaining base parameters using RAttributeForwarder
            var seq = 20;
            builder.ForwardParameters(ref seq, this, "Value", "ValueChanged", "InputType", "Type", "AutoConvert", 
                "DateFormat", "MinDate", "ShowTime", "NumberFormat", "Culture", "IsMultiLine");
            
            builder.CloseComponent();
        }
        
        private T ConvertValue<T>(object value)
        {
            if (value == null) return default(T);
            
            try
            {
                var targetType = typeof(T);
                var nullableType = Nullable.GetUnderlyingType(targetType);
                var actualType = nullableType ?? targetType;
                
                if (actualType == typeof(string))
                    return (T)(object)value.ToString();
                
                return (T)Convert.ChangeType(value, actualType);
            }
            catch
            {
                return default(T);
            }
        }
        
        private async Task OnTypedValueChanged<T>(T newValue)
        {
            Value = newValue;
            await ValueChanged.InvokeAsync(Value);
        }
        
    }
}