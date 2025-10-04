namespace RR.Blazor.Models;

/// <summary>
/// Enum representing grid layout modes
/// </summary>
public enum GridLayoutMode
{
    Auto = 0,
    Cards = 1,
    List = 2,
    Table = 3,
    Tiles = 4,
    Gallery = 5,
    Masonry = 6
}

/// <summary>
/// Context information for grid components to share type and configuration data
/// </summary>
public class GridContext
{
    public Type ItemType { get; }
    public string GridId { get; }
    public bool IsSmartGrid { get; }
    public GridLayoutMode CurrentMode { get; set; }
    public int CurrentPage { get; set; } = 1;
    public int PageSize { get; set; } = 24;
    public int TotalItems { get; set; }
    public bool IsFiltered { get; set; }

    public GridContext(Type itemType, string gridId, bool isSmartGrid = false)
    {
        ItemType = itemType ?? throw new ArgumentNullException(nameof(itemType));
        GridId = gridId ?? throw new ArgumentNullException(nameof(gridId));
        IsSmartGrid = isSmartGrid;
        CurrentMode = GridLayoutMode.Auto;
    }
    
    public int TotalPages => Math.Max(1, (int)Math.Ceiling((double)TotalItems / PageSize));
    
    public bool CanGoToPrevious => CurrentPage > 1;
    
    public bool CanGoToNext => CurrentPage < TotalPages;
    
    public int GetStartIndex()
    {
        if (TotalItems == 0) return 0;
        return ((CurrentPage - 1) * PageSize) + 1;
    }
    
    public int GetEndIndex()
    {
        if (TotalItems == 0) return 0;
        return Math.Min(CurrentPage * PageSize, TotalItems);
    }
}