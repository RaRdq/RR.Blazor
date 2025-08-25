using Microsoft.AspNetCore.Components;
using RR.Blazor.Enums;

namespace RR.Blazor.Templates.Progress;

/// <summary>
/// Context for progress templates
/// </summary>
/// <typeparam name="T">The type of data being rendered</typeparam>
public class ProgressContext<T> where T : class
{
    /// <summary>
    /// The data item being rendered
    /// </summary>
    public T Item { get; set; }
    
    /// <summary>
    /// Size for rendering
    /// </summary>
    public SizeType Size { get; set; } = SizeType.Medium;
    
    /// <summary>
    /// Density for spacing and layout
    /// </summary>
    public DensityType Density { get; set; } = DensityType.Normal;
    
    /// <summary>
    /// Additional CSS classes to apply
    /// </summary>
    public string CssClass { get; set; }
    
    /// <summary>
    /// Whether the item is in a disabled state
    /// </summary>
    public bool Disabled { get; set; }

    /// <summary>
    /// Current progress value
    /// </summary>
    public double Value { get; set; }
    
    /// <summary>
    /// Maximum value for progress
    /// </summary>
    public double Max { get; set; } = 100;
    
    /// <summary>
    /// Calculated percentage (0-100)
    /// </summary>
    public double Percentage => Math.Max(0, Math.Min(100, (Value / Max) * 100));
    
    /// <summary>
    /// Progress type (linear, circular, ring, steps)
    /// </summary>
    public ProgressType Type { get; set; } = ProgressType.Linear;
    
    /// <summary>
    /// Color variant for progress
    /// </summary>
    public VariantType Variant { get; set; } = VariantType.Primary;
    
    /// <summary>
    /// Whether to show percentage text
    /// </summary>
    public bool ShowPercentage { get; set; } = true;
    
    /// <summary>
    /// Whether to show value text
    /// </summary>
    public bool ShowValue { get; set; }
    
    /// <summary>
    /// Custom label text
    /// </summary>
    public string Label { get; set; }
    
    /// <summary>
    /// Whether progress is striped
    /// </summary>
    public bool Striped { get; set; }
    
    /// <summary>
    /// Whether striped animation is active
    /// </summary>
    public bool Animated { get; set; }
    
    /// <summary>
    /// Whether progress is indeterminate (unknown duration)
    /// </summary>
    public bool Indeterminate { get; set; }
    
    /// <summary>
    /// Segments for multi-segment progress
    /// </summary>
    public List<ProgressSegment> Segments { get; set; } = new();
    
    /// <summary>
    /// Steps for step progress
    /// </summary>
    public List<ProgressStep> Steps { get; set; } = new();
    
    /// <summary>
    /// Current step index for step progress
    /// </summary>
    public int CurrentStep { get; set; }
    
    /// <summary>
    /// Height for linear progress bars
    /// </summary>
    public int Height { get; set; }
    
    /// <summary>
    /// Diameter for circular/ring progress
    /// </summary>
    public int Diameter { get; set; }
    
    /// <summary>
    /// Stroke width for circular/ring progress
    /// </summary>
    public int StrokeWidth { get; set; }

    public ProgressContext(T item)
    {
        Item = item;
    }
}

/// <summary>
/// Progress display types
/// </summary>
public enum ProgressType
{
    Linear,
    Circular,
    Ring,
    Steps,
    MultiSegment
}

/// <summary>
/// Segment for multi-segment progress
/// </summary>
public class ProgressSegment
{
    public double Value { get; set; }
    public VariantType Variant { get; set; } = VariantType.Primary;
    public string Label { get; set; }
    public string Tooltip { get; set; }
}

/// <summary>
/// Step for step-based progress
/// </summary>
public class ProgressStep
{
    public string Title { get; set; }
    public string Description { get; set; }
    public StepStatus Status { get; set; } = StepStatus.Pending;
    public string Icon { get; set; }
}

/// <summary>
/// Status for progress steps
/// </summary>
public enum StepStatus
{
    Pending,
    Active,
    Completed,
    Error,
    Skipped
}