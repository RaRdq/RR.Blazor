using RR.Blazor.Enums;
using RR.Blazor.Templates;

namespace RR.Blazor.Templates.Actions;

/// <summary>
/// Context for rendering actions template
/// </summary>
public class ActionsContext<T> : TemplateContext<T> where T : class
{
    public ActionsContext(T item) : base(item) { }
    
    /// <summary>
    /// List of action buttons to display
    /// </summary>
    public List<ActionButton<T>> Actions { get; set; } = new();
    
    /// <summary>
    /// Display style for actions
    /// </summary>
    public ActionsDisplayStyle DisplayStyle { get; set; }
    
    /// <summary>
    /// Maximum number of visible actions
    /// </summary>
    public int MaxVisibleActions { get; set; }
    
    /// <summary>
    /// Size of action buttons
    /// </summary>
    public new SizeType Size { get; set; }
    
    /// <summary>
    /// Density of action buttons
    /// </summary>
    public new DensityType Density { get; set; }
    
    /// <summary>
    /// Alignment of actions
    /// </summary>
    public ActionsAlignment Alignment { get; set; }
    
    /// <summary>
    /// Whether to show tooltips
    /// </summary>
    public bool ShowTooltips { get; set; }
    
    /// <summary>
    /// Dropdown icon
    /// </summary>
    public string DropdownIcon { get; set; }
    
    /// <summary>
    /// Dropdown text
    /// </summary>
    public string DropdownText { get; set; }
}