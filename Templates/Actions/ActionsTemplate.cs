using Microsoft.AspNetCore.Components;
using RR.Blazor.Enums;

namespace RR.Blazor.Templates.Actions;

/// <summary>
/// Template for rendering action buttons in table columns
/// </summary>
/// <typeparam name="T">Type of data the template handles</typeparam>
public class ActionsTemplate<T> where T : class
{
    /// <summary>
    /// Unique identifier for this template
    /// </summary>
    public string Id { get; set; } = Guid.NewGuid().ToString();
    
    /// <summary>
    /// Display name for the template
    /// </summary>
    public string Name { get; set; } = "Actions Template";
    
    /// <summary>
    /// List of action buttons to display
    /// </summary>
    public List<ActionButton<T>> Actions { get; set; } = new();
    
    /// <summary>
    /// Display style for actions
    /// </summary>
    public ActionsDisplayStyle DisplayStyle { get; set; } = ActionsDisplayStyle.Inline;
    
    /// <summary>
    /// Maximum number of visible actions (rest go to dropdown)
    /// </summary>
    public int MaxVisibleActions { get; set; } = 3;
    
    /// <summary>
    /// Size of action buttons
    /// </summary>
    public SizeType Size { get; set; } = SizeType.Small;
    
    /// <summary>
    /// Density of action buttons
    /// </summary>
    public DensityType Density { get; set; } = DensityType.Compact;
    
    /// <summary>
    /// CSS class for the actions container
    /// </summary>
    public string CssClass { get; set; }
    
    /// <summary>
    /// Alignment of actions within the cell
    /// </summary>
    public ActionsAlignment Alignment { get; set; } = ActionsAlignment.End;
    
    /// <summary>
    /// Whether to show tooltips on hover
    /// </summary>
    public bool ShowTooltips { get; set; } = true;
    
    /// <summary>
    /// Dropdown icon for overflow menu
    /// </summary>
    public string DropdownIcon { get; set; } = "more_vert";
    
    /// <summary>
    /// Dropdown text for overflow menu
    /// </summary>
    public string DropdownText { get; set; } = "More";
    
    /// <summary>
    /// Renders the template for the given item
    /// </summary>
    public RenderFragment Render(T item)
    {
        var context = CreateContext(item);
        var renderer = new ActionsRenderer<T>();
        return renderer.Render(context);
    }
    
    private ActionsContext<T> CreateContext(T item)
    {
        var context = new ActionsContext<T>(item)
        {
            Actions = Actions,
            DisplayStyle = DisplayStyle,
            MaxVisibleActions = MaxVisibleActions,
            Size = Size,
            Density = Density,
            CssClass = CssClass,
            Alignment = Alignment,
            ShowTooltips = ShowTooltips,
            DropdownIcon = DropdownIcon,
            DropdownText = DropdownText
        };
        
        return context;
    }
}

/// <summary>
/// Display style for actions
/// </summary>
public enum ActionsDisplayStyle
{
    /// <summary>
    /// Display actions inline horizontally
    /// </summary>
    Inline,
    
    /// <summary>
    /// Display all actions in a dropdown menu
    /// </summary>
    Dropdown,
    
    /// <summary>
    /// Display primary actions inline with overflow in dropdown
    /// </summary>
    Mixed
}

/// <summary>
/// Alignment options for actions
/// </summary>
public enum ActionsAlignment
{
    Start,
    Center,
    End
}