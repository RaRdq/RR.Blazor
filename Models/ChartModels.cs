using Microsoft.AspNetCore.Components;
using RR.Blazor.Enums;

namespace RR.Blazor.Models
{
    public class ChartDataPoint
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Label { get; set; } = "";
        public double Value { get; set; }
        public string Color { get; set; } = "";
        public string Category { get; set; } = "";
        public string Series { get; set; } = "";
        public string Description { get; set; } = "";
        public Dictionary<string, object> Metadata { get; set; } = new();
        public string AccessibilityText { get; set; } = "";
        public bool IsVisible { get; set; } = true;
        public EventCallback<ChartDataPoint> OnClick { get; set; }
    }

    public class ChartSeries
    {
        public string Name { get; set; } = "";
        public List<ChartDataPoint> Data { get; set; } = new();
        public string Color { get; set; } = "";
        public ChartType Type { get; set; } = ChartType.Line;
        public bool IsVisible { get; set; } = true;
        public Dictionary<string, object> Metadata { get; set; } = new();
    }

    public class ChartConfiguration
    {
        public string Title { get; set; } = "";
        public string Subtitle { get; set; } = "";
        public string Description { get; set; } = "";
        public ChartTheme Theme { get; set; } = ChartTheme.Auto;
        public ChartResponsiveMode ResponsiveMode { get; set; } = ChartResponsiveMode.Auto;
        public bool ShowLegend { get; set; } = true;
        public ChartLegendPosition LegendPosition { get; set; } = ChartLegendPosition.Bottom;
        public bool ShowTooltip { get; set; } = true;
        public ChartTooltipTrigger TooltipTrigger { get; set; } = ChartTooltipTrigger.Hover;
        public bool EnableAnimation { get; set; } = true;
        public int AnimationDuration { get; set; } = 300;
        public ChartAnimationEasing AnimationEasing { get; set; } = ChartAnimationEasing.EaseOut;
        public bool EnableInteraction { get; set; } = true;
        public bool EnableAccessibility { get; set; } = true;
        public bool GenerateDataTable { get; set; } = true;
        public Dictionary<string, object> CustomSettings { get; set; } = new();
    }

    public class ChartTooltipContent
    {
        public string Title { get; set; } = "";
        public string Label { get; set; } = "";
        public string Value { get; set; } = "";
        public string Percentage { get; set; } = "";
        public string Color { get; set; } = "";
        public string CustomContent { get; set; } = "";
    }

    public class ChartAxisConfiguration
    {
        public string Title { get; set; } = "";
        public bool ShowGrid { get; set; } = true;
        public bool ShowLabels { get; set; } = true;
        public bool ShowTitle { get; set; } = true;
        public double? Min { get; set; }
        public double? Max { get; set; }
        public double? Step { get; set; }
        public string Format { get; set; } = "";
        public int LabelRotation { get; set; } = 0;
    }

    public class ChartEventArgs
    {
        public ChartDataPoint DataPoint { get; set; } = new();
        public int Index { get; set; }
        public string EventType { get; set; } = "";
        public Dictionary<string, object> AdditionalData { get; set; } = new();
    }
}