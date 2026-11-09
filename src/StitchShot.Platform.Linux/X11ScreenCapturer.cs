using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using SkiaSharp;
using StitchShot.Core;

namespace StitchShot.Platform.Linux;

/// <summary>
/// XGetImage on the root window. Works on X11 sessions and, under XWayland, for X11 apps only.
/// shortcut: no Wayland capture; add an xdg-desktop-portal Screenshot path (with a restore token for silent repeats) when Wayland users ask.
/// </summary>
[SupportedOSPlatform("linux")]
public sealed unsafe class X11ScreenCapturer : IScreenCapturer, IDisposable
{
    IntPtr _display;

    IntPtr Display
    {
        get
        {
            if (_display == IntPtr.Zero) _display = XOpenDisplay(IntPtr.Zero);
            if (_display == IntPtr.Zero)
                throw new InvalidOperationException("Cannot open the X display. StitchShot needs an X11 session (or XWayland) on Linux.");
            return _display;
        }
    }

    public ScreenRect VirtualDesktop
    {
        get
        {
            int screen = XDefaultScreen(Display);
            return new ScreenRect(0, 0, XDisplayWidth(Display, screen), XDisplayHeight(Display, screen));
        }
    }

    public SKBitmap Capture(ScreenRect r)
    {
        var root = XRootWindow(Display, XDefaultScreen(Display));
        var image = XGetImage(Display, root, r.X, r.Y, (uint)r.Width, (uint)r.Height, nuint.MaxValue, ZPixmap);
        if (image == null) throw new InvalidOperationException("XGetImage failed; is the region on screen?");
        try
        {
            if (image->bits_per_pixel != 32)
                throw new NotSupportedException($"{image->bits_per_pixel}-bit X visuals are not supported; use a 24/32-bit TrueColor display.");
            var bitmap = new SKBitmap(r.Width, r.Height, SKColorType.Bgra8888, SKAlphaType.Opaque);
            var src = (byte*)image->data;
            var dst = (byte*)bitmap.GetPixels();
            long row = (long)r.Width * 4;
            for (int y = 0; y < r.Height; y++)
                Buffer.MemoryCopy(src + (long)y * image->bytes_per_line, dst + y * bitmap.RowBytes, row, row);
            return bitmap;
        }
        finally
        {
            ((delegate* unmanaged<XImage*, int>)image->destroy_image)(image); // what the XDestroyImage macro does
        }
    }

    public void Dispose()
    {
        if (_display != IntPtr.Zero) XCloseDisplay(_display);
        _display = IntPtr.Zero;
    }

    const int ZPixmap = 2;
    const string Lib = "libX11.so.6";

    // First part of Xlib's XImage; only the fields read here plus the function table that frees it.
    [StructLayout(LayoutKind.Sequential)]
    struct XImage
    {
        public int width, height, xoffset, format;
        public IntPtr data;
        public int byte_order, bitmap_unit, bitmap_bit_order, bitmap_pad, depth, bytes_per_line, bits_per_pixel;
        public nuint red_mask, green_mask, blue_mask;
        public IntPtr obdata;
        public IntPtr create_image, destroy_image, get_pixel, put_pixel, sub_image, add_pixel;
    }

    [DllImport(Lib)] static extern IntPtr XOpenDisplay(IntPtr name);
    [DllImport(Lib)] static extern int XCloseDisplay(IntPtr display);
    [DllImport(Lib)] static extern int XDefaultScreen(IntPtr display);
    [DllImport(Lib)] static extern IntPtr XRootWindow(IntPtr display, int screen);
    [DllImport(Lib)] static extern int XDisplayWidth(IntPtr display, int screen);
    [DllImport(Lib)] static extern int XDisplayHeight(IntPtr display, int screen);
    [DllImport(Lib)] static extern XImage* XGetImage(IntPtr display, IntPtr drawable, int x, int y, uint width, uint height, nuint planeMask, int format);
}
