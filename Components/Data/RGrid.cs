using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.JSInterop;
using RR.Blazor.Attributes;
using RR.Blazor.Components.Base;
using RR.Blazor.Components.Core;
using RR.Blazor.Components.Display;
using RR.Blazor.Enums;
using RR.Blazor.Models;
using System.Collections;
using System.Reflection;

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
/// Smart grid component that intelligently delegates to appropriate R components.
/// Uses CSS Grid for responsive layouts, not HTML tables.
/// 
/// Component Delegation:
/// - Table mode → RTable (HTML tables for tabular data)
/// - Cards mode → RCard + CSS Grid layout
/// - List mode → RList component
/// - Tiles mode → RTile component (TODO)
/// - Gallery mode → RGallery component (TODO)
/// - Masonry mode → RMasonry component (TODO)
/// </summary>
[Component("RGrid", Category = "Data")]
[AIOptimized(
    Prompt = "Smart grid layout with auto mode detection and responsive design", 
    CommonUse = "responsive grids, card layouts, image galleries, data displays", 
    AvoidUsage = "Don't use for tabular data (use RTable), simple lists (use RList)"
)]
public class RGrid : RComponentBase
{
    private object _items;
    private Type _itemType;
    private GridMode _detectedMode;
    private bool _modeResolved;
    private object _builtInFilter;
    private IJSObjectReference _jsModule;
    private DotNetObjectReference<RGrid> _dotNetRef;
    private string _elementId;
    
    [Inject] private IJSRuntime JSRuntime { get; set; }
    
    #region Core Parameters
    
    /// <summary>
    /// The data items to display in the grid
    /// </summary>
    [Parameter, AIParameter("Collection of items to display", "@employees or @products")]
    public object Items 
    { 
        get => _items;
        set
        {
            _items = value;
            _modeResolved = false; // Reset mode detection when data changes
        }
    }
    
    /// <summary>
    /// Explicit grid mode. If Auto, mode will be detected from data properties.
    /// </summary>
    [Parameter, AIParameter("Grid layout mode", "GridMode.Auto for smart detection")]
    public GridMode Mode { get; set; } = GridMode.Auto;
    
    /// <summary>
    /// Custom template for rendering individual items
    /// </summary>
    [Parameter]
    public RenderFragment<object> ItemTemplate { get; set; }
    
    #endregion
    
    #region Filter Integration
    
    /// <summary>
    /// External filter reference for Case 1 integration
    /// </summary>
    [Parameter, AIParameter("External filter reference", "@myFilter")]
    public object Filter { get; set; }
    
    /// <summary>
    /// Enable built-in toolbar filter for Case 2 integration
    /// </summary>
    [Parameter, AIParameter("Enable built-in filter in toolbar", "true")]
    public bool EnableFilter { get; set; }
    
    /// <summary>
    /// Show quick search in toolbar
    /// </summary>
    [Parameter] public bool ShowQuickSearch { get; set; } = true;
    
    #endregion
    
    #region Responsive Configuration
    
    /// <summary>Default grid columns</summary>
    [Parameter] public int Columns { get; set; } = 4;
    
    /// <summary>Grid columns for extra small screens (mobile)</summary>
    [Parameter] public int ColumnsXs { get; set; } = 1;
    
    /// <summary>Grid columns for small screens</summary>
    [Parameter] public int ColumnsSm { get; set; } = 2;
    
    /// <summary>Grid columns for medium screens</summary>
    [Parameter] public int ColumnsMd { get; set; } = 3;
    
    /// <summary>Grid columns for large screens</summary>
    [Parameter] public int ColumnsLg { get; set; } = 4;
    
    /// <summary>Grid columns for extra large screens</summary>
    [Parameter] public int ColumnsXl { get; set; } = 6;
    
    /// <summary>Override mode for extra small screens</summary>
    [Parameter] public GridMode? ModeXs { get; set; }
    
    /// <summary>Override mode for small screens</summary>
    [Parameter] public GridMode? ModeSm { get; set; }
    
    /// <summary>Override mode for medium screens</summary>
    [Parameter] public GridMode? ModeMd { get; set; }
    
    /// <summary>Override mode for large screens</summary>
    [Parameter] public GridMode? ModeLg { get; set; }
    
    /// <summary>Override mode for extra large screens</summary>
    [Parameter] public GridMode? ModeXl { get; set; }
    
    #endregion
    
    #region Performance Features
    
    /// <summary>
    /// Enable virtualization for large datasets
    /// </summary>
    [Parameter] public bool EnableVirtualization { get; set; }
    
    /// <summary>
    /// Threshold for automatic virtualization
    /// </summary>
    [Parameter] public int VirtualizationThreshold { get; set; } = 1000;
    
    /// <summary>
    /// Gap between grid items
    /// </summary>
    [Parameter] public string Gap { get; set; } = "var(--space-4)";
    
    #endregion
    
    #region Pagination
    
    /// <summary>
    /// Enable pagination for the grid
    /// </summary>
    [Parameter] public bool EnablePagination { get; set; }
    
