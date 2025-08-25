using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using RR.Blazor.Attributes;
using RR.Blazor.Components.Base;
using RR.Blazor.Enums;
using RR.Blazor.Models;
using System.Collections;
using System.Linq.Expressions;

namespace RR.Blazor.Components.Data;

/// <summary>
/// Base class for grid components with shared parameters.
/// Provides common functionality for both smart and generic grid implementations.
/// </summary>
public abstract class RGridBase : RComponentBase
{
    #region Core Parameters
    
    [Parameter] public object Configuration { get; set; }
    [Parameter] public object State { get; set; }
    [Parameter] public string Title { get; set; }
    [Parameter] public string Subtitle { get; set; }
    [Parameter] public string Icon { get; set; }
    [Parameter] public bool ShowHeader { get; set; } = true;
    
    #endregion
    
    #region Feature Toggles
    
    [Parameter] public bool EnablePaging { get; set; } = true;
    [Parameter] public int PageSize { get; set; } = 10;
    [Parameter] public bool EnableSorting { get; set; } = true;
    [Parameter] public bool EnableFiltering { get; set; } = true;
    [Parameter] public bool EnableGrouping { get; set; }
    [Parameter] public bool EnableColumnReordering { get; set; }
    [Parameter] public bool EnableExport { get; set; }
    [Parameter] public bool EnableVirtualization { get; set; }
    [Parameter] public int VirtualizationThreshold { get; set; } = 100;
    [Parameter] public bool EnableRealTimeUpdates { get; set; }
    [Parameter] public bool EnableMasterDetail { get; set; }
    [Parameter] public bool EnableRowSelection { get; set; }
    [Parameter] public GridSelectionMode SelectionMode { get; set; } = GridSelectionMode.Single;
    
    #endregion
    
    #region Events - Object-based for smart detection
    
    [Parameter] public EventCallback<object> StateChanged { get; set; }
    [Parameter] public EventCallback<object> OnRowClick { get; set; }
    [Parameter] public EventCallback<object> OnCellClick { get; set; }
    [Parameter] public EventCallback<object> OnSelectionChanged { get; set; }
    [Parameter] public EventCallback<object> OnDataUpdated { get; set; }
    
    #endregion
    
    #region Content
    
    [Parameter] public RenderFragment HeaderActions { get; set; }
    [Parameter] public RenderFragment EmptyContent { get; set; }
    // ChildContent inherited from RForwardingComponentBase (using new keyword override)
    
    #endregion
    
    #region Column Configuration
    
    [Parameter] public Func<object, string> ItemKeySelector { get; set; }
    [Parameter] public Func<object, string> ItemDisplaySelector { get; set; }
    [Parameter] public bool AutoGenerateColumns { get; set; } = true;
    [Parameter] public List<string> VisibleColumns { get; set; }
    [Parameter] public List<string> HiddenColumns { get; set; }
    
    #endregion
}

/// <summary>
/// Smart wrapper for RGrid that automatically detects data type from DataSource.
/// Provides type-agnostic usage while maintaining backward compatibility.
/// 
/// AI GUIDANCE:
/// - Use for tabular data display without specifying TItem type
/// - Automatically infers type from DataSource collection
/// - Supports IEnumerable, IQueryable, arrays, lists, and dictionaries
/// - Falls back gracefully when data is null or empty
/// 
/// SMART DETECTION RULES:
/// - Infers type from generic collection interfaces first
/// - Falls back to examining first item in collection
/// - Handles Dictionary by extracting values
/// - Defaults to object type if detection fails
/// 
/// COMMON PATTERNS:
/// - Simple usage: RGrid DataSource="@employees"
/// - With columns: RGrid DataSource="@data" Columns="@columns"
/// - With configuration: RGrid DataSource="@items" Configuration="@gridConfig"
/// 
/// TYPE HANDLING:
/// - Automatically handles IEnumerable<T> and IQueryable<T>
/// - Supports Dictionary<TKey,TValue> by extracting values
/// - Handles arrays, lists, and custom collections
/// - Graceful fallback for non-generic collections
/// </summary>
[Component("RGrid", Category = "Data")]
[AIOptimized(
    Prompt = "Smart data grid with automatic type detection from DataSource", 
    CommonUse = "tabular data display, lists, reports, data management interfaces", 
    AvoidUsage = "Don't use for simple card layouts (use RCard), non-tabular data (use appropriate display component)"
)]
public class RGrid : RGridBase
{
    private object _dataSource;
    private Type _itemType;
    private bool _itemTypeResolved;
    
