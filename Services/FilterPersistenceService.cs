using Blazored.LocalStorage;
using RR.Blazor.Interfaces;
using RR.Blazor.Models;

namespace RR.Blazor.Services;

public class FilterPersistenceService : IFilterPersistenceService
{
    private readonly ILocalStorageService _localStorage;
    private const string FilterPrefix = "rr_filter_";
    private const string ConfigPrefix = "rr_filter_config_";
    
    public bool IsEnabled { get; set; } = false; // Off by default per requirements
    
    public FilterPersistenceService(ILocalStorageService localStorage)
    {
        _localStorage = localStorage;
    }
    
    public async Task SaveFilterAsync(string key, FilterConfiguration config)
    {
        if (!IsEnabled) return;
        await _localStorage.SetItemAsync($"{ConfigPrefix}{key}", config);
    }
    
    public async Task SaveConfigurationAsync(string key, FilterConfiguration config)
    {
        await SaveFilterAsync(key, config);
    }
    
    public async Task<FilterConfiguration?> LoadFilterAsync(string key)
    {
        if (!IsEnabled) return null;
        return await _localStorage.GetItemAsync<FilterConfiguration>($"{ConfigPrefix}{key}");
    }
    
    public async Task<GridFilterState?> LoadFilterStateAsync(string key)
    {
        if (!IsEnabled) return null;
        return await _localStorage.GetItemAsync<GridFilterState>($"{FilterPrefix}{key}");
    }
    
    public async Task SaveFilterStateAsync(string key, GridFilterState state)
    {
        if (!IsEnabled) return;
        await _localStorage.SetItemAsync($"{FilterPrefix}{key}", state);
    }
    
    public async Task<List<FilterConfiguration>> GetSavedFiltersAsync(string componentKey)
    {
        if (!IsEnabled) return new List<FilterConfiguration>();
        
        var configs = new List<FilterConfiguration>();
        var keys = await _localStorage.KeysAsync();
        var prefix = $"{ConfigPrefix}{componentKey}_";
        
        foreach (var key in keys)
        {
            if (key.StartsWith(prefix))
            {
                var config = await _localStorage.GetItemAsync<FilterConfiguration>(key);
                if (config != null)
                    configs.Add(config);
            }
        }
        
        return configs;
    }
    
    public async Task<List<FilterConfiguration>> GetConfigurationsAsync(string componentKey)
    {
        return await GetSavedFiltersAsync(componentKey);
    }
    
    public async Task DeleteFilterAsync(string key)
    {
        if (!IsEnabled) return;
        await _localStorage.RemoveItemAsync($"{ConfigPrefix}{key}");
        await _localStorage.RemoveItemAsync($"{FilterPrefix}{key}");
    }
}