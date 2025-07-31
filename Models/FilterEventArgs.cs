using Microsoft.AspNetCore.Components;
using RR.Blazor.Enums;

namespace RR.Blazor.Models
{
    // =============================================================================
    // CONSOLIDATED FILTER EVENT ARGS - All filter-related event arguments
    // =============================================================================

    /// <summary>
    /// Event arguments for basic filter changes (legacy compatibility)
    /// </summary>
    public class FilterChangedEventArgs
    {
        public string Key { get; set; } = "";
        public string Value { get; set; } = "";
        public Dictionary<string, string> AllFilters { get; set; } = new();
        
        /// <summary>
        /// Converts legacy event args to modern FilterEventArgs
        /// </summary>
        public FilterEventArgs ToModern(FilterType filterType = FilterType.Text)
        {
            var oldFilter = new FilterState(Key, filterType, FilterOperator.Contains, "", null, false);
            var newFilter = new FilterState(Key, filterType, FilterOperator.Contains, Value, null, !string.IsNullOrEmpty(Value));
            var currentFilters = new FilterGroup(
                AllFilters.Where(kvp => !string.IsNullOrEmpty(kvp.Value))
                         .Select(kvp => new FilterState(kvp.Key, filterType, FilterOperator.Contains, kvp.Value))
                         .ToList()
            );
            
            return new FilterEventArgs(Key, oldFilter, newFilter, currentFilters, string.IsNullOrEmpty(Value));
        }
    }

    /// <summary>
    /// Event arguments for date range changes
    /// </summary>
    public class DateRangeChangedEventArgs
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
        
        /// <summary>
        /// Gets the effective start date (StartDate or DateFrom)
        /// </summary>
        public DateTime? EffectiveStartDate => StartDate ?? DateFrom;
        
        /// <summary>
        /// Gets the effective end date (EndDate or DateTo)
        /// </summary>
        public DateTime? EffectiveEndDate => EndDate ?? DateTo;
        
        /// <summary>
        /// Converts to modern FilterEventArgs with date range
        /// </summary>
        public FilterEventArgs ToModern(string columnKey, string? displayName = null)
        {
            var hasRange = EffectiveStartDate.HasValue || EffectiveEndDate.HasValue;
            var oldFilter = new FilterState(columnKey, FilterType.DateRange, FilterOperator.InRange, null, null, false, displayName);
            var newFilter = hasRange 
                ? new FilterState(columnKey, FilterType.DateRange, FilterOperator.InRange, EffectiveStartDate, EffectiveEndDate, true, displayName)
                : null;
                
            return new FilterEventArgs(columnKey, oldFilter, newFilter, 
                new FilterGroup(newFilter != null ? [newFilter] : []), !hasRange);
        }
    }

    /// <summary>
    /// Event arguments for quick filter toggles
    /// </summary>
    public class QuickFilterToggledEventArgs
    {
        public string Key { get; set; } = "";
        public bool IsActive { get; set; }
        public Dictionary<string, bool> AllQuickFilters { get; set; } = new();
        public List<string> ActiveFilters { get; set; } = new();
        
        /// <summary>
        /// Converts to modern FilterEventArgs for quick filters
        /// </summary>
        public FilterEventArgs ToModern(string? displayName = null)
        {
            var oldFilter = new FilterState(Key, FilterType.Boolean, FilterOperator.IsTrue, !IsActive, null, !IsActive, displayName);
            var newFilter = IsActive 
                ? new FilterState(Key, FilterType.Boolean, FilterOperator.IsTrue, true, null, true, displayName)
                : null;
                
            var currentFilters = new FilterGroup(
                ActiveFilters.Select(key => new FilterState(key, FilterType.Boolean, FilterOperator.IsTrue, true, null, true))
                           .ToList()
            );
            
            return new FilterEventArgs(Key, oldFilter, newFilter, currentFilters, !IsActive);
        }
    }

    /// <summary>
    /// Definition for quick filter options
    /// </summary>
    public class QuickFilterDefinition
    {
        public string Key { get; set; } = "";
        public string Label { get; set; } = "";
        public string Icon { get; set; } = "";
        public bool IsActive { get; set; }
        public int Count { get; set; }
        
        /// <summary>
        /// Converts to FilterOption for compatibility
        /// </summary>
        public FilterOption ToFilterOption()
        {
            return new FilterOption(IsActive, Label, IsActive, $"{Label} ({Count})", null, false);
        }
    }

    /// <summary>
    /// Event arguments for column-specific filter changes
    /// </summary>
    public class ColumnFilterEventArgs
    {
        public string ColumnKey { get; set; } = "";
        public string FilterValue { get; set; } = "";
        public FilterType FilterType { get; set; }
        public List<string> SelectedValues { get; set; } = new();
        
        /// <summary>
        /// Converts to modern FilterEventArgs for column filters
        /// </summary>
        public FilterEventArgs ToModern(string? displayName = null)
        {
            var hasValue = !string.IsNullOrEmpty(FilterValue) || SelectedValues.Any();
            var oldFilter = new FilterState(ColumnKey, FilterType, GetOperatorForType(), null, null, false, displayName);
            
            FilterState? newFilter = null;
            if (hasValue)
            {
                object? value = FilterType == FilterType.MultiSelect ? (object)SelectedValues : FilterValue;
                var op = FilterType == FilterType.MultiSelect ? FilterOperator.In : GetOperatorForType();
                newFilter = new FilterState(ColumnKey, FilterType, op, value, null, true, displayName);
            }
            
            return new FilterEventArgs(ColumnKey, oldFilter, newFilter, 
                new FilterGroup(newFilter != null ? [newFilter] : []), !hasValue);
        }
        
        private FilterOperator GetOperatorForType() => FilterType switch
        {
            FilterType.Text => FilterOperator.Contains,
            FilterType.Number => FilterOperator.Equals,
            FilterType.Date => FilterOperator.On,
            FilterType.DateRange => FilterOperator.InRange,
            FilterType.Boolean => FilterOperator.IsTrue,
            FilterType.Select => FilterOperator.Equals,
            FilterType.MultiSelect => FilterOperator.In,
            _ => FilterOperator.Contains
        };
    }

    // =============================================================================
    // COMPATIBILITY ALIASES - For backward compatibility during migration
    // =============================================================================
    
    /// <summary>
    /// Alias for FilterEventArgs (from FilterModels.cs) for consistency
    /// </summary>
    // Note: Using aliases should be at top of file, not inside namespace
}