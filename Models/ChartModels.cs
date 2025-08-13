using Microsoft.AspNetCore.Components;
using RR.Blazor.Enums;
using System.Reflection;

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

    public class ChartDataAnalyzer
    {
        public ChartType AnalyzeAndRecommend<T>(IEnumerable<T> data)
        {
            if (!data.Any()) return ChartType.Column;

            var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            var sample = data.Take(100).ToList();

            if (HasDateTimeProperty(properties)) return ChartType.Line;

            var numericProps = properties.Count(IsNumericProperty);
            var categoricalProps = properties.Count(IsCategoricalProperty);

            return (numericProps, categoricalProps) switch
            {
                (1, 1) => ChartType.Column,
                (> 1, 1) => ChartType.Column,
                (1, 0) => ChartType.Pie,
                _ => ChartType.Column
            };
        }

        public List<ChartDataPoint> ConvertToChartData<T>(IEnumerable<T> data)
        {
            if (!data.Any()) return new List<ChartDataPoint>();

            var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            var labelProp = FindLabelProperty(properties);
            var valueProp = FindValueProperty(properties);
            var categoryProp = FindCategoryProperty(properties);

            return data.Select(item => new ChartDataPoint
            {
                Label = labelProp?.GetValue(item)?.ToString() ?? "Unknown",
                Value = GetNumericValue(valueProp?.GetValue(item)),
                Category = categoryProp?.GetValue(item)?.ToString() ?? "",
                Metadata = properties.ToDictionary(p => p.Name, p => p.GetValue(item) ?? "")
            }).ToList();
        }

        private static bool HasDateTimeProperty(PropertyInfo[] properties) =>
            properties.Any(p => p.PropertyType == typeof(DateTime) || p.PropertyType == typeof(DateTime?));

        private static bool IsNumericProperty(PropertyInfo prop) =>
            prop.PropertyType == typeof(int) || prop.PropertyType == typeof(double) || 
            prop.PropertyType == typeof(decimal) || prop.PropertyType == typeof(float) ||
            prop.PropertyType == typeof(int?) || prop.PropertyType == typeof(double?) ||
            prop.PropertyType == typeof(decimal?) || prop.PropertyType == typeof(float?);

        private static bool IsCategoricalProperty(PropertyInfo prop) =>
            prop.PropertyType == typeof(string);

        private static PropertyInfo? FindLabelProperty(PropertyInfo[] properties) =>
            properties.FirstOrDefault(p => 
                p.Name.Equals("Label", StringComparison.OrdinalIgnoreCase) ||
                p.Name.Equals("Name", StringComparison.OrdinalIgnoreCase) ||
                p.PropertyType == typeof(string)) ??
            properties.FirstOrDefault();

        private static PropertyInfo? FindValueProperty(PropertyInfo[] properties) =>
            properties.FirstOrDefault(p =>
                p.Name.Equals("Value", StringComparison.OrdinalIgnoreCase) ||
                p.Name.Equals("Amount", StringComparison.OrdinalIgnoreCase) ||
                p.Name.Equals("Count", StringComparison.OrdinalIgnoreCase) ||
                IsNumericProperty(p)) ??
            properties.FirstOrDefault(IsNumericProperty);

        private static PropertyInfo? FindCategoryProperty(PropertyInfo[] properties) =>
            properties.FirstOrDefault(p =>
                p.Name.Equals("Category", StringComparison.OrdinalIgnoreCase) ||
                p.Name.Equals("Group", StringComparison.OrdinalIgnoreCase));

        private static double GetNumericValue(object? value) =>
            value switch
            {
                null => 0,
                int i => i,
                double d => d,
                decimal dec => (double)dec,
                float f => f,
                _ => double.TryParse(value.ToString(), out var result) ? result : 0
            };
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