using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using RR.Blazor.Components.Base;
using RR.Blazor.Enums;

namespace RR.Blazor.Components.Layout
{
    /// <summary>
    /// Professional responsive grid component with RR.Blazor patterns
    /// Category: Layout, Complexity: Simple
    /// AI Prompt: grid, layout, responsive, stats, cards
    /// Common Use: Dashboard grids, stats cards, responsive layouts, card containers
    /// </summary>
    public class RGrid : RForwardingComponentBase
    {
        [Parameter] public RenderFragment ChildContent { get; set; }
        [Parameter] public GridType Type { get; set; } = GridType.Auto;
        [Parameter] public GridVariant Variant { get; set; } = GridVariant.Default;
        [Parameter] public string? Class { get; set; }
        [Parameter] public int? MinColumnWidth { get; set; }
        [Parameter] public string? Gap { get; set; }

        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            int seq = 0;
            builder.OpenElement(seq++, "div");
            builder.AddAttribute(seq++, "class", GetGridClasses());
            
            // Forward all additional parameters except the ones we handle explicitly
            ForwardParametersExcept(builder, ref seq, 
                nameof(ChildContent), nameof(Type), nameof(Variant), nameof(Class), nameof(MinColumnWidth), nameof(Gap));
            
            builder.AddContent(seq++, ChildContent);
            builder.CloseElement();
        }

        private string GetGridClasses()
        {
            var classes = new List<string>();

            switch (Type)
            {
                case GridType.Stats:
                    classes.Add("d-grid");
                    classes.Add("w-full");
                    classes.Add(Variant == GridVariant.Compact ? "grid-auto-fit-200" : "grid-auto-fit-250");
                    classes.Add(Variant == GridVariant.Compact ? "gap-2" : "gap-4");
                    break;
                case GridType.Action:
                    classes.Add("action-grid");
                    break;
                case GridType.Auto:
                default:
                    classes.Add("grid-auto-fit");
                    break;
            }

            if (Type == GridType.Stats && Variant == GridVariant.Spacious)
            {
                classes.RemoveAll(c => c.StartsWith("gap-"));
                classes.Add("gap-6");
            }

            if (!string.IsNullOrEmpty(Class))
                classes.Add(Class);

            return string.Join(" ", classes);
        }
    }
}