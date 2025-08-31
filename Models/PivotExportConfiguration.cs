using RR.Blazor.Enums;
using RR.Blazor.Services.Export;

namespace RR.Blazor.Models
{
    public class PivotExportConfiguration
    {
        public string FileName { get; set; } = "PivotTable_Export";
        public ExportFormat Format { get; set; } = ExportFormat.Excel;
        public PivotExportStructure Structure { get; set; } = PivotExportStructure.CrossTab;
        
        // Structure Options
        public bool PreservePivotStructure { get; set; } = true;
        public bool ExpandAllGroups { get; set; } = false;
        
        // Include Options
        public bool IncludeHeaders { get; set; } = true;
        public bool IncludeSubtotals { get; set; } = true;
        public bool IncludeGrandTotals { get; set; } = true;
        public bool IncludeFilters { get; set; } = false;
        public bool IncludeFieldList { get; set; } = false;
        public bool IncludeGrouping { get; set; } = false;
        
        // Formatting Options
        public bool ApplyNumberFormat { get; set; } = true;
        public bool UseCurrencyFormat { get; set; } = true;
        public bool HighlightSubtotals { get; set; } = true;
        public bool ColorCodeGroups { get; set; } = false;
    }
    
    public enum PivotExportStructure
    {
        CrossTab = 0,
        FlatTable = 1,
        DetailedWithContext = 2,
        SummaryOnly = 3
    }
}