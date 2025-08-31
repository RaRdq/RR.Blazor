using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.RenderTree;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.JSInterop;
using RR.Blazor.Enums;
using RR.Blazor.Models;
using System.Text;
using System.Text.Json;

namespace RR.Blazor.Services;

/// <summary>
/// Renders Blazor components directly to portal DOM elements
/// </summary>
public sealed class ModalPortalRenderer : IDisposable
{
    private readonly IJSRuntime _jsRuntime;
    private readonly IServiceProvider _serviceProvider;
    private readonly Dictionary<string, ModalRenderContext> _activeContexts = new();
    private bool _disposed;

    public ModalPortalRenderer(IJSRuntime jsRuntime, IServiceProvider serviceProvider)
    {
        _jsRuntime = jsRuntime;
        _serviceProvider = serviceProvider;
    }

    public async Task<string> RenderModalToPortalAsync(
        string modalId,
        Type componentType,
        Dictionary<string, object> parameters,
        ModalOptions options)
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(ModalPortalRenderer));

        // Create render context for this modal
        var context = new ModalRenderContext
        {
            ModalId = modalId,
            ComponentType = componentType,
            Parameters = parameters ?? new(),
            Options = options
        };

        _activeContexts[modalId] = context;

        try
        {
            // Create portal via JS
            var portalOptions = new
            {
                id = modalId,
                useBackdrop = options?.CloseOnBackdrop ?? true,
                closeOnBackdrop = options?.CloseOnBackdrop ?? true,
                closeOnEscape = options?.CloseOnEscape ?? true,
                trapFocus = true,
                animation = "scale",
                animationSpeed = "normal",
                backdropClass = GetBackdropClass(options?.Variant ?? VariantType.Default),
                className = GetModalClass(options)
            };

            // Create a container div for the modal content
            await _jsRuntime.InvokeVoidAsync("eval", $@"
                (function() {{
                    // Create modal container element
                    const modalContainer = document.createElement('div');
                    modalContainer.id = '{modalId}-container';
                    modalContainer.className = 'modal-content-container';
                    modalContainer.setAttribute('data-modal-id', '{modalId}');
                    
                    // Store reference for later
                    window.__modalContainers = window.__modalContainers || {{}};
                    window.__modalContainers['{modalId}'] = modalContainer;
                    
                    // Create the modal in portal with the container
                    window.RRBlazor.Modal.create(modalContainer, {System.Text.Json.JsonSerializer.Serialize(portalOptions)});
                }})();
            ");

            // Render the Blazor component into the container
            await RenderComponentToContainerAsync(context);

            return modalId;
        }
        catch (Exception ex)
        {
            _activeContexts.Remove(modalId);
            throw new InvalidOperationException($"Failed to render modal to portal: {ex.Message}", ex);
        }
    }

    private async Task RenderComponentToContainerAsync(ModalRenderContext context)
    {
        var componentHtml = await GenerateComponentHtmlAsync(context);
        
        // Inject the HTML into the modal container
        await _jsRuntime.InvokeVoidAsync("eval", $@"
            (function() {{
                const container = window.__modalContainers['{context.ModalId}'];
                if (container) {{
                    container.innerHTML = {System.Text.Json.JsonSerializer.Serialize(componentHtml)};
                    
                    // Trigger Blazor component initialization if needed
                    if (window.Blazor) {{
                        // For Blazor Server/WebAssembly, reconnect event handlers
                        window.Blazor._internal?.attachRootComponentToElement?.(
                            container.firstElementChild,
                            '{context.ComponentType.FullName}'
                        );
                    }}
                }}
            }})();
        ");
    }

    private async Task<string> GenerateComponentHtmlAsync(ModalRenderContext context)
    {
        var sb = new StringBuilder();
        
        // Build modal structure based on options
        sb.Append("<div class=\"modal-wrapper\">");
        
        // Modal header if needed
        if (context.Options?.ShowHeader ?? true)
        {
            sb.Append("<div class=\"modal-header\">");
            
            if (!string.IsNullOrEmpty(context.Options?.Icon))
            {
                sb.Append($"<span class=\"modal-icon\">{context.Options.Icon}</span>");
            }
            
            if (!string.IsNullOrEmpty(context.Options?.Title))
            {
                sb.Append($"<h2 class=\"modal-title\">{context.Options.Title}</h2>");
            }
            
            if (!string.IsNullOrEmpty(context.Options?.Subtitle))
            {
                sb.Append($"<p class=\"modal-subtitle\">{context.Options.Subtitle}</p>");
            }
            
            if (context.Options?.ShowCloseButton ?? true)
            {
                sb.Append($@"
                    <button class=""modal-close-button"" 
                            onclick=""window.RRBlazor.Modal.destroy('{context.ModalId}')"">
                        <span aria-hidden=""true"">&times;</span>
                    </button>");
            }
            
            sb.Append("</div>");
        }
        
        // Modal body with component placeholder
        sb.Append("<div class=\"modal-body\" id=\"" + context.ModalId + "-body\">");
        
        // Create blazor component mount point
        sb.Append($@"
            <div class=""blazor-modal-component"" 
                 data-component-type=""{context.ComponentType.FullName}""
                 data-modal-id=""{context.ModalId}"">
                <!-- Component will be rendered here -->
            </div>");
        
        sb.Append("</div>");
        
        // Modal footer if needed
        if ((context.Options?.ShowFooter ?? false) || context.Options?.Buttons?.Any() == true)
        {
            sb.Append("<div class=\"modal-footer\">");
            
            if (context.Options?.Buttons != null)
            {
                foreach (var button in context.Options.Buttons)
                {
                    var buttonClass = GetButtonClass(button.Variant);
                    var onClick = $"window.__modalButtonClick('{context.ModalId}', '{button.Text}')";
                    
                    sb.Append($@"
                        <button class=""{buttonClass}"" 
                                onclick=""{onClick}"">
                            {button.Text}
                        </button>");
                }
            }
            
            sb.Append("</div>");
        }
        
        sb.Append("</div>");
        
        return sb.ToString();
    }

    public async Task DestroyModalAsync(string modalId)
    {
        if (_activeContexts.Remove(modalId, out var context))
        {
            try
            {
                // Clean up JS references
                await _jsRuntime.InvokeVoidAsync("eval", $@"
                    (function() {{
                        // Destroy the modal via modal.js
                        window.RRBlazor.Modal.destroy('{modalId}');
                        
                        // Clean up container reference
                        if (window.__modalContainers && window.__modalContainers['{modalId}']) {{
                            delete window.__modalContainers['{modalId}'];
                        }}
                    }})();
                ");
            }
            catch (JSException)
            {
                // Modal may already be destroyed on JS side
            }
        }
    }

    public async Task<DotNetObjectReference<T>> CreateDotNetReferenceAsync<T>(T instance) where T : class
    {
        return DotNetObjectReference.Create(instance);
    }

    private string GetModalClass(ModalOptions options)
    {
        var classes = new List<string> { "rr-modal" };
        
        if (options != null)
        {
            if (options.Size != SizeType.Medium) // Assuming Medium is default
                classes.Add($"modal-{options.Size.ToString().ToLower()}");
            
            if (options.Variant != VariantType.Default) // Assuming Default is default
                classes.Add($"modal-{options.Variant.ToString().ToLower()}");
            
            if (!string.IsNullOrEmpty(options.Class))
                classes.Add(options.Class);
        }
        
        return string.Join(" ", classes);
    }

    private string GetBackdropClass(VariantType variant)
    {
        return variant switch
        {
            VariantType.Error => "modal-backdrop-destructive",
            VariantType.Warning => "modal-backdrop-warning",
            _ => "modal-backdrop-dark"
        };
    }

    private string GetButtonClass(VariantType variant)
    {
        return variant switch
        {
            VariantType.Primary => "btn btn-primary",
            VariantType.Secondary => "btn btn-secondary",
            VariantType.Error => "btn btn-danger",
            VariantType.Success => "btn btn-success",
            VariantType.Warning => "btn btn-warning",
            VariantType.Info => "btn btn-info",
            _ => "btn btn-default"
        };
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            // Clean up all active modals
            var modalIds = _activeContexts.Keys.ToList();
            foreach (var modalId in modalIds)
            {
                _ = DestroyModalAsync(modalId);
            }
            
            _activeContexts.Clear();
            _disposed = true;
        }
    }

    private sealed class ModalRenderContext
    {
        public string ModalId { get; set; }
        public Type ComponentType { get; set; }
        public Dictionary<string, object> Parameters { get; set; }
        public ModalOptions Options { get; set; }
    }
}