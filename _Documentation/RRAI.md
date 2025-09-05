# RR.Blazor AI for AI Agent

## 🎯 Framework Vision & Philosophy

**RR.Blazor** is a **lightweight, project-agnostic, generic Blazor component framework** designed to serve as the foundation for modern web applications. The framework prioritizes **minimal bundle size**, **maximum customizability**, and **utility-first composition** to deliver exceptional developer experience and performance.

### 🌳 CSS Tree-Shaking Optimization
**RR.Blazor** includes advanced CSS tree-shaking that automatically removes unused styles from production builds

## Essential Files

### Primary Documentation
- `RR.Blazor/wwwroot/rr-ai-components.json` - Complete component API reference (64 components)
- `RR.Blazor/wwwroot/rr-ai-styles.json` - All utility classes with bracket notation (3,309 classes)
- `RR.Blazor/Documentation/CSS-TREE-SHAKING.md` - CSS optimization guide and usage
- `CLAUDE.md` - Project-specific implementation guidelines
- `README.md` - Comprehensive feature overview

### Key Patterns
- **Component Format**: `<RComponentName Parameter="value" />` 
- **Utility Classes**: Use bracket notation extrapolation (e.g., `p-[0,1,2,4,8]` means `p-0`, `p-1`, `p-2`, `p-4`, `p-8`)
- **Smart Components**: No generic parameters needed - automatic type detection
- **Theme System**: CSS variables with semantic naming (`--color-text-primary`)

## Component Categories & Count

### Core Components (13)
**Purpose**: Foundational UI elements and theming
- RActionGroup, RAvatar, RBadge, RButton, RButtonRefactored, RCard, RChip, RDivider, RHeaderCard, RSectionDivider, RStatus, RThemeProvider, RThemeSwitcher

### Form Components (10) 
**Purpose**: Modern form controls with validation
- RCheckbox, RDatePicker, RFileUpload, RFormGeneric, RFormSection, RRadio, RSelectField, RSwitcherGeneric, RTextInput, RToggleGeneric

### Data Components (7)
**Purpose**: Data display and management
- RCalendar, RDataTableColumnGeneric, RDataTableGeneric, RFilterBar, RList, RListItem, RVirtualListGeneric

### Display Components (13)
**Purpose**: Information presentation and visualization  
- RAccordion, RAccordionItem, RChart, RColumnChart, REmptyState, RInfoItem, RMetric, RPieChart, RProgressBar, RSkeleton, RStatsCard, RSummaryItem, RTimeline

### Feedback Components (10)
**Purpose**: User interaction and notifications
- RAlert, RConfirmationModal, RDetailModal, RFormModal, RMessageModal, RModal, RModalProvider, RPreviewModal, RSelectModalGeneric, RToastContainer

### Navigation Components (7)
**Purpose**: Site navigation and menus
- RBreadcrumbs, RDropdown, RDropdownGeneric, RNavMenu, RSmartDropdown, RTabItem, RTabs

### Layout Components (4)
**Purpose**: Page structure and organization
- RAppShell, RContent, RGrid, RSection

## Key Architecture Patterns

### Unified Smart Components
**Breakthrough feature**: Eliminates generic type parameters through runtime type detection

```razor
<!-- Old way -->
<RDropdownGeneric<User> Items="users" />
<RFormGeneric<UserModel> Model="user" />

<!-- New way - automatic type detection -->
<RDropdown Items="users" />
<RForm Model="user" />
```

### R* Component Standards
- **Consistent Parameters**: All components follow unified parameter patterns
- **Icon System**: Unified `Icon` + `IconPosition` pattern across all components
- **Variant Support**: Professional styling variants (Primary, Secondary, Success, Warning, Error)
- **Density Options**: Normal, Dense, Compact, Spacious for different layouts
- **Accessibility**: WCAG 2.1 AA compliant with screen reader support

### Essential Components for AI Agents

#### RCard - Universal Container
```razor
<RCard Title="Dashboard" 
       Subtitle="Real-time analytics"
       Icon="dashboard"
       Elevation="4"
       Variant="CardVariant.Elevated"
       Class="glass-light">
    <div class="pa-6">Content here</div>
</RCard>
```

#### RButton - Action Component
```razor
<RButton Text="Save Changes"
         Icon="save" 
         IconPosition="IconPosition.Start"
         Type="ButtonType.Submit"
         Variant="ButtonVariant.Primary"
         Size="ButtonSize.Medium" />
```

#### RDataTableGeneric - Auto-Generated Columns
```razor
<!-- Zero configuration - generates columns from model -->
<RDataTableGeneric TItem="User" 
                   Items="@users"
                   Title="User Management"
                   ShowFilters="true" />
```

#### RForm - Smart Type Detection
```razor
<RForm Model="@userModel" OnValidSubmit="SaveUser">
    <RTextInput Type="FieldType.Email" @bind-Value="userModel.Email" Required />
    <RCheckbox Text="Active" @bind-Checked="userModel.IsActive" />
</RForm>
```

## Utility-First CSS System

