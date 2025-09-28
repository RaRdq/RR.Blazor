namespace RR.Blazor.Models;

/// <summary>
/// Modal context for service-managed modals
/// Cascaded by RModalProvider to indicate modal is managed by ModalService
/// </summary>
public class ModalContext
{
    public string ModalId { get; set; }
}
