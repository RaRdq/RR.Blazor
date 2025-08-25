using Microsoft.AspNetCore.Components;
using RR.Blazor.Models;
using RR.Blazor.Enums;
using RR.Blazor.Components.Base;

namespace RR.Blazor.Components;

/// <summary>
/// Smart autosuggest wrapper that automatically detects value types and provides 
/// type-safe suggestions. Supports string search, custom objects, and extensible patterns.
/// </summary>
public class RAutosuggest : RComponentBase
{
    [Parameter] public object Value { get; set; }
    [Parameter] public EventCallback<object> ValueChanged { get; set; }
    [Parameter] public System.Collections.IEnumerable Items { get; set; }
    [Parameter] public Func<string, CancellationToken, Task<System.Collections.IEnumerable>> SearchFunc { get; set; }
    [Parameter] public Func<object, string> ItemTextSelector { get; set; }
    [Parameter] public Func<object, string> ItemIconSelector { get; set; }
    [Parameter] public RenderFragment<object> ItemTemplate { get; set; }
    [Parameter] public RenderFragment EmptyTemplate { get; set; }
    [Parameter] public string Placeholder { get; set; }
    [Parameter] public string Label { get; set; }
    [Parameter] public string HelpText { get; set; }
    [Parameter] public string Icon { get; set; }
    [Parameter] public string StartIcon { get; set; }
    [Parameter] public string EndIcon { get; set; }
    [Parameter] public FieldType InputType { get; set; } = FieldType.Text;
    [Parameter] public SizeType Size { get; set; } = SizeType.Medium;
    [Parameter] public int MinSearchLength { get; set; } = 1;
    [Parameter] public int DebounceDelay { get; set; } = 300;
    [Parameter] public int MaxSuggestions { get; set; } = 10;
    [Parameter] public bool ShowLoading { get; set; } = true;
    [Parameter] public bool ShowDropdownIcon { get; set; } = true;
    [Parameter] public bool ClearOnSelect { get; set; }
    [Parameter] public bool OpenOnFocus { get; set; }
    [Parameter] public bool ReadOnly { get; set; }
    [Parameter] public bool Required { get; set; }
    [Parameter] public EventCallback<object> OnItemSelected { get; set; }
    [Parameter] public EventCallback<string> OnSearchChanged { get; set; }
    [Parameter(CaptureUnmatchedValues = true)] public Dictionary<string, object> AdditionalAttributes { get; set; }

    protected override void BuildRenderTree(Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder builder)
    {
        var valueType = Value?.GetType() ?? typeof(string);
        var itemsType = GetItemsType() ?? typeof(string);
        
        // Use the most specific type available
        var componentType = itemsType != typeof(object) ? itemsType : valueType;
        
        // Build the generic component with the detected type
        var autosuggestType = typeof(RAutosuggestGeneric<>).MakeGenericType(componentType);
        
        builder.OpenComponent(0, autosuggestType);
        
        // Map all parameters to the generic component
        builder.AddAttribute(1, "Value", Value);
        builder.AddAttribute(2, "ValueChanged", CreateTypedCallback(componentType));
        builder.AddAttribute(3, "Items", ConvertItems(componentType));
        builder.AddAttribute(4, "SearchFunc", ConvertSearchFunc(componentType));
        builder.AddAttribute(5, "ItemTextSelector", CreateItemTextSelector(componentType));
        builder.AddAttribute(6, "ItemIconSelector", CreateItemIconSelector(componentType));
        builder.AddAttribute(7, "ItemTemplate", ItemTemplate);
        builder.AddAttribute(8, "EmptyTemplate", EmptyTemplate);
        builder.AddAttribute(9, "Placeholder", Placeholder);
        builder.AddAttribute(10, "Label", Label);
        builder.AddAttribute(11, "HelpText", HelpText);
        builder.AddAttribute(12, "Icon", Icon);
        builder.AddAttribute(13, "StartIcon", StartIcon);
        builder.AddAttribute(14, "EndIcon", EndIcon);
        builder.AddAttribute(15, "InputType", InputType);
        builder.AddAttribute(16, "Size", Size);
        builder.AddAttribute(17, "MinSearchLength", MinSearchLength);
        builder.AddAttribute(18, "DebounceDelay", DebounceDelay);
        builder.AddAttribute(19, "MaxSuggestions", MaxSuggestions);
        builder.AddAttribute(20, "ShowLoading", ShowLoading);
        builder.AddAttribute(21, "ShowDropdownIcon", ShowDropdownIcon);
        builder.AddAttribute(22, "ClearOnSelect", ClearOnSelect);
        builder.AddAttribute(23, "OpenOnFocus", OpenOnFocus);
        builder.AddAttribute(24, "Disabled", Disabled);
        builder.AddAttribute(25, "ReadOnly", ReadOnly);
        builder.AddAttribute(26, "Required", Required);
        builder.AddAttribute(27, "OnItemSelected", CreateItemSelectedCallback(componentType));
        builder.AddAttribute(28, "OnSearchChanged", OnSearchChanged);
        builder.AddAttribute(29, "Class", Class);
        
        // Add additional attributes
        if (AdditionalAttributes != null)
        {
            foreach (var attr in AdditionalAttributes)
            {
                builder.AddAttribute(30, attr.Key, attr.Value);
            }
        }
        
        builder.CloseComponent();
    }

