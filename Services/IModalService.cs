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
    Task<Models.ModalResult> ShowAsync(Type componentType, Dictionary<string, object> parameters = null, ModalOptions options = null);
    Task<ModalResult<T>> ShowRawAsync<T>(Type componentType, Dictionary<string, object> parameters = null, ModalOptions<T> options = null);
    
    // Modal management
    Task CloseAsync(string modalId, Enums.ModalResult result = Enums.ModalResult.None);
    Task CloseAllAsync();
    bool IsModalOpen(string modalId = null);
    
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
    IModalBuilder<T> WithVariant(VariantType variant);
    IModalBuilder<T> WithComponent<TComponent>() where TComponent : ComponentBase;
    IModalBuilder<T> WithComponent(Type componentType);
    IModalBuilder<T> WithParameter(string name, object value);
    IModalBuilder<T> WithParameters(Dictionary<string, object> parameters);
    IModalBuilder<T> WithData(T data);
    IModalBuilder<T> WithButton(ModalButton button);
    IModalBuilder<T> WithButton(string text, VariantType variant, Func<T, Task<bool>> onClick = null);
    IModalBuilder<T> WithCloseButton(bool show = true);
    IModalBuilder<T> WithBackdropClose(bool allow = true);
    IModalBuilder<T> WithEscapeClose(bool allow = true);
    IModalBuilder<T> WithClass(string Class);
    IModalBuilder<T> WithAutoClose(TimeSpan delay);
    Task<ModalResult<T>> ShowAsync();
}