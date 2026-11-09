using System.IO;
using Avalonia.Controls;
using Avalonia.Media.Imaging;
using SkiaSharp;

namespace StitchShot.App;

/// <summary>Draws the Seam app icon (design/logo) at run time, so the tray and the windows need no image files.</summary>
static class TrayIconImage
{
    public static WindowIcon Create() => new(Png(64, small: true)); // the OS shows it at 16-24 px, so use the small cut

    /// <summary>The icon as a bitmap for windows that show it large, like About.</summary>
    public static Bitmap Bitmap(int size) => new(Png(size, small: size <= 48));

    static MemoryStream Png(int size, bool small)
    {
        using var bmp = new SKBitmap(size, size, SKColorType.Bgra8888, SKAlphaType.Premul);
        using var canvas = new SKCanvas(bmp);
        canvas.Clear(SKColors.Transparent);
        canvas.Scale(size / 256f);
        using var blue = new SKPaint { Color = new SKColor(0x24, 0x57, 0xC5), IsAntialias = true };
        using var white = new SKPaint { Color = SKColors.White, IsAntialias = true };
        using var yellow = new SKPaint { Color = new SKColor(0xFF, 0xC4, 0x3D), IsAntialias = true };
        canvas.DrawRoundRect(new SKRect(0, 0, 256, 256), 56, 56, blue);
        if (small)
        {
            canvas.Translate(32, 32);
            canvas.Scale(0.75f); // stroke 32, three stitches
            canvas.DrawRoundRect(new SKRect(28, 20, 228, 236), 28, 28, white);
            canvas.DrawRoundRect(new SKRect(60, 52, 196, 204), 2, 2, blue);
            for (int i = 0; i < 3; i++)
                canvas.DrawRect(new SKRect(60 + i * 52, 110, 92 + i * 52, 138), yellow);
        }
        else
        {
            canvas.Translate(38.4f, 38.4f);
            canvas.Scale(0.7f); // stroke 24, five stitches
            canvas.DrawRoundRect(new SKRect(28, 20, 228, 236), 28, 28, white);
            canvas.DrawRoundRect(new SKRect(52, 44, 204, 212), 4, 4, blue);
            for (int i = 0; i < 5; i++)
                canvas.DrawRect(new SKRect(52 + i * 33, 114, 72 + i * 33, 134), yellow);
        }
        using var image = SKImage.FromBitmap(bmp);
        using var png = image.Encode(SKEncodedImageFormat.Png, 100);
        return new MemoryStream(png.ToArray());
    }
}
