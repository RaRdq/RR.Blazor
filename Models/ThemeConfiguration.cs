namespace RR.Blazor.Models;

/// <summary>
/// Theme mode options for RR.Blazor
/// </summary>
public enum ThemeMode
{
    /// <summary>Follow system preference</summary>
    System = 0,
    /// <summary>Light theme</summary>
    Light = 1,
    /// <summary>Dark theme</summary>
    Dark = 2,
    /// <summary>Custom theme defined by user</summary>
    Custom = 3
}

/// <summary>
/// Comprehensive theme configuration for RR.Blazor design system
/// </summary>
public class ThemeConfiguration
{
    // Core settings
    public string Name { get; set; } = "Default";
    public ThemeMode Mode { get; set; } = ThemeMode.System;
    public bool AnimationsEnabled { get; set; } = true;
    public bool AccessibilityMode { get; set; } = false;
    public bool HighContrastMode { get; set; } = false;
    
    // Core colors (using RR.Blazor variable names)
    public string PrimaryColor { get; set; }
    public string SecondaryColor { get; set; }
    public string ErrorColor { get; set; }
    public string WarningColor { get; set; }
    public string InfoColor { get; set; }
    public string SuccessColor { get; set; }
    
    // Extended theme colors
    public string BackgroundPrimary { get; set; }
    public string BackgroundSecondary { get; set; }
    public string BackgroundElevated { get; set; }
    public string TextPrimary { get; set; }
    public string TextSecondary { get; set; }
    public string TextTertiary { get; set; }
    
    // Component overrides
    public Dictionary<string, string> CustomVariables { get; set; } = new();
    
    // Custom theme files (name -> scss file path)
    public Dictionary<string, string> CustomThemes { get; set; } = new();
    
    // Default theme configuration
    public static ThemeConfiguration Default => new()
    {
        Mode = ThemeMode.System,
        AnimationsEnabled = true,
        AccessibilityMode = false,
        HighContrastMode = false
    };
    
    /// <summary>Get effective theme mode based on system preference</summary>
    public ThemeMode GetEffectiveMode(bool systemPrefersDark)
    {
        return Mode == ThemeMode.System 
            ? (systemPrefersDark ? ThemeMode.Dark : ThemeMode.Light)
            : Mode;
    }
}