using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using RR.Blazor.Components.Feedback;
using RR.Blazor.Enums;
using RR.Blazor.Models;
using System.Collections.Concurrent;

namespace RR.Blazor.Services;

public sealed class ModalService : IModalService, IDisposable
{
    private readonly IJSRuntime _jsRuntime;
    private readonly ConcurrentDictionary<string, ModalInstance> _activeModals = new();
    private readonly ConcurrentDictionary<string, TaskCompletionSource<ModalResult<object>>> _modalCompletions = new();
    private readonly Stack<string> _modalStack = new();
    private readonly object _stackLock = new();
    private int _currentZIndexBase = 1000;
    private readonly int _zIndexIncrement = 100;
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

        var modalId = $"modal-{Guid.NewGuid():N}";
        var taskSource = new TaskCompletionSource<ModalResult<object>>();
        _modalCompletions[modalId] = taskSource;

        string parentModalId = null;
        int stackLevel = 0;
        int zIndex = _currentZIndexBase;
        
        lock (_stackLock)
        {
            if (_modalStack.Count > 0)
            {
                parentModalId = _modalStack.Peek();
                var parentModal = _activeModals[parentModalId];
                parentModal.ChildModalIds.Add(modalId);
            }
            
            stackLevel = _modalStack.Count;
            zIndex = _currentZIndexBase + (stackLevel * _zIndexIncrement);
            _modalStack.Push(modalId);
        }

        var instance = new ModalInstance
        {
            Id = modalId,
            Options = options,
            Visible = true,
            CreatedAt = DateTime.UtcNow,
            ParentModalId = parentModalId,
            StackLevel = stackLevel,
            ZIndex = zIndex
        };

        _activeModals[modalId] = instance;

        try
        {
            var jsOptions = new
            {
                id = modalId,
                closeOnBackdropClick = options.CloseOnBackdrop,
                closeOnEscape = options.CloseOnEscape,
                useBackdrop = true,
                backdropClass = GetBackdropClass(options.Variant),
                animation = "scale",
                animationSpeed = "normal",
                trapFocus = true,
                stackLevel = stackLevel,
                zIndex = zIndex,
                parentModalId = parentModalId
            };

            try
            {
                await _jsRuntime.InvokeVoidAsync("RRBlazor.Modal.createAndShow", modalId, modalType?.Name ?? "CustomModal", options.Parameters, jsOptions);
            }
            catch (JSException jsEx)
            {
                Console.WriteLine($"JS Error creating modal {modalId}: {jsEx.Message}");
                // Try fallback method without parameters to avoid JS compatibility issues
                await _jsRuntime.InvokeVoidAsync("RRBlazor.Modal.create", modalId, jsOptions);
            }

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


    private string GetBackdropClass(ModalVariant variant) => variant switch
    {
        ModalVariant.Destructive => "modal-backdrop-destructive",
        ModalVariant.Warning => "modal-backdrop-warning",
        _ => "modal-backdrop-dark"
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
        if (!_activeModals.TryGetValue(modalId, out var modal))
            return;
        
        if (modal.ChildModalIds.Count > 0)
            foreach (var childId in modal.ChildModalIds.ToList())
            {
                await CloseAsync(childId, Enums.ModalResult.Cancel);
            }
        
        if (!string.IsNullOrEmpty(modal.ParentModalId) && _activeModals.TryGetValue(modal.ParentModalId, out var parent))
            parent.ChildModalIds.Remove(modalId);
        
        lock (_stackLock)
        {
            var tempStack = new Stack<string>();
            while (_modalStack.Count > 0)
            {
                var id = _modalStack.Pop();
                if (id != modalId)
                {
                    tempStack.Push(id);
                }
            }
            while (tempStack.Count > 0)
            {
                _modalStack.Push(tempStack.Pop());
            }
        }
        
        _activeModals.TryRemove(modalId, out _);

        modal.Visible = false;
        modal.LastResult = result;

        // Complete the task
        if (_modalCompletions.TryRemove(modalId, out var completion))
        {
            completion.TrySetResult(new ModalResult<object> 
            { 
                ResultType = result,
                Data = result == Enums.ModalResult.Ok
            });
        }

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


    private static Dictionary<string, object> ConvertParametersToDict<TParams>(TParams parameters)
    {
        if (parameters == null) return new Dictionary<string, object>();
        if (parameters is Dictionary<string, object> dict) return dict;

        return typeof(TParams).GetProperties()
            .Select(prop => (prop.Name, Value: prop.GetValue(parameters)))
            .Where(item => item.Value != null)
            .ToDictionary(item => item.Name, item => item.Value);
    }



    async Task<ModalResult<T>> IModalService.ShowFormAsync<T>(string title, T initialData, SizeType size)
    {
        var parameters = new Dictionary<string, object> { { "Data", initialData } };
        return await ShowAsync<T>(null, parameters, new ModalOptions { Title = title, Size = size });
    }

    async Task<ModalResult<T>> IModalService.ShowFormAsync<T>(FormModalOptions<T> options)
    {
        var parameters = new Dictionary<string, object> { { "Data", options.InitialData } };
        return await ShowAsync<T>(null, parameters, new ModalOptions { Title = options.Title, Size = options.Size });
    }


    async Task IModalService.ShowDetailAsync<T>(T data, string title, SizeType size)
    {
        var parameters = new Dictionary<string, object> { { "Data", data } };
        await ShowAsync(null, parameters, new ModalOptions { Title = title, Size = size });
    }

    async Task IModalService.ShowPreviewAsync(string content, string title, string contentType)
    {
        var parameters = new Dictionary<string, object> { { "Content", content }, { "ContentType", contentType } };
        await ShowAsync(null, parameters, new ModalOptions { Title = title, Size = SizeType.Large });
    }

    async Task<T> IModalService.ShowSelectAsync<T>(IEnumerable<T> items, string title, Func<T, string> displaySelector)
    {
        var parameters = new Dictionary<string, object> { { "Items", items }, { "DisplaySelector", displaySelector } };
        var result = await ShowAsync<T>(null, parameters, new ModalOptions { Title = title });
        return result.Data;
    }

    async Task<IEnumerable<T>> IModalService.ShowMultiSelectAsync<T>(IEnumerable<T> items, string title, Func<T, string> displaySelector)
    {
        var parameters = new Dictionary<string, object> { { "Items", items }, { "DisplaySelector", displaySelector }, { "MultiSelect", true } };
        var result = await ShowAsync<IEnumerable<T>>(null, parameters, new ModalOptions { Title = title });
        return result.Data;
    }

    public void Dispose()
    {
        if (_isDisposed) return;
        
        _activeModals.Clear();
        _modalCompletions.Clear();
        
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