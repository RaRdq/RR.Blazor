using RR.Blazor.Enums;

namespace RR.Blazor.Configuration;

/// <summary>
/// Configuration constants for table components following RR.Blazor design system standards
/// </summary>
public static class TableConstants
{
    #region PageSize Configuration
    
    /// <summary>Default page size options for tables</summary>
    public static readonly int[] DefaultPageSizeOptions = new[] { 10, 25, 50, 100 };
    
    /// <summary>Default page size for new tables</summary>
    public const int DefaultPageSize = 50;
    
    /// <summary>Minimum allowed page size</summary>
    public const int MinPageSize = 1;
    
    /// <summary>Maximum allowed page size</summary>
    public const int MaxPageSize = 10000;
    
    #endregion
    
    #region Smart PageSize Options
    
    /// <summary>Page size breakpoints for smart option generation</summary>
    public static readonly (int threshold, int[] options)[] SmartPageSizeBreakpoints = new[]
    {
        (10, new[] { 5, 10 }),
        (50, new[] { 10, 25, 50 }),
        (200, new[] { 10, 25, 50, 100 }),
        (1000, new[] { 25, 50, 100, 250 }),
        (10000, new[] { 50, 100, 250, 500 }),
        (int.MaxValue, new[] { 100, 250, 500, 1000 })
    };
    
    /// <summary>Smart percentage thresholds for large datasets</summary>
    public const double SmartOptionPercentage = 0.05; // 5% of total data
    
    /// <summary>Rounding factor for smart options</summary>
    public const int SmartOptionRounding = 50;
    
    /// <summary>Maximum smart options to show</summary>
    public const int MaxSmartOptions = 5;
    
    #endregion
    
    #region Pagination UI Configuration
    
    /// <summary>Default pagination button size based on component density</summary>
    public static SizeType GetPaginationButtonSize(DensityType density) => density switch
    {
        DensityType.Compact => SizeType.ExtraSmall,
        DensityType.Dense => SizeType.Small,
        DensityType.Normal => SizeType.Small,
        DensityType.Spacious => SizeType.Medium,
        _ => SizeType.Small
    };
    
    /// <summary>Default pagination chip size for selection indicators</summary>
    public static SizeType GetPaginationChipSize(DensityType density) => density switch
    {
        DensityType.Compact => SizeType.Small,
        DensityType.Dense => SizeType.Small,
        DensityType.Normal => SizeType.Small,
        DensityType.Spacious => SizeType.Medium,
        _ => SizeType.Small
    };
    
    #endregion
    
    #region Virtualization Configuration
    
    /// <summary>Default overscan rows for virtualized tables</summary>
    public const int DefaultOverscanRows = 10;
    
    /// <summary>Virtualization threshold - use virtualization above this count</summary>
    public const int VirtualizationThreshold = 1000;
    
    /// <summary>Row height based on density</summary>
    public static int GetRowHeight(DensityType density) => density switch
    {
        DensityType.Compact => 36,
        DensityType.Dense => 48,
        DensityType.Normal => 56,
        DensityType.Spacious => 72,
        _ => 56
    };
    
    #endregion
    
    #region Layout Configuration
    
    /// <summary>Default gap classes by density</summary>
    public static string GetTableGap(DensityType density) => density switch
    {
        DensityType.Compact => "gap-1",
        DensityType.Dense => "gap-2", 
        DensityType.Normal => "gap-3",
        DensityType.Spacious => "gap-4",
        _ => "gap-3"
    };
    
    /// <summary>Default padding classes by density</summary>
    public static string GetTablePadding(DensityType density) => density switch
    {
        DensityType.Compact => "px-2 py-1",
        DensityType.Dense => "px-3 py-1.5",
        DensityType.Normal => "px-4 py-2", 
        DensityType.Spacious => "px-6 py-3",
        _ => "px-4 py-2"
    };
    
    #endregion
}