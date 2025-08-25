using RR.Blazor.Enums;
using RR.Blazor.Templates.Stack;

namespace RR.Blazor.Templates.Configuration;

/// <summary>
/// Global template configuration for RR.Blazor
/// Provides fluent API for configuring template defaults
/// </summary>
public class TemplateConfiguration
{
    /// <summary>
    /// Badge template defaults
    /// </summary>
    public BadgeTemplateDefaults Badge { get; set; } = new();
    
    /// <summary>
    /// Currency template defaults
    /// </summary>
    public CurrencyTemplateDefaults Currency { get; set; } = new();
    
    /// <summary>
    /// Stack template defaults
    /// </summary>
    public StackTemplateDefaults Stack { get; set; } = new();
    
    /// <summary>
    /// Smart detection settings
    /// </summary>
    public SmartDetectionSettings SmartDetection { get; set; } = new();
}

/// <summary>
/// Badge template default configuration
/// </summary>
public class BadgeTemplateDefaults
{
    /// <summary>
    /// Default badge variant
    /// </summary>
    public VariantType DefaultVariant { get; set; } = VariantType.Primary;
    
    /// <summary>
    /// Default badge size
    /// </summary>
    public SizeType DefaultSize { get; set; } = SizeType.Small;
    
    /// <summary>
    /// Default badge density
    /// </summary>
    public DensityType DefaultDensity { get; set; } = DensityType.Compact;
    
    /// <summary>
    /// Whether badges are clickable by default
    /// </summary>
    public bool DefaultClickable { get; set; } = false;
    
    /// <summary>
    /// Global status-to-variant mapping
    /// </summary>
    public Dictionary<string, VariantType> GlobalStatusMapping { get; set; } = new()
    {
        { "active", VariantType.Success },
        { "inactive", VariantType.Secondary },
        { "pending", VariantType.Warning },
        { "error", VariantType.Danger },
        { "success", VariantType.Success },
        { "warning", VariantType.Warning },
        { "info", VariantType.Info },
        { "approved", VariantType.Success },
        { "rejected", VariantType.Danger },
        { "cancelled", VariantType.Secondary }
    };
}

/// <summary>
/// Currency template default configuration
/// </summary>
public class CurrencyTemplateDefaults
{
    /// <summary>
    /// Default currency code
    /// </summary>
    public string DefaultCurrencyCode { get; set; } = "USD";
    
    /// <summary>
    /// Default number format
    /// </summary>
    public string DefaultFormat { get; set; } = "C";
    
    /// <summary>
    /// Whether to use compact formatting by default
    /// </summary>
    public bool DefaultCompact { get; set; } = false;
    
    /// <summary>
    /// Whether to show value colors by default
    /// </summary>
    public bool DefaultShowColors { get; set; } = true;
    
    /// <summary>
    /// Threshold for automatic compact formatting
    /// </summary>
    public decimal AutoCompactThreshold { get; set; } = 100_000;
}

/// <summary>
/// Stack template default configuration
/// </summary>
public class StackTemplateDefaults
{
    /// <summary>
    /// Default stack orientation
    /// </summary>
    public StackOrientation DefaultOrientation { get; set; } = StackOrientation.Vertical;
    
    /// <summary>
    /// Whether to truncate text by default
    /// </summary>
    public bool DefaultTruncateText { get; set; } = true;
    
    /// <summary>
    /// Default maximum text length
    /// </summary>
    public int DefaultMaxLength { get; set; } = 50;
    
    /// <summary>
    /// Default stack size
    /// </summary>
    public SizeType DefaultSize { get; set; } = SizeType.Medium;
    
    /// <summary>
    /// Default stack density
    /// </summary>
    public DensityType DefaultDensity { get; set; } = DensityType.Normal;
}

/// <summary>
/// Smart detection configuration
/// </summary>
public class SmartDetectionSettings
{
    /// <summary>
    /// Whether smart detection is enabled globally
    /// </summary>
    public bool Enabled { get; set; } = true;
    
    /// <summary>
    /// Minimum confidence threshold for auto-application
    /// </summary>
    public double AutoApplyThreshold { get; set; } = 0.8;
    
    /// <summary>
    /// Minimum confidence threshold for suggestions
    /// </summary>
    public double SuggestionThreshold { get; set; } = 0.6;
    
    /// <summary>
    /// Whether to analyze sample data for better detection
    /// </summary>
    public bool AnalyzeSampleData { get; set; } = true;
    
    /// <summary>
    /// Number of sample records to analyze
    /// </summary>
    public int SampleDataLimit { get; set; } = 10;
}