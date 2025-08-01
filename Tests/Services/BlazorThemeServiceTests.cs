using RR.Blazor.Services;
using RR.Blazor.Models;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using Blazored.LocalStorage;
using System.Text.Json;

namespace RR.Blazor.Tests.Services;

/// <summary>
/// Unit tests for BlazorThemeService covering JavaScript integration and error handling
/// </summary>
public class BlazorThemeServiceTests
{
    private readonly Mock<ILocalStorageService> mockLocalStorage;
    private readonly Mock<IJSRuntime> mockJSRuntime;
    private readonly Mock<ILogger<BlazorThemeService>> mockLogger;
    private readonly BlazorThemeService themeService;

    public BlazorThemeServiceTests()
    {
        mockLocalStorage = new Mock<ILocalStorageService>();
        mockJSRuntime = new Mock<IJSRuntime>();
        mockLogger = new Mock<ILogger<BlazorThemeService>>();

        themeService = new BlazorThemeService(
            mockLocalStorage.Object,
            mockJSRuntime.Object,
            mockLogger.Object);
    }

    [Fact]
    public async Task InitializeAsync_ShouldDetectSystemPreferences()
    {
        // Arrange
        var themeInfo = new { systemDark = true, highContrast = false };
        mockJSRuntime.Setup(x => x.InvokeAsync<dynamic>("RRTheme.getThemeInfo", It.IsAny<object[]>()))
                     .ReturnsAsync(themeInfo);

        // Act
        await themeService.InitializeAsync();

        // Assert
        themeService.IsSystemDark.Should().BeTrue();
        themeService.IsHighContrastActive.Should().BeFalse();
    }

    [Fact]
    public async Task InitializeAsync_ShouldHandleJSInteropFailure()
    {
        // Arrange
        mockJSRuntime.Setup(x => x.InvokeAsync<dynamic>("RRTheme.getThemeInfo", It.IsAny<object[]>()))
                     .ThrowsAsync(new JSException("JavaScript error"));

        // Act - Should not throw
        await themeService.InitializeAsync();

        // Assert - Should use safe defaults
        themeService.IsSystemDark.Should().BeFalse();
        themeService.IsHighContrastActive.Should().BeFalse();
    }

    [Fact]
    public async Task SetThemeAsync_ShouldApplyThemeViaJavaScript()
    {
        // Arrange
        var testTheme = new ThemeConfiguration 
        { 
            Mode = ThemeMode.Dark,
            AnimationsEnabled = true,
            AccessibilityMode = false
        };

        var capturedThemeData = (object)null;
        mockJSRuntime.Setup(x => x.InvokeVoidAsync("window.RRBlazor.Theme.apply", It.IsAny<object[]>()))
                     .Callback<string, object[]>((method, args) => capturedThemeData = args[0]);

        // Initialize first
        await themeService.InitializeAsync();

        // Act
        await themeService.SetThemeAsync(testTheme);

        // Assert
        mockJSRuntime.Verify(x => x.InvokeVoidAsync("window.RRBlazor.Theme.apply", It.IsAny<object[]>()), 
                           Times.AtLeastOnce());
        capturedThemeData.Should().NotBeNull();
    }

