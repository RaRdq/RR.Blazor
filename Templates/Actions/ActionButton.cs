using Microsoft.AspNetCore.Components;
using RR.Blazor.Enums;

namespace RR.Blazor.Templates.Actions;

/// <summary>
/// Represents a single action button configuration
/// </summary>
public class ActionButton<T> where T : class
{
    /// <summary>
    /// Unique identifier for the action
    /// </summary>
    public string Id { get; set; }
    
    /// <summary>
    /// Display text or tooltip
    /// </summary>
    public string Text { get; set; }
    
    /// <summary>
    /// Material Symbols Rounded icon name
    /// </summary>
    public string Icon { get; set; }
    
    /// <summary>
    /// Button variant (Primary, Secondary, Success, Warning, Danger, etc.)
    /// </summary>
    public VariantType Variant { get; set; } = VariantType.Secondary;
    
    /// <summary>
    /// Button style (Filled, Outlined, Ghost, etc.)
    /// </summary>
    public ButtonStyle Style { get; set; } = ButtonStyle.Ghost;
    
    /// <summary>
    /// Whether to show only icon (no text)
    /// </summary>
    public bool IconOnly { get; set; } = true;
    
    /// <summary>
    /// Permission required to show this action
    /// </summary>
    public string RequiredPermission { get; set; }
    
    /// <summary>
    /// Function to determine if action is visible for the item
    /// </summary>
    public Func<T, bool> IsVisible { get; set; }
    
    /// <summary>
    /// Function to determine if action is enabled for the item
    /// </summary>
    public Func<T, bool> IsEnabled { get; set; }
    
    /// <summary>
    /// Click handler for the action
    /// </summary>
    public EventCallback<T> OnClick { get; set; }
    
    /// <summary>
    /// Whether this action requires confirmation
    /// </summary>
    public bool RequiresConfirmation { get; set; }
    
    /// <summary>
    /// Confirmation message if RequiresConfirmation is true
    /// </summary>
    public string ConfirmationMessage { get; set; }
    
    /// <summary>
    /// CSS class for the button
    /// </summary>
    public string CssClass { get; set; }
}

/// <summary>
/// Button style options
/// </summary>
public enum ButtonStyle
{
    Filled,
    Outlined,
    Ghost,
    Text
}