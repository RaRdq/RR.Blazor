using Microsoft.AspNetCore.Components;
using RR.Blazor.Components;
using RR.Blazor.Enums;

namespace RR.Blazor.Templates.Chip;

public static class ChipRenderer<T> where T : class
{
    public static RenderFragment<T> Create(
        Func<T, string> textSelector,
        Func<T, VariantType> variantSelector = null,
        Func<T, string> iconSelector = null,
        ChipStyle style = ChipStyle.Chip,
        bool closeable = false,
        EventCallback<T> onClick = default,
        EventCallback<T> onClose = default)
    {
        return item => builder =>
        {
            builder.OpenComponent<RChip>(0);
            builder.AddAttribute(1, nameof(RChip.Text), textSelector?.Invoke(item) ?? string.Empty);
            
            if (variantSelector != null)
                builder.AddAttribute(2, nameof(RChip.Variant), variantSelector(item));
                
            if (iconSelector != null)
                builder.AddAttribute(3, nameof(RChip.Icon), iconSelector(item));
                
            builder.AddAttribute(4, nameof(RChip.StyleVariant), style);
            builder.AddAttribute(5, nameof(RChip.Closeable), closeable);

            if (onClick.HasDelegate)
                builder.AddAttribute(6, nameof(RChip.OnClick), EventCallback.Factory.Create(item, () => onClick.InvokeAsync(item)));

            if (onClose.HasDelegate)
                builder.AddAttribute(7, nameof(RChip.OnClose), EventCallback.Factory.Create(item, () => onClose.InvokeAsync(item)));
                
            builder.CloseComponent();
        };
    }
}