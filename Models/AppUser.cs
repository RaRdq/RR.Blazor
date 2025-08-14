namespace RR.Blazor.Models;

/// <summary>
/// Generic user model for RAppShell user management
/// </summary>
public class AppUser
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Avatar { get; set; }
    public string Role { get; set; }
    public bool IsOnline { get; set; } = true;
    public bool IsAuthenticated { get; set; } = true;
    public DateTime LastActivity { get; set; }
    public Dictionary<string, object> Metadata { get; set; } = new();
    
    /// <summary>Get initials from name for avatar</summary>
    public string GetInitials()
    {
        if (string.IsNullOrEmpty(Name)) return "?";
        
        var parts = Name.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 1)
            return parts[0].Substring(0, Math.Min(2, parts[0].Length)).ToUpper();
        
        return $"{parts[0].Substring(0, 1)}{parts[parts.Length - 1].Substring(0, 1)}".ToUpper();
    }
    
    /// <summary>Get display name with fallback</summary>
    public string GetDisplayName() => !string.IsNullOrEmpty(Name) ? Name : Email;
}