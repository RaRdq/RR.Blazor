using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace RR.Blazor.Services.Export.Providers;

/// <summary>
/// JSON export provider implementation
/// </summary>
public class JsonExportProvider : IStreamingExportProvider
{
    public string Name => "JSON";
    public int Priority => 80;
    public List<ExportFormat> SupportedFormats => new() { ExportFormat.JSON };
    public bool SupportsStreaming => true;
    
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
                    Format = ExportFormat.JSON
                };
            }
            
            var progress = new ExportProgress { TotalRows = dataList.Count };
            
            var settings = GetJsonSettings(options);
            
            // Apply column filtering if specified
            var filteredData = options.IncludeColumns?.Any() == true || options.ExcludeColumns?.Any() == true
                ? FilterData(dataList, options)
                : dataList.Cast<object>();
            
            // Serialize data
            var json = await Task.Run(() => JsonConvert.SerializeObject(filteredData, settings), cancellationToken);
            var resultData = Encoding.UTF8.GetBytes(json);
            
            // Compress if requested
            if (options.CompressOutput)
            {
                resultData = await CompressData(resultData);
            }
            
            progress.CurrentRow = dataList.Count;
            progress.CurrentOperation = "Export completed";
            ProgressChanged?.Invoke(progress);
            
            return new ExportResult
            {
                Success = true,
                Data = resultData,
                FileName = options.FileName ?? $"export_{DateTime.Now:yyyyMMddHHmmss}.json",
                MimeType = "application/json",
                Format = ExportFormat.JSON,
                RowCount = dataList.Count,
                FileSize = resultData.Length,
                Metadata = new Dictionary<string, object>
                {
                    ["Compressed"] = options.CompressOutput,
                    ["Indented"] = settings.Formatting == Formatting.Indented
                }
            };
        }
        catch (Exception ex)
        {
            return new ExportResult
            {
                Success = false,
                ErrorMessage = $"JSON export failed: {ex.Message}",
                Format = ExportFormat.JSON
            };
        }
    }
    
    public async IAsyncEnumerable<byte[]> ExportStreamAsync<T>(IAsyncEnumerable<T> data, ExportOptions options, CancellationToken cancellationToken = default)
    {
        var settings = GetJsonSettings(options);
        var isFirst = true;
        
        // Start JSON array
        yield return Encoding.UTF8.GetBytes("[");
        
        await foreach (var item in data.WithCancellation(cancellationToken))
        {
            if (!isFirst)
            {
                yield return Encoding.UTF8.GetBytes(",");
            }
            
            var filteredItem = options.IncludeColumns?.Any() == true || options.ExcludeColumns?.Any() == true
                ? FilterItem(item, options)
                : item;
            
            var json = JsonConvert.SerializeObject(filteredItem, settings);
            yield return Encoding.UTF8.GetBytes(json);
            
            isFirst = false;
        }
        
        // End JSON array
        yield return Encoding.UTF8.GetBytes("]");
    }
    
    public bool CanExport<T>(IEnumerable<T> data, ExportFormat format)
    {
        return format == ExportFormat.JSON;
    }
    
    public ExportValidationResult ValidateOptions<T>(IEnumerable<T> data, ExportOptions options)
    {
        var result = new ExportValidationResult { IsValid = true };
        
        if (options.Format != ExportFormat.JSON)
        {
            result.IsValid = false;
            result.Errors.Add("JSON provider only supports JSON format");
        }
        
        return result;
    }
    
    public ExportOptions GetDefaultOptions(ExportFormat format)
    {
        return new ExportOptions
        {
            Format = ExportFormat.JSON,
            DateFormat = "yyyy-MM-dd'T'HH:mm:ss'Z'",
            NullValue = null,
            ProviderSpecificOptions = new Dictionary<string, object>
            {
                ["Indented"] = true,
                ["CamelCase"] = true,
                ["IncludeTypeInfo"] = false
            }
        };
    }
    
    private static JsonSerializerSettings GetJsonSettings(ExportOptions options)
    {
        var indented = options.ProviderSpecificOptions?.TryGetValue("Indented", out var indentedValue) == true 
            && indentedValue is bool indent ? indent : true;
        
        var camelCase = options.ProviderSpecificOptions?.TryGetValue("CamelCase", out var camelCaseValue) == true 
            && camelCaseValue is bool camel ? camel : true;
        
        var includeTypeInfo = options.ProviderSpecificOptions?.TryGetValue("IncludeTypeInfo", out var typeInfoValue) == true 
            && typeInfoValue is bool typeInfo && typeInfo;
        
        var settings = new JsonSerializerSettings
        {
            Formatting = indented ? Formatting.Indented : Formatting.None,
            ContractResolver = camelCase ? new CamelCasePropertyNamesContractResolver() : new DefaultContractResolver(),
            DateFormatString = options.DateFormat,
            NullValueHandling = string.IsNullOrEmpty(options.NullValue) ? NullValueHandling.Include : NullValueHandling.Ignore,
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
        };
        
        if (includeTypeInfo)
        {
            settings.TypeNameHandling = TypeNameHandling.Auto;
        }
        
        // Add custom converters
        settings.Converters.Add(new DecimalFormatConverter(options.NumberFormat));
        settings.Converters.Add(new BooleanFormatConverter(options.BooleanTrueValue, options.BooleanFalseValue));
        
        return settings;
    }
    
    private static IEnumerable<object> FilterData<T>(IEnumerable<T> data, ExportOptions options)
    {
        foreach (var item in data)
        {
            yield return FilterItem(item, options);
        }
    }
    
    private static object FilterItem<T>(T item, ExportOptions options)
    {
        var type = typeof(T);
        var properties = type.GetProperties()
            .Where(p => p.CanRead)
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
        
        var result = new Dictionary<string, object>();
        foreach (var prop in properties)
        {
            var name = options.ColumnMappings?.TryGetValue(prop.Name, out var mappedName) == true 
                ? mappedName 
                : prop.Name;
            
            var value = prop.GetValue(item);
            
            // Apply custom formatter if available
            if (options.CustomFormatters?.TryGetValue(prop.Name, out var formatter) == true)
            {
                value = formatter(value);
            }
            
            result[name] = value;
        }
        
        return result;
    }
    
    private static async Task<byte[]> CompressData(byte[] data)
    {
        using var output = new MemoryStream();
        using (var gzip = new System.IO.Compression.GZipStream(output, System.IO.Compression.CompressionLevel.Optimal))
        {
            await gzip.WriteAsync(data, 0, data.Length);
        }
        return output.ToArray();
    }
    
    /// <summary>
    /// Custom converter for decimal formatting
    /// </summary>
    private class DecimalFormatConverter : JsonConverter<decimal>
    {
        private readonly string format;
        
        public DecimalFormatConverter(string format)
        {
            this.format = format ?? "F2";
        }
        
        public override void WriteJson(JsonWriter writer, decimal value, JsonSerializer serializer)
        {
            writer.WriteValue(value.ToString(format));
        }
        
        public override decimal ReadJson(JsonReader reader, Type objectType, decimal existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            return decimal.Parse(reader.Value.ToString());
        }
    }
    
    /// <summary>
    /// Custom converter for boolean formatting
    /// </summary>
    private class BooleanFormatConverter : JsonConverter<bool>
    {
        private readonly string trueValue;
        private readonly string falseValue;
        
        public BooleanFormatConverter(string trueValue, string falseValue)
        {
            this.trueValue = trueValue ?? "true";
            this.falseValue = falseValue ?? "false";
        }
        
        public override void WriteJson(JsonWriter writer, bool value, JsonSerializer serializer)
        {
            writer.WriteValue(value ? trueValue : falseValue);
        }
        
        public override bool ReadJson(JsonReader reader, Type objectType, bool existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            var value = reader.Value?.ToString();
            return value?.Equals(trueValue, StringComparison.OrdinalIgnoreCase) == true;
        }
    }
}