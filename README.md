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

## Installation

### From GitHub (Recommended)
```bash
# Clone as submodule
git submodule add https://github.com/RaRdq/RR.Blazor.git

# Reference in your project
<ProjectReference Include="RR.Blazor/RR.Blazor.csproj" />
```

### NuGet Package (Coming Soon)
```powershell
# Package Manager Console
Install-Package RR.Blazor

# .NET CLI
dotnet add package RR.Blazor

# PackageReference
<PackageReference Include="RR.Blazor" Version="1.0.0" />
```

## Quick Start

### 1. Register Services

```csharp
// Program.cs
builder.Services.AddRRBlazor();

// OR with customization
builder.Services.AddRRBlazor(blazor => blazor
    .WithTheme(theme => theme.Mode = ThemeMode.System)
    .WithAnimations(true)
);
```

### 2. Add Theme Provider

```razor
<!-- App.razor -->
<ThemeProvider>
    <Router AppAssembly="@typeof(App).Assembly">
        <!-- Your app content -->
    </Router>
</ThemeProvider>
```

### 3. Import Styles

```html
<!-- index.html or App.razor -->
<link href="_content/RR.Blazor/css/rr-blazor.min.css" rel="stylesheet" />
```

### 4. Use Components

```razor
@using RR.Blazor.Components

<RCard Title="Welcome" Elevation="4">
    <RButton Text="Get Started" 
             Variant="ButtonVariant.Primary"
             OnClick="@HandleClick" />
</RCard>
```

## AI Integration (Claude, GPT-4, etc.)

### Elite Designer Command Setup

RR.Blazor includes an **Elite Frontend Architect** Claude command that implements Anthropic's Plan-Implement-Reflect methodology for creating world-class user interfaces.

#### Installation
```bash
# 1. Copy the designer command to your project
cp RR.Blazor/.claude/commands/designer.md .claude/commands/

# 2. Use the designer command in Claude
/designer Plan a comprehensive data dashboard with real-time updates and mobile optimization
```

#### What the Designer Command Provides
- ✅ **Plan-Implement-Reflect mastery** (Anthropic 2024-2025 methodology)
- ✅ **Complete RR.Blazor expertise** (all 51 components + 800+ utilities)
- ✅ **2025 design trends** (neomorphism, glassmorphism, micro-animations)
- ✅ **Performance optimization** (virtual scrolling, lazy loading, 60fps)
- ✅ **Accessibility compliance** (WCAG 2.1 AA+, screen reader optimized)

#### Usage Examples
```bash
# Planning phase
/designer Plan an employee onboarding wizard with multi-step progress tracking

# Implementation phase  
/designer Implement the designed wizard using RR.Blazor components and utility-first styling

# Reflection phase
/designer Reflect on the implementation with accessibility and performance validation
```

### Quick Start with Claude Desktop
```bash
# 1. Add RR.Blazor context to Claude
Provide @RR.Blazor/RRBlazor.md

# 2. Initialize in new project
"Initialize my Blazor project with RR.Blazor design system"

# 3. Update existing project
"Update all my .razor files to use RR.Blazor components"
```

### Custom AI Commands
Add these to your AI assistant's context:
```markdown
/designer - Elite Frontend Architect with Plan-Implement-Reflect methodology
/rr-blazor-init - Initialize RR.Blazor in current project
/rr-blazor-upgrade - Upgrade components to latest patterns
/rr-blazor-theme - Configure theme and styling
/rr-blazor-component - Generate new component following patterns
```

### Example AI Prompts
```markdown
# Using the Elite Designer Command
/designer Create a modern data visualization dashboard with:
- Real-time chart updates
- Responsive grid layout
- Dark/light theme support
- Mobile-optimized interactions

# Using RR.Blazor Reference
Using @RR.Blazor/RRBlazor.md as reference:
1. Replace all custom cards with RCard
2. Replace all buttons with RButton
3. Update forms to use RFormField
4. Ensure theme-aware styling throughout
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

## Unified Icon System

All RR.Blazor components use a consistent Icon system for maximum flexibility and maintainability:

### Usage Pattern
```html
<!-- Unified Icon + IconPosition pattern -->
<RButton Icon="save" IconPosition="IconPosition.Start" Text="Save" />
<RCard Icon="dashboard" Title="Analytics Dashboard" />
<RSection Icon="settings" Title="Configuration" />
<RListItem Icon="person" IconPosition="IconPosition.End" Title="User Profile" />
```

### IconPosition Values
- `IconPosition.Start` - Icon at the beginning (default)
- `IconPosition.End` - Icon at the end  
- `IconPosition.Top` - Icon above content
- `IconPosition.Bottom` - Icon below content

### Benefits
- ✅ **Consistent API** across all components
- ✅ **Flexible positioning** with IconPosition enum
- ✅ **Clean code** without legacy properties
- ✅ **Type-safe** with C# enums

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
       Class="glass-light">
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

## RR.Core Integration (Optional)

RR.Blazor can optionally integrate with RR.Core for enhanced functionality:

```xml
<!-- Enable RR.Core in Directory.Build.props -->
<PropertyGroup>
  <RRCoreEnabled>true</RRCoreEnabled>
</PropertyGroup>
```

Then use the enhanced service registration:
```csharp
// With RR.Core
builder.Services
    .AddRRCore()
    .AddRRBlazor(blazor => blazor
        .WithTheme(theme => theme.Mode = ThemeMode.Dark)
    );
```

## Documentation

- [Complete Component Reference & Guide](RRBlazor.md) - The single source of truth
- [Contributing Guide](CONTRIBUTING.md) - AI-optimized development guide
- [Live Examples](https://github.com/RaRdq/RR.Blazor/wiki) - Coming soon

## Browser Support

- Chrome 90+
- Firefox 88+
- Safari 14+
- Edge 90+

## Contributing

We welcome contributions! Please see our [AI-Optimized Contributing Guide](CONTRIBUTING.md) for details.

## License

MIT License - see [LICENSE](LICENSE) file for details.

## Author

Created and maintained by RaRdq (rardqq@gmail.com)

---

Built with ❤️ for the Blazor community