# RR.Blazor - Universal Design System

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![Blazor](https://img.shields.io/badge/Blazor-.NET%209-512BD4?logo=.net)](https://dotnet.microsoft.com/apps/aspnet/web-apps/blazor)
[![CSS](https://img.shields.io/badge/CSS-Modern%20Design%20System-1572B6?logo=css3)](https://www.w3.org/Style/CSS/)

## Overview
Ultra-generic, lightweight and project-agnostic design system with enterprise-grade components and utilities for professional Blazor applications. Built primary for AI coding agents working over Blazor.

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

### Package Manager Console
```powershell
Install-Package RR.Blazor
```

### .NET CLI
```bash
dotnet add package RR.Blazor
```

### PackageReference
```xml
<PackageReference Include="RR.Blazor" Version="1.0.0" />
```

## Quick Start

### 1. Register Services
```csharp
// Program.cs
builder.Services.AddRRBlazor();

// OR with customization
builder.Services.AddRRBlazor(options =>
{
    options.Theme.Mode = ThemeMode.System; // Auto, Light, Dark, System
    options.Theme.PrimaryColor = "#3498DB";
    options.EnableAnimations = true;
    options.EnableAccessibility = true;
});
```

### 2. Add Theme Provider
```html
<!-- App.razor -->
<RThemeProvider>
    <Router AppAssembly="@typeof(App).Assembly">
        <!-- Your app content -->
    </Router>
</RThemeProvider>
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

<!-- Simplified boolean attributes (recommended) -->
<RButton Text="Save" Disabled Loading />
<RButton Text="Delete" Variant="ButtonVariant.Danger" Disabled />

<!-- Conditional boolean attributes -->
<RButton Text="Submit" 
         Disabled="@isProcessing" 
         Loading="@isProcessing" />
```

## CSS Selector Reference

### Component Classes

#### Action Groups
- `.action-group` - Action group container
- `.action-group--centered` - Centered alignment
- `.action-group--spaced` - Spaced items
- `.action-group--compact` - Compact spacing
- `.action-group--responsive` - Stack on mobile
- `.action-group__primary` - Primary actions
- `.action-group__secondary` - Secondary actions

#### App Shell
- `.app-shell` - Main shell container
- `.app-shell__header` - Header section
- `.app-shell__main` - Main content area
- `.app-shell__footer` - Footer section
- `.app-shell__sidebar` - Sidebar panel
- `.app-shell__sidebar--left` - Left sidebar
- `.app-shell__sidebar--right` - Right sidebar
- `.app-shell__content` - Content wrapper

#### Avatars
- `.avatar` - Base avatar
- `.avatar--xs`, `--sm`, `--md`, `--lg`, `--xl` - Sizes
- `.avatar--primary`, `--secondary`, `--success`, `--warning`, `--error` - Color variants
- `.avatar--circle`, `--square`, `--rounded` - Shape variants
- `.avatar--online`, `--away`, `--busy`, `--offline` - Status indicators
- `.avatar__image` - Avatar image
- `.avatar__initials` - Text initials
- `.avatar__status` - Status badge
- `.avatar-group` - Multiple avatars
- `.avatar-group--stacked` - Overlapping avatars

#### Badges
- `.badge` - Base badge
- `.badge--primary`, `--secondary`, `--success`, `--warning`, `--error`, `--info` - Color variants
- `.badge--subtle`, `--solid`, `--outlined` - Style variants
- `.badge--sm`, `--md`, `--lg` - Size variants
- `.badge--pill`, `--square` - Shape variants
- `.badge--dot` - Dot indicator
- `.badge__icon` - Badge icon
- `.badge__text` - Badge text
- `.badge__close` - Dismissible badge

#### Banners
- `.banner` - Full-width banner
- `.banner--info`, `--success`, `--warning`, `--error` - Status variants
- `.banner--sticky` - Sticky positioning
- `.banner__icon` - Banner icon
- `.banner__content` - Content area
- `.banner__message` - Banner message
- `.banner__actions` - Action buttons

#### Buttons
- `.button` - Base button
- `.button--primary`, `--secondary`, `--success`, `--warning`, `--danger`, `--info` - Color variants
- `.button--ghost`, `--outlined`, `--text`, `--gradient`, `--glass` - Style variants
- `.button--sm`, `--md`, `--lg`, `--xl` - Size variants
- `.button--icon`, `--icon-only` - Icon buttons
- `.button--loading`, `--disabled` - States
- `.button--full`, `--block` - Full width
- `.button--fab` - Floating action button
- `.button__icon` - Button icon
- `.button__text` - Button text
- `.button__spinner` - Loading spinner
- `.button-group` - Button container
- `.button-group--vertical`, `--spaced` - Group variants

#### Cards
- `.card` - Base card
- `.card--elevated`, `--flat`, `--outlined`, `--glass` - Style variants
- `.card--clickable`, `--interactive` - Interactive states
- `.card--compact`, `--comfortable`, `--spacious` - Density variants
- `.card__header` - Card header
- `.card__title` - Title text
- `.card__subtitle` - Subtitle text
- `.card__media` - Media section
- `.card__content` - Main content
- `.card__actions` - Action buttons
- `.card__footer` - Footer section

#### Charts
- `.chart` - Chart container
- `.chart--line`, `--bar`, `--pie`, `--doughnut`, `--area` - Chart types
- `.chart--responsive` - Responsive sizing
- `.chart__canvas` - Canvas element
- `.chart__legend` - Legend container
- `.chart__tooltip` - Tooltip overlay
- `.chart__title` - Chart title
- `.chart__subtitle` - Chart subtitle
- `.chart__loading` - Loading state
- `.chart__empty` - No data state

#### Comments
- `.comment` - Comment block
- `.comment--primary`, `--warning` - Style variants
- `.comment--nested` - Nested comment
- `.comment__avatar` - User avatar
- `.comment__content` - Content wrapper
- `.comment__header` - Header info
- `.comment__author` - Author name
- `.comment__time` - Timestamp
- `.comment__body` - Comment text
- `.comment__actions` - Action buttons
- `.comment__replies` - Reply container

#### Date Picker
- `.datepicker` - Date picker container
- `.datepicker__input` - Input field
- `.datepicker__toggle` - Calendar toggle
- `.datepicker__calendar` - Calendar dropdown
- `.datepicker__header` - Calendar header
- `.datepicker__nav` - Month/year navigation
- `.datepicker__grid` - Days grid
- `.datepicker__day` - Day cell
- `.datepicker__day--today`, `--selected`, `--disabled` - Day states
- `.datepicker__time` - Time picker section

#### Dropdowns
- `.dropdown` - Dropdown container
- `.dropdown--top`, `--bottom`, `--left`, `--right` - Position variants
- `.dropdown--auto` - Auto positioning
- `.dropdown__trigger` - Trigger element
- `.dropdown__viewport` - Content viewport
- `.dropdown__content` - Content wrapper
- `.dropdown__header` - Header section
- `.dropdown__menu` - Menu items container
- `.dropdown__item` - Menu item
- `.dropdown__item--active`, `--disabled` - Item states
- `.dropdown__separator` - Divider line
- `.dropdown__footer` - Footer section
- `.dropdown-backdrop` - Click-away backdrop

#### Empty States
- `.empty-state` - Empty state container
- `.empty-state--compact`, `--comfortable`, `--large` - Size variants
- `.empty-state--primary`, `--success`, `--warning`, `--error`, `--info` - Color variants
- `.empty-state--subtle`, `--muted` - Style variants
- `.empty-state__icon` - Icon display
- `.empty-state__title` - Title text
- `.empty-state__description` - Description text
- `.empty-state__actions` - Action buttons
- `.empty-state__image` - Illustration

#### File Upload
- `.file-upload` - Upload container
- `.file-upload--dragging` - Drag active state
- `.file-upload--disabled` - Disabled state
- `.file-upload__dropzone` - Drop area
- `.file-upload__input` - Hidden input
- `.file-upload__icon` - Upload icon
- `.file-upload__text` - Instructions text
- `.file-upload__button` - Browse button
- `.file-upload__preview` - File preview
- `.file-upload__progress` - Upload progress
- `.file-upload__list` - File list

#### Forms
- `.form` - Form container
- `.form--inline`, `--horizontal` - Layout variants
- `.form__group` - Field group
- `.form__label` - Field label
- `.form__control` - Input wrapper
- `.form__hint` - Helper text
- `.form__error` - Error message
- `.form__success` - Success message
- `.form__required` - Required indicator
- `.form-field` - Field wrapper
- `.form-field--error`, `--success`, `--disabled`, `--required` - Field states
- `.form-field__wrapper` - Input wrapper
- `.form-field__icon-container` - Icon container
- `.form-field__icon-container--start`, `--end` - Icon positions
- `.form-field__validation` - Validation message

#### Fraud Alerts
- `.fraud-alert` - Security alert
- `.fraud-alert__icon` - Warning icon
- `.fraud-alert__content` - Content wrapper
- `.fraud-alert__title` - Alert title
- `.fraud-alert__details` - Details text
- `.fraud-alert__risk-score` - Risk indicator
- `.fraud-alert__risk-score--high`, `--medium`, `--low` - Risk levels

#### Info Items
- `.info-item` - Info container
- `.info-item--small`, `--medium`, `--large` - Size variants
- `.info-item--horizontal`, `--compact` - Layout variants
- `.info-item--emphasized`, `--muted` - Style variants
- `.info-item__label` - Label text
- `.info-item__value` - Value text
- `.info-item__icon` - Optional icon

#### Lists
- `.list` - List container
- `.list--bordered`, `--hoverable` - Style variants
- `.list--small`, `--large` - Size variants
- `.list--danger` - Danger variant
- `.list__item` - List item
- `.list__item--active`, `--disabled` - Item states
- `.list__icon` - Item icon
- `.list__content` - Item content
- `.list__title` - Item title
- `.list__subtitle` - Item subtitle
- `.list__actions` - Item actions
- `.list__divider` - Divider element

#### Loading States
- `.loading` - Loading container
- `.loading--inline`, `--overlay` - Display variants
- `.loading__spinner` - Spinner element
- `.loading__text` - Loading text
- `.loading-dots` - Dot animation
- `.loading-bar` - Progress bar
- `.skeleton` - Skeleton loader
- `.skeleton--text`, `--title`, `--avatar`, `--button`, `--card` - Skeleton types
- `.skeleton--animated` - Animated shimmer

#### Modals
- `.modal` - Modal overlay
- `.modal--sm`, `--md`, `--lg`, `--xl`, `--fullscreen` - Size variants
- `.modal__backdrop` - Background overlay
- `.modal__container` - Content container
- `.modal__content` - Content wrapper
- `.modal__header` - Header section
- `.modal__title` - Title text
- `.modal__subtitle` - Subtitle text
- `.modal__close` - Close button
- `.modal__body` - Body content
- `.modal__footer` - Footer actions
- `.modal-overlay` - Alternative overlay
- `.drawer` - Side panel modal
- `.drawer--left`, `--right` - Position variants

#### Navigation
- `.nav` - Navigation container
- `.nav--horizontal`, `--vertical` - Orientation
- `.nav--pills`, `--tabs`, `--underline` - Style variants
- `.nav__list` - Nav list
- `.nav__item` - Nav item
- `.nav__link` - Nav link
- `.nav__link--active`, `--disabled` - Link states
- `.nav__icon` - Nav icon
- `.nav__text` - Nav text
- `.nav__badge` - Nav badge
- `.nav__divider` - Section divider
- `.nav-menu` - Menu component
- `.nav-menu--collapsed` - Collapsed state

#### Notification Cards
- `.notification-card` - Notification container
- `.notification-card--unread` - Unread state
- `.notification-card__icon` - Icon display
- `.notification-card__content` - Content wrapper
- `.notification-card__title` - Title text
- `.notification-card__message` - Message text
- `.notification-card__time` - Timestamp
- `.notification-card__actions` - Action buttons

#### Pagination
- `.pagination` - Pagination container
- `.pagination--compact` - Compact variant
- `.pagination__list` - Page list
- `.pagination__item` - Page item
- `.pagination__link` - Page link
- `.pagination__link--active`, `--disabled` - Link states
- `.pagination__prev`, `__next` - Navigation buttons
- `.pagination__info` - Page info text
- `.pagination__select` - Page size selector

#### Progress
- `.progress` - Progress container
- `.progress--striped`, `--animated` - Style variants
- `.progress--sm`, `--md`, `--lg` - Size variants
- `.progress__bar` - Progress bar
- `.progress__value` - Value text
- `.progress__label` - Label text
- `.progress-circular` - Circular progress
- `.progress-circular__svg` - SVG element
- `.progress-circular__circle` - Circle path

#### Sections
- `.section` - Section container
- `.section--elevated`, `--compact` - Style variants
- `.section--bordered` - Border variant
- `.section__header` - Header area
- `.section__title` - Title text
- `.section__subtitle` - Subtitle text
- `.section__actions` - Header actions
- `.section__content` - Main content
- `.section__footer` - Footer area

#### Sidebar/Layout
- `.layout` - Main layout container
- `.layout__container` - Content container
- `.layout__main` - Main content area
- `.sidebar` - Sidebar panel
- `.sidebar--collapsed` - Collapsed state
- `.sidebar--overlay` - Overlay mode
- `.sidebar__header` - Header section
- `.sidebar__content` - Main content
- `.sidebar__footer` - Footer section
- `.sidebar__toggle` - Toggle button
- `.sidebar-backdrop` - Mobile backdrop

#### Status Messages
- `.status-message` - Inline status
- `.status-message--info`, `--success`, `--warning`, `--error` - Status types
- `.status-message__icon` - Status icon

#### Summary Items
- `.summary-item` - Summary display
- `.summary-item--emphasized`, `--muted` - Style variants
- `.summary-item--horizontal` - Layout variant
- `.summary-item__label` - Label text
- `.summary-item__value` - Value text

#### Switcher
- `.switcher` - Switcher container
- `.switcher--horizontal`, `--vertical` - Orientation
- `.switcher--pills`, `--tabs`, `--buttons` - Style variants
- `.switcher--sm`, `--md`, `--lg` - Size variants
- `.switcher__item` - Switch item
- `.switcher__item--active`, `--disabled` - Item states
- `.switcher__label` - Item label
- `.switcher__check` - Check indicator
- `.switcher__loading` - Loading state

#### Tables
- `.table` - Table container
- `.table--compact`, `--comfortable` - Density variants
- `.table--bordered`, `--striped`, `--hover` - Style variants
- `.table--loading` - Loading state
- `.table__header` - Table header
- `.table__content` - Table body
- `.table__footer` - Table footer
- `.table__row` - Table row
- `.table__row--selected`, `--highlighted` - Row states
- `.table__cell` - Table cell
- `.table-cell-primary`, `-icon`, `-muted`, `-amount` - Cell variants
- `.table-status` - Status cell
- `.table-status--active`, `--inactive`, `--pending`, `--draft` - Status types
- `.table-actions` - Actions cell
- `.table-empty` - Empty state

#### Tabs
- `.tabs` - Tabs container
- `.tabs--pills`, `--underline` - Style variants
- `.tabs--vertical` - Vertical layout
- `.tabs__list` - Tab list
- `.tabs__tab` - Tab item
- `.tabs__tab--active`, `--disabled` - Tab states
- `.tabs__icon` - Tab icon
- `.tabs__text` - Tab text
- `.tabs__badge` - Tab badge
- `.tabs__panel` - Tab content
- `.tabs__content` - Panel wrapper

#### Theme Switcher
- `.theme-switcher` - Switcher container
- `.theme-switcher__group` - Button group
- `.theme-switcher__button` - Theme button
- `.theme-switcher__button--active` - Active theme
- `.theme-switcher__icon` - Theme icon

#### Timeline
- `.timeline` - Timeline container
- `.timeline--vertical`, `--horizontal` - Orientation
- `.timeline--compact` - Compact variant
- `.timeline__item` - Timeline entry
- `.timeline__marker` - Event marker
- `.timeline__connector` - Connection line
- `.timeline__content` - Content area
- `.timeline__time` - Timestamp
- `.timeline__title` - Event title
- `.timeline__description` - Event details
- `.timeline-meta` - Meta information

#### Toasts
- `.toast-container` - Toast container
- `.toast` - Toast notification
- `.toast--entering`, `--visible`, `--exiting` - Animation states
- `.toast--success`, `--error`, `--warning`, `--info` - Toast types
- `.toast__icon` - Toast icon
- `.toast__content` - Content wrapper
- `.toast__message` - Message text
- `.toast__close` - Close button

### Utility Classes

#### Spacing (MudBlazor-style)
##### Padding
- `.pa-0` to `.pa-32` - All sides padding
- `.pt-0` to `.pt-32` - Top padding
- `.pb-0` to `.pb-32` - Bottom padding
- `.pl-0` to `.pl-32` - Left padding
- `.pr-0` to `.pr-32` - Right padding
- `.px-0` to `.px-32` - Horizontal padding
- `.py-0` to `.py-32` - Vertical padding

##### Margin
- `.ma-0` to `.ma-32` - All sides margin
- `.mt-0` to `.mt-32` - Top margin
- `.mb-0` to `.mb-32` - Bottom margin
- `.ml-0` to `.ml-32` - Left margin
- `.mr-0` to `.mr-32` - Right margin
- `.mx-0` to `.mx-32` - Horizontal margin
- `.my-0` to `.my-32` - Vertical margin
- `.mx-auto`, `.ml-auto`, `.mr-auto` - Auto margins

##### Negative Margins
- `.ma-n1` to `.ma-n6` - Negative all sides
- `.mt-n1` to `.mt-n6` - Negative top
- `.mb-n1` to `.mb-n6` - Negative bottom
- `.ml-n1` to `.ml-n6` - Negative left
- `.mr-n1` to `.mr-n6` - Negative right
- `.mx-n1` to `.mx-n6` - Negative horizontal
- `.my-n1` to `.my-n6` - Negative vertical

##### Gap
- `.gap-0` to `.gap-24` - Flexbox/Grid gap
- `.gap-x-0` to `.gap-x-12` - Column gap
- `.gap-y-0` to `.gap-y-12` - Row gap
- `.space-x-0` to `.space-x-8` - Space between (horizontal)
- `.space-y-0` to `.space-y-8` - Space between (vertical)

#### Display
- `.d-none` - Display none
- `.d-inline` - Display inline
- `.d-inline-block` - Display inline-block
- `.d-block` - Display block
- `.d-table` - Display table
- `.d-table-row` - Display table-row
- `.d-table-cell` - Display table-cell
- `.d-flex` - Display flex
- `.d-inline-flex` - Display inline-flex
- `.d-grid` - Display grid

#### Flexbox
- `.flex` - Display flex (shorthand)
- `.inline-flex` - Display inline-flex
- `.flex-row` - Flex direction row
- `.flex-row-reverse` - Flex direction row-reverse
- `.flex-col`, `.flex-column` - Flex direction column
- `.flex-col-reverse` - Flex direction column-reverse
- `.flex-wrap` - Flex wrap
- `.flex-wrap-reverse` - Flex wrap reverse
- `.flex-nowrap` - Flex no wrap
- `.flex-1` - Flex: 1
- `.flex-auto` - Flex: auto
- `.flex-initial` - Flex: initial
- `.flex-none` - Flex: none
- `.flex-grow-0`, `.flex-grow` - Flex grow
- `.flex-shrink-0`, `.flex-shrink` - Flex shrink
- `.basis-0`, `.basis-auto`, `.basis-full` - Flex basis

##### Justify Content
- `.justify-start` - Justify flex-start
- `.justify-end` - Justify flex-end
- `.justify-center` - Justify center
- `.justify-between` - Justify space-between
- `.justify-around` - Justify space-around
- `.justify-evenly` - Justify space-evenly

##### Align Items
- `.items-start` - Align items flex-start
- `.items-end` - Align items flex-end
- `.items-center` - Align items center
- `.items-baseline` - Align items baseline
- `.items-stretch` - Align items stretch

##### Align Content
- `.content-normal` - Align content normal
- `.content-start` - Align content flex-start
- `.content-end` - Align content flex-end
- `.content-center` - Align content center
- `.content-between` - Align content space-between
- `.content-around` - Align content space-around
- `.content-evenly` - Align content space-evenly
- `.content-baseline` - Align content baseline
- `.content-stretch` - Align content stretch

##### Align Self
- `.self-auto` - Align self auto
- `.self-start` - Align self flex-start
- `.self-end` - Align self flex-end
- `.self-center` - Align self center
- `.self-stretch` - Align self stretch
- `.self-baseline` - Align self baseline

#### Grid
- `.grid` - Display grid
- `.inline-grid` - Display inline-grid
- `.grid-cols-1` to `.grid-cols-12` - Grid columns
- `.grid-rows-1` to `.grid-rows-6` - Grid rows
- `.col-auto`, `.col-span-1` to `.col-span-12` - Column span
- `.col-start-1` to `.col-start-13` - Column start
- `.col-end-1` to `.col-end-13` - Column end
- `.row-auto`, `.row-span-1` to `.row-span-6` - Row span
- `.row-start-1` to `.row-start-7` - Row start
- `.row-end-1` to `.row-end-7` - Row end
- `.auto-cols-auto`, `.auto-cols-min`, `.auto-cols-max`, `.auto-cols-fr` - Auto columns
- `.auto-rows-auto`, `.auto-rows-min`, `.auto-rows-max`, `.auto-rows-fr` - Auto rows
- `.grid-flow-row`, `.grid-flow-col`, `.grid-flow-dense` - Grid flow

##### Place Content/Items/Self
- `.place-content-center`, `.place-content-start`, `.place-content-end` - Place content
- `.place-items-start`, `.place-items-end`, `.place-items-center` - Place items
- `.place-self-auto`, `.place-self-start`, `.place-self-end`, `.place-self-center` - Place self

#### Typography
##### Font Size
- `.text-xs` - Extra small text
- `.text-sm` - Small text
- `.text-base` - Base text
- `.text-lg` - Large text
- `.text-xl` to `.text-9xl` - Extra large text
- `.text-h1` to `.text-h6` - Heading sizes
- `.text-body-1`, `.text-body-2` - Body text
- `.text-caption` - Caption text

##### Font Weight
- `.font-thin` - Font weight 100
- `.font-extralight` - Font weight 200
- `.font-light` - Font weight 300
- `.font-normal` - Font weight 400
- `.font-medium` - Font weight 500
- `.font-semibold` - Font weight 600
- `.font-bold` - Font weight 700
- `.font-extrabold` - Font weight 800
- `.font-black` - Font weight 900

##### Font Style & Transform
- `.italic` - Font style italic
- `.not-italic` - Font style normal
- `.uppercase` - Text transform uppercase
- `.lowercase` - Text transform lowercase
- `.capitalize` - Text transform capitalize
- `.normal-case` - Text transform none

##### Text Alignment
- `.text-left` - Text align left
- `.text-center` - Text align center
- `.text-right` - Text align right
- `.text-justify` - Text align justify

##### Text Color
- `.text-primary` - Primary text color
- `.text-secondary`, `.text--secondary` - Secondary text color
- `.text-tertiary`, `.text--tertiary` - Tertiary text color
- `.text-success` - Success text color
- `.text-warning` - Warning text color
- `.text-error` - Error text color
- `.text-info` - Info text color
- `.text-muted`, `.text--muted` - Muted text color
- `.text-inverse` - Inverse text color
- `.text-current` - Current color
- `.text-transparent` - Transparent text

##### Text Behavior
- `.text-truncate` - Text overflow ellipsis
- `.text-wrap` - Text wrap
- `.text-nowrap` - White-space nowrap
- `.text-break` - Word break
- `.line-clamp-1` to `.line-clamp-6` - Line clamping
- `.line-clamp-none` - Remove line clamp
- `.whitespace-normal`, `.whitespace-nowrap`, `.whitespace-pre` - Whitespace
- `.break-normal`, `.break-words`, `.break-all`, `.break-keep` - Word breaking

##### Line Height
- `.leading-none` - Line height 1
- `.leading-tight` - Line height 1.25
- `.leading-snug` - Line height 1.375
- `.leading-normal` - Line height 1.5
- `.leading-relaxed` - Line height 1.625
- `.leading-loose` - Line height 2

##### Letter Spacing
- `.tracking-tighter` - Letter spacing -0.05em
- `.tracking-tight` - Letter spacing -0.025em
- `.tracking-normal` - Letter spacing 0
- `.tracking-wide` - Letter spacing 0.025em
- `.tracking-wider` - Letter spacing 0.05em
- `.tracking-widest` - Letter spacing 0.1em

#### Colors & Backgrounds
##### Background Colors
- `.bg-transparent` - Transparent background
- `.bg-primary` - Primary background
- `.bg-secondary` - Secondary background
- `.bg-elevated` - Elevated background
- `.bg-surface` - Surface background
- `.bg-subtle` - Subtle background
- `.bg-success` - Success background
- `.bg-warning` - Warning background
- `.bg-error` - Error background
- `.bg-info` - Info background
- `.bg-white` - White background
- `.bg-black` - Black background

##### Background Gradients
- `.bg-gradient-primary` - Primary gradient
- `.bg-gradient-secondary` - Secondary gradient
- `.bg-gradient-subtle` - Subtle gradient
- `.bg-gradient-radial` - Radial gradient
- `.bg-gradient-conic` - Conic gradient

##### Background Properties
- `.bg-fixed` - Background attachment fixed
- `.bg-local` - Background attachment local
- `.bg-scroll` - Background attachment scroll
- `.bg-clip-border` - Background clip border-box
- `.bg-clip-padding` - Background clip padding-box
- `.bg-clip-content` - Background clip content-box
- `.bg-clip-text` - Background clip text
- `.bg-origin-border` - Background origin border-box
- `.bg-origin-padding` - Background origin padding-box
- `.bg-origin-content` - Background origin content-box
- `.bg-repeat` - Background repeat
- `.bg-no-repeat` - Background no repeat
- `.bg-repeat-x` - Background repeat-x
- `.bg-repeat-y` - Background repeat-y
- `.bg-repeat-round` - Background repeat round
- `.bg-repeat-space` - Background repeat space
- `.bg-auto` - Background size auto
- `.bg-cover` - Background size cover
- `.bg-contain` - Background size contain

##### Background Position
- `.bg-bottom` - Background position bottom
- `.bg-center` - Background position center
- `.bg-left` - Background position left
- `.bg-left-bottom` - Background position left bottom
- `.bg-left-top` - Background position left top
- `.bg-right` - Background position right
- `.bg-right-bottom` - Background position right bottom
- `.bg-right-top` - Background position right top
- `.bg-top` - Background position top

#### Borders
##### Border Width
- `.border` - Border 1px
- `.border-0` - Border 0
- `.border-2` - Border 2px
- `.border-4` - Border 4px
- `.border-8` - Border 8px
- `.border-t`, `.border-r`, `.border-b`, `.border-l` - Directional borders
- `.border-x`, `.border-y` - Axis borders
- `.border-t-0`, `.border-r-0`, `.border-b-0`, `.border-l-0` - Remove directional
- `.border-none` - No border

##### Border Style
- `.border-solid` - Border style solid
- `.border-dashed` - Border style dashed
- `.border-dotted` - Border style dotted
- `.border-double` - Border style double

##### Border Color
- `.border-transparent` - Transparent border
- `.border-current` - Current color border
- `.border-primary` - Primary border
- `.border-secondary` - Secondary border
- `.border-success` - Success border
- `.border-warning` - Warning border
- `.border-error` - Error border
- `.border-light` - Light border
- `.border-medium` - Medium border
- `.border-strong` - Strong border

##### Border Radius
- `.rounded-none` - Border radius 0
- `.rounded-sm` - Small radius
- `.rounded` - Default radius
- `.rounded-md` - Medium radius
- `.rounded-lg` - Large radius
- `.rounded-xl` - Extra large radius
- `.rounded-2xl` - 2x large radius
- `.rounded-3xl` - 3x large radius
- `.rounded-full` - Full radius (circle/pill)
- `.rounded-t-*`, `.rounded-r-*`, `.rounded-b-*`, `.rounded-l-*` - Directional radius
- `.rounded-tl-*`, `.rounded-tr-*`, `.rounded-br-*`, `.rounded-bl-*` - Corner radius

#### Effects
##### Box Shadow
- `.shadow-none` - No shadow
- `.shadow-sm` - Small shadow
- `.shadow` - Default shadow
- `.shadow-md` - Medium shadow
- `.shadow-lg` - Large shadow
- `.shadow-xl` - Extra large shadow
- `.shadow-2xl` - 2x large shadow
- `.shadow-inner` - Inner shadow
- `.shadow-outline` - Outline shadow

##### Colored Shadows
- `.shadow-primary` - Primary colored shadow
- `.shadow-secondary` - Secondary colored shadow
- `.shadow-success` - Success colored shadow
- `.shadow-warning` - Warning colored shadow
- `.shadow-error` - Error colored shadow
- `.shadow-info` - Info colored shadow

##### Ring
- `.ring-0` to `.ring-8` - Ring width
- `.ring-inset` - Inset ring
- `.ring-primary` - Primary ring color
- `.ring-secondary` - Secondary ring color
- `.ring-success` - Success ring color
- `.ring-warning` - Warning ring color
- `.ring-error` - Error ring color
- `.ring-offset-0` to `.ring-offset-8` - Ring offset

##### Opacity
- `.opacity-0` - Opacity 0%
- `.opacity-5` - Opacity 5%
- `.opacity-10` - Opacity 10%
- `.opacity-20` - Opacity 20%
- `.opacity-25` - Opacity 25%
- `.opacity-30` - Opacity 30%
- `.opacity-40` - Opacity 40%
- `.opacity-50` - Opacity 50%
- `.opacity-60` - Opacity 60%
- `.opacity-70` - Opacity 70%
- `.opacity-75` - Opacity 75%
- `.opacity-80` - Opacity 80%
- `.opacity-90` - Opacity 90%
- `.opacity-95` - Opacity 95%
- `.opacity-100` - Opacity 100%

#### Filters
##### Blur
- `.blur-none` - No blur
- `.blur-sm` - Small blur
- `.blur` - Default blur
- `.blur-md` - Medium blur
- `.blur-lg` - Large blur
- `.blur-xl` - Extra large blur
- `.blur-2xl` - 2x large blur
- `.blur-3xl` - 3x large blur

##### Backdrop Blur
- `.backdrop-blur-none` - No backdrop blur
- `.backdrop-blur-sm` - Small backdrop blur
- `.backdrop-blur` - Default backdrop blur
- `.backdrop-blur-md` - Medium backdrop blur
- `.backdrop-blur-lg` - Large backdrop blur
- `.backdrop-blur-xl` - Extra large backdrop blur
- `.backdrop-blur-2xl` - 2x large backdrop blur
- `.backdrop-blur-3xl` - 3x large backdrop blur

##### Other Filters
- `.brightness-0` to `.brightness-200` - Brightness
- `.contrast-0` to `.contrast-200` - Contrast
- `.grayscale-0`, `.grayscale` - Grayscale
- `.hue-rotate-0` to `.hue-rotate-180` - Hue rotate
- `.invert-0`, `.invert` - Invert
- `.saturate-0` to `.saturate-200` - Saturate
- `.sepia-0`, `.sepia` - Sepia
- `.drop-shadow-*` - Drop shadow variants

#### Glassmorphism
- `.glass-light` - Light glass effect
- `.glass-medium` - Medium glass effect
- `.glass-heavy` - Heavy glass effect
- `.glass-frost` - Frosted glass effect
- `.glass-crystal` - Crystal clear effect
- `.glass-primary` - Primary colored glass
- `.glass-success` - Success colored glass
- `.glass-warning` - Warning colored glass
- `.glass-error` - Error colored glass
- `.glass-surface` - Glass surface for elevated components
- `.glass-elevated` - Glass with elevation
- `.glass-interactive` - Interactive glass with hover states

#### Transforms
##### Scale
- `.scale-0` to `.scale-150` - Scale transform
- `.scale-x-0` to `.scale-x-150` - Scale X
- `.scale-y-0` to `.scale-y-150` - Scale Y

##### Rotate
- `.rotate-0` to `.rotate-180` - Rotate
- `.-rotate-180` to `.-rotate-1` - Negative rotate

##### Translate
- `.translate-x-0` to `.translate-x-full` - Translate X
- `.translate-y-0` to `.translate-y-full` - Translate Y
- `.-translate-x-full` to `.-translate-x-0` - Negative translate X
- `.-translate-y-full` to `.-translate-y-0` - Negative translate Y

##### Skew
- `.skew-x-0` to `.skew-x-12` - Skew X
- `.skew-y-0` to `.skew-y-12` - Skew Y
- `.-skew-x-12` to `.-skew-x-0` - Negative skew X
- `.-skew-y-12` to `.-skew-y-0` - Negative skew Y

##### Transform Origin
- `.origin-center` - Transform origin center
- `.origin-top` - Transform origin top
- `.origin-top-right` - Transform origin top right
- `.origin-right` - Transform origin right
- `.origin-bottom-right` - Transform origin bottom right
- `.origin-bottom` - Transform origin bottom
- `.origin-bottom-left` - Transform origin bottom left
- `.origin-left` - Transform origin left
- `.origin-top-left` - Transform origin top left

#### Layout & Positioning
##### Position
- `.static` - Position static
- `.fixed` - Position fixed
- `.absolute` - Position absolute
- `.relative` - Position relative
- `.sticky` - Position sticky

##### Position Values
- `.inset-0` - All sides 0
- `.inset-auto` - All sides auto
- `.inset-x-0`, `.inset-y-0` - Axis positioning
- `.top-0` to `.top-full` - Top position
- `.right-0` to `.right-full` - Right position
- `.bottom-0` to `.bottom-full` - Bottom position
- `.left-0` to `.left-full` - Left position

##### Z-Index
- `.z-0` - Z-index 0
- `.z-10` - Z-index 10
- `.z-20` - Z-index 20
- `.z-30` - Z-index 30
- `.z-40` - Z-index 40
- `.z-50` - Z-index 50
- `.z-auto` - Z-index auto

##### Overflow
- `.overflow-auto` - Overflow auto
- `.overflow-hidden` - Overflow hidden
- `.overflow-visible` - Overflow visible
- `.overflow-scroll` - Overflow scroll
- `.overflow-x-auto` - Overflow-x auto
- `.overflow-y-auto` - Overflow-y auto
- `.overflow-x-hidden` - Overflow-x hidden
- `.overflow-y-hidden` - Overflow-y hidden
- `.overflow-x-scroll` - Overflow-x scroll
- `.overflow-y-scroll` - Overflow-y scroll

##### Object Fit
- `.object-contain` - Object fit contain
- `.object-cover` - Object fit cover
- `.object-fill` - Object fit fill
- `.object-none` - Object fit none
- `.object-scale-down` - Object fit scale-down

##### Object Position
- `.object-bottom` - Object position bottom
- `.object-center` - Object position center
- `.object-left` - Object position left
- `.object-left-bottom` - Object position left bottom
- `.object-left-top` - Object position left top
- `.object-right` - Object position right
- `.object-right-bottom` - Object position right bottom
- `.object-right-top` - Object position right top
- `.object-top` - Object position top

#### Sizing
##### Width
- `.w-0` - Width 0
- `.w-px` - Width 1px
- `.w-0.5` to `.w-96` - Width rem values
- `.w-auto` - Width auto
- `.w-1/2`, `.w-1/3`, `.w-2/3`, `.w-1/4`, `.w-3/4` - Width fractions
- `.w-full` - Width 100%
- `.w-screen` - Width 100vw
- `.w-min` - Width min-content
- `.w-max` - Width max-content
- `.w-fit` - Width fit-content

##### Min Width
- `.min-w-0` - Min width 0
- `.min-w-full` - Min width 100%
- `.min-w-min` - Min width min-content
- `.min-w-max` - Min width max-content
- `.min-w-fit` - Min width fit-content

##### Max Width
- `.max-w-none` - Max width none
- `.max-w-xs` - Max width 20rem
- `.max-w-sm` - Max width 24rem
- `.max-w-md` - Max width 28rem
- `.max-w-lg` - Max width 32rem
- `.max-w-xl` - Max width 36rem
- `.max-w-2xl` - Max width 42rem
- `.max-w-3xl` - Max width 48rem
- `.max-w-4xl` - Max width 56rem
- `.max-w-5xl` - Max width 64rem
- `.max-w-6xl` - Max width 72rem
- `.max-w-7xl` - Max width 80rem
- `.max-w-full` - Max width 100%
- `.max-w-screen-*` - Max width screen breakpoints

##### Height
- `.h-0` - Height 0
- `.h-px` - Height 1px
- `.h-0.5` to `.h-96` - Height rem values
- `.h-auto` - Height auto
- `.h-1/2`, `.h-1/3`, `.h-2/3`, `.h-1/4`, `.h-3/4` - Height fractions
- `.h-full` - Height 100%
- `.h-screen` - Height 100vh
- `.h-min` - Height min-content
- `.h-max` - Height max-content
- `.h-fit` - Height fit-content

##### Min/Max Height
- `.min-h-0` - Min height 0
- `.min-h-full` - Min height 100%
- `.min-h-screen` - Min height 100vh
- `.min-h-min` - Min height min-content
- `.min-h-max` - Min height max-content
- `.min-h-fit` - Min height fit-content
- `.max-h-*` - Max height values

#### Interactions
##### Cursor
- `.cursor-auto` - Cursor auto
- `.cursor-default` - Cursor default
- `.cursor-pointer` - Cursor pointer
- `.cursor-wait` - Cursor wait
- `.cursor-text` - Cursor text
- `.cursor-move` - Cursor move
- `.cursor-help` - Cursor help
- `.cursor-not-allowed` - Cursor not-allowed
- `.cursor-none` - Cursor none
- `.cursor-context-menu` - Cursor context-menu
- `.cursor-progress` - Cursor progress
- `.cursor-cell` - Cursor cell
- `.cursor-crosshair` - Cursor crosshair
- `.cursor-vertical-text` - Cursor vertical-text
- `.cursor-alias` - Cursor alias
- `.cursor-copy` - Cursor copy
- `.cursor-no-drop` - Cursor no-drop
- `.cursor-grab` - Cursor grab
- `.cursor-grabbing` - Cursor grabbing

##### User Select
- `.select-none` - User select none
- `.select-text` - User select text
- `.select-all` - User select all
- `.select-auto` - User select auto

##### Pointer Events
- `.pointer-events-none` - Pointer events none
- `.pointer-events-auto` - Pointer events auto

##### Resize
- `.resize-none` - Resize none
- `.resize-y` - Resize vertical
- `.resize-x` - Resize horizontal
- `.resize` - Resize both

#### Transitions & Animations
##### Transition Property
- `.transition-none` - No transition
- `.transition-all` - All properties
- `.transition` - Default properties
- `.transition-colors` - Color properties
- `.transition-opacity` - Opacity only
- `.transition-shadow` - Shadow only
- `.transition-transform` - Transform only

##### Transition Duration
- `.duration-75` - 75ms
- `.duration-100` - 100ms
- `.duration-150` - 150ms
- `.duration-200` - 200ms
- `.duration-300` - 300ms
- `.duration-500` - 500ms
- `.duration-700` - 700ms
- `.duration-1000` - 1000ms

##### Transition Timing
- `.ease-linear` - Linear timing
- `.ease-in` - Ease in
- `.ease-out` - Ease out
- `.ease-in-out` - Ease in-out

##### Transition Delay
- `.delay-75` to `.delay-1000` - Transition delays

##### Animation
- `.animate-none` - No animation
- `.animate-spin` - Spin animation
- `.animate-ping` - Ping animation
- `.animate-pulse` - Pulse animation
- `.animate-bounce` - Bounce animation
- `.animate-fade-in` - Fade in
- `.animate-fade-out` - Fade out
- `.animate-scale-in` - Scale in
- `.animate-scale-out` - Scale out
- `.animate-slide-in-right` - Slide from right
- `.animate-slide-in-left` - Slide from left
- `.animate-slide-in-top` - Slide from top
- `.animate-slide-in-bottom` - Slide from bottom
- `.animate-shake` - Shake animation

#### Icons
##### Icon Sizes
- `.icon-xs` - Extra small icon
- `.icon-sm` - Small icon
- `.icon-base` - Base icon size
- `.icon-lg` - Large icon
- `.icon-xl` - Extra large icon
- `.icon-2xl` to `.icon-5xl` - Larger icons

##### Icon Classes
- `.icon` - Base icon class
- `.icon--material` - Material symbols
- `.icon--primary` - Primary color
- `.icon--secondary` - Secondary color
- `.icon--success` - Success color
- `.icon--warning` - Warning color
- `.icon--error` - Error color
- `.icon--info` - Info color
- `.icon--muted` - Muted icon
- `.icon--interactive` - Interactive icon
- `.icon--clickable` - Clickable icon
- `.icon--button` - Button-like icon
- `.icon--filled` - Filled variant
- `.icon--outlined` - Outlined variant
- `.icon--loading` - Spinning icon

#### Accessibility
- `.sr-only` - Screen reader only
- `.sr-only-focusable` - Screen reader only but focusable
- `.not-sr-only` - Not screen reader only
- `.focus-visible` - Focus visible only
- `.focus-ring` - Focus ring
- `.focus-ring--thick` - Thick focus ring
- `.focus-ring--inset` - Inset focus ring
- `.high-contrast-border` - High contrast mode border
- `.high-contrast-text` - High contrast mode text
- `.aria-live-polite` - ARIA live region polite
- `.aria-live-assertive` - ARIA live region assertive
- `.touch-target` - 44px minimum touch target
- `.touch-target--lg` - 48px touch target
- `.motion-safe-transition` - Transition only if motion allowed
- `.motion-safe-animate` - Animate only if motion allowed
- `.motion-reduce\\:transition-none` - No transition if reduced motion
- `.motion-reduce\\:transform-none` - No transform if reduced motion

#### Other Utilities
##### Aspect Ratio
- `.aspect-auto` - Aspect ratio auto
- `.aspect-square` - Aspect ratio 1/1
- `.aspect-video` - Aspect ratio 16/9
- `.aspect-4-3` - Aspect ratio 4/3
- `.aspect-21-9` - Aspect ratio 21/9

##### Columns
- `.columns-1` to `.columns-12` - Column count
- `.columns-auto` - Auto columns
- `.columns-3xs` to `.columns-7xl` - Column widths

##### Container
- `.container` - Responsive container
- `.container-sm` to `.container-2xl` - Container sizes

##### Scroll
- `.scroll-auto` - Scroll behavior auto
- `.scroll-smooth` - Scroll behavior smooth
- `.scroll-p-*` - Scroll padding
- `.scroll-m-*` - Scroll margin
- `.snap-none` - Scroll snap none
- `.snap-x`, `.snap-y` - Scroll snap axis
- `.snap-mandatory`, `.snap-proximity` - Scroll snap strictness
- `.snap-start`, `.snap-end`, `.snap-center` - Scroll snap align
- `.snap-always`, `.snap-normal` - Scroll snap stop

##### Touch Action
- `.touch-auto` - Touch action auto
- `.touch-none` - Touch action none
- `.touch-pan-x` - Touch action pan-x
- `.touch-pan-y` - Touch action pan-y
- `.touch-pan-left` - Touch action pan-left
- `.touch-pan-right` - Touch action pan-right
- `.touch-pan-up` - Touch action pan-up
- `.touch-pan-down` - Touch action pan-down
- `.touch-pinch-zoom` - Touch action pinch-zoom
- `.touch-manipulation` - Touch action manipulation

##### Will Change
- `.will-change-auto` - Will change auto
- `.will-change-scroll` - Will change scroll
- `.will-change-contents` - Will change contents
- `.will-change-transform` - Will change transform

#### Responsive Prefixes
All utility classes support responsive prefixes:
- `sm:` - Small screens (640px+)
- `md:` - Medium screens (768px+)
- `lg:` - Large screens (1024px+)
- `xl:` - Extra large screens (1280px+)
- `2xl:` - 2x large screens (1536px+)

Examples:
- `.d-none .d-md-block` - Hidden on mobile, visible on medium+
- `.text-center .text-lg-left` - Center on mobile, left on large+
- `.pa-4 .md:pa-6 .lg:pa-8` - Responsive padding

#### State Modifiers
Many utilities support state modifiers:
- `hover:` - Hover state
- `focus:` - Focus state
- `active:` - Active state
- `disabled:` - Disabled state
- `visited:` - Visited state
- `group-hover:` - Parent hover state

Examples:
- `.hover:bg-primary` - Primary background on hover
- `.focus:ring-2` - Ring on focus
- `.active:scale-95` - Scale on active
- `.group-hover:text-white` - White text when parent hovered

### Animation Keyframes

#### Basic Motion
- `@keyframes fadeIn` - Fade in effect
- `@keyframes fadeOut` - Fade out effect
- `@keyframes fadeInUp` - Fade in with upward motion
- `@keyframes fadeOutDown` - Fade out with downward motion
- `@keyframes scaleIn` - Scale in from 0.9
- `@keyframes scaleOut` - Scale out to 0.9

#### Directional Slides
- `@keyframes slideLeft` - Slide from left
- `@keyframes slideRight` - Slide from right
- `@keyframes slideUp` - Slide from bottom
- `@keyframes slideDown` - Slide from top
- `@keyframes slide-in-from-right` - Slide in from right
- `@keyframes slide-in-from-left` - Slide in from left
- `@keyframes slide-in-from-top` - Slide in from top
- `@keyframes slide-in-from-bottom` - Slide in from bottom

#### Rotation & Movement
- `@keyframes spin` - 360° continuous rotation
- `@keyframes shake` - Horizontal shake
- `@keyframes bounce` - Vertical bounce
- `@keyframes pulse` - Opacity pulse
- `@keyframes ping` - Scale and fade ping
- `@keyframes float` - Gentle floating
- `@keyframes floatOrb` - Complex orb floating

#### Loading & Progress
- `@keyframes shimmer` - Shimmer loading effect
- `@keyframes progress` - Progress bar animation
- `@keyframes loading-shimmer` - Loading skeleton
- `@keyframes skeleton-wave` - Skeleton wave effect
- `@keyframes loading-dots` - Dot loading animation

#### Status Effects
- `@keyframes pulseRing` - Expanding ring pulse
- `@keyframes pulseSoft` - Soft scale pulse
- `@keyframes breathe` - Breathing effect
- `@keyframes glow` - Glowing effect

#### Special Effects
- `@keyframes fade-in-luxury` - Premium fade with scale
- `@keyframes protected-modal-shake` - Security modal shake
- `@keyframes ripple` - Material ripple effect
- `@keyframes iconSpin` - Icon rotation

### CSS Variables (Custom Properties)

#### Spacing Scale
- `--space-0` through `--space-32` - 0 to 128px (4px increments)
- `--space-px` - 1px

#### Color System
##### Brand Colors
- `--color-primary` - Primary brand color
- `--color-primary-light` - Light variant
- `--color-primary-dark` - Dark variant
- `--color-secondary` - Secondary brand color
- `--color-accent` - Accent color

##### Semantic Colors
- `--color-success` - Success state
- `--color-success-light` - Light success
- `--color-success-dark` - Dark success
- `--color-warning` - Warning state
- `--color-warning-light` - Light warning
- `--color-warning-dark` - Dark warning
- `--color-error` - Error state
- `--color-error-light` - Light error
- `--color-error-dark` - Dark error
- `--color-info` - Info state
- `--color-info-light` - Light info
- `--color-info-dark` - Dark info

##### Background Colors
- `--color-background-primary` - Main background
- `--color-background-secondary` - Secondary background
- `--color-background-elevated` - Elevated surfaces
- `--color-background-subtle` - Subtle background
- `--color-background-modal` - Modal backdrop

##### Text Colors
- `--color-text-primary` - Primary text
- `--color-text-secondary` - Secondary text
- `--color-text-tertiary` - Tertiary text
- `--color-text-muted` - Muted text
- `--color-text-inverse` - Inverse text
- `--color-text-on-primary` - Text on primary

##### Border Colors
- `--color-border-light` - Light borders
- `--color-border-medium` - Medium borders
- `--color-border-strong` - Strong borders

##### Interactive Colors
- `--color-interactive-primary` - Primary interactive
- `--color-interactive-hover` - Hover state
- `--color-interactive-active` - Active state
- `--color-interactive-disabled` - Disabled state

#### Typography
##### Font Families
- `--font-family-primary` - Primary font stack
- `--font-family-secondary` - Secondary font stack
- `--font-family-mono` - Monospace font stack

##### Font Sizes
- `--text-xs` through `--text-9xl` - Font size scale
- `--text-h1` through `--text-h6` - Heading sizes
- `--text-body-1`, `--text-body-2` - Body text sizes
- `--text-caption` - Caption size

##### Font Weights
- `--font-thin` through `--font-black` - Weight scale (100-900)

##### Line Heights
- `--leading-none` through `--leading-loose` - Line height scale

##### Letter Spacing
- `--tracking-tighter` through `--tracking-widest` - Letter spacing scale

#### Layout
##### Border Radius
- `--radius-sm` through `--radius-full` - Radius scale
- `--radius-none` - No radius

##### Borders
- `--border-1`, `--border-2`, `--border-4` - Border widths

##### Shadows
- `--shadow-sm` through `--shadow-2xl` - Shadow scale
- `--shadow-inner` - Inner shadow
- `--shadow-outline` - Outline shadow
- `--shadow-color-*` - Shadow color opacity

##### Z-Index
- `--z-dropdown` - Dropdown menus (1000)
- `--z-sticky` - Sticky elements (1020)
- `--z-fixed` - Fixed elements (1030)
- `--z-modal-backdrop` - Modal backdrop (1040)
- `--z-modal` - Modal content (1050)
- `--z-popover` - Popovers (1060)
- `--z-tooltip` - Tooltips (1070)
- `--z-toast` - Toast notifications (1080)

#### Animation
##### Durations
- `--duration-75` through `--duration-1000` - Animation durations

##### Easing Functions
- `--ease-linear` - Linear easing
- `--ease-in` - Ease in
- `--ease-out` - Ease out
- `--ease-in-out` - Ease in-out
- `--ease-out-back` - Ease out with overshoot

#### Effects
##### Blur
- `--blur-none` through `--blur-3xl` - Blur amounts

##### Opacity
- `--opacity-0` through `--opacity-100` - Opacity scale

#### Icons
##### Icon Sizes
- `--icon-xs` through `--icon-5xl` - Icon size scale

#### Glass Effects
- `--glass-bg-light` - Light glass background
- `--glass-bg-medium` - Medium glass background
- `--glass-bg-strong` - Strong glass background
- `--glass-border-light` - Light glass border
- `--glass-border-strong` - Strong glass border

### SCSS Extends (Placeholders)

#### Component Bases
- `%component-base` - Generic component foundation
- `%card-base-enhanced` - Enhanced card with hover effects
- `%card-elevated` - Elevated card variant
- `%button-base` - Button foundation styles
- `%button-state-base` - Button state handling
- `%icon-button` - Icon button specific styles
- `%modal-base` - Modal foundation
- `%input-base` - Form input foundation
- `%table-base` - Table foundation
- `%list-base` - List foundation

#### Layout Patterns
- `%section-base` - Section container
- `%section-title` - Section title styling
- `%section-title-with-icon` - Title with icon
- `%content-grid` - Content grid layout
- `%responsive-grid-base` - Responsive grid foundation
- `%responsive-grid-auto-fit` - Auto-fit grid
- `%responsive-grid-auto-sm` - Small auto grid
- `%responsive-grid-auto-lg` - Large auto grid

#### Typography
- `%text-truncate` - Single line truncation
- `%text-wrap` - Text wrapping
- `%text-no-select` - Prevent text selection
- `%heading-base` - Heading foundation
- `%body-text-base` - Body text foundation

#### Form Elements
- `%form-base` - Form container
- `%form-group-base` - Form group wrapper
- `%form-row-base` - Form row layout
- `%input-base-style` - Input styling
- `%input-search` - Search input specific
- `%checkbox-base` - Checkbox foundation
- `%radio-base` - Radio foundation

#### Status & Feedback
- `%status-indicator-base` - Status indicators
- `%loading-skeleton-base` - Loading skeleton
- `%empty-state-base` - Empty state layout
- `%error-state-base` - Error state display

#### Icons
- `%icon-container-base` - Icon container
- `%icon-container-sm` - Small icon container
- `%icon-container-md` - Medium icon container
- `%icon-container-lg` - Large icon container
- `%icon-container-xl` - Extra large icon container

#### Tables
- `%table-cell-base` - Table cell foundation
- `%table-header-base` - Table header styling
- `%table-row-hover` - Row hover effects

#### Lists
- `%list-reset` - Reset list styles
- `%list-styled` - Styled list
- `%list-item-base` - List item foundation

#### Images
- `%img-avatar` - Avatar image styling
- `%img-logo` - Logo image styling
- `%img-cover` - Cover image
- `%img-contain` - Contained image

#### Utilities
- `%scrollable-area-base` - Custom scrollbar area
- `%visually-hidden` - Accessible hiding
- `%clearfix` - Clear floats

## R*.razor Component Reference

### Core UI Components

#### RButton
```razor
<RButton Text="Click Me" 
         Variant="ButtonVariant.Primary"
         Size="ButtonSize.Medium"
         Icon="add" IconPosition="IconPosition.Start"
         Loading="@isLoading"
         Disabled="@isDisabled"
         FullWidth="false"
         Elevation="2"
         OnClick="@HandleClick" />
```
- **Properties**: Text, Variant, Size, StartIcon, EndIcon, Loading, Disabled, FullWidth, Elevation, OnClick, Type, AriaLabel

#### RCard
```razor
<RCard Title="Dashboard" 
       Subtitle="Overview of your data"
       Elevation="4"
       IsClickable="true"
       Variant="CardVariant.Elevated"
       Class="custom-class">
    <HeaderActions>
        <RButton Text="Edit" Size="ButtonSize.Small" />
    </HeaderActions>
    <ChildContent>
        <!-- Card content -->
    </ChildContent>
    <FooterContent>
        <!-- Footer actions -->
    </FooterContent>
</RCard>
```
- **Properties**: Title, Subtitle, Elevation, IsClickable, Variant, Class, HeaderActions, ChildContent, FooterContent, OnClick

#### RModal
```razor
<RModal @ref="modal"
        Title="Confirm Action"
        Size="ModalSize.Medium"
        ShowCloseButton="true"
        ShowFooter="true"
        PreventClose="false"
        Variant="ModalVariant.Default">
    <BodyContent>
        <!-- Modal content -->
    </BodyContent>
    <FooterContent>
        <RButton Text="Cancel" Variant="ButtonVariant.Secondary" OnClick="@modal.Close" />
        <RButton Text="Confirm" Variant="ButtonVariant.Primary" />
    </FooterContent>
</RModal>
```
- **Properties**: Title, Subtitle, Size, ShowCloseButton, ShowFooter, PreventClose, Variant, IsVisible, OnClose
- **Methods**: Show(), Close(), Toggle()

#### RSection
```razor
<RSection Title="User Management"
          Subtitle="Manage system users"
          Elevation="2"
          IsCollapsible="true"
          DefaultExpanded="true">
    <HeaderActions>
        <RButton Text="Add User" Icon="add" IconPosition="IconPosition.Start" />
    </HeaderActions>
    <ChildContent>
        <!-- Section content -->
    </ChildContent>
</RSection>
```
- **Properties**: Title, Subtitle, Elevation, IsCollapsible, DefaultExpanded, Class, HeaderActions, ChildContent

#### RTabs
```razor
<RTabs @bind-ActiveTab="activeTab"
       Variant="TabsVariant.Standard">
    <RTab Title="Overview" Icon="dashboard">
        <!-- Tab 1 content -->
    </RTab>
    <RTab Title="Settings" Icon="settings" BadgeText="3">
        <!-- Tab 2 content -->
    </RTab>
    <RTab Title="Reports" Icon="analytics" Disabled="true">
        <!-- Tab 3 content -->
    </RTab>
</RTabs>
```
- **Properties**: ActiveTab, Variant, OnTabChanged
- **RTab Properties**: Title, Icon, BadgeText, BadgeVariant, Disabled

### Form Components

#### RFormField
```razor
<RFormField Label="Email Address"
            Type="FieldType.Email"
            @bind-Value="email"
            Required="true"
            Placeholder="Enter your email"
            HelperText="We'll never share your email"
            Error="@emailError"
            Icon="email" IconPosition="IconPosition.Start" />
```
- **Properties**: Label, Type, Value, Required, Placeholder, HelperText, Error, Icon, IconPosition, Disabled, ReadOnly, OnValueChanged, ValidationRules

#### RDatePicker
```razor
<RDatePicker @bind-Value="selectedDate"
             Label="Select Date"
             MinDate="@DateTime.Today"
             MaxDate="@DateTime.Today.AddYears(1)"
             DateFormat="yyyy-MM-dd"
             ShowTime="false"
             Required="true" />
```
- **Properties**: Value, Label, MinDate, MaxDate, DateFormat, ShowTime, TimeFormat, Required, Disabled, OnValueChanged

#### RFileUpload
```razor
<RFileUpload Label="Upload Documents"
             Accept=".pdf,.doc,.docx"
             Multiple="true"
             MaxFileSize="10485760"
             ShowPreview="true"
             OnFilesSelected="@HandleFilesSelected"
             OnUploadProgress="@HandleProgress">
    <PreviewTemplate Context="file">
        <div class="file-preview">
            <span>@file.Name</span>
            <span>@file.Size bytes</span>
        </div>
    </PreviewTemplate>
</RFileUpload>
```
- **Properties**: Label, Accept, Multiple, MaxFileSize, ShowPreview, DragAndDrop, OnFilesSelected, OnUploadProgress, PreviewTemplate

### Display Components

#### RDataTable
```razor
<RDataTable TItem="User"
            Items="@users"
            PageSize="10"
            ShowPagination="true"
            ShowSearch="true"
            Striped="true"
            Hoverable="true"
            Loading="@isLoading"
            EmptyMessage="No users found">
    <HeaderContent>
        <RButton Text="Export" Icon="download" IconPosition="IconPosition.Start" />
    </HeaderContent>
    <Columns>
        <RDataTableColumn TItem="User" Property="u => u.Name" Title="Name" Sortable="true" />
        <RDataTableColumn TItem="User" Property="u => u.Email" Title="Email" />
        <RDataTableColumn TItem="User" Property="u => u.Role" Title="Role" Filterable="true" />
        <RDataTableColumn TItem="User" Title="Actions">
            <Template Context="user">
                <RButton Text="Edit" Size="ButtonSize.Small" />
            </Template>
        </RDataTableColumn>
    </Columns>
</RDataTable>
```
- **Properties**: Items, PageSize, ShowPagination, ShowSearch, Striped, Hoverable, Loading, EmptyMessage, OnRowClick
- **Column Properties**: Property, Title, Sortable, Filterable, Width, Template

#### RBadge
```razor
<RBadge Text="New"
        Variant="BadgeVariant.Success"
        Size="BadgeSize.Medium"
        Icon="star" IconPosition="IconPosition.Start"
        Pill="true"
        Clickable="true"
        OnClick="@HandleBadgeClick" />
```
- **Properties**: Text, Variant, Size, StartIcon, EndIcon, Pill, Dot, Clickable, OnClick

#### RAlert (Component version, not CSS class)
```razor
<RAlert Type="AlertType.Warning"
        Title="Important Notice"
        Message="Please review your settings"
        ShowIcon="true"
        Dismissible="true"
        AutoDismiss="5000"
        OnDismiss="@HandleDismiss" />
```
- **Properties**: Type, Title, Message, ShowIcon, Dismissible, AutoDismiss, OnDismiss

#### RProgress
```razor
<RProgress Value="75"
           Max="100"
           Variant="ProgressVariant.Primary"
           Size="ProgressSize.Medium"
           Striped="true"
           Animated="true"
           ShowLabel="true"
           LabelFormat="{0}%" />
```
- **Properties**: Value, Max, Variant, Size, Striped, Animated, ShowLabel, LabelFormat

#### REmptyState
```razor
<REmptyState Title="No Data Found"
             Description="Try adjusting your filters"
             Icon="search_off"
             Variant="EmptyStateVariant.Default">
    <Actions>
        <RButton Text="Clear Filters" Variant="ButtonVariant.Primary" />
        <RButton Text="Learn More" Variant="ButtonVariant.Text" />
    </Actions>
</REmptyState>
```
- **Properties**: Title, Description, Icon, Variant, Actions

#### RSkeleton
```razor
<RSkeleton Variant="SkeletonVariant.Text"
           Width="200px"
           Height="20px"
           Count="3"
           Animation="wave" />
```
- **Properties**: Variant, Width, Height, Count, Animation, Class

#### RSpinner
```razor
<RSpinner Size="SpinnerSize.Medium"
          Variant="SpinnerVariant.Primary"
          Label="Loading..."
          Overlay="false" />
```
- **Properties**: Size, Variant, Label, Overlay

### Content Organization Components

#### RList
```razor
<RList TItem="MenuItem"
       Items="@menuItems"
       Bordered="true"
       Hoverable="true"
       Dense="false">
    <ItemTemplate Context="item">
        <RListItem Icon="@item.Icon" IconPosition="IconPosition.Start"
                   Title="@item.Title"
                   Subtitle="@item.Description"
                   Clickable="true"
                   OnClick="@(() => NavigateTo(item.Url))">
            <EndContent>
                <RBadge Text="@item.BadgeText" />
            </EndContent>
        </RListItem>
    </ItemTemplate>
</RList>
```
- **Properties**: Items, Bordered, Hoverable, Dense, ItemTemplate
- **RListItem Properties**: StartIcon, Title, Subtitle, Clickable, Selected, Disabled, OnClick, StartContent, EndContent

#### RAccordion
```razor
<RAccordion AllowMultiple="false">
    <RAccordionItem Title="General Settings" Icon="settings" DefaultExpanded="true">
        <!-- Accordion content -->
    </RAccordionItem>
    <RAccordionItem Title="Advanced Options" Icon="tune">
        <!-- Accordion content -->
    </RAccordionItem>
</RAccordion>
```
- **Properties**: AllowMultiple, OnItemToggle
- **RAccordionItem Properties**: Title, Icon, DefaultExpanded, Disabled

#### RTimeline
```razor
<RTimeline Orientation="TimelineOrientation.Vertical">
    <RTimelineItem Time="2024-01-15 10:30"
                   Title="Order Placed"
                   Description="Your order has been confirmed"
                   Icon="shopping_cart"
                   Variant="TimelineVariant.Success" />
    <RTimelineItem Time="2024-01-15 14:45"
                   Title="Processing"
                   Icon="pending"
                   Variant="TimelineVariant.Warning" />
</RTimeline>
```
- **Properties**: Orientation, Compact
- **RTimelineItem Properties**: Time, Title, Description, Icon, Variant

### Navigation Components

#### RBreadcrumbs
```razor
<RBreadcrumbs Separator="/">
    <RBreadcrumbItem Text="Home" Href="/" Icon="home" />
    <RBreadcrumbItem Text="Products" Href="/products" />
    <RBreadcrumbItem Text="Electronics" Active="true" />
</RBreadcrumbs>
```
- **Properties**: Separator, MaxItems
- **RBreadcrumbItem Properties**: Text, Href, Icon, Active

#### RPagination
```razor
<RPagination TotalItems="250"
             PageSize="10"
             @bind-CurrentPage="currentPage"
             ShowPageSizeSelector="true"
             PageSizeOptions="@(new[] { 10, 25, 50, 100 })"
             ShowFirstLast="true"
             ShowInfo="true" />
```
- **Properties**: TotalItems, PageSize, CurrentPage, ShowPageSizeSelector, PageSizeOptions, ShowFirstLast, ShowInfo, OnPageChanged

#### RDropdown
```razor
<RDropdown Text="Options"
           Variant="DropdownVariant.Primary"
           Icon="settings" IconPosition="IconPosition.Start">
    <RDropdownItem Text="Edit" Icon="edit" OnClick="@HandleEdit" />
    <RDropdownItem Text="Duplicate" Icon="content_copy" />
    <RDropdownDivider />
    <RDropdownItem Text="Delete" Icon="delete" Variant="DropdownItemVariant.Danger" />
</RDropdown>
```
- **Properties**: Text, Variant, StartIcon, EndIcon, Disabled
- **RDropdownItem Properties**: Text, Icon, Variant, Disabled, OnClick

### Interactive Components

#### RTooltip
```razor
<RTooltip Text="This action cannot be undone"
          Position="TooltipPosition.Top"
          ShowArrow="true"
          Delay="500">
    <ChildContent>
        <RButton Text="Delete" Variant="ButtonVariant.Danger" />
    </ChildContent>
</RTooltip>
```
- **Properties**: Text, Position, ShowArrow, Delay, MaxWidth

#### RPopover
```razor
<RPopover Title="User Info"
          Position="PopoverPosition.Right"
          Trigger="PopoverTrigger.Click"
          ShowArrow="true">
    <TriggerContent>
        <RAvatar Src="@user.Avatar" Size="AvatarSize.Medium" />
    </TriggerContent>
    <BodyContent>
        <div>
            <p>@user.Name</p>
            <p>@user.Email</p>
        </div>
    </BodyContent>
</RPopover>
```
- **Properties**: Title, Position, Trigger, ShowArrow, OnOpen, OnClose

#### RAutocomplete
```razor
<RAutocomplete TItem="Product"
               TValue="string"
               Items="@products"
               @bind-Value="selectedProductId"
               Label="Search Products"
               DisplayProperty="p => p.Name"
               ValueProperty="p => p.Id"
               MinSearchLength="2"
               ShowClearButton="true"
               Loading="@isSearching"
               OnSearch="@SearchProducts" />
```
- **Properties**: Items, Value, Label, DisplayProperty, ValueProperty, MinSearchLength, ShowClearButton, Loading, OnSearch, OnValueChanged

### Media Components

#### RAvatar
```razor
<RAvatar Src="@user.ProfileImage"
         Alt="@user.Name"
         Size="AvatarSize.Large"
         Variant="AvatarVariant.Circle"
         Status="AvatarStatus.Online"
         Initials="@GetInitials(user.Name)"
         ShowStatusBadge="true" />
```
- **Properties**: Src, Alt, Size, Variant, Status, Initials, ShowStatusBadge, OnClick

#### RImage
```razor
<RImage Src="/images/product.jpg"
        Alt="Product Image"
        Width="300"
        Height="200"
        ObjectFit="cover"
        LazyLoad="true"
        Placeholder="/images/placeholder.jpg"
        OnLoad="@HandleImageLoad"
        OnError="@HandleImageError" />
```
- **Properties**: Src, Alt, Width, Height, ObjectFit, LazyLoad, Placeholder, OnLoad, OnError

#### RImageGallery
```razor
<RImageGallery Images="@productImages"
               ShowThumbnails="true"
               ShowNavigation="true"
               AutoPlay="false"
               AutoPlayInterval="3000"
               EnableZoom="true"
               EnableFullscreen="true" />
```
- **Properties**: Images, ShowThumbnails, ShowNavigation, AutoPlay, AutoPlayInterval, EnableZoom, EnableFullscreen

### Chart Components

#### RChart
```razor
<RChart Type="ChartType.Line"
        Data="@chartData"
        Options="@chartOptions"
        Width="100%"
        Height="400px"
        Responsive="true" />
```
- **Properties**: Type, Data, Options, Width, Height, Responsive

#### RSparkline
```razor
<RSparkline Data="@monthlyRevenue"
            Type="SparklineType.Line"
            Width="100px"
            Height="30px"
            ShowTooltip="true"
            Color="var(--color-success)" />
```
- **Properties**: Data, Type, Width, Height, ShowTooltip, Color

### Layout Components

#### RAppShell
```razor
<RAppShell ShowSidebar="true"
           SidebarCollapsed="@isSidebarCollapsed"
           ShowHeader="true"
           ShowFooter="false"
           SidebarPosition="SidebarPosition.Left">
    <Logo>
        <img src="/logo.png" alt="Company Logo" />
    </Logo>
    <HeaderContent>
        <RThemeSwitcher />
        <RUserMenu />
    </HeaderContent>
    <SidebarContent>
        <NavMenu />
    </SidebarContent>
    <MainContent>
        @Body
    </MainContent>
</RAppShell>
```
- **Properties**: ShowSidebar, SidebarCollapsed, ShowHeader, ShowFooter, SidebarPosition, OnSidebarToggle

#### RDashboard
```razor
<RDashboard>
    <RDashboardWidget Title="Revenue" 
                      Value="$125,430" 
                      Change="+12.5%" 
                      Icon="trending_up"
                      Variant="WidgetVariant.Success" />
    <RDashboardWidget Title="Users" 
                      Value="1,234" 
                      Change="+5.2%" />
</RDashboard>
```
- **Properties**: Columns, Gap, Responsive

### Utility Components

#### RThemeSwitcher
```razor
<RThemeSwitcher ShowLabel="true"
                DefaultTheme="ThemeMode.System"
                OnThemeChanged="@HandleThemeChange" />
```
- **Properties**: ShowLabel, DefaultTheme, OnThemeChanged

#### RColorPicker
```razor
<RColorPicker @bind-Value="selectedColor"
              Label="Choose Color"
              ShowAlpha="true"
              ShowPresets="true"
              PresetColors="@presetColors"
              Format="ColorFormat.Hex" />
```
- **Properties**: Value, Label, ShowAlpha, ShowPresets, PresetColors, Format, OnValueChanged

#### RCodeEditor
```razor
<RCodeEditor @bind-Value="sourceCode"
             Language="javascript"
             Theme="vs-dark"
             ShowLineNumbers="true"
             ReadOnly="false"
             Height="400px"
             OnChange="@HandleCodeChange" />
```
- **Properties**: Value, Language, Theme, ShowLineNumbers, ReadOnly, Height, OnChange

### Advanced Components

#### RKanban
```razor
<RKanban TItem="Task"
         Columns="@kanbanColumns"
         Items="@tasks"
         OnItemMoved="@HandleTaskMoved"
         ShowAddButton="true"
         OnAddItem="@HandleAddTask">
    <ItemTemplate Context="task">
        <RCard Title="@task.Title" Class="mb-2">
            <p>@task.Description</p>
            <RBadge Text="@task.Priority" />
        </RCard>
    </ItemTemplate>
</RKanban>
```
- **Properties**: Columns, Items, OnItemMoved, ShowAddButton, OnAddItem, ItemTemplate

#### RTreeView
```razor
<RTreeView TItem="FileNode"
           Items="@fileSystem"
           ChildrenProperty="n => n.Children"
           TextProperty="n => n.Name"
           IconProperty="n => n.Icon"
           ShowCheckboxes="true"
           @bind-SelectedItems="selectedFiles"
           OnItemClick="@HandleFileClick" />
```
- **Properties**: Items, ChildrenProperty, TextProperty, IconProperty, ShowCheckboxes, SelectedItems, OnItemClick

#### RVirtualScroll
```razor
<RVirtualScroll TItem="Product"
                Items="@allProducts"
                ItemHeight="80"
                Height="600px"
                OverscanCount="5">
    <ItemTemplate Context="product">
        <ProductCard Product="@product" />
    </ItemTemplate>
</RVirtualScroll>
```
- **Properties**: Items, ItemHeight, Height, OverscanCount, ItemTemplate

## Component Enums

### Badge Variants
- `Primary`, `Secondary`, `Success`, `Warning`, `Error`, `Info`
- `PriorityCritical`, `PriorityHigh`, `PriorityMedium`, `PriorityLow`

### Button Variants
- `Primary`, `Secondary`, `Success`, `Warning`, `Danger`, `Info`
- `Ghost`, `Outlined`, `Text`, `Gradient`, `Glass`

### Button Sizes
- `Small`, `Medium`, `Large`, `ExtraLarge`

### Card Variants
- `Elevated`, `Flat`, `Outlined`, `Glass`

### Modal Sizes
- `Small`, `Medium`, `Large`, `ExtraLarge`, `FullScreen`

### Modal Variants
- `Default`, `Confirmation`, `Destructive`, `Success`

### Field Types
- `Text`, `Email`, `Password`, `Number`, `Tel`, `Url`
- `Date`, `DateTime`, `Time`, `Month`, `Week`
- `Checkbox`, `Radio`, `Switch`
- `Select`, `MultiSelect`
- `TextArea`, `File`, `Color`, `Range`, `Search`

### Alert Types
- `Info`, `Success`, `Warning`, `Error`

### Progress Variants
- `Primary`, `Secondary`, `Success`, `Warning`, `Error`, `Info`

### Avatar Sizes
- `ExtraSmall`, `Small`, `Medium`, `Large`, `ExtraLarge`

### Avatar Status
- `Online`, `Away`, `Busy`, `Offline`

### Theme Modes
- `Light`, `Dark`, `System`

### Tooltip/Popover Positions
- `Top`, `TopStart`, `TopEnd`
- `Right`, `RightStart`, `RightEnd`
- `Bottom`, `BottomStart`, `BottomEnd`
- `Left`, `LeftStart`, `LeftEnd`

### Chart Types
- `Line`, `Bar`, `Pie`, `Doughnut`, `Area`, `Radar`, `Scatter`

### Skeleton Variants
- `Text`, `Title`, `Avatar`, `Button`, `Card`, `Table`, `Image`

### Empty State Variants
- `Default`, `Search`, `Error`, `NoPermission`, `Maintenance`

## Browser Support
- Chrome 90+
- Firefox 88+
- Safari 14+
- Edge 90+
- Opera 76+

## Performance Specifications
- CSS: ~45KB gzipped
- Components lazy loaded on demand
- Virtual scrolling for large datasets
- Memoization prevents unnecessary re-renders
- CSS containment for optimal rendering

## Accessibility Features
- WCAG 2.1 AA compliant
- Full keyboard navigation support
- Screen reader optimized
- Focus management
- ARIA labels and descriptions
- High contrast mode support
- Reduced motion support
- 44px minimum touch targets

## GitHub Repository

Repository: [github.com/RaRdq/RR.Blazor](https://github.com/RaRdq/RR.Blazor)  
Author: RaRdq (rardqq@gmail.com)

## License
MIT