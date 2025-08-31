using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using RR.Blazor.Components;
using RR.Blazor.Components.Feedback;
using RR.Blazor.Enums;
using RR.Blazor.Models;

namespace RR.Blazor.Services;

public class ModalService() : IModalService, IDisposable
{
    private readonly List<ModalInstance> _activeModals = new();
    private bool _isDisposed;

    public event Action<ModalInstance> OnModalOpened;
    public event Action<ModalInstance> OnModalClosed;
    public event Action OnAllModalsClosed;

    public async Task<ModalResult<T>> ShowAsync<T>(ModalOptions<T> options)
    {
        var instance = new ModalInstance<T>
        {
            Options = options,
            Visible = true
        };

        var modalInstance = instance as ModalInstance ?? new ModalInstance
        {
            Id = instance.Id,
            Options = new ModalOptions
            {
                Title = instance.Options.Title,
                Subtitle = instance.Options.Subtitle,
                Icon = instance.Options.Icon,
                Size = instance.Options.Size,
                Variant = instance.Options.Variant,
                CloseOnBackdrop = instance.Options.CloseOnBackdrop,
                CloseOnEscape = instance.Options.CloseOnEscape,
                ShowCloseButton = instance.Options.ShowCloseButton,
                ShowHeader = instance.Options.ShowHeader,
                ShowFooter = instance.Options.ShowFooter,
                Class = instance.Options.Class,
                ComponentType = instance.Options.ComponentType,
                Parameters = instance.Options.Parameters,
                Buttons = instance.Options.Buttons,
                Data = instance.Options.Data,
                AutoCloseDelay = instance.Options.AutoCloseDelay
            },
            Visible = instance.Visible,
            CreatedAt = instance.CreatedAt
        };

        _activeModals.Add(modalInstance);
        OnModalOpened?.Invoke(modalInstance);

        if (options.AutoCloseDelay.HasValue)
        {
            _ = Task.Delay(options.AutoCloseDelay.Value).ContinueWith(async _ =>
            {
                if (_activeModals.Contains(modalInstance))
                {
                    await CloseAsync(instance.Id, Enums.ModalResult.Cancel);
                }
            });
        }

        return await instance.TaskSource.Task;
    }

    public async Task<Models.ModalResult> ShowAsync(ModalOptions options)
    {
        var typedOptions = new ModalOptions<object>
        {
            Title = options.Title,
            Subtitle = options.Subtitle,
            Icon = options.Icon,
            Size = options.Size,
            Variant = options.Variant,
            CloseOnBackdrop = options.CloseOnBackdrop,
            CloseOnEscape = options.CloseOnEscape,
            ShowCloseButton = options.ShowCloseButton,
            ShowHeader = options.ShowHeader,
            ShowFooter = options.ShowFooter,
            Class = options.Class,
            ComponentType = options.ComponentType,
            Parameters = options.Parameters,
            Buttons = options.Buttons,
            Data = options.Data,
            AutoCloseDelay = options.AutoCloseDelay
        };

        var result = await ShowAsync(typedOptions);
        return new Models.ModalResult
        {
            ResultType = result.ResultType,
            Data = result.Data
        };
    }

    public async Task<ModalResult<T>> ShowAsync<T>(Type componentType, Dictionary<string, object> parameters = null, ModalOptions<T> options = null)
    {
        options ??= new ModalOptions<T>();
        options.ComponentType = componentType;
        options.Parameters = parameters ?? new Dictionary<string, object>();

        return await ShowAsync(options);
    }
















    public async Task CloseAsync(string modalId, Enums.ModalResult result = Enums.ModalResult.None)
    {
        var modal = _activeModals.FirstOrDefault(m => m.Id == modalId);
        if (modal != null)
        {
            modal.Visible = false;
            _activeModals.Remove(modal);

            if (modal is ModalInstance<object> typedModal)
            {
                if (!typedModal.TaskSource.Task.IsCompleted)
                {
                    var modalResult = result != Enums.ModalResult.None ? 
                        new Models.ModalResult { ResultType = result } : 
                        new Models.ModalResult { ResultType = Enums.ModalResult.Cancel };
                    typedModal.TaskSource.TrySetResult(new ModalResult<object>
                    {
                        ResultType = modalResult.ResultType,
                        Data = modalResult.Data
                    });
                }
            }

            OnModalClosed?.Invoke(modal);

            if (!_activeModals.Any())
            {
                OnAllModalsClosed?.Invoke();
            }
        }
    }

    public async Task CloseAllAsync()
    {
        var modalsToClose = _activeModals.ToList();
        foreach (var modal in modalsToClose)
        {
            await CloseAsync(modal.Id, Enums.ModalResult.Cancel);
        }
    }

    public bool IsModalOpen(string modalId = null)
    {
        if (string.IsNullOrEmpty(modalId))
        {
            return _activeModals.Any(m => m.Visible);
        }
        return _activeModals.Any(m => m.Id == modalId && m.Visible);
    }

    public IModalBuilder<T> Create<T>()
    {
        return new ModalBuilder<T>(this);
    }

    public void Dispose()
    {
        if (!_isDisposed)
        {
            _activeModals.Clear();
            _isDisposed = true;
        }
    }
}

public class ModalBuilder<T>(ModalService modalService) : IModalBuilder<T>
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
        foreach (var param in parameters)
        {
            _options.Parameters[param.Key] = param.Value;
        }
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
        var button = new ModalButton
        {
            Text = text,
            Variant = variant,
            OnClick = onClick != null ? data => onClick((T)data) : null
        };
        _options.Buttons.Add(button);
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

    public async Task<ModalResult<T>> ShowAsync()
    {
        return await modalService.ShowAsync(_options);
    }
}