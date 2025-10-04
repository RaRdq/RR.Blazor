# RFilter - Universal Smart Filter System

## Overview
RFilter is a single, smart component that provides enterprise S-tier filtering capabilities for all RR.Blazor data views. It follows the Smart Components Architecture pattern, working both as a standalone component and integrated part of data views.

## Core Features
- **Predicate-based filtering** - Generates predicates only, no data copies
- **Auto type detection** - Smart detection of data types from values
- **Master-detail coordination** - Single filter controls multiple views
- **300ms debounce** for text, immediate for select/checkbox
- **Extensible operators** - Custom operators via interfaces
- **Virtual scrolling** for 100+ filter values

## Quick Start

### Basic Usage
```razor
@* Standalone filter *@
<RFilter @ref="filter" Items="@employees" OnPredicateChanged="OnFilterChanged" />
<RTable Items="@filteredEmployees" />

@code {
    private RFilter<Employee> filter;
    private IEnumerable<Employee> filteredEmployees;
    
    private void OnFilterChanged(Expression<Func<Employee, bool>> predicate)
    {
        filteredEmployees = employees.Where(predicate.Compile());
    }
}
```

### Table Integration with Column Filters
```razor
@* Table with column filters *@
<RTable Items="@employees" EnableColumnFilters="true">
    <Columns>
        <RColumn Field="Name" Filterable="true" FilterType="FilterType.Text" />
        <RColumn Field="Department" Filterable="true" FilterType="FilterType.Select" />
        <RColumn Field="Salary" Filterable="true" FilterType="FilterType.Number" />
        <RColumn Field="HireDate" Filterable="true" FilterType="FilterType.Date" />
    </Columns>
</RTable>
```

### Inverted Control Pattern (Preferred)
```razor
@* Data view controls its filter *@
<RTable @ref="tableRef" Items="@employees" BindFilter="@filterRef" />
<RFilter @ref="filterRef" />
```

### Master-Detail Coordination
```razor
@* Single filter controls multiple views *@
<RFilter @ref="masterFilter" Items="@employees">
    <QuickFilters>
        <QuickFilterState Key="active" Label="Active Only" />
        <QuickFilterState Key="recent" Label="Recent Hires" />
    </QuickFilters>
</RFilter>

<RTable Items="@employees" BindFilter="@masterFilter" />
<RGrid Items="@employees" BindFilter="@masterFilter" />
<RChart Data="@employees" BindFilter="@masterFilter" />
```

## Filter Modes

### Smart Mode (Default)
Automatic detection and smart UI based on data:
```razor
<RFilter Mode="FilterMode.Smart" Items="@data" />
```

### Column Menu Mode
Excel-like dropdown filters in table headers:
```razor
<RFilter Mode="FilterMode.ColumnMenu" 
         IsColumnFilter="true"
         ColumnKey="Department"
         ColumnValues="@GetDepartments()" />
```

### Column Row Mode
Inline filter row below headers:
```razor
<RTable Items="@data">
    <FilterRow>
        <RFilter Mode="FilterMode.ColumnRow" />
    </FilterRow>
</RTable>
```

## Filter Types

### Text Filtering
```razor
<RFilter ColumnType="FilterType.Text" />
// Operators: Contains, StartsWith, EndsWith, Equals, NotEquals, IsEmpty
```

### Number Filtering
```razor
<RFilter ColumnType="FilterType.Number" />
// Operators: Equals, >, <, >=, <=, Between, IsEmpty
```

### Date Filtering
```razor
<RFilter ColumnType="FilterType.Date" />
// Operators: On, Before, After, Between, Today, ThisWeek, ThisMonth
```

### Select/MultiSelect
```razor
<RFilter ColumnType="FilterType.Select" />
// Single selection from values

<RFilter ColumnType="FilterType.MultiSelect" />
// Multiple selection with checkboxes
```

### Boolean Filtering
```razor
<RFilter ColumnType="FilterType.Boolean" />
// Options: True, False, All
```

## Configuration

### Basic Configuration
```razor
<RFilter TItem="Employee"
         ShowSearch="true"
         SearchPlaceholder="Search employees..."
         ShowQuickFilters="true"
         ShowAdvancedToggle="true"
         ShowClearButton="true"
         ShowFilterCount="true"
         ShowFilterChips="true"
         Density="DensityType.Normal"
         DebounceMs="300" />
```

### Persistence
```razor
<RFilter EnablePersistence="true" 
         PersistenceKey="employee-filters" />
```

### Column Filter Configuration
```razor
<RTable EnableColumnFilters="true"
        ColumnFilterLogic="FilterLogic.And"
        ShowFilterBadges="true">
```

