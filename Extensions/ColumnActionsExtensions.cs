using Microsoft.AspNetCore.Components;
using RR.Blazor.Models;
using RR.Blazor.Templates.Actions;
using RR.Blazor.Enums;

namespace RR.Blazor.Extensions;

/// <summary>
/// Extension methods for configuring column actions
/// </summary>
public static class ColumnActionsExtensions
{
    /// <summary>
    /// Configures actions for a column
    /// </summary>
    public static ColumnDefinition<TItem> WithActions<TItem>(this ColumnDefinition<TItem> column,
        Action<ActionsBuilder<TItem>> configure) where TItem : class
    {
        var builder = new ActionsBuilder<TItem>();
        configure(builder);
        column.ActionsTemplate = builder.Build();
        return column;
    }
    
    /// <summary>
    /// Sets the column template to Actions
    /// </summary>
    public static ColumnDefinition<TItem> AsActionsColumn<TItem>(this ColumnDefinition<TItem> column) where TItem : class
    {
        column.Template = ColumnTemplate.Actions;
        return column;
    }
}

/// <summary>
/// Builder for creating actions templates
/// </summary>
public class ActionsBuilder<T> where T : class
{
    private readonly ActionsTemplate<T> _template = new();
    
    public ActionsBuilder()
    {
        _template.Size = SizeType.Small;
        _template.Density = DensityType.Compact;
        _template.DisplayStyle = ActionsDisplayStyle.Inline;
        _template.ShowTooltips = true;
        _template.MaxVisibleActions = 3;
        _template.Alignment = ActionsAlignment.End;
    }
    
    /// <summary>
    /// Sets the display style
    /// </summary>
    public ActionsBuilder<T> DisplayStyle(ActionsDisplayStyle style)
    {
        _template.DisplayStyle = style;
        return this;
    }
    
    /// <summary>
    /// Sets the alignment
    /// </summary>
    public ActionsBuilder<T> Alignment(ActionsAlignment alignment)
    {
        _template.Alignment = alignment;
        return this;
    }
    
    /// <summary>
    /// Sets the size
    /// </summary>
    public ActionsBuilder<T> Size(SizeType size)
    {
        _template.Size = size;
        return this;
    }
    
    /// <summary>
    /// Sets the density
    /// </summary>
    public ActionsBuilder<T> Density(DensityType density)
    {
        _template.Density = density;
        return this;
    }
    
    /// <summary>
    /// Sets maximum visible actions
    /// </summary>
    public ActionsBuilder<T> MaxVisible(int count)
    {
        _template.MaxVisibleActions = count;
        return this;
    }
    
    /// <summary>
    /// Adds a view action
    /// </summary>
    public ActionsBuilder<T> AddView(string text = "View", string icon = "visibility", EventCallback<T> onClick = default)
    {
        var action = new ActionButton<T>
        {
            Id = "view",
            Text = text,
            Icon = icon,
            Variant = VariantType.Secondary,
            Style = ButtonStyle.Ghost,
            IconOnly = true,
            OnClick = onClick
        };
        
        _template.Actions.Add(action);
        return this;
    }
    
    /// <summary>
    /// Adds an edit action
    /// </summary>
    public ActionsBuilder<T> AddEdit(string text = "Edit", string icon = "edit", EventCallback<T> onClick = default)
    {
        var action = new ActionButton<T>
        {
            Id = "edit",
            Text = text,
            Icon = icon,
            Variant = VariantType.Primary,
            Style = ButtonStyle.Ghost,
            IconOnly = true,
            OnClick = onClick
        };
        
        _template.Actions.Add(action);
        return this;
    }
    
    /// <summary>
    /// Adds a delete action
    /// </summary>
    public ActionsBuilder<T> AddDelete(string text = "Delete", string icon = "delete", 
        string confirmationMessage = "Are you sure you want to delete this item?", EventCallback<T> onClick = default)
    {
        var action = new ActionButton<T>
        {
            Id = "delete",
            Text = text,
            Icon = icon,
            Variant = VariantType.Danger,
            Style = ButtonStyle.Ghost,
            IconOnly = true,
            RequiresConfirmation = true,
            ConfirmationMessage = confirmationMessage,
            OnClick = onClick
        };
        
        _template.Actions.Add(action);
        return this;
    }
    
    /// <summary>
    /// Adds a custom action
    /// </summary>
    public ActionsBuilder<T> AddCustom(Action<ActionButton<T>> configure)
    {
        var action = new ActionButton<T>
        {
            Id = Guid.NewGuid().ToString(),
            Variant = VariantType.Secondary,
            Style = ButtonStyle.Ghost,
            IconOnly = true
        };
        
        configure(action);
        _template.Actions.Add(action);
        return this;
    }
    
    /// <summary>
    /// Adds a conditional action that's only visible when condition is met
    /// </summary>
    public ActionsBuilder<T> AddConditional(string id, string text, string icon, 
        Func<T, bool> condition, EventCallback<T> onClick, 
        VariantType variant = VariantType.Secondary, bool requiresConfirmation = false)
    {
        var action = new ActionButton<T>
        {
            Id = id,
            Text = text,
            Icon = icon,
            Variant = variant,
            Style = ButtonStyle.Ghost,
            IconOnly = true,
            IsVisible = condition,
            OnClick = onClick,
            RequiresConfirmation = requiresConfirmation
        };
        
        if (requiresConfirmation)
        {
            action.ConfirmationMessage = $"Are you sure you want to {text.ToLowerInvariant()} this item?";
        }
        
        _template.Actions.Add(action);
        return this;
    }
    
    /// <summary>
    /// Builds the actions template
    /// </summary>
    internal ActionsTemplate<T> Build() => _template;
}