// Takes the product screenshots: shows each StitchShot window over a prepared "desktop" picture on the primary monitor,
// captures it with the app's own screen capturer and writes PNGs. Reflection reaches into two windows to put them in
// a mid-action state (a selection being dragged, a few annotations); nothing in the app is changed for this.
//
//   dotnet run --project tools/Screenshots -- tools/Screenshots/in docs/screenshots
//
// in/desktop.png   1920 × 1080, what the "screen" shows behind the windows
// in/longpage.png  1280 wide, tall; sliced into overlapping frames for the stitched example
using System.Reflection;
using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Media.Imaging;
using Avalonia.Styling;
using Avalonia.Threading;
using SkiaSharp;
using StitchShot.App;
using StitchShot.Core;
using StitchShot.Platform.Windows;

class Program
{
    [STAThread]
    static int Main(string[] args) =>
        AppBuilder.Configure<ShotApp>().UsePlatformDetect().WithInterFont().StartWithClassicDesktopLifetime(args, ShutdownMode.OnExplicitShutdown);
}

sealed class ShotApp : Application
{
    public override void Initialize()
    {
        Styles.Add(new Avalonia.Themes.Fluent.FluentTheme());
        RequestedThemeVariant = ThemeVariant.Light;
    }

    public override void OnFrameworkInitializationCompleted()
    {
        var life = (IClassicDesktopStyleApplicationLifetime)ApplicationLifetime!;
        Dispatcher.UIThread.Post(async () =>
        {
            int code = 0;
            try { await new Shots(life.Args ?? Array.Empty<string>()).Run(); }
            catch (Exception ex) { Console.Error.WriteLine(ex); code = 1; }
            life.Shutdown(code);
        });
    }
}

sealed class Shots
{
    readonly string _in, _out;
    readonly GdiScreenCapturer _cap = new();
    Window? _backdrop;

    public Shots(string[] args)
    {
        if (args.Length != 2) throw new ArgumentException("usage: Screenshots <in dir> <out dir>");
        (_in, _out) = (args[0], args[1]);
        Directory.CreateDirectory(_out);
    }

    public async Task Run()
    {
        using var desktop = SKBitmap.Decode(Path.Combine(_in, "desktop.png")) ?? throw new FileNotFoundException("desktop.png");
        using var longPage = SKBitmap.Decode(Path.Combine(_in, "longpage.png")) ?? throw new FileNotFoundException("longpage.png");

        // The stitched example: a long page captured 720 rows at a time with 220 rows of overlap, joined by the real composer.
        using var composer = new StitchComposer();
        for (int y = 0; y + 720 <= longPage.Height && composer.Count < 6; y += 500)
            composer.Add(Crop(longPage, new SKRectI(0, y, 1280, y + 720)));
        using var stitched = composer.Compose();
        Save(stitched, "stitched.png");
        Console.WriteLine($"stitched: {composer.Count} frames -> {stitched.Width} x {stitched.Height}");

        await ShowBackdrop(desktop);

        // Settings.
        var settings = new Settings();
        var sw = new SettingsWindow(settings, new NoStartup()) { Topmost = true };
        await Show(sw);
        Save(Grab(FrameRect(sw)), "settings.png");
        sw.Close();

        // About.
        AboutWindow.Open();
        var about = Windows().First(w => w is AboutWindow);
        about.Topmost = true;
        about.Activate();
        await Settle();
        Save(Grab(FrameRect(about)), "about.png");
        about.Close();

        // The HUD during a session, bottom-right of the screen, with some desktop around it.
        var hud = new HudWindow(settings, () => { }, () => { }, () => { }, () => { }, () => { }, () => { });
        hud.Update(composer);
        await Show(hud);
        var hr = FrameRect(hud);
        Save(Grab(new PixelRect(hr.Right - 760, hr.Bottom - 300, 760, 300).Intersect(Screen())), "stitch-hud.png");
        hud.Close();

        // The "Saved" notification, top-right, with a thumbnail of the stitched image.
        ToastWindow.Settings = new Settings();
        ToastWindow.Anchor = new PixelPoint(100, 100);
        ToastWindow.Show("Saved", "StitchShot 2026-11-10 10.41.07.png · " + $"{stitched.Width} × {stitched.Height}", stitched, path: Path.Combine(_out, "stitched.png"), edit: () => { });
        var toast = Windows().First(w => w is ToastWindow);
        await Settle();
        var tr = FrameRect(toast);
        Save(Grab(new PixelRect(tr.Right - 760, 0, 760, 300).Intersect(Screen())), "notification.png");
        toast.Close();

        // The editor with a few marks on a piece of the desktop.
        using var piece = Crop(desktop, new SKRectI(160, 120, 1260, 800));
        var editor = new EditorWindow(piece, _ => { }, _ => { }) { Topmost = true };
        var canvas = Field<object>(editor, "_canvas");
        var items = Field<List<Annotation>>(canvas, "_items");
        items.Add(new RectAnnotation(new SKRect(60, 80, 560, 300), SKColors.Red, 3));
        items.Add(new LineAnnotation(new SKPoint(760, 520), new SKPoint(590, 310), SKColors.Red, 3, Arrow: true));
        items.Add(new TextAnnotation(new SKPoint(770, 540), "This part", SKColors.Red, 18));
        items.Add(new PixelateAnnotation(new SKRect(640, 90, 1040, 180), 12));
        Call(canvas, "Refresh");
        await Show(editor);
        Save(Grab(FrameRect(editor)), "editor.png");
        editor.Close();

        // The overlay in the middle of a drag: crosshair, selection with its size, loupe at the cursor and at the start corner.
        _backdrop!.Hide();
        var overlay = new OverlayWindow(desktop.Copy(), new ScreenRect(0, 0, desktop.Width, desktop.Height));
        await Show(overlay);
        var view = Field<Control>(overlay, "_view");
        Set(view, "_dragStart", new PixelPoint(380, 120));
        Set(view, "_cursor", new PixelPoint(1140, 700));
        Set(view, "_selection", (PixelRect?)new PixelRect(380, 120, 760, 580));
        Set(view, "_dragging", true);
        view.InvalidateVisual();
        await Settle();
        Save(Grab(new PixelRect(160, 20, 1400, 840)), "overlay.png");
        overlay.Close();
        _backdrop.Close();
    }

