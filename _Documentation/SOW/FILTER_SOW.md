# RR.Blazor Universal Filter System - Statement of Work

## Executive Summary
A single, smart RFilter component that provides enterprise S-tier filtering capabilities for all RR.Blazor data views. Two primary integration modes: standalone filter controlling data views via reference, and integrated compact filters within component headers.

## Core Integration Patterns

### 1. Case 1: Standalone Filter (External Control)
Filter exists as a separate component and controls data views through reference binding.

```razor
@* Standalone filter controls table via Filter parameter *@
<RFilter @ref="myFilter" />
<RTable Items="@employees" Filter="@myFilter" />

@* One filter controls multiple views *@
<RFilter @ref="masterFilter" />
<RTable Items="@employees" Filter="@masterFilter" />
<RGrid Items="@employees" Filter="@masterFilter" />
```

**Key Points:**
- Filter is completely separate from data view
- Data view accepts filter via `Filter` parameter
- Filter sends predicates to linked components
- Multiple components can share one filter

### 2. Case 2: Integrated Column Filters (Built-in)
Tiny filter icons integrated directly into table/grid headers, next to sort chevrons.

```razor
@* Default: Tiny filter icon in each column header *@
<RTable Items="@employees" EnableColumnFilters="true" />

@* Optional: Add [Advanced] button to top toolbar *@
<RTable Items="@employees" 
        EnableColumnFilters="true" 
        ShowAdvancedFilter="true" />
```

**Visual Implementation:**
```
Column Header (Default):
┌─────────────────────────┐
│ Name ↕ 🔽              │  <- Sort chevron + tiny filter icon
└─────────────────────────┘

When clicked - compact dropdown:
┌─────────────────────────┐
│ 🔍 Search...           │
├─────────────────────────┤
│ ☑ John Smith          │
│ ☑ Jane Doe            │
│ ☐ Bob Johnson         │
│ [Select All] [Clear]   │
└─────────────────────────┘
```

**Key Points:**
- Filter icon is TINY, next to sort chevron
- No bulky UI elements under columns
- Clicking opens compact dropdown
- Optional [Advanced] button in toolbar for complex filters

## Component-Specific Integration

### 3. RTable Integration

#### 3.1 Default Column Filter Behavior
- **Location**: Tiny icon in column header, next to sort chevron
- **Size**: 14px icon, subtle until hovered
- **UI**: Compact dropdown on click

#### 3.2 Table Parameters
```csharp
// Core filter parameters for RTable
[Parameter] public RFilter? Filter { get; set; }  // Case 1: External filter
[Parameter] public bool EnableColumnFilters { get; set; } = false;  // Case 2: Built-in
[Parameter] public bool ShowAdvancedFilter { get; set; } = false;  // Shows [Advanced] in toolbar
[Parameter] public bool ShowFilterBadges { get; set; } = true;  // Shows count badges
[Parameter] public FilterIconPosition IconPosition { get; set; } = FilterIconPosition.AfterSort;
```

#### 3.3 Visual Specifications

**Header with Filter (Compact Mode):**
```html
<th class="table-header">
    <div class="table-header-content">
        <span class="table-header-title">Column Name</span>
        <button class="table-header-sort">↕</button>
        <button class="table-header-filter">▽</button>  <!-- TINY 14px -->
    </div>
</th>
```

**CSS Implementation:**
```scss
.table-header-filter {
    width: 14px;
    height: 14px;
    opacity: 0.3;
    margin-left: 4px;
    
    &:hover {
        opacity: 0.7;
    }
    
    &.active {
        opacity: 1;
        color: var(--primary);
    }
}

```

### 4. RGrid Integration

#### 4.1 Grid Filter Behavior
Since RGrid has no explicit columns/rows, filters are always integrated in the top toolbar.

