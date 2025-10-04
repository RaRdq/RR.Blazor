using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using RR.Blazor.Enums;

namespace RR.Blazor.Templates.Progress;

/// <summary>
/// Renders progress templates using HTML output
/// Supports linear, circular, ring, steps, and multi-segment progress
/// </summary>
/// <typeparam name="T">Type of data being rendered</typeparam>
public class ProgressRenderer<T> where T : class
{
    /// <summary>
    /// Renders the progress template
    /// </summary>
    public RenderFragment Render(ProgressContext<T> context)
    {
        return builder =>
        {
            if (context?.Item == null) return;
            
            builder.OpenElement(0, "div");
            builder.AddAttribute(1, "class", $"progress-container {context.Class}".Trim());
            builder.AddAttribute(2, "data-template", "progress");
            builder.AddAttribute(3, "data-type", context.Type.ToString().ToLower());
            
            if (context.Disabled)
                builder.AddAttribute(4, "disabled", true);
            
            // Render based on type
            switch (context.Type)
            {
                case ProgressType.Linear:
                    RenderLinearProgress(builder, context, 100);
                    break;
                case ProgressType.Circular:
                    RenderCircularProgress(builder, context, 200);
                    break;
                case ProgressType.Ring:
                    RenderRingProgress(builder, context, 300);
                    break;
                case ProgressType.Steps:
                    RenderStepProgress(builder, context, 400);
                    break;
                case ProgressType.MultiSegment:
                    RenderMultiSegmentProgress(builder, context, 500);
                    break;
            }
            
            builder.CloseElement(); // progress-container
        };
    }
    
    private static void RenderLinearProgress(RenderTreeBuilder builder, ProgressContext<T> context, int sequence)
    {
        // Label if present
        if (!string.IsNullOrEmpty(context.Label))
        {
            builder.OpenElement(sequence, "div");
            builder.AddAttribute(sequence + 1, "class", "progress-label mb-1");
            builder.AddContent(sequence + 2, context.Label);
            builder.CloseElement();
        }
        
        // Progress wrapper
        builder.OpenElement(sequence + 10, "div");
        builder.AddAttribute(sequence + 11, "class", "progress");
        
        if (context.Height > 0)
            builder.AddAttribute(sequence + 12, "style", $"height: {context.Height}px;");
        
        // Progress bar
        var progressClass = BuildProgressBarClass(context);
        var widthStyle = context.Indeterminate ? "width: 100%;" : $"width: {context.Percentage:0.##}%;";
        
        builder.OpenElement(sequence + 20, "div");
        builder.AddAttribute(sequence + 21, "class", progressClass);
        builder.AddAttribute(sequence + 22, "style", widthStyle);
        builder.AddAttribute(sequence + 23, "role", "progressbar");
        builder.AddAttribute(sequence + 24, "aria-valuenow", context.Value.ToString("0.##"));
        builder.AddAttribute(sequence + 25, "aria-valuemin", "0");
        builder.AddAttribute(sequence + 26, "aria-valuemax", context.Max.ToString("0.##"));
        
        // Progress text
        if ((context.ShowPercentage || context.ShowValue) && !context.Indeterminate)
        {
            var text = context.ShowValue 
                ? $"{context.Value:0.##}/{context.Max:0.##}"
                : $"{context.Percentage:0.#}%";
            builder.AddContent(sequence + 27, text);
        }
        
        builder.CloseElement(); // progress-bar
        builder.CloseElement(); // progress
    }
    
