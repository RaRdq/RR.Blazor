namespace RR.Blazor.Enums;

/// <summary>
/// Semantic color variants for chips, determining meaning and visual appearance.
/// </summary>
public enum ChipVariant
{
    /// <summary>Primary brand color for emphasis and main actions</summary>
    Primary,
    /// <summary>Secondary neutral color for supporting elements</summary>
    Secondary,
    /// <summary>Green color variant indicating positive states, completion, approval</summary>
    Success,
    /// <summary>Orange/yellow color variant for warnings, attention, pending states</summary>
    Warning,
    /// <summary>Red color variant for errors, failures, critical states</summary>
    Error,
    /// <summary>Blue color variant for information, tips, neutral notifications</summary>
    Info,
    /// <summary>Yellow/orange color variant for pending, waiting, or queued states</summary>
    Pending,
    /// <summary>Blue animated color variant for processing, loading, or active operations</summary>
    Processing
}

/// <summary>
/// Visual style variants for chips, determining shape, typography, and behavior patterns.
/// Unifies badge, chip, and status styling approaches.
/// </summary>
public enum ChipStyle
{
    /// <summary>Standard chip style for tags, categories, filters with rounded corners</summary>
    Chip,
    /// <summary>Compact badge style for counts, notifications with pill shape and bold text</summary>
    Badge,
    /// <summary>Status indicator style for state display with subtle rounded corners</summary>
    Status
}