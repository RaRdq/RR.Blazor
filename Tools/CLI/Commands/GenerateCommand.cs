using Microsoft.Extensions.Logging;
using RR.Blazor.CLI.Services;

namespace RR.Blazor.CLI.Commands;

public class GenerateCommand(
    IAICodeGenService aiService,
    IComponentRegistryService registryService,
    IFileService fileService,
    ILogger<GenerateCommand> logger)
{
    public async Task ExecuteAsync(string prompt, string output, string type)
    {
        try
        {
            logger.LogInformation($"🤖 Generating {type} from prompt: '{prompt}'");

            // Load component registry for AI context
            var registry = await registryService.LoadRegistryAsync();

            // Generate code using AI
            var result = await aiService.GenerateFromPromptAsync(prompt, type, registry);

            if (result == null)
            {
                logger.LogError("Failed to generate code from prompt");
                return;
            }

            // Determine output file name
            var fileName = type switch
            {
                "form" => $"{SanitizeFileName(prompt)}Form.razor",
                "layout" => $"{SanitizeFileName(prompt)}Layout.razor", 
                "dashboard" => $"{SanitizeFileName(prompt)}Dashboard.razor",
                _ => $"{SanitizeFileName(prompt)}Component.razor"
            };

            var outputPath = Path.Combine(output, fileName);

            // Write generated code
            await fileService.WriteFileAsync(outputPath, result.Code);

            logger.LogInformation($"✅ Generated {fileName} at {outputPath}");
            logger.LogInformation($"\n📝 Generated code:\n{result.Code}");

            // Show AI analysis
            if (!string.IsNullOrEmpty(result.Analysis))
            {
                logger.LogInformation($"\n🧠 AI Analysis:\n{result.Analysis}");
            }

            // Show component suggestions
            if (result.SuggestedComponents.Any())
            {
                logger.LogInformation("\n💡 Components used:");
                foreach (var component in result.SuggestedComponents)
                {
                    logger.LogInformation($"  • {component}");
                }
            }

            // Show utility suggestions
            if (result.SuggestedUtilities.Any())
            {
                logger.LogInformation("\n🎨 Utility classes used:");
                foreach (var utility in result.SuggestedUtilities)
                {
                    logger.LogInformation($"  • {utility}");
                }
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, $"Error generating component: {ex.Message}");
        }
    }

    private string SanitizeFileName(string input)
    {
        var sanitized = string.Concat(input.Split(' ').Select(word => 
            char.ToUpper(word[0]) + word.Substring(1).ToLower()));
        
        return new string(sanitized.Where(c => char.IsLetterOrDigit(c)).ToArray());
    }
}