    private static void RenderCircularProgress(RenderTreeBuilder builder, ProgressContext<T> context, int sequence)
    {
        var radius = (context.Diameter - context.StrokeWidth) / 2;
        var circumference = 2 * Math.PI * radius;
        var strokeDashoffset = circumference - (context.Percentage / 100 * circumference);
        
        builder.OpenElement(sequence, "div");
        builder.AddAttribute(sequence + 1, "class", "progress-circular");
        builder.AddAttribute(sequence + 2, "style", $"width: {context.Diameter}px; height: {context.Diameter}px;");
        
        // SVG element
        builder.OpenElement(sequence + 10, "svg");
        builder.AddAttribute(sequence + 11, "width", context.Diameter);
        builder.AddAttribute(sequence + 12, "height", context.Diameter);
        builder.AddAttribute(sequence + 13, "viewBox", $"0 0 {context.Diameter} {context.Diameter}");
        
        // Background circle
        builder.OpenElement(sequence + 20, "circle");
        builder.AddAttribute(sequence + 21, "cx", context.Diameter / 2);
        builder.AddAttribute(sequence + 22, "cy", context.Diameter / 2);
        builder.AddAttribute(sequence + 23, "r", radius);
        builder.AddAttribute(sequence + 24, "fill", "none");
        builder.AddAttribute(sequence + 25, "stroke", "var(--bs-gray-300)");
        builder.AddAttribute(sequence + 26, "stroke-width", context.StrokeWidth);
        builder.CloseElement();
        
        // Progress circle
        if (!context.Indeterminate)
        {
            builder.OpenElement(sequence + 30, "circle");
            builder.AddAttribute(sequence + 31, "cx", context.Diameter / 2);
            builder.AddAttribute(sequence + 32, "cy", context.Diameter / 2);
            builder.AddAttribute(sequence + 33, "r", radius);
            builder.AddAttribute(sequence + 34, "fill", "none");
            builder.AddAttribute(sequence + 35, "stroke", GetVariantColor(context.Variant));
            builder.AddAttribute(sequence + 36, "stroke-width", context.StrokeWidth);
            builder.AddAttribute(sequence + 37, "stroke-dasharray", circumference);
            builder.AddAttribute(sequence + 38, "stroke-dashoffset", strokeDashoffset);
            builder.AddAttribute(sequence + 39, "stroke-linecap", "round");
            builder.AddAttribute(sequence + 40, "transform", $"rotate(-90 {context.Diameter / 2} {context.Diameter / 2})");
            builder.AddAttribute(sequence + 41, "class", context.Animated ? "progress-circular-animated" : "");
            builder.CloseElement();
        }
        else
        {
            // Indeterminate spinner
            builder.OpenElement(sequence + 50, "circle");
            builder.AddAttribute(sequence + 51, "cx", context.Diameter / 2);
            builder.AddAttribute(sequence + 52, "cy", context.Diameter / 2);
            builder.AddAttribute(sequence + 53, "r", radius);
            builder.AddAttribute(sequence + 54, "fill", "none");
            builder.AddAttribute(sequence + 55, "stroke", GetVariantColor(context.Variant));
            builder.AddAttribute(sequence + 56, "stroke-width", context.StrokeWidth);
            builder.AddAttribute(sequence + 57, "stroke-dasharray", $"{circumference * 0.25} {circumference * 0.75}");
            builder.AddAttribute(sequence + 58, "stroke-linecap", "round");
            builder.AddAttribute(sequence + 59, "class", "progress-circular-indeterminate");
            builder.CloseElement();
        }
        
        builder.CloseElement(); // svg
        
        // Center text
        if ((context.ShowPercentage || context.ShowValue || !string.IsNullOrEmpty(context.Label)) && !context.Indeterminate)
        {
            builder.OpenElement(sequence + 70, "div");
            builder.AddAttribute(sequence + 71, "class", "progress-circular-text");
            
            if (!string.IsNullOrEmpty(context.Label))
            {
                builder.AddContent(sequence + 72, context.Label);
            }
            else if (context.ShowValue)
            {
                builder.AddContent(sequence + 73, $"{context.Value:0.##}");
            }
            else if (context.ShowPercentage)
            {
                builder.AddContent(sequence + 74, $"{context.Percentage:0}%");
            }
            
            builder.CloseElement();
        }
        
        builder.CloseElement(); // progress-circular
    }
    
