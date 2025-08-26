using RR.Blazor.Services.Export;

namespace RR.Blazor.Models.Export;

public record ExportOptions(
    string FileName,
    bool IncludeHeaders = true,
    bool IncludeHiddenColumns = false,
    string DateFormat = "yyyy-MM-dd",
    string NumberFormat = "N2",
    Dictionary<string, string>? CustomFormats = null,
    Dictionary<string, string>? CustomHeaders = null,
    List<string>? ColumnSelection = null,
    ExportTemplate? Template = null,
    ExportDataSource DataSource = ExportDataSource.All,
    Func<object, bool>? DataFilter = null,
    bool EnableBackgroundProcessing = false,
    int? MaxConcurrentExports = null
);

public record ExcelExportOptions : ExportOptions
{
    public bool CreateMultipleSheets { get; init; } = false;
    public Dictionary<string, string> SheetNames { get; init; } = new();
    public bool IncludeCharts { get; init; } = false;
    public bool ApplyFormatting { get; init; } = true;
    public bool IncludeFormulas { get; init; } = false;
    public bool ProtectWorkbook { get; init; } = false;
    public string Password { get; init; } = "";
    public ExcelTemplate? ExcelTemplate { get; init; }
    public Dictionary<string, ExcelCellStyle> CellStyles { get; init; } = new();
    public List<ExcelChart> Charts { get; init; } = new();

    public ExcelExportOptions(string fileName) : base(fileName) { }
}

public record PdfTemplateOptions : ExportOptions
{
    public PdfTemplate? PdfTemplate { get; init; }
    public Dictionary<string, object> TemplateVariables { get; init; } = new();
    public bool IncludePageNumbers { get; init; } = true;
    public bool IncludeWatermark { get; init; } = false;
    public string WatermarkText { get; init; } = "";
    public PdfOrientation Orientation { get; init; } = PdfOrientation.Portrait;
    public PdfPageSize PageSize { get; init; } = PdfPageSize.A4;
    public byte[]? HeaderLogo { get; init; }
    public string HeaderText { get; init; } = "";
    public string FooterText { get; init; } = "";
    public Dictionary<string, string> CssStyles { get; init; } = new();

    public PdfTemplateOptions(string fileName) : base(fileName) { }
}

public record PowerBIExportOptions : ExportOptions
{
    public string WorkspaceId { get; init; } = "";
    public string DatasetId { get; init; } = "";
    public string TableName { get; init; } = "";
    public bool RefreshDataset { get; init; } = true;
    public Dictionary<string, string> ColumnMappings { get; init; } = new();
    public PowerBIAuthenticationType AuthType { get; init; } = PowerBIAuthenticationType.ServicePrincipal;

    public PowerBIExportOptions(string fileName) : base(fileName) { }
}

public record ScheduledExportOptions
{
    public string Name { get; init; } = "";
    public ExportFormat Format { get; init; }
    public string CronExpression { get; init; } = "";
    public List<string> EmailRecipients { get; init; } = new();
    public string EmailSubject { get; init; } = "";
    public string EmailBody { get; init; } = "";
    public Dictionary<string, object> ExportParameters { get; init; } = new();
    public bool IsActive { get; init; } = true;
    public string UserId { get; init; } = "";
    public string CompanyId { get; init; } = "";
    public DateTime? NextRunTime { get; init; }
    public DateTime? LastRunTime { get; init; }
    public int MaxRetries { get; init; } = 3;
}

public abstract record ExportTemplate(
    string Name,
    string Description,
    Dictionary<string, object> Settings
)
{
    public string Id { get; init; } = Guid.NewGuid().ToString();
    public string CreatedBy { get; init; } = "";
    public DateTime Created { get; init; } = DateTime.UtcNow;
    public Dictionary<string, object> Variables { get; init; } = new();
    public bool IsSystem { get; init; } = false;
    public string Category { get; init; } = "General";
    public List<string> SupportedFormats { get; init; } = new();
}

public record ExcelTemplate(
    string Name,
    string Description,
    Dictionary<string, object> Settings
) : ExportTemplate(Name, Description, Settings)
{
    public byte[]? TemplateFile { get; init; }
    public Dictionary<string, string> CellMappings { get; init; } = new();
    public List<ExcelChart> Charts { get; init; } = new();
    public Dictionary<string, ExcelCellStyle> CellStyles { get; init; } = new();
    public List<ExcelWorksheet> Worksheets { get; init; } = new();
    public bool EnableFormulas { get; init; } = false;
}

public record PdfTemplate(
    string Name,
    string Description,
    Dictionary<string, object> Settings
) : ExportTemplate(Name, Description, Settings)
{
    public string HtmlTemplate { get; init; } = "";
    public byte[]? HeaderLogo { get; init; }
    public string HeaderText { get; init; } = "";
    public string FooterText { get; init; } = "";
    public Dictionary<string, string> CssStyles { get; init; } = new();
    public PdfPageSettings PageSettings { get; init; } = new();
    public List<PdfSection> Sections { get; init; } = new();
}

