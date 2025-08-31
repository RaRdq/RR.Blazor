using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using RR.Blazor.Enums;
using RR.Blazor.Models;
using RR.Blazor.Interfaces;
using System.Collections;
using System.Linq.Expressions;
using System.Collections.Concurrent;
using System.Reflection;

namespace RR.Blazor.Components.Data;

/// <summary>
/// Smart filter wrapper that auto-detects data type and creates RFilterGeneric&lt;T&gt; dynamically
/// Follows Smart Components Architecture pattern exactly like RForm
/// </summary>
public class RFilter : RFilterBase
{
    
    /// <summary>
    /// Data source to filter - accepts any IEnumerable or IQueryable
    /// Auto-detects item type and creates appropriate generic filter
    /// </summary>
    [Parameter] public object? DataSource { get; set; }
    
    /// <summary>
    /// Target component to filter (RTable, RGrid, etc)
    /// </summary>
    [Parameter] public object? TargetComponent { get; set; }
    
    /// <summary>
    /// Filter configuration and UI settings
    /// </summary>
    [Parameter] public UniversalFilterConfig Config { get; set; } = new();
    
    /// <summary>
    /// Quick filter definitions
    /// </summary>
    [Parameter] public List<QuickFilterState> QuickFilters { get; set; } = new();
    
    /// <summary>
    /// Available filter column definitions
    /// </summary>
    [Parameter] public Dictionary<string, FilterColumnDefinition> ColumnDefinitions { get; set; } = new();
    
    /// <summary>
    /// Fields to include in search functionality
    /// </summary>
    [Parameter] public List<string> SearchFields { get; set; } = new();
    
    /// <summary>
    /// Initial filter criteria
    /// </summary>
    [Parameter] public object? InitialCriteria { get; set; }
    
    /// <summary>
    /// Filter display mode
    /// </summary>
    [Parameter] public FilterMode Mode { get; set; } = FilterMode.Smart;
    
    /// <summary>
    /// Enable real-time filtering with debouncing
    /// </summary>
    [Parameter] public bool EnableRealTime { get; set; } = true;
    
    /// <summary>
    /// Debounce delay in milliseconds for real-time filtering
    /// </summary>
    [Parameter] public int DebounceMs { get; set; } = 300;
    
    /// <summary>
    /// Event fired when filter predicate changes
    /// </summary>
    [Parameter] public EventCallback<object> OnPredicateChanged { get; set; }
    
    /// <summary>
    /// Event fired when filter criteria changes
    /// </summary>
    [Parameter] public EventCallback<object> OnCriteriaChanged { get; set; }
    
    /// <summary>
    /// Event fired when filter state changes
    /// </summary>
    [Parameter] public EventCallback<FilterStateChangedEventArgs> OnFilterChanged { get; set; }
    
    /// <summary>
    /// Enable persistence to LocalStorage (off by default)
    /// </summary>
    [Parameter] public bool EnablePersistence { get; set; } = false;
    
    /// <summary>
    /// Persistence key for saving filter state (auto-generated if not provided)
    /// </summary>
    [Parameter] public string PersistenceKey { get; set; }
    
    private Type? _detectedItemType;
    private object? _genericFilterInstance;
    
    private static readonly ConcurrentDictionary<Type, Type> _genericFilterTypeCache = new();
    
    // ===== SERVICES =====
    
    [Inject] private IFilterPersistenceService? FilterPersistence { get; set; }
    
    // ===== LIFECYCLE =====
    
