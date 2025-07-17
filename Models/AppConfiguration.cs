using RR.Blazor.Enums;

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
    
    /// <summary>
    /// Gets the default features based on configuration settings
    /// </summary>
    public AppShellFeatures GetDefaultFeatures()
    {
        var features = AppShellFeatures.Header | AppShellFeatures.Sidebar | AppShellFeatures.SidebarToggle | AppShellFeatures.Toasts;
        
        if (ShowSearch) features |= AppShellFeatures.Search;
        if (ShowNotifications) features |= AppShellFeatures.Notifications;
        if (ShowUserMenu) features |= AppShellFeatures.UserMenu;
        if (ShowBreadcrumbs) features |= AppShellFeatures.Breadcrumbs;
        if (ShowStatusBar) features |= AppShellFeatures.StatusBar;
        if (ShowQuickActions) features |= AppShellFeatures.QuickActions;
        
        // Theme toggle is typically always available
        features |= AppShellFeatures.ThemeToggle;
        
        return features;
    }
}