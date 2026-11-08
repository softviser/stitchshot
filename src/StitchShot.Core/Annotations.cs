using SkiaSharp;

namespace StitchShot.Core;

/// <summary>A mark drawn over a screenshot. Kept as data until export so it can be undone.</summary>
public abstract record Annotation;

public sealed record RectAnnotation(SKRect Box, SKColor Color, float Stroke) : Annotation;
public sealed record EllipseAnnotation(SKRect Box, SKColor Color, float Stroke) : Annotation;
public sealed record LineAnnotation(SKPoint From, SKPoint To, SKColor Color, float Stroke, bool Dashed = false, bool Arrow = false) : Annotation;
public sealed record PenAnnotation(IReadOnlyList<SKPoint> Points, SKColor Color, float Stroke) : Annotation;
public sealed record TextAnnotation(SKPoint At, string Text, SKColor Color, float Size) : Annotation;
/// <summary>Hides what is under <paramref name="Box"/> by enlarging its pixels.</summary>
public sealed record PixelateAnnotation(SKRect Box, int Block = 10) : Annotation;

public static class AnnotationRenderer
{
    /// <summary>Draws the source, the annotations, then crops. The caller owns the result.</summary>
    public static SKBitmap Compose(SKBitmap source, IEnumerable<Annotation> items, SKRectI? crop = null)
    {
        var full = new SKBitmap(source.Width, source.Height, SKColorType.Bgra8888, SKAlphaType.Premul);
        using (var canvas = new SKCanvas(full))
        {
            canvas.DrawBitmap(source, 0, 0);
            Draw(canvas, source, items);
        }
        if (crop is not { } c || c == new SKRectI(0, 0, source.Width, source.Height)) return full;
        c.Intersect(new SKRectI(0, 0, source.Width, source.Height));
        var result = new SKBitmap(c.Width, c.Height, SKColorType.Bgra8888, SKAlphaType.Premul);
        if (!full.ExtractSubset(result, c)) throw new InvalidOperationException("crop outside the image");
        full.Dispose();
        return result.Copy(); // ExtractSubset shares pixels with `full`; copy so it can be freed
    }

    public static void Draw(SKCanvas canvas, SKBitmap source, IEnumerable<Annotation> items)
    {
        foreach (var item in items) Draw(canvas, source, item);
    }

    public static void Draw(SKCanvas canvas, SKBitmap source, Annotation item)
    {
        switch (item)
        {
            case RectAnnotation r:
                using (var p = Stroke(r.Color, r.Stroke)) canvas.DrawRect(r.Box.Standardized, p);
                break;
            case EllipseAnnotation e:
                using (var p = Stroke(e.Color, e.Stroke)) canvas.DrawOval(e.Box.Standardized, p);
                break;
            case LineAnnotation l:
                DrawLine(canvas, l);
                break;
            case PenAnnotation pen when pen.Points.Count > 0:
                using (var p = Stroke(pen.Color, pen.Stroke))
                using (var path = new SKPath())
                {
                    path.MoveTo(pen.Points[0]);
                    for (int i = 1; i < pen.Points.Count; i++) path.LineTo(pen.Points[i]);
                    if (pen.Points.Count == 1) path.LineTo(pen.Points[0].X + 0.1f, pen.Points[0].Y); // a dot
                    canvas.DrawPath(path, p);
                }
                break;
            case TextAnnotation t:
                using (var font = new SKFont(SKTypeface.Default, t.Size))
                using (var p = new SKPaint { Color = t.Color, IsAntialias = true })
                {
                    float y = t.At.Y + t.Size; // At is the top-left corner, DrawText wants the baseline
                    foreach (var line in t.Text.Split('\n'))
                    {
                        canvas.DrawText(line, t.At.X, y, SKTextAlign.Left, font, p);
                        y += font.Spacing;
                    }
                }
                break;
            case PixelateAnnotation px:
                Pixelate(canvas, source, px);
                break;
        }
    }

    static SKPaint Stroke(SKColor color, float width) =>
        new() { Color = color, StrokeWidth = width, Style = SKPaintStyle.Stroke, IsAntialias = true, StrokeCap = SKStrokeCap.Round, StrokeJoin = SKStrokeJoin.Round };

    static void DrawLine(SKCanvas canvas, LineAnnotation l)
    {
        using var p = Stroke(l.Color, l.Stroke);
        if (l.Dashed) p.PathEffect = SKPathEffect.CreateDash(new[] { l.Stroke * 3, l.Stroke * 2 }, 0);
        var to = l.To;
        float dx = l.To.X - l.From.X, dy = l.To.Y - l.From.Y, len = MathF.Sqrt(dx * dx + dy * dy);
        if (l.Arrow && len > 0)
        {
            float head = MathF.Max(10, l.Stroke * 4);
            float ux = dx / len, uy = dy / len;
            to = new SKPoint(l.To.X - ux * head * 0.6f, l.To.Y - uy * head * 0.6f); // stop the shaft inside the head
            using var fill = new SKPaint { Color = l.Color, Style = SKPaintStyle.Fill, IsAntialias = true };
            using var path = new SKPath();
            path.MoveTo(l.To);
            path.LineTo(l.To.X - ux * head - uy * head * 0.5f, l.To.Y - uy * head + ux * head * 0.5f);
            path.LineTo(l.To.X - ux * head + uy * head * 0.5f, l.To.Y - uy * head - ux * head * 0.5f);
            path.Close();
            canvas.DrawPath(path, fill);
        }
        canvas.DrawLine(l.From, to, p);
    }

    static void Pixelate(SKCanvas canvas, SKBitmap source, PixelateAnnotation px)
    {
        var box = SKRectI.Round(px.Box.Standardized);
        box.Intersect(new SKRectI(0, 0, source.Width, source.Height));
        if (box.IsEmpty) return;
        using var region = new SKBitmap(box.Width, box.Height, source.ColorType, source.AlphaType);
        if (!source.ExtractSubset(region, box)) return;
        var smallSize = new SKSizeI(Math.Max(1, box.Width / px.Block), Math.Max(1, box.Height / px.Block));
        using var small = region.Resize(smallSize, new SKSamplingOptions(SKFilterMode.Nearest));
        if (small is null) return;
        canvas.DrawBitmap(small, box, new SKSamplingOptions(SKFilterMode.Nearest));
    }
}
