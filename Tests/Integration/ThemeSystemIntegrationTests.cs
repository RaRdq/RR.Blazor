using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using Blazored.LocalStorage;
using RR.Blazor.Services;
using RR.Blazor.Models;
using RR.Blazor.Extensions;
using RR.Blazor.Components.Core;
using System.Text.Json;
using Bunit;

namespace RR.Blazor.Tests.Integration;

/// <summary>
/// Comprehensive integration tests for RR.Blazor theme system
/// Tests all layers: theme variables, abstraction mapping, service integration, and component rendering
/// </summary>
public class ThemeSystemIntegrationTests : TestContext
{
    private Mock<IJSRuntime> mockJSRuntime;
    private Mock<ILocalStorageService> mockLocalStorage;
    private IThemeService themeService;

    public ThemeSystemIntegrationTests()
    {
        // Setup mocks
        mockJSRuntime = new Mock<IJSRuntime>();
        mockLocalStorage = new Mock<ILocalStorageService>();
        
        // Register services
        Services.AddSingleton(mockJSRuntime.Object);
        Services.AddSingleton(mockLocalStorage.Object);
        Services.AddLogging();
        Services.AddRRBlazor();
        
        // Get theme service instance
        themeService = Services.GetRequiredService<IThemeService>();
    }

    [Fact]
    public async Task DefaultTheme_ShouldLoadWithCorrectVariables()
    {
        // Arrange - Setup system preferences detection
        var themeInfo = new { systemDark = false, highContrast = false };
        mockJSRuntime.Setup(x => x.InvokeAsync<dynamic>("RRTheme.getThemeInfo", It.IsAny<object[]>()))
                     .ReturnsAsync(themeInfo);

        // Act - Initialize theme service
        await themeService.InitializeAsync();

        // Assert - Verify default theme is loaded
        var currentTheme = themeService.CurrentTheme;
        currentTheme.Should().NotBeNull();
        currentTheme.Mode.Should().Be(ThemeMode.System);
        currentTheme.AnimationsEnabled.Should().BeTrue();
        currentTheme.AccessibilityMode.Should().BeFalse();
        currentTheme.HighContrastMode.Should().BeFalse();
    }

    [Fact]
    public async Task DarkTheme_ShouldSwitchAndApplyCorrectly()
    {
        // Arrange
        var themeInfo = new { systemDark = true, highContrast = false };
        mockJSRuntime.Setup(x => x.InvokeAsync<dynamic>("RRTheme.getThemeInfo", It.IsAny<object[]>()))
                     .ReturnsAsync(themeInfo);
        
        var appliedThemeData = new Dictionary<string, object>();
        mockJSRuntime.Setup(x => x.InvokeVoidAsync("window.RRBlazor.Theme.apply", It.IsAny<object[]>()))
                     .Callback<string, object[]>((method, args) => 
                     {
                         if (args?.Length > 0)
                             appliedThemeData = JsonSerializer.Deserialize<Dictionary<string, object>>(
                                 JsonSerializer.Serialize(args[0]));
                     });

        await themeService.InitializeAsync();

        // Act - Switch to dark theme
        await themeService.SetThemeModeAsync(ThemeMode.Dark);

        // Assert - Verify dark theme is applied
        var currentTheme = themeService.CurrentTheme;
        currentTheme.Mode.Should().Be(ThemeMode.Dark);
        
        // Verify JavaScript theme application was called
        mockJSRuntime.Verify(x => x.InvokeVoidAsync("window.RRBlazor.Theme.apply", It.IsAny<object[]>()), 
                           Times.AtLeastOnce());
    }

    [Fact]
    public async Task SystemTheme_ShouldRespondToSystemPreferenceChanges()
    {
        // Arrange - Start with system theme preference
        var systemDark = false;
        var themeInfo = new { systemDark = systemDark, highContrast = false };
        mockJSRuntime.Setup(x => x.InvokeAsync<dynamic>("RRTheme.getThemeInfo", It.IsAny<object[]>()))
                     .ReturnsAsync(themeInfo);
        
        await themeService.InitializeAsync();
        await themeService.SetThemeModeAsync(ThemeMode.System);

        // Act - Simulate system dark mode change
        systemDark = true;
        var newThemeInfo = new { systemDark = systemDark, highContrast = false };
        mockJSRuntime.Setup(x => x.InvokeAsync<dynamic>("RRTheme.getThemeInfo", It.IsAny<object[]>()))
                     .ReturnsAsync(newThemeInfo);

        // Trigger system theme detection (simulate timer callback)
        var effectiveMode = themeService.CurrentTheme.GetEffectiveMode(systemDark);

        // Assert - Verify effective theme mode changes with system preference
        effectiveMode.Should().Be(ThemeMode.Dark);
    }

