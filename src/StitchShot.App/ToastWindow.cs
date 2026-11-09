using System;
using System.Diagnostics;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Threading;
using SkiaSharp;
using StitchShot.Core;

namespace StitchShot.App;

/// <summary>
/// Small notification: what happened, a thumbnail, and a folder icon that reveals the file.
/// In the chosen corner of the monitor the last capture was on; drag it anywhere and it remembers. Stays while the pointer is over it.
/// One at a time; a new one replaces the previous.
/// </summary>
public sealed class ToastWindow : Window
{
    /// <summary>Set once at startup so toasts can remember where the user dragged them.</summary>
    public static Settings? Settings;
    /// <summary>A point on the monitor the next toast should appear on; FlashWindow sets it to the captured region.</summary>
    public static PixelPoint? Anchor;
    static ToastWindow? _current;

    readonly DispatcherTimer _timer;
    bool _moved;

    public static void Show(string title, string detail, SKBitmap? preview = null, string? path = null, Action? edit = null, int? seconds = null)
    {
        _current?.Close();
        _current = new ToastWindow(title, detail, preview, path, edit, seconds);
        _current.Closed += (s, _) => { if (ReferenceEquals(_current, s)) _current = null; };
        _current.Show();
    }

    ToastWindow(string title, string detail, SKBitmap? preview, string? path, Action? edit, int? seconds)
    {
        WindowDecorations = WindowDecorations.None;
        Topmost = true;
        ShowInTaskbar = false;
        ShowActivated = false; // never steal focus from what the user is doing
        CanResize = false;
        SizeToContent = SizeToContent.WidthAndHeight;
        Background = Brushes.Transparent;
        TransparencyLevelHint = new[] { WindowTransparencyLevel.Transparent };

        var row = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 10 };
        if (preview is not null) row.Children.Add(Thumbnail(preview));
        var text = new StackPanel { Spacing = 2, VerticalAlignment = VerticalAlignment.Center, MaxWidth = 260 };
        text.Children.Add(new TextBlock { Text = title, Foreground = Brushes.White, FontWeight = FontWeight.SemiBold, TextWrapping = TextWrapping.Wrap });
        text.Children.Add(new TextBlock { Text = detail, Foreground = Brushes.White, Opacity = 0.75, FontSize = 12, TextWrapping = TextWrapping.Wrap });
        row.Children.Add(text);
        if (path is not null)
            row.Children.Add(IconButton.Create(IconButton.Folder, "Show in folder", () =>
            {
                Shell.Reveal(path);
                Close();
            }));
        if (edit is not null) row.Children.Add(IconButton.Create(IconButton.Pencil, "Annotate in the editor", () => { edit(); Close(); }));
        row.Children.Add(IconButton.Create(IconButton.Close, "Dismiss", Close));

        var panel = new Border
        {
            Background = IconButton.PanelBrush,
            CornerRadius = new CornerRadius(6),
            Padding = new Thickness(10, 8),
            Child = row,
            Cursor = new Cursor(StandardCursorType.SizeAll),
        };
        panel.PointerPressed += (_, e) =>
        {
            if (!e.GetCurrentPoint(panel).Properties.IsLeftButtonPressed) return;
            _moved = true;
            BeginMoveDrag(e);
        };
        panel.PointerEntered += (_, _) => _timer!.Stop();
        panel.PointerExited += (_, _) => _timer!.Start();
        Content = panel;

        _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(seconds ?? (path is null ? 4 : 6)) };
        _timer.Tick += (_, _) => { _timer.Stop(); Close(); };
        _timer.Start();

        if (Remembered is { } p)
        {
            WindowStartupLocation = WindowStartupLocation.Manual;
            Position = p;
        }
    }

    /// <summary>Where the user dragged the last toast, if that is the chosen position and the spot is still on a screen (monitor unplugged).</summary>
    PixelPoint? Remembered =>
        Settings is { ToastCorner: "Custom", ToastX: { } x, ToastY: { } y } && Screens.ScreenFromPoint(new PixelPoint(x, y)) is not null ? new PixelPoint(x, y) : null;

    static Image Thumbnail(SKBitmap bitmap)
    {
        const int maxW = 120, maxH = 72;
        double k = Math.Min((double)maxW / bitmap.Width, (double)maxH / bitmap.Height);
        if (k > 1) k = 1;
        var size = new SKSizeI(Math.Max(1, (int)(bitmap.Width * k)), Math.Max(1, (int)(bitmap.Height * k)));
        using var small = bitmap.Resize(size, new SKSamplingOptions(SKFilterMode.Linear, SKMipmapMode.Linear)) ?? bitmap.Copy();
        return new Image { Source = OverlayView.ToAvalonia(small), Width = size.Width, Height = size.Height, VerticalAlignment = VerticalAlignment.Center };
    }

    protected override void OnOpened(EventArgs e)
    {
        base.OnOpened(e);
        if (Remembered is not null) return;
        var screen = (Anchor is { } a ? Screens.ScreenFromPoint(a) : null) ?? Screens.Primary;
        if (screen is null) return;
        var area = screen.WorkingArea;
        var corner = Settings?.ToastCorner ?? "TopRight";
        int w = (int)(Bounds.Width * RenderScaling), hgt = (int)(Bounds.Height * RenderScaling);
        Position = new PixelPoint(
            corner.EndsWith("Left") ? area.X + 16 : area.Right - w - 16,
            corner.StartsWith("Bottom") ? area.Bottom - hgt - 16 : area.Y + 16);
    }

    protected override void OnClosing(WindowClosingEventArgs e)
    {
        if (_moved && Settings is { } s)
        {
            (s.ToastX, s.ToastY, s.ToastCorner) = (Position.X, Position.Y, "Custom");
            try { s.Save(); } catch (Exception ex) { Log.Write(ex.ToString()); }
        }
        base.OnClosing(e);
    }
}