```razor
@* Case 1: External filter control *@
<RFilter @ref="gridFilter" />
<RGrid Items="@employees" Filter="@gridFilter" />

@* Case 3: Integrated toolbar filter *@
<RGrid Items="@employees" EnableFilter="true" />
```

**Visual Implementation:**
```
Grid Toolbar (with integrated filter):
┌────────────────────────────────────────────┐
│ Grid Title            [] [Filter] [⚙]    │  <- Compact filter in toolbar
└────────────────────────────────────────────┘
│ ┌─────┐ ┌─────┐ ┌─────┐ ┌─────┐          │
│ │     │ │     │ │     │ │     │          │  <- Grid items
│ └─────┘ └─────┘ └─────┘ └─────┘          │
```

**Key Points:**
- Filter always appears in grid toolbar/title bar
- No column-specific filters (grid has no columns)
- Quick search + filter button combo
- Filters apply to entire grid data

#### 4.2 Grid Parameters
```csharp
[Parameter] public RFilter? Filter { get; set; }  // Case 1: External filter
[Parameter] public bool EnableFilter { get; set; } = false;  // Case 3: Toolbar filter
[Parameter] public bool ShowQuickSearch { get; set; } = true;  // Search box in toolbar
[Parameter] public FilterPosition Position { get; set; } = FilterPosition.ToolbarRight;
```

## Additional Integration Cases - Questions for Clarification

### 5. Case 4: RChart Filter Integration
**Question**: For charts, should filters:
a) Appear as overlay controls on the chart itself?
b) Be in a sidebar panel next to the chart?
c) Only work via external RFilter reference?

```razor
@* Proposed: Chart with filter sidebar *@
<RChart Data="@salesData" EnableFilterPanel="true" />

@* Or overlay controls? *@
<RChart Data="@salesData" EnableOverlayFilters="true" />
```

### 6. Case 5: RCard/RList Filter Integration  
**Question**: For card layouts and lists, should we:
a) Add a filter toolbar above the cards?
b) Support filter chips that float above content?
c) Only external filter reference?

```razor
@* Proposed: Cards with filter bar *@
<RCardGrid Items="@products" EnableFilterBar="true" />

@* Or floating filter chips? *@
<RList Items="@items" ShowFilterChips="true" />
```

### 7. Case 6: Master-Detail Filtering
**Question**: When we have master-detail views, should:
a) Master filter automatically cascade to detail?
b) Each have independent filters?
c) Support both modes via parameter?

```razor
@* Proposed: Cascading filters *@
<RMasterDetail Items="@orders" CascadeFilters="true">
    <MasterTemplate>
        <RTable Items="@context.Orders" />  @* Inherits filter *@
    </MasterTemplate>
    <DetailTemplate>
        <RTable Items="@context.OrderItems" />  @* Inherits + extends filter *@
    </DetailTemplate>
</RMasterDetail>
```

### 8. Case 7: RTreeView/Hierarchical Filtering
**Question**: For tree structures, should filters:
a) Hide entire branches if parent doesn't match?
b) Show parent if any child matches?
c) Flatten filtered results?

```razor
@* Proposed options *@
<RTreeView Items="@categories" 
           EnableFilter="true"
           FilterMode="FilterMode.PreserveHierarchy" />  @* or Flatten, or ParentChild *@
```

### 9. Case 8: RKanban Board Filtering
**Question**: For kanban boards, should filters:
a) Hide entire columns if empty after filtering?
b) Keep columns but show "No items" message?
c) Highlight filtered cards differently?

```razor
@* Proposed *@
<RKanban Items="@tasks" 
         EnableFilter="true"
         EmptyColumnBehavior="EmptyColumnBehavior.Hide" />
```

### 10. Case 9: Quick Filter Presets
**Question**: Should we support quick filter presets/templates that appear as buttons?