    private static void RenderRingProgress(RenderTreeBuilder builder, ProgressContext<T> context, int sequence)
    {
        // Ring progress is similar to circular but with larger stroke and different styling
        var radius = (context.Diameter - context.StrokeWidth * 2) / 2;
        var circumference = 2 * Math.PI * radius;
        var strokeDashoffset = circumference - (context.Percentage / 100 * circumference);
        
        builder.OpenElement(sequence, "div");
        builder.AddAttribute(sequence + 1, "class", "progress-ring");
        builder.AddAttribute(sequence + 2, "style", $"width: {context.Diameter}px; height: {context.Diameter}px;");
        
        // SVG element
        builder.OpenElement(sequence + 10, "svg");
        builder.AddAttribute(sequence + 11, "width", context.Diameter);
        builder.AddAttribute(sequence + 12, "height", context.Diameter);
        builder.AddAttribute(sequence + 13, "viewBox", $"0 0 {context.Diameter} {context.Diameter}");
        
        // Background ring
        builder.OpenElement(sequence + 20, "circle");
        builder.AddAttribute(sequence + 21, "cx", context.Diameter / 2);
        builder.AddAttribute(sequence + 22, "cy", context.Diameter / 2);
        builder.AddAttribute(sequence + 23, "r", radius);
        builder.AddAttribute(sequence + 24, "fill", "none");
        builder.AddAttribute(sequence + 25, "stroke", "var(--bs-gray-200)");
        builder.AddAttribute(sequence + 26, "stroke-width", context.StrokeWidth * 2);
        builder.CloseElement();
        
        // Progress ring
        builder.OpenElement(sequence + 30, "circle");
        builder.AddAttribute(sequence + 31, "cx", context.Diameter / 2);
        builder.AddAttribute(sequence + 32, "cy", context.Diameter / 2);
        builder.AddAttribute(sequence + 33, "r", radius);
        builder.AddAttribute(sequence + 34, "fill", "none");
        builder.AddAttribute(sequence + 35, "stroke", GetVariantColor(context.Variant));
        builder.AddAttribute(sequence + 36, "stroke-width", context.StrokeWidth * 2);
        builder.AddAttribute(sequence + 37, "stroke-dasharray", circumference);
        builder.AddAttribute(sequence + 38, "stroke-dashoffset", strokeDashoffset);
        builder.AddAttribute(sequence + 39, "stroke-linecap", "round");
        builder.AddAttribute(sequence + 40, "transform", $"rotate(-90 {context.Diameter / 2} {context.Diameter / 2})");
        builder.CloseElement();
        
        builder.CloseElement(); // svg
        
        // Center content
        builder.OpenElement(sequence + 50, "div");
        builder.AddAttribute(sequence + 51, "class", "progress-ring-content");
        
        if (context.ShowPercentage)
        {
            builder.OpenElement(sequence + 52, "div");
            builder.AddAttribute(sequence + 53, "class", "progress-ring-percentage");
            builder.AddContent(sequence + 54, $"{context.Percentage:0}%");
            builder.CloseElement();
        }
        
        if (!string.IsNullOrEmpty(context.Label))
        {
            builder.OpenElement(sequence + 55, "div");
            builder.AddAttribute(sequence + 56, "class", "progress-ring-label");
            builder.AddContent(sequence + 57, context.Label);
            builder.CloseElement();
        }
        
        builder.CloseElement(); // progress-ring-content
        builder.CloseElement(); // progress-ring
    }
    
    private static void RenderStepProgress(RenderTreeBuilder builder, ProgressContext<T> context, int sequence)
    {
        if (!context.Steps.Any()) return;
        
        builder.OpenElement(sequence, "div");
        builder.AddAttribute(sequence + 1, "class", "progress-steps");
        
        for (int i = 0; i < context.Steps.Count; i++)
        {
            var step = context.Steps[i];
            var isActive = i == context.CurrentStep;
            var isCompleted = i < context.CurrentStep || step.Status == StepStatus.Completed;
            var isError = step.Status == StepStatus.Error;
            
            // Step item
            builder.OpenElement(sequence + i * 100 + 10, "div");
            builder.AddAttribute(sequence + i * 100 + 11, "class", BuildStepClass(isActive, isCompleted, isError));
            
            // Step indicator
            builder.OpenElement(sequence + i * 100 + 20, "div");
            builder.AddAttribute(sequence + i * 100 + 21, "class", "step-indicator");
            
            if (!string.IsNullOrEmpty(step.Icon))
            {
                builder.OpenElement(sequence + i * 100 + 22, "i");
                builder.AddAttribute(sequence + i * 100 + 23, "class", "icon");
                builder.AddContent(sequence + i * 100 + 24, step.Icon);
                builder.CloseElement();
            }
            else if (isCompleted)
            {
                builder.OpenElement(sequence + i * 100 + 25, "i");
                builder.AddAttribute(sequence + i * 100 + 26, "class", "icon");
                builder.AddContent(sequence + i * 100 + 27, "check");
                builder.CloseElement();
            }
            else if (isError)
            {
                builder.OpenElement(sequence + i * 100 + 28, "i");
                builder.AddAttribute(sequence + i * 100 + 29, "class", "icon");
                builder.AddContent(sequence + i * 100 + 30, "close");
                builder.CloseElement();
            }
            else
            {
                builder.AddContent(sequence + i * 100 + 31, (i + 1).ToString());
            }
            
            builder.CloseElement(); // step-indicator
            
            // Step content
            builder.OpenElement(sequence + i * 100 + 40, "div");
            builder.AddAttribute(sequence + i * 100 + 41, "class", "step-content");
            
            builder.OpenElement(sequence + i * 100 + 42, "div");
            builder.AddAttribute(sequence + i * 100 + 43, "class", "step-title");
            builder.AddContent(sequence + i * 100 + 44, step.Title);
            builder.CloseElement();
            
            if (!string.IsNullOrEmpty(step.Description))
            {
                builder.OpenElement(sequence + i * 100 + 45, "div");
                builder.AddAttribute(sequence + i * 100 + 46, "class", "step-description");
                builder.AddContent(sequence + i * 100 + 47, step.Description);
                builder.CloseElement();
            }
            
            builder.CloseElement(); // step-content
            
            // Connector line (except for last step)
            if (i < context.Steps.Count - 1)
            {
                builder.OpenElement(sequence + i * 100 + 50, "div");
                builder.AddAttribute(sequence + i * 100 + 51, "class", isCompleted ? "step-connector completed" : "step-connector");
                builder.CloseElement();
            }
            
            builder.CloseElement(); // step-item
        }
        
        builder.CloseElement(); // progress-steps
    }
    
