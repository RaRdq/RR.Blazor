namespace RR.Blazor.Models;

/// <summary>
/// Navigation menu item
/// </summary>
public class NavMenuItem
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Text { get; set; } = string.Empty;
    public string Icon { get; set; }
    public string Href { get; set; } = "#";
    public bool IsVisible { get; set; } = true;
    public bool IsHighlighted { get; set; }
    public bool IsDivider { get; set; }
    public bool MatchExact { get; set; }
    public NavMenuBadge Badge { get; set; }
    public List<NavMenuItem> Children { get; set; }
}

/// <summary>
/// Navigation menu badge
/// </summary>
public class NavMenuBadge
{
    public string Text { get; set; } = string.Empty;
    public string Type { get; set; } = "info"; // info, success, warning, error
}