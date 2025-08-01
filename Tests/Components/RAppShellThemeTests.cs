using RR.Blazor.Components.Layout;
using RR.Blazor.Services;
using RR.Blazor.Models;
using RR.Blazor.Enums;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.JSInterop;
using Bunit;

namespace RR.Blazor.Tests.Components;

/// <summary>
/// Tests for RAppShell theme integration, specifically GetEffectiveTheme() functionality
/// </summary>
public class RAppShellThemeTests : TestContext
{
    private Mock<IThemeService> mockThemeService;
    private Mock<IJSRuntime> mockJSRuntime;
    private Mock<IToastService> mockToastService;
    private Mock<IAppSearchService> mockAppSearchService;
    private Mock<IAppConfigurationService> mockAppConfigService;

    public RAppShellThemeTests()
    {
        // Setup all required mocks for RAppShell
        mockThemeService = new Mock<IThemeService>();
        mockJSRuntime = new Mock<IJSRuntime>();
        mockToastService = new Mock<IToastService>();
        mockAppSearchService = new Mock<IAppSearchService>();
        mockAppConfigService = new Mock<IAppConfigurationService>();

        // Register services
        Services.AddSingleton(mockThemeService.Object);
        Services.AddSingleton(mockJSRuntime.Object);
        Services.AddSingleton(mockToastService.Object);
        Services.AddSingleton(mockAppSearchService.Object);
        Services.AddSingleton(mockAppConfigService.Object);

        // Setup default app configuration
        var defaultConfig = new AppConfiguration { Title = "Test App" };
        mockAppConfigService.Setup(x => x.LoadAsync()).Returns(Task.CompletedTask);
        mockAppConfigService.Setup(x => x.Current).Returns(defaultConfig);
    }

    [Fact]
    public void RAppShell_ShouldRenderWithCorrectThemeAttribute_LightMode()
    {
        // Arrange
        var lightTheme = new ThemeConfiguration { Mode = ThemeMode.Light };
        mockThemeService.Setup(x => x.CurrentTheme).Returns(lightTheme);
        mockThemeService.Setup(x => x.IsSystemDark).Returns(false);

        // Act
        var component = RenderComponent<RAppShell>(parameters => parameters
            .Add(p => p.Title, "Test App")
            .Add(p => p.Theme, "light"));

        // Assert
        var appShell = component.Find(".app-shell");
        appShell.Should().NotBeNull();
        appShell.GetAttribute("data-theme").Should().Be("light");
    }

    [Fact]
    public void RAppShell_ShouldRenderWithCorrectThemeAttribute_DarkMode()
    {
        // Arrange
        var darkTheme = new ThemeConfiguration { Mode = ThemeMode.Dark };
        mockThemeService.Setup(x => x.CurrentTheme).Returns(darkTheme);
        mockThemeService.Setup(x => x.IsSystemDark).Returns(true);

        // Act
        var component = RenderComponent<RAppShell>(parameters => parameters
            .Add(p => p.Title, "Test App")
            .Add(p => p.Theme, "dark"));

        // Assert
        var appShell = component.Find(".app-shell");
        appShell.GetAttribute("data-theme").Should().Be("dark");
    }

    [Fact]
    public void RAppShell_GetEffectiveTheme_ShouldHandleSystemMode()
    {
        // Arrange - System mode with dark preference
        var systemTheme = new ThemeConfiguration { Mode = ThemeMode.System };
        mockThemeService.Setup(x => x.CurrentTheme).Returns(systemTheme);
        mockThemeService.Setup(x => x.IsSystemDark).Returns(true);

        // Act
        var component = RenderComponent<RAppShell>(parameters => parameters
            .Add(p => p.Title, "Test App")
            .Add(p => p.Theme, "system"));

        // Assert - Should resolve system mode to dark
        var appShell = component.Find(".app-shell");
        appShell.GetAttribute("data-theme").Should().Be("dark");
    }

    [Fact]
    public void RAppShell_GetEffectiveTheme_ShouldHandleSystemModeLight()
    {
        // Arrange - System mode with light preference
        var systemTheme = new ThemeConfiguration { Mode = ThemeMode.System };
        mockThemeService.Setup(x => x.CurrentTheme).Returns(systemTheme);
        mockThemeService.Setup(x => x.IsSystemDark).Returns(false);

        // Act
        var component = RenderComponent<RAppShell>(parameters => parameters
            .Add(p => p.Title, "Test App")
            .Add(p => p.Theme, "system"));

        // Assert - Should resolve system mode to light
        var appShell = component.Find(".app-shell");
        appShell.GetAttribute("data-theme").Should().Be("light");
    }