```razor
@* Proposed: Quick filter buttons *@
<RTable Items="@invoices">
    <QuickFilters>
        <FilterPreset Name="Overdue" Predicate="@(x => x.DueDate < DateTime.Now)" />
        <FilterPreset Name="High Value" Predicate="@(x => x.Amount > 10000)" />
        <FilterPreset Name="This Month" Predicate="@(x => x.Date.Month == DateTime.Now.Month)" />
    </QuickFilters>
</RTable>
```

### 11. Case 10: Inline Edit Mode with Filters
**Question**: When table is in edit mode, should:
a) Filters be disabled during editing?
b) Filters remain active but only on non-editing rows?
c) Have separate "edit filter" to show only rows being edited?

### 12. Case 11: Virtual Scrolling with Filters
**Question**: For virtualized tables with 10K+ rows:
a) Should filter counts show immediately or calculate async?
b) Should we pre-index common filter fields for performance?
c) Show a "Filtering..." progress for large datasets?

### 13. Case 12: Cross-Component Filter Sync
**Question**: When multiple components show same data differently:
a) Should they share filter state automatically if same data source?
b) Require explicit filter sharing via reference?
c) Support "filter groups" where components opt-in to sync?

```razor
@* Proposed: Filter group *@
<RFilterGroup @ref="dashboardFilters">
    <RTable Items="@employees" FilterGroup="dashboardFilters" />
    <RGrid Items="@employees" FilterGroup="dashboardFilters" />
    <RChart Data="@employees" FilterGroup="dashboardFilters" />
</RFilterGroup>
```

### 14. Implementation Priority Questions:

1. **Default Behavior**: Should `EnableColumnFilters` be true or false by default for tables?

2. **Icon Choice**: Should filter icon be:
   - Funnel (▽)  
   - Three lines with circles (☰)
   - Down arrow (▼)
   - Custom per theme?

3. **Dropdown Position**: Should filter dropdown:
   - Always drop down?
   - Smart position (up if near bottom)?  
   - Align left or right with column?

4. **Multi-column Filter Logic**: When multiple columns filtered:
   - Always AND between columns?
   - Allow OR option?
   - Let developer configure?

5. **Performance Threshold**: At what data size should we:
   - Switch to virtual filtering?
   - Show progress indicators?
   - Defer filter calculation?

6. **Mobile Behavior**: On mobile devices, should column filters:
   - Open as full-screen modal?
   - Stay as dropdown?
   - Switch to horizontal scroll panel?

7. **Keyboard Shortcuts**: Should we support:
   - Ctrl+Shift+F to open filter?
   - Escape to close?
   - Enter to apply?
   - Tab through filter options?

8. **Filter Persistence**: Should active filters:
   - Save to localStorage automatically?
   - Require explicit save action?
   - Never persist?

9. **Empty State**: When filter returns no results:
   - Show "No results" message?
   - Auto-suggest broadening filter?
   - Show button to clear filters?

10. **Export Behavior**: When exporting filtered data:
    - Export only filtered rows?
    - Export all with filter indication?
    - Ask user preference?
        <RTableColumn Field="Id" Filterable="false" />  @* No filter for ID *@
    </Columns>
</RTable>
```

### 4. RGrid Integration (Row/Column Filters)

#### 4.1 Grid Filter Modes
- **Row Filters**: Filter entire rows based on criteria
- **Column Filters**: Filter columns (hide/show columns)
- **Cell Filters**: Filter individual cells (advanced mode)
- **Area Filters**: Filter rectangular regions

#### 4.2 Grid Configuration
```csharp
public class RGridConfiguration
{
    public GridFilterMode FilterMode { get; set; } = GridFilterMode.Row;
    public bool EnableQuickFilter { get; set; } = true;
    public bool EnableAdvancedFilter { get; set; } = false;
    public FilterAnimation FilterAnimation { get; set; } = FilterAnimation.Fade;
}
```

#### 4.3 Grid Filter UI
```razor
<RGrid Items="@data" BindFilter="@gridFilter">
    <GridFilters>
        <RowFilter>
            <RFilter Mode="FilterMode.Row" />
        </RowFilter>
        <ColumnFilter>
            <RFilter Mode="FilterMode.Column" />
        </ColumnFilter>
    </GridFilters>
