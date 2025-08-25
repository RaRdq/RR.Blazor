# RR.Blazor Custom Theming Guide

RR.Blazor provides a powerful and flexible theming system that allows you to completely customize the look and feel of your application using SCSS variables with enterprise-grade security.

## Quick Start

### 1. Generate a Theme Template

```powershell
# Generate a basic theme
pwsh RR.Blazor/Scripts/GenerateTheme.ps1 -ThemeName "my-theme"

# Generate with all variables included
pwsh RR.Blazor/Scripts/GenerateTheme.ps1 -ThemeName "my-theme" -IncludeAllVariables
```

### 2. Register Your Theme

```csharp
// In Program.cs
builder.Services.AddRRBlazor(options =>
{
    options.WithCustomTheme("my-theme", "Themes/my-theme.scss");
});
```

### 3. Use Your Theme

```razor
<!-- In your component -->
<RThemeProvider Theme="my-theme">
    <Router AppAssembly="@typeof(App).Assembly">
        <!-- Your app content -->
    </Router>
</RThemeProvider>
```

## Theme Structure

A theme file is a standard SCSS file that defines CSS variables within a theme-specific selector:

```scss
:root[data-theme="my-theme"] {
  // Core Colors
  --theme-primary: #4687f1;
  --theme-success: #10b981;
  --theme-warning: #f59e0b;
  --theme-error: #ef4444;
  --theme-info: #06b6d4;
  
  // Surface Colors
  --theme-canvas: linear-gradient(135deg, #f8fafc 0%, #e2e8f0 100%);
  --theme-surface: #fafbfc;
  --theme-surface-elevated: #f4f5f7;
  
  // Text Colors
  --theme-text: #0f172a;
  --theme-text-muted: #475569;
  --theme-text-subtle: #94a3b8;
  
  // Shadows (5 elevation levels)
  --theme-shadow-sm: 0 2px 4px -1px rgb(0 0 0 / 0.06);
  --theme-shadow-md: 0 8px 25px -5px rgb(0 0 0 / 0.1);
  --theme-shadow-lg: 0 20px 40px -10px rgb(0 0 0 / 0.15);
  --theme-shadow-xl: 0 32px 64px -12px rgb(0 0 0 / 0.25);
  --theme-shadow-2xl: 0 48px 96px -16px rgb(0 0 0 / 0.35);
}
```

## Available Theme Variables

### Core Color Variables
- `--theme-primary`: Primary brand color
- `--theme-primary-hover`: Primary hover state
- `--theme-primary-active`: Primary active/pressed state
- `--theme-success`: Success/positive actions
- `--theme-warning`: Warning/caution states
- `--theme-error`: Error/danger states
- `--theme-info`: Informational states

### Surface Variables
- `--theme-canvas`: Background gradient or color
- `--theme-surface`: Card/component backgrounds
- `--theme-surface-elevated`: Elevated surface (modals, dropdowns)

### Text Variables
- `--theme-text`: Primary text color
- `--theme-text-muted`: Secondary/muted text
- `--theme-text-subtle`: Tertiary/disabled text

### Border & Shadow Variables
- `--theme-border`: Default border color
- `--theme-shadow-sm`: Small shadow (buttons, inputs)
- `--theme-shadow-md`: Medium shadow (cards)
- `--theme-shadow-lg`: Large shadow (modals)
- `--theme-shadow-xl`: Extra large shadow
- `--theme-shadow-2xl`: Maximum elevation shadow

### Gradient Variables
- `--theme-success-gradient`: Success gradient
- `--theme-warning-gradient`: Warning gradient
- `--theme-error-gradient`: Error gradient
- `--theme-info-gradient`: Info gradient
- `--theme-bg-gradient-*`: Background gradients for each state

### Additional Customization

You can override any RR.Blazor CSS variable:

```scss
:root[data-theme="my-theme"] {
  // Border radius customization
  --radius-sm: 2px;
  --radius-md: 4px;
  --radius-lg: 8px;
  
  // Spacing adjustments
  --space-4: 1.25rem; // Increase base spacing
  
  // Typography
  --font-family-primary: 'Inter', sans-serif;
  --text-base: 15px;
  
  // Component-specific
  --button-height: 48px;
  --modal-width: 600px;
}
```

## Complete Theme Example

```scss
// Themes/my-brand.scss - Comprehensive theming example
:root[data-theme="my-brand"] {
  // Method 1: Override default/dark theme variables
  --theme-primary: #003d82;        // Corporate blue
  --theme-success: #00875a;        // Override success color
  --theme-warning: #ff991f;        // Override warning color
  
  // Method 2: Create entirely new theme design
  --theme-canvas: linear-gradient(135deg, #f7f9fb 0%, #e8ecf1 100%);
  --theme-surface: #ffffff;        // Clean surfaces
  --theme-surface-elevated: #f7f9fb; // Elevated components
  
  // Method 3: Professional shadows system
  --theme-shadow-sm: 0 1px 2px rgba(9, 30, 66, 0.08);
  --theme-shadow-md: 0 4px 8px -2px rgba(9, 30, 66, 0.08);
  --theme-shadow-lg: 0 8px 16px -4px rgba(9, 30, 66, 0.08);
  
  // Method 4: Complete system customization
  // Typography
  --font-family-primary: 'Inter', sans-serif;
  --text-base: 15px;
  
  // Spacing adjustments  
  --space-4: 1.25rem;              // Increase base spacing
  
  // Border radius customization
  --radius-sm: 2px;
  --radius-md: 6px;                // Softer corners
  --radius-lg: 10px;
  
  // Component-specific overrides
  --button-height: 48px;           // Larger buttons
  --modal-width: 600px;            // Wider modals
  
  // Advanced: Custom gradients
  --theme-success-gradient: linear-gradient(135deg, #00875a 0%, #006644 100%);
  --theme-bg-gradient-primary: linear-gradient(135deg, #e4effa, #cce0f5);
}
```