public record JsonTemplate(
    string Name,
    string Description,
    Dictionary<string, object> Settings
) : ExportTemplate(Name, Description, Settings)
{
    public string JsonSchema { get; init; } = "";
    public bool IncludeMetadata { get; init; } = true;
    public JsonStructureType StructureType { get; init; } = JsonStructureType.Array;
    public Dictionary<string, JsonFieldMapping> FieldMappings { get; init; } = new();
}

public record ExportProgressEventArgs(
    string FileName,
    int ProcessedItems,
    int TotalItems,
    double ProgressPercentage,
    string CurrentStep,
    bool IsCompleted = false,
    string? ErrorMessage = null
)
{
    public string JobId { get; init; } = "";
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
    public TimeSpan? EstimatedTimeRemaining { get; init; }
    public double ProcessingRate { get; init; } = 0; // items per second
    public string Phase { get; init; } = ""; // e.g., "Processing", "Formatting", "Finalizing"
    public Dictionary<string, object> AdditionalData { get; init; } = new();
    public bool CanCancel { get; init; } = true;
}


public enum ExportDataSource
{
    All,
    Visible,
    Selected,
    CurrentPage,
    Custom,
    Filtered
}

public enum PdfOrientation
{
    Portrait,
    Landscape
}

public enum PdfPageSize
{
    A4,
    A3,
    A5,
    Letter,
    Legal,
    Tabloid,
    Custom
}

public enum PowerBIAuthenticationType
{
    ServicePrincipal,
    UserCredentials,
    AccessToken
}

public enum JsonStructureType
{
    Array,
    Object,
    Nested,
    Flat
}

public enum ExportJobStatus
{
    Pending,
    InProgress,
    Completed,
    Failed,
    Cancelled,
    Scheduled
}

public enum ExportPriority
{
    Low,
    Normal,
    High,
    Critical
}

public record ExportResult(
    bool Success,
    byte[]? Data,
    string? FileName,
    string? MimeType,
    string? ErrorMessage = null
)
{
    public string JobId { get; init; } = Guid.NewGuid().ToString();
    public DateTime StartTime { get; init; } = DateTime.UtcNow;
    public DateTime? EndTime { get; init; }
    public TimeSpan? Duration => EndTime?.Subtract(StartTime);
    public long? FileSizeBytes { get; init; }
    public int RecordsProcessed { get; init; } = 0;
    public Dictionary<string, object> Metadata { get; init; } = new();
    public ExportJobStatus Status { get; init; } = ExportJobStatus.Completed;
    public string? DownloadUrl { get; init; }
    public DateTime? ExpiresAt { get; init; }
    public List<string> Warnings { get; init; } = new();
    public ExportStatistics Statistics { get; init; } = new();
}

public record ExportStatistics
{
    public int TotalRecords { get; init; } = 0;
    public int ProcessedRecords { get; init; } = 0;
    public int SkippedRecords { get; init; } = 0;
    public int ErrorRecords { get; init; } = 0;
    public double ProcessingTimeMs { get; init; } = 0;
    public double MemoryUsageMb { get; init; } = 0;
    public Dictionary<string, int> FieldStatistics { get; init; } = new();
}

public static class ExportMimeTypes
{
    public const string CSV = "text/csv";
    public const string Excel = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
    public const string JSON = "application/json";
    public const string PDF = "application/pdf";
    public const string PowerBI = "application/powerbi";
    public const string XML = "application/xml";
    public const string YAML = "application/x-yaml";
    public const string HTML = "text/html";
    public const string TSV = "text/tab-separated-values";

    public static string GetMimeType(ExportFormat format) => format switch
    {
        ExportFormat.CSV => CSV,
        ExportFormat.Excel => Excel,
        ExportFormat.JSON => JSON,
        ExportFormat.PDF => PDF,
        ExportFormat.PowerBI => PowerBI,
        ExportFormat.XML => XML,
        ExportFormat.YAML => YAML,
        ExportFormat.HTML => HTML,
        ExportFormat.TSV => TSV,
        _ => "application/octet-stream"
    };

    public static string GetFileExtension(ExportFormat format) => format switch
    {
        ExportFormat.CSV => ".csv",
        ExportFormat.Excel => ".xlsx",
        ExportFormat.JSON => ".json",
        ExportFormat.PDF => ".pdf",
        ExportFormat.PowerBI => ".pbix",
        ExportFormat.XML => ".xml",
        ExportFormat.YAML => ".yaml",
        ExportFormat.HTML => ".html",
        ExportFormat.TSV => ".tsv",
        _ => ".bin"
    };

    public static string GetFormatDisplayName(ExportFormat format) => format switch
    {
        ExportFormat.CSV => "CSV (Comma Separated Values)",
        ExportFormat.Excel => "Microsoft Excel",
        ExportFormat.JSON => "JSON (JavaScript Object Notation)",
        ExportFormat.PDF => "PDF (Portable Document Format)",
        ExportFormat.PowerBI => "Power BI Dataset",
        ExportFormat.XML => "XML (Extensible Markup Language)",
        ExportFormat.YAML => "YAML (YAML Ain't Markup Language)",
        ExportFormat.HTML => "HTML (HyperText Markup Language)",
        ExportFormat.TSV => "TSV (Tab Separated Values)",
        _ => "Unknown Format"
    };
}

