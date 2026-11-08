using System;
using System.Globalization;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using SkiaSharp;
using StitchShot.Core;

namespace StitchShot.App;

/// <summary>What the user chose in the overlay. Region is in bitmap pixel coordinates.</summary>
public sealed record CaptureResult(ScreenRect Region, bool Save);

/// <summary>
/// Full-screen window showing a frozen copy of the desktop. The user drags a rectangle, then a small
/// toolbar under it offers Save / Copy / Cancel. <see cref="Result"/> completes with null on cancel.
/// </summary>
public sealed class OverlayWindow : Window
{
    readonly OverlayView _view;
    readonly Border _toolbar;
    readonly double _scale;
    readonly bool _stitchMode;
    public Task<CaptureResult?> Result => _view.Result;

    // shortcut: one window spans all monitors, so mixed-DPI setups mis-scale secondary screens; upgrade to one window per monitor.
    public OverlayWindow(SKBitmap frozen, ScreenRect desktop, bool stitchMode = false)
    {
        _stitchMode = stitchMode;
        WindowDecorations = WindowDecorations.None;
        Topmost = true;
        ShowInTaskbar = false;
        CanResize = false;
        Background = Brushes.Black;
        Cursor = new Cursor(StandardCursorType.None);
        WindowStartupLocation = WindowStartupLocation.Manual;
        Position = new PixelPoint(desktop.X, desktop.Y);
        _scale = Screens.ScreenFromPoint(Position)?.Scaling ?? 1;
        Width = desktop.Width / _scale;
        Height = desktop.Height / _scale;

        _view = new OverlayView(frozen);
        _toolbar = BuildToolbar();
        var canvas = new Canvas();
        canvas.Children.Add(_toolbar);
        Content = new Panel { Children = { _view, canvas } };

        _view.SelectionChanged += PlaceToolbar;
        _view.Done += () => Close();
    }

    Border BuildToolbar()
    {
        var row = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 2 };
        if (_stitchMode)
            row.Children.Add(IconButton.Create(IconButton.Layers, "Start stitching with this region (Enter)", () => _view.Confirm(save: true)));
        else
        {
            row.Children.Add(IconButton.Create(IconButton.Save, "Save to file (Enter)", () => _view.Confirm(save: true)));
            row.Children.Add(IconButton.Create(IconButton.Copy, "Copy to clipboard (Ctrl+C)", () => _view.Confirm(save: false)));
        }
        row.Children.Add(IconButton.Create(IconButton.Close, "Cancel (Esc)", _view.CancelAndClose));
        return new Border
        {
            Background = IconButton.PanelBrush,
            CornerRadius = new CornerRadius(4),
            Padding = new Thickness(2),
            Cursor = new Cursor(StandardCursorType.Arrow),
            IsVisible = false,
            Child = row,
        };
    }

    void PlaceToolbar(PixelRect? selection)
    {
        if (selection is not { } s) { _toolbar.IsVisible = false; Cursor = new Cursor(StandardCursorType.None); return; }
        Cursor = new Cursor(StandardCursorType.Arrow);
        _toolbar.IsVisible = true; // an invisible control measures to zero
        _toolbar.Measure(Size.Infinity);
        var size = _toolbar.DesiredSize;
        var r = new Rect(s.X / _scale, s.Y / _scale, s.Width / _scale, s.Height / _scale);
        const double gap = 6;
        double x = Math.Clamp(r.Right - size.Width, 0, Math.Max(0, Width - size.Width));
        double y = r.Bottom + gap;
        if (y + size.Height > Height) y = r.Top - gap - size.Height;
        if (y < 0) y = r.Bottom - gap - size.Height;
        Canvas.SetLeft(_toolbar, x);
        Canvas.SetTop(_toolbar, y);
    }

    protected override void OnOpened(EventArgs e)
    {
        base.OnOpened(e);
        Activate();
        _view.Focus();
    }

    protected override void OnClosed(EventArgs e)
    {
        _view.Cancel();
        base.OnClosed(e);
    }
}

sealed class OverlayView : Control
{
    const int LoupePixels = 15, Zoom = 8;
    const double LoupeSize = LoupePixels * Zoom;

