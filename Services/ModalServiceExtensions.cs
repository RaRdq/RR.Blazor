using Microsoft.AspNetCore.Components;
using RR.Blazor.Components.Feedback;
using RR.Blazor.Enums;
using RR.Blazor.Models;

namespace RR.Blazor.Services;

public static class ModalServiceExtensions
{
    private static string GetIconForVariant(VariantType variant) => variant switch
    {
        VariantType.Info => "info",
        VariantType.Warning => "warning",
        VariantType.Error => "error",
        VariantType.Success => "check_circle",
        VariantType.Primary => "star",
        VariantType.Secondary => "circle",
        _ => "help_outline"
    };
    
    /// <summary>
    /// Shows a confirmation dialog with customizable buttons and styling.
    /// Returns true if user confirms, false if cancelled.
    /// </summary>
    public static async Task<bool> ShowConfirmationAsync(
        this IModalService modalService,
        string message,
        string title = "Confirm",
        string confirmText = "Yes",
        string cancelText = "Cancel",
        VariantType variant = VariantType.Default)
    {
        var tcs = new TaskCompletionSource<bool>();
        var modalId = $"confirmation-{Guid.NewGuid():N}";
        
        Dictionary<string, object> parameters = new()
        {
            ["Message"] = message,
            ["Title"] = title,
            ["ConfirmText"] = confirmText,
            ["CancelText"] = cancelText,
            ["Variant"] = variant,
            ["Icon"] = GetIconForVariant(variant),
            ["Size"] = SizeType.Small,
            ["CloseOnBackdrop"] = variant != VariantType.Error,
            ["CloseOnEscape"] = true,
            ["ShowCloseButton"] = true,
            ["ShowHeader"] = true,
            ["OnConfirm"] = EventCallback.Factory.Create(modalService, async () => 
            {
                tcs.TrySetResult(true);
                await modalService.CloseAsync(modalId, Enums.ModalResult.Ok);
            }),
            ["OnCancel"] = EventCallback.Factory.Create(modalService, async () => 
            {
                tcs.TrySetResult(false);
                await modalService.CloseAsync(modalId, Enums.ModalResult.Cancel);
            })
        };

        ModalOptions<bool> options = new()
        {
            ComponentType = typeof(RConfirmationModal),
            Parameters = parameters,
            CloseOnBackdrop = variant != VariantType.Error,
            CloseOnEscape = true,
            ModalId = modalId
        };

        _ = modalService.ShowAsync(options);
        
        return await tcs.Task;
    }
    
    public static Task<bool> ShowConfirmationAsync(
        this IModalService modalService,
        ConfirmationOptions confirmOptions) => modalService.ShowConfirmationAsync(
            confirmOptions.Message,
            confirmOptions.Title,
            confirmOptions.ConfirmText ?? "Confirm",
            confirmOptions.CancelText ?? "Cancel",
            confirmOptions.IsDestructive ? VariantType.Error : confirmOptions.Variant);

    public static Task<bool> ShowInfoAsync(this IModalService modalService, string message, string title = "Information")
        => modalService.ShowConfirmationAsync(message, title, "OK", "", VariantType.Info);

    public static Task<bool> ShowWarningAsync(this IModalService modalService, string message, string title = "Warning")
        => modalService.ShowConfirmationAsync(message, title, "OK", "", VariantType.Warning);
    
    public static Task<bool> ShowErrorAsync(this IModalService modalService, string message, string title = "Error")
        => modalService.ShowConfirmationAsync(message, title, "OK", "", VariantType.Error);

    public static Task<bool> ShowSuccessAsync(this IModalService modalService, string message, string title = "Success")
        => modalService.ShowConfirmationAsync(message, title, "OK", "", VariantType.Success);

    public static async Task<ModalResult<T>> ShowFormAsync<T, TComponent>(
        this IModalService modalService,
        string title,
        T initialData = default,
        SizeType size = SizeType.Medium,
        Func<T, Task<bool>> onValidate = null,
        Func<T, Task<bool>> onSave = null) where TComponent : ComponentBase
    {
        Dictionary<string, object> parameters = new()
        {
            ["InitialData"] = initialData,
            ["OnValidate"] = onValidate,
            ["OnSave"] = onSave
        };

        ModalOptions<T> options = new()
        {
            Title = title,
            Size = size,
            Parameters = parameters,
            Buttons = [ModalButton.Cancel("Cancel"), ModalButton.Primary("Save")]
        };

        return await modalService.ShowAsync(typeof(TComponent), parameters, options);
    }
    
    public static async Task<ModalResult<T>> ShowFormAsync<T>(
        this IModalService modalService,
        string title,
        T initialData = default,
        SizeType size = SizeType.Medium,
        Func<T, Task<bool>> onValidate = null,
        Func<T, Task<bool>> onSave = null)
    {
        Dictionary<string, object> parameters = new()
        {
            ["Data"] = initialData,
            ["OnValidate"] = onValidate,
            ["OnSave"] = onSave
        };

        ModalOptions<T> options = new()
        {
            Title = title,
            Size = size,
            Parameters = parameters,
            Data = initialData,
            Buttons =
            [
                ModalButton.Cancel("Cancel"),
                ModalButton.Primary("Save", async data => onValidate != null ? await onValidate((T)data) : true)
            ]
        };

        return await modalService.ShowAsync(options);
    }

