using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using SkiaSharp;
using StitchShot.Core;

namespace StitchShot.App;

public enum EditorTool { Rect, Ellipse, Line, Dashed, Arrow, Pen, Text, Pixelate, Crop }

/// <summary>Annotation editor: tools on top, the image below at 1 image pixel per DIP, status at the bottom.</summary>
public sealed class EditorWindow : Window
{
    // Tool glyphs are drawn here as simple outlines (outer path clockwise, inner counter-clockwise) so they fill under either fill rule.
    static readonly (string Name, EditorTool Tool, Key Key, string Icon)[] Tools =
    {
        ("Rectangle", EditorTool.Rect, Key.R, "M3,5H21V19H3Z M5,7V17H19V7Z"),
        ("Ellipse", EditorTool.Ellipse, Key.O, "M12,4A8,8 0 1,1 12,20A8,8 0 1,1 12,4Z M12,6A6,6 0 1,0 12,18A6,6 0 1,0 12,6Z"),
        ("Line", EditorTool.Line, Key.L, "M4,18.6L18.6,4L20,5.4L5.4,20Z"),
        ("Dashed line", EditorTool.Dashed, Key.D, "M4,18.6L6.6,16L8,17.4L5.4,20Z M10,12.6L12.6,10L14,11.4L11.4,14Z M16,6.6L18.6,4L20,5.4L17.4,8Z"),
        ("Arrow", EditorTool.Arrow, Key.A, "M5,17.59L15.59,7H9V5H19V15H17V8.41L6.41,19L5,17.59Z"),
        ("Pen", EditorTool.Pen, Key.P, IconButton.Pencil),
        ("Text", EditorTool.Text, Key.T, "M5,4V7H10.5V19H13.5V7H19V4H5Z"),
        ("Pixelate", EditorTool.Pixelate, Key.X, "M3,3H9V9H3Z M15,3H21V9H15Z M9,9H15V15H9Z M3,15H9V21H3Z M15,15H21V21H15Z"),
        ("Crop", EditorTool.Crop, Key.C, "M7,17V1H5V5H1V7H5V17A2,2 0 0,0 7,19H17V23H19V19H23V17M17,15H19V7C19,5.89 18.1,5 17,5H9V7H17V15Z"),
    };
    static readonly (string Name, float Width, string Icon)[] Strokes =
    {
        ("Thin (2 px)", 2, "M12,10A2,2 0 1,1 12,14A2,2 0 1,1 12,10Z"),
        ("Medium (4 px)", 4, "M12,8A4,4 0 1,1 12,16A4,4 0 1,1 12,8Z"),
        ("Thick (8 px)", 8, "M12,5.5A6.5,6.5 0 1,1 12,18.5A6.5,6.5 0 1,1 12,5.5Z"),
    };
    static readonly SKColor[] Palette = { SKColors.Red, SKColors.Orange, SKColors.Gold, SKColors.LimeGreen, SKColors.DodgerBlue, SKColors.Magenta, SKColors.Black, SKColors.White };
    static readonly IBrush Selected = new SolidColorBrush(Color.FromArgb(0x50, 0xFF, 0xFF, 0xFF));

    readonly EditorCanvas _canvas;
    readonly Canvas _textLayer = new();
    readonly List<Button> _toolButtons = new(), _strokeButtons = new();
    readonly List<Border> _swatches = new();
    readonly TextBlock _status = new() { Margin = new Thickness(8, 4), FontSize = 12, Opacity = 0.7 };
    readonly Action<SKBitmap> _save, _copy;

