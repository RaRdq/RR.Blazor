using RR.Blazor.Components;

namespace RR.Blazor.Models;

/// <summary>
/// Enhanced navigation item for RAppShell with additional features
/// </summary>
public class AppNavItem : NavMenuItem
{
    /// <summary>Permission required to view this item</summary>
    public string RequiredPermission { get; set; }
    
    /// <summary>Roles allowed to view this item</summary>
    public string[] AllowedRoles { get; set; }
    
    /// <summary>Whether this item requires authentication</summary>
    public bool RequiresAuth { get; set; } = true;
    
    /// <summary>Custom action to execute on click (instead of navigation)</summary>
    public Func<Task> OnClickAction { get; set; }
    
    /// <summary>Tooltip text for the item</summary>
    public string Tooltip { get; set; }
    
    /// <summary>Whether to open in new tab/window</summary>
    public bool OpenInNewTab { get; set; }
    
    /// <summary>Keyboard shortcut</summary>
    public string Shortcut { get; set; }
    
    /// <summary>Whether this is a favorite/pinned item</summary>
    public bool IsFavorite { get; set; }
    
    /// <summary>Search keywords for this item</summary>
    public string[] SearchKeywords { get; set; }
    
    /// <summary>Custom CSS class for this specific item</summary>
    public string CustomClass { get; set; }
    
    /// <summary>Whether to show loading indicator when navigating</summary>
    public bool ShowLoadingIndicator { get; set; } = true;
    
    /// <summary>Check if user can access this item</summary>
    public bool CanAccess(AppUser user, string[] userPermissions = null)
    {
        if (!IsVisible) return false;
        
        if (RequiresAuth && (user == null || !user.IsAuthenticated))
            return false;
        
        if (AllowedRoles?.Length > 0 && user != null)
        {
            if (string.IsNullOrEmpty(user.Role) || !AllowedRoles.Contains(user.Role))
                return false;
        }
        
        if (!string.IsNullOrEmpty(RequiredPermission) && userPermissions != null)
        {
            if (!userPermissions.Contains(RequiredPermission))
                return false;
        }
        
        return true;
    }
}

/// <summary>
/// Factory for creating common app navigation items
/// </summary>
public static class AppNavItems
{
    public static AppNavItem Dashboard() => new()
    {
        Id = "dashboard",
        Text = "Dashboard",
        Icon = "dashboard",
        Href = "/",
        MatchExact = true,
        SearchKeywords = new[] { "home", "overview", "summary" }
    };
    
    public static AppNavItem Settings() => new()
    {
        Id = "settings",
        Text = "Settings",
        Icon = "settings",
        Href = "/settings",
        SearchKeywords = new[] { "preferences", "configuration", "options" }
    };
    
    public static AppNavItem Profile() => new()
    {
        Id = "profile",
        Text = "Profile",
        Icon = "person",
        Href = "/profile",
        SearchKeywords = new[] { "account", "user", "personal" }
    };
    
    public static AppNavItem Logout() => new()
    {
        Id = "logout",
        Text = "Logout",
        Icon = "logout",
        Href = "/logout",
        IsHighlighted = true,
        SearchKeywords = new[] { "sign out", "exit" }
    };
    
    public static AppNavItem Admin() => new()
    {
        Id = "admin",
        Text = "Administration",
        Icon = "admin_panel_settings",
        Href = "/admin",
        AllowedRoles = new[] { "Admin", "SuperAdmin" },
        SearchKeywords = new[] { "management", "system", "users" }
    };
}