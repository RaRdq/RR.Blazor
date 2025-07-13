using Microsoft.Extensions.Logging;
using RR.Blazor.CLI.Services;

namespace RR.Blazor.CLI.Commands;

public class ValidateCommand(IValidationService validationService, ILogger<ValidateCommand> logger)
{
    public async Task ExecuteAsync(string path, bool checkPatterns, bool checkAccessibility)
    {
        try
        {
            logger.LogInformation($"🔍 Validating project at: {path}");

            var result = await validationService.ValidateProjectAsync(path, checkPatterns, checkAccessibility);

            // Report errors
            if (result.Errors.Any())
            {
                logger.LogError($"❌ Found {result.Errors.Count} error(s):");
                foreach (var error in result.Errors)
                {
                    logger.LogError($"  • {error}");
                }
            }

            // Report warnings
            if (result.Warnings.Any())
            {
                logger.LogWarning($"⚠️  Found {result.Warnings.Count} warning(s):");
                foreach (var warning in result.Warnings)
                {
                    logger.LogWarning($"  • {warning}");
                }
            }

            // Report suggestions
            if (result.Suggestions.Any())
            {
                logger.LogInformation($"💡 Found {result.Suggestions.Count} suggestion(s):");
                foreach (var suggestion in result.Suggestions)
                {
                    logger.LogInformation($"  • {suggestion}");
                }
            }

            // Summary
            if (!result.Errors.Any() && !result.Warnings.Any())
            {
                logger.LogInformation("✅ Validation passed! No issues found.");
            }
            else
            {
                logger.LogInformation($"📊 Validation summary: {result.Errors.Count} errors, {result.Warnings.Count} warnings, {result.Suggestions.Count} suggestions");
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, $"Error validating project: {ex.Message}");
        }
    }
}

public class RefreshCommand(IComponentRegistryService registryService, ILogger<RefreshCommand> logger)
{
    public async Task ExecuteAsync(bool scanComponents, string? output)
    {
        try
        {
            logger.LogInformation("🔄 Refreshing component registry...");

            var registry = await registryService.GenerateRegistryAsync(".");
            await registryService.SaveRegistryAsync(registry, output);

            logger.LogInformation($"✅ Registry updated with {registry.Components.Count} components");
            logger.LogInformation($"📊 Registry statistics:");
            logger.LogInformation($"  • Components: {registry.Components.Count}");
            logger.LogInformation($"  • Utility categories: {registry.Utilities.Spacing.Count + registry.Utilities.Layout.Count + registry.Utilities.Effects.Count}");
            logger.LogInformation($"  • Patterns: {registry.Patterns.Count}");

            if (!string.IsNullOrEmpty(output))
            {
                logger.LogInformation($"📁 Registry saved to: {output}");
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, $"Error refreshing registry: {ex.Message}");
        }
    }
}

public class InitCommand(IFileService fileService, ILogger<InitCommand> logger)
{
    public async Task ExecuteAsync(string path, string theme)
    {
        try
        {
            logger.LogInformation($"🚀 Initializing RR.Blazor in: {path}");

            // Create RR.Blazor configuration
            var configContent = $$"""
// RR.Blazor Configuration
builder.Services.AddRRBlazor(options =>
{
    options.Theme.Mode = ThemeMode.{{theme switch 
    {
        "light" => "Light",
        "dark" => "Dark", 
        _ => "System"
    }}};
    options.Theme.PrimaryColor = "#3498DB";
    options.EnableAnimations = true;
    options.EnableAccessibility = true;
});
""";

            var configPath = Path.Combine(path, "RRBlazorConfig.cs");
            await fileService.WriteFileAsync(configPath, configContent);

            // Create example component
            var exampleContent = """
@page "/rr-example"
@using RR.Blazor.Components

<PageTitle>RR.Blazor Example</PageTitle>

<div class="d-flex flex-column gap-6 pa-6">
    <div class="d-flex justify-between align-center">
        <h1 class="text-h4 ma-0">RR.Blazor Components</h1>
        <RThemeSwitcher />
    </div>
    
    <RCard Title="Welcome to RR.Blazor" 
           Subtitle="Enterprise-grade components for modern Blazor apps"
           Elevation="4" 
           class="glass-light">
        <div class="pa-6">
            <div class="d-flex flex-column gap-4">
                <p class="text-body-1">
                    RR.Blazor provides professional components with AI-first development patterns.
                </p>
                
                <div class="d-flex gap-3">
                    <RButton Text="Primary Action" 
                             Variant="ButtonVariant.Primary"
                             Icon="rocket_launch" IconPosition="IconPosition.Start" />
                    <RButton Text="Secondary" 
                             Variant="ButtonVariant.Secondary" />
                    <RButton Icon="settings" 
                             Variant="ButtonVariant.Ghost" 
                             Elevation="2" />
                </div>
            </div>
        </div>
    </RCard>
    
    <div class="stats-grid gap-6">
        <RStatsCard Title="Components" 
                    Value="51" 
                    Change="+5 new"
                    Icon="extension" 
                    Variant="StatsVariant.Success" />
        <RStatsCard Title="Utilities" 
                    Value="800+" 
                    Change="All included"
                    Icon="palette" 
                    Variant="StatsVariant.Info" />
        <RStatsCard Title="Themes" 
                    Value="2" 
                    Change="Light &amp; Dark"
                    Icon="dark_mode" 
                    Variant="StatsVariant.Primary" />
    </div>
</div>

@code {
    // Example component logic
}
""";

            var examplePath = Path.Combine(path, "Pages", "RRExample.razor");
            await fileService.WriteFileAsync(examplePath, exampleContent);

            logger.LogInformation("✅ RR.Blazor initialized successfully!");
            logger.LogInformation($"📁 Created files:");
            logger.LogInformation($"  • {configPath}");
            logger.LogInformation($"  • {examplePath}");
            logger.LogInformation($"🎯 Next steps:");
            logger.LogInformation($"  1. Add the configuration to your Program.cs");
            logger.LogInformation($"  2. Navigate to /rr-example to see components");
            logger.LogInformation($"  3. Run 'rr refresh' to generate component registry");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, $"Error initializing project: {ex.Message}");
        }
    }
}

public class SearchCommand(IComponentRegistryService registryService, ILogger<SearchCommand> logger)
{
    public async Task ExecuteAsync(string query, string? category, bool verbose)
    {
        try
        {
            logger.LogInformation($"🔍 Searching for: '{query}'{(category != null ? $" in {category}" : "")}");

            var components = await registryService.SearchComponentsAsync(query, category);

            if (!components.Any())
            {
                logger.LogWarning("No components found matching your search.");
                return;
            }

            logger.LogInformation($"✅ Found {components.Count} component(s):");

            foreach (var component in components)
            {
                logger.LogInformation($"\n📦 {component.Name}");
                logger.LogInformation($"   {component.Description}");
                
                if (!string.IsNullOrEmpty(component.AI.CommonUse))
                {
                    logger.LogInformation($"   💡 Common use: {component.AI.CommonUse}");
                }

                if (verbose)
                {
                    logger.LogInformation($"   📂 Category: {component.Category}");
                    logger.LogInformation($"   🎯 Complexity: {component.Complexity}");
                    
                    if (component.Parameters.Any())
                    {
                        logger.LogInformation($"   ⚙️  Parameters: {string.Join(", ", component.Parameters.Keys.Take(5))}");
                    }

                    if (component.Examples.Any())
                    {
                        var example = component.Examples.First();
                        logger.LogInformation($"   📝 Example: {example.Code}");
                    }
                }
            }

            logger.LogInformation($"\n💡 To add a component: rr add {components.First().Name.ToLower()}");
            logger.LogInformation($"💡 For more details: rr search {query} --verbose");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, $"Error searching components: {ex.Message}");
        }
    }
}