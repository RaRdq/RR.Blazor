# Contributing to RR.Blazor

## Development Philosophy

RR.Blazor is designed to be AI-agent friendly. Whether you're Claude, GPT-4, or a human developer, this guide ensures efficient contribution.

## Core Principles

### 1. Ultra-Generic Design
- Components must work across ANY Blazor project
- Zero business logic in components
- Pure UI/UX functionality only
- Theme-aware by default

### 2. AI-Optimized Code Structure
- Self-descriptive naming (no comments needed)
- Flat structure with early returns
- Let exceptions bubble up
- Dense, production-ready code

### 3. Zero Dependencies Philosophy
- RR.Core is optional (use #if RRCORE_ENABLED)
- No external UI library dependencies
- Pure Blazor components only

## AI Agent Instructions

### For Adding New Components

1. Check existing components to avoid duplicates:
   - Search: "R*.razor" in Components/
   - Review wwwroot/rr-ai-components.json for component list

2. Follow component template:
   - Place in appropriate subfolder (Form/, Display/, Layout/)
   - Use RComponent naming convention
   - Implement theme-aware styling
   - AI documentation auto-generated during build

3. Component must include:
   - Full parameter documentation
   - Keyboard navigation support
   - ARIA attributes for accessibility
   - Loading/disabled states
   - Theme CSS variable usage

### For Updating Existing Components

Always check these before modifying:
1. Run: `git grep "ComponentName" -- "*.razor" "*.cs"`
2. Check for breaking changes in consuming projects
3. Maintain backward compatibility
4. Documentation auto-updates during Release builds

## Development Patterns

### Component Structure
```razor
@* RExampleComponent.razor *@
@**
<summary>Example component for demonstration</summary>
<category>Core</category>
<complexity>Simple</complexity>
<ai-prompt>example, demo, template</ai-prompt>
<ai-common-use>UI patterns, component templates</ai-common-use>
**@

@namespace RR.Blazor.Components
@inherits ComponentBase

<div class="@GetClasses()" @attributes="AdditionalAttributes">
    @* Component content *@
</div>

@code {
    // Parameters first
    [Parameter] public string Text { get; set; }
    [Parameter] public ExampleVariant Variant { get; set; } = ExampleVariant.Default;
    [Parameter] public EventCallback OnClick { get; set; }
    [Parameter(CaptureUnmatchedValues = true)] 
    public Dictionary<string, object> AdditionalAttributes { get; set; }
    
    // Private methods last
    private string GetClasses() => Variant switch
    {
        ExampleVariant.Primary => "example example--primary",
        _ => "example"
    };
}
```

### SCSS Structure
```scss
// components/_example.scss
.example {
  // Use CSS variables for theming
  color: var(--color-text-primary);
  background: var(--color-background-elevated);
  
  // BEM modifiers
  &--primary {
    background: var(--color-primary);
    color: var(--color-text-on-primary);
  }
  
  // Responsive design
  @include breakpoint(md) {
    padding: $space-6;
  }
}
```

## Quick Contribution Checklist

### For AI Agents
- [ ] Component is generic (no business logic)
- [ ] Uses RR.Blazor naming convention (R prefix)
- [ ] Theme-aware (uses CSS variables)
- [ ] Accessibility compliant (ARIA, keyboard nav)
- [ ] AI metadata added with @** blocks
- [ ] No hardcoded colors/spacing
- [ ] Responsive design implemented
- [ ] Loading/error states handled
- [ ] Smart type detection implemented (if applicable)
- [ ] Legacy, obsolete components removed after unified implementation

#### QA Debug Verification (Required for AI Agents)
- [ ] `window.RRDebug.analyze()` shows 85%+ health score
- [ ] `window.RRDebug.component('.your-component')` reports zero critical issues
- [ ] `window.RRDebug.scan('interactive-elements')` shows proper accessibility
- [ ] All responsive breakpoints tested with debug tools
- [ ] Debug output included in PR/commit description

### For Human Developers
1. Fork and clone the repository
2. Create feature branch: `feature/component-name`
3. Follow AI agent checklist above
4. Submit PR with clear description

## Testing Requirements

### Unit Testing
```csharp
// Minimum test coverage for new components
[TestClass]
public class RComponentTests
{
    [TestMethod]
    public void Should_Render_With_Default_Parameters() { }
    
    [TestMethod]
    public void Should_Handle_All_Variants() { }
    
    [TestMethod]
    public void Should_Trigger_Events() { }
    
    [TestMethod]
    public void Should_Apply_Custom_CSS_Classes() { }
}
```

### JavaScript Debug Utilities for QA Testing

RR.Blazor includes enterprise-grade debug utilities that automatically load in development environments. These tools are essential for component validation and QA automation.

#### Accessing Debug Tools
The debug utilities are available in development environments (localhost, dev ports, debug URLs):
```javascript
// Tools are available on the global window object
window.RRDebug.analyze()        // Full page analysis
window.RRDebug.component()      // Component-specific analysis  
window.RRDebug.scan()          // Element scanning
window.RRDebug.checkComponent() // Quick health checks
```

#### Core Debug Commands

**1. Full Page Analysis**
```javascript
// Complete page health analysis with scoring
window.RRDebug.analyze()
// Output: Health score, element statistics, issue recommendations

// Quick page overview
window.RRDebug.analyze({depth: 'summary'})
```

**2. Component-Specific Analysis**
```javascript
// Analyze specific component and all children
window.RRDebug.component('.modal')
window.RRDebug.component('#sidebar')
window.RRDebug.component('.rr-card')

// Quick component health check
window.RRDebug.checkComponent('.nav-menu')
// Output: Health score, issue count, element count
```

**3. Element Scanning**
```javascript
// Scan all buttons on page
window.RRDebug.scan('button')

// Scan first 5 cards with detailed analysis
window.RRDebug.scan('.card', {limit: 5, detail: 'full'})

// Quick scan of form inputs
window.RRDebug.scan('input, textarea, select', {limit: 10})
```

**4. QA Automation Reports**
```javascript
// Generate automation-friendly report
const report = window.RRDebug.getQAReport()
// Returns: {url, timestamp, score, status, issueCount, recommendations}

// Check if page meets quality standards
window.RRDebug.isHealthy() // returns true/false (70%+ score)
```

#### Common QA Testing Workflow

**For Component Development:**
```javascript
// 1. Test component rendering
window.RRDebug.component('.your-component')

// 2. Check accessibility compliance
window.RRDebug.scan('[role="button"]', {detail: 'full'})

// 3. Validate responsive behavior
window.RRDebug.analyze({scope: 'component', target: '.responsive-element'})

// 4. Generate final report
window.RRDebug.getQAReport()
```

**For Page-Level Testing:**
```javascript
// 1. Overall page health
window.RRDebug.analyze()

// 2. Check critical components
window.RRDebug.checkComponent('.header')
window.RRDebug.checkComponent('.sidebar') 
window.RRDebug.checkComponent('.main-content')

// 3. Scan interactive elements
window.RRDebug.scan('button, [role="button"], .btn', {limit: 20})
```

#### Issue Detection Patterns

The debug tools automatically detect:
- **Layout Issues**: Zero-size elements, broken flex/grid, invalid positioning
- **Accessibility Issues**: Missing alt text, poor contrast, missing focus indicators
- **CSS Issues**: Invalid class names, missing variables, forced height corruption
- **Performance Issues**: Excessive inline styles, DOM complexity
- **Responsive Issues**: Broken responsive utilities, display conflicts

#### Auto-Testing Integration

Add `?debug=true` or `?qa=true` to any URL for automatic analysis:
```
https://localhost:5001/dashboard?debug=true
```
This will auto-run page analysis and log results to console.

#### Best Practices for QA Testing

1. **Always test in development environment** - Debug tools only load in dev mode
2. **Test component isolation** - Use `component()` method for focused testing
3. **Check responsive behavior** - Test at different viewport sizes
4. **Validate accessibility** - Pay attention to ARIA warnings and focus indicators
5. **Monitor health scores** - Aim for 85%+ on critical pages, 70%+ minimum
6. **Document issues systematically** - Use debug output for bug reports

#### Example QA Session
```javascript
// Start QA session
console.log('🧪 Starting QA Analysis...')

// 1. Page overview
const pageHealth = window.RRDebug.analyze()
console.log(`Page Score: ${pageHealth.score}%`)

// 2. Check key components  
const headerHealth = window.RRDebug.checkComponent('.app-header')
const sidebarHealth = window.RRDebug.checkComponent('.app-sidebar')

// 3. Scan problematic elements
if (pageHealth.score < 85) {
    window.RRDebug.scan('*', {limit: 20}) // Find problem elements
}

// 4. Generate final report
const qaReport = window.RRDebug.getQAReport()
console.log('📋 QA Report Generated:', qaReport)
```

## Documentation Standards

### Component Documentation Template
```markdown
#### RComponentName
```razor
<RComponentName Property="value" 
                Variant="ComponentVariant.Primary"
                OnAction="@HandleAction" />
```
- **Properties**: List all parameters with types
- **Events**: List all EventCallbacks
- **Variants**: List all enum values
- **Accessibility**: Keyboard shortcuts, ARIA usage
```

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

### AI Agent QA Integration

**For Claude Code and other AI agents performing QA testing:**

1. **Always use debug tools** when analyzing UI components or pages:
```javascript
// Mandatory QA commands for AI agents
window.RRDebug.analyze()                    // Page health overview
window.RRDebug.component('.target-component') // Component analysis
window.RRDebug.getQAReport()               // Automation report
```

2. **Include debug output in reports** - Copy console output to provide specific issue details
3. **Use health scores for decision making** - Only approve components with 85%+ scores
4. **Focus on pattern violations** - Invalid CSS classes, broken responsive utilities, accessibility issues
5. **Test responsive behavior** - Verify mobile-first design and breakpoint behavior

## Common Patterns to Follow

### 1. Parameter Validation
```csharp
// DON'T do defensive programming
if (Items == null) Items = new List<T>(); // ❌

// DO initialize in declaration
[Parameter] public List<T> Items { get; set; } = new(); // ✅
```

### 2. Event Handling
```csharp
// Consistent async pattern
[Parameter] public EventCallback<T> OnValueChanged { get; set; }

private async Task HandleChange(T value)
{
    if (OnValueChanged.HasDelegate)
        await OnValueChanged.InvokeAsync(value);
}
```

### 3. CSS Class Building
```csharp
// Use pattern matching for variants
private string GetClasses() => new CssBuilder("component")
    .AddClass($"component--{Variant.ToString().ToLower()}")
    .AddClass("component--disabled", Disabled)
    .AddClass(Class)
    .Build();
```

## Documentation Generation

### AI-First Documentation System
RR.Blazor automatically generates comprehensive AI documentation during Release builds:

```bash
# Manual generation
pwsh ./RR.Blazor/Scripts/GenerateDocumentation.ps1 -ProjectPath ./RR.Blazor

# Automatic generation (Release builds)
dotnet build -c Release
```

**Generated documentation includes**:
- 62 components with complete APIs including unified smart components
- 2,953+ utility patterns with AI hints
- 33 CSS variable pattern categories
- Real-world usage patterns
- Best practices and accessibility guidelines

## Questions or Issues?

- **For AI Agents**: Include full context in your PR description
- **For Humans**: Open an issue with the question template
- **For Urgent**: Tag @RaRdq in the PR/issue

## Building from Source

```bash
# Clone repository
git clone https://github.com/RaRdq/RR.Blazor.git
cd RR.Blazor

# Generate AI documentation
pwsh ./Scripts/GenerateDocumentation.ps1 -ProjectPath .

# Build project with tools
dotnet build

# Build with documentation generation (Release only)
dotnet build -c Release

# Use development tools
dotnet run --project Tools/CLI -- validate --check-patterns
dotnet run --project Tools/CLI -- generate component --name RNewComponent
```

---

**Remember**: RR.Blazor's goal is to make Blazor development faster and more consistent, especially when working with AI coding assistants. Every contribution should enhance this goal.