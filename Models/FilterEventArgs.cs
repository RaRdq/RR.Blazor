using Microsoft.AspNetCore.Components;

namespace RR.Blazor.Models
{
    public class FilterChangedEventArgs
    {
        public string Key { get; set; } = "";
        public string Value { get; set; } = "";
        public Dictionary<string, string> AllFilters { get; set; } = new();
    }

    public class DateRangeChangedEventArgs
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
    }

    public class QuickFilterToggledEventArgs
    {
        public string Key { get; set; } = "";
        public bool IsActive { get; set; }
        public Dictionary<string, bool> AllQuickFilters { get; set; } = new();
        public List<string> ActiveFilters { get; set; } = new();
    }

    public class QuickFilterDefinition
    {
        public string Key { get; set; } = "";
        public string Label { get; set; } = "";
        public string Icon { get; set; } = "";
        public bool IsActive { get; set; }
        public int Count { get; set; }
    }
}