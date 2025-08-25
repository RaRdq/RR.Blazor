# Universal Template System for RR.Blazor

The Universal Template System provides a consistent, reusable way to render common data patterns across all RR.Blazor components including RGrid, RChoice, RTable, RList, and more.

## Overview

Instead of recreating templates for badges, currency, progress bars, avatars, and other common patterns in each component, the Universal Template System provides:

- **Reusable Templates**: Pre-built templates for common business scenarios
- **Type Safety**: Strongly typed templates that work with any data type T
- **Consistency**: Uniform styling and behavior across all components
- **Flexibility**: Both fluent API and declarative configuration options
- **Performance**: Compiled expressions for optimal rendering speed

## Core Templates

### 1. Badge Template
Display status indicators, counts, and labels with automatic color coding.

```csharp
// Simple usage
@RTemplates.Badge<Employee>(
    e => e.Status,
    e => GetStatusVariant(e.Status),
    e => "check_circle"
)(employee)

// With automatic status mapping
var template = new BadgeTemplate<Employee>
{
    PropertySelector = e => e.Status,
    StatusMapping = new Dictionary<string, VariantType>
    {
        { "active", VariantType.Success },
        { "inactive", VariantType.Danger },
        { "pending", VariantType.Warning }
    }
};
```

### 2. Currency Template
Format monetary values with proper localization and compact notation.

```csharp
// Basic currency display
@RTemplates.Currency<Employee>(e => e.Salary, "USD")(employee)

// Compact format (1.2K instead of 1,200)
@RTemplates.Currency<Employee>(e => e.Salary, "USD", compact: true)(employee)

// With color coding for positive/negative values
@RTemplates.Currency<Employee>(e => e.Balance, "USD", showColors: true)(employee)
```

### 3. Stack Template
Display multi-line information in vertical or horizontal layouts.

```csharp
// Vertical stack (default)
@RTemplates.Stack<Employee>(
    e => e.FullName,
    e => $"{e.Department} • {e.Position}",
    e => "person"
)(employee)

// Horizontal stack
@RTemplates.Stack<Employee>(
    e => e.Name,
    e => e.Email,
    orientation: StackOrientation.Horizontal
)(employee)
```

### 4. Group Template
Render collections of items with separators and overflow handling.

```csharp
// Simple comma-separated list
@RTemplates.Group<Project>(
    p => p.Tags,
    maxItems: 3
)(project)

// Custom separator and template
@RTemplates.Group<Team>(
    t => t.Members,
    itemTemplate: member => builder => { /* custom rendering */ },
    separator: " | ",
    maxItems: 5
)(team)
```

### 5. Avatar Template
Display user avatars with initials fallback and Gravatar support.

```csharp
// Basic avatar
@RTemplates.Avatar<User>(u => u.FullName)(user)

// With image URL
@RTemplates.Avatar<User>(
    u => u.FullName,
    u => u.ProfileImageUrl,
    SizeType.Large
)(user)

// With Gravatar support
var template = new AvatarTemplate<User>
{
    NameSelector = u => u.FullName,
    EmailSelector = u => u.Email,
    UseGravatar = true
};
```

### 6. Progress Template
Show progress bars and completion percentages.

```csharp
// Basic progress bar
@RTemplates.Progress<Task>(t => t.CompletionPercentage)(task)

// Custom styling and labels
@RTemplates.Progress<Project>(
    p => p.Progress,
    max: 100,
    showPercentage: true,
    variant: VariantType.Success
)(project)
```

### 7. Rating Template
Display star ratings with half-star support.

```csharp
// 5-star rating
@RTemplates.Rating<Product>(p => p.Rating)(product)

// Custom rating scale with value display
@RTemplates.Rating<Review>(
    r => r.Score,
    maxRating: 10,
    showValue: true,
    size: SizeType.Large
)(review)
```

## Integration with Components

### Table Columns

The Universal Template System integrates seamlessly with `ColumnDefinition<T>`:

```csharp
// Using fluent extensions
var columns = new List<ColumnDefinition<Employee>>
{
    TemplateExtensions.Column<Employee>("status", "Status")
        .WithProperty(e => e.Status)
        .UseBadgeTemplate(),
    
    TemplateExtensions.Column<Employee>("salary", "Salary")
        .WithProperty(e => e.Salary)
        .UseCurrencyTemplate("USD", compact: true),
    
    TemplateExtensions.Column<Employee>("info", "Employee Info")
        .UseStackTemplate(e => e.FullName, e => e.Department)
};

// Direct template assignment
var column = new ColumnDefinition<Employee>
{
    Title = "Performance",
    BadgeTemplate = new BadgeTemplate<Employee>
    {
        PropertySelector = e => e.Status,
        Variant = VariantType.Primary
    }
};
```

### Choice Components

Templates work with any component that accepts `RenderFragment<T>`:

```csharp
<RChoiceGeneric Items="@employees" TValue="Employee"
                ItemTemplate="@employees.AsBadgeTemplate(e => e.Status)">
</RChoiceGeneric>

<RChoiceGeneric Items="@users" TValue="User"
                ItemTemplate="@users.AsAvatarTemplate(u => u.FullName, u => u.ImageUrl)">
</RChoiceGeneric>
```

## Template Context