</RGrid>
```

### 5. RChart Integration (Data Rebuild)

#### 5.1 Chart Filter Behavior
- **Data Filtering**: Filters underlying data before chart render
- **Series Filtering**: Show/hide specific series
- **Range Filtering**: Zoom to filtered date/value ranges
- **Category Filtering**: Show/hide categories
- **Live Update**: Chart animates as filter changes

#### 5.2 Chart Configuration
```csharp
public class RChartConfiguration
{
    public bool EnableDataFilter { get; set; } = true;
    public bool RebuildOnFilter { get; set; } = true;
    public ChartFilterPosition FilterPosition { get; set; } = ChartFilterPosition.Top;
    public bool AnimateFilterChanges { get; set; } = true;
}
```

#### 5.3 Chart Filter Example
```razor
<RChart Data="@salesData" Type="ChartType.Line" BindFilter="@chartFilter">
    <ChartFilter>
        <RFilter Mode="FilterMode.Range" Fields="@(new[] { "Date", "Amount" })" />
    </ChartFilter>
</RChart>

@code {
    // Chart automatically rebuilds when filter changes
    private void OnChartFilterChanged(FilterState state)
    {
        // Chart handles this internally via BindFilter
    }
}
```

## Base Class Integration

### 6. RComponentBase Filter Support

#### 6.1 Base Properties (Available to ALL RR.Blazor Components)
```csharp
public abstract class RComponentBase : ComponentBase
{
    // Filter binding - available in all components
    [Parameter] public RFilter BindFilter { get; set; }
    [Parameter] public FilterConfiguration FilterConfig { get; set; }
    [Parameter] public EventCallback<FilterState> OnFilterChanged { get; set; }
    
    // Global density control affects filters too
    [Parameter] public ComponentDensity Density { get; set; } = ComponentDensity.Normal;
    
    // Filter visibility control
    [Parameter] public bool ShowFilter { get; set; } = true;
    [Parameter] public FilterPosition FilterPosition { get; set; } = FilterPosition.Auto;
}
```

#### 6.2 Density Impact on Filters
```csharp
public enum ComponentDensity
{
    Compact,    // Smaller filter inputs, less padding
    Normal,     // Default filter size
    Comfortable // Larger touch targets, more spacing
}

// Filter adapts to component density
private string GetFilterClass() => Density switch
{
    ComponentDensity.Compact => "filter-compact",
    ComponentDensity.Comfortable => "filter-comfortable",
    _ => "filter-normal"
};
```

### 7. Filter State Management

#### 7.1 FilterState Class
```csharp
public class FilterState
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public Dictionary<string, FilterCriteria> Filters { get; set; } = new();
    public string QuickSearch { get; set; }
    public bool IsActive => Filters.Any() || !string.IsNullOrEmpty(QuickSearch);
    public DateTime LastModified { get; set; }
    
    // Methods for state manipulation
    public void AddFilter(string field, FilterOperator op, object value) { }
    public void RemoveFilter(string field) { }
    public void Clear() { }
    public FilterState Clone() { }
}
```

#### 7.2 Filter Persistence
```csharp
public interface IFilterPersistence
{
    Task SaveFilterState(string key, FilterState state);
    Task<FilterState> LoadFilterState(string key);
    Task<List<FilterState>> GetSavedFilters(string componentId);
}
```

### 8. Custom Component Integration

#### 8.1 Simple Integration (No Interface Required)
```razor
@* Any component with Items property works automatically *@
<MyCustomList Items="@data" BindFilter="@filter" />
<RFilter @ref="filter" />
```

#### 8.2 Advanced Integration (Optional Interface)
```csharp
// Optional interface for advanced scenarios
public interface IFilterable
{
    IEnumerable<object> GetFilterableData();
    void ApplyFilter(FilterState state);
    IEnumerable<FilterField> GetFilterableFields();
}

