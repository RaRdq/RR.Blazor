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
    }
}