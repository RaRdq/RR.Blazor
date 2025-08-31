using Microsoft.AspNetCore.Components;
using RR.Blazor.Components.Feedback;
using RR.Blazor.Enums;
using RR.Blazor.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
    
    public static async Task<bool> ShowConfirmationAsync<TComponent>(
        this IModalService modalService,
        string message,
        string title = "Confirm",
        string confirmText = "Confirm",
        string cancelText = "Cancel",
        VariantType variant = VariantType.Default) where TComponent : ComponentBase
    {
        var parameters = new Dictionary<string, object>
        {
            { "Message", message },
            { "Variant", variant },
            { "IsDestructive", variant == VariantType.Error }
        };

        var options = new ModalOptions<bool>
        {
            Title = title,
            Icon = GetIconForVariant(variant),
            Size = SizeType.Small,
            Variant = variant,
            CloseOnBackdrop = false,
            CloseOnEscape = true,
            Buttons = string.IsNullOrEmpty(cancelText) 
                ? new List<ModalButton>
                {
                    variant == VariantType.Error
                        ? ModalButton.Error(confirmText)
                        : ModalButton.Primary(confirmText)
                }
                : new List<ModalButton>
                {
                    ModalButton.Cancel(cancelText),
                    variant == VariantType.Error
                        ? ModalButton.Error(confirmText)
                        : ModalButton.Primary(confirmText)
                }
        };

        var result = await modalService.ShowAsync(typeof(TComponent), parameters, options);
        return result.IsConfirmed;
    }
    
    // Overload for backward compatibility that uses default confirmation modal
    public static Task<bool> ShowConfirmationAsync(
        this IModalService modalService,
        string message,
        string title = "Confirm",
        string confirmText = "Confirm",
        string cancelText = "Cancel",
        VariantType variant = VariantType.Default)
    {
        // Use RConfirmationModal as default but via generic compile-time resolution
        return modalService.ShowConfirmationAsync<RConfirmationModal>(
            message, title, confirmText, cancelText, variant);
    }

    public static Task<bool> ShowConfirmationAsync(
        this IModalService modalService,
        ConfirmationOptions confirmOptions)
    {
        return modalService.ShowConfirmationAsync(
            confirmOptions.Message,
            confirmOptions.Title,
            confirmOptions.ConfirmText ?? "Confirm",
            confirmOptions.CancelText ?? "Cancel",
            confirmOptions.IsDestructive ? VariantType.Error : confirmOptions.Variant);
    }

    public static async Task<bool> ShowInfoAsync(
        this IModalService modalService,
        string message,
        string title = "Information")
    {
        var parameters = new Dictionary<string, object>
        {
            { "Message", message },
            { "Variant", VariantType.Info },
            { "IsDestructive", false }
        };

        var options = new ModalOptions<bool>
        {
            Title = title,
            Icon = "info", // Set the info icon
            Size = SizeType.Small,
            Variant = VariantType.Info,
            CloseOnBackdrop = true,
            CloseOnEscape = true,
            Buttons = new List<ModalButton>
            {
                ModalButton.Primary("OK")
            }
        };

        var result = await modalService.ShowAsync(typeof(RConfirmationModal), parameters, options);
        return result.IsConfirmed;
    }

    public static async Task<bool> ShowWarningAsync(
        this IModalService modalService,
        string message,
        string title = "Warning")
    {
        var parameters = new Dictionary<string, object>
        {
            { "Message", message },
            { "Variant", VariantType.Warning },
            { "IsDestructive", false }
        };

        var options = new ModalOptions<bool>
        {
            Title = title,
            Icon = "warning", // Set the warning icon
            Size = SizeType.Small,
            Variant = VariantType.Warning,
            CloseOnBackdrop = true,
            CloseOnEscape = true,
            Buttons = new List<ModalButton>
            {
                ModalButton.Primary("OK")
            }
        };

        var result = await modalService.ShowAsync(typeof(RConfirmationModal), parameters, options);
        return result.IsConfirmed;
    }

    public static Task<bool> ShowErrorAsync(
        this IModalService modalService,
        string message,
        string title = "Error")
    {
        return modalService.ShowConfirmationAsync(message, title, "OK", "", VariantType.Error);
    }

    public static Task<bool> ShowSuccessAsync(
        this IModalService modalService,
        string message,
        string title = "Success")
    {
        return modalService.ShowConfirmationAsync(message, title, "OK", "", VariantType.Success);
    }

    public static Task<bool> ConfirmAsync(
        this IModalService modalService,
        string message,
        string title = "Confirm",
        bool isDestructive = false)
    {
        return modalService.ShowConfirmationAsync(
            message, 
            title, 
            "Confirm", 
            "Cancel", 
            isDestructive ? VariantType.Error : VariantType.Default);
    }

    public static async Task<ModalResult<T>> ShowFormAsync<T, TComponent>(
        this IModalService modalService,
        string title,
        T initialData = default,
        SizeType size = SizeType.Medium,
        Func<T, Task<bool>> onValidate = null,
        Func<T, Task<bool>> onSave = null) where TComponent : ComponentBase
    {
        var parameters = new Dictionary<string, object>
        {
            { "InitialData", initialData },
            { "OnValidate", onValidate },
            { "OnSave", onSave }
        };

        var options = new ModalOptions<T>
        {
            Title = title,
            Size = size,
            Parameters = parameters,
            Buttons = new List<ModalButton>
            {
                ModalButton.Cancel("Cancel"),
                ModalButton.Primary("Save")
            }
        };

        return await modalService.ShowAsync(typeof(TComponent), parameters, options);
    }
    
    // Overload that allows passing any form component type dynamically
    public static async Task<ModalResult<T>> ShowFormAsync<T>(
        this IModalService modalService,
        string title,
        T initialData = default,
        SizeType size = SizeType.Medium,
        Func<T, Task<bool>> onValidate = null,
        Func<T, Task<bool>> onSave = null)
    {
        // Just pass parameters - let modal provider handle the content
        var parameters = new Dictionary<string, object>
        {
            { "Data", initialData },
            { "OnValidate", onValidate },
            { "OnSave", onSave }
        };

        var options = new ModalOptions<T>
        {
            Title = title,
            Size = size,
            Parameters = parameters,
            Data = initialData,
            Buttons = new List<ModalButton>
            {
                ModalButton.Cancel("Cancel"),
                ModalButton.Primary("Save", async data =>
                {
                    if (onValidate != null)
                        return await onValidate((T)data);
                    return true;
                })
            }
        };

        return await modalService.ShowAsync(options);
    }

    public static async Task<ModalResult<T>> ShowFormAsync<T>(
        this IModalService modalService,
        FormModalOptions<T> options)
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
                ModalButton.Cancel(options.CancelButtonText ?? "Cancel"),
                ModalButton.Primary(options.SaveButtonText ?? "Save", async data =>
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

        return await modalService.ShowAsync(modalOptions);
    }

    public static async Task ShowDetailAsync<T, TComponent>(
        this IModalService modalService,
        T data,
        string title = "",
        SizeType size = SizeType.Large) where TComponent : ComponentBase
    {
        var parameters = new Dictionary<string, object>
        {
            { "Data", data }
        };

        var options = new ModalOptions<T>
        {
            Title = string.IsNullOrEmpty(title) ? $"{typeof(T).Name} Details" : title,
            Size = size,
            Data = data,
            Buttons = new List<ModalButton> { ModalButton.Primary("Close") }
        };

        await modalService.ShowAsync(typeof(TComponent), parameters, options);
    }
    
    // Overload for displaying data without specific component
    public static async Task ShowDetailAsync<T>(
        this IModalService modalService,
        T data,
        string title = "",
        SizeType size = SizeType.Large)
    {
        var options = new ModalOptions<T>
        {
            Title = string.IsNullOrEmpty(title) ? $"{typeof(T).Name} Details" : title,
            Size = size,
            Data = data,
            Parameters = new Dictionary<string, object> { { "Data", data } },
            Buttons = new List<ModalButton> { ModalButton.Primary("Close") }
        };

        await modalService.ShowAsync(options);
    }

    public static async Task ShowPreviewAsync<TComponent>(
        this IModalService modalService,
        string content,
        string title = "Preview",
        string contentType = "text/plain") where TComponent : ComponentBase
    {
        var parameters = new Dictionary<string, object>
        {
            { "Content", content },
            { "ContentType", contentType }
        };

        var options = new ModalOptions
        {
            Title = title,
            Size = SizeType.Large,
            Buttons = new List<ModalButton> { ModalButton.Primary("Close") }
        };

        await modalService.ShowAsync(typeof(TComponent), parameters, options);
    }
    
    // Overload for preview without specific component
    public static async Task ShowPreviewAsync(
        this IModalService modalService,
        string content,
        string title = "Preview",
        string contentType = "text/plain")
    {
        var options = new ModalOptions
        {
            Title = title,
            Size = SizeType.Large,
            Data = new { Content = content, ContentType = contentType },
            Buttons = new List<ModalButton> { ModalButton.Primary("Close") }
        };

        await modalService.ShowAsync(options);
    }

    public static async Task<T> ShowSelectAsync<T, TComponent>(
        this IModalService modalService,
        IEnumerable<T> items,
        string title = "Select Item",
        Func<T, string> displaySelector = null) where TComponent : ComponentBase
    {
        var parameters = new Dictionary<string, object>
        {
            { "Items", items },
            { "DisplaySelector", displaySelector ?? (item => item?.ToString() ?? "") },
            { "AllowMultiple", false }
        };

        var options = new ModalOptions<T>
        {
            Title = title,
            Size = SizeType.Medium,
            Buttons = new List<ModalButton>
            {
                ModalButton.Cancel(),
                ModalButton.Primary("Select")
            }
        };

        var result = await modalService.ShowAsync<T>(typeof(TComponent), parameters, options);
        return result.IsConfirmed ? result.Data : default;
    }
    
    // Overload without specific component
    public static async Task<T> ShowSelectAsync<T>(
        this IModalService modalService,
        IEnumerable<T> items,
        string title = "Select Item",
        Func<T, string> displaySelector = null)
    {
        var options = new ModalOptions<T>
        {
            Title = title,
            Size = SizeType.Medium,
            Data = items.FirstOrDefault(),
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

        var result = await modalService.ShowAsync(options);
        return result.IsConfirmed ? result.Data : default;
    }

    public static async Task<IEnumerable<T>> ShowMultiSelectAsync<T, TComponent>(
        this IModalService modalService,
        IEnumerable<T> items,
        string title = "Select Items",
        Func<T, string> displaySelector = null) where TComponent : ComponentBase
    {
        var parameters = new Dictionary<string, object>
        {
            { "Items", items },
            { "DisplaySelector", displaySelector ?? (item => item?.ToString() ?? "") },
            { "AllowMultiple", true }
        };

        var options = new ModalOptions<IEnumerable<T>>
        {
            Title = title,
            Size = SizeType.Medium,
            Buttons = new List<ModalButton>
            {
                ModalButton.Cancel(),
                ModalButton.Primary("Select")
            }
        };

        var result = await modalService.ShowAsync<IEnumerable<T>>(typeof(TComponent), parameters, options);
        return result.IsConfirmed ? result.Data : Enumerable.Empty<T>();
    }
    
    // Overload without specific component
    public static async Task<IEnumerable<T>> ShowMultiSelectAsync<T>(
        this IModalService modalService,
        IEnumerable<T> items,
        string title = "Select Items",
        Func<T, string> displaySelector = null)
    {
        var options = new ModalOptions<IEnumerable<T>>
        {
            Title = title,
            Size = SizeType.Medium,
            Data = items,
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
            paramDict = new Dictionary<string, object>();
            var type = parameters.GetType();
            foreach (var prop in type.GetProperties())
            {
                paramDict[prop.Name] = prop.GetValue(parameters);
            }
        }

        var typedOptions = new ModalOptions<T>
        {
            Title = options?.Title,
            Subtitle = options?.Subtitle,
            Icon = options?.Icon,
            Size = options?.Size ?? SizeType.Medium,
            Variant = options?.Variant ?? VariantType.Default,
            ComponentType = componentType,
            Parameters = paramDict ?? new Dictionary<string, object>()
        };

        return await modalService.ShowAsync(typedOptions);
    }

}