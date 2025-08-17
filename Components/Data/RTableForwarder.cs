using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using System.Linq.Expressions;
using RR.Blazor.Models;
using RR.Blazor.Components.Base;

namespace RR.Blazor.Components.Data;

[CascadingTypeParameter(nameof(TItem))]
public class RTableForwarder<TItem> : ComponentBase where TItem : class
{
    [Parameter] public IEnumerable<TItem> Items { get; set; }
    [Parameter] public RenderFragment ChildContent { get; set; }
    [Parameter] public int PageSize { get; set; } = 50;
    [Parameter(CaptureUnmatchedValues = true)] public Dictionary<string, object> AdditionalAttributes { get; set; } = new();
    
    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        var tableContext = new TableContext(typeof(TItem), $"table-{GetHashCode()}", true);
        
        builder.OpenComponent<CascadingValue<TableContext>>(0);
        builder.AddAttribute(1, "Value", tableContext);
        builder.AddAttribute(2, "ChildContent", (RenderFragment)(contextBuilder =>
        {
            contextBuilder.OpenComponent<RTableGeneric<TItem>>(0);
            contextBuilder.AddAttribute(1, "Items", Items);
            contextBuilder.AddAttribute(2, "PageSize", PageSize);
            
            if (AdditionalAttributes != null)
            {
                // Forward ALL Blazor component parameters for RTableGeneric
                // RTableForwarder forwards to a Blazor component, not HTML element
                var processedAttributes = new Dictionary<string, object>();
                foreach (var attr in AdditionalAttributes)
                {
                    var value = attr.Value;
                    
                    // Fix PageSize casting error using RAttributeForwarder logic
                    if (attr.Key == "PageSize" && value is string strPageSize)
                    {
                        if (int.TryParse(strPageSize, out var intPageSize))
                        {
                            value = intPageSize;
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
/// Extension to simplify table creation with compile-time type resolution
/// </summary>
public static class RTableExtensions
{
    /// <summary>
    /// Creates a typed table forwarder at compile time
    /// </summary>
    public static RenderFragment CreateTable<TItem>(
        IEnumerable<TItem> items,
        RenderFragment columns,
        Dictionary<string, object> attributes = null) where TItem : class
    {
        return builder =>
        {
            builder.OpenComponent<RTableForwarder<TItem>>(0);
            builder.AddAttribute(1, "Items", items);
            builder.AddAttribute(2, "ChildContent", columns);
            if (attributes != null)
            {
                builder.AddMultipleAttributes(3, attributes);
            }
            builder.CloseComponent();
        };
    }
}