    static readonly IBrush Dim = new SolidColorBrush(Colors.Black, 0.45);
    static readonly IBrush Panel = new SolidColorBrush(Colors.Black, 0.75);
    static readonly Pen Cross = new(new SolidColorBrush(Colors.White, 0.7), 1);
    static readonly Pen Edge = new(Brushes.White, 1);
    static readonly Pen Marker = new(Brushes.Red, 1);
    static readonly Typeface Font = new("Segoe UI");

    readonly SKBitmap _frozen;
    readonly WriteableBitmap _image;
    readonly TaskCompletionSource<CaptureResult?> _result = new();

    PixelPoint _cursor, _dragStart;
    PixelRect? _selection;
    bool _dragging;

    public Task<CaptureResult?> Result => _result.Task;
    public event Action? Done;
    public event Action<PixelRect?>? SelectionChanged;

    public OverlayView(SKBitmap frozen)
    {
        _frozen = frozen;
        _image = ToAvalonia(frozen);
        Focusable = true;
        RenderOptions.SetBitmapInterpolationMode(this, BitmapInterpolationMode.None);
    }

    double Scale => (VisualRoot as TopLevel)?.RenderScaling ?? 1;

    public void Cancel() => _result.TrySetResult(null);

    public void CancelAndClose() => Finish(null);

    public void Confirm(bool save)
    {
        if (_selection is { } s) Finish(new CaptureResult(new ScreenRect(s.X, s.Y, s.Width, s.Height), save));
    }

    void Finish(CaptureResult? r)
    {
        _result.TrySetResult(r);
        Done?.Invoke();
    }

    void SetSelection(PixelRect? r)
    {
        _selection = r;
        SelectionChanged?.Invoke(r);
        InvalidateVisual();
    }

    PixelPoint ToPixel(Point p) => new(
        Math.Clamp((int)(p.X * Scale), 0, _frozen.Width - 1),
        Math.Clamp((int)(p.Y * Scale), 0, _frozen.Height - 1));