    public EditorWindow(SKBitmap image, Action<SKBitmap> save, Action<SKBitmap> copy)
    {
        _save = save;
        _copy = copy;
        Title = $"StitchShot editor · {image.Width} × {image.Height}";
        Icon = TrayIconImage.Create();
        _canvas = new EditorCanvas(image);
        _canvas.TextRequested += BeginText;
        _canvas.Changed += UpdateStatus;

        var row = new WrapPanel { VerticalAlignment = VerticalAlignment.Center };
        foreach (var (name, tool, key, icon) in Tools)
        {
            var b = IconButton.Create(icon, $"{name} ({key})", () => SelectTool(tool));
            _toolButtons.Add(b);
            row.Children.Add(b);
        }
        row.Children.Add(Gap());
        foreach (var color in Palette)
        {
            var swatch = new Border
            {
                Width = 18, Height = 18, Margin = new Thickness(2, 0), CornerRadius = new CornerRadius(9), VerticalAlignment = VerticalAlignment.Center,
                Background = new SolidColorBrush(Color.FromArgb(color.Alpha, color.Red, color.Green, color.Blue)),
                BorderBrush = Brushes.White, Cursor = new Cursor(StandardCursorType.Hand), Tag = color,
            };
            ToolTip.SetTip(swatch, "Colour for the next mark");
            swatch.PointerPressed += (_, _) => SelectColor(color);
            _swatches.Add(swatch);
            row.Children.Add(swatch);
        }
        row.Children.Add(Gap());
        foreach (var (name, width, icon) in Strokes)
        {
            var b = IconButton.Create(icon, $"{name}; text is six times this", () => SelectStroke(width));
            _strokeButtons.Add(b);
            row.Children.Add(b);
        }
        row.Children.Add(Gap());
        row.Children.Add(IconButton.Create(IconButton.Undo, "Undo (Ctrl+Z)", _canvas.Undo));
        row.Children.Add(IconButton.Create(IconButton.Redo, "Redo (Ctrl+Y)", _canvas.Redo));
        row.Children.Add(Gap());
        row.Children.Add(IconButton.Create(IconButton.Save, "Save to file and copy to clipboard (Ctrl+S)", Save));
        row.Children.Add(IconButton.Create(IconButton.Copy, "Copy to clipboard only (Ctrl+C)", () => Copy()));
        var toolbar = new Border { Background = IconButton.PanelBrush, Padding = new Thickness(6, 4), Child = row };
        SelectTool(_canvas.Tool);
        SelectColor(_canvas.Color);
        SelectStroke(_canvas.Stroke);

        var scroller = new ScrollViewer
        {
            HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
            Background = new SolidColorBrush(Color.FromRgb(0x50, 0x50, 0x50)),
            Padding = new Thickness(12),
            Content = new Panel { Children = { _canvas, _textLayer }, HorizontalAlignment = HorizontalAlignment.Left, VerticalAlignment = VerticalAlignment.Top },
        };

        var dock = new DockPanel();
        DockPanel.SetDock(toolbar, Dock.Top);
        DockPanel.SetDock(_status, Dock.Bottom);
        dock.Children.Add(toolbar);
        dock.Children.Add(_status);
        dock.Children.Add(scroller);
        Content = dock;

        var area = Screens.Primary?.WorkingArea ?? new PixelRect(0, 0, 1600, 900);
        var scale = Screens.Primary?.Scaling ?? 1;
        Width = Math.Min(image.Width + 60, area.Width / scale * 0.9);
        Height = Math.Min(image.Height + 140, area.Height / scale * 0.9);
        MinWidth = 720;
        MinHeight = 360;
        WindowStartupLocation = WindowStartupLocation.CenterScreen;
        UpdateStatus();
    }

    static Control Gap() => new Border { Width = 1, Height = 18, Margin = new Thickness(6, 0), Background = Brushes.White, Opacity = 0.3, VerticalAlignment = VerticalAlignment.Center };

    void SelectTool(EditorTool tool)
    {
        _canvas.Tool = tool;
        for (int i = 0; i < Tools.Length; i++) _toolButtons[i].Background = Tools[i].Tool == tool ? Selected : Brushes.Transparent;
        UpdateStatus();
    }

    void SelectColor(SKColor color)
    {
        _canvas.Color = color;
        foreach (var s in _swatches) s.BorderThickness = new Thickness((SKColor)s.Tag! == color ? 2.5 : 0);
    }

    void SelectStroke(float width)
    {
        _canvas.Stroke = width;
        for (int i = 0; i < Strokes.Length; i++) _strokeButtons[i].Background = Strokes[i].Width == width ? Selected : Brushes.Transparent;
    }

    void UpdateStatus()
    {
        var size = _canvas.Crop is { } c ? $"{c.Width} × {c.Height} (cropped)" : $"{_canvas.Width:0} × {_canvas.Height:0}";
        var hint = _canvas.Tool switch
        {
            EditorTool.Text => "click where the text should start, type, Enter to place (Shift+Enter for a new line)",
            EditorTool.Crop => "drag the area to keep; Undo restores the full image",
            EditorTool.Pen => "drag to draw",
            _ => "drag to draw; Shift keeps squares, circles and straight angles",
        };
        _status.Text = $"{size}  ·  {_canvas.Count} annotation{(_canvas.Count == 1 ? "" : "s")}  ·  {hint}";
    }

