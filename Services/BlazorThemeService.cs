using Microsoft.JSInterop;
using Blazored.LocalStorage;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using System;
using RR.Blazor.Models;

namespace RR.Blazor.Services;

/// <summary>
/// Theme service interface for RR.Blazor
/// </summary>
public interface IThemeService
{
    event Action<ThemeConfiguration> ThemeChanged;
    event Action<bool> SystemThemeChanged;
    
    ThemeConfiguration CurrentTheme { get; }
    bool IsSystemDark { get; }
    bool IsHighContrastActive { get; }
    
    Task InitializeAsync();
    Task SetThemeAsync(ThemeConfiguration theme);
    Task SetThemeModeAsync(ThemeMode mode);
    Task ApplyThemeAsync(ThemeConfiguration theme);
    Task<bool> GetSystemDarkModeAsync();
    Task<bool> GetSystemHighContrastAsync();
    Task ResetToDefaultAsync();
    Task<IEnumerable<ThemeConfiguration>> GetPresetThemesAsync();
    Task SaveCustomThemeAsync(ThemeConfiguration theme);
    Task<IEnumerable<ThemeConfiguration>> GetCustomThemesAsync();
    Task DeleteCustomThemeAsync(string themeName);
    Task<bool> IsDarkModeAsync();
    Task<ThemeMode> GetThemeModeAsync();
}

/// <summary>
/// Production-ready theme service for RR.Blazor design system
/// </summary>
public class BlazorThemeService : IThemeService, IAsyncDisposable
{
    private readonly ILocalStorageService localStorage;
    private readonly IJSRuntime jsRuntime;
    private readonly ILogger<BlazorThemeService> logger;
    
    private ThemeConfiguration currentTheme = ThemeConfiguration.Default;
    private readonly System.Timers.Timer systemThemeTimer;
    private bool isSystemDark;
    private bool isHighContrast;
    private bool isInitialized;
    private bool isDisposed;
    
    private const string THEME_KEY = "rr-blazor-theme";
    private const string CUSTOM_THEMES_KEY = "rr-blazor-custom-themes";
    
    public event Action<ThemeConfiguration> ThemeChanged;
    public event Action<bool> SystemThemeChanged;
    
    public ThemeConfiguration CurrentTheme => currentTheme;
    public bool IsSystemDark => isSystemDark;
    public bool IsHighContrastActive => isHighContrast;
    
    public BlazorThemeService(
        ILocalStorageService localStorage,
        IJSRuntime jsRuntime,
        ILogger<BlazorThemeService> logger)
    {
        this.localStorage = localStorage;
        this.jsRuntime = jsRuntime;
        this.logger = logger;
        
        systemThemeTimer = new System.Timers.Timer(5000); // Check every 5 seconds
        systemThemeTimer.Elapsed += async (sender, e) => await CheckSystemThemeAsync();
        systemThemeTimer.AutoReset = true;
    }
    
