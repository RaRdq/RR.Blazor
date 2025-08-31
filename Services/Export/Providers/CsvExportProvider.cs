using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RR.Blazor.Services.Export.Providers;

/// <summary>
/// CSV export provider implementation
/// </summary>
public class CsvExportProvider : IExportProvider
{
    public string Name => "CSV";
    public int Priority => 100;
    public List<ExportFormat> SupportedFormats => new() { ExportFormat.CSV, ExportFormat.TSV };
    
    public event Action<ExportProgress> ProgressChanged;
    
    public async Task<ExportResult> ExportAsync<T>(IEnumerable<T> data, ExportOptions options, CancellationToken cancellationToken = default)
    {
        try
        {
            var dataList = data.ToList();
            if (!dataList.Any())
            {
                return new ExportResult
                {
                    Success = false,
                    ErrorMessage = "No data to export",
                    Format = options.Format
                };
            }
            
            var delimiter = options.Format == ExportFormat.TSV ? "\t" : (options.Delimiter ?? ",");
            var progress = new ExportProgress { TotalRows = dataList.Count };
            
            using var memoryStream = new MemoryStream();
            using var writer = new StreamWriter(memoryStream, Encoding.GetEncoding(options.Encoding ?? "UTF-8"));
            
            var properties = GetExportableProperties<T>(options);
            var columnCount = properties.Count;
            
            // Write headers
            if (options.IncludeHeaders)
            {
                var headers = properties.Select(p => GetColumnName(p, options));
                await writer.WriteLineAsync(string.Join(delimiter, headers.Select(h => FormatValue(h, options.UseQuotes))));
            }
            
            // Write data rows
            var rowIndex = 0;
            foreach (var item in dataList)
            {
                cancellationToken.ThrowIfCancellationRequested();
                
                if (rowIndex >= options.MaxRows)
                    break;
                
                var values = new List<string>();
                foreach (var prop in properties)
                {
                    var value = prop.GetValue(item);
                    var formattedValue = FormatPropertyValue(value, prop, options);
                    values.Add(FormatValue(formattedValue, options.UseQuotes));
                }
                
                await writer.WriteLineAsync(string.Join(delimiter, values));
                
                rowIndex++;
                progress.CurrentRow = rowIndex;
                progress.CurrentOperation = $"Exporting row {rowIndex} of {dataList.Count}";
                ProgressChanged?.Invoke(progress);
                
                // Yield periodically for UI responsiveness
                if (rowIndex % 100 == 0)
                {
                    await Task.Yield();
                }
            }
            
            await writer.FlushAsync();
            var resultData = memoryStream.ToArray();
            
            return new ExportResult
            {
                Success = true,
                Data = resultData,
                FileName = options.FileName ?? $"export_{DateTime.Now:yyyyMMddHHmmss}.csv",
                MimeType = options.Format == ExportFormat.TSV ? "text/tab-separated-values" : "text/csv",
                Format = options.Format,
                RowCount = rowIndex,
                ColumnCount = columnCount,
                FileSize = resultData.Length,
                Metadata = new Dictionary<string, object>
                {
                    ["Delimiter"] = delimiter,
                    ["Encoding"] = options.Encoding ?? "UTF-8",
                    ["HasHeaders"] = options.IncludeHeaders
                }
            };
        }
        catch (Exception ex)
        {
            return new ExportResult
            {
                Success = false,
                ErrorMessage = $"CSV export failed: {ex.Message}",
                Format = options.Format
            };
        }
    }
    
    public bool CanExport<T>(IEnumerable<T> data, ExportFormat format)
    {
        return SupportedFormats.Contains(format);
    }
    
    public ExportValidationResult ValidateOptions<T>(IEnumerable<T> data, ExportOptions options)
    {
        var result = new ExportValidationResult { IsValid = true };
        
        if (!SupportedFormats.Contains(options.Format))
        {
            result.IsValid = false;
            result.Errors.Add($"Format {options.Format} is not supported by CSV provider");
        }
        
        if (options.MaxRows <= 0)
        {
            result.IsValid = false;
            result.Errors.Add("MaxRows must be greater than 0");
        }
        
        if (string.IsNullOrEmpty(options.Encoding))
        {
            result.Warnings.Add("No encoding specified, using UTF-8");
        }
        
        return result;
    }
    
    public ExportOptions GetDefaultOptions(ExportFormat format)
    {
        return new ExportOptions
        {
            Format = format,
            IncludeHeaders = true,
            Delimiter = format == ExportFormat.TSV ? "\t" : ",",
            UseQuotes = true,
            Encoding = "UTF-8",
            DateFormat = "yyyy-MM-dd",
            NumberFormat = "F2",
            CurrencyFormat = "C2",
            BooleanTrueValue = "True",
            BooleanFalseValue = "False",
            NullValue = string.Empty
        };
    }
    
    private static List<PropertyInfo> GetExportableProperties<T>(ExportOptions options)
    {
        var type = typeof(T);
        var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.CanRead)
            .OrderBy(p => p.Name)
            .ToList();
        
        // Apply include filter
        if (options.IncludeColumns?.Any() == true)
        {
            properties = properties.Where(p => options.IncludeColumns.Contains(p.Name)).ToList();
        }
        
        // Apply exclude filter
        if (options.ExcludeColumns?.Any() == true)
        {
            properties = properties.Where(p => !options.ExcludeColumns.Contains(p.Name)).ToList();
        }
        
        return properties;
    }
    
    private static string GetColumnName(PropertyInfo property, ExportOptions options)
    {
        if (options.ColumnMappings?.TryGetValue(property.Name, out var mappedName) == true)
        {
            return mappedName;
        }
        
        return property.Name;
    }
    
    private static string FormatPropertyValue(object value, PropertyInfo property, ExportOptions options)
    {
        // Check for custom formatter
        if (options.CustomFormatters?.TryGetValue(property.Name, out var formatter) == true)
        {
            return formatter(value);
        }
        
        // Handle null values
        if (value == null)
        {
            return options.NullValue ?? string.Empty;
        }
        
        // Format based on type
        return value switch
        {
            DateTime dateTime => dateTime.ToString(options.DateFormat, CultureInfo.InvariantCulture),
            DateTimeOffset dateTimeOffset => dateTimeOffset.ToString(options.DateFormat, CultureInfo.InvariantCulture),
            bool boolValue => boolValue ? options.BooleanTrueValue : options.BooleanFalseValue,
            decimal decimalValue => IsCurrencyProperty(property) 
                ? decimalValue.ToString(options.CurrencyFormat, CultureInfo.InvariantCulture)
                : decimalValue.ToString(options.NumberFormat, CultureInfo.InvariantCulture),
            double doubleValue => doubleValue.ToString(options.NumberFormat, CultureInfo.InvariantCulture),
            float floatValue => floatValue.ToString(options.NumberFormat, CultureInfo.InvariantCulture),
            _ => value.ToString()
        };
    }
    
    private static bool IsCurrencyProperty(PropertyInfo property)
    {
        var name = property.Name.ToLowerInvariant();
        return name.Contains("price") || name.Contains("cost") || name.Contains("amount") || 
               name.Contains("salary") || name.Contains("payment") || name.Contains("currency");
    }
    
    private static string FormatValue(string value, bool useQuotes)
    {
        if (!useQuotes)
            return value;
        
        // Escape quotes in value
        value = value?.Replace("\"", "\"\"") ?? string.Empty;
        
        // Quote if contains delimiter, quotes, or newlines
        if (value.Contains(',') || value.Contains('"') || value.Contains('\n') || value.Contains('\r'))
        {
            return $"\"{value}\"";
        }
        
        return value;
    }
}