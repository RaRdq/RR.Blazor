using Microsoft.Extensions.DependencyInjection;
using RR.Blazor.Services;
using RR.Blazor.Models;
using System;

namespace RR.Blazor.Extensions
{
    /// <summary>
    /// Service collection extensions for RR.Blazor
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Add RR.Blazor services to the service collection
        /// </summary>
        public static IServiceCollection AddRRBlazor(this IServiceCollection services, Action<RRBlazorOptions> configure = null)
        {
            // Core services
            services.AddScoped<IModalService, ModalService>();
            services.AddScoped<IThemeService, BlazorThemeService>();
            services.AddScoped<IAppSearchService, AppSearchService>();
            services.AddScoped<IToastService, ToastService>();
            
            // Configuration
            var options = new RRBlazorOptions();
            configure?.Invoke(options);
            services.AddSingleton(options);
            
            // Theme configuration
            if (options.Theme != null)
            {
                services.AddSingleton(options.Theme);
            }
            
            return services;
        }
    }
    
    /// <summary>
    /// Configuration options for RR.Blazor
    /// </summary>
    public class RRBlazorOptions
    {
        /// <summary>Theme configuration</summary>
        public ThemeConfiguration Theme { get; set; } = ThemeConfiguration.Default;
        
        /// <summary>Whether animations are enabled</summary>
        public bool AnimationsEnabled { get; set; } = true;
        
        /// <summary>Configure theme settings</summary>
        public RRBlazorOptions WithTheme(Action<ThemeConfiguration> configure)
        {
            configure?.Invoke(Theme);
            return this;
        }
        
        /// <summary>Enable or disable animations</summary>
        public RRBlazorOptions WithAnimations(bool enabled)
        {
            AnimationsEnabled = enabled;
            Theme.AnimationsEnabled = enabled;
            return this;
        }
    }
}