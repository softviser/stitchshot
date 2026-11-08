using SkiaSharp;
using StitchShot.Core;

namespace StitchShot.Core.Tests;

public class AnnotationTests
{
    static SKBitmap White(int w, int h)
    {
        var b = new SKBitmap(w, h, SKColorType.Bgra8888, SKAlphaType.Premul);
        b.Erase(SKColors.White);
        return b;
    }

    [Fact]
    public void Rect_draws_stroke_and_leaves_inside_alone()
    {
        using var src = White(100, 100);
        using var result = AnnotationRenderer.Compose(src, new[] { new RectAnnotation(new SKRect(10, 10, 60, 60), SKColors.Red, 4) });
        Assert.NotEqual(SKColors.White, result.GetPixel(10, 30)); // on the edge
        Assert.Equal(SKColors.White, result.GetPixel(35, 35));    // inside
        Assert.Equal(SKColors.White, result.GetPixel(90, 90));    // outside
    }

    [Fact]
    public void Arrow_fills_a_head_at_the_end()
    {
        using var src = White(100, 50);
        using var result = AnnotationRenderer.Compose(src, new[] { new LineAnnotation(new SKPoint(10, 25), new SKPoint(90, 25), SKColors.Blue, 2, Arrow: true) });
        Assert.NotEqual(SKColors.White, result.GetPixel(86, 25)); // on the head
        Assert.NotEqual(SKColors.White, result.GetPixel(50, 25)); // on the shaft
        Assert.Equal(SKColors.White, result.GetPixel(50, 10));    // off the line
    }

    [Fact]
    public void Text_puts_ink_below_its_anchor()
    {
        using var src = White(200, 60);
        using var result = AnnotationRenderer.Compose(src, new[] { new TextAnnotation(new SKPoint(5, 5), "Hello", SKColors.Black, 24) });
        bool anyInk = false;
        for (int y = 5; y < 40 && !anyInk; y++)
            for (int x = 5; x < 80 && !anyInk; x++)
                anyInk = result.GetPixel(x, y) != SKColors.White;
        Assert.True(anyInk);
    }

    [Fact]
    public void Pixelate_flattens_detail_inside_the_box_only()
    {
        using var src = White(100, 100);
        for (int x = 0; x < 100; x++) for (int y = 0; y < 100; y++) src.SetPixel(x, y, (x + y) % 2 == 0 ? SKColors.Black : SKColors.White);
        using var result = AnnotationRenderer.Compose(src, new[] { new PixelateAnnotation(new SKRect(20, 20, 60, 60), Block: 10) });
        // inside a block every pixel is the same; the checkerboard outside survives
        Assert.Equal(result.GetPixel(25, 25), result.GetPixel(26, 25));
        Assert.NotEqual(result.GetPixel(80, 80), result.GetPixel(81, 80));
    }

    [Fact]
    public void Crop_returns_the_requested_size_and_content()
    {
        using var src = White(100, 100);
        src.SetPixel(30, 40, SKColors.Red);
        using var result = AnnotationRenderer.Compose(src, Array.Empty<Annotation>(), new SKRectI(20, 30, 70, 90));
        Assert.Equal(50, result.Width);
        Assert.Equal(60, result.Height);
        Assert.Equal(SKColors.Red, result.GetPixel(10, 10));
    }
}
