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
            helpLinkUri: "https://docs.rrblazor.dev/components");

        private static readonly DiagnosticDescriptor UnknownComponentRule = new DiagnosticDescriptor(
            "RR1002",
            "Unknown RR.Blazor component",
            "Unknown RR.Blazor component '{0}'. Check component name spelling.",
            Category,
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            helpLinkUri: "https://docs.rrblazor.dev/components");

        private static readonly DiagnosticDescriptor DeprecatedParameterRule = new DiagnosticDescriptor(
            "RR1003",
            "Deprecated RR.Blazor parameter",
            "Parameter '{0}' on component '{1}' is deprecated. Use '{2}' instead.",
            Category,
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            helpLinkUri: "https://docs.rrblazor.dev/migration");

        private static readonly DiagnosticDescriptor InlineStyleRule = new DiagnosticDescriptor(
            "RR1004",
            "Inline styles not allowed",
            "Inline styles are not allowed. Use utility classes with the 'Class' parameter instead.",
            Category,
            DiagnosticSeverity.Error,
            isEnabledByDefault: true,
            helpLinkUri: "https://docs.rrblazor.dev/styling");

        private static readonly DiagnosticDescriptor MalformedIconRule = new DiagnosticDescriptor(
            "RR1005",
            "Malformed icon attribute",
            "Icon attribute appears to be malformed. Ensure proper syntax: Icon=\"value\" IconPosition=\"IconPosition.Start\"",
            Category,
            DiagnosticSeverity.Error,
            isEnabledByDefault: true,
            helpLinkUri: "https://docs.rrblazor.dev/icons");

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create(InvalidComponentParameterRule, UnknownComponentRule, DeprecatedParameterRule, InlineStyleRule, MalformedIconRule);

        private Dictionary<string, ComponentInfo> componentRegistry = new Dictionary<string, ComponentInfo>();

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();

            context.RegisterCompilationStartAction(compilationContext =>
            {
                // Build component registry from actual source files
                BuildComponentRegistryFromSource(compilationContext);

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

        private void BuildComponentRegistryFromSource(CompilationStartAnalysisContext context)
        {
            // Find all R*.razor component files in RR.Blazor project
            var componentFiles = context.Options.AdditionalFiles
                .Where(f => f.Path.Contains("RR.Blazor") && 
                           f.Path.Contains("Components") && 
                           Path.GetExtension(f.Path) == ".razor" &&
                           Path.GetFileNameWithoutExtension(f.Path).StartsWith("R"))
                .ToList();

            foreach (var file in componentFiles)
            {
                try
                {
                    var sourceText = file.GetText();
                    if (sourceText == null) continue;

                    var content = sourceText.ToString();
                    var componentName = Path.GetFileNameWithoutExtension(file.Path);
                    
                    // Extract parameters from @code block
                    var parameters = ExtractParametersFromRazorFile(content);
                    
                    componentRegistry[componentName] = new ComponentInfo 
                    { 
                        Name = componentName,
                        Parameters = new HashSet<string>(parameters)
                    };
                }
                catch (Exception)
                {
                    // Silently continue - don't crash analyzer
                }
            }
        }

        private List<string> ExtractParametersFromRazorFile(string content)
        {
            var parameters = new List<string>();
            
            // Extract @code blocks with better handling of nested braces
            var codeBlockRegex = new Regex(@"@code\s*{((?:[^{}]|{[^{}]*})*?)}", RegexOptions.Singleline);
            var codeBlocks = codeBlockRegex.Matches(content);

            foreach (Match codeBlock in codeBlocks)
            {
                var codeContent = codeBlock.Groups[1].Value;
                
                // Find [Parameter] properties with improved regex that handles multi-line definitions and attributes
                var parameterRegex = new Regex(@"\[Parameter\](?:\s*\[[^\]]*\])*\s*(?:\/\/\/[^\r\n]*[\r\n]+)*\s*(?:\/\*\*[^*]*\*+(?:[^/*][^*]*\*+)*\/\s*)*(?:public\s+)?(?:virtual\s+)?(?:override\s+)?([^=\s]+)\s+(\w+)\s*(?:{[^}]*}|;)", RegexOptions.Multiline | RegexOptions.IgnoreCase);
                var parameterMatches = parameterRegex.Matches(codeContent);

                foreach (Match paramMatch in parameterMatches)
                {
                    var parameterName = paramMatch.Groups[2].Value;
                    if (!string.IsNullOrEmpty(parameterName))
                    {
                        parameters.Add(parameterName);
                    }
                }

                // Also find EventCallback parameters which might have different patterns
                var eventCallbackRegex = new Regex(@"\[Parameter\](?:\s*\[[^\]]*\])*\s*(?:\/\/\/[^\r\n]*[\r\n]+)*\s*(?:public\s+)?EventCallback(?:<[^>]*>)?\s+(\w+)\s*(?:{[^}]*}|;)", RegexOptions.Multiline | RegexOptions.IgnoreCase);
                var eventCallbackMatches = eventCallbackRegex.Matches(codeContent);

                foreach (Match eventMatch in eventCallbackMatches)
                {
                    var parameterName = eventMatch.Groups[1].Value;
                    if (!string.IsNullOrEmpty(parameterName))
                    {
                        parameters.Add(parameterName);
                    }
                }
            }

            return parameters;
        }

        private void AnalyzeRazorFile(AdditionalFileAnalysisContext context)
        {
            var sourceText = context.AdditionalFile.GetText(context.CancellationToken);
            if (sourceText == null) return;

            var content = sourceText.ToString();
            
            // Skip if no RR.Blazor components
            if (!content.Contains("<R") && !content.Contains("</R")) return;

            // Skip RR.Blazor component files themselves
            if (context.AdditionalFile.Path.Contains("RR.Blazor") && 
                context.AdditionalFile.Path.Contains("Components")) return;

            // Analyze component usage
            AnalyzeComponentUsage(context, sourceText, content);
        }

        private void AnalyzeComponentUsage(AdditionalFileAnalysisContext context, SourceText sourceText, string content)
        {
            // Regex to match RR.Blazor components (both self-closing and with content)
            var componentRegex = new Regex(@"<(R[A-Z][a-zA-Z]*)\s+([^>]*?)(?:\s*/>|>)", RegexOptions.Multiline);
            var matches = componentRegex.Matches(content);

            foreach (Match match in matches)
            {
                var componentName = match.Groups[1].Value;
                var parametersText = match.Groups[2].Value;
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
            
            // Handle both simple and complex parameter patterns
            var paramRegex = new Regex(@"([a-zA-Z][a-zA-Z0-9]*)\s*=\s*""([^""]*)""", RegexOptions.Multiline);
            var matches = paramRegex.Matches(parametersText);

            foreach (Match match in matches)
            {
                var paramName = match.Groups[1].Value;
                var paramValue = match.Groups[2].Value;
                parameters[paramName] = paramValue;
            }

            return parameters;
        }

        private void ValidateComponent(AdditionalFileAnalysisContext context, string componentName, 
            Dictionary<string, string> parameters, LinePositionSpan linePositionSpan)
        {
            var location = Location.Create(context.AdditionalFile.Path, 
                new TextSpan(0, 0), linePositionSpan);

            // Check if component exists in our registry
            if (!componentRegistry.ContainsKey(componentName))
            {
                context.ReportDiagnostic(Diagnostic.Create(UnknownComponentRule, location, componentName));
                return;
            }

            var componentInfo = componentRegistry[componentName];
            var validParameters = new HashSet<string>(componentInfo.Parameters);
            
            // Add standard Blazor parameters
            validParameters.UnionWith(new[] { 
                "Class", "Id", "ChildContent", "AdditionalAttributes",
                "@ref", "@key", "@onclick", "@onmouseenter", "@onmouseleave",
                "@onfocus", "@onblur", "@onchange", "@oninput", "@onkeydown", "@onkeyup"
            });

            // Current deprecated parameters (based on CLAUDE.md migration)
            var deprecatedParams = new Dictionary<string, string>
            {
                ["StartIcon"] = "Icon + IconPosition=\"IconPosition.Start\"",
                ["EndIcon"] = "Icon + IconPosition=\"IconPosition.End\"",
                ["MaxHeight"] = "removed (use utility classes)"
            };

            foreach (var param in parameters)
            {
                var paramName = param.Key.TrimStart('@');
                var paramValue = param.Value;

                // Check for inline styles
                if (paramName == "Style" || paramName == "style")
                {
                    context.ReportDiagnostic(Diagnostic.Create(InlineStyleRule, location));
                    continue;
                }

                // Check for malformed icon attributes
                if (paramName == "Icon" && paramValue.Contains("IconPosition="))
                {
                    context.ReportDiagnostic(Diagnostic.Create(MalformedIconRule, location));
                    continue;
                }

                // Check for deprecated parameters
                if (deprecatedParams.ContainsKey(paramName))
                {
                    var replacement = deprecatedParams[paramName];
                    context.ReportDiagnostic(Diagnostic.Create(DeprecatedParameterRule, location, 
                        paramName, componentName, replacement));
                    continue;
                }

                // Check if parameter is valid
                if (!validParameters.Contains(paramName) && 
                    !paramName.StartsWith("on") && 
                    !paramName.StartsWith("@") &&
                    !paramName.StartsWith("bind-"))
                {
                    var availableParams = string.Join(", ", validParameters.Take(10));
                    if (validParameters.Count > 10) availableParams += "...";
                    
                    context.ReportDiagnostic(Diagnostic.Create(InvalidComponentParameterRule, location, 
                        componentName, paramName, availableParams));
                }
            }
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