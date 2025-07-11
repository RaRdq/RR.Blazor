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
    private IJSObjectReference jsModule;
    private bool isSystemDark;
    private bool isHighContrast;
    private bool isInitialized;
    
    private const string THEME_KEY = "rr-blazor-theme";
    private const string CUSTOM_THEMES_KEY = "rr-blazor-custom-themes";
    private const string THEME_MODULE_PATH = "./_content/RR.Blazor/js/theme.js";
    
    public event Action<ThemeConfiguration> ThemeChanged;
    public event Action<bool> SystemThemeChanged;
    
    public ThemeConfiguration CurrentTheme => currentTheme;
    public bool IsSystemDark => isSystemDark;
    public bool IsHighContrastActive => isHighContrast;
    
    public BlazorThemeService(
        ILocalStorageService localStorage,
        IJSRuntime jsRuntime,
        ILogger<BlazorThemeService> logger = null)
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
            jsModule = await jsRuntime.InvokeAsync<IJSObjectReference>("import", THEME_MODULE_PATH);
            
            await LoadThemeFromStorageAsync();
            await DetectSystemPreferencesAsync();
            await ApplyThemeAsync(currentTheme);
            
            systemThemeTimer.Start();
            isInitialized = true;
            
            logger?.LogInformation("Theme service initialized with mode: {Mode}", currentTheme.Mode);
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, "Failed to initialize theme service");
            await ResetToDefaultAsync();
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
        if (jsModule == null) return;
        
        try
        {
            var effectiveMode = theme.GetEffectiveMode(isSystemDark);
            var themeData = new
            {
                mode = effectiveMode.ToString().ToLower(),
                colors = GetThemeColors(theme),
                animations = theme.AnimationsEnabled,
                accessibility = theme.AccessibilityMode,
                highContrast = theme.HighContrastMode,
                customVariables = theme.CustomVariables
            };
            
            await jsModule.InvokeVoidAsync("applyTheme", themeData);
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, "Failed to apply theme");
        }
    }
    
    public async Task<bool> GetSystemDarkModeAsync()
    {
        if (jsModule == null) return false;
        
        try
        {
            return await jsModule.InvokeAsync<bool>("getSystemDarkMode");
        }
        catch
        {
            return false;
        }
    }
    
    public async Task<bool> GetSystemHighContrastAsync()
    {
        if (jsModule == null) return false;
        
        try
        {
            return await jsModule.InvokeAsync<bool>("getSystemHighContrast");
        }
        catch
        {
            return false;
        }
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
            logger?.LogError(ex, "Failed to save custom theme: {ThemeName}", theme.Name);
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
            logger?.LogError(ex, "Failed to delete custom theme: {ThemeName}", themeName);
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
            logger?.LogWarning(ex, "Failed to load theme from storage, using default");
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
            logger?.LogError(ex, "Failed to save theme to storage");
        }
    }
    
    private async Task DetectSystemPreferencesAsync()
    {
        isSystemDark = await GetSystemDarkModeAsync();
        isHighContrast = await GetSystemHighContrastAsync();
    }
    
    private async Task CheckSystemThemeAsync()
    {
        try
        {
            var newSystemDark = await GetSystemDarkModeAsync();
            var newHighContrast = await GetSystemHighContrastAsync();
            
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
            logger?.LogDebug(ex, "Failed to check system theme");
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
            
            if (jsModule != null)
            {
                await jsModule.DisposeAsync();
            }
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, "Failed to dispose theme service");
        }
    }
}