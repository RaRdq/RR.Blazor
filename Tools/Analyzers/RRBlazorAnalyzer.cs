using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RRBlazorAnalyzer : DiagnosticAnalyzer
{
    private static readonly DiagnosticDescriptor AIDocGeneratedRule = new DiagnosticDescriptor(
        "RR1000",
        "RR.Blazor AI documentation generated",
        "Generated comprehensive AI documentation with {0} components and {1} utility patterns",
        "RRBlazor",
        DiagnosticSeverity.Info,
        isEnabledByDefault: true);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(AIDocGeneratedRule);

    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.RegisterCompilationAction(GenerateAIDocumentation);
    }

    private void GenerateAIDocumentation(CompilationAnalysisContext context)
    {
        var components = new Dictionary<string, object>();
        var utilityPatterns = GetUtilityPatterns();
        var cssVariables = GetCSSVariables();
        var bestPractices = GetBestPractices();

        // Analyze all syntax trees for components
        foreach (var syntaxTree in context.Compilation.SyntaxTrees)
        {
            if (Path.GetExtension(syntaxTree.FilePath) == ".razor")
            {
                var component = AnalyzeRazorComponent(syntaxTree, context.Compilation);
                if (component != null)
                {
                    var componentName = component.ContainsKey("name") ? component["name"].ToString() : "Unknown";
                    components[componentName] = component;
                }
            }
        }

        // Create comprehensive documentation
        var documentation = new Dictionary<string, object>
        {
            ["$schema"] = "https://rr-blazor.dev/schema/ai-docs.json",
            ["version"] = "1.0.0",
            ["generated"] = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ"),
            ["info"] = new Dictionary<string, object>
            {
                ["name"] = "RR.Blazor",
                ["description"] = "Enterprise Blazor component library - AI-optimized documentation",
                ["author"] = "RaRdq",
                ["license"] = "MIT",
                ["componentCount"] = components.Count,
                ["utilityPatternCount"] = utilityPatterns.Count
            },
            ["components"] = components,
            ["utilityPatterns"] = utilityPatterns,
            ["cssVariables"] = cssVariables,
            ["bestPractices"] = bestPractices
        };

        // Report success (file generation will be handled by MSBuild)
        var diagnostic = Diagnostic.Create(
            AIDocGeneratedRule,
            Location.None,
            components.Count,
            utilityPatterns.Count);
        context.ReportDiagnostic(diagnostic);
    }

    private Dictionary<string, object> AnalyzeRazorComponent(SyntaxTree syntaxTree, Compilation compilation)
    {
        var sourceText = syntaxTree.GetText();
        var content = sourceText.ToString();
        var fileName = Path.GetFileNameWithoutExtension(syntaxTree.FilePath);

        if (!fileName.StartsWith("R"))
            return null;

        var component = new Dictionary<string, object>
        {
            ["name"] = fileName,
            ["category"] = "Unknown",
            ["complexity"] = "Simple",
            ["description"] = "",
            ["aiPrompt"] = "",
            ["commonUse"] = "",
            ["avoidUsage"] = "",
            ["patterns"] = new Dictionary<string, string>(),
            ["parameters"] = new Dictionary<string, object>()
        };

        // Extract AI metadata from @** blocks
        var aiBlockMatch = Regex.Match(content, @"@\*\*\s*(.*?)\*\*@", RegexOptions.Singleline);
        if (aiBlockMatch.Success)
        {
            var aiBlock = aiBlockMatch.Groups[1].Value;

            ExtractXmlTag(aiBlock, "summary", (value) => component["description"] = value.Trim());
            ExtractXmlTag(aiBlock, "category", (value) => component["category"] = value.Trim());
            ExtractXmlTag(aiBlock, "complexity", (value) => component["complexity"] = value.Trim());
            ExtractXmlTag(aiBlock, "ai-prompt", (value) => component["aiPrompt"] = value.Trim());
            ExtractXmlTag(aiBlock, "ai-common-use", (value) => component["commonUse"] = value.Trim());
            ExtractXmlTag(aiBlock, "ai-avoid", (value) => component["avoidUsage"] = value.Trim());

            // Extract AI patterns
            var patterns = (Dictionary<string, string>)component["patterns"];
            var patternMatches = Regex.Matches(aiBlock, @"<ai-pattern name=""([^""]+)"">(.*?)</ai-pattern>");
            foreach (Match match in patternMatches)
            {
                patterns[match.Groups[1].Value] = match.Groups[2].Value.Trim();
            }
        }

        // Extract parameters from C# code
        var semanticModel = compilation.GetSemanticModel(syntaxTree);
        var root = syntaxTree.GetRoot();
        var classDeclarations = root.DescendantNodes().OfType<ClassDeclarationSyntax>();

        foreach (var classDecl in classDeclarations)
        {
            if (classDecl.Identifier.ValueText == fileName)
            {
                var parameters = (Dictionary<string, object>)component["parameters"];
                ExtractParameters(classDecl, parameters);
                break;
            }
        }

        return component;
    }

    private void ExtractXmlTag(string content, string tagName, Action<string> setValue)
    {
        var pattern = $@"<{tagName}>(.*?)</{tagName}>";
        var match = Regex.Match(content, pattern, RegexOptions.Singleline);
        if (match.Success)
        {
            setValue(match.Groups[1].Value);
        }
    }

    private void ExtractParameters(ClassDeclarationSyntax classDecl, Dictionary<string, object> parameters)
    {
        var properties = classDecl.Members.OfType<PropertyDeclarationSyntax>();
        foreach (var prop in properties)
        {
            var hasParameterAttr = prop.AttributeLists
                .SelectMany(al => al.Attributes)
                .Any(attr => attr.Name.ToString().Contains("Parameter"));

            if (hasParameterAttr)
            {
                var paramInfo = new Dictionary<string, object>
                {
                    ["name"] = prop.Identifier.ValueText,
                    ["type"] = prop.Type.ToString(),
                    ["description"] = "",
                    ["aiHint"] = "",
                    ["isRequired"] = false
                };

                // Extract XML documentation
                var leadingTrivia = prop.GetLeadingTrivia();
                foreach (var trivia in leadingTrivia)
                {
                    if (trivia.IsKind(SyntaxKind.SingleLineDocumentationCommentTrivia) ||
                        trivia.IsKind(SyntaxKind.MultiLineDocumentationCommentTrivia))
                    {
                        var docText = trivia.ToString();
                        ExtractXmlTag(docText, "summary", (value) => paramInfo["description"] = value.Trim());
                        ExtractXmlTag(docText, "ai-hint", (value) => paramInfo["aiHint"] = value.Trim());
                    }
                }

                parameters[prop.Identifier.ValueText] = paramInfo;
            }
        }
    }

    private Dictionary<string, object> GetUtilityPatterns()
    {
        return new Dictionary<string, object>
        {
            ["spacing"] = new Dictionary<string, object>
            {
                ["padding"] = new Dictionary<string, object>
                {
                    ["pattern"] = "pa-{0-24}, px-{0-24}, py-{0-24}, pt-{0-24}, pr-{0-24}, pb-{0-24}, pl-{0-24}",
                    ["description"] = "Padding utilities following design system scale",
                    ["aiHint"] = "Use pa-6 for standard card padding, px-4 py-2 for buttons",
                    ["scale"] = new[] { 0, 1, 2, 3, 4, 5, 6, 8, 10, 12, 16, 20, 24 }
                },
                ["margin"] = new Dictionary<string, object>
                {
                    ["pattern"] = "ma-{0-24}, mx-{0-24}, my-{0-24}, mt-{0-24}, mr-{0-24}, mb-{0-24}, ml-{0-24}, mx-auto",
                    ["description"] = "Margin utilities including auto centering",
                    ["aiHint"] = "Use mx-auto for centering, mb-4 for standard spacing",
                    ["scale"] = new[] { 0, 1, 2, 3, 4, 5, 6, 8, 10, 12, 16, 20, 24 }
                },
                ["gap"] = new Dictionary<string, object>
                {
                    ["pattern"] = "gap-{0-24}, gap-x-{0-24}, gap-y-{0-24}",
                    ["description"] = "Grid and flexbox gap utilities",
                    ["aiHint"] = "Use gap-4 for standard spacing in flex/grid layouts"
                }
            },
            ["layout"] = new Dictionary<string, object>
            {
                ["flexbox"] = new Dictionary<string, object>
                {
                    ["pattern"] = "d-flex, flex-{direction}, justify-{content}, align-{items}, flex-{wrap}",
                    ["values"] = new Dictionary<string, string[]>
                    {
                        ["direction"] = new[] { "row", "column", "row-reverse", "column-reverse" },
                        ["content"] = new[] { "start", "end", "center", "between", "around", "evenly" },
                        ["items"] = new[] { "start", "end", "center", "baseline", "stretch" },
                        ["wrap"] = new[] { "wrap", "nowrap", "wrap-reverse" }
                    },
                    ["aiHint"] = "Use d-flex justify-between align-center for header layouts"
                },
                ["grid"] = new Dictionary<string, object>
                {
                    ["pattern"] = "d-grid, grid-cols-{1-12}, grid-rows-{1-6}, col-span-{1-12}",
                    ["description"] = "CSS Grid utilities for complex layouts",
                    ["aiHint"] = "Use d-grid grid-cols-2 gap-4 for two-column layouts"
                }
            },
            ["effects"] = new Dictionary<string, object>
            {
                ["elevation"] = new Dictionary<string, object>
                {
                    ["pattern"] = "elevation-{0-24}, elevation-lift, hover:elevation-{0-24}",
                    ["description"] = "Material Design elevation system with interactive states",
                    ["aiHint"] = "Use elevation-4 for cards, elevation-8 for modals, elevation-lift for hover",
                    ["scale"] = new[] { 0, 1, 2, 4, 6, 8, 12, 16, 20, 24 }
                },
                ["glassmorphism"] = new Dictionary<string, object>
                {
                    ["pattern"] = "glass, glass-{variant}, backdrop-blur-{size}",
                    ["variants"] = new[] { "light", "medium", "heavy", "frost", "crystal", "interactive" },
                    ["description"] = "Modern glassmorphism effects with backdrop filters",
                    ["aiHint"] = "Use glass-light for subtle effects, glass-medium for prominence"
                }
            },
            ["typography"] = new Dictionary<string, object>
            {
                ["textSize"] = new Dictionary<string, object>
                {
                    ["pattern"] = "text-{size}",
                    ["values"] = new[] { "xs", "sm", "base", "lg", "xl", "2xl", "3xl", "4xl", "5xl", "6xl" },
                    ["semantic"] = new[] { "text-h1", "text-h2", "text-h3", "text-h4", "text-h5", "text-h6", "text-body-1", "text-body-2", "text-caption" },
                    ["aiHint"] = "Use text-h4 for section headers, text-body-1 for content"
                },
                ["textWeight"] = new Dictionary<string, object>
                {
                    ["pattern"] = "font-{weight}",
                    ["values"] = new[] { "thin", "light", "normal", "medium", "semibold", "bold", "extrabold", "black" },
                    ["aiHint"] = "Use font-semibold for emphasis, font-medium for buttons"
                }
            },
            ["business"] = new Dictionary<string, object>
            {
                ["formGrids"] = new Dictionary<string, object>
                {
                    ["pattern"] = "form-grid, form-grid--{columns}",
                    ["values"] = new[] { "1", "2", "3", "4", "auto" },
                    ["description"] = "Professional form layout grids",
                    ["aiHint"] = "Use form-grid--2 for dual-column forms"
                },
                ["statsGrids"] = new Dictionary<string, object>
                {
                    ["pattern"] = "stats-grid, action-grid",
                    ["description"] = "Dashboard and analytics layout patterns",
                    ["aiHint"] = "Use stats-grid for metric cards layout"
                }
            }
        };
    }

    private Dictionary<string, object> GetCSSVariables()
    {
        return new Dictionary<string, object>
        {
            ["colors"] = new Dictionary<string, object>
            {
                ["interactive"] = new[] { "--color-interactive-primary", "--color-interactive-secondary", "--color-interactive-focus" },
                ["text"] = new[] { "--color-text-primary", "--color-text-secondary", "--color-text-muted", "--color-text-inverse" },
                ["background"] = new[] { "--color-background-primary", "--color-background-elevated", "--color-background-glass" },
                ["status"] = new[] { "--color-success", "--color-warning", "--color-error", "--color-info" },
                ["aiHint"] = "Use semantic color variables for consistent theming"
            },
            ["spacing"] = new Dictionary<string, object>
            {
                ["scale"] = new[] { "--space-0", "--space-1", "--space-2", "--space-3", "--space-4", "--space-5", "--space-6", "--space-8", "--space-10", "--space-12", "--space-16", "--space-20", "--space-24" },
                ["aiHint"] = "Design system spacing scale for consistent layouts"
            },
            ["effects"] = new Dictionary<string, object>
            {
                ["glass"] = new[] { "--glass-bg", "--glass-light-bg", "--glass-medium-bg", "--glass-heavy-bg", "--glass-blur" },
                ["shadows"] = new[] { "--shadow-sm", "--shadow-md", "--shadow-lg", "--shadow-xl" },
                ["aiHint"] = "Professional visual effects for modern UIs"
            }
        };
    }

    private Dictionary<string, object> GetBestPractices()
    {
        return new Dictionary<string, object>
        {
            ["componentUsage"] = new Dictionary<string, string>
            {
                ["cards"] = "Use RCard for content containers, combine with elevation-4 and glass-light for professional appearance",
                ["buttons"] = "Use ButtonVariant.Primary for main actions, Secondary for supporting actions, Danger for destructive actions",
                ["forms"] = "Use form-grid--2 for dual-column layouts, Required=\"true\" for mandatory fields",
                ["spacing"] = "Use pa-6 for card content, gap-4 for flex layouts, mb-4 for standard element separation"
            },
            ["layoutPatterns"] = new Dictionary<string, string>
            {
                ["professionalCard"] = "elevation-4 glass-light pa-6 rounded-lg",
                ["headerLayout"] = "d-flex justify-between align-center py-3 px-4",
                ["formSection"] = "bg-elevated pa-4 rounded-md border border-light",
                ["statsGrid"] = "stats-grid gap-6 mb-8"
            },
            ["accessibility"] = new Dictionary<string, string>
            {
                ["colors"] = "All color combinations meet WCAG AA contrast requirements",
                ["focus"] = "Focus rings automatically applied to interactive elements",
                ["touchTargets"] = "Minimum 44px touch targets for mobile"
            }
        };
    }

}