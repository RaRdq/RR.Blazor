namespace RR.Blazor.Models;

/// <summary>
/// Context information for table components to share type and configuration data
/// </summary>
public class TableContext
{
    public Type ItemType { get; }
    public string TableId { get; }
    public bool IsSmartTable { get; }

    public TableContext(Type itemType, string tableId, bool isSmartTable = false)
    {
        ItemType = itemType ?? throw new ArgumentNullException(nameof(itemType));
        TableId = tableId ?? throw new ArgumentNullException(nameof(tableId));
        IsSmartTable = isSmartTable;
    }
}