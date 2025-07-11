namespace RR.Blazor.Enums;

public enum RFileUploadDisplayMode
{
    DropZone = 0,
    Button = 1,
    Minimal = 2,
    Inline = 3
}

public enum RFileUploadStatus
{
    Pending = 0,
    Uploading = 1,
    Completed = 2,
    Failed = 3,
    Cancelled = 4
}