    /// <summary>
    /// Number of items per page
    /// </summary>
    [Parameter] public int PageSize { get; set; } = 24;
    
    /// <summary>
    /// Current page index (0-based)
    /// </summary>
    [Parameter] public int CurrentPage { get; set; }
    
    /// <summary>
    /// Callback when page changes
    /// </summary>
    [Parameter] public EventCallback<int> OnPageChanged { get; set; }
    
    #endregion
    
    #region Content
    
    /// <summary>Grid title</summary>
    [Parameter] public string Title { get; set; }
    
    /// <summary>Grid subtitle</summary>
    [Parameter] public string Subtitle { get; set; }
    
    /// <summary>Content to show when no items</summary>
    [Parameter] public RenderFragment EmptyContent { get; set; }
    
    /// <summary>Toolbar actions</summary>
    [Parameter] public RenderFragment ToolbarActions { get; set; }
    
    /// <summary>Event fired when responsive mode changes</summary>
    [Parameter] public EventCallback<GridMode> OnModeChanged { get; set; }
    
    /// <summary>Event fired when more items should be loaded (infinite scroll)</summary>
    [Parameter] public EventCallback OnLoadMore { get; set; }
    
    /// <summary>Grid height (for virtualization)</summary>
    [Parameter] public string Height { get; set; }
    
    /// <summary>Maximum grid height</summary>
    [Parameter] public string MaxHeight { get; set; }
    
    #endregion
    
    protected override void OnInitialized()
    {
        base.OnInitialized();
        _elementId = $"rgrid-{Guid.NewGuid():N}";
        _dotNetRef = DotNetObjectReference.Create(this);
    }

    protected override void OnParametersSet()
    {
        base.OnParametersSet();
        
        if (!_modeResolved)
        {
            _itemType = GetItemType();
            _detectedMode = Mode == GridMode.Auto ? DetectMode() : Mode;
            _modeResolved = true;
        }
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await base.OnAfterRenderAsync(firstRender);
        
        if (firstRender)
        {
            try
            {
                await InitializeJavaScript();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to initialize RGrid JavaScript: {ex.Message}");
            }
        }
    }

    private async Task InitializeJavaScript()
    {
        try
        {
            // Load rgrid module through the RR.Blazor module system
            var rgridModule = await JSRuntime.InvokeAsync<IJSObjectReference>("window.RRBlazor.loadModule", "rgrid");
            
            var options = new
            {
                dotNetRef = _dotNetRef,
                mode = Mode.ToString().ToLower(),
                modeXs = ModeXs?.ToString().ToLower(),
                modeSm = ModeSm?.ToString().ToLower(),
                modeMd = ModeMd?.ToString().ToLower(),
                modeLg = ModeLg?.ToString().ToLower(),
                modeXl = ModeXl?.ToString().ToLower(),
                columns = Columns,
                columnsXs = ColumnsXs,
                columnsSm = ColumnsSm,
                columnsMd = ColumnsMd,
                columnsLg = ColumnsLg,
                columnsXl = ColumnsXl,
                enableVirtualization = EnableVirtualization,
                hasMoreItems = false // Can be extended for infinite scroll
            };
            
            // Initialize through the loaded module
            await JSRuntime.InvokeVoidAsync("window.RGridModule.initialize", _elementId, options);
            
            _jsModule = rgridModule; // Store reference for disposal
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to initialize RGrid JavaScript: {ex.Message}");
            // Try fallback direct import
            try
            {
                _jsModule = await JSRuntime.InvokeAsync<IJSObjectReference>("import", "./_content/RR.Blazor/js/rgrid.js");
                
                var options = new
                {
                    dotNetRef = _dotNetRef,
                    mode = Mode.ToString().ToLower(),
                    modeXs = ModeXs?.ToString().ToLower(),
                    modeSm = ModeSm?.ToString().ToLower(),
                    modeMd = ModeMd?.ToString().ToLower(),
                    modeLg = ModeLg?.ToString().ToLower(),
                    modeXl = ModeXl?.ToString().ToLower(),
                    columns = Columns,
                    columnsXs = ColumnsXs,
                    columnsSm = ColumnsSm,
                    columnsMd = ColumnsMd,
                    columnsLg = ColumnsLg,
                    columnsXl = ColumnsXl,
                    enableVirtualization = EnableVirtualization,
                    hasMoreItems = false
                };
                
                await _jsModule.InvokeVoidAsync("RGridModule.initialize", _elementId, options);
            }
            catch (Exception fallbackEx)
            {
                Console.WriteLine($"Fallback RGrid JavaScript initialization also failed: {fallbackEx.Message}");
            }
        }
    }
    
    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        // Main container with element ID for JavaScript integration
        builder.OpenElement(0, "div");
        builder.AddAttribute(1, "id", _elementId);
        builder.AddAttribute(2, "class", "rgrid-container");
        builder.AddAttribute(3, "style", GetContainerStyles());
        
