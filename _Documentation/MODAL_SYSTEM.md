# Modal System - RR.Blazor

## Overview

RR.Blazor's modal system provides enterprise-grade modal dialogs that render directly to DOM portals for optimal performance and z-index management. The system eliminates the need for component-based modal hosts by leveraging JavaScript portal management.

## Architecture

### Portal-Based Rendering

The modal system uses a portal-based architecture:

1. **ModalProvider Component** (`RModalProvider.razor`) - Manages portal system and renders modals
2. **JavaScript Portal System** (`portal.js`) - Creates DOM portals at document.body level
3. **Modal Manager** (`modal.js`) - Handles modal lifecycle, animations, and events
4. **ModalService** (`ModalService.cs`) - Blazor service interface
5. **Modal Components** (`RModal`, `RConfirmationModal`) - Blazor modal components

### Key Benefits

- **Simple ModalProvider setup** - Single component manages all modals
- **Better z-index management** - Portal system handles stacking automatically
- **Improved performance** - Direct DOM manipulation without Blazor reconciliation
- **Clean separation of concerns** - JavaScript handles DOM, Blazor handles data

## Usage

### Basic Setup

```csharp
// Program.cs
builder.Services.AddRRBlazor();
```

```razor
<!-- App.razor or MainLayout.razor -->
<ModalProvider />
@Body
```

### Confirmation Modals

```csharp
@inject IModalService ModalService

// Simple confirmation
var confirmed = await ModalService.ConfirmAsync("Are you sure?", "Confirm Action");

// Destructive confirmation
var confirmed = await ModalService.ConfirmAsync(
    "This action cannot be undone!", 
    "Delete Item", 
    true // isDestructive
);

// Custom confirmation
var options = new ConfirmationOptions
{
    Title = "Custom Confirmation",
    Message = "This is a custom confirmation dialog.",
    ConfirmText = "Yes, Proceed",
    CancelText = "No, Cancel",
    Variant = ModalVariant.Warning
};
var confirmed = await ModalService.ConfirmAsync(options);
```

### Enterprise Confirmation Modals

For enterprise scenarios requiring validation:

```csharp
var result = await ModalService.ShowAsync<bool>(
    typeof(RConfirmationModal),
    new Dictionary<string, object>
    {
        ["Title"] = "Delete Employee",
        ["Message"] = $"Permanently delete <strong>{employee.Name}</strong>?",
        ["Icon"] = "delete_forever",
        ["Variant"] = ConfirmationVariant.Destructive,
        ["ShowInputField"] = true,
        ["InputLabel"] = "Type employee's last name to confirm",
        ["InputPlaceholder"] = employee.LastName,
        ["InputRequired"] = true,
        ["OnInputValidate"] = new Func<string, Task<bool>>(input => 
            Task.FromResult(string.Equals(input, employee.LastName, StringComparison.OrdinalIgnoreCase))),
        ["ShowDetails"] = true,
        ["DetailsDictionary"] = new Dictionary<string, string>
        {
            ["Employee ID"] = employee.Id,
            ["Department"] = employee.Department,
            ["Status"] = employee.IsActive ? "Active" : "Inactive"
        }
    }
);
```

### Custom Modal Components

```csharp
// Show custom component in modal
var result = await ModalService.ShowAsync<MyData>(
    typeof(MyCustomModal),
    new Dictionary<string, object>
    {
        ["InitialData"] = myData,
        ["Title"] = "Edit Data"
    },
    new ModalOptions
    {
        Size = SizeType.Large,
        CloseOnBackdrop = false
    }
);
```

## Modal Options

### ModalOptions

```csharp
var options = new ModalOptions
{
    Title = "Modal Title",
    Subtitle = "Optional subtitle",
    Icon = "info",
    Size = SizeType.Medium,
    Variant = ModalVariant.Default,
    CloseOnBackdrop = true,
    CloseOnEscape = true,
    ShowCloseButton = true,
    ShowHeader = true,
    ShowFooter = true,
    Class = "custom-modal-class"
};
```

### ConfirmationVariant

