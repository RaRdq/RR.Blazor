namespace RR.Blazor.Enums;

/// <summary>
/// Features that can be enabled/disabled in the app shell
/// </summary>
[Flags]
public enum AppShellFeatures
{
    None = 0,
    Header = 1,
    Sidebar = 2,
    Search = 4,
    Notifications = 8,
    ThemeToggle = 16,
    UserMenu = 32,
    SidebarToggle = 64,
    Breadcrumbs = 128,
    StatusBar = 256,
    QuickActions = 512,
    Toasts = 1024,
    
    // Predefined combinations
    Minimal = Header | Sidebar,
    Standard = Header | Sidebar | Search | ThemeToggle | UserMenu | SidebarToggle | Toasts,
    All = Header | Sidebar | Search | Notifications | ThemeToggle | UserMenu | SidebarToggle | Breadcrumbs | StatusBar | QuickActions | Toasts
}

/// <summary>
/// App shell layout density options
/// </summary>
public enum AppShellDensity
{
    Comfortable,
    Compact,
    Dense
}

/// <summary>
/// App shell sidebar behavior options
/// </summary>
public enum SidebarBehavior
{
    Auto,
    AlwaysVisible,
    AlwaysCollapsed,
    Mobile
}