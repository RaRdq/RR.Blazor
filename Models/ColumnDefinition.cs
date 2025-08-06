using Microsoft.AspNetCore.Components;
using RR.Blazor.Enums;
using RR.Blazor.Services;
using System.Linq.Expressions;

namespace RR.Blazor.Models;

/// <summary>
/// Column definition for the new RTable system
/// </summary>
public class ColumnDefinition<TItem> where TItem : class
{
    // Core properties
    public string Key { get; set; }
    public string Title { get; set; }
    public bool Visible { get; set; } = true;
    public int Order { get; set; }
    
    // Data access
    public Expression<Func<TItem, object>> Property { get; set; }
    public Func<TItem, object> CompiledProperty { get; set; }
    
    // Templates
    public ColumnTemplate Template { get; set; } = ColumnTemplate.Auto;
    public RenderFragment<TItem> CustomTemplate { get; set; }
    public RenderFragment HeaderTemplate { get; set; }
    
    // Formatting
    public string Format { get; set; }
    public string EmptyText { get; set; } = "-";
    public Func<TItem, string> Formatter { get; set; }
    
    // Sorting & Filtering
    public bool Sortable { get; set; } = true;
    public bool Filterable { get; set; } = true;
    public bool Searchable { get; set; } = true;
    public FilterType FilterType { get; set; } = FilterType.Auto;
    
    // Layout
    public string Width { get; set; }
    public string MinWidth { get; set; }
    public string MaxWidth { get; set; }
    public ColumnAlign Align { get; set; } = ColumnAlign.Auto;
    
    // Styling
    public string HeaderClass { get; set; }
    public string CellClass { get; set; }
    public Func<TItem, string> CellClassFunc { get; set; }
    
    // Export
    public bool Exportable { get; set; } = true;
    public string ExportHeader { get; set; }
    public Func<TItem, object> ExportValue { get; set; }
    
    /// <summary>
    /// Gets the cell content for rendering
    /// </summary>
    public RenderFragment GetCellContent(TItem item)
    {
        if (CustomTemplate != null)
        {
            return builder => CustomTemplate(item)(builder);
        }
        
        // Use built-in template rendering based on Template property
        return builder =>
        {
            var renderer = new ColumnTemplateRenderer<TItem>(this);
            renderer.RenderCell(builder, item);
        };
    }
    
    /// <summary>
    /// Gets the value of the column for the given item
    /// </summary>
    public object GetValue(TItem item)
    {
        if (item == null)
            return null;
            
        if (CompiledProperty != null)
            return CompiledProperty(item);
            
        if (Property != null)
        {
            CompiledProperty = Property.Compile();
            return CompiledProperty(item);
        }
        
        return null;
    }
    
    /// <summary>
    /// Gets the formatted value for display
    /// </summary>
    public string GetFormattedValue(TItem item)
    {
        if (Formatter != null)
            return Formatter(item);
            
        var value = GetValue(item);
        
        if (value == null)
            return EmptyText;
            
        if (!string.IsNullOrEmpty(Format) && value is IFormattable formattable)
            return formattable.ToString(Format, null);
            
        return value.ToString();
    }
}

/// <summary>
/// Column alignment options
/// </summary>
public enum ColumnAlign
{
    Auto,
    Left,
    Center,
    Right
}

/// <summary>
/// Built-in column templates
/// </summary>
public enum ColumnTemplate
{
    Auto,          // Auto-detect based on type
    Text,          // Plain text
    Number,        // Formatted number
    Currency,      // Currency with symbol
    Percentage,    // Percentage formatting
    Date,          // Date only
    DateTime,      // Date and time
    Time,          // Time only
    Boolean,       // Yes/No or checkmark
    Status,        // Status badge with color
    Progress,      // Progress bar
    Rating,        // Star rating
    Email,         // Email link
    Phone,         // Phone link
    Link,          // External link
    Image,         // Image thumbnail
    Avatar,        // Avatar with initials
    Tags,          // Tag list
    Actions,       // Action buttons
    Custom         // Fully custom template
}