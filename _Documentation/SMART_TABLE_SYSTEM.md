# Smart Table System (RTable/RColumn)

## Overview

The RR.Blazor Smart Table System provides a modern, type-safe, and efficient table implementation with multiple approaches to suit every developer's preference and use case.

## 🚀 Quick Start - Choose Your Style

### 1. Zero Configuration (Auto-Generated Columns)
```razor
@* Automatically generates columns from all public properties *@
<RTable Items="@employees" />
```

### 2. Compile-Time Safe with `nameof` (BEST FOR REFACTORING)
```razor
<RTable Items="@employees">
    <RColumn Property="@nameof(Employee.Name)" />
    <RColumn Property="@nameof(Employee.Email)" />
    <RColumn Property="@nameof(Employee.Salary)" Format="C" />
</RTable>
```
**✅ Refactoring-safe, IntelliSense, no type parameters needed**

### 3. Lambda with Explicit Types (FOR COMPUTED PROPERTIES)
```razor
<RTable Items="@employees">
    <RColumn For="@((Employee e) => e.Name)" />
    <RColumn For="@((Employee e) => GetFullName(e))" Header="Full Name" />
    <RColumn For="@((Employee e) => e.Salary)" Format="C" />
</RTable>
```
**✅ Type-safe, supports methods and computed values**

### 4. Custom Templates (FOR COMPLEX RENDERING)
```razor
<RTable Items="@employees">
    <RColumn Property="@nameof(Employee.Name)" Header="Employee">
        <Template Context="emp">
            <div class="d-flex items-center gap-2">
                <RAvatar Text="@emp.Name" Size="AvatarSize.Small" />
                <div>
                    <div class="font-semibold">@emp.Name</div>
                    <div class="text-xs text-secondary">@emp.Department</div>
                </div>
            </div>
        </Template>
    </RColumn>
    <RColumn Property="@nameof(Employee.Status)">
        <Template Context="emp">
            <RChip Text="@emp.Status" 
                   Variant="@(emp.Status == "Active" ? ChipVariant.Success : ChipVariant.Secondary)" 
                   Size="ChipSize.Small" />
        </Template>
    </RColumn>
</RTable>
```

## 🎯 Smart Features

### Column Management & Visibility
Built-in column manager for dynamic table control:
```razor
<RTable Items="@employees" 
        ShowColumnManager="true"
        EnableColumnReordering="true"
        EnableStickyColumns="true">
```
- **Show/Hide Columns**: Users can toggle column visibility
- **Pin Columns**: Sticky columns that stay visible when scrolling
- **Reorder Columns**: Drag-and-drop column reordering
- **Save Preferences**: Persists to localStorage automatically

### Horizontal Scrolling
Adaptive horizontal scrolling for wide tables:
```razor
<RTable Items="@data" 
        EnableHorizontalScroll="true"
        Class="min-w-fit>
```
- **Auto-enabled by default**: Tables scroll horizontally when content overflows
- **CSS-controlled width**: Use `Style` or `Class` for table width control
- **Mobile-optimized**: Touch-friendly scrolling on mobile devices
- **Custom scrollbars**: Styled to match your theme

### Intelligent PageSize Options
The table automatically suggests optimal page sizes based on your dataset:
- **Small datasets (≤50 rows)**: `[5, 10]`
- **Medium datasets (≤200 rows)**: `[10, 25, 50]`
- **Large datasets (≤1000 rows)**: `[25, 50, 100, 250]`
- **Very large (10K+ rows)**: Exponential scaling with smart options
- **Override anytime**: Set `PageSizeOptions="@new[] { 20, 40, 60 }"` for custom values

### High-Performance Virtualization
For massive datasets, use `RTableVirtualized`:
```razor
<RTableVirtualized Items="@largeDataset" 
                   Height="600px"
                   ShowPerformanceMetrics="true"
                   ShowColumnManager="true" />
```
- Handles 1M+ records smoothly
- Real-time performance metrics
- Minimal DOM nodes (only visible rows rendered)
- Full column management support