    /// <summary>
    /// The data source for the grid. Supports any IEnumerable collection.
    /// Type will be automatically detected from the collection.
    /// </summary>
    [Parameter, AIParameter("Collection of items to display in the grid", "@employees or @dataList")]
    public object DataSource 
    { 
        get => _dataSource;
        set
        {
            _dataSource = value;
            _itemTypeResolved = false; // Reset type resolution when data changes
        }
    }
    
    /// <summary>
    /// Optional explicit type specification for backward compatibility.
    /// If not provided, type will be inferred from DataSource.
    /// </summary>
    [Parameter, AIParameter("Optional explicit item type (usually auto-detected)", "typeof(Employee)")]
    public Type TItem { get; set; }
    
    /// <summary>
    /// Column definitions. If not provided, columns will be auto-generated from data properties.
    /// </summary>
    [Parameter] public object Columns { get; set; }
    
    /// <summary>
    /// Master-detail template for expandable rows.
    /// </summary>
    [Parameter] public RenderFragment<object> MasterDetailTemplate { get; set; }
    
    protected override void OnParametersSet()
    {
        base.OnParametersSet();
        
        if (!_itemTypeResolved)
        {
            _itemType = GetItemType();
            _itemTypeResolved = true;
        }
    }
    
    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        // Ensure we have a valid item type
        if (_itemType == null)
        {
            _itemType = GetItemType();
        }
        
        // If still no type, show empty state
        if (_itemType == null)
        {
            builder.OpenElement(0, "div");
            builder.AddAttribute(1, "class", "rgrid-empty");
            builder.AddContent(2, EmptyContent ?? (RenderFragment)(b => b.AddContent(0, "No data available")));
            builder.CloseElement();
            return;
        }
        
        // Security check: Ensure we're working with a safe type
        // Don't check IsGenericTypeDefinition - we work with constructed generic types
        if (_itemType.IsPointer || _itemType.IsByRef || _itemType.ContainsGenericParameters)
        {
            builder.AddContent(0, "Invalid data type provided to RGrid");
            return;
        }
        
