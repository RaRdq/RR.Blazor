# RDataTable Auto-Generated Columns Documentation

## Overview

The RDataTable component includes an intelligent auto-generated columns feature that automatically creates column definitions from your model properties using reflection. This eliminates the need for manual column setup in most scenarios while providing smart formatting and display logic. This feature is part of RR.Blazor's 66-component library with 94% CLAUDE.md compliance.

## Quick Start

The simplest way to use auto-generated columns is to provide only the data:

```razor
<RDataTable TItem="Product" Items="@products" />
```

No column definitions needed! The system automatically analyzes the `Product` type and creates appropriate columns for all public properties.

## How It Works

When no explicit column definitions are provided (via `Columns` parameter or `ColumnsContent`), the RDataTable automatically:

1. **Discovers Properties**: Uses reflection to find all public, readable properties
2. **Filters Properties**: Excludes complex types, collections, and marked properties
3. **Generates Columns**: Creates column definitions with smart defaults
4. **Applies Formatting**: Uses type-appropriate formatting and display logic

## Property Analysis

### Included Properties

Auto-generation includes properties that are:
- Public and readable (`get` accessor)
- Primitive types (int, string, decimal, etc.)
- Value types (DateTime, DateOnly, TimeOnly, etc.)
- Enums
- Nullable versions of the above

### Excluded Properties

Auto-generation excludes properties that have:
- `[NotMapped]` attribute
- `[JsonIgnore]` attribute
- Complex object types (except string)
- Collection types (IEnumerable, List, etc.)

```csharp
public class Employee
{
    public string Id { get; set; }                    // ✓ Included
    public string Name { get; set; }                  // ✓ Included
    public decimal Salary { get; set; }               // ✓ Included
    public DateTime HireDate { get; set; }            // ✓ Included
    public Department Department { get; set; }        // ✓ Included (enum)
    
    [NotMapped]
    public string InternalNotes { get; set; }         // ✗ Excluded
    
    public Address Address { get; set; }              // ✗ Excluded (complex type)
    public List<string> Skills { get; set; }          // ✗ Excluded (collection)
}
```

## Customization with Attributes

### Display Attributes

Control column headers and order using standard .NET attributes:

```csharp
public class Product
{
    [Display(Name = "Product ID", Order = 1)]
    public int Id { get; set; }
    
    [Display(Name = "Product Name", Order = 2)]
    public string Name { get; set; }
    
    [DisplayName("Unit Price")]  // Alternative to Display
    public decimal Price { get; set; }
}
```

### Formatting Attributes

Apply custom formatting using `DisplayFormat`:

```csharp
public class Employee
{
    [DisplayFormat(DataFormatString = "{0:C0}")]
    public decimal Salary { get; set; }                // $75,000
    
    [DisplayFormat(DataFormatString = "{0:P1}")]
    public decimal Commission { get; set; }            // 12.5%
    
    [DisplayFormat(DataFormatString = "{0:MMM dd, yyyy}")]
    public DateTime StartDate { get; set; }            // Jan 15, 2023
}
```

## Smart Type Formatting

Auto-generation applies intelligent formatting based on property types:

### DateTime Types
```csharp
public DateTime CreatedDate { get; set; }      // → "Jan 15, 2024"
public DateOnly EventDate { get; set; }        // → "Mar 22, 2024"
public TimeOnly StartTime { get; set; }        // → "14:30"
public TimeSpan Duration { get; set; }         // → "02:15"
```

### Numeric Types
```csharp
public decimal Price { get; set; }             // → "$1,299.99"
public double Rating { get; set; }             // → "4.85"
public int Quantity { get; set; }              // → "42"
```

### Boolean Types
```csharp
public bool IsActive { get; set; }             // → "✓" or "✗"
```

### String Types with Smart Detection
```csharp
public string Email { get; set; }              // → Clickable mailto: link
public string WebsiteUrl { get; set; }         // → Clickable external link
public string Phone { get; set; }              // → Formatted: (555) 123-4567
```

### Enum Types
```csharp
public enum Status { Active, Inactive, Pending }
public Status CurrentStatus { get; set; }      // → "Active" (with space formatting)
```

## Column Features

### Auto-Sizing

Columns are automatically sized based on content type:

