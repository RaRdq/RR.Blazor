# RGrid Smart Component - Enterprise Implementation Guide

## Component Purpose & Philosophy

**RGrid** is THE ultimate enterprise-grade smart grid component that handles ALL grid display scenarios - from data tables to card layouts, from virtualized lists to masonry galleries. It follows the smart component pattern with RGrid (smart wrapper) and RGridGeneric<T> (typed implementation).

## Core Architecture

### Smart Component Pattern (Following RForm/RFormGeneric)
```
RGridBase (abstract base class with shared parameters)
├── RGrid (smart wrapper with type detection)
└── RGridGeneric<T> (strongly-typed implementation)
```

### What RGrid IS:
- **Universal grid solution** for all enterprise data display needs
- **Multi-mode component** supporting tables, cards, tiles, lists, galleries
- **Smart type detection** with automatic template selection
- **Enterprise features** including virtualization, drag-drop, real-time updates
- **Ultra-customizable** with full template control
- **S-tier performance** optimized for 100k+ items

### What RGrid HANDLES:
- Tabular data grids with columns and rows
- Card-based layouts for dashboards
- Tile grids for compact displays
- Masonry layouts for variable content
- Gallery views for media
- List views for simple data
- Tree grids for hierarchical data
- Kanban boards with drag-drop
- Timeline views for temporal data

## Implementation Pattern (Following RChoice/RForm Architecture)

### Component Files Structure
```
RR.Blazor/Components/Data/
├── RGrid.cs              - Smart wrapper + base class (like RChoice.cs)
├── RGridGeneric.razor     - Generic implementation (existing)
└── RGridBase class        - Shared parameters (in RGrid.cs)
```

### Smart Wrapper Implementation (RGrid.cs)
```csharp
public abstract class RGridBase : RComponentBase
{
    // Core parameters
    [Parameter] public object DataSource { get; set; }
    [Parameter] public object Columns { get; set; }
    [Parameter] public GridMode Mode { get; set; } = GridMode.Auto;
    
    // Events as object-based for flexibility
    [Parameter] public EventCallback<object> OnRowClick { get; set; }
    [Parameter] public EventCallback<object> OnSelectionChanged { get; set; }
    [Parameter] public EventCallback<object> OnItemReordered { get; set; }
    
    // Features
    [Parameter] public bool EnableFiltering { get; set; }
    [Parameter] public bool EnableSorting { get; set; } = true;
    [Parameter] public bool EnablePaging { get; set; } = true;
    [Parameter] public bool EnableVirtualization { get; set; }
    [Parameter] public bool EnableDragDrop { get; set; }
    
    // Filter Integration
    [Parameter] public RenderFragment FilterContent { get; set; }
    [Parameter] public FilterPosition FilterPosition { get; set; } = FilterPosition.Top;
}

public class RGrid : RGridBase
{
    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        // Type detection
        var itemType = GetItemType();
        if (itemType == null) return;
        
        // Create RGridGeneric<T>
        var genericType = typeof(RGridGeneric<>).MakeGenericType(itemType);
        builder.OpenComponent(0, genericType);
        
        // Forward parameters
        ForwardParameters(builder);
        
        builder.CloseComponent();
    }
    
    private Type GetItemType()
    {
        if (DataSource == null) return typeof(object);
        
        var type = DataSource.GetType();
        
        // Check for IEnumerable<T>
        var enumerable = type.GetInterfaces()
            .FirstOrDefault(i => i.IsGenericType && 
                           i.GetGenericTypeDefinition() == typeof(IEnumerable<>));
        
        if (enumerable != null)
            return enumerable.GetGenericArguments()[0];
            
        // Check first item
        if (DataSource is IEnumerable items)
        {
            foreach (var item in items)
            {
                if (item != null) return item.GetType();
            }
        }
        
        return typeof(object);
    }
}
```

## Grid Modes & Visual Patterns

### GridMode Enum - Automatic Layout Detection
```csharp
public enum GridMode
{
    Auto,         // Smart detection based on data
    Table,        // Traditional data table
    Cards,        // Card-based layout  
    Tiles,        // Compact tiles
    List,         // Single column list
    Gallery,      // Image gallery
    Masonry,      // Pinterest-style
    Kanban,       // Drag-drop columns
    Timeline,     // Temporal view
    Tree          // Hierarchical
}
```

