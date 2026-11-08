using System.IO;
using Avalonia.Controls;
using SkiaSharp;

namespace StitchShot.App;

/// <summary>Draws the tray icon at runtime: a blue tile with three stacked bars, no asset file needed.</summary>
static class TrayIconImage
{
    public static WindowIcon Create()
    {
        using var bmp = new SKBitmap(32, 32, SKColorType.Bgra8888, SKAlphaType.Premul);
        using var canvas = new SKCanvas(bmp);
        canvas.Clear(SKColors.Transparent);
        using var fill = new SKPaint { Color = new SKColor(0x2B, 0x6C, 0xB0), IsAntialias = true };
        canvas.DrawRoundRect(new SKRect(1, 1, 31, 31), 6, 6, fill);
        using var bar = new SKPaint { Color = SKColors.White, IsAntialias = true };
        for (int i = 0; i < 3; i++)
            canvas.DrawRoundRect(new SKRect(7, 7 + i * 7, 25, 12 + i * 7), 1.5f, 1.5f, bar);
        using var image = SKImage.FromBitmap(bmp);
        using var png = image.Encode(SKEncodedImageFormat.Png, 100);
        return new WindowIcon(new MemoryStream(png.ToArray()));
    }
}
