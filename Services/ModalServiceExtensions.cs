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
    public static async Task<bool> ShowConfirmationAsync(
        this IModalServiceCore modalService,
        string message,
        string title = "Confirm",
        string confirmText = "Confirm",
        string cancelText = "Cancel",
        ModalVariant variant = ModalVariant.Default)
    {
        var parameters = new Dictionary<string, object>
        {
            { nameof(RConfirmationModal.Title), title },
            { nameof(RConfirmationModal.Message), message },
            { nameof(RConfirmationModal.ConfirmText), confirmText },
            { nameof(RConfirmationModal.CancelText), cancelText },
            { nameof(RConfirmationModal.Variant), 
                variant == ModalVariant.Destructive ? ConfirmationVariant.Destructive : 
                variant == ModalVariant.Warning ? ConfirmationVariant.Warning : 
                ConfirmationVariant.Info },
            { nameof(RConfirmationModal.Visible), true }
        };

        var options = new ModalOptions
        {
            Title = title,
            Icon = variant == ModalVariant.Destructive ? "error" : "help",
            Size = SizeType.Small,
            Variant = variant,
            CloseOnBackdrop = false,
            CloseOnEscape = true
        };

        var result = await modalService.ShowAsync<bool>(
            typeof(RConfirmationModal),
            parameters,
            options);

        return result.IsConfirmed && result.Data;
    }

    public static async Task<bool> ShowConfirmationAsync(
        this IModalServiceCore modalService,
        ConfirmationOptions confirmOptions)
    {
        return await modalService.ShowConfirmationAsync(
            confirmOptions.Message,
            confirmOptions.Title,
            confirmOptions.ConfirmText ?? "Confirm",
            confirmOptions.CancelText ?? "Cancel",
            confirmOptions.IsDestructive ? ModalVariant.Destructive : confirmOptions.Variant);
    }

    public static async Task ShowInfoAsync(
        this IModalServiceCore modalService,
        string message,
        string title = "Information")
    {
        var parameters = new Dictionary<string, object>
        {
            { nameof(RMessageModal.Message), message }
        };

        var options = new ModalOptions
        {
            Title = title,
            Icon = "info",
            Size = SizeType.Small,
            Variant = ModalVariant.Info,
            Buttons = new List<ModalButton> { ModalButton.Primary("OK") }
        };

        await modalService.ShowAsync(
            typeof(RMessageModal),
            parameters,
            options);
    }

    public static async Task ShowWarningAsync(
        this IModalServiceCore modalService,
        string message,
        string title = "Warning")
    {
        var parameters = new Dictionary<string, object>
        {
            { nameof(RMessageModal.Message), message }
        };

        var options = new ModalOptions
        {
            Title = title,
            Icon = "warning",
            Size = SizeType.Small,
            Variant = ModalVariant.Warning,
            Buttons = new List<ModalButton> { ModalButton.Primary("OK") }
        };

        await modalService.ShowAsync(
            typeof(RMessageModal),
            parameters,
            options);
    }

    public static async Task ShowErrorAsync(
        this IModalServiceCore modalService,
        string message,
        string title = "Error")
    {
        var parameters = new Dictionary<string, object>
        {
            { nameof(RMessageModal.Message), message }
        };

        var options = new ModalOptions
        {
            Title = title,
            Icon = "error",
            Size = SizeType.Small,
            Variant = ModalVariant.Destructive,
            Buttons = new List<ModalButton> { ModalButton.Primary("OK") }
        };

        await modalService.ShowAsync(
            typeof(RMessageModal),
            parameters,
            options);
    }

    public static async Task ShowSuccessAsync(
        this IModalServiceCore modalService,
        string message,
        string title = "Success")
    {
        var parameters = new Dictionary<string, object>
        {
            { nameof(RMessageModal.Message), message }
        };

        var options = new ModalOptions
        {
            Title = title,
            Icon = "check_circle",
            Size = SizeType.Small,
            Variant = ModalVariant.Success,
            Buttons = new List<ModalButton> { ModalButton.Success("OK") },
            AutoCloseDelay = TimeSpan.FromSeconds(3)
        };

        await modalService.ShowAsync(
            typeof(RMessageModal),
            parameters,
            options);
    }

    public static async Task<ModalResult<T>> ShowFormAsync<T>(
        this IModalServiceCore modalService,
        string title,
        T initialData = default,
        SizeType size = SizeType.Medium,
        Func<T, Task<bool>> onValidate = null,
        Func<T, Task<bool>> onSave = null)
    {
        var parameters = new Dictionary<string, object>
        {
            { "InitialData", initialData },
            { "OnValidate", onValidate },
            { "OnSave", onSave }
        };

        var options = new ModalOptions
        {
            Title = title,
            Size = size,
            ComponentType = typeof(RFormModal<>).MakeGenericType(typeof(T)),
            Buttons = new List<ModalButton>
            {
                ModalButton.Cancel("Cancel"),
                ModalButton.Primary("Save")
            }
        };

        var events = new ModalEvents<T>
        {
            OnValidate = onValidate
        };

        return await modalService.ShowAsync<T>(
            options.ComponentType,
            parameters,
            options,
            events);
    }

    public static async Task<ModalResult<T>> ShowFormAsync<T>(
        this IModalServiceCore modalService,
        FormModalOptions<T> formOptions)
    {
        return await modalService.ShowFormAsync(
            formOptions.Title,
            formOptions.InitialData,
            formOptions.Size,
            formOptions.OnValidate,
            formOptions.OnSave);
    }

    public static async Task ShowDetailAsync<T>(
        this IModalServiceCore modalService,
        T data,
        string title = "",
        SizeType size = SizeType.Large)
    {
        var parameters = new Dictionary<string, object>
        {
            { "Data", data }
        };

        var options = new ModalOptions<T>
        {
            Title = string.IsNullOrEmpty(title) ? $"{typeof(T).Name} Details" : title,
            Size = size,
            ComponentType = typeof(RDetailModal<>).MakeGenericType(typeof(T)),
            Data = data,
            Buttons = new List<ModalButton> { ModalButton.Primary("Close") }
        };

        await modalService.ShowAsync(
            options.ComponentType,
            parameters,
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
            });
    }

    public static async Task ShowPreviewAsync(
        this IModalServiceCore modalService,
        string content,
        string title = "Preview",
        string contentType = "text/plain")
    {
        var parameters = new Dictionary<string, object>
        {
            { nameof(RPreviewModal.Content), content },
            { nameof(RPreviewModal.ContentType), contentType }
        };

        var options = new ModalOptions
        {
            Title = title,
            Size = SizeType.Large,
            Buttons = new List<ModalButton> { ModalButton.Primary("Close") }
        };

        await modalService.ShowAsync(
            typeof(RPreviewModal),
            parameters,
            options);
    }

    public static async Task<T> ShowSelectAsync<T>(
        this IModalServiceCore modalService,
        IEnumerable<T> items,
        string title = "Select Item",
        Func<T, string> displaySelector = null)
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
            ComponentType = typeof(RSelectModalGeneric<>).MakeGenericType(typeof(T)),
            Buttons = new List<ModalButton>
            {
                ModalButton.Cancel(),
                ModalButton.Primary("Select")
            }
        };

        var result = await modalService.ShowAsync<T>(
            options.ComponentType,
            parameters,
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
            });

        return result.IsConfirmed ? result.Data : default;
    }

    public static async Task<IEnumerable<T>> ShowMultiSelectAsync<T>(
        this IModalServiceCore modalService,
        IEnumerable<T> items,
        string title = "Select Items",
        Func<T, string> displaySelector = null)
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
            ComponentType = typeof(RSelectModalGeneric<>).MakeGenericType(typeof(T)),
            Buttons = new List<ModalButton>
            {
                ModalButton.Cancel(),
                ModalButton.Primary("Select")
            }
        };

        var result = await modalService.ShowAsync<IEnumerable<T>>(
            options.ComponentType,
            parameters,
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
            });

        return result.IsConfirmed ? result.Data : Enumerable.Empty<T>();
    }

    public static async Task<ModalResult<T>> ShowCustomAsync<T>(
        this IModalServiceCore modalService,
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

        return await modalService.ShowAsync<T>(
            componentType,
            paramDict,
            options);
    }

    public static IModalBuilder<T> BuildModal<T>(this IModalServiceCore modalService)
    {
        return modalService.Create<T>();
    }
}