using RR.Blazor.CLI.Models;
using System.Text;
using System.Text.RegularExpressions;

namespace RR.Blazor.CLI.Services;

public interface IAICodeGenService
{
    Task<GenerationResult?> GenerateFromPromptAsync(string prompt, string type, ComponentRegistry registry);
    Task<string> OptimizeCodeAsync(string code, ComponentRegistry registry);
    Task<List<string>> SuggestComponentsAsync(string description, ComponentRegistry registry);
}

public class AICodeGenService : IAICodeGenService
{
    private readonly Dictionary<string, string> _templates = new()
    {
        ["button"] = """
<RButton Text="{text}" 
         Variant="{variant}"
         {icon}
         OnClick="@HandleClick" />
""",
        ["card"] = """
<RCard Title="{title}" 
       Elevation="4" 
       class="glass-light">
    <div class="pa-6">
        {content}
    </div>
</RCard>
""",
        ["form"] = """
<RForm @bind-Model="model" OnValidSubmit="@HandleSubmit">
    <div class="d-flex flex-column gap-4">
        {fields}
        <div class="d-flex justify-end gap-3 pt-4">
            <RButton Text="Cancel" Variant="ButtonVariant.Secondary" />
            <RButton Text="Save" Variant="ButtonVariant.Primary" Type="submit" />
        </div>
    </div>
</RForm>
""",
        ["dashboard"] = """
<div class="d-flex flex-column gap-6 pa-6">
    <div class="d-flex justify-between align-center">
        <h1 class="text-h4 ma-0">{title}</h1>
        <div class="d-flex gap-3">
            {actions}
        </div>
    </div>
    
    <div class="stats-grid gap-6">
        {stats}
    </div>
    
    <RCard Title="Data Overview" Elevation="4" class="glass-light">
        <div class="pa-6">
            {content}
        </div>
    </RCard>
</div>
"""
    };

    public async Task<GenerationResult?> GenerateFromPromptAsync(string prompt, string type, ComponentRegistry registry)
    {
        try
        {
            var result = new GenerationResult();
            
            // Analyze prompt for intent
            var analysis = AnalyzePrompt(prompt, type, registry);
            result.Analysis = analysis.Description;
            result.SuggestedComponents = analysis.Components;
            result.SuggestedUtilities = analysis.Utilities;

            // Generate code based on type
            result.Code = type switch
            {
                "form" => await GenerateFormAsync(prompt, analysis, registry),
                "dashboard" => await GenerateDashboardAsync(prompt, analysis, registry),
                "layout" => await GenerateLayoutAsync(prompt, analysis, registry),
                _ => await GenerateComponentAsync(prompt, analysis, registry)
            };

            return result;
        }
        catch (Exception)
        {
            return null;
        }
    }

    public async Task<string> OptimizeCodeAsync(string code, ComponentRegistry registry)
    {
        // Analyze code and suggest optimizations
        var optimized = code;

        // Replace verbose component usage with utility classes where appropriate
        optimized = OptimizeWithUtilities(optimized, registry);

        // Standardize parameter patterns
        optimized = StandardizeParameters(optimized);

        return optimized;
    }

    public async Task<List<string>> SuggestComponentsAsync(string description, ComponentRegistry registry)
    {
        var suggestions = new List<string>();
        var lowerDesc = description.ToLower();

        foreach (var component in registry.Components.Values)
        {
            var score = 0;

            // Check name match
            if (lowerDesc.Contains(component.Name.ToLower().Replace("r", "")))
                score += 10;

            // Check description match
            if (component.Description.ToLower().Contains(lowerDesc) || 
                lowerDesc.Contains(component.Description.ToLower()))
                score += 5;

            // Check common use match
            if (component.AI.CommonUse.ToLower().Contains(lowerDesc) ||
                lowerDesc.Contains(component.AI.CommonUse.ToLower()))
                score += 8;

            // Check tags match
            foreach (var tag in component.AI.Tags)
            {
                if (lowerDesc.Contains(tag.ToLower()))
                    score += 3;
            }

            if (score > 0)
                suggestions.Add(component.Name);
        }

        return suggestions.Take(5).ToList();
    }

