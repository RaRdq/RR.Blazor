using Microsoft.AspNetCore.Components;
using System.Linq.Expressions;
using RR.Blazor.Enums;

namespace RR.Blazor.Templates.Avatar;

/// <summary>
/// Avatar template for user profiles, initials, and status indicators
/// </summary>
/// <typeparam name="T">Type of data the template handles</typeparam>
public class AvatarTemplate<T> where T : class
{
    /// <summary>
    /// Unique identifier for this template
    /// </summary>
    public string Id { get; set; } = Guid.NewGuid().ToString();
    
    /// <summary>
    /// Display name for the template
    /// </summary>
    public string Name { get; set; } = "Avatar Template";
    
    /// <summary>
    /// Property selector for extracting data from the item
    /// </summary>
    public Expression<Func<T, object>> PropertySelector { get; set; }
    
    /// <summary>
    /// CSS classes to apply to the rendered template
    /// </summary>
    public string Class { get; set; }
    
    /// <summary>
    /// Size variant
    /// </summary>
    public SizeType Size { get; set; } = SizeType.Medium;
    
    /// <summary>
    /// Density variant
    /// </summary>
    public DensityType Density { get; set; } = DensityType.Normal;

    /// <summary>
    /// Shape of the avatar
    /// </summary>
    public AvatarShape Shape { get; set; } = AvatarShape.Circle;
    
    /// <summary>
    /// Default color variant when no image
    /// </summary>
    public VariantType ColorVariant { get; set; } = VariantType.Primary;
    
    /// <summary>
    /// Name selector for generating initials
    /// </summary>
    public Expression<Func<T, string>> NameSelector { get; set; }
    
    /// <summary>
    /// Image URL selector
    /// </summary>
    public Expression<Func<T, string>> ImageSelector { get; set; }
    
    /// <summary>
    /// Status selector for online/offline indicators
    /// </summary>
    public Expression<Func<T, AvatarStatus>> StatusSelector { get; set; }
    
    /// <summary>
    /// Badge selector for notification counts
    /// </summary>
    public Expression<Func<T, string>> BadgeSelector { get; set; }
    
    /// <summary>
    /// Color variant selector for dynamic coloring
    /// </summary>
    public Expression<Func<T, VariantType>> ColorSelector { get; set; }
    
    /// <summary>
    /// Whether avatars are clickable
    /// </summary>
    public bool Clickable { get; set; }
    
    /// <summary>
    /// Click event handler
    /// </summary>
    public EventCallback<T> OnClick { get; set; }
    
    /// <summary>
    /// Whether to show border
    /// </summary>
    public bool ShowBorder { get; set; }
    
    /// <summary>
    /// Color assignment for consistent avatar colors based on names
    /// </summary>
    public Dictionary<string, VariantType> ColorMapping { get; set; } = new();

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
    /// Generates initials from a name
    /// </summary>
    public static string GenerateInitials(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return "?";
        
        var parts = name.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
        
        return parts.Length switch
        {
            0 => "?",
            1 => parts[0].Length >= 2 
                ? parts[0].Substring(0, 2).ToUpperInvariant()
                : parts[0].Substring(0, 1).ToUpperInvariant(),
            _ => $"{parts[0][0]}{parts[^1][0]}".ToUpperInvariant()
        };
    }
    
    /// <summary>
    /// Gets consistent color variant based on name hash
    /// </summary>
    public static VariantType GetConsistentColor(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return VariantType.Primary;
        
        var variants = new[] 
        { 
            VariantType.Primary, 
            VariantType.Secondary, 
            VariantType.Success, 
            VariantType.Info, 
            VariantType.Warning, 
            VariantType.Error 
        };
        
        var hash = name.GetHashCode();
        var index = Math.Abs(hash) % variants.Length;
        return variants[index];
    }

    /// <summary>
    /// Renders the template for the given item
    /// </summary>
    public RenderFragment Render(T item)
    {
        var context = CreateContext(item);
        var renderer = new AvatarRenderer<T>();
        return renderer.Render(context);
    }

    private AvatarContext<T> CreateContext(T item)
    {
        var context = new AvatarContext<T>(item)
        {
            Size = Size,
            Density = Density,
            Class = Class,
            Shape = Shape,
            Clickable = Clickable,
            OnClick = OnClick,
            ShowBorder = ShowBorder
        };
        
        // Get name
        if (NameSelector != null)
        {
            var nameGetter = NameSelector.Compile();
            context.Name = nameGetter(item);
        }
        else
        {
            var value = GetValue(item);
            context.Name = value?.ToString() ?? string.Empty;
        }
        
        // Generate initials
        context.Initials = GenerateInitials(context.Name);
        
        // Get image URL
        if (ImageSelector != null)
        {
            var imageGetter = ImageSelector.Compile();
            context.ImageUrl = imageGetter(item);
        }
        
        // Get status
        if (StatusSelector != null)
        {
            var statusGetter = StatusSelector.Compile();
            context.Status = statusGetter(item);
        }
        
        // Get badge
        if (BadgeSelector != null)
        {
            var badgeGetter = BadgeSelector.Compile();
            context.Badge = badgeGetter(item);
        }
        
        // Get color variant
        if (ColorSelector != null)
        {
            var colorGetter = ColorSelector.Compile();
            context.ColorVariant = colorGetter(item);
        }
        else if (ColorMapping.Any() && !string.IsNullOrEmpty(context.Name))
        {
            var key = context.Name.ToLowerInvariant();
            context.ColorVariant = ColorMapping.ContainsKey(key) 
                ? ColorMapping[key] 
                : GetConsistentColor(context.Name);
        }
        else
        {
            context.ColorVariant = ColorVariant;
        }
        
        return context;
    }
}
