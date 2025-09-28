# RGrid - Smart Layout Component

## Overview

RGrid is an intelligent layout component that automatically detects the best display mode for your data and creates responsive CSS Grid layouts. Unlike RTable (which uses HTML tables for tabular data), RGrid provides flexible, modern layouts perfect for cards, galleries, lists, and more.

## Key Features

 **Smart Mode Detection** - Automatically chooses the best layout based on your data  
 **Component Delegation** - Routes to existing R components (RCard, RList, RTable)  
 **Responsive Design** - Configurable columns per breakpoint  
 **Filter Integration** - Works seamlessly with RFilter  
 **Custom Templates** - Override auto-generated layouts  
 **Performance Optimized** - CSS Grid with GPU acceleration  

## Quick Start

### Basic Usage
```razor
@* Auto-detects best layout mode *@
<RGrid Items="@employees" />
```

### With Custom Columns
```razor
@* Responsive grid with different column counts *@
<RGrid Items="@products" 
       ColumnsXs="1" 
       ColumnsSm="2" 
       ColumnsMd="3" 
       ColumnsLg="4" />
```

### With Filter
```razor
@* External filter *@
<RFilter @ref="myFilter" />
<RGrid Items="@data" Filter="@myFilter" />

@* Built-in toolbar filter *@
<RGrid Items="@data" EnableFilter="true" />
```

### Custom Template
```razor
<RGrid Items="@products" Mode="GridMode.Cards">
    <ItemTemplate Context="product">
        <RCard Title="@product.Name"
               Subtitle="@product.Category"
               ImageSrc="@product.ImageUrl">
            <FooterContent>
                <RBadge Text="@($"${product.Price}")" />
            </FooterContent>
        </RCard>
    </ItemTemplate>
</RGrid>
```

## Grid Modes

### Auto Mode (Default)
Smart detection based on data properties:
- **Objects with image properties** → Gallery mode
- **Complex objects (5+ properties)** → Cards mode  
- **Simple objects (≤3 properties)** → List mode
- **Explicit columns** → Table mode (edge case)

### Explicit Modes
```razor
<RGrid Items="@data" Mode="GridMode.Cards" />
<RGrid Items="@data" Mode="GridMode.List" />
<RGrid Items="@data" Mode="GridMode.Gallery" />
```

**Available Modes:**
- `Auto` - Smart detection (default)
- `Cards` - Card-based layouts using RCard
- `List` - List layouts using RList  
- `Table` - Table layouts using RTable (edge case)
- `Gallery` - Image galleries (TODO: RGallery)
- `Tiles` - Uniform tiles (TODO: RTile)  
- `Masonry` - Variable heights (TODO: RMasonry)

## Responsive Configuration

### Column Configuration
```razor
<RGrid Items="@data"
       ColumnsXs="1"    @* Mobile: 1 column *@
       ColumnsSm="2"    @* Tablet: 2 columns *@
       ColumnsMd="3"    @* Desktop: 3 columns *@
       ColumnsLg="4"    @* Large: 4 columns *@
       ColumnsXl="6" /> @* XL: 6 columns *@
```

### Mode Overrides per Breakpoint
```razor
<RGrid Items="@data"
       ModeXs="GridMode.List"      @* Mobile: List view *@
       ModeMd="GridMode.Cards"     @* Tablet: Cards *@
       ModeLg="GridMode.Gallery" /> @* Desktop: Gallery *@
```

## Filter Integration

RGrid follows the Filter SOW specifications:

### Case 1: External Filter Control
```razor
<RFilter @ref="gridFilter" 
         OnFilterChanged="@HandleFilterChange" />
<RGrid Items="@employees" Filter="@gridFilter" />
```

### Case 2: Built-in Toolbar Filter
```razor
<RGrid Items="@employees" 
       EnableFilter="true"
       ShowQuickSearch="true" />
```

**Filter Behavior:**
- Always appears in grid toolbar (no column filters)
- Quick search + advanced filter options
- Filters data before delegating to component modes

## Advanced Features

### With Title and Toolbar
```razor
<RGrid Items="@data" 
       Title="Product Catalog"
       Subtitle="Browse our latest products"
       EnableFilter="true">
    <ToolbarActions>
        <RButton Text="Add Product" Variant="ButtonVariant.Primary" />
        <RButton Text="Export" Variant="ButtonVariant.Secondary" />
    </ToolbarActions>
</RGrid>
```

