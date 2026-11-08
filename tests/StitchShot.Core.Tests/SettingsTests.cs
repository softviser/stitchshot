using StitchShot.Core;

namespace StitchShot.Core.Tests;

public class SettingsTests
{
    [Fact]
    public void Round_trips_through_json()
    {
        var path = Path.Combine(Path.GetTempPath(), $"stitchshot-{Guid.NewGuid():N}", "settings.json");
        try
        {
            new Settings { SaveFolder = @"D:\shots", RegionHotkey = "F8", CopyToClipboard = false }.Save(path);
            var loaded = Settings.Load(path);
            Assert.Equal(@"D:\shots", loaded.SaveFolder);
            Assert.Equal("F8", loaded.RegionHotkey);
            Assert.Equal("Ctrl+PrintScreen", loaded.FullScreenHotkey);
            Assert.False(loaded.CopyToClipboard);
        }
        finally { Directory.Delete(Path.GetDirectoryName(path)!, true); }
    }

    [Fact]
    public void Missing_or_broken_file_gives_defaults()
    {
        Assert.Equal("PrintScreen", Settings.Load(Path.Combine(Path.GetTempPath(), "does-not-exist.json")).RegionHotkey);
        var broken = Path.GetTempFileName();
        File.WriteAllText(broken, "{ not json");
        Assert.Equal("PrintScreen", Settings.Load(broken).RegionHotkey);
        File.Delete(broken);
    }
}
