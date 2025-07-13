using Newtonsoft.Json;
using RR.Blazor.CLI.Models;
using System.Text.RegularExpressions;

namespace RR.Blazor.CLI.Services;

public interface IComponentRegistryService
{
    Task<ComponentRegistry> LoadRegistryAsync(string? path = null);
    Task SaveRegistryAsync(ComponentRegistry registry, string? path = null);
    Task<ComponentMetadata?> FindComponentAsync(string name);
    Task<List<ComponentMetadata>> SearchComponentsAsync(string query, string? category = null);
    Task<ComponentRegistry> GenerateRegistryAsync(string projectPath);
}

public class ComponentRegistryService : IComponentRegistryService
{
    private readonly string _defaultRegistryPath = "rr-components.json";
    private ComponentRegistry? _cachedRegistry;

    public async Task<ComponentRegistry> LoadRegistryAsync(string? path = null)
    {
        var registryPath = path ?? _defaultRegistryPath;
        
        if (_cachedRegistry != null && path == null)
            return _cachedRegistry;

        if (!File.Exists(registryPath))
        {
            // Try to find registry in common locations
            var searchPaths = new[]
            {
                "wwwroot/rr-components.json",
                "../wwwroot/rr-components.json",
                "RR.Blazor/wwwroot/rr-components.json"
            };

            foreach (var searchPath in searchPaths)
            {
                if (File.Exists(searchPath))
                {
                    registryPath = searchPath;
                    break;
                }
            }
        }

        if (!File.Exists(registryPath))
        {
            return CreateDefaultRegistry();
        }

        var json = await File.ReadAllTextAsync(registryPath);
        var registry = JsonConvert.DeserializeObject<ComponentRegistry>(json) ?? CreateDefaultRegistry();
        
        if (path == null)
            _cachedRegistry = registry;

        return registry;
    }

    public async Task SaveRegistryAsync(ComponentRegistry registry, string? path = null)
    {
        var registryPath = path ?? _defaultRegistryPath;
        var json = JsonConvert.SerializeObject(registry, Formatting.Indented);
        
        Directory.CreateDirectory(Path.GetDirectoryName(registryPath) ?? "");
        await File.WriteAllTextAsync(registryPath, json);
        
        _cachedRegistry = registry;
    }

    public async Task<ComponentMetadata?> FindComponentAsync(string name)
    {
        var registry = await LoadRegistryAsync();
        
        // Try exact match first
        if (registry.Components.TryGetValue(name, out var component))
            return component;

        // Try case-insensitive match
        var match = registry.Components.FirstOrDefault(kvp => 
            string.Equals(kvp.Key, name, StringComparison.OrdinalIgnoreCase));
        
        if (match.Key != null)
            return match.Value;

        // Try partial match
        match = registry.Components.FirstOrDefault(kvp => 
            kvp.Key.Contains(name, StringComparison.OrdinalIgnoreCase));
        
        return match.Key != null ? match.Value : null;
    }

