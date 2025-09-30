using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using RR.Blazor.Components.Base;
using System.Collections;
using System.Collections.Concurrent;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace RR.Blazor.Components.Data;

public class RTable : RTableBase
{
    [Parameter] public object Items { get; set; }

    private bool _isDisposed = false;

    // Static cache for compiled EventCallback wrappers - compiled once at startup/first use
    private static readonly ConcurrentDictionary<string, object> _compiledWrapperCache = new();

    // Cache for reflection method lookups to avoid repeated reflection
    private static readonly ConcurrentDictionary<Type, System.Reflection.MethodInfo> _invokeAsyncMethodCache = new();

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

                // Special handling for OnRowClicked EventCallback
                if (attr.Key == "OnRowClicked" && attr.Value != null)
                {
                    var valueType = attr.Value.GetType();

                    // Check if it's EventCallback<T> where T matches itemType
                    if (valueType.IsGenericType &&
                        valueType.GetGenericTypeDefinition() == typeof(EventCallback<>) &&
                        valueType.GetGenericArguments()[0] == itemType)
                    {
                        // The types match exactly, pass through as-is
                        builder.AddAttribute(++seq, attr.Key, attr.Value);
                        continue;
                    }

                    // Create a wrapper EventCallback that invokes the original
                    if (valueType.IsGenericType &&
                        valueType.GetGenericTypeDefinition() == typeof(EventCallback<>))
                    {
                        // Get the InvokeAsync method from the original EventCallback (cached)
                        var invokeAsyncMethod = GetCachedInvokeAsyncMethod(valueType);
                        if (invokeAsyncMethod != null)
                        {
                            var originalCallback = attr.Value;

                            // Create a factory for the correctly typed EventCallback
                            var factory = new EventCallbackFactory();

                            // Create wrapper action that calls the original callback (cached)
                            var wrapperAction = GetCachedDynamicWrapper(itemType, valueType, originalCallback, invokeAsyncMethod);

                            // Use the factory to create EventCallback<TItem>
                            var createMethod = typeof(EventCallbackFactory)
                                .GetMethods()
                                .FirstOrDefault(m => m.Name == "Create" &&
                                                    m.IsGenericMethodDefinition &&
                                                    m.GetGenericArguments().Length == 1 &&
                                                    m.GetParameters().Length == 2 &&
                                                    m.GetParameters()[1].ParameterType.Name.StartsWith("Func"));

                            if (createMethod != null)
                            {
                                var genericMethod = createMethod.MakeGenericMethod(itemType);
                                var eventCallback = genericMethod.Invoke(factory, new[] { this, wrapperAction });
                                builder.AddAttribute(++seq, attr.Key, eventCallback);
                                continue;
                            }
                        }
                    }
                }

                // Default: pass through as-is
                builder.AddAttribute(++seq, attr.Key, attr.Value);
            }
        }
        builder.CloseComponent();
    }

    // Cached method lookup to avoid repeated reflection
    private static System.Reflection.MethodInfo GetCachedInvokeAsyncMethod(Type eventCallbackType)
    {
        return _invokeAsyncMethodCache.GetOrAdd(eventCallbackType, type => type.GetMethod("InvokeAsync"));
    }

    // Cached dynamic wrapper creation - compiles expression only once per type combination
    private static object GetCachedDynamicWrapper(Type itemType, Type callbackType, object originalCallback, System.Reflection.MethodInfo invokeAsyncMethod)
    {
        var cacheKey = $"{itemType.FullName}_{callbackType.FullName}";

        // Get or create the compiled wrapper factory for this type combination
        var wrapperFactory = (Func<object, object>)_compiledWrapperCache.GetOrAdd(cacheKey, _ =>
            CreateCompiledWrapperFactory(itemType, invokeAsyncMethod));

        // Use the factory to create a wrapper for this specific callback instance
        return wrapperFactory(originalCallback);

        // Inner method to create the wrapper factory
        static Func<object, object> CreateCompiledWrapperFactory(Type itemType, System.Reflection.MethodInfo invokeAsyncMethod)
        {
            var funcType = typeof(Func<,>).MakeGenericType(itemType, typeof(Task));
            var callbackParam = Expression.Parameter(typeof(object), "callback");
            var itemParam = Expression.Parameter(itemType, "item");

            var callExpr = Expression.Call(
                callbackParam,
                invokeAsyncMethod,
                itemParam);

            var innerLambda = Expression.Lambda(funcType, callExpr, itemParam);
            var factoryLambda = Expression.Lambda<Func<object, object>>(innerLambda, callbackParam);

            return factoryLambda.Compile();
        }
    }
}