    void BeginText(Point at)
    {
        var box = new TextBox
        {
            AcceptsReturn = true,
            FontSize = _canvas.Stroke * 6,
            MinWidth = 120,
            Foreground = new SolidColorBrush(Color.FromArgb(_canvas.Color.Alpha, _canvas.Color.Red, _canvas.Color.Green, _canvas.Color.Blue)),
            Background = new SolidColorBrush(Colors.White, 0.6),
            Padding = new Thickness(2),
            BorderThickness = new Thickness(1),
        };
        Canvas.SetLeft(box, at.X - 3);
        Canvas.SetTop(box, at.Y - 3);
        box.KeyDown += (_, e) =>
        {
            if (e.Key == Key.Escape) { _textLayer.Children.Remove(box); _canvas.Focus(); e.Handled = true; }
            else if (e.Key == Key.Enter && !e.KeyModifiers.HasFlag(KeyModifiers.Shift))
            {
                if (!string.IsNullOrWhiteSpace(box.Text)) _canvas.AddText(at, box.Text);
                _textLayer.Children.Remove(box);
                _canvas.Focus();
                e.Handled = true;
            }
        };
        box.LostFocus += (_, _) => { if (_textLayer.Children.Contains(box) && string.IsNullOrWhiteSpace(box.Text)) _textLayer.Children.Remove(box); };
        _textLayer.Children.Add(box);
        box.Focus();
    }

    void Save() { using var img = _canvas.Export(); _save(img); Close(); }
    void Copy() { using var img = _canvas.Export(); _copy(img); }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        if (e.Handled || e.Source is TextBox) return;
        bool ctrl = e.KeyModifiers.HasFlag(KeyModifiers.Control);
        if (ctrl && e.Key == Key.Z) _canvas.Undo();
        else if (ctrl && e.Key == Key.Y) _canvas.Redo();
        else if (ctrl && e.Key == Key.S) Save();
        else if (ctrl && e.Key == Key.C) Copy();
        else if (e.Key == Key.Escape) Close();
        else if (!ctrl && Array.FindIndex(Tools, t => t.Key == e.Key) is >= 0 and var i) SelectTool(Tools[i].Tool);
        else return;
        e.Handled = true;
    }
}

/// <summary>Draws the image plus annotations through one Skia renderer straight into an Avalonia bitmap, so preview and export match.</summary>
sealed class EditorCanvas : Control
{
    readonly SKBitmap _source;
    readonly WriteableBitmap _surface;
    readonly List<Annotation> _items = new();
    readonly Stack<(Annotation[] Items, SKRectI? Crop)> _undo = new(), _redo = new();
    Annotation? _draft;
    SKPoint _start;
    bool _shift;

    public EditorTool Tool { get; set; } = EditorTool.Rect;
    public SKColor Color { get; set; } = SKColors.Red;
    public float Stroke { get; set; } = 4;
    public SKRectI? Crop { get; private set; }
    public int Count => _items.Count;
    public event Action<Point>? TextRequested;
    public event Action? Changed;

    public EditorCanvas(SKBitmap source)
    {
        _source = source;
        _surface = new WriteableBitmap(new PixelSize(source.Width, source.Height), new Vector(96, 96), PixelFormat.Bgra8888, AlphaFormat.Premul);
        Width = source.Width;
        Height = source.Height;
        Focusable = true;
        Cursor = new Cursor(StandardCursorType.Cross);
    }

    public SKBitmap Export() => AnnotationRenderer.Compose(_source, _items, Crop);

    public void AddText(Point at, string text)
    {
        Push();
        _items.Add(new TextAnnotation(new SKPoint((float)at.X, (float)at.Y), text, Color, Stroke * 6));
        Refresh();
    }

    public void Undo()
    {
        if (_undo.Count == 0) return;
        _redo.Push((_items.ToArray(), Crop));
        Restore(_undo.Pop());
    }

    public void Redo()
    {
        if (_redo.Count == 0) return;
        _undo.Push((_items.ToArray(), Crop));
        Restore(_redo.Pop());
    }

    void Restore((Annotation[] Items, SKRectI? Crop) state)
    {
        _items.Clear();
        _items.AddRange(state.Items);
        Crop = state.Crop;
        Refresh();
    }

    void Push()
    {
        _undo.Push((_items.ToArray(), Crop));
        _redo.Clear();
    }

    void Refresh()
    {
        InvalidateVisual();
        Changed?.Invoke();
    }

