using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using RR.Blazor.Components.Feedback;
using RR.Blazor.Enums;
using RR.Blazor.Models;
using System.Collections.Concurrent;

namespace RR.Blazor.Services;

public sealed class ModalService : IModalService, IModalServiceCore, IDisposable
{
    private readonly IJSRuntime _jsRuntime;
    private readonly ConcurrentDictionary<string, ModalInstance> _activeModals = new();
    private readonly ConcurrentDictionary<string, TaskCompletionSource<ModalResult<object>>> _modalCompletions = new();
    private readonly ConcurrentDictionary<string, DotNetObjectReference<ModalService>> _modalProxies = new();
    private readonly Dictionary<string, TaskCompletionSource<bool>> _confirmationSources = [];
    private bool _isDisposed;

    public event Action<ModalInstance> OnModalOpened;
    public event Action<ModalInstance> OnModalClosed;
    public event Action OnAllModalsClosed;

    public bool HasVisibleModals => _activeModals.Any(m => m.Value.Visible);
    public IEnumerable<ModalInstance> ActiveModals => _activeModals.Values.Where(m => m.Visible);
    public IJSRuntime JSRuntime => _jsRuntime;

    public ModalService(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime ?? throw new ArgumentNullException(nameof(jsRuntime));
    }

    public async Task<ModalResult<TResult>> ShowAsync<TModal, TParams, TResult>(
        TParams parameters = default,
        ModalOptions options = null,
        ModalEvents<TResult> events = null) where TModal : ComponentBase
    {
        return await ShowAsync<TResult>(
            typeof(TModal),
            ConvertParametersToDict(parameters),
            options,
            events);
    }

