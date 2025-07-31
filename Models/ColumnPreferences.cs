using RR.Blazor.Enums;

namespace RR.Blazor.Models
{
    /// <summary>
    /// Represents user preferences for a specific table column
    /// </summary>
    public record ColumnPreferences(
        bool Visible = true,
        string? Width = null,
        int Order = 0,
        bool IsPinned = false,
        StickyPosition PinPosition = StickyPosition.Left
    );

    /// <summary>
    /// Event arguments for column management actions
    /// </summary>
    public class ColumnManagementEventArgs
    {
        public string ColumnKey { get; set; } = "";
        public ColumnManagementAction Action { get; set; }
        public object? NewValue { get; set; }
        public object? OldValue { get; set; }
    }

    /// <summary>
    /// Event arguments for column resize operations
    /// </summary>
    public class ColumnResizeEventArgs
    {
        public string ColumnKey { get; set; } = "";
        public string NewWidth { get; set; } = "";
        public string OldWidth { get; set; } = "";
        public bool IsComplete { get; set; }
    }
}