### Spacing (MudBlazor-inspired)
```html
<!-- Padding: pa-{size}, px-{size}, py-{size}, pt-{size}, pr-{size}, pb-{size}, pl-{size} -->
<div class="pa-6">All sides padding</div>
<div class="px-4 py-2">Horizontal and vertical</div>

<!-- Margin: ma-{size}, mx-{size}, my-{size}, mt-{size}, mr-{size}, mb-{size}, ml-{size} -->
<div class="ma-4">All sides margin</div>
<div class="mb-4">Bottom margin only</div>

<!-- Values: 0, 1, 2, 3, 4, 5, 6, 8, 10, 12, 16, 20, 24 -->
```

### Layout & Display
```html
<!-- Flexbox -->
<div class="d-flex justify-center align-center gap-4">
<div class="d-flex justify-between align-center">

<!-- Grid -->
<div class="grid grid-cols-1 grid-cols-md-2 gap-4">
<div class="grid grid-cols-12">

<!-- Common patterns -->
<div class="flex-grow-1">Flexible item</div>
<div class="text-center">Centered text</div>
```

### Professional Effects
```html
<!-- Elevation (0-24 levels) -->
<div class="elevation-2">Subtle elevation</div>
<div class="elevation-8">Strong elevation</div>

<!-- Glassmorphism -->
<div class="glass-light">Light glass effect</div>
<div class="glass-heavy backdrop-blur-xl">Heavy glass effect</div>

<!-- Theme-aware colors -->
<div class="bg-primary text-primary">Primary themed</div>
<div class="bg-surface-elevated">Elevated surface</div>
```

### Typography
```html
<!-- Semantic text sizes -->
<h1 class="text-h4 font-bold">Section Header</h1>
<p class="text-body-1 text-secondary">Body text</p>
<span class="text-caption text-muted">Helper text</span>

<!-- Font weights: thin, light, normal, medium, semibold, bold, black -->
<span class="font-semibold">Important text</span>
```

## Professional Implementation Patterns

### Enterprise Dashboard Card
```razor
<RCard Title="Revenue Overview" 
       Icon="analytics"
       Elevation="4" 
       Class="glass-light">
    <div class="pa-6">
        <div class="d-flex justify-between align-center mb-4">
            <span class="text-h3 font-bold">$1,247,580</span>
            <RBadge Text="+12.3%" Variant="BadgeVariant.Success" />
        </div>
        <div class="grid grid-cols-2 gap-4">
            <RStatsCard Text="Monthly" Value="$892K" Icon="trending_up" />
            <RStatsCard Text="Quarterly" Value="$2.1M" Icon="bar_chart" />
        </div>
    </div>
</RCard>
```

### Data Management Interface
```razor
<div class="d-flex justify-between align-center mb-6">
    <RTextInput Type="FieldType.Search" 
                Placeholder="Search users..."
                Class="flex-grow-1 mr-4" />
    <RActionGroup>
        <RButton Text="Export" Icon="download" Variant="ButtonVariant.Secondary" />
        <RButton Text="Add User" Icon="add" Variant="ButtonVariant.Primary" />
    </RActionGroup>
</div>

<RDataTableGeneric TItem="User" 
                   Items="@users"
                   Title="User Management"
                   ShowFilters="true"
                   Class="elevation-2" />
```

### Modal System - 4 Usage Patterns

#### **Case 1: Service confirmations**
```razor
await ModalService.ShowConfirmationAsync("Delete?", "Confirm", "Delete", "Cancel", VariantType.Error);
```

#### **Case 2: Internal RModal components (Auto-detected)**
```razor
// Components with internal RModal are automatically detected - no wrapping needed
await ModalService.ShowAsync(new ModalOptions { ComponentType = typeof(PaymentOrderQuickActionModal), ShowHeader = false, ShowFooter = false });
```

#### **Case 3: Direct RModal binding**
```razor
<RModal @bind-Visible="showModal" Title="Form"><ChildContent>Content</ChildContent></RModal>
```

#### **Case 4: Pure content components (Auto-wrapped)**
```razor
// Pure content components are automatically wrapped with RModal
await ModalService.ShowAsync(new ModalOptions { ComponentType = typeof(UserFormContent), Title = "Edit User" });
```

## Performance Optimization Patterns

### Virtual Scrolling for Large Datasets
```razor
<RVirtualListGeneric TItem="Order" 
                     Items="@thousandsOfOrders"
                     ItemHeight="80"
                     Class="h-400">
    <ItemTemplate Context="order">
        <div class="pa-4 border-b">
            <div class="d-flex justify-between">
                <span class="font-semibold">@order.Id</span>
                <RBadge Text="@order.Status" />
            </div>
        </div>
    </ItemTemplate>
</RVirtualListGeneric>
```

### Optimistic UI Updates
```razor
@code {
    private async Task SaveUser()
    {
        // Show immediate feedback
        ToastService.ShowInfo("Saving user...");
        
        try 
        {
            // Optimistically update UI
            users.Add(newUser);
            StateHasChanged();
            
            // Actual save
            await UserService.SaveAsync(newUser);
            ToastService.ShowSuccess("User saved successfully");
        }
        catch 
        {
            // Rollback on failure
            users.Remove(newUser);
            ToastService.ShowError("Failed to save user");
            StateHasChanged();
        }
    }
}
```

