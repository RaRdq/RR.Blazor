using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using RR.Blazor.Models;
using RR.Blazor.Enums;
using RR.Blazor.Components.Base;
using System.Linq;

namespace RR.Blazor.Components.Data;

/// <summary>
/// Smart table wrapper that enables ultra-simple syntax: <RTable Items="@data" />
/// Uses RTableForwarder internally for compile-time type optimization
/// </summary>
public abstract class RTableBase : RComponentBase
{
    public override async Task SetParametersAsync(ParameterView parameters)
    {
        // Fix parameter casting errors: intercept and convert parameter values to correct types
        var parametersDict = new Dictionary<string, object>();
        
        foreach (var parameter in parameters)
        {
            var value = parameter.Value;
            
            // Handle specific parameter type conversions
            value = parameter.Name switch
            {
                nameof(PageSize) => ConvertToInt(value, 50),
                nameof(Virtualize) => ConvertToBool(value, false),
                nameof(Striped) => ConvertToBool(value, true),
                nameof(Hover) => ConvertToBool(value, true),
                nameof(Bordered) => ConvertToBool(value, false),
                nameof(Compact) => ConvertToBool(value, false),
                nameof(FixedHeader) => ConvertToBool(value, false),
                nameof(StickyHeader) => ConvertToBool(value, false),
                nameof(AutoGenerateColumns) => ConvertToBool(value, true),
                nameof(MultiSelection) => ConvertToBool(value, false),
                nameof(SearchEnabled) => ConvertToBool(value, false),
                nameof(FilterEnabled) => ConvertToBool(value, false),
                nameof(ExportEnabled) => ConvertToBool(value, false),
                nameof(RefreshEnabled) => ConvertToBool(value, false),
                nameof(BulkOperationsEnabled) => ConvertToBool(value, false),
                nameof(ShowTitle) => ConvertToBool(value, true),
                nameof(ShowFooter) => ConvertToBool(value, false),
                nameof(ShowPagination) => ConvertToBool(value, true),
                nameof(ShowSearch) => ConvertToBool(value, true),
                nameof(ShowToolbar) => ConvertToBool(value, true),
                nameof(ShowChartButton) => ConvertToBool(value, false),
                nameof(ShowExportButton) => ConvertToBool(value, true),
                nameof(ShowColumnManager) => ConvertToBool(value, false),
                nameof(EnableColumnReordering) => ConvertToBool(value, false),
                nameof(EnableStickyColumns) => ConvertToBool(value, false),
                nameof(EnableHorizontalScroll) => ConvertToBool(value, false),
                nameof(EnableSorting) => ConvertToBool(value, false),
                nameof(EnableFiltering) => ConvertToBool(value, false),
                nameof(EnableSelection) => ConvertToBool(value, false),
                nameof(EnableExport) => ConvertToBool(value, false),
                nameof(EnablePaging) => ConvertToBool(value, true),
                nameof(Loading) => ConvertToBool(value, false),
                "ShowFilters" => ConvertToBool(value, false),
                "EnableColumnFilters" => ConvertToBool(value, false),
                _ => value // Keep original value for all other parameters
            };
            
            parametersDict[parameter.Name] = value;
        }
        
        // Create new ParameterView with corrected parameters
        var correctedParameters = ParameterView.FromDictionary(parametersDict);
        await base.SetParametersAsync(correctedParameters);
    }
    
    private static object ConvertToInt(object value, int defaultValue)
    {
        if (value == null) return defaultValue;
        if (value is int) return value;
        if (value is string strValue && int.TryParse(strValue, out var intValue)) return intValue;
        
        try
        {
            return Convert.ToInt32(value);
        }
        catch
        {
            return defaultValue;
        }
    }
    
    private static object ConvertToBool(object value, bool defaultValue)
    {
        if (value == null) return defaultValue;
        if (value is bool) return value;
        if (value is string strValue && bool.TryParse(strValue, out var boolValue)) return boolValue;
        
        try
        {
            return Convert.ToBoolean(value);
        }
        catch
        {
            return defaultValue;
        }
    }
    [Parameter] public string Title { get; set; }
    [Parameter] public string Subtitle { get; set; }
    [Parameter] public bool Loading { get; set; }
    [Parameter] public string LoadingText { get; set; } = "Loading...";
    [Parameter] public string EmptyText { get; set; } = "No data available";
    [Parameter] public bool ShowTitle { get; set; } = true;
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
    [Parameter] public string CssClass { get; set; }
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
    [Parameter] public bool ShowExportButton { get; set; } = true;
    [Parameter] public string ChartButtonText { get; set; } = "Show as Chart";
    [Parameter] public RR.Blazor.Enums.ChartType? DefaultChartType { get; set; }
    
    [Parameter] public bool ShowColumnManager { get; set; }
    [Parameter] public bool EnableColumnReordering { get; set; }
    [Parameter] public bool EnableStickyColumns { get; set; }
    [Parameter] public bool EnableHorizontalScroll { get; set; }
    [Parameter] public bool EnableSorting { get; set; }
    [Parameter] public bool EnableFiltering { get; set; }
    [Parameter] public bool EnableSelection { get; set; }
    [Parameter] public bool EnableExport { get; set; }
    [Parameter] public bool EnablePaging { get; set; } = true;
    
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
        
        // Forward Blazor component parameters (not HTML attributes)
        var seq = 3;
        
        // Forward PageSize explicitly first to ensure it's handled correctly
        builder.AddAttribute(++seq, nameof(PageSize), PageSize);
        
        // Forward critical table parameters explicitly
        builder.AddAttribute(++seq, nameof(ShowToolbar), ShowToolbar);
        builder.AddAttribute(++seq, nameof(ShowColumnManager), ShowColumnManager);
        builder.AddAttribute(++seq, nameof(ShowSearch), ShowSearch);
        builder.AddAttribute(++seq, nameof(EnableColumnReordering), EnableColumnReordering);
        builder.AddAttribute(++seq, nameof(EnableStickyColumns), EnableStickyColumns);
        builder.AddAttribute(++seq, nameof(EnableHorizontalScroll), EnableHorizontalScroll);
        builder.AddAttribute(++seq, nameof(Hover), Hover);
        builder.AddAttribute(++seq, nameof(Bordered), Bordered);
        builder.AddAttribute(++seq, nameof(AutoGenerateColumns), AutoGenerateColumns);
        
        if (AdditionalAttributes != null)
        {
            // Remove PageSize from additional attributes to avoid duplication
            var filteredAttributes = AdditionalAttributes
                .Where(kvp => !string.Equals(kvp.Key, nameof(PageSize), StringComparison.OrdinalIgnoreCase))
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
                
            if (filteredAttributes.Any())
            {
                builder.AddMultipleAttributes(++seq, filteredAttributes);
                seq += filteredAttributes.Count;
            }
        }
        
        // Forward render fragments
        if (BulkOperations != null)
            builder.AddAttribute(++seq, nameof(BulkOperations), BulkOperations);
        if (TableActions != null)
            builder.AddAttribute(++seq, nameof(TableActions), TableActions);
        if (EmptyContent != null)
            builder.AddAttribute(++seq, nameof(EmptyContent), EmptyContent);
        if (LoadingContent != null)
            builder.AddAttribute(++seq, nameof(LoadingContent), LoadingContent);
        if (HeaderTemplate != null)
            builder.AddAttribute(++seq, nameof(HeaderTemplate), HeaderTemplate);
        if (FooterTemplate != null)
            builder.AddAttribute(++seq, nameof(FooterTemplate), FooterTemplate);
        
        builder.CloseComponent();
    }
}

