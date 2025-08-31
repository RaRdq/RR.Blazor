using RR.Blazor.Enums;

namespace RR.Blazor.Models;

public class FilterFieldChangedEventArgs
{
    public string Property { get; set; } = string.Empty;
    public object? Value { get; set; }
    public FilterFieldType Type { get; set; }
}