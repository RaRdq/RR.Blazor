# Contributing to RR.Blazor - AI-Optimized Development Guide

## 🤖 AI-First Development Philosophy

RR.Blazor is designed to be **AI-agent friendly**. Whether you're Claude, GPT-4, or a human developer, this guide ensures efficient contribution.

## Core Principles

### 1. **Ultra-Generic Design**
- Components must work across ANY Blazor project
- Zero business logic in components
- Pure UI/UX functionality only
- Theme-aware by default

### 2. **AI-Optimized Code Structure**
- Self-descriptive naming (no comments needed)
- Flat structure with early returns
- Let exceptions bubble up
- Dense, production-ready code

### 3. **Zero Dependencies Philosophy**
- RR.Core is optional (use #if RRCORE_ENABLED)
- No external UI library dependencies
- Pure Blazor components only

## AI Agent Instructions

### For Adding New Components

```markdown
1. Check existing components to avoid duplicates:
   - Search: "R*.razor" in Components/
   - Review RRBlazor.md for component list

2. Follow component template:
   - Place in appropriate subfolder (Form/, Display/, Layout/)
   - Use RComponent naming convention
   - Implement theme-aware styling
   - Add to RRBlazor.md documentation

3. Component must include:
   - Full parameter documentation
   - Keyboard navigation support
   - ARIA attributes for accessibility
   - Loading/disabled states
   - Theme CSS variable usage
```

### For Updating Existing Components

```csharp
// ALWAYS check these before modifying:
1. Run: git grep "ComponentName" -- "*.razor" "*.cs"
2. Check for breaking changes in consuming projects
3. Maintain backward compatibility
4. Update RRBlazor.md if parameters change
```

## Development Patterns

### Component Structure
```razor
@* RExampleComponent.razor *@
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
```markdown
- [ ] Component is generic (no business logic)
- [ ] Uses RR.Blazor naming convention (R prefix)
- [ ] Theme-aware (uses CSS variables)
- [ ] Accessibility compliant (ARIA, keyboard nav)
- [ ] Added to RRBlazor.md with examples
- [ ] No hardcoded colors/spacing
- [ ] Responsive design implemented
- [ ] Loading/error states handled
```

### For Human Developers
1. Fork and clone the repository
2. Create feature branch: `feature/component-name`
3. Follow AI agent checklist above
4. Submit PR with clear description

## Testing Requirements

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

## Documentation Standards

### Component Documentation Template
```markdown
#### RComponentName
\`\`\`razor
<RComponentName Property="value" 
                Variant="ComponentVariant.Primary"
                OnAction="@HandleAction" />
\`\`\`
- **Properties**: List all parameters with types
- **Events**: List all EventCallbacks
- **Variants**: List all enum values
- **Accessibility**: Keyboard shortcuts, ARIA usage
```

## Integration with AI Tools

### Claude Desktop Instructions
```bash
# Add to Claude's context
Provide @RR.Blazor/RRBlazor.md to Claude and ask:
"Update my Blazor project to use RR.Blazor components"

# For new projects
"Initialize a new Blazor project with RR.Blazor design system"
```

### Custom AI Commands
```markdown
/rr-blazor-init - Initialize RR.Blazor in current project
/rr-blazor-upgrade - Upgrade components to latest patterns
/rr-blazor-theme - Configure theme and styling
/rr-blazor-component - Generate new component following patterns
```

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
    .AddClass(CssClass)
    .Build();
```

## Questions or Issues?

- **For AI Agents**: Include full context in your PR description
- **For Humans**: Open an issue with the question template
- **For Urgent**: Tag @RaRdq in the PR/issue

---

**Remember**: RR.Blazor's goal is to make Blazor development faster and more consistent, especially when working with AI coding assistants. Every contribution should enhance this goal.