    private static void RenderMultiSegmentProgress(RenderTreeBuilder builder, ProgressContext<T> context, int sequence)
    {
        if (!context.Segments.Any())
        {
            // Fall back to linear progress if no segments
            RenderLinearProgress(builder, context, sequence);
            return;
        }
        
        var totalValue = context.Segments.Sum(s => s.Value);
        
        builder.OpenElement(sequence, "div");
        builder.AddAttribute(sequence + 1, "class", "progress progress-multi");
        
        if (context.Height > 0)
            builder.AddAttribute(sequence + 2, "style", $"height: {context.Height}px;");
        
        var currentSequence = sequence + 10;
        foreach (var segment in context.Segments)
        {
            var percentage = totalValue > 0 ? (segment.Value / totalValue) * 100 : 0;
            
            builder.OpenElement(currentSequence, "div");
            builder.AddAttribute(currentSequence + 1, "class", $"progress-bar bg-{segment.Variant.ToString().ToLower()}");
            builder.AddAttribute(currentSequence + 2, "style", $"width: {percentage:0.##}%;");
            builder.AddAttribute(currentSequence + 3, "role", "progressbar");
            
            if (!string.IsNullOrEmpty(segment.Tooltip))
            {
                builder.AddAttribute(currentSequence + 4, "title", segment.Tooltip);
                builder.AddAttribute(currentSequence + 5, "data-bs-toggle", "tooltip");
            }
            
            if (!string.IsNullOrEmpty(segment.Label) && percentage > 5) // Only show label if segment is wide enough
            {
                builder.AddContent(currentSequence + 6, segment.Label);
            }
            
            builder.CloseElement();
            currentSequence += 10;
        }
        
        builder.CloseElement(); // progress
    }
    
    private static string BuildProgressBarClass(ProgressContext<T> context)
    {
        var classes = new List<string> { "progress-bar" };
        
        classes.Add($"bg-{context.Variant.ToString().ToLower()}");
        
        if (context.Striped)
            classes.Add("progress-bar-striped");
        
        if (context.Animated)
            classes.Add("progress-bar-animated");
        
        if (context.Indeterminate)
            classes.Add("progress-bar-indeterminate");
        
        return string.Join(" ", classes);
    }
    
    private static string BuildStepClass(bool isActive, bool isCompleted, bool isError)
    {
        var classes = new List<string> { "step-item" };
        
        if (isActive)
            classes.Add("active");
        if (isCompleted)
            classes.Add("completed");
        if (isError)
            classes.Add("error");
        
        return string.Join(" ", classes);
    }
    
    private static string GetVariantColor(VariantType variant) => variant switch
    {
        VariantType.Primary => "var(--bs-primary)",
        VariantType.Secondary => "var(--bs-secondary)",
        VariantType.Success => "var(--bs-success)",
        VariantType.Info => "var(--bs-info)",
        VariantType.Warning => "var(--bs-warning)",
        VariantType.Error => "var(--bs-danger)",
        _ => "var(--bs-primary)"
    };
    
    /// <summary>
    /// Create fluent progress with simplified API
    /// </summary>
    public static RenderFragment<T> Create(
        Func<T, double> valueSelector,
        double max = 100,
        ProgressType type = ProgressType.Linear,
        bool showPercentage = true,
        VariantType variant = VariantType.Primary)
    {
        return item => builder =>
        {
            if (item == null) return;
            
            var value = valueSelector?.Invoke(item) ?? 0;
            var percentage = Math.Max(0, Math.Min(100, (value / max) * 100));
            
            if (type == ProgressType.Linear)
            {
                builder.OpenElement(0, "div");
                builder.AddAttribute(1, "class", "progress");
                builder.AddAttribute(2, "data-template", "progress");
                
                builder.OpenElement(3, "div");
                builder.AddAttribute(4, "class", $"progress-bar bg-{variant.ToString().ToLower()}");
                builder.AddAttribute(5, "style", $"width: {percentage:0.##}%;");
                builder.AddAttribute(6, "role", "progressbar");
                builder.AddAttribute(7, "aria-valuenow", value.ToString("0.##"));
                builder.AddAttribute(8, "aria-valuemin", "0");
                builder.AddAttribute(9, "aria-valuemax", max.ToString("0.##"));
                
                if (showPercentage)
                    builder.AddContent(10, $"{percentage:0.#}%");
                
                builder.CloseElement();
                builder.CloseElement();
            }
        };
    }
}