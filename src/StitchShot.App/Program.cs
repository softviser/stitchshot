using System;
using Avalonia;
using StitchShot.Core;

namespace StitchShot.App;

class Program
{
    [STAThread]
    public static int Main(string[] args)
    {
        // Headless path: `StitchShot --fullscreen` saves the whole desktop and exits. Handy for scripts and smoke tests.
        if (args is ["--fullscreen"])
        {
            var (capturer, clipboard) = App.CreatePlatform();
            using var shot = capturer.Capture(capturer.VirtualDesktop);
            var path = ImageSaver.SavePng(shot, Settings.Load().SaveFolder, DateTime.Now);
            clipboard.SetImage(shot);
            Console.WriteLine(path);
            return 0;
        }
        try
        {
            var code = BuildAvaloniaApp().StartWithClassicDesktopLifetime(args, Avalonia.Controls.ShutdownMode.OnExplicitShutdown);
            Log.Write($"main loop returned {code}");
            return code;
        }
        catch (Exception ex)
        {
            Log.Write(ex.ToString());
            throw;
        }
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
#if DEBUG
            .WithDeveloperTools()
#endif
            .WithInterFont()
            .LogToTrace();
}
