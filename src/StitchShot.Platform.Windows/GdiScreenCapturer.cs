using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using SkiaSharp;
using StitchShot.Core;

namespace StitchShot.Platform.Windows;

/// <summary>BitBlt from the desktop DC. Needs the process to be per-monitor DPI aware, which Avalonia sets up.</summary>
[SupportedOSPlatform("windows")]
public sealed class GdiScreenCapturer : IScreenCapturer
{
    public ScreenRect VirtualDesktop => new(
        GetSystemMetrics(SM_XVIRTUALSCREEN), GetSystemMetrics(SM_YVIRTUALSCREEN),
        GetSystemMetrics(SM_CXVIRTUALSCREEN), GetSystemMetrics(SM_CYVIRTUALSCREEN));

    public SKBitmap Capture(ScreenRect r)
    {
        var bitmap = new SKBitmap(r.Width, r.Height, SKColorType.Bgra8888, SKAlphaType.Opaque);
        var screen = GetDC(IntPtr.Zero);
        var mem = CreateCompatibleDC(screen);
        var info = new BITMAPINFOHEADER { biSize = 40, biWidth = r.Width, biHeight = -r.Height, biPlanes = 1, biBitCount = 32 };
        var dib = CreateDIBSection(screen, ref info, 0, out var bits, IntPtr.Zero, 0);
        try
        {
            if (dib == IntPtr.Zero) throw new InvalidOperationException("CreateDIBSection failed.");
            var old = SelectObject(mem, dib);
            if (!BitBlt(mem, 0, 0, r.Width, r.Height, screen, r.X, r.Y, SRCCOPY | CAPTUREBLT))
                throw new InvalidOperationException($"BitBlt failed: {Marshal.GetLastWin32Error()}");
            SelectObject(mem, old);
            unsafe { Buffer.MemoryCopy((void*)bits, (void*)bitmap.GetPixels(), bitmap.ByteCount, (long)r.Width * r.Height * 4); }
            return bitmap;
        }
        finally
        {
            if (dib != IntPtr.Zero) DeleteObject(dib);
            DeleteDC(mem);
            ReleaseDC(IntPtr.Zero, screen);
        }
    }

    const int SM_XVIRTUALSCREEN = 76, SM_YVIRTUALSCREEN = 77, SM_CXVIRTUALSCREEN = 78, SM_CYVIRTUALSCREEN = 79;
    const uint SRCCOPY = 0x00CC0020, CAPTUREBLT = 0x40000000;

    [StructLayout(LayoutKind.Sequential)]
    struct BITMAPINFOHEADER
    {
        public uint biSize; public int biWidth; public int biHeight; public ushort biPlanes; public ushort biBitCount;
        public uint biCompression; public uint biSizeImage; public int biXPelsPerMeter; public int biYPelsPerMeter;
        public uint biClrUsed; public uint biClrImportant;
    }

    [DllImport("user32.dll")] static extern int GetSystemMetrics(int index);
    [DllImport("user32.dll")] static extern IntPtr GetDC(IntPtr hwnd);
    [DllImport("user32.dll")] static extern int ReleaseDC(IntPtr hwnd, IntPtr dc);
    [DllImport("gdi32.dll")] static extern IntPtr CreateCompatibleDC(IntPtr dc);
    [DllImport("gdi32.dll")] static extern bool DeleteDC(IntPtr dc);
    [DllImport("gdi32.dll")] static extern IntPtr SelectObject(IntPtr dc, IntPtr obj);
    [DllImport("gdi32.dll")] static extern bool DeleteObject(IntPtr obj);
    [DllImport("gdi32.dll", SetLastError = true)]
    static extern bool BitBlt(IntPtr dst, int x, int y, int w, int h, IntPtr src, int sx, int sy, uint rop);
    [DllImport("gdi32.dll")]
    static extern IntPtr CreateDIBSection(IntPtr dc, ref BITMAPINFOHEADER info, uint usage, out IntPtr bits, IntPtr section, uint offset);
}
