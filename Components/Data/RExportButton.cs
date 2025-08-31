using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using RR.Blazor.Components.Base;
using RR.Blazor.Services.Export;
using RR.Blazor.Enums;
using System.Collections;
using System.Linq;

namespace RR.Blazor.Components.Data;

/// <summary>
/// Base class for export button components with shared parameters
/// </summary>
public abstract class RExportButtonBase : RComponentBase
{
    [Parameter] public string FileName { get; set; }
    [Parameter] public ExportFormat DefaultFormat { get; set; } = ExportFormat.CSV;
    [Parameter] public List<ExportFormat> AllowedFormats { get; set; }
    [Parameter] public bool ShowAdvancedOptions { get; set; }
    [Parameter] public bool AutoDownload { get; set; } = true;
    [Parameter] public List<string> ColumnsToExport { get; set; }
    [Parameter] public Dictionary<string, string> ColumnMappings { get; set; }
    [Parameter] public Dictionary<string, Func<object, string>> CustomFormatters { get; set; }
    [Parameter] public ExportOptions CustomOptions { get; set; }
    [Parameter] public EventCallback<ExportResult> OnExportComplete { get; set; }
    [Parameter] public EventCallback<ExportResult> OnExportError { get; set; }
    
    // UI parameters
    [Parameter] public string Text { get; set; } = "Export";
    [Parameter] public string Icon { get; set; } = "download";
    [Parameter] public bool IconRight { get; set; }
    [Parameter] public bool ShowDropdown { get; set; }
    [Parameter] public bool ShowProgress { get; set; } = true;
    [Parameter] public string ButtonClass { get; set; }
    [Parameter] public string ContainerClass { get; set; }
    [Parameter] public ButtonType ButtonType { get; set; } = ButtonType.Secondary;
    [Parameter] public VariantType ButtonVariant { get; set; } = VariantType.Primary;
    [Parameter] public SizeType ButtonSize { get; set; } = SizeType.Medium;
}

/// <summary>
/// Smart export button wrapper that automatically detects type from DataSource
/// Usage: <RExportButton DataSource="@employees" /> - no TItem required!
/// </summary>
public class RExportButton : RExportButtonBase
{
    [Parameter] public object DataSource { get; set; }
    
    private Type _itemType;
    private bool _itemTypeResolved;

    protected override void OnParametersSet()
    {
        base.OnParametersSet();
        if (!_itemTypeResolved && DataSource != null)
        {
            _itemType = GetItemType();
            _itemTypeResolved = true;
        }
    }
    
    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        if (DataSource == null)
        {
            // Render disabled button when no data
            builder.OpenElement(0, "button");
            builder.AddAttribute(1, "class", "btn btn-secondary btn-disabled");
            builder.AddAttribute(2, "disabled", true);
            builder.OpenElement(3, "i");
            builder.AddAttribute(4, "class", "icon");
            builder.AddContent(5, Icon);
            builder.CloseElement();
            builder.AddContent(6, " " + Text);
            builder.CloseElement();
            return;
        }
        
        // Resolve item type if not already done
        if (_itemType == null)
        {
            _itemType = GetItemType();
        }
        
        // Create generic RExportButtonGeneric<T> component from the razor file
        // We need to find the compiled Razor component type
        var assembly = typeof(RExportButton).Assembly;
        var openGenericTypeName = "RR.Blazor.Components.Data.RExportButtonGeneric`1";
        var openGenericType = assembly.GetType(openGenericTypeName);
        
        if (openGenericType == null)
        {
            // If not found, throw an error with helpful message
            throw new InvalidOperationException($"Could not find type {openGenericTypeName} in assembly {assembly.FullName}");
        }
        
        var genericExportButtonType = openGenericType.MakeGenericType(_itemType);
        
        builder.OpenComponent(0, genericExportButtonType);
        
        // Forward all parameters
        ForwardAllParameters(builder);
        
        builder.CloseComponent();
    }
    
    private Type GetItemType()
    {
        if (DataSource == null)
            return typeof(object);
            
        var dataSourceType = DataSource.GetType();
        
        // Check for generic IEnumerable<T>
        var genericInterface = dataSourceType.GetInterfaces()
            .FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEnumerable<>));
        
        if (genericInterface != null)
        {
            return genericInterface.GetGenericArguments()[0];
        }
        
        // Check if it's an array
        if (dataSourceType.IsArray)
        {
            return dataSourceType.GetElementType();
        }
        
        // Try to get type from first item in collection
        if (DataSource is IEnumerable enumerable && !(DataSource is string))
        {
            foreach (var item in enumerable)
            {
                if (item != null)
                {
                    return item.GetType();
                }
            }
        }
        
        // Default to object
        return typeof(object);
    }
    
    private void ForwardAllParameters(RenderTreeBuilder builder)
    {
        var seq = 1;
        
        // Forward DataSource
        builder.AddAttribute(seq++, "DataSource", DataSource);
        
        // Forward all base class parameters
        if (!string.IsNullOrEmpty(FileName))
            builder.AddAttribute(seq++, "FileName", FileName);
            
        builder.AddAttribute(seq++, "DefaultFormat", DefaultFormat);
        
        if (AllowedFormats?.Any() == true)
            builder.AddAttribute(seq++, "AllowedFormats", AllowedFormats);
            
        builder.AddAttribute(seq++, "ShowAdvancedOptions", ShowAdvancedOptions);
        builder.AddAttribute(seq++, "AutoDownload", AutoDownload);
        
        if (ColumnsToExport?.Any() == true)
            builder.AddAttribute(seq++, "ColumnsToExport", ColumnsToExport);
            
        if (ColumnMappings?.Any() == true)
            builder.AddAttribute(seq++, "ColumnMappings", ColumnMappings);
            
        if (CustomFormatters?.Any() == true)
            builder.AddAttribute(seq++, "CustomFormatters", CustomFormatters);
            
        if (CustomOptions != null)
            builder.AddAttribute(seq++, "CustomOptions", CustomOptions);
            
        if (OnExportComplete.HasDelegate)
            builder.AddAttribute(seq++, "OnExportComplete", OnExportComplete);
            
        if (OnExportError.HasDelegate)
            builder.AddAttribute(seq++, "OnExportError", OnExportError);
        
        // Forward UI parameters
        builder.AddAttribute(seq++, "Text", Text);
        builder.AddAttribute(seq++, "Icon", Icon);
        builder.AddAttribute(seq++, "IconRight", IconRight);
        builder.AddAttribute(seq++, "ShowDropdown", ShowDropdown);
        builder.AddAttribute(seq++, "ShowProgress", ShowProgress);
        
        if (!string.IsNullOrEmpty(ButtonClass))
            builder.AddAttribute(seq++, "ButtonClass", ButtonClass);
            
        if (!string.IsNullOrEmpty(ContainerClass))
            builder.AddAttribute(seq++, "ContainerClass", ContainerClass);
            
        builder.AddAttribute(seq++, "ButtonType", ButtonType);
        builder.AddAttribute(seq++, "ButtonVariant", ButtonVariant);
        builder.AddAttribute(seq++, "ButtonSize", ButtonSize);
        
        // Forward any additional attributes
        if (AdditionalAttributes?.Any() == true)
        {
            builder.AddMultipleAttributes(seq, AdditionalAttributes);
        }
    }
}