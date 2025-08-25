namespace RR.Blazor.Models;

public class TestModalParams
{
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    
    public TestModalParams() { }
    
    public TestModalParams(string title, string message)
    {
        Title = title;
        Message = message;
    }
}