    public async Task InitializeAsync()
    {
        if (isInitialized) return;
        isInitialized = true;
        
        try
        {
            // Theme is already applied by JavaScript, just sync state
            try
            {
                var themeInfo = await jsRuntime.InvokeAsync<dynamic>("getSystemTheme");
                if (themeInfo != null)
                {
                    isSystemDark = Convert.ToBoolean(themeInfo.systemDark ?? false);
                    isHighContrast = Convert.ToBoolean(themeInfo.highContrast ?? false);
                    
                    // Get current theme from DOM
                    var currentThemeMode = themeInfo.current?.ToString() ?? "light";
                    if (Enum.TryParse<ThemeMode>(currentThemeMode, true, out ThemeMode parsedMode))
                    {
                        currentTheme.Mode = parsedMode;
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogDebug(ex, "Failed to get theme info from JavaScript, using defaults");
            }
            
            // Load saved configuration directly
            try
            {
                await LoadThemeFromStorageAsync();
            }
            catch (Exception ex)
            {
                logger.LogDebug(ex, "Theme load failed");
            }
            
            // Start system theme monitoring
            // DISABLED TEMPORARILY - may be causing infinite loops
            // systemThemeTimer.Start();
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to initialize theme service, using defaults");
            currentTheme = ThemeConfiguration.Default;
        }
    }
    
    public async Task SetThemeAsync(ThemeConfiguration theme)
    {
        currentTheme = theme;
        await SaveThemeToStorageAsync();
        await ApplyThemeAsync(theme);
        ThemeChanged?.Invoke(theme);
    }
    
    public async Task SetThemeModeAsync(ThemeMode mode)
    {
        currentTheme.Mode = mode;
        await SetThemeAsync(currentTheme);
    }
    
    public async Task ApplyThemeAsync(ThemeConfiguration theme)
    {
        try
        {
            var effectiveMode = theme.GetEffectiveMode(isSystemDark);
            // Use proper JS interop method only
            await jsRuntime.InvokeVoidAsync("setTheme", effectiveMode.ToString().ToLower());
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to apply theme");
        }
    }
    
    public async Task<bool> GetSystemDarkModeAsync()
    {
        return isSystemDark;
    }
    
    public async Task<bool> GetSystemHighContrastAsync()
    {
        return isHighContrast;
    }
    
    public async Task ResetToDefaultAsync()
    {
        await SetThemeAsync(ThemeConfiguration.Default);
    }
    
    public async Task<IEnumerable<ThemeConfiguration>> GetPresetThemesAsync()
    {
        return await Task.FromResult(new[]
        {
            ThemeConfiguration.Default,
            // Add other preset themes here
        });
    }
    
    public async Task SaveCustomThemeAsync(ThemeConfiguration theme)
    {
        try
        {
            var customThemes = (await GetCustomThemesAsync()).ToList();
            var existingIndex = customThemes.FindIndex(t => t.Name == theme.Name);
            
            if (existingIndex >= 0)
                customThemes[existingIndex] = theme;
            else
                customThemes.Add(theme);
            
            await localStorage.SetItemAsync(CUSTOM_THEMES_KEY, customThemes);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to save custom theme: {ThemeName}", theme.Name);
        }
    }
    
    public async Task<IEnumerable<ThemeConfiguration>> GetCustomThemesAsync()
    {
        try
        {
            var themes = await localStorage.GetItemAsync<List<ThemeConfiguration>>(CUSTOM_THEMES_KEY);
            return themes ?? new List<ThemeConfiguration>();
        }
        catch
        {
            return new List<ThemeConfiguration>();
        }
    }
    
    public async Task DeleteCustomThemeAsync(string themeName)
    {
        try
        {
            var customThemes = (await GetCustomThemesAsync()).ToList();
            customThemes.RemoveAll(t => t.Name == themeName);
            await localStorage.SetItemAsync(CUSTOM_THEMES_KEY, customThemes);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to delete custom theme: {ThemeName}", themeName);
        }
    }
    
    public async Task<bool> IsDarkModeAsync()
    {
        var effectiveMode = currentTheme.GetEffectiveMode(isSystemDark);
        return effectiveMode == ThemeMode.Dark;
    }
    
    public async Task<ThemeMode> GetThemeModeAsync()
    {
        return currentTheme.Mode;
    }
    
    private async Task LoadThemeFromStorageAsync()
    {
        try
        {
            var savedTheme = await localStorage.GetItemAsync<ThemeConfiguration>(THEME_KEY);
            if (savedTheme != null)
            {
                currentTheme = savedTheme;
            }
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to load theme from storage, using default");
            currentTheme = ThemeConfiguration.Default;
        }
    }
    
    private async Task SaveThemeToStorageAsync()
    {
        try
        {
            await localStorage.SetItemAsync(THEME_KEY, currentTheme);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to save theme to storage");
        }
    }
    
    private async Task DetectSystemPreferencesAsync()
    {
        try
        {
            var themeInfo = await jsRuntime.InvokeAsync<dynamic>("getSystemTheme");
            if (themeInfo != null)
            {
                isSystemDark = Convert.ToBoolean(themeInfo.systemDark ?? false);
                isHighContrast = Convert.ToBoolean(themeInfo.highContrast ?? false);
            }
        }
        catch (Exception ex)
        {
            logger.LogDebug(ex, "Failed to detect system preferences");
            isSystemDark = false;
            isHighContrast = false;
        }
    }
    
    private async Task CheckSystemThemeAsync()
    {
        try
        {
            // Skip if not initialized or disposed
            if (!isInitialized || isDisposed || jsRuntime == null) return;
            
            var themeInfo = await jsRuntime.InvokeAsync<dynamic>("getSystemTheme");
            if (themeInfo == null) return;
            
            var newSystemDark = Convert.ToBoolean(themeInfo.systemDark ?? false);
            var newHighContrast = Convert.ToBoolean(themeInfo.highContrast ?? false);
            
            if (newSystemDark != isSystemDark)
            {
                isSystemDark = newSystemDark;
                SystemThemeChanged?.Invoke(isSystemDark);
                
                if (currentTheme.Mode == ThemeMode.System)
                {
                    await ApplyThemeAsync(currentTheme);
                }
            }
            
            if (newHighContrast != isHighContrast)
            {
                isHighContrast = newHighContrast;
                if (currentTheme.AccessibilityMode)
                {
                    await ApplyThemeAsync(currentTheme);
                }
            }
        }
        catch (Exception ex)
        {
            logger.LogDebug(ex, "Failed to check system theme");
        }
    }
    
    private object GetThemeColors(ThemeConfiguration theme)
    {
        return new
        {
            primary = theme.PrimaryColor,
            secondary = theme.SecondaryColor,
            error = theme.ErrorColor,
            warning = theme.WarningColor,
            info = theme.InfoColor,
            success = theme.SuccessColor,
            backgroundPrimary = theme.BackgroundPrimary,
            backgroundSecondary = theme.BackgroundSecondary,
            backgroundElevated = theme.BackgroundElevated,
            textPrimary = theme.TextPrimary,
            textSecondary = theme.TextSecondary,
            textTertiary = theme.TextTertiary
        };
    }
    
    public async ValueTask DisposeAsync()
    {
        try
        {
            isDisposed = true;
            systemThemeTimer?.Stop();
            systemThemeTimer?.Dispose();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to dispose theme service");
        }
    }
}