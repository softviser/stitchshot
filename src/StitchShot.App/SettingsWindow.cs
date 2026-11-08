using System;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Platform.Storage;
using StitchShot.Core;

namespace StitchShot.App;

/// <summary>Save folder, hotkeys and clipboard option. Hotkey boxes record the next key combination pressed in them.</summary>
public sealed class SettingsWindow : Window
{
    readonly Settings _settings;
    readonly TextBox _folder, _regionKey, _fullKey;
    readonly CheckBox _clipboard;

    /// <summary>Raised after the user saves; the app re-reads the settings object it passed in.</summary>
    public event Action? Saved;

    public SettingsWindow(Settings settings)
    {
        _settings = settings;
        Title = "StitchShot settings";
        Width = 460;
        SizeToContent = SizeToContent.Height;
        CanResize = false;
        WindowStartupLocation = WindowStartupLocation.CenterScreen;

        _folder = new TextBox { Text = settings.SaveFolder };
        _regionKey = HotkeyBox(settings.RegionHotkey);
        _fullKey = HotkeyBox(settings.FullScreenHotkey);
        _clipboard = new CheckBox { Content = "Also copy every capture to the clipboard", IsChecked = settings.CopyToClipboard };

        var browse = new Button { Content = "Browse…" };
        browse.Click += async (_, _) =>
        {
            var picked = await StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions { Title = "Screenshots folder" });
            if (picked.FirstOrDefault()?.TryGetLocalPath() is { } path) _folder.Text = path;
        };
        var folderRow = new Grid { ColumnDefinitions = new ColumnDefinitions("*,Auto"), ColumnSpacing = 6 };
        folderRow.Children.Add(_folder);
        Grid.SetColumn(browse, 1);
        folderRow.Children.Add(browse);

        var save = new Button { Content = "Save", IsDefault = true, HorizontalAlignment = HorizontalAlignment.Right };
        save.Click += (_, _) => SaveAndClose();
        var cancel = new Button { Content = "Cancel", IsCancel = true };
        cancel.Click += (_, _) => Close();
        var buttons = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 6, HorizontalAlignment = HorizontalAlignment.Right };
        buttons.Children.Add(save);
        buttons.Children.Add(cancel);

        var form = new StackPanel { Margin = new Thickness(16), Spacing = 6 };
        form.Children.Add(new TextBlock { Text = "Save screenshots to" });
        form.Children.Add(folderRow);
        form.Children.Add(new TextBlock { Text = "Region capture hotkey", Margin = new Thickness(0, 10, 0, 0) });
        form.Children.Add(_regionKey);
        form.Children.Add(new TextBlock { Text = "Full screen hotkey", Margin = new Thickness(0, 10, 0, 0) });
        form.Children.Add(_fullKey);
        form.Children.Add(new TextBlock { Text = "Click a hotkey box and press the combination you want.", Opacity = 0.6, FontSize = 12 });
        form.Children.Add(_clipboard);
        _clipboard.Margin = new Thickness(0, 10, 0, 0);
        form.Children.Add(new TextBlock { Text = $"Log file: {Log.Path}", Opacity = 0.6, FontSize = 11, Margin = new Thickness(0, 10, 0, 0), TextWrapping = Avalonia.Media.TextWrapping.Wrap });
        buttons.Margin = new Thickness(0, 16, 0, 0);
        form.Children.Add(buttons);
        Content = form;
    }

    static TextBox HotkeyBox(string current)
    {
        var box = new TextBox { Text = current, IsReadOnly = true, PlaceholderText = "Press keys…" };
        box.KeyDown += (_, e) =>
        {
            e.Handled = true;
            var key = KeyName(e.Key);
            if (key is null) return; // a lone modifier; wait for the real key
            box.Text = new HotkeyBinding(key,
                Ctrl: e.KeyModifiers.HasFlag(KeyModifiers.Control),
                Shift: e.KeyModifiers.HasFlag(KeyModifiers.Shift),
                Alt: e.KeyModifiers.HasFlag(KeyModifiers.Alt),
                Win: e.KeyModifiers.HasFlag(KeyModifiers.Meta)).ToString();
        };
        return box;
    }

    /// <summary>Avalonia key names mostly match SharpHook's Vc* names; digits and PrintScreen need help.</summary>
    static string? KeyName(Key key) => key switch
    {
        Key.LeftCtrl or Key.RightCtrl or Key.LeftShift or Key.RightShift or Key.LeftAlt or Key.RightAlt or Key.LWin or Key.RWin => null,
        Key.Snapshot => "PrintScreen",
        >= Key.D0 and <= Key.D9 => ((char)('0' + (key - Key.D0))).ToString(),
        Key.Return => "Enter",
        _ => key.ToString(),
    };

    void SaveAndClose()
    {
        foreach (var box in new[] { _regionKey, _fullKey })
        {
            try { HotkeyBinding.Parse(box.Text ?? ""); }
            catch (FormatException) { box.Focus(); return; }
        }
        _settings.SaveFolder = string.IsNullOrWhiteSpace(_folder.Text) ? ImageSaver.DefaultFolder : _folder.Text.Trim();
        _settings.RegionHotkey = _regionKey.Text!;
        _settings.FullScreenHotkey = _fullKey.Text!;
        _settings.CopyToClipboard = _clipboard.IsChecked == true;
        _settings.Save();
        Saved?.Invoke();
        Close();
    }
}
