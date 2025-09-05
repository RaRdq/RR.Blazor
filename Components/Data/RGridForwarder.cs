using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using RR.Blazor.Models;
using RR.Blazor.Components.Base;

namespace RR.Blazor.Components.Data;

[CascadingTypeParameter(nameof(TItem))]
public class RGridForwarder<TItem> : RComponentBase where TItem : class
{
    [Parameter] public IEnumerable<TItem> Items { get; set; }
    [Parameter] public int PageSize { get; set; } = 24;
    
    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        var gridContext = new GridContext(typeof(TItem), $"grid-{GetHashCode()}", true);
        
        builder.OpenComponent<CascadingValue<GridContext>>(0);
        builder.AddAttribute(1, "Value", gridContext);
        builder.AddAttribute(2, "ChildContent", (RenderFragment)(contextBuilder =>
        {
            contextBuilder.OpenComponent<RGridGeneric<TItem>>(0);
            contextBuilder.AddAttribute(1, "Items", Items);
            contextBuilder.AddAttribute(2, "PageSize", PageSize);
            
            // Forward additional attributes with proper type handling
            if (AdditionalAttributes != null && AdditionalAttributes.Count > 0)
            {
                var processedAttributes = new Dictionary<string, object>();
                foreach (var attr in AdditionalAttributes)
                {
                    var value = attr.Value;
                    
                    // Handle common type conversions for grid-specific attributes
                    if (attr.Key == "PageSize" && value is string strPageSize)
                    {
                        if (int.TryParse(strPageSize, out var intPageSize))
                        {
                            value = intPageSize;
                        }
                    }
                    else if (attr.Key == "CurrentPage" && value is string strCurrentPage)
                    {
                        if (int.TryParse(strCurrentPage, out var intCurrentPage))
                        {
                            value = intCurrentPage;
                        }
                    }
                    else if (attr.Key == "EnablePagination" && value is string strEnablePagination)
                    {
                        if (bool.TryParse(strEnablePagination, out var boolEnablePagination))
                        {
                            value = boolEnablePagination;
                        }
                    }
                    
                    processedAttributes[attr.Key] = value;
                }
                
                if (processedAttributes.Any())
                {
                    contextBuilder.AddMultipleAttributes(3, processedAttributes);
                }
            }
            
            var childContentSeq = AdditionalAttributes?.Count ?? 0;
            contextBuilder.AddAttribute(4 + childContentSeq, "ChildContent", ChildContent);
            contextBuilder.CloseComponent();
        }));
        builder.CloseComponent();
    }
}

/// <summary>
/// Extension to simplify grid creation with compile-time type resolution
/// </summary>
public static class RGridExtensions
{
    /// <summary>
    /// Creates a typed grid forwarder at compile time
    /// </summary>
    public static RenderFragment CreateGrid<TItem>(
        IEnumerable<TItem> items,
        RenderFragment itemTemplate = null,
        Dictionary<string, object> attributes = null) where TItem : class
    {
        return builder =>
        {
            builder.OpenComponent<RGridForwarder<TItem>>(0);
            builder.AddAttribute(1, "Items", items);
            if (itemTemplate != null)
            {
                builder.AddAttribute(2, "ItemTemplate", itemTemplate);
            }
            if (attributes != null)
            {
                builder.AddMultipleAttributes(3, attributes);
            }
            builder.CloseComponent();
        };
    }
}