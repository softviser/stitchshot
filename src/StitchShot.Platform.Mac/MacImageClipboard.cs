using System.Diagnostics;
using SkiaSharp;
using StitchShot.Core;

namespace StitchShot.Platform.Mac;

/// <summary>Hands a PNG file to the pasteboard through AppleScript; avoids binding AppKit from .NET for one call.</summary>
public sealed class MacImageClipboard : IImageClipboard
{
    public void SetImage(SKBitmap bitmap)
    {
        var path = Path.Combine(Path.GetTempPath(), $"stitchshot-clip-{Guid.NewGuid():N}.png");
        try
        {
            using (var png = bitmap.Encode(SKEncodedImageFormat.Png, 100))
            using (var file = File.Create(path))
                png.SaveTo(file);
            var psi = new ProcessStartInfo("/usr/bin/osascript") { UseShellExecute = false };
            psi.ArgumentList.Add("-e");
            psi.ArgumentList.Add($"set the clipboard to (read (POSIX file \"{path}\") as «class PNGf»)");
            using var p = Process.Start(psi) ?? throw new InvalidOperationException("osascript did not start.");
            p.WaitForExit();
            if (p.ExitCode != 0) throw new InvalidOperationException("Could not put the image on the clipboard.");
        }
        finally { File.Delete(path); }
    }
}
