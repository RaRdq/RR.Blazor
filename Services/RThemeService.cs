using Microsoft.JSInterop;
using Blazored.LocalStorage;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using System;
using RR.Blazor.Models;
using RR.Blazor.Services;

namespace RR.Blazor.Services;

public interface IRThemeService
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
    Task ClearThemeStorageAsync();
}

public class RThemeService : IRThemeService, IAsyncDisposable
{
    private readonly ILocalStorageService localStorage;
    private readonly IJSRuntime jsRuntime;
    private readonly IJavaScriptInteropService jsInterop;
    private readonly ILogger<RThemeService> logger;
    
    private ThemeConfiguration currentTheme = ThemeConfiguration.Default;
    private bool isSystemDark;
    private bool isHighContrast;
    private bool isInitialized;
    private bool isDisposed;
    private DotNetObjectReference<RThemeService> dotNetRef;
    
    private const string THEME_KEY = "rr-blazor-theme";
    private const string CUSTOM_THEMES_KEY = "rr-blazor-custom-themes";
    
    public event Action<ThemeConfiguration> ThemeChanged;
    public event Action<bool> SystemThemeChanged;
    
    public ThemeConfiguration CurrentTheme => currentTheme;
    public bool IsSystemDark => isSystemDark;
    public bool IsHighContrastActive => isHighContrast;
    
    public RThemeService(
        ILocalStorageService localStorage,
        IJSRuntime jsRuntime,
        IJavaScriptInteropService jsInterop,
        ILogger<RThemeService> logger)
    {
        this.localStorage = localStorage;
        this.jsRuntime = jsRuntime;
        this.jsInterop = jsInterop;
        this.logger = logger;
    }
    
    public async Task InitializeAsync()
    {
        if (isInitialized) return;
        isInitialized = true;
        
        try
        {
            await DetectSystemPreferencesAsync();
        }
        catch
        {
            isSystemDark = false;
            isHighContrast = false;
        }
        
        try
        {
            await LoadThemeFromStorageAsync();
            await ApplyThemeAsync(currentTheme);
        }
        catch
        {
            currentTheme = ThemeConfiguration.Default;
        }
        
        await RegisterSystemThemeListenerAsync();
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
            var themeData = new
            {
                mode = effectiveMode.ToString().ToLower(),
                colors = GetThemeColors(theme),
                customVariables = new Dictionary<string, string>(),
                animations = theme.AnimationsEnabled,
                accessibility = theme.AccessibilityMode,
                highContrast = theme.HighContrastMode || isHighContrast
            };
            
            await jsInterop.TryInvokeVoidAsync("window.RRBlazor.Theme.apply", themeData);
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
    
    public virtual async Task<IEnumerable<ThemeConfiguration>> GetPresetThemesAsync()
    {
        return await Task.FromResult(new[]
        {
            ThemeConfiguration.Default
        });
    }
    
    public virtual async Task SaveCustomThemeAsync(ThemeConfiguration theme)
    {
        try
        {
            var customThemes = (await GetCustomThemesAsync()).ToList();
            var existingIndex = customThemes.FindIndex(t => t.Name == theme.Name);
            
            if (existingIndex >= 0)
                customThemes[existingIndex] = theme;
            else
                customThemes.Add(theme);
            
            await SafeLocalStorageSetAsync(CUSTOM_THEMES_KEY, customThemes);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to save custom theme: {ThemeName}", theme.Name);
        }
    }
    
    public async Task<IEnumerable<ThemeConfiguration>> GetCustomThemesAsync()
    {
        var themes = await SafeLocalStorageGetAsync<List<ThemeConfiguration>>(CUSTOM_THEMES_KEY);
        return themes ?? new List<ThemeConfiguration>();
    }
    
    public async Task DeleteCustomThemeAsync(string themeName)
    {
        try
        {
            var customThemes = (await GetCustomThemesAsync()).ToList();
            customThemes.RemoveAll(t => t.Name == themeName);
            await SafeLocalStorageSetAsync(CUSTOM_THEMES_KEY, customThemes);
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
    
    public async Task ClearThemeStorageAsync()
    {
        try
        {
            await SafeLocalStorageRemoveAsync(THEME_KEY);
            currentTheme = ThemeConfiguration.Default;
            await ApplyThemeAsync(currentTheme);
            ThemeChanged?.Invoke(currentTheme);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to clear theme storage");
        }
    }
    
    private async Task LoadThemeFromStorageAsync()
    {
        var savedTheme = await SafeLocalStorageGetAsync<ThemeConfiguration>(THEME_KEY);
        currentTheme = savedTheme ?? ThemeConfiguration.Default;
    }
    
    private async Task SaveThemeToStorageAsync()
    {
        await SafeLocalStorageSetAsync(THEME_KEY, currentTheme);
    }
    
    private async Task DetectSystemPreferencesAsync()
    {
        var themeInfo = await jsInterop.TryInvokeAsync<dynamic>("window.RRBlazor.Theme.getThemeInfo");
        
        isSystemDark = Convert.ToBoolean(themeInfo?.systemDark ?? false);
        isHighContrast = Convert.ToBoolean(themeInfo?.highContrast ?? false);
    }

    private async Task<T> SafeLocalStorageGetAsync<T>(string key)
    {
        if (!await jsInterop.IsInteractiveAsync())
            return default(T);
        
        try
        {
            return await localStorage.GetItemAsync<T>(key);
        }
        catch (JSException)
        {
            return default(T);
        }
    }

    private async Task<bool> SafeLocalStorageSetAsync<T>(string key, T value)
    {
        if (!await jsInterop.IsInteractiveAsync())
            return false;
        
        try
        {
            await localStorage.SetItemAsync(key, value);
            return true;
        }
        catch (JSException)
        {
            return false;
        }
    }

    private async Task<bool> SafeLocalStorageRemoveAsync(string key)
    {
        if (!await jsInterop.IsInteractiveAsync())
            return false;
        
        try
        {
            await localStorage.RemoveItemAsync(key);
            return true;
        }
        catch (JSException)
        {
            return false;
        }
    }
    
    private async Task RegisterSystemThemeListenerAsync()
    {
        if (!await jsInterop.IsInteractiveAsync()) return;
        
        dotNetRef = DotNetObjectReference.Create(this);
        await jsInterop.TryInvokeVoidAsync("window.RRBlazor.Theme.registerSystemThemeListener", dotNetRef);
    }

    [JSInvokable]
    public async Task OnSystemThemeChanged(bool isDark)
    {
        try
        {
            if (isDark != isSystemDark)
            {
                isSystemDark = isDark;
                SystemThemeChanged?.Invoke(isSystemDark);
                
                if (currentTheme.Mode == ThemeMode.System)
                    await ApplyThemeAsync(currentTheme);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to handle system theme change");
        }
    }
    
    [JSInvokable]
    public async Task OnSystemContrastChanged(bool isHighContrastMode)
    {
        try
        {
            if (isHighContrastMode == isHighContrast) return;

            isHighContrast = isHighContrastMode;

            if (currentTheme.AccessibilityMode)
                await ApplyThemeAsync(currentTheme);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to handle system contrast change");
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
            
            if (dotNetRef != null)
            {
                await jsInterop.TryInvokeVoidAsync("window.RRBlazor.Theme.disposeSystemThemeListener");
                dotNetRef.Dispose();
                dotNetRef = null;
            }
        }
        catch (ObjectDisposedException) { }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to dispose theme service");
        }
    }
}