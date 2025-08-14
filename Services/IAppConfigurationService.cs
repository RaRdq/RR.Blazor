using RR.Blazor.Models;
using Blazored.LocalStorage;

namespace RR.Blazor.Services;

/// <summary>
/// App configuration service interface
/// </summary>
public interface IAppConfigurationService
{
    event Action<AppConfiguration> ConfigurationChanged;
    
    AppConfiguration Current { get; }
    
    Task LoadAsync();
    Task SaveAsync(AppConfiguration configuration);
    Task UpdateAsync(Action<AppConfiguration> updateAction);
    Task ResetToDefaultAsync();
    T GetCustomSetting<T>(string key, T defaultValue = default);
    Task SetCustomSettingAsync<T>(string key, T value);
}

/// <summary>
/// Default implementation using local storage
/// </summary>
public class AppConfigurationService(ILocalStorageService localStorage) : IAppConfigurationService
{
    private const string CONFIG_KEY = "rr-app-configuration";
    
    public event Action<AppConfiguration> ConfigurationChanged;
    
    public AppConfiguration Current { get; private set; } = new();

    public async Task LoadAsync()
    {
        try
        {
            var saved = await localStorage.GetItemAsync<AppConfiguration>(CONFIG_KEY);
            if (saved != null)
            {
                Current = saved;
            }
        }
        catch
        {
            // Use default configuration if loading fails
            Current = new AppConfiguration();
        }
        
        ConfigurationChanged?.Invoke(Current);
    }
    
    public async Task SaveAsync(AppConfiguration configuration)
    {
        Current = configuration;
        await localStorage.SetItemAsync(CONFIG_KEY, configuration);
        ConfigurationChanged?.Invoke(Current);
    }
    
    public async Task UpdateAsync(Action<AppConfiguration> updateAction)
    {
        updateAction(Current);
        await SaveAsync(Current);
    }
    
    public async Task ResetToDefaultAsync()
    {
        await SaveAsync(new AppConfiguration());
    }
    
    public T GetCustomSetting<T>(string key, T defaultValue = default)
    {
        if (Current.CustomSettings.TryGetValue(key, out var value))
        {
            try
            {
                if (value is T typedValue)
                    return typedValue;
                
                // Try to convert
                return (T)Convert.ChangeType(value, typeof(T));
            }
            catch
            {
                return defaultValue;
            }
        }
        
        return defaultValue;
    }
    
    public async Task SetCustomSettingAsync<T>(string key, T value)
    {
        Current.CustomSettings[key] = value!;
        await SaveAsync(Current);
    }
}