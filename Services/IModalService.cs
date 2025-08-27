using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using RR.Blazor.Enums;
using RR.Blazor.Models;

namespace RR.Blazor.Services;

public interface IModalService
{
    // Core modal methods (from IModalServiceCore)
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
    
    // High-level modal methods (original IModalService)
    Task<ModalResult<T>> ShowAsync<T>(ModalOptions<T> options);
    Task<Models.ModalResult> ShowAsync(ModalOptions options);
    Task<ModalResult<T>> ShowAsync<T>(Type componentType, Dictionary<string, object> parameters = null, ModalOptions<T> options = null);
    
    // Form modals
    Task<ModalResult<T>> ShowFormAsync<T>(string title, T initialData = default, SizeType size = SizeType.Medium);
    Task<ModalResult<T>> ShowFormAsync<T>(FormModalOptions<T> options);
    
    // Detail/preview modals
    Task ShowDetailAsync<T>(T data, string title = "", SizeType size = SizeType.Large);
    Task ShowPreviewAsync(string content, string title = "Preview", string contentType = "text/plain");
    
    // Selection modals
    Task<T> ShowSelectAsync<T>(IEnumerable<T> items, string title = "Select Item", Func<T, string> displaySelector = null);
    Task<IEnumerable<T>> ShowMultiSelectAsync<T>(IEnumerable<T> items, string title = "Select Items", Func<T, string> displaySelector = null);
    
    // Modal management
    Task CloseAsync(string modalId, Enums.ModalResult result = Enums.ModalResult.None);
    Task CloseAllAsync();
    bool IsModalOpen(string modalId = null);
    bool HasVisibleModals { get; }
    IEnumerable<ModalInstance> ActiveModals { get; }
    
    // Strongly-typed modal methods
    Task<ModalResult<TResult>> ShowAsync<TModal, TParameters, TResult>(
        TParameters parameters = default,
        ModalOptions options = null) 
        where TModal : ComponentBase 
        where TParameters : IModalParameters, new();
    
    // Builder pattern
    IModalBuilder<T> Create<T>();
    
    // Events
    event Action<ModalInstance> OnModalOpened;
    event Action<ModalInstance> OnModalClosed;
    event Action OnAllModalsClosed;
    
    // Access to JS runtime (from IModalServiceCore)
    IJSRuntime JSRuntime { get; }
}

public interface IModalBuilder<T>
{
    IModalBuilder<T> WithTitle(string title);
    IModalBuilder<T> WithSubtitle(string subtitle);
    IModalBuilder<T> WithIcon(string icon);
    IModalBuilder<T> WithSize(SizeType size);
    IModalBuilder<T> WithVariant(ModalVariant variant);
    IModalBuilder<T> WithComponent<TComponent>() where TComponent : ComponentBase;
    IModalBuilder<T> WithComponent(Type componentType);
    IModalBuilder<T> WithParameter(string name, object value);
    IModalBuilder<T> WithParameters(Dictionary<string, object> parameters);
    IModalBuilder<T> WithData(T data);
    IModalBuilder<T> WithButton(ModalButton button);
    IModalBuilder<T> WithButton(string text, ModalButtonType type, Func<T, Task<bool>> onClick = null);
    IModalBuilder<T> WithCloseButton(bool show = true);
    IModalBuilder<T> WithBackdropClose(bool allow = true);
    IModalBuilder<T> WithEscapeClose(bool allow = true);
    IModalBuilder<T> WithClass(string Class);
    IModalBuilder<T> WithAutoClose(TimeSpan delay);
    Task<ModalResult<T>> ShowAsync();
}