    private PromptAnalysis AnalyzePrompt(string prompt, string type, ComponentRegistry registry)
    {
        var analysis = new PromptAnalysis
        {
            Description = $"Generating {type} based on: {prompt}"
        };

        var lowerPrompt = prompt.ToLower();

        // Detect common patterns
        var patterns = new Dictionary<string, List<string>>
        {
            ["login"] = new() { "RFormField", "RButton", "RCard" },
            ["dashboard"] = new() { "RStatsCard", "RCard", "RDataTable", "RButton" },
            ["form"] = new() { "RForm", "RFormField", "RButton" },
            ["table"] = new() { "RDataTable", "RButton", "RFormField" },
            ["card"] = new() { "RCard", "RButton" },
            ["modal"] = new() { "RModal", "RButton", "RFormField" },
            ["navigation"] = new() { "RTabs", "RBreadcrumbs", "RButton" }
        };

        foreach (var pattern in patterns)
        {
            if (lowerPrompt.Contains(pattern.Key))
            {
                analysis.Components.AddRange(pattern.Value);
            }
        }

        // Suggest utilities based on layout keywords
        if (lowerPrompt.Contains("professional") || lowerPrompt.Contains("enterprise"))
        {
            analysis.Utilities.AddRange(new[] { "elevation-4", "glass-light", "pa-6" });
        }

        if (lowerPrompt.Contains("grid") || lowerPrompt.Contains("columns"))
        {
            analysis.Utilities.AddRange(new[] { "d-grid", "gap-4", "form-grid" });
        }

        if (lowerPrompt.Contains("flex") || lowerPrompt.Contains("horizontal"))
        {
            analysis.Utilities.AddRange(new[] { "d-flex", "justify-between", "align-center" });
        }

        return analysis;
    }

    private async Task<string> GenerateFormAsync(string prompt, PromptAnalysis analysis, ComponentRegistry registry)
    {
        var fields = new List<string>();
        var lowerPrompt = prompt.ToLower();

        // Extract common form fields from prompt
        var fieldPatterns = new Dictionary<string, string>
        {
            ["name"] = """<RFormField Label="Name" @bind-Value="model.Name" Required="true" />""",
            ["email"] = """<RFormField Label="Email" Type="FieldType.Email" @bind-Value="model.Email" Required="true" />""",
            ["password"] = """<RFormField Label="Password" Type="FieldType.Password" @bind-Value="model.Password" Required="true" />""",
            ["phone"] = """<RFormField Label="Phone" Type="FieldType.Tel" @bind-Value="model.Phone" />""",
            ["address"] = """<RFormField Label="Address" Type="FieldType.Textarea" @bind-Value="model.Address" />""",
            ["date"] = """<RFormField Label="Date" Type="FieldType.Date" @bind-Value="model.Date" />"""
        };

        foreach (var pattern in fieldPatterns)
        {
            if (lowerPrompt.Contains(pattern.Key))
            {
                fields.Add(pattern.Value);
            }
        }

        // Default fields if none detected
        if (!fields.Any())
        {
            fields.AddRange(new[]
            {
                """<RFormField Label="Name" @bind-Value="model.Name" Required="true" />""",
                """<RFormField Label="Email" Type="FieldType.Email" @bind-Value="model.Email" Required="true" />"""
            });
        }

        return _templates["form"].Replace("{fields}", string.Join("\n        ", fields));
    }

    private async Task<string> GenerateDashboardAsync(string prompt, PromptAnalysis analysis, ComponentRegistry registry)
    {
        var title = ExtractTitle(prompt);
        var actions = """<RButton Text="Add New" Variant="ButtonVariant.Primary" Icon="add" IconPosition="IconPosition.Start" />""";        
        
        var stats = string.Join("\n        ", new[]
        {
            """<RStatsCard Title="Total" Value="1,234" Change="+12%" Icon="trending_up" Variant="StatsVariant.Success" />""",
            """<RStatsCard Title="Active" Value="856" Change="+5%" Icon="people" Variant="StatsVariant.Info" />""",
            """<RStatsCard Title="Revenue" Value="$45K" Change="+18%" Icon="attach_money" Variant="StatsVariant.Success" />"""
        });

        var content = """<RDataTable Items="@data" Striped="true" Hoverable="true" class="elevation-1" />""";

        return _templates["dashboard"]
            .Replace("{title}", title)
            .Replace("{actions}", actions)
            .Replace("{stats}", stats)
            .Replace("{content}", content);
    }

