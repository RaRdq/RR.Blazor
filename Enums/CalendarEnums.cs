using RR.Blazor.Attributes;

namespace RR.Blazor.Enums
{
    /// <summary>Calendar view modes</summary>
    public enum CalendarView
    {
        /// <summary>Monthly calendar view</summary>
        Month,
        
        /// <summary>Weekly calendar view</summary>
        Week
    }
    
    /// <summary>Calendar size variants</summary>
    public enum CalendarSize
    {
        /// <summary>Small calendar for compact spaces</summary>
        Small,
        
        /// <summary>Default calendar size</summary>
        Default,
        
        /// <summary>Large calendar for detailed views</summary>
        Large
    }
}