## 📋 The Magic: Auto-Generated Columns

### Zero Configuration = Full Featured Table

Just pass your data and the table figures out everything:

```razor
@* This single line gives you: *@
<RTable Items="@employees" />

@* Results in: *@
- ✅ All columns auto-generated from properties
- ✅ Smart formatting (dates, currency, booleans)
- ✅ Sortable columns (automatic for primitives)
- ✅ Searchable text fields
- ✅ Responsive pagination
- ✅ Loading states
- ✅ Empty state handling
```

### Control Auto-Generation with Attributes

```csharp
public class Employee
{
    [Display(Name = "Employee ID", Order = 1)]
    public string Id { get; set; }
    
    [Display(Name = "Full Name", Order = 2)]
    public string Name { get; set; }
    
    [Display(Name = "Annual Salary", Order = 3)]
    [DisplayFormat(DataFormatString = "{0:C0}")]
    public decimal Salary { get; set; }
    
    [Display(Name = "Start Date", Order = 4)]
    [DisplayFormat(DataFormatString = "{0:MMM dd, yyyy}")]
    public DateTime HireDate { get; set; }
    
    // This won't appear in table
    [NotMapped]
    public string InternalNotes { get; set; }
    
    // Also excluded
    [JsonIgnore]
    public string SecretData { get; set; }
}
```

## 🛠️ Advanced Usage Patterns

### Helper Methods for Complex Logic
```csharp
@code {
    // Clean, reusable display logic
    private string GetEmployeeDisplay(Employee e) => 
        $"{e.FirstName} {e.LastName} ({e.Department})";
    
    private string GetStatusBadge(Employee e) => 
        e.IsActive ? "Active" : "Inactive";
    
    private decimal CalculateBonus(Employee e) => 
        e.Salary * (e.Performance > 90 ? 0.15m : 0.10m);
}
```

```razor
<RTable Items="@employees">
    <RColumn For="@((Employee e) => GetEmployeeDisplay(e))" Header="Employee" />
    <RColumn For="@((Employee e) => GetStatusBadge(e))" Header="Status" />
    <RColumn For="@((Employee e) => CalculateBonus(e))" Header="Bonus" Format="C" />
</RTable>
```

### Automatic Formatting

| Type | Auto-Format | Example |
|------|-------------|---------|
| `DateTime` | MMM dd, yyyy | Jan 15, 2024 |
| `decimal` | Currency | $1,299.99 |
| `bool` | Checkmarks | ✓ / ✗ |
| `enum` | Spaced words | Active Status |
| Email properties | Mailto links | `<a href="mailto:...">` |
| Phone properties | Formatted | (555) 123-4567 |

### Smart Type Detection & Formatting

| Property Type | Auto-Detection | Default Format | Example Output |
|--------------|----------------|----------------|----------------|
| `string` with "Email" | ✅ Email link | `mailto:` link | 📧 john@company.com |
| `string` with "Phone" | ✅ Phone format | (XXX) XXX-XXXX | 📞 (555) 123-4567 |
| `string` with "Url" | ✅ Hyperlink | Clickable link | 🔗 Visit Site |
| `DateTime` | ✅ Date format | MMM dd, yyyy | Jan 15, 2024 |
| `decimal`/`double` with "Price", "Cost", "Salary" | ✅ Currency | $X,XXX.XX | $1,299.99 |
| `bool` | ✅ Check/Cross | ✓ / ✗ | ✓ Active |
| `enum` | ✅ Spaced words | Pascal → Title | Active Status |
| Percentage properties | ✅ Percent | XX.X% | 85.5% |

## 🎨 Custom Templates Without Complexity

### Using Template Context (No Casting Required)
```razor
<RTable Items="@employees">
    <RColumn Property="@nameof(Employee.Name)">
        <Template Context="emp">
            @* emp is strongly typed as Employee! *@
            <div class="d-flex items-center gap-2">
                <img src="@emp.PhotoUrl" class="avatar-sm" />
                <div>
                    <div class="font-semibold">@emp.Name</div>
                    <div class="text-xs text-muted">@emp.Title</div>
                </div>
            </div>
        </Template>
    </RColumn>
</RTable>
```