// Custom implementation
public class MyDashboard : RComponentBase, IFilterable
{
    public IEnumerable<object> GetFilterableData() => AllWidgetData;
    
    public void ApplyFilter(FilterState state)
    {
        // Custom filter logic for dashboard
        foreach (var widget in Widgets)
        {
            widget.Filter(state);
        }
    }
}
```

### 9. Enterprise Customization

#### 9.1 Filter Templates
```csharp
public class FilterTemplate
{
    public string Name { get; set; }
    public string Description { get; set; }
    public FilterState State { get; set; }
    public bool IsDefault { get; set; }
    public string CreatedBy { get; set; }
    public DateTime Created { get; set; }
}
```

#### 9.2 Custom Operators
```csharp
// Extensible operator system
public class CustomOperator : FilterOperator
{
    public string Name { get; set; }
    public Func<object, object, bool> Evaluate { get; set; }
    public string DisplayText { get; set; }
}

// Register custom operators
FilterRegistry.RegisterOperator(new CustomOperator
{
    Name = "IsWeekend",
    Evaluate = (value, _) => ((DateTime)value).DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday,
    DisplayText = "Is Weekend"
});
```

#### 9.3 Filter Plugins
```csharp
public abstract class FilterPlugin
{
    public abstract string Name { get; }
    public abstract Type[] SupportedTypes { get; }
    public abstract RenderFragment RenderUI(FilterContext context);
    public abstract Expression<Func<T, bool>> BuildExpression<T>(FilterCriteria criteria);
}
```

## API & Events

### 10. Full Control API

#### 10.1 RFilter Public API
```csharp
public class RFilter : RComponentBase
{
    // Properties
    public FilterState State { get; }
    public bool HasActiveFilters { get; }
    public int FilteredCount { get; }
    public int TotalCount { get; }
    
    // Methods
    public Task ApplyFilter(string field, FilterOperator op, object value);
    public Task RemoveFilter(string field);
    public Task ClearFilters();
    public Task LoadTemplate(FilterTemplate template);
    public Task SaveTemplate(string name);
    public Task<IEnumerable<T>> GetFilteredData<T>();
    
    // Events
    [Parameter] public EventCallback<FilterState> OnFilterApplied { get; set; }
    [Parameter] public EventCallback<FilterState> OnFilterCleared { get; set; }
    [Parameter] public EventCallback<FilterChangedEventArgs> OnFilterChanged { get; set; }
    [Parameter] public EventCallback<FilterTemplate> OnTemplateSaved { get; set; }
    [Parameter] public EventCallback<FilterTemplate> OnTemplateLoaded { get; set; }
}
```

#### 10.2 Dashboard Example
```razor
@page "/dashboard"

<RFilter @ref="dashboardFilter" OnFilterApplied="OnDashboardFiltered">
    <QuickFilters>
        <FilterButton Text="Today" Filter="@TodayFilter" />
        <FilterButton Text="This Month" Filter="@MonthFilter" />
        <FilterButton Text="High Priority" Filter="@PriorityFilter" />
    </QuickFilters>
</RFilter>

<div class="dashboard-widgets">
    @foreach (var widget in Widgets)
    {
        <DashboardWidget Data="@GetFilteredData(widget.DataSource)" />
    }
</div>

