using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using RR.Blazor.Attributes;
using RR.Blazor.Enums;
using RR.Blazor.Components.Base;
using RR.Blazor.Utilities;
using System.Collections;
using static RR.Blazor.Enums.ChoiceVariant;
using static RR.Blazor.Enums.ChoiceType;
using static RR.Blazor.Enums.SizeType;

namespace RR.Blazor.Components.Form;

/// <summary>
/// Base class for choice components with shared parameters
/// Integrates with RR.Blazor design system inheritance chain
/// </summary>
public abstract class RChoiceBase : RSizedComponentBase<SizeType>
{
    #region Form Integration Parameters
    
    [Parameter] public string Label { get; set; } = "";
    [Parameter] public string Placeholder { get; set; } = "";
    [Parameter] public string HelpText { get; set; } = "";
    [Parameter] public string FieldName { get; set; } = "";
    [Parameter] public bool Required { get; set; }
    [Parameter] public bool ReadOnly { get; set; }
    
    #endregion
    
    #region Validation Parameters
    
    [Parameter] public bool HasError { get; set; }
    [Parameter] public string ErrorMessage { get; set; } = "";
    
    #endregion
    
    #region Events
    
    [Parameter] public EventCallback<object> SelectedValueChanged { get; set; }
    [Parameter] public Func<object, string> ItemLabelSelector { get; set; }
    [Parameter] public Func<object, string> ItemIconSelector { get; set; }
    [Parameter] public Func<object, string> ItemTitleSelector { get; set; }
    [Parameter] public Func<object, string> ItemAriaLabelSelector { get; set; }
    [Parameter] public Func<object, bool> ItemDisabledSelector { get; set; }
    [Parameter] public Func<object, bool> ItemLoadingSelector { get; set; }
    [Parameter] public bool ShowLabels { get; set; } = true;
    [Parameter] public bool ShowActiveIndicator { get; set; }
    [Parameter] public Direction Direction { get; set; } = Direction.Horizontal;
    [Parameter] public bool CloseOnSelect { get; set; } = true;
    [Parameter] public int? MaxItemsInline { get; set; } = 5;
    [Parameter] public int? MaxLabelLength { get; set; } = 20;
    
    #endregion
    
    #region Choice Styling
    
    /// <summary>
    /// Style variant for the choice component
    /// </summary>
    [Parameter, AIParameter("Style variant for choice component", "ChoiceType.Standard")]
    public ChoiceType Type { get; set; } = Standard;
    
    #endregion
    
    #region RSizedComponentBase Implementation
    
    /// <summary>
    /// Gets size-specific CSS classes using DensityHelper
    /// </summary>
    protected override string GetSizeClasses()
    {
        return Size switch
        {
            ExtraSmall => "choice-xs " + DensityHelper.GetInputDensityClasses(Density),
            Small => "choice-sm " + DensityHelper.GetInputDensityClasses(Density),
            Medium => "choice-md " + DensityHelper.GetInputDensityClasses(Density),
            Large => "choice-lg " + DensityHelper.GetInputDensityClasses(Density),
            ExtraLarge => "choice-xl " + DensityHelper.GetInputDensityClasses(Density),
            _ => "choice-md " + DensityHelper.GetInputDensityClasses(Density)
        };
    }
    
    /// <summary>
    /// Gets default size for choice components
    /// </summary>
    protected override SizeType GetDefaultSize() => Medium;
    
    /// <summary>
    /// Gets text size classes for choice components
    /// </summary>
    protected override string GetTextSizeClasses()
    {
        return SizeHelper.GetTextSize(Size, Density);
    }
    
    /// <summary>
    /// Gets icon size classes for choice components  
    /// </summary>
    protected override string GetIconSizeClasses()
    {
        return SizeHelper.GetIconSize(Size, Density);
    }
    
    #endregion
}

/// <summary>
/// Universal smart choice component that automatically detects between inline and dropdown modes.
/// Replaces RSwitcher with enhanced capabilities including form integration, custom selectors, and portal positioning.
/// 
/// AI GUIDANCE:
/// - Use for 2-20 exclusive options (status, roles, modes, filters, view switching)
/// - ChoiceVariant.Auto intelligently chooses inline vs dropdown based on item count and label length
/// - For boolean toggles use RToggle, for large datasets use searchable components
/// 
/// SMART DETECTION RULES:
/// - Auto mode switches to dropdown when: >5 items (MaxItemsInline), long labels (MaxLabelLength>20), vertical direction with >3 items
/// - Otherwise uses inline mode with configurable styles (Standard, Pills, Tabs, Buttons, Compact)
/// 
/// COMMON PATTERNS:
/// - Status selection: RChoice Items="@statuses" Variant="ChoiceVariant.Auto" ItemIconSelector for icons
/// - View switching: Type="ChoiceType.Tabs" for tab-like behavior  
/// - Filters: Type="ChoiceType.Pills" Density="DensityType.Compact"
/// - Settings: Use with form integration (Label, Required, HelpText, ErrorMessage)
/// 
/// DROPDOWN FEATURES:
/// - Advanced portal positioning with edge detection and viewport awareness
/// - Click-outside handling and keyboard navigation (Space, Enter, Escape)
/// - Custom trigger content and item templates supported
/// 
/// TYPE HANDLING:
/// - Automatically handles Dictionary<TKey,TValue> by extracting keys and using values as labels
/// - Supports complex objects with ItemLabelSelector, ItemIconSelector, ItemDisabledSelector
/// - Generic type inference from Items collection or SelectedValue
/// </summary>
[Component("RChoice", Category = "Form")]
[AIOptimized(Prompt = "Universal choice component with smart inline/dropdown detection", 
            CommonUse = "status selection, view switching, filters, multi-option toggles, dropdown menus", 
            AvoidUsage = "Don't use for boolean toggles (use RToggle), large datasets (use searchable), immediate actions (use buttons)")]