    private async Task<string> GenerateLayoutAsync(string prompt, PromptAnalysis analysis, ComponentRegistry registry)
    {
        var utilities = string.Join(" ", analysis.Utilities.DefaultIfEmpty("elevation-2 pa-4"));
        
        return $"""
<div class="{utilities}">
    <div class="d-flex justify-between align-center mb-4">
        <h2 class="text-h5 ma-0">{ExtractTitle(prompt)}</h2>
    </div>
    <div class="d-flex flex-column gap-4">
        <!-- Content goes here -->
    </div>
</div>
""";
    }

    private async Task<string> GenerateComponentAsync(string prompt, PromptAnalysis analysis, ComponentRegistry registry)
    {
        var lowerPrompt = prompt.ToLower();

        if (lowerPrompt.Contains("button"))
        {
            var variant = lowerPrompt.Contains("primary") ? "ButtonVariant.Primary" : "ButtonVariant.Secondary";
            var text = ExtractText(prompt) ?? "Click Me";
            var icon = ExtractIcon(prompt);
            
            return _templates["button"]
                .Replace("{text}", text)
                .Replace("{variant}", variant)
                .Replace("{icon}", string.IsNullOrEmpty(icon) ? "" : $"""Icon="{icon}" IconPosition="IconPosition.Start""");
        }

        if (lowerPrompt.Contains("card"))
        {
            var title = ExtractTitle(prompt);
            return _templates["card"]
                .Replace("{title}", title)
                .Replace("{content}", "<!-- Card content goes here -->");
        }

        // Default to card
        return _templates["card"]
            .Replace("{title}", ExtractTitle(prompt))
            .Replace("{content}", "<!-- Generated content -->");
    }

    private string OptimizeWithUtilities(string code, ComponentRegistry registry)
    {
        // Replace common component patterns with utilities
        var optimizations = new Dictionary<string, string>
        {
            [@"<div[^>]*class=""[^""]*card[^""]*""[^>]*>"] = """<div class="elevation-4 glass-light pa-6 rounded-lg">""",
            [@"<div[^>]*class=""[^""]*flex[^""]*""[^>]*>"] = """<div class="d-flex justify-between align-center">"""
        };

        var optimized = code;
        foreach (var optimization in optimizations)
        {
            optimized = Regex.Replace(optimized, optimization.Key, optimization.Value);
        }

        return optimized;
    }

    private string StandardizeParameters(string code)
    {
        // Convert legacy icon patterns to new unified pattern
        code = Regex.Replace(code, @"StartIcon=""([^""]+)""", """Icon="$1" IconPosition="IconPosition.Start""");
        code = Regex.Replace(code, @"EndIcon=""([^""]+)""", """Icon="$1" IconPosition="IconPosition.End""");
        
        return code;
    }

    private string ExtractTitle(string prompt)
    {
        var words = prompt.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        return string.Join(" ", words.Take(3).Select(w => char.ToUpper(w[0]) + w.Substring(1).ToLower()));
    }

    private string? ExtractText(string prompt)
    {
        var textMatch = Regex.Match(prompt, @"""([^""]+)""");
        return textMatch.Success ? textMatch.Groups[1].Value : null;
    }

    private string? ExtractIcon(string prompt)
    {
        var iconKeywords = new Dictionary<string, string>
        {
            ["save"] = "save",
            ["delete"] = "delete",
            ["edit"] = "edit",
            ["add"] = "add",
            ["search"] = "search",
            ["settings"] = "settings",
            ["home"] = "home"
        };

        var lowerPrompt = prompt.ToLower();
        return iconKeywords.FirstOrDefault(kvp => lowerPrompt.Contains(kvp.Key)).Value;
    }
}

public class GenerationResult
{
    public string Code { get; set; } = "";
    public string Analysis { get; set; } = "";
    public List<string> SuggestedComponents { get; set; } = new();
    public List<string> SuggestedUtilities { get; set; } = new();
}

public class PromptAnalysis
{
    public string Description { get; set; } = "";
    public List<string> Components { get; set; } = new();
    public List<string> Utilities { get; set; } = new();
}