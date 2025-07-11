using Microsoft.Extensions.DependencyInjection;
using System;

namespace RR.Blazor.Services;

/// <summary>
/// Toast notification types
/// </summary>
public enum ToastType
{
    /// <summary>Default/no specific type</summary>
    None = 0,
    /// <summary>Success notification</summary>
    Success = 1,
    /// <summary>Error notification</summary>
    Error = 2,
    /// <summary>Warning notification</summary>
    Warning = 4,
    /// <summary>Informational notification</summary>
    Info = 8
}

/// <summary>
/// Toast notification position on screen
/// </summary>
public enum ToastPosition
{
    TopRight,
    TopLeft,
    TopCenter,
    BottomRight,
    BottomLeft,
    BottomCenter
}

/// <summary>
/// Toast message model
/// </summary>
public class ToastMessage
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Message { get; set; } = string.Empty;
    public string Title { get; set; }
    public ToastType Type { get; set; } = ToastType.Info;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public int Duration { get; set; } = 4000; // milliseconds
    public bool ShowCloseButton { get; set; } = true;
    public string Icon { get; set; }
    public string ActionText { get; set; }
    public Action OnAction { get; set; }
}

/// <summary>
/// Toast service configuration
/// </summary>
public class ToastServiceOptions
{
    public ToastPosition Position { get; set; } = ToastPosition.TopRight;
    public int MaxToasts { get; set; } = 5;
    public int DefaultDuration { get; set; } = 4000;
    public bool ShowCloseButton { get; set; } = true;
    public bool NewestOnTop { get; set; } = true;
    public bool PreventDuplicates { get; set; } = false;
}

/// <summary>
/// Toast service interface
/// </summary>
public interface IToastService
{
    event Action<ToastMessage> OnShow;
    event Action<string> OnRemove;
    event Action OnClearAll;
    
    ToastServiceOptions Options { get; }
    
    void Show(string message, ToastType type = ToastType.Info, string title = null);
    void Show(ToastMessage toast);
    void ShowSuccess(string message, string title = null);
    void ShowError(string message, string title = null);
    void ShowWarning(string message, string title = null);
    void ShowInfo(string message, string title = null);
    void Remove(string id);
    void ClearAll();
}

/// <summary>
/// Toast notification service
/// </summary>
public class ToastService(ToastServiceOptions options = null) : IToastService
{
    private readonly HashSet<string> activeToasts = new();
    
    public event Action<ToastMessage> OnShow;
    public event Action<string> OnRemove;
    public event Action OnClearAll;
    
    public ToastServiceOptions Options { get; } = options ?? new ToastServiceOptions();

    public void Show(string message, ToastType type = ToastType.Info, string title = null)
    {
        var toast = new ToastMessage
        {
            Message = message,
            Title = title,
            Type = type,
            Duration = Options.DefaultDuration,
            ShowCloseButton = Options.ShowCloseButton
        };
        
        Show(toast);
    }
    
    public void Show(ToastMessage toast)
    {
        if (toast == null) throw new ArgumentNullException(nameof(toast));
        
        // Check for duplicates if enabled
        if (Options.PreventDuplicates && activeToasts.Contains(GetToastKey(toast)))
            return;
        
        activeToasts.Add(toast.Id);
        OnShow?.Invoke(toast);
    }
    
    public void ShowSuccess(string message, string title = null)
    {
        Show(message, ToastType.Success, title);
    }
    
    public void ShowError(string message, string title = null)
    {
        Show(message, ToastType.Error, title);
    }
    
    public void ShowWarning(string message, string title = null)
    {
        Show(message, ToastType.Warning, title);
    }
    
    public void ShowInfo(string message, string title = null)
    {
        Show(message, ToastType.Info, title);
    }
    
    public void Remove(string id)
    {
        if (activeToasts.Remove(id))
        {
            OnRemove?.Invoke(id);
        }
    }
    
    public void ClearAll()
    {
        activeToasts.Clear();
        OnClearAll?.Invoke();
    }
    
    private string GetToastKey(ToastMessage toast)
    {
        return $"{toast.Type}:{toast.Title}:{toast.Message}";
    }
}

/// <summary>
/// Service collection extensions for toast service
/// </summary>
public static class ToastServiceExtensions
{
    public static IServiceCollection AddRRToast(this IServiceCollection services, Action<ToastServiceOptions> configure = null)
    {
        var options = new ToastServiceOptions();
        configure?.Invoke(options);
        
        services.AddSingleton(options);
        services.AddScoped<IToastService, ToastService>();
        
        return services;
    }
}