    Rect ToDip(PixelRect r) => new(r.X / Scale, r.Y / Scale, r.Width / Scale, r.Height / Scale);

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        var p = ToPixel(e.GetPosition(this));
        if (e.ClickCount == 2 && _selection is { } s && s.Contains(p)) { Confirm(save: true); return; }
        if (!e.GetCurrentPoint(this).Properties.IsLeftButtonPressed) return;
        _dragStart = _cursor = p;
        _dragging = true;
        SetSelection(null);
    }

    protected override void OnPointerMoved(PointerEventArgs e)
    {
        _cursor = ToPixel(e.GetPosition(this));
        if (_dragging) _selection = Normalize(_dragStart, _cursor);
        InvalidateVisual();
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        if (!_dragging) return;
        _dragging = false;
        SetSelection(_selection is { Width: >= 2, Height: >= 2 } ? _selection : null);
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        e.Handled = true;
        int step = e.KeyModifiers.HasFlag(KeyModifiers.Shift) ? 10 : 1;
        switch (e.Key)
        {
            case Key.Escape: Finish(null); break;
            case Key.Enter: Confirm(save: true); break;
            case Key.C when e.KeyModifiers.HasFlag(KeyModifiers.Control): Confirm(save: false); break;
            case Key.Left: Nudge(-step, 0); break;
            case Key.Right: Nudge(step, 0); break;
            case Key.Up: Nudge(0, -step); break;
            case Key.Down: Nudge(0, step); break;
            default: e.Handled = false; break;
        }
    }

    void Nudge(int dx, int dy)
    {
        if (_selection is not { } s) return;
        int x = Math.Clamp(s.X + dx, 0, _frozen.Width - s.Width);
        int y = Math.Clamp(s.Y + dy, 0, _frozen.Height - s.Height);
        SetSelection(new PixelRect(x, y, s.Width, s.Height));
    }

    // Inclusive of both corners so the pixel under the cursor is part of the selection.
    static PixelRect Normalize(PixelPoint a, PixelPoint b) => new(
        Math.Min(a.X, b.X), Math.Min(a.Y, b.Y), Math.Abs(a.X - b.X) + 1, Math.Abs(a.Y - b.Y) + 1);

    public override void Render(DrawingContext ctx)
    {
        var bounds = new Rect(Bounds.Size);
        ctx.DrawImage(_image, bounds);
        ctx.FillRectangle(Dim, bounds);

        if (_selection is { } s)
        {
            var dip = ToDip(s);
            ctx.DrawImage(_image, new Rect(s.X, s.Y, s.Width, s.Height), dip);
            ctx.DrawRectangle(Edge, dip);
            double labelY = dip.Y >= 26 ? dip.Y - 26 : dip.Y + 4;
            DrawLabel(ctx, $"{s.Width} × {s.Height}", new Point(dip.X, labelY), bounds);
            if (!_dragging) return; // selection is made: no crosshair or loupe, the toolbar takes over
        }

        var c = new Point((_cursor.X + 0.5) / Scale, (_cursor.Y + 0.5) / Scale);
        ctx.DrawLine(Cross, new Point(c.X, 0), new Point(c.X, bounds.Height));
        ctx.DrawLine(Cross, new Point(0, c.Y), new Point(bounds.Width, c.Y));

        DrawLoupe(ctx, _cursor, bounds);
        if (_dragging) DrawLoupe(ctx, _dragStart, bounds);
    }

    void DrawLoupe(DrawingContext ctx, PixelPoint px, Rect bounds)
    {
        var anchor = new Point((px.X + 0.5) / Scale, (px.Y + 0.5) / Scale);
        // The hint rides under the cursor's loupe, so on multi-monitor desktops it is always where the user looks.
        bool showHint = px == _cursor && _selection is null && !_dragging;
        const double gap = 20;
        double textHeight = showHint ? 42 : 20;
        double x = anchor.X + gap, y = anchor.Y + gap;
        if (x + LoupeSize > bounds.Width) x = anchor.X - gap - LoupeSize;
        if (y + LoupeSize + textHeight > bounds.Height) y = anchor.Y - gap - LoupeSize - textHeight;
        var loupe = new Rect(x, y, LoupeSize, LoupeSize);

        ctx.FillRectangle(Brushes.Black, loupe);
        var ideal = new PixelRect(px.X - LoupePixels / 2, px.Y - LoupePixels / 2, LoupePixels, LoupePixels);
        var src = ideal.Intersect(new PixelRect(0, 0, _frozen.Width, _frozen.Height));
        var dst = new Rect(x + (src.X - ideal.X) * Zoom, y + (src.Y - ideal.Y) * Zoom, src.Width * Zoom, src.Height * Zoom);
        ctx.DrawImage(_image, new Rect(src.X, src.Y, src.Width, src.Height), dst);

        var mid = loupe.Center;
        ctx.DrawLine(Marker, new Point(mid.X, loupe.Top), new Point(mid.X, loupe.Bottom));
        ctx.DrawLine(Marker, new Point(loupe.Left, mid.Y), new Point(loupe.Right, mid.Y));
        ctx.DrawRectangle(Edge, loupe);

        var color = _frozen.GetPixel(px.X, px.Y);
        DrawLabel(ctx, $"{px.X}, {px.Y}   #{color.Red:X2}{color.Green:X2}{color.Blue:X2}", new Point(x, loupe.Bottom), bounds, LoupeSize);
        if (showHint) DrawLabel(ctx, "Drag to select  ·  Esc cancels", new Point(x, loupe.Bottom + 22), bounds, LoupeSize);
    }

    static void DrawLabel(DrawingContext ctx, string text, Point at, Rect bounds, double minWidth = 0)
    {
        var ft = new FormattedText(text, CultureInfo.InvariantCulture, FlowDirection.LeftToRight, Font, 12, Brushes.White);
        var box = new Rect(at, new Size(Math.Max(minWidth, ft.Width + 10), ft.Height + 6));
        if (box.Bottom > bounds.Height) box = box.WithY(bounds.Height - box.Height);
        if (box.Right > bounds.Width) box = box.WithX(bounds.Width - box.Width);
        if (box.X < 0) box = box.WithX(0);
        ctx.FillRectangle(Panel, box);
        ctx.DrawText(ft, new Point(box.X + 5, box.Y + 3));
    }

    static unsafe WriteableBitmap ToAvalonia(SKBitmap sk)
    {
        var wb = new WriteableBitmap(new PixelSize(sk.Width, sk.Height), new Vector(96, 96), PixelFormat.Bgra8888, AlphaFormat.Opaque);
        using var fb = wb.Lock();
        var src = (byte*)sk.GetPixels();
        var dst = (byte*)fb.Address;
        int row = sk.Width * 4;
        for (int y = 0; y < sk.Height; y++)
            Buffer.MemoryCopy(src + y * sk.RowBytes, dst + y * fb.RowBytes, row, row);
        return wb;
    }
}
