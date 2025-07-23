namespace RR.Blazor.Components.Data;

/// <summary>
/// Interface for table components that can accept column definitions
/// </summary>
public interface ITableParent
{
    /// <summary>
    /// The item type this table works with
    /// </summary>
    Type ItemType { get; }

    /// <summary>
    /// Add a column definition to the table
    /// </summary>
    /// <param name="columnInfo">Column information dictionary or typed column object</param>
    void AddColumn(object columnInfo);

    /// <summary>
    /// Update an existing column definition
    /// </summary>
    /// <param name="columnInfo">Updated column information</param>
    void UpdateColumn(object columnInfo);
}