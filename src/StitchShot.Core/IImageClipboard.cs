using SkiaSharp;

namespace StitchShot.Core;

/// <summary>Avalonia has no portable "bitmap to clipboard", so each platform provides one.</summary>
public interface IImageClipboard
{
    void SetImage(SKBitmap bitmap);
}