    public async Task<ModalResult<TResult>> ShowAsync<TResult>(
        Type modalType,
        Dictionary<string, object> parameters = null,
        ModalOptions options = null,
        ModalEvents<TResult> events = null)
    {
        options ??= new ModalOptions();
        options.ComponentType = modalType;
        options.Parameters = parameters ?? new Dictionary<string, object>();

        var modalId = Guid.NewGuid().ToString();
        var taskSource = new TaskCompletionSource<ModalResult<object>>();
        _modalCompletions[modalId] = taskSource;

        var instance = new ModalInstance
        {
            Id = modalId,
            Options = options,
            Visible = true,
            CreatedAt = DateTime.UtcNow
        };

        _activeModals[modalId] = instance;
        _modalInstances[modalId] = this; // Register this instance for static callback

        try
        {
            // Create DOM element for modal
            var modalElement = await CreateModalElement(modalId, modalType, options);
            
            // Create modal through JavaScript portal system
            var jsOptions = new
            {
                id = modalId,
                closeOnBackdropClick = options.CloseOnBackdrop,
                closeOnEscape = options.CloseOnEscape,
                useBackdrop = true,
                backdropClass = GetBackdropClass(options.Variant),
                animation = "scale",
                animationSpeed = "normal",
                trapFocus = true
            };

            // Pass the element directly, not as a parameter to InvokeVoidAsync
            await _jsRuntime.InvokeVoidAsync("eval", $@"
                window.RRBlazor.Modal.create(window.__modalElements['{modalId}'], {System.Text.Json.JsonSerializer.Serialize(jsOptions)});
            ");

            OnModalOpened?.Invoke(instance);

            // Handle auto-close
            if (options.AutoCloseDelay.HasValue)
            {
                _ = Task.Delay(options.AutoCloseDelay.Value).ContinueWith(async _ =>
                {
                    if (_activeModals.ContainsKey(modalId))
                    {
                        await CloseAsync(modalId, Enums.ModalResult.Cancel);
                    }
                }, TaskScheduler.Default);
            }

            // Setup event handlers
            if (events != null)
            {
                RegisterEventHandlers(modalId, events, taskSource);
            }

            // Wait for modal to complete
            var result = await taskSource.Task;
            
            return new ModalResult<TResult>
            {
                ResultType = result.ResultType,
                Data = result.Data is TResult tr ? tr : default
            };
        }
        catch (Exception ex)
        {
            _activeModals.TryRemove(modalId, out _);
            _modalCompletions.TryRemove(modalId, out _);
            throw new InvalidOperationException($"Failed to create modal: {ex.Message}", ex);
        }
    }

    private async Task<object> CreateModalElement(string modalId, Type modalType, ModalOptions options)
    {
        // Create a wrapper element that will be passed to modal.js
        var wrapperElementId = $"modal-wrapper-{modalId}";
        
        // For built-in modal types, create the HTML directly
        if (modalType == typeof(RConfirmationModal))
        {
            return await CreateConfirmationModalElement(modalId, options);
        }
        
        // For custom components, create a placeholder
        // The actual Blazor component rendering would need to be handled separately
        await _jsRuntime.InvokeVoidAsync("eval", $@"
            (function() {{
                const wrapper = document.createElement('div');
                wrapper.id = '{wrapperElementId}';
                wrapper.className = 'modal-wrapper';
                wrapper.setAttribute('data-modal-id', '{modalId}');
                window.__modalElements = window.__modalElements || {{}};
                window.__modalElements['{modalId}'] = wrapper;
            }})();
        ");
        
        return await _jsRuntime.InvokeAsync<object>("eval", $"window.__modalElements['{modalId}']");
    }

    private async Task<object> CreateConfirmationModalElement(string modalId, ModalOptions options)
    {
        var title = options.Parameters.GetValueOrDefault("Title", "Confirm")?.ToString() ?? "Confirm";
        var message = options.Parameters.GetValueOrDefault("Message", "")?.ToString() ?? "";
        var confirmText = options.Parameters.GetValueOrDefault("ConfirmText", "Confirm")?.ToString() ?? "Confirm";
        var cancelText = options.Parameters.GetValueOrDefault("CancelText", "Cancel")?.ToString() ?? "Cancel";
        var variant = options.Parameters.GetValueOrDefault("Variant", ConfirmationVariant.Info);

        var variantClass = GetModalVariantClass(variant);

        await _jsRuntime.InvokeVoidAsync("eval", $@"
            (function() {{
                const modal = document.createElement('div');
                modal.className = 'rmodal rmodal-confirmation {variantClass}';
                modal.setAttribute('data-modal-id', '{modalId}');
                
                modal.innerHTML = `
                    <div class=""modal-content"">
                        <div class=""modal-header"">
                            <h2 class=""modal-title"">{System.Web.HttpUtility.JavaScriptStringEncode(title)}</h2>
                        </div>
                        <div class=""modal-body"">
                            <p class=""modal-message"">{System.Web.HttpUtility.JavaScriptStringEncode(message)}</p>
                        </div>
                        <div class=""modal-footer"">
                            <button class=""btn btn-secondary modal-cancel"" data-action=""cancel"">{System.Web.HttpUtility.JavaScriptStringEncode(cancelText)}</button>
                            <button class=""btn btn-primary modal-confirm"" data-action=""confirm"">{System.Web.HttpUtility.JavaScriptStringEncode(confirmText)}</button>
                        </div>
                    </div>
                `;
                
                // Add event listeners
                modal.querySelector('[data-action=""cancel""]').addEventListener('click', () => {{
                    DotNet.invokeMethodAsync('RR.Blazor', 'HandleModalAction', '{modalId}', 'cancel');
                }});
                
                modal.querySelector('[data-action=""confirm""]').addEventListener('click', () => {{
                    DotNet.invokeMethodAsync('RR.Blazor', 'HandleModalAction', '{modalId}', 'confirm');
                }});
                
                window.__modalElements = window.__modalElements || {{}};
                window.__modalElements['{modalId}'] = modal;
            }})();
        ");
        
        return await _jsRuntime.InvokeAsync<object>("eval", $"window.__modalElements['{modalId}']");
    }

    [JSInvokable]
    public static async Task HandleModalAction(string modalId, string action)
    {
        // Find the modal instance and close it with the appropriate result
        if (_modalInstances.TryGetValue(modalId, out var modalService))
        {
            var result = action switch
            {
                "confirm" => Enums.ModalResult.Ok,
                "cancel" => Enums.ModalResult.Cancel,
                _ => Enums.ModalResult.None
            };
            
            await modalService.CloseAsync(modalId, result);
        }
    }
    
    private static readonly ConcurrentDictionary<string, ModalService> _modalInstances = new();

    private string GetBackdropClass(ModalVariant variant) => variant switch
    {
        ModalVariant.Destructive => "modal-backdrop-destructive",
        ModalVariant.Warning => "modal-backdrop-warning",
        _ => "modal-backdrop-dark"
    };

    private string GetModalVariantClass(object variant) => variant switch
    {
        ConfirmationVariant.Destructive => "modal-destructive",
        ConfirmationVariant.Danger => "modal-destructive",
        ConfirmationVariant.Warning => "modal-warning",
        _ => "modal-default"
    };

    private void RegisterEventHandlers<TResult>(string modalId, ModalEvents<TResult> events, TaskCompletionSource<ModalResult<object>> taskSource)
    {
        if (events == null) return;

        var modal = _activeModals[modalId];
        if (modal == null) return;

        modal.Options.Buttons ??= [];

        if (events.OnValidate != null)
        {
            var primaryButton = modal.Options.Buttons.FirstOrDefault(b => b.Type == ModalButtonType.Primary);
            if (primaryButton != null)
            {
                var originalClick = primaryButton.OnClick;
                primaryButton.OnClick = async data =>
                {
                    if (await events.OnValidate((TResult)data))
                        return originalClick != null ? await originalClick(data) : true;
                    return false;
                };
            }
        }
    }

    public async Task<Models.ModalResult> ShowAsync(
        Type modalType,
        Dictionary<string, object> parameters = null,
        ModalOptions options = null,
        ModalEvents events = null)
    {
        var result = await ShowAsync<object>(modalType, parameters, options, null);
        return new Models.ModalResult
        {
            ResultType = result.ResultType,
            Data = result.Data
        };
    }

    public async Task<ModalResult<T>> ShowAsync<T>(ModalOptions<T> options)
    {
        return await ShowAsync<T>(
            options.ComponentType,
            options.Parameters,
            new ModalOptions
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
                AutoCloseDelay = options.AutoCloseDelay
            },
            null);
    }

    public async Task<Models.ModalResult> ShowAsync(ModalOptions options)
    {
        return await ShowAsync(
            options.ComponentType,
            options.Parameters,
            options,
            null);
    }

    public async Task<ModalResult<T>> ShowAsync<T>(
        Type componentType,
        Dictionary<string, object> parameters = null,
        ModalOptions<T> options = null)
    {
        options ??= new ModalOptions<T>();
        options.ComponentType = componentType;
        options.Parameters = parameters ?? [];

        return await ShowAsync(options);
    }

    public async Task CloseAsync(string modalId, Enums.ModalResult result = Enums.ModalResult.None)
    {
        if (!_activeModals.TryRemove(modalId, out var modal))
            return;

        modal.Visible = false;
        _modalInstances.TryRemove(modalId, out _); // Clean up static reference

        // Complete the task
        if (_modalCompletions.TryRemove(modalId, out var completion))
        {
            completion.TrySetResult(new ModalResult<object> 
            { 
                ResultType = result,
                Data = result == Enums.ModalResult.Ok
            });
        }

        // Destroy the modal in JavaScript
        try
        {
            await _jsRuntime.InvokeVoidAsync("window.RRBlazor.Modal.destroy", modalId);
        }
        catch { }

        OnModalClosed?.Invoke(modal);

        if (_activeModals.IsEmpty)
        {
            OnAllModalsClosed?.Invoke();
        }
    }

    public async Task CloseAllAsync()
    {
        var modalsToClose = _activeModals.Keys.ToList();
        foreach (var modalId in modalsToClose)
        {
            await CloseAsync(modalId, Enums.ModalResult.Cancel);
        }
    }

    public bool IsModalOpen(string modalId = null)
    {
        if (string.IsNullOrEmpty(modalId))
            return _activeModals.Any(m => m.Value.Visible);
        
        return _activeModals.TryGetValue(modalId, out var modal) && modal.Visible;
    }

    public IModalBuilder<T> Create<T>()
    {
        return new ModalBuilder<T>(this);
    }

    // Confirmation modal specific methods
    public async Task<bool> ConfirmAsync(string message, string title = "Confirm", bool isDestructive = false)
    {
        return await ModalServiceExtensions.ShowConfirmationAsync(
            this,
            message,
            title,
            isDestructive ? "Delete" : "Confirm",
            "Cancel",
            isDestructive ? ModalVariant.Destructive : ModalVariant.Warning);
    }

    public async Task<bool> ConfirmAsync(ConfirmationOptions options)
    {
        return await ModalServiceExtensions.ShowConfirmationAsync(this, options);
    }

    public async Task<Models.ModalResult> ConfirmWithResultAsync(
        string message,
        string title = "Confirm",
        ModalVariant variant = ModalVariant.Default)
    {
        var result = await ModalServiceExtensions.ShowConfirmationAsync(this, message, title, "Confirm", "Cancel", variant);
        return result ? Models.ModalResult.Ok() : Models.ModalResult.Cancel();
    }

    public async Task<ModalResult<T>> ShowFormAsync<T>(
        string title,
        T initialData = default,
        SizeType size = SizeType.Medium)
    {
        return await ModalServiceExtensions.ShowFormAsync(this, title, initialData, size);
    }

    public async Task<ModalResult<T>> ShowFormAsync<T>(FormModalOptions<T> options)
    {
        return await ModalServiceExtensions.ShowFormAsync(this, options);
    }

    public async Task ShowInfoAsync(string message, string title = "Information")
    {
        await ModalServiceExtensions.ShowInfoAsync(this, message, title);
    }

    public async Task ShowWarningAsync(string message, string title = "Warning")
    {
        await ModalServiceExtensions.ShowWarningAsync(this, message, title);
    }

    public async Task ShowErrorAsync(string message, string title = "Error")
    {
        await ModalServiceExtensions.ShowErrorAsync(this, message, title);
    }

    public async Task ShowSuccessAsync(string message, string title = "Success")
    {
        await ModalServiceExtensions.ShowSuccessAsync(this, message, title);
    }

    public async Task ShowDetailAsync<T>(T data, string title = "", SizeType size = SizeType.Large)
    {
        await ModalServiceExtensions.ShowDetailAsync(this, data, title, size);
    }

    public async Task ShowPreviewAsync(string content, string title = "Preview", string contentType = "text/plain")
    {
        await ModalServiceExtensions.ShowPreviewAsync(this, content, title, contentType);
    }

    public async Task<T> ShowSelectAsync<T>(
        IEnumerable<T> items,
        string title = "Select Item",
        Func<T, string> displaySelector = null)
    {
        return await ModalServiceExtensions.ShowSelectAsync(this, items, title, displaySelector);
    }

    public async Task<IEnumerable<T>> ShowMultiSelectAsync<T>(
        IEnumerable<T> items,
        string title = "Select Items",
        Func<T, string> displaySelector = null)
    {
        return await ModalServiceExtensions.ShowMultiSelectAsync(this, items, title, displaySelector);
    }

    public void ConfirmModal(string modalId)
    {
        if (_confirmationSources.TryGetValue(modalId, out var source))
        {
            source.TrySetResult(true);
            _confirmationSources.Remove(modalId);
            _ = CloseAsync(modalId, Enums.ModalResult.Ok);
        }
    }

    public void CancelModal(string modalId)
    {
        if (_confirmationSources.TryGetValue(modalId, out var source))
        {
            source.TrySetResult(false);
            _confirmationSources.Remove(modalId);
            _ = CloseAsync(modalId, Enums.ModalResult.Cancel);
        }
    }

    private static Dictionary<string, object> ConvertParametersToDict<TParams>(TParams parameters)
    {
        if (parameters == null) return new Dictionary<string, object>();
        if (parameters is Dictionary<string, object> dict) return dict;

        return typeof(TParams).GetProperties()
            .Select(prop => (prop.Name, Value: prop.GetValue(parameters)))
            .Where(item => item.Value != null)
            .ToDictionary(item => item.Name, item => item.Value);
    }

    public void Dispose()
    {
        if (_isDisposed) return;

        foreach (var proxy in _modalProxies.Values)
        {
            proxy?.Dispose();
        }
        
        _activeModals.Clear();
        _modalCompletions.Clear();
        _modalProxies.Clear();
        _confirmationSources.Clear();
        
        _isDisposed = true;
    }

    // Modal builder implementation
    private sealed class ModalBuilder<T> : IModalBuilder<T>
    {
        private readonly ModalService _service;
        private readonly ModalOptions<T> _options = new();
        private readonly ModalEvents<T> _events = new();

        public ModalBuilder(ModalService service)
        {
            _service = service;
        }

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

        public IModalBuilder<T> WithClass(string cssClass)
        {
            _options.Class = cssClass;
            return this;
        }

        public IModalBuilder<T> WithAutoClose(TimeSpan delay)
        {
            _options.AutoCloseDelay = delay;
            return this;
        }

        public IModalBuilder<T> WithEvent(string eventName, Func<T, Task> handler)
        {
            switch (eventName.ToLower())
            {
                case "onclose":
                    _events.OnClose = handler;
                    break;
                case "oncancel":
                    _events.OnCancel = async () => await handler(default);
                    break;
                case "onshow":
                    _events.OnShow = async () => await handler(default);
                    break;
            }
            return this;
        }

        public async Task<ModalResult<T>> ShowAsync()
        {
            return await _service.ShowAsync<T>(
                _options.ComponentType,
                _options.Parameters,
                new ModalOptions
                {
                    Title = _options.Title,
                    Subtitle = _options.Subtitle,
                    Icon = _options.Icon,
                    Size = _options.Size,
                    Variant = _options.Variant,
                    CloseOnBackdrop = _options.CloseOnBackdrop,
                    CloseOnEscape = _options.CloseOnEscape,
                    ShowCloseButton = _options.ShowCloseButton,
                    ShowHeader = _options.ShowHeader,
                    ShowFooter = _options.ShowFooter,
                    Class = _options.Class,
                    ComponentType = _options.ComponentType,
                    Parameters = _options.Parameters,
                    Buttons = _options.Buttons,
                    AutoCloseDelay = _options.AutoCloseDelay
                },
                _events);
        }
    }
}