using System.Diagnostics;
using SkiaSharp;
using StitchShot.Core;

namespace StitchShot.Platform.Mac;

/// <summary>
/// Uses the system's screencapture tool, so the Screen Recording permission prompt and Retina scaling are Apple's problem, not ours.
/// shortcut: main display only, cropped from a full grab; switch to CGDisplayCreateImage per display when multi-monitor Macs matter.
/// </summary>
public sealed class MacScreenCapturer : IScreenCapturer
{
    ScreenRect? _desktop;

    public ScreenRect VirtualDesktop
    {
        get
        {
            if (_desktop is null) Grab().Dispose(); // a grab records the pixel size
            return _desktop!.Value;
        }
    }

    public SKBitmap Capture(ScreenRect r)
    {
        using var full = Grab();
        return ImageSaver.Crop(full, r);
    }

    SKBitmap Grab()
    {
        var path = Path.Combine(Path.GetTempPath(), $"stitchshot-{Guid.NewGuid():N}.png");
        try
        {
            using var p = Process.Start(new ProcessStartInfo("/usr/sbin/screencapture") { ArgumentList = { "-x", "-t", "png", path }, UseShellExecute = false })
                ?? throw new InvalidOperationException("screencapture did not start.");
            p.WaitForExit();
            if (p.ExitCode != 0 || !File.Exists(path))
                throw new InvalidOperationException("screencapture failed. Allow StitchShot under System Settings > Privacy & Security > Screen Recording, then try again.");
            using var decoded = SKBitmap.Decode(path) ?? throw new InvalidOperationException("Could not decode the screenshot.");
            var bitmap = decoded.ColorType == SKColorType.Bgra8888 ? decoded.Copy() : decoded.Copy(SKColorType.Bgra8888);
            _desktop = new ScreenRect(0, 0, bitmap.Width, bitmap.Height);
            return bitmap;
        }
        finally { File.Delete(path); }
    }
}
