using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using RR.Blazor.Attributes;
using RR.Blazor.Enums;
using RR.Blazor.Components.Base;
using System.Collections;
using static RR.Blazor.Enums.ChoiceVariant;
using static RR.Blazor.Enums.ChoiceStyle;
using static RR.Blazor.Enums.ChoiceSize;
using static RR.Blazor.Enums.ChoiceDirection;

namespace RR.Blazor.Components.Form;

/// <summary>
/// Base class for choice components with shared parameters
/// </summary>
public abstract class RChoiceBase : ComponentBase
{
    #region Form Integration Parameters
    
    [Parameter] public string Label { get; set; } = "";
    [Parameter] public string Placeholder { get; set; } = "";
    [Parameter] public string HelpText { get; set; } = "";
    [Parameter] public string FieldName { get; set; } = "";
    [Parameter] public bool Required { get; set; }
    [Parameter] public bool ReadOnly { get; set; }
    [Parameter] public bool Loading { get; set; }
    
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
    [Parameter] public ChoiceSize Size { get; set; } = Medium;
    [Parameter] public ComponentDensity Density { get; set; } = ComponentDensity.Normal;
    [Parameter] public ChoiceDirection Direction { get; set; } = Horizontal;
    [Parameter] public bool Disabled { get; set; }
    [Parameter] public bool CloseOnSelect { get; set; } = true;
    [Parameter] public string AriaLabel { get; set; }
    [Parameter] public int? MaxItemsInline { get; set; } = 5;
    [Parameter] public int? MaxLabelLength { get; set; } = 20;
    [Parameter] public string Class { get; set; }
    [Parameter] public RenderFragment ChildContent { get; set; }
    
    #endregion
}

