# Smart Table System (RTable/RColumn)

## Overview

The RR.Blazor Smart Table System provides a modern, type-safe, and efficient table implementation using `RTable` and `RColumn` components. This system replaces the legacy `RDataTable`/`RDataTableColumn` approach with better performance, cleaner syntax, and automatic type detection.

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

1. **Type Safety**: Compile-time checking of property expressions
2. **Reduced Reflection**: Property expressions compiled once
3. **Memory Efficiency**: No template overhead for simple columns
4. **Better IntelliSense**: Full IDE support for properties
5. **Cleaner Code**: Helper methods instead of complex templates

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