using Microsoft.JSInterop;

namespace RR.Blazor.Services;

public interface IJavaScriptInteropService
{
    Task<bool> IsInteractiveAsync();
    Task<bool> TryInvokeVoidAsync(string identifier, params object[] args);
    Task<T> TryInvokeAsync<T>(string identifier, params object[] args);
}

public class JavaScriptInteropService(IJSRuntime jsRuntime) : IJavaScriptInteropService
{
    private bool? _isInteractiveCache;
    private DateTime _lastCheck;

    public async Task<bool> IsInteractiveAsync()
    {
        if (_isInteractiveCache.HasValue && DateTime.UtcNow - _lastCheck < TimeSpan.FromMilliseconds(1000))
        {
            return _isInteractiveCache.Value;
        }
        
        try
        {
            var isSafe = await jsRuntime.InvokeAsync<bool>("RRBlazor.isSafeForInterop");
            _isInteractiveCache = isSafe;
            _lastCheck = DateTime.UtcNow;
            return isSafe;
        }
        catch
        {
            _isInteractiveCache = false;
            _lastCheck = DateTime.UtcNow;
            return false;
        }
    }
    
    public async Task<bool> TryInvokeVoidAsync(string identifier, params object[] args)
    {
        try
        {
            var allParams = new object[args.Length + 1];
            allParams[0] = identifier;
            Array.Copy(args, 0, allParams, 1, args.Length);
            
            return await jsRuntime.InvokeAsync<bool>("RRBlazor.safeInvoke", allParams);
        }
        catch (JSException)
        {
            return false;
        }
    }
    
    public async Task<T> TryInvokeAsync<T>(string identifier, params object[] args)
    {
        try
        {
            var allParams = new object[args.Length + 1];
            allParams[0] = identifier;
            Array.Copy(args, 0, allParams, 1, args.Length);
            
            return await jsRuntime.InvokeAsync<T>("RRBlazor.safeInvoke", allParams);
        }
        catch (JSException)
        {
            return default(T);
        }
    }
}