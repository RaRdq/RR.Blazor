using System;

namespace RR.Blazor.Attributes;

/// <summary>
/// Marks a class as an RR.Blazor component for auto-discovery and schema generation.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class ComponentAttribute(string name) : Attribute
{
    public string Name { get; } = name;
    public string Category { get; set; } = "Uncategorized";
    public ComponentComplexity Complexity { get; set; } = ComponentComplexity.Simple;
    public bool IsAIOptimized { get; set; } = true;
}

/// <summary>
/// Indicates the component is optimized for AI-driven development.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class AIOptimizedAttribute : Attribute
{
    public string Prompt { get; set; } = "";
    public string CommonUse { get; set; } = "";
    public string AvoidUsage { get; set; } = "";
}

/// <summary>
/// Provides AI-specific metadata for component parameters.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public class AIParameterAttribute : Attribute
{
    public AIParameterAttribute(){}
    public AIParameterAttribute(string hint)
    {
        Hint=hint;
    }

public AIParameterAttribute(string hint, string example)
    {
        Hint = hint;
        Example = example;
    }
    public string Hint { get; set; } = "";
    public string[] SuggestedValues { get; set; } = Array.Empty<string>();
    public bool IsRequired { get; set; } = false;

    public string Example { get; set; }
}

/// <summary>
/// Component complexity levels for AI understanding.
/// </summary>
public enum ComponentComplexity
{
    Simple,
    Intermediate,
    Complex,
    Advanced
}

/// <summary>
/// Component categories for organization.
/// </summary>
public enum ComponentCategory
{
    Core,
    Layout,
    Navigation,
    Form,
    Data,
    Feedback,
    Display,
    Theme,
    Utility
}