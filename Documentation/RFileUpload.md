# RFileUpload - Universal AI-Optimized File Upload Component

## Overview

RFileUpload is a comprehensive, zero-configuration file upload component designed for RR.Blazor. It provides professional file handling with drag/drop, validation, progress tracking, and multiple display modes out of the box.

## Key Features

### 🚀 Zero Configuration
```html
<!-- This works immediately -->
<RFileUpload />
```

### 🎯 AI-Optimized Design
- **Smart Defaults**: Works perfectly without any configuration
- **Intelligent Validation**: Built-in file type, size, and count validation
- **Professional UI**: Uses existing SCSS classes from `_uploads.scss`
- **Accessibility**: Full keyboard navigation and screen reader support

### 🎨 Multiple Display Modes
1. **DropZone** (default) - Large drag/drop area with visual feedback
2. **Button** - Simple button for file selection
3. **Minimal** - Basic file input with custom styling
4. **Inline** - Compact attachment button for forms

### 📊 Advanced Features
- **Drag & Drop**: Visual feedback with dragover states
- **Image Thumbnails**: Automatic preview generation for images
- **Progress Tracking**: Real-time upload progress with events
- **File Validation**: Type, size, and custom validation support
- **Error Handling**: Clear validation messages and error states
- **Mobile Friendly**: Touch-optimized for mobile devices

## Usage Examples

### Basic Usage
```html
<!-- Zero config - works immediately -->
<RFileUpload />

<!-- Basic configuration -->
<RFileUpload AllowedTypes="@(new[] {".pdf", ".jpg", ".png"})"
             MaxSize="5 * 1024 * 1024"
             Multiple="true"
             @bind-Files="uploadedFiles" />
```

### Display Modes
```html
<!-- Drop Zone Mode (default) -->
<RFileUpload DisplayMode="RFileUploadDisplayMode.DropZone"
             UploadText="Drop your files here"
             HintText="PDF, JPG, PNG accepted" />

<!-- Button Mode -->
<RFileUpload DisplayMode="RFileUploadDisplayMode.Button"
             ButtonText="Upload Documents"
             StartIcon="upload" />

<!-- Inline Mode -->
<RFileUpload DisplayMode="RFileUploadDisplayMode.Inline"
             Multiple="false"
             ShowPreview="false" />

<!-- Minimal Mode -->
<RFileUpload DisplayMode="RFileUploadDisplayMode.Minimal" />
```

### Advanced Configuration
```html
<RFileUpload AllowedTypes="@(new[] {".pdf", ".docx", ".xlsx"})"
             MaxSize="10 * 1024 * 1024"
             MaxFiles="5"
             GenerateThumbnails="true"
             ShowProgress="true"
             AllowRemove="true"
             CustomValidator="ValidateFile"
             OnFilesSelected="HandleFilesSelected"
             OnUploadComplete="HandleUploadComplete"
             OnProgressChanged="HandleProgressChanged"
             @bind-Files="files" />
```

## Parameters

### Core Configuration
| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `ComponentId` | string | auto-generated | Unique identifier for the component |
| `DisplayMode` | RFileUploadDisplayMode | DropZone | Visual display mode |
| `IsDisabled` | bool | false | Whether component is disabled |
| `Class` | string? | null | Additional CSS classes |

### File Configuration
| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `AllowedTypes` | string[]? | null | Allowed file extensions or MIME types |
| `MaxSize` | long | 10MB | Maximum file size in bytes |
| `MaxFiles` | int | 10 | Maximum number of files |
| `Multiple` | bool | true | Allow multiple file selection |
| `AllowDuplicates` | bool | true | Allow duplicate filenames |

### UI Configuration
| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `ShowPreview` | bool | true | Show file preview/list |
| `ShowProgress` | bool | true | Show upload progress |
| `DragDrop` | bool | true | Enable drag and drop |
| `GenerateThumbnails` | bool | true | Generate image thumbnails |
| `AllowRemove` | bool | true | Allow file removal |
| `ShowBrowseButton` | bool | true | Show browse button in drop zone |

### Text Configuration
| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `UploadText` | string? | auto-generated | Upload area title text |
| `HintText` | string? | auto-generated | Upload area hint text |
| `HelpText` | string? | null | Help text below component |
| `ButtonText` | string | "Choose Files" | Button text for button mode |
| `BrowseButtonText` | string | "Browse Files" | Browse button text |
| `UploadIcon` | string | "cloud_upload" | Icon for upload area |
| `StartIcon` | string? | null | Start icon for button mode |

### Data Binding & Events
| Parameter | Type | Description |
|-----------|------|-------------|
| `Files` | List<RFileInfo> | Selected files collection |
| `FilesChanged` | EventCallback<List<RFileInfo>> | Callback when files change |
| `OnFilesSelected` | EventCallback<RFileUploadResult> | Called when files are selected |
| `OnUploadComplete` | EventCallback<RFileUploadResult> | Called when upload completes |
| `OnProgressChanged` | EventCallback<(string, double)> | Called when progress changes |
| `OnFileRemoved` | EventCallback<string> | Called when file is removed |
| `CustomValidator` | Func<IBrowserFile, Task<string?>> | Custom validation function |

## Data Models

### RFileInfo
```csharp
public class RFileInfo
{
    public string Id { get; set; }           // Unique identifier
    public string Name { get; set; }         // File name
    public string Extension { get; set; }    // File extension
    public long Size { get; set; }           // File size in bytes
    public string ContentType { get; set; }  // MIME type
    public DateTime LastModified { get; set; } // Last modified date
    public bool IsImage { get; set; }        // Whether file is an image
    public string? ThumbnailUrl { get; set; } // Base64 thumbnail for images
    public IBrowserFile? BrowserFile { get; set; } // Original browser file
    public RFileUploadStatus Status { get; set; } // Upload status
    public string? ErrorMessage { get; set; } // Error message if failed
    public double UploadProgress { get; set; } // Upload progress (0-100)
}
```

