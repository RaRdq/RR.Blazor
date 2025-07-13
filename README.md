# RR.Blazor - Universal Design System for Blazor

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![Blazor](https://img.shields.io/badge/Blazor-.NET%209-512BD4?logo=.net)](https://dotnet.microsoft.com/apps/aspnet/web-apps/blazor)
[![AI-First](https://img.shields.io/badge/AI-First%20Design-00D2FF)](https://github.com/RaRdq/RR.Blazor)

## Overview

RR.Blazor is an ultra-generic, lightweight, and project-agnostic design system with enterprise-grade components and utilities for professional Blazor applications. Built primarily for AI coding agents working with Blazor, it provides a comprehensive set of components that work out of the box with zero configuration.

AI agents consistently hallucinate component APIs, mix framework versions, and generate inconsistent CSS. RR.Blazor eliminates this by providing predictable patterns and comprehensive machine-readable documentation.

## Key Features

- **100% Theme-aware**: Dynamic light/dark/high-contrast modes with CSS variables
- **Accessibility First**: WCAG 2.1 AA compliant with screen reader support  
- **Zero Dependencies**: Pure Blazor components, no external UI libraries
- **Fully Responsive**: Mobile-first design with 44px touch targets
- **Type-Safe**: Full C# type safety with generic components
- **Performance**: Optimized rendering with minimal re-renders
- **Customizable**: 800+ CSS utility classes
- **Tree-Shakeable**: Use only what you need
- **High Contrast Mode**: Built-in support for accessibility preferences
- **Motion Preferences**: Respects prefers-reduced-motion
- **AI-First Documentation**: Auto-generated JSON schema optimized for AI consumption

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
// Program.cs - Basic setup
builder.Services.AddRRBlazor();

// Program.cs - With configuration
builder.Services.AddRRBlazor(blazor => blazor
    .WithTheme(theme => theme
        .Mode = ThemeMode.System
        .PrimaryColor = "#3498DB"
        .WithAnimations(true)
    )
);
```

### 2. Add Theme Provider

```razor
<!-- App.razor -->
<RThemeProvider>
    <Router AppAssembly="@typeof(App).Assembly">
        <Found Context="routeData">
            <RouteView RouteData="@routeData" DefaultLayout="@typeof(MainLayout)" />
        </Found>
    </Router>
</RThemeProvider>
```

### 3. Import Styles

```html
<!-- wwwroot/index.html -->
<link href="_content/RR.Blazor/css/main.css" rel="stylesheet" />
```

### 4. Import Components

```razor
<!-- _Imports.razor -->
@using RR.Blazor.Components
@using RR.Blazor.Enums
```

### 5. Use Components

```razor
@using RR.Blazor.Components

<RCard Title="Welcome" Elevation="4" Class="glass-light">
    <div class="pa-6">
        <RButton Text="Get Started" 
                 Variant="ButtonVariant.Primary"
                 Icon="rocket_launch"
                 IconPosition="IconPosition.Start"
                 OnClick="@HandleClick" />
    </div>
</RCard>
```

## AI Integration

### Auto-Generated Documentation

RR.Blazor features an AI-first documentation system that automatically generates comprehensive, machine-readable documentation for AI agents.

**Location**: `wwwroot/rr-ai-docs.json`

**Contains**:
- 49 components with complete API documentation
- 800+ utility patterns with AI hints  
- 8 CSS variable pattern categories
- 8 real-world usage patterns
- Best practices and accessibility guidelines

### For Claude Code Users

```bash
# Load the AI-optimized documentation
# File: RR.Blazor/wwwroot/rr-ai-docs.json contains everything AI needs

# Generate new documentation manually
pwsh ./RR.Blazor/Scripts/GenerateAIDocsAdvanced.ps1 -ProjectPath ./RR.Blazor

# Documentation regenerates automatically on Release builds
dotnet build -c Release
```

### Designer Command Setup

RR.Blazor includes an Elite Frontend Architect Claude command that implements Plan-Implement-Reflect methodology for creating professional user interfaces.

**Installation**:
```bash
# 1. Copy the designer command to your project
cp RR.Blazor/.claude/commands/designer.md .claude/commands/

# 2. Use the designer command in Claude
/designer Plan a comprehensive data dashboard with real-time updates and mobile optimization
```

**What the Designer Command Provides**:
- Plan-Implement-Reflect mastery (current methodology)
- Complete RR.Blazor expertise (all 49 components + 800+ utilities)  
- Modern design patterns (glassmorphism, micro-animations)
- Performance optimization (virtual scrolling, lazy loading, 60fps)
- Accessibility compliance (WCAG 2.1 AA+, screen reader optimized)

**Usage Examples**:
```bash
# Planning phase
/designer Plan an employee onboarding wizard with multi-step progress tracking

# Implementation phase  
/designer Implement the designed wizard using RR.Blazor components and utility-first styling

# Reflection phase
/designer Reflect on the implementation with accessibility and performance validation
```

### AI Prompts

```
Using RR.Blazor components from rr-ai-docs.json, create a user management interface with:
- Search and filter functionality using RFormField
- Data display using RDataTable with elevation-2
- Action buttons using RButton Primary variant  
- Professional styling with glass-light and pa-6 utility classes
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

## Component Library

### Core Components (8)
- **RButton** - Versatile button with multiple variants and states
- **RCard** - Container component with elevation and glass effects  
- **RBadge** - Status indicators and labels
- **RAvatar** - User profile images and initials
- **RDivider** - Content separation lines
- **RSectionDivider** - Section headers with icons
- **RThemeProvider** - Theme management wrapper
- **RThemeSwitcher** - User theme toggle control

### Form Components (7)
- **RForm** - Form container with validation
- **RFormField** - Universal input component supporting 15+ field types
- **RFormSection** - Organized form layout sections
- **RDatePicker** - Date and time selection with calendar
- **RDatePickerBasic** - Simplified date picker
- **RFileUpload** - Drag-and-drop file upload with preview
- **RSwitcher** - Toggle switch control

### Data Components (6)
- **RDataTable** - Full-featured data grid with sorting and pagination
- **RDataTableColumn** - Table column configuration
- **RList** - List container with items
- **RListItem** - Individual list entries
- **RVirtualList** - Performance-optimized virtual scrolling
- **RFilterBar** - Data filtering interface

### Display Components (10)
- **RAccordion** - Collapsible content panels
- **RAccordionItem** - Individual accordion sections
- **REmptyState** - Empty content states
- **RInfoItem** - Information display cards
- **RMetric** - Statistical data presentation
- **RProgressBar** - Progress indicators
- **RSkeleton** - Loading skeletons
- **RStatsCard** - Dashboard statistics cards
- **RSummaryItem** - Summary information display
- **RTimeline** - Event timeline visualization

### Feedback Components (10)
- **RConfirmationModal** - User confirmation dialogs
- **RConfirmModal** - Simple confirm/cancel modals
- **RDetailModal** - Detailed information modals
- **RFormModal** - Modal forms
- **RMessageModal** - Message display modals
- **RModal** - Base modal component
- **RModalProvider** - Modal management service
- **RPreviewModal** - Content preview modals
- **RSelectModal** - Selection interface modals
- **RToastContainer** - Toast notification system

### Navigation Components (5)
- **RBreadcrumbs** - Breadcrumb trail navigation
- **RDropdown** - Dropdown menu system
- **RNavMenu** - Main navigation menus
- **RTabItem** - Individual tab components
- **RTabs** - Tab navigation system

### Layout Components (3)
- **RAppShell** - Complete application shell
- **RSection** - Content section containers

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
- Consistent API across all components
- Flexible positioning with IconPosition enum
- Clean code without legacy properties
- Type-safe with C# enums

## CSS Utilities

RR.Blazor includes 800+ utility classes inspired by modern CSS frameworks:

### Spacing (MudBlazor-inspired)
```html
<!-- Padding: pa-{size}, px-{size}, py-{size} -->
<div class="pa-6">Standard card padding</div>
<div class="px-4 py-2">Button padding</div>

<!-- Margin: ma-{size}, mx-{size}, my-{size} -->
<div class="ma-4 mx-auto">Centered with margin</div>
<div class="mb-4">Bottom margin only</div>

<!-- Gap for flex/grid -->
<div class="d-flex gap-4">Flex items with gap</div>
```

### Display & Layout
```html
<!-- Flexbox -->
<div class="d-flex justify-center align-center">
<div class="d-flex justify-between align-center">

<!-- Grid -->
<div class="d-grid grid-cols-1 grid-cols-md-2">
<div class="d-grid grid-cols-12">
```

### Typography
```html
<!-- Semantic typography -->
<h1 class="text-h4 font-semibold">Section header</h1>
<p class="text-body-1 text--secondary">Supporting text</p>

<!-- Font weights and sizes -->
<span class="font-bold text-xl">Important text</span>
<span class="font-light text-sm">Light text</span>
```

### Visual Effects
```html
<!-- Elevation (0-24 levels) -->
<div class="elevation-4">Standard elevation</div>
<div class="elevation-8 hover:elevation-12">Interactive elevation</div>

<!-- Glassmorphism -->
<div class="glass-light">Subtle glass effect</div>
<div class="glass-frost backdrop-blur-xl">Heavy glass effect</div>

<!-- Shadows -->
<div class="shadow-md">Medium shadow</div>
<div class="shadow-lg">Large shadow</div>
```

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
        <div class="d-flex gap-4 mb-4">
            <div class="flex-grow-1 text-center pa-3 bg-elevated rounded">
                <div class="text-body-2 text--secondary mb-1">Revenue</div>
                <div class="text-h6 font-semibold">$892K</div>
            </div>
            <div class="flex-grow-1 text-center pa-3 bg-elevated rounded">
                <div class="text-body-2 text--secondary mb-1">Profit</div>
                <div class="text-h6 font-semibold">$355K</div>
            </div>
        </div>
    </div>
</RCard>
```

### Data Management Interface
```razor
<div class="d-flex justify-between align-center mb-4">
    <RFormField Type="FieldType.Search" 
                Placeholder="Search employees..." 
                Class="flex-grow-1 mr-4" />
    <RButton Text="Add Employee" 
             Variant="ButtonVariant.Primary" 
             Icon="add" 
             IconPosition="IconPosition.Start" />
</div>

<RDataTable Items="@employees" 
            Class="elevation-2" 
            Striped="true" 
            Hoverable="true">
    <Columns>
        <RDataTableColumn Field="Name" Title="Employee" />
        <RDataTableColumn Field="Department" Title="Department" />
        <RDataTableColumn Field="Salary" Title="Salary" Format="C" />
    </Columns>
</RDataTable>
```

### Form with Validation
```razor
<RForm TModel="UserModel" @bind-Model="model" OnValidSubmit="@HandleSubmit">
    <FormFields>
        <RFormField Label="Email Address"
                    FieldType="FieldType.Email"
                    @bind-Value="model.Email"
                    Size="FieldSize.Large"
                    Variant="FieldVariant.FloatingLabel"
                    StartIcon="email"
                    Required />
                    
        <RFormField Label="Password"
                    FieldType="@(showPassword ? FieldType.Text : FieldType.Password)"
                    @bind-Value="model.Password"
                    StartIcon="lock"
                    EndIcon="@(showPassword ? "visibility_off" : "visibility")"
                    OnEndIconClick="TogglePasswordVisibility"
                    Required />
    </FormFields>
</RForm>
```

## Advanced Features

### Virtual Scrolling
```razor
<RVirtualList Items="@thousandsOfItems" ItemHeight="60">
    <ItemTemplate Context="item">
        <RListItem Title="@item.Name" Subtitle="@item.Description" />
    </ItemTemplate>
</RVirtualList>
```

### Modal System
```razor
@inject IModalService ModalService

@code {
    private async Task ShowConfirmation()
    {
        var result = await ModalService.ShowAsync<RConfirmModal>(new ModalParameters
        {
            ["Title"] = "Confirm Delete",
            ["Message"] = "This action cannot be undone.",
            ["ConfirmText"] = "Delete",
            ["CancelText"] = "Cancel"
        });
        
        if (result.Confirmed)
        {
            // Handle confirmation
        }
    }
}
```

### Toast Notifications
```razor
@inject IToastService ToastService

@code {
    private void ShowSuccess()
    {
        ToastService.ShowSuccess("Operation completed successfully");
    }
    
    private void ShowError()
    {
        ToastService.ShowError("An error occurred", "Error Details");
    }
}
```

## Theme System

### Automatic Theme Detection
```csharp
// Follows system preference by default
builder.Services.AddRRBlazor(blazor => blazor
    .WithTheme(theme => theme.Mode = ThemeMode.System)
);
```

### Manual Theme Control
```razor
<!-- Theme switcher component -->
<RThemeSwitcher />

<!-- Programmatic theme switching -->
@inject IThemeService ThemeService

@code {
    private async Task SwitchToDark()
    {
        await ThemeService.SetThemeAsync(ThemeMode.Dark);
    }
}
```

### CSS Variables
All components use semantic CSS variables:
```css
/* Colors */
--color-text-primary
--color-background-elevated
--color-interactive-primary

/* Spacing */
--space-0 through --space-24

/* Effects */
--shadow-md, --shadow-lg
--gradient-subtle, --gradient-executive
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

## Performance

### Optimizations
- Minimal re-renders through careful change detection
- Virtual scrolling for large datasets
- CSS-in-CSS approach (no runtime style generation)
- Tree-shakeable utility classes
- Precompiled SCSS with design tokens

### Benchmarks
- Page load: <3 seconds for 10k records
- First contentful paint: <1.5 seconds
- Memory usage: <50MB for typical applications

## Browser Support

- Chrome 90+
- Firefox 88+  
- Safari 14+
- Edge 90+

Modern CSS features used: CSS Grid, Flexbox, CSS Variables, backdrop-filter

## Documentation

- [AI-Optimized Component Documentation](wwwroot/rr-ai-docs.json) - Machine-readable component reference
- [Contributing Guide](CONTRIBUTING.md) - AI-optimized development guide
- Live Examples - Coming soon

## Development Tools

RR.Blazor includes integrated development tools to streamline component development:

### CLI Tool
```bash
# Run CLI commands
dotnet run --project Tools/CLI -- validate --check-patterns
dotnet run --project Tools/CLI -- generate component --name RNewComponent
dotnet run --project Tools/CLI -- add --template card --name CustomerCard
```

### Roslyn Analyzers
```xml
<!-- Enable analyzers in your project -->
<PropertyGroup>
  <RRBlazorAnalyzersEnabled>true</RRBlazorAnalyzersEnabled>
</PropertyGroup>
```

The analyzers provide:
- Component pattern validation
- AI metadata verification
- Theme usage compliance
- Accessibility rule checking

## Building from Source

```bash
# Clone repository
git clone https://github.com/RaRdq/RR.Blazor.git
cd RR.Blazor

# Generate AI documentation
pwsh ./Scripts/GenerateAIDocsAdvanced.ps1 -ProjectPath . -OutputPath wwwroot/rr-ai-docs.json

# Build project with tools
dotnet build

# Build with documentation generation (Release only)
dotnet build -c Release
```

## Contributing

We welcome contributions! Please see our AI-Optimized Contributing Guide for details.

Requirements:
- Follow existing component patterns
- Add AI metadata to new components
- Include utility-first styling examples
- Update auto-generated documentation

Component structure:
```razor
@**
<summary>Component description for AI</summary>
<category>Core|Forms|Data|Display|Feedback|Navigation|Layout</category>
<complexity>Simple|Medium|Complex</complexity>
<ai-prompt>search terms for AI discovery</ai-prompt>
<ai-common-use>common use cases</ai-common-use>
**@

<!-- Component markup -->
@code {
    // Component logic
}
```

## License

MIT License - see [LICENSE](LICENSE) file for details.

## Author

Created and maintained by RaRdq (rardqq@gmail.com)

---

Built with ❤️ for the Blazor community