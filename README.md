# RR.Blazor

[![.NET](https://img.shields.io/badge/.NET-9-512BD4?logo=.net&logoColor=white)](https://dotnet.microsoft.com/en-us/download/dotnet/9.0)
[![Blazor](https://img.shields.io/badge/Blazor-WebAssembly%20%7C%20Server-512BD4?logo=blazor&logoColor=white)](https://blazor.net)
[![License: MIT](https://img.shields.io/badge/License-MIT-green.svg)](LICENSE)
[![Buy me a coffee](https://img.shields.io/badge/Support%20Us-Buy%20Me%20A%20Coffee-FF813F?logo=buy-me-a-coffee&logoColor=white)](https://rr-store.lemonsqueezy.com/buy/ef6f32b3-19ae-4d3f-a9d3-bfa83022f594)
<img src="https://cdn3.emoji.gg/emojis/5963-shrek-cat.png" alt="Shrek Cat" width="24" height="24" style="vertical-align:middle; margin-left:8px;" />

Modern Blazor component library with 63 components, utility-first styling, and AI-optimized documentation.

## Live Demo

**[View Components Demo →](https://rrblazor.dev)**

## Features

- **Zero Configuration** - Works out-of-the-box with sensible defaults
- **63 Components** - Complete UI toolkit from buttons to data grids
- **Smart Table-Chart Integration** - Tables as data sources for charts with real-time binding
- **Enterprise Virtualization** - Handle 100k+ rows with `RTableVirtualized` and `RVirtualList`
- **Intelligent Search System** - Built-in search with collapsible interface and role-based filtering
- **Smart Type Detection** - Auto-detects generics, eliminating boilerplate
- **3,300+ Utilities** - Comprehensive CSS utility classes
- **Tree-Shakeable CSS** - Advanced optimization reduces bundle size by 87%+ (727KB→92KB)
- **Theme System** - Light/dark modes with CSS variables
- **AI-Optimized** - Machine-readable documentation for AI coding

## Quick Start

### 1. Add as submodule

```bash
git submodule add https://github.com/RaRdq/RR.Blazor.git
```

### 2. Configure your project

```xml
<!-- In your .csproj file -->
<Project Sdk="Microsoft.NET.Sdk.BlazorWebAssembly">
  <!-- ... other configuration ... -->
  
  <ItemGroup>
    <ProjectReference Include="..\RR.Blazor\RR.Blazor.csproj" />
  </ItemGroup>
  
  <!-- Enable build-time code generation features -->
  <!-- This enables: Modal registry auto-discovery, CSS tree-shaking, validation, and more -->
  <Import Project="..\RR.Blazor\build\RR.Blazor.targets" />
</Project>
```

### 3. Register services

```csharp
// Program.cs
var builder = WebApplication.CreateBuilder(args);

// REQUIRED for Blazor Server: Enable static web assets
builder.WebHost.UseStaticWebAssets();

// Register RR.Blazor services (includes modal system, templates, etc.)
builder.Services.AddRRBlazor();
```

For Blazor WebAssembly projects:
```csharp
// Program.cs
builder.Services.AddRRBlazor();
```

### 3. Add CSS and JavaScript references

Choose the appropriate file based on your Blazor hosting model:

#### Blazor WebAssembly (WASM)
```html
<!-- wwwroot/index.html -->
<head>
    <link href="_content/RR.Blazor/css/main.css" rel="stylesheet" />
</head>
<body>
    <script type="module" src="_content/RR.Blazor/js/rr-blazor.js"></script>
</body>
```

#### Blazor Server
```html
<!-- Pages/_Host.cshtml or Pages/_Layout.cshtml -->
<head>
    <link href="_content/RR.Blazor/css/main.css" rel="stylesheet" />
</head>
<body>
    <script type="module" src="_content/RR.Blazor/js/rr-blazor.js"></script>
</body>
```

#### Blazor Web (Interactive Server/WASM)
```razor
<!-- Components/App.razor or layout components -->
<head>
    <link href="_content/RR.Blazor/css/main.css" rel="stylesheet" />
</head>
<body>
    <script type="module" src="_content/RR.Blazor/js/rr-blazor.js"></script>
</body>
```

### 4. Use RAppShell (Zero Hustle)

```razor
<!-- MainLayout.razor - Complete app shell with everything included -->
<RAppShell>
    @Body
</RAppShell>
```

**That's it!** RAppShell includes theme provider, portal-based modal system (with ModalProvider), toast container, intelligent search system, and styles.

### Alternative: Manual Setup

```razor
<!-- _Imports.razor -->
@using RR.Blazor.Components
@using RR.Blazor.Enums
```

```razor
<!-- MainLayout.razor -->
<RThemeProvider>
    <RToastContainer />
    <ModalProvider />
    @Body
</RThemeProvider>
```

## Usage Guide

### 🤖 AI Agent Integration
For AI agents (Claude, GPT-4, etc.), add a rule or manually refer to [`@RR.Blazor\_Documentation\RRAI.md`](_Documentation/RRAI.md) for comprehensive component documentation, patterns, and AI-optimized examples.

### Smart Components

```razor
<!-- Auto-detects User type from Items -->
<RDropdown Items="users" @bind-SelectedValue="selectedUser" />

<!-- Auto-detects model type -->
<RForm Model="user" OnValidSubmit="SaveUser" />
```

### Smart Data Tables & Chart Integration

**Zero-Config Tables with Built-in Chart Integration:**

```razor
<!-- Auto-generates columns, includes search, sorting, pagination -->
<RTable Items="@salesData" 
        Title="Sales Report"
        ShowChartButton="true"
        ShowSearch="true" />

<!-- High-performance virtualization for large datasets -->
<RTableVirtualized Items="@largeDataset" 
                   Height="600px"
                   ShowChartButton="true"
                   ExportEnabled="true" />

<!-- Smart chart that auto-detects best visualization -->
<RChart Data="@tableRef.FilteredData" 
        Title="Dynamic Analytics" />
```

**Table-Chart Data Binding:**
- Tables automatically expose `FilteredData` property for chart binding
- Real-time chart updates as table data changes
- Built-in chart modal with one-click visualization
- Smart chart type detection based on data structure

### Professional UI Components

```razor
<RCard Title="Dashboard" Elevation="4" Class="pa-6">
    <RButton Text="Save Changes" 
             Icon="save" 
             IconPosition="IconPosition.Start"
             OnClick="HandleSave" />
</RCard>

<RTable Items="@employees" />  @* Zero configuration - auto-generates all columns! *@
```

### Forms with Validation

```razor
<RForm Model="user" OnValidSubmit="SaveUser">
    <RTextInput @bind-value="user.Email" Type="email" Required />
    <RTextInput @bind-value="user.Password" Type="password" Required />
    <RButton Text="Register" Type="ButtonType.Submit" />
</RForm>
```

### Toast Notifications

```razor
@inject IToastService ToastService

<RButton Text="Show Success" 
         OnClick="@(() => ToastService.ShowSuccess("Operation completed!"))" />
```

### Responsive Images with Lazy Loading

```razor
<!-- Basic image with lazy loading and blur placeholder -->
<RImage Src="/api/products/123/image" 
        Alt="Product thumbnail"
        LazyLoading="true"
        ShowSkeleton="true" />

<!-- Advanced image with responsive sources -->
<RImage Src="/images/hero.jpg"
        Alt="Hero banner"
        Srcset="/images/hero-sm.jpg 640w, /images/hero-md.jpg 1024w, /images/hero-lg.jpg 1920w"
        Sizes="(max-width: 640px) 100vw, (max-width: 1024px) 50vw, 33vw"
        AspectRatio="16/9"
        ObjectFit="ObjectFit.Cover"
        Priority="true" />

<!-- Avatar with fallback -->
<RImage Src="@user.AvatarUrl"
        Alt="@user.Name"
        Variant="ImageVariant.Circle"
        Width="64"
        Height="64"
        ErrorSrc="/images/default-avatar.png" />
```

### Modal System

RR.Blazor provides a unified modal system with 4 distinct usage patterns. All patterns use the same portal-based rendering, backdrop management, and z-index stacking.

#### **Case 1: Raw Content Components** (WITHOUT internal RModal)
Components without RModal wrapper - Provider auto-wraps them. Use `ShowRawAsync<T>()` for strongly-typed results.
```razor
// Component without internal RModal - gets wrapped automatically
var result = await ModalService.ShowRawAsync<UserData>(
    typeof(UserFormContent),
    parameters: new() { { "UserId", userId } },
    options: new() { Title = "Edit User", Size = SizeType.Large }
);
if (result.ResultType == ModalResult.Ok)
{
    var userData = result.Data; // Strongly typed!
}
```

#### **Case 2: Self-Contained Components** (WITH internal RModal)
Components with their own `<RModal>` wrapper - Provider renders directly without double-wrapping.
```razor
// Component has <RModal> inside - no double wrap
await ModalService.ShowAsync(new ModalOptions {
    ComponentType = typeof(SettingsModalWithWrapper),
    Parameters = new() { { "Settings", currentSettings } }
});
```

#### **Case 3: Direct RModal Usage** (@bind-Visible)
Standalone modals with two-way binding - integrates with JS modal system for portals/backdrops.
```razor
<RModal @bind-Visible="showModal"
        Title="My Modal"
        CloseOnBackdrop="true"
        Size="SizeType.Large">
    <ChildContent>
        Your content here
    </ChildContent>
    <FooterContent>
        <RButton @onclick="() => showModal = false">Close</RButton>
    </FooterContent>
</RModal>
```

#### **Case 4: Service-Based Utility Modals**
Quick confirmations and info dialogs via ModalService convenience methods.
```razor
// Confirmation dialog
bool confirmed = await ModalService.ShowConfirmationAsync(
    "Are you sure you want to delete?",
    "Delete Item",
    "Delete", "Cancel",
    VariantType.Error
);

// Info dialog
await ModalService.ShowInfoAsync(
    "Operation completed successfully",
    "Success"
);
```

#### Modal Features
- ✅ Unified portal-based rendering for all 4 cases
- ✅ Automatic z-index stacking for nested modals
- ✅ Individual backdrop per modal with proper click handling
- ✅ Escape key closes topmost modal only
- ✅ Parent closing cascades to child modals/dropdowns
- ✅ Strongly-typed results with `ShowRawAsync<T>()`
- ✅ Loading states, progress indicators, multi-step support
- ✅ Memory-safe cleanup with proper disposal

### Intelligent Search System

RAppShell includes a built-in, role-aware search system with collapsible interface:

```razor
<!-- Automatic integration with RAppShell -->
<RAppShell SearchCollapsible="true">
    <!-- Search providers are registered automatically -->
    @Body
</RAppShell>
```

**Search Features:**
- **Universal Search**: Searches across all registered data providers
- **Role-Based Filtering**: Results filtered by user permissions
- **Lightning Fast**: Sub-2-second response times with intelligent caching
- **Collapsible Interface**: Expands on click, auto-collapses when empty
- **Smart Suggestions**: Contextual results with relevance scoring

**Search Provider Registration:**

```csharp
// Register custom search providers
public class MySearchProvider : ISearchProvider
{
    public string Name => "MyData";
    public int Priority => 10;
    
    public async Task<List<AppSearchResult>> SearchAsync(string query, CancellationToken cancellationToken = default)
    {
        // Your search implementation
        return await MyDataService.SearchAsync(query);
    }
}

// In your layout
@inject IAppSearchService AppSearchService

@code {
    protected override void OnInitialized()
    {
        AppSearchService.RegisterSearchProvider(new MySearchProvider());
    }
}
```

## Component Categories

| Category | Components | Examples |
|----------|------------|----------|
| **Core** | 11 components | RButton, RCard, RBadge, RAvatar, RChip, RDivider, RActionGroup, RHeaderCard |
| **Forms** | 10 components | RForm, RTextInput, RCheckbox, RDatePicker, RRadio, RSelectField, RFileUpload, RToggle |
| **Data** | 9 components | RTable, RTableVirtualized, RList, RVirtualList, RFilterBar, RCalendar, RColumn |
| **Display** | 15 components | RChart, RAccordion, RTimeline, RStatsCard, RProgressBar, RMetric, REmptyState, RSkeleton, RImage |
| **Feedback** | 10 components | RModal, RToastContainer, RConfirmationModal, RAlert, RTooltip, RErrorBoundary, RDetailModal |
| **Navigation** | 4 components | RBreadcrumbs, RNavMenu, RTabs, RTabItem |
| **Layout** | 4 components | RAppShell, RSection, RGrid, RContent |
| **Search** | Built-in system | Global search, role-based filtering, collapsible interface |

## Styling System

### Utility Classes

```html
<!-- Spacing (MudBlazor-inspired) -->
<div class="pa-6 ma-4">Padding 6, margin 4</div>

<!-- Layout -->
<div class="flex justify-center align-center gap-4">
<div class="grid grid-cols-1 grid-cols-md-3">

<!-- Visual Effects -->
<div class="elevation-4 glass-light rounded">
<div class="shadow-md hover:shadow-lg transition-all">
```

### CSS Variables

```css
/* Theme-aware colors */
--color-primary
--color-text-primary
--color-background-elevated

/* Semantic spacing */
--space-0 through --space-24

/* Effects */
--shadow-md, --shadow-lg
--gradient-subtle
```

## Configuration

### Basic Setup

```csharp
// Everything enabled by default
builder.Services.AddRRBlazor();
```

### Custom Configuration

```csharp
builder.Services.AddRRBlazor(options => options
    .WithTheme(theme => {
        theme.Mode = ThemeMode.Dark;
        theme.PrimaryColor = "#0078D4";
    })
    .WithToasts(toast => {
        toast.Position = ToastPosition.BottomRight;
        toast.DefaultDuration = 6000;
    })
);
```

## AI Integration

RR.Blazor includes AI-optimized documentation for seamless integration with AI coding assistants:

- **Components**: [`rr-ai-components.json`](wwwroot/rr-ai-components.json) - 65 components with structured APIs
- **Styles**: [`rr-ai-styles.json`](wwwroot/rr-ai-styles.json) - 3,300+ utility classes with patterns

### AI Prompt Example

```
Using RR.Blazor components, create a user management interface with:
- Search functionality using RTextInput
- Data display using RDataTable with elevation-2
- Action buttons using RButton Primary variant
- Professional styling with glass-light and pa-6 utilities
```

## Performance

- **Virtual Scrolling**: Built-in for large datasets
- **CSS Tree Shaking**: 85%+ bundle size reduction (608KB → 86KB)

### Advanced CSS Optimization

RR.Blazor includes intelligent CSS tree-shaking that dramatically reduces bundle sizes while preserving all functionality:

```bash
# Run CSS optimization
pwsh ./Scripts/TreeShakeOptimize.ps1

# Results: 85.9% size reduction
# Original: 608.7 KB → Optimized: 86.0 KB
# Components preserved: 1,556 styles
# Utilities preserved: 145 classes
```

**Tree Shaking Configuration:**

```csharp
builder.Services.AddRRBlazor(options => options
    .DisableTreeShaking()  // Opt-out if needed
    .WithTreeShaking(ts => {
        ts.VerboseLogging = true;
        ts.OutputPath = "./wwwroot/css/optimized";
        ts.EnableCaching = true;
    })
);
```

## Browser Support

Compatible with all modern browsers.

## Documentation

- [Component Reference](wwwroot/rr-ai-components.json) - Machine-readable component docs
- [Utility Classes](wwwroot/rr-ai-styles.json) - Complete styling reference
- [Modal System](_Documentation/MODAL_SYSTEM.md) - Portal-based modal system guide
- [Contributing Guide](CONTRIBUTING.md) - Development guidelines

## Custom Theming

RR.Blazor includes a powerful theming system that allows complete visual customization through SCSS variables.

### Quick Start

```powershell
# Generate a theme template
pwsh RR.Blazor/Scripts/GenerateTheme.ps1 -ThemeName "my-brand"
```

```csharp
// Register theme in Program.cs
builder.Services.AddRRBlazor(options =>
{
    options.WithCustomTheme("my-brand", "Themes/my-brand.scss");
});
```

```razor
<!-- Use your theme -->
<RThemeProvider Theme="my-brand">
    <Router AppAssembly="@typeof(App).Assembly">
        <!-- Your app content -->
    </Router>
</RThemeProvider>
```

### Theme Examples

```scss
// Themes/corporate-theme.scss
:root[data-theme="corporate-theme"] {
  // Override existing themes (extends default/dark)
  --theme-primary: #003d82;        // Corporate blue
  --theme-surface: #ffffff;        // Clean white surfaces
  --theme-text: #172b4d;          // Professional text
  
  // Create entirely new theme
  --theme-canvas: linear-gradient(135deg, #f7f9fb 0%, #e8ecf1 100%);
  --theme-shadow-md: 0 4px 8px -2px rgba(9, 30, 66, 0.08);
  
  // Full customization - override any variable
  --radius-md: 8px;               // Border radius
  --space-4: 1.25rem;             // Spacing
  --font-family-primary: 'Inter'; // Typography
  --button-height: 48px;          // Component sizing
}
```

See [Docs/THEMING.md](Docs/THEMING.md) for complete guide.

## Contributing

1. **Add as submodule** to your working project
2. **Create feature/fix branch** for your changes
3. **Commit and push** to your branch
4. **Open pull request** to master - we'll squash merge after review

```bash
# In your project
git submodule add https://github.com/RaRdq/RR.Blazor.git
cd RR.Blazor
git checkout -b feature/my-new-component
# Make your changes
git add . && git commit -m "Add new super duper component"
git push origin feature/my-new-component
# Open PR to master on GitHub
```

See [CONTRIBUTING.md](CONTRIBUTING.md).

## Integration with AI Tools

### Claude Code Instructions
```bash
# Add to Claude's context
Provide @RR.Blazor/wwwroot/rr-ai-components.json and @RR.Blazor/wwwroot/rr-ai-styles.json to Claude and ask:
"Update my Blazor project to use RR.Blazor unified smart components"

# For new projects
"Initialize a new Blazor project with RR.Blazor design system featuring smart type detection"
```

### Custom AI Commands
```markdown
/designer - Elite Frontend Architect with Plan-Implement-Reflect methodology
/rr-blazor-init - Initialize RR.Blazor in current project
/rr-blazor-upgrade - Upgrade components to latest patterns
/rr-blazor-theme - Configure theme and styling
/rr-blazor-component - Generate new component following patterns
```

## Troubleshooting

### Static Files Returning 404 Errors

**Problem**: CSS/JS files from `_content/RR.Blazor/` return 404 errors

**Solution for Blazor Server projects**:
```csharp
// Program.cs - Add this BEFORE builder.Build()
builder.WebHost.UseStaticWebAssets();
```

- CSS should load from: `http[s]://yourapp/_content/RR.Blazor/css/main.css`
- JS should load from: `http[s]://yourapp/_content/RR.Blazor/js/rr-blazor.js`


## License

MIT License - Free for all use, commercial and non-commercial.

See [LICENSE](LICENSE.md) for details.

---

Built for the Blazor community 💙