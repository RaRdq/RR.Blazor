# Smart Table System (RTable/RColumn)

## Overview

The RR.Blazor Smart Table System provides a modern, type-safe, and efficient table implementation using `RTable` and `RColumn` components. This system replaces the legacy `RDataTable`/`RDataTableColumn` approach with better performance, cleaner syntax, and automatic type detection.

## Auto-Generated Columns (Zero Configuration)

Both `RTable` and `RDataTable` support automatic column generation from model properties with zero configuration required.

### TL;DR

**Before (Manual Column Definition):**
```razor
<!-- RDataTable with manual columns -->
<RDataTable TItem="Product" Items="@products">
    <ColumnsContent>
        <RDataTableColumn Key="Id" Header="ID" />
        <RDataTableColumn Key="Name" Header="Product Name" />
        <RDataTableColumn Key="Price" Header="Price" />
    </ColumnsContent>
</RDataTable>

<!-- RTable with manual columns -->
<RTable Items="@products">
    <ColumnsContent>
        <RColumn Property="@((Product p) => p.Id)" Header="ID" Sortable="true" />
        <RColumn Property="@((Product p) => p.Name)" Header="Product Name" Sortable="true" />
        <RColumn Property="@((Product p) => p.Price)" Header="Price" Format="c" Sortable="true" />
    </ColumnsContent>
</RTable>
```

**After (Auto-Generated):**
```razor
<!-- Both components support zero-config auto-generation -->
<RDataTable TItem="Product" Items="@products" />
<RTable Items="@products" />
```

### Quick Setup

#### Basic Usage (Zero Configuration)
```razor
<!-- RDataTable requires explicit type parameter -->
<RDataTable TItem="Employee" Items="@employees" />

<!-- RTable infers type from Items collection -->
<RTable Items="@employees" />
```
Both automatically generate columns from all public properties with smart formatting using the `PropertyColumnGenerator` class.

#### Customized with Attributes
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
    public DateTime HireDate { get; set; }
    
    // This won't appear in table
    [NotMapped]
    public string InternalNotes { get; set; }
}
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

### Included/Excluded Properties

#### ✅ Included
- Primitive types (int, string, decimal)
- DateTime types (DateTime, DateOnly, TimeOnly)
- Enums
- Nullable versions of above

#### ❌ Excluded
- Properties with `[NotMapped]`
- Properties with `[JsonIgnore]`
- Complex objects (except string)
- Collections (List, Array, etc.)

### Control with Attributes

#### Column Headers & Order
```csharp
[Display(Name = "Custom Header", Order = 2)]
public string PropertyName { get; set; }

[DisplayName("Alternative Header")]  // Alternative to Display
public string AnotherProperty { get; set; }
```

#### Custom Formatting
```csharp
[DisplayFormat(DataFormatString = "{0:C}")]    // Currency
[DisplayFormat(DataFormatString = "{0:P1}")]   // Percentage
[DisplayFormat(DataFormatString = "{0:N2}")]   // 2 decimal places
```

#### Exclude Properties
```csharp
[NotMapped]                           // Entity Framework style
[JsonIgnore]                          // JSON serialization style
public string ExcludedProperty { get; set; }
```

### Auto-Generated Column Features

- **Sortable**: Automatic for primitives, dates, enums
- **Filterable**: Automatic for strings, bools, enums  
- **Auto-Sizing**: Type-based width optimization
- **Smart Detection**: Email, phone, URL special handling

### When to Use Manual vs Auto-Generated Columns

**Use Auto-Generated** when:
- Simple data display
- Standard formatting is sufficient
- Minimal customization needed

**Use Manual Columns** when you need:
- Custom cell templates
- Action buttons
- Conditional formatting
- Complex data transformations

### Migration Strategy

1. **Start Simple**: Remove existing `<ColumnsContent>` and let auto-generation work
2. **Add Attributes**: Use `[Display]` and `[DisplayFormat]` for customization
3. **Hybrid Approach**: Mix auto-generation with manual columns when needed
4. **Performance Testing**: Use `PropertyColumnGenerator.GetCacheStats()` to monitor cache efficiency

