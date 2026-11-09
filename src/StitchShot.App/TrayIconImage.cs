using System.IO;
using Avalonia.Controls;
using SkiaSharp;

namespace StitchShot.App;

/// <summary>Draws the Seam mark at runtime (blue tile, white frame, yellow stitches); same geometry as design/logo, no asset file needed.</summary>
static class TrayIconImage
{
    public static WindowIcon Create()
    {
        const int size = 64;
        using var bmp = new SKBitmap(size, size, SKColorType.Bgra8888, SKAlphaType.Premul);
        using var canvas = new SKCanvas(bmp);
        canvas.Clear(SKColors.Transparent);
        canvas.Scale(size / 256f);
        using var blue = new SKPaint { Color = new SKColor(0x24, 0x57, 0xC5), IsAntialias = true };
        using var white = new SKPaint { Color = SKColors.White, IsAntialias = true };
        using var yellow = new SKPaint { Color = new SKColor(0xFF, 0xC4, 0x3D), IsAntialias = true };
        canvas.DrawRoundRect(new SKRect(0, 0, 256, 256), 56, 56, blue);
        canvas.Translate(32, 32);
        canvas.Scale(0.75f); // the small-size cut: stroke 32, three stitches
        canvas.DrawRoundRect(new SKRect(28, 20, 228, 236), 28, 28, white);
        canvas.DrawRoundRect(new SKRect(60, 52, 196, 204), 2, 2, blue);
        for (int i = 0; i < 3; i++)
            canvas.DrawRect(new SKRect(60 + i * 52, 110, 92 + i * 52, 138), yellow);
        using var image = SKImage.FromBitmap(bmp);
        using var png = image.Encode(SKEncodedImageFormat.Png, 100);
        return new WindowIcon(new MemoryStream(png.ToArray()));
    }
}
