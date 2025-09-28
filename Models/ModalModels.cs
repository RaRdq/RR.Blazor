using Microsoft.AspNetCore.Components;
using RR.Blazor.Enums;

namespace RR.Blazor.Models;

public class ModalOptions
{
    public string? ModalId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Subtitle { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public SizeType Size { get; set; } = SizeType.Medium;
    public VariantType Variant { get; set; } = VariantType.Default;
    public bool CloseOnBackdrop { get; set; } = true;
    public bool CloseOnEscape { get; set; } = true;
    public bool ShowCloseButton { get; set; } = true;
    public bool ShowHeader { get; set; } = true;
    public bool ShowFooter { get; set; } = true;
    public string Class { get; set; } = string.Empty;
    public Type? ComponentType { get; set; }
    public Dictionary<string, object> Parameters { get; set; } = [];
    public List<ModalButton> Buttons { get; set; } = [];
    public object? Data { get; set; }
    public TimeSpan? AutoCloseDelay { get; set; }
    public AnimationType Animation { get; set; } = AnimationType.Scale;
    public AnimationSpeed AnimationSpeed { get; set; } = AnimationSpeed.Normal;
    public bool UsePortal { get; set; } = true;
    public bool UseBackdrop { get; set; } = true;
    public bool IsRawContent { get; set; }
    public int BackdropBlur { get; set; } = 8;
    public string BackdropClassName { get; set; } = "modal-backdrop-dark";
    public double BackdropOpacity { get; set; } = 0.6;
    
    public T GetData<T>() => Data is T typedData ? typedData : default;
}

public class ModalOptions<T> : ModalOptions
{
    public new T? Data
    {
        get => GetData<T>();
        set => base.Data = value;
    }
}

public sealed class ModalButton
{
    public string Text { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public VariantType Variant { get; set; } = VariantType.Primary;
    public bool IsDisabled { get; set; }
    public bool IsLoading { get; set; }
    public string Class { get; set; } = string.Empty;
    public Func<object?, Task<bool>>? OnClick { get; set; }
    public Enums.ModalResult Result { get; set; } = Enums.ModalResult.Custom;

    public static ModalButton Cancel(string text = "Cancel", Func<object?, Task<bool>>? onClick = null) => new()
    {
        Text = text,
        Variant = VariantType.Secondary,
        Result = Enums.ModalResult.Cancel,
        OnClick = onClick ?? (_ => Task.FromResult(true))
    };

    public static ModalButton Primary(string text = "Save", Func<object?, Task<bool>>? onClick = null, Enums.ModalResult result = Enums.ModalResult.Ok) => new()
    {
        Text = text,
        Variant = VariantType.Primary,
        Result = result,
        OnClick = onClick ?? (_ => Task.FromResult(true))
    };

    public static ModalButton Success(string text = "Confirm", Func<object?, Task<bool>>? onClick = null) => new()
    {
        Text = text,
        Variant = VariantType.Success,
        Result = Enums.ModalResult.Yes,
        OnClick = onClick ?? (_ => Task.FromResult(true))
    };

    public static ModalButton Error(string text = "Delete", Func<object?, Task<bool>>? onClick = null) => new()
    {
        Text = text,
        Variant = VariantType.Error,
        Result = Enums.ModalResult.Delete,
        OnClick = onClick ?? (_ => Task.FromResult(true))
    };

    public static ModalButton Warning(string text = "Continue", Func<object?, Task<bool>>? onClick = null) => new()
    {
        Text = text,
        Variant = VariantType.Warning,
        Result = Enums.ModalResult.Ok,
        OnClick = onClick ?? (_ => Task.FromResult(true))
    };

    public static ModalButton Info(string text = "Info", Func<object?, Task<bool>>? onClick = null) => new()
    {
        Text = text,
        Variant = VariantType.Info,
        Result = Enums.ModalResult.Ok,
        OnClick = onClick ?? (_ => Task.FromResult(true))
    };

    public static ModalButton Custom(string text, VariantType variant = VariantType.Default, Func<object?, Task<bool>>? onClick = null, Enums.ModalResult result = Enums.ModalResult.Custom) => new()
    {
        Text = text,
        Variant = variant,
        Result = result,
        OnClick = onClick ?? (_ => Task.FromResult(true))
    };
}

public class ModalInstance
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public ModalOptions Options { get; set; } = new();
    public TaskCompletionSource<ModalResult> TaskSource { get; } = new();
    public DateTime CreatedAt { get; } = DateTime.UtcNow;
    public bool Visible { get; set; }
    public object? Result { get; set; }
    public string? jsModalId { get; set; }
}

public sealed class ModalInstance<T> : ModalInstance
{
    public ModalInstance() => Options = new ModalOptions<T> { Title = string.Empty };
    
    public T? TypedResult
    {
        get => Result is T typedResult ? typedResult : default;
        set => Result = value;
    }
}

public record ModalResult
{
    public Enums.ModalResult ResultType { get; init; } = Enums.ModalResult.None;
    public object? Data { get; init; }
    public bool IsConfirmed => ResultType is Enums.ModalResult.Ok or Enums.ModalResult.Yes or Enums.ModalResult.Save or Enums.ModalResult.Delete;
}

public sealed record ModalResult<T> : ModalResult
{
    public new T? Data { get; init; }
}

public record ConfirmationOptions
{
    public string Message { get; init; } = string.Empty;
    public string Title { get; init; } = "Confirm";
    public string? ConfirmText { get; init; }
    public string? CancelText { get; init; }
    public VariantType Variant { get; init; } = VariantType.Default;
    public bool IsDestructive { get; init; }
}

public record FormModalOptions<T>
{
    public string Title { get; init; } = string.Empty;
    public string Subtitle { get; init; } = string.Empty;
    public T? InitialData { get; init; }
    public SizeType Size { get; init; } = SizeType.Medium;
    public Type? FormComponentType { get; init; }
    public Dictionary<string, object> FormParameters { get; init; } = [];
    public string? SaveButtonText { get; init; }
    public string? CancelButtonText { get; init; }
    public Func<T, Task<bool>>? OnValidate { get; init; }
    public Func<T, Task<bool>>? OnSave { get; init; }
}