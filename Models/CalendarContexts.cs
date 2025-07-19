using RR.Blazor.Enums;

namespace RR.Blazor.Models
{
    /// <summary>
    /// Context object passed to calendar header templates
    /// </summary>
    public class CalendarHeaderContext
    {
        /// <summary>Currently displayed date</summary>
        public DateTime CurrentDate { get; set; }
        
        /// <summary>Current calendar view</summary>
        public CalendarView View { get; set; }
        
        /// <summary>Formatted title for the current view</summary>
        public string Title { get; set; } = string.Empty;
    }
    
    /// <summary>
    /// Context object passed to calendar day cell templates
    /// </summary>
    public class CalendarDayContext
    {
        /// <summary>The date for this day cell</summary>
        public DateTime Date { get; set; }
        
        /// <summary>Whether this day is today</summary>
        public bool IsToday { get; set; }
        
        /// <summary>Whether this day is selected</summary>
        public bool IsSelected { get; set; }
        
        /// <summary>Whether this day belongs to the current month</summary>
        public bool IsCurrentMonth { get; set; }
        
        /// <summary>Whether this day is a weekend</summary>
        public bool IsWeekend { get; set; }
        
        /// <summary>Events occurring on this day</summary>
        public List<CalendarEvent> Events { get; set; } = new();
        
        /// <summary>Whether this day has any events</summary>
        public bool HasEvents => Events.Any();
        
        /// <summary>Number of events on this day</summary>
        public int EventCount => Events.Count;
    }
}