namespace RR.Blazor.Models
{
    public class FilterDefinition
    {
        public string Key { get; set; } = "";
        public string Label { get; set; } = "";
        public string Type { get; set; } = "text"; // text, select, number, date
        public string Value { get; set; } = "";
        public List<FilterOption> Options { get; set; } = new();
        public string Placeholder { get; set; } = "";
        public bool Required { get; set; }
    }

    public class FilterOption
    {
        public string Value { get; set; } = "";
        public string Label { get; set; } = "";
        public bool IsSelected { get; set; }
    }

    public class QuickFilter
    {
        public string Key { get; set; } = "";
        public string Label { get; set; } = "";
        public bool IsActive { get; set; }
        public int Count { get; set; }
    }
}