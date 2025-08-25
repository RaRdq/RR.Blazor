using Microsoft.AspNetCore.Components;
using RR.Blazor.Models;
using System.Linq.Expressions;

namespace RR.Blazor.Interfaces;

/// <summary>
/// Interface for components that can be filtered
/// </summary>
public interface IFilterable
{
    /// <summary>
    /// Apply filter to the component
    /// </summary>
    Task ApplyFilterAsync(object filteredData);
    
    /// <summary>
    /// Get the filterable data from the component
    /// </summary>
    IEnumerable<object> GetFilterableData();
    
    /// <summary>
    /// Get the fields that can be filtered
    /// </summary>
    IEnumerable<string> GetFilterableFields();
    
    /// <summary>
    /// Apply predicate filter
    /// </summary>
    Task ApplyPredicateAsync(object predicate);
    
    /// <summary>
    /// Handle filter changes
    /// </summary>
    Task OnFilterChangedAsync();
}

/// <summary>
/// Generic interface for components that can be filtered with strongly-typed data
/// </summary>
public interface IFilterable<T> : IFilterable where T : class
{
    /// <summary>
    /// Apply strongly-typed predicate filter
    /// </summary>
    Task ApplyPredicateAsync(Expression<Func<T, bool>> predicate);
    
    /// <summary>
    /// Get the strongly-typed filterable data from the component
    /// </summary>
    IEnumerable<T> GetTypedFilterableData();
}

/// <summary>
/// Interface for providing filters to components
/// </summary>
public interface IFilterProvider<T>
{
    /// <summary>
    /// Get current filter predicate
    /// </summary>
    Expression<Func<T, bool>>? GetPredicate();
    
    /// <summary>
    /// Apply filter and return results
    /// </summary>
    Task<FilterResult<T>> ApplyFilterAsync(IEnumerable<T> data);
    
    /// <summary>
    /// Event fired when filter changes
    /// </summary>
    event Action<FilterStateChangedEventArgs>? OnFilterChanged;
}

/// <summary>
/// Interface for persisting filter configurations
/// </summary>
public interface IFilterPersistenceService
{
    /// <summary>
    /// Whether persistence is enabled (off by default)
    /// </summary>
    bool IsEnabled { get; set; }
    
    /// <summary>
    /// Save filter configuration
    /// </summary>
    Task SaveFilterAsync(string key, FilterConfiguration config);
    
    /// <summary>
    /// Save filter configuration with metadata
    /// </summary>
    Task SaveConfigurationAsync(string key, FilterConfiguration config);
    
    /// <summary>
    /// Load filter configuration
    /// </summary>
    Task<FilterConfiguration?> LoadFilterAsync(string key);
    
    /// <summary>
    /// Load filter state by key
    /// </summary>
    Task<GridFilterState?> LoadFilterStateAsync(string key);
    
    /// <summary>
    /// Save filter state
    /// </summary>
    Task SaveFilterStateAsync(string key, GridFilterState state);
    
    /// <summary>
    /// Get all saved filter configurations for a component
    /// </summary>
    Task<List<FilterConfiguration>> GetSavedFiltersAsync(string componentKey);
    
    /// <summary>
    /// Get all configurations for a component
    /// </summary>
    Task<List<FilterConfiguration>> GetConfigurationsAsync(string componentKey);
    
    /// <summary>
    /// Delete saved filter configuration
    /// </summary>
    Task DeleteFilterAsync(string key);
}