Each template uses a strongly-typed context for rendering:

```csharp
public class BadgeTemplateContext<T> : TemplateContext<T>
{
    public VariantType Variant { get; set; }
    public string Text { get; set; }
    public string Icon { get; set; }
    public bool Clickable { get; set; }
    public EventCallback<T> OnClick { get; set; }
}
```

Common properties available in all contexts:
- `Item`: The data item being rendered
- `Size`: Size variant (Small, Medium, Large, etc.)
- `Density`: Layout density (Compact, Dense, Normal, Spacious)
- `CssClass`: Additional CSS classes
- `Disabled`: Whether the item is disabled
- `Loading`: Whether the item is in loading state
- `Selected`: Whether the item is selected/active

## Advanced Configuration

### Custom Template Creation

Create your own templates by inheriting from `TemplateDefinition<T>`:

```csharp
public class ChipTemplate<T> : TemplateDefinition<T> where T : class
{
    public ChipStyle Style { get; set; } = ChipStyle.Default;
    public bool Closeable { get; set; }
    public EventCallback<T> OnClose { get; set; }

    public override RenderFragment Render(T item)
    {
        return builder =>
        {
            // Custom rendering logic
            builder.OpenElement(0, "div");
            builder.AddAttribute(1, "class", "custom-chip");
            builder.AddContent(2, FormatValue(GetValue(item)));
            builder.CloseElement();
        };
    }
}
```

### Template Builders

Use the fluent `TemplateBuilder<T>` for complex configurations:

```csharp
var badgeTemplate = TemplateBuilder<Employee>
    .Badge(e => e.Status)
    .Configure(template =>
    {
        template.Variant = VariantType.Success;
        template.Size = SizeType.Small;
        template.Clickable = true;
        template.StatusMapping = GetCustomStatusMappings();
    });
```

### Expression-Based Selectors

Templates support complex property selections:

```csharp
// Nested property access
var template = new StackTemplate<Employee>
{
    PrimaryTextSelector = e => e.PersonalInfo.FullName,
    SecondaryTextSelector = e => $"{e.JobInfo.Department} - {e.JobInfo.Title}",
    IconSelector = e => e.IsManager ? "admin_panel_settings" : "person"
};

// Conditional formatting
var currencyTemplate = new CurrencyTemplate<Transaction>
{
    PropertySelector = t => t.Amount,
    CurrencyCodeSelector = t => t.Account.Currency,
    Formatter = value => value < 0 ? $"({Math.Abs((decimal)value):C})" : $"{value:C}"
};
```

## Performance Considerations

1. **Expression Compilation**: Property selectors are compiled once and cached for performance
2. **Lightweight Rendering**: Templates render directly to HTML elements when possible
3. **Selective Updates**: Only affected templates re-render when data changes
4. **Memory Efficiency**: Templates reuse contexts and avoid unnecessary allocations

## Migration from Existing Templates

### From ColumnTemplateBuilder

**Before:**
```csharp
ColumnTemplateBuilder.StatusBadge<Employee>(e => e.Status, statusClasses)
```

**After:**
```csharp
RTemplates.Badge<Employee>(e => e.Status, e => GetVariant(e.Status))
```

### From Custom RenderFragment<T>

**Before:**
```csharp
RenderFragment<Employee> employeeTemplate = employee => builder =>
{
    builder.OpenElement(0, "div");
    builder.AddContent(1, employee.Name);
    builder.AddContent(2, employee.Department);
    builder.CloseElement();
};
```

**After:**
```csharp
RTemplates.Stack<Employee>(e => e.Name, e => e.Department)
```

## Best Practices

1. **Use Semantic Templates**: Choose templates that match your data semantics (Badge for status, Currency for money, etc.)
2. **Consistent Styling**: Use the same template configurations across your application
3. **Performance**: Prefer fluent API for simple cases, template objects for complex reusable configurations
4. **Accessibility**: Templates include proper ARIA attributes and semantic markup
5. **Extensibility**: Create custom templates for domain-specific patterns

## API Reference

### Core Classes
- `RTemplates`: Static factory methods for creating templates
- `TemplateDefinition<T>`: Base class for all templates
- `TemplateContext<T>`: Rendering context with item and configuration
- `TemplateBuilder<T>`: Fluent API for template creation

### Extension Methods
- `ColumnDefinition<T>.UseBadgeTemplate()`
- `ColumnDefinition<T>.UseCurrencyTemplate()`
- `ColumnDefinition<T>.UseStackTemplate()`
- `ColumnDefinition<T>.UseGroupTemplate()`
- `ColumnDefinition<T>.UseAvatarTemplate()`
- `ColumnDefinition<T>.UseProgressTemplate()`
- `ColumnDefinition<T>.UseRatingTemplate()`
- `IEnumerable<T>.AsBadgeTemplate()`
- `IEnumerable<T>.AsCurrencyTemplate()`
- `IEnumerable<T>.AsStackTemplate()`
- `IEnumerable<T>.AsGroupTemplate()`
- `IEnumerable<T>.AsAvatarTemplate()`
- `IEnumerable<T>.AsProgressTemplate()`
- `IEnumerable<T>.AsRatingTemplate()`

## Examples

See `TemplateSystemExamples.razor` for comprehensive examples of all template types and usage patterns.