using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using RR.Blazor.Attributes;
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
        [AIParameter("Enable automatic type conversion")]
        public bool AutoConvert { get; set; } = true;

        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            if (Value == null)
            {
                // Default to string type when value is null
                RenderGenericInput<string>(builder);
                return;
            }

            var valueType = Value.GetType();
            
            // Handle nullable types
            var nullableType = Nullable.GetUnderlyingType(valueType);
            var actualType = nullableType ?? valueType;
            
            // Create appropriate generic RInput based on type
            if (actualType == typeof(string))
                RenderGenericInput<string>(builder);
            else if (actualType == typeof(DateTime))
                RenderGenericInput<DateTime>(builder);
            else if (actualType == typeof(DateTimeOffset))
                RenderGenericInput<DateTimeOffset>(builder);
            else if (actualType == typeof(TimeSpan))
                RenderGenericInput<TimeSpan>(builder);
            else if (actualType == typeof(bool))
                RenderGenericInput<bool>(builder);
            else if (actualType == typeof(int))
                RenderGenericInput<int>(builder);
            else if (actualType == typeof(long))
                RenderGenericInput<long>(builder);
            else if (actualType == typeof(decimal))
                RenderGenericInput<decimal>(builder);
            else if (actualType == typeof(double))
                RenderGenericInput<double>(builder);
            else if (actualType == typeof(float))
                RenderGenericInput<float>(builder);
            else
                // Fallback to string for unknown types
                RenderGenericInput<string>(builder);
        }
        
        private void RenderGenericInput<T>(RenderTreeBuilder builder)
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
            
            // Forward all base parameters
            ForwardBaseParameters(builder, 10);
            
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
        
        private void ForwardBaseParameters(RenderTreeBuilder builder, int startSequence)
        {
            var seq = startSequence;
            
            if (InputType != FieldType.Text) builder.AddAttribute(seq++, "InputType", InputType);
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
    }
}