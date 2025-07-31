namespace RR.Blazor.Enums
{
    /// <summary>
    /// Defines the type of filter to apply to a column
    /// </summary>
    public enum FilterType
    {
        /// <summary>AI-powered automatic filter type detection</summary>
        Auto = 0,
        /// <summary>Text-based filtering with multiple operators</summary>
        Text = 1,
        /// <summary>Numeric filtering with comparison operators</summary>
        Number = 2,
        /// <summary>Date filtering with range and specific date options</summary>
        Date = 3,
        /// <summary>Boolean true/false filtering</summary>
        Boolean = 4,
        /// <summary>Single selection from predefined options</summary>
        Select = 5,
        /// <summary>Multiple selection from predefined options</summary>
        MultiSelect = 6,
        /// <summary>Date range filtering with start and end dates</summary>
        DateRange = 7,
        /// <summary>Custom filter implementation</summary>
        Custom = 8
    }

    /// <summary>
    /// Defines the filtering mode for the entire table
    /// </summary>
    public enum FilterMode
    {
        /// <summary>AI-powered smart filtering with auto-detection</summary>
        Smart = 0,
        /// <summary>Simple global search with basic filters</summary>
        Simple = 1,
        /// <summary>Per-column dropdown filter menus</summary>
        ColumnMenu = 2,
        /// <summary>Inline filtering row below headers</summary>
        ColumnRow = 3
    }

    /// <summary>
    /// Defines filter operators for different data types
    /// </summary>
    public enum FilterOperator
    {
        // Text operators
        Contains = 0,
        StartsWith = 1,
        EndsWith = 2,
        Equals = 3,
        NotEquals = 4,
        IsEmpty = 5,
        IsNotEmpty = 6,
        
        // Number operators
        GreaterThan = 10,
        LessThan = 11,
        GreaterThanOrEqual = 12,
        LessThanOrEqual = 13,
        Between = 14,
        NotBetween = 15,
        
        // Date operators
        On = 20,
        Before = 21,
        After = 22,
        OnOrBefore = 23,
        OnOrAfter = 24,
        InRange = 25,
        Today = 26,
        Yesterday = 27,
        ThisWeek = 28,
        LastWeek = 29,
        ThisMonth = 30,
        LastMonth = 31,
        ThisYear = 32,
        LastYear = 33,
        
        // Boolean operators
        IsTrue = 40,
        IsFalse = 41,
        
        // Collection operators
        In = 50,
        NotIn = 51,
        Any = 52,
        All = 53
    }

    /// <summary>
    /// Defines logical operators for combining multiple filters
    /// </summary>
    public enum FilterLogic
    {
        And = 0,
        Or = 1
    }

    /// <summary>
    /// Defines sticky column positioning options
    /// </summary>
    public enum StickyPosition
    {
        /// <summary>Sticky to the left side of the table</summary>
        Left = 0,
        /// <summary>Sticky to the right side of the table</summary>
        Right = 1
    }

    /// <summary>
    /// Defines column management actions that can be performed
    /// </summary>
    public enum ColumnManagementAction
    {
        Show = 0,
        Hide = 1,
        Pin = 2,
        Unpin = 3,
        Resize = 4,
        Reorder = 5
    }

    /// <summary>
    /// Defines virtualization modes for table data loading
    /// </summary>
    public enum VirtualizationMode
    {
        Auto,           // Decide based on data size
        ClientSide,     // All data loaded, virtualized rendering
        ServerSide,     // Data loaded on demand
        Hybrid          // Combination approach
    }
}