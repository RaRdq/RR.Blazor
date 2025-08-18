using System;
using System.Threading.Tasks;

namespace RR.Blazor.Models;

public class ModalEvents<T>
{
    public Func<Task> OnShow { get; set; }
    public Func<T, Task> OnClose { get; set; }
    public Func<Task> OnCancel { get; set; }
    public Func<T, Task<bool>> OnValidate { get; set; }
    public Func<string, T, Task> OnAction { get; set; }
}

public class ModalEvents : ModalEvents<object>
{
}