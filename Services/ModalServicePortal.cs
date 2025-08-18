using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using RR.Blazor.Enums;
using RR.Blazor.Models;
using System.Text.Json;

namespace RR.Blazor.Services;

/// <summary>
/// Modal service implementation that renders directly to DOM portals
/// Works with both Blazor Server and WebAssembly
/// </summary>
public sealed class ModalServicePortal : IModalServiceCore, IDisposable
{
    private readonly IJSRuntime _jsRuntime;
    private readonly IServiceProvider _serviceProvider;
    private readonly List<ModalInstance> _activeModals = [];
    private readonly Dictionary<string, TaskCompletionSource<object>> _modalCompletionSources = [];
    private readonly Dictionary<string, DotNetObjectReference<ModalCallbackHandler>> _dotNetReferences = [];
    private IJSObjectReference _modalModule;
    private bool _disposed;

    public event Action<ModalInstance> OnModalOpened;
    public event Action<ModalInstance> OnModalClosed;
    public event Action OnAllModalsClosed;

    public bool HasVisibleModals => _activeModals.Exists(m => m.Visible);
    public IEnumerable<ModalInstance> ActiveModals => _activeModals.Where(m => m.Visible);
    public IJSRuntime JSRuntime => _jsRuntime;

    public ModalServicePortal(IJSRuntime jsRuntime, IServiceProvider serviceProvider)
    {
        _jsRuntime = jsRuntime;
        _serviceProvider = serviceProvider;
    }

    private async Task EnsureModalModuleAsync()
    {
        if (_modalModule == null && _jsRuntime != null)
        {
            // Import modal module for advanced operations
            try
            {
                _modalModule = await _jsRuntime.InvokeAsync<IJSObjectReference>("import", "/js/modal.js");
            }
            catch
            {
                // Fallback to global access
            }
        }
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
        await EnsureModalModuleAsync();

        options ??= new ModalOptions();
        options.ComponentType = modalType;
        options.Parameters = parameters ?? [];

        var modalId = $"modal-{Guid.NewGuid():N}";
        var taskSource = new TaskCompletionSource<object>();
        _modalCompletionSources[modalId] = taskSource;

        var instance = new ModalInstance
        {
            Id = modalId,
            Options = options,
            Visible = true,
            CreatedAt = DateTime.UtcNow
        };

        _activeModals.Add(instance);

        try
        {
            // Create callback handler for this modal
            var callbackHandler = new ModalCallbackHandler(modalId, this, events);
            var dotNetRef = DotNetObjectReference.Create(callbackHandler);
            _dotNetReferences[modalId] = dotNetRef;

            // Render modal directly to portal
            await RenderModalToPortalAsync(instance, dotNetRef);

            OnModalOpened?.Invoke(instance);

            // Handle auto-close if specified
            if (options.AutoCloseDelay.HasValue)
            {
                _ = Task.Delay(options.AutoCloseDelay.Value).ContinueWith(async _ =>
                {
                    if (_activeModals.Contains(instance))
                    {
                        await CloseAsync(modalId, Enums.ModalResult.Cancel);
                    }
                }, TaskScheduler.Default);
            }

            // Wait for modal result
            var result = await taskSource.Task;
            
            if (result is TResult typedResult)
            {
                return new ModalResult<TResult>
                {
                    ResultType = instance.LastResult ?? Enums.ModalResult.None,
                    Data = typedResult
                };
            }

            return new ModalResult<TResult>
            {
                ResultType = instance.LastResult ?? Enums.ModalResult.Cancel,
                Data = default
            };
        }
        catch (Exception ex)
        {
            _activeModals.Remove(instance);
            _modalCompletionSources.Remove(modalId);
            CleanupDotNetReference(modalId);
            throw new InvalidOperationException($"Failed to show modal: {ex.Message}", ex);
        }
    }

    public async Task<Models.ModalResult> ShowAsync(
        Type modalType,
        Dictionary<string, object> parameters = null,
        ModalOptions options = null,
        ModalEvents events = null)
    {
        var result = await ShowAsync<object>(modalType, parameters, options, ConvertEvents(events));
        return new Models.ModalResult
        {
            ResultType = result.ResultType,
            Data = result.Data
        };
    }

