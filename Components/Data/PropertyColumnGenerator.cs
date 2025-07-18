using System.Collections.Concurrent;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.Encodings.Web;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using RR.Blazor.Enums;

namespace RR.Blazor.Components.Data;

/// <summary>
/// High-performance utility class for generating data table columns automatically from model properties.
/// Uses reflection caching and compiled property accessors for optimal performance.
/// </summary>
public static class PropertyColumnGenerator
{
    // Static caches for performance optimization
    private static readonly ConcurrentDictionary<Type, List<ColumnMetadata>> _columnCache = new();
    private static readonly ConcurrentDictionary<Type, Dictionary<string, Func<object, object>>> _propertyAccessors = new();
    private static readonly ConcurrentDictionary<string, Func<object, string>> _formatters = new();
    private static readonly ConcurrentDictionary<Type, DateTime> _cacheAccessLog = new();
    /// <summary>
    /// Generate column definitions from a model type using optimized caching
    /// </summary>
    public static List<RDataTableGeneric<TItem>.RDataTableColumn<TItem>> GenerateColumns<TItem>()
    {
        var itemType = typeof(TItem);
        
        // Update access log for cache management
        _cacheAccessLog.AddOrUpdate(itemType, DateTime.UtcNow, (_, _) => DateTime.UtcNow);
        
        // Get cached column metadata or generate it
        var columnMetadata = _columnCache.GetOrAdd(itemType, _ => GenerateColumnMetadata<TItem>());
        
        // Ensure property accessors are cached
        _propertyAccessors.GetOrAdd(itemType, _ => GeneratePropertyAccessors<TItem>());
        
        return BuildColumnsFromMetadata<TItem>(columnMetadata);
    }

    /// <summary>
    /// Generate cached column metadata for a type
    /// </summary>
    private static List<ColumnMetadata> GenerateColumnMetadata<TItem>()
    {
        var type = typeof(TItem);
        var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.CanRead && ShouldIncludeProperty(p));
        
        var metadata = new List<ColumnMetadata>();
        
        foreach (var property in properties)
        {
            metadata.Add(new ColumnMetadata
            {
                PropertyName = property.Name,
                DisplayName = GetDisplayName(property),
                PropertyType = property.PropertyType,
                DisplayOrder = GetDisplayOrder(property),
                Width = GetColumnWidth(property),
                Sortable = IsSortable(property),
                Filterable = IsFilterable(property),
                DisplayFormat = property.GetCustomAttribute<DisplayFormatAttribute>()?.DataFormatString,
                IsEmail = property.Name.Contains("Email", StringComparison.OrdinalIgnoreCase),
                IsUrl = property.Name.Contains("Url", StringComparison.OrdinalIgnoreCase),
                IsPhone = property.Name.Contains("Phone", StringComparison.OrdinalIgnoreCase)
            });
        }
        