### Mode Auto-Detection Logic
```csharp
private GridMode DetectMode(Type itemType)
{
    // If columns are defined, use Table mode
    if (Columns != null && Columns.Any()) 
        return GridMode.Table;
    
    // Image-like objects → Gallery
    if (HasProperties(itemType, "Url", "Thumbnail", "Alt"))
        return GridMode.Gallery;
        
    // Task-like objects → Kanban
    if (HasProperties(itemType, "Status", "Priority", "Assignee"))
        return GridMode.Kanban;
        
    // Event-like objects → Timeline
    if (HasProperties(itemType, "StartDate", "EndDate", "Title"))
        return GridMode.Timeline;
        
    // Hierarchical data → Tree
    if (HasProperties(itemType, "Children", "ParentId"))
        return GridMode.Tree;
        
    // Complex objects → Cards
    if (PropertyCount(itemType) > 3)
        return GridMode.Cards;
        
    // Simple objects → List
    return GridMode.List;
}
```

## Enterprise Features

### 1. Virtualization (100k+ Items)
```razor
<RGrid DataSource="@largeDataset" 
       EnableVirtualization="true"
       VirtualizationThreshold="50"
       OverscanCount="5" />
```

### 2. Drag & Drop with Auto-Save
```razor
<RGrid DataSource="@tasks"
       Mode="GridMode.Kanban"
       EnableDragDrop="true"
       OnItemReordered="@SaveReorder"
       DragDropMode="DragDropMode.Between" />
```

### 3. Real-Time Updates via SignalR
```razor
<RGrid DataSource="@liveData"
       EnableRealTimeUpdates="true"
       UpdateChannel="@($"grid-{companyId}")"
       OnDataUpdated="@HandleLiveUpdate" />
```

### 4. Advanced Column Features
```csharp
var columns = new List<GridColumn>
{
    new GridColumn("Name") 
    { 
        Sortable = true,
        Filterable = true,
        Resizable = true,
        Frozen = true,
        MinWidth = "100px",
        MaxWidth = "300px"
    },
    new GridColumn("Amount")
    {
        Template = RTemplates.Currency(),
        Align = ColumnAlign.Right,
        Aggregation = AggregationType.Sum,
        FooterTemplate = @<text>Total: @context</text>
    }
};
```

### 5. Multi-Level Grouping
```razor
<RGrid DataSource="@sales"
       GroupBy="@(new[] { "Region", "Product", "Quarter" })"
       ShowGroupFooter="true"
       GroupExpanded="true" />
```

### 6. Export Capabilities
```razor
<RGrid DataSource="@data"
       EnableExport="true"
       ExportFormats="@(new[] { ExportFormat.Excel, ExportFormat.CSV, ExportFormat.PDF })"
       OnExport="@HandleExport" />
```

## RFilter Integration

### Header Integration
```razor
<RGrid DataSource="@employees">
    <FilterContent>
        <RFilterGeneric TItem="Employee" 
                       OnPredicateChanged="@ApplyFilter"
                       Config="@(new() { ShowSearch = true, ShowDateRange = true })" />
    </FilterContent>
</RGrid>
```

### Inline Column Filters
```razor
<RGrid DataSource="@products"
       EnableColumnFiltering="true"
       FilterMode="FilterMode.Menu">
    <Columns>
        <GridColumn Field="Name" FilterType="FilterType.Text" />
        <GridColumn Field="Price" FilterType="FilterType.Number" />
        <GridColumn Field="Category" FilterType="FilterType.Select" />
    </Columns>
</RGrid>
```

### External Filter Binding
```razor
<RFilterGeneric @ref="filter" TItem="Order" />
<RGrid DataSource="@FilteredOrders" 
       FilterPredicate="@filter.CurrentPredicate" />

@code {
    IEnumerable<Order> FilteredOrders => 
        orders.Where(filter.CurrentPredicate?.Compile() ?? (o => true));
}
```

