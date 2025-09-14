namespace RR.Blazor.Models;

/// <summary>
/// Navigation menu item with full feature support
/// </summary>
public class NavMenuItem
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Text { get; set; } = string.Empty;
    public string Icon { get; set; }
    public string Href { get; set; } = "#";
    public bool Visible { get; set; } = true;
    public bool IsHighlighted { get; set; }
    public bool IsDivider { get; set; }
    public bool MatchExact { get; set; }
    public NavMenuBadge Badge { get; set; }
    public List<NavMenuItem> Children { get; set; }

    public string RequiredPermission { get; set; }
    public string[] AllowedRoles { get; set; }
    public bool RequiresAuth { get; set; } = false;
    public Func<Task> OnClickAction { get; set; }
    public string Tooltip { get; set; }
    public bool OpenInNewTab { get; set; }
    public string Shortcut { get; set; }
    public bool IsFavorite { get; set; }
    public string[] SearchKeywords { get; set; }
    public string CustomClass { get; set; }
    public bool ShowLoadingIndicator { get; set; } = true;
    public bool IsDisabled { get; set; }

    /// <summary>Check if user can access this item</summary>
    public bool CanAccess(AppUser user, string[] userPermissions = null)
    {
        if (!Visible) return false;

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
/// Navigation menu badge
/// </summary>
public class NavMenuBadge
{
    public string Text { get; set; } = string.Empty;
    public string Type { get; set; } = "info"; // info, success, warning, error
}