    SKPoint Clamp(Point p) => new((float)Math.Clamp(p.X, 0, _source.Width), (float)Math.Clamp(p.Y, 0, _source.Height));

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);
        if (!e.GetCurrentPoint(this).Properties.IsLeftButtonPressed) return;
        Focus();
        var p = e.GetPosition(this);
        if (Tool == EditorTool.Text) { TextRequested?.Invoke(p); return; }
        _start = Clamp(p);
        _shift = e.KeyModifiers.HasFlag(KeyModifiers.Shift);
        _draft = Tool == EditorTool.Pen ? new PenAnnotation(new List<SKPoint> { _start }, Color, Stroke) : Shape(_start);
        e.Pointer.Capture(this);
        InvalidateVisual();
    }

    protected override void OnPointerMoved(PointerEventArgs e)
    {
        base.OnPointerMoved(e);
        if (_draft is null) return;
        var p = Clamp(e.GetPosition(this));
        _shift = e.KeyModifiers.HasFlag(KeyModifiers.Shift);
        if (_draft is PenAnnotation pen) ((List<SKPoint>)pen.Points).Add(p);
        else _draft = Shape(p);
        InvalidateVisual();
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(e);
        if (_draft is null) return;
        var draft = _draft;
        _draft = null;
        e.Pointer.Capture(null);
        var end = Clamp(e.GetPosition(this));
        bool tiny = draft is not PenAnnotation && Math.Abs(end.X - _start.X) < 2 && Math.Abs(end.Y - _start.Y) < 2;
        if (tiny) { InvalidateVisual(); return; }
        Push();
        if (Tool == EditorTool.Crop)
        {
            var r = SKRectI.Round(Box(end));
            r.Intersect(new SKRectI(0, 0, _source.Width, _source.Height));
            Crop = r.IsEmpty ? null : r;
        }
        else _items.Add(draft);
        Refresh();
    }

    /// <summary>Rectangle from the drag start to <paramref name="p"/>; Shift makes it square.</summary>
    SKRect Box(SKPoint p)
    {
        float w = p.X - _start.X, h = p.Y - _start.Y;
        if (_shift)
        {
            float side = Math.Max(Math.Abs(w), Math.Abs(h));
            w = Math.Sign(w) * side; h = Math.Sign(h) * side;
        }
        return new SKRect(_start.X, _start.Y, _start.X + w, _start.Y + h).Standardized;
    }

    Annotation Shape(SKPoint p)
    {
        if (_shift && Tool is EditorTool.Line or EditorTool.Dashed or EditorTool.Arrow)
        {
            // snap to 45° steps
            float dx = p.X - _start.X, dy = p.Y - _start.Y, len = MathF.Sqrt(dx * dx + dy * dy);
            double angle = Math.Round(Math.Atan2(dy, dx) / (Math.PI / 4)) * (Math.PI / 4);
            p = new SKPoint(_start.X + (float)(Math.Cos(angle) * len), _start.Y + (float)(Math.Sin(angle) * len));
        }
        return Tool switch
        {
            EditorTool.Ellipse => new EllipseAnnotation(Box(p), Color, Stroke),
            EditorTool.Line => new LineAnnotation(_start, p, Color, Stroke),
            EditorTool.Dashed => new LineAnnotation(_start, p, Color, Stroke, Dashed: true),
            EditorTool.Arrow => new LineAnnotation(_start, p, Color, Stroke, Arrow: true),
            EditorTool.Pixelate => new PixelateAnnotation(Box(p)),
            EditorTool.Crop => new RectAnnotation(Box(p), SKColors.White, 1), // drawn as the crop frame
            _ => new RectAnnotation(Box(p), Color, Stroke),
        };
    }

    public override void Render(DrawingContext context)
    {
        using (var fb = _surface.Lock())
        using (var surface = SKSurface.Create(new SKImageInfo(fb.Size.Width, fb.Size.Height, SKColorType.Bgra8888, SKAlphaType.Premul), fb.Address, fb.RowBytes))
        {
            var canvas = surface.Canvas;
            canvas.DrawBitmap(_source, 0, 0, new SKSamplingOptions(SKFilterMode.Nearest));
            AnnotationRenderer.Draw(canvas, _source, _items);
            if (_draft is not null && Tool != EditorTool.Crop) AnnotationRenderer.Draw(canvas, _source, _draft);
            var crop = _draft is RectAnnotation d && Tool == EditorTool.Crop ? SKRectI.Round(d.Box) : Crop;
            if (crop is { } c) DrawCropFrame(canvas, c);
        }
        context.DrawImage(_surface, new Rect(0, 0, _source.Width, _source.Height));
    }

    static void DrawCropFrame(SKCanvas canvas, SKRectI c)
    {
        using var dim = new SKPaint { Color = new SKColor(0, 0, 0, 110) };
        canvas.Save();
        canvas.ClipRect(c, SKClipOperation.Difference);
        canvas.DrawRect(canvas.LocalClipBounds, dim);
        canvas.Restore();
        using var edge = new SKPaint { Color = SKColors.White, Style = SKPaintStyle.Stroke, StrokeWidth = 1, PathEffect = SKPathEffect.CreateDash(new[] { 6f, 4f }, 0) };
        canvas.DrawRect(c, edge);
    }
}
