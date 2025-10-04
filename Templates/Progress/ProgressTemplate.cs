using Microsoft.AspNetCore.Components;
using System.Linq.Expressions;
using RR.Blazor.Enums;

namespace RR.Blazor.Templates.Progress;

/// <summary>
/// Progress template for displaying progress bars, rings, and steps
/// </summary>
/// <typeparam name="T">Type of data the template handles</typeparam>
public class ProgressTemplate<T> where T : class
{
    /// <summary>
    /// Unique identifier for this template
    /// </summary>
    public string Id { get; set; } = Guid.NewGuid().ToString();
    
    /// <summary>
    /// Display name for the template
    /// </summary>
    public string Name { get; set; } = "Progress Template";
    
    /// <summary>
    /// Property selector for extracting data from the item
    /// </summary>
    public Expression<Func<T, object>> PropertySelector { get; set; }
    
    /// <summary>
    /// CSS classes to apply to the rendered template
    /// </summary>
    public string Class { get; set; }
    
    /// <summary>
    /// Size variant
    /// </summary>
    public SizeType Size { get; set; } = SizeType.Medium;
    
    /// <summary>
    /// Density variant
    /// </summary>
    public DensityType Density { get; set; } = DensityType.Normal;

    /// <summary>
    /// Progress display type
    /// </summary>
    public ProgressType Type { get; set; } = ProgressType.Linear;
    
    /// <summary>
    /// Default color variant
    /// </summary>
    public VariantType Variant { get; set; } = VariantType.Primary;
    
    /// <summary>
    /// Value selector for progress value
    /// </summary>
    public Expression<Func<T, double>> ValueSelector { get; set; }
    
    /// <summary>
    /// Max value selector
    /// </summary>
    public Expression<Func<T, double>> MaxSelector { get; set; }
    
    /// <summary>
    /// Label selector for custom labels
    /// </summary>
    public Expression<Func<T, string>> LabelSelector { get; set; }
    
    /// <summary>
    /// Variant selector for dynamic coloring
    /// </summary>
    public Expression<Func<T, VariantType>> VariantSelector { get; set; }
    
    /// <summary>
    /// Segments selector for multi-segment progress
    /// </summary>
    public Expression<Func<T, List<ProgressSegment>>> SegmentsSelector { get; set; }
    
    /// <summary>
    /// Steps selector for step progress
    /// </summary>
    public Expression<Func<T, List<ProgressStep>>> StepsSelector { get; set; }
    
    /// <summary>
    /// Current step selector
    /// </summary>
    public Expression<Func<T, int>> CurrentStepSelector { get; set; }
    
    /// <summary>
    /// Whether to show percentage
    /// </summary>
    public bool ShowPercentage { get; set; } = true;
    
    /// <summary>
    /// Whether to show value
    /// </summary>
    public bool ShowValue { get; set; }
    
    /// <summary>
    /// Whether progress is striped
    /// </summary>
    public bool Striped { get; set; }
    
    /// <summary>
    /// Whether striped animation is active
    /// </summary>
    public bool Animated { get; set; }
    
    /// <summary>
    /// Whether progress is indeterminate
    /// </summary>
    public bool Indeterminate { get; set; }
    
    /// <summary>
    /// Default max value
    /// </summary>
    public double DefaultMax { get; set; } = 100;
    
    /// <summary>
    /// Height for linear progress (pixels)
    /// </summary>
    public int Height { get; set; }
    
    /// <summary>
    /// Diameter for circular/ring progress (pixels)
    /// </summary>
    public int Diameter { get; set; }
    
    /// <summary>
    /// Stroke width for circular/ring progress (pixels)
    /// </summary>
    public int StrokeWidth { get; set; }
    
    /// <summary>
    /// Threshold mappings for automatic variant selection
    /// </summary>
    public Dictionary<double, VariantType> ThresholdMapping { get; set; } = new()
    {
        { 0, VariantType.Error },
        { 25, VariantType.Warning },
        { 50, VariantType.Info },
        { 75, VariantType.Primary },
        { 100, VariantType.Success }
    };

    /// <summary>
    /// Gets the value from the item using the property selector
    /// </summary>
    public virtual object GetValue(T item)
    {
        if (item == null) return null;
        
        var compiledSelector = PropertySelector?.Compile();
        return compiledSelector?.Invoke(item);
    }
    
