# RR.Blazor Tests

Comprehensive test suite for RR.Blazor component library covering all aspects of the theme system, components, and services.

## Test Structure

```
Tests/
├── Components/           # Component-specific tests
│   ├── RAppShellThemeTests.cs
│   └── RThemeProviderTests.cs
├── Integration/          # Full integration tests
│   └── ThemeSystemIntegrationTests.cs
├── Security/            # Security and validation tests
│   └── ThemeSecurityTests.cs
├── Services/            # Service layer tests
│   └── BlazorThemeServiceTests.cs
└── README.md
```

## Test Categories

### 🧩 Component Tests
- **RAppShellThemeTests**: Tests RAppShell theme integration and GetEffectiveTheme() functionality
- **RThemeProviderTests**: Tests RThemeProvider component rendering and cascading values

### 🔗 Integration Tests
- **ThemeSystemIntegrationTests**: End-to-end testing of the complete theme system including:
  - Theme variable architecture verification
  - SCSS layer separation testing
  - JavaScript interop integration
  - LocalStorage persistence
  - Component rendering with themes

### 🛡️ Security Tests
- **ThemeSecurityTests**: Comprehensive security validation including:
  - Path traversal attack prevention
  - Input sanitization for theme names
  - File extension validation
  - Theme name length and character restrictions

### ⚡ Service Tests
- **BlazorThemeServiceTests**: Detailed testing of BlazorThemeService including:
  - JavaScript interop error handling
  - System preference detection
  - Theme persistence and loading
  - Event handling and state management

## Running Tests

### Prerequisites
- .NET 9.0 SDK
- All RR.Blazor dependencies resolved

### Run All Tests
```bash
cd RR.Blazor/Tests
dotnet test
```

### Run Specific Test Category
```bash
# Component tests only
dotnet test --filter "namespace~RR.Blazor.Tests.Components"

# Security tests only
dotnet test --filter "namespace~RR.Blazor.Tests.Security"

# Integration tests only
dotnet test --filter "namespace~RR.Blazor.Tests.Integration"
```

### Run Tests with Coverage
```bash
dotnet test --collect:"XPlat Code Coverage"
```

## Test Coverage

### Core Features Covered ✅
- **Theme System Architecture**: 3-layer validation (theme → semantic → utility)
- **Default/Dark Theme Integrity**: Complete variable coverage verification
- **Custom Theme Support**: Registration, validation, and security
- **Component Integration**: RAppShell and RThemeProvider functionality
- **JavaScript Interop**: Error handling and system preference detection
- **Storage Persistence**: LocalStorage save/load with corruption handling
- **Security Validation**: Comprehensive attack prevention testing

### Current Test Metrics
- **85 test cases** covering all major functionality
- **Component Tests**: 15 tests
- **Integration Tests**: 25 tests  
- **Security Tests**: 20 tests
- **Service Tests**: 25 tests

## Test Framework Stack

- **xUnit**: Primary testing framework
- **Bunit**: Blazor component testing
- **FluentAssertions**: Readable assertion syntax
- **Moq**: Mocking framework for dependencies
- **Microsoft.Playwright**: End-to-end testing capabilities

## Contributing to Tests

### Adding New Tests
1. Place tests in appropriate category folder
2. Follow existing naming conventions
3. Use descriptive test method names following pattern: `MethodName_Scenario_ExpectedBehavior`
4. Include comprehensive arrange/act/assert sections
5. Add appropriate test categories and documentation

### Test Guidelines
- **Arrange**: Set up all necessary test data and mocks
- **Act**: Execute the method being tested
- **Assert**: Verify expected outcomes using FluentAssertions
- **Cleanup**: Ensure proper disposal of resources

### Security Test Requirements
- All new theme-related features must include security tests
- Path traversal protection must be verified
- Input validation should be thoroughly tested
- Error handling scenarios must be covered

## Test Data & Mocks

### Common Test Scenarios
- **Valid theme configurations**: Light, dark, system, custom themes
- **Invalid inputs**: Malformed theme names, path traversal attempts
- **Error conditions**: JavaScript failures, storage corruption, network issues
- **Edge cases**: Empty themes, maximum length inputs, concurrent operations

### Mock Strategies
- **IJSRuntime**: Simulate JavaScript interop success/failure scenarios
- **ILocalStorageService**: Test persistence and corruption handling
- **ILogger**: Verify proper error logging and diagnostics
- **ThemeService**: Component integration testing

## Continuous Integration

These tests are designed to run in CI/CD pipelines and include:
- Fast execution times (< 30 seconds total)
- No external dependencies
- Deterministic results
- Comprehensive error reporting
- Cross-platform compatibility

## Troubleshooting

### Common Issues
- **Path-related test failures**: Ensure tests use relative paths and proper path separators
- **JavaScript interop tests**: Verify mock setups match actual JavaScript interfaces  
- **File access tests**: Check that test files exist and are accessible
- **Timing issues**: Use appropriate waiting strategies for async operations

### Debug Mode
Run tests with detailed output:
```bash
dotnet test --logger "console;verbosity=detailed"
```