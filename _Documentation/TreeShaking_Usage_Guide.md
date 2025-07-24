# RR.Blazor Tree-Shaking Usage Guide

## Overview

RR.Blazor now includes advanced CSS tree-shaking and golden ratio optimization capabilities that can reduce your CSS bundle size by 25-50% while maintaining mathematical design precision. This guide shows you how to implement and configure these features.

## Quick Start

### 1. Basic Setup (Zero Configuration)

The easiest way to enable tree-shaking is to use the default configuration:

```csharp
// Program.cs or Startup.cs
public void ConfigureServices(IServiceCollection services)
{
    // Enable RR.Blazor with default tree-shaking
    services.AddRRBlazor();
}
```

### 2. Advanced Configuration

For more control over the optimization process:

```csharp
// Program.cs
services.AddRRBlazor(builder => builder
    .WithTreeShaking()
    .WithGoldenRatio()
    .OutputTo("./wwwroot/css/optimized")
    .WithVerboseLogging()
);
```

### 3. Disable Tree-Shaking (Legacy Mode)

If you need to disable optimization:

```csharp
services.AddRRBlazor(builder => builder
    .DisableTreeShaking()
    .DisableGoldenRatio()
);
```

## Configuration Options

### Tree-Shaking Configuration

```csharp
services.AddRRBlazor(builder => builder
    .WithTreeShaking(options =>
    {
        options.EnableTreeShaking = true;
        options.EnableGoldenRatio = true;
        options.VerboseLogging = false;
        options.EnableInDevelopment = false;
        options.MinimumFileSizeKB = 50;
        options.EnableCaching = true;
        options.TimeoutMs = 60000;
        
        // Custom safelist patterns
        options.SafelistPatterns.AddRange(new[]
        {
            "my-custom-*",
            "legacy-component-*"
        });
        
        // File patterns to scan
        options.ContentPatterns.AddRange(new[]
        {
            "**/*.razor",
            "**/*.cs",
            "**/*.html"
        });
    })
);
```

### Golden Ratio Configuration

```csharp
services.AddRRBlazor(builder => builder
    .WithGoldenRatio(golden =>
    {
        golden.BaseUnit = "1rem";
        golden.Phi = 1.618033988749;
        golden.GenerateSpacingUtilities = true;
        golden.GenerateTypographyUtilities = true;
        golden.GenerateLayoutUtilities = true;
        golden.PowerRange = (-4, 4); // φ-4 through φ4
        golden.EnableOpticalCorrections = true;
    })
);
```

## Build Integration

### MSBuild Properties

Control tree-shaking behavior through MSBuild properties:

```xml
<!-- In your .csproj file -->
<PropertyGroup>
  <!-- Enable/disable tree-shaking -->
  <RRBlazorEnableTreeShaking>true</RRBlazorEnableTreeShaking>
  
  <!-- Enable golden ratio utilities -->
  <RRBlazorEnableGoldenRatio>true</RRBlazorEnableGoldenRatio>
  
  <!-- Run optimization in Debug builds -->
  <RRBlazorOptimizeInDebug>false</RRBlazorOptimizeInDebug>
  
  <!-- Custom output path -->
  <RRBlazorOutputPath>./wwwroot/css/custom</RRBlazorOutputPath>
  
  <!-- Verbose logging -->
  <RRBlazorVerbose>false</RRBlazorVerbose>
  
  <!-- Treat optimization errors as warnings -->
  <RRBlazorTreatOptimizationErrorsAsWarnings>false</RRBlazorTreatOptimizationErrorsAsWarnings>
</PropertyGroup>
```

### Command Line Usage

Run tree-shaking manually from command line:

```bash
# Basic optimization
pwsh ./Scripts/TreeShakeOptimize.ps1 -ProjectPath "./MyBlazorApp"

# Advanced options
pwsh ./Scripts/TreeShakeOptimize.ps1 \
  -ProjectPath "./MyBlazorApp" \
  -OutputPath "./wwwroot/css/optimized" \
  -EnableGoldenRatio \
  -Verbose

# Force optimization (ignore cache)
pwsh ./Scripts/BuildIntegration.ps1 -Force -BuildConfiguration Release
```

## HTML Integration

### Single Import Approach

Include only the optimized CSS in your application:

```html
<!-- In _Host.cshtml or index.html -->
<head>
    <!-- Use optimized CSS bundle (tree-shaken) -->
    <link href="~/css/optimized/rr-blazor.min.css" rel="stylesheet" />
    
    <!-- Fallback to full CSS if optimization fails -->
    <link href="~/css/rr-blazor.css" rel="stylesheet" 
          onerror="this.remove(); document.head.appendChild(Object.assign(document.createElement('link'), {
            rel: 'stylesheet', 
            href: '~/css/rr-blazor.css'
          }));" />
</head>
```

