# Actions Template

The ActionsTemplate provides a flexible way to render action buttons in table columns with support for different display styles, permissions, and conditional visibility.

## Features

- **Multiple Display Styles**: Inline, Dropdown, or Mixed (primary actions inline with overflow in dropdown)
- **Icon-based Actions**: Material Symbols Rounded icons with optional text
- **Conditional Visibility**: Show/hide actions based on item properties
- **Permission Support**: Built-in permission checking (requires implementation)
- **Confirmation Dialogs**: Built-in support for destructive actions
- **Customizable Styling**: Configurable variants, sizes, and alignment

## Basic Usage

```csharp
// In your column configuration
var actionsColumn = new ColumnDefinition<MyEntity>
{
    Key = "actions",
    Title = "Actions",
    Template = ColumnTemplate.Actions,
    Sortable = false,
    Width = "120px"
}.WithActions(actions =>
{
    actions
        .AddView("View Details", "visibility", EventCallback.Factory.Create<MyEntity>(this, OnView))
        .AddEdit("Edit Item", "edit", EventCallback.Factory.Create<MyEntity>(this, OnEdit))
        .AddDelete("Delete", "delete", "Are you sure?", EventCallback.Factory.Create<MyEntity>(this, OnDelete));
});
```

## Advanced Configuration

```csharp
// Custom actions with conditions
var actionsColumn = new ColumnDefinition<UserModel>
{
    Key = "actions",
    Title = "Actions"
}.WithActions(actions =>
{
    actions
        .DisplayStyle(ActionsDisplayStyle.Mixed)
        .MaxVisible(2)
        .Alignment(ActionsAlignment.End)
        .Size(SizeType.Small)
        .AddView(onClick: EventCallback.Factory.Create<UserModel>(this, ViewUser))
        .AddEdit(onClick: EventCallback.Factory.Create<UserModel>(this, EditUser))
        .AddConditional("activate", "Activate", "check_circle", 
            condition: user => !user.IsActive,
            onClick: EventCallback.Factory.Create<UserModel>(this, ActivateUser))
        .AddConditional("deactivate", "Deactivate", "cancel",
            condition: user => user.IsActive,
            onClick: EventCallback.Factory.Create<UserModel>(this, DeactivateUser),
            variant: VariantType.Warning,
            requiresConfirmation: true)
        .AddCustom(action =>
        {
            action.Id = "export";
            action.Text = "Export Data";
            action.Icon = "download";
            action.Variant = VariantType.Info;
            action.OnClick = EventCallback.Factory.Create<UserModel>(this, ExportUserData);
            action.IsVisible = user => user.HasExportPermission;
        });
});
```

## Display Styles

### Inline
All actions are displayed as individual buttons in a row:
```csharp
.DisplayStyle(ActionsDisplayStyle.Inline)
```

### Dropdown
All actions are contained within a dropdown menu:
```csharp
.DisplayStyle(ActionsDisplayStyle.Dropdown)
```

### Mixed
Primary actions are shown inline, with overflow actions in a dropdown:
```csharp
.DisplayStyle(ActionsDisplayStyle.Mixed)
.MaxVisible(2) // First 2 actions inline, rest in dropdown
```

## Built-in Action Types

### View Action
```csharp
.AddView("View Details", "visibility", EventCallback.Factory.Create<T>(this, OnView))
```

### Edit Action
```csharp
.AddEdit("Edit", "edit", EventCallback.Factory.Create<T>(this, OnEdit))
```

### Delete Action (with confirmation)
```csharp
.AddDelete("Delete", "delete", "Are you sure you want to delete this item?", 
    EventCallback.Factory.Create<T>(this, OnDelete))
```

### Conditional Action
```csharp
.AddConditional("approve", "Approve", "check", 
    condition: item => item.Status == "Pending",
    onClick: EventCallback.Factory.Create<T>(this, OnApprove),
    variant: VariantType.Success)
```

## Styling Options

### Button Variants
- Primary, Secondary, Success, Warning, Danger, Info

### Button Styles
- Filled, Outlined, Ghost, Text

### Sizes
- ExtraSmall, Small, Medium, Large, ExtraLarge

### Alignment
- Start, Center, End

## Event Handlers

Action event handlers receive the row item as a parameter:

```csharp
private async Task OnEdit(UserModel user)
{
    // Handle edit action
    await ShowEditModal(user);
}

private async Task OnDelete(UserModel user)
{
    // Handle delete action
    await DeleteUser(user.Id);
}
```