## Advanced Features

### Custom Operators
```csharp
// Register custom operator
FilterRegistry.RegisterOperator(new CustomOperator
{
    Name = "IsWeekend",
    Evaluate = (value, _) => ((DateTime)value).DayOfWeek 
        is DayOfWeek.Saturday or DayOfWeek.Sunday,
    DisplayText = "Is Weekend"
});
```

### Quick Filters
```razor
<RFilter>
    <QuickFilters>
        @foreach (var filter in GetQuickFilters())
        {
            <QuickFilterState Key="@filter.Key" 
                            Label="@filter.Label"
                            Icon="@filter.Icon" />
        }
    </QuickFilters>
</RFilter>
```

### Filter Templates
```razor
@* Save current filter state *@
<button @onclick="SaveTemplate">Save Filter Template</button>

@* Load saved template *@
<select @onchange="LoadTemplate">
    @foreach (var template in savedTemplates)
    {
        <option value="@template.Id">@template.Name</option>
    }
</select>

@code {
    private async Task SaveTemplate()
    {
        await filter.SaveTemplate("My Filter Set");
    }
    
    private async Task LoadTemplate(ChangeEventArgs e)
    {
        await filter.LoadTemplate(e.Value.ToString());
    }
}
```

## Events

### OnPredicateChanged
Fired when filter predicate changes:
```razor
<RFilter OnPredicateChanged="@OnFilterChanged" />

@code {
    private void OnFilterChanged(Expression<Func<TItem, bool>> predicate)
    {
        // Apply predicate to data
        filteredData = data.Where(predicate.Compile());
    }
}
```

### OnFilterChanged
Fired when filter state changes:
```razor
<RFilter OnFilterChanged="@OnStateChanged" />

@code {
    private void OnStateChanged(FilterState state)
    {
        Console.WriteLine($"Active filters: {state.Filters.Count}");
    }
}
```

### OnFilterCleared
Fired when all filters are cleared:
```razor
<RFilter OnFilterCleared="@OnCleared" />

@code {
    private void OnCleared()
    {
        // Reset to original data
        filteredData = data;
    }
}
```

## Styling

### Density Variations
```razor
<RFilter Density="DensityType.Compact" />   // 28px height
<RFilter Density="DensityType.Normal" />    // 36px height
<RFilter Density="DensityType.Dense" />     // 32px height
<RFilter Density="DensityType.Spacious" />  // 44px height
```

### Custom Styling
```razor
<RFilter Class="my-custom-filter" Style="--filter-primary: #007bff;" />
```

```scss
.my-custom-filter {
    .rfilter-bar {
        background: var(--custom-bg);
    }
    
    .rfilter-badge {
        background: var(--filter-primary);
    }
}
```

## Performance

### Virtual Scrolling
Automatically enabled for 100+ filter values:
```razor
<RFilter ColumnValues="@largeDataSet" /> // Auto virtual scroll
```

### Debouncing
Text inputs are debounced, selects are immediate:
```razor
<RFilter DebounceMs="300" /> // Default 300ms for text
```

### Caching
Column values are cached for performance:
```csharp
// Values are computed once and cached
private IEnumerable<object> GetColumnValues(string columnKey)
{
    return cache.GetOrAdd(columnKey, k => ComputeValues(k));
}
```

## Keyboard Shortcuts
- `Ctrl+Shift+F` - Focus filter search
- `Escape` - Close dropdown/clear search
- `Enter` - Apply filter
- `Arrow keys` - Navigate filter options
- `Space` - Toggle checkbox selection

## Mobile Support
On mobile devices, filter dropdowns automatically convert to bottom sheets:
- Swipe down to dismiss
- Touch optimized (44px minimum targets)
- Momentum scrolling
- Haptic feedback on selection

## Browser Compatibility
- Chrome 90+
- Firefox 88+
- Safari 14+
- Edge 90+

## Best Practices

### 1. Use Appropriate Filter Types
```razor
@* Let auto-detection handle it when possible *@
<RFilter ColumnType="FilterType.Auto" />

@* Or specify for better performance *@
<RFilter ColumnType="FilterType.Select" />
```

### 2. Optimize Large Datasets
```razor
@* Use server-side filtering for 10K+ items *@
<RFilter OnPredicateChanged="ApplyServerFilter" />

@code {
    private async Task ApplyServerFilter(Expression<Func<TItem, bool>> predicate)
    {
        // Convert to API query
        var query = PredicateToQuery(predicate);
        data = await api.GetFilteredData(query);
    }
}
```

### 3. Provide Quick Filters
```razor
@* Common filters as quick access buttons *@
<QuickFilterState Key="today" Label="Today" />
<QuickFilterState Key="active" Label="Active" />
<QuickFilterState Key="urgent" Label="Urgent" />
```