### RFileUploadResult
```csharp
public class RFileUploadResult
{
    public bool Success { get; set; }        // Whether operation succeeded
    public List<RFileInfo> Files { get; set; } // Processed files
    public List<string> Errors { get; set; } // Validation errors
    public string? Message { get; set; }     // Result message
}
```

## Event Handling Examples

### File Selection
```csharp
private async Task HandleFilesSelected(RFileUploadResult result)
{
    if (result.Success)
    {
        Console.WriteLine($"Selected {result.Files.Count} files");
        // Process valid files
        foreach (var file in result.Files)
        {
            // Handle each file
        }
    }
    else
    {
        // Handle validation errors
        foreach (var error in result.Errors)
        {
            Console.WriteLine($"Error: {error}");
        }
    }
}
```

### Progress Tracking
```csharp
private async Task HandleProgressChanged((string FileId, double Progress) progressInfo)
{
    var file = files.FirstOrDefault(f => f.Id == progressInfo.FileId);
    if (file != null)
    {
        file.UploadProgress = progressInfo.Progress;
        StateHasChanged();
    }
}
```

### Custom Validation
```csharp
private async Task<string?> ValidateFile(IBrowserFile file)
{
    // Custom validation logic
    if (file.Name.Contains("virus"))
    {
        return "Suspicious filename detected";
    }
    
    if (file.Size > 1024 * 1024) // 1MB limit
    {
        return "File too large for this field";
    }
    
    return null; // Valid
}
```

## Styling

The component uses existing SCSS classes from `_uploads.scss`:

- `.file-upload-area` - Main drop zone styling
- `.upload-zone` - Alternative drop zone with dragover states
- `.file-preview` - File preview cards
- `.upload-progress` - Progress bars with animations
- `.file-attachments` - File list styling

### Custom Styling
```html
<!-- Add custom classes -->
<RFileUpload Class="my-custom-upload border-dashed" />
```

## Validation

### Built-in Validation
- **File Size**: Validates against `MaxSize` parameter
- **File Type**: Validates against `AllowedTypes` parameter
- **File Count**: Validates against `MaxFiles` parameter

### Custom Validation
```csharp
// Async custom validator
private async Task<string?> ValidateDocument(IBrowserFile file)
{
    // Check file content, name patterns, etc.
    if (!file.Name.ToLower().Contains("invoice"))
    {
        return "File must be an invoice document";
    }
    
    return null; // Valid
}
```

## Accessibility

- **Keyboard Navigation**: Full keyboard support for file selection
- **Screen Reader**: Proper ARIA labels and descriptions
- **Focus Management**: Clear focus indicators
- **Error Announcements**: Validation errors announced to screen readers

## Browser Support

- **Modern Browsers**: Chrome, Firefox, Safari, Edge
- **File API**: Uses modern File and FileReader APIs
- **Drag & Drop**: HTML5 drag and drop with fallbacks
- **Mobile**: Touch-optimized for mobile devices

## Performance

- **Lazy Loading**: JavaScript module loaded only when needed
- **Thumbnail Generation**: Efficient canvas-based image thumbnails
- **Memory Management**: Proper cleanup on component disposal
- **Large Files**: Chunked processing for large file uploads

## Integration Examples

### With Forms
```html
<EditForm Model="@model" OnValidSubmit="HandleSubmit">
    <div class="form-group">
        <label>Attach Documents</label>
        <RFileUpload @bind-Files="model.Documents"
                     AllowedTypes="@(new[] {".pdf", ".doc", ".docx"})"
                     MaxFiles="3" />
    </div>
    <button type="submit">Submit</button>
</EditForm>
```

### With Validation
```html
<RFileUpload @bind-Files="files"
             CustomValidator="ValidateFile"
             OnFilesSelected="HandleValidation" />

@if (validationErrors.Any())
{
    <div class="text-error">
        @foreach (var error in validationErrors)
        {
            <div>@error</div>
        }
    </div>
}
```

### Real Upload Implementation
```csharp
private async Task HandleUpload(RFileUploadResult result)
{
    foreach (var fileInfo in result.Files)
    {
        if (fileInfo.BrowserFile != null)
        {
            fileInfo.Status = RFileUploadStatus.Uploading;
            StateHasChanged();
            
            try
            {
                // Actual upload logic
                using var stream = fileInfo.BrowserFile.OpenReadStream();
                var response = await httpClient.PostAsync("/api/upload", 
                    new StreamContent(stream));
                
                if (response.IsSuccessStatusCode)
                {
                    fileInfo.Status = RFileUploadStatus.Completed;
                }
                else
                {
                    fileInfo.Status = RFileUploadStatus.Failed;
                    fileInfo.ErrorMessage = "Upload failed";
                }
            }
            catch (Exception ex)
            {
                fileInfo.Status = RFileUploadStatus.Failed;
                fileInfo.ErrorMessage = ex.Message;
            }
            
            StateHasChanged();
        }
    }
}
```

## Best Practices

1. **Always set MaxSize** to prevent memory issues
2. **Use AllowedTypes** for security and UX
3. **Handle validation errors** gracefully
4. **Show progress feedback** for better UX
5. **Implement real upload logic** in OnFilesSelected
6. **Test with large files** and slow connections
7. **Provide clear error messages** for users

## Security Considerations

- **File Type Validation**: Always validate on server-side too
- **Size Limits**: Enforce both client and server limits
- **Content Scanning**: Scan uploaded files for malware
- **Access Control**: Verify user permissions before upload
- **Temporary Storage**: Clean up temporary files promptly