using Microsoft.AspNetCore.Components;
using RR.Blazor.Components;
using RR.Blazor.Components.Feedback;
using RR.Blazor.Enums;
using RR.Blazor.Models;

namespace RR.Blazor.Services;

public sealed class ModalService(IJavaScriptInteropService jsInterop) : IModalService, IDisposable
{
    private readonly List<ModalInstance> _activeModals = [];
    private bool _isDisposed;

    public event Action<ModalInstance> OnModalOpened;
    public event Action<ModalInstance> OnModalClosed;
    public event Action OnAllModalsClosed;

    public async Task<ModalResult<T>> ShowAsync<T>(ModalOptions<T> options)
    {
        var instance = new ModalInstance<T>
        {
            Id = options.ModalId ?? $"modal-{Guid.NewGuid():N}",
            Options = options,
            Visible = true
        };

        _activeModals.Add(instance);
        OnModalOpened?.Invoke(instance);

        if (options.AutoCloseDelay.HasValue)
        {
            _ = Task.Delay(options.AutoCloseDelay.Value).ContinueWith(async _ =>
            {
                if (_activeModals.Contains(instance))
                {
                    await CloseAsync(instance.Id, Enums.ModalResult.Cancel);
                }
            });
        }

        var result = await instance.TaskSource.Task;
        return new() { ResultType = result.ResultType, Data = instance.TypedResult };
    }

    public async Task<Models.ModalResult> ShowAsync(ModalOptions options)
    {
        var instance = new ModalInstance
        {
            Id = options.ModalId ?? $"modal-{Guid.NewGuid():N}",
            Options = options,
            Visible = true
        };

        _activeModals.Add(instance);
        OnModalOpened?.Invoke(instance);

        if (options.AutoCloseDelay.HasValue)
        {
            _ = Task.Delay(options.AutoCloseDelay.Value).ContinueWith(async _ =>
            {
                if (_activeModals.Contains(instance))
                {
                    await CloseAsync(instance.Id, Enums.ModalResult.Cancel);
                }
            });
        }

        return await instance.TaskSource.Task;
    }

    public async Task<ModalResult<T>> ShowAsync<T>(Type componentType, Dictionary<string, object> parameters = null, ModalOptions<T> options = null)
    {
        options ??= new();
        options.ComponentType = componentType;
        options.Parameters = parameters ?? [];
        return await ShowAsync(options);
    }

    public async Task<Models.ModalResult> ShowAsync(Type componentType, Dictionary<string, object> parameters = null, ModalOptions options = null)
    {
        options ??= new();
        options.ComponentType = componentType;
        options.Parameters = parameters ?? [];
        return await ShowAsync(options);
    }

    public async Task<ModalResult<T>> ShowRawAsync<T>(Type componentType, Dictionary<string, object> parameters = null, ModalOptions<T> options = null)
    {
        options ??= new();
        options.ComponentType = componentType;
        options.Parameters = parameters ?? [];
        options.IsRawContent = true;
        return await ShowAsync(options);
    }

    public async Task<Models.ModalResult> ShowRawAsync(Type componentType, Dictionary<string, object> parameters = null, ModalOptions options = null)
    {
        options ??= new();
        options.ComponentType = componentType;
        options.Parameters = parameters ?? [];
        options.IsRawContent = true;
        return await ShowAsync(options);
    }

    public async Task CloseAsync(string modalId, Enums.ModalResult result = Enums.ModalResult.None)
    {
        var modal = _activeModals.FirstOrDefault(m => m.Id == modalId);
        if (modal == null)
            return;

        modal.Visible = false;
        _activeModals.Remove(modal);

        try
        {
            await jsInterop.TryInvokeVoidAsync("RRBlazor.Modal.hide", modalId);
        }
        catch
        {
            // JS hide failed, continue with cleanup
        }

        if (!modal.TaskSource.Task.IsCompleted)
        {
            var modalResult = result != Enums.ModalResult.None
                ? new Models.ModalResult { ResultType = result, Data = modal.Result }
                : new Models.ModalResult { ResultType = Enums.ModalResult.Cancel, Data = modal.Result };
            modal.TaskSource.TrySetResult(modalResult);
        }

        OnModalClosed?.Invoke(modal);

        if (!_activeModals.Any())
        {
            OnAllModalsClosed?.Invoke();
        }
    }

    public async Task CloseAllAsync()
    {
        var modalsToClose = _activeModals.ToList();
        foreach (var modal in modalsToClose)
            await CloseAsync(modal.Id, Enums.ModalResult.Cancel);
    }

    public bool IsModalOpen(string modalId = null) => string.IsNullOrEmpty(modalId)
        ? _activeModals.Any(m => m.Visible)
        : _activeModals.Any(m => m.Id == modalId && m.Visible);

    public IModalBuilder<T> Create<T>() => new ModalBuilder<T>(this);

    public void Dispose()
    {
        if (!_isDisposed)
        {
            _activeModals.Clear();
            _isDisposed = true;
        }
    }
}

public sealed class ModalBuilder<T>(ModalService modalService) : IModalBuilder<T>
{
    private readonly ModalOptions<T> _options = new();

    public IModalBuilder<T> WithTitle(string title)
    {
        _options.Title = title;
        return this;
    }

    public IModalBuilder<T> WithSubtitle(string subtitle)
    {
        _options.Subtitle = subtitle;
        return this;
    }

    public IModalBuilder<T> WithIcon(string icon)
    {
        _options.Icon = icon;
        return this;
    }

    public IModalBuilder<T> WithSize(SizeType size)
    {
        _options.Size = size;
        return this;
    }

    public IModalBuilder<T> WithVariant(VariantType variant)
    {
        _options.Variant = variant;
        return this;
    }

    public IModalBuilder<T> WithComponent<TComponent>() where TComponent : ComponentBase
    {
        _options.ComponentType = typeof(TComponent);
        return this;
    }

    public IModalBuilder<T> WithComponent(Type componentType)
    {
        _options.ComponentType = componentType;
        return this;
    }

    public IModalBuilder<T> WithParameter(string name, object value)
    {
        _options.Parameters[name] = value;
        return this;
    }

    public IModalBuilder<T> WithParameters(Dictionary<string, object> parameters)
    {
        foreach (var (key, value) in parameters)
            _options.Parameters[key] = value;
        return this;
    }

    public IModalBuilder<T> WithData(T data)
    {
        _options.Data = data;
        return this;
    }

    public IModalBuilder<T> WithButton(ModalButton button)
    {
        _options.Buttons.Add(button);
        return this;
    }

    public IModalBuilder<T> WithButton(string text, VariantType variant, Func<T, Task<bool>> onClick = null)
    {
        _options.Buttons.Add(new ModalButton
        {
            Text = text,
            Variant = variant,
            OnClick = onClick != null ? data => onClick((T)data) : null
        });
        return this;
    }

    public IModalBuilder<T> WithCloseButton(bool show = true)
    {
        _options.ShowCloseButton = show;
        return this;
    }

    public IModalBuilder<T> WithBackdropClose(bool allow = true)
    {
        _options.CloseOnBackdrop = allow;
        return this;
    }

    public IModalBuilder<T> WithEscapeClose(bool allow = true)
    {
        _options.CloseOnEscape = allow;
        return this;
    }

    public IModalBuilder<T> WithClass(string Class)
    {
        _options.Class = Class;
        return this;
    }

    public IModalBuilder<T> WithAutoClose(TimeSpan delay)
    {
        _options.AutoCloseDelay = delay;
        return this;
    }

    public async Task<ModalResult<T>> ShowAsync() => await modalService.ShowAsync(_options);
}
