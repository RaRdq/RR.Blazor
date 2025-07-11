namespace RR.Blazor.Models;

/// <summary>
/// App configuration model for RAppShell
/// </summary>
public class AppConfiguration
{
    public string Title { get; set; } = "RR.Blazor App";
    public string Logo { get; set; }
    public string FavIcon { get; set; }
    public ThemeConfiguration Theme { get; set; } = ThemeConfiguration.Default;
    public bool ShowSearch { get; set; } = true;
    public bool ShowNotifications { get; set; } = true;
    public bool ShowUserMenu { get; set; } = true;
    public bool ShowBreadcrumbs { get; set; } = true;
    public bool ShowStatusBar { get; set; } = false;
    public bool ShowQuickActions { get; set; } = false;
    public bool EnableKeyboardShortcuts { get; set; } = true;
    public bool CollapseSidebarOnMobile { get; set; } = true;
    public bool RememberSidebarState { get; set; } = true;
    public string DefaultRoute { get; set; } = "/";
    public Dictionary<string, object> CustomSettings { get; set; } = new();
}