using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using RR.Blazor.Enums;
using RR.Blazor.Models;
using RR.Blazor.Services;
using System;
using System.Threading.Tasks;

namespace RR.Blazor.Components.Base;

public abstract class RModalBase : RComponentBase
{
    [Inject] protected IModalService ModalService { get; set; }
    
    [Parameter] public string ModalId { get; set; }
    [Parameter] public bool Visible { get; set; }
    [Parameter] public EventCallback<bool> VisibleChanged { get; set; }
    [Parameter] public EventCallback OnClose { get; set; }
    [Parameter] public EventCallback OnCancel { get; set; }
    [Parameter] public EventCallback OnShow { get; set; }
    [Parameter] public EventCallback OnHide { get; set; }
    
    [Parameter] public bool UseModalService { get; set; } = true;
    [Parameter] public bool RegisterWithService { get; set; } = true;
    
    protected bool IsServiceManaged => !string.IsNullOrEmpty(ModalId) && UseModalService;
    protected string InternalModalId { get; set; }
    
    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        
        if (string.IsNullOrEmpty(ModalId))
        {
            InternalModalId = $"modal-{Guid.NewGuid():N}";
        }
        else
        {
            InternalModalId = ModalId;
        }
        
        if (RegisterWithService && ModalService != null && IsServiceManaged)
        {
            RegisterModalWithService();
        }
    }
    
    protected virtual void RegisterModalWithService()
    {
        // Override in derived classes to register specific modal types
    }
    
    public virtual async Task ShowAsync()
    {
        if (Visible) return;
        
        Visible = true;
        await VisibleChanged.InvokeAsync(Visible);
        await OnShow.InvokeAsync();
        
        StateHasChanged();
    }
    
    public virtual async Task HideAsync()
    {
        if (!Visible) return;
        
        Visible = false;
        await VisibleChanged.InvokeAsync(Visible);
        await OnHide.InvokeAsync();
        
        if (IsServiceManaged)
        {
            await NotifyServiceModalClosed();
        }
        
        StateHasChanged();
    }
    
    public virtual async Task CloseAsync()
    {
        await HideAsync();
        await OnClose.InvokeAsync();
    }
    
    public virtual async Task CancelAsync()
    {
        await HideAsync();
        await OnCancel.InvokeAsync();
    }
    
    protected virtual async Task NotifyServiceModalClosed()
    {
        if (ModalService != null && IsServiceManaged)
        {
            try
            {
                await ModalService.CloseAsync(InternalModalId, Enums.ModalResult.None);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to close modal via service: {InternalModalId}", ex);
            }
        }
    }
    
    protected virtual ModalOptions GetModalOptions()
    {
        return new ModalOptions
        {
            ComponentType = GetType(),
            Parameters = []
        };
    }
    
    protected async Task HandleBackdropClick()
    {
        if (GetModalOptions().CloseOnBackdrop)
        {
            await CloseAsync();
        }
    }
    
    protected async Task HandleEscapeKey()
    {
        if (GetModalOptions().CloseOnEscape)
        {
            await CloseAsync();
        }
    }
}