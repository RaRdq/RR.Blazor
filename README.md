# RR.Blazor - Universal Design System for Blazor

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![Blazor](https://img.shields.io/badge/Blazor-.NET%209-512BD4?logo=.net)](https://dotnet.microsoft.com/apps/aspnet/web-apps/blazor)
[![CSS](https://img.shields.io/badge/CSS-Modern%20Design%20System-1572B6?logo=css3)](https://www.w3.org/Style/CSS/)

## Overview

RR.Blazor is an ultra-generic, lightweight, and project-agnostic design system with enterprise-grade components and utilities for professional Blazor applications. Built primarily for AI coding agents working with Blazor, it provides a comprehensive set of components that work out of the box with zero configuration.

### Key Features

- 🎨 **100% Theme-aware**: Dynamic light/dark/high-contrast modes with CSS variables
- ♿ **Accessibility First**: WCAG 2.1 AA compliant with screen reader support
- 🚀 **Zero Dependencies**: Pure Blazor components, no external UI libraries
- 📱 **Fully Responsive**: Mobile-first design with 44px touch targets
- 🎯 **Type-Safe**: Full C# type safety with generic components
- ⚡ **Performance**: Optimized rendering with minimal re-renders
- 🔧 **Customizable**: 400+ CSS utility classes
- 📦 **Tree-Shakeable**: Use only what you need
- 🌈 **High Contrast Mode**: Built-in support for accessibility preferences
- 🎭 **Motion Preferences**: Respects prefers-reduced-motion

## Quick Start

### 1. Installation

```bash
dotnet add package RR.Blazor
```

### 2. Register Services

```csharp
// Program.cs
builder.Services.AddRRBlazor();
```

### 3. Add Theme Provider

```razor
<!-- App.razor -->
<ThemeProvider>
    <Router AppAssembly="@typeof(App).Assembly">
        <!-- Your app content -->
    </Router>
</ThemeProvider>
```

### 4. Import Styles

```html
<!-- index.html or App.razor -->
<link href="_content/RR.Blazor/css/rr-blazor.min.css" rel="stylesheet" />
```

### 5. Use Components

```razor
@using RR.Blazor.Components

<RCard Title="Welcome" Elevation="4">
    <RButton Text="Get Started" 
             Variant="ButtonVariant.Primary"
             OnClick="@HandleClick" />
</RCard>
```

## Component Library

### Core Components
- **RButton** - Versatile button with multiple variants and states
- **RCard** - Container component with elevation and glass effects
- **RModal** - Accessible modal dialog system
- **RSection** - Content section with collapsible support

### Form Components
- **RFormField** - Universal input component supporting 15+ field types
- **RDatePicker** - Date and time selection with calendar
- **RFileUpload** - Drag-and-drop file upload with preview

### Display Components
- **RDataTable** - Full-featured data grid with sorting and pagination
- **RBadge** - Status indicators and labels
- **RAlert** - Notification messages
- **RProgress** - Progress bars and indicators
- **REmptyState** - Empty content states
- **RSkeleton** - Loading skeletons

### Navigation
- **RTabs** - Tab navigation system
- **RBreadcrumbs** - Breadcrumb trail
- **RPagination** - Page navigation
- **RDropdown** - Dropdown menus

### Layout
- **RAppShell** - Complete application shell
- **RList** - List container with items
- **RAccordion** - Collapsible content panels

## CSS Utilities

RR.Blazor includes over 400 utility classes inspired by modern CSS frameworks:

### Spacing
- Padding: `pa-0` to `pa-32`
- Margin: `ma-0` to `ma-32`
- Gap: `gap-0` to `gap-24`

### Display & Layout
- Flexbox: `d-flex`, `justify-center`, `align-center`
- Grid: `d-grid`, `grid-cols-1` to `grid-cols-12`

### Typography
- Font sizes: `text-xs` to `text-9xl`
- Font weights: `font-light` to `font-black`
- Text alignment: `text-left`, `text-center`, `text-right`

### Effects
- Elevation: `elevation-0` to `elevation-24`
- Glassmorphism: `glass-light`, `glass-medium`, `glass-frost`
- Shadows: `shadow-sm` to `shadow-2xl`

## Professional Examples

### Executive Dashboard Card
```razor
<RCard Title="Financial Summary" 
       Elevation="4" 
       CssClass="glass-light">
    <div class="pa-6">
        <div class="d-flex justify-between align-center mb-4">
            <span class="text-h4 font-bold">$1,247,580</span>
            <RBadge Text="+8.2%" Variant="BadgeVariant.Success" />
        </div>
    </div>
</RCard>
```

### Data Management Interface
```razor
<RDataTable Items="@employees" 
            Striped="true" 
            Hoverable="true">
    <Columns>
        <RDataTableColumn Field="Name" Title="Employee" />
        <RDataTableColumn Field="Department" Title="Department" />
        <RDataTableColumn Field="Salary" Title="Salary" Format="C" />
    </Columns>
</RDataTable>
```

## Browser Support

- Chrome 90+
- Firefox 88+
- Safari 14+
- Edge 90+

## Documentation

For complete documentation, component examples, and API reference, visit the [RR.Blazor Documentation](https://github.com/rardqq/RR.Blazor/wiki).

## Contributing

We welcome contributions! Please see our [Contributing Guide](CONTRIBUTING.md) for details.

## License

MIT License - see [LICENSE](LICENSE) file for details.

## Author

Created and maintained by RaRdq (rardqq@gmail.com)

---

Built with ❤️ for the Blazor community