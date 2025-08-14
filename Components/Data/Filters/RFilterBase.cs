using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using RR.Blazor.Enums;
using RR.Blazor.Models;

namespace RR.Blazor.Components.Data.Filters;

/// <summary>
/// Base class for all filter components
/// </summary>
/// <typeparam name="T">The data type being filtered</typeparam>
public abstract class RFilterBase<T> : ComponentBase
{
    #region Parameters
    
    /// <summary>
    /// The column key this filter applies to
    /// </summary>
    [Parameter] public string ColumnKey { get; set; } = "";
    
    /// <summary>
    /// The current filter operator
    /// </summary>
    [Parameter] public FilterOperator Operator { get; set; } = FilterOperator.Equals;
    
    /// <summary>
    /// The filter value
    /// </summary>
    [Parameter] public T? Value { get; set; }
    
    /// <summary>
    /// Second value for range operators (Between, NotBetween)
    /// </summary>
    [Parameter] public T? SecondValue { get; set; }
    
    /// <summary>
    /// Whether the filter is currently active
    /// </summary>
    [Parameter] public bool IsActive { get; set; } = true;
    
    /// <summary>
    /// Placeholder text for input fields
    /// </summary>
    [Parameter] public string? Placeholder { get; set; }
    
    /// <summary>
    /// Whether to show the operator selection dropdown
    /// </summary>
    [Parameter] public bool ShowOperatorSelection { get; set; } = true;
    
    /// <summary>
    /// Whether to show the clear button
    /// </summary>
    [Parameter] public bool ShowClearButton { get; set; } = true;
    
    /// <summary>
    /// Custom CSS classes to apply
    /// </summary>
    [Parameter] public string? Class { get; set; }
    
    /// <summary>
    /// Whether the filter is disabled
    /// </summary>
    [Parameter] public bool Disabled { get; set; }
    
    /// <summary>
    /// Size variant for the filter UI
    /// </summary>
    [Parameter] public DensityType Density { get; set; } = DensityType.Normal;
    
    #endregion
    
    #region Events
    
    /// <summary>
    /// Fired when the filter value changes
    /// </summary>
    [Parameter] public EventCallback<FilterEventArgs> OnFilterChanged { get; set; }
    
    /// <summary>
    /// Fired when the filter is cleared
    /// </summary>
    [Parameter] public EventCallback<string> OnFilterCleared { get; set; }
    
    /// <summary>
    /// Fired when the operator changes
    /// </summary>
    [Parameter] public EventCallback<FilterOperator> OnOperatorChanged { get; set; }
    
    #endregion
    
    #region Protected Properties
    
    /// <summary>
    /// Gets the available operators for this filter type
    /// </summary>
    protected abstract List<FilterOperator> AvailableOperators { get; }
    
    /// <summary>
    /// Gets the filter type for this component
    /// </summary>
    protected abstract FilterType FilterType { get; }
    
    /// <summary>
    /// Gets the current filter state
    /// </summary>
    protected FilterState CurrentFilterState => new(
        ColumnKey, 
        FilterType, 
        Operator, 
        Value, 
        SecondValue, 
        IsActive
    );
    
    #endregion
    
    #region Protected Methods
    
    /// <summary>
    /// Renders the filter component
    /// </summary>
    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenElement(0, "div");
        builder.AddAttribute(1, "class", GetContainerClasses());
        
        if (ShowOperatorSelection && AvailableOperators.Count > 1)
        {
            RenderOperatorSelector(builder);
        }
        
        RenderFilterInput(builder);
        
        if (ShowClearButton)
        {
            RenderClearButton(builder);
        }
        