## Core Components

### RTable<TItem>
Generic table component that provides:
- Automatic pagination and virtualization
- Built-in sorting and filtering
- Multi-selection with bulk operations
- Export capabilities
- Loading states and skeleton UI
- Responsive design

### RColumn
Smart column definition with automatic type detection:
- Property-based column definitions
- Helper method integration
- Automatic header generation
- Type-safe sorting and formatting
- Custom templates when needed

## Basic Usage

```razor
<RTable Items="@employees" 
        Title="Employee Directory"
        ExportEnabled="true"
        MultiSelection="true">
    <ColumnsContent>
        <RColumn Property="@((Employee e) => e.Name)" Header="Full Name" Sortable="true" />
        <RColumn Property="@((Employee e) => e.Department)" Header="Department" Sortable="true" />
        <RColumn Property="@((Employee e) => e.Salary)" Header="Salary" Format="c" Sortable="true" />
        <RColumn Property="@((Employee e) => e.StartDate)" Header="Start Date" Sortable="true" />
        <RColumn Header="Actions" Sortable="false" />
    </ColumnsContent>
</RTable>
```

## Advanced Features

### Helper Method Pattern
For complex display logic, use helper methods instead of templates:

```csharp
// Instead of complex templates, use helper methods
private string GetEmployeeStatus(Employee employee)
{
    return employee.IsActive ? "Active" : "Inactive";
}

private string GetEmployeeDisplay(Employee employee)
{
    return $"{employee.FirstName} {employee.LastName} ({employee.Email})";
}
```

```razor
<RColumn Property="@((Employee e) => GetEmployeeDisplay(e))" Header="Employee" Sortable="true" />
<RColumn Property="@((Employee e) => GetEmployeeStatus(e))" Header="Status" Sortable="true" />
```

### Bulk Operations
Enable bulk operations for multi-row actions:

```razor
<RTable Items="@payrolls"
        BulkOperationsEnabled="true" 
        MultiSelection="true"
        SelectedItems="@selectedPayrolls"
        SelectedItemsChanged="@HandleSelectionChanged">
    <BulkOperations>
        <RActionGroup>
            <RButton Icon="publish" Text="Bulk Publish" OnClick="@OnBulkPublish" />
            <RButton Icon="delete" Text="Bulk Archive" OnClick="@OnBulkArchive" />
        </RActionGroup>
    </BulkOperations>
    <ColumnsContent>
        <!-- Column definitions -->
    </ColumnsContent>
</RTable>
```

### Export and Filtering
Built-in export and filtering capabilities:

```razor
<RTable Items="@data" 
        ExportEnabled="true"
        FilterEnabled="true"
        SearchEnabled="true">
```

## Migration from Legacy System

### Before (RDataTable/RDataTableColumn)
```razor
<RDataTable Items="@employees">
    <RDataTableColumn TItem="Employee" Header="Name">
        <CellTemplate Context="employee">
            @employee.FirstName @employee.LastName
        </CellTemplate>
    </RDataTableColumn>
</RDataTable>
```

### After (RTable/RColumn)
```razor
<RTable Items="@employees">
    <ColumnsContent>
        <RColumn Property="@((Employee e) => GetFullName(e))" Header="Name" Sortable="true" />
    </ColumnsContent>
</RTable>
```

## Performance Benefits

### RTable/RColumn System
1. **Type Safety**: Compile-time checking of property expressions
2. **Reduced Reflection**: Property expressions compiled once
3. **Memory Efficiency**: No template overhead for simple columns
4. **Better IntelliSense**: Full IDE support for properties
5. **Cleaner Code**: Helper methods instead of complex templates

