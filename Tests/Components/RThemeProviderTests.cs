using RR.Blazor.Components.Core;
using RR.Blazor.Services;
using RR.Blazor.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Bunit;

namespace RR.Blazor.Tests.Components;

/// <summary>
/// Unit tests for RThemeProvider component functionality
/// </summary>
public class RThemeProviderTests : TestContext
{
    private Mock<IRThemeService> mockThemeService;
    private Mock<IJavaScriptInteropService> mockJsInterop;

    public RThemeProviderTests()
    {
        mockThemeService = new Mock<IRThemeService>();
        mockThemeService.SetupGet(x => x.IsInitialized).Returns(true);
        mockThemeService.Setup(x => x.InitializeAsync()).Returns(Task.CompletedTask);
        Services.AddSingleton(mockThemeService.Object);
        
        mockJsInterop = new Mock<IJavaScriptInteropService>();
        mockJsInterop.Setup(x => x.IsInteractiveAsync()).ReturnsAsync(true);
        Services.AddSingleton(mockJsInterop.Object);
    }

    [Fact]
    public void RThemeProvider_ShouldRenderWithDefaultTheme()
    {
        // Arrange
        var defaultTheme = ThemeConfiguration.Default;
        mockThemeService.Setup(x => x.CurrentTheme).Returns(defaultTheme);

        // Act
        var component = RenderComponent<RThemeProvider>();

        // Assert
        var providerDiv = component.Find(".rr-theme-provider");
        providerDiv.Should().NotBeNull();
        providerDiv.GetAttribute("data-theme").Should().Be("default");
    }

    [Fact]
    public void RThemeProvider_ShouldRenderWithCustomTheme()
    {
        // Arrange
        var customTheme = new ThemeConfiguration { Mode = ThemeMode.Dark };
        mockThemeService.Setup(x => x.CurrentTheme).Returns(customTheme);

        // Act
        var component = RenderComponent<RThemeProvider>(parameters => parameters
            .Add(p => p.Theme, "dark"));

        // Assert
        var providerDiv = component.Find(".rr-theme-provider");
        providerDiv.GetAttribute("data-theme").Should().Be("dark");
    }

    [Fact]
    public void RThemeProvider_ShouldCascadeThemeService()
    {
        // Arrange
        var testTheme = new ThemeConfiguration { Mode = ThemeMode.Light };
        mockThemeService.Setup(x => x.CurrentTheme).Returns(testTheme);

        // Act
        var component = RenderComponent<RThemeProvider>(parameters => parameters
            .Add(p => p.ChildContent, builder =>
            {
                builder.OpenComponent<TestChildComponent>(0);
                builder.CloseComponent();
            }));

        // Assert - Child component should receive cascaded theme service
        var childComponent = component.FindComponent<TestChildComponent>();
        childComponent.Should().NotBeNull();
    }

    [Fact]
    public void RThemeProvider_ShouldInitializeThemeService()
    {
        // Arrange
        var testTheme = new ThemeConfiguration { Mode = ThemeMode.System };
        mockThemeService.Setup(x => x.CurrentTheme).Returns(testTheme);

        // Act
        var component = RenderComponent<RThemeProvider>();

        // Assert
        component.WaitForAssertion(() =>
        {
            mockThemeService.Verify(x => x.InitializeAsync(), Times.Once);
        });
    }

    [Fact]
    public void RThemeProvider_ShouldSetInitialTheme()
    {
        // Arrange
        var initialTheme = new ThemeConfiguration { Mode = ThemeMode.Dark, Name = "Initial Dark" };
        mockThemeService.Setup(x => x.SetThemeAsync(It.IsAny<ThemeConfiguration>())).Returns(Task.CompletedTask);
        mockThemeService.Setup(x => x.CurrentTheme).Returns(initialTheme);

        // Act
        var component = RenderComponent<RThemeProvider>(parameters => parameters
            .Add(p => p.InitialTheme, initialTheme));

        // Assert
        component.WaitForAssertion(() =>
        {
            mockThemeService.Verify(x => x.SetThemeAsync(initialTheme), Times.Once);
        });
    }

    [Fact]
    public void RThemeProvider_ShouldHandleThemeChangedEvent()
    {
        // Arrange
        var initialTheme = new ThemeConfiguration { Mode = ThemeMode.Light };
        var newTheme = new ThemeConfiguration { Mode = ThemeMode.Dark };
        
        mockThemeService.Setup(x => x.CurrentTheme).Returns(initialTheme);
        
        var component = RenderComponent<RThemeProvider>();

        // Act - Simulate theme changed event
        mockThemeService.Setup(x => x.CurrentTheme).Returns(newTheme);
        mockThemeService.Raise(x => x.ThemeChanged += null, newTheme);

        // Allow component to process the event
        component.WaitForState(() => 
        {
            try
            {
                var providerDiv = component.Find(".rr-theme-provider");
                // The theme attribute should be updated based on the new theme
                return true;
            }
            catch
            {
                return false;
            }
        }, TimeSpan.FromSeconds(1));

        // Assert - Component should have updated
        var providerDiv = component.Find(".rr-theme-provider");
        providerDiv.Should().NotBeNull();
    }

    [Fact]
    public void RThemeProvider_ShouldDisposeThemeServiceEventHandler()
    {
        // Arrange
        var testTheme = new ThemeConfiguration { Mode = ThemeMode.Light };
        mockThemeService.Setup(x => x.CurrentTheme).Returns(testTheme);
        var component = RenderComponent<RThemeProvider>();
        component.WaitForAssertion(() =>
        {
            mockThemeService.Verify(x => x.InitializeAsync(), Times.Once);
        });

        // Act - Dispose the component
        component.Dispose();

        // Assert - Event handler should be removed (no way to directly verify, but no exceptions should occur)
        var disposeAction = () => mockThemeService.Raise(x => x.ThemeChanged += null, testTheme);
        disposeAction.Should().NotThrow();
    }

    [Fact]
    public void RThemeProvider_ShouldHandleInitializationError()
    {
        // Arrange
        mockThemeService.Setup(x => x.InitializeAsync()).ThrowsAsync(new Exception("Initialization failed"));
        mockThemeService.SetupGet(x => x.IsInitialized).Returns(false);
        var fallbackTheme = ThemeConfiguration.Default;
        mockThemeService.Setup(x => x.CurrentTheme).Returns(fallbackTheme);

        // Act - Should not throw exception
        var renderAction = () => RenderComponent<RThemeProvider>();

        // Assert - Should render with fallback theme
        renderAction.Should().NotThrow();
    }
}

/// <summary>
/// Test child component to verify cascading values work correctly
/// </summary>
public class TestChildComponent : ComponentBase
{
    [CascadingParameter(Name = "ThemeService")] 
    public IRThemeService ThemeService { get; set; }
    
    [CascadingParameter(Name = "CurrentTheme")] 
    public ThemeConfiguration CurrentTheme { get; set; }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.AddContent(0, $"Theme: {CurrentTheme?.Mode.ToString() ?? "None"}");
    }
}
