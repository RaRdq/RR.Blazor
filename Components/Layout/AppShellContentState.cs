using System;
using System.Collections.Generic;

namespace RR.Blazor.Components.Layout;

internal sealed class AppShellContentState
{
    private readonly Stack<AppShellContentMode> modeStack = new();

    internal AppShellContentState()
    {
        modeStack.Push(AppShellContentMode.Standard);
    }

    internal event Action StateChanged = delegate { };

    internal AppShellContentMode CurrentMode => modeStack.Peek();

    internal void Push(AppShellContentMode mode)
    {
        modeStack.Push(mode);
        NotifyChanged();
    }

    internal void Pop()
    {
        if (modeStack.Count <= 1)
        {
            return;
        }

        modeStack.Pop();
        NotifyChanged();
    }

    internal void Reset()
    {
        if (modeStack.Count == 1 && modeStack.Peek() == AppShellContentMode.Standard)
        {
            return;
        }

        modeStack.Clear();
        modeStack.Push(AppShellContentMode.Standard);
        NotifyChanged();
    }

    private void NotifyChanged() => StateChanged();
}
