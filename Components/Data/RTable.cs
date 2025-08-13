using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using RR.Blazor.Models;
using RR.Blazor.Enums;
using RR.Blazor.Components.Base;

namespace RR.Blazor.Components.Data;

/// <summary>
/// Smart table wrapper that enables ultra-simple syntax: <RTable Items="@data" />
/// Uses RTableForwarder internally for compile-time type optimization
/// </summary>
public abstract class RTableBase : ComponentBase
{
    [Parameter] public string Title { get; set; }
    [Parameter] public string Subtitle { get; set; }
    [Parameter] public bool Loading { get; set; }
    [Parameter] public string LoadingText { get; set; } = "Loading...";
    [Parameter] public string EmptyText { get; set; } = "No data available";
    [Parameter] public bool ShowHeader { get; set; } = true;
    [Parameter] public bool ShowFooter { get; set; }
    [Parameter] public bool Striped { get; set; } = true;
    [Parameter] public bool Hover { get; set; } = true;
    [Parameter] public bool Bordered { get; set; }
    [Parameter] public bool Compact { get; set; }
    [Parameter] public bool FixedHeader { get; set; }
    [Parameter] public bool Virtualize { get; set; }
    [Parameter] public int PageSize { get; set; } = 50;
    [Parameter] public string Height { get; set; }
    [Parameter] public string MaxHeight { get; set; }
    [Parameter] public bool StickyHeader { get; set; }
    [Parameter] public bool AutoGenerateColumns { get; set; } = true;
    [Parameter] public bool MultiSelection { get; set; }
    [Parameter] public bool SearchEnabled { get; set; }
    [Parameter] public bool FilterEnabled { get; set; }
    [Parameter] public bool ExportEnabled { get; set; }
    [Parameter] public bool RefreshEnabled { get; set; }
    [Parameter] public bool BulkOperationsEnabled { get; set; }
    [Parameter] public string RowHeight { get; set; }
    [Parameter] public string HeaderHeight { get; set; }
    [Parameter] public string FooterHeight { get; set; }
    [Parameter] public string Class { get; set; }
    [Parameter] public string Style { get; set; }
    [Parameter] public string CssClass { get; set; }
    [Parameter] public RenderFragment ChildContent { get; set; }
    [Parameter] public RenderFragment ColumnsContent { get; set; }
    [Parameter] public RenderFragment BulkOperations { get; set; }
    [Parameter] public RenderFragment TableActions { get; set; }
    [Parameter] public RenderFragment EmptyContent { get; set; }
    [Parameter] public RenderFragment LoadingContent { get; set; }
    [Parameter] public RenderFragment HeaderTemplate { get; set; }
    [Parameter] public RenderFragment FooterTemplate { get; set; }
    
    // Additional parameters for RTableGeneric compatibility
    [Parameter] public bool ShowPagination { get; set; } = true;
    [Parameter] public bool ShowSearch { get; set; } = true;
    [Parameter] public bool ShowToolbar { get; set; } = true;
    [Parameter] public bool ShowChartButton { get; set; }
    [Parameter] public string ChartButtonText { get; set; } = "Show as Chart";
    [Parameter] public RR.Blazor.Enums.ChartType? DefaultChartType { get; set; }
    [Parameter] public RR.Blazor.Enums.ComponentDensity Density { get; set; } = RR.Blazor.Enums.ComponentDensity.Normal;
    
    [Parameter(CaptureUnmatchedValues = true)] public Dictionary<string, object> AdditionalAttributes { get; set; }
}

/// <summary>
/// Smart wrapper that detects type from Items and uses RTableForwarder internally
/// Usage: <RTable Items="@employees" /> - type inferred automatically!
/// Developer writes: <RTable Items="@data"><RColumn For="@(e => e.Name)" /></RTable>
/// </summary>
public class RTable : RTableBase
{
    [Parameter] public object Items { get; set; }
    
    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        if (Items == null)
        {
            return;
        }
        
        var itemsType = Items.GetType();
        Type itemType = null;
        
        // Detect item type from collection
        if (itemsType.IsGenericType)
        {
            var genericArgs = itemsType.GetGenericArguments();
            if (genericArgs.Length > 0)
            {
                itemType = genericArgs[0];
            }
        }
        else if (itemsType.IsArray)
        {
            itemType = itemsType.GetElementType();
        }
        
        if (itemType == null || !itemType.IsClass)
        {
            throw new InvalidOperationException($"Cannot determine item type from Items collection of type {itemsType.Name}");
        }
        
        // Use RTableForwarder internally for compile-time type forwarding
        var forwarderType = typeof(RTableForwarder<>).MakeGenericType(itemType);
        
        builder.OpenComponent(0, forwarderType);
        builder.AddAttribute(1, "Items", Items);
        builder.AddAttribute(2, "ChildContent", ColumnsContent ?? ChildContent);
        
        // Forward all base class parameters
        builder.AddAttribute(3, "Title", Title);
        builder.AddAttribute(4, "Subtitle", Subtitle);
        builder.AddAttribute(5, "Loading", Loading);
        builder.AddAttribute(6, "LoadingText", LoadingText);
        builder.AddAttribute(7, "EmptyText", EmptyText);
        builder.AddAttribute(8, "ShowHeader", ShowHeader);
        builder.AddAttribute(9, "ShowFooter", ShowFooter);
        builder.AddAttribute(10, "Striped", Striped);
        builder.AddAttribute(11, "Hover", Hover);
        builder.AddAttribute(12, "Bordered", Bordered);
        builder.AddAttribute(13, "Compact", Compact);
        builder.AddAttribute(14, "FixedHeader", FixedHeader);
        builder.AddAttribute(15, "Virtualize", Virtualize);
        builder.AddAttribute(16, "PageSize", PageSize);
        builder.AddAttribute(17, "Height", Height);
        builder.AddAttribute(18, "MaxHeight", MaxHeight);
        builder.AddAttribute(19, "StickyHeader", StickyHeader);
        builder.AddAttribute(20, "AutoGenerateColumns", AutoGenerateColumns);
        builder.AddAttribute(21, "MultiSelection", MultiSelection);
        builder.AddAttribute(22, "SearchEnabled", SearchEnabled);
        builder.AddAttribute(23, "FilterEnabled", FilterEnabled);
        builder.AddAttribute(24, "ExportEnabled", ExportEnabled);
        builder.AddAttribute(25, "RefreshEnabled", RefreshEnabled);
        builder.AddAttribute(26, "BulkOperationsEnabled", BulkOperationsEnabled);
        builder.AddAttribute(27, "RowHeight", RowHeight);
        builder.AddAttribute(28, "HeaderHeight", HeaderHeight);
        builder.AddAttribute(29, "FooterHeight", FooterHeight);
        builder.AddAttribute(30, "Class", Class);
        builder.AddAttribute(31, "Style", Style);
        builder.AddAttribute(32, "CssClass", CssClass);
        builder.AddAttribute(33, "ShowPagination", ShowPagination);
        builder.AddAttribute(34, "ShowSearch", ShowSearch);
        builder.AddAttribute(35, "ShowToolbar", ShowToolbar);
        builder.AddAttribute(36, "ShowChartButton", ShowChartButton);
        builder.AddAttribute(37, "ChartButtonText", ChartButtonText);
        builder.AddAttribute(38, "DefaultChartType", DefaultChartType);
        builder.AddAttribute(39, "Density", Density);
        
        // Forward all additional attributes
        if (AdditionalAttributes != null)
        {
            builder.AddMultipleAttributes(40, AdditionalAttributes);
        }
        
        builder.CloseComponent();
    }
}

