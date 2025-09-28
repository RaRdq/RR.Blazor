using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using RR.Blazor.Components.Base;
using System.Collections;

namespace RR.Blazor.Components.Data;

/// <summary>
/// Grid display modes for intelligent layout selection
/// </summary>
public enum GridMode
{
    Auto,     // Smart detection based on data properties
    Table,    // Use RTable for tabular data (edge case)
    Cards,    // Card-based responsive layouts
    List,     // Single/multi-column lists
    Tiles,    // Uniform tile layouts (TODO: implement RTile)
    Gallery,  // Image-focused grids (TODO: implement RGallery)
    Masonry   // Pinterest-style variable heights (TODO: implement RMasonry)
}

/// <summary>
/// Smart grid component with type inference - follows RTable pattern exactly.
/// Architecture: RGrid → RGridBase → RGridGeneric&lt;T&gt;
/// Uses RAttributeForwarder for efficient parameter passing.
///
/// </summary>
[Obsolete("RGrid is deprecated. Use semantic HTML + responsive-grid CSS classes instead. See RGRID_MIGRATION.md", false)]
public class RGrid : RGridBase
{
    /// <summary>
    /// The data items to display in the grid
    /// </summary>
    [Parameter] public object Items { get; set; }

    /// <summary>
    /// Custom template for rendering individual items
    /// </summary>
    [Parameter] public RenderFragment<object> ItemTemplate { get; set; }

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
                    BuildGenericGrid(builder, enumerable, itemType);
                }
                else if (enumerableType.IsArray)
                {
                    var elementType = enumerableType.GetElementType();
                    if (elementType != null)
                    {
                        BuildGenericGrid(builder, enumerable, elementType);
                    }
                }
                else
                {
                    throw new InvalidOperationException($"Cannot determine item type from collection of type {enumerableType.Name}");
                }
                break;

            default:
                throw new InvalidOperationException($"RGrid Items parameter must be an IEnumerable, got {Items.GetType().Name}");
        }
    }

    public void Dispose()
    {
        _isDisposed = true;
    }

    private void BuildGenericGrid(RenderTreeBuilder builder, IEnumerable items, Type itemType)
    {
        var gridType = typeof(RGridGeneric<>).MakeGenericType(itemType);

        builder.OpenComponent(0, gridType);
        builder.AddAttribute(1, "Items", items);

        // Handle ItemTemplate with type conversion
        if (ItemTemplate != null)
        {
            builder.AddAttribute(2, "ItemTemplateObject", ItemTemplate);
        }

        // Use RAttributeForwarder to forward all parameters from RGridBase
        var seq = 3;
        builder.ForwardAllParameters(ref seq, this);

        // Handle EventCallbacks from AdditionalAttributes
        if (AdditionalAttributes != null)
        {
            foreach (var attr in AdditionalAttributes)
            {
                // Skip attributes that are already handled as parameters
                if (attr.Key == "Items" || attr.Key == "TItem" || attr.Key == "ItemTemplate") continue;

                // Forward all attributes with standard conversion
                var convertedValue = ConvertParameterValue(attr.Key, attr.Value, itemType);
                builder.AddAttribute(++seq, attr.Key, convertedValue);
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

    private static bool IsBooleanParameter(string parameterName)
    {
        return parameterName switch
        {
            "ShowTitle" or "ShowToolbar" or "ShowSearch" or "ShowFilters" or "ShowExportButton" or
            "EnableFilter" or "ShowQuickSearch" or "EnablePagination" or "EnableVirtualization" or
            "Loading" or "Selectable" or "MultiSelect" => true,
            _ => false
        };
    }

    private static bool IsIntegerParameter(string parameterName)
    {
        return parameterName switch
        {
            "PageSize" or "CurrentPage" or "Columns" or "ColumnsXs" or "ColumnsSm" or
            "ColumnsMd" or "ColumnsLg" or "ColumnsXl" or "VirtualizationThreshold" => true,
            _ => false
        };
    }
}
