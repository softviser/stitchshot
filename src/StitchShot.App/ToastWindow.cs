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
/// Bottom-right by default; drag it anywhere and it remembers. Stays while the pointer is over it.
/// One at a time; a new one replaces the previous.
/// </summary>
public sealed class ToastWindow : Window
{
    /// <summary>Set once at startup so toasts can remember where the user dragged them.</summary>
    public static Settings? Settings;
    static ToastWindow? _current;

    readonly DispatcherTimer _timer;
    bool _moved;

    public static void Show(string title, string detail, SKBitmap? preview = null, string? path = null, Action? edit = null)
    {
        _current?.Close();
        _current = new ToastWindow(title, detail, preview, path, edit);
        _current.Closed += (s, _) => { if (ReferenceEquals(_current, s)) _current = null; };
        _current.Show();
    }

    ToastWindow(string title, string detail, SKBitmap? preview, string? path, Action? edit)
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
                Process.Start(new ProcessStartInfo("explorer.exe", $"/select,\"{path}\"") { UseShellExecute = true });
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

        _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(path is null ? 4 : 6) };
        _timer.Tick += (_, _) => { _timer.Stop(); Close(); };
        _timer.Start();

        if (Settings is { ToastX: { } x, ToastY: { } y })
        {
            WindowStartupLocation = WindowStartupLocation.Manual;
            Position = new PixelPoint(x, y);
        }
    }

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
        if (Settings?.ToastX is not null || Screens.Primary is not { } screen) return;
        var area = screen.WorkingArea;
        Position = new PixelPoint(area.Right - (int)(Bounds.Width * RenderScaling) - 16, area.Bottom - (int)(Bounds.Height * RenderScaling) - 16);
    }

    protected override void OnClosing(WindowClosingEventArgs e)
    {
        if (_moved && Settings is { } s)
        {
            (s.ToastX, s.ToastY) = (Position.X, Position.Y);
            try { s.Save(); } catch (Exception ex) { Log.Write(ex.ToString()); }
        }
        base.OnClosing(e);
    }
}
