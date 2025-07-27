using Microsoft.JSInterop;

namespace RR.Blazor.Services;

/// <summary>
/// Service to manage modal stacking and z-index coordination for nested modals
/// </summary>
public class ModalStackService(IJSRuntime jsRuntime)
{
    private readonly List<string> activeModals = new();
    private bool scrollLocked;

    /// <summary>
    /// Register a modal as active and get its z-index
    /// </summary>
    public async Task<int> RegisterModalAsync(string modalId)
    {
        if (!activeModals.Contains(modalId))
        {
            activeModals.Add(modalId);
        }

        // Register with JS modal manager
        await jsRuntime.InvokeVoidAsync("RRBlazor.Modal.register", modalId);

        // Calculate z-index based on stack position
        var stackPosition = activeModals.IndexOf(modalId);
        var baseZIndex = 1050; // Start above all CSS modal z-index values (--z-modal-content: 1040)
        return baseZIndex + (stackPosition * 50); // Larger gaps to avoid conflicts
    }

    /// <summary>
    /// Unregister a modal and manage scroll lock
    /// </summary>
    public async Task UnregisterModalAsync(string modalId)
    {
        activeModals.Remove(modalId);

        // Unregister with JS modal manager
        await jsRuntime.InvokeVoidAsync("RRBlazor.Modal.unregister", modalId);
    }

    /// <summary>
    /// Check if this modal is the top-most (should respond to backdrop/escape)
    /// </summary>
    public bool IsTopModal(string modalId)
    {
        return activeModals.Count > 0 && activeModals.Last() == modalId;
    }

    /// <summary>
    /// Get count of active modals
    /// </summary>
    public int ActiveModalCount => activeModals.Count;

    /// <summary>
    /// Get stack position of modal (0-based)
    /// </summary>
    public int GetStackPosition(string modalId)
    {
        return activeModals.IndexOf(modalId);
    }

    /// <summary>
    /// Force unlock scroll (for cleanup scenarios)
    /// </summary>
    public async Task ForceUnlockAsync()
    {
        activeModals.Clear();
        await jsRuntime.InvokeVoidAsync("RRBlazor.Modal.forceUnlock");
        scrollLocked = false;
    }
}