### Action Columns
```razor
<RColumn Header="Actions">
    <Template Context="emp">
        <RActionGroup Pattern="ActionGroupPattern.Icon" Size="ActionGroupSize.Small">
            <RButton Icon="edit" OnClick="@(() => EditEmployee(emp))" />
            <RButton Icon="delete" OnClick="@(() => DeleteEmployee(emp))" />
        </RActionGroup>
    </Template>
</RColumn>
```

## 🚀 Enterprise Features

### Bulk Operations
```razor
<RTable Items="@payrolls" 
        MultiSelection="true"
        BulkOperationsEnabled="true">
    <BulkOperations>
        <RActionGroup>
            <RButton Icon="publish" Text="Publish Selected" OnClick="@PublishSelected" />
            <RButton Icon="archive" Text="Archive Selected" OnClick="@ArchiveSelected" />
            <RButton Icon="delete" Text="Delete Selected" OnClick="@DeleteSelected" />
        </RActionGroup>
    </BulkOperations>
</RTable>
```

### Export & Search
```razor
<RTable Items="@data" 
        ExportEnabled="true"
        SearchEnabled="true"
        ShowToolbar="true"
        ShowChartButton="true" />
```

### Loading States & Empty States
```razor
<RTable Items="@employees" 
        Loading="@isLoading"
        LoadingText="Fetching employee data...">
    <EmptyContent>
        <REmptyState Icon="group" 
                     Text="No Employees Found" 
                     Description="Start by adding your first employee" />
    </EmptyContent>
</RTable>
```

## 📊 Performance & Optimization

### Choose the Right Component

| Component | Use Case | Max Records | Features |
|-----------|----------|-------------|----------|
| `RTable` | Standard tables | < 10K | Full features, auto-pagination |
| `RTableVirtualized` | Large datasets | 1M+ | Virtualization, minimal DOM |
| `RDataTable` | Legacy compatibility | < 1K | Basic features |

### Performance Tips

1. **Enable Virtualization** for 1000+ rows:
```razor
<RTable Items="@largeDataset" Virtualize="true" />
```

2. **Use Helper Methods** instead of inline lambdas:
```csharp
// ❌ Avoid
For="@((Employee e) => e.FirstName + " " + e.LastName + " (" + e.Department + ")")"

// ✅ Better
For="@((Employee e) => GetEmployeeDisplay(e))"
```

3. **Leverage Caching**:
```csharp
// Monitor cache performance
var stats = PropertyColumnGenerator.GetCacheStats();
Console.WriteLine($"Cached: {stats.Item1} columns, {stats.Item2} accessors");
```

## 🔄 Migration Guide

### From Old RDataTable
```razor
@* Old way *@
<RDataTable TItem="Employee" Items="@employees">
    <RDataTableColumn Key="Name" Header="Employee Name" />
    <RDataTableColumn Key="Salary" Header="Annual Salary" />
</RDataTable>

@* New way - Option 1: Zero config *@
<RTable Items="@employees" />

@* New way - Option 2: With customization *@
<RTable Items="@employees">
    <RColumn Property="@nameof(Employee.Name)" Header="Employee Name" />
    <RColumn Property="@nameof(Employee.Salary)" Header="Annual Salary" Format="C" />
</RTable>
```

### From Complex Templates
```razor
@* Old way with casting *@
CustomTemplate="@(context => @<div>@(((Employee)context).Name)</div>)"

@* New way - no casting! *@
<Template Context="emp">
    <div>@emp.Name</div>
</Template>
```

## ⚡ Real-World Examples

