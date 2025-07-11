# RR.Blazor Components Organization

This directory contains all RR.Blazor components organized by category for better maintainability and discoverability.

## Component Categories

### Core (`/Core`)
Foundational components that are used throughout the system:
- **RButton** - Primary button component with variants and states
- **RCard** - Generic card container with elevation and variants
- **RAvatar** - User avatar with status indicators and sizes
- **RBadge** - Status badges and indicators
- **RDivider** - Visual separators and dividers
- **RThemeProvider** - Theme context provider
- **RThemeSwitcher** - Theme toggle component

### Display (`/Display`)
Components for displaying information and data:
- **RStatsCard** - Dashboard statistics card with icons and values
- **REmptyState** - Empty state messaging
- **RSkeleton** - Loading placeholders
- **RProgressBar** - Progress indicators
- **RInfoItem** - Key-value information display
- **RSummaryItem** - Summary data display
- **RAccordion** - Collapsible content sections
- **RAccordionItem** - Individual accordion panels

### Form (`/Form`)
Input and form-related components:
- **RFormField** - Universal form input component
- **RDatePicker** - Date selection component
- **RDatePickerBasic** - Simplified date picker
- **RFileUpload** - File upload with drag-and-drop
- **RSwitcher** - Toggle switches and radio groups

### Navigation (`/Navigation`)
Components for navigation and wayfinding:
- **RBreadcrumbs** - Breadcrumb navigation
- **RNavMenu** - Navigation menu
- **RTabs** - Tab navigation container
- **RTabItem** - Individual tab items
- **RDropdown** - Dropdown menus

### Feedback (`/Feedback`)
Components for user feedback and modals:
- **RModal** - Base modal component
- **RModalProvider** - Modal service provider
- **RConfirmModal** - Confirmation dialogs
- **RConfirmationModal** - Enhanced confirmation dialogs
- **RMessageModal** - Message display modals
- **RDetailModal** - Detail view modals
- **RFormModal** - Form wrapper modals
- **RPreviewModal** - Content preview modals
- **RSelectModal** - Selection modals
- **RToastContainer** - Toast notification system

### Data (`/Data`)
Components for displaying structured data:
- **RDataTable** - Data table with sorting and filtering
- **RDataTableColumn** - Table column configuration
- **RList** - Generic list component
- **RListItem** - List item with actions
- **RVirtualList** - Virtualized list for performance

### Layout (`/Layout`)
Components for layout and structure:
- **RAppShell** - Complete application shell
- **RSection** - Content sections with headers
- **RActionGroup** - Action button groupings

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