### 4. Use Filter Persistence
```razor
@* Remember user's filter preferences *@
<RFilter EnablePersistence="true" />
```

### 5. Show Filter Context
```razor
@* Always show what's filtered *@
<RFilter ShowFilterCount="true" ShowFilterChips="true" />
```

## Troubleshooting

### Filters Not Applying
- Ensure `Items` or `DataSource` is provided
- Check that `OnPredicateChanged` is handled
- Verify column keys match property names

### Performance Issues
- Enable virtual scrolling for large value lists
- Use `FilterType.Select` instead of `MultiSelect` for many options
- Consider server-side filtering for 10K+ items

### Type Detection Issues
- Explicitly set `ColumnType` if auto-detection fails
- Ensure column values are consistent types
- Check for null values affecting detection

## API Reference

### Parameters
| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| Mode | FilterMode | Smart | Filter UI mode |
| Density | DensityType | Normal | UI density |
| ShowSearch | bool | true | Show search box |
| ShowQuickFilters | bool | true | Show quick filter buttons |
| ShowAdvancedToggle | bool | true | Show advanced panel toggle |
| ShowClearButton | bool | true | Show clear all button |
| ShowFilterCount | bool | true | Show active filter count |
| ShowFilterChips | bool | true | Show filter chips |
| EnablePersistence | bool | false | Enable filter persistence |
| PersistenceKey | string | auto | Storage key for persistence |
| DebounceMs | int | 300 | Debounce milliseconds |
| ColumnType | FilterType | Auto | Column filter type |
| ColumnKey | string | null | Column identifier |
| ColumnTitle | string | null | Column display name |
| ColumnValues | IEnumerable | null | Available filter values |

### Methods
| Method | Returns | Description |
|--------|---------|-------------|
| ApplyFilter | Task | Apply filter programmatically |
| RemoveFilter | Task | Remove specific filter |
| ClearFilters | Task | Clear all filters |
| LoadTemplate | Task | Load filter template |
| SaveTemplate | Task | Save current filters as template |
| GetFilteredData | IEnumerable | Get filtered data |

### Events
| Event | Arguments | Description |
|-------|-----------|-------------|
| OnPredicateChanged | Expression<Func<T, bool>> | Filter predicate changed |
| OnFilterChanged | FilterState | Filter state changed |
| OnFilterCleared | - | All filters cleared |
| OnTemplateSaved | FilterTemplate | Template saved |
| OnTemplateLoaded | FilterTemplate | Template loaded |

## Examples

### Complete Employee Filter
```razor
@page "/employees"

<div class="employee-page">
    <RFilter @ref="employeeFilter" 
             Items="@employees"
             OnPredicateChanged="OnFilterChanged"
             ShowSearch="true"
             ShowQuickFilters="true"
             EnablePersistence="true">
        <QuickFilters>
            <QuickFilterState Key="active" Label="Active" Icon="check_circle" />
            <QuickFilterState Key="managers" Label="Managers" Icon="supervisor_account" />
            <QuickFilterState Key="recent" Label="Recent Hires" Icon="schedule" />
        </QuickFilters>
    </RFilter>
    
    <RTable Items="@filteredEmployees" 
            EnableColumnFilters="true"
            ColumnFilterLogic="FilterLogic.And">
        <Columns>
            <RColumn Field="Name" Filterable="true" />
            <RColumn Field="Department" Filterable="true" FilterType="FilterType.Select" />
            <RColumn Field="Position" Filterable="true" FilterType="FilterType.Select" />
            <RColumn Field="Salary" Filterable="true" FilterType="FilterType.Number" />
            <RColumn Field="HireDate" Filterable="true" FilterType="FilterType.Date" />
            <RColumn Field="IsActive" Filterable="true" FilterType="FilterType.Boolean" />
        </Columns>
    </RTable>
</div>

@code {
    private RFilter<Employee> employeeFilter;
    private List<Employee> employees = new();
    private IEnumerable<Employee> filteredEmployees = new List<Employee>();
    
    protected override async Task OnInitializedAsync()
    {
        employees = await LoadEmployees();
        filteredEmployees = employees;
    }
    
    private void OnFilterChanged(Expression<Func<Employee, bool>> predicate)
    {
        filteredEmployees = employees.Where(predicate.Compile());
        StateHasChanged();
    }
}
```

## Related Components
- [RTable](../RTableGeneric.razor) - Data table with column filters
- [RGrid](../RGridGeneric.razor) - Data grid with cell filters
- [RChart](../Display/RChart.razor) - Charts with data filtering