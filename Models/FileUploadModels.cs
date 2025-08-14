using Microsoft.AspNetCore.Components.Forms;
using RR.Blazor.Enums;

namespace RR.Blazor.Models;

public class RFileInfo
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N")[..8];
    public string Name { get; set; } = string.Empty;
    public string Extension { get; set; } = string.Empty;
    public long Size { get; set; }
    public string ContentType { get; set; } = string.Empty;
    public DateTime LastModified { get; set; }
    public bool IsImage { get; set; }
    public string ThumbnailUrl { get; set; }
    public IBrowserFile BrowserFile { get; set; }
    public RFileUploadStatus Status { get; set; } = RFileUploadStatus.Pending;
    public string ErrorMessage { get; set; }
    public double UploadProgress { get; set; }
    public RFileMetadata Metadata { get; set; }
    public RFileAnalysisResult Analysis { get; set; }
}

public class RFileUploadResult
{
    public bool Success { get; set; }
    public List<RFileInfo> Files { get; set; } = new();
    public List<string> Errors { get; set; } = new();
    public string Message { get; set; }
}

public class RFileValidationSettings
{
    public string[] AllowedExtensions { get; set; }
    public string[] AllowedMimeTypes { get; set; }
    public long MaxFileSize { get; set; } = 10 * 1024 * 1024; // 10MB default
    public int MaxFiles { get; set; } = 10;
    public bool AllowDuplicates { get; set; } = true;
}

// Enhanced models for AI analysis and chunked upload
public class RFileAnalysisResult
{
    public RFileMetadata Metadata { get; set; } = new();
    public string ContentType { get; set; } = string.Empty;
    public int ProcessingTime { get; set; }
    public string ContentPreview { get; set; }
    public string SuggestedCategory { get; set; } = "Other";
    public double Confidence { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string Error { get; set; }
    public Dictionary<string, int> CategoryScores { get; set; } = new();
}

public class RFileMetadata
{
    public string Name { get; set; } = string.Empty;
    public long Size { get; set; }
    public string Type { get; set; } = string.Empty;
    public DateTime LastModified { get; set; }
    public string Extension { get; set; } = string.Empty;
    public string SizeFormatted { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public int EstimatedProcessingTime { get; set; }
    public string WebkitRelativePath { get; set; } = string.Empty;
    public bool IsImage { get; set; }
    public bool IsDocument { get; set; }
    public bool IsSpreadsheet { get; set; }
    public bool IsArchive { get; set; }
}

public class RChunkProgressArgs
{
    public string FileId { get; set; } = string.Empty;
    public int ChunkIndex { get; set; }
    public int TotalChunks { get; set; }
    public double Progress { get; set; }
    public long BytesUploaded { get; set; }
    public long TotalBytes { get; set; }
}

public class RFileValidationError
{
    public string FileName { get; set; } = string.Empty;
    public List<string> Errors { get; set; } = new();
    public List<string> Warnings { get; set; } = new();
}

public class RUploadProgressStats
{
    public int TotalFiles { get; set; }
    public int CompletedFiles { get; set; }
    public int FailedFiles { get; set; }
    public long BytesUploaded { get; set; }
    public long TotalBytes { get; set; }
    public double Percentage { get; set; }
}

public class REnhancedUploadOptions
{
    // Chunked upload settings
    public bool EnableChunkedUpload { get; set; } = true;
    public long ChunkThreshold { get; set; } = 50 * 1024 * 1024; // 50MB
    public int ChunkSize { get; set; } = 5 * 1024 * 1024; // 5MB
    public int MaxRetries { get; set; } = 3;
    
    // AI Analysis settings
    public bool EnableAnalysis { get; set; }
    public bool EnableCategorization { get; set; }
    
    // Security and validation
    public bool EnableAdvancedValidation { get; set; } = true;
    public string[] DangerousExtensions { get; set; } = 
    {
        ".exe", ".bat", ".cmd", ".scr", ".vbs", ".js", ".jar"
    };
}

