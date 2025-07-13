using RR.Blazor.CLI.Models;

namespace RR.Blazor.CLI.Services;

public interface ITemplateService
{
    Task<string> GenerateComponentCodeAsync(ComponentMetadata component, Dictionary<string, string?> parameters);
    Task<string> LoadTemplateAsync(string templateName);
}

public class TemplateService : ITemplateService
{
    public async Task<string> GenerateComponentCodeAsync(ComponentMetadata component, Dictionary<string, string?> parameters)
    {
        var code = $"<{component.Name}";
        
        // Add provided parameters
        foreach (var param in parameters)
        {
            if (!string.IsNullOrEmpty(param.Value) && component.Parameters.ContainsKey(param.Key))
            {
                var paramInfo = component.Parameters[param.Key];
                code += $" {param.Key}=\"{FormatParameterValue(param.Value, paramInfo)}\"";
            }
        }

        // Add default parameters for common ones
        if (component.Parameters.ContainsKey("Text") && !parameters.ContainsKey("text"))
        {
            code += " Text=\"Example\"";
        }

        if (component.Parameters.ContainsKey("Variant") && !parameters.ContainsKey("variant"))
        {
            var variantParam = component.Parameters["Variant"];
            if (variantParam.EnumValues.Any())
            {
                var defaultVariant = variantParam.EnumValues.First();
                code += $" Variant=\"{variantParam.Type.Split('.').LastOrDefault()}.{defaultVariant}\"";
            }
        }

        code += " />";
        return code;
    }

    public async Task<string> LoadTemplateAsync(string templateName)
    {
        var templatePath = Path.Combine("Templates", $"{templateName}.template");
        
        if (File.Exists(templatePath))
        {
            return await File.ReadAllTextAsync(templatePath);
        }

        // Return default template if not found
        return $"<{templateName} />";
    }

    private string FormatParameterValue(string value, ParameterMetadata param)
    {
        // Handle enum parameters
        if (param.EnumValues.Any())
        {
            var enumMatch = param.EnumValues.FirstOrDefault(e => 
                string.Equals(e, value, StringComparison.OrdinalIgnoreCase));
            
            if (enumMatch != null)
            {
                return $"{param.Type.Split('.').LastOrDefault()}.{enumMatch}";
            }
        }

        return value;
    }
}

public interface IFileService
{
    Task WriteFileAsync(string path, string content);
    Task<string> ReadFileAsync(string path);
    Task<bool> FileExistsAsync(string path);
}

public class FileService : IFileService
{
    public async Task WriteFileAsync(string path, string content)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(path) ?? "");
        await File.WriteAllTextAsync(path, content);
    }

    public async Task<string> ReadFileAsync(string path)
    {
        return await File.ReadAllTextAsync(path);
    }

    public async Task<bool> FileExistsAsync(string path)
    {
        return File.Exists(path);
    }
}

public interface IValidationService
{
    Task<ValidationResult> ValidateProjectAsync(string path, bool checkPatterns, bool checkAccessibility);
}

public class ValidationService : IValidationService
{
    public async Task<ValidationResult> ValidateProjectAsync(string path, bool checkPatterns, bool checkAccessibility)
    {
        var result = new ValidationResult();
        
        // Scan for .razor files
        var razorFiles = Directory.GetFiles(path, "*.razor", SearchOption.AllDirectories);
        
        foreach (var file in razorFiles)
        {
            var content = await File.ReadAllTextAsync(file);
            
            if (checkPatterns)
            {
                ValidatePatterns(content, file, result);
            }
            
            if (checkAccessibility)
            {
                ValidateAccessibility(content, file, result);
            }
        }
        
        return result;
    }

    private void ValidatePatterns(string content, string filePath, ValidationResult result)
    {
        // Check for legacy icon patterns
        if (content.Contains("StartIcon") || content.Contains("EndIcon"))
        {
            result.Warnings.Add($"{filePath}: Use Icon + IconPosition instead of StartIcon/EndIcon");
        }

        // Check for hardcoded styles
        if (content.Contains("style="))
        {
            result.Warnings.Add($"{filePath}: Use utility classes instead of inline styles");
        }

        // Check for missing elevation on cards
        if (content.Contains("<RCard") && !content.Contains("Elevation") && !content.Contains("elevation-"))
        {
            result.Suggestions.Add($"{filePath}: Consider adding elevation to RCard for better visual hierarchy");
        }
    }

    private void ValidateAccessibility(string content, string filePath, ValidationResult result)
    {
        // Check for missing alt text on images
        if (content.Contains("<img") && !content.Contains("alt="))
        {
            result.Errors.Add($"{filePath}: Images must have alt text for accessibility");
        }

        // Check for button accessibility
        if (content.Contains("<button") && !content.Contains("aria-label") && !content.Contains("Text="))
        {
            result.Warnings.Add($"{filePath}: Buttons should have accessible labels");
        }
    }
}

public class ValidationResult
{
    public List<string> Errors { get; set; } = new();
    public List<string> Warnings { get; set; } = new();
    public List<string> Suggestions { get; set; } = new();
}