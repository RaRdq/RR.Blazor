# RR.Blazor Template System

## Folder Structure

```
RR.Blazor/Templates/
├── Badge/
│   ├── BadgeTemplate.cs
│   ├── BadgeContext.cs
│   └── BadgeRenderer.cs
├── Currency/
│   ├── CurrencyTemplate.cs
│   ├── CurrencyContext.cs
│   └── CurrencyRenderer.cs
├── Stack/
│   ├── StackTemplate.cs
│   ├── StackContext.cs
│   └── StackRenderer.cs
├── Detection/
│   ├── TemplateDetector.cs
│   └── TemplateSuggestion.cs
├── Configuration/
│   ├── TemplateConfiguration.cs
│   └── TemplateConfigurationBuilder.cs
├── TemplateRegistry.cs
├── RTemplates.cs
└── RTemplates_Original.cs
```

## Template Detection
```csharp
var suggestion = TemplateDetector.CreateSuggestion<Employee>(property, sampleData);
if (suggestion.AutoApply) {
    // Apply template automatically
}

@RTemplates.Auto(employee, nameof(Employee.Status))
@RTemplates.Auto(employee, nameof(Employee.Salary))
```

## Configuration
```csharp
services.AddRRBlazor(config => config
    .WithTemplates(templates => templates
        .WithBadgeDefaults(badge => badge
            .WithVariant(VariantType.Success)
            .AddStatusMapping("active", VariantType.Success)
            .AddStatusMapping("pending", VariantType.Warning))
        .WithCurrencyDefaults(currency => currency
            .WithCurrencyCode("USD")
            .Compact(true)
            .WithAutoCompactThreshold(100_000))
        .WithStackDefaults(stack => stack
            .WithOrientation(StackOrientation.Vertical)
            .TruncateText(true))
        .WithSmartDetection(detection => detection
            .Enable(true)
            .WithAutoApplyThreshold(0.8)
            .AnalyzeSampleData(true))));
```

## Usage

```csharp
// Badge
@RTemplates.Badge<Employee>(e => e.Status, e => GetStatusVariant(e.Status))

// Currency
@RTemplates.Currency<Employee>(e => e.Salary, "USD", compact: true)

// Stack
@RTemplates.Stack<Employee>(e => e.Name, e => $"{e.Department} • {e.Position}")
```

## Advanced Usage
```csharp
var badgeTemplate = new BadgeTemplate<Employee>
{
    TextSelector = e => e.Status,
    VariantSelector = e => GetDynamicVariant(e),
    IconSelector = e => GetStatusIcon(e),
    Clickable = true,
    OnClick = EventCallback.Factory.Create<Employee>(this, HandleEmployeeClick)
};

@badgeTemplate.Render(employee)
```

## Detection API

```csharp
var property = typeof(Employee).GetProperty(nameof(Employee.Status));
var suggestion = TemplateDetector.CreateSuggestion(property, employees);

// Returns: TemplateType, Confidence, AutoApply flag
```

## Migration

```csharp
// Old
@RTemplates.BadgeTemplate(badgeTemplate, item)

// New
@RTemplates.Badge<T>(e => e.Status, e => GetVariant(e.Status))
```

## Testing

Location: `/Test/Templates/TemplateSystemExamples.razor`
Route: `/test/templates`