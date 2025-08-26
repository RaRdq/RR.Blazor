using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
#if RRCORE_ENABLED
using RR.Core.ServiceSetup;
#endif
using RR.Blazor.Services;
using RR.Blazor.Services.Export;
using RR.Blazor.Services.Export.Providers;
using RR.Blazor.Models;
using RR.Blazor.Interfaces;
using Blazored.LocalStorage;
using System.Linq;

namespace RR.Blazor.ServiceSetup
{
    public static class RRBlazorExtensions
    {
#if RRCORE_ENABLED
        /// <summary>
        /// Adds RR.Blazor component library with RR.Core integration
        /// Must be called after AddRRCore() for proper dependency chain
        /// Provides full functionality out-of-the-box following DI principles
        /// </summary>
        /// <param name="services">RR Core services</param>
        /// <param name="blazorConfig">Optional configuration action to override defaults</param>
        /// <returns>RR Core services for chaining</returns>
        public static RRCoreServices AddRRBlazor(this RRCoreServices services, Action<RRBlazorConfiguration> blazorConfig = null)
        {
            // Register default services with full functionality enabled
            RegisterDefaultServices(services.ServiceCollection);
            RegisterDefaultConfiguration(services.ServiceCollection);

            // Allow custom configuration overrides
            if (blazorConfig != null)
            {
                var configuration = new RRBlazorConfiguration(services.ServiceCollection);
                blazorConfig.Invoke(configuration);
                configuration.Build();
            }

            return services;
        }
#endif

        /// <summary>
        /// Adds RR.Blazor component library standalone (without RR.Core dependency)
        /// Provides full functionality out-of-the-box following DI principles
        /// </summary>
        /// <param name="services">Service collection</param>
        /// <param name="blazorConfig">Optional configuration action to override defaults</param>
        /// <returns>Service collection for chaining</returns>
        public static IServiceCollection AddRRBlazor(this IServiceCollection services, Action<RRBlazorConfiguration> blazorConfig = null)
        {
            // Register default services with full functionality enabled
            RegisterDefaultServices(services);
            RegisterDefaultConfiguration(services);

            // Allow custom configuration overrides
            if (blazorConfig != null)
            {
                var configuration = new RRBlazorConfiguration(services);
                blazorConfig.Invoke(configuration);
                configuration.Build();
            }

            return services;
        }

        private static void RegisterDefaultServices(IServiceCollection serviceCollection)
        {
            // Register Blazored LocalStorage if not already registered
            if (!serviceCollection.Any(x => x.ServiceType == typeof(ILocalStorageService)))
            {
                serviceCollection.AddBlazoredLocalStorage();
            }

            // Register core RR.Blazor services - Universal services work with both Server and WebAssembly
            serviceCollection.AddScoped<IJavaScriptInteropService, JavaScriptInteropService>();
            serviceCollection.AddScoped<IThemeService, BlazorThemeService>();
            
            serviceCollection.AddScoped<IModalService, ModalService>();
            serviceCollection.AddSingleton<IToastService, ToastService>();
            serviceCollection.AddSingleton<IAppSearchService, AppSearchService>();
            serviceCollection.AddScoped<IAppConfigurationService, AppConfigurationService>();
            serviceCollection.AddScoped<IFilterPersistenceService, FilterPersistenceService>();
            serviceCollection.AddScoped<IPivotService, PivotService>();
            
            // Register export services with default providers (enabled by default - use DisableExport to remove)
            RegisterExportServices(serviceCollection);
            
            // Register default toast configuration
            serviceCollection.AddSingleton(new ToastServiceOptions
            {
                Position = ToastPosition.TopRight,
                MaxToasts = 5,
                DefaultDuration = 4000,
                ShowCloseButton = true,
                NewestOnTop = true,
                PreventDuplicates = false
            });

            // Register tree-shaking options - enabled by default, disable what you don't need
            serviceCollection.AddSingleton(new RRBlazorTreeShakingOptions
            {
                Enabled = true,
                EnableInDevelopment = false,
                OutputPath = "./wwwroot/css/optimized",
                VerboseLogging = false
            });
        }