    private Type GetItemsType()
    {
        if (Items == null) return null;
        
        var itemsType = Items.GetType();
        
        // Handle IEnumerable<T>
        if (itemsType.IsGenericType)
        {
            var genericDef = itemsType.GetGenericTypeDefinition();
            if (genericDef == typeof(List<>) || 
                genericDef == typeof(IEnumerable<>) ||
                genericDef == typeof(ICollection<>) ||
                genericDef == typeof(IList<>))
            {
                return itemsType.GetGenericArguments()[0];
            }
        }
        
        // Handle arrays
        if (itemsType.IsArray)
        {
            return itemsType.GetElementType();
        }
        
        // Try to get type from first item
        var enumerator = Items.GetEnumerator();
        if (enumerator.MoveNext() && enumerator.Current != null)
        {
            return enumerator.Current.GetType();
        }
        
        return typeof(object);
    }

    private object CreateTypedCallback(Type itemType)
    {
        var eventCallbackType = typeof(EventCallback<>).MakeGenericType(itemType);
        var method = GetType().GetMethod(nameof(TypedValueChangedWrapper), System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var genericMethod = method.MakeGenericMethod(itemType);
        return Activator.CreateInstance(eventCallbackType, this, genericMethod);
    }

    private object CreateItemSelectedCallback(Type itemType)
    {
        var eventCallbackType = typeof(EventCallback<>).MakeGenericType(itemType);
        var method = GetType().GetMethod(nameof(TypedItemSelectedWrapper), System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var genericMethod = method.MakeGenericMethod(itemType);
        return Activator.CreateInstance(eventCallbackType, this, genericMethod);
    }

    private async Task TypedValueChangedWrapper<T>(T value)
    {
        Value = value;
        await ValueChanged.InvokeAsync(value);
    }

    private async Task TypedItemSelectedWrapper<T>(T item)
    {
        await OnItemSelected.InvokeAsync(item);
    }

    private object ConvertItems(Type itemType)
    {
        if (Items == null) return Activator.CreateInstance(typeof(List<>).MakeGenericType(itemType));
        
        // If types match, return as-is
        if (Items.GetType().IsAssignableFrom(typeof(IEnumerable<>).MakeGenericType(itemType)))
        {
            return Items;
        }
        
        // Convert to correct generic type
        var listType = typeof(List<>).MakeGenericType(itemType);
        var list = Activator.CreateInstance(listType);
        var addMethod = listType.GetMethod("Add");
        
        foreach (var item in Items)
        {
            if (item != null && itemType.IsInstanceOfType(item))
            {
                addMethod.Invoke(list, new[] { item });
            }
        }
        
        return list;
    }

    private object ConvertSearchFunc(Type itemType)
    {
        if (SearchFunc == null) return null;
        
        // Create a typed wrapper for the search function
        var funcType = typeof(Func<,,>).MakeGenericType(typeof(string), typeof(CancellationToken), typeof(Task<>).MakeGenericType(typeof(IEnumerable<>).MakeGenericType(itemType)));
        
        return Delegate.CreateDelegate(funcType, this, nameof(TypedSearchWrapper));
    }

    private async Task<IEnumerable<T>> TypedSearchWrapper<T>(string query, CancellationToken cancellationToken)
    {
        if (SearchFunc == null) return Enumerable.Empty<T>();
        
        var result = await SearchFunc(query, cancellationToken);
        
        if (result == null) return Enumerable.Empty<T>();
        
        return result.OfType<T>();
    }

    private object CreateItemTextSelector(Type itemType)
    {
        if (ItemTextSelector != null)
        {
            // Create typed delegate wrapper
            var funcType = typeof(Func<,>).MakeGenericType(itemType, typeof(string));
            return Delegate.CreateDelegate(funcType, this, nameof(TypedItemTextWrapper));
        }
        
        // Default selector - use ToString()
        var defaultFuncType = typeof(Func<,>).MakeGenericType(itemType, typeof(string));
        return Delegate.CreateDelegate(defaultFuncType, this, nameof(DefaultItemTextSelector));
    }

    private string TypedItemTextWrapper<T>(T item)
    {
        return ItemTextSelector?.Invoke(item) ?? item?.ToString() ?? string.Empty;
    }

    private string DefaultItemTextSelector<T>(T item)
    {
        return item?.ToString() ?? string.Empty;
    }

    private object CreateItemIconSelector(Type itemType)
    {
        if (ItemIconSelector == null) return null;
        
        var funcType = typeof(Func<,>).MakeGenericType(itemType, typeof(string));
        return Delegate.CreateDelegate(funcType, this, nameof(TypedItemIconWrapper));
    }

    private string TypedItemIconWrapper<T>(T item)
    {
        return ItemIconSelector?.Invoke(item) ?? string.Empty;
    }
}