## Advanced Usage

### Multiple Themes

```csharp
builder.Services.AddRRBlazor(options =>
{
    options.WithCustomThemes(new Dictionary<string, string>
    {
        ["corporate"] = "Themes/corporate.scss",
        ["festive"] = "Themes/festive.scss",
        ["high-contrast"] = "Themes/high-contrast.scss"
    });
});
```

### Dynamic Theme Switching

```razor
@inject IThemeService ThemeService

<RButton OnClick="@(() => ThemeService.SetThemeAsync("corporate"))">
    Corporate Theme
</RButton>

<RButton OnClick="@(() => ThemeService.SetThemeAsync("festive"))">
    Festive Theme
</RButton>
```

### Theme Inheritance

You can extend existing themes:

```scss
// Import base theme variables
@use '~RR.Blazor/Styles/themes/default' as default;

:root[data-theme="my-extended-theme"] {
  // Copy all default theme variables
  @extend :root;
  
  // Override specific ones
  --theme-primary: #ff6b6b;
  --theme-success: #51cf66;
}
```

## Best Practices

1. **Start with the Generator**: Use the PowerShell script to create a template with all available variables.

2. **Test Accessibility**: Ensure sufficient color contrast ratios (WCAG AA minimum).

3. **Use Semantic Variables**: Override theme-level variables rather than component-specific ones when possible.

4. **Keep It Organized**: Group related variables together with comments.

5. **Test Responsively**: Ensure your theme works well on all device sizes.

## Troubleshooting

### Theme Not Loading
- Ensure the SCSS file path is correct relative to your project root
- Check that the theme name matches between registration and usage
- Verify the SCSS syntax is valid

### Build Errors
- Make sure you have `AspNetCore.SassCompiler` package installed
- Check for SCSS syntax errors in your theme file
- Ensure all imported paths are correct

### Hot Reload
- Theme changes require a rebuild to take effect
- Use `dotnet watch` for automatic rebuilding during development

## Complete Example

```csharp
// Program.cs
builder.Services.AddRRBlazor(options =>
{
    options.WithCustomTheme("acme-corp", "Themes/acme-corp.scss")
           .WithAnimations(true)
           .WithToasts(toast =>
           {
               toast.Position = ToastPosition.BottomRight;
               toast.DefaultDuration = 3000;
           });
});

// App.razor
<RThemeProvider Theme="acme-corp">
    <Router AppAssembly="@typeof(App).Assembly">
        <Found Context="routeData">
            <RouteView RouteData="@routeData" DefaultLayout="@typeof(MainLayout)" />
        </Found>
        <NotFound>
            <PageTitle>Not found</PageTitle>
            <LayoutView Layout="@typeof(MainLayout)">
                <p role="alert">Sorry, there's nothing at this address.</p>
            </LayoutView>
        </NotFound>
    </Router>
</RThemeProvider>
```

## Security Features

RR.Blazor theming includes enterprise-grade security:

- **🔒 Path Traversal Protection**: Prevents `../` attacks and directory escape attempts
- **🛡️ Input Sanitization**: Theme names validated against reserved system names  
- **⚡ Build-Time Validation**: SCSS syntax validation and compilation errors caught early
- **🎯 Atomic Operations**: Theme files written atomically with automatic backup
- **📦 Dependency Injection**: Clean architecture with proper service interfaces

## VS Code Integration

RR.Blazor includes VS Code snippets for rapid theme development:

- **`rrtheme`**: Complete theme template with all core variables
- **`rrvar`**: Individual CSS variable snippet with IntelliSense
- **`rrshadow`**: Shadow system variables
- **`rrgradient`**: Gradient system variables

Snippets are automatically available in `.vscode/rr-blazor-theme.code-snippets`.

## Best Practices

Add to your `.vscode/settings.json` for optimal development:

```json
{
  "emmet.includeLanguages": {
    "scss": "scss"
  },
  "scss.completion.completePropertyWithSemicolon": true,
  "scss.completion.triggerPropertyValueCompletion": true
}
```

Theme snippet for `.vscode/scss.code-snippets`:

```json
{
  "RR.Blazor Theme": {
    "prefix": "rrtheme",
    "body": [
      ":root[data-theme=\"${1:theme-name}\"] {",
      "  // Core Colors",
      "  --theme-primary: ${2:#4687f1};",
      "  --theme-success: ${3:#10b981};",
      "  --theme-warning: ${4:#f59e0b};",
      "  --theme-error: ${5:#ef4444};",
      "  --theme-info: ${6:#06b6d4};",
      "  ",
      "  // Surface Colors",
      "  --theme-canvas: ${7:#f8fafc};",
      "  --theme-surface: ${8:#fafbfc};",
      "  --theme-surface-elevated: ${9:#f4f5f7};",
      "  ",
      "  // Text Colors",
      "  --theme-text: ${10:#0f172a};",
      "  --theme-text-muted: ${11:#475569};",
      "  --theme-text-subtle: ${12:#94a3b8};",
      "  ",
      "  // Border",
      "  --theme-border: ${13:#e2e8f0};",
      "  ",
      "  // Shadows",
      "  --theme-shadow-sm: ${14:0 2px 4px -1px rgb(0 0 0 / 0.06)};",
      "  --theme-shadow-md: ${15:0 8px 25px -5px rgb(0 0 0 / 0.1)};",
      "}"
    ],
    "description": "RR.Blazor theme template"
  }
}
```