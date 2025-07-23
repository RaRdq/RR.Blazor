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

        // Lock scroll only when first modal opens
        if (activeModals.Count == 1 && !scrollLocked)
        {
            await LockScrollAsync();
        }

        // Calculate z-index based on stack position
        var stackPosition = activeModals.IndexOf(modalId);
        var baseZIndex = 1000; // --z-modal-backdrop base value
        return baseZIndex + (stackPosition * 10);
    }

    /// <summary>
    /// Unregister a modal and manage scroll lock
    /// </summary>
    public async Task UnregisterModalAsync(string modalId)
    {
        activeModals.Remove(modalId);

        // Unlock scroll only when all modals are closed
        if (activeModals.Count == 0 && scrollLocked)
        {
            await UnlockScrollAsync();
        }
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

    private async Task LockScrollAsync()
    {
        try
        {
            await jsRuntime.InvokeVoidAsync("eval", 
                "document.body.classList.add('modal-open'); " +
                "document.body.style.setProperty('--scrollbar-width', (window.innerWidth - document.documentElement.clientWidth) + 'px');");
            scrollLocked = true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error locking scroll: {ex.Message}");
        }
    }

    private async Task UnlockScrollAsync()
    {
        try
        {
            await jsRuntime.InvokeVoidAsync("eval", 
                "document.body.classList.remove('modal-open'); " +
                "document.body.style.removeProperty('--scrollbar-width');");
            scrollLocked = false;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error unlocking scroll: {ex.Message}");
        }
    }
}