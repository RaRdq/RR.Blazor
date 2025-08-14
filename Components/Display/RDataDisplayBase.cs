using Microsoft.AspNetCore.Components;
using RR.Blazor.Attributes;
using RR.Blazor.Enums;
using RR.Blazor.Components.Base;

namespace RR.Blazor.Components.Display;

/// <summary>
/// Base class for data display components with shared parameters
/// </summary>
public abstract class RDataDisplayBase : RTextComponentBase
{
    #region Core Display Parameters
    
    [Parameter, AIParameter("Value text to display", "John Doe, $1,250.00, Active")]
    public string Value { get; set; } = "";
    
    [Parameter, AIParameter("Display mode", "DataDisplayMode.Auto for smart detection")]
    public DataDisplayMode Mode { get; set; } = DataDisplayMode.Auto;
    
    [Parameter, AIParameter("Visual variant", "DataDisplayVariant.Standard")]
    public DataDisplayVariant Variant { get; set; } = DataDisplayVariant.Standard;
    
    [Parameter, AIParameter("Size variant", "SizeType.Medium")]
    public SizeType Size { get; set; } = SizeType.Medium;
    
    [Parameter, AIParameter("Layout arrangement", "DataDisplayLayout.Vertical")]
    public DataDisplayLayout Layout { get; set; } = DataDisplayLayout.Vertical;
    
    [Parameter, AIParameter("Value emphasis level", "DataDisplayEmphasis.Normal")]
    public DataDisplayEmphasis Emphasis { get; set; } = DataDisplayEmphasis.Normal;
    
    #endregion
    
    #region Enhanced Display Features
    
    [Parameter, AIParameter("Required field indicator", "true for required fields")]
    public bool Required { get; set; }
    
    [Parameter, AIParameter("Show separator line", "true for visual separation")]
    public bool ShowSeparator { get; set; }
    
    [Parameter, AIParameter("Emphasize value text", "true for bold value")]
    public bool EmphasizeValue { get; set; }
    
    
    #endregion
    
    #region Content Parameters
    
    [Parameter, AIParameter("Custom value content", "For badges, buttons, or complex value display")]
    public RenderFragment ValueContent { get; set; }
    
    [Parameter, AIParameter("Additional content after value", "For secondary information")]
    public RenderFragment AdditionalContent { get; set; }
    
    #endregion
    
    #region Smart Detection Properties
    
    protected virtual bool HasLongText => !string.IsNullOrEmpty(Text) && Text.Length > 25;
    protected virtual bool HasLongValue => !string.IsNullOrEmpty(Value) && Value.Length > 15;
    protected virtual bool IsBusinessMetric => Value?.Contains("$") == true || Value?.Contains("%") == true || Value?.Contains(",") == true;
    protected virtual bool HasEmphasis => Emphasis != DataDisplayEmphasis.Normal || EmphasizeValue;
    
    #endregion
    
    #region Style Generation Methods
    
    protected virtual string GetContainerClasses()
    {
        var classes = new List<string> { "data-display" };
        
        classes.Add($"data-display-{Variant.ToString().ToLowerInvariant()}");
        classes.Add($"data-display-{Size.ToString().ToLowerInvariant()}");
        classes.Add($"data-display-{Layout.ToString().ToLowerInvariant()}");
        
        if (HasEmphasis)
            classes.Add($"data-display-emphasis-{Emphasis.ToString().ToLowerInvariant()}");
            
        if (ShowSeparator)
            classes.Add("data-display-separator");
            
        if (Loading)
            classes.Add("data-display-loading");
            
        if (Required)
            classes.Add("data-display-required");
            
        if (!string.IsNullOrEmpty(Class))
            classes.Add(Class);
            
        return string.Join(" ", classes);
    }
    
    protected virtual string GetLabelClasses()
    {
        var classes = new List<string> { "data-display-label" };
        
        classes.Add(Size switch
        {
            SizeType.Small => "text-xs font-medium",
            SizeType.Medium => "text-sm font-medium", 
            SizeType.Large => "text-base font-medium",
            _ => "text-sm font-medium"
        });
        
        classes.Add("text-muted");
        
        return string.Join(" ", classes);
    }
    
    protected virtual string GetValueClasses()
    {
        var classes = new List<string> { "data-display-value" };
        
        classes.Add(Size switch
        {
            SizeType.Small => "text-sm",
            SizeType.Medium => "text-base",
            SizeType.Large => "text-lg", 
            _ => "text-base"
        });
        
        classes.Add(Emphasis switch
        {
            DataDisplayEmphasis.Strong => "font-semibold text-interactive",
            DataDisplayEmphasis.Bold => "font-bold text-interactive",
            DataDisplayEmphasis.Success => "font-semibold text-success",
            DataDisplayEmphasis.Warning => "font-semibold text-warning",
            DataDisplayEmphasis.Error => "font-semibold text-error",
            DataDisplayEmphasis.Muted => "text-muted",
            _ => "text-primary"
        });
        
        if (EmphasizeValue && Emphasis == DataDisplayEmphasis.Normal)
            classes.Add("font-semibold");
            
        if (Layout == DataDisplayLayout.Horizontal)
            classes.Add("text-right");
            
        return string.Join(" ", classes);
    }
    
    protected override string GetIconClasses()
    {
        return Size switch
        {
            SizeType.Small => "text-sm text-interactive",
            SizeType.Medium => "text-base text-interactive",
            SizeType.Large => "text-lg text-interactive",
            _ => "text-base text-interactive"
        };
    }
    
    protected override string GetTextSizeClasses()
    {
        return Size switch
        {
            SizeType.Small => "text-xs",
            SizeType.Medium => "text-sm",
            SizeType.Large => "text-base",
            _ => "text-sm"
        };
    }
    
    protected override string GetIconSizeClasses()
    {
        return Size switch
        {
            SizeType.Small => "text-sm",
            SizeType.Medium => "text-base",
            SizeType.Large => "text-lg",
            _ => "text-base"
        };
    }
    
    #endregion
}