### Employee Directory with Everything
```razor
<RTable Items="@employees" 
        Title="Employee Directory"
        ShowSearch="true"
        ExportEnabled="true"
        MultiSelection="true"
        Loading="@isLoading">
    
    @* Mix auto-generated and custom columns *@
    <RColumn Property="@nameof(Employee.Name)">
        <Template Context="emp">
            <div class="d-flex items-center gap-2">
                <RAvatar Text="@emp.Name" Size="AvatarSize.Small" />
                <div>
                    <div>@emp.Name</div>
                    <div class="text-xs text-muted">@emp.Email</div>
                </div>
            </div>
        </Template>
    </RColumn>
    
    <RColumn Property="@nameof(Employee.Department)" />
    <RColumn Property="@nameof(Employee.Salary)" Format="C0" />
    
    <RColumn Property="@nameof(Employee.Status)">
        <Template Context="emp">
            <RChip Text="@emp.Status" 
                   Variant="@(GetStatusVariant(emp.Status))" 
                   Size="ChipSize.Small" />
        </Template>
    </RColumn>
    
    <RColumn Header="Actions">
        <Template Context="emp">
            <RButton Icon="edit" Size="ButtonSize.Small" 
                     OnClick="@(() => EditEmployee(emp))" />
        </Template>
    </RColumn>
</RTable>
```

### Simple 5-Line Table
```razor
<RTable Items="@products" Title="Product Catalog" ShowSearch="true">
    <RColumn Property="@nameof(Product.Name)" />
    <RColumn Property="@nameof(Product.Category)" />
    <RColumn Property="@nameof(Product.Price)" Format="C" />
    <RColumn Property="@nameof(Product.InStock)" />
</RTable>
```

### Virtualized Table for Big Data
```razor
@if (records.Count > 10000)
{
    <RTableVirtualized Items="@records" 
                       Height="800px"
                       ShowPerformanceMetrics="true">
        <RColumn Property="@nameof(Record.Id)" />
        <RColumn Property="@nameof(Record.Timestamp)" Format="g" />
        <RColumn Property="@nameof(Record.Value)" Format="N2" />
    </RTableVirtualized>
}
else
{
    <RTable Items="@records" />
}
```

## 📋 Component Reference

### RTable Parameters
| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `Items` | `IEnumerable<T>` | Required | Data collection |
| `Title` | `string` | null | Table header title |
| `Loading` | `bool` | false | Shows loading state |
| `PageSize` | `int` | Auto | Items per page (smart default based on data size) |
| `PageSizeOptions` | `int[]` | Auto | Custom page size options |
| `ShowSearch` | `bool` | true | Enable search bar |
| `ShowPagination` | `bool` | true | Enable pagination |
| `ShowChartButton` | `bool` | false | Show chart visualization button |
| `ShowColumnManager` | `bool` | false | Show column visibility manager |
| `EnableHorizontalScroll` | `bool` | true | Enable horizontal scrolling for wide tables |
| `EnableColumnReordering` | `bool` | false | Allow drag-and-drop column reordering |
| `EnableStickyColumns` | `bool` | false | Allow pinning columns to left/right |
| `ColumnPreferences` | `Dictionary` | {} | Column visibility/order preferences |
| `MultiSelection` | `bool` | false | Enable row multi-selection |
| `ExportEnabled` | `bool` | false | Enable export functionality |
| `Virtualize` | `bool` | false | Enable virtualization for large datasets |
| `AutoGenerateColumns` | `bool` | true | Auto-generate columns from properties |

### RColumn Parameters
| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `Property` | `string` | - | Property name (use with `nameof`) |
| `For` | `Expression` | - | Lambda expression for property/computed value |
| `Header` | `string` | Auto | Column header text |
| `Format` | `string` | Auto | Format string (C, N, P, d, etc.) |
| `Sortable` | `bool` | true | Enable column sorting |
| `Searchable` | `bool` | true | Include in search |
| `Width` | `string` | Auto | Column width |
| `Template` | `RenderFragment` | - | Custom cell template |

## 🎯 Best Practices

### DO ✅
- Use `nameof()` for refactoring-safe property references
- Use helper methods for complex display logic
- Enable virtualization for 1000+ rows
- Let auto-generation work when possible
- Use `Template` context for custom rendering
- Leverage smart PageSize defaults