        if (Items == null || !GetFilteredItems().Any())
        {
            BuildEmptyState(builder);
            builder.CloseElement(); // Close main container
            return;
        }
        
        // Build toolbar if needed
        if (!string.IsNullOrEmpty(Title) || EnableFilter || ToolbarActions != null)
        {
            BuildToolbar(builder);
        }
        
        // Delegate to appropriate component based on detected mode
        switch (_detectedMode)
        {
            case GridMode.Table:
                BuildTableMode(builder);
                break;
                
            case GridMode.Cards:
                BuildCardsMode(builder);
                break;
                
            case GridMode.List:
                BuildListMode(builder);
                break;
                
            case GridMode.Tiles:
                BuildTilesMode(builder);
                break;
                
            case GridMode.Gallery:
                BuildGalleryMode(builder);
                break;
                
            case GridMode.Masonry:
                BuildMasonryMode(builder);
                break;
                
            default:
                BuildCardsMode(builder); // Default fallback
                break;
        }
        
        // Build pagination footer if enabled
        if (EnablePagination)
        {
            BuildPagination(builder);
        }
        
        // Close main container
        builder.CloseElement();
    }
    
    private void BuildToolbar(RenderTreeBuilder builder)
    {
        builder.OpenElement(0, "div");
        builder.AddAttribute(1, "class", "rgrid-toolbar");
        
        // Title section
        if (!string.IsNullOrEmpty(Title))
        {
            builder.OpenElement(2, "div");
            builder.AddAttribute(3, "class", "rgrid-toolbar-title");
            
            builder.OpenElement(4, "h3");
            builder.AddAttribute(5, "class", "rgrid-title");
            builder.AddContent(6, Title);
            builder.CloseElement(); // h3
            
            if (!string.IsNullOrEmpty(Subtitle))
            {
                builder.OpenElement(7, "p");
                builder.AddAttribute(8, "class", "rgrid-subtitle");
                builder.AddContent(9, Subtitle);
                builder.CloseElement(); // p
            }
            
            builder.CloseElement(); // title div
        }
        
        // Toolbar actions section
        if (EnableFilter || ToolbarActions != null)
        {
            builder.OpenElement(10, "div");
            builder.AddAttribute(11, "class", "rgrid-toolbar-actions");
            
            // Built-in filter when EnableFilter is true
            if (EnableFilter)
            {
                var filterType = typeof(RFilter);
                builder.OpenComponent(12, filterType);
                builder.AddAttribute(13, "DataSource", Items);
                builder.AddAttribute(14, "Compact", true);
                builder.AddAttribute(15, "ShowQuickFilters", false);
                builder.AddAttribute(16, "ShowAdvanced", false);
                builder.AddAttribute(17, "ShowSearch", ShowQuickSearch);
                builder.AddAttribute(18, "Class", "rgrid-built-in-filter");
                
                // Handle filter changes to trigger grid refresh
                builder.AddAttribute(19, "OnFilterChanged", EventCallback.Factory.Create<FilterStateChangedEventArgs>(this, async (args) =>
                {
                    // Reset pagination when filter changes
                    CurrentPage = 0;
                    
                    // Trigger re-render
                    StateHasChanged();
                    
                    // Notify parent of page change
                    if (OnPageChanged.HasDelegate)
                        await OnPageChanged.InvokeAsync(0);
                }));
                
                // Store filter reference
                builder.AddComponentReferenceCapture(20, (obj) => _builtInFilter = obj);
                
                builder.CloseComponent();
            }
            
            // Custom actions
            if (ToolbarActions != null)
            {
                builder.AddContent(18, ToolbarActions);
            }
            
            builder.CloseElement(); // actions div
        }
        
        builder.CloseElement(); // toolbar
    }
    
    private void BuildEmptyState(RenderTreeBuilder builder)
    {
        builder.OpenElement(0, "div");
        builder.AddAttribute(1, "class", "rgrid-empty");
        
        if (EmptyContent != null)
        {
            builder.AddContent(2, EmptyContent);
        }
        else
        {
            builder.AddContent(3, "No items to display");
        }
        
        builder.CloseElement();
    }
    
    private void BuildTableMode(RenderTreeBuilder builder)
    {
        // Delegate to RTableGeneric component
        var itemType = _itemType ?? typeof(object);
        var tableType = typeof(RTableGeneric<>).MakeGenericType(itemType);
        builder.OpenComponent(0, tableType);
        builder.AddAttribute(1, "Items", Items);
        builder.AddAttribute(2, "Filter", Filter);
        builder.AddAttribute(3, "Class", GetGridClasses());
        builder.AddAttribute(4, "EnablePaging", EnablePagination);
        builder.AddAttribute(5, "PageSize", PageSize);
        builder.AddAttribute(6, "EnableFiltering", EnableFilter);
        
        // Forward additional attributes if needed
        if (AdditionalAttributes != null)
        {
            builder.AddMultipleAttributes(7, AdditionalAttributes);
        }
        
        builder.CloseComponent();
    }
    
    private void BuildListMode(RenderTreeBuilder builder)
    {
        // Delegate to RList component
        var listType = typeof(RList);
        builder.OpenComponent(0, listType);
        builder.AddAttribute(1, "Class", GetGridClasses());
        
        // Add list items as child content
        builder.AddAttribute(2, "ChildContent", (RenderFragment)(listBuilder =>
        {
            var sequence = 0;
            foreach (var item in GetPagedItems())
            {
                if (ItemTemplate != null)
                {
                    listBuilder.AddContent(sequence++, ItemTemplate(item));
                }
                else
                {
                    BuildAutoListItem(listBuilder, sequence++, item);
                }
            }
        }));
        
        builder.CloseComponent();
    }
    
    private void BuildCardsMode(RenderTreeBuilder builder)
    {
        var items = GetPagedItems();
        var shouldVirtualize = EnableVirtualization && GetFilteredItems().Count() >= VirtualizationThreshold;
        
        if (shouldVirtualize)
        {
            // Use virtualized grid
            var virtualListType = typeof(RVirtualListGeneric<>).MakeGenericType(_itemType ?? typeof(object));
            builder.OpenComponent(0, virtualListType);
            builder.AddAttribute(1, "Items", items);
            builder.AddAttribute(2, "Height", "400px");
            builder.AddAttribute(3, "ItemHeight", 150);
            builder.AddAttribute(4, "OverscanCount", 5);
            builder.AddAttribute(5, "Class", GetGridClasses() + " rgrid-virtualized");
            
            // Create item template
            builder.AddAttribute(6, "ItemTemplate", (RenderFragment<object>)((item) => (builder) =>
            {
                builder.OpenElement(0, "div");
                builder.AddAttribute(1, "class", "rgrid-virtual-item");
                
                if (ItemTemplate != null)
                {
                    builder.AddContent(2, ItemTemplate(item));
                }
                else
                {
                    BuildAutoCard(builder, 3, item);
                }
                
                builder.CloseElement();
            }));
            
            builder.CloseComponent();
        }
        else
        {
            // Use regular CSS Grid with RCard components
            builder.OpenElement(0, "div");
            builder.AddAttribute(1, "class", GetGridClasses());
            builder.AddAttribute(2, "style", GetGridStyles());
            
            var sequence = 3;
            foreach (var item in items)
            {
                if (ItemTemplate != null)
                {
                    builder.AddContent(sequence++, ItemTemplate(item));
                }
                else
                {
                    BuildAutoCard(builder, sequence++, item);
                }
            }
            
            builder.CloseElement();
        }
    }
    
    private void BuildTilesMode(RenderTreeBuilder builder)
    {
        // Use CSS Grid with RTile components
        builder.OpenElement(0, "div");
        builder.AddAttribute(1, "class", GetGridClasses());
        builder.AddAttribute(2, "style", GetGridStyles());
        
        var sequence = 3;
        foreach (var item in GetPagedItems())
        {
            if (ItemTemplate != null)
            {
                builder.AddContent(sequence++, ItemTemplate(item));
            }
            else
            {
                BuildAutoTile(builder, sequence++, item);
            }
        }
        
        builder.CloseElement();
    }
    
    private void BuildGalleryMode(RenderTreeBuilder builder)
    {
        // Delegate to RGallery component
        var galleryType = typeof(RGallery);
        builder.OpenComponent(0, galleryType);
        builder.AddAttribute(1, "Images", GetPagedItems());
        builder.AddAttribute(2, "Columns", ColumnsXl);
        builder.AddAttribute(3, "ColumnsSm", ColumnsSm);
        builder.AddAttribute(4, "ColumnsMd", ColumnsMd);
        builder.AddAttribute(5, "ColumnsLg", ColumnsLg);
        builder.AddAttribute(6, "Gap", Gap);
        builder.AddAttribute(7, "Class", GetGridClasses());
        builder.CloseComponent();
    }
    
    private void BuildMasonryMode(RenderTreeBuilder builder)
    {
        // Delegate to RMasonry component
        var masonryType = typeof(RMasonry);
        builder.OpenComponent(0, masonryType);
        builder.AddAttribute(1, "Items", GetPagedItems());
        builder.AddAttribute(2, "ItemTemplate", ItemTemplate);
        builder.AddAttribute(3, "Columns", ColumnsXl);
        builder.AddAttribute(4, "ColumnsSm", ColumnsSm);
        builder.AddAttribute(5, "ColumnsMd", ColumnsMd);
        builder.AddAttribute(6, "ColumnsLg", ColumnsLg);
        builder.AddAttribute(7, "Gap", Gap);
        builder.AddAttribute(8, "Class", GetGridClasses());
        builder.CloseComponent();
    }
    
    private void BuildAutoTile(RenderTreeBuilder builder, int sequence, object item)
    {
        var tileType = typeof(RTile);
        builder.OpenComponent(sequence, tileType);
        
        // Extract common properties for tile
        var title = GetPropertyValue(item, "Name")?.ToString() ?? 
                   GetPropertyValue(item, "Title")?.ToString();
                   
        var subtitle = GetPropertyValue(item, "Department")?.ToString() ?? 
                      GetPropertyValue(item, "Category")?.ToString();
                      
        var description = GetPropertyValue(item, "Description")?.ToString() ?? 
                         GetPropertyValue(item, "Position")?.ToString();
                         
        var imageUrl = GetPropertyValue(item, "ImageUrl")?.ToString() ?? 
                      GetPropertyValue(item, "Avatar")?.ToString();
        
        if (!string.IsNullOrEmpty(title))
            builder.AddAttribute(sequence + 1, "Title", title);
            
        if (!string.IsNullOrEmpty(subtitle))
            builder.AddAttribute(sequence + 2, "Subtitle", subtitle);
            
        if (!string.IsNullOrEmpty(description))
            builder.AddAttribute(sequence + 3, "Description", description);
            
        if (!string.IsNullOrEmpty(imageUrl))
            builder.AddAttribute(sequence + 4, "ImageUrl", imageUrl);
        
        builder.CloseComponent();
    }
    
    private void BuildAutoCard(RenderTreeBuilder builder, int sequence, object item)
    {
        if (item == null)
        {
            BuildFallbackCard(builder, sequence, "No data");
            return;
        }
        
        try
        {
            var cardType = typeof(RCard);
            builder.OpenComponent(sequence, cardType);
            
            var itemType = item.GetType();
            var properties = itemType.GetProperties()
                .Where(p => p.CanRead && p.GetIndexParameters().Length == 0)
                .ToList();
            
            string title = null;
            string subtitle = null;
            string content = null;
            string imageUrl = null;
            
            // Smart detection: find best properties for card display
            // Priority 1: Title-like properties
            var titleCandidates = properties
                .Where(p => p.Name.Contains("Name", StringComparison.OrdinalIgnoreCase) ||
                           p.Name.Contains("Title", StringComparison.OrdinalIgnoreCase) ||
                           p.Name.Contains("Display", StringComparison.OrdinalIgnoreCase) ||
                           p.Name.Contains("Label", StringComparison.OrdinalIgnoreCase))
                .OrderBy(p => p.Name.Length)
                .FirstOrDefault();
            
            if (titleCandidates != null)
            {
                title = titleCandidates.GetValue(item)?.ToString();
            }
            
            // If no title found, use ID or first string property
            if (string.IsNullOrWhiteSpace(title))
            {
                var idProp = properties.FirstOrDefault(p => p.Name.EndsWith("Id", StringComparison.OrdinalIgnoreCase));
                if (idProp != null)
                {
                    var idValue = idProp.GetValue(item)?.ToString();
                    title = !string.IsNullOrWhiteSpace(idValue) ? $"{itemType.Name} #{idValue}" : itemType.Name;
                }
                else
                {
                    var firstString = properties.FirstOrDefault(p => p.PropertyType == typeof(string));
                    title = firstString?.GetValue(item)?.ToString() ?? itemType.Name;
                }
            }
            
            // Priority 2: Subtitle-like properties
            var subtitleCandidates = properties
                .Where(p => p != titleCandidates &&
                           (p.Name.Contains("Description", StringComparison.OrdinalIgnoreCase) ||
                            p.Name.Contains("Subtitle", StringComparison.OrdinalIgnoreCase) ||
                            p.Name.Contains("Category", StringComparison.OrdinalIgnoreCase) ||
                            p.Name.Contains("Type", StringComparison.OrdinalIgnoreCase)))
                .FirstOrDefault();
            
            if (subtitleCandidates != null)
            {
                subtitle = subtitleCandidates.GetValue(item)?.ToString();
            }
            
            // Priority 3: Build content from other significant properties
            var significantProps = properties
                .Where(p => p != titleCandidates && p != subtitleCandidates)
                .Where(p => p.PropertyType.IsPrimitive || 
                           p.PropertyType == typeof(string) || 
                           p.PropertyType == typeof(decimal) ||
                           p.PropertyType == typeof(DateTime))
                .Take(3)
                .ToList();
            
            var contentParts = new List<string>();
            foreach (var prop in significantProps)
            {
                var value = prop.GetValue(item);
                if (value != null && !string.IsNullOrWhiteSpace(value.ToString()))
                {
                    var formatted = value switch
                    {
                        DateTime dt => dt.ToString("MMM dd, yyyy"),
                        decimal dec => dec.ToString("C"),
                        double d => d.ToString("F2"),
                        float f => f.ToString("F2"),
                        bool b => b ? "✓" : "✗",
                        _ => value.ToString()
                    };
                    
                    if (!string.IsNullOrWhiteSpace(formatted))
                        contentParts.Add($"{prop.Name}: {formatted}");
                }
            }
            
            if (contentParts.Any())
                content = string.Join(" • ", contentParts);
            
            // Priority 4: Image URL detection
            var imageProp = properties
                .Where(p => p.PropertyType == typeof(string))
                .FirstOrDefault(p => p.Name.Contains("Image", StringComparison.OrdinalIgnoreCase) ||
                                    p.Name.Contains("Photo", StringComparison.OrdinalIgnoreCase) ||
                                    p.Name.Contains("Picture", StringComparison.OrdinalIgnoreCase) ||
                                    p.Name.Contains("Avatar", StringComparison.OrdinalIgnoreCase) ||
                                    p.Name.Contains("Thumbnail", StringComparison.OrdinalIgnoreCase));
            
            if (imageProp != null)
                imageUrl = imageProp.GetValue(item)?.ToString();
            
            // Set card properties
            builder.AddAttribute(sequence + 1, "Text", title ?? itemType.Name);
                
            if (!string.IsNullOrWhiteSpace(subtitle))
                builder.AddAttribute(sequence + 2, "Subtitle", subtitle);
                
            if (!string.IsNullOrWhiteSpace(content))
                builder.AddAttribute(sequence + 3, "Content", content);
                
            if (!string.IsNullOrWhiteSpace(imageUrl))
                builder.AddAttribute(sequence + 4, "ImageSrc", imageUrl);
                
            // Set styling
            builder.AddAttribute(sequence + 5, "Clickable", true);
            builder.AddAttribute(sequence + 6, "Variant", CardVariant.Elevated);
            builder.AddAttribute(sequence + 7, "Density", Density);
            builder.AddAttribute(sequence + 8, "Class", "h-full");
            
            builder.CloseComponent();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error building auto card: {ex.Message}");
            BuildFallbackCard(builder, sequence, $"Error: {ex.Message}");
        }
    }
    
    private void BuildFallbackCard(RenderTreeBuilder builder, int sequence, string message)
    {
        var cardType = typeof(RCard);
        builder.OpenComponent(sequence, cardType);
        builder.AddAttribute(sequence + 1, "Text", "Error");
        builder.AddAttribute(sequence + 2, "Content", message);
        builder.AddAttribute(sequence + 3, "Variant", CardVariant.Outlined);
        builder.CloseComponent();
    }
    
    private void BuildAutoListItem(RenderTreeBuilder builder, int sequence, object item)
    {
        if (item == null)
        {
            BuildFallbackListItem(builder, sequence, "No data");
            return;
        }
        
        try
        {
            var listItemType = typeof(RListItem);
            builder.OpenComponent(sequence, listItemType);
            
            // Extract properties for list item
            var title = GetPropertyValue(item, "Name")?.ToString() ?? 
                       GetPropertyValue(item, "Title")?.ToString() ?? 
                       GetPropertyValue(item, "DisplayName")?.ToString() ??
                       item.ToString();
                       
            var subtitle = GetPropertyValue(item, "Position")?.ToString() ?? 
                          GetPropertyValue(item, "Department")?.ToString() ?? 
                          GetPropertyValue(item, "Category")?.ToString() ?? 
                          GetPropertyValue(item, "Email")?.ToString();
                          
            var description = GetPropertyValue(item, "Description")?.ToString() ?? 
                             GetPropertyValue(item, "Details")?.ToString();
                             
            var status = GetPropertyValue(item, "Status")?.ToString();
            var avatar = GetPropertyValue(item, "Avatar")?.ToString() ?? 
                        GetPropertyValue(item, "Initial")?.ToString();
            
            // Set properties
            if (!string.IsNullOrWhiteSpace(title))
                builder.AddAttribute(sequence + 1, "Text", title);
                
            if (!string.IsNullOrWhiteSpace(subtitle))
                builder.AddAttribute(sequence + 2, "Subtitle", subtitle);
                
            if (!string.IsNullOrWhiteSpace(description))
                builder.AddAttribute(sequence + 3, "Description", description);
                
            if (!string.IsNullOrWhiteSpace(status))
                builder.AddAttribute(sequence + 4, "BadgeText", status);
                
            if (!string.IsNullOrWhiteSpace(avatar))
                builder.AddAttribute(sequence + 5, "AvatarText", avatar);
            
            // Make clickable
            builder.AddAttribute(sequence + 6, "Clickable", true);
            builder.AddAttribute(sequence + 7, "Class", "rgrid-list-item");
            
            builder.CloseComponent();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error building auto list item: {ex.Message}");
            BuildFallbackListItem(builder, sequence, $"Error: {ex.Message}");
        }
    }
    
    private void BuildFallbackListItem(RenderTreeBuilder builder, int sequence, string message)
    {
        var listItemType = typeof(RListItem);
        builder.OpenComponent(sequence, listItemType);
        builder.AddAttribute(sequence + 1, "Text", "Error");
        builder.AddAttribute(sequence + 2, "Subtitle", message);
        builder.AddAttribute(sequence + 3, "Icon", "error");
        builder.AddAttribute(sequence + 4, "Class", "rgrid-list-item-error");
        builder.CloseComponent();
    }
    
    private Type GetItemType()
    {
        if (Items == null) return typeof(object);
        
        var itemsType = Items.GetType();
        
        // Check for generic collection types
        if (itemsType.IsGenericType)
        {
            var genericArgs = itemsType.GetGenericArguments();
            if (genericArgs.Length == 1)
                return genericArgs[0];
        }
        
        // Check for array types
        if (itemsType.IsArray)
            return itemsType.GetElementType();
        
        // Check interfaces for IEnumerable<T>
        foreach (var interfaceType in itemsType.GetInterfaces())
        {
            if (interfaceType.IsGenericType && 
                interfaceType.GetGenericTypeDefinition() == typeof(IEnumerable<>))
            {
                var genericArgs = interfaceType.GetGenericArguments();
                if (genericArgs.Length == 1)
                    return genericArgs[0];
            }
        }
        
        // Examine first item
        if (Items is IEnumerable enumerable)
        {
            foreach (var item in enumerable)
            {
                return item?.GetType() ?? typeof(object);
            }
        }
        
        return typeof(object);
    }
    
    private GridMode DetectMode()
    {
        if (_itemType == null) return GridMode.Cards;
        
        var properties = _itemType.GetProperties(BindingFlags.Public | BindingFlags.Instance);
        var propertyNames = properties.Select(p => p.Name.ToLowerInvariant()).ToHashSet();
        
        // Image properties → Gallery mode
        if (HasAnyProperty(propertyNames, "imageurl", "url", "thumbnail", "src", "image"))
            return GridMode.Gallery;
        
        // Complex objects (many properties) → Cards mode
        if (properties.Length > 5)
            return GridMode.Cards;
        
        // Simple objects → List mode
        if (properties.Length <= 3)
            return GridMode.List;
        
        // Default to Cards
        return GridMode.Cards;
    }
    
    private bool HasAnyProperty(HashSet<string> propertyNames, params string[] searchNames)
    {
        return searchNames.Any(name => propertyNames.Contains(name));
    }
    
    private object GetPropertyValue(object item, string propertyName)
    {
        if (item == null) return null;
        
        try
        {
            var property = item.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
            if (property?.CanRead == true)
            {
                return property.GetValue(item);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error getting property '{propertyName}': {ex.Message}");
        }
        
        return null;
    }
    
    private IEnumerable<object> GetAllItems()
    {
        if (Items == null) return Enumerable.Empty<object>();
        
        if (Items is IEnumerable enumerable)
        {
            return enumerable.Cast<object>();
        }
        
        return Enumerable.Empty<object>();
    }
    
    private IEnumerable<object> GetFilteredItems()
    {
        var allItems = GetAllItems();
        
        // Apply external filter if provided
        if (Filter != null)
        {
            try
            {
                var filterType = Filter.GetType();
                var getPredicateMethod = filterType.GetMethod("GetPredicate", new Type[0]);
                
                if (getPredicateMethod != null && _itemType != null)
                {
                    var predicate = getPredicateMethod.Invoke(Filter, null);
                    if (predicate != null)
                    {
                        var compiledProperty = predicate.GetType().GetProperty("Compile");
                        if (compiledProperty != null)
                        {
                            var compiled = compiledProperty.GetValue(predicate, null);
                            if (compiled is Delegate compiledDelegate)
                            {
                                return allItems.Where(item => 
                                {
                                    try
                                    {
                                        return (bool)compiledDelegate.DynamicInvoke(item);
                                    }
                                    catch
                                    {
                                        return true; // Include item if filtering fails
                                    }
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"External filter error: {ex.Message}");
            }
        }
        
        // Apply built-in filter if enabled
        if (EnableFilter && _builtInFilter != null)
        {
            try
            {
                var filterType = _builtInFilter.GetType();
                var getPredicateMethod = filterType.GetMethod("GetPredicate", new Type[0]);
                
                if (getPredicateMethod != null)
                {
                    var predicate = getPredicateMethod.Invoke(_builtInFilter, null);
                    if (predicate != null)
                    {
                        var compiledProperty = predicate.GetType().GetProperty("Compile");
                        if (compiledProperty != null)
                        {
                            var compiled = compiledProperty.GetValue(predicate, null);
                            if (compiled is Delegate compiledDelegate)
                            {
                                return allItems.Where(item => 
                                {
                                    try
                                    {
                                        return (bool)compiledDelegate.DynamicInvoke(item);
                                    }
                                    catch
                                    {
                                        return true;
                                    }
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Built-in filter error: {ex.Message}");
            }
        }
        
        return allItems;
    }
    
    private IEnumerable<object> GetPagedItems()
    {
        var filteredItems = GetFilteredItems();
        
        if (!EnablePagination)
            return filteredItems;
        
        // Apply pagination AFTER filtering
        return filteredItems
            .Skip(CurrentPage * PageSize)
            .Take(PageSize);
    }
    
    private int GetTotalCount()
    {
        // Total count should be of filtered items, not paged items
        return GetFilteredItems().Count();
    }
    
    private int GetTotalPages()
    {
        var totalItems = GetTotalCount();
        if (totalItems == 0 || PageSize == 0) return 1;
        return (int)Math.Ceiling((double)totalItems / PageSize);
    }
    
    private string GetGridClasses()
    {
        var classes = new List<string> { "rgrid" };
        
        // Add mode-specific classes
        classes.Add($"rgrid-{_detectedMode.ToString().ToLower()}");
        
        // Add responsive mode classes if specified
        if (ModeXs.HasValue) classes.Add($"rgrid-xs-{ModeXs.Value.ToString().ToLower()}");
        if (ModeSm.HasValue) classes.Add($"rgrid-sm-{ModeSm.Value.ToString().ToLower()}");
        if (ModeMd.HasValue) classes.Add($"rgrid-md-{ModeMd.Value.ToString().ToLower()}");
        if (ModeLg.HasValue) classes.Add($"rgrid-lg-{ModeLg.Value.ToString().ToLower()}");
        if (ModeXl.HasValue) classes.Add($"rgrid-xl-{ModeXl.Value.ToString().ToLower()}");
        
        // Add density classes
        if (Density != DensityType.Normal)
            classes.Add($"rgrid-{Density.ToString().ToLower()}");
        
        // Add custom classes
        if (!string.IsNullOrEmpty(Class))
            classes.Add(Class);
        
        return string.Join(" ", classes);
    }
    
    private string GetGridStyles()
    {
        var styles = new List<string>();
        
        // Set CSS Grid column variables for responsive design
        styles.Add($"--grid-columns-xs: {ColumnsXs}");
        styles.Add($"--grid-columns-sm: {ColumnsSm}");
        styles.Add($"--grid-columns-md: {ColumnsMd}");
        styles.Add($"--grid-columns-lg: {ColumnsLg}");
        styles.Add($"--grid-columns-xl: {ColumnsXl}");
        
        // Set the active column count and explicit grid-template-columns
        var currentColumns = ColumnsLg; // Default fallback
        styles.Add($"--rgrid-columns: {currentColumns}");
        
        // Explicitly set grid-template-columns to ensure it works
        var columnTemplate = string.Join(" ", Enumerable.Repeat("1fr", currentColumns));
        styles.Add($"grid-template-columns: {columnTemplate}");
        
        // Gap
        if (!string.IsNullOrEmpty(Gap))
            styles.Add($"gap: {Gap}");
        else
            styles.Add("gap: var(--space-4)");
        
        return string.Join("; ", styles);
    }
    
    private void BuildPagination(RenderTreeBuilder builder)
    {
        var paginationFooterType = typeof(RPaginationFooter);
        builder.OpenComponent(0, paginationFooterType);
        
        // Pass pagination parameters
        builder.AddAttribute(1, "CurrentPage", CurrentPage);
        builder.AddAttribute(2, "PageSize", PageSize);
        builder.AddAttribute(3, "TotalItems", GetTotalCount());
        builder.AddAttribute(4, "TotalPages", GetTotalPages());
        builder.AddAttribute(5, "OnPageChanged", EventCallback.Factory.Create<int>(this, async (newPage) =>
        {
            CurrentPage = newPage;
            await OnPageChanged.InvokeAsync(newPage);
            StateHasChanged();
        }));
        
        builder.CloseComponent();
    }

    // ===== JAVASCRIPT CALLBACK METHODS =====

    [JSInvokable]
    public async Task OnJSModeChanged(string newMode, string breakpoint)
    {
        if (Enum.TryParse<GridMode>(newMode, true, out var parsedMode))
        {
            _detectedMode = parsedMode;
            StateHasChanged();
            
            if (OnModeChanged.HasDelegate)
            {
                await OnModeChanged.InvokeAsync(parsedMode);
            }
        }
    }

    [JSInvokable]
    public async Task LoadMoreItems()
    {
        // Future enhancement: implement infinite scroll
        if (OnLoadMore.HasDelegate)
        {
            await OnLoadMore.InvokeAsync();
        }
    }

    // ===== HELPER METHODS =====

    private string GetResponsiveClasses()
    {
        var classes = new List<string>
        {
            "rgrid",
            $"rgrid-mode-{_detectedMode.ToString().ToLower()}"
        };

        if (EnableVirtualization)
            classes.Add("rgrid-virtualized");

        if (EnablePagination)
            classes.Add("rgrid-paginated");

        return string.Join(" ", classes);
    }

    private string GetContainerStyles()
    {
        var styles = new List<string>();

        if (Height != null)
            styles.Add($"height: {Height}");

        if (MaxHeight != null)
            styles.Add($"max-height: {MaxHeight}");

        return string.Join("; ", styles);
    }

    // ===== DISPOSE =====

    protected override async ValueTask DisposeAsyncCore()
    {
        try
        {
            if (_jsModule != null && !string.IsNullOrEmpty(_elementId))
            {
                await _jsModule.InvokeVoidAsync("RGridModule.dispose", _elementId);
            }
        }
        catch
        {
            // Ignore disposal errors
        }

        if (_jsModule != null)
            await _jsModule.DisposeAsync();
        
        _dotNetRef?.Dispose();
        
        await base.DisposeAsyncCore();
    }
}