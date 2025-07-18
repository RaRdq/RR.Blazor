# RDatePicker Component

## Overview

RDatePicker is a comprehensive date and time selection component for RR.Blazor that provides professional calendar functionality with AI-optimized ease of use. It supports single dates, date ranges, time selection, validation, and extensive customization options.

## Key Features

- **Zero Configuration**: Works out of the box with sensible defaults
- **Date Range Selection**: Built-in support for start/end date ranges
- **Time Selection**: Optional time picker with 12/24 hour formats
- **Professional Calendar**: Month navigation, year picker, keyboard support
- **Validation**: Min/max dates, disabled dates, custom validation
- **Responsive Design**: Mobile-friendly touch interface
- **Accessibility**: Full keyboard navigation and screen reader support
- **Theme Integration**: Automatic light/dark theme support

## Component Variants

### RDatePicker (Full-Featured)
Complete date picker with calendar popup, range selection, and time picker.

### RDatePickerBasic (Simplified)
Lightweight version using native HTML date/datetime inputs for basic scenarios.

## Basic Usage

```html
<!-- Simple date selection -->
<RDatePicker @bind-value="selectedDate" />

<!-- Date range selection -->
<RDatePicker @bind-value="startDate" 
             @bind-EndValue="endDate"
             Range="true" />

<!-- Date and time selection -->
<RDatePicker @bind-value="appointmentTime"
             ShowTime="true" />
```

## Parameters

### Core Parameters

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `Value` | `DateTime?` | `null` | The selected date/time value |
| `ValueChanged` | `EventCallback<DateTime?>` | - | Callback when value changes |
| `EndValue` | `DateTime?` | `null` | End date for range selection |
| `EndValueChanged` | `EventCallback<DateTime?>` | - | Callback when end value changes |

### Display Parameters

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `Label` | `string?` | `null` | Label text above the input |
| `Placeholder` | `string?` | `null` | Placeholder text in input |
| `Format` | `string?` | `"dd/MM/yyyy"` | Date format string |
| `Size` | `FieldSize` | `Medium` | Input field size |
| `StartIcon` | `string?` | `null` | Material icon at start of input |
| `Class` | `string?` | `null` | Additional CSS classes |

### Feature Parameters

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `Range` | `bool` | `false` | Enable date range selection |
| `ShowTime` | `bool` | `false` | Show time picker |
| `Use24HourFormat` | `bool` | `true` | Use 24-hour time format |
| `MinuteInterval` | `int` | `1` | Minute selection interval |
| `ShowClearButton` | `bool` | `true` | Show clear button when value exists |
| `ShowTodayButton` | `bool` | `true` | Show "Today" button in calendar |
| `ShowWeekNumbers` | `bool` | `false` | Display week numbers in calendar |

### Validation Parameters

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `MinDate` | `DateTime?` | `null` | Minimum selectable date |
| `MaxDate` | `DateTime?` | `null` | Maximum selectable date |
| `DisabledDates` | `Func<DateTime, bool>?` | `null` | Function to disable specific dates |
| `DisabledDaysOfWeek` | `DayOfWeek[]?` | `null` | Array of disabled days of week |

### State Parameters

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `Disabled` | `bool` | `false` | Disable the entire component |
| `ReadOnly` | `bool` | `false` | Make input read-only |
| `HasError` | `bool` | `false` | Show error state styling |
| `ErrorMessage` | `string?` | `null` | Error message to display |

### Event Parameters

| Parameter | Type | Description |
|-----------|------|-------------|
| `OnDateSelected` | `EventCallback<DateTime?>` | Called when a date is selected |
| `OnCalendarOpen` | `EventCallback` | Called when calendar opens |
| `OnCalendarClose` | `EventCallback` | Called when calendar closes |

## Advanced Examples

### Business Date Selector
```html
<RDatePicker @bind-value="meetingDate"
             Label="Meeting Date"
             ShowTime="true"
             Use24HourFormat="false"
             MinDate="@DateTime.Today"
             MaxDate="@DateTime.Today.AddMonths(3)"
             DisabledDaysOfWeek="@(new[] { DayOfWeek.Saturday, DayOfWeek.Sunday })"
             MinuteInterval="15"
             Placeholder="Select business day..." />
```

### Date Range with Validation
```html
<RDatePicker @bind-value="projectStart"
             @bind-EndValue="projectEnd"
             Range="true"
             Label="Project Timeline"
             MinDate="@DateTime.Today"
             DisabledDates="@IsHoliday"
             Format="dd MMM yyyy"
             HasError="@(!IsValidRange(projectStart, projectEnd))"
             ErrorMessage="End date must be after start date" />
```

### Custom Time Intervals
```html
<RDatePicker @bind-value="appointmentTime"
             Label="Appointment Time"
             ShowTime="true"
             MinuteInterval="30"
             MinDate="@DateTime.Today.AddHours(9)"
             MaxDate="@DateTime.Today.AddHours(17)"
             Placeholder="9 AM - 5 PM only..." />
```