### DON'T ❌
- Don't cast `context` to types - use `Template Context="emp"`
- Don't use inline complex lambdas - use helper methods
- Don't define columns unnecessarily - use auto-generation
- Don't use RenderTreeBuilder - use Razor syntax
- Don't load 10K+ records without virtualization

## 🔧 Troubleshooting

### Common Issues & Solutions

| Issue | Solution |
|-------|----------|
| **Pagination not updating rows** | Ensure `PageSize` parameter is set correctly |
| **Loading state not showing** | Pass `Loading="@isLoading"` parameter |
| **Columns not auto-generating** | Check `AutoGenerateColumns="true"` (default) |
| **Poor performance with large data** | Use `RTableVirtualized` or enable `Virtualize="true"` |
| **Custom template not typed** | Use `<Template Context="item">` for strong typing |
| **Search not working** | Ensure `ShowSearch="true"` and columns have `Searchable="true"` |

## 🚢 Architecture & Performance

### Type Inference Magic
The table automatically detects your data type from the `Items` collection:
```csharp
public class RTable : RTableBase
{
    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        // Detects TItem from Items.GetType()
        var itemType = Items.GetType().GetGenericArguments()[0];
        
        // Creates strongly-typed RTableGeneric<TItem> internally
        var tableType = typeof(RTableGeneric<>).MakeGenericType(itemType);
        
        // Full type safety without developer specifying <TItem>!
    }
}
```

### Performance Optimizations
- **Smart Caching**: Column definitions cached per type
- **Compiled Expressions**: Property access via compiled lambdas
- **Virtual Scrolling**: Only visible rows in DOM
- **Lazy Loading**: Data fetched as needed
- **Intelligent Pagination**: Automatic page size optimization

## 📚 Additional Resources

### Related Documentation
- [SMART_COMPONENTS_ARCHITECTURE.md](./SMART_COMPONENTS_ARCHITECTURE.md) - Component architecture patterns
- [RR_BLAZOR_COMPONENTS.md](./RR_BLAZOR_COMPONENTS.md) - Full component library reference
- [PERFORMANCE_OPTIMIZATION.md](./PERFORMANCE_OPTIMIZATION.md) - Performance best practices

### Test Pages
- `/test/data-display` - DataDisplayShowcase with all table features
- `/test/virtualization` - Performance testing with large datasets
- `/test/table-features` - Interactive feature demonstrations

## 🎉 Summary

The RR.Blazor Smart Table System delivers:
- **Zero to Hero**: From zero config to full customization
- **Type Safety**: No casting, full IntelliSense
- **Performance**: Handles 1M+ records smoothly
- **Smart Defaults**: Intelligent pagination, formatting, and features
- **Developer Joy**: Write tables like regular Razor components

**Start simple, scale infinitely!**

```razor
@* This is all you need! *@
<RTable Items="@yourData" />
```

## Cache Management

The auto-generated column system includes sophisticated caching for optimal performance:

### Cache Types
- **Column Metadata Cache**: Stores analyzed property information per type
- **Property Accessor Cache**: Compiled expression trees for fast property access  
- **Formatter Cache**: Type-specific formatting functions
- **Access Log Cache**: Tracks usage for memory management

### Cache Management Methods
```csharp
// Clear cache for specific type (useful for development)
PropertyColumnGenerator.ClearCache<Employee>();

// Clear all caches (useful for testing)
PropertyColumnGenerator.ClearAllCaches();

// Monitor cache performance
var (columnCount, accessorCount, formatterCount) = PropertyColumnGenerator.GetCacheStats();
```

### Security Features
- **HTML Encoding**: All string values are HTML encoded to prevent XSS
- **URL Validation**: Only HTTP/HTTPS URLs are allowed for auto-generated links
- **Email Validation**: RFC-compliant email validation with length limits
- **Input Sanitization**: Phone numbers and other inputs are sanitized before display