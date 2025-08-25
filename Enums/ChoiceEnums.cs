namespace RR.Blazor.Enums;

public enum ChoiceVariant
{
    Auto,       // Smart detection based on content and context
    Inline,     // Always show as inline switcher
    Dropdown,   // Always show as dropdown
    Grouped,    // Show with grouped headers
    Tree        // Show as hierarchical tree
}

public enum ChoiceType
{
    Standard,   // Default styling
    Compact,    // Compact layout with no gaps
    Pills,      // Pill-shaped items
    Tabs,       // Tab-style items
    Buttons,    // Button-style items
    Cards       // Card-style items for groups
}

public enum ChoiceDirection
{
    Horizontal,
    Vertical
}

public enum ChoiceSelectionMode
{
    Single,     // Single selection (default)
    Multiple,   // Multiple selection with checkboxes
    Cascade     // Tree cascade selection (parent affects children)
}

public enum ChoiceGroupStyle
{
    Header,     // Simple header with line
    Card,       // Card-style group with background
    Divider,    // Divider line with label
    Badge       // Badge-style compact header
}

public enum ChoiceTreeStyle
{
    Standard,   // Standard tree with expand/collapse icons
    Compact,    // Compact tree with minimal spacing
    Lines,      // Tree with connection lines
    Modern      // Modern tree with hover effects
}