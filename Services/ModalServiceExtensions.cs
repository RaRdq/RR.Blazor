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
            ["ModalId"] = modalId,
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
            UseBackdrop = true,
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
        => await ShowFormCoreAsync(modalService, typeof(TComponent), title, initialData, size, onValidate, onSave);

    public static async Task<ModalResult<T>> ShowFormAsync<T>(
        this IModalService modalService,
        string title,
        T initialData = default,
        SizeType size = SizeType.Medium,
        Func<T, Task<bool>> onValidate = null,
        Func<T, Task<bool>> onSave = null)
        => await ShowFormCoreAsync(modalService, null, title, initialData, size, onValidate, onSave);

    private static async Task<ModalResult<T>> ShowFormCoreAsync<T>(
        IModalService modalService,
        Type componentType,
        string title,
        T initialData,
        SizeType size,
        Func<T, Task<bool>> onValidate,
        Func<T, Task<bool>> onSave)
    {
        var parameters = new Dictionary<string, object>
        {
            ["InitialData"] = initialData,
            ["Data"] = initialData,
            ["OnValidate"] = onValidate,
            ["OnSave"] = onSave
        };

        var options = new ModalOptions<T>
        {
            Title = title,
            Size = size,
            Parameters = parameters,
            Data = initialData,
            ComponentType = componentType,
            Buttons = [ModalButton.Cancel("Cancel"), ModalButton.Primary("Save")]
        };

        return componentType != null
            ? await modalService.ShowAsync(componentType, parameters, options)
            : await modalService.ShowAsync(options);
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
        => await ShowDetailCoreAsync(modalService, typeof(TComponent), data, title, size);

    public static async Task ShowDetailAsync<T>(
        this IModalService modalService,
        T data,
        string title = "",
        SizeType size = SizeType.Large)
        => await ShowDetailCoreAsync(modalService, null, data, title, size);

    private static async Task ShowDetailCoreAsync<T>(
        IModalService modalService,
        Type componentType,
        T data,
        string title,
        SizeType size)
    {
        var parameters = new Dictionary<string, object> { ["Data"] = data };
        var options = new ModalOptions<T>
        {
            Title = string.IsNullOrEmpty(title) ? $"{typeof(T).Name} Details" : title,
            Size = size,
            Data = data,
            Parameters = parameters,
            Buttons = [ModalButton.Primary("Close")]
        };

        if (componentType != null)
            await modalService.ShowAsync(componentType, parameters, options);
        else
            await modalService.ShowAsync(options);
    }

    public static async Task ShowPreviewAsync<TComponent>(
        this IModalService modalService,
        string content,
        string title = "Preview",
        string contentType = "text/plain") where TComponent : ComponentBase
        => await ShowPreviewCoreAsync(modalService, typeof(TComponent), content, title, contentType);

    public static async Task ShowPreviewAsync(
        this IModalService modalService,
        string content,
        string title = "Preview",
        string contentType = "text/plain")
        => await ShowPreviewCoreAsync(modalService, null, content, title, contentType);

    private static async Task ShowPreviewCoreAsync(
        IModalService modalService,
        Type componentType,
        string content,
        string title,
        string contentType)
    {
        var parameters = new Dictionary<string, object>
        {
            ["Content"] = content,
            ["ContentType"] = contentType
        };

        var options = new ModalOptions
        {
            Title = title,
            Size = SizeType.Large,
            Parameters = parameters,
            Data = new { Content = content, ContentType = contentType },
            Buttons = [ModalButton.Primary("Close")]
        };

        if (componentType != null)
            await modalService.ShowAsync(componentType, parameters, options);
        else
            await modalService.ShowAsync(options);
    }

    public static async Task<T> ShowSelectAsync<T, TComponent>(
        this IModalService modalService,
        IEnumerable<T> items,
        string title = "Select Item",
        Func<T, string> displaySelector = null) where TComponent : ComponentBase
        => await ShowSelectCoreAsync<T>(modalService, typeof(TComponent), items, title, displaySelector, false);

    public static async Task<T> ShowSelectAsync<T>(
        this IModalService modalService,
        IEnumerable<T> items,
        string title = "Select Item",
        Func<T, string> displaySelector = null)
        => await ShowSelectCoreAsync<T>(modalService, null, items, title, displaySelector, false);

    private static async Task<T> ShowSelectCoreAsync<T>(
        IModalService modalService,
        Type componentType,
        IEnumerable<T> items,
        string title,
        Func<T, string> displaySelector,
        bool allowMultiple)
    {
        var parameters = new Dictionary<string, object>
        {
            ["Items"] = items,
            ["DisplaySelector"] = displaySelector ?? (item => item?.ToString() ?? ""),
            ["AllowMultiple"] = allowMultiple
        };

        var options = new ModalOptions<T>
        {
            Title = title,
            Size = SizeType.Medium,
            Data = items.FirstOrDefault(),
            Parameters = parameters,
            Buttons = [ModalButton.Cancel(), ModalButton.Primary("Select")]
        };

        var result = componentType != null
            ? await modalService.ShowAsync<T>(componentType, parameters, options)
            : await modalService.ShowAsync(options);

        return result.IsConfirmed ? result.Data : default;
    }

    public static async Task<IEnumerable<T>> ShowMultiSelectAsync<T, TComponent>(
        this IModalService modalService,
        IEnumerable<T> items,
        string title = "Select Items",
        Func<T, string> displaySelector = null) where TComponent : ComponentBase
        => await ShowMultiSelectCoreAsync<T>(modalService, typeof(TComponent), items, title, displaySelector);

    public static async Task<IEnumerable<T>> ShowMultiSelectAsync<T>(
        this IModalService modalService,
        IEnumerable<T> items,
        string title = "Select Items",
        Func<T, string> displaySelector = null)
        => await ShowMultiSelectCoreAsync<T>(modalService, null, items, title, displaySelector);

    private static async Task<IEnumerable<T>> ShowMultiSelectCoreAsync<T>(
        IModalService modalService,
        Type componentType,
        IEnumerable<T> items,
        string title,
        Func<T, string> displaySelector)
    {
        var parameters = new Dictionary<string, object>
        {
            ["Items"] = items,
            ["DisplaySelector"] = displaySelector ?? (item => item?.ToString() ?? ""),
            ["AllowMultiple"] = true
        };

        var options = new ModalOptions<IEnumerable<T>>
        {
            Title = title,
            Size = SizeType.Medium,
            Data = items,
            Parameters = parameters,
            Buttons = [ModalButton.Cancel(), ModalButton.Primary("Select")]
        };

        var result = componentType != null
            ? await modalService.ShowAsync<IEnumerable<T>>(componentType, parameters, options)
            : await modalService.ShowAsync(options);

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
    public static async Task<ModalResult<T>> ShowRawAsync<T, TComponent>(
        this IModalService modalService,
        string title = null,
        Dictionary<string, object> parameters = null,
        SizeType size = SizeType.Medium,
        List<ModalButton> buttons = null) where TComponent : ComponentBase
        => await ShowRawCoreAsync<T>(modalService, typeof(TComponent), title, parameters, size, buttons);

    /// <summary>
    /// Shows a modal without expecting a return value by wrapping raw component content in RModal.
    /// Use this for simple dialogs that don't return data.
    /// </summary>
    public static async Task ShowRawAsync<TComponent>(
        this IModalService modalService,
        string title = null,
        Dictionary<string, object> parameters = null,
        SizeType size = SizeType.Medium) where TComponent : ComponentBase
    {
        await ShowRawCoreAsync<object>(modalService, typeof(TComponent), title, parameters, size, [ModalButton.Primary("Close")]);
    }

    private static async Task<ModalResult<T>> ShowRawCoreAsync<T>(
        IModalService modalService,
        Type componentType,
        string title,
        Dictionary<string, object> parameters,
        SizeType size,
        List<ModalButton> buttons)
    {
        var options = new ModalOptions<T>
        {
            Title = title ?? componentType.Name,
            Size = size,
            Buttons = buttons ?? [ModalButton.Primary("OK")]
        };

        return await modalService.ShowRawAsync<T>(componentType, parameters, options);
    }
    

}