        builder.CloseElement();
    }
    
    /// <summary>
    /// Renders the operator selection dropdown
    /// </summary>
    protected virtual void RenderOperatorSelector(RenderTreeBuilder builder)
    {
        var sequence = 10;
        
        builder.OpenElement(sequence++, "select");
        builder.AddAttribute(sequence++, "class", GetOperatorSelectorClasses());
        builder.AddAttribute(sequence++, "value", Operator.ToString());
        builder.AddAttribute(sequence++, "disabled", Disabled);
        builder.AddAttribute(sequence++, "onchange", EventCallback.Factory.Create<ChangeEventArgs>(
            this, async args => await HandleOperatorChange(args)));
        
        foreach (var op in AvailableOperators)
        {
            builder.OpenElement(sequence++, "option");
            builder.AddAttribute(sequence++, "value", op.ToString());
            builder.AddContent(sequence++, GetOperatorDisplayName(op));
            builder.CloseElement();
        }
        
        builder.CloseElement();
    }
    
    /// <summary>
    /// Renders the main filter input - must be implemented by derived classes
    /// </summary>
    protected abstract void RenderFilterInput(RenderTreeBuilder builder);
    
    /// <summary>
    /// Renders the clear button
    /// </summary>
    protected virtual void RenderClearButton(RenderTreeBuilder builder)
    {
        var sequence = 100;
        
        builder.OpenElement(sequence++, "button");
        builder.AddAttribute(sequence++, "type", "button");
        builder.AddAttribute(sequence++, "class", GetClearButtonClasses());
        builder.AddAttribute(sequence++, "disabled", Disabled || !HasValue());
        builder.AddAttribute(sequence++, "onclick", EventCallback.Factory.Create(this, HandleClear));
        builder.AddAttribute(sequence++, "title", "Clear filter");
        
        builder.OpenElement(sequence++, "i");
        builder.AddAttribute(sequence++, "class", "material-symbols-rounded");
        builder.AddContent(sequence++, "clear");
        builder.CloseElement();
        
        builder.CloseElement();
    }
    
    /// <summary>
    /// Gets the CSS classes for the container
    /// </summary>
    protected virtual string GetContainerClasses()
    {
        var classes = new List<string>
        {
            "filter-container",
            "d-flex",
            "items-center",
            "gap-2"
        };
        
        classes.Add(Density switch
        {
            DensityType.Compact => "filter-compact",
            DensityType.Dense => "filter-dense",
            DensityType.Spacious => "filter-spacious",
            _ => "filter-normal"
        });
        
        if (!string.IsNullOrEmpty(Class))
            classes.Add(Class);
        
        if (Disabled)
            classes.Add("filter-disabled");
            
        return string.Join(" ", classes);
    }
    
    /// <summary>
    /// Gets the CSS classes for the operator selector
    /// </summary>
    protected virtual string GetOperatorSelectorClasses()
    {
        return "operator-selector form-select text-sm border border-border rounded px-2 py-1 bg-surface text-foreground focus:border-primary focus:ring-1 focus:ring-primary/20";
    }
    
    /// <summary>
    /// Gets the CSS classes for the clear button
    /// </summary>
    protected virtual string GetClearButtonClasses()
    {
        return "clear-filter-btn btn btn-ghost btn-sm opacity-60 hover:opacity-100 disabled:opacity-30";
    }
    
    /// <summary>
    /// Gets the display name for an operator
    /// </summary>
    protected virtual string GetOperatorDisplayName(FilterOperator op) => op switch
    {
        FilterOperator.Contains => "Contains",
        FilterOperator.StartsWith => "Starts with",
        FilterOperator.EndsWith => "Ends with", 
        FilterOperator.Equals => "Equals",
        FilterOperator.NotEquals => "Not equals",
        FilterOperator.GreaterThan => "Greater than",
        FilterOperator.LessThan => "Less than",
        FilterOperator.GreaterThanOrEqual => "Greater or equal",
        FilterOperator.LessThanOrEqual => "Less or equal",
        FilterOperator.Between => "Between",
        FilterOperator.NotBetween => "Not between",
        FilterOperator.On => "On",
        FilterOperator.Before => "Before",
        FilterOperator.After => "After",
        FilterOperator.OnOrBefore => "On or before",
        FilterOperator.OnOrAfter => "On or after",
        FilterOperator.InRange => "In range",
        FilterOperator.Today => "Today",
        FilterOperator.Yesterday => "Yesterday",
        FilterOperator.ThisWeek => "This week",
        FilterOperator.LastWeek => "Last week",
        FilterOperator.ThisMonth => "This month",
        FilterOperator.LastMonth => "Last month",
        FilterOperator.ThisYear => "This year",
        FilterOperator.LastYear => "Last year",
        FilterOperator.IsTrue => "Is true",
        FilterOperator.IsFalse => "Is false",
        FilterOperator.In => "In",
        FilterOperator.NotIn => "Not in",
        FilterOperator.IsEmpty => "Is empty",
        FilterOperator.IsNotEmpty => "Is not empty",
        _ => op.ToString()
    };
    
    /// <summary>
    /// Checks if the filter has a value
    /// </summary>
    protected virtual bool HasValue()
    {
        return Value != null || (RequiresSecondValue() && SecondValue != null);
    }
    
    /// <summary>
    /// Checks if the current operator requires a second value
    /// </summary>
    protected virtual bool RequiresSecondValue()
    {
        return Operator is FilterOperator.Between or FilterOperator.NotBetween;
    }
    
    /// <summary>
    /// Checks if the current operator requires no input value
    /// </summary>
    protected virtual bool RequiresNoValue()
    {
        return Operator is 
            FilterOperator.IsEmpty or FilterOperator.IsNotEmpty or
            FilterOperator.IsTrue or FilterOperator.IsFalse or
            FilterOperator.Today or FilterOperator.Yesterday or
            FilterOperator.ThisWeek or FilterOperator.LastWeek or
            FilterOperator.ThisMonth or FilterOperator.LastMonth or
            FilterOperator.ThisYear or FilterOperator.LastYear;
    }
    
    #endregion
    
    #region Event Handlers
    
    /// <summary>
    /// Handles operator selection changes
    /// </summary>
    protected virtual async Task HandleOperatorChange(ChangeEventArgs args)
    {
        if (Enum.TryParse<FilterOperator>(args.Value?.ToString(), out var newOperator))
        {
            var oldFilter = CurrentFilterState;
            Operator = newOperator;
            
            // Clear values if new operator requires no input
            if (RequiresNoValue())
            {
                Value = default;
                SecondValue = default;
            }
            
            await OnOperatorChanged.InvokeAsync(newOperator);
            await NotifyFilterChanged(oldFilter);
        }
    }
    
    /// <summary>
    /// Handles value changes
    /// </summary>
    protected virtual async Task HandleValueChange(T? newValue)
    {
        var oldFilter = CurrentFilterState;
        Value = newValue;
        await NotifyFilterChanged(oldFilter);
    }
    
    /// <summary>
    /// Handles second value changes (for range operators)
    /// </summary>
    protected virtual async Task HandleSecondValueChange(T? newSecondValue)
    {
        var oldFilter = CurrentFilterState;
        SecondValue = newSecondValue;
        await NotifyFilterChanged(oldFilter);
    }
    
    /// <summary>
    /// Handles clearing the filter
    /// </summary>
    protected virtual async Task HandleClear()
    {
        var oldFilter = CurrentFilterState;
        Value = default;
        SecondValue = default;
        
        await OnFilterCleared.InvokeAsync(ColumnKey);
        await NotifyFilterChanged(oldFilter, isCleared: true);
    }
    
    /// <summary>
    /// Notifies subscribers of filter changes
    /// </summary>
    protected virtual async Task NotifyFilterChanged(FilterState oldFilter, bool isCleared = false)
    {
        var eventArgs = new FilterEventArgs(
            ColumnKey,
            oldFilter,
            isCleared ? null : CurrentFilterState,
            new FilterGroup(new List<FilterState> { CurrentFilterState }),
            isCleared
        );
        
        await OnFilterChanged.InvokeAsync(eventArgs);
        StateHasChanged();
    }
    
    #endregion
}