### Auto-Generated Columns (Both Systems)
1. **Cached Reflection**: Column definitions cached per type using `ConcurrentDictionary`
2. **Compiled Accessors**: Property access through compiled expressions for optimal performance
3. **Cached Formatters**: Type-specific formatters cached to avoid repeated format string processing
4. **Minimal Runtime Overhead**: Reflection only runs once per type, same performance as manual columns after first render
5. **Smart Caching**: Cache invalidation only when necessary, with access logging for memory management

## Component Parameters

### RTable Parameters
- `Items` - Data collection (required)
- `Title` - Table title
- `Subtitle` - Table subtitle
- `Loading` - Loading state
- `ExportEnabled` - Enable export functionality
- `MultiSelection` - Enable row selection
- `BulkOperationsEnabled` - Enable bulk operation toolbar
- `PageSize` - Items per page
- `Virtualize` - Enable virtualization for large datasets
- `SearchEnabled` - Enable global search
- `FilterEnabled` - Enable column filtering

### RColumn Parameters
- `Property` - Property expression (required for data columns)
- `Header` - Column header text
- `Sortable` - Enable sorting
- `Filterable` - Enable filtering
- `Format` - Display format (c, n, p, d, etc.)
- `Width` - Column width
- `Sticky` - Make column sticky
- `Visible` - Column visibility

## Best Practices

1. **Use Helper Methods**: Keep templates simple, move logic to C# methods
2. **Type-Safe Properties**: Always use strongly-typed property expressions
3. **Meaningful Headers**: Provide clear, descriptive column headers
4. **Performance**: Enable virtualization for large datasets (1000+ items)
5. **Accessibility**: Include proper ARIA labels and semantic markup
6. **Responsive**: Test tables in different container sizes
7. **Loading States**: Always handle loading and empty states

## Examples

### Basic Employee Table
```razor
<RTable Items="@employees" Title="Employee Directory">
    <ColumnsContent>
        <RColumn Property="@((Employee e) => GetEmployeeDisplay(e))" Header="Employee" Sortable="true" />
        <RColumn Property="@((Employee e) => e.Department)" Header="Department" Sortable="true" />
        <RColumn Property="@((Employee e) => e.Salary)" Header="Salary" Format="c" Sortable="true" />
        <RColumn Property="@((Employee e) => GetStatusDisplay(e))" Header="Status" Sortable="true" />
    </ColumnsContent>
</RTable>

@code {
    private string GetEmployeeDisplay(Employee employee) =>
        $"{employee.FirstName} {employee.LastName}";
    
    private string GetStatusDisplay(Employee employee) =>
        employee.IsActive ? "Active" : "Inactive";
}
```

### Payroll Management Table
```razor
<RTable Items="@payrolls" 
        Title="Payroll Management"
        BulkOperationsEnabled="true"
        MultiSelection="true"
        ExportEnabled="true">
    <BulkOperations>
        <RActionGroup>
            <RButton Icon="publish" Text="Bulk Publish" OnClick="@BulkPublish" />
            <RButton Icon="archive" Text="Bulk Archive" OnClick="@BulkArchive" />
        </RActionGroup>
    </BulkOperations>
    <ColumnsContent>
        <RColumn Property="@((Payroll p) => p.Title)" Header="Payroll" Sortable="true" />
        <RColumn Property="@((Payroll p) => GetStatusDisplay(p))" Header="Status" Sortable="true" />
        <RColumn Property="@((Payroll p) => p.EmployeeCount)" Header="Employees" Sortable="true" />
        <RColumn Property="@((Payroll p) => p.TotalAmount)" Header="Total" Format="c" Sortable="true" />
        <RColumn Header="Actions" />
    </ColumnsContent>
</RTable>
```

## Architecture

The Smart Table System uses cascading parameters and interface-based registration:

1. **TableContext**: Provides table metadata and type information
2. **ITableParent**: Interface for table components that accept columns
3. **Property Expressions**: Compiled lambda expressions for type-safe access
4. **Helper Method Pattern**: C# methods for complex display logic

This architecture ensures type safety, performance, and maintainability while providing a clean, declarative syntax for table definitions.

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