public class RChoice : RChoiceBase
{
    private object _items = new List<object>();
    private IDictionary _originalDictionary;
    
    [Parameter, AIParameter("Collection of items to choose between - supports arrays, lists, dictionaries", "new[] { \"Option1\", \"Option2\" }")]
    public object Items 
    { 
        get => _items;
        set
        {
            if (value == null)
            {
                _items = new List<object>();
                _originalDictionary = null;
                return;
            }
            
            // Handle Dictionary<TKey, TValue> by extracting keys
            if (value is IDictionary dictionary)
            {
                _originalDictionary = dictionary;
                var keys = new List<object>();
                foreach (var key in dictionary.Keys)
                {
                    keys.Add(key);
                }
                _items = keys;
                return;
            }
            
            // Handle IEnumerable
            if (value is IEnumerable enumerable && !(value is string))
            {
                _items = value;
                _originalDictionary = null;
                return;
            }
            
            // Handle single item
            _items = new[] { value };
            _originalDictionary = null;
        }
    }
    
    [Parameter, AIParameter("Currently selected value from Items collection", "selectedOption")]
    public object SelectedValue { get; set; }
    
    [Parameter, AIParameter("Auto detects inline vs dropdown, or force specific mode", "ChoiceVariant.Auto")]
    public ChoiceVariant Variant { get; set; } = ChoiceVariant.Auto;
    


    private Type _valueType;
    private bool _valueTypeResolved;

    protected override void OnParametersSet()
    {
        base.OnParametersSet();
        if (!_valueTypeResolved)
        {
            _valueType = GetValueType();
            _valueTypeResolved = true;
        }
    }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        var itemsToUse = Items ?? new List<object>();
        
        var itemsCollection = itemsToUse as IEnumerable ?? new[] { itemsToUse };
        var itemsList = itemsCollection.Cast<object>().ToList();
        
        // Resolve value type if not already done
        if (_valueType == null)
        {
            _valueType = GetValueType();
        }

        // Smart detection logic
        var effectiveVariant = GetEffectiveVariant(itemsList);
        
        var genericChoiceType = typeof(RChoiceGeneric<>).MakeGenericType(_valueType);
        
        builder.OpenComponent(0, genericChoiceType);
        
        // Forward all base parameters
        ForwardBaseParameters(builder, _valueType);
        
        // Set computed variant and style
        builder.AddAttribute(1, "EffectiveVariant", effectiveVariant);
        
