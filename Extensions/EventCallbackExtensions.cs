using System;
using System.Reflection;
using Microsoft.AspNetCore.Components;

namespace RR.Blazor.Extensions;

internal static class EventCallbackExtensions
{
    public static bool HasDelegate(this object? callback)
    {
        if (callback is null)
        {
            return false;
        }

        switch (callback)
        {
            case EventCallback eventCallback:
                return eventCallback.HasDelegate;
            default:
                var callbackType = callback.GetType();
                if (callbackType.IsGenericType && callbackType.GetGenericTypeDefinition() == typeof(EventCallback<>))
                {
                    var property = callbackType.GetProperty("HasDelegate", BindingFlags.Instance | BindingFlags.Public);
                    if (property != null)
                    {
                        var value = property.GetValue(callback);
                        if (value is bool hasDelegate)
                        {
                            return hasDelegate;
                        }
                    }
                }

                return false;
        }
    }
}
