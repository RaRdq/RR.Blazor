# Contributing to RR.Blazor

First of all, thank you for taking your time and considering to contribute 🖤

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
   - Search: "R*.razor" and "R*.cs" in Components/
   - Review wwwroot/rr-ai-components.json for component list

2. Follow component template:
   - Place in appropriate subfolder (Form/, Display/, Layout/)
   - Use RComponent naming convention
   - Implement theme-aware styling
   - AI documentation auto-generated during build

3. Component must include:
   - Full parameter documentation
   - WCAG 2.1 AA accessibility compliance (ARIA attributes, keyboard navigation)
   - Adaptive responsive design (mobile, PC, laptop, iPad portrait/landscape)
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
        ExampleVariant.Primary => "example example-primary",
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
  
  // BEM modifiers - use &-* pattern, NOT --& or &--
  &-primary {
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
- [ ] WCAG 2.1 AA accessibility compliant (ARIA attributes, keyboard navigation)
- [ ] Responsive (true Adaptive) design implemented (mobile, PC, laptop, iPad portrait/landscape)
- [ ] AI metadata added with @** blocks or/and [AIParameter]
- [ ] No hardcoded colors/spacing
- [ ] Loading/error states handled with [RErrorBoundary.razor] and [LoadingStateManager.razor] or custom
- [ ] Smart type detection implemented (if applicable), see [SMART_COMPONENTS_ARCHITECTURE.md]
- [ ] Legacy, obsolete components removed after unified implementation (add !breaking changes to commit)

#### QA Debug Verification (Required for AI Agents)
- [ ] `window.RRDebug.health()` shows 85%+ health score
- [ ] `window.RRDebug.css()` reports zero critical CSS issues
- [ ] `window.RRDebug.responsive()` shows all breakpoints working
- [ ] All responsive breakpoints tested (mobile, PC, laptop, iPad portrait/landscape)
- [ ] `window.RRDebug.report()` generates clean comprehensive report
- [ ] Debug output included in PR/commit description

### For Human Developers

**Option 1: Direct Repository Access** (for core maintainers)
1. Clone the repository directly
2. Create feature branch: `feature/component-name`
3. Follow AI agent checklist above
4. Submit PR with clear description

**Option 2: Fork-Based Development** (recommended for external contributors)
1. Fork the repository to your GitHub account
2. Clone your fork: `git clone https://github.com/YOUR-USERNAME/RR.Blazor.git`
3. Create feature branch: `feature/component-name`
4. Follow AI agent checklist above
5. Push to your fork and submit PR with clear description

## Pull Request Requirements

### PR Policy
- **All new features and fixes must be developed via Pull Requests**
- No direct commits to main/master branch (except for RaRdq and his monkey)
- PRs must pass all automated checks before merge
- All PRs are **squashed on merge** to maintain clean commit history

### PR Template Requirements
When creating a PR, include:

1. **Clear Description**: What does this PR accomplish?
2. **QA Debug Output**: Include `window.RRDebug.health()` and `window.RRDebug.report()` results for UI changes
3. **Testing Evidence**: Screenshots, test results, or debug reports
4. **Breaking Changes**: List any breaking changes and migration steps
5. **Component Impact**: List which components are affected

### Example PR Description
```markdown
## Summary
Adds new RNotificationPanel component with toast notifications

## QA Debug Output
- Page Health Score: 92%
- Component Health: 95%
- WCAG 2.1 AA: ✅ Compliant
- Responsive Design: ✅ All breakpoints tested

## Testing
- [ ] Unit tests passing
- [ ] Manual testing on mobile/desktop
- [ ] Accessibility compliance verified
- [ ] Performance impact assessed

## Breaking Changes
None

## Component Impact
- New: RNotificationPanel
- Updated: RToast (enhanced with panel integration)
```

### Merge Process
1. PR submitted with all requirements met
2. Automated checks must pass (tests, build, QA validation)
3. Code review by maintainer
4. **Squash and merge** - All commits in PR are squashed into single commit
5. Feature branch automatically deleted after merge

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
window.RRDebug.health()         // Health analysis with scoring
window.RRDebug.report()         // Complete AI report
window.RRDebug.css()           // CSS validation
window.RRDebug.performance()   // Performance analysis
window.RRDebug.responsive()    // Responsive design testing
```

#### Core Debug Commands

**1. Health Analysis**
```javascript
// Complete page health analysis with scoring
const health = window.RRDebug.health()
// Returns: { score, issues, summary, elements }
```

**2. Complete Report Generation**
```javascript
// Generate comprehensive AI-friendly report
const report = window.RRDebug.report()
// Returns: { health, css, performance, responsive, logs, timestamp }
```

**3. CSS Validation**
```javascript
// Validate CSS classes and variables
const cssReport = window.RRDebug.css()
// Returns: { validClasses, invalidClasses, missingVariables }
```

**4. Responsive Design Testing**
```javascript
// Test across all breakpoints
const responsive = window.RRDebug.responsive()
// Returns: { mobile, tablet, desktop, issues }
```

#### Common QA Testing Workflow

**For Component Development:**
```javascript
// 1. Check overall health
const health = window.RRDebug.health()
console.log(`Health Score: ${health.score}%`)

// 2. Validate CSS usage
const css = window.RRDebug.css()
console.log('CSS Issues:', css.invalidClasses)

// 3. Test responsive behavior
const responsive = window.RRDebug.responsive()
console.log('Responsive Issues:', responsive.issues)

// 4. Generate final report
const report = window.RRDebug.report()
```

**For Page-Level Testing:**
```javascript
// 1. Complete analysis
const report = window.RRDebug.report()
console.log('Full Report:', report)

// 2. Check specific areas
const health = window.RRDebug.health()
const css = window.RRDebug.css()
const performance = window.RRDebug.performance()
```

#### Issue Detection Patterns

The debug tools automatically detect:
- **Layout Issues**: Zero-size elements, broken flex/grid, invalid positioning
- **Accessibility Issues**: WCAG 2.1 AA violations, missing ARIA attributes, keyboard navigation gaps
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
3. **Check responsive behavior** - Test mobile, PC, laptop, iPad portrait/landscape
4. **Validate accessibility compliance** - Ensure WCAG 2.1 AA standards met
5. **Monitor health scores** - Aim for 85%+ on critical pages, 70%+ minimum
6. **Document issues systematically** - Use debug output for bug reports

#### Example QA Session
```javascript
// Start QA session
console.log('🧪 Starting QA Analysis...')

// 1. Health overview
const health = window.RRDebug.health()
console.log(`Page Score: ${health.score}%`)

// 2. CSS validation
const css = window.RRDebug.css()
console.log('CSS Issues:', css.invalidClasses?.length || 0)

// 3. Responsive testing
const responsive = window.RRDebug.responsive()
console.log('Responsive Issues:', responsive.summary?.totalIssues || 0)

// 4. Generate final report
const report = window.RRDebug.report()
console.log('📋 Complete Report:', report)
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
- **Accessibility**: WCAG 2.1 AA compliance, ARIA usage, keyboard navigation
- **Responsive Design**: Mobile, PC, laptop, iPad portrait/landscape support
```

### AI Agent QA Integration

**For Claude Code and other AI agents performing QA testing:**

1. **Always use debug tools** when analyzing UI components or pages:
```javascript
// Mandatory QA commands for AI agents
window.RRDebug.health()      // Health analysis with scoring
window.RRDebug.report()      // Complete automation report
window.RRDebug.css()         // CSS validation
```

2. **Include debug output in reports** - Copy console output to provide specific issue details
3. **Use health scores for decision making** - Only approve components with 85%+ scores
4. **Focus on compliance violations** - WCAG 2.1 AA accessibility, responsive design failures
5. **Test cross-device compatibility** - Mobile, PC, laptop, iPad portrait/landscape behavior

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
// Use pattern matching for variants - use component-variant pattern
private string GetClasses() => new CssBuilder("component")
    .AddClass($"component-{Variant.ToString().ToLower()}")
    .AddClass("component-disabled", Disabled)
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

## Questions or Issues?

- **For AI Agents**: Include full context in your PR description
- **For Humans**: Open an issue with the question template
- **Urgent**: Tag @RaRdq in the PR/issue

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