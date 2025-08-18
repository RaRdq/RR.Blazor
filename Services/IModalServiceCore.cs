using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using RR.Blazor.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RR.Blazor.Services;

public interface IModalServiceCore
{
    Task<ModalResult<TResult>> ShowAsync<TModal, TParams, TResult>(
        TParams parameters = default,
        ModalOptions options = null,
        ModalEvents<TResult> events = null) where TModal : ComponentBase;
    
    Task<ModalResult<TResult>> ShowAsync<TResult>(
        Type modalType,
        Dictionary<string, object> parameters = null,
        ModalOptions options = null,
        ModalEvents<TResult> events = null);
    
    Task<Models.ModalResult> ShowAsync(
        Type modalType,
        Dictionary<string, object> parameters = null,
        ModalOptions options = null,
        ModalEvents events = null);
    
    Task CloseAsync(string modalId, Enums.ModalResult result = Enums.ModalResult.None);
    Task CloseAllAsync();
    
    bool IsModalOpen(string modalId = null);
    bool HasVisibleModals { get; }
    IEnumerable<ModalInstance> ActiveModals { get; }
    
    event Action<ModalInstance> OnModalOpened;
    event Action<ModalInstance> OnModalClosed;
    event Action OnAllModalsClosed;
    
    IModalBuilder<T> Create<T>();
    IJSRuntime JSRuntime { get; }
}