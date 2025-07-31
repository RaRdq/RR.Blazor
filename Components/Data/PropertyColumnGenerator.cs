using System.Collections.Concurrent;
using System.Reflection;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using RR.Blazor.Enums;
using RR.Blazor.Models;
using System.Text.RegularExpressions;

namespace RR.Blazor.Components.Data;

/// <summary>
/// High-performance property column generator with smart UI element detection
/// </summary>
public static class PropertyColumnGenerator
{
    private static readonly ConcurrentDictionary<Type, object> _columnCache = new();
    private static readonly ConcurrentDictionary<Type, Dictionary<string, PropertyMetadata>> _metadataCache = new();

    /// <summary>
    /// Generate smart column definitions from a model type
    /// </summary>
    public static List<RDataTableColumn<TItem>> GenerateColumns<TItem>()
    {
        var itemType = typeof(TItem);
        
        return (List<RDataTableColumn<TItem>>)_columnCache.GetOrAdd(itemType, _ =>
        {
            var properties = itemType.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.CanRead && IsDisplayableProperty(p))
                .Select(p => CreateSmartColumn<TItem>(p))
                .ToList();
            
            return properties;
        });
    }

    /// <summary>
    /// Create smart column that auto-detects UI elements
    /// </summary>
    private static RDataTableColumn<TItem> CreateSmartColumn<TItem>(PropertyInfo property)
    {
        var metadata = GetPropertyMetadata(property);
        
        return new RDataTableColumn<TItem>
        {
            Key = property.Name,
            Header = metadata.DisplayName,
            Sortable = metadata.IsSortable,
            CellTemplate = CreateSmartCellTemplate<TItem>(property, metadata)
        };
    }

    /// <summary>
    /// Create smart cell template that renders appropriate UI elements
    /// </summary>
    private static RenderFragment<TItem> CreateSmartCellTemplate<TItem>(PropertyInfo property, PropertyMetadata metadata)
    {
        return item => builder =>
        {
            var value = property.GetValue(item);
            var stringValue = value?.ToString() ?? "";

            // Handle different data types with smart UI generation
            if (metadata.IsEmail && IsValidEmail(stringValue))
            {
                RenderEmailLink(builder, stringValue);
            }
            else if (metadata.IsUrl && IsValidUrl(stringValue))
            {
                RenderUrlLink(builder, stringValue);
            }
            else if (metadata.IsPhone && IsValidPhone(stringValue))
            {
                RenderPhoneLink(builder, stringValue);
            }
            else if (metadata.IsBoolean)
            {
                RenderBooleanBadge(builder, value);
            }
            else if (metadata.IsDate)
            {
                RenderFormattedDate(builder, value);
            }
            else if (metadata.IsMoney)
            {
                RenderCurrency(builder, value);
            }
            else if (metadata.IsStatus)
            {
                RenderStatusBadge(builder, stringValue);
            }
            else if (metadata.IsAction)
            {
                RenderActionButton(builder, stringValue, item);
            }
            else
            {
                // Default text rendering
                builder.AddContent(0, stringValue);
            }
        };
    }

    /// <summary>
    /// Get enhanced property metadata for smart rendering
    /// </summary>
    private static PropertyMetadata GetPropertyMetadata(PropertyInfo property)
    {
        var type = property.PropertyType;
        var name = property.Name.ToLowerInvariant();
        
        // Check for attributes
        var displayAttr = property.GetCustomAttribute<DisplayAttribute>();
        var displayNameAttr = property.GetCustomAttribute<DisplayNameAttribute>();
        
        var metadata = new PropertyMetadata
        {
            DisplayName = displayAttr?.Name ?? displayNameAttr?.DisplayName ?? property.Name,
            IsSortable = IsSortableType(type),
            IsBoolean = type == typeof(bool) || type == typeof(bool?),
            IsDate = type == typeof(DateTime) || type == typeof(DateOnly) || type == typeof(DateTime?) || type == typeof(DateOnly?),
            IsMoney = name.Contains("price") || name.Contains("cost") || name.Contains("amount") || name.Contains("salary") || name.Contains("wage"),
            IsEmail = name.Contains("email") || name.Contains("mail"),
            IsUrl = name.Contains("url") || name.Contains("link") || name.Contains("website"),
            IsPhone = name.Contains("phone") || name.Contains("mobile") || name.Contains("tel"),
            IsStatus = name.Contains("status") || name.Contains("state"),
            IsAction = name.Contains("action") || name.Contains("button") || property.PropertyType == typeof(string) && name.EndsWith("action")
        };

        return metadata;
    }

    #region Smart UI Renderers

    private static void RenderEmailLink(RenderTreeBuilder builder, string email)
    {
        builder.OpenElement(0, "a");
        builder.AddAttribute(1, "href", $"mailto:{email}");
        builder.AddAttribute(2, "class", "text-primary hover:underline");
        builder.AddContent(3, email);
        builder.CloseElement();
    }

    private static void RenderUrlLink(RenderTreeBuilder builder, string url)
    {
        builder.OpenElement(0, "a");
        builder.AddAttribute(1, "href", url);
        builder.AddAttribute(2, "target", "_blank");
        builder.AddAttribute(3, "rel", "noopener noreferrer");
        builder.AddAttribute(4, "class", "text-primary hover:underline");
        builder.AddContent(5, url);
        builder.CloseElement();
    }

    private static void RenderPhoneLink(RenderTreeBuilder builder, string phone)
    {
        builder.OpenElement(0, "a");
        builder.AddAttribute(1, "href", $"tel:{phone}");
        builder.AddAttribute(2, "class", "text-primary hover:underline");
        builder.AddContent(3, phone);
        builder.CloseElement();
    }

    private static void RenderBooleanBadge(RenderTreeBuilder builder, object value)
    {
        var isTrue = value is bool b && b;
        var badgeClass = isTrue ? "badge badge-success" : "badge badge-secondary";
        var text = isTrue ? "Yes" : "No";
        
        builder.OpenElement(0, "span");
        builder.AddAttribute(1, "class", badgeClass);
        builder.AddContent(2, text);
        builder.CloseElement();
    }

    private static void RenderFormattedDate(RenderTreeBuilder builder, object value)
    {
        if (value is DateTime dateTime)
        {
            builder.AddContent(0, dateTime.ToString("MMM dd, yyyy"));
        }
        else if (value is DateOnly dateOnly)
        {
            builder.AddContent(0, dateOnly.ToString("MMM dd, yyyy"));
        }
        else
        {
            builder.AddContent(0, value?.ToString() ?? "");
        }
    }

    private static void RenderCurrency(RenderTreeBuilder builder, object value)
    {
        if (value is decimal decimalValue)
        {
            builder.AddContent(0, decimalValue.ToString("C"));
        }
        else if (decimal.TryParse(value?.ToString(), out var parsedValue))
        {
            builder.AddContent(0, parsedValue.ToString("C"));
        }
        else
        {
            builder.AddContent(0, value?.ToString() ?? "");
        }
    }

    private static void RenderStatusBadge(RenderTreeBuilder builder, string status)
    {
        var badgeClass = status.ToLowerInvariant() switch
        {
            "active" or "enabled" or "success" => "badge badge-success",
            "inactive" or "disabled" or "error" => "badge badge-danger", 
            "pending" or "warning" => "badge badge-warning",
            _ => "badge badge-secondary"
        };
        
        builder.OpenElement(0, "span");
        builder.AddAttribute(1, "class", badgeClass);
        builder.AddContent(2, status);
        builder.CloseElement();
    }

    private static void RenderActionButton<TItem>(RenderTreeBuilder builder, string action, TItem item)
    {
        builder.OpenElement(0, "button");
        builder.AddAttribute(1, "class", "btn btn-sm btn-primary");
        builder.AddAttribute(2, "type", "button");
        builder.AddContent(3, action);
        builder.CloseElement();
    }

    #endregion

    #region Validation Helpers

    private static bool IsValidEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email)) return false;
        return Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$");
    }

    private static bool IsValidUrl(string url)
    {
        if (string.IsNullOrWhiteSpace(url)) return false;
        return Uri.TryCreate(url, UriKind.Absolute, out _);
    }

    private static bool IsValidPhone(string phone)
    {
        if (string.IsNullOrWhiteSpace(phone)) return false;
        return Regex.IsMatch(phone, @"^[\+]?[1-9][\d]{0,15}$");
    }

    private static bool IsDisplayableProperty(PropertyInfo property)
    {
        var type = property.PropertyType;
        
        // Skip complex objects that aren't displayable
        if (type.IsClass && type != typeof(string) && !type.IsPrimitive)
            return false;
            
        // Skip indexed properties
        if (property.GetIndexParameters().Length > 0)
            return false;
            
        return true;
    }

    private static bool IsSortableType(Type type)
    {
        return type.IsPrimitive || 
               type == typeof(string) || 
               type == typeof(DateTime) || 
               type == typeof(DateOnly) || 
               type == typeof(decimal) ||
               Nullable.GetUnderlyingType(type) != null;
    }

    #endregion

    private class PropertyMetadata
    {
        public string DisplayName { get; set; } = "";
        public bool IsSortable { get; set; }
        public bool IsBoolean { get; set; }
        public bool IsDate { get; set; }
        public bool IsMoney { get; set; }
        public bool IsEmail { get; set; }
        public bool IsUrl { get; set; }
        public bool IsPhone { get; set; }
        public bool IsStatus { get; set; }
        public bool IsAction { get; set; }
    }
}