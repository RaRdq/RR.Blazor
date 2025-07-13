using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RR.Blazor.CLI.Commands;
using RR.Blazor.CLI.Services;
using System.CommandLine;

namespace RR.Blazor.CLI;

class Program
{
    static async Task<int> Main(string[] args)
    {
        var host = CreateHostBuilder(args).Build();
        
        var rootCommand = new RootCommand("RR.Blazor CLI - AI-first Blazor component management")
        {
            CreateAddCommand(host),
            CreateGenerateCommand(host),
            CreateValidateCommand(host),
            CreateRefreshCommand(host),
            CreateInitCommand(host),
            CreateSearchCommand(host)
        };

        return await rootCommand.InvokeAsync(args);
    }

    static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureServices((context, services) =>
            {
                services.AddSingleton<IComponentRegistryService, ComponentRegistryService>();
                services.AddSingleton<ITemplateService, TemplateService>();
                services.AddSingleton<IAICodeGenService, AICodeGenService>();
                services.AddSingleton<IFileService, FileService>();
                services.AddSingleton<IValidationService, ValidationService>();
                
                // Register command handlers
                services.AddTransient<AddCommand>();
                services.AddTransient<GenerateCommand>();
                services.AddTransient<ValidateCommand>();
                services.AddTransient<RefreshCommand>();
                services.AddTransient<InitCommand>();
                services.AddTransient<SearchCommand>();
            });

    static Command CreateAddCommand(IHost host)
    {
        var addCommand = new Command("add", "Add RR.Blazor components to your project");
        var componentArg = new Argument<string>("component", "Component name to add");
        var variantOption = new Option<string>("--variant", "Component variant");
        var textOption = new Option<string>("--text", "Component text content");
        var iconOption = new Option<string>("--icon", "Component icon");
        var outputOption = new Option<string>("--output", () => ".", "Output directory");

        addCommand.AddArgument(componentArg);
        addCommand.AddOption(variantOption);
        addCommand.AddOption(textOption);
        addCommand.AddOption(iconOption);
        addCommand.AddOption(outputOption);

        addCommand.SetHandler(async (component, variant, text, icon, output) =>
        {
            var addCommandHandler = host.Services.GetRequiredService<AddCommand>();
            await addCommandHandler.ExecuteAsync(component, variant, text, icon, output);
        }, componentArg, variantOption, textOption, iconOption, outputOption);

        return addCommand;
    }

    static Command CreateGenerateCommand(IHost host)
    {
        var generateCommand = new Command("generate", "Generate components from AI prompts");
        var promptArg = new Argument<string>("prompt", "AI prompt describing the component");
        var outputOption = new Option<string>("--output", () => ".", "Output directory");
        var typeOption = new Option<string>("--type", () => "component", "Generation type: component, form, layout, dashboard");

        generateCommand.AddArgument(promptArg);
        generateCommand.AddOption(outputOption);
        generateCommand.AddOption(typeOption);

        generateCommand.SetHandler(async (prompt, output, type) =>
        {
            var generateCommandHandler = host.Services.GetRequiredService<GenerateCommand>();
            await generateCommandHandler.ExecuteAsync(prompt, output, type);
        }, promptArg, outputOption, typeOption);

        return generateCommand;
    }

    static Command CreateValidateCommand(IHost host)
    {
        var validateCommand = new Command("validate", "Validate component usage and patterns");
        var pathOption = new Option<string>("--path", () => ".", "Path to validate");
        var checkPatternsOption = new Option<bool>("--check-patterns", "Check for pattern compliance");
        var checkAccessibilityOption = new Option<bool>("--check-accessibility", "Check accessibility compliance");

        validateCommand.AddOption(pathOption);
        validateCommand.AddOption(checkPatternsOption);
        validateCommand.AddOption(checkAccessibilityOption);

        validateCommand.SetHandler(async (path, checkPatterns, checkAccessibility) =>
        {
            var validateCommandHandler = host.Services.GetRequiredService<ValidateCommand>();
            await validateCommandHandler.ExecuteAsync(path, checkPatterns, checkAccessibility);
        }, pathOption, checkPatternsOption, checkAccessibilityOption);

        return validateCommand;
    }

    static Command CreateRefreshCommand(IHost host)
    {
        var refreshCommand = new Command("refresh", "Refresh component registry and schemas");
        var scanOption = new Option<bool>("--scan-components", "Scan for new components");
        var outputOption = new Option<string>("--output", "Output path for registry");

        refreshCommand.AddOption(scanOption);
        refreshCommand.AddOption(outputOption);

        refreshCommand.SetHandler(async (scanComponents, output) =>
        {
            var refreshCommandHandler = host.Services.GetRequiredService<RefreshCommand>();
            await refreshCommandHandler.ExecuteAsync(scanComponents, output);
        }, scanOption, outputOption);

        return refreshCommand;
    }

    static Command CreateInitCommand(IHost host)
    {
        var initCommand = new Command("init", "Initialize RR.Blazor in a project");
        var pathOption = new Option<string>("--path", () => ".", "Project path");
        var themeOption = new Option<string>("--theme", () => "system", "Default theme: light, dark, system");

        initCommand.AddOption(pathOption);
        initCommand.AddOption(themeOption);

        initCommand.SetHandler(async (path, theme) =>
        {
            var initCommandHandler = host.Services.GetRequiredService<InitCommand>();
            await initCommandHandler.ExecuteAsync(path, theme);
        }, pathOption, themeOption);

        return initCommand;
    }

    static Command CreateSearchCommand(IHost host)
    {
        var searchCommand = new Command("search", "Search available components");
        var queryArg = new Argument<string>("query", "Search query");
        var categoryOption = new Option<string>("--category", "Filter by category");
        var verboseOption = new Option<bool>("--verbose", "Show detailed information");

        searchCommand.AddArgument(queryArg);
        searchCommand.AddOption(categoryOption);
        searchCommand.AddOption(verboseOption);

        searchCommand.SetHandler(async (query, category, verbose) =>
        {
            var searchCommandHandler = host.Services.GetRequiredService<SearchCommand>();
            await searchCommandHandler.ExecuteAsync(query, category, verbose);
        }, queryArg, categoryOption, verboseOption);

        return searchCommand;
    }
}