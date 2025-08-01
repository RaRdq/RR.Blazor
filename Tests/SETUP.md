# RR.Blazor Test Setup Guide

Quick setup instructions for running the RR.Blazor test suite.

## Prerequisites

- .NET 9.0 SDK or later
- Visual Studio 2022 or VS Code with C# extension
- Git (for repository management)

## Initial Setup

### 1. Restore NuGet Packages
```bash
cd RR.Blazor/Tests
dotnet restore
```

### 2. Build the Test Project
```bash
dotnet build
```

### 3. Run All Tests
```bash
dotnet test
```

## Verification

After running the setup, you should see output similar to:
```
Test run for RR.Blazor.Tests.dll (.NET 9.0)
Microsoft (R) Test Execution Command Line Tool Version 17.8.0

Starting test execution, please wait...
A total of 85 test files matched the specified pattern.

Passed!  - Failed:     0, Passed:    85, Skipped:     0, Total:    85, Duration: 2s
```

## IDE Integration

### Visual Studio 2022
1. Open the solution file
2. Build → Rebuild Solution
3. Test → Run All Tests

### VS Code
1. Install C# extension
2. Install .NET Core Test Explorer extension
3. Press `Ctrl+Shift+P` → "Test: Run All Tests"

## Running Specific Test Categories

```bash
# Component tests only
dotnet test --filter "namespace~Components"

# Security tests only  
dotnet test --filter "namespace~Security"

# Integration tests only
dotnet test --filter "namespace~Integration"

# Service tests only
dotnet test --filter "namespace~Services"
```

## Continuous Integration

The tests are designed to run in CI/CD environments:

### GitHub Actions Example
```yaml
- name: Test RR.Blazor
  run: |
    cd RR.Blazor/Tests
    dotnet test --logger trx --results-directory TestResults
```

### Azure DevOps Example
```yaml
- task: DotNetCoreCLI@2
  displayName: 'Run RR.Blazor Tests'
  inputs:
    command: 'test'
    projects: 'RR.Blazor/Tests/RR.Blazor.Tests.csproj'
    arguments: '--configuration Release --logger trx --results-directory $(Agent.TempDirectory)'
```

## Troubleshooting

### Common Issues

**Issue**: `Could not find project reference`
**Solution**: Ensure you're running from the correct directory and the main RR.Blazor project builds successfully.

**Issue**: `Package restore failed`
**Solution**: Clear NuGet cache and restore:
```bash
dotnet nuget locals all --clear
dotnet restore
```

**Issue**: Tests fail with file access errors
**Solution**: Ensure the test runner has appropriate file system permissions.

## Development Workflow

1. **Before committing**: Always run the full test suite
2. **Adding new features**: Write tests first (TDD approach)
3. **Bug fixes**: Add regression tests to prevent reoccurrence
4. **Performance changes**: Verify tests still run within acceptable time limits

## Test Coverage

To generate test coverage reports:

```bash
dotnet test --collect:"XPlat Code Coverage"
```

Coverage reports will be generated in the `TestResults` directory.

## Support

For issues with the test setup:
1. Check the main RR.Blazor README for dependencies
2. Verify all NuGet packages are restored correctly
3. Ensure .NET 9.0 SDK is properly installed
4. Check that the main RR.Blazor project builds without errors