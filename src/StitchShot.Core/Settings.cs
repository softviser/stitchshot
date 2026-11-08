using System.Text.Json;

namespace StitchShot.Core;

public sealed class Settings
{
    public string SaveFolder { get; set; } = ImageSaver.DefaultFolder;
    public string RegionHotkey { get; set; } = "PrintScreen";
    public string FullScreenHotkey { get; set; } = "Ctrl+PrintScreen";
    public bool CopyToClipboard { get; set; } = true;

    public string StitchStartHotkey { get; set; } = "Ctrl+Shift+S";
    public string StitchCaptureHotkey { get; set; } = "F8";
    public string StitchScrollCaptureHotkey { get; set; } = "F10";
    public string StitchFinishHotkey { get; set; } = "F9";
    public bool TrimOverlap { get; set; } = true;
    public int CropTop { get; set; }
    public int ScrollDelayMs { get; set; } = 400;
    public int? HudX { get; set; }
    public int? HudY { get; set; }
    public int? ToastX { get; set; }
    public int? ToastY { get; set; }

    public static string DefaultPath => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "StitchShot", "settings.json");

    static readonly JsonSerializerOptions Json = new() { WriteIndented = true };

    /// <summary>A missing or unreadable file yields defaults; the app must start either way.</summary>
    public static Settings Load(string? path = null)
    {
        path ??= DefaultPath;
        try
        {
            if (File.Exists(path))
                return JsonSerializer.Deserialize<Settings>(File.ReadAllText(path), Json) ?? new Settings();
        }
        catch (JsonException) { }
        catch (IOException) { }
        return new Settings();
    }

    public void Save(string? path = null)
    {
        path ??= DefaultPath;
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllText(path, JsonSerializer.Serialize(this, Json));
    }
}
