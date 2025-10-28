using Microsoft.AspNetCore.Components;
using RR.Blazor.Components;
using RR.Blazor.Enums;

namespace RR.Blazor.Templates.Chip;

public class ChipTemplate<T> where T : class
{
    public Func<T, string> TextSelector { get; set; }
    public Func<T, VariantType> VariantSelector { get; set; }
    public Func<T, string> IconSelector { get; set; }
    public Func<T, ChipStyle> StyleSelector { get; set; }
    public ChipStyle DefaultStyle { get; set; } = ChipStyle.Chip;
    public SizeType Size { get; set; } = SizeType.Small;
    public DensityType Density { get; set; } = DensityType.Compact;
    public bool Closeable { get; set; }
    public EventCallback<T> OnClick { get; set; }
    public EventCallback<T> OnClose { get; set; }
    
    public RenderFragment Render(T item)
    {
        return builder =>
        {
            builder.OpenComponent<RChip>(0);
            builder.AddAttribute(1, nameof(RChip.Text), TextSelector?.Invoke(item) ?? string.Empty);
            
            if (VariantSelector != null)
                builder.AddAttribute(2, nameof(RChip.Variant), VariantSelector(item));
                
            if (IconSelector != null)
                builder.AddAttribute(3, nameof(RChip.Icon), IconSelector(item));
                
            builder.AddAttribute(4, nameof(RChip.StyleVariant), StyleSelector?.Invoke(item) ?? DefaultStyle);
            builder.AddAttribute(5, nameof(RChip.Size), Size);
            builder.AddAttribute(6, nameof(RChip.Density), Density);
            builder.AddAttribute(7, nameof(RChip.Closeable), Closeable);
            
            if (OnClick.HasDelegate)
                builder.AddAttribute(8, nameof(RChip.OnClick), EventCallback.Factory.Create(item, () => OnClick.InvokeAsync(item)));

            if (OnClose.HasDelegate)
                builder.AddAttribute(9, nameof(RChip.OnClose), EventCallback.Factory.Create(item, () => OnClose.InvokeAsync(item)));
                
            builder.CloseComponent();
        };
    }
}
