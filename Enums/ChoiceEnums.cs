namespace RR.Blazor.Enums;

public enum ChoiceVariant
{
    Auto,       // Smart detection based on content and context
    Inline,     // Always show as inline switcher
    Dropdown    // Always show as dropdown
}

public enum ChoiceStyle
{
    Standard,   // Default styling
    Compact,    // Compact layout with no gaps
    Pills,      // Pill-shaped items
    Tabs,       // Tab-style items
    Buttons     // Button-style items
}

public enum ChoiceDirection
{
    Horizontal,
    Vertical
}