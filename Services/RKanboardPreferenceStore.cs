using System;
using System.Threading;
using System.Threading.Tasks;
using Blazored.LocalStorage;
using RR.Blazor.Models;

namespace RR.Blazor.Services;

public interface IRKanboardPreferenceStore
{
    Task<RKanboardUserPreferences?> GetAsync(string boardKey, CancellationToken ct = default);
    Task SaveAsync(RKanboardUserPreferences preferences, CancellationToken ct = default);
    Task ClearAsync(string boardKey, CancellationToken ct = default);
}

public class RKanboardPreferenceStore : IRKanboardPreferenceStore
{
    private readonly ILocalStorageService localStorage;
    private const string StoragePrefix = "rr:rkanboard:pref:";

    public RKanboardPreferenceStore(ILocalStorageService localStorage)
    {
        this.localStorage = localStorage ?? throw new ArgumentNullException(nameof(localStorage));
    }

    public async Task<RKanboardUserPreferences?> GetAsync(string boardKey, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(boardKey))
        {
            throw new ArgumentException("Board key is required", nameof(boardKey));
        }

        return await localStorage.GetItemAsync<RKanboardUserPreferences>(BuildKey(boardKey), ct);
    }

    public async Task SaveAsync(RKanboardUserPreferences preferences, CancellationToken ct = default)
    {
        if (preferences is null) throw new ArgumentNullException(nameof(preferences));
        if (string.IsNullOrWhiteSpace(preferences.BoardKey))
        {
            throw new ArgumentException("Board key is required", nameof(preferences));
        }

        await localStorage.SetItemAsync(BuildKey(preferences.BoardKey), preferences, ct);
    }

    public async Task ClearAsync(string boardKey, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(boardKey))
        {
            throw new ArgumentException("Board key is required", nameof(boardKey));
        }

        await localStorage.RemoveItemAsync(BuildKey(boardKey), ct);
    }

    private static string BuildKey(string boardKey) => $"{StoragePrefix}{boardKey}";
}
