namespace RR.Blazor.Enums
{
    /// <summary>
    /// Column alignment options for table cells and headers
    /// Provides semantic alignment based on content type
    /// </summary>
    public enum ColumnAlignment
    {
        /// <summary>Auto-detect alignment based on data type (text=left, numbers=right, etc.)</summary>
        Auto,
        
        /// <summary>Left alignment - ideal for text content</summary>
        Left,
        
        /// <summary>Center alignment - ideal for short labels, status badges</summary>
        Center,
        
        /// <summary>Right alignment - ideal for numbers, amounts, dates</summary>
        Right,
        
        /// <summary>Justify text alignment - fills the available width</summary>
        Justify
    }
}