@code {
    private RFilter dashboardFilter;
    
    private async Task OnDashboardFiltered(FilterState state)
    {
        // All widgets automatically update via data binding
        await RefreshDashboard(state);
    }
    
    private IEnumerable<object> GetFilteredData(IEnumerable<object> source)
    {
        return dashboardFilter?.GetFilteredData(source) ?? source;
    }
}
```

## Implementation Architecture

### 11. No Overengineering Approach

#### 11.1 What We DON'T Need
-  Source generation
-  Complex expression trees for simple filters
-  Multiple filter service layers
-  Separate filter components for each data type
-  Required interfaces for basic usage

#### 11.2 What We DO Need
-  Single RFilter.razor component
-  Simple LINQ for filtering
-  Property detection via reflection (cached)
-  Optional interfaces for advanced scenarios
-  Event-driven architecture

#### 11.3 Simple Implementation
```csharp
// Core filtering - just LINQ, no magic
private IEnumerable<T> ApplyFilters<T>(IEnumerable<T> items)
{
    var result = items;
    
    foreach (var filter in State.Filters)
    {
        result = result.Where(item => EvaluateFilter(item, filter));
    }
    
    return result;
}

private bool EvaluateFilter<T>(T item, FilterCriteria filter)
{
    var value = GetPropertyValue(item, filter.Field);
    
    return filter.Operator switch
    {
        FilterOperator.Equals => Equals(value, filter.Value),
        FilterOperator.Contains => value?.ToString().Contains(filter.Value.ToString()) ?? false,
        FilterOperator.GreaterThan => Comparer.Default.Compare(value, filter.Value) > 0,
        _ => true
    };
}
```

## Requirements

### Architecture Decisions:
1. **Filter Data Flow**:  Predicates only - no data copies, components apply filtering
2. **State Management**:  Off by default, configurable via parameter
3. **Multi-Filter Coordination**:  Master-detail pattern for coordination

### Table-Specific Decisions:
4. **Column Filter UI**:  "Most modern 2025 and most dense and sexy way possible"
5. **Filter Combination Logic**:  Configurable AND/OR logic
6. **Null Handling**:  Separate "Is Empty" operator for nulls

### Grid-Specific Decisions:
7. **Grid Filter Granularity**:  Support row/column/cell filtering
8. **Grid Performance**:  Virtual filtering for 10K+ items
9. **Grid Grouping**:  Filters should respect and modify grouping

### Chart-Specific Decisions:
10. **Chart Rebuild Strategy**:  Animated transitions required
11. **Series Filtering**:  Support both data points and series filtering
12. **Time Series**:  Special handling for time-based charts

### Enterprise Features Decisions:
13. **Filter Templates**:  Browser localStorage, shareable via export/import
14. **Custom Operators**:  Extensible via interfaces
15. **Performance Thresholds**:  10K+ items triggers virtual filtering
16. **Audit Trail**:  Optional via event system

### API Design Decisions:
17. **Async vs Sync**:  Sync by default for performance
18. **Validation**:  Basic type validation only
19. **Error Handling**:  Graceful degradation, log errors
20. **Bulk Operations**:  Atomic bulk filter operations supported

## Visual Design Specifications

### 1. Column Filter Visual (Tables/Grids)

#### 1.1 Filter Icon State
```scss
// Default state - no filter active
.filter-icon {
  opacity: 0.3;
  transition: all 150ms ease;
  
  &:hover {
    opacity: 0.7;
  }
}