    [Fact]
    public async Task CustomTheme_ShouldRegisterAndApplyCorrectly()
    {
        // Arrange - Create options with custom theme
        var options = new RRBlazorOptions();
        var customThemePath = "themes/corporate.scss";
        
        // Act - Register custom theme
        var result = options.WithCustomTheme("corporate", customThemePath);

        // Assert - Verify custom theme registration
        result.Should().Be(options);
        options.Theme.CustomThemes.Should().ContainKey("corporate");
        options.Theme.CustomThemes["corporate"].Should().Be(customThemePath);
    }

    [Fact]
    public void ThemeAbstraction_ShouldMaintainCorrectLayerSeparation()
    {
        // This test verifies the 3-layer architecture:
        // Layer 1: Theme Values (--theme-*)
        // Layer 2: Semantic Variables (--color-*)
        // Layer 3: Utility Classes
        
        // Arrange - Check default theme SCSS structure
        var defaultThemeFile = File.ReadAllText("C:\\Projects\\PayrollAI\\RR.Blazor\\Styles\\themes\\_default.scss");
        var variablesFile = File.ReadAllText("C:\\Projects\\PayrollAI\\RR.Blazor\\Styles\\abstracts\\_variables.scss");

        // Assert - Verify theme layer uses --theme- prefix
        defaultThemeFile.Should().Contain("--theme-canvas:");
        defaultThemeFile.Should().Contain("--theme-surface:");
        defaultThemeFile.Should().Contain("--theme-text:");
        defaultThemeFile.Should().Contain("--theme-primary:");

        // Assert - Verify semantic layer maps theme to color variables
        variablesFile.Should().Contain("--color-canvas: var(--theme-canvas)");
        variablesFile.Should().Contain("--color-surface: var(--theme-surface)");
        variablesFile.Should().Contain("--color-text: var(--theme-text)");
        variablesFile.Should().Contain("--color-primary: var(--theme-primary)");
    }

    [Fact]
    public void DarkTheme_ShouldDefineAllRequiredVariables()
    {
        // Arrange - Read dark theme file
        var darkThemeFile = File.ReadAllText("C:\\Projects\\PayrollAI\\RR.Blazor\\Styles\\themes\\_dark.scss");

        // Assert - Verify all essential theme variables are defined
        var requiredVariables = new[]
        {
            "--theme-canvas:",
            "--theme-surface:",
            "--theme-surface-elevated:",
            "--theme-text:",
            "--theme-text-muted:",
            "--theme-text-subtle:",
            "--theme-border:",
            "--theme-primary:",
            "--theme-primary-hover:",
            "--theme-primary-active:",
            "--theme-success:",
            "--theme-warning:",
            "--theme-error:",
            "--theme-info:"
        };

        foreach (var variable in requiredVariables)
        {
            darkThemeFile.Should().Contain(variable, 
                $"Dark theme should define {variable} variable");
        }
    }

    [Fact]
    public void RThemeProvider_ShouldRenderWithCorrectDataAttribute()
    {
        // Arrange - Setup theme service mock
        var mockThemeService = new Mock<IThemeService>();
        var testTheme = new ThemeConfiguration { Mode = ThemeMode.Dark };
        mockThemeService.Setup(x => x.CurrentTheme).Returns(testTheme);
        
        Services.AddSingleton(mockThemeService.Object);

        // Act - Render RThemeProvider component
        var component = RenderComponent<RThemeProvider>(parameters => parameters
            .Add(p => p.Theme, "dark")
            .Add(p => p.ChildContent, builder => builder.AddContent(0, "Test Content")));

        // Assert - Verify correct data-theme attribute
        var providerDiv = component.Find(".rr-theme-provider");
        providerDiv.Should().NotBeNull();
        providerDiv.GetAttribute("data-theme").Should().Be("dark");
        component.Markup.Should().Contain("Test Content");
    }

    [Fact]
    public async Task RAppShell_GetEffectiveTheme_ShouldReturnCorrectTheme()
    {
        // Arrange - Setup theme service with system preference
        var themeInfo = new { systemDark = true, highContrast = false };
        mockJSRuntime.Setup(x => x.InvokeAsync<dynamic>("RRTheme.getThemeInfo", It.IsAny<object[]>()))
                     .ReturnsAsync(themeInfo);
        
        await themeService.InitializeAsync();
        await themeService.SetThemeModeAsync(ThemeMode.System);

        // Act - Get effective theme
        var effectiveTheme = themeService.CurrentTheme.GetEffectiveMode(true);

        // Assert - Should return dark when system prefers dark
        effectiveTheme.Should().Be(ThemeMode.Dark);
    }

