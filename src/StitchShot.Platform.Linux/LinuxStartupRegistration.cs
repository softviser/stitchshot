using StitchShot.Core;

namespace StitchShot.Platform.Linux;

/// <summary>XDG autostart entry in ~/.config/autostart; honoured by GNOME, KDE, XFCE and the rest.</summary>
public sealed class LinuxStartupRegistration : IStartupRegistration
{
    static string Path => System.IO.Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "autostart", "stitchshot.desktop");

    public bool Enabled
    {
        get => File.Exists(Path);
        set
        {
            if (!value) { File.Delete(Path); return; }
            Directory.CreateDirectory(System.IO.Path.GetDirectoryName(Path)!);
            File.WriteAllText(Path, $"""
                [Desktop Entry]
                Type=Application
                Name=StitchShot
                Comment=Screenshot tool that stitches captures into one tall image
                Exec="{Environment.ProcessPath}"
                Terminal=false
                X-GNOME-Autostart-enabled=true

                """);
        }
    }
}
