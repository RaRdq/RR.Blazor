using RR.Blazor.Attributes;

namespace RR.Blazor.Models
{
    /// <summary>
    /// Represents a calendar event with comprehensive metadata for display and interaction
    /// </summary>
    [AIOptimized(Prompt = "Calendar event with title, dates, and metadata", 
                 CommonUse = "event scheduling, booking systems, leave tracking")]
    public class CalendarEvent
    {
        /// <summary>Unique identifier for the event</summary>
        public string Id { get; set; } = Guid.NewGuid().ToString();
        
        /// <summary>Event title or name</summary>
        [AIParameter("Event title", Example = "Team Meeting")]
        public string Title { get; set; } = string.Empty;
        
        /// <summary>Event description or details</summary>
        [AIParameter("Event description", Example = "Weekly team standup meeting")]
        public string? Description { get; set; }
        
        /// <summary>Event start date and time</summary>
        [AIParameter("Event start date", Example = "DateTime.Today")]
        public DateTime StartDate { get; set; }
        
        /// <summary>Event end date and time</summary>
        [AIParameter("Event end date", Example = "DateTime.Today.AddHours(1)")]
        public DateTime EndDate { get; set; }
        
        /// <summary>Whether this is an all-day event</summary>
        [AIParameter("All-day event flag", Example = "true for vacation days")]
        public bool IsAllDay { get; set; }
        
        /// <summary>Event category for styling and grouping</summary>
        [AIParameter("Event category", Example = "meeting, vacation, holiday")]
        public string? Category { get; set; }
        
        /// <summary>Event location</summary>
        [AIParameter("Event location", Example = "Conference Room A")]
        public string? Location { get; set; }
        
        /// <summary>Event organizer or owner</summary>
        [AIParameter("Event organizer", Example = "John Smith")]
        public string? Organizer { get; set; }
        
        /// <summary>Event attendees</summary>
        [AIParameter("Event attendees", Example = "List of participant names")]
        public List<string> Attendees { get; set; } = new();
        
        /// <summary>Custom CSS class for styling</summary>
        [AIParameter("Custom styling class", Example = "bg-success text-white")]
        public string? Class { get; set; }
        
        /// <summary>Event priority level</summary>
        [AIParameter("Event priority", Example = "EventPriority.High")]
        public EventPriority Priority { get; set; } = EventPriority.Normal;
        
        /// <summary>Whether the event is clickable</summary>
        [AIParameter("Event clickable", Example = "true to enable click interactions")]
        public bool IsClickable { get; set; } = true;
        
        /// <summary>Custom data for application-specific use</summary>
        [AIParameter("Custom event data", Example = "Dictionary for app-specific properties")]
        public Dictionary<string, object> CustomData { get; set; } = new();
        
        /// <summary>Event URL for external links</summary>
        [AIParameter("Event URL", Example = "https://example.com/event/123")]
        public string? Url { get; set; }
    }
    
    /// <summary>Event priority levels for visual hierarchy</summary>
    public enum EventPriority
    {
        Low,
        Normal,
        High,
        Critical
    }
}