    /// <summary>
    /// Gets variant based on progress percentage
    /// </summary>
    public VariantType GetVariantByPercentage(double percentage)
    {
        if (!ThresholdMapping.Any())
            return Variant;
        
        var applicableThreshold = ThresholdMapping
            .Where(t => percentage >= t.Key)
            .OrderByDescending(t => t.Key)
            .FirstOrDefault();
        
        return applicableThreshold.Value != default ? applicableThreshold.Value : Variant;
    }
    
    /// <summary>
    /// Calculates size dimensions based on size type
    /// </summary>
    public static (int height, int diameter, int stroke) GetDimensions(SizeType size, ProgressType type)
    {
        return type switch
        {
            ProgressType.Linear => size switch
            {
                SizeType.ExtraSmall => (4, 0, 0),
                SizeType.Small => (8, 0, 0),
                SizeType.Medium => (16, 0, 0),
                SizeType.Large => (24, 0, 0),
                SizeType.ExtraLarge => (32, 0, 0),
                _ => (16, 0, 0)
            },
            ProgressType.Circular or ProgressType.Ring => size switch
            {
                SizeType.ExtraSmall => (0, 32, 3),
                SizeType.Small => (0, 48, 4),
                SizeType.Medium => (0, 64, 5),
                SizeType.Large => (0, 96, 6),
                SizeType.ExtraLarge => (0, 128, 8),
                _ => (0, 64, 5)
            },
            _ => (16, 64, 5)
        };
    }

    /// <summary>
    /// Renders the template for the given item
    /// </summary>
    public RenderFragment Render(T item)
    {
        var context = CreateContext(item);
        var renderer = new ProgressRenderer<T>();
        return renderer.Render(context);
    }

    private ProgressContext<T> CreateContext(T item)
    {
        var context = new ProgressContext<T>(item)
        {
            Size = Size,
            Density = Density,
            Class = Class,
            Type = Type,
            ShowPercentage = ShowPercentage,
            ShowValue = ShowValue,
            Striped = Striped,
            Animated = Animated,
            Indeterminate = Indeterminate
        };
        
        // Get value
        if (ValueSelector != null)
        {
            var valueGetter = ValueSelector.Compile();
            context.Value = valueGetter(item);
        }
        else
        {
            var value = GetValue(item);
            context.Value = Convert.ToDouble(value ?? 0);
        }
        
        // Get max
        if (MaxSelector != null)
        {
            var maxGetter = MaxSelector.Compile();
            context.Max = maxGetter(item);
        }
        else
        {
            context.Max = DefaultMax;
        }
        
        // Get label
        if (LabelSelector != null)
        {
            var labelGetter = LabelSelector.Compile();
            context.Label = labelGetter(item);
        }
        
        // Get variant
        if (VariantSelector != null)
        {
            var variantGetter = VariantSelector.Compile();
            context.Variant = variantGetter(item);
        }
        else if (ThresholdMapping.Any())
        {
            context.Variant = GetVariantByPercentage(context.Percentage);
        }
        else
        {
            context.Variant = Variant;
        }
        
        // Get segments
        if (SegmentsSelector != null)
        {
            var segmentsGetter = SegmentsSelector.Compile();
            context.Segments = segmentsGetter(item) ?? new List<ProgressSegment>();
            if (context.Segments.Any())
                context.Type = ProgressType.MultiSegment;
        }
        
        // Get steps
        if (StepsSelector != null)
        {
            var stepsGetter = StepsSelector.Compile();
            context.Steps = stepsGetter(item) ?? new List<ProgressStep>();
            if (context.Steps.Any())
                context.Type = ProgressType.Steps;
        }
        
        // Get current step
        if (CurrentStepSelector != null)
        {
            var stepGetter = CurrentStepSelector.Compile();
            context.CurrentStep = stepGetter(item);
        }
        
        // Set dimensions
        var (height, diameter, stroke) = GetDimensions(Size, Type);
        context.Height = Height > 0 ? Height : height;
        context.Diameter = Diameter > 0 ? Diameter : diameter;
        context.StrokeWidth = StrokeWidth > 0 ? StrokeWidth : stroke;
        
        return context;
    }
}