    private async Task RenderModalToPortalAsync(ModalInstance instance, DotNetObjectReference<ModalCallbackHandler> dotNetRef)
    {
        var componentHtml = GenerateComponentHtml(instance, dotNetRef);
        
        // Create modal options for JS
        var jsOptions = new
        {
            id = instance.Id,
            useBackdrop = instance.Options?.CloseOnBackdrop ?? true,
            closeOnBackdrop = instance.Options?.CloseOnBackdrop ?? true,
            closeOnEscape = instance.Options?.CloseOnEscape ?? true,
            trapFocus = true,
            animation = "scale",
            animationSpeed = "normal",
            backdropClass = GetBackdropClass(instance.Options?.Variant ?? ModalVariant.Default),
            className = GetModalClass(instance.Options as ModalOptions ?? new ModalOptions())
        };

        // Create modal element and render it via portal
        await _jsRuntime.InvokeVoidAsync("eval", $@"
            (async function() {{
                // Create modal container
                const modalContainer = document.createElement('div');
                modalContainer.id = '{instance.Id}';
                modalContainer.className = 'modal-container';
                modalContainer.innerHTML = {System.Text.Json.JsonSerializer.Serialize(componentHtml)};
                
                // Store DotNet reference for callbacks
                modalContainer._dotNetRef = {dotNetRef.Value};
                
                // Setup button click handlers
                modalContainer.querySelectorAll('[data-modal-action]').forEach(btn => {{
                    btn.addEventListener('click', async (e) => {{
                        e.preventDefault();
                        const action = btn.dataset.modalAction;
                        const data = btn.dataset.modalData || null;
                        
                        try {{
                            await modalContainer._dotNetRef.invokeMethodAsync('HandleAction', action, data);
                        }} catch (err) {{
                            console.error('Modal action failed:', err);
                        }}
                    }});
                }});
                
                // Create modal in portal
                await window.RRBlazor.Modal.create(modalContainer, {System.Text.Json.JsonSerializer.Serialize(jsOptions)});
                
                // If this is a Blazor component, attempt to attach it
                if (window.Blazor && modalContainer.querySelector('[data-blazor-component]')) {{
                    const componentEl = modalContainer.querySelector('[data-blazor-component]');
                    const componentType = componentEl.dataset.componentType;
                    const componentParams = JSON.parse(componentEl.dataset.componentParams || '{{}}');
                    
                    // This would need server-side or WASM specific handling
                    // For now, we'll rely on pre-rendered HTML
                }}
            }})();
        ");
    }

    private string GenerateComponentHtml(ModalInstance instance, DotNetObjectReference<ModalCallbackHandler> dotNetRef)
    {
        var sb = new System.Text.StringBuilder();
        var options = instance.Options;
        
        sb.Append(@"<div class=""modal-wrapper"">");
        
        // Header
        if (options?.ShowHeader ?? true)
        {
            sb.Append(@"<div class=""modal-header"">");
            
            if (!string.IsNullOrEmpty(options?.Icon))
            {
                sb.Append($@"<span class=""modal-icon"">{options.Icon}</span>");
            }
            
            if (!string.IsNullOrEmpty(options?.Title))
            {
                sb.Append($@"<h2 class=""modal-title"">{options.Title}</h2>");
            }
            
            if (!string.IsNullOrEmpty(options?.Subtitle))
            {
                sb.Append($@"<p class=""modal-subtitle"">{options.Subtitle}</p>");
            }
            
            if (options?.ShowCloseButton ?? true)
            {
                sb.Append($@"
                    <button class=""modal-close-button"" 
                            data-modal-action=""close""
                            aria-label=""Close"">
                        <span aria-hidden=""true"">&times;</span>
                    </button>");
            }
            
            sb.Append("</div>");
        }
        
        // Body
        sb.Append(@"<div class=""modal-body"">");
        
        // Handle specific component types
        if (options?.ComponentType == typeof(RR.Blazor.Components.Feedback.RConfirmationModal))
        {
            // Render confirmation modal content directly
            var message = options.Parameters?.GetValueOrDefault("Message")?.ToString() ?? "";
            var variant = options.Parameters?.GetValueOrDefault("Variant")?.ToString() ?? "Default";
            
            sb.Append($@"
                <div class=""confirmation-modal-content"">
                    <div class=""confirmation-icon confirmation-icon-{variant.ToLower()}"">
                        {GetConfirmationIcon(variant)}
                    </div>
                    <div class=""confirmation-message"">{message}</div>
                </div>");
        }
        else
        {
            // For other components, create a mount point
            sb.Append($@"
                <div data-blazor-component=""true""
                     data-component-type=""{options?.ComponentType?.FullName}""
                     data-component-params=""{System.Text.Json.JsonSerializer.Serialize(options?.Parameters)}"">
                    <!-- Component content would be rendered here -->
                    <div class=""modal-component-placeholder"">
                        {GetComponentPlaceholder(options as ModalOptions ?? new ModalOptions())}
                    </div>
                </div>");
        }
        
        sb.Append("</div>");
        
        // Footer with buttons
        if ((options?.ShowFooter ?? false) || options?.Buttons?.Any() == true)
        {
            sb.Append(@"<div class=""modal-footer"">");
            
            if (options?.Buttons != null)
            {
                foreach (var button in options.Buttons)
                {
                    var buttonClass = GetButtonClass(button.Type);
                    var buttonData = System.Text.Json.JsonSerializer.Serialize(new { type = button.Type.ToString(), text = button.Text });
                    
                    sb.Append($@"
                        <button class=""{buttonClass}""
                                data-modal-action=""button""
                                data-modal-data=""{buttonData.Replace("\"", "&quot;")}"">
                            {button.Text}
                        </button>");
                }
            }
            else if (options?.ComponentType == typeof(RR.Blazor.Components.Feedback.RConfirmationModal))
            {
                // Default confirmation buttons
                var confirmText = options.Parameters?.GetValueOrDefault("ConfirmText")?.ToString() ?? "Confirm";
                var cancelText = options.Parameters?.GetValueOrDefault("CancelText")?.ToString() ?? "Cancel";
                
                sb.Append($@"
                    <button class=""btn btn-outline""
                            data-modal-action=""cancel"">
                        {cancelText}
                    </button>
                    <button class=""btn btn-primary""
                            data-modal-action=""confirm"">
                        {confirmText}
                    </button>");
            }
            
            sb.Append("</div>");
        }
        
        sb.Append("</div>");
        
        return sb.ToString();
    }

    public async Task CloseAsync(string modalId, Enums.ModalResult result = Enums.ModalResult.None)
    {
        var modal = _activeModals.FirstOrDefault(m => m.Id == modalId);
        if (modal == null) return;

        modal.Visible = false;
        modal.LastResult = result;
        _activeModals.Remove(modal);

        // Complete the task source
        if (_modalCompletionSources.TryGetValue(modalId, out var taskSource))
        {
            taskSource.TrySetResult(result == Enums.ModalResult.Ok ? true : null);
            _modalCompletionSources.Remove(modalId);
        }

        // Destroy modal in JS
        if (_jsRuntime != null)
        {
            try
            {
                await _jsRuntime.InvokeVoidAsync("RRBlazor.Modal.destroy", modalId);
            }
            catch { }
        }

        CleanupDotNetReference(modalId);
        OnModalClosed?.Invoke(modal);

        if (_activeModals.Count == 0)
        {
            OnAllModalsClosed?.Invoke();
        }
    }

    public async Task CloseAllAsync()
    {
        var modalsToClose = _activeModals.ToList();
        _activeModals.Clear();

        foreach (var modal in modalsToClose)
        {
            modal.Visible = false;
            
            if (_jsRuntime != null)
            {
                try
                {
                    await _jsRuntime.InvokeVoidAsync("RRBlazor.Modal.destroy", modal.Id);
                }
                catch { }
            }

            CleanupDotNetReference(modal.Id);
            OnModalClosed?.Invoke(modal);
        }

        _modalCompletionSources.Clear();
        OnAllModalsClosed?.Invoke();
    }

    public bool IsModalOpen(string modalId = null)
    {
        return string.IsNullOrEmpty(modalId)
            ? _activeModals.Exists(m => m.Visible)
            : _activeModals.Exists(m => m.Id == modalId && m.Visible);
    }

    public IModalBuilder<T> Create<T>()
    {
        return new PortalModalBuilder<T>(this);
    }

    private void CleanupDotNetReference(string modalId)
    {
        if (_dotNetReferences.TryGetValue(modalId, out var dotNetRef))
        {
            dotNetRef?.Dispose();
            _dotNetReferences.Remove(modalId);
        }
    }

    private static Dictionary<string, object> ConvertParametersToDict<TParams>(TParams parameters)
    {
        return parameters switch
        {
            null => [],
            Dictionary<string, object> dict => dict,
            _ => typeof(TParams).GetProperties()
                .Select(prop => (prop.Name, Value: prop.GetValue(parameters)))
                .Where(item => item.Value != null)
                .ToDictionary(item => item.Name, item => item.Value)
        };
    }

    private static ModalEvents<object> ConvertEvents(ModalEvents events)
    {
        if (events == null) return null;
        
        // ModalEvents inherits from ModalEvents<object>, so just return it
        return events;
    }

    private string GetModalClass(ModalOptions options)
    {
        var classes = new List<string> { "rr-modal" };
        
        if (options != null)
        {
            classes.Add($"modal-{options.Size.ToString().ToLower()}");
            classes.Add($"modal-{options.Variant.ToString().ToLower()}");
            
            if (!string.IsNullOrEmpty(options.Class))
                classes.Add(options.Class);
        }
        
        return string.Join(" ", classes);
    }

    private string GetBackdropClass(ModalVariant variant)
    {
        return variant switch
        {
            ModalVariant.Destructive => "modal-backdrop-destructive",
            ModalVariant.Warning => "modal-backdrop-warning",
            _ => "modal-backdrop-dark"
        };
    }

    private string GetButtonClass(ModalButtonType type)
    {
        return type switch
        {
            ModalButtonType.Primary => "btn btn-primary",
            ModalButtonType.Secondary => "btn btn-secondary",
            ModalButtonType.Danger => "btn btn-danger",
            ModalButtonType.Cancel => "btn btn-outline",
            _ => "btn btn-default"
        };
    }

    private string GetConfirmationIcon(string variant)
    {
        return variant?.ToLower() switch
        {
            "destructive" => "⚠️",
            "warning" => "⚠",
            "info" => "ℹ",
            "success" => "✓",
            _ => "?"
        };
    }

    private string GetComponentPlaceholder(ModalOptions options)
    {
        // Provide a basic placeholder for components that need server-side rendering
        return $@"<div class=""text-center p-4"">
            <div class=""spinner-border"" role=""status"">
                <span class=""sr-only"">Loading...</span>
            </div>
        </div>";
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            _ = CloseAllAsync();
            _modalModule?.DisposeAsync();
            
            foreach (var dotNetRef in _dotNetReferences.Values)
            {
                dotNetRef?.Dispose();
            }
            
            _dotNetReferences.Clear();
            _disposed = true;
        }
    }

    // Callback handler for modal actions from JS
    public sealed class ModalCallbackHandler
    {
        private readonly string _modalId;
        private readonly ModalServicePortal _service;
        private readonly object _events;

        public ModalCallbackHandler(string modalId, ModalServicePortal service, object events)
        {
            _modalId = modalId;
            _service = service;
            _events = events;
        }

        [JSInvokable]
        public async Task HandleAction(string action, string data)
        {
            switch (action?.ToLower())
            {
                case "close":
                    await _service.CloseAsync(_modalId, Enums.ModalResult.Cancel);
                    break;
                    
                case "confirm":
                    if (_service._modalCompletionSources.TryGetValue(_modalId, out var confirmSource))
                    {
                        confirmSource.TrySetResult(true);
                    }
                    await _service.CloseAsync(_modalId, Enums.ModalResult.Ok);
                    break;
                    
                case "cancel":
                    if (_service._modalCompletionSources.TryGetValue(_modalId, out var cancelSource))
                    {
                        cancelSource.TrySetResult(false);
                    }
                    await _service.CloseAsync(_modalId, Enums.ModalResult.Cancel);
                    break;
                    
                case "button":
                    // Handle custom button clicks
                    if (!string.IsNullOrEmpty(data))
                    {
                        try
                        {
                            var buttonData = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(data);
                            // Process button action
                            await _service.CloseAsync(_modalId, Enums.ModalResult.Ok);
                        }
                        catch { }
                    }
                    break;
            }
        }
    }
}

// Modal builder for portal-based modals
public sealed class PortalModalBuilder<T> : IModalBuilder<T>
{
    private readonly ModalServicePortal _modalService;
    private readonly ModalOptions<T> _options = new();
    private readonly ModalEvents<T> _events = new();

    public PortalModalBuilder(ModalServicePortal modalService)
    {
        _modalService = modalService;
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
        return await _modalService.ShowAsync<T>(
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