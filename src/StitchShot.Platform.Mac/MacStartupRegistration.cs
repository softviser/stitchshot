using StitchShot.Core;

namespace StitchShot.Platform.Mac;

/// <summary>Per-user LaunchAgent; takes effect at the next login without touching Login Items.</summary>
public sealed class MacStartupRegistration : IStartupRegistration
{
    static string Path => System.IO.Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Library", "LaunchAgents", "com.softviser.stitchshot.plist");

    public bool Enabled
    {
        get => File.Exists(Path);
        set
        {
            if (!value) { File.Delete(Path); return; }
            Directory.CreateDirectory(System.IO.Path.GetDirectoryName(Path)!);
            File.WriteAllText(Path, $"""
                <?xml version="1.0" encoding="UTF-8"?>
                <!DOCTYPE plist PUBLIC "-//Apple//DTD PLIST 1.0//EN" "http://www.apple.com/DTDs/PropertyList-1.0.dtd">
                <plist version="1.0">
                <dict>
                    <key>Label</key><string>com.softviser.stitchshot</string>
                    <key>ProgramArguments</key><array><string>{Environment.ProcessPath}</string></array>
                    <key>RunAtLoad</key><true/>
                </dict>
                </plist>

                """);
        }
    }
}