    // ---- windows and capture ----

    async Task ShowBackdrop(SKBitmap desktop)
    {
        using var ms = new MemoryStream(desktop.Encode(SKEncodedImageFormat.Png, 100).ToArray());
        _backdrop = new Window
        {
            WindowDecorations = WindowDecorations.None, ShowInTaskbar = false, CanResize = false, Topmost = true, // this process is not in the foreground; Topmost is the only way over other apps
            WindowStartupLocation = WindowStartupLocation.Manual, Position = new PixelPoint(0, 0),
            Width = desktop.Width, Height = desktop.Height,
            Content = new Image { Source = new Bitmap(ms), Stretch = Avalonia.Media.Stretch.Fill },
        };
        await Show(_backdrop);
    }

    static async Task Show(Window w)
    {
        w.Show();
        await Settle();
    }

    /// <summary>Lets layout, fonts and the compositor catch up before the screen is read.</summary>
    static async Task Settle()
    {
        for (int i = 0; i < 3; i++) await Dispatcher.UIThread.InvokeAsync(() => { }, DispatcherPriority.Background);
        await Task.Delay(700);
    }

    static IEnumerable<Window> Windows() => ((IClassicDesktopStyleApplicationLifetime)Application.Current!.ApplicationLifetime!).Windows;

    PixelRect Screen() => new(0, 0, (int)_cap.VirtualDesktop.Width, (int)_cap.VirtualDesktop.Height);

    /// <summary>The window as it is drawn, without the invisible resize border Windows adds around decorated windows.</summary>
    static PixelRect FrameRect(Window w)
    {
        var handle = w.TryGetPlatformHandle()?.Handle ?? IntPtr.Zero;
        if (handle != IntPtr.Zero && DwmGetWindowAttribute(handle, 9, out var r, Marshal.SizeOf<RECT>()) == 0)
            return new PixelRect(r.Left, r.Top, r.Right - r.Left, r.Bottom - r.Top);
        var size = w.FrameSize ?? w.Bounds.Size;
        return new PixelRect(w.Position, new PixelSize((int)(size.Width * w.RenderScaling), (int)(size.Height * w.RenderScaling)));
    }

    SKBitmap Grab(PixelRect r) => _cap.Capture(new ScreenRect(r.X, r.Y, r.Width, r.Height));

    void Save(SKBitmap bmp, string name)
    {
        using var data = bmp.Encode(SKEncodedImageFormat.Png, 100);
        using var f = File.Create(Path.Combine(_out, name));
        data.SaveTo(f);
        Console.WriteLine($"{name}: {bmp.Width} x {bmp.Height}");
    }

    static SKBitmap Crop(SKBitmap src, SKRectI r)
    {
        var dst = new SKBitmap(r.Width, r.Height, src.ColorType, SKAlphaType.Opaque);
        using var c = new SKCanvas(dst);
        c.DrawBitmap(src, r, new SKRect(0, 0, r.Width, r.Height));
        return dst;
    }

    // ---- reflection into private state, for the two mid-action pictures ----

    static T Field<T>(object o, string name) => (T)o.GetType().GetField(name, BindingFlags.NonPublic | BindingFlags.Instance)!.GetValue(o)!;
    static void Set(object o, string name, object? value) => o.GetType().GetField(name, BindingFlags.NonPublic | BindingFlags.Instance)!.SetValue(o, value);
    static void Call(object o, string name) => o.GetType().GetMethod(name, BindingFlags.NonPublic | BindingFlags.Instance)!.Invoke(o, null);

    [StructLayout(LayoutKind.Sequential)] struct RECT { public int Left, Top, Right, Bottom; }
    [DllImport("dwmapi.dll")] static extern int DwmGetWindowAttribute(IntPtr hwnd, int attr, out RECT rect, int size);

    sealed class NoStartup : IStartupRegistration { public bool Enabled { get; set; } }
}