## Performance Optimizations

### 1. Virtual Scrolling
- Only renders visible rows + overscan
- Supports variable row heights
- Smooth scrolling with 60fps

### 2. Column Virtualization
- Horizontal virtualization for 100+ columns
- Frozen columns always visible
- Smart column measurement

### 3. Lazy Loading
```razor
<RGrid DataSource="@LoadData"
       LoadMode="LoadMode.OnDemand"
       PageSize="50"
       OnLoadMore="@LoadMoreData" />
```

### 4. Debounced Operations
- Search: 300ms debounce
- Resize: 100ms throttle
- Scroll: RequestAnimationFrame

## Responsive Design

### Breakpoint System
```razor
<RGrid DataSource="@items"
       ColumnsXs="1"    @* Mobile: 1 column *@
       ColumnsSm="2"    @* Tablet: 2 columns *@
       ColumnsMd="3"    @* Desktop: 3 columns *@
       ColumnsLg="4"    @* Large: 4 columns *@
       ColumnsXl="6" /> @* XL: 6 columns *@
```

### Adaptive Modes
```razor
<RGrid DataSource="@data"
       ModeXs="GridMode.List"      @* Mobile: List view *@
       ModeMd="GridMode.Cards"     @* Tablet: Cards *@
       ModeLg="GridMode.Table" />  @* Desktop: Table *@
```

## Customization

### Custom Item Templates
```razor
<RGrid DataSource="@products" Mode="GridMode.Cards">
    <ItemTemplate Context="product">
        <RCard Title="@product.Name"
               Subtitle="@product.Category"
               Image="@product.ImageUrl">
            <div class="pa-4">
                <RBadge Text="@($"${product.Price}")" 
                        Variant="BadgeVariant.Success" />
            </div>
        </RCard>
    </ItemTemplate>
</RGrid>
```

### Custom Cell Renderers
```razor
<GridColumn Field="Status">
    <CellTemplate Context="item">
        @switch(item.Status)
        {
            case "Active":
                <RBadge Text="Active" Variant="BadgeVariant.Success" />
                break;
            case "Pending":
                <RBadge Text="Pending" Variant="BadgeVariant.Warning" />
                break;
            default:
                <RBadge Text="@item.Status" />
                break;
        }
    </CellTemplate>
</GridColumn>
```

### Custom Empty State
```razor
<RGrid DataSource="@items">
    <EmptyTemplate>
        <REmptyState Icon="inbox"
                     Title="No items found"
                     Description="Try adjusting your filters">
            <RButton Text="Clear Filters" OnClick="@ClearFilters" />
        </REmptyState>
    </EmptyTemplate>
</RGrid>
```

## Advanced Scenarios

### Master-Detail Grid
```razor
<RGrid DataSource="@orders"
       EnableMasterDetail="true">
    <MasterDetailTemplate Context="order">
        <RGrid DataSource="@order.LineItems" 
               Mode="GridMode.Table" />
    </MasterDetailTemplate>
</RGrid>
```

### Infinite Scroll with Virtualization
```razor
<RGrid DataSource="@items"
       LoadMode="LoadMode.Infinite"
       VirtualizationThreshold="100"
       OnReachEnd="@LoadMoreItems"
       ShowLoadingIndicator="true" />
```

### Cell Editing
```razor
<RGrid DataSource="@data"
       EditMode="EditMode.Cell"
       OnCellEdit="@SaveCell">
    <Columns>
        <GridColumn Field="Name" Editable="true" Editor="@typeof(RTextInput)" />
        <GridColumn Field="Price" Editable="true" Editor="@typeof(RNumberInput)" />
    </Columns>
</RGrid>
```

## Components & Patterns to Reuse

### Core Components
1. **RVirtualListGeneric** - Virtualization with buffer management
2. **RPaginationFooter** - Complete pagination solution  
3. **RSkeleton** - Smart loading states with auto-detection
4. **REmptyState** - Professional empty states
5. **RTableColumnManager** - Drag-drop column management
6. **RExportButton/Modal** - Export functionality
7. **Universal Template System** - Badge, Currency, Avatar templates