- **Boolean**: 80px (for checkmarks)
- **Dates**: 120px (for formatted dates)
- **Numbers**: 80-100px (based on type)
- **Enums**: 100px
- **Email**: 200px
- **Phone**: 120px
- **ID fields**: 100px
- **Other strings**: Auto-size

### Sorting

Columns are automatically marked as sortable for:
- Primitive types (int, string, decimal, etc.)
- DateTime types
- Enum types

### Filtering

Columns are automatically marked as filterable for:
- String types
- Boolean types
- Enum types

## Usage Examples

### Basic Auto-Generation

```razor
<!-- Simple product table -->
<RDataTable TItem="Product" Items="@products" />

<!-- Employee directory with automatic formatting -->
<RDataTable TItem="Employee" Items="@employees" 
           Title="Employee Directory"
           ShowFilters="true" />
```

### With Display Attributes

```csharp
public class Order
{
    [Display(Name = "Order #", Order = 1)]
    public string OrderNumber { get; set; }
    
    [Display(Name = "Customer", Order = 2)]
    public string CustomerName { get; set; }
    
    [Display(Name = "Total Amount", Order = 3)]
    [DisplayFormat(DataFormatString = "{0:C}")]
    public decimal Total { get; set; }
    
    [Display(Name = "Order Date", Order = 4)]
    public DateTime CreatedDate { get; set; }
    
    [Display(Name = "Status", Order = 5)]
    public OrderStatus Status { get; set; }
    
    // This won't appear in the table
    [NotMapped]
    public string InternalReference { get; set; }
}
```

```razor
<RDataTable TItem="Order" Items="@orders" 
           Title="Order Management"
           Sortable="true"
           ShowPagination="true" />
```

### Comparison: Manual vs Auto

```razor
<!-- Manual column definition (traditional approach) -->
<RDataTable TItem="Employee" Items="@employees">
    <ColumnsContent>
        <RDataTableColumn Key="Id" Header="Employee ID" Sortable="true" />
        <RDataTableColumn Key="Name" Header="Full Name" Sortable="true" />
        <RDataTableColumn Key="Email" Header="Email Address" />
        <RDataTableColumn Key="Department" Header="Department" />
        <RDataTableColumn Key="Salary" Header="Salary">
            <CellTemplate Context="item">
                @(((Employee)item).Salary.ToString("C"))
            </CellTemplate>
        </RDataTableColumn>
    </ColumnsContent>
</RDataTable>

<!-- Auto-generated columns (modern approach) -->
<RDataTable TItem="Employee" Items="@employees" />
<!-- Produces equivalent output with proper formatting automatically -->
```

## Best Practices

### 1. Use Display Attributes for Control

```csharp
public class Product
{
    // Control display order
    [Display(Order = 1)]
    public int Id { get; set; }
    
    // Customize headers
    [Display(Name = "Product Name", Order = 2)]
    public string Name { get; set; }
    
    // Custom formatting
    [Display(Name = "Price", Order = 3)]
    [DisplayFormat(DataFormatString = "{0:C}")]
    public decimal Price { get; set; }
}
```

### 2. Exclude Internal Properties

```csharp
public class Employee
{
    public string Name { get; set; }
    
    // Exclude sensitive or internal data
    [NotMapped]
    public string PasswordHash { get; set; }
    
    [JsonIgnore]
    public string InternalNotes { get; set; }
}
```

### 3. Use Semantic Property Names

```csharp
public class Contact
{
    // These will be auto-detected and formatted
    public string Email { get; set; }           // → Mailto link
    public string WebsiteUrl { get; set; }      // → External link
    public string PhoneNumber { get; set; }     // → Formatted phone
}
```

### 4. Leverage Enums for Categories

```csharp
public enum Priority { Low, Medium, High, Critical }
public enum Status { Draft, Active, Completed, Cancelled }

public class Task
{
    public string Title { get; set; }
    public Priority Priority { get; set; }      // Auto-formatted enum
    public Status Status { get; set; }          // Auto-formatted enum
}
```

## Advanced Scenarios

### When to Use Manual Columns

Consider manual column definitions when you need:
- Custom cell templates with complex rendering
- Action buttons or interactive elements
- Conditional formatting based on row data
- Non-standard data transformations

