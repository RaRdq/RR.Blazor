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
        
        try
        {
            logger.LogInformation("Initializing theme service...");
            
            await DetectSystemPreferencesAsync();
            await LoadThemeFromStorageAsync();
            await ApplyThemeAsync(currentTheme);
            
            systemThemeTimer.Start();
            isInitialized = true;
            logger.LogInformation("Theme service initialized successfully with mode: {Mode}", currentTheme.Mode);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to initialize theme service");
            isInitialized = true; // Mark as initialized to prevent retry loops
            currentTheme = ThemeConfiguration.Default;
            await ApplyThemeAsync(currentTheme);
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
        logger.LogInformation("Setting theme mode to: {Mode}", mode);
        currentTheme.Mode = mode;
        await SetThemeAsync(currentTheme);
        logger.LogInformation("Theme mode set successfully to: {Mode}", mode);
    }
    
    public async Task ApplyThemeAsync(ThemeConfiguration theme)
    {
        try
        {
            var effectiveMode = theme.GetEffectiveMode(isSystemDark);
            logger.LogInformation("Applying theme mode: {ThemeMode} (effective: {EffectiveMode}, systemDark: {IsSystemDark})", theme.Mode, effectiveMode, isSystemDark);
            
            await jsRuntime.InvokeVoidAsync("eval", $"document.documentElement.setAttribute('data-theme', '{effectiveMode.ToString().ToLower()}')");
            logger.LogInformation("Theme applied: {EffectiveMode}", effectiveMode);
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
            var preferences = await jsRuntime.InvokeAsync<bool[]>("eval", "[window.matchMedia && window.matchMedia('(prefers-color-scheme: dark)').matches, window.matchMedia && window.matchMedia('(prefers-contrast: more)').matches]");
            isSystemDark = preferences[0];
            isHighContrast = preferences[1];
        }
        catch
        {
            isSystemDark = false;
            isHighContrast = false;
        }
    }
    
    private async Task CheckSystemThemeAsync()
    {
        try
        {
            var preferences = await jsRuntime.InvokeAsync<bool[]>("eval", "[window.matchMedia && window.matchMedia('(prefers-color-scheme: dark)').matches, window.matchMedia && window.matchMedia('(prefers-contrast: more)').matches]");
            var newSystemDark = preferences[0];
            var newHighContrast = preferences[1];
            
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
            systemThemeTimer?.Stop();
            systemThemeTimer?.Dispose();
            
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to dispose theme service");
        }
    }
}