### Base Classes
- **RComponentBase** - JavaScript interop, density, elevation
- **ColumnDefinition<T>** - Column model with templates
- **TableContext** - Type-safe cascading context

### Performance Patterns
```csharp
// From existing components
private static readonly ConcurrentDictionary<string, Func<TItem, object>> _propertyGetterCache = new();
private TimeSpan _lastRenderTime;
private GridPerformanceMetrics _metrics;
```

### Selection Pattern (from RTableVirtualized)
```razor
@if (MultiSelect)
{
    <RCheckbox Checked="@IsSelected(item)" 
               OnChange="@((bool value) => OnSelectionChanged(item, value))" />
}
else
{
    <RRadio Name="@($"grid-{GridId}")"
            Checked="@IsSelected(item)" />
}
```

### Drag-Drop Pattern (from RTableColumnManager)
```razor
draggable="@EnableReordering.ToString().ToLower()"
@ondragstart="@(() => OnDragStart(column))"
@ondragend="@OnDragEnd"
@ondragenter="@(() => OnDragEnter(column))"
@ondragover="@((e) => OnDragOver(e, column))"
@ondrop="@(() => OnDrop(column))"
```

## CSS Architecture (Use _grid.scss Only)

### Reuse Existing _grid.scss Classes
```scss
// From RR.Blazor\Styles\components\_grid.scss
.grid {
    // Density variants
    &-density-compact { }
    &-density-normal { }
    &-density-spacious { }
    
    // Visual modifiers
    &-striped { }
    &-bordered { }
    &-hover { }
    
    // Layout components
    &-header { }
    &-layout { }  // CSS Grid system
    &-header-cell { }
    &-row { }
    &-cell { }
}

// Grid layout system (already exists)
.grid-layout {
    display: grid;
    grid-auto-flow: row dense;
    // Already handles CSS Grid properly
}
```

### Animation Reuse (from _animations.scss)
```scss
// Available animations to use
@keyframes fadeIn { }
@keyframes scaleIn { }
@keyframes slideUp { }
@keyframes shimmer { }  // For loading states
@keyframes pulse { }    // For skeleton loading
@keyframes float { }    // For hover effects
@keyframes dropdownSlideIn { }  // For filter dropdowns
```

## Migration from Current Implementation

### Current Issues to Fix:
1. ✅ Remove double-wrapping (grid-container + grid-layout)
2. ✅ Fix CSS Grid column generation
3. ✅ Fix primitive type handling
4. ✅ Proper column value extraction

### Migration Steps:
1. Keep RGridGeneric.razor as-is (fix rendering issues)
2. Update RGrid.cs to follow smart wrapper pattern
3. Remove RDataGrid references (not needed)
4. Fix CSS Grid implementation
5. Add RFilter integration points

## Testing Requirements

### Unit Tests:
- Type detection for all data types
- Parameter forwarding validation
- Mode auto-detection logic
- Event callback propagation

### Integration Tests:
- All grid modes rendering
- Filter integration
- Virtualization performance
- Drag-drop functionality
- Real-time updates

### E2E Tests:
- Large dataset handling (100k+ items)
- Responsive behavior
- Export functionality
- Accessibility compliance

## Accessibility

### WCAG 2.1 AA Compliance:
- Proper ARIA labels for grid roles
- Keyboard navigation (Arrow keys, Tab, Enter)
- Screen reader announcements
- Focus management
- High contrast support

### Keyboard Shortcuts:
- `Arrow Keys`: Navigate cells
- `Space`: Select item
- `Enter`: Edit cell/Activate item
- `Ctrl+A`: Select all
- `Escape`: Cancel edit/Clear selection

## Performance Targets

- Initial render: < 100ms for 1000 items
- Scroll FPS: 60fps with virtualization
- Filter apply: < 50ms for 10k items
- Sort: < 100ms for 10k items
- Export: < 2s for 50k items

## Conclusion

RGrid is the single, unified solution for ALL grid display needs in enterprise applications. By following the established smart component pattern (RGrid/RGridGeneric) and supporting multiple display modes, it eliminates the need for separate components while maintaining S-tier performance and customization.