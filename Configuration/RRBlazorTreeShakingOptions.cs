using System.ComponentModel.DataAnnotations;

namespace RR.Blazor.Configuration;

/// <summary>
/// Configuration options for RR.Blazor CSS tree-shaking and optimization
/// </summary>
public class RRBlazorTreeShakingOptions
{
    /// <summary>
    /// Enable or disable CSS tree-shaking optimization
    /// </summary>
    public bool EnableTreeShaking { get; set; } = true;


    /// <summary>
    /// Path to the component registry JSON file
    /// </summary>
    [Required]
    public string ComponentRegistryPath { get; set; } = "./wwwroot/component-registry.json";

    /// <summary>
    /// Output path for optimized CSS files
    /// </summary>
    [Required]
    public string OutputPath { get; set; } = "./wwwroot/css/optimized";

    /// <summary>
    /// Enable verbose logging during optimization
    /// </summary>
    public bool VerboseLogging { get; set; } = false;

    /// <summary>
    /// Patterns to always include in the CSS bundle (safelist)
    /// </summary>
    public List<string> SafelistPatterns { get; set; } = new()
    {
        "rr-*", "blazor-*", "table-*", "file-*", "upload-*", "button-*",
        "hover\\:", "focus\\:", "active\\:", "disabled\\:",
        "sm\\:", "md\\:", "lg\\:", "xl\\:",
        "animate-*", "transition-*", "duration-*", "ease-*"
    };

    /// <summary>
    /// File patterns to scan for component usage
    /// </summary>
    public List<string> ContentPatterns { get; set; } = new()
    {
        "**/*.razor",
        "**/*.cs", 
        "**/*.html",
        "**/*.js"
    };

    /// <summary>
    /// Run tree-shaking optimization in development environment
    /// </summary>
    public bool EnableInDevelopment { get; set; } = false;

    /// <summary>
    /// Minimum file size (in KB) before tree-shaking is applied
    /// </summary>
    [Range(1, 10000)]
    public int MinimumFileSizeKB { get; set; } = 50;

    /// <summary>
    /// Cache optimization results for faster subsequent builds
    /// </summary>
    public bool EnableCaching { get; set; } = true;

    /// <summary>
    /// Path to store optimization cache
    /// </summary>
    public string CachePath { get; set; } = "./obj/rr-blazor-cache";

    /// <summary>
    /// Generate source maps for optimized CSS
    /// </summary>
    public bool GenerateSourceMaps { get; set; } = false;

    /// <summary>
    /// Additional PowerShell parameters for the tree-shaking script
    /// </summary>
    public Dictionary<string, object> PowerShellParameters { get; set; } = new();

    /// <summary>
    /// Timeout for tree-shaking process in milliseconds
    /// </summary>
    [Range(1000, 300000)]
    public int TimeoutMs { get; set; } = 60000;

}


/// <summary>
/// Fluent configuration builder for RR.Blazor tree-shaking
/// </summary>
public class RRBlazorTreeShakingBuilder
{
    private readonly RRBlazorTreeShakingOptions _options = new();

    /// <summary>
    /// Disable CSS tree-shaking optimization
    /// </summary>
    public RRBlazorTreeShakingBuilder DisableTreeShaking()
    {
        _options.EnableTreeShaking = false;
        return this;
    }

    /// <summary>
    /// Enable tree-shaking with custom options
    /// </summary>
    public RRBlazorTreeShakingBuilder WithTreeShaking(Action<RRBlazorTreeShakingOptions>? configure = null)
    {
        _options.EnableTreeShaking = true;
        configure?.Invoke(_options);
        return this;
    }


    /// <summary>
    /// Set output path for optimized CSS files
    /// </summary>
    public RRBlazorTreeShakingBuilder OutputTo(string path)
    {
        _options.OutputPath = path;
        return this;
    }

    /// <summary>
    /// Enable verbose logging
    /// </summary>
    public RRBlazorTreeShakingBuilder WithVerboseLogging(bool enabled = true)
    {
        _options.VerboseLogging = enabled;
        return this;
    }

    /// <summary>
    /// Add custom safelist patterns
    /// </summary>
    public RRBlazorTreeShakingBuilder WithSafelist(params string[] patterns)
    {
        _options.SafelistPatterns.AddRange(patterns);
        return this;
    }

    /// <summary>
    /// Enable optimization in development environment
    /// </summary>
    public RRBlazorTreeShakingBuilder EnableInDevelopment(bool enabled = true)
    {
        _options.EnableInDevelopment = enabled;
        return this;
    }

    /// <summary>
    /// Configure caching options
    /// </summary>
    public RRBlazorTreeShakingBuilder WithCaching(bool enabled = true, string? cachePath = null)
    {
        _options.EnableCaching = enabled;
        if (!string.IsNullOrEmpty(cachePath))
            _options.CachePath = cachePath;
        return this;
    }

    /// <summary>
    /// Build the configuration options
    /// </summary>
    internal RRBlazorTreeShakingOptions Build() => _options;
}