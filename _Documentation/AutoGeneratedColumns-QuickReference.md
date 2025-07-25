# RDataTable Auto-Generated Columns - Quick Reference

## TL;DR

**Before (Manual):**
```razor
<RDataTable TItem="Product" Items="@products">
    <ColumnsContent>
        <RDataTableColumn Key="Id" Header="ID" />
        <RDataTableColumn Key="Name" Header="Product Name" />
        <RDataTableColumn Key="Price" Header="Price" />
    </ColumnsContent>
</RDataTable>
```

**After (Auto-Generated):**
```razor
<RDataTable TItem="Product" Items="@products" />
```

## Quick Setup

### 1. Basic Usage (Zero Configuration)
```razor
<RDataTable TItem="Employee" Items="@employees" />
```
Automatically generates columns from all public properties with smart formatting.

### 2. Customized with Attributes
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

## Automatic Formatting

| Type | Auto-Format | Example |
|------|-------------|---------|
| `DateTime` | MMM dd, yyyy | Jan 15, 2024 |
| `decimal` | Currency | $1,299.99 |
| `bool` | Checkmarks | ✓ / ✗ |
| `enum` | Spaced words | Active Status |
| Email properties | Mailto links | `<a href="mailto:...">` |
| Phone properties | Formatted | (555) 123-4567 |

## Included/Excluded Properties

### ✅ Included
- Primitive types (int, string, decimal)
- DateTime types (DateTime, DateOnly, TimeOnly)
- Enums
- Nullable versions of above

### ❌ Excluded
- Properties with `[NotMapped]`
- Properties with `[JsonIgnore]`
- Complex objects (except string)
- Collections (List, Array, etc.)

## Control with Attributes

### Column Headers & Order
```csharp
[Display(Name = "Custom Header", Order = 2)]
public string PropertyName { get; set; }

[DisplayName("Alternative Header")]  // Alternative to Display
public string AnotherProperty { get; set; }
```

### Custom Formatting
```csharp
[DisplayFormat(DataFormatString = "{0:C}")]    // Currency
[DisplayFormat(DataFormatString = "{0:P1}")]   // Percentage
[DisplayFormat(DataFormatString = "{0:N2}")]   // 2 decimal places
```

### Exclude Properties
```csharp
[NotMapped]                           // Entity Framework style
[JsonIgnore]                          // JSON serialization style
public string ExcludedProperty { get; set; }
```

## Column Features

- **Sortable**: Automatic for primitives, dates, enums
- **Filterable**: Automatic for strings, bools, enums  
- **Auto-Sizing**: Type-based width optimization
- **Smart Detection**: Email, phone, URL special handling

## When to Use Manual Columns

Use manual `<ColumnsContent>` when you need:
- Custom cell templates
- Action buttons
- Conditional formatting
- Complex data transformations

## Migration Strategy

1. **Start Simple**: Remove existing `<ColumnsContent>` and let auto-generation work
2. **Add Attributes**: Use `[Display]` and `[DisplayFormat]` for customization
3. **Hybrid Approach**: Mix auto-generation with manual columns when needed

## Performance Notes

- Column definitions cached per type
- Reflection only runs once per type
- Minimal overhead after initial analysis
- Same performance as manual columns after first render
- Part of RR.Blazor's 66-component architecture with 3,309 utility classes

---

📖 **[Full Documentation](RDataTable-AutoGeneratedColumns.md)** | 🏠 **[Back to README](../README.md)**