    [Fact]
    public async Task ApplyThemeAsync_ShouldHandleJavaScriptFailure()
    {
        // Arrange
        var testTheme = new ThemeConfiguration { Mode = ThemeMode.Light };
        mockJSRuntime.Setup(x => x.InvokeVoidAsync("window.RRBlazor.Theme.apply", It.IsAny<object[]>()))
                     .ThrowsAsync(new JSException("Theme application failed"));

        await themeService.InitializeAsync();

        // Act - Should not throw
        await themeService.ApplyThemeAsync(testTheme);

        // Assert - Should log error but continue
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Failed to apply theme")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task SetThemeAsync_ShouldPersistToLocalStorage()
    {
        // Arrange
        var testTheme = new ThemeConfiguration 
        { 
            Mode = ThemeMode.Dark, 
            Name = "Test Dark Theme" 
        };

        await themeService.InitializeAsync();

        // Act
        await themeService.SetThemeAsync(testTheme);

        // Assert
        mockLocalStorage.Verify(x => x.SetItemAsync("rr-blazor-theme", testTheme, default), 
                                Times.Once());
    }

    [Fact]
    public async Task SetThemeAsync_ShouldTriggerThemeChangedEvent()
    {
        // Arrange
        var testTheme = new ThemeConfiguration { Mode = ThemeMode.Dark };
        var eventTriggered = false;
        ThemeConfiguration eventTheme = null;

        themeService.ThemeChanged += (theme) => 
        {
            eventTriggered = true;
            eventTheme = theme;
        };

        await themeService.InitializeAsync();

        // Act
        await themeService.SetThemeAsync(testTheme);

        // Assert
        eventTriggered.Should().BeTrue();
        eventTheme.Should().Be(testTheme);
    }

    [Fact]
    public async Task LoadThemeFromStorage_ShouldRestoreSavedTheme()
    {
        // Arrange
        var savedTheme = new ThemeConfiguration 
        { 
            Mode = ThemeMode.Light,
            AnimationsEnabled = false,
            Name = "Saved Light Theme"
        };

        mockLocalStorage.Setup(x => x.GetItemAsync<ThemeConfiguration>("rr-blazor-theme", default))
                        .ReturnsAsync(savedTheme);

        // Act
        await themeService.InitializeAsync();

        // Assert
        themeService.CurrentTheme.Mode.Should().Be(ThemeMode.Light);
        themeService.CurrentTheme.AnimationsEnabled.Should().BeFalse();
        themeService.CurrentTheme.Name.Should().Be("Saved Light Theme");
    }

    [Fact]
    public async Task LoadThemeFromStorage_ShouldHandleCorruptedData()
    {
        // Arrange
        mockLocalStorage.Setup(x => x.GetItemAsync<ThemeConfiguration>("rr-blazor-theme", default))
                        .ThrowsAsync(new JsonException("Invalid JSON"));

        // Act - Should not throw
        await themeService.InitializeAsync();

        // Assert - Should use default theme and clear corrupted storage
        themeService.CurrentTheme.Mode.Should().Be(ThemeMode.System);
        mockLocalStorage.Verify(x => x.RemoveItemAsync("rr-blazor-theme", default), Times.Once());
    }

    [Fact]
    public async Task GetEffectiveMode_ShouldReturnCorrectModeForSystemPreference()
    {
        // Arrange
        var systemTheme = new ThemeConfiguration { Mode = ThemeMode.System };
        await themeService.InitializeAsync();
        await themeService.SetThemeAsync(systemTheme);

        // Act & Assert - System dark = true
        var darkMode = systemTheme.GetEffectiveMode(true);
        darkMode.Should().Be(ThemeMode.Dark);

        // Act & Assert - System dark = false
        var lightMode = systemTheme.GetEffectiveMode(false);
        lightMode.Should().Be(ThemeMode.Light);
    }

    [Fact]
    public async Task SetThemeModeAsync_ShouldUpdateCurrentThemeMode()
    {
        // Arrange
        await themeService.InitializeAsync();

        // Act
        await themeService.SetThemeModeAsync(ThemeMode.Dark);

        // Assert
        themeService.CurrentTheme.Mode.Should().Be(ThemeMode.Dark);
    }

    [Fact]
    public async Task IsDarkModeAsync_ShouldReturnCorrectValue()
    {
        // Arrange
        var themeInfo = new { systemDark = true, highContrast = false };
        mockJSRuntime.Setup(x => x.InvokeAsync<dynamic>("RRTheme.getThemeInfo", It.IsAny<object[]>()))
                     .ReturnsAsync(themeInfo);

        await themeService.InitializeAsync();

        // Test direct dark mode
        await themeService.SetThemeModeAsync(ThemeMode.Dark);
        var isDark = await themeService.IsDarkModeAsync();
        isDark.Should().BeTrue();

        // Test light mode
        await themeService.SetThemeModeAsync(ThemeMode.Light);
        isDark = await themeService.IsDarkModeAsync();
        isDark.Should().BeFalse();

        // Test system mode with dark preference
        await themeService.SetThemeModeAsync(ThemeMode.System);
        isDark = await themeService.IsDarkModeAsync();
        isDark.Should().BeTrue(); // Because system prefers dark
    }

    [Fact]
    public async Task ClearThemeStorageAsync_ShouldResetToDefault()
    {
        // Arrange
        var customTheme = new ThemeConfiguration { Mode = ThemeMode.Dark };
        await themeService.InitializeAsync();
        await themeService.SetThemeAsync(customTheme);

        // Act
        await themeService.ClearThemeStorageAsync();

        // Assert
        themeService.CurrentTheme.Mode.Should().Be(ThemeMode.System);
        mockLocalStorage.Verify(x => x.RemoveItemAsync("rr-blazor-theme", default), Times.Once());
    }

    [Fact]
    public async Task SaveCustomThemeAsync_ShouldPersistCustomTheme()
    {
        // Arrange
        var customTheme = new ThemeConfiguration 
        { 
            Name = "Corporate Blue",
            Mode = ThemeMode.Light,
            PrimaryColor = "#0066cc"
        };

        mockLocalStorage.Setup(x => x.GetItemAsync<List<ThemeConfiguration>>("rr-blazor-custom-themes", default))
                        .ReturnsAsync(new List<ThemeConfiguration>());

        // Act
        await themeService.SaveCustomThemeAsync(customTheme);

        // Assert
        mockLocalStorage.Verify(x => x.SetItemAsync("rr-blazor-custom-themes", 
            It.Is<List<ThemeConfiguration>>(list => list.Contains(customTheme)), default), 
            Times.Once());
    }

    [Fact]
    public async Task GetCustomThemesAsync_ShouldReturnStoredThemes()
    {
        // Arrange
        var customThemes = new List<ThemeConfiguration>
        {
            new() { Name = "Corporate", Mode = ThemeMode.Light },
            new() { Name = "Dark Pro", Mode = ThemeMode.Dark }
        };

        mockLocalStorage.Setup(x => x.GetItemAsync<List<ThemeConfiguration>>("rr-blazor-custom-themes", default))
                        .ReturnsAsync(customThemes);

        // Act
        var result = await themeService.GetCustomThemesAsync();

        // Assert
        result.Should().HaveCount(2);
        result.Should().Contain(t => t.Name == "Corporate");
        result.Should().Contain(t => t.Name == "Dark Pro");
    }

    [Fact]
    public async Task DeleteCustomThemeAsync_ShouldRemoveTheme()
    {
        // Arrange
        var customThemes = new List<ThemeConfiguration>
        {
            new() { Name = "Theme1", Mode = ThemeMode.Light },
            new() { Name = "Theme2", Mode = ThemeMode.Dark }
        };

        mockLocalStorage.Setup(x => x.GetItemAsync<List<ThemeConfiguration>>("rr-blazor-custom-themes", default))
                        .ReturnsAsync(customThemes);

        // Act
        await themeService.DeleteCustomThemeAsync("Theme1");

        // Assert
        mockLocalStorage.Verify(x => x.SetItemAsync("rr-blazor-custom-themes", 
            It.Is<List<ThemeConfiguration>>(list => 
                list.Count == 1 && list.All(t => t.Name != "Theme1")), default), 
            Times.Once());
    }

    [Fact]
    public async Task DisposeAsync_ShouldCleanupResources()
    {
        // Arrange
        await themeService.InitializeAsync();

        // Act
        await themeService.DisposeAsync();

        // Assert - Should dispose without exceptions
        // Subsequent operations should handle disposed state gracefully
        var act = async () => await themeService.SetThemeModeAsync(ThemeMode.Dark);
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task GetThemeColors_ShouldReturnCorrectColorStructure()
    {
        // This test verifies the internal GetThemeColors method produces expected structure
        // Arrange
        var testTheme = new ThemeConfiguration
        {
            PrimaryColor = "#0066cc",
            ErrorColor = "#ff0000",
            BackgroundPrimary = "#ffffff",
            TextPrimary = "#000000"
        };

        await themeService.InitializeAsync();

        object capturedColors = null;
        mockJSRuntime.Setup(x => x.InvokeVoidAsync("window.RRBlazor.Theme.apply", It.IsAny<object[]>()))
                     .Callback<string, object[]>((method, args) => 
                     {
                         var themeData = args[0];
                         // Extract colors from the theme data structure
                         var json = JsonSerializer.Serialize(themeData);
                         var parsed = JsonSerializer.Deserialize<Dictionary<string, object>>(json);
                         capturedColors = parsed.ContainsKey("colors") ? parsed["colors"] : null;
                     });

        // Act
        await themeService.ApplyThemeAsync(testTheme);

        // Assert
        capturedColors.Should().NotBeNull();
        var colorsJson = JsonSerializer.Serialize(capturedColors);
        colorsJson.Should().Contain("primary");
        colorsJson.Should().Contain("error");
        colorsJson.Should().Contain("backgroundPrimary");
        colorsJson.Should().Contain("textPrimary");
    }
}