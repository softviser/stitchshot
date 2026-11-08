using SkiaSharp;

namespace StitchShot.Core;

public static class ImageSaver
{
    public static string DefaultFolder =>
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyPictures), "StitchShot");

    public static string FileName(DateTime now) => $"StitchShot_{now:yyyy-MM-dd_HH-mm-ss}.png";

    /// <summary>Writes a PNG into the folder and returns its full path.</summary>
    public static string SavePng(SKBitmap bitmap, string folder, DateTime now)
    {
        Directory.CreateDirectory(folder);
        var path = Path.Combine(folder, FileName(now));
        using var image = SKImage.FromBitmap(bitmap);
        using var data = image.Encode(SKEncodedImageFormat.Png, 100);
        using var file = File.Create(path);
        data.SaveTo(file);
        return path;
    }

    public static SKBitmap Crop(SKBitmap source, ScreenRect r)
    {
        var dst = new SKBitmap(r.Width, r.Height, source.ColorType, source.AlphaType);
        using var canvas = new SKCanvas(dst);
        canvas.DrawBitmap(source, new SKRect(r.X, r.Y, r.X + r.Width, r.Y + r.Height), new SKRect(0, 0, r.Width, r.Height), new SKSamplingOptions(SKFilterMode.Nearest));
        return dst;
    }
}