    protected override void OnInitialized()
    {
        base.OnInitialized();
        
        if (DataSource == null && TargetComponent != null)
        {
            DetectDataSourceFromTarget();
        }
        
        _detectedItemType = DetectItemType(DataSource);
        
        if (_detectedItemType != null && !ColumnDefinitions.Any())
        {
            ColumnDefinitions = AutoDetectColumnDefinitions(_detectedItemType);
        }
    }
    
    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        
        if (EnablePersistence && FilterPersistence != null)
        {
            FilterPersistence.IsEnabled = true;
            
            if (string.IsNullOrEmpty(PersistenceKey))
            {
                PersistenceKey = $"{GetType().Name}_{_detectedItemType?.Name ?? "Unknown"}";
            }
            
            await LoadPersistedState();
        }
    }
    
    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        if (_detectedItemType == null && DataSource != null)
        {
            _detectedItemType = DetectItemType(DataSource);
        }
        
        if (_detectedItemType == null)
        {
            RenderEmptyState(builder);
            return;
        }
        
        var genericFilterType = GetOrCreateGenericFilterType(_detectedItemType);
        
        builder.OpenComponent(0, genericFilterType);
        
        ForwardBaseParameters(builder);
        
        builder.AddAttribute(20, nameof(DataSource), DataSource);
        builder.AddAttribute(21, nameof(TargetComponent), TargetComponent);
        builder.AddAttribute(22, nameof(Config), Config);
        builder.AddAttribute(23, nameof(QuickFilters), QuickFilters);
        builder.AddAttribute(24, nameof(ColumnDefinitions), ColumnDefinitions);
        builder.AddAttribute(25, nameof(InitialCriteria), InitialCriteria);
        builder.AddAttribute(26, nameof(Mode), Mode);
        builder.AddAttribute(27, nameof(EnableRealTime), EnableRealTime);
        builder.AddAttribute(28, nameof(DebounceMs), DebounceMs);
        builder.AddAttribute(29, nameof(SearchFields), SearchFields);
        
        builder.AddAttribute(30, "OnPredicateChanged", OnPredicateChanged);
        builder.AddAttribute(31, "OnCriteriaChanged", OnCriteriaChanged);
        builder.AddAttribute(32, "OnFilterChanged", OnFilterChanged);
        
        builder.AddComponentReferenceCapture(35, inst => _genericFilterInstance = inst);
        
        builder.CloseComponent();
    }
    
    // ===== TYPE DETECTION =====
    
    /// <summary>
    /// Detect item type from data source using fast type detection
    /// </summary>
    private static Type? DetectItemType(object? dataSource)
    {
        if (dataSource == null) return null;
        
        var dataSourceType = dataSource.GetType();
        
        if (dataSourceType.IsGenericType)
        {
            var genericDef = dataSourceType.GetGenericTypeDefinition();
            
            if (genericDef == typeof(List<>) || 
                genericDef == typeof(IEnumerable<>) ||
                genericDef == typeof(ICollection<>) ||
                genericDef == typeof(IList<>) ||
                genericDef == typeof(IQueryable<>))
            {
                return dataSourceType.GetGenericArguments()[0];
            }
        }
        
        if (dataSource is IEnumerable enumerable)
        {
            var enumerableInterface = dataSourceType
                .GetInterfaces()
                .FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEnumerable<>));
            
            if (enumerableInterface != null)
            {
                return enumerableInterface.GetGenericArguments()[0];
            }
        }
        
        return null;
    }
    
    /// <summary>
    /// Get or create cached generic filter type to avoid repeated MakeGenericType calls
    /// </summary>
    private static Type GetOrCreateGenericFilterType(Type itemType)
    {
        return _genericFilterTypeCache.GetOrAdd(itemType, 
            type => typeof(RFilterGeneric<>).MakeGenericType(type));
    }
    
    /// <summary>
    /// Detect data source from target component properties
    /// </summary>
    private void DetectDataSourceFromTarget()
    {
        if (TargetComponent == null) return;
        
        var targetType = TargetComponent.GetType();
        
        var dataSourceProperty = targetType.GetProperty("DataSource") ??
                                targetType.GetProperty("Items") ??
                                targetType.GetProperty("Data");
        
        if (dataSourceProperty?.CanRead == true)
        {
            DataSource = dataSourceProperty.GetValue(TargetComponent);
        }
    }
    
    /// <summary>
    /// Auto-detect column definitions from item type
    /// </summary>
    private Dictionary<string, FilterColumnDefinition> AutoDetectColumnDefinitions(Type itemType)
    {
        var columns = new Dictionary<string, FilterColumnDefinition>();
        
        var properties = itemType.GetProperties()
            .Where(p => p.CanRead && IsFilterableProperty(p));
        
        foreach (var prop in properties)
        {
            var filterType = DetectFilterType(prop.PropertyType);
            var displayName = FormatDisplayName(prop.Name);
            
            columns[prop.Name] = new FilterColumnDefinition(
                Key: prop.Name,
                DisplayName: displayName,
                Type: filterType,
                IsFilterable: true,
                IsSearchable: filterType == FilterType.Text,
                IsSortable: true,
                SupportedOperators: GetOperatorsForType(filterType),
                UniqueValues: null, // Will be populated on demand
                MinValue: null,
                MaxValue: null,
                Format: GetPropertyFormat(prop)
            );
        }
        
        return columns;
    }
    
    /// <summary>
    /// Determine if a property should be filterable
    /// </summary>
    private static bool IsFilterableProperty(System.Reflection.PropertyInfo prop)
    {
        return !prop.GetCustomAttributes(typeof(System.ComponentModel.DataAnnotations.Schema.NotMappedAttribute), false).Any() &&
               !prop.GetCustomAttributes(typeof(System.ComponentModel.DataAnnotations.Schema.ForeignKeyAttribute), false).Any() &&
               prop.PropertyType != typeof(object);
    }
    
    /// <summary>
    /// Detect appropriate filter type from property type
    /// </summary>
    private static FilterType DetectFilterType(Type propertyType)
    {
        var underlyingType = Nullable.GetUnderlyingType(propertyType) ?? propertyType;
        
        if (underlyingType == typeof(bool)) return FilterType.Boolean;
        if (underlyingType == typeof(DateTime) || underlyingType == typeof(DateTimeOffset)) return FilterType.Date;
        if (underlyingType.IsEnum) return FilterType.Select;
        if (IsNumericType(underlyingType)) return FilterType.Number;
        if (underlyingType == typeof(string)) return FilterType.Text;
        
        return FilterType.Custom;
    }
    
    /// <summary>
    /// Check if type is numeric
    /// </summary>
    private static bool IsNumericType(Type type)
    {
        return type == typeof(byte) || type == typeof(sbyte) ||
               type == typeof(short) || type == typeof(ushort) ||
               type == typeof(int) || type == typeof(uint) ||
               type == typeof(long) || type == typeof(ulong) ||
               type == typeof(float) || type == typeof(double) ||
               type == typeof(decimal);
    }
    
    /// <summary>
    /// Format property name for display
    /// </summary>
    private static string FormatDisplayName(string propertyName)
    {
        return System.Text.RegularExpressions.Regex.Replace(propertyName, "([a-z])([A-Z])", "$1 $2");
    }
    
    /// <summary>
    /// Get supported operators for filter type
    /// </summary>
    private static List<FilterOperator> GetOperatorsForType(FilterType filterType)
    {
        return filterType switch
        {
            FilterType.Text => new List<FilterOperator>
            {
                FilterOperator.Contains, FilterOperator.NotContains,
                FilterOperator.StartsWith, FilterOperator.EndsWith,
                FilterOperator.Equals, FilterOperator.NotEquals,
                FilterOperator.IsEmpty, FilterOperator.IsNotEmpty
            },
            FilterType.Number => new List<FilterOperator>
            {
                FilterOperator.Equals, FilterOperator.NotEquals,
                FilterOperator.GreaterThan, FilterOperator.LessThan,
                FilterOperator.GreaterThanOrEqual, FilterOperator.LessThanOrEqual,
                FilterOperator.Between, FilterOperator.NotBetween
            },
            FilterType.Date => new List<FilterOperator>
            {
                FilterOperator.On, FilterOperator.Before, FilterOperator.After,
                FilterOperator.OnOrBefore, FilterOperator.OnOrAfter,
                FilterOperator.Between, FilterOperator.Today,
                FilterOperator.Yesterday, FilterOperator.ThisWeek,
                FilterOperator.ThisMonth, FilterOperator.ThisYear
            },
            FilterType.Boolean => new List<FilterOperator>
            {
                FilterOperator.IsTrue, FilterOperator.IsFalse
            },
            FilterType.Select => new List<FilterOperator>
            {
                FilterOperator.Equals, FilterOperator.NotEquals,
                FilterOperator.In, FilterOperator.NotIn
            },
            _ => new List<FilterOperator> { FilterOperator.Equals, FilterOperator.NotEquals }
        };
    }
    
    /// <summary>
    /// Get format string from property attributes
    /// </summary>
    private static string? GetPropertyFormat(System.Reflection.PropertyInfo prop)
    {
        var formatAttr = prop.GetCustomAttribute<System.ComponentModel.DataAnnotations.DisplayFormatAttribute>();
        return formatAttr?.DataFormatString;
    }
    
    // ===== PARAMETER FORWARDING =====
    
    /// <summary>
    /// Forward all base parameters to the generic filter component
    /// </summary>
    private void ForwardBaseParameters(RenderTreeBuilder builder)
    {
        builder.AddAttribute(10, nameof(ShowSearch), ShowSearch);
        builder.AddAttribute(11, nameof(SearchPlaceholder), SearchPlaceholder);
        builder.AddAttribute(12, nameof(ShowDateRange), ShowDateRange);
        builder.AddAttribute(13, nameof(ShowQuickFilters), ShowQuickFilters);
        builder.AddAttribute(14, nameof(ShowAdvanced), ShowAdvanced);
        builder.AddAttribute(15, nameof(ShowClearButton), ShowClearButton);
        builder.AddAttribute(16, nameof(ShowFilterCount), ShowFilterCount);
        builder.AddAttribute(17, nameof(Minimal), Minimal);
        builder.AddAttribute(18, nameof(Compact), Compact);
        builder.AddAttribute(19, nameof(CustomFilters), CustomFilters);
        
        if (!string.IsNullOrEmpty(Class))
            builder.AddAttribute(40, "class", Class);
        
        if (AdditionalAttributes?.Count > 0)
            builder.AddMultipleAttributes(41, AdditionalAttributes);
    }
    
    // ===== EMPTY STATE =====
    
    /// <summary>
    /// Render empty state when no data source is configured
    /// </summary>
    private void RenderEmptyState(RenderTreeBuilder builder)
    {
        builder.OpenElement(0, "div");
        builder.AddAttribute(1, "class", "rfilter-empty flex items-center justify-center p-4 text-muted");
        
        builder.OpenElement(2, "i");
        builder.AddAttribute(3, "class", "icon text-lg mr-2");
        builder.AddContent(4, "filter_list_off");
        builder.CloseElement();
        
        builder.AddContent(5, "No data source configured. Provide DataSource or link a component.");
        
        builder.CloseElement();
    }
    
    // ===== PUBLIC API DELEGATION =====
    
    /// <summary>
    /// Get the detected item type
    /// </summary>
    public override Type? GetItemType() => _detectedItemType;
    
    /// <summary>
    /// Clear all filters by delegating to generic instance
    /// </summary>
    public override async Task ClearAsync()
    {
        if (_genericFilterInstance != null)
        {
            var method = _genericFilterInstance.GetType().GetMethod(nameof(ClearAsync));
            if (method != null)
            {
                await (Task)method.Invoke(_genericFilterInstance, null)!;
            }
        }
    }
    
    /// <summary>
    /// Refresh filters by delegating to generic instance
    /// </summary>
    public override async Task RefreshAsync()
    {
        if (_genericFilterInstance != null)
        {
            var method = _genericFilterInstance.GetType().GetMethod(nameof(RefreshAsync));
            if (method != null)
            {
                await (Task)method.Invoke(_genericFilterInstance, null)!;
            }
        }
    }
    
    /// <summary>
    /// Get current filter criteria from generic instance
    /// </summary>
    public object? GetCurrentCriteria()
    {
        if (_genericFilterInstance == null) return null;
        
        var method = _genericFilterInstance.GetType().GetMethod("GetCurrentCriteria");
        return method?.Invoke(_genericFilterInstance, null);
    }
    
    /// <summary>
    /// Set filter criteria on generic instance
    /// </summary>
    public async Task SetCriteriaAsync(object criteria)
    {
        if (_genericFilterInstance == null) return;
        
        var method = _genericFilterInstance.GetType().GetMethod("SetCriteriaAsync");
        if (method != null)
        {
            await (Task)method.Invoke(_genericFilterInstance, new[] { criteria })!;
        }
    }
    
    /// <summary>
    /// Get the current filter predicate
    /// </summary>
    public Expression<Func<T, bool>> GetPredicate<T>() where T : class
    {
        if (_genericFilterInstance == null) return null;
        
        var method = _genericFilterInstance.GetType().GetMethod("GetPredicate");
        if (method != null)
        {
            return (Expression<Func<T, bool>>)method.Invoke(_genericFilterInstance, null);
        }
        
        return null;
    }
    
    // ===== REQUIRED ABSTRACT IMPLEMENTATIONS =====
    
    /// <summary>
    /// Apply filter - delegated to generic instance (deprecated pattern)
    /// </summary>
    [Obsolete("Use GetPredicate() pattern instead. Components should apply predicates to their own data.")]
    public override Task<object> ApplyFilterAsync(object dataSource)
    {
        return Task.FromResult(dataSource);
    }
    
    protected override FilterType FilterType => FilterType.Auto;
    
    protected override List<FilterOperator> AvailableOperators => new()
    {
        FilterOperator.Contains,
        FilterOperator.Equals,
        FilterOperator.NotEquals
    };
    
    protected override void RenderFilterInput(RenderTreeBuilder builder)
    {
    }
    
    protected override SizeType GetDefaultSize() => SizeType.Medium;
    
    // ===== PERSISTENCE METHODS =====
    
    private async Task LoadPersistedState()
    {
        if (!EnablePersistence || FilterPersistence == null || string.IsNullOrEmpty(PersistenceKey))
            return;
        
        try
        {
            var state = await FilterPersistence.LoadFilterStateAsync(PersistenceKey);
            if (state != null && _genericFilterInstance != null)
            {
                var method = _genericFilterInstance.GetType().GetMethod("ApplyState");
                if (method != null)
                {
                    await (Task)method.Invoke(_genericFilterInstance, new object[] { state })!;
                }
            }
        }
        catch (Exception ex)
        {
        }
    }
    
    private async Task SavePersistedState()
    {
        if (!EnablePersistence || FilterPersistence == null || string.IsNullOrEmpty(PersistenceKey))
            return;
        
        try
        {
            var state = GetCurrentState();
            if (state != null)
            {
                await FilterPersistence.SaveFilterStateAsync(PersistenceKey, state);
            }
        }
        catch (Exception ex)
        {
        }
    }
    
    private GridFilterState GetCurrentState()
    {
        if (_genericFilterInstance == null) return null;
        
        var method = _genericFilterInstance.GetType().GetMethod("GetCurrentState");
        if (method != null)
        {
            return (GridFilterState)method.Invoke(_genericFilterInstance, null);
        }
        
        return null;
    }
}