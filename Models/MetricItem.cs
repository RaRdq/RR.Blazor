using Microsoft.AspNetCore.Components;

namespace RR.Blazor.Models
{
    public class MetricItem
    {
        public string Label { get; set; } = "";
        public object Value { get; set; } = 0;
        public string Prefix { get; set; } = "";
        public string Suffix { get; set; } = "";
        public string Format { get; set; } = ""; // currency, percentage, number
        public decimal? Change { get; set; }
        public string ChangeLabel { get; set; } = "";
        public string Variant { get; set; } = "default"; // default, success, warning, error, info
        public string Icon { get; set; } = "";
        public bool ShowChange { get; set; }
        public bool IsClickable { get; set; }
        public EventCallback OnClick { get; set; }
    }
}