using Microsoft.Extensions.DependencyInjection;
using RR.Blazor.Services;
using RR.Blazor.Models;
using RR.Blazor.Configuration;
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
            // Configuration - create first so we can use it
            var options = new RRBlazorOptions();
            configure?.Invoke(options);
            services.AddSingleton(options);
            
            // Core services
            services.AddScoped<IModalService, ModalService>();
            services.AddScoped<IThemeService, BlazorThemeService>();
            services.AddScoped<IAppSearchService, AppSearchService>();
            
            // Toast service with configuration from options
            services.AddRRToast(toastOptions =>
            {
                toastOptions.Position = options.Toast.Position;
                toastOptions.MaxToasts = options.Toast.MaxToasts;
                toastOptions.DefaultDuration = options.Toast.DefaultDuration;
                toastOptions.ShowCloseButton = options.Toast.ShowCloseButton;
                toastOptions.NewestOnTop = options.Toast.NewestOnTop;
                toastOptions.PreventDuplicates = options.Toast.PreventDuplicates;
            });
            
            // Theme configuration
            if (options.Theme != null)
            {
                services.AddSingleton(options.Theme);
            }
            
            // Tree-shaking options (for configuration only)
            var treeShakingOptions = new RRBlazorTreeShakingOptions();
            services.AddSingleton(treeShakingOptions);
            
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
        
        /// <summary>Toast service configuration</summary>
        public ToastServiceOptions Toast { get; set; } = new ToastServiceOptions
        {
            Position = ToastPosition.TopRight,
            MaxToasts = 5,
            DefaultDuration = 4000,
            ShowCloseButton = true,
            NewestOnTop = true,
            PreventDuplicates = false
        };
        
        /// <summary>Whether animations are enabled</summary>
        public bool AnimationsEnabled { get; set; } = true;
        
        /// <summary>Configure theme settings</summary>
        public RRBlazorOptions WithTheme(Action<ThemeConfiguration> configure)
        {
            configure?.Invoke(Theme);
            return this;
        }
        
        /// <summary>Configure toast settings</summary>
        public RRBlazorOptions WithToasts(Action<ToastServiceOptions> configure)
        {
            configure?.Invoke(Toast);
            return this;
        }
        
        /// <summary>Enable or disable animations</summary>
        public RRBlazorOptions WithAnimations(bool enabled)
        {
            AnimationsEnabled = enabled;
            Theme.AnimationsEnabled = enabled;
            return this;
        }
        
        /// <summary>Tree-shaking configuration (always enabled with golden ratio)</summary>
        public TreeShakingOptions TreeShaking { get; set; } = new TreeShakingOptions();
        
        /// <summary>Configure tree-shaking settings</summary>
        public RRBlazorOptions WithTreeShaking(Action<TreeShakingOptions> configure)
        {
            configure?.Invoke(TreeShaking);
            return this;
        }
        
        /// <summary>Disable tree-shaking optimization</summary>
        public RRBlazorOptions DisableTreeShaking()
        {
            TreeShaking.Enabled = false;
            return this;
        }
    }
    
    /// <summary>
    /// Tree-shaking configuration options
    /// </summary>
    public class TreeShakingOptions
    {
        /// <summary>Enable tree-shaking optimization (default: true)</summary>
        public bool Enabled { get; set; } = true;
        
        /// <summary>Enable in development environment (default: false)</summary>
        public bool EnableInDevelopment { get; set; } = false;
        
        /// <summary>Output path for optimized CSS</summary>
        public string OutputPath { get; set; } = "./wwwroot/css/optimized";
        
        /// <summary>Enable verbose logging</summary>
        public bool VerboseLogging { get; set; } = false;
    }
}