using Microsoft.JSInterop;

namespace RR.Blazor.Services;

/// <summary>
/// Centralized portal management service for all portal components
/// Coordinates z-index registry, cleanup, and lifecycle following fail-fast principles
/// </summary>
public sealed class PortalService(IJSRuntime jsRuntime)
{
    private readonly Dictionary<string, PortalInfo> activePortals = new();
    private const int BaseZIndex = 1000;
    private const int ZIndexIncrement = 10;

    /// <summary>
    /// Register a portal and get its z-index with universal interface support
    /// Throws immediately if portal already registered or JS operation fails
    /// </summary>
    public async Task<int> RegisterPortalAsync(string portalId, PortalType portalType, string? parentPortalId = null, 
        bool useBackdrop = false, string backdropClass = "", bool closeOnBackdropClick = true, bool closeOnEscape = true)
    {
        if (string.IsNullOrEmpty(portalId))
            throw new ArgumentException("Portal ID cannot be null or empty", nameof(portalId));

        if (activePortals.ContainsKey(portalId))
            throw new InvalidOperationException($"Portal '{portalId}' is already registered");

        var stackLevel = CalculateStackLevel(parentPortalId);
        var zIndex = BaseZIndex + (stackLevel * ZIndexIncrement);

        var portalInfo = new PortalInfo
        {
            Id = portalId,
            Type = portalType,
            ParentPortalId = parentPortalId,
            ZIndex = zIndex,
            RegisteredAt = DateTime.UtcNow,
            StackLevel = stackLevel,
            UseBackdrop = useBackdrop,
            BackdropClass = backdropClass,
            CloseOnBackdropClick = closeOnBackdropClick,
            CloseOnEscape = closeOnEscape
        };

        activePortals[portalId] = portalInfo;

        var portalConfig = new
        {
            portalId,
            portalType = portalType.ToString().ToLowerInvariant(),
            zIndex,
            useBackdrop,
            backdropClass,
            closeOnBackdropClick,
            closeOnEscape
        };
        
        await jsRuntime.InvokeVoidAsync("RRBlazor.Portal.register", portalConfig);

        return zIndex;
    }

    /// <summary>
    /// Unregister portal and cleanup resources
    /// Throws if portal not found - no silent failures
    /// </summary>
    public async Task UnregisterPortalAsync(string portalId)
    {
        if (string.IsNullOrEmpty(portalId))
            throw new ArgumentException("Portal ID cannot be null or empty", nameof(portalId));

        if (!activePortals.Remove(portalId, out var portalInfo))
            throw new InvalidOperationException($"Portal '{portalId}' is not registered");

        await jsRuntime.InvokeVoidAsync("RRBlazor.Portal.unregister", portalId);
    }

    /// <summary>
    /// Check if portal is the top-most in its parent context
    /// </summary>
    public bool IsTopPortal(string portalId)
    {
        if (!activePortals.TryGetValue(portalId, out var portal))
            return false;

        var siblingsAtSameLevel = activePortals.Values
            .Where(p => p.ParentPortalId == portal.ParentPortalId && p.StackLevel == portal.StackLevel)
            .OrderBy(p => p.RegisteredAt)
            .ToList();

        return siblingsAtSameLevel.LastOrDefault()?.Id == portalId;
    }

    /// <summary>
    /// Get portal information - throws if not found
    /// </summary>
    public PortalInfo GetPortalInfo(string portalId)
    {
        if (!activePortals.TryGetValue(portalId, out var portal))
            throw new InvalidOperationException($"Portal '{portalId}' is not registered");

        return portal;
    }

    /// <summary>
    /// Get all active portals of specific type
    /// </summary>
    public IReadOnlyList<PortalInfo> GetActivePortals(PortalType? portalType = null)
    {
        var portals = activePortals.Values.AsEnumerable();
        
        if (portalType.HasValue)
            portals = portals.Where(p => p.Type == portalType.Value);

        return portals.OrderBy(p => p.ZIndex).ToList();
    }

    /// <summary>
    /// Force cleanup all portals - emergency cleanup
    /// </summary>
    public async Task ForceCleanupAsync()
    {
        var portalIds = activePortals.Keys.ToList();
        activePortals.Clear();

        await jsRuntime.InvokeVoidAsync("RRBlazor.Portal.forceCleanup", portalIds);
    }

    /// <summary>
    /// Get active portal count by type
    /// </summary>
    public int GetActivePortalCount(PortalType? portalType = null)
    {
        return portalType.HasValue 
            ? activePortals.Values.Count(p => p.Type == portalType.Value)
            : activePortals.Count;
    }

    /// <summary>
    /// Calculate stack level based on parent hierarchy
    /// </summary>
    private int CalculateStackLevel(string? parentPortalId)
    {
        if (string.IsNullOrEmpty(parentPortalId))
            return 0;

        if (!activePortals.TryGetValue(parentPortalId, out var parentPortal))
            throw new InvalidOperationException($"Parent portal '{parentPortalId}' is not registered");

        return parentPortal.StackLevel + 1;
    }
}

/// <summary>
/// Portal type enumeration
/// </summary>
public enum PortalType
{
    Modal,
    Dropdown,
    DatePicker,
    Tooltip,
    Autosuggest,
    Popover
}

/// <summary>
/// Portal information record with universal interface support
/// </summary>
public sealed record PortalInfo
{
    public required string Id { get; init; }
    public required PortalType Type { get; init; }
    public string? ParentPortalId { get; init; }
    public required int ZIndex { get; init; }
    public required DateTime RegisteredAt { get; init; }
    public required int StackLevel { get; init; }
    
    // Universal Portal Interface Properties
    public bool UseBackdrop { get; init; } = false;
    public string BackdropClass { get; init; } = "";
    public bool CloseOnBackdropClick { get; init; } = true;
    public bool CloseOnEscape { get; init; } = true;
}