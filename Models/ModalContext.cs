namespace RR.Blazor.Models;

/// <summary>
/// Modal context marker for SOW compliance
/// Components with this parameter have internal RModal wrapper
/// Components without this need RModal wrapper from provider
/// </summary>
public class ModalContext
{
    public string ModalId { get; set; }
    public ModalOptions Options { get; set; }
    public object Result { get; set; }
}
