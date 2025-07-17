namespace RR.Blazor.Enums
{
    public enum ChartType
    {
        Line,
        Bar,
        Column,
        Pie,
        Doughnut,
        Area,
        Scatter,
        Mixed
    }

    public enum ChartVariant
    {
        Default,
        Filled,
        Outlined,
        Glass,
        Minimal,
        Elevated
    }

    public enum ChartSize
    {
        Small,
        Medium,
        Large,
        Full
    }

    public enum ChartLegendPosition
    {
        None,
        Top,
        Bottom,
        Left,
        Right,
        TopLeft,
        TopRight,
        BottomLeft,
        BottomRight
    }

    public enum ChartTooltipTrigger
    {
        Hover,
        Click,
        Focus,
        Disabled
    }

    public enum ChartAnimationEasing
    {
        Linear,
        EaseIn,
        EaseOut,
        EaseInOut,
        Bounce,
        Elastic
    }

    public enum ChartResponsiveMode
    {
        Auto,
        Desktop,
        Tablet,
        Mobile
    }

    public enum ChartTheme
    {
        Auto,
        Light,
        Dark,
        System
    }

    public enum ChartDataLabelPosition
    {
        None,
        Inside,
        Outside,
        Center,
        Top,
        Bottom
    }
}