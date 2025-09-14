using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using RR.Blazor.Components.Base;
using System.Collections;
using System.Linq;
using System.Reflection;

namespace RR.Blazor.Components.Data;

public class RTable : RTableBase
{
    [Parameter] public object Items { get; set; }
    
    private bool _isDisposed = false;
    
    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        if (_isDisposed || Items == null)
        {
            return;
        }
        
        switch (Items)
        {
            case IEnumerable enumerable:
                var enumerableType = enumerable.GetType();
                if (enumerableType.IsGenericType)
                {
                    var itemType = enumerableType.GetGenericArguments()[0];
                    BuildGenericTable(builder, enumerable, itemType);
                }
                else if (enumerableType.IsArray)
                {
                    var elementType = enumerableType.GetElementType();
                    if (elementType != null)
                    {
                        BuildGenericTable(builder, enumerable, elementType);
                    }
                }
                else
                {
                    throw new InvalidOperationException($"Cannot determine item type from collection of type {enumerableType.Name}");
                }
                break;
                
            default:
                throw new InvalidOperationException($"RTable Items parameter must be an IEnumerable, got {Items.GetType().Name}");
        }
    }
    
    public void Dispose()
    {
        _isDisposed = true;
    }
    
    private void BuildGenericTable(RenderTreeBuilder builder, IEnumerable items, Type itemType)
    {
        var tableType = typeof(RTableGeneric<>).MakeGenericType(itemType);
        
        builder.OpenComponent(0, tableType);
        builder.AddAttribute(1, "Items", items);
        
        var seq = 2;
        builder.AddAttribute(++seq, "Title", Title);
        builder.AddAttribute(++seq, "Subtitle", Subtitle);
        builder.AddAttribute(++seq, "Loading", Loading);
        builder.AddAttribute(++seq, "LoadingText", LoadingText);
        builder.AddAttribute(++seq, "EmptyText", EmptyText);
        builder.AddAttribute(++seq, "ShowTitle", ShowTitle);
        builder.AddAttribute(++seq, "ShowFooter", ShowFooter);
        builder.AddAttribute(++seq, "Striped", Striped);
        builder.AddAttribute(++seq, "Hover", Hover);
        builder.AddAttribute(++seq, "Bordered", Bordered);
        builder.AddAttribute(++seq, "Compact", Compact);
        builder.AddAttribute(++seq, "FixedHeader", FixedHeader);
        builder.AddAttribute(++seq, "Virtualize", Virtualize);
        builder.AddAttribute(++seq, "StickyHeader", StickyHeader);
        builder.AddAttribute(++seq, "Height", Height);
        builder.AddAttribute(++seq, "MaxHeight", MaxHeight);
        builder.AddAttribute(++seq, "RowHeight", RowHeight);
        builder.AddAttribute(++seq, "HeaderHeight", HeaderHeight);
        builder.AddAttribute(++seq, "FooterHeight", FooterHeight);
        builder.AddAttribute(++seq, "CssClass", CssClass);
        builder.AddAttribute(++seq, "AutoGenerateColumns", AutoGenerateColumns);
        builder.AddAttribute(++seq, "MultiSelection", MultiSelection);
        builder.AddAttribute(++seq, "SearchEnabled", SearchEnabled);
        builder.AddAttribute(++seq, "FilterEnabled", FilterEnabled);
        builder.AddAttribute(++seq, "ExportEnabled", ExportEnabled);
        builder.AddAttribute(++seq, "RefreshEnabled", RefreshEnabled);
        builder.AddAttribute(++seq, "BulkOperationsEnabled", BulkOperationsEnabled);
        builder.AddAttribute(++seq, "ShowPagination", ShowPagination);
        builder.AddAttribute(++seq, "ShowSearch", ShowSearch);
        builder.AddAttribute(++seq, "ShowToolbar", ShowToolbar);
        builder.AddAttribute(++seq, "ShowChartButton", ShowChartButton);
        builder.AddAttribute(++seq, "ShowExportButton", ShowExportButton);
        builder.AddAttribute(++seq, "ShowColumnManager", ShowColumnManager);
        builder.AddAttribute(++seq, "EnableColumnReordering", EnableColumnReordering);
        builder.AddAttribute(++seq, "EnableStickyColumns", EnableStickyColumns);
        builder.AddAttribute(++seq, "EnableHorizontalScroll", EnableHorizontalScroll);
        builder.AddAttribute(++seq, "EnableSorting", EnableSorting);
        builder.AddAttribute(++seq, "EnableFiltering", EnableFiltering);
        builder.AddAttribute(++seq, "EnableSelection", EnableSelection);
        builder.AddAttribute(++seq, "EnableExport", EnableExport);
        builder.AddAttribute(++seq, "EnablePaging", EnablePaging);
        builder.AddAttribute(++seq, "ShowFilters", ShowFilters);
        builder.AddAttribute(++seq, "Selectable", Selectable);
        builder.AddAttribute(++seq, "MultiSelect", MultiSelect);
        builder.AddAttribute(++seq, "RowClickable", RowClickable);
        builder.AddAttribute(++seq, "PageSize", PageSize);
        builder.AddAttribute(++seq, "CurrentPage", CurrentPage);
        builder.AddAttribute(++seq, "ChartButtonText", ChartButtonText);
        builder.AddAttribute(++seq, "DefaultChartType", DefaultChartType);
        
        builder.AddAttribute(++seq, "ColumnsContent", ColumnsContent);
        builder.AddAttribute(++seq, "BulkOperations", BulkOperations);
        builder.AddAttribute(++seq, "TableActions", TableActions);
        builder.AddAttribute(++seq, "EmptyContent", EmptyContent);
        builder.AddAttribute(++seq, "LoadingContent", LoadingContent);
        builder.AddAttribute(++seq, "HeaderTemplate", HeaderTemplate);
        builder.AddAttribute(++seq, "FooterTemplate", FooterTemplate);
        
        builder.AddAttribute(++seq, "PageSizeChanged", PageSizeChanged);
        builder.AddAttribute(++seq, "CurrentPageChanged", CurrentPageChanged);
        builder.AddAttribute(++seq, "SortByChanged", SortByChanged);
        builder.AddAttribute(++seq, "SortDescendingChanged", SortDescendingChanged);
        
        // Handle EventCallbacks from AdditionalAttributes
        if (AdditionalAttributes != null)
        {
            foreach (var attr in AdditionalAttributes)
            {
                // Skip attributes that are already handled as parameters
                if (attr.Key == "Items" || attr.Key == "TItem") continue;
                
                if (attr.Key == "OnRowClicked")
                {
                    // Convert delegate to EventCallback<T> at runtime
                    var eventCallback = CreateEventCallback(attr.Value, itemType);
                    builder.AddAttribute(++seq, "OnRowClicked", eventCallback);
                }
                else if (IsEventCallbackParameter(attr.Key))
                {
                    // Convert other event callbacks
                    var eventCallback = CreateEventCallback(attr.Value, itemType);
                    builder.AddAttribute(++seq, attr.Key, eventCallback);
                }
                else
                {
                    // Forward all other attributes with standard conversion
                    var convertedValue = ConvertParameterValue(attr.Key, attr.Value, itemType);
                    builder.AddAttribute(++seq, attr.Key, convertedValue);
                }
            }
        }
        builder.CloseComponent();
    }
    
    private object ConvertParameterValue(string parameterName, object value, Type itemType)
    {
        if (value == null)
            return null;
            
        if (value is string stringValue && IsBooleanParameter(parameterName))
        {
            return bool.TryParse(stringValue, out var boolResult) ? boolResult : false;
        }
        
        if (value is string intString && IsIntegerParameter(parameterName))
        {
            return int.TryParse(intString, out var intResult) ? intResult : 0;
        }
        
        return value;
    }
    
    private object CreateEventCallback(object value, Type itemType)
    {
        if (value == null)
            return null;

        // If it's already an EventCallback, return it as-is
        var valueType = value.GetType();
        if (valueType.IsGenericType && valueType.GetGenericTypeDefinition() == typeof(EventCallback<>))
        {
            return value;
        }

        var factory = new EventCallbackFactory();

        // Check if the value is an Action<T>
        if (valueType.IsGenericType && valueType.GetGenericTypeDefinition() == typeof(Action<>))
        {
            var actionGenericArg = valueType.GetGenericArguments()[0];
            if (actionGenericArg == itemType)
            {
                // Find the generic Create<T> method
                var createMethod = typeof(EventCallbackFactory)
                    .GetMethods()
                    .Where(m => m.Name == "Create" && m.IsGenericMethodDefinition)
                    .FirstOrDefault(m =>
                    {
                        var parameters = m.GetParameters();
                        if (parameters.Length != 2) return false;

                        var genericArgs = m.GetGenericArguments();
                        if (genericArgs.Length != 1) return false;

                        // Check if second parameter is Action<T>
                        var secondParam = parameters[1].ParameterType;
                        return secondParam.IsGenericType &&
                               secondParam.GetGenericTypeDefinition() == typeof(Action<>);
                    });

                if (createMethod != null)
                {
                    var genericMethod = createMethod.MakeGenericMethod(itemType);
                    return genericMethod.Invoke(factory, new[] { this, value });
                }
            }
        }

        // Check if the value is a Func<T, Task>
        if (valueType.IsGenericType && valueType.GetGenericTypeDefinition() == typeof(Func<,>))
        {
            var funcGenericArgs = valueType.GetGenericArguments();
            if (funcGenericArgs.Length == 2 && funcGenericArgs[0] == itemType && funcGenericArgs[1] == typeof(Task))
            {
                // Find the generic Create<T> method for Func<T, Task>
                var createMethod = typeof(EventCallbackFactory)
                    .GetMethods()
                    .Where(m => m.Name == "Create" && m.IsGenericMethodDefinition)
                    .FirstOrDefault(m =>
                    {
                        var parameters = m.GetParameters();
                        if (parameters.Length != 2) return false;

                        var genericArgs = m.GetGenericArguments();
                        if (genericArgs.Length != 1) return false;

                        // Check if second parameter is Func<T, Task>
                        var secondParam = parameters[1].ParameterType;
                        return secondParam.IsGenericType &&
                               secondParam.GetGenericTypeDefinition() == typeof(Func<,>) &&
                               secondParam.GetGenericArguments().Length == 2 &&
                               secondParam.GetGenericArguments()[1] == typeof(Task);
                    });

                if (createMethod != null)
                {
                    var genericMethod = createMethod.MakeGenericMethod(itemType);
                    return genericMethod.Invoke(factory, new[] { this, value });
                }
            }
        }

        // non-generic fallback for simple delegates
        var nonGenericCreate = typeof(EventCallbackFactory)
            .GetMethod("Create", new[] { typeof(object), typeof(MulticastDelegate) });

        if (nonGenericCreate != null && value is MulticastDelegate)
            return nonGenericCreate.Invoke(factory, new[] { this, value });

        throw new InvalidOperationException($"Cannot create EventCallback<{itemType.Name}> from {valueType.Name}. Expected Action<{itemType.Name}> or Func<{itemType.Name}, Task>.");
    }
    
    private static bool IsEventCallbackParameter(string parameterName)
    {
        return parameterName switch
        {
            "OnRowClicked" or "OnRowSelected" or "OnEdit" or "OnDelete" or "OnView" or 
            "SelectedItemChanged" or "OnSelectionChanged" or "SelectedItemsChanged" or
            "OnEnhancedRowClick" or "OnColumnResized" or "PageSizeChanged" or 
            "CurrentPageChanged" or "SortByChanged" or "SortDescendingChanged" => true,
            _ => false
        };
    }
    
    private static bool IsBooleanParameter(string parameterName)
    {
        return parameterName switch
        {
            "AutoGenerateColumns" or "ShowPagination" or "ShowSearch" or "ShowTitle" or 
            "ShowFooter" or "Striped" or "Hover" or "Bordered" or "Compact" or 
            "FixedHeader" or "Virtualize" or "StickyHeader" or "MultiSelection" or
            "SearchEnabled" or "FilterEnabled" or "ExportEnabled" or "RefreshEnabled" or
            "BulkOperationsEnabled" or "ShowToolbar" or "ShowChartButton" or 
            "ShowExportButton" or "ShowColumnManager" or "EnableColumnReordering" or
            "EnableStickyColumns" or "EnableHorizontalScroll" or "EnableSorting" or
            "EnableFiltering" or "EnableSelection" or "EnableExport" or "EnablePaging" or
            "ShowFilters" or "Selectable" or "MultiSelect" or "RowClickable" or "Loading" => true,
            _ => false
        };
    }
    
    private static bool IsIntegerParameter(string parameterName)
    {
        return parameterName switch
        {
            "PageSize" or "CurrentPage" => true,
            _ => false
        };
    }
}