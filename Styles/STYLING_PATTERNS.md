# RR.Blazor Styling Patterns Guide

## Overview

RR.Blazor follows a **hybrid approach** that combines utility-first CSS with component-specific patterns. This guide explains when to use different styling approaches.

## 1. Pure Utility Classes (Preferred for Static Styling)

Use utility classes for all static styling:

```razor
<div class="flex items-center justify-between p-4 bg-surface-elevated rounded-lg shadow-md">
    <span class="text-lg font-semibold text-primary">Static Content</span>
</div>
```

**Benefits:**
- Consistent spacing and sizing
- Theme-aware colors
- Mobile-first responsive design
- Easy maintenance

## 2. CSS Custom Properties for Dynamic Values (Component Libraries)

Use inline CSS custom properties for values calculated in C#:

```razor
<!-- ✅ CORRECT: Hybrid approach -->
<div class="progress-dynamic bg-surface-secondary rounded-full" 
     style="--progress-width: @(Progress)%">
    <div class="h-2 bg-primary rounded-full transition-all" 
         style="width: var(--progress-width)"></div>
</div>

<!-- ❌ INCORRECT: Pure inline styles -->
<div style="background: #f0f0f0; border-radius: 8px; width: @(Progress)%">
</div>
```

**When to Use:**
- Progress bars with dynamic percentages
- Icon sizes calculated from component props
- Color values computed from theme variables
- Chart dimensions and positions

## 3. Component-Specific CSS Classes

Define component-specific classes in SCSS for complex patterns:

```scss
// _component-name.scss
.my-component {
  @extend %card-base;
  
  &-header {
    // Component-specific styling
  }
  
  &-dynamic-icon {
    font-size: var(--icon-size, 1rem);
    color: var(--icon-color, var(--color-text-primary));
  }
}
```

## 4. CSS Container Queries (Inline Required)

Some modern CSS features must be inline:

```razor
<!-- ✅ CORRECT: Container queries need inline declaration -->
<div class="nav-menu responsive-nav" style="container-type: inline-size;">
    @* Content adapts to container size *@
</div>
```

## 5. Patterns by Component Type

### Progress Indicators
```razor
<div class="button-progress absolute bottom-0 left-0 h-1 bg-primary-light transition-all" 
     style="--progress-width: @(Progress)%"></div>
```

### Dynamic Icons
```razor
<div class="icon-container-dynamic flex items-center justify-center" 
     style="--icon-size: @(size)px; --icon-color: var(--color-@color);">
    <i class="material-symbols-rounded">@Icon</i>
</div>
```

### Chart Elements
```razor
<div class="chart-bar-dynamic" 
     style="--bar-height: @(percentage)%; --bar-color: @color;"></div>
```

### Virtual Lists
```razor
<div class="virtual-item" 
     style="--item-height: @(itemHeight)px; --item-offset: @(offset)px;">
    @* Item content *@
</div>
```

## 6. Validation Guidelines

The CSS validation script will flag these patterns:

### ✅ Acceptable "Violations"
- CSS custom properties: `style="--variable: @(value)"`
- Container queries: `style="container-type: inline-size"`
- Computed colors: `style="--color: var(--color-@theme)"`
- Dynamic transforms: `style="--transform: translateY(@offset)"`

### ❌ Actual Violations to Fix
- Hardcoded colors: `style="color: #ff0000"`
- Static spacing: `style="padding: 16px"`
- Fixed dimensions: `style="width: 300px; height: 200px"`
- Positioning: `style="position: absolute; top: 20px"`

## 7. Migration Strategy

When updating components:

1. **Replace static inline styles** with utility classes
2. **Keep dynamic CSS custom properties** for computed values
3. **Use utility classes** for responsive behavior
4. **Extract complex patterns** to component-specific SCSS

## 8. Best Practices

### DO:
- Use utility classes for 90% of styling
- Use CSS custom properties for dynamic values from C#
- Follow mobile-first responsive patterns
- Leverage RR.Blazor semantic color variables

### DON'T:
- Hardcode colors, spacing, or typography in inline styles
- Duplicate existing utility patterns
- Use inline styles for static positioning or dimensions
- Ignore responsive design considerations

## 9. Available Dynamic Property Utilities

RR.Blazor provides these utility classes for dynamic properties:

- `.progress-dynamic` - Width based on `--progress-width`
- `.icon-dynamic` - Size and color from `--icon-size` and `--icon-color`
- `.value-dynamic` - Color from `--value-color`
- `.chart-bar-dynamic` - Height and color for charts
- `.virtual-item` - Position and size for virtualization
- `.position-dynamic` - Dynamic positioning values

## 10. Example: Complete Progress Bar Component

```razor
@* Static styling with utility classes + dynamic value via CSS custom property *@
<div class="w-full bg-surface-secondary rounded-full overflow-hidden">
    <div class="progress-dynamic h-2 bg-primary transition-all duration-300" 
         style="--progress-width: @(Value)%"
         role="progressbar" 
         aria-valuenow="@Value" 
         aria-valuemin="0" 
         aria-valuemax="100">
    </div>
</div>
```

This approach provides:
- ✅ Utility-first static styling
- ✅ Theme-aware colors and spacing  
- ✅ Responsive design
- ✅ Dynamic values where needed
- ✅ Accessibility compliance
- ✅ Performance optimization