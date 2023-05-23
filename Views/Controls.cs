using System;
using System.Threading;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.LogicalTree;

namespace Chief_Reloaded.Views;

public static class Controls
{
    private static DateTime _lastAnimationTime = DateTime.MinValue;

    public static void PageSwitch(UserControl from, UserControl to)
    {
        if (!CanAnimate()) return;

        foreach (var control in from.GetLogicalDescendants())
            if (control is Control c)
                c.IsEnabled = false;

        var window = (Application.Current!.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)!
            .MainWindow!;
        var contentControl = window.Find<TransitioningContentControl>("ContentControl")!;
        contentControl.Content = to;
    }

    public static void SwitchControlVisibility(Visual from, Visual to)
    {
        if (!CanAnimate()) return;
        foreach (var control in from.GetLogicalDescendants())
            if (control is Control c)
                c.IsEnabled = false;

        var transition = new CrossFade(TimeSpan.FromMilliseconds(500));
        transition.Start(from, to, new CancellationToken());

        foreach (var control in to.GetLogicalDescendants())
            if (control is Control c)
                c.IsEnabled = true;
    }

    private static bool CanAnimate()
    {
        var now = DateTime.Now;
        var canAnimate = now - _lastAnimationTime > TimeSpan.FromMilliseconds(500);
        if (canAnimate) _lastAnimationTime = now;

        return canAnimate;
    }

    public static MainWindow? GetMainWindow()
    {
        return (Application.Current!.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)!
            .MainWindow as MainWindow;
    }
}