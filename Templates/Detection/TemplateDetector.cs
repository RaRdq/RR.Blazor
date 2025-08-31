using System.Reflection;
using System.Text.RegularExpressions;
using RR.Blazor.Templates.Badge;
using RR.Blazor.Templates.Currency;
using RR.Blazor.Templates.Stack;
using RR.Blazor.Enums;

namespace RR.Blazor.Templates.Detection;

/// <summary>
/// Smart template detection engine that analyzes data to suggest appropriate templates
/// Implements automatic template suggestions based on data analysis
/// </summary>
public static class TemplateDetector
{
    private static readonly Dictionary<string, TemplateType> PropertyNamePatterns = new()
    {
        // Badge patterns
        { "status", TemplateType.Badge },
        { "state", TemplateType.Badge },
        { "priority", TemplateType.Badge },
        { "type", TemplateType.Badge },
        { "category", TemplateType.Badge },
        { "level", TemplateType.Badge },
        
        // Currency patterns
        { "amount", TemplateType.Currency },
        { "price", TemplateType.Currency },
        { "cost", TemplateType.Currency },
        { "salary", TemplateType.Currency },
        { "wage", TemplateType.Currency },
        { "total", TemplateType.Currency },
        { "balance", TemplateType.Currency },
        { "value", TemplateType.Currency },
        
        // Stack patterns
        { "name", TemplateType.Stack },
        { "title", TemplateType.Stack },
        { "description", TemplateType.Stack },
        { "address", TemplateType.Stack },
        { "profile", TemplateType.Stack },
        
        // Avatar patterns
        { "avatar", TemplateType.Avatar },
        { "user", TemplateType.Avatar },
        { "employee", TemplateType.Avatar },
        { "owner", TemplateType.Avatar },
        { "manager", TemplateType.Avatar },
        { "assignee", TemplateType.Avatar },
        
        // Progress patterns
        { "progress", TemplateType.Progress },
        { "completion", TemplateType.Progress },
        { "percentage", TemplateType.Progress },
        { "utilization", TemplateType.Progress },
        { "capacity", TemplateType.Progress },
        
        // Rating patterns
        { "rating", TemplateType.Rating },
        { "score", TemplateType.Rating },
        { "stars", TemplateType.Rating },
        { "review", TemplateType.Rating },
        { "feedback", TemplateType.Rating },
        { "satisfaction", TemplateType.Rating }
    };

    private static readonly Dictionary<Type, TemplateType> TypePatterns = new()
    {
        { typeof(decimal), TemplateType.Currency },
        { typeof(decimal?), TemplateType.Currency },
        { typeof(double), TemplateType.Currency },
        { typeof(double?), TemplateType.Currency },
        { typeof(float), TemplateType.Currency },
        { typeof(float?), TemplateType.Currency }
    };

    /// <summary>
    /// Analyzes a property and suggests the best template type
    /// </summary>
    public static TemplateType DetectTemplate<T>(PropertyInfo property, IEnumerable<T> sampleData = null)
    {
        if (property == null) return TemplateType.None;

        // Check property name patterns first
        var templateFromName = DetectFromPropertyName(property.Name);
        if (templateFromName != TemplateType.None)
            return templateFromName;

        // Check property type patterns
        var templateFromType = DetectFromPropertyType(property.PropertyType);
        if (templateFromType != TemplateType.None)
            return templateFromType;

        // Analyze sample data if provided
        if (sampleData?.Any() == true)
        {
            var templateFromData = AnalyzeSampleData(property, sampleData);
            if (templateFromData != TemplateType.None)
                return templateFromData;
        }

        return TemplateType.None;
    }

    /// <summary>
    /// Detects template type from property name patterns
    /// </summary>
    private static TemplateType DetectFromPropertyName(string propertyName)
    {
        if (string.IsNullOrWhiteSpace(propertyName))
            return TemplateType.None;

        var name = propertyName.ToLowerInvariant();
        
        // Direct matches
        if (PropertyNamePatterns.TryGetValue(name, out var directMatch))
            return directMatch;

        // Partial matches
        foreach (var pattern in PropertyNamePatterns)
        {
            if (name.Contains(pattern.Key))
                return pattern.Value;
        }

        // Regex patterns for complex scenarios
        if (Regex.IsMatch(name, @"(price|cost|amount|salary|wage)"))
            return TemplateType.Currency;

        if (Regex.IsMatch(name, @"(status|state|priority|level|type)"))
            return TemplateType.Badge;
            
        if (Regex.IsMatch(name, @"(avatar|user|employee|owner|manager)"))
            return TemplateType.Avatar;
            
        if (Regex.IsMatch(name, @"(progress|completion|percentage|utilization)"))
            return TemplateType.Progress;
            
        if (Regex.IsMatch(name, @"(rating|score|stars|review|feedback)"))
            return TemplateType.Rating;

        return TemplateType.None;
    }