        // Convert items to the correct generic type for RChoiceGeneric
        var convertedItems = ConvertItemsToGenericType(itemsToUse, _valueType);
        builder.AddAttribute(2, "Items", convertedItems);
        builder.AddAttribute(3, "SelectedValue", SelectedValue);
        builder.AddAttribute(4, "Loading", Loading);
        
        
        builder.CloseComponent();
    }
    
    private object ConvertItemsToGenericType(object items, Type valueType)
    {
        if (items == null)
        {
            // Create empty list of the correct generic type
            var listType = typeof(List<>).MakeGenericType(valueType);
            return Activator.CreateInstance(listType);
        }
        
        if (items is IEnumerable enumerable)
        {
            // Convert to generic List<TValue>
            var listType = typeof(List<>).MakeGenericType(valueType);
            var list = Activator.CreateInstance(listType) as IList;
            
            foreach (var item in enumerable)
            {
                if (item != null && valueType.IsInstanceOfType(item))
                {
                    list.Add(item);
                }
                else if (item != null)
                {
                    // Try to convert the item to the target type
                    try
                    {
                        var convertedItem = Convert.ChangeType(item, valueType);
                        list.Add(convertedItem);
                    }
                    catch
                    {
                        // If conversion fails, add as string representation
                        if (valueType == typeof(string))
                        {
                            list.Add(item.ToString());
                        }
                    }
                }
            }
            
            return list;
        }
        
        // Single item - wrap in generic list
        var singleItemListType = typeof(List<>).MakeGenericType(valueType);
        var singleItemList = Activator.CreateInstance(singleItemListType) as IList;
        
        if (valueType.IsInstanceOfType(items))
        {
            singleItemList.Add(items);
        }
        else
        {
            try
            {
                var convertedItem = Convert.ChangeType(items, valueType);
                singleItemList.Add(convertedItem);
            }
            catch
            {
                if (valueType == typeof(string))
                {
                    singleItemList.Add(items.ToString());
                }
            }
        }
        
        return singleItemList;
    }
    
    private ChoiceVariant GetEffectiveVariant(List<object> items)
    {
        // Explicit variant takes precedence
        if (Variant != ChoiceVariant.Auto)
            return Variant;
        
        // Smart detection rules
        
        // Rule 1: Too many items? Use dropdown
        if (MaxItemsInline.HasValue && items.Count > MaxItemsInline.Value)
            return ChoiceVariant.Dropdown;
        
        // Rule 2: Long text content? Use dropdown  
        if (MaxLabelLength.HasValue && HasLongLabels(items))
            return ChoiceVariant.Dropdown;
        
        // Rule 3: Vertical direction with many items? Use dropdown
        if (Direction == Direction.Vertical && items.Count > 3)
            return ChoiceVariant.Dropdown;
        
        // Rule 4: Default to inline for simple cases
        return ChoiceVariant.Inline;
    }
    
    private bool HasLongLabels(List<object> items)
    {
        if (!MaxLabelLength.HasValue) return false;
        
        return items.Any(item =>
        {
            var label = ItemLabelSelector?.Invoke(item) ?? item?.ToString() ?? "";
            return label.Length > MaxLabelLength.Value;
        });
    }
    
    private void ForwardBaseParameters(RenderTreeBuilder builder, Type itemType)
    {
        var sequence = 100;
        
        // Forward events using object-based parameters - only add if they have delegates
        if (SelectedValueChanged.HasDelegate)
            builder.AddAttribute(sequence++, "SelectedValueChangedObject", SelectedValueChanged);
        
        // Smart ItemLabelSelector for dictionaries
        var labelSelector = ItemLabelSelector;
        if (labelSelector == null && _originalDictionary != null)
        {
            labelSelector = (item) =>
            {
                if (_originalDictionary.Contains(item))
                {
                    return _originalDictionary[item]?.ToString() ?? item?.ToString() ?? "";
                }
                return item?.ToString() ?? "";
            };
        }
        
        // Forward component-specific parameters that can't be auto-forwarded
        builder.AddAttribute(sequence++, "ItemLabelSelectorTyped", labelSelector);
        builder.AddAttribute(sequence++, "ItemIconSelectorTyped", ItemIconSelector);
        builder.AddAttribute(sequence++, "ItemTitleSelectorTyped", ItemTitleSelector);
        builder.AddAttribute(sequence++, "ItemAriaLabelSelectorTyped", ItemAriaLabelSelector);
        builder.AddAttribute(sequence++, "ItemDisabledSelectorTyped", ItemDisabledSelector);
        builder.AddAttribute(sequence++, "ItemLoadingSelectorTyped", ItemLoadingSelector);
        builder.AddAttribute(sequence++, "ShowLabels", ShowLabels);
        builder.AddAttribute(sequence++, "ShowActiveIndicator", ShowActiveIndicator);
        builder.AddAttribute(sequence++, "Size", Size);
        builder.AddAttribute(sequence++, "Type", Type);
        builder.AddAttribute(sequence++, "ChildContent", ChildContent);
        
        // Use RAttributeForwarder for all remaining parameters
        builder.ForwardParameters(ref sequence, this, 
            "Items", "SelectedValue", "Variant",
            "ItemLabelSelector", "ItemIconSelector", "ItemTitleSelector", "ItemAriaLabelSelector",
            "ItemDisabledSelector", "ItemLoadingSelector", "ShowLabels", "ShowActiveIndicator",
            "Size", "Type", "ChildContent", "SelectedValueChanged");
    }

    private Type GetValueType()
    {
        // Try to infer from SelectedValue first
        if (SelectedValue != null)
        {
            return SelectedValue.GetType();
        }

        // Try to infer from Items collection
        if (Items != null)
        {
            var itemsToUse = Items;
            var enumerableType = itemsToUse.GetType();
            
            // Check for generic IEnumerable<T>
            var genericInterface = enumerableType.GetInterfaces()
                .FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEnumerable<>));
            
            if (genericInterface != null)
            {
                return genericInterface.GetGenericArguments()[0];
            }

            // Fallback: check first item
            if (itemsToUse is IEnumerable enumerable)
            {
                var firstItem = enumerable.Cast<object>().FirstOrDefault();
                if (firstItem != null)
                {
                    return firstItem.GetType();
                }
            }
        }

        // Default to object
        return typeof(object);
    }

}


