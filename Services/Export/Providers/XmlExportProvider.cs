using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using Microsoft.Extensions.Logging;

namespace RR.Blazor.Services.Export.Providers;

/// <summary>
/// XML export provider for data serialization
/// </summary>
public class XmlExportProvider : IExportProvider
{
    private readonly ILogger<XmlExportProvider> logger;
    
    public string Name => "XML Export Provider";
    public int Priority => 50;
    public List<ExportFormat> SupportedFormats => new() { ExportFormat.XML };
    
    public event Action<ExportProgress> ProgressChanged;
    
    public XmlExportProvider(ILogger<XmlExportProvider> logger)
    {
        this.logger = logger;
    }
    
    public async Task<ExportResult> ExportAsync<T>(IEnumerable<T> data, ExportOptions options, CancellationToken cancellationToken = default)
    {
        try
        {
            var dataList = data.ToList();
            var totalRows = dataList.Count;
            var processedRows = 0;
            
            ReportProgress(new ExportProgress
            {
                CurrentRow = 0,
                TotalRows = totalRows,
                CurrentOperation = "Preparing XML export..."
            });
            
            using var memoryStream = new MemoryStream();
            var settings = new XmlWriterSettings
            {
                Indent = true,
                IndentChars = "  ",
                Encoding = Encoding.UTF8,
                NewLineChars = Environment.NewLine,
                NewLineHandling = NewLineHandling.Replace
            };
            
            using (var writer = XmlWriter.Create(memoryStream, settings))
            {
                writer.WriteStartDocument();
                writer.WriteStartElement("Data");
                writer.WriteAttributeString("ExportDate", DateTime.UtcNow.ToString("O"));
                writer.WriteAttributeString("TotalRecords", totalRows.ToString());
                
                if (options.IncludeMetadata)
                {
                    writer.WriteStartElement("Metadata");
                    writer.WriteElementString("Generated", DateTime.UtcNow.ToString("O"));
                    writer.WriteElementString("RecordCount", totalRows.ToString());
                    writer.WriteElementString("Format", "XML");
                    writer.WriteEndElement();
                }
                
                writer.WriteStartElement("Records");
                
                foreach (var item in dataList.Take(options.MaxRows))
                {
                    if (cancellationToken.IsCancellationRequested)
                        break;
                    
                    writer.WriteStartElement("Record");
                    
                    var properties = typeof(T).GetProperties()
                        .Where(p => p.CanRead)
                        .Where(p => options.IncludeColumns == null || options.IncludeColumns.Contains(p.Name))
                        .Where(p => options.ExcludeColumns == null || !options.ExcludeColumns.Contains(p.Name));
                    
                    foreach (var property in properties)
                    {
                        var value = property.GetValue(item);
                        var elementName = options.ColumnMappings?.ContainsKey(property.Name) == true
                            ? options.ColumnMappings[property.Name]
                            : property.Name;
                        
                        if (value == null)
                        {
                            writer.WriteElementString(elementName, options.NullValue);
                        }
                        else if (options.CustomFormatters?.ContainsKey(property.Name) == true)
                        {
                            var formatted = options.CustomFormatters[property.Name](value);
                            writer.WriteElementString(elementName, formatted);
                        }
                        else
                        {
                            writer.WriteElementString(elementName, FormatValue(value, property.PropertyType, options));
                        }
                    }
                    
                    writer.WriteEndElement(); // Record
                    
                    processedRows++;
                    if (processedRows % 100 == 0)
                    {
                        ReportProgress(new ExportProgress
                        {
                            CurrentRow = processedRows,
                            TotalRows = totalRows,
                            CurrentOperation = $"Exporting records... {processedRows}/{totalRows}"
                        });
                    }
                }
                
                writer.WriteEndElement(); // Records
                writer.WriteEndElement(); // Data
                writer.WriteEndDocument();
                writer.Flush();
            }
            
            var xmlBytes = memoryStream.ToArray();
            
            // Apply compression if requested
            if (options.CompressOutput)
            {
                xmlBytes = await CompressData(xmlBytes);
            }
            
            logger.LogInformation($"XML export completed: {processedRows} rows exported");
            
            return new ExportResult
            {
                Success = true,
                Data = xmlBytes,
                FileName = options.FileName ?? $"export_{DateTime.Now:yyyyMMddHHmmss}.xml",
                MimeType = "application/xml",
                Format = ExportFormat.XML,
                RowCount = processedRows,
                FileSize = xmlBytes.Length,
                ProcessingTime = TimeSpan.Zero
            };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "XML export failed");
            return new ExportResult
            {
                Success = false,
                ErrorMessage = $"XML export failed: {ex.Message}",
                Format = ExportFormat.XML
            };
        }
    }
    
    public bool CanExport<T>(IEnumerable<T> data, ExportFormat format)
    {
        return format == ExportFormat.XML;
    }
    
    public ExportValidationResult ValidateOptions<T>(IEnumerable<T> data, ExportOptions options)
    {
        var result = new ExportValidationResult { IsValid = true };
        
        if (options.Format != ExportFormat.XML)
        {
            result.IsValid = false;
            result.Errors.Add("Format must be XML");
        }
        
        if (options.MaxRows < 0)
        {
            result.IsValid = false;
            result.Errors.Add("MaxRows cannot be negative");
        }
        
        return result;
    }
    
    public ExportOptions GetDefaultOptions(ExportFormat format)
    {
        return new ExportOptions
        {
            Format = ExportFormat.XML,
            IncludeHeaders = true,
            IncludeMetadata = true,
            DateFormat = "yyyy-MM-dd",
            NullValue = "",
            MaxRows = int.MaxValue
        };
    }
    
    private string FormatValue(object value, Type type, ExportOptions options)
    {
        return type switch
        {
            _ when type == typeof(DateTime) || type == typeof(DateTime?) => 
                ((DateTime)value).ToString(options.DateFormat),
            _ when type == typeof(DateTimeOffset) || type == typeof(DateTimeOffset?) => 
                ((DateTimeOffset)value).ToString(options.DateFormat),
            _ when type == typeof(bool) || type == typeof(bool?) => 
                (bool)value ? options.BooleanTrueValue : options.BooleanFalseValue,
            _ when type == typeof(decimal) || type == typeof(decimal?) => 
                ((decimal)value).ToString(options.NumberFormat),
            _ when type == typeof(double) || type == typeof(double?) => 
                ((double)value).ToString(options.NumberFormat),
            _ when type == typeof(float) || type == typeof(float?) => 
                ((float)value).ToString(options.NumberFormat),
            _ => value.ToString()
        };
    }
    
    private async Task<byte[]> CompressData(byte[] data)
    {
        using var output = new MemoryStream();
        using (var gzip = new System.IO.Compression.GZipStream(output, System.IO.Compression.CompressionLevel.Optimal))
        {
            await gzip.WriteAsync(data, 0, data.Length);
        }
        return output.ToArray();
    }
    
    private void ReportProgress(ExportProgress progress)
    {
        ProgressChanged?.Invoke(progress);
    }
}