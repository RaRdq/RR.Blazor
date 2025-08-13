using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using System.Linq.Expressions;
using RR.Blazor.Models;

namespace RR.Blazor.Components.Data;

[CascadingTypeParameter(nameof(TItem))]
public class RTableForwarder<TItem> : ComponentBase where TItem : class
{
    [Parameter] public IEnumerable<TItem> Items { get; set; }
    [Parameter] public RenderFragment ChildContent { get; set; }
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
            
            if (AdditionalAttributes != null)
            {
                foreach (var attr in AdditionalAttributes)
                {
                    contextBuilder.AddAttribute(2, attr.Key, attr.Value);
                }
            }
            
            contextBuilder.AddAttribute(3, "ChildContent", ChildContent);
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