using Microsoft.AspNetCore.Components;
using RR.Blazor.Enums;
using RR.Blazor.Models;

namespace RR.Blazor.Services;

public interface IModalService
{
    // Core modal methods
    Task<ModalResult<T>> ShowAsync<T>(ModalOptions<T> options);
    Task<Models.ModalResult> ShowAsync(ModalOptions options);
    Task<ModalResult<T>> ShowAsync<T>(Type componentType, Dictionary<string, object> parameters = null, ModalOptions<T> options = null);
    
    // Confirmation modals
    Task<bool> ConfirmAsync(string message, string title = "Confirm", bool isDestructive = false);
    Task<bool> ConfirmAsync(ConfirmationOptions options);
    Task<Models.ModalResult> ConfirmWithResultAsync(string message, string title = "Confirm", ModalVariant variant = ModalVariant.Default);
    
    // Form modals
    Task<ModalResult<T>> ShowFormAsync<T>(string title, T initialData = default, SizeType size = SizeType.Medium);
    Task<ModalResult<T>> ShowFormAsync<T>(FormModalOptions<T> options);
    
    // Quick modals
    Task ShowInfoAsync(string message, string title = "Information");
    Task ShowWarningAsync(string message, string title = "Warning");
    Task ShowErrorAsync(string message, string title = "Error");
    Task ShowSuccessAsync(string message, string title = "Success");
    
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
    
    // Builder pattern
    IModalBuilder<T> Create<T>();
    
    // Confirmation modal callbacks
    void ConfirmModal(string modalId);
    void CancelModal(string modalId);
    
    // Events
    event Action<ModalInstance> OnModalOpened;
    event Action<ModalInstance> OnModalClosed;
    event Action OnAllModalsClosed;
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