// Excel-specific models
public record ExcelCellStyle
{
    public string FontName { get; init; } = "Arial";
    public int FontSize { get; init; } = 10;
    public bool Bold { get; init; } = false;
    public bool Italic { get; init; } = false;
    public string BackgroundColor { get; init; } = "";
    public string ForegroundColor { get; init; } = "";
    public string BorderStyle { get; init; } = "None";
    public string NumberFormat { get; init; } = "";
    public string HorizontalAlignment { get; init; } = "Left";
    public string VerticalAlignment { get; init; } = "Top";
    public bool WrapText { get; init; } = false;
}

public record ExcelChart
{
    public string Name { get; init; } = "";
    public string Title { get; init; } = "";
    public string ChartType { get; init; } = "Column";
    public string DataRange { get; init; } = "";
    public int Width { get; init; } = 400;
    public int Height { get; init; } = 300;
    public int Row { get; init; } = 1;
    public int Column { get; init; } = 1;
    public Dictionary<string, object> Properties { get; init; } = new();
}

public record ExcelWorksheet
{
    public string Name { get; init; } = "";
    public string DataRange { get; init; } = "";
    public Dictionary<string, ExcelCellStyle> CellStyles { get; init; } = new();
    public List<ExcelChart> Charts { get; init; } = new();
    public bool Protected { get; init; } = false;
    public string Password { get; init; } = "";
    public Dictionary<string, object> Properties { get; init; } = new();
}

// PDF-specific models
public record PdfPageSettings
{
    public PdfPageSize PageSize { get; init; } = PdfPageSize.A4;
    public PdfOrientation Orientation { get; init; } = PdfOrientation.Portrait;
    public PdfMargins Margins { get; init; } = new();
    public bool ShowPageNumbers { get; init; } = true;
    public string PageNumberFormat { get; init; } = "Page {0} of {1}";
    public string WatermarkText { get; init; } = "";
}

public record PdfMargins
{
    public double Top { get; init; } = 20;
    public double Right { get; init; } = 20;
    public double Bottom { get; init; } = 20;
    public double Left { get; init; } = 20;
}

public record PdfSection
{
    public string Name { get; init; } = "";
    public string Content { get; init; } = "";
    public bool PageBreakBefore { get; init; } = false;
    public bool PageBreakAfter { get; init; } = false;
    public Dictionary<string, object> Properties { get; init; } = new();
}

// JSON-specific models
public record JsonFieldMapping
{
    public string SourceField { get; init; } = "";
    public string TargetField { get; init; } = "";
    public string DataType { get; init; } = "string";
    public Func<object, object>? Transform { get; init; }
    public bool IsRequired { get; init; } = false;
    public object? DefaultValue { get; init; }
}

// Export job tracking models
public record ExportJob
{
    public string Id { get; init; } = Guid.NewGuid().ToString();
    public string Name { get; init; } = "";
    public ExportFormat Format { get; init; }
    public ExportJobStatus Status { get; init; } = ExportJobStatus.Pending;
    public ExportPriority Priority { get; init; } = ExportPriority.Normal;
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
    public DateTime? StartedAt { get; init; }
    public DateTime? CompletedAt { get; init; }
    public string CreatedBy { get; init; } = "";
    public string CompanyId { get; init; } = "";
    public Dictionary<string, object> Parameters { get; init; } = new();
    public ExportResult? Result { get; init; }
    public List<string> Errors { get; init; } = new();
    public double ProgressPercentage { get; init; } = 0;
    public string CurrentStep { get; init; } = "";
    public int RetryCount { get; init; } = 0;
    public int MaxRetries { get; init; } = 3;
}

public record ScheduledExport
{
    public string Id { get; init; } = Guid.NewGuid().ToString();
    public string Name { get; init; } = "";
    public string CronExpression { get; init; } = "";
    public ExportFormat Format { get; init; }
    public bool IsActive { get; init; } = true;
    public DateTime? NextRunTime { get; init; }
    public DateTime? LastRunTime { get; init; }
    public string CreatedBy { get; init; } = "";
    public string CompanyId { get; init; } = "";
    public List<string> EmailRecipients { get; init; } = new();
    public string EmailSubject { get; init; } = "";
    public string EmailBody { get; init; } = "";
    public Dictionary<string, object> ExportParameters { get; init; } = new();
    public List<ExportJob> RecentJobs { get; init; } = new();
    public bool EmailOnSuccess { get; init; } = true;
    public bool EmailOnFailure { get; init; } = true;
}

// Event argument models
public record ExportStartedEventArgs(
    string JobId,
    string FileName,
    ExportFormat Format,
    int TotalRecords
);

public record ExportCompletedEventArgs(
    string JobId,
    ExportResult Result
);

public record ExportErrorEventArgs(
    string JobId,
    string ErrorMessage,
    Exception? Exception = null
);