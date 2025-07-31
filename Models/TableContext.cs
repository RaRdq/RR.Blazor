namespace RR.Blazor.Models;

/// <summary>
/// Enum representing sort direction
/// </summary>
public enum SortDirection
{
    None = 0,
    Ascending = 1,
    Descending = 2
}

/// <summary>
/// Represents the sorting state for a single column
/// </summary>
public class TableSortState
{
    public string ColumnKey { get; set; } = "";
    public SortDirection Direction { get; set; } = SortDirection.None;
    public int Priority { get; set; } = 0;
    
    public TableSortState() { }
    
    public TableSortState(string columnKey, SortDirection direction, int priority = 0)
    {
        ColumnKey = columnKey;
        Direction = direction;
        Priority = priority;
    }
}

/// <summary>
/// Event args for sort operations
/// </summary>
public class SortEventArgs
{
    public string ColumnKey { get; set; } = "";
    public bool IsMultiSort { get; set; }
    public List<TableSortState> CurrentSortStates { get; set; } = new();
}

/// <summary>
/// Context information for table components to share type and configuration data
/// </summary>
public class TableContext
{
    public Type ItemType { get; }
    public string TableId { get; }
    public bool IsSmartTable { get; }
    public bool IsHeaderContext { get; }

    public TableContext(Type itemType, string tableId, bool isSmartTable = false, bool isHeaderContext = false)
    {
        ItemType = itemType ?? throw new ArgumentNullException(nameof(itemType));
        TableId = tableId ?? throw new ArgumentNullException(nameof(tableId));
        IsSmartTable = isSmartTable;
        IsHeaderContext = isHeaderContext;
    }
}