        try
        {
            // Create the generic RGridGeneric<TItem> component
            var genericGridType = typeof(RGridGeneric<>).MakeGenericType(_itemType);
            
            builder.OpenComponent(0, genericGridType);
            
            // Forward all parameters
            ForwardParameters(builder);
            
            builder.CloseComponent();
        }
        catch (Exception ex)
        {
            // Fallback error display with detailed info
            builder.OpenElement(0, "div");
            builder.AddAttribute(1, "class", "rgrid-error");
            builder.AddContent(2, $"Error creating grid: {ex.Message}. Type: {_itemType?.FullName ?? "null"}");
            builder.CloseElement();
        }
    }
    
    private void ForwardParameters(RenderTreeBuilder builder)
    {
        var sequence = 1;
        
        // Forward DataSource as Items (converted to correct generic type)
        var convertedData = ConvertDataToGenericType(_dataSource, _itemType);
        builder.AddAttribute(sequence++, "Items", convertedData);
        
        // Forward column definitions if provided
        if (Columns != null)
        {
            var convertedColumns = ConvertColumnsToGenericType(Columns, _itemType);
            builder.AddAttribute(sequence++, "Columns", convertedColumns);
        }
        
        // Forward configuration (create default if not provided)
        if (Configuration != null)
        {
            var genericConfigType = typeof(GridConfiguration<>).MakeGenericType(_itemType);
            var convertedConfig = ConvertConfigurationToGenericType(Configuration, genericConfigType);
            builder.AddAttribute(sequence++, "Configuration", convertedConfig);
        }
        else
        {
            // Create default configuration with parameters
            var genericConfigType = typeof(GridConfiguration<>).MakeGenericType(_itemType);
            var defaultConfig = Activator.CreateInstance(genericConfigType);
            
            // Apply individual parameters to configuration
            if (defaultConfig != null)
            {
                ApplyParametersToConfiguration(defaultConfig);
            }
            
            builder.AddAttribute(sequence++, "Configuration", defaultConfig);
        }
        
        // Forward state
        if (State != null)
        {
            var genericStateType = typeof(GridState<>).MakeGenericType(_itemType);
            var convertedState = ConvertStateToGenericType(State, genericStateType);
            builder.AddAttribute(sequence++, "State", convertedState);
        }
        
        // Forward events using object versions for compatibility
        if (StateChanged.HasDelegate)
            builder.AddAttribute(sequence++, "StateChangedObject", StateChanged);
        if (OnRowClick.HasDelegate)
            builder.AddAttribute(sequence++, "OnRowClickObject", OnRowClick);
        if (OnCellClick.HasDelegate)
            builder.AddAttribute(sequence++, "OnCellClickObject", OnCellClick);
        if (OnSelectionChanged.HasDelegate)
            builder.AddAttribute(sequence++, "OnSelectionChangedObject", OnSelectionChanged);
        if (OnDataUpdated.HasDelegate)
            builder.AddAttribute(sequence++, "OnDataUpdatedObject", OnDataUpdated);
        
        // Forward content fragments
        if (HeaderActions != null)
            builder.AddAttribute(sequence++, "HeaderActions", HeaderActions);
        if (EmptyContent != null)
            builder.AddAttribute(sequence++, "EmptyContent", EmptyContent);
        if (MasterDetailTemplate != null)
        {
            // Convert to typed RenderFragment
            var typedFragment = CreateTypedRenderFragment(MasterDetailTemplate);
            builder.AddAttribute(sequence++, "MasterDetailTemplate", typedFragment);
        }
        
        // Forward base component attributes
        if (!string.IsNullOrEmpty(Class))
            builder.AddAttribute(sequence++, "Class", Class);
        
        // Don't forward AdditionalAttributes as a parameter - the target component has CaptureUnmatchedValues
        // Instead, forward individual attributes using AddMultipleAttributes
        if (AdditionalAttributes != null && AdditionalAttributes.Count > 0)
        {
            builder.AddMultipleAttributes(sequence++, AdditionalAttributes);
        }
    }
    
    private Type GetItemType()
    {
        // Priority 1: Explicit TItem parameter
        if (TItem != null)
        {
            return TItem;
        }
        
        // Priority 2: Infer from DataSource
        if (_dataSource == null)
        {
            return null; // No data, no type
        }
        
        var dataType = _dataSource.GetType();
        
        // Check if it's directly a generic collection type (List<T>, IEnumerable<T>, etc.)
        if (dataType.IsGenericType)
        {
            var genericDef = dataType.GetGenericTypeDefinition();
            var genericArgs = dataType.GetGenericArguments();
            
            // Handle common generic collection types
            if ((genericDef == typeof(List<>) || 
                 genericDef == typeof(IList<>) ||
                 genericDef == typeof(ICollection<>) ||
                 genericDef == typeof(IEnumerable<>) ||
                 genericDef == typeof(IQueryable<>) ||
                 genericDef == typeof(HashSet<>)) && 
                genericArgs.Length == 1)
            {
                return genericArgs[0];
            }
            
            // Handle Dictionary<TKey, TValue> - use value type
            if (genericDef == typeof(Dictionary<,>) && genericArgs.Length == 2)
            {
                return genericArgs[1];
            }
        }
        
        // Check for array types
        if (dataType.IsArray)
        {
            return dataType.GetElementType();
        }
        
        // Check interfaces for generic IEnumerable<T> or IQueryable<T>
        foreach (var interfaceType in dataType.GetInterfaces())
        {
            if (interfaceType.IsGenericType)
            {
                var genericDef = interfaceType.GetGenericTypeDefinition();
                if ((genericDef == typeof(IEnumerable<>) || genericDef == typeof(IQueryable<>)))
                {
                    var genericArgs = interfaceType.GetGenericArguments();
                    if (genericArgs.Length == 1 && genericArgs[0] != typeof(object))
                    {
                        return genericArgs[0];
                    }
                }
            }
        }
        
        // Last resort: examine first item in collection
        if (_dataSource is IEnumerable enumerable)
        {
            foreach (var item in enumerable)
            {
                if (item != null)
                {
                    var itemType = item.GetType();
                    // Avoid returning system types as item types
                    if (!itemType.Namespace.StartsWith("System") || itemType.IsValueType || itemType == typeof(string))
                    {
                        return itemType;
                    }
                    return itemType;
                }
            }
        }
        
        // Default fallback for non-null data source
        return typeof(object);
    }
    
    private object ConvertDataToGenericType(object data, Type itemType)
    {
        if (data == null || itemType == null)
        {
            // Return empty list of correct type
            var safeType = itemType ?? typeof(object);
            var listType = typeof(List<>).MakeGenericType(safeType);
            return Activator.CreateInstance(listType);
        }
        
        var dataType = data.GetType();
        
        // If data is already of correct type, return as-is
        if (dataType.IsGenericType)
        {
            var genericDef = dataType.GetGenericTypeDefinition();
            var genericArgs = dataType.GetGenericArguments();
            
            // Check if it's already the correct generic collection
            if (genericArgs.Length == 1 && genericArgs[0] == itemType)
            {
                if (genericDef == typeof(List<>) || 
                    genericDef == typeof(IList<>) ||
                    genericDef == typeof(ICollection<>) ||
                    genericDef == typeof(IEnumerable<>) || 
                    genericDef == typeof(IQueryable<>))
                {
                    return data;
                }
            }
        }
        
        // Handle Dictionary - extract values
        if (dataType.IsGenericType && dataType.GetGenericTypeDefinition() == typeof(Dictionary<,>))
        {
            var dictMethod = dataType.GetProperty("Values");
            if (dictMethod != null)
            {
                var values = dictMethod.GetValue(data) as IEnumerable;
                return ConvertEnumerableToList(values, itemType);
            }
        }
        
        // Special handling for strings - don't enumerate as char collection
        if (data is string stringData && itemType == typeof(string))
        {
            var stringListType = typeof(List<>).MakeGenericType(typeof(string));
            var stringList = Activator.CreateInstance(stringListType) as IList;
            stringList.Add(stringData);
            return stringList;
        }
        
        // Convert enumerable to strongly-typed List<T> (but not strings)
        if (data is IEnumerable enumerable && !(data is string))
        {
            return ConvertEnumerableToList(enumerable, itemType);
        }
        
        var singleItemListType = typeof(List<>).MakeGenericType(itemType);
        var singleItemList = Activator.CreateInstance(singleItemListType) as IList;
        
        if (itemType.IsInstanceOfType(data))
        {
            singleItemList.Add(data);
        }
        
        return singleItemList;
    }
    
    private object ConvertEnumerableToList(IEnumerable enumerable, Type itemType)
    {
        var listType = typeof(List<>).MakeGenericType(itemType);
        var list = Activator.CreateInstance(listType) as IList;
        
        foreach (var item in enumerable)
        {
            if (item == null)
                continue;
            
            if (itemType.IsInstanceOfType(item))
                list.Add(item);
                
            else if (itemType.IsValueType || itemType == typeof(string))
            {
                try
                {
                    var convertedItem = Convert.ChangeType(item, itemType);
                    list.Add(convertedItem);
                }
                catch
                {
                    // Skip items that can't be converted
                }
            }
            // For reference types, add if assignment compatible
            else if (itemType.IsAssignableFrom(item.GetType()))
                list.Add(item);
        }
        
        return list;
    }
    
    private object ConvertColumnsToGenericType(object columns, Type itemType)
    {
        // If columns is already a list of GridColumnDefinition<T>, use as-is
        var columnsType = columns.GetType();
        if (columnsType.IsGenericType)
        {
            var genericDef = columnsType.GetGenericTypeDefinition();
            if (genericDef == typeof(List<>) || genericDef == typeof(IEnumerable<>))
            {
                var elementType = columnsType.GetGenericArguments()[0];
                if (elementType.IsGenericType && 
                    elementType.GetGenericTypeDefinition() == typeof(GridColumnDefinition<>))
                {
                    return columns;
                }
            }
        }
        
        // Create empty list if conversion not possible
        var columnDefType = typeof(GridColumnDefinition<>).MakeGenericType(itemType);
        var listType = typeof(List<>).MakeGenericType(columnDefType);
        return Activator.CreateInstance(listType);
    }
    
    private object ConvertConfigurationToGenericType(object config, Type genericConfigType)
    {
        if (config == null) return null;
        
        // If already correct type, return as-is
        if (config.GetType() == genericConfigType)
        {
            return config;
        }
        
        // Create new instance and copy properties
        var newConfig = Activator.CreateInstance(genericConfigType);
        
        // Copy matching properties
        var sourceProps = config.GetType().GetProperties();
        var targetProps = genericConfigType.GetProperties();
        
        foreach (var sourceProp in sourceProps)
        {
            var targetProp = targetProps.FirstOrDefault(p => p.Name == sourceProp.Name && 
                                                             p.PropertyType == sourceProp.PropertyType);
            if (targetProp != null && targetProp.CanWrite)
            {
                var value = sourceProp.GetValue(config);
                targetProp.SetValue(newConfig, value);
            }
        }
        
        return newConfig;
    }
    
    private object ConvertStateToGenericType(object state, Type genericStateType)
    {
        if (state == null) return null;
        
        // If already correct type, return as-is
        if (state.GetType() == genericStateType)
        {
            return state;
        }
        
        // Create new instance and copy properties
        var newState = Activator.CreateInstance(genericStateType);
        
        // Copy matching properties
        var sourceProps = state.GetType().GetProperties();
        var targetProps = genericStateType.GetProperties();
        
        foreach (var sourceProp in sourceProps)
        {
            var targetProp = targetProps.FirstOrDefault(p => p.Name == sourceProp.Name);
            if (targetProp != null && targetProp.CanWrite)
            {
                try
                {
                    var value = sourceProp.GetValue(state);
                    targetProp.SetValue(newState, value);
                }
                catch
                {
                    // Skip properties that can't be copied
                }
            }
        }
        
        return newState;
    }
    
    private void ApplyParametersToConfiguration(object config)
    {
        if (config == null) return;
        
        var configType = config.GetType();
        
        // Map individual parameters to configuration properties
        SetPropertyIfExists(config, configType, "Title", Title);
        SetPropertyIfExists(config, configType, "Subtitle", Subtitle);
        SetPropertyIfExists(config, configType, "Icon", Icon);
        SetPropertyIfExists(config, configType, "ShowHeader", ShowHeader);
        SetPropertyIfExists(config, configType, "EnablePaging", EnablePaging);
        SetPropertyIfExists(config, configType, "PageSize", PageSize);
        SetPropertyIfExists(config, configType, "EnableSorting", EnableSorting);
        SetPropertyIfExists(config, configType, "EnableFiltering", EnableFiltering);
        SetPropertyIfExists(config, configType, "EnableGrouping", EnableGrouping);
        SetPropertyIfExists(config, configType, "EnableColumnReordering", EnableColumnReordering);
        SetPropertyIfExists(config, configType, "EnableExport", EnableExport);
        SetPropertyIfExists(config, configType, "EnableVirtualization", EnableVirtualization);
        SetPropertyIfExists(config, configType, "VirtualizationThreshold", VirtualizationThreshold);
        SetPropertyIfExists(config, configType, "EnableRealTimeUpdates", EnableRealTimeUpdates);
        SetPropertyIfExists(config, configType, "EnableMasterDetail", EnableMasterDetail);
        SetPropertyIfExists(config, configType, "EnableRowSelection", EnableRowSelection);
        SetPropertyIfExists(config, configType, "SelectionMode", SelectionMode);
    }
    
    private void SetPropertyIfExists(object obj, Type type, string propertyName, object value)
    {
        if (value == null) return;
        
        var property = type.GetProperty(propertyName);
        if (property != null && property.CanWrite)
        {
            property.SetValue(obj, value);
        }
    }
    
    private object CreateTypedEventCallback(string eventName)
    {
        // Get the original event callback
        EventCallback<object> originalCallback = eventName switch
        {
            "StateChanged" => StateChanged,
            "OnRowClick" => OnRowClick,
            "OnCellClick" => OnCellClick,
            "OnSelectionChanged" => OnSelectionChanged,
            "OnDataUpdated" => OnDataUpdated,
            _ => default
        };
        
        if (!originalCallback.HasDelegate)
            return null;
        
        // The RGridGeneric will handle type conversion internally
        return originalCallback;
    }
    
    private object CreateTypedRenderFragment(RenderFragment<object> objectFragment)
    {
        // The RGridGeneric component will handle type casting internally
        return objectFragment;
    }
}

// Note: The actual RGridGeneric<TItem> implementation is in RGridGeneric.razor
// It inherits from RComponentBase directly, not from RGridBase
// This separation allows the smart wrapper and generic implementation to coexist