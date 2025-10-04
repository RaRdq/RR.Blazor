# RGrid Smart Component - Statement of Work

## Executive Summary

RGrid is a smart layout component that provides intelligent grid-based display for any data collection. Unlike RTable (HTML tables for tabular data), RGrid uses CSS Grid to create flexible, responsive layouts that automatically adapt to content type and screen size.

## Core Architecture

### Smart Component Pattern
```
RGridBase (abstract base class with shared parameters)
├── RGrid (smart wrapper with type detection and mode selection)
└── Component delegation to existing R components based on detected mode
```

### Component Delegation Strategy

RGrid acts as an intelligent router that delegates to existing R components:

| Mode | Component Used | Purpose |
|------|---------------|---------|
| Table | RTable | Tabular data with HTML `<table>` (edge case) |
| Cards | RCard + CSS Grid | Card-based responsive layouts |
| List | RList | Single/multi-column lists |
| Tiles | RTile | Uniform tile layouts |
| Gallery | RGallery | Image-focused grids |
| Masonry | RMasonry | Pinterest-style variable heights |

## Requirements

### 1. Smart Mode Detection

RGrid automatically detects the best layout mode based on data properties:

```csharp
private GridMode DetectMode(Type itemType)
{
    // Explicit columns → Table mode (use RTable)
    if (Columns?.Any() == true) 
        return GridMode.Table;
    
    // Image properties → Gallery mode
    if (HasProperties(itemType, "Url", "ImageUrl", "Thumbnail", "Src"))
        return GridMode.Gallery;
        
    // Complex objects → Cards mode  
    if (PropertyCount(itemType) > 3)
        return GridMode.Cards;
        
    // Simple objects → List mode
    return GridMode.List;
}
```

### 2. Filter Integration

RGrid integrates with RFilter following the SOW specifications:

```razor
@* Case 1: External filter *@
<RFilter @ref="gridFilter" />
<RGrid Items="@employees" Filter="@gridFilter" />

@* Case 2: Built-in toolbar filter *@
<RGrid Items="@employees" EnableFilter="true" />
```

**Filter Behavior:**
- Always appears in grid toolbar/title bar (no column filters)
- Quick search + filter button combo
- Filters apply to entire grid data before mode delegation

### 3. Responsive Grid System

Follows RR.Blazor responsive principles:

```razor
<RGrid Items="@products"
       ColumnsXs="1"    @* Mobile: 1 column *@
       ColumnsSm="2"    @* Tablet: 2 columns *@
       ColumnsMd="3"    @* Desktop: 3 columns *@
       ColumnsLg="4"    @* Large: 4 columns *@
       ColumnsXl="6" /> @* XL: 6 columns *@
```

**Adaptive Modes:**
```razor
<RGrid Items="@data"
       ModeXs="GridMode.List"      @* Mobile: List view *@
       ModeMd="GridMode.Cards"     @* Tablet: Cards *@
       ModeLg="GridMode.Masonry" /> @* Desktop: Masonry *@
```

### 4. Template System Integration

Uses existing RR.Blazor template system:

```razor
@* Auto-generated templates based on mode *@
<RGrid Items="@employees" Mode="GridMode.Cards" />

@* Custom item template *@
<RGrid Items="@products" Mode="GridMode.Cards">
    <ItemTemplate Context="product">
        <RCard Title="@product.Name"
               Subtitle="@product.Category"
               ImageSrc="@product.ImageUrl">
            <FooterContent>
                <RBadge Text="@($"${product.Price}")" Variant="BadgeVariant.Success" />
            </FooterContent>
        </RCard>
    </ItemTemplate>
</RGrid>
```

**Template Fallback Logic:**
1. Custom `ItemTemplate` if provided
2. Auto-generated template based on detected mode
3. Generic object display as fallback

### 5. Performance Features

- **Virtualization** for large datasets (1000+ items)
- **Lazy loading** with intersection observer
- **Responsive images** with `loading="lazy"`
- **CSS Grid optimization** for smooth animations

## Implemented Features

###  Complete Implementation

The RGrid component has been fully implemented with the following features:

#### Core Parameters
```csharp
public class RGrid : RComponentBase
{
    // Data & Display
    [Parameter] public object Items { get; set; }
    [Parameter] public GridMode Mode { get; set; } = GridMode.Auto;
    [Parameter] public RenderFragment<object> ItemTemplate { get; set; }
    
    // Filter Integration (SOW Compliant)
    [Parameter] public object Filter { get; set; }
    [Parameter] public bool EnableFilter { get; set; }
    [Parameter] public bool ShowQuickSearch { get; set; } = true;
    
    // Responsive Configuration
    [Parameter] public int ColumnsXs { get; set; } = 1;
    [Parameter] public int ColumnsSm { get; set; } = 2;
    [Parameter] public int ColumnsMd { get; set; } = 3;
    [Parameter] public int ColumnsLg { get; set; } = 4;
    [Parameter] public int ColumnsXl { get; set; } = 6;
    
    // Responsive Mode Overrides
    [Parameter] public GridMode? ModeXs { get; set; }
    [Parameter] public GridMode? ModeSm { get; set; }
    [Parameter] public GridMode? ModeMd { get; set; }
    [Parameter] public GridMode? ModeLg { get; set; }
    [Parameter] public GridMode? ModeXl { get; set; }
    
    // Pagination (Uses existing RPaginationFooter)
    [Parameter] public bool EnablePagination { get; set; }
    [Parameter] public int PageSize { get; set; } = 24;
    [Parameter] public int CurrentPage { get; set; }
    [Parameter] public EventCallback<int> OnPageChanged { get; set; }
    
    // Performance Features
    [Parameter] public bool EnableVirtualization { get; set; }
    [Parameter] public int VirtualizationThreshold { get; set; } = 1000;
    [Parameter] public string Gap { get; set; } = "var(--space-4)";
    
    // Content & UI
    [Parameter] public string Title { get; set; }
    [Parameter] public string Subtitle { get; set; }
    [Parameter] public RenderFragment EmptyContent { get; set; }
    [Parameter] public RenderFragment ToolbarActions { get; set; }
}
```