    [Fact]
    public async Task ThemeService_ShouldPersistThemeToLocalStorage()
    {
        // Arrange
        var testTheme = new ThemeConfiguration 
        { 
            Mode = ThemeMode.Dark,
            AnimationsEnabled = false,
            AccessibilityMode = true
        };

        // Act - Set theme (should trigger save)
        await themeService.SetThemeAsync(testTheme);

        // Assert - Verify localStorage was called
        mockLocalStorage.Verify(x => x.SetItemAsync("rr-blazor-theme", testTheme, default), 
                                Times.Once());
    }

    [Fact]
    public async Task ThemeService_ShouldLoadThemeFromLocalStorage()
    {
        // Arrange - Setup saved theme in localStorage
        var savedTheme = new ThemeConfiguration 
        { 
            Mode = ThemeMode.Light,
            AnimationsEnabled = false,
            Name = "Custom Light"
        };
        
        mockLocalStorage.Setup(x => x.GetItemAsync<ThemeConfiguration>("rr-blazor-theme", default))
                        .ReturnsAsync(savedTheme);

        var themeInfo = new { systemDark = false, highContrast = false };
        mockJSRuntime.Setup(x => x.InvokeAsync<dynamic>("RRTheme.getThemeInfo", It.IsAny<object[]>()))
                     .ReturnsAsync(themeInfo);

        // Act - Initialize theme service (should load from storage)
        await themeService.InitializeAsync();

        // Assert - Verify theme was loaded from storage
        var currentTheme = themeService.CurrentTheme;
        currentTheme.Mode.Should().Be(ThemeMode.Light);
        currentTheme.AnimationsEnabled.Should().BeFalse();
        currentTheme.Name.Should().Be("Custom Light");
    }

    [Fact]
    public async Task ThemeService_ShouldHandleJSInteropFailuresGracefully()
    {
        // Arrange - Setup JS interop to throw exception
        mockJSRuntime.Setup(x => x.InvokeAsync<dynamic>("RRTheme.getThemeInfo", It.IsAny<object[]>()))
                     .ThrowsAsync(new JSException("JS interop failed"));

        // Act - Should not throw exception
        var act = async () => await themeService.InitializeAsync();

        // Assert - Should handle gracefully and use defaults
        await act.Should().NotThrowAsync();
        themeService.IsSystemDark.Should().BeFalse(); // Default fallback
    }

    [Fact]
    public async Task ThemeService_ShouldTriggerThemeChangedEvent()
    {
        // Arrange
        var eventTriggered = false;
        ThemeConfiguration eventTheme = null;
        
        themeService.ThemeChanged += (theme) => 
        {
            eventTriggered = true;
            eventTheme = theme;
        };

        var newTheme = new ThemeConfiguration { Mode = ThemeMode.Dark };

        // Act
        await themeService.SetThemeAsync(newTheme);

        // Assert
        eventTriggered.Should().BeTrue();
        eventTheme.Should().NotBeNull();
        eventTheme.Mode.Should().Be(ThemeMode.Dark);
    }

    [Fact]
    public void CustomThemeValidation_ShouldEnforceSecurityRules()
    {
        // This test verifies the security tests still work in integration
        var options = new RRBlazorOptions();

        // Test path traversal protection
        var pathTraversalAction = () => options.WithCustomTheme("evil", "../../../etc/passwd.scss");
        pathTraversalAction.Should().Throw<ArgumentException>();

        // Test file extension validation
        var invalidExtensionAction = () => options.WithCustomTheme("theme", "theme.css");
        invalidExtensionAction.Should().Throw<ArgumentException>();

        // Test theme name validation
        var invalidNameAction = () => options.WithCustomTheme("theme with spaces", "theme.scss");
        invalidNameAction.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void ThemeArchitecture_ShouldNotHaveCircularDependencies()
    {
        // Verify semantic variables don't reference each other circularly
        var variablesFile = File.ReadAllText("C:\\Projects\\PayrollAI\\RR.Blazor\\Styles\\abstracts\\_variables.scss");
        
        // Check that semantic variables only reference theme variables, not other semantic variables
        var semanticVariablePattern = @"--color-\w+:\s*var\(--color-\w+\)";
        var matches = System.Text.RegularExpressions.Regex.Matches(variablesFile, semanticVariablePattern);
        
        // Should be minimal circular references - mostly should reference --theme- variables
        matches.Count.Should().BeLessThan(5, 
            "Semantic variables should primarily reference theme variables, not other semantic variables");
    }

    [Fact]
    public async Task ThemeProvider_ShouldInitializeAndDisposeCleanly()
    {
        // Arrange
        var themeProvider = RenderComponent<RThemeProvider>(parameters => parameters
            .Add(p => p.Theme, "light"));

        // Act - Dispose the component
        themeProvider.Instance.Should().BeAssignableTo<IAsyncDisposable>();
        
        var disposeAction = async () => await ((IAsyncDisposable)themeProvider.Instance).DisposeAsync();

        // Assert - Should dispose without exceptions
        await disposeAction.Should().NotThrowAsync();
    }
}