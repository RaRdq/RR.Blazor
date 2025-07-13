using System;
using System.Collections.Generic;

namespace RR.Blazor.CLI.Models;

public class ComponentMetadata
{
    public string Name { get; set; } = "";
    public string FullName { get; set; } = "";
    public string Category { get; set; } = "";
    public string Complexity { get; set; } = "";
    public string Description { get; set; } = "";
    public AIMetadata AI { get; set; } = new();
    public Dictionary<string, ParameterMetadata> Parameters { get; set; } = new();
    public List<ComponentExample> Examples { get; set; } = new();
    public List<string> Dependencies { get; set; } = new();
    public string FilePath { get; set; } = "";
    public DateTime LastModified { get; set; }
}

public class AIMetadata
{
    public string Prompt { get; set; } = "";
    public string CommonUse { get; set; } = "";
    public string AvoidUsage { get; set; } = "";
    public List<string> Tags { get; set; } = new();
    public List<AIPattern> Patterns { get; set; } = new();
}

public class AIPattern
{
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public string Code { get; set; } = "";
    public string Prompt { get; set; } = "";
}

public class ParameterMetadata
{
    public string Name { get; set; } = "";
    public string Type { get; set; } = "";
    public string Description { get; set; } = "";
    public bool IsRequired { get; set; }
    public object? DefaultValue { get; set; }
    public string AIHint { get; set; } = "";
    public List<string> SuggestedValues { get; set; } = new();
    public List<string> EnumValues { get; set; } = new();
}

public class ComponentExample
{
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public string Code { get; set; } = "";
    public string AIPrompt { get; set; } = "";
    public List<string> Tags { get; set; } = new();
}

public class ComponentRegistry
{
    public string Schema { get; set; } = "https://rr-blazor.dev/schema/component-registry.json";
    public string Version { get; set; } = "1.0.0";
    public DateTime Generated { get; set; } = DateTime.UtcNow;
    public RegistryInfo Info { get; set; } = new();
    public Dictionary<string, ComponentMetadata> Components { get; set; } = new();
    public UtilityRegistry Utilities { get; set; } = new();
    public List<PatternLibrary> Patterns { get; set; } = new();
}

public class RegistryInfo
{
    public string Name { get; set; } = "RR.Blazor";
    public string Description { get; set; } = "Enterprise-grade Blazor component library with AI-first development patterns";
    public string Author { get; set; } = "RaRdq";
    public string Repository { get; set; } = "https://github.com/RaRdq/RR.Blazor";
    public string License { get; set; } = "MIT";
    public int ComponentCount { get; set; }
    public int UtilityCount { get; set; }
}

public class UtilityRegistry
{
    public List<UtilityCategory> Spacing { get; set; } = new();
    public List<UtilityCategory> Layout { get; set; } = new();
    public List<UtilityCategory> Typography { get; set; } = new();
    public List<UtilityCategory> Effects { get; set; } = new();
    public List<UtilityCategory> Colors { get; set; } = new();
}

public class UtilityCategory
{
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public List<string> Classes { get; set; } = new();
    public string AIHint { get; set; } = "";
    public List<string> CommonPatterns { get; set; } = new();
}

public class PatternLibrary
{
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public string Category { get; set; } = "";
    public string Code { get; set; } = "";
    public string AIPrompt { get; set; } = "";
    public List<string> Components { get; set; } = new();
    public List<string> Utilities { get; set; } = new();
}