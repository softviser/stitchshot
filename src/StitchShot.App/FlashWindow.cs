using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Threading;
using StitchShot.Core;

namespace StitchShot.App;

/// <summary>Brief white flash over the captured region, so silent captures (hotkeys, stitch frames) are felt.</summary>
public sealed class FlashWindow : Window
{
    public static void Show(ScreenRect region)
    {
        ToastWindow.Anchor = new PixelPoint(region.X + region.Width / 2, region.Y + region.Height / 2);
        new FlashWindow(region).Show();
    }

    FlashWindow(ScreenRect region)
    {
        WindowDecorations = WindowDecorations.None;
        Topmost = true;
        ShowInTaskbar = false;
        ShowActivated = false;
        CanResize = false;
        Background = Brushes.Transparent;
        TransparencyLevelHint = new[] { WindowTransparencyLevel.Transparent };
        WindowStartupLocation = WindowStartupLocation.Manual;
        Position = new PixelPoint(region.X, region.Y);
        var scale = Screens.ScreenFromPoint(Position)?.Scaling ?? 1;
        Width = region.Width / scale;
        Height = region.Height / scale;

        var fill = new Border { Background = Brushes.White, Opacity = 0.8 };
        Content = fill;
        var timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(16) };
        timer.Tick += (_, _) =>
        {
            fill.Opacity -= 0.032; // ~400 ms fade
            if (fill.Opacity > 0) return;
            timer.Stop();
            Close();
        };
        timer.Start();
    }
}