## Theme System Integration

### CSS Variables Usage
```css
/* Use semantic variables for custom styling */
.custom-component {
    background: var(--color-background-elevated);
    border: 1px solid var(--color-border-subtle);
    color: var(--color-text-primary);
    padding: var(--space-4);
    border-radius: var(--radius-md);
    box-shadow: var(--shadow-md);
}
```

### Theme-Aware Components
```razor
<!-- Components automatically adapt to theme changes -->
<div class="bg-surface text-primary pa-4 rounded elevation-2">
    Content adapts to light/dark theme automatically
</div>
```

## Common AI Implementation Mistakes to Avoid

### ❌ Don't Do This
```razor
<!-- Explicit generic types (old pattern) -->
<RDropdownGeneric<User> Items="users" />
<RFormGeneric<UserModel> Model="user" />

<!-- Hardcoded styling -->
<div style="padding: 24px; background: #ffffff;">

<!-- Mixed utility frameworks -->
<div class="p-6 bootstrap-class tailwind-class">

<!-- Double modal: Case 2 with ShowHeader=true -->
await ModalService.ShowAsync(new ModalOptions { ComponentType = typeof(PaymentOrderQuickActionModal), ShowHeader = true });
```

### ✅ Do This Instead
```razor
<!-- Smart type detection (new pattern) -->
<RDropdown Items="users" />
<RForm Model="user" />

<!-- Utility-first classes -->
<div class="pa-6 bg-surface">

<!-- Consistent RR.Blazor utilities -->
<div class="pa-6 bg-surface elevation-2 rounded">

<!-- Case 2 fix: ShowHeader=false -->
await ModalService.ShowAsync(new ModalOptions { ComponentType = typeof(PaymentOrderQuickActionModal), ShowHeader = false });
```

## Quick Development Checklist

### For New Components
- [ ] Use R* naming convention
- [ ] Include Icon and IconPosition parameters
- [ ] Support Variant, Size, and Density parameters
- [ ] Add Class parameter for utility class support
- [ ] Include Loading and Disabled states
- [ ] Implement proper keyboard navigation
- [ ] Add ARIA attributes for accessibility

### For Styling
- [ ] Use utility-first classes instead of inline styles
- [ ] Apply semantic CSS variables for custom styling
- [ ] Use elevation and glass effects professionally
- [ ] Ensure mobile responsiveness with touch targets
- [ ] Test in both light and dark themes

### For Forms
- [ ] Use RForm for automatic validation
- [ ] Implement proper field types (email, password, search, etc.)
- [ ] Add Required and validation attributes
- [ ] Use RFormSection for organized layouts
- [ ] Include loading states during submission

## Integration Commands

```bash
# Load AI documentation (run in project root)
# Contains all component APIs and utility classes
cat RR.Blazor/wwwroot/rr-ai-components.json
cat RR.Blazor/wwwroot/rr-ai-styles.json

# Generate fresh documentation
pwsh ./RR.Blazor/Scripts/GenerateDocumentation.ps1

# Validate component usage
pwsh ./RR.Blazor/Scripts/ValidateComponentUsage.ps1

# Check for CSS class compliance
pwsh ./RR.Blazor/Scripts/ValidateClassUsage.ps1
```

---

**Quick Start for AI Agents**: Read `RR.Blazor/wwwroot/rr-ai-components.json` for complete component APIs, use utility-first classes from `RR.Blazor/wwwroot/rr-ai-styles.json`, apply R* component patterns with automatic type detection, and follow the professional implementation examples above.

**Framework Philosophy**: Lightweight, customizable, generic foundation that empowers developers to build exceptional user experiences through utility-first composition.

**Key Principle**: RR.Blazor provides the tools, projects create the experience.

**Success Measure**: Developers can build any UI using only utility classes and minimal custom CSS.

## Container-Agnostic Component Architecture

**Critical Principle**: Every RR.Blazor component must be fully container-agnostic and context-independent. Components should never include protective styling, hardcoded positioning, or assumptions about their parent container.

### Component Independence Rules
- **No Protective Styling**: Components must not include CSS that compensates for layout context (e.g., sidebar width, modal positioning)
- **No Context Assumptions**: Components should work identically whether placed in modals, sidebars, main content, or nested containers
- **Pure Utility Composition**: All layout concerns handled through utility classes applied by the consumer
- **Container Responsibility**: Parent containers handle their own layout requirements without child component interference

### Architecture Pattern
```razor
<!-- ❌ Wrong: Component with protective styling -->
<div class="dashboard-content" style="margin-left: var(--sidebar-width)">
    <RCard>Content</RCard>
</div>

<!-- ✅ Correct: Container handles layout, component stays pure -->
<div class="main-content-with-sidebar">
    <RCard>Content</RCard>
</div>
```

### Implementation Guidelines
- Layout concerns belong in app shell architecture, not individual components
- Use semantic CSS variables and utility classes for consistent spacing
- Components should render identically in any container context
- Test components in isolation and in various container contexts