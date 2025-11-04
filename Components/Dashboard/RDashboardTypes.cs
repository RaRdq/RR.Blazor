using System.Collections.Generic;
using System.Linq;

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

    public bool UpdateRegistration(RWidgetRegistration registration)
    {
        if (registration is null)
        {
            throw new ArgumentNullException(nameof(registration));
        }

        if (RWidgetRegistrationComparer.AreEqual(Registration, registration))
        {
            return false;
        }

        Registration = registration;
        ColumnSpan = Math.Clamp(ColumnSpan, registration.MinColumnSpan, registration.MaxColumnSpan);
        RowSpan = Math.Clamp(RowSpan, registration.MinRowSpan, registration.MaxRowSpan);
        return true;
    }

    public RWidgetLayoutSnapshot ToSnapshot() => new(Registration.Id, ColumnSpan, RowSpan, Order);
}

internal static class RWidgetRegistrationComparer
{
    public static bool AreEqual(RWidgetRegistration left, RWidgetRegistration right)
    {
        if (ReferenceEquals(left, right))
        {
            return true;
        }

        if (left is null || right is null)
        {
            return false;
        }

        return left.Id == right.Id
            && left.ColumnSpan == right.ColumnSpan
            && left.RowSpan == right.RowSpan
            && left.MinColumnSpan == right.MinColumnSpan
            && left.MaxColumnSpan == right.MaxColumnSpan
            && left.MinRowSpan == right.MinRowSpan
            && left.MaxRowSpan == right.MaxRowSpan
            && left.AllowResize == right.AllowResize
            && left.AllowReorder == right.AllowReorder
            && DictionaryEquals(left.ResponsiveColumnSpans, right.ResponsiveColumnSpans)
            && DictionaryEquals(left.ResponsiveRowSpans, right.ResponsiveRowSpans);
    }

    private static bool DictionaryEquals(IReadOnlyDictionary<string, int> left, IReadOnlyDictionary<string, int> right)
    {
        if (ReferenceEquals(left, right))
        {
            return true;
        }

        if (left is null || right is null || left.Count != right.Count)
        {
            return false;
        }

        foreach (var kvp in left)
        {
            if (!right.TryGetValue(kvp.Key, out var value) || value != kvp.Value)
            {
                return false;
            }
        }

        return true;
    }
}