    public static async Task<ModalResult<T>> ShowFormAsync<T>(
        this IModalService modalService,
        FormModalOptions<T> options)
    {
        ModalOptions<T> modalOptions = new()
        {
            Title = options.Title,
            Subtitle = options.Subtitle,
            Size = options.Size,
            ComponentType = options.FormComponentType ?? typeof(RFormModal<>).MakeGenericType(typeof(T)),
            Parameters = new()
            {
                ["InitialData"] = options.InitialData,
                ["OnValidate"] = options.OnValidate,
                ["OnSave"] = options.OnSave
            },
            Data = options.InitialData,
            Buttons =
            [
                ModalButton.Cancel(options.CancelButtonText ?? "Cancel"),
                ModalButton.Primary(options.SaveButtonText ?? "Save",
                    async data => options.OnValidate != null ? await options.OnValidate((T)data) : true)
            ]
        };

        foreach (var (key, value) in options.FormParameters)
            modalOptions.Parameters[key] = value;

        return await modalService.ShowAsync(modalOptions);
    }

    public static async Task ShowDetailAsync<T, TComponent>(
        this IModalService modalService,
        T data,
        string title = "",
        SizeType size = SizeType.Large) where TComponent : ComponentBase
    {
        Dictionary<string, object> parameters = new() { ["Data"] = data };

        ModalOptions<T> options = new()
        {
            Title = string.IsNullOrEmpty(title) ? $"{typeof(T).Name} Details" : title,
            Size = size,
            Data = data,
            Buttons = [ModalButton.Primary("Close")]
        };

        await modalService.ShowAsync(typeof(TComponent), parameters, options);
    }
    
    public static async Task ShowDetailAsync<T>(
        this IModalService modalService,
        T data,
        string title = "",
        SizeType size = SizeType.Large)
    {
        ModalOptions<T> options = new()
        {
            Title = string.IsNullOrEmpty(title) ? $"{typeof(T).Name} Details" : title,
            Size = size,
            Data = data,
            Parameters = new() { ["Data"] = data },
            Buttons = [ModalButton.Primary("Close")]
        };

        await modalService.ShowAsync(options);
    }

    public static async Task ShowPreviewAsync<TComponent>(
        this IModalService modalService,
        string content,
        string title = "Preview",
        string contentType = "text/plain") where TComponent : ComponentBase
    {
        Dictionary<string, object> parameters = new()
        {
            ["Content"] = content,
            ["ContentType"] = contentType
        };

        ModalOptions options = new()
        {
            Title = title,
            Size = SizeType.Large,
            Buttons = [ModalButton.Primary("Close")]
        };

        await modalService.ShowAsync(typeof(TComponent), parameters, options);
    }
    
    public static async Task ShowPreviewAsync(
        this IModalService modalService,
        string content,
        string title = "Preview",
        string contentType = "text/plain")
    {
        ModalOptions options = new()
        {
            Title = title,
            Size = SizeType.Large,
            Data = new { Content = content, ContentType = contentType },
            Buttons = [ModalButton.Primary("Close")]
        };

        await modalService.ShowAsync(options);
    }

    public static async Task<T> ShowSelectAsync<T, TComponent>(
        this IModalService modalService,
        IEnumerable<T> items,
        string title = "Select Item",
        Func<T, string> displaySelector = null) where TComponent : ComponentBase
    {
        Dictionary<string, object> parameters = new()
        {
            ["Items"] = items,
            ["DisplaySelector"] = displaySelector ?? (item => item?.ToString() ?? ""),
            ["AllowMultiple"] = false
        };

        ModalOptions<T> options = new()
        {
            Title = title,
            Size = SizeType.Medium,
            Buttons = [ModalButton.Cancel(), ModalButton.Primary("Select")]
        };

        var result = await modalService.ShowAsync<T>(typeof(TComponent), parameters, options);
        return result.IsConfirmed ? result.Data : default;
    }
    
    public static async Task<T> ShowSelectAsync<T>(
        this IModalService modalService,
        IEnumerable<T> items,
        string title = "Select Item",
        Func<T, string> displaySelector = null)
    {
        ModalOptions<T> options = new()
        {
            Title = title,
            Size = SizeType.Medium,
            Data = items.FirstOrDefault(),
            Parameters = new()
            {
                ["Items"] = items,
                ["DisplaySelector"] = displaySelector ?? (item => item?.ToString() ?? ""),
                ["AllowMultiple"] = false
            },
            Buttons = [ModalButton.Cancel(), ModalButton.Primary("Select")]
        };

        var result = await modalService.ShowAsync(options);
        return result.IsConfirmed ? result.Data : default;
    }

