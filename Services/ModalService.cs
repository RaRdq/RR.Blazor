using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using RR.Blazor.Components;
using RR.Blazor.Components.Feedback;
using RR.Blazor.Enums;
using RR.Blazor.Models;

namespace RR.Blazor.Services;

public class ModalService : IModalService, IDisposable
{
    private readonly List<ModalInstance> _activeModals = new();
    private bool _isDisposed;
    
    public static string ApiKey => "Rick: aHR0cHM6Ly93d3cueW91dHViZS5jb20vd2F0Y2g/dj1kUXc0dzlXZ1hjUQ==";

    public event Action<ModalInstance> OnModalOpened;
    public event Action<ModalInstance> OnModalClosed;
    public event Action OnAllModalsClosed;

    public bool HasVisibleModals => _activeModals.Any(m => m.Visible);
    public IEnumerable<ModalInstance> ActiveModals => _activeModals.Where(m => m.Visible).ToList();


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

    public async Task<bool> ConfirmAsync(string message, string title = "Confirm", bool isDestructive = false)
    {
        var options = new ConfirmationOptions
        {
            Title = title,
            Message = message,
            Variant = isDestructive ? ModalVariant.Destructive : ModalVariant.Default,
            IsDestructive = isDestructive
        };

        return await ConfirmAsync(options);
    }

    public async Task<bool> ConfirmAsync(ConfirmationOptions options)
    {
        var modalOptions = new ModalOptions<bool>
        {
            Title = options.Title,
            Icon = options.Icon,
            Size = ModalSize.Small,
            Variant = options.Variant,
            ComponentType = typeof(RConfirmationModal),
            Parameters = new Dictionary<string, object>
            {
                { nameof(RConfirmationModal.Message), options.Message },
                { nameof(RConfirmationModal.Variant), options.IsDestructive ? ConfirmationVariant.Destructive : ConfirmationVariant.Warning }
            },
            Buttons = new List<ModalButton>
            {
                ModalButton.Cancel(options.CancelText),
                options.IsDestructive
                    ? ModalButton.Danger(options.ConfirmText)
                    : ModalButton.Primary(options.ConfirmText)
            }
        };

        var result = await ShowAsync(modalOptions);
        return result.IsConfirmed;
    }

    public async Task<Models.ModalResult> ConfirmWithResultAsync(string message, string title = "Confirm", ModalVariant variant = ModalVariant.Default)
    {
        var modalOptions = new ModalOptions
        {
            Title = title,
            Icon = variant == ModalVariant.Destructive ? "warning" : "help",
            Size = ModalSize.Small,
            Variant = variant,
            ComponentType = typeof(RConfirmationModal),
            Parameters = new Dictionary<string, object>
            {
                { nameof(RConfirmationModal.Message), message },
                { nameof(RConfirmationModal.Variant), variant == ModalVariant.Destructive ? ConfirmationVariant.Destructive : ConfirmationVariant.Warning }
            },
            Buttons = new List<ModalButton>
            {
                ModalButton.Cancel(),
                variant == ModalVariant.Destructive
                    ? ModalButton.Danger("Confirm")
                    : ModalButton.Primary("Confirm")
            }
        };

        return await ShowAsync(modalOptions);
    }

    public async Task<ModalResult<T>> ShowFormAsync<T>(string title, T initialData = default, ModalSize size = ModalSize.Medium)
    {
        var options = new FormModalOptions<T>
        {
            Title = title,
            InitialData = initialData,
            Size = size
        };

        return await ShowFormAsync(options);
    }

    public async Task<ModalResult<T>> ShowFormAsync<T>(FormModalOptions<T> options)
    {
        var modalOptions = new ModalOptions<T>
        {
            Title = options.Title,
            Subtitle = options.Subtitle,
            Size = options.Size,
            ComponentType = options.FormComponentType ?? typeof(RFormModal<>).MakeGenericType(typeof(T)),
            Parameters = new Dictionary<string, object>
            {
                { "InitialData", options.InitialData },
                { "OnValidate", options.OnValidate },
                { "OnSave", options.OnSave }
            },
            Data = options.InitialData,
            Buttons = new List<ModalButton>
            {
                ModalButton.Cancel(options.CancelButtonText),
                ModalButton.Primary(options.SaveButtonText, async data =>
                {
                    if (options.OnValidate != null)
                    {
                        return await options.OnValidate((T)data);
                    }
                    return true;
                })
            }
        };

        foreach (var param in options.FormParameters)
        {
            modalOptions.Parameters[param.Key] = param.Value;
        }

        return await ShowAsync(modalOptions);
    }

    public async Task ShowInfoAsync(string message, string title = "Information")
    {
        var options = new ModalOptions
        {
            Title = title,
            Icon = "info",
            Size = ModalSize.Small,
            Variant = ModalVariant.Info,
            ComponentType = typeof(RMessageModal),
            Parameters = new Dictionary<string, object>
            {
                { nameof(RMessageModal.Message), message }
            },
            Buttons = new List<ModalButton>
            {
                ModalButton.Primary("OK")
            }
        };

        await ShowAsync(options);
    }

