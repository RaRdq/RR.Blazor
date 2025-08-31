using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using RR.Blazor.Components.Base;
using RR.Blazor.Models;
using RR.Blazor.Enums;
using RR.Blazor.Interfaces;

namespace RR.Blazor.Components.Data;

/// <summary>
/// Base class for all filter components following Onion architecture
/// </summary>
public abstract class RFilterBase : RSizedComponentBase<SizeType>
{
    // ===== LINKABLE INTERFACE =====
    
    /// <summary>
    /// Components linked to this filter for automatic updates
    /// </summary>
    public List<IFilterable> LinkedComponents { get; } = new();
    
    /// <summary>
    /// Link a component to this filter
    /// </summary>
    public void LinkComponent(IFilterable component)
    {
        if (!LinkedComponents.Contains(component))
        {
            LinkedComponents.Add(component);
        }
    }
    
    /// <summary>
    /// Unlink a component from this filter
    /// </summary>
    public void UnlinkComponent(IFilterable component)
    {
        LinkedComponents.Remove(component);
    }
    
    // ===== CORE PARAMETERS =====
    
    /// <summary>Filter value</summary>
    [Parameter] public object Value { get; set; }
    
    /// <summary>Second value for range operators</summary>
    [Parameter] public object SecondValue { get; set; }
    
    /// <summary>Filter operator</summary>
    [Parameter] public FilterOperator Operator { get; set; } = FilterOperator.Contains;
    
    /// <summary>Placeholder text for filter input</summary>
    [Parameter] public string Placeholder { get; set; } = "Enter value...";
    
    /// <summary>Current filter state</summary>
    protected FilterCondition CurrentFilterState { get; set; }
    
    /// <summary>Show search input</summary>
    [Parameter] public bool ShowSearch { get; set; } = true;
    
    /// <summary>Search placeholder text</summary>
    [Parameter] public string SearchPlaceholder { get; set; } = "Search...";
    
    /// <summary>Show date range filter</summary>
    [Parameter] public bool ShowDateRange { get; set; }
    
    /// <summary>Show quick filter buttons</summary>
    [Parameter] public bool ShowQuickFilters { get; set; } = true;
    
    /// <summary>Show advanced filter panel toggle</summary>
    [Parameter] public bool ShowAdvanced { get; set; } = true;
    
    /// <summary>Show clear all button</summary>
    [Parameter] public bool ShowClearButton { get; set; } = true;
    
    /// <summary>Show active filter count</summary>
    [Parameter] public bool ShowFilterCount { get; set; } = true;
    
    /// <summary>Minimal mode - just search box</summary>
    [Parameter] public bool Minimal { get; set; }
    
    /// <summary>Compact mode for toolbar integration</summary>
    [Parameter] public bool Compact { get; set; }
    
    /// <summary>Custom filter content</summary>
    [Parameter] public RenderFragment CustomFilters { get; set; }
    
    /// <summary>Component ID for persistence</summary>
    [Parameter] public string ComponentId { get; set; } = Guid.NewGuid().ToString();
    
    // ===== ABSTRACT METHODS =====
    
    /// <summary>
    /// Apply filter to data source
    /// </summary>
    public abstract Task<object> ApplyFilterAsync(object dataSource);
    
    /// <summary>
    /// Get the type of items being filtered
    /// </summary>
    public abstract Type GetItemType();
    
    /// <summary>
    /// Clear all filters
    /// </summary>
    public abstract Task ClearAsync();
    
    /// <summary>
    /// Refresh filter results
    /// </summary>
    public abstract Task RefreshAsync();
    
    /// <summary>
    /// Get the filter type for this component
    /// </summary>
    protected abstract FilterType FilterType { get; }
    
    /// <summary>
    /// Get available operators for this filter type
    /// </summary>
    protected abstract List<FilterOperator> AvailableOperators { get; }
    
    /// <summary>
    /// Render the filter input control
    /// </summary>
    protected abstract void RenderFilterInput(RenderTreeBuilder builder);
    
    // ===== PROTECTED HELPERS =====
    
    /// <summary>
    /// Handle value change events
    /// </summary>
    protected virtual async Task HandleValueChange(object newValue)
    {
        Value = newValue;
        await NotifyFilterChanged();
    }
    
    /// <summary>
    /// Handle operator change events
    /// </summary>
    protected virtual async Task OnOperatorChanged(FilterOperator newOperator)
    {
        Operator = newOperator;
        await NotifyFilterChanged();
    }
    
    /// <summary>
    /// Notify filter changed event
    /// </summary>
    protected virtual async Task NotifyFilterChanged()
    {
        // Update current filter state
        CurrentFilterState = new FilterCondition(
            ComponentId,
            FilterType,
            Operator,
            Value,
            SecondValue,
            HasValue()
        );
        
        // Notify any change callbacks
        await InvokeAsync(StateHasChanged);
    }
    
    /// <summary>
    /// Check if operator requires no value input
    /// </summary>
    protected virtual bool RequiresNoValue => Operator switch
    {
        FilterOperator.IsEmpty => true,
        FilterOperator.IsNotEmpty => true,
        FilterOperator.IsTrue => true,
        FilterOperator.IsFalse => true,
        _ => false
    };
    
    /// <summary>
    /// Check if operator requires a second value (for range operations)
    /// </summary>
    protected virtual bool RequiresSecondValue => Operator switch
    {
        FilterOperator.InRange => true,
        FilterOperator.Between => true,
        _ => false
    };
    
    /// <summary>
    /// Check if the filter has a value
    /// </summary>
    protected virtual bool HasValue()
    {
        if (RequiresNoValue) return true;
        if (RequiresSecondValue) return Value != null && SecondValue != null;
        return Value != null && !string.IsNullOrEmpty(Value.ToString());
    }
    
    /// <summary>
    /// Notify all linked components of filter changes
    /// </summary>
    protected async Task NotifyLinkedComponents(object filteredData)
    {
        var tasks = LinkedComponents.Select(component => component.ApplyFilterAsync(filteredData));
        await Task.WhenAll(tasks);
    }
    
    /// <summary>
    /// Get container CSS classes
    /// </summary>
    protected string GetContainerClasses()
    {
        var classes = new List<string> { "rfilter" };
        
        if (Minimal) classes.Add("rfilter-minimal");
        if (Compact) classes.Add("rfilter-compact");
        if (!string.IsNullOrEmpty(Class)) classes.Add(Class);
        
        return string.Join(" ", classes);
    }
    
    /// <summary>
    /// Get combined attributes
    /// </summary>
    protected Dictionary<string, object> GetCombinedAttributes()
    {
        var attributes = new Dictionary<string, object>
        {
            ["class"] = GetContainerClasses(),
            ["data-filter-id"] = ComponentId
        };
        
        // AdditionalAttributes is inherited from RComponentBase
        if (AdditionalAttributes != null)
        {
            foreach (var attr in AdditionalAttributes)
            {
                attributes[attr.Key] = attr.Value;
            }
        }
        
        return attributes;
    }
    
    // ===== RSIZEDCOMPONENTBASE IMPLEMENTATION =====
    
    /// <summary>
    /// Get size-specific CSS classes for filter components
    /// </summary>
    protected override string GetSizeClasses()
    {
        return Size switch
        {
            SizeType.ExtraSmall => "rfilter-xs",
            SizeType.Small => "rfilter-sm",
            SizeType.Medium => "rfilter-md",
            SizeType.Large => "rfilter-lg",
            SizeType.ExtraLarge => "rfilter-xl",
            _ => "rfilter-md"
        };
    }
}