### Development vs Production

```html
<!-- Conditional loading based on environment -->
@if (Environment.IsDevelopment())
{
    <!-- Development: Use full CSS for faster builds -->
    <link href="~/css/rr-blazor.css" rel="stylesheet" />
}
else
{
    <!-- Production: Use optimized CSS -->
    <link href="~/css/optimized/rr-blazor.min.css" rel="stylesheet" />
}
```

## Component Registry

### Custom Component Registration

Add your custom components to the registry for better optimization:

```json
// wwwroot/component-registry.json
{
  "version": "2.0.0",
  "components": {
    "MyCustomButton": {
      "dependencies": {
        "css": ["_custom-buttons.scss"],
        "utilities": ["btn-custom-*", "φ-*"],
        "components": ["RButton"]
      },
      "variants": ["Primary", "Secondary"],
      "treeShaking": {
        "safelist": ["btn-custom-hover", "btn-custom-focus"],
        "purgeable": ["btn-custom-deprecated-*"]
      },
      "goldenRatio": {
        "paddingRatio": "φ-2",
        "heightRatio": "φ0",
        "spacing": ["space-φ-2", "space-φ-1"]
      }
    }
  }
}
```

## Performance Monitoring

### Optimization Reports

Tree-shaking generates detailed reports at `wwwroot/css/optimized/optimization-report.json`:

```json
{
  "timestamp": "2025-01-24T10:30:00Z",
  "originalSize": { "kb": 245.7 },
  "optimizedSize": { "kb": 123.2 },
  "minifiedSize": { "kb": 87.5 },
  "savings": { "percentage": 49.8, "kb": 122.5 },
  "components": { "RButton": 15, "RCard": 8 },
  "utilities": { "btn-primary": 12, "card-elevated": 5 },
  "filesScanned": 47,
  "selectorsPreserved": 1205
}
```

### Performance Metrics Access

```csharp
// In your Blazor component or service
[Inject] public IRRBlazorTreeShakingService TreeShakingService { get; set; }

public async Task ShowOptimizationStats()
{
    var report = await TreeShakingService.GetLatestReportAsync("./");
    
    if (report != null)
    {
        Console.WriteLine($"CSS optimized by {report.Savings.Percentage}%");
        Console.WriteLine($"Bundle size: {report.OptimizedSize.Kb} KB");
        Console.WriteLine($"Components found: {report.Components.Count}");
    }
}
```

## CI/CD Integration

### GitHub Actions

```yaml
# .github/workflows/build.yml
name: Build with RR.Blazor Optimization

on: [push, pull_request]

jobs:
  build:
    runs-on: ubuntu-latest
    
    steps:
    - uses: actions/checkout@v4
    
    - name: Setup .NET
      uses: actions/setup-dotnet@v4
      with:
        dotnet-version: '8.0.x'
        
    - name: Install PowerShell
      shell: bash
      run: |
        wget -q https://github.com/PowerShell/PowerShell/releases/download/v7.4.0/powershell-7.4.0-linux-x64.tar.gz
        mkdir -p pwsh
        tar -xzf powershell-7.4.0-linux-x64.tar.gz -C pwsh
        chmod +x pwsh/pwsh
        echo "${{ github.workspace }}/pwsh" >> $GITHUB_PATH
    
    - name: Restore dependencies
      run: dotnet restore
      
    - name: Build with RR.Blazor optimization
      run: dotnet build --configuration Release
      env:
        RRBlazorEnableTreeShaking: true
        RRBlazorVerbose: true
        
    - name: Upload optimization report
      uses: actions/upload-artifact@v4
      with:
        name: css-optimization-report
        path: wwwroot/css/optimized/optimization-report.json
```

### Azure DevOps

```yaml
# azure-pipelines.yml
trigger:
- main

pool:
  vmImage: 'ubuntu-latest'

variables:
  RRBlazorEnableTreeShaking: true
  RRBlazorVerbose: true

steps:
- task: UseDotNet@2
  inputs:
    version: '8.0.x'

- task: PowerShell@2
  displayName: 'Install PowerShell Core'
  inputs:
    targetType: 'inline'
    script: |
      # PowerShell is pre-installed on Azure DevOps agents
      pwsh --version

- task: DotNetCoreCLI@2
  displayName: 'Build with RR.Blazor Optimization'
  inputs:
    command: 'build'
    configuration: 'Release'

- task: PublishBuildArtifacts@1
  displayName: 'Publish Optimization Report'
  inputs:
    PathtoPublish: 'wwwroot/css/optimized'
    ArtifactName: 'rr-blazor-optimized-css'
```

