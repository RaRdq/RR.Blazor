using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace RR.Blazor.Components.Base;

/// <summary>
/// Simplified base class for components using the unified portal system.
/// This base provides common popup state management while delegating positioning to portal.js
/// </summary>
public abstract class RPopupComponentBase : RComponentBase, IAsyncDisposable
{
    [Inject] protected IJSRuntime JSRuntime { get; set; } = default!;
    
    [Parameter] public bool DisableClickOutside { get; set; }
    [Parameter] public bool DisableEscapeKey { get; set; }
    [Parameter] public EventCallback OnPopupOpen { get; set; }
    [Parameter] public EventCallback OnPopupClose { get; set; }
    [Parameter] public EventCallback<bool> OnPopupStateChanged { get; set; }
    
    protected string? portalId;
    protected bool isPopupOpen;
    protected DotNetObjectReference<RPopupComponentBase>? dotNetRef;
    
    protected override void OnInitialized()
    {
        base.OnInitialized();
        dotNetRef = DotNetObjectReference.Create(this);
        portalId = $"portal-{Guid.NewGuid():N}";
    }
    
    /// <summary>
    /// Open the popup - derived classes should override to create portal
    /// </summary>
    protected virtual async Task OpenPopup()
    {
        if (isPopupOpen) return;
        
        isPopupOpen = true;
        await OnPopupOpen.InvokeAsync();
        await OnPopupStateChanged.InvokeAsync(true);
        StateHasChanged();
    }
    
    /// <summary>
    /// Close the popup
    /// </summary>
    protected virtual async Task ClosePopup()
    {
        if (!isPopupOpen) return;
        
        isPopupOpen = false;
        
        if (!string.IsNullOrEmpty(portalId))
        {
            try
            {
                await JSRuntime.InvokeVoidAsync("RRBlazor.Portal.destroy", portalId);
            }
            catch
            {
                // Portal might already be destroyed
            }
        }
        
        await OnPopupClose.InvokeAsync();
        await OnPopupStateChanged.InvokeAsync(false);
        StateHasChanged();
    }
    
    /// <summary>
    /// Toggle popup state
    /// </summary>
    protected virtual async Task TogglePopup()
    {
        if (isPopupOpen)
            await ClosePopup();
        else
            await OpenPopup();
    }
    
    /// <summary>
    /// Handle click outside event from JavaScript
    /// </summary>
    [JSInvokable]
    public virtual async Task HandleClickOutside()
    {
        if (!DisableClickOutside)
            await ClosePopup();
    }
    
    /// <summary>
    /// Handle escape key event from JavaScript
    /// </summary>
    [JSInvokable]
    public virtual async Task HandleEscapeKey()
    {
        if (!DisableEscapeKey)
            await ClosePopup();
    }
    
    public virtual async ValueTask DisposeAsync()
    {
        try
        {
            if (!string.IsNullOrEmpty(portalId))
            {
                await JSRuntime.InvokeVoidAsync("RRBlazor.Portal.destroy", portalId);
            }
        }
        catch
        {
            // Ignore disposal errors
        }
        
        dotNetRef?.Dispose();
    }
}