    /// <summary>
    /// Detects template type from property type
    /// </summary>
    private static TemplateType DetectFromPropertyType(Type propertyType)
    {
        if (TypePatterns.TryGetValue(propertyType, out var templateType))
            return templateType;

        // Check for nullable types
        var underlyingType = Nullable.GetUnderlyingType(propertyType);
        if (underlyingType != null && TypePatterns.TryGetValue(underlyingType, out var nullableTemplateType))
            return nullableTemplateType;

        // Enum types often work well as badges
        if (propertyType.IsEnum)
            return TemplateType.Badge;

        return TemplateType.None;
    }

    /// <summary>
    /// Analyzes sample data to detect patterns
    /// </summary>
    private static TemplateType AnalyzeSampleData<T>(PropertyInfo property, IEnumerable<T> sampleData)
    {
        var samples = sampleData.Take(10).Select(item => property.GetValue(item)).Where(v => v != null).ToList();
        if (!samples.Any()) return TemplateType.None;

        // Check if values look like currency
        if (samples.All(v => IsNumericType(v.GetType()) && IsLikelyCurrency(v)))
            return TemplateType.Currency;

        // Check if values look like status/category badges
        if (samples.All(v => v is string s && IsLikelyStatus(s)))
            return TemplateType.Badge;

        // Check if values suggest stacked display
        if (samples.All(v => v is string s && s.Length > 30))
            return TemplateType.Stack;

        return TemplateType.None;
    }

    /// <summary>
    /// Creates template suggestion with configuration
    /// </summary>
    public static TemplateSuggestion<T> CreateSuggestion<T>(PropertyInfo property, IEnumerable<T> sampleData = null) where T : class
    {
        var templateType = DetectTemplate(property, sampleData);
        var confidence = CalculateConfidence(property, templateType, sampleData);

        return new TemplateSuggestion<T>
        {
            PropertyName = property.Name,
            TemplateType = templateType,
            Confidence = confidence,
            Reason = GenerateReason(property, templateType),
            ConfigurationHint = GenerateConfigurationHint(property, templateType, sampleData)
        };
    }

    private static bool IsNumericType(Type type)
    {
        return type == typeof(decimal) || type == typeof(decimal?) ||
               type == typeof(double) || type == typeof(double?) ||
               type == typeof(float) || type == typeof(float?) ||
               type == typeof(int) || type == typeof(int?) ||
               type == typeof(long) || type == typeof(long?);
    }

    private static bool IsLikelyCurrency(object value)
    {
        if (value == null) return false;

        // Check if the numeric value is in a typical currency range
        if (decimal.TryParse(value.ToString(), out var decimalValue))
        {
            var absValue = Math.Abs(decimalValue);
            return absValue >= 0.01m && absValue <= 10_000_000m;
        }

        return false;
    }

    private static bool IsLikelyStatus(string value)
    {
        if (string.IsNullOrWhiteSpace(value)) return false;

        var lowerValue = value.ToLowerInvariant();
        var statusWords = new[] { "active", "inactive", "pending", "completed", "cancelled", "approved", "rejected", "success", "error", "warning", "info" };
        
        return statusWords.Any(word => lowerValue.Contains(word)) || value.Length < 20;
    }

    private static double CalculateConfidence<T>(PropertyInfo property, TemplateType templateType, IEnumerable<T> sampleData)
    {
        if (templateType == TemplateType.None) return 0.0;

        var confidence = 0.5; // Base confidence

        // Increase confidence for direct name matches
        var name = property.Name.ToLowerInvariant();
        if (PropertyNamePatterns.ContainsKey(name))
            confidence += 0.3;

        // Increase confidence for type matches
        if (TypePatterns.ContainsKey(property.PropertyType))
            confidence += 0.2;

        // Decrease confidence if no sample data available
        if (sampleData?.Any() != true)
            confidence -= 0.1;

        return Math.Min(1.0, Math.Max(0.0, confidence));
    }

    private static string GenerateReason(PropertyInfo property, TemplateType templateType)
    {
        var name = property.Name;
        var type = property.PropertyType.Name;

        return templateType switch
        {
            TemplateType.Badge => $"Property '{name}' appears to be a status/category field based on name pattern",
            TemplateType.Currency => $"Property '{name}' of type '{type}' appears to be a monetary value",
            TemplateType.Stack => $"Property '{name}' appears to contain multi-line or detailed text content",
            TemplateType.Avatar => $"Property '{name}' appears to be a user/profile field suitable for avatar display",
            TemplateType.Progress => $"Property '{name}' appears to be a progress/percentage field",
            TemplateType.Rating => $"Property '{name}' appears to be a rating/score field",
            _ => $"No specific template pattern detected for '{name}'"
        };
    }

    private static string GenerateConfigurationHint(PropertyInfo property, TemplateType templateType, IEnumerable<object> sampleData)
    {
        return templateType switch
        {
            TemplateType.Badge => "Consider configuring status-to-variant mapping for automatic color coding",
            TemplateType.Currency => "Configure currency code and compact formatting based on your data scale",
            TemplateType.Stack => "Consider using primary/secondary text selectors for better information hierarchy",
            TemplateType.Avatar => "Configure image and name selectors, consider adding status indicators",
            TemplateType.Progress => "Set appropriate max value and consider using different progress types",
            TemplateType.Rating => "Choose rating type (stars, thumbs, emoji) and configure max rating",
            _ => null
        };
    }
}