    public async Task<List<ComponentMetadata>> SearchComponentsAsync(string query, string? category = null)
    {
        var registry = await LoadRegistryAsync();
        var components = registry.Components.Values.AsEnumerable();

        if (!string.IsNullOrEmpty(category))
        {
            components = components.Where(c => 
                string.Equals(c.Category, category, StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrEmpty(query))
        {
            components = components.Where(c =>
                c.Name.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                c.Description.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                c.AI.CommonUse.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                c.AI.Tags.Any(tag => tag.Contains(query, StringComparison.OrdinalIgnoreCase)));
        }

        return components.OrderBy(c => c.Name).ToList();
    }

    public async Task<ComponentRegistry> GenerateRegistryAsync(string projectPath)
    {
        var registry = CreateDefaultRegistry();
        
        // Scan for .razor files in the project
        var razorFiles = Directory.GetFiles(projectPath, "*.razor", SearchOption.AllDirectories)
            .Where(f => Path.GetFileName(f).StartsWith("R"))
            .ToList();

        foreach (var file in razorFiles)
        {
            var component = await AnalyzeRazorFileAsync(file);
            if (component != null)
            {
                registry.Components[component.Name] = component;
            }
        }

        // Scan for .cs component files
        var csFiles = Directory.GetFiles(projectPath, "*.cs", SearchOption.AllDirectories)
            .Where(f => Path.GetFileName(f).StartsWith("R") && !f.Contains("bin") && !f.Contains("obj"))
            .ToList();

        foreach (var file in csFiles)
        {
            var component = await AnalyzeCSharpFileAsync(file);
            if (component != null)
            {
                registry.Components[component.Name] = component;
            }
        }

        registry.Info.ComponentCount = registry.Components.Count;
        registry.Generated = DateTime.UtcNow;

        return registry;
    }

    private ComponentRegistry CreateDefaultRegistry()
    {
        return new ComponentRegistry
        {
            Version = "1.0.0",
            Generated = DateTime.UtcNow,
            Info = new RegistryInfo(),
            Components = new Dictionary<string, ComponentMetadata>(),
            Utilities = CreateDefaultUtilities(),
            Patterns = CreateDefaultPatterns()
        };
    }

    private UtilityRegistry CreateDefaultUtilities()
    {
        return new UtilityRegistry
        {
            Spacing = new List<UtilityCategory>
            {
                new UtilityCategory
                {
                    Name = "Padding",
                    Description = "Padding utilities for all sides",
                    Classes = Enumerable.Range(0, 25).Select(i => $"pa-{i}").ToList(),
                    AIHint = "Use pa-{number} for padding all sides, px-{number} for horizontal, py-{number} for vertical"
                },
                new UtilityCategory
                {
                    Name = "Margin",
                    Description = "Margin utilities for all sides",
                    Classes = Enumerable.Range(0, 25).Select(i => $"ma-{i}").ToList(),
                    AIHint = "Use ma-{number} for margin all sides, mx-auto for centering"
                }
            },
            Layout = new List<UtilityCategory>
            {
                new UtilityCategory
                {
                    Name = "Flexbox",
                    Description = "Flexbox layout utilities",
                    Classes = new List<string> { "d-flex", "flex-column", "justify-between", "align-center", "gap-4" },
                    AIHint = "Use d-flex with justify-between and align-center for header layouts"
                }
            },
            Effects = new List<UtilityCategory>
            {
                new UtilityCategory
                {
                    Name = "Elevation",
                    Description = "Material Design elevation system",
                    Classes = Enumerable.Range(0, 25).Select(i => $"elevation-{i}").ToList(),
                    AIHint = "Use elevation-4 for cards, elevation-8 for modals, elevation-16 for floating elements"
                },
                new UtilityCategory
                {
                    Name = "Glassmorphism",
                    Description = "Glass effect utilities",
                    Classes = new List<string> { "glass", "glass-light", "glass-medium", "glass-heavy", "glass-frost" },
                    AIHint = "Use glass-light for subtle effects, glass-heavy for prominent glass elements"
                }
            }
        };
    }

    private List<PatternLibrary> CreateDefaultPatterns()
    {
        return new List<PatternLibrary>
        {
            new PatternLibrary
            {
                Name = "Professional Card",
                Description = "Enterprise-grade card with elevation and glass effects",
                Category = "Layout",
                Code = "<div class=\"elevation-4 glass-light pa-6 rounded-lg\">\n    <div class=\"d-flex justify-between align-center mb-4\">\n        <h3 class=\"text-h5 ma-0\">Card Title</h3>\n        <button class=\"btn-ghost btn-sm\">Action</button>\n    </div>\n    <!-- Card content -->\n</div>",
                AIPrompt = "professional card layout",
                Components = new List<string> { "RCard" },
                Utilities = new List<string> { "elevation-4", "glass-light", "pa-6", "d-flex", "justify-between" }
            }
        };
    }

    private async Task<ComponentMetadata?> AnalyzeRazorFileAsync(string filePath)
    {
        try
        {
            var content = await File.ReadAllTextAsync(filePath);
            var fileName = Path.GetFileNameWithoutExtension(filePath);

            if (!fileName.StartsWith("R"))
                return null;

            var component = new ComponentMetadata
            {
                Name = fileName,
                FullName = fileName,
                FilePath = filePath,
                LastModified = File.GetLastWriteTime(filePath),
                Category = "Uncategorized",
                Complexity = "Simple"
            };

            // Extract XML documentation comments
            var xmlCommentMatch = Regex.Match(content, @"@\*\*(.*?)\*@", RegexOptions.Singleline);
            if (xmlCommentMatch.Success)
            {
                ParseXmlDocumentation(xmlCommentMatch.Groups[1].Value, component);
            }

            // Extract parameters from @code block
            var codeBlockMatch = Regex.Match(content, @"@code\s*{(.*?)}", RegexOptions.Singleline);
            if (codeBlockMatch.Success)
            {
                ExtractParameters(codeBlockMatch.Groups[1].Value, component);
            }

            return component;
        }
        catch
        {
            return null;
        }
    }

    private async Task<ComponentMetadata?> AnalyzeCSharpFileAsync(string filePath)
    {
        try
        {
            var content = await File.ReadAllTextAsync(filePath);
            var fileName = Path.GetFileNameWithoutExtension(filePath);

            if (!fileName.StartsWith("R") || !content.Contains("ComponentBase"))
                return null;

            var component = new ComponentMetadata
            {
                Name = fileName,
                FullName = fileName,
                FilePath = filePath,
                LastModified = File.GetLastWriteTime(filePath),
                Category = "Uncategorized",
                Complexity = "Simple"
            };

            // Extract XML documentation
            var xmlDocMatches = Regex.Matches(content, @"///\s*(.*)", RegexOptions.Multiline);
            if (xmlDocMatches.Count > 0)
            {
                var xmlDoc = string.Join("\n", xmlDocMatches.Cast<Match>().Select(m => m.Groups[1].Value));
                ParseXmlDocumentation(xmlDoc, component);
            }

            // Extract parameters
            ExtractParameters(content, component);

            return component;
        }
        catch
        {
            return null;
        }
    }

    private void ParseXmlDocumentation(string xmlDoc, ComponentMetadata component)
    {
        var summaryMatch = Regex.Match(xmlDoc, @"<summary>(.*?)</summary>", RegexOptions.Singleline);
        if (summaryMatch.Success)
        {
            component.Description = summaryMatch.Groups[1].Value.Trim();
        }

        var promptMatch = Regex.Match(xmlDoc, @"<ai-prompt>(.*?)</ai-prompt>", RegexOptions.Singleline);
        if (promptMatch.Success)
        {
            component.AI.Prompt = promptMatch.Groups[1].Value.Trim();
        }

        var useMatch = Regex.Match(xmlDoc, @"<ai-common-use>(.*?)</ai-common-use>", RegexOptions.Singleline);
        if (useMatch.Success)
        {
            component.AI.CommonUse = useMatch.Groups[1].Value.Trim();
        }
    }

    private void ExtractParameters(string code, ComponentMetadata component)
    {
        var parameterMatches = Regex.Matches(code, @"\[Parameter\]\s*public\s+(\w+(?:\?)?)\s+(\w+)", RegexOptions.Multiline);
        
        foreach (Match match in parameterMatches)
        {
            var paramType = match.Groups[1].Value;
            var paramName = match.Groups[2].Value;

            component.Parameters[paramName] = new ParameterMetadata
            {
                Name = paramName,
                Type = paramType,
                Description = $"{paramName} parameter",
                IsRequired = !paramType.Contains("?")
            };
        }
    }
}