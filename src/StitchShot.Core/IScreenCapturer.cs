using SkiaSharp;

namespace StitchShot.Core;

/// <summary>A rectangle in physical screen pixels, relative to the virtual desktop origin.</summary>
public readonly record struct ScreenRect(int X, int Y, int Width, int Height);

/// <summary>Implemented once per OS under src/StitchShot.Platform.*.</summary>
public interface IScreenCapturer
{
    /// <summary>Bounds of the whole virtual desktop (all monitors).</summary>
    ScreenRect VirtualDesktop { get; }

    SKBitmap Capture(ScreenRect region);
}
