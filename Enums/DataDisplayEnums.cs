using System.ComponentModel;

namespace RR.Blazor.Enums;

public enum DataDisplayLayout
{
    [Description("Vertical label-value arrangement")]
    Vertical,
    
    [Description("Horizontal side-by-side layout")]
    Horizontal,
    
    [Description("Inline compact layout")]
    Inline
}

public enum DataDisplayEmphasis
{
    [Description("Standard text weight")]
    Normal,
    
    [Description("Semi-bold emphasis")]
    Strong,
    
    [Description("Bold emphasis")]
    Bold,
    
    [Description("Success state with color")]
    Success,
    
    [Description("Warning state with color")]
    Warning,
    
    [Description("Error state with color")]
    Error,
    
    [Description("Muted secondary text")]
    Muted
}

public enum DataDisplayMode
{
    [Description("Auto-detect based on content")]
    Auto,
    
    [Description("Simple info display")]
    Info,
    
    [Description("Business summary display")]
    Summary
}

public enum DataDisplayVariant
{
    [Description("Clean minimal display")]
    Minimal,
    
    [Description("Standard data display")]
    Standard,
    
    [Description("Elevated card container")]
    Card,
    
    [Description("Highlighted with accent")]
    Highlighted,
    
    [Description("Premium glass effect")]
    Premium
}

