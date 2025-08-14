using RR.Blazor.Models;

namespace RR.Blazor.Services;

/// <summary>
/// Search result item
/// </summary>
public class AppSearchResult
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; }
    public string Url { get; set; }
    public string Icon { get; set; }
    public string Category { get; set; } = "General";
    public float Relevance { get; set; } = 1.0f;
    public Dictionary<string, object> Metadata { get; set; } = new();
    public Func<Task> OnClick { get; set; }
}

/// <summary>
/// Search service interface for RAppShell global search
/// </summary>
public interface IAppSearchService
{
    event Action<List<AppSearchResult>> SearchResultsChanged;
    
    Task<List<AppSearchResult>> SearchAsync(string query, CancellationToken cancellationToken = default);
    Task<List<AppSearchResult>> GetRecentSearchesAsync();
    Task<List<AppSearchResult>> GetPopularItemsAsync();
    Task AddToRecentAsync(AppSearchResult result);
    Task ClearRecentAsync();
    void RegisterSearchProvider(ISearchProvider provider);
    void UnregisterSearchProvider(ISearchProvider provider);
}

/// <summary>
/// Search provider interface for extending search functionality
/// </summary>
public interface ISearchProvider
{
    string Name { get; }
    int Priority { get; }
    Task<List<AppSearchResult>> SearchAsync(string query, CancellationToken cancellationToken = default);
}

/// <summary>
/// Default implementation of app search service
/// </summary>
public class AppSearchService : IAppSearchService
{
    private readonly List<ISearchProvider> providers = new();
    private readonly List<AppSearchResult> recentSearches = new();
    private const int MaxRecentSearches = 10;
    
    public event Action<List<AppSearchResult>> SearchResultsChanged;
    
    public async Task<List<AppSearchResult>> SearchAsync(string query, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(query))
            return await GetPopularItemsAsync();
        
        var allResults = new List<AppSearchResult>();
        
        // Get results from all providers
        var tasks = providers.OrderByDescending(p => p.Priority)
                            .Select(p => p.SearchAsync(query, cancellationToken));
        
        var results = await Task.WhenAll(tasks);
        
        foreach (var providerResults in results)
        {
            allResults.AddRange(providerResults);
        }
        
        // Sort by relevance and category
        var sortedResults = allResults
            .OrderByDescending(r => r.Relevance)
            .ThenBy(r => r.Category)
            .ThenBy(r => r.Title)
            .Take(50)
            .ToList();
        
        SearchResultsChanged?.Invoke(sortedResults);
        return sortedResults;
    }
    
    public Task<List<AppSearchResult>> GetRecentSearchesAsync()
    {
        return Task.FromResult(recentSearches.ToList());
    }
    
    public async Task<List<AppSearchResult>> GetPopularItemsAsync()
    {
        // Return recent searches or default popular items
        if (recentSearches.Any())
            return recentSearches.Take(5).ToList();
        
        // Default popular items
        return new List<AppSearchResult>
        {
            new() { Title = "Dashboard", Icon = "dashboard", Url = "/", Category = "Navigation" },
            new() { Title = "Settings", Icon = "settings", Url = "/settings", Category = "Navigation" },
            new() { Title = "Profile", Icon = "person", Url = "/profile", Category = "Account" }
        };
    }
    
    public Task AddToRecentAsync(AppSearchResult result)
    {
        // Remove if already exists
        recentSearches.RemoveAll(r => r.Id == result.Id || r.Title == result.Title);
        
        // Add to beginning
        recentSearches.Insert(0, result);
        
        // Limit size
        while (recentSearches.Count > MaxRecentSearches)
        {
            recentSearches.RemoveAt(recentSearches.Count - 1);
        }
        
        return Task.CompletedTask;
    }
    
    public Task ClearRecentAsync()
    {
        recentSearches.Clear();
        return Task.CompletedTask;
    }
    
    public void RegisterSearchProvider(ISearchProvider provider)
    {
        if (providers.All(p => p.Name != provider.Name))
        {
            providers.Add(provider);
        }
    }
    
    public void UnregisterSearchProvider(ISearchProvider provider)
    {
        providers.RemoveAll(p => p.Name == provider.Name);
    }
}

/// <summary>
/// Navigation search provider - searches through nav items
/// </summary>
public class NavigationSearchProvider(List<AppNavItem> navItems) : ISearchProvider
{
    public string Name => "Navigation";
    public int Priority => 10;

    public Task<List<AppSearchResult>> SearchAsync(string query, CancellationToken cancellationToken = default)
    {
        var results = new List<AppSearchResult>();
        var searchLower = query.ToLowerInvariant();
        
        foreach (var item in navItems.Where(i => i.Visible))
        {
            var relevance = CalculateRelevance(item, searchLower);
            if (relevance > 0)
            {
                results.Add(new AppSearchResult
                {
                    Id = item.Id,
                    Title = item.Text,
                    Url = item.Href,
                    Icon = item.Icon,
                    Category = "Navigation",
                    Relevance = relevance
                });
            }
        }
        
        return Task.FromResult(results);
    }
    
    private float CalculateRelevance(AppNavItem item, string searchLower)
    {
        var titleLower = item.Text.ToLowerInvariant();
        
        // Exact match
        if (titleLower == searchLower) return 1.0f;
        
        // Starts with
        if (titleLower.StartsWith(searchLower)) return 0.9f;
        
        // Contains
        if (titleLower.Contains(searchLower)) return 0.7f;
        
        // Keywords match
        if (item.SearchKeywords != null)
        {
            foreach (var keyword in item.SearchKeywords)
            {
                var keywordLower = keyword.ToLowerInvariant();
                if (keywordLower == searchLower) return 0.8f;
                if (keywordLower.StartsWith(searchLower)) return 0.6f;
                if (keywordLower.Contains(searchLower)) return 0.4f;
            }
        }
        
        return 0f;
    }
}