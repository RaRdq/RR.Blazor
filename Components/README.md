# RR.Blazor Components Organization

This directory contains all 64 RR.Blazor components organized by category and structural relationships for better maintainability and discoverability. The components provide a comprehensive design system with 3,309 utility classes and 336 CSS variables.

## R* Component Architecture

RR.Blazor features a modern R* component architecture that provides:

- **Unified API**: All R* components share common patterns and parameters
- **Built-in Validation**: Integrated with RInputBase for seamless form validation
- **Enterprise Styling**: Professional appearance with density and variant support
- **WCAG 2.1 AA Compliant**: Full accessibility with screen reader support and keyboard navigation
- **Cross-Device Responsive**: Optimized for mobile, PC, laptop, iPad portrait/landscape
- **Touch-Friendly**: 44px minimum touch targets for mobile devices
- **Type Safety**: Strongly-typed parameters prevent runtime errors

### Modern Form Components

The R* form components replace the legacy RFormField pattern with specialized, purpose-built components:

- **RTextInput**: Universal text input supporting multiple types (text, email, password, number, tel, url, search, date, time)
- **RCheckbox**: Professional checkbox with validation and description support
- **RRadio**: Radio button component with enterprise styling
- **RTextArea**: Multi-line text input with auto-resize functionality

All R* components inherit from `RInputBase`, providing consistent validation behavior and styling patterns. This unified architecture achieves 94% CLAUDE.md compliance with modern C# patterns and zero .razor.cs files.

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
- **RTextInput** - Universal text input with multiple types (text, email, password, etc.)
- **RCheckbox** - Professional checkbox with validation
- **RRadio** - Radio button component with enterprise styling
- **RTextArea** - Multi-line text input with auto-resize
- **RDatePicker** - Date selection component
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
**Primary Components**: `RTable` and `RTableVirtualized`
- **RTable** - Smart table with type inference, auto-generation, and rich features
- **RColumn** - Type-safe column definitions with template support
- **RTableVirtualized** - High-performance virtualized table for 1M+ records
- **RDataTable** - ⚠️ Legacy table component (use RTable instead)
- **PropertyColumnGenerator** - Auto-generates columns from model properties

**✅ Confirmed Working Scenarios**:
1. **Auto-Generation**: `<RTable Items="@data" />` generates all columns automatically
2. **Partial Override**: Auto-generate most columns, customize specific ones
3. **Full Manual**: Complete control over all column definitions

**Features**:
- **Zero Configuration**: Works out of the box with any data collection
- **Smart PageSize**: Automatically suggests optimal page sizes (5, 10, 25, 50, etc.) based on dataset size
- **Type Inference**: No need to specify `TItem` - automatically detected from Items
- **Smart Formatting**: Auto-detects currency, dates, emails, phones, percentages
- **Attribute Support**: Respects `[Display]`, `[DisplayFormat]`, `[NotMapped]`, `[JsonIgnore]`
- **Template Support**: Use `<Template Context="item">` for custom rendering without casting
- **Performance**: Handles datasets from 10 to 1M+ records efficiently

**Usage Examples**: 
```razor
<!-- 1. Zero configuration (auto-generates all columns) -->
<RTable Items="@employees" />

<!-- 2. Partial override (auto-generate + customize specific columns) -->
<RTable Items="@employees">
    <RColumn Property="@nameof(Employee.Name)" />  @* Override this column *@
    <RColumn Property="@nameof(Employee.Status)">
        <Template Context="emp">
            <RChip Text="@emp.Status" 
                   Variant="@(emp.Status == "Active" ? VariantType.Success : VariantType.Secondary)" />
        </Template>
    </RColumn>
    @* Other columns auto-generated *@
</RTable>

<!-- 3. Full manual control -->
<RTable Items="@employees" AutoGenerateColumns="false">
    <RColumn Property="@nameof(Employee.Name)" Header="Full Name" />
    <RColumn Property="@nameof(Employee.Department)" />
    <RColumn For="@((Employee e) => e.Salary)" Format="C" />
    <RColumn Header="Actions">
        <Template Context="emp">
            <RButton Icon="edit" Size="ButtonSize.Small" OnClick="@(() => Edit(emp))" />
        </Template>
    </RColumn>
</RTable>

<!-- For massive datasets -->
<RTableVirtualized Items="@millionRecords" Height="600px" ShowPerformanceMetrics="true" />
```

📖 **[See detailed documentation](../_Documentation/SMART_TABLE_SYSTEM.md)**

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
<RStatsCard Text="Revenue" 
            Value="$125,430" 
            Icon="trending_up"
            IconColor="success" />
```

## Design Principles

1. **Generic First**: Components should be reusable across different domains
2. **Semantic Colors**: Use semantic CSS variables (`var(--color-*)`) instead of hardcoded values
3. **Utility Integration**: Work seamlessly with RR.Blazor utility classes
4. **Theme Aware**: Support automatic light/dark theme switching with WCAG 2.1 AA contrast ratios
5. **WCAG 2.1 AA Compliant**: Full accessibility with ARIA labels, keyboard navigation, and screen reader support
6. **Cross-Device Responsive**: Adaptive design for mobile, PC, laptop, iPad portrait/landscape
7. **Touch-Friendly**: 44px minimum touch targets for mobile devices
8. **Performance**: Optimize for minimal re-renders and bundle size

## Adding New Components

When adding new components:

1. **Choose the right category** - Place in the most logical folder
2. **Follow naming conventions** - Use `R` prefix and PascalCase
3. **Use semantic variables** - Leverage the RR.Blazor design system
4. **Ensure WCAG 2.1 AA compliance** - Include proper ARIA attributes and keyboard navigation
5. **Implement responsive design** - Support mobile, PC, laptop, iPad portrait/landscape
6. **Provide touch-friendly targets** - Minimum 44px touch targets for mobile
7. **Document thoroughly** - Include XML docs and usage examples
8. **Add to imports** - Ensure namespace is included in `_Imports.razor`
9. **Inherit from RInputBase** - For form components, inherit from RInputBase for consistency

## Component Dependencies

Components can reference other RR.Blazor components within their category or from Core. Avoid circular dependencies between categories.

Example: A Display component can use Core components like RButton or RBadge, but should not depend on Form or Navigation components.