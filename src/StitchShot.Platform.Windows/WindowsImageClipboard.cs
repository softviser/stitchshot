using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using SkiaSharp;
using StitchShot.Core;

namespace StitchShot.Platform.Windows;

/// <summary>Puts the bitmap on the clipboard as CF_DIB (32-bit top-down). Must run on an STA thread.</summary>
[SupportedOSPlatform("windows")]
public sealed class WindowsImageClipboard : IImageClipboard
{
    public void SetImage(SKBitmap bitmap)
    {
        using var src = bitmap.ColorType == SKColorType.Bgra8888 ? null : bitmap.Copy(SKColorType.Bgra8888);
        var pixels = src ?? bitmap;
        const int headerSize = 40;
        int imageSize = pixels.Width * pixels.Height * 4;
        var mem = GlobalAlloc(GMEM_MOVEABLE, (UIntPtr)(headerSize + imageSize));
        if (mem == IntPtr.Zero) throw new OutOfMemoryException();
        var p = GlobalLock(mem);
        try
        {
            var header = new int[] { headerSize, pixels.Width, -pixels.Height, 1 | (32 << 16), 0, imageSize, 0, 0, 0, 0 };
            Marshal.Copy(header, 0, p, header.Length);
            unsafe { Buffer.MemoryCopy((void*)pixels.GetPixels(), (void*)(p + headerSize), imageSize, imageSize); }
        }
        finally { GlobalUnlock(mem); }

        if (!OpenClipboard(IntPtr.Zero)) { GlobalFree(mem); throw new InvalidOperationException("Clipboard is busy."); }
        try
        {
            EmptyClipboard();
            if (SetClipboardData(CF_DIB, mem) == IntPtr.Zero) { GlobalFree(mem); throw new InvalidOperationException("SetClipboardData failed."); }
        }
        finally { CloseClipboard(); }
    }

    const uint GMEM_MOVEABLE = 2, CF_DIB = 8;
    [DllImport("kernel32.dll")] static extern IntPtr GlobalAlloc(uint flags, UIntPtr bytes);
    [DllImport("kernel32.dll")] static extern IntPtr GlobalFree(IntPtr mem);
    [DllImport("kernel32.dll")] static extern IntPtr GlobalLock(IntPtr mem);
    [DllImport("kernel32.dll")] static extern bool GlobalUnlock(IntPtr mem);
    [DllImport("user32.dll")] static extern bool OpenClipboard(IntPtr owner);
    [DllImport("user32.dll")] static extern bool CloseClipboard();
    [DllImport("user32.dll")] static extern bool EmptyClipboard();
    [DllImport("user32.dll")] static extern IntPtr SetClipboardData(uint format, IntPtr mem);
}
