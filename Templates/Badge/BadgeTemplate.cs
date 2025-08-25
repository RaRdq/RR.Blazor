using Microsoft.AspNetCore.Components;
using System.Linq.Expressions;
using RR.Blazor.Enums;

namespace RR.Blazor.Templates.Badge;

/// <summary>
/// Badge template for status indicators, counts, and labels
/// </summary>
/// <typeparam name="T">Type of data the template handles</typeparam>
public class BadgeTemplate<T> where T : class
{
    /// <summary>
    /// Unique identifier for this template
    /// </summary>
    public string Id { get; set; } = Guid.NewGuid().ToString();
    
    /// <summary>
    /// Display name for the template
    /// </summary>
    public string Name { get; set; } = "Badge Template";
    
    /// <summary>
    /// Property selector for extracting data from the item
    /// </summary>
    public Expression<Func<T, object>> PropertySelector { get; set; }
    
    /// <summary>
    /// CSS classes to apply to the rendered template
    /// </summary>
    public string CssClass { get; set; }
    
    /// <summary>
    /// Size variant
    /// </summary>
    public SizeType Size { get; set; } = SizeType.Medium;
    
    /// <summary>
    /// Density variant
    /// </summary>
    public DensityType Density { get; set; } = DensityType.Normal;

    /// <summary>
    /// Variant for badge styling
    /// </summary>
    public VariantType Variant { get; set; } = VariantType.Primary;
    
    /// <summary>
    /// Custom text selector (overrides PropertySelector for text)
    /// </summary>
    public Expression<Func<T, string>> TextSelector { get; set; }
    
    /// <summary>
    /// Icon selector for badge icon
    /// </summary>
    public Expression<Func<T, string>> IconSelector { get; set; }
    
    /// <summary>
    /// Variant selector for dynamic styling
    /// </summary>
    public Expression<Func<T, VariantType>> VariantSelector { get; set; }
    
    /// <summary>
    /// Whether badges are clickable
    /// </summary>
    public bool Clickable { get; set; }
    
    /// <summary>
    /// Click event handler
    /// </summary>
    public EventCallback<T> OnClick { get; set; }
    
    /// <summary>
    /// Status-to-variant mapping for automatic styling
    /// </summary>
    public Dictionary<string, VariantType> StatusMapping { get; set; } = new()
    {
        { "active", VariantType.Success },
        { "inactive", VariantType.Secondary },
        { "pending", VariantType.Warning },
        { "error", VariantType.Danger },
        { "success", VariantType.Success },
        { "warning", VariantType.Warning },
        { "info", VariantType.Info }
    };

    /// <summary>
    /// Gets the value from the item using the property selector
    /// </summary>
    public virtual object GetValue(T item)
    {
        if (item == null) return null;
        
        var compiledSelector = PropertySelector?.Compile();
        return compiledSelector?.Invoke(item);
    }
    
    /// <summary>
    /// Formats the value using default formatting
    /// </summary>
    public virtual string FormatValue(object value)
    {
        return value?.ToString() ?? string.Empty;
    }

    /// <summary>
    /// Renders the template for the given item
    /// </summary>
    public RenderFragment Render(T item)
    {
        var context = CreateContext(item);
        var renderer = new BadgeRenderer<T>();
        return renderer.Render(context);
    }

    private BadgeContext<T> CreateContext(T item)
    {
        var context = new BadgeContext<T>(item)
        {
            Size = Size,
            Density = Density,
            CssClass = CssClass,
            Clickable = Clickable,
            OnClick = OnClick
        };
        
        // Get text
        if (TextSelector != null)
        {
            var textGetter = TextSelector.Compile();
            context.Text = textGetter(item);
        }
        else
        {
            var value = GetValue(item);
            context.Text = FormatValue(value);
        }
        
        // Get variant
        if (VariantSelector != null)
        {
            var variantGetter = VariantSelector.Compile();
            context.Variant = variantGetter(item);
        }
        else if (StatusMapping.Any())
        {
            var statusKey = context.Text?.ToLowerInvariant();
            if (!string.IsNullOrEmpty(statusKey) && StatusMapping.ContainsKey(statusKey))
            {
                context.Variant = StatusMapping[statusKey];
            }
            else
            {
                context.Variant = Variant;
            }
        }
        else
        {
            context.Variant = Variant;
        }
        
        // Get icon
        if (IconSelector != null)
        {
            var iconGetter = IconSelector.Compile();
            context.Icon = iconGetter(item);
        }
        
        return context;
    }
}