```razor
<!-- Hybrid approach: Mix auto-generation with custom columns -->
<RDataTable TItem="Employee" Items="@employees">
    <ColumnsContent>
        <!-- Auto-generate basic columns -->
        @foreach (var column in PropertyColumnGenerator.GenerateColumns<Employee>())
        {
            <RDataTableColumn Key="@column.Key" Header="@column.Header" 
                            Sortable="@column.Sortable" />
        }
        
        <!-- Add custom action column -->
        <RDataTableColumn Key="actions" Header="Actions">
            <CellTemplate Context="item">
                <RButton Size="ButtonSize.Small" 
                        OnClick="() => EditEmployee(item)">
                    Edit
                </RButton>
            </CellTemplate>
        </RDataTableColumn>
    </ColumnsContent>
</RDataTable>
```

### Performance Considerations

Auto-generation uses reflection, but column definitions are cached per type:
- First render analyzes the type and caches column definitions
- Subsequent renders use cached definitions for optimal performance
- Minimal overhead after initial type analysis

## Migration Guide

### From Manual to Auto-Generated

**Before (Manual):**
```razor
<RDataTable TItem="Product" Items="@products">
    <ColumnsContent>
        <RDataTableColumn Key="Id" Header="ID" Sortable="true" />
        <RDataTableColumn Key="Name" Header="Product Name" Sortable="true" />
        <RDataTableColumn Key="Price" Header="Price" Sortable="true" />
        <RDataTableColumn Key="CreatedDate" Header="Created" />
    </ColumnsContent>
</RDataTable>
```

**After (Auto-Generated):**
```csharp
// Add attributes to your model
public class Product
{
    [Display(Order = 1)]
    public int Id { get; set; }
    
    [Display(Name = "Product Name", Order = 2)]
    public string Name { get; set; }
    
    [Display(Order = 3)]
    [DisplayFormat(DataFormatString = "{0:C}")]
    public decimal Price { get; set; }
    
    [Display(Name = "Created", Order = 4)]
    public DateTime CreatedDate { get; set; }
}
```

```razor
<!-- Much simpler usage -->
<RDataTable TItem="Product" Items="@products" />
```

## Troubleshooting

### Common Issues

**Q: Why aren't my properties showing up?**
- Check if properties are public with get accessors
- Verify they're not complex types or collections
- Ensure no `[NotMapped]` or `[JsonIgnore]` attributes

**Q: How do I control column order?**
- Use `[Display(Order = n)]` attributes on your properties
- Properties without Order appear after ordered ones

**Q: Can I mix auto-generated with manual columns?**
- Auto-generation only works when no manual columns are defined
- Use hybrid approach with PropertyColumnGenerator for advanced scenarios

**Q: How do I exclude specific properties?**
- Add `[NotMapped]` attribute to exclude from auto-generation
- Or use `[JsonIgnore]` for JSON serialization exclusion

## API Reference

### PropertyColumnGenerator Class

```csharp
public static class PropertyColumnGenerator
{
    // Generate columns for a type
    public static List<RDataTableColumn<TItem>> GenerateColumns<TItem>()
    
    // Internal methods for customization
    private static RDataTableColumn<TItem> CreateColumn<TItem>(PropertyInfo property)
    private static string FormatValue(object value, PropertyInfo property)
    private static string GetDisplayName(PropertyInfo property)
    private static int GetDisplayOrder(PropertyInfo property)
    private static string GetColumnWidth(PropertyInfo property)
    private static bool ShouldIncludeProperty(PropertyInfo property)
    private static bool IsSortable(PropertyInfo property)
    private static bool IsFilterable(PropertyInfo property)
}
```

### Supported Attributes

- `[Display(Name, Order)]` - Control header text and column order
- `[DisplayName]` - Alternative way to set header text
- `[DisplayFormat(DataFormatString)]` - Custom value formatting
- `[NotMapped]` - Exclude property from auto-generation
- `[JsonIgnore]` - Exclude property from auto-generation

### Auto-Generated Column Properties

```csharp
public class RDataTableColumn<TItem>
{
    public string Key { get; set; }              // Property name
    public string Header { get; set; }           // Display name
    public bool Sortable { get; set; }           // Auto-determined
    public bool Filterable { get; set; }         // Auto-determined
    public string Width { get; set; }            // Type-based sizing
    public RenderFragment<TItem> CellTemplate { get; set; }  // Smart formatting
}
```

This auto-generated columns feature significantly reduces boilerplate code while providing intelligent defaults that work well for most data display scenarios. Combined with the attribute-based customization system, it offers the perfect balance of convenience and control.