        return metadata.OrderBy(m => m.DisplayOrder).ToList();
    }

    /// <summary>
    /// Generate compiled property accessors for fast property access
    /// </summary>
    private static Dictionary<string, Func<object, object>> GeneratePropertyAccessors<TItem>()
    {
        var type = typeof(TItem);
        var accessors = new Dictionary<string, Func<object, object>>();
        var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.CanRead);
        
        foreach (var property in properties)
        {
            try
            {
                var parameter = Expression.Parameter(typeof(object), "obj");
                var convert = Expression.Convert(parameter, type);
                var propertyAccess = Expression.Property(convert, property);
                var convertResult = Expression.Convert(propertyAccess, typeof(object));
                var lambda = Expression.Lambda<Func<object, object>>(convertResult, parameter);
                
                accessors[property.Name] = lambda.Compile();
            }
            catch
            {
                // Fallback to reflection for problematic properties
                var prop = property;
                accessors[property.Name] = obj => prop.GetValue(obj);
            }
        }
        
        return accessors;
    }

    /// <summary>
    /// Build columns from cached metadata
    /// </summary>
    private static List<RDataTableGeneric<TItem>.RDataTableColumn<TItem>> BuildColumnsFromMetadata<TItem>(List<ColumnMetadata> metadata)
    {
        var columns = new List<RDataTableGeneric<TItem>.RDataTableColumn<TItem>>();
        
        foreach (var meta in metadata)
        {
            var column = new RDataTableGeneric<TItem>.RDataTableColumn<TItem>
            {
                Key = meta.PropertyName,
                Header = meta.DisplayName,
                Sortable = meta.Sortable,
                Filterable = meta.Filterable,
                Width = meta.Width,
                CellTemplate = CreateOptimizedCellTemplate<TItem>(meta),
                ValueFunc = CreatePropertyValueFunc<TItem>(meta.PropertyName)
            };
            
            columns.Add(column);
        }
        
        return columns;
    }

    /// <summary>
    /// Create property value function using compiled accessors
    /// </summary>
    private static Func<TItem, object> CreatePropertyValueFunc<TItem>(string propertyName)
    {
        var itemType = typeof(TItem);
        var accessors = _propertyAccessors.GetOrAdd(itemType, _ => GeneratePropertyAccessors<TItem>());
        
        if (accessors.TryGetValue(propertyName, out var accessor))
        {
            return item => accessor(item);
        }
        
        // Fallback to reflection
        var property = itemType.GetProperty(propertyName);
        return item => property?.GetValue(item);
    }

    /// <summary>
    /// Create optimized render fragment using compiled accessors
    /// </summary>
    private static RenderFragment<TItem> CreateOptimizedCellTemplate<TItem>(ColumnMetadata metadata)
    {
        var itemType = typeof(TItem);
        
        return item => builder =>
        {
            var accessors = _propertyAccessors.GetOrAdd(itemType, _ => GeneratePropertyAccessors<TItem>());
            var value = accessors.TryGetValue(metadata.PropertyName, out var accessor) 
                ? accessor(item) 
                : GetCellValueFallback(item, metadata.PropertyName);
            var formattedValue = FormatValueOptimized(value, metadata);
            
            if (metadata.IsEmail && !string.IsNullOrEmpty(formattedValue) && IsValidEmail(formattedValue))
            {
                builder.AddMarkupContent(0, CreateSecureEmailLink(formattedValue));
            }
            else if (metadata.IsUrl && !string.IsNullOrEmpty(formattedValue) && IsValidUrl(formattedValue))
            {
                builder.AddMarkupContent(0, CreateSecureUrlLink(formattedValue));
            }
            else
            {
                builder.AddContent(0, formattedValue);
            }
        };
    }

    /// <summary>
    /// Fallback method to get cell value using reflection
    /// </summary>
    private static object GetCellValueFallback<TItem>(TItem item, string propertyName)
    {
        if (item == null) return null;
        
        try
        {
            var property = typeof(TItem).GetProperty(propertyName);
            return property?.GetValue(item);
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Optimized formatting using cached formatters
    /// </summary>
    private static string FormatValueOptimized(object value, ColumnMetadata metadata)
    {
        if (value == null) return "";

        // Check for custom format
        if (!string.IsNullOrEmpty(metadata.DisplayFormat))
        {
            try
            {
                return string.Format(metadata.DisplayFormat, value);
            }
            catch
            {
                // Fallback if format fails
            }
        }

        // Use cached formatter
        var cacheKey = GetFormatterCacheKey(metadata);
        var formatter = _formatters.GetOrAdd(cacheKey, _ => CreateFormatter(metadata));
        
        return formatter(value);
    }

    /// <summary>
    /// Create cache key for formatters
    /// </summary>
    private static string GetFormatterCacheKey(ColumnMetadata metadata)
    {
        var type = Nullable.GetUnderlyingType(metadata.PropertyType) ?? metadata.PropertyType;
        return $"{type.FullName}_{metadata.IsEmail}_{metadata.IsUrl}_{metadata.IsPhone}_{metadata.DisplayFormat}";
    }

    /// <summary>
    /// Create compiled formatter function
    /// </summary>
    private static Func<object, string> CreateFormatter(ColumnMetadata metadata)
    {
        var type = Nullable.GetUnderlyingType(metadata.PropertyType) ?? metadata.PropertyType;
        
        // Special string formatting with security validation
        if (type == typeof(string))
        {
            if (metadata.IsEmail)
                return obj => CreateSecureEmailLink(obj?.ToString() ?? "");
            if (metadata.IsUrl)
                return obj => CreateSecureUrlLink(obj?.ToString() ?? "");
            if (metadata.IsPhone)
                return obj => FormatPhoneNumber(obj?.ToString() ?? "");
        }
        
        // Type-specific formatters
        return type switch
        {
            Type t when t == typeof(DateTime) => obj => ((DateTime)obj).ToString("MMM dd, yyyy"),
            Type t when t == typeof(DateTimeOffset) => obj => ((DateTimeOffset)obj).ToString("MMM dd, yyyy"),
            Type t when t == typeof(DateOnly) => obj => ((DateOnly)obj).ToString("MMM dd, yyyy"),
            Type t when t == typeof(TimeOnly) => obj => ((TimeOnly)obj).ToString("HH:mm"),
            Type t when t == typeof(TimeSpan) => obj => ((TimeSpan)obj).ToString(@"hh\:mm"),
            Type t when t == typeof(decimal) => obj => ((decimal)obj).ToString("C"),
            Type t when t == typeof(double) => obj => ((double)obj).ToString("N2"),
            Type t when t == typeof(float) => obj => ((float)obj).ToString("N2"),
            Type t when t == typeof(bool) => obj => ((bool)obj) ? "✓" : "✗",
            Type t when t.IsEnum => obj => obj.ToString().Replace("_", " "),
            _ => obj => obj?.ToString() ?? ""
        };
    }

    /// <summary>
    /// Get display name from DisplayName attribute or property name
    /// </summary>
    private static string GetDisplayName(PropertyInfo property)
    {
        var displayAttr = property.GetCustomAttribute<DisplayAttribute>();
        if (displayAttr?.Name != null) return displayAttr.Name;

        var displayNameAttr = property.GetCustomAttribute<DisplayNameAttribute>();
        if (displayNameAttr?.DisplayName != null) return displayNameAttr.DisplayName;

        // Convert PascalCase to Title Case
        return string.Concat(property.Name.Select((x, i) => i > 0 && char.IsUpper(x) ? " " + x : x.ToString()));
    }

    /// <summary>
    /// Get display order from DisplayAttribute or default
    /// </summary>
    private static int GetDisplayOrder(PropertyInfo property)
    {
        var displayAttr = property.GetCustomAttribute<DisplayAttribute>();
        return displayAttr?.Order ?? int.MaxValue;
    }

    /// <summary>
    /// Get column width based on property type and attributes
    /// </summary>
    private static string GetColumnWidth(PropertyInfo property)
    {
        var type = Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;
        
        return type switch
        {
            Type t when t == typeof(bool) => "80px",
            Type t when t == typeof(DateTime) || t == typeof(DateTimeOffset) || t == typeof(DateOnly) => "120px",
            Type t when t == typeof(TimeOnly) || t == typeof(TimeSpan) => "80px",
            Type t when t == typeof(decimal) || t == typeof(double) || t == typeof(float) => "100px",
            Type t when t == typeof(int) || t == typeof(long) || t == typeof(short) => "80px",
            Type t when t.IsEnum => "100px",
            Type t when t == typeof(string) && property.Name.Contains("Email", StringComparison.OrdinalIgnoreCase) => "200px",
            Type t when t == typeof(string) && property.Name.Contains("Phone", StringComparison.OrdinalIgnoreCase) => "120px",
            Type t when t == typeof(string) && property.Name.Contains("Id", StringComparison.OrdinalIgnoreCase) => "100px",
            _ => null // Auto-size
        };
    }

    /// <summary>
    /// Clear cache for specific type (useful for development/testing)
    /// </summary>
    public static void ClearCache<TItem>()
    {
        var type = typeof(TItem);
        _columnCache.TryRemove(type, out _);
        _propertyAccessors.TryRemove(type, out _);
        _cacheAccessLog.TryRemove(type, out _);
    }

    /// <summary>
    /// Clear all caches (useful for development/testing)
    /// </summary>
    public static void ClearAllCaches()
    {
        _columnCache.Clear();
        _propertyAccessors.Clear();
        _formatters.Clear();
        _cacheAccessLog.Clear();
    }

    /// <summary>
    /// Get cache statistics for monitoring
    /// </summary>
    public static (int ColumnCacheCount, int AccessorCacheCount, int FormatterCacheCount) GetCacheStats()
    {
        return (_columnCache.Count, _propertyAccessors.Count, _formatters.Count);
    }

    /// <summary>
    /// Determine if property should be included in auto-generation
    /// </summary>
    private static bool ShouldIncludeProperty(PropertyInfo property)
    {
        // Skip properties with certain attributes
        if (property.GetCustomAttribute<System.ComponentModel.DataAnnotations.Schema.NotMappedAttribute>() != null)
            return false;
            
        // Skip JSON ignored properties
        if (property.GetCustomAttribute<System.Text.Json.Serialization.JsonIgnoreAttribute>() != null)
            return false;

        // Skip complex types that aren't displayable
        var type = Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;
        if (type.IsClass && type != typeof(string) && !type.IsEnum)
            return false;

        // Skip collections
        if (typeof(System.Collections.IEnumerable).IsAssignableFrom(type) && type != typeof(string))
            return false;

        return true;
    }

    /// <summary>
    /// Determine if property should be sortable
    /// </summary>
    private static bool IsSortable(PropertyInfo property)
    {
        var type = Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;
        return type.IsPrimitive || type == typeof(string) || type == typeof(DateTime) || 
               type == typeof(DateTimeOffset) || type == typeof(DateOnly) || type == typeof(decimal) || 
               type == typeof(double) || type == typeof(float) || type.IsEnum;
    }

    /// <summary>
    /// Determine if property should be filterable
    /// </summary>
    private static bool IsFilterable(PropertyInfo property)
    {
        var type = Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;
        return type == typeof(string) || type == typeof(bool) || type.IsEnum;
    }

    /// <summary>
    /// Create secure email link with validation and encoding
    /// </summary>
    private static string CreateSecureEmailLink(string email)
    {
        if (string.IsNullOrWhiteSpace(email)) return "";
        
        // Validate email format
        if (!IsValidEmail(email))
        {
            return HtmlEncoder.Default.Encode(email);
        }
        
        var encodedEmail = HtmlEncoder.Default.Encode(email);
        var encodedHref = UrlEncoder.Default.Encode($"mailto:{email}");
        return $"<a href=\"{encodedHref}\">{encodedEmail}</a>";
    }
    
    /// <summary>
    /// Create secure URL link with validation and encoding
    /// </summary>
    private static string CreateSecureUrlLink(string url)
    {
        if (string.IsNullOrWhiteSpace(url)) return "";
        
        // Validate and sanitize URL
        if (!IsValidUrl(url))
        {
            return HtmlEncoder.Default.Encode(url);
        }
        
        var encodedUrl = HtmlEncoder.Default.Encode(url);
        var encodedHref = UrlEncoder.Default.Encode(url);
        return $"<a href=\"{encodedHref}\" target=\"_blank\" rel=\"noopener noreferrer\">{encodedUrl}</a>";
    }
    
    /// <summary>
    /// Validate email format using regex
    /// </summary>
    private static bool IsValidEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email)) return false;
        
        try
        {
            // Simple but effective email validation regex
            var emailRegex = new Regex(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$", RegexOptions.IgnoreCase);
            return emailRegex.IsMatch(email) && email.Length <= 254; // RFC 5321 limit
        }
        catch
        {
            return false;
        }
    }
    
    /// <summary>
    /// Validate URL format and ensure it's safe
    /// </summary>
    private static bool IsValidUrl(string url)
    {
        if (string.IsNullOrWhiteSpace(url)) return false;
        
        try
        {
            var uri = new Uri(url);
            // Only allow http and https schemes for security
            return uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps;
        }
        catch
        {
            return false;
        }
    }
    
    /// <summary>
    /// Format phone number for display
    /// </summary>
    private static string FormatPhoneNumber(string phone)
    {
        if (string.IsNullOrEmpty(phone)) return "";
        
        // HTML encode the phone number to prevent XSS
        var encodedPhone = HtmlEncoder.Default.Encode(phone);
        
        // Simple US phone number formatting
        var digits = new string(phone.Where(char.IsDigit).ToArray());
        if (digits.Length == 10)
        {
            return $"({digits[..3]}) {digits[3..6]}-{digits[6..]}";
        }
        
        return encodedPhone;
    }
}

/// <summary>
/// Cached metadata for a table column
/// </summary>
public class ColumnMetadata
{
    public string PropertyName { get; set; } = "";
    public string DisplayName { get; set; } = "";
    public Type PropertyType { get; set; } = typeof(object);
    public int DisplayOrder { get; set; } = int.MaxValue;
    public string? Width { get; set; }
    public bool Sortable { get; set; }
    public bool Filterable { get; set; }
    public string? DisplayFormat { get; set; }
    public bool IsEmail { get; set; }
    public bool IsUrl { get; set; }
    public bool IsPhone { get; set; }
}