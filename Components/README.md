# RR.Blazor Components Organization

This directory contains all RR.Blazor components organized by category and structural relationships for better maintainability and discoverability.

## Component Architecture

### Standalone Components
Components that work independently without requiring other components:

#### Core (`/Core`)
- **RButton** - Primary button component with variants and states
- **RCard** - Generic card container with elevation and variants
- **RAvatar** - User avatar with status indicators and sizes
- **RBadge** - Status badges and indicators
- **RDivider** - Visual separators and dividers
- **RChip** - Removable labels and tags
- **RThemeProvider** - Theme context provider
- **RThemeSwitcher** - Theme toggle component

#### Display (`/Display`)
- **RStatsCard** - Dashboard statistics card with icons and values
- **REmptyState** - Empty state messaging
- **RSkeleton** - Loading placeholders
- **RProgressBar** - Progress indicators
- **RInfoItem** - Key-value information display
- **RSummaryItem** - Summary data display
- **RMetric** - Statistical data presentation
- **RTimeline** - Event timeline visualization

#### Form (`/Form`)
- **RFormField** - Universal form input component
- **RDatePicker** - Date selection component
- **RDatePickerBasic** - Simplified date picker
- **RFileUpload** - File upload with drag-and-drop
- **RSwitcher** - Toggle switches and radio groups

#### Navigation (`/Navigation`)
- **RBreadcrumbs** - Breadcrumb navigation
- **RNavMenu** - Navigation menu
- **RDropdown** - Dropdown menus

#### Layout (`/Layout`)
- **RAppShell** - Complete application shell
- **RSection** - Content sections with headers
- **RGrid** - Grid layout system

### Composite Component Systems
Components that work together as integrated systems:

#### Data Tables (`/Data`)
**Primary Component**: `RDataTable`
- **RDataTable** - Main data table with sorting and filtering
- **RDataTableColumn** - ⚠️ Column configuration (requires RDataTable)

**Usage**: RDataTableColumn components are defined within RDataTable's ColumnsContent

#### Lists (`/Data`)
**Primary Component**: `RList` or `RVirtualList`
- **RList** - Generic list container
- **RListItem** - ⚠️ List item with actions (requires RList)
- **RVirtualList** - Performance-optimized virtual scrolling

**Usage**: RListItem components are defined within RList's ChildContent

#### Accordions (`/Display`)
**Primary Component**: `RAccordion`
- **RAccordion** - Collapsible content sections container
- **RAccordionItem** - ⚠️ Individual accordion panels (requires RAccordion)

**Usage**: RAccordionItem components are defined within RAccordion's ChildContent

#### Tabs (`/Navigation`)
**Primary Component**: `RTabs`
- **RTabs** - Tab navigation container
- **RTabItem** - ⚠️ Individual tab items (requires RTabs)

**Usage**: RTabItem components are defined within RTabs' ChildContent

#### Forms (`/Form`)
**Primary Component**: `RForm`
- **RForm** - Form container with validation
- **RFormSection** - ⚠️ Organized form layout sections (requires RForm)

**Usage**: RFormSection components are defined within RForm's FormFields

#### Modals (`/Feedback`)
**Primary Component**: `RModal` + `RModalProvider`
- **RModal** - Base modal component
- **RModalProvider** - Modal service provider (required for service-based modals)
- **RConfirmModal** - Confirmation dialogs
- **RConfirmationModal** - Enhanced confirmation dialogs
- **RMessageModal** - Message display modals
- **RDetailModal** - Detail view modals
- **RFormModal** - Form wrapper modals
- **RPreviewModal** - Content preview modals
- **RSelectModal** - Selection modals
- **RToastContainer** - Toast notification system

#### Action Groups (`/Core`)
**Primary Component**: `RActionGroup`
- **RActionGroup** - Action button groupings

#### Filter System (`/Data`)
**Primary Component**: `RFilterBar`
- **RFilterBar** - Data filtering interface

## Usage

All components are automatically available through the `@using RR.Blazor.Components.*` directives in `_Imports.razor`. Simply use any component by its name:

```razor
<RStatsCard Label="Revenue" 
            Value="$125,430" 
            Icon="trending_up"
            IconColor="success" />
```

## Design Principles

1. **Generic First**: Components should be reusable across different domains
2. **Semantic Colors**: Use semantic CSS variables (`var(--color-*)`) instead of hardcoded values
3. **Utility Integration**: Work seamlessly with RR.Blazor utility classes
4. **Theme Aware**: Support automatic light/dark theme switching
5. **Accessible**: Follow WCAG guidelines and provide proper ARIA labels
6. **Performance**: Optimize for minimal re-renders and bundle size

## Adding New Components

When adding new components:

1. **Choose the right category** - Place in the most logical folder
2. **Follow naming conventions** - Use `R` prefix and PascalCase
3. **Use semantic variables** - Leverage the RR.Blazor design system
4. **Document thoroughly** - Include XML docs and usage examples
5. **Add to imports** - Ensure namespace is included in `_Imports.razor`

## Component Dependencies

Components can reference other RR.Blazor components within their category or from Core. Avoid circular dependencies between categories.

Example: A Display component can use Core components like RButton or RBadge, but should not depend on Form or Navigation components.