## Docker Integration

### Dockerfile Example

```dockerfile
# Multi-stage build with RR.Blazor optimization
FROM mcr.microsoft.com/dotnet/sdk:8.0 as build
WORKDIR /src

# Install PowerShell
RUN wget -q https://github.com/PowerShell/PowerShell/releases/download/v7.4.0/powershell-7.4.0-linux-x64.tar.gz \
    && mkdir -p /opt/microsoft/powershell/7 \
    && tar zxf powershell-7.4.0-linux-x64.tar.gz -C /opt/microsoft/powershell/7 \
    && chmod +x /opt/microsoft/powershell/7/pwsh \
    && ln -s /opt/microsoft/powershell/7/pwsh /usr/bin/pwsh

# Copy project files
COPY ["MyApp.csproj", "./"]
RUN dotnet restore

COPY . .

# Build with RR.Blazor optimization
ENV RRBlazorEnableTreeShaking=true
ENV RRBlazorVerbose=false
RUN dotnet build -c Release --no-restore

# Runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /src/bin/Release/net8.0/publish .
COPY --from=build /src/wwwroot/css/optimized ./wwwroot/css/optimized
ENTRYPOINT ["dotnet", "MyApp.dll"]
```

## Troubleshooting

### Common Issues

1. **PowerShell Not Found**
   ```bash
   # Install PowerShell Core on Linux/macOS
   # Ubuntu/Debian
   sudo apt-get install -y powershell
   
   # macOS
   brew install powershell
   
   # Windows (if needed)
   winget install Microsoft.PowerShell
   ```

2. **Optimization Skipped in Development**
   ```csharp
   // Force optimization in development
   services.AddRRBlazor(builder => builder
       .WithTreeShaking(options => options.EnableInDevelopment = true)
   );
   ```

3. **Missing Component Registry**
   ```
   Warning: Component registry not found, tree-shaking may be less effective
   ```
   Copy the provided `component-registry.json` to your `wwwroot` folder.

4. **Optimization Timeout**
   ```csharp
   services.AddRRBlazor(builder => builder
       .WithTreeShaking(options => options.TimeoutMs = 120000) // 2 minutes
   );
   ```

### Debug Mode

Enable verbose logging to diagnose issues:

```csharp
services.AddRRBlazor(builder => builder
    .WithVerboseLogging()
    .WithTreeShaking(options => 
    {
        options.VerboseLogging = true;
        options.EnableInDevelopment = true;
    })
);
```

### Manual Execution

Test tree-shaking manually:

```bash
# Test the script directly
pwsh -ExecutionPolicy Bypass -File "./Scripts/TreeShakeOptimize.ps1" -Verbose

# Check output
ls -la wwwroot/css/optimized/

# View optimization report
cat wwwroot/css/optimized/optimization-report.json | jq '.'
```

## Best Practices

### 1. Use Semantic Component Names
```razor
<!-- Good: Easily detected by tree-shaking -->
<RButton Variant="Primary">Click me</RButton>
<RCard Elevated="true">Content</RCard>

<!-- Avoid: Dynamic component names harder to detect -->
@{
    var componentType = GetDynamicComponentType();
}
<DynamicComponent Type="componentType" />
```

### 2. Consistent Utility Class Usage
```razor
<!-- Good: Clear utility pattern -->
<div class="d-flex items-center gap-4 p-φ-2">
    <RButton class="btn-primary">Action</RButton>
</div>

<!-- Avoid: Dynamic class generation -->
<div class="@($"d-flex gap-{dynamicGap}")">
    <!-- Tree-shaking can't detect dynamic classes -->
</div>
```

### 3. Component Registry Maintenance
- Update `component-registry.json` when adding new components
- Include all component variants in the registry
- Specify accurate CSS dependencies

### 4. Performance Monitoring
- Check optimization reports regularly
- Monitor bundle size changes in CI/CD
- Set up alerts for significant size increases

## Examples

See the complete working examples in:
- [Basic Integration Example](./examples/BasicIntegration/)
- [Advanced Configuration Example](./examples/AdvancedConfiguration/)
- [CI/CD Pipeline Example](./examples/CICD/)
- [Docker Integration Example](./examples/Docker/)

## Support

For issues or questions about RR.Blazor tree-shaking:
1. Check the optimization report for detailed information
2. Enable verbose logging to see detailed output
3. Review the component registry for missing dependencies
4. Test manual script execution to isolate issues