# RR.Blazor Template System

## Folder Structure

```
RR.Blazor/Templates/
├── Chip/
│   ├── ChipTemplate.cs
│   ├── ChipContext.cs
│   └── ChipRenderer.cs
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
        .WithChipDefaults(chip => chip
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
// Chip
@RTemplates.Chip<Employee>(e => e.Status, e => GetStatusVariant(e.Status))

// Currency
@RTemplates.Currency<Employee>(e => e.Salary, "USD", compact: true)

// Stack
@RTemplates.Stack<Employee>(e => e.Name, e => $"{e.Department} • {e.Position}")
```

## Advanced Usage
```csharp
// Use RChip component directly
<RChip Variant="@GetDynamicVariant(employee)" 
       Icon="@GetStatusIcon(employee)"
       StyleVariant="ChipStyle.Badge"
       Clickable="true"
       OnClick="@(() => HandleEmployeeClick(employee))">
    @employee.Status
</RChip>

// Or use template registry
@RTemplates.Chip<Employee>(e => e.Status, e => GetStatusVariant(e.Status))
```

## Detection API

```csharp
var property = typeof(Employee).GetProperty(nameof(Employee.Status));
var suggestion = TemplateDetector.CreateSuggestion(property, employees);

// Returns: TemplateType, Confidence, AutoApply flag
```

## Migration

```csharp

// RChip template usage
@RTemplates.Chip<T>(e => e.Status, e => GetVariant(e.Status))

// Or direct RChip component
<RChip StyleVariant="ChipStyle.Badge" Variant="@GetVariant(item.Status)">
    @item.Status
</RChip>
```

## Testing

Location: `/Test/Templates/TemplateSystemExamples.razor`
Route: `/test/templates`