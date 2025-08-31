namespace RR.Blazor.Enums
{
    /// <summary>
    /// Unified direction enum for all components (progress, dropdowns, animations, etc.)
    /// </summary>
    public enum Direction
    {
        /// <summary>Auto-detect direction based on context/available space</summary>
        Auto = 9,
        
        /// <summary>Primary directions</summary>
        Up = 12,
        Down = 13,
        Left = 14,
        Right = 15,
        
        /// <summary>Diagonal directions</summary>
        UpLeft = 18,
        UpRight = 19,
        DownLeft = 20,
        DownRight = 21,
        
        /// <summary>Special orientations</summary>
        Horizontal = 24,
        Vertical = 25
    }
    
    /// <summary>
    /// Simple orientation enum for basic horizontal/vertical layouts
    /// </summary>
    public enum Orientation
    {
        Horizontal,
        Vertical
    }

    /// <summary>
    /// Action group direction for layout
    /// </summary>
    public enum ActionGroupDirection
    {
        Horizontal = 24, // Match Direction.Horizontal
        Vertical = 25    // Match Direction.Vertical
    }

    /// <summary>
    /// Dropdown direction for positioning
    /// </summary>
    public enum DropdownDirection
    {
        Auto = 9,     // Match Direction.Auto
        Down = 13,    // Match Direction.Down
        Up = 12,      // Match Direction.Up
        Left = 14,    // Match Direction.Left
        Right = 15    // Match Direction.Right
    }

    /// <summary>
    /// Extension methods for Direction enum conversions
    /// </summary>
    public static class DirectionExtensions
    {
        public static Direction ToDirection(this ActionGroupDirection direction)
        {
            return (Direction)(int)direction;
        }

        public static Direction ToDirection(this DropdownDirection direction)
        {
            return (Direction)(int)direction;
        }

        public static ActionGroupDirection ToActionGroupDirection(this Direction direction)
        {
            return direction switch
            {
                Direction.Horizontal => ActionGroupDirection.Horizontal,
                Direction.Vertical => ActionGroupDirection.Vertical,
                _ => ActionGroupDirection.Horizontal
            };
        }

        public static DropdownDirection ToDropdownDirection(this Direction direction)
        {
            return direction switch
            {
                Direction.Auto => DropdownDirection.Auto,
                Direction.Down => DropdownDirection.Down,
                Direction.Up => DropdownDirection.Up,
                Direction.Left => DropdownDirection.Left,
                Direction.Right => DropdownDirection.Right,
                _ => DropdownDirection.Auto
            };
        }
    }
}