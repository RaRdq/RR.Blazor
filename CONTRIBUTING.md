# Contributing to RR.Blazor

Thank you for your interest in contributing to RR.Blazor! This document provides guidelines and instructions for contributing.

## Code of Conduct

By participating in this project, you agree to maintain a respectful and inclusive environment for all contributors.

## How to Contribute

### Reporting Issues

1. Check if the issue already exists
2. Create a new issue with a clear title and description
3. Include steps to reproduce if applicable
4. Add relevant labels

### Submitting Pull Requests

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Make your changes following our coding standards
4. Write or update tests as needed
5. Update documentation
6. Commit your changes (`git commit -m 'Add amazing feature'`)
7. Push to your branch (`git push origin feature/amazing-feature`)
8. Open a Pull Request

## Development Guidelines

### Component Development

- Keep components generic and reusable
- Follow the existing component patterns
- Include proper TypeScript/C# types
- Add XML documentation comments
- Test across different themes and screen sizes

### CSS/SCSS Guidelines

- Use semantic CSS variables
- Follow BEM naming for component classes
- Maintain theme compatibility
- Test in light/dark/high-contrast modes
- Keep specificity low

### Testing

- Write unit tests for new components
- Test accessibility with screen readers
- Verify responsive behavior
- Check browser compatibility

## Code Style

### C# Conventions

```csharp
// Use PascalCase for public members
public string PropertyName { get; set; }

// Use camelCase for private fields
private string fieldName;

// Always use 'var' for local variables
var result = GetResult();

// Use string interpolation
var message = $"Hello {name}";
```

### Component Structure

```razor
@* Component parameters first *@
[Parameter] public string Title { get; set; }
[Parameter] public EventCallback<string> OnClick { get; set; }

@* Private fields *@
private bool isLoading;

@* Lifecycle methods in order *@
protected override async Task OnInitializedAsync() { }
protected override async Task OnParametersSetAsync() { }

@* Public methods *@
public void Show() { }

@* Private methods *@
private async Task HandleClick() { }
```

## Documentation

- Update RRBlazor.md for new components
- Include usage examples
- Document all public APIs
- Add to the appropriate section

## Questions?

Feel free to open an issue for any questions about contributing.