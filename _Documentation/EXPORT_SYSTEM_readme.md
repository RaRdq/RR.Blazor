# RR.Blazor Export System

## Overview
The RR.Blazor Export System provides a flexible, extensible architecture for exporting data to various formats. Export functionality is **enabled by default** following RR.Blazor's inverted configuration principle - you only need to configure if you want to disable or customize features.

## Quick Start

### 1. Automatic Registration (Default Behavior)

```csharp
// Export is ENABLED by default when you add RR.Blazor
builder.Services.AddRRBlazor();

// This automatically includes:
// - CSV Export Provider ✓
// - JSON Export Provider ✓  
// - XML Export Provider ✓
// - Excel Export Provider x
```

### 2. Inverted Configuration (Only Configure to Disable)

```csharp
// ONLY configure if you want to disable export
builder.Services.AddRRBlazor(config => 
    config.DisableExport()  // Completely disable export features
);

// Or customize export options
builder.Services.AddRRBlazor(config => 
    config.WithExportOptions(export =>
    {
        export.DisableExcel = true;  // Disable Excel only
        export.AdditionalProviders.Add(typeof(MyCustomProvider));
        export.DefaultOptions.DateFormat = "yyyy-MM-dd";
    })
);
```

### 3. Use in Components (No Setup Required)

#### Grid Export
```csharp
@inject IExportService ExportService

// Export grid data
private async Task ExportGrid()
{
    var options = new ExportOptions
    {
        Format = ExportFormat.Excel,
        FileName = "grid_data.xlsx",
        IncludeHeaders = true,
        SheetName = "Grid Data"
    };
    
    var result = await ExportService.ExportGridDataAsync(
        gridData,
        gridColumns,
        options
    );
    
    if (result.Success)
    {
        // Download the file
        await DownloadFile(result.Data, result.FileName, result.MimeType);
    }
}
```

#### Pivot Export
```csharp
// Export pivot data
private async Task ExportPivot()
{
    var options = new ExportOptions
    {
        Format = ExportFormat.CSV,
        FileName = "pivot_data.csv",
        IncludeHeaders = true
    };
    
    var result = await ExportService.ExportPivotDataAsync(
        pivotResult,
        options
    );
    
    if (result.Success)
    {
        await DownloadFile(result.Data, result.FileName, result.MimeType);
    }
}
```

### 4. Available Export Formats

- **CSV** - Always included, no dependencies ✓
- **JSON** - Always included with Newtonsoft.Json ✓
- **XML** - Always included, built-in serialization ✓
- **PDF** - Add custom provider (see examples below)

## Creating Custom Export Providers

### Example: Custom PDF Provider

```csharp
using QuestPDF;
using RR.Blazor.Services.Export;
using RR.Blazor.Services.Export.Providers;

public class QuestPdfExportProvider : PdfExportProviderBase
{
    public override string Name => "QuestPDF Provider";
    
    public override async Task<ExportResult> ExportAsync<T>(
        IEnumerable<T> data, 
        ExportOptions options, 
        CancellationToken cancellationToken = default)
    {
        // Implement QuestPDF logic here
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                // Build your PDF layout
            });
        });
        
        var pdfBytes = document.GeneratePdf();
        
        return new ExportResult
        {
            Success = true,
            Data = pdfBytes,
            FileName = options.FileName ?? "export.pdf",
            MimeType = "application/pdf",
            Format = ExportFormat.PDF
        };
    }
    
    // Implement other abstract methods...
}
```

### Register Custom Provider

```csharp
// Custom providers are automatically discovered!
// Just implement IExportProvider and register:
builder.Services.AddRRBlazor(config =>
    config.WithExportOptions(export =>
    {
        export.AdditionalProviders.Add(typeof(QuestPdfExportProvider));
    })
);

// Or register directly (provider will be auto-discovered)
builder.Services.AddSingleton<IExportProvider, QuestPdfExportProvider>();
```

## Advanced Features

### 1. Batch Export (Multiple Sheets/Files)