### Performance Options
```razor
<RGrid Items="@largeDataset"
       EnableVirtualization="true"
       VirtualizationThreshold="1000" />
```

### Custom Styling
```razor
<RGrid Items="@data" 
       Class="custom-grid"
       Gap="var(--space-6)"
       Density="DensityType.Compact" />
```

## CSS Customization

### CSS Variables
```scss
.rgrid {
  --rgrid-gap: var(--space-6);           // Custom gap
  --grid-columns-md: 4;                  // Override column count
}
```

### Custom Grid Classes
```scss
.my-custom-grid {
  .rgrid {
    // Custom grid modifications
    grid-template-columns: repeat(auto-fit, minmax(300px, 1fr));
  }
}
```

## Component Delegation

RGrid automatically delegates to appropriate components:

| Mode | Delegates To | Use Case |
|------|-------------|----------|
| Table | `RTable` | Tabular data (edge case) |
| Cards | CSS Grid + `RCard` | Product catalogs, dashboards |
| List | `RList` | Simple data lists |
| Gallery | `RGallery` | Image collections (TODO) |
| Tiles | `RTile` | Uniform content (TODO) |
| Masonry | `RMasonry` | Variable heights (TODO) |

## Performance

### Optimizations
- **CSS Grid Layout** - Native browser optimization
- **Hardware Acceleration** - GPU-accelerated transforms
- **Lazy Loading** - Images load on scroll
- **Virtualization** - Handle 1000+ items efficiently

### Performance Tips
```razor
@* Enable virtualization for large datasets *@
<RGrid Items="@largeList" EnableVirtualization="@(largeList.Count > 1000)" />

@* Use appropriate density for content *@
<RGrid Items="@compactData" Density="DensityType.Compact" />
```

## Accessibility

 **Keyboard Navigation** - Full keyboard support  
 **Screen Readers** - Proper ARIA labels  
 **Focus Management** - Visible focus indicators  
 **Responsive Design** - Mobile-first approach  

## Examples

### Product Catalog
```razor
<RGrid Items="@products" 
       Mode="GridMode.Cards"
       ColumnsMd="3"
       EnableFilter="true"
       Title="Product Catalog">
    <ItemTemplate Context="product">
        <RCard Title="@product.Name"
               Subtitle="@product.Category"
               ImageSrc="@product.ImageUrl"
               Clickable="true"
               OnClick="@(() => ViewProduct(product.Id))">
            <FooterContent>
                <div class="flex justify-between items-center">
                    <RBadge Text="@($"${product.Price}")" 
                            Variant="BadgeVariant.Success" />
                    <RButton Text="Add to Cart" 
                             Size="SizeType.Small" />
                </div>
            </FooterContent>
        </RCard>
    </ItemTemplate>
</RGrid>
```

### Employee Directory
```razor
<RGrid Items="@employees" 
       Mode="GridMode.List"
       EnableFilter="true"
       Title="Team Directory" />
```

### Image Gallery
```razor
<RGrid Items="@photos" 
       Mode="GridMode.Gallery"
       ColumnsLg="4"
       Gap="var(--space-2)" />
```

## Migration from Old RGrid

The old RGrid incorrectly rendered HTML tables. The new RGrid uses CSS Grid and delegates appropriately:

### Before (Incorrect)
```html
<!-- Old RGrid rendered HTML table -->
<table>
  <tr><td>Data</td></tr>
</table>
```

### After (Correct)
```html
<!-- New RGrid uses CSS Grid -->
<div class="rgrid rgrid-cards">
  <div class="card">Data</div>
</div>
```

## Browser Support

-  Modern browsers (Chrome 57+, Firefox 52+, Safari 10.1+)
-  CSS Grid support required
-  Graceful fallback for older browsers

## Related Components

- **RTable** - HTML tables for tabular data
- **RCard** - Individual card components
- **RList** - List layouts
- **RFilter** - Advanced filtering
- **RTile** - Uniform tiles (TODO)
- **RGallery** - Image galleries (TODO)
- **RMasonry** - Variable height layouts (TODO)