/// <summary>
/// Universal smart choice component that automatically detects between inline and dropdown modes.
/// Replaces RSwitcher with enhanced capabilities including form integration, custom selectors, and portal positioning.
/// 
/// AI GUIDANCE:
/// - Use for 2-20 exclusive options (status, roles, modes, filters, view switching)
/// - ChoiceVariant.Auto intelligently chooses inline vs dropdown based on item count and label length
/// - Supports RSwitcher migration with SwitcherVariant/SwitcherSize compatibility parameters
/// - For boolean toggles use RToggle, for large datasets use searchable components
/// 
/// SMART DETECTION RULES:
/// - Auto mode switches to dropdown when: >5 items (MaxItemsInline), long labels (MaxLabelLength>20), vertical direction with >3 items
/// - Otherwise uses inline mode with configurable styles (Standard, Pills, Tabs, Buttons, Compact)
/// 
/// COMMON PATTERNS:
/// - Status selection: RChoice Items="@statuses" Variant="ChoiceVariant.Auto" ItemIconSelector for icons
/// - View switching: Style="ChoiceStyle.Tabs" for tab-like behavior  
/// - Filters: Style="ChoiceStyle.Pills" Density="ComponentDensity.Compact"
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
    private object _items;
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
    public ChoiceVariant Variant { get; set; } = Auto;
    
    [Parameter, AIParameter("Visual style for inline mode: Standard, Pills, Tabs, Buttons, Compact", "ChoiceStyle.Standard")]
    public ChoiceStyle Style { get; set; } = Standard;

    // RSwitcher compatibility parameters
    [Parameter] public SwitcherVariant? SwitcherVariant { get; set; }
    [Parameter] public ButtonSize? SwitcherSize { get; set; }
    [Parameter] public string LoadingText { get; set; } = "Loading...";
    [Parameter] public EventCallback<object> OnSelectionChanged { get; set; }

    private Type _valueType;
    private bool _valueTypeResolved;

    public RChoice()
    {
        _items = new List<object>();
    }

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
        
        var itemsCollection = itemsToUse as System.Collections.IEnumerable ?? new[] { itemsToUse };
        var itemsList = itemsCollection.Cast<object>().ToList();
        
        // Resolve value type if not already done
        if (_valueType == null)
        {
            _valueType = GetValueType();
        }

        // Smart detection logic
        var effectiveVariant = GetEffectiveVariant(itemsList);
        var effectiveStyle = GetEffectiveStyle();
        var effectiveSize = GetEffectiveSize();
        
        var genericChoiceType = typeof(RChoiceGeneric<>).MakeGenericType(_valueType);
        
        builder.OpenComponent(0, genericChoiceType);
        
        // Forward all base parameters
        ForwardBaseParameters(builder, _valueType, effectiveStyle, effectiveSize);
        
        // Set computed variant and style
        builder.AddAttribute(1, "EffectiveVariant", effectiveVariant);
        
        // Convert items to the correct generic type for RChoiceGeneric
        var convertedItems = ConvertItemsToGenericType(itemsToUse, _valueType);
        builder.AddAttribute(2, "Items", convertedItems);
        builder.AddAttribute(3, "SelectedValue", SelectedValue);
        builder.AddAttribute(4, "Loading", Loading);
        
        // Handle OnSelectionChanged compatibility
        if (OnSelectionChanged.HasDelegate)
        {
            builder.AddAttribute(7, "SelectedValueChangedObject", OnSelectionChanged);
        }
        
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
        
        if (items is System.Collections.IEnumerable enumerable)
        {
            // Convert to generic List<TValue>
            var listType = typeof(List<>).MakeGenericType(valueType);
            var list = Activator.CreateInstance(listType) as System.Collections.IList;
            
            foreach (var item in enumerable)
            {
                if (item != null && valueType.IsAssignableFrom(item.GetType()))
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
        var singleItemList = Activator.CreateInstance(singleItemListType) as System.Collections.IList;
        
        if (items != null && valueType.IsAssignableFrom(items.GetType()))
        {
            singleItemList.Add(items);
        }
        else if (items != null)
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
        if (Variant != Auto)
            return Variant;
        
        // Smart detection rules
        
        // Rule 1: Too many items? Use dropdown
        if (MaxItemsInline.HasValue && items.Count > MaxItemsInline.Value)
            return Dropdown;
        
        // Rule 2: Long text content? Use dropdown  
        if (MaxLabelLength.HasValue && HasLongLabels(items))
            return Dropdown;
        
        // Rule 3: Vertical direction with many items? Use dropdown
        if (Direction == Vertical && items.Count > 3)
            return Dropdown;
        
        // Rule 4: Default to inline for simple cases
        return Inline;
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
    
    private void ForwardBaseParameters(RenderTreeBuilder builder, Type itemType, ChoiceStyle effectiveStyle, ChoiceSize effectiveSize)
    {
        // Forward events using object-based parameters - only add if they have delegates
        if (SelectedValueChanged.HasDelegate)
            builder.AddAttribute(100, "SelectedValueChangedObject", SelectedValueChanged);
        
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
        
        builder.AddAttribute(101, "ItemLabelSelectorTyped", labelSelector);
        builder.AddAttribute(102, "ItemIconSelectorTyped", ItemIconSelector);
        builder.AddAttribute(103, "ItemTitleSelectorTyped", ItemTitleSelector);
        builder.AddAttribute(104, "ItemAriaLabelSelectorTyped", ItemAriaLabelSelector);
        builder.AddAttribute(105, "ItemDisabledSelectorTyped", ItemDisabledSelector);
        builder.AddAttribute(106, "ItemLoadingSelectorTyped", ItemLoadingSelector);
        builder.AddAttribute(107, "ShowLabels", ShowLabels);
        builder.AddAttribute(108, "ShowActiveIndicator", ShowActiveIndicator);
        builder.AddAttribute(109, "Size", effectiveSize);
        builder.AddAttribute(110, "Density", Density);
        builder.AddAttribute(111, "Direction", Direction);
        builder.AddAttribute(112, "Disabled", Disabled);
        builder.AddAttribute(113, "CloseOnSelect", CloseOnSelect);
        builder.AddAttribute(114, "AriaLabel", AriaLabel);
        builder.AddAttribute(115, "Style", effectiveStyle);
        builder.AddAttribute(116, "Class", Class);
        builder.AddAttribute(117, "ChildContent", ChildContent);
        
        // Forward form integration parameters
        builder.AddAttribute(118, "Label", Label);
        builder.AddAttribute(119, "Placeholder", Placeholder);
        builder.AddAttribute(120, "HelpText", HelpText);
        builder.AddAttribute(121, "FieldName", FieldName);
        builder.AddAttribute(122, "Required", Required);
        builder.AddAttribute(123, "ReadOnly", ReadOnly);
        builder.AddAttribute(124, "Loading", Loading);
        builder.AddAttribute(125, "HasError", HasError);
        builder.AddAttribute(126, "ErrorMessage", ErrorMessage);
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

    private ChoiceStyle GetEffectiveStyle()
    {
        // RSwitcher compatibility - SwitcherVariant takes precedence
        if (SwitcherVariant.HasValue)
        {
            return SwitcherVariant.Value switch
            {
                Enums.SwitcherVariant.Tabs => Tabs,
                Enums.SwitcherVariant.Pills => Pills,
                Enums.SwitcherVariant.Buttons => Buttons,
                Enums.SwitcherVariant.Compact => Compact,
                _ => Standard
            };
        }
        
        return Style;
    }

    private ChoiceSize GetEffectiveSize()
    {
        // RSwitcher compatibility - SwitcherSize takes precedence
        if (SwitcherSize.HasValue)
        {
            return SwitcherSize.Value switch
            {
                ButtonSize.Small => Small,
                ButtonSize.Large => Large,
                _ => Medium
            };
        }
        
        return Size;
    }
}


