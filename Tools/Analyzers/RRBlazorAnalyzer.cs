using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace RR.Blazor.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class RRBlazorAnalyzer : DiagnosticAnalyzer
    {
        private const string Category = "RRBlazor";

        private static readonly DiagnosticDescriptor InvalidComponentParameterRule = new DiagnosticDescriptor(
            "RR1001",
            "Invalid RR.Blazor component parameter",
            "Component '{0}' does not have a parameter named '{1}'. Available parameters: {2}.",
            Category,
            DiagnosticSeverity.Error,
            isEnabledByDefault: true,
            helpLinkUri: "https://docs.rr-blazor.dev/components");

        private static readonly DiagnosticDescriptor UnknownComponentRule = new DiagnosticDescriptor(
            "RR1002",
            "Unknown RR.Blazor component",
            "Unknown RR.Blazor component '{0}'. Check component name spelling.",
            Category,
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            helpLinkUri: "https://docs.rr-blazor.dev/components");

        private static readonly DiagnosticDescriptor DeprecatedParameterRule = new DiagnosticDescriptor(
            "RR1003",
            "Deprecated RR.Blazor parameter",
            "Parameter '{0}' on component '{1}' is deprecated. Use '{2}' instead{3}.",
            Category,
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            helpLinkUri: "https://docs.rr-blazor.dev/migration");

        private static readonly DiagnosticDescriptor InlineStyleRule = new DiagnosticDescriptor(
            "RR1004",
            "Inline styles not allowed",
            "Inline styles are not allowed. Use utility classes with the 'Class' parameter instead.",
            Category,
            DiagnosticSeverity.Error,
            isEnabledByDefault: true,
            helpLinkUri: "https://docs.rr-blazor.dev/styling");

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create(InvalidComponentParameterRule, UnknownComponentRule, DeprecatedParameterRule, InlineStyleRule);

        private Dictionary<string, ComponentInfo> componentDocs = new Dictionary<string, ComponentInfo>();

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();

            // Register to analyze additional files (AI docs)
            context.RegisterCompilationStartAction(compilationContext =>
            {
                // Load AI documentation from additional files
                var aiDocsFile = compilationContext.Options.AdditionalFiles
                    .FirstOrDefault(f => Path.GetFileName(f.Path) == "rr-ai-docs.json");

                if (aiDocsFile != null)
                {
                    LoadComponentDocumentation(aiDocsFile);
                }

                // Register to analyze Razor files
                compilationContext.RegisterAdditionalFileAction(fileContext =>
                {
                    if (Path.GetExtension(fileContext.AdditionalFile.Path) == ".razor")
                    {
                        AnalyzeRazorFile(fileContext);
                    }
                });
            });
        }

        private void LoadComponentDocumentation(AdditionalText aiDocsFile)
        {
            try
            {
                var content = aiDocsFile.GetText()?.ToString();
                if (string.IsNullOrEmpty(content)) return;

                using var doc = JsonDocument.Parse(content);
                if (doc.RootElement.TryGetProperty("components", out var componentsElement))
                {
                    foreach (var component in componentsElement.EnumerateObject())
                    {
                        var info = new ComponentInfo { Name = component.Name };
                        
                        if (component.Value.TryGetProperty("parameters", out var parametersElement))
                        {
                            foreach (var param in parametersElement.EnumerateObject())
                            {
                                info.Parameters.Add(param.Name);
                            }
                        }

                        componentDocs[component.Name] = info;
                    }
                }
            }
            catch (Exception)
            {
                // Silently fail - analyzer shouldn't crash compilation
            }
        }

        private void AnalyzeRazorFile(AdditionalFileAnalysisContext context)
        {
            var sourceText = context.AdditionalFile.GetText(context.CancellationToken);
            if (sourceText == null) return;

            var content = sourceText.ToString();
            
            // Skip if no RR.Blazor components
            if (!content.Contains("<R")) return;

            // Regex to match RR.Blazor components
            var componentRegex = new Regex(@"<(R[A-Z][a-zA-Z]*)\s+([^>]*?)/?>");
            var matches = componentRegex.Matches(content);

            foreach (Match match in matches)
            {
                var componentName = match.Groups[1].Value;
                var parametersText = match.Groups[2].Value;
                var lineNumber = GetLineNumber(sourceText, match.Index);
                var linePositionSpan = GetLinePositionSpan(sourceText, match.Index, match.Length);

                // Parse parameters
                var parameters = ParseParameters(parametersText);

                // Validate component
                ValidateComponent(context, componentName, parameters, linePositionSpan);
            }
        }

        private Dictionary<string, string> ParseParameters(string parametersText)
        {
            var parameters = new Dictionary<string, string>();
            var paramRegex = new Regex(@"([a-zA-Z][a-zA-Z0-9]*)\s*=\s*""([^""]*)""");
            var matches = paramRegex.Matches(parametersText);

            foreach (Match match in matches)
            {
                parameters[match.Groups[1].Value] = match.Groups[2].Value;
            }

            return parameters;
        }

        private void ValidateComponent(AdditionalFileAnalysisContext context, string componentName, 
            Dictionary<string, string> parameters, LinePositionSpan linePositionSpan)
        {
            var location = Location.Create(context.AdditionalFile.Path, 
                new TextSpan(0, 0), linePositionSpan);

            // Check if component exists
            if (!componentDocs.ContainsKey(componentName))
            {
                context.ReportDiagnostic(Diagnostic.Create(UnknownComponentRule, location, componentName));
                return;
            }

            var componentInfo = componentDocs[componentName];
            var validParameters = new HashSet<string>(componentInfo.Parameters);
            
            // Add common Blazor parameters
            validParameters.UnionWith(new[] { "Class", "Id", "ChildContent", "@onclick", "@ref", "@bind-Value" });

            // Deprecated parameter mappings
            var deprecatedParams = new Dictionary<string, (string replacement, string message)>
            {
                ["Style"] = ("Class", " (use utility classes instead)"),
                ["IsClickable"] = ("Clickable", ""),
                ["Icon"] = ("StartIcon or EndIcon", " (based on position)"),
                ["IconPosition"] = ("StartIcon or EndIcon", " (use StartIcon/EndIcon directly)")
            };

            foreach (var param in parameters)
            {
                var paramName = param.Key.TrimStart('@');

                // Check for Style attribute
                if (paramName == "Style")
                {
                    context.ReportDiagnostic(Diagnostic.Create(InlineStyleRule, location));
                    continue;
                }

                // Check for deprecated parameters
                if (deprecatedParams.ContainsKey(paramName))
                {
                    var (replacement, message) = deprecatedParams[paramName];
                    context.ReportDiagnostic(Diagnostic.Create(DeprecatedParameterRule, location, 
                        paramName, componentName, replacement, message));
                    continue;
                }

                // Check if parameter is valid
                if (!validParameters.Contains(paramName) && !paramName.StartsWith("on"))
                {
                    var availableParams = string.Join(", ", validParameters.Take(10));
                    if (validParameters.Count > 10) availableParams += "...";
                    
                    context.ReportDiagnostic(Diagnostic.Create(InvalidComponentParameterRule, location, 
                        componentName, paramName, availableParams));
                }
            }
        }

        private int GetLineNumber(SourceText sourceText, int position)
        {
            return sourceText.Lines.GetLineFromPosition(position).LineNumber + 1;
        }

        private LinePositionSpan GetLinePositionSpan(SourceText sourceText, int start, int length)
        {
            var startLine = sourceText.Lines.GetLineFromPosition(start);
            var endLine = sourceText.Lines.GetLineFromPosition(start + length);
            
            return new LinePositionSpan(
                new LinePosition(startLine.LineNumber, start - startLine.Start),
                new LinePosition(endLine.LineNumber, (start + length) - endLine.Start));
        }

        private class ComponentInfo
        {
            public string Name { get; set; } = "";
            public HashSet<string> Parameters { get; set; } = new HashSet<string>();
        }
    }
}