namespace StitchShot.Core;

/// <summary>
/// A key combination as the user writes it in settings: "Ctrl+Shift+S", "PrintScreen", "F8".
/// Key names follow the .NET/Avalonia Key enum; the platform layer maps them to its own codes.
/// </summary>
public sealed record HotkeyBinding(string Key, bool Ctrl = false, bool Shift = false, bool Alt = false, bool Win = false)
{
    public static HotkeyBinding Parse(string text)
    {
        var parts = text.Split('+', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (parts.Length == 0) throw new FormatException("Empty hotkey.");

        bool ctrl = false, shift = false, alt = false, win = false;
        foreach (var p in parts[..^1])
        {
            switch (p.ToLowerInvariant())
            {
                case "ctrl" or "control": ctrl = true; break;
                case "shift": shift = true; break;
                case "alt": alt = true; break;
                case "win" or "meta" or "cmd" or "super": win = true; break;
                default: throw new FormatException($"Unknown modifier '{p}' in '{text}'.");
            }
        }
        var key = parts[^1];
        if (key.Length == 1) key = key.ToUpperInvariant();
        return new HotkeyBinding(key, ctrl, shift, alt, win);
    }

    public override string ToString() =>
        string.Join("+", new[] { Ctrl ? "Ctrl" : null, Shift ? "Shift" : null, Alt ? "Alt" : null, Win ? "Win" : null, Key }
            .Where(s => s is not null));
}