// Active filter state
.filter-icon--active {
  opacity: 1;
  color: var(--primary);
  
  &::after {
    content: attr(data-count); // Shows count badge
    position: absolute;
    top: -4px;
    right: -4px;
    font-size: 9px;
    background: var(--primary);
    color: var(--on-primary);
    border-radius: 10px;
    padding: 1px 4px;
  }
}
```

#### 1.2 Filter Dropdown UI 
```
┌──────────────────────────┐
│  Quick search...       │  <- Instant search within values
├──────────────────────────┤
│  Quick Actions         │
│ ┌──────────────────────┐ │
│ │ [All] [None] [Invert]│ │  <- One-click actions
│ └──────────────────────┘ │
├──────────────────────────┤
│  Values (247)          │  <- Count indicator
│ ┌──────────────────────┐ │
│ │ ☑ Sales (45)         │ │  <- Checkbox + value + count
│ │ ☑ Marketing (23)     │ │
│ │ ☐ Engineering (67)   │ │
│ │ ☑ HR (12)            │ │
│ │ ... scroll for more  │ │  <- Virtual scroll for 100+ items
│ └──────────────────────┘ │
├──────────────────────────┤
│  Advanced              │  <- Collapsible section
│ ├─ Operator: [Contains▼]│ │
│ ├─ Match: [Any ▼]       │ │
│ └─ Case: [] Sensitive  │ │
├──────────────────────────┤
│ [Clear] [Cancel] [Apply] │  <- Action buttons
└──────────────────────────┘
```

#### 1.3 Inline Filter Mode (Alternative)
```
Column Header
[ Type to filter...] [▼]  <- Inline input + dropdown for operators
```

### 2. Standalone Filter Visual

#### 2.1 Compact Mode (Default)
```
┌─────────────────────────────────────────────┐
│  Search all fields... [Today][Priority][+]│  <- Search + quick filters
└─────────────────────────────────────────────┘
```

#### 2.2 Expanded Mode
```
┌──────────────────────────────────────────────────┐
│  Search all fields...              [Clear All] │
├──────────────────────────────────────────────────┤
│ Quick Filters:                                   │
│ [ Today] [ High Priority] [Open] [Urgent]     │
├──────────────────────────────────────────────────┤
│ Active Filters (3):                              │
│ [Department = Sales ×] [Status != Closed ×]      │  <- Chips with remove
│ [Amount > 1000 ×]                                │
├──────────────────────────────────────────────────┤
│ + Add Filter  ⚙ Save Template  📥 Import         │
└──────────────────────────────────────────────────┘
```

### 3. Grid Filter Visual (Cell-Level)

#### 3.1 Cell Filter Indicator
```
┌─────────┐
│ $1,234  │  <- Normal cell
└─────────┘

