using Microsoft.AspNetCore.Components;
using RR.Blazor.Enums;

namespace RR.Blazor.Models;

public class ModalOptions<T>
{
    public string Title { get; set; } = "";
    public string Subtitle { get; set; } = "";
    public string Icon { get; set; } = "";
    public SizeType Size { get; set; } = SizeType.Medium;
    public VariantType Variant { get; set; } = VariantType.Default;
    public bool CloseOnBackdrop { get; set; } = true;
    public bool CloseOnEscape { get; set; } = true;
    public bool ShowCloseButton { get; set; } = true;
    public bool ShowHeader { get; set; } = true;
    public bool ShowFooter { get; set; } = true;
    public string Class { get; set; } = "";
    public Type ComponentType { get; set; }
    public Dictionary<string, object> Parameters { get; set; } = new();
    public List<ModalButton> Buttons { get; set; } = new();
    public T Data { get; set; }
    public TimeSpan? AutoCloseDelay { get; set; }
    public AnimationType Animation { get; set; } = AnimationType.Scale;
    public AnimationSpeed AnimationSpeed { get; set; } = AnimationSpeed.Normal;
    public bool UsePortal { get; set; } = true;
    public bool UseBackdrop { get; set; } = true;
}

public class ModalOptions : ModalOptions<object>
{
}

public class ModalButton
{
    public string Text { get; set; } = "";
    public string Icon { get; set; } = "";
    public VariantType Variant { get; set; } = VariantType.Primary;
    public bool IsDisabled { get; set; }
    public bool IsLoading { get; set; }
    public string Class { get; set; } = "";
    public Func<object, Task<bool>> OnClick { get; set; }
    public Enums.ModalResult Result { get; set; } = Enums.ModalResult.Custom;

    public static ModalButton Cancel(string text = "Cancel", Func<object, Task<bool>> onClick = null)
    {
        return new ModalButton
        {
            Text = text,
            Variant = VariantType.Secondary,
            Result = Enums.ModalResult.Cancel,
            OnClick = onClick ?? (_ => Task.FromResult(true))
        };
    }

    public static ModalButton Primary(string text = "Save", Func<object, Task<bool>> onClick = null, Enums.ModalResult result = Enums.ModalResult.Ok)
    {
        return new ModalButton
        {
            Text = text,
            Variant = VariantType.Primary,
            Result = result,
            OnClick = onClick ?? (_ => Task.FromResult(true))
        };
    }

    public static ModalButton Success(string text = "Confirm", Func<object, Task<bool>> onClick = null)
    {
        return new ModalButton
        {
            Text = text,
            Variant = VariantType.Success,
            Result = Enums.ModalResult.Yes,
            OnClick = onClick ?? (_ => Task.FromResult(true))
        };
    }

    public static ModalButton Error(string text = "Delete", Func<object, Task<bool>> onClick = null)
    {
        return new ModalButton
        {
            Text = text,
            Variant = VariantType.Error,
            Result = Enums.ModalResult.Delete,
            OnClick = onClick ?? (_ => Task.FromResult(true))
        };
    }

    [Obsolete("Use Error instead - Danger variant no longer exists")]
    public static ModalButton Danger(string text = "Delete", Func<object, Task<bool>> onClick = null)
    {
        return Error(text, onClick);
    }
}

public class ModalInstance<T>
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public ModalOptions<T> Options { get; set; } = new();
    public TaskCompletionSource<ModalResult<T>> TaskSource { get; set; } = new();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool Visible { get; set; }
    public T Result { get; set; }
}

public class ModalInstance : ModalInstance<object>
{
}

public class ModalResult
{
    public Enums.ModalResult ResultType { get; set; } = Enums.ModalResult.None;
    public object Data { get; set; }
    public bool IsCancelled => ResultType == Enums.ModalResult.Cancel;
    public bool IsConfirmed => ResultType is Enums.ModalResult.Ok or Enums.ModalResult.Yes or Enums.ModalResult.Save;

    public static ModalResult Cancel() => new() { ResultType = Enums.ModalResult.Cancel };
    public static ModalResult Ok(object data = default) => new() { ResultType = Enums.ModalResult.Ok, Data = data };
    public static ModalResult Yes(object data = default) => new() { ResultType = Enums.ModalResult.Yes, Data = data };
    public static ModalResult No() => new() { ResultType = Enums.ModalResult.No };
    public static ModalResult Save(object data = default) => new() { ResultType = Enums.ModalResult.Save, Data = data };
    public static ModalResult Delete() => new() { ResultType = Enums.ModalResult.Delete };
}

public class ModalResult<T>
{
    public Enums.ModalResult ResultType { get; set; } = Enums.ModalResult.None;
    public T Data { get; set; }
    public bool IsCancelled => ResultType == Enums.ModalResult.Cancel;
    public bool IsConfirmed => ResultType is Enums.ModalResult.Ok or Enums.ModalResult.Yes or Enums.ModalResult.Save;

    public static ModalResult<T> Cancel() => new() { ResultType = Enums.ModalResult.Cancel };
    public static ModalResult<T> Ok(T data = default) => new() { ResultType = Enums.ModalResult.Ok, Data = data };
    public static ModalResult<T> Yes(T data = default) => new() { ResultType = Enums.ModalResult.Yes, Data = data };
    public static ModalResult<T> No() => new() { ResultType = Enums.ModalResult.No };
    public static ModalResult<T> Save(T data = default) => new() { ResultType = Enums.ModalResult.Save, Data = data };
    public static ModalResult<T> Delete() => new() { ResultType = Enums.ModalResult.Delete };
}

public class ConfirmationOptions
{
    public string Title { get; set; } = "Confirm Action";
    public string Message { get; set; } = "";
    public string Icon { get; set; } = "help";
    public string ConfirmText { get; set; } = "Confirm";
    public string CancelText { get; set; } = "Cancel";
    public VariantType Variant { get; set; } = VariantType.Default;
    public bool IsDestructive { get; set; }
}

public class FormModalOptions<T>
{
    public string Title { get; set; } = "";
    public string Subtitle { get; set; } = "";
    public T InitialData { get; set; }
    public Type FormComponentType { get; set; }
    public Dictionary<string, object> FormParameters { get; set; } = new();
    public SizeType Size { get; set; } = SizeType.Medium;
    public string SaveButtonText { get; set; } = "Save";
    public string CancelButtonText { get; set; } = "Cancel";
    public Func<T, Task<bool>> OnValidate { get; set; }
    public Func<T, Task<bool>> OnSave { get; set; }
}