        private static void RegisterDefaultConfiguration(IServiceCollection serviceCollection)
        {
            // Register default theme configuration - full functionality enabled
            // Colors default to null to use CSS variables from SCSS
            serviceCollection.AddSingleton(new RRBlazorThemeOptions
            {
                Mode = ThemeMode.System,
                PrimaryColor = null,      // Uses --color-primary from SCSS
                SecondaryColor = null,    // Uses CSS variables from SCSS
                ErrorColor = null,        // Uses --color-error from SCSS
                SuccessColor = null,      // Uses --color-success from SCSS
                WarningColor = null,      // Uses --color-warning from SCSS
                InfoColor = null,         // Uses --color-info from SCSS
                AnimationsEnabled = true
            });

            // Register default app shell configuration - all features enabled
            serviceCollection.AddSingleton(new AppConfiguration
            {
                Title = "Application",
                ShowBreadcrumbs = true,
                ShowQuickActions = true,
                ShowSearch = true,
                ShowNotifications = true,
                ShowUserMenu = true,
                ShowStatusBar = false,
                EnableKeyboardShortcuts = true,
                CollapseSidebarOnMobile = true,
                RememberSidebarState = true
            });

            // Register default animation options - full animations enabled
            serviceCollection.AddSingleton(new RRBlazorAnimationOptions
            {
                EnableAnimations = true,
                DefaultDuration = 200,
                DefaultEasing = "cubic-bezier(0, 0, 0.2, 1)"
            });

            // Register default theme configuration for runtime theme management
            serviceCollection.AddScoped<ThemeConfiguration>(sp =>
            {
                var options = sp.GetService<RRBlazorThemeOptions>() ?? new RRBlazorThemeOptions();
                return new ThemeConfiguration
                {
                    Mode = options.Mode,
                    PrimaryColor = options.PrimaryColor,
                    SecondaryColor = options.SecondaryColor,
                    ErrorColor = options.ErrorColor,
                    WarningColor = options.WarningColor,
                    InfoColor = options.InfoColor,
                    SuccessColor = options.SuccessColor,
                    AnimationsEnabled = options.AnimationsEnabled
                };
            });
        }

        public class RRBlazorConfiguration
        {
            private readonly IServiceCollection serviceCollection;
            protected event Action OnBuild;

            internal RRBlazorConfiguration(IServiceCollection serviceCollection)
            {
                this.serviceCollection = serviceCollection;
            }

            /// <summary>
            /// Enable tree-shaking optimization
            /// </summary>
            /// <param name="enabled">Whether to enable tree-shaking</param>
            /// <returns>Configuration instance for chaining</returns>
            public RRBlazorConfiguration WithTreeShaking(bool enabled = true)
            {
                OnBuild += () =>
                {
                    serviceCollection.AddSingleton(new RRBlazorTreeShakingOptions
                    {
                        Enabled = enabled
                    });
                };
                return this;
            }

            /// <summary>
            /// Overrides default theme settings
            /// </summary>
            /// <param name="themeOptions">Theme configuration action</param>
            /// <returns>Configuration instance for chaining</returns>
            public RRBlazorConfiguration WithTheme(Action<RRBlazorThemeOptions> themeOptions)
            {
                OnBuild += () =>
                {
                    var options = new RRBlazorThemeOptions();
                    themeOptions(options);
                    serviceCollection.AddSingleton(options);

                    // Configure initial theme configuration
                    serviceCollection.AddScoped<ThemeConfiguration>(sp =>
                    {
                        var config = new ThemeConfiguration
                        {
                            Mode = options.Mode,
                            PrimaryColor = options.PrimaryColor,
                            SecondaryColor = options.SecondaryColor,
                            ErrorColor = options.ErrorColor,
                            WarningColor = options.WarningColor,
                            InfoColor = options.InfoColor,
                            SuccessColor = options.SuccessColor,
                            AnimationsEnabled = options.AnimationsEnabled
                        };
                        return config;
                    });
                };
                return this;
            }

            /// <summary>
            /// Overrides animation settings (animations enabled by default)
            /// </summary>
            /// <param name="enableAnimations">Whether to enable animations</param>
            /// <returns>Configuration instance for chaining</returns>
            public RRBlazorConfiguration WithAnimations(bool enableAnimations = true)
            {
                OnBuild += () =>
                {
                    serviceCollection.AddSingleton(new RRBlazorAnimationOptions
                    {
                        EnableAnimations = enableAnimations
                    });
                };
                return this;
            }
            
            /// <summary>
            /// Overrides AppShell configuration (all features enabled by default)
            /// </summary>
            /// <param name="appConfig">App configuration action</param>
            /// <returns>Configuration instance for chaining</returns>
            public RRBlazorConfiguration WithAppShell(Action<AppConfiguration> appConfig = null)
            {
                OnBuild += () =>
                {
                    var config = new AppConfiguration();
                    appConfig?.Invoke(config);
                    serviceCollection.AddSingleton(config);
                };
                return this;
            }
            
