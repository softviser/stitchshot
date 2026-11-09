using System.ComponentModel;
using System.Diagnostics;
using SkiaSharp;
using StitchShot.Core;

namespace StitchShot.Platform.Linux;

/// <summary>
/// Pipes a PNG to wl-copy (Wayland) or xclip (X11), the tools every distro packages.
/// shortcut: owning the selection ourselves needs an X event loop or a Wayland data source; do it when a user hits a machine without either tool.
/// </summary>
public sealed class LinuxImageClipboard : IImageClipboard
{
    public void SetImage(SKBitmap bitmap)
    {
        bool wayland = !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("WAYLAND_DISPLAY"));
        var psi = wayland
            ? new ProcessStartInfo("wl-copy") { ArgumentList = { "--type", "image/png" } }
            : new ProcessStartInfo("xclip") { ArgumentList = { "-selection", "clipboard", "-t", "image/png", "-i" } };
        psi.RedirectStandardInput = true;
        psi.UseShellExecute = false;

        Process process;
        try { process = Process.Start(psi) ?? throw new InvalidOperationException($"{psi.FileName} did not start."); }
        catch (Win32Exception) { throw new InvalidOperationException($"Install {psi.FileName} to copy images to the clipboard."); }
        using (process)
        {
            using var png = bitmap.Encode(SKEncodedImageFormat.Png, 100);
            png.SaveTo(process.StandardInput.BaseStream);
            process.StandardInput.Close();
            process.WaitForExit(5000); // xclip forks a child that serves the selection, the parent returns at once
        }
    }
}
