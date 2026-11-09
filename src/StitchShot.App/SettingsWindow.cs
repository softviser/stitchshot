using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Platform.Storage;
using StitchShot.Core;

namespace StitchShot.App;

/// <summary>Save folder, hotkeys and stitch options. Hotkey boxes record the next key combination pressed in them.</summary>
public sealed class SettingsWindow : Window
{
    readonly Settings _settings;
    readonly TextBox _folder;
    readonly CheckBox _clipboard, _trimOverlap;
    readonly NumericUpDown _cropTop, _scrollDelay;
    readonly List<(TextBox Box, Action<string> Apply)> _hotkeys = new();

    /// <summary>Raised after the user saves; the app re-reads the settings object it passed in.</summary>
    public event Action? Saved;

    public SettingsWindow(Settings s, IStartupRegistration startup)
    {
        _settings = s;
        Title = "StitchShot settings";
        Icon = TrayIconImage.Create();
        Width = 480;
        SizeToContent = SizeToContent.Height;
        CanResize = false;
        WindowStartupLocation = WindowStartupLocation.CenterScreen;

        var form = new StackPanel { Margin = new Thickness(16), Spacing = 6 };

        _folder = new TextBox { Text = s.SaveFolder };
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
        Section(form, "Save screenshots to");
        form.Children.Add(folderRow);
        _clipboard = new CheckBox { Content = "Also copy every capture to the clipboard", IsChecked = s.CopyToClipboard };
        form.Children.Add(_clipboard);
        if (startup.ExternalSettingsUri is { } uri)
        {
            form.Children.Add(new HyperlinkButton { Content = "Start at login is set in Windows Settings > Apps > Startup for this install. Open it…", NavigateUri = new Uri(uri), Padding = new Thickness(0), FontSize = 12 });
        }
        else
        {
            var startWithOs = new CheckBox { Content = "Start StitchShot when I log in", IsChecked = startup.Enabled };
            startWithOs.IsCheckedChanged += (_, _) =>
            {
                try { startup.Enabled = startWithOs.IsChecked == true; }
                catch (Exception ex) { Log.Write(ex.ToString()); ToastWindow.Show("Could not change startup setting", ex.Message); }
            };
            form.Children.Add(startWithOs);
        }

        Section(form, "Hotkeys");
        form.Children.Add(new TextBlock { Text = "Click a box and press the combination you want.", Opacity = 0.6, FontSize = 12 });
        var grid = new Grid { ColumnDefinitions = new ColumnDefinitions("*,*"), ColumnSpacing = 10, RowSpacing = 4 };
        Hotkey(grid, "Capture region", s.RegionHotkey, v => s.RegionHotkey = v);
        Hotkey(grid, "Capture full screen", s.FullScreenHotkey, v => s.FullScreenHotkey = v);
        Hotkey(grid, "Start stitch / add region", s.StitchStartHotkey, v => s.StitchStartHotkey = v);
        Hotkey(grid, "Stitch: capture frame", s.StitchCaptureHotkey, v => s.StitchCaptureHotkey = v);
        Hotkey(grid, "Stitch: Page Down + capture", s.StitchScrollCaptureHotkey, v => s.StitchScrollCaptureHotkey = v);
        Hotkey(grid, "Stitch: finish and save", s.StitchFinishHotkey, v => s.StitchFinishHotkey = v);
        form.Children.Add(grid);

        Section(form, "Stitching");
        _trimOverlap = new CheckBox { Content = "Trim the overlap between scrolled frames", IsChecked = s.TrimOverlap };
        form.Children.Add(_trimOverlap);
        _cropTop = Number(s.CropTop, 0, 2000);
        form.Children.Add(Labeled("Crop this many pixels from the top of frames 2+ (sticky headers)", _cropTop));
        _scrollDelay = Number(s.ScrollDelayMs, 0, 5000);
        form.Children.Add(Labeled("Wait after Page Down, milliseconds", _scrollDelay));

        var save = new Button { Content = "Save", IsDefault = true };
        save.Click += (_, _) => SaveAndClose();
        var cancel = new Button { Content = "Cancel", IsCancel = true };
        cancel.Click += (_, _) => Close();
        var about = new Button { Content = "About…" };
        about.Click += (_, _) => AboutWindow.Open();
        var buttons = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 6, HorizontalAlignment = HorizontalAlignment.Right, Margin = new Thickness(0, 16, 0, 0) };
        buttons.Children.Add(about);
        buttons.Children.Add(save);
        buttons.Children.Add(cancel);
        form.Children.Add(buttons);

        Content = form;
    }

    static void Section(Panel form, string title) =>
        form.Children.Add(new TextBlock { Text = title, FontWeight = FontWeight.SemiBold, Margin = new Thickness(0, 10, 0, 0) });

    static NumericUpDown Number(int value, int min, int max) =>
        new() { Value = value, Minimum = min, Maximum = max, Increment = 1, FormatString = "0", Width = 110, HorizontalAlignment = HorizontalAlignment.Left };

    static Control Labeled(string label, Control input)
    {
        var row = new StackPanel { Spacing = 2 };
        row.Children.Add(new TextBlock { Text = label, FontSize = 12 });
        row.Children.Add(input);
        return row;
    }

    void Hotkey(Grid grid, string label, string current, Action<string> apply)
    {
        int row = grid.RowDefinitions.Count;
        grid.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
        var text = new TextBlock { Text = label, VerticalAlignment = VerticalAlignment.Center };
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
        Grid.SetRow(text, row);
        Grid.SetRow(box, row);
        Grid.SetColumn(box, 1);
        grid.Children.Add(text);
        grid.Children.Add(box);
        _hotkeys.Add((box, apply));
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
        var seen = new HashSet<string>();
        foreach (var (box, _) in _hotkeys)
        {
            try { if (!seen.Add(HotkeyBinding.Parse(box.Text ?? "").ToString())) throw new FormatException("duplicate"); }
            catch (FormatException) { box.Focus(); box.SelectAll(); return; } // leave the offending box focused
        }
        foreach (var (box, apply) in _hotkeys) apply(box.Text!);
        _settings.SaveFolder = string.IsNullOrWhiteSpace(_folder.Text) ? ImageSaver.DefaultFolder : _folder.Text.Trim();
        _settings.CopyToClipboard = _clipboard.IsChecked == true;
        _settings.TrimOverlap = _trimOverlap.IsChecked == true;
        _settings.CropTop = (int)(_cropTop.Value ?? 0);
        _settings.ScrollDelayMs = (int)(_scrollDelay.Value ?? 400);
        _settings.Save();
        Saved?.Invoke();
        Close();
    }
}
