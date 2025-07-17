namespace RR.Blazor.Models
{
    public class SelectItem
    {
        public string Value { get; set; } = string.Empty;
        public string Text { get; set; } = string.Empty;
        public bool Disabled { get; set; } = false;
        public string Icon { get; set; } = string.Empty;
        public string Group { get; set; } = string.Empty;
    }
}