using Microsoft.Extensions.DependencyInjection;
using RR.Blazor.Services;
using RR.Blazor.Models;
using RR.Blazor.Configuration;
using RR.Blazor.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;

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
            
            // Theme services
            services.AddScoped<IThemeCompiler, LibSassThemeCompiler>();
            services.AddScoped<ICustomThemeProvider, CustomThemeProvider>();
            
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
        
        /// <summary>Validation configuration</summary>
        public ValidationOptions Validation { get; set; } = new ValidationOptions();
        
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
        
        /// <summary>Configure validation settings</summary>
        public RRBlazorOptions WithValidation(Action<ValidationOptions> configure)
        {
            configure?.Invoke(Validation);
            return this;
        }
        
        /// <summary>Disable validation during builds</summary>
        public RRBlazorOptions DisableValidation()
        {
            Validation.Enabled = false;
            return this;
        }
        
        /// <summary>Add a custom theme from SCSS file</summary>
        public RRBlazorOptions WithCustomTheme(string themeName, string scssFilePath)
        {
            ValidateTheme(themeName, scssFilePath);
            
            Theme.CustomThemes ??= new Dictionary<string, string>();
            Theme.CustomThemes[themeName] = scssFilePath;
            return this;
        }
        
        /// <summary>Add multiple custom themes</summary>
        public RRBlazorOptions WithCustomThemes(Dictionary<string, string> themes)
        {
            ArgumentNullException.ThrowIfNull(themes);
            
            Theme.CustomThemes ??= new Dictionary<string, string>();
            foreach (var theme in themes)
            {
                ValidateTheme(theme.Key, theme.Value);
                Theme.CustomThemes[theme.Key] = theme.Value;
            }
            return this;
        }
        
        /// <summary>Validate theme configuration</summary>
        private static void ValidateTheme(string themeName, string scssFilePath)
        {
            // Validate theme name
            if (string.IsNullOrWhiteSpace(themeName))
                throw new ArgumentException("Theme name cannot be null or empty", nameof(themeName));
            
            if (themeName.Length > 50)
                throw new ArgumentException("Theme name cannot exceed 50 characters", nameof(themeName));
            
            if (!System.Text.RegularExpressions.Regex.IsMatch(themeName, @"^[a-zA-Z0-9_-]+$"))
                throw new ArgumentException("Theme name must contain only alphanumeric characters, hyphens, and underscores", nameof(themeName));
            
            // Validate file path
            if (string.IsNullOrWhiteSpace(scssFilePath))
                throw new ArgumentException("SCSS file path cannot be null or empty", nameof(scssFilePath));
            
            if (!scssFilePath.EndsWith(".scss", StringComparison.OrdinalIgnoreCase))
                throw new ArgumentException("Theme file must have .scss extension", nameof(scssFilePath));
            
            if (scssFilePath.Contains("..") || Path.IsPathRooted(scssFilePath) && !Path.IsPathFullyQualified(scssFilePath))
                throw new ArgumentException("Invalid file path. Use relative paths within project", nameof(scssFilePath));
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
    
    /// <summary>
    /// Validation configuration options
    /// </summary>
    public class ValidationOptions
    {
        /// <summary>Enable validation during builds (default: true)</summary>
        public bool Enabled { get; set; } = true;
        
        /// <summary>Enable component parameter validation (default: true)</summary>
        public bool ValidateComponents { get; set; } = true;
        
        /// <summary>Enable CSS class usage validation (default: true)</summary>
        public bool ValidateClasses { get; set; } = true;
        
        /// <summary>Enable in development environment (default: true)</summary>
        public bool EnableInDevelopment { get; set; } = true;
        
        /// <summary>Fail build on validation errors (default: false)</summary>
        public bool FailBuildOnErrors { get; set; } = false;
        
        /// <summary>Enable verbose validation logging</summary>
        public bool VerboseLogging { get; set; } = false;
    }
}