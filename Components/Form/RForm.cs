using Microsoft.AspNetCore.Components;
using RR.Blazor.Enums;
using RR.Blazor.Models;
using RR.Blazor.Attributes;

namespace RR.Blazor.Components.Form;

// Base form component that all RForm variants inherit from
[Component("RForm", Category = "Form", Complexity = ComponentComplexity.Simple)]
[AIOptimized(Prompt = "Create smart form for any model type", 
             CommonUse = "any form scenario - automatically detects model type", 
             AvoidUsage = "None - this is the universal form component")]
public abstract class RFormBase : ComponentBase
{
    // Core form parameters - shared by all variants
    [Parameter] public ValidationMode ValidationMode { get; set; } = ValidationMode.DataAnnotations;
    [Parameter] public FormLayout Layout { get; set; } = FormLayout.Default;
    [Parameter] public FormDensity Density { get; set; } = FormDensity.Comfortable;
    [Parameter] public FormOptions Options { get; set; }
    
    // UI parameters
    [Parameter] public string Title { get; set; }
    [Parameter] public string Description { get; set; }
    [Parameter] public string Icon { get; set; }
    [Parameter] public bool ShowFormHeader { get; set; } = true;
    [Parameter] public bool ShowDefaultActions { get; set; } = true;
    [Parameter] public bool ShowCancelButton { get; set; } = true;
    [Parameter] public string SubmitText { get; set; } = "Submit";
    [Parameter] public string CancelText { get; set; } = "Cancel";
    [Parameter] public bool RequireValidation { get; set; } = true;
    [Parameter] public ButtonSize SubmitButtonSize { get; set; } = ButtonSize.Medium;
    [Parameter] public bool SubmitButtonFullWidth { get; set; } = false;
    
    // Content
    [Parameter] public RenderFragment ChildContent { get; set; }
    [Parameter] public RenderFragment FormFields { get; set; }
    [Parameter] public RenderFragment HeaderContent { get; set; }
    [Parameter] public RenderFragment FooterContent { get; set; }
    [Parameter] public string Class { get; set; }
    [Parameter] public string AdditionalClass { get; set; }
    
    // Events - Object-based for smart detection
    [Parameter] public EventCallback<object> OnValidSubmit { get; set; }
    [Parameter] public EventCallback<object> OnInvalidSubmit { get; set; }
    [Parameter] public EventCallback<object> OnStateChanged { get; set; }
    [Parameter] public EventCallback OnCancel { get; set; }
    [Parameter] public object CustomValidator { get; set; }
}

// Smart wrapper for automatic type detection
public class RForm : RFormBase
{
    [Parameter] public object Model { get; set; }
    
    protected override void BuildRenderTree(Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder builder)
    {
        if (Model == null) 
        {
            builder.AddContent(0, "Model is required for RForm component");
            return;
        }
        
        var modelType = Model.GetType();
        
        // Security check: Ensure we're working with a safe type
        if (modelType.IsPointer || modelType.IsByRef || modelType.IsGenericTypeDefinition)
        {
            builder.AddContent(0, "Invalid model type provided");
            return;
        }
        
        try
        {
            var genericFormType = typeof(RFormGeneric<>).MakeGenericType(modelType);
            
            builder.OpenComponent(0, genericFormType);
            
            // Forward all base properties
            ForwardBaseParameters(builder);
            
            // Forward model
            builder.AddAttribute(1, "Model", Model);
            
            builder.CloseComponent();
        }
        catch (Exception ex)
        {
            builder.AddContent(0, $"Error creating form for model type: {ex.Message}");
        }
    }
    
    private void ForwardBaseParameters(Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder builder)
    {
        var index = 2;
        
        // Forward all base parameters - only add non-null values
        builder.AddAttribute(index++, nameof(ValidationMode), ValidationMode);
        builder.AddAttribute(index++, nameof(Layout), Layout);
        builder.AddAttribute(index++, nameof(Density), Density);
        
        if (Options != null)
            builder.AddAttribute(index++, nameof(Options), Options);
        if (!string.IsNullOrEmpty(Title))
            builder.AddAttribute(index++, nameof(Title), Title);
        if (!string.IsNullOrEmpty(Description))
            builder.AddAttribute(index++, nameof(Description), Description);
        if (!string.IsNullOrEmpty(Icon))
            builder.AddAttribute(index++, nameof(Icon), Icon);
        
        builder.AddAttribute(index++, nameof(ShowFormHeader), ShowFormHeader);
        builder.AddAttribute(index++, nameof(ShowDefaultActions), ShowDefaultActions);
        builder.AddAttribute(index++, nameof(ShowCancelButton), ShowCancelButton);
        builder.AddAttribute(index++, nameof(SubmitText), SubmitText);
        builder.AddAttribute(index++, nameof(CancelText), CancelText);
        builder.AddAttribute(index++, nameof(RequireValidation), RequireValidation);
        builder.AddAttribute(index++, nameof(SubmitButtonSize), SubmitButtonSize);
        builder.AddAttribute(index++, nameof(SubmitButtonFullWidth), SubmitButtonFullWidth);
        
        if (!string.IsNullOrEmpty(Class))
            builder.AddAttribute(index++, nameof(Class), Class);
        if (!string.IsNullOrEmpty(AdditionalClass))
            builder.AddAttribute(index++, nameof(AdditionalClass), AdditionalClass);
        
        // Forward content - only add non-null values
        if (ChildContent != null)
            builder.AddAttribute(index++, nameof(ChildContent), ChildContent);
        if (FormFields != null)
            builder.AddAttribute(index++, nameof(FormFields), FormFields);
        if (HeaderContent != null)
            builder.AddAttribute(index++, nameof(HeaderContent), HeaderContent);
        if (FooterContent != null)
            builder.AddAttribute(index++, nameof(FooterContent), FooterContent);
            
        // Forward events using object-based parameters - only add if they have delegates
        if (OnValidSubmit.HasDelegate)
            builder.AddAttribute(index++, "OnValidSubmitObject", OnValidSubmit);
        if (OnInvalidSubmit.HasDelegate)
            builder.AddAttribute(index++, "OnInvalidSubmitObject", OnInvalidSubmit);
        if (OnStateChanged.HasDelegate)
            builder.AddAttribute(index++, "OnStateChangedObject", OnStateChanged);
        if (OnCancel.HasDelegate)
            builder.AddAttribute(index++, nameof(OnCancel), OnCancel);
        if (CustomValidator != null)
            builder.AddAttribute(index++, "CustomValidatorTyped", CustomValidator);
    }
}