┌─────────┐
│ $1,234 🔸│  <- Filtered cell (subtle indicator)
└─────────┘
```

#### 3.2 Grid Filter Panel (Floating)
```
┌────────────────────────┐
│ Grid Filters           │
├────────────────────────┤
│ Row Filters:     [3]   │
│ Column Filters:  [1]   │
│ Cell Filters:    [12]  │
├────────────────────────┤
│ [Edit] [Clear] [Save]  │
└────────────────────────┘
```

### 4. Chart Filter Visual

#### 4.1 Chart Filter Controls
```
┌─────────────────────────────────────┐
│  Chart Filters         [Reset]    │
├─────────────────────────────────────┤
│ Date Range:                         │
│ [Jan 1, 2024] to [Dec 31, 2024]    │
├─────────────────────────────────────┤
│ Series:                              │
│ ☑ Revenue  ☑ Costs  ☐ Profit       │
├─────────────────────────────────────┤
│ Categories:                          │
│ ☑ Q1  ☑ Q2  ☑ Q3  ☑ Q4            │
└─────────────────────────────────────┘
```

### 5. Animation Specifications

#### 5.1 Filter Application
- **Fade out**: 150ms ease-out (filtered out items)
- **Slide up**: 200ms ease-in-out (remaining items reposition)
- **Highlight**: 300ms pulse on newly filtered items

#### 5.2 Dropdown Animations
- **Open**: 200ms slide-down + fade-in
- **Close**: 150ms slide-up + fade-out
- **Item hover**: 50ms background transition

#### 5.3 Chart Transitions
- **Data update**: 400ms smooth morph
- **Series toggle**: 300ms fade + scale
- **Zoom**: 250ms ease-in-out

### 6. Density Variations

#### 6.1 Compact Density
- Row height: 32px
- Font size: 12px
- Padding: 4px 8px
- Icon size: 14px

#### 6.2 Normal Density
- Row height: 40px
- Font size: 14px
- Padding: 8px 12px
- Icon size: 16px

#### 6.3 Comfortable Density
- Row height: 48px
- Font size: 16px
- Padding: 12px 16px
- Icon size: 20px

### 7. Mobile Responsive

#### 7.1 Mobile Filter Drawer
```
Full screen drawer from bottom:
┌─────────────────────┐
│ ━━━━━               │  <- Drag handle
│ Filters        Done │
├─────────────────────┤
│ [Full filter UI]    │
│                     │
│                     │
└─────────────────────┘
```

#### 7.2 Touch Optimizations
- Minimum touch target: 44x44px
- Swipe to dismiss
- Haptic feedback on selection
- Momentum scrolling

### 8. Accessibility

- **Keyboard**: Full keyboard navigation
- **ARIA**: Proper labels and roles
- **Focus**: Visible focus indicators
- **Announce**: Screen reader announcements
- **Contrast**: WCAG AAA compliance

### 9. Performance Indicators

#### 9.1 Loading States
```
[ Filtering...] - During filter application
[ 1,234 results] - After completion
[⚠ No results]    - Empty state
```

#### 9.2 Virtual Scrolling
- Render only visible + buffer items
- Smooth scroll with momentum
- Position restoration on filter change

## Success Metrics

1. **Single File**: Core logic in one RFilter.razor file
2. **Zero Dependencies**: Works without requiring interfaces
3. **Performance**: <50ms filter time for 10K items
4. **Integration**: Works with any component that has Items property
5. **Customization**: S-tier enterprise features via optional extensions
6. **SOLID**: Complete separation of concerns
7. **Developer Experience**: One-line integration for basic usage

## Non-Goals

-  No GraphQL/OData filter generation
-  No server-side filter execution (client-side only)
-  No automatic SQL generation
-  No required base class inheritance
-  No complex state management library

## Deliverables

1. **RFilter.razor** - Single smart component
2. **FilterState.cs** - Simple state model (already exists in FilterModels.cs)
3. **filter.scss** - Minimal styling (not _rfilter.scss)
4. **filter_readme.md** - Documentation on filter usage and binding

## Implementation Clarifications

### Final Implementation Decisions:

1. **Filter Dropdown Positioning**: 
    Auto-reposition to stay in viewport using existing portal.js

2. **Filter Icons**: 
    Use existing RR.Blazor icon system (framework is icon-agnostic)

3. **Quick Filter Presets**: 
    Developers define all presets (maximum flexibility)

4. **Filter Persistence Key**: 
    Auto-generate key from component ID + user context

5. **Filter Change Debouncing**: 
    300ms debounce for text inputs, immediate for select/checkbox (2025 best practice)

6. **Empty State Messaging**: 
    Generic "No results found" + "Clear filters" suggestion button

7. **Column Type Detection**: 
    Build-time analysis for static types (like forwarder pattern)
    Runtime analysis of all data for dynamic sources
    Smart + strongly typed optimized version

8. **Filter Badge Display**: 
    Show on both column headers AND toolbar for maximum UX
    Column badge shows column filter count
    Toolbar badge shows total active filters

9. **Keyboard Shortcuts**: 
    Integrate with RR.Blazor global keyboard patterns
    Global JS control for keyboard shortcuts
    Escape to close, Enter to apply, Ctrl+Shift+F to focus

10. **Mobile Gesture Support**: 
    ⏳ TODO: Add in future iteration (not MVP)
    - Bottom sheet pattern when implemented

## Ready for Implementation

With the approved requirements and visual specifications, the SOW is now comprehensive. The only remaining clarifications are minor UX details that can be decided during implementation based on best practices.

### Key Implementation Priorities:
1. **Predicate-based filtering** (no data copies)
2. **Modern 2025 dense UI** with animations
3. **Virtual scrolling** for large datasets
4. **Master-detail coordination** pattern
5. **Extensible operators** and templates via interfaces
6. **Client-side only** (WASM and Server compatible)

The filter system is ready to implement as specified.