    public static async Task<IEnumerable<T>> ShowMultiSelectAsync<T, TComponent>(
        this IModalService modalService,
        IEnumerable<T> items,
        string title = "Select Items",
        Func<T, string> displaySelector = null) where TComponent : ComponentBase
    {
        Dictionary<string, object> parameters = new()
        {
            ["Items"] = items,
            ["DisplaySelector"] = displaySelector ?? (item => item?.ToString() ?? ""),
            ["AllowMultiple"] = true
        };

        ModalOptions<IEnumerable<T>> options = new()
        {
            Title = title,
            Size = SizeType.Medium,
            Buttons = [ModalButton.Cancel(), ModalButton.Primary("Select")]
        };

        var result = await modalService.ShowAsync<IEnumerable<T>>(typeof(TComponent), parameters, options);
        return result.IsConfirmed ? result.Data : Enumerable.Empty<T>();
    }
    
    public static async Task<IEnumerable<T>> ShowMultiSelectAsync<T>(
        this IModalService modalService,
        IEnumerable<T> items,
        string title = "Select Items",
        Func<T, string> displaySelector = null)
    {
        ModalOptions<IEnumerable<T>> options = new()
        {
            Title = title,
            Size = SizeType.Medium,
            Data = items,
            Parameters = new()
            {
                ["Items"] = items,
                ["DisplaySelector"] = displaySelector ?? (item => item?.ToString() ?? ""),
                ["AllowMultiple"] = true
            },
            Buttons = [ModalButton.Cancel(), ModalButton.Primary("Select")]
        };

        var result = await modalService.ShowAsync(options);
        return result.IsConfirmed ? result.Data : Enumerable.Empty<T>();
    }

    public static async Task<ModalResult<T>> ShowCustomAsync<T>(
        this IModalService modalService,
        Type componentType,
        object parameters = null,
        ModalOptions options = null)
    {
        var paramDict = parameters as Dictionary<string, object>;
        if (paramDict == null && parameters != null)
        {
            paramDict = [];
            var type = parameters.GetType();
            foreach (var prop in type.GetProperties())
                paramDict[prop.Name] = prop.GetValue(parameters);
        }

        ModalOptions<T> typedOptions = new()
        {
            Title = options?.Title,
            Subtitle = options?.Subtitle,
            Icon = options?.Icon,
            Size = options?.Size ?? SizeType.Medium,
            Variant = options?.Variant ?? VariantType.Default,
            ComponentType = componentType,
            Parameters = paramDict ?? []
        };

        return await modalService.ShowAsync(typedOptions);
    }
    
    /// <summary>
    /// Shows a modal by wrapping raw component content in RModal automatically.
    /// Use this when your component does NOT contain RModal inside it.
    /// </summary>
    /// <typeparam name="T">The type of data returned by the modal</typeparam>
    /// <typeparam name="TComponent">The component type to display (must NOT contain RModal)</typeparam>
    /// <param name="modalService">The modal service instance</param>
    /// <param name="title">Optional title for the modal. If null, uses component type name</param>
    /// <param name="parameters">Parameters to pass to the component</param>
    /// <param name="size">Size of the modal (Small, Medium, Large, XLarge, Wide)</param>
    /// <param name="buttons">Modal buttons. If null, adds a single OK button</param>
    /// <returns>Modal result containing the data and result type</returns>
    /// <remarks>
    /// This method automatically wraps the component in RModal. 
    /// Use ShowAsync() for components that already contain RModal internally.
    /// </remarks>
    public static async Task<ModalResult<T>> ShowRawAsync<T, TComponent>(
        this IModalService modalService,
        string title = null,
        Dictionary<string, object> parameters = null,
        SizeType size = SizeType.Medium,
        List<ModalButton> buttons = null) where TComponent : ComponentBase
    {
        ModalOptions<T> options = new()
        {
            Title = title ?? typeof(TComponent).Name,
            Size = size,
            Buttons = buttons ?? [ModalButton.Primary("OK")]
        };

        return await modalService.ShowRawAsync<T>(typeof(TComponent), parameters, options);
    }
    
    /// <summary>
    /// Shows a modal without expecting a return value by wrapping raw component content in RModal.
    /// Use this for simple dialogs that don't return data.
    /// </summary>
    /// <typeparam name="TComponent">The component type to display (must NOT contain RModal)</typeparam>
    /// <param name="modalService">The modal service instance</param>
    /// <param name="title">Optional title for the modal. If null, uses component type name</param>
    /// <param name="parameters">Parameters to pass to the component</param>
    /// <param name="size">Size of the modal (Small, Medium, Large, XLarge, Wide)</param>
    /// <remarks>
    /// This method automatically wraps the component in RModal. 
    /// Use ShowAsync() for components that already contain RModal internally.
    /// </remarks>
    public static async Task ShowRawAsync<TComponent>(
        this IModalService modalService,
        string title = null,
        Dictionary<string, object> parameters = null,
        SizeType size = SizeType.Medium) where TComponent : ComponentBase
    {
        ModalOptions<object> options = new()
        {
            Title = title ?? typeof(TComponent).Name,
            Size = size,
            Buttons = [ModalButton.Primary("Close")]
        };

        await modalService.ShowRawAsync<object>(typeof(TComponent), parameters, options);
    }
    

}