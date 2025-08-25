using System;
using System.Collections.Generic;

namespace RR.Blazor.Services.Export;

/// <summary>
/// Supported export formats
/// </summary>
public enum ExportFormat
{
    None = 0,
    CSV = 1,
    Excel = 2,
    JSON = 4,
    PDF = 8,
    XML = 16,
    TSV = 32,
    PowerBI = 64,
    YAML = 128,
    HTML = 256
}

/// <summary>
/// Export operation result
/// </summary>
public record ExportResult
{
    public bool Success { get; init; }
    public byte[] Data { get; init; }
    public string FileName { get; init; }
    public string MimeType { get; init; }
    public ExportFormat Format { get; init; }
    public string ErrorMessage { get; init; }
    public int RowCount { get; init; }
    public int ColumnCount { get; init; }
    public long FileSize { get; init; }
    public TimeSpan ProcessingTime { get; init; }
    public Dictionary<string, object> Metadata { get; init; } = new();
}

/// <summary>
/// Export configuration options
/// </summary>
public class ExportOptions
{
    public ExportFormat Format { get; set; } = ExportFormat.CSV;
    public string FileName { get; set; }
    public bool IncludeHeaders { get; set; } = true;
    public bool IncludeMetadata { get; set; } = true;
    public string DateFormat { get; set; } = "yyyy-MM-dd";
    public string NumberFormat { get; set; } = "F2";
    public string CurrencyFormat { get; set; } = "C2";
    public string BooleanTrueValue { get; set; } = "Yes";
    public string BooleanFalseValue { get; set; } = "No";
    public string NullValue { get; set; } = string.Empty;
    public string Delimiter { get; set; } = ",";
    public bool UseQuotes { get; set; } = true;
    public string Encoding { get; set; } = "UTF-8";
    public List<string> IncludeColumns { get; set; }
    public List<string> ExcludeColumns { get; set; }
    public Dictionary<string, string> ColumnMappings { get; set; } = new();
    public Dictionary<string, Func<object, string>> CustomFormatters { get; set; } = new();
    public bool CompressOutput { get; set; }
    public int MaxRows { get; set; } = int.MaxValue;
    public string SheetName { get; set; } = "Sheet1";
    public bool AutoFitColumns { get; set; } = true;
    public bool FreezePanes { get; set; } = true;
    public Dictionary<string, object> ProviderSpecificOptions { get; set; } = new();
}

/// <summary>
/// Column metadata for export
/// </summary>
public class ExportColumn
{
    public string PropertyName { get; set; }
    public string DisplayName { get; set; }
    public Type DataType { get; set; }
    public int Order { get; set; }
    public bool IsVisible { get; set; } = true;
    public string Format { get; set; }
    public int? Width { get; set; }
    public Func<object, string> CustomFormatter { get; set; }
}

/// <summary>
/// Export field configuration
/// </summary>
public class ExportFieldConfig
{
    public string FieldName { get; set; }
    public string Header { get; set; }
    public Type DataType { get; set; }
    public string Format { get; set; }
    public bool IsExportable { get; set; } = true;
    public int Order { get; set; }
    public Func<object, object> ValueTransformer { get; set; }
}

/// <summary>
/// Batch export request for multiple datasets
/// </summary>
public class BatchExportRequest
{
    public List<ExportDataset> Datasets { get; set; } = new();
    public ExportFormat Format { get; set; } = ExportFormat.Excel;
    public string FileName { get; set; }
    public bool CreateSeparateFiles { get; set; }
    public bool CompressOutput { get; set; }
}

/// <summary>
/// Individual dataset for batch export
/// </summary>
public class ExportDataset
{
    public string Name { get; set; }
    public object Data { get; set; }
    public ExportOptions Options { get; set; }
    public Dictionary<string, object> Metadata { get; set; } = new();
}

/// <summary>
/// Export progress tracking
/// </summary>
public class ExportProgress
{
    public int CurrentRow { get; set; }
    public int TotalRows { get; set; }
    public string CurrentOperation { get; set; }
    public double PercentComplete => TotalRows > 0 ? (CurrentRow * 100.0 / TotalRows) : 0;
    public TimeSpan ElapsedTime { get; set; }
    public TimeSpan EstimatedTimeRemaining { get; set; }
}

/// <summary>
/// Export validation result
/// </summary>
public class ExportValidationResult
{
    public bool IsValid { get; set; }
    public List<string> Errors { get; set; } = new();
    public List<string> Warnings { get; set; } = new();
    public Dictionary<string, object> ValidationMetadata { get; set; } = new();
}

/// <summary>
/// Generic export configuration with custom settings
/// </summary>
public class ExportConfiguration<T>
{
    public ExportFormat Format { get; set; }
    public string FileName { get; set; }
    public int MaxRows { get; set; }
    public int MaxColumns { get; set; }
    public T CustomConfiguration { get; set; }
}