```csharp
var batchRequest = new BatchExportRequest
{
    Format = ExportFormat.Excel,
    FileName = "batch_export.xlsx",
    Datasets = new List<ExportDataset>
    {
        new() { Name = "Sheet1", Data = dataset1 },
        new() { Name = "Sheet2", Data = dataset2 }
    }
};

var result = await exportService.ExportBatchAsync(batchRequest);
```

### 2. Streaming Export (Large Datasets)

```csharp
// For memory-efficient large data exports
await foreach (var chunk in exportService.ExportStreamAsync(largeDataStream, options))
{
    // Process chunk
    await WriteToFile(chunk);
}
```

### 3. Export with Templates

```csharp
// Register a template
await templateService.RegisterTemplateAsync("MonthlyReport", new ExportTemplate
{
    Name = "Monthly Report",
    Format = ExportFormat.Excel,
    Fields = new List<ExportFieldConfig>
    {
        new() { FieldName = "Date", Header = "Transaction Date", Format = "MM/dd/yyyy" },
        new() { FieldName = "Amount", Header = "Total Amount", Format = "C2" }
    }
});

// Use the template
var result = await templateService.ExportWithTemplateAsync(data, "MonthlyReport");
```

### 4. Export with Caching

```csharp
// Cache export results for frequently requested data
var result = await cachedExportService.ExportWithCacheAsync(
    data,
    options,
    cacheKey: "monthly_report_2024",
    cacheDuration: TimeSpan.FromHours(1)
);
```

## Export Options

```csharp
var options = new ExportOptions
{
    // Basic options
    Format = ExportFormat.Excel,
    FileName = "export.xlsx",
    IncludeHeaders = true,
    MaxRows = 10000,
    
    // Formatting
    DateFormat = "yyyy-MM-dd",
    NumberFormat = "#,##0.00",
    CurrencyFormat = "$#,##0.00",
    BooleanTrueValue = "Yes",
    BooleanFalseValue = "No",
    NullValue = "",
    
    // Column control
    IncludeColumns = new List<string> { "Name", "Amount", "Date" },
    ExcludeColumns = new List<string> { "InternalId" },
    ColumnMappings = new Dictionary<string, string>
    {
        ["FirstName"] = "First Name",
        ["LastName"] = "Last Name"
    },
    
    // Excel-specific
    SheetName = "Data",
    AutoFitColumns = true,
    FreezePanes = true,
    
    // Performance
    CompressOutput = true
};
```

## Progress Tracking

```csharp
exportService.ExportProgressChanged += (progress) =>
{
    Console.WriteLine($"Export progress: {progress.PercentComplete}%");
    Console.WriteLine($"Current operation: {progress.CurrentOperation}");
};
```

## Error Handling

```csharp
var result = await exportService.ExportAsync(data, options);

if (!result.Success)
{
    Console.WriteLine($"Export failed: {result.ErrorMessage}");
}
else
{
    Console.WriteLine($"Exported {result.RowCount} rows, {result.FileSize} bytes");
}
```

## Architecture Overview

### Service Layers

1. **ICoreExportService** - Core engine managing providers
2. **IExportService** - High-level wrapper for components
3. **IExportProvider** - Provider interface for format implementations

### Provider Auto-Discovery

Providers implementing `IExportProvider` are automatically discovered and registered at startup. No manual registration needed for providers in the same assembly.

### Inverted Configuration Philosophy

Following RR.Blazor principles:
- Export is **enabled by default**
- Only configure to **disable or customize**
- Zero configuration for standard use cases
- Smart defaults that just work

## Performance Considerations

1. **Large Datasets**: Use streaming export for datasets > 10,000 rows
2. **Memory Usage**: Excel provider loads entire dataset in memory
3. **Compression**: Enable for exports > 10MB
4. **Caching**: Use for frequently requested, static data

## Troubleshooting

### Excel Export Not Available
Excel provider auto-detects ClosedXML. To enable:
```bash
dotnet add package ClosedXML
```
The provider will automatically activate after restart.

### Export Not Working
Check if export was explicitly disabled:
```csharp
// Remove any DisableExport() calls
builder.Services.AddRRBlazor(); // Export enabled by default
```

### Slow Export Performance
1. Reduce MaxRows in options
2. Use column filtering (IncludeColumns)
3. Enable compression for large files
4. Consider caching for static data