- `Info` - Default informational style
- `Success` - Success/positive action style
- `Warning` - Warning/caution style
- `Danger` - Dangerous action style
- `Destructive` - Destructive/irreversible action style

## RConfirmationModal Features

### Basic Features

- **Title and Message** - Customizable header and content
- **Icon Support** - Material Icons with variant-based defaults
- **Button Customization** - Custom text and styling
- **Variant Styling** - Pre-defined color schemes

### Enterprise Features

- **Input Validation** - Require user input for confirmation
- **Checkbox Confirmation** - Additional confirmation checkbox
- **Details Display** - Show structured information
- **Warning Messages** - Highlight destructive actions
- **Custom Templates** - Override any section with custom content

### Input Validation

```csharp
var parameters = new Dictionary<string, object>
{
    ["ShowInputField"] = true,
    ["InputLabel"] = "Type 'DELETE' to confirm",
    ["InputRequired"] = true,
    ["OnInputValidate"] = new Func<string, Task<bool>>(input => 
        Task.FromResult(input == "DELETE"))
};
```

### Details Display

```csharp
var parameters = new Dictionary<string, object>
{
    ["ShowDetails"] = true,
    ["DetailsTitle"] = "Employee Information",
    ["DetailsDictionary"] = new Dictionary<string, string>
    {
        ["Name"] = employee.FullName,
        ["Department"] = employee.Department,
        ["Hire Date"] = employee.HireDate.ToString("yyyy-MM-dd")
    }
};
```

## JavaScript Integration

### Portal Creation

The modal system automatically creates DOM portals:

```javascript
// Automatic portal creation
const portal = window.RRBlazor.Portal.create({
    id: 'modal-12345',
    className: 'modal-portal'
});

// Modal creation
window.RRBlazor.Modal.create(modalElement, {
    useBackdrop: true,
    closeOnEscape: true,
    trapFocus: true
});
```

### Event System

The modal system uses a comprehensive event system:

- `PORTAL_CREATED` - Portal created successfully
- `PORTAL_DESTROYED` - Portal destroyed
- `UI_COMPONENT_OPENED` - Modal opened
- `UI_COMPONENT_CLOSED` - Modal closed

## Browser Support

- **Modern Browsers** - Chrome 88+, Firefox 85+, Safari 14+, Edge 88+
- **Mobile Support** - iOS Safari 14+, Chrome Mobile 88+
- **Accessibility** - WCAG 2.1 AA compliant

## Migration from RModalHost

If migrating from older versions that used RModalHost:

### Before (with RModalHost)

```razor
<!-- MainLayout.razor -->
<RModalHost />
@Body
```

### After (portal-based)

```razor
<!-- MainLayout.razor -->
<ModalProvider />
@Body
```

The ModalProvider component manages the portal system and creates the necessary DOM structure automatically when the first modal is shown.

## Performance

- **Portal Creation** - On-demand portal creation reduces initial DOM size
- **Memory Management** - Automatic cleanup when modals are destroyed
- **Z-Index Management** - Efficient stacking without CSS conflicts
- **Animation Performance** - Hardware-accelerated CSS animations

## Best Practices

1. **Use confirmation modals for destructive actions**
2. **Provide clear, actionable button text**
3. **Use input validation for critical confirmations**
4. **Show relevant details for context**
5. **Handle modal results appropriately**
6. **Don't nest modals deeply**

## Troubleshooting

### Common Issues

**Modal doesn't appear:**
- Check browser console for JavaScript errors
- Ensure RR.Blazor JavaScript files are loaded
- Verify ModalService is properly injected

**Modal doesn't close:**
- Check if custom validation is preventing closure
- Ensure button click handlers are properly bound
- Verify JavaScript event system is functioning

**Z-index issues:**
- The portal system automatically manages z-index
- Avoid manual z-index CSS overrides
- Check for CSS conflicts with other libraries

### Debug Mode

Enable debug logging:

```javascript
window.RRBlazor.Debug = true;
```

This will log detailed information about portal creation, modal lifecycle, and event handling.