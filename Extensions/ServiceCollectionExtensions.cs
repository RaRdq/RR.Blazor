using Microsoft.Extensions.DependencyInjection;
using RR.Blazor.Templates;
using RR.Blazor.Templates.Configuration;

namespace RR.Blazor.Extensions;

/// <summary>
/// Service collection extensions for RR.Blazor configuration
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds RR.Blazor services with optional template configuration
    /// </summary>
    public static IServiceCollection AddRRBlazor(
        this IServiceCollection services, 
        Action<RRBlazorConfiguration> configure = null)
    {
        var configuration = new RRBlazorConfiguration();
        configure?.Invoke(configuration);
        
        // Configure templates globally
        TemplateRegistry.Configure(configuration.Templates);
        
        return services;
    }
}

/// <summary>
/// Main configuration class for RR.Blazor
/// </summary>
public class RRBlazorConfiguration
{
    /// <summary>
    /// Template configuration
    /// </summary>
    public TemplateConfiguration Templates { get; set; } = new();
    
    /// <summary>
    /// Configure templates with fluent API
    /// </summary>
    public RRBlazorConfiguration WithTemplates(Action<TemplateConfigurationBuilder> configure)
    {
        var builder = new TemplateConfigurationBuilder();
        configure(builder);
        Templates = builder.Build();
        return this;
    }
}