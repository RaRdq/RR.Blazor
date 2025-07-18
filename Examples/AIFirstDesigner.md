# AI-First Designer Integration Example

This document demonstrates how to use the `/designer` command with RR.Blazor's AI-first documentation system for creating Fortune 500-grade interfaces.

## Quick Start

The `/designer` command automatically loads `@RR.Blazor/wwwroot/rr-ai-components.json` + `@RR.Blazor/wwwroot/rr-ai-styles.json` to access:
- **65+ Components**: Complete R* component library with AI metadata and structured parameters
- **Modern R* Architecture**: RTextInput, RCheckbox, RRadio, RTextArea with RInputBase inheritance
- **2,100+ Utility Classes**: Comprehensive spacing, typography, layout, and visual effects with bracket notation
- **562 CSS Variables**: Semantic design tokens with pattern documentation and theme support
- **AI Patterns**: Pre-built executive dashboard and form patterns
- **Performance Standards**: Sub-3s load times, 60fps interactions guaranteed
- **Enterprise Consolidation**: Unified design system with mobile-first responsive design
- **Accessibility Compliance**: WCAG 2.1 AA compliant with screen reader optimization

## Example Design Request

### Input
```
/designer
Create an executive-grade business analytics dashboard with real-time metrics, data visualization, and interactive filtering.
```

### Expected AI Process

#### 1. **PLAN Phase**
The AI will automatically:
- Load `rr-ai-components.json` + `rr-ai-styles.json` for complete design system discovery
- Analyze available RR.Blazor components (RCard, RDataTable, RStatsCard, etc.)
- Plan responsive layout using utility patterns
- Design accessibility-compliant interface

#### 2. **IMPLEMENT Phase**  
Generate production-ready code using optimal component selection:
```razor
<!-- Executive Analytics Dashboard -->
<div class="d-flex flex-column gap-6 pa-8">
  <!-- Header Section with Actions -->
  <div class="d-flex justify-between align-center mb-6">
    <h1 class="text-h4 font-semibold ma-0">Business Analytics</h1>
    <div class="d-flex gap-3">
      <RTextInput Type="search" Placeholder="Search..." StartIcon="search" class="w-64" />
      <RButton Text="Export Data" Icon="download" IconPosition="IconPosition.Start" 
               Variant="ButtonVariant.Secondary" Elevation="2" />
    </div>
  </div>

  <!-- Key Metrics Grid -->
  <div class="stats-grid gap-4 mb-6">
    <RCard Title="Active Users" Elevation="4" class="glass-light">
      <div class="pa-6">
        <div class="d-flex justify-between align-center">
          <span class="text-3xl font-bold text-success">1,247</span>
          <RBadge Text="+12%" Variant="BadgeVariant.Success" />
        </div>
        <p class="text-body-2 text--secondary mt-2 mb-0">↗ 8.2% from last month</p>
      </div>
    </RCard>
    
    <RCard Title="Revenue Processing" Elevation="4" class="glass-light">
      <div class="pa-6">
        <div class="d-flex justify-between align-center">
          <span class="text-3xl font-bold text-primary">$2.4M</span>
          <RBadge Text="On Time" Variant="BadgeVariant.Info" />
        </div>
        <p class="text-body-2 text--secondary mt-2 mb-0">Monthly total processed</p>
      </div>
    </RCard>
  </div>
  
  <!-- Data Table Section -->
  <RCard Title="User Directory" Elevation="6" class="glass-medium">
    <div class="pa-6">
      <RDataTable Items="@users" class="elevation-1" 
                  Striped="true" Hoverable="true" />
    </div>
  </RCard>
</div>
```

#### 3. **REFLECT Phase**
Validate implementation against:
- Cross-browser compatibility
- Mobile responsiveness
- WCAG 2.1 AA compliance
- Performance metrics (60fps, <3s load)

## Advanced Usage Patterns

### Complex Dashboard Request
```
/designer
Plan and implement a multi-tenant business dashboard featuring:
- Real-time order processing status
- Interactive customer data filtering
- Mobile-responsive design with touch optimization
- Dark/light theme support
- Accessibility compliance for screen readers
```

### Form Design Request
```
/designer
Create a dense, professional customer onboarding form with:
- Multi-step wizard interface using modern R* components
- Real-time validation feedback with RTextInput, RCheckbox, RRadio
- File upload capabilities with RFileUpload
- Signature capture with touch support
- Progress tracking with RProgressBar
```

### Modern R* Form Example
```razor
<RForm @bind-Model="customer" OnValidSubmit="HandleSubmit">
  <RFormSection Title="Personal Information" Icon="person">
    <RTextInput @bind-value="customer.FirstName" Label="First Name" Required />
    <RTextInput @bind-value="customer.LastName" Label="Last Name" Required />
    <RTextInput @bind-value="customer.Email" Type="email" Label="Email Address" 
                StartIcon="email" Required />
    <RTextInput @bind-value="customer.Phone" Type="tel" Label="Phone Number" 
                StartIcon="phone" />
  </RFormSection>
  
  <RFormSection Title="Preferences" Icon="settings">
    <RCheckbox @bind-Checked="customer.NewsletterOptIn" 
               Text="Subscribe to Newsletter" 
               Description="Receive updates about new features and promotions" />
    <RRadio @bind-value="customer.PreferredContact" Value="email" Text="Email" />
    <RRadio @bind-value="customer.PreferredContact" Value="phone" Text="Phone" />
    <RRadio @bind-value="customer.PreferredContact" Value="sms" Text="SMS" />
  </RFormSection>
</RForm>
```

## Integration with Business Applications

The designer command understands common business application patterns:
- Multi-tenant data visualization
- Business process workflows
- Customer management interfaces
- Real-time SignalR integration
- Role-based access controls

## Best Practices

1. **Always Load Documentation**: Start with `/designer` command
2. **Be Specific**: Detailed requirements yield better results
3. **Iterate**: Use PLAN → IMPLEMENT → REFLECT cycle
4. **Validate**: Test across devices and accessibility tools
5. **Document**: Generate handoff documentation for development team

## Example Output Quality

### Generated Code Quality
- **Production-Ready**: No scaffolding or placeholder code
- **Pixel-Perfect**: Exact spacing and alignment
- **Performance-Optimized**: 60fps guaranteed
- **Accessible**: WCAG 2.1 AA compliant
- **Responsive**: Mobile-first design

### Documentation Output
- Component usage summary
- Utility class application
- Performance metrics achieved
- Accessibility compliance report
- Browser compatibility matrix

---

**Ready to create world-class interfaces**: The AI-first designer system combines human creativity with machine precision to deliver exceptional user experiences.