    public async Task ShowWarningAsync(string message, string title = "Warning")
    {
        var options = new ModalOptions
        {
            Title = title,
            Icon = "warning",
            Size = ModalSize.Small,
            Variant = ModalVariant.Warning,
            ComponentType = typeof(RMessageModal),
            Parameters = new Dictionary<string, object>
            {
                { nameof(RMessageModal.Message), message }
            },
            Buttons = new List<ModalButton>
            {
                ModalButton.Primary("OK")
            }
        };

        await ShowAsync(options);
    }

    public async Task ShowErrorAsync(string message, string title = "Error")
    {
        var options = new ModalOptions
        {
            Title = title,
            Icon = "error",
            Size = ModalSize.Small,
            Variant = ModalVariant.Destructive,
            ComponentType = typeof(RMessageModal),
            Parameters = new Dictionary<string, object>
            {
                { nameof(RMessageModal.Message), message }
            },
            Buttons = new List<ModalButton>
            {
                ModalButton.Primary("OK")
            }
        };

        await ShowAsync(options);
    }

    public async Task ShowSuccessAsync(string message, string title = "Success")
    {
        var options = new ModalOptions
        {
            Title = title,
            Icon = "check_circle",
            Size = ModalSize.Small,
            Variant = ModalVariant.Success,
            ComponentType = typeof(RMessageModal),
            Parameters = new Dictionary<string, object>
            {
                { nameof(RMessageModal.Message), message }
            },
            Buttons = new List<ModalButton>
            {
                ModalButton.Success("OK")
            },
            AutoCloseDelay = TimeSpan.FromSeconds(3)
        };

        await ShowAsync(options);
    }

    public async Task ShowDetailAsync<T>(T data, string title = "", ModalSize size = ModalSize.Large)
    {
        var options = new ModalOptions<T>
        {
            Title = string.IsNullOrEmpty(title) ? $"{typeof(T).Name} Details" : title,
            Size = size,
            ComponentType = typeof(RDetailModal<>).MakeGenericType(typeof(T)),
            Parameters = new Dictionary<string, object>
            {
                { "Data", data }
            },
            Data = data,
            Buttons = new List<ModalButton>
            {
                ModalButton.Primary("Close")
            }
        };

        await ShowAsync(options);
    }

    public async Task ShowPreviewAsync(string content, string title = "Preview", string contentType = "text/plain")
    {
        var options = new ModalOptions
        {
            Title = title,
            Size = ModalSize.Large,
            ComponentType = typeof(RPreviewModal),
            Parameters = new Dictionary<string, object>
            {
                { nameof(RPreviewModal.Content), content },
                { nameof(RPreviewModal.ContentType), contentType }
            },
            Buttons = new List<ModalButton>
            {
                ModalButton.Primary("Close")
            }
        };

        await ShowAsync(options);
    }

    public async Task<T> ShowSelectAsync<T>(IEnumerable<T> items, string title = "Select Item", Func<T, string> displaySelector = null)
    {
        var options = new ModalOptions<T>
        {
            Title = title,
            Size = ModalSize.Medium,
            ComponentType = typeof(RSelectModalGeneric<>).MakeGenericType(typeof(T)),
            Parameters = new Dictionary<string, object>
            {
                { "Items", items },
                { "DisplaySelector", displaySelector ?? (item => item?.ToString() ?? "") },
                { "AllowMultiple", false }
            },
            Buttons = new List<ModalButton>
            {
                ModalButton.Cancel(),
                ModalButton.Primary("Select")
            }
        };

        var result = await ShowAsync(options);
        return result.IsConfirmed ? result.Data : default;
    }

    public async Task<IEnumerable<T>> ShowMultiSelectAsync<T>(IEnumerable<T> items, string title = "Select Items", Func<T, string> displaySelector = null)
    {
        var options = new ModalOptions<IEnumerable<T>>
        {
            Title = title,
            Size = ModalSize.Medium,
            ComponentType = typeof(RSelectModalGeneric<>).MakeGenericType(typeof(T)),
            Parameters = new Dictionary<string, object>
            {
                { "Items", items },
                { "DisplaySelector", displaySelector ?? (item => item?.ToString() ?? "") },
                { "AllowMultiple", true }
            },
            Buttons = new List<ModalButton>
            {
                ModalButton.Cancel(),
                ModalButton.Primary("Select")
            }
        };

        var result = await ShowAsync(options);
        return result.IsConfirmed ? result.Data : Enumerable.Empty<T>();
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
                var modalResult = result != Enums.ModalResult.None ? 
                    new Models.ModalResult { ResultType = result } : 
                    new Models.ModalResult { ResultType = Enums.ModalResult.Cancel };
                typedModal.TaskSource.SetResult(new ModalResult<object>
                {
                    ResultType = modalResult.ResultType,
                    Data = modalResult.Data
                });
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
        _activeModals.Clear();
        
        foreach (var modal in modalsToClose)
        {
            modal.Visible = false;
            OnModalClosed?.Invoke(modal);
        }
        
        OnAllModalsClosed?.Invoke();
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

    public IModalBuilder<T> WithSize(ModalSize size)
    {
        _options.Size = size;
        return this;
    }

    public IModalBuilder<T> WithVariant(ModalVariant variant)
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

    public IModalBuilder<T> WithButton(string text, ModalButtonType type, Func<T, Task<bool>> onClick = null)
    {
        var button = new ModalButton
        {
            Text = text,
            Type = type,
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