#### Filter + Pagination Integration
The implementation correctly handles the combination of filtering and pagination:

```csharp
// Data Flow: Items → Filter → Pagination → Display
private IEnumerable<object> GetAllItems() { ... }           // Raw data
private IEnumerable<object> GetFilteredItems() { ... }     // After filtering
private IEnumerable<object> GetPagedItems() { ... }        // After pagination
private int GetTotalCount() { ... }                        // Count of filtered items
private int GetTotalPages() { ... }                        // Pages from filtered count
```

**Key Features:**
-  Filter applied BEFORE pagination
-  Pagination count based on filtered results
-  Uses existing RPaginationFooter component
-  Page changes trigger re-rendering
-  Filter changes reset pagination to page 0

### Phase 2: Component Delegation

```razor
@switch (DetectedMode)
{
    case GridMode.Table:
        <RTable Items="@FilteredItems" 
                Filter="@Filter"
                Class="@GetGridClasses()" />
        break;
        
    case GridMode.Cards:
        <div class="@GetGridClasses()">
            @foreach (var item in FilteredItems)
            {
                @if (ItemTemplate != null)
                {
                    @ItemTemplate(item)
                }
                else
                {
                    <RCard Title="@GetTitle(item)"
                           Subtitle="@GetSubtitle(item)"
                           Content="@GetContent(item)" />
                }
            }
        </div>
        break;
        
    case GridMode.List:
        <RList Items="@FilteredItems" 
               Class="@GetGridClasses()" />
        break;
}
```

### Phase 3: CSS Grid Layout

```scss
.rgrid {
  display: grid;
  gap: var(--space-4);
  width: 100%;
  
  // Responsive columns via CSS custom properties
  grid-template-columns: repeat(var(--grid-columns), 1fr);
  
  // Responsive breakpoints
  @include responsive-down(sm) {
    --grid-columns: var(--grid-columns-xs, 1);
  }
  
  @include responsive-up(sm) {
    --grid-columns: var(--grid-columns-sm, 2);
  }
  
  @include responsive-up(md) {
    --grid-columns: var(--grid-columns-md, 3);
  }
  
  @include responsive-up(lg) {
    --grid-columns: var(--grid-columns-lg, 4);
  }
  
  @include responsive-up(xl) {
    --grid-columns: var(--grid-columns-xl, 6);
  }
}
```

## Missing Components to Implement

Since RGrid delegates to existing components, we need:

1. **RTile** - Uniform tile layouts
2. **RGallery** - Image-focused grids with lightbox
3. **RMasonry** - Pinterest-style variable heights

These should be separate components that RGrid can delegate to.

## Success Criteria

1. **Smart Delegation**: Automatically routes to appropriate R component
2. **Filter Integration**: Works seamlessly with RFilter per SOW
3. **Responsive**: Adapts columns and modes per breakpoint
4. **Template System**: Supports custom templates with fallbacks
5. **Performance**: Handles 1000+ items with virtualization
6. **Developer Experience**: Simple API with intelligent defaults

## API Design

### Basic Usage
```razor
<RGrid Items="@data" />
```

### Advanced Usage
```razor
<RGrid Items="@products" 
       Mode="GridMode.Cards"
       ColumnsMd="4"
       EnableFilter="true"
       EnableVirtualization="@(products.Count > 1000)">
    <ItemTemplate Context="product">
        @* Custom template *@
    </ItemTemplate>
</RGrid>
```

### Filter Integration
```razor
<RFilter @ref="filter" />
<RGrid Items="@data" Filter="@filter" />
```

## Non-Goals

- ❌ RGrid does not replace RTable for tabular data
- ❌ No HTML table rendering (use RTable explicitly)
- ❌ No inline editing (use appropriate form components)
- ❌ No server-side pagination (client-side only)

## Implementation Priority

1. Core RGrid with mode detection
2. Component delegation to existing RCard, RList, RTable
3. Filter integration per SOW
4. Responsive breakpoint system
5. Template system integration
6. Missing component implementation (RTile, RGallery, RMasonry)
7. Performance optimizations (virtualization)

This SOW ensures RGrid acts as an intelligent layout router that leverages existing RR.Blazor components while providing a unified, responsive grid experience.