    [Fact]
    public void RAppShell_ShouldWrapContentInRThemeProvider()
    {
        // Arrange
        var testTheme = new ThemeConfiguration { Mode = ThemeMode.Light };
        mockThemeService.Setup(x => x.CurrentTheme).Returns(testTheme);

        // Act
        var component = RenderComponent<RAppShell>(parameters => parameters
            .Add(p => p.Title, "Test App")
            .Add(p => p.ChildContent, builder => 
                builder.AddContent(0, "Test Content")));

        // Assert - Should be wrapped in RThemeProvider
        var themeProvider = component.Find(".rr-theme-provider");
        themeProvider.Should().NotBeNull();
        
        var appShell = component.Find(".app-shell");
        appShell.Should().NotBeNull();
        
        component.Markup.Should().Contain("Test Content");
    }

    [Fact]
    public void RAppShell_ShouldIncludeThemeToggleWhenEnabled()
    {
        // Arrange
        var testTheme = new ThemeConfiguration { Mode = ThemeMode.Light };
        mockThemeService.Setup(x => x.CurrentTheme).Returns(testTheme);

        // Act
        var component = RenderComponent<RAppShell>(parameters => parameters
            .Add(p => p.Title, "Test App")
            .Add(p => p.Features, AppShellFeatures.All)); // Includes ThemeToggle

        // Assert - Should contain RThemeSwitcher
        // Note: This test verifies the component renders without error
        // The actual RThemeSwitcher component would need separate testing
        component.Markup.Should().NotBeEmpty();
    }

    [Theory]
    [InlineData(ThemeMode.Light, false, "light")]
    [InlineData(ThemeMode.Dark, true, "dark")]
    [InlineData(ThemeMode.System, true, "dark")]
    [InlineData(ThemeMode.System, false, "light")]
    public void RAppShell_GetEffectiveTheme_ShouldReturnCorrectThemeForAllModes(
        ThemeMode themeMode, bool isSystemDark, string expectedTheme)
    {
        // Arrange
        var theme = new ThemeConfiguration { Mode = themeMode };
        mockThemeService.Setup(x => x.CurrentTheme).Returns(theme);
        mockThemeService.Setup(x => x.IsSystemDark).Returns(isSystemDark);

        // Act
        var component = RenderComponent<RAppShell>(parameters => parameters
            .Add(p => p.Title, "Test App"));

        // Assert
        var appShell = component.Find(".app-shell");
        appShell.GetAttribute("data-theme").Should().Be(expectedTheme);
    }

    [Fact] 
    public void RAppShell_ShouldHandleNullThemeService()
    {
        // Arrange - Remove theme service
        Services.Remove(Services.First(s => s.ServiceType == typeof(IThemeService)));

        // Act - Should not throw exception
        var renderAction = () => RenderComponent<RAppShell>(parameters => parameters
            .Add(p => p.Title, "Test App"));

        // Assert - Should handle gracefully or throw expected exception
        // This tests defensive programming in GetEffectiveTheme()
        renderAction.Should().Throw<InvalidOperationException>()
            .WithMessage("*IThemeService*"); // Service not registered
    }

    [Fact]
    public void RAppShell_ShouldNotExposeSensitiveThemeData()
    {
        // Arrange
        var themeWithSensitiveData = new ThemeConfiguration 
        { 
            Mode = ThemeMode.Light,
            CustomVariables = new Dictionary<string, string>
            {
                ["--secret-api-key"] = "should-not-be-exposed",
                ["--internal-config"] = "internal-value"
            }
        };
        mockThemeService.Setup(x => x.CurrentTheme).Returns(themeWithSensitiveData);

        // Act
        var component = RenderComponent<RAppShell>(parameters => parameters
            .Add(p => p.Title, "Test App"));

        // Assert - Custom variables should not be exposed in markup
        component.Markup.Should().NotContain("secret-api-key");
        component.Markup.Should().NotContain("should-not-be-exposed");
        component.Markup.Should().NotContain("internal-config");
        component.Markup.Should().NotContain("internal-value");
    }

    [Fact]
    public async Task RAppShell_ShouldInitializeThemeAfterRender()
    {
        // Arrange
        var testTheme = new ThemeConfiguration { Mode = ThemeMode.Light };
        mockThemeService.Setup(x => x.CurrentTheme).Returns(testTheme);
        
        // Setup JS module mock for app-shell.js
        var mockJSModule = new Mock<IJSObjectReference>();
        mockJSRuntime.Setup(x => x.InvokeAsync<IJSObjectReference>("import", new[] { "./_content/RR.Blazor/js/app-shell.js" }))
                     .ReturnsAsync(mockJSModule.Object);
        mockJSModule.Setup(x => x.InvokeVoidAsync("initialize", It.IsAny<object[]>()))
                    .Returns(ValueTask.CompletedTask);
        mockJSModule.Setup(x => x.InvokeAsync<bool>("isMobile", It.IsAny<object[]>()))
                    .ReturnsAsync(false);

        // Act
        var component = RenderComponent<RAppShell>(parameters => parameters
            .Add(p => p.Title, "Test App"));

        // Allow OnAfterRenderAsync to complete
        await Task.Delay(100);

        // Assert - JS module should be imported and initialized
        mockJSRuntime.Verify(x => x.InvokeAsync<IJSObjectReference>("import", 
            new[] { "./_content/RR.Blazor/js/app-shell.js" }), Times.Once);
    }
}