            /// <summary>
            /// Disable export features (inverted configuration - enabled by default)
            /// </summary>
            /// <returns>Configuration instance for chaining</returns>
            public RRBlazorConfiguration DisableExport()
            {
                OnBuild += () =>
                {
                    // Remove export services if user explicitly disables
                    var exportDescriptors = serviceCollection
                        .Where(d => d.ServiceType.Namespace?.Contains("Export") == true)
                        .ToList();
                    
                    foreach (var descriptor in exportDescriptors)
                    {
                        serviceCollection.Remove(descriptor);
                    }
                };
                return this;
            }
            
            /// <summary>
            /// Configure export options (export is enabled by default)
            /// </summary>
            /// <param name="configure">Export configuration action</param>
            /// <returns>Configuration instance for chaining</returns>
            public RRBlazorConfiguration WithExportOptions(Action<ExportConfiguration> configure)
            {
                OnBuild += () =>
                {
                    var config = new ExportConfiguration();
                    configure(config);
                    
                    // Apply configuration to registered services
                    
                    if (config.AdditionalProviders?.Any() == true)
                    {
                        foreach (var providerType in config.AdditionalProviders)
                        {
                            serviceCollection.AddSingleton(typeof(IExportProvider), providerType);
                        }
                    }
                };
                return this;
            }

            internal void Build()
            {
                OnBuild?.Invoke();
                OnBuild = null;
            }
        }

        public class RRBlazorThemeOptions
        {
            public ThemeMode Mode { get; set; } = ThemeMode.System;
            public string PrimaryColor { get; set; } // null = use CSS variable
            public string SecondaryColor { get; set; } // null = use CSS variable
            public string ErrorColor { get; set; } // null = use CSS variable
            public string SuccessColor { get; set; } // null = use CSS variable
            public string WarningColor { get; set; } // null = use CSS variable
            public string InfoColor { get; set; } // null = use CSS variable
            public bool AnimationsEnabled { get; set; } = true;

        }

        public class RRBlazorAnimationOptions
        {
            public bool EnableAnimations { get; set; } = true;
            public int DefaultDuration { get; set; } = 200;
            public string DefaultEasing { get; set; } = "cubic-bezier(0, 0, 0.2, 1)";
        }
        
        private static void RegisterExportServices(IServiceCollection serviceCollection)
        {
            // Register core export services
            serviceCollection.AddSingleton<ICoreExportService>(provider =>
            {
                var logger = provider.GetRequiredService<ILogger<CoreExportService>>();
                var exportService = new CoreExportService(logger);
                
                // Auto-register all IExportProvider implementations
                var providers = provider.GetServices<IExportProvider>();
                foreach (var exportProvider in providers)
                {
                    exportService.RegisterExportProvider(exportProvider);
                }
                
                return exportService;
            });
            
            serviceCollection.AddScoped<IExportService, ExportService>();
            
            // Register default providers (CSV, JSON, XML are always included)
            serviceCollection.AddSingleton<IExportProvider, CsvExportProvider>();
            serviceCollection.AddSingleton<IExportProvider, JsonExportProvider>();
            serviceCollection.AddSingleton<IExportProvider, XmlExportProvider>();
            
            // Excel provider removed - not part of UI component library
        }
        
    }
    
    /// <summary>
    /// Export configuration for RR.Blazor
    /// </summary>
    public class ExportConfiguration
    {
        /// <summary>Disable Excel export (enabled by default if ClosedXML available)</summary>
        public bool DisableExcel { get; set; }
        
        /// <summary>Additional custom export providers to register</summary>
        public List<Type> AdditionalProviders { get; set; } = new();
        
        /// <summary>Default export options</summary>
        public ExportOptions DefaultOptions { get; set; } = new();
    }
    
    /// <summary>
    /// Tree-shaking configuration options for RR.Blazor
    /// </summary>
    public class RRBlazorTreeShakingOptions
    {
        /// <summary>Enable tree-shaking optimization (default: true - disable what you don't need)</summary>
        public bool Enabled { get; set; } = true;
        
        /// <summary>Enable in development environment (default: false)</summary>
        public bool EnableInDevelopment { get; set; } = false;
        
        /// <summary>Output path for optimized CSS</summary>
        public string OutputPath { get; set; } = "./wwwroot/css/optimized";
        
        /// <summary>Enable verbose logging</summary>
        public bool VerboseLogging { get; set; } = false;
    }
}
