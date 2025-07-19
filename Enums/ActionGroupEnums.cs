namespace RR.Blazor.Enums;

/// <summary>
/// Direction variants for action groups
/// </summary>
public enum ActionGroupDirection
{
    Horizontal,
    Vertical
}

/// <summary>
/// Alignment variants for action groups
/// </summary>
public enum ActionGroupAlignment
{
    Start,
    Center,
    End,
    SpaceBetween,
    SpaceAround,
    SpaceEvenly
}

/// <summary>
/// Spacing variants for action groups
/// </summary>
public enum ActionGroupSpacing
{
    None,
    Small,
    Medium,
    Large,
    ExtraLarge
}

/// <summary>
/// Standard action patterns for common button combinations
/// </summary>
public enum ActionGroupPattern
{
    /// <summary>No pattern - use custom child content</summary>
    None,
    /// <summary>Cancel and Save buttons for forms</summary>
    CancelSave,
    /// <summary>Back and Next buttons for wizards</summary>
    BackNext,
    /// <summary>Approve and Reject buttons for approval workflows</summary>
    ApproveReject
}