## JavaScript Integration

The component includes optional JavaScript enhancements for:

- Advanced keyboard navigation
- Smart popup positioning
- Touch gesture support
- Outside click detection

JavaScript features are loaded automatically and fail gracefully if unavailable.

## Styling and Themes

RDatePicker integrates seamlessly with the RR.Blazor theme system:

- Automatic light/dark theme adaptation
- Professional elevation and shadows
- Consistent color scheme
- Mobile-responsive design
- High contrast mode support

### Custom Styling

```html
<!-- Custom CSS classes -->
<RDatePicker @bind-value="date" 
             Class="my-custom-datepicker" />

<!-- Size variants -->
<RDatePicker @bind-value="date" Size="FieldSize.Large" />
```

## Accessibility Features

- Full keyboard navigation (Arrow keys, Home, End, Page Up/Down)
- Screen reader compatible
- ARIA labels and descriptions
- Focus management
- High contrast support
- Touch-friendly targets (44px minimum)

## Keyboard Shortcuts

| Key | Action |
|-----|--------|
| `Enter` / `Space` | Open/close calendar |
| `Escape` | Close calendar |
| `Arrow Keys` | Navigate calendar days |
| `Home` / `End` | Navigate to week start/end |
| `Page Up` / `Page Down` | Navigate months |
| `Tab` | Navigate between elements |

## Browser Support

- Modern browsers with ES6+ support
- Graceful degradation to native HTML inputs
- Mobile Safari, Chrome, Firefox, Edge
- Progressive enhancement approach

## Performance Considerations

- Lazy-loaded JavaScript modules
- Efficient re-rendering with proper key usage
- Memory leak prevention with proper disposal
- Minimal DOM manipulation
- CSS-based animations over JavaScript

## Integration with Forms

RDatePicker works seamlessly with Blazor forms and validation:

```html
<EditForm Model="@model" OnValidSubmit="@HandleSubmit">
    <DataAnnotationsValidator />
    
    <RDatePicker @bind-value="model.EventDate"
                 Label="Event Date"
                 Required="true" />
    
    <ValidationMessage For="@(() => model.EventDate)" />
    
    <button type="submit">Submit</button>
</EditForm>
```

## Migration from RFormField

Existing `RFormField` date inputs can be easily migrated to the modern R* component architecture:

```html
<!-- Old RFormField approach -->
<RFormField Type="FieldType.Date" 
            @bind-value="dateString" 
            Text="Select Date" />

<!-- New RDatePicker approach -->
<RDatePicker @bind-value="dateValue" 
             Label="Select Date" />

<!-- Alternative: RTextInput for simple date inputs -->
<RTextInput Type="date" 
            @bind-value="dateString" 
            Label="Select Date" />
```

## Common Patterns

### Meeting Scheduler
```html
<div class="meeting-scheduler">
    <RDatePicker @bind-value="meetingStart"
                 Label="Meeting Start"
                 ShowTime="true"
                 Use24HourFormat="false"
                 MinDate="@DateTime.Today"
                 DisabledDaysOfWeek="@weekends" />
    
    <RDatePicker @bind-value="meetingEnd"
                 Label="Meeting End"
                 ShowTime="true"
                 Use24HourFormat="false"
                 MinDate="@(meetingStart ?? DateTime.Today)" />
</div>
```

### Travel Booking
```html
<div class="travel-dates">
    <RDatePicker @bind-value="checkIn"
                 @bind-EndValue="checkOut"
                 Range="true"
                 Label="Stay Dates"
                 MinDate="@DateTime.Today"
                 Placeholder="Check-in → Check-out" />
</div>
```

## Best Practices

1. **Use Range mode** for date ranges instead of separate inputs
2. **Set appropriate MinDate/MaxDate** to guide user selection
3. **Provide clear labels** for accessibility
4. **Use validation** to prevent invalid date combinations
5. **Consider time zones** for international applications
6. **Test keyboard navigation** for accessibility compliance
7. **Use MinuteInterval** for appointment scheduling
8. **Disable irrelevant dates** (weekends, holidays) when appropriate

## Troubleshooting

### Common Issues

**Calendar not opening:**
- Check for JavaScript errors in browser console
- Ensure proper CSS is loaded
- Verify component is not disabled

**Date format issues:**
- Use standard format strings (dd/MM/yyyy, yyyy-MM-dd)
- Ensure culture settings are correct
- Check DateTime parsing logic

**Performance issues:**
- Limit date ranges with MinDate/MaxDate
- Use appropriate MinuteInterval values
- Consider RDatePickerBasic for simple scenarios

**Styling problems:**
- Ensure RR.Blazor CSS is properly loaded
- Check for CSS conflicts with other libraries
- Use browser developer tools to inspect styles

## Examples Repository

See `/RR.Blazor/Components/RDatePickerDemo.razor` for comprehensive examples and live demonstrations of all features.