using Microsoft.JSInterop;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;

namespace RR.Blazor.Services;

public interface IJavaScriptInteropService
{
    Task<bool> IsInteractiveAsync();
    Task<bool> TryInvokeVoidAsync(string identifier, params object[] args);
    Task<T> TryInvokeAsync<T>(string identifier, params object[] args);
}

public class JavaScriptInteropService(IJSRuntime jsRuntime, ILogger<JavaScriptInteropService> logger) : IJavaScriptInteropService
{
    private bool? _isInteractiveCache;
    private DateTime _lastCheck;
    private bool _isWebAssembly = false;

    public async Task<bool> IsInteractiveAsync()
    {
        // Check if we're in prerendering mode (Blazor Server)
        if (jsRuntime is IJSInProcessRuntime)
        {
            _isWebAssembly = true;
            return true; // WebAssembly is always interactive when JS is available
        }
        
        // For Blazor Server, check circuit state
        if (_isInteractiveCache.HasValue && DateTime.UtcNow - _lastCheck < TimeSpan.FromMilliseconds(500))
        {
            return _isInteractiveCache.Value;
        }
        
        try
        {
            // Simple test to see if JS is available and circuit is connected
            await jsRuntime.InvokeAsync<bool>("eval", "true");
            _isInteractiveCache = true;
            _lastCheck = DateTime.UtcNow;
            return true;
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("prerendering") || ex.Message.Contains("statically rendered"))
        {
            // Definitely in prerendering mode
            _isInteractiveCache = false;
            _lastCheck = DateTime.UtcNow;
            return false;
        }
        catch (JSDisconnectedException)
        {
            // Circuit disconnected
            _isInteractiveCache = false;
            _lastCheck = DateTime.UtcNow;
            return false;
        }
        catch (Exception ex)
        {
            // Other JS errors (timeout, etc.)
            logger.LogDebug(ex, "JavaScript interop check failed");
            _isInteractiveCache = false;
            _lastCheck = DateTime.UtcNow;
            return false;
        }
    }
    
    public async Task<bool> TryInvokeVoidAsync(string identifier, params object[] args)
    {
        if (!await IsInteractiveAsync())
        {
            return false;
        }
        
        try
        {
            // For safety, use the RRBlazor wrapper if available
            if (!_isWebAssembly)
            {
                var allParams = new object[args.Length + 1];
                allParams[0] = identifier;
                Array.Copy(args, 0, allParams, 1, args.Length);
                
                return await jsRuntime.InvokeAsync<bool>("RRBlazor.safeInvoke", allParams);
            }
            else
            {
                // Direct call for WebAssembly
                await jsRuntime.InvokeVoidAsync(identifier, args);
                return true;
            }
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("prerendering") || ex.Message.Contains("statically rendered"))
        {
            _isInteractiveCache = false;
            return false;
        }
        catch (JSDisconnectedException)
        {
            _isInteractiveCache = false;
            return false;
        }
        catch (JSException ex)
        {
            logger.LogDebug(ex, "JavaScript invocation failed: {Identifier}", identifier);
            return false;
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Unexpected error during JavaScript invocation: {Identifier}", identifier);
            return false;
        }
    }
    
    public async Task<T> TryInvokeAsync<T>(string identifier, params object[] args)
    {
        if (!await IsInteractiveAsync())
        {
            return default(T);
        }
        
        try
        {
            // For safety, use the RRBlazor wrapper if available
            if (!_isWebAssembly)
            {
                var allParams = new object[args.Length + 1];
                allParams[0] = identifier;
                Array.Copy(args, 0, allParams, 1, args.Length);
                
                return await jsRuntime.InvokeAsync<T>("RRBlazor.safeInvoke", allParams);
            }
            else
            {
                // Direct call for WebAssembly
                return await jsRuntime.InvokeAsync<T>(identifier, args);
            }
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("prerendering") || ex.Message.Contains("statically rendered"))
        {
            _isInteractiveCache = false;
            return default(T);
        }
        catch (JSDisconnectedException)
        {
            _isInteractiveCache = false;
            return default(T);
        }
        catch (JSException ex)
        {
            logger.LogDebug(ex, "JavaScript invocation failed: {Identifier}", identifier);
            return default(T);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Unexpected error during JavaScript invocation: {Identifier}", identifier);
            return default(T);
        }
    }
}