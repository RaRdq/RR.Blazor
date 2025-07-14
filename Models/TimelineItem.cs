using Microsoft.AspNetCore.Components;
using RR.Blazor.Enums;

namespace RR.Blazor.Models
{
    public class TimelineItem
    {
        public string Id { get; set; } = "";
        public string Title { get; set; } = "";
        public string Description { get; set; } = "";
        public string Subtitle { get; set; } = "";
        public DateTime? Timestamp { get; set; } = DateTime.UtcNow;
        public string Status { get; set; } = "default"; // default, success, warning, error, info, pending
        public string Icon { get; set; } = "";
        public string Avatar { get; set; } = "";
        public bool ShowTime { get; set; } = true;
        public bool IsCompleted { get; set; }
        public bool IsHighlighted { get; set; }
        public List<string> Tags { get; set; } = new();
        public List<TimelineAction> Actions { get; set; } = new();
        public RenderFragment? CustomContent { get; set; }
        public object? Data { get; set; }
    }

    public class TimelineAction
    {
        public string Text { get; set; } = "";
        public string Icon { get; set; } = "";
        public ButtonVariant Variant { get; set; } = ButtonVariant.Ghost;
        public EventCallback OnClick { get; set; }
        public bool IsDisabled { get; set; }
    }
}