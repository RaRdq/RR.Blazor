namespace RR.Blazor.Components.Dashboard;

public enum RWidgetSizeCategory
{
    Micro = 1,
    Compact,
    Standard,
    Extended,
    Showcase
}

public sealed record RWidgetLayoutSnapshot(string WidgetId, int ColumnSpan, int RowSpan, int Order);

public sealed record RDashboardLayoutChangedEventArgs(IReadOnlyList<RWidgetLayoutSnapshot> Layout);

public sealed record RWidgetViewContext(
    string WidgetId,
    int ColumnSpan,
    int RowSpan,
    RWidgetSizeCategory Size,
    bool IsEditing);

internal sealed record RWidgetRegistration(
    string Id,
    int ColumnSpan,
    int RowSpan,
    int MinColumnSpan,
    int MaxColumnSpan,
    int MinRowSpan,
    int MaxRowSpan,
    bool AllowResize,
    bool AllowReorder,
    IReadOnlyDictionary<string, int> ResponsiveColumnSpans,
    IReadOnlyDictionary<string, int> ResponsiveRowSpans);

internal sealed class RDashboardWidgetState
{
    public RDashboardWidgetState(RWidget widget, RWidgetRegistration registration, int order)
    {
        Widget = widget ?? throw new ArgumentNullException(nameof(widget));
        Registration = registration ?? throw new ArgumentNullException(nameof(registration));
        Order = order;
        ColumnSpan = registration.ColumnSpan;
        RowSpan = registration.RowSpan;
    }

    public RWidget Widget { get; }
    public RWidgetRegistration Registration { get; private set; }
    public int Order { get; set; }
    public int ColumnSpan { get; set; }
    public int RowSpan { get; set; }

    public void UpdateRegistration(RWidgetRegistration registration)
    {
        Registration = registration ?? throw new ArgumentNullException(nameof(registration));
        ColumnSpan = Math.Clamp(ColumnSpan, registration.MinColumnSpan, registration.MaxColumnSpan);
        RowSpan = Math.Clamp(RowSpan, registration.MinRowSpan, registration.MaxRowSpan);
    }

    public RWidgetLayoutSnapshot ToSnapshot() => new(Registration.Id, ColumnSpan, RowSpan, Order);
}
