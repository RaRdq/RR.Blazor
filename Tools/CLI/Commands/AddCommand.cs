using Microsoft.Extensions.Logging;
using RR.Blazor.CLI.Services;

namespace RR.Blazor.CLI.Commands;

public class AddCommand(
    IComponentRegistryService registryService,
    ITemplateService templateService,
    IFileService fileService,
    ILogger logger)
{
    public async Task ExecuteAsync(string component, string? variant, string? text, string? icon, string output)
    {
        try
        {
            logger.LogInformation($"Adding component: {component}");

            // Find component in registry
            var componentInfo = await registryService.FindComponentAsync(component);
            if (componentInfo == null)
            {
                logger.LogError($"Component '{component}' not found. Use 'rr search {component}' to find available components.");
                return;
            }

            // Generate component code
            var code = await templateService.GenerateComponentCodeAsync(componentInfo, new Dictionary<string, string?>
            {
                ["variant"] = variant,
                ["text"] = text,
                ["icon"] = icon
            });

            // Determine output file
            var fileName = $"{component}Example.razor";
            var outputPath = Path.Combine(output, fileName);

            // Write file
            await fileService.WriteFileAsync(outputPath, code);
            
            logger.LogInformation($"✅ Generated {fileName} at {outputPath}");
            logger.LogInformation($"💡 Example usage:\n{code}");

            // Show additional suggestions
            if (componentInfo.AI.Patterns.Any())
            {
                logger.LogInformation("\n🎯 Common patterns:");
                foreach (var pattern in componentInfo.AI.Patterns.Take(3))
                {
                    logger.LogInformation($"  • {pattern.Name}: {pattern.Code}");
                }
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, $"Error adding component: {ex.Message}");
        }
    }
}