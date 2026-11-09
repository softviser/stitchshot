using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using SharpHook;
using SharpHook.Data;
using SharpHook.Simulation;
using SkiaSharp;
using StitchShot.Core;
using StitchShot.Platform.Linux;
using StitchShot.Platform.Mac;
using StitchShot.Platform.Windows;

namespace StitchShot.App;

public partial class App : Application
{
    readonly bool _firstRun = !File.Exists(Settings.DefaultPath);
    readonly Settings _settings = Settings.Load();
    readonly List<(HotkeyBinding Key, Action Action)> _hotkeys = new();

    IScreenCapturer _capturer = null!;
    IImageClipboard _clipboard = null!;
    IStartupRegistration _startup = null!;
    SimpleGlobalHook? _hook;
    readonly EventSimulator _simulator = EventSimulator.Create("StitchShot");
    TrayIcon? _tray;
    OverlayWindow? _overlay;
    SettingsWindow? _settingsWindow;

    // Stitch session: the region in absolute screen pixels, the frames collected so far, and the HUD.
    ScreenRect _stitchRegion;
    StitchComposer? _stitch;
    HudWindow? _hud;

    public override void Initialize() => AvaloniaXamlLoader.Load(this);

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime desktop) return;
        Log.Write($"starting, shutdown mode {desktop.ShutdownMode}");
        desktop.Exit += (_, e) => Log.Write($"exit code {e.ApplicationExitCode}");
        (_capturer, _clipboard, _startup) = CreatePlatform();
        ToastWindow.Settings = _settings;
        ApplyHotkeys();

        var menu = new NativeMenu();
        menu.Items.Add(MenuItem("Capture region", () => _ = CaptureRegionAsync()));
        menu.Items.Add(MenuItem("Capture full screen", CaptureFullScreen));
        menu.Items.Add(MenuItem("Start stitch session", () => _ = StartStitchAsync()));
        menu.Items.Add(new NativeMenuItemSeparator());
        menu.Items.Add(MenuItem("Open screenshots folder", () => Shell.OpenFolder(_settings.SaveFolder)));
        menu.Items.Add(MenuItem("Settings…", OpenSettings));
        menu.Items.Add(MenuItem("Help", HelpWindow.Open));
        menu.Items.Add(MenuItem("About StitchShot", AboutWindow.Open));
        menu.Items.Add(new NativeMenuItemSeparator());
        menu.Items.Add(MenuItem("Exit", () => { _hook?.Dispose(); desktop.Shutdown(); }));

        _tray = new TrayIcon { Icon = TrayIconImage.Create(), Menu = menu };
        _tray.Clicked += (_, _) => _ = CaptureRegionAsync();
        TrayIcon.SetIcons(this, new TrayIcons { _tray });
        UpdateTooltip();

        _hook = new SimpleGlobalHook();
        _hook.KeyPressed += OnKeyPressed;
        _hook.RunAsync(GlobalHookType.Keyboard).ContinueWith(t => Log.Write($"hook stopped: {t.Exception?.GetBaseException().Message ?? "ok"}"));
        Log.Write("tray and hook up");
        if (_firstRun)
        {
            if (_startup.Enabled || _startup.ExternalSettingsUri is not null) _settings.Save(); // the installer already asked, or Windows owns the setting; just mark the first run done
            else new FirstRunWindow(_settings, _startup).Show();
        }
        base.OnFrameworkInitializationCompleted();
    }

    public static (IScreenCapturer, IImageClipboard, IStartupRegistration) CreatePlatform()
    {
        if (OperatingSystem.IsWindows()) return (new GdiScreenCapturer(), new WindowsImageClipboard(), PackagedStartupRegistration.IsPackaged() ? new PackagedStartupRegistration() : new WindowsStartupRegistration());
        if (OperatingSystem.IsLinux()) return (new X11ScreenCapturer(), new LinuxImageClipboard(), new LinuxStartupRegistration());
        if (OperatingSystem.IsMacOS()) return (new MacScreenCapturer(), new MacImageClipboard(), new MacStartupRegistration());
        throw new PlatformNotSupportedException("StitchShot runs on Windows, Linux (X11) and macOS.");
    }

    void ApplyHotkeys()
    {
        _hotkeys.Clear();
        _hotkeys.Add((HotkeyBinding.Parse(_settings.RegionHotkey), () => _ = CaptureRegionAsync()));
        _hotkeys.Add((HotkeyBinding.Parse(_settings.FullScreenHotkey), CaptureFullScreen));
        _hotkeys.Add((HotkeyBinding.Parse(_settings.StitchStartHotkey), () => _ = StartStitchAsync()));
        _hotkeys.Add((HotkeyBinding.Parse(_settings.StitchCaptureHotkey), StitchCapture));
        _hotkeys.Add((HotkeyBinding.Parse(_settings.StitchScrollCaptureHotkey), () => _ = StitchScrollCaptureAsync()));
        _hotkeys.Add((HotkeyBinding.Parse(_settings.StitchFinishHotkey), StitchFinish));
    }

    void UpdateTooltip()
    {
        if (_tray is null) return;
        _tray.ToolTipText = $"StitchShot\n{_settings.RegionHotkey}: capture region\n{_settings.FullScreenHotkey}: full screen\n{_settings.StitchStartHotkey}: start stitch session";
    }

    static NativeMenuItem MenuItem(string header, Action action)
    {
        var item = new NativeMenuItem(header);
        item.Click += (_, _) => action();
        return item;
    }

    void OpenSettings()
    {
        if (_settingsWindow is not null) { _settingsWindow.Activate(); return; }
        _settingsWindow = new SettingsWindow(_settings, _startup);
        _settingsWindow.Saved += () => { ApplyHotkeys(); UpdateTooltip(); };
        _settingsWindow.Closed += (_, _) => _settingsWindow = null;
        _settingsWindow.Show();
    }

    void OnKeyPressed(object? sender, KeyboardHookEventArgs e)
    {
        if (e.IsEventSimulated) return; // our own Page Down
        foreach (var (key, action) in _hotkeys)
        {
            if (!Matches(key, e)) continue;
            e.SuppressEvent = true;
            // Typing a new hotkey into the settings window must not trigger a capture.
            Dispatcher.UIThread.Post(() => { if (_settingsWindow?.IsActive != true) action(); });
            return;
        }
    }

    static bool Matches(HotkeyBinding b, KeyboardHookEventArgs e)
    {
        if (!Enum.TryParse<KeyCode>("Vc" + b.Key, out var code) || e.Data.KeyCode != code) return false;
        var m = e.RawEvent.Mask;
        return (m & EventMask.Ctrl) != 0 == b.Ctrl
            && (m & EventMask.Shift) != 0 == b.Shift
            && (m & EventMask.Alt) != 0 == b.Alt
            && (m & EventMask.Meta) != 0 == b.Win;
    }

    /// <summary>Freezes the screen and lets the user pick a region. Returns the frozen desktop and the choice (null when cancelled).</summary>
    async Task<(SKBitmap Frozen, ScreenRect Desktop, CaptureResult? Result)?> PickRegionAsync(bool stitchMode)
    {
        if (_overlay is not null) return null;
        var desktop = _capturer.VirtualDesktop;
        var frozen = _capturer.Capture(desktop);
        try
        {
            _overlay = new OverlayWindow(frozen, desktop, stitchMode, sessionRunning: _stitch is not null);
            _overlay.Show();
            return (frozen, desktop, await _overlay.Result);
        }
        finally { _overlay = null; }
    }

    async Task CaptureRegionAsync()
    {
        try
        {
            if (await PickRegionAsync(stitchMode: false) is not { } pick) return;
            using var frozen = pick.Frozen;
            if (pick.Result is not { } result) return;
            var r = result.Region;
            using var crop = ImageSaver.Crop(frozen, r);
            FlashWindow.Show(new ScreenRect(pick.Desktop.X + r.X, pick.Desktop.Y + r.Y, r.Width, r.Height));
            switch (result.Action)
            {
                case CaptureAction.Save: SaveAndCopy(crop); break;
                case CaptureAction.Edit: OpenEditor(crop.Copy()); break;
                default: CopyOnly(crop); break;
            }
        }
        catch (Exception ex) { Report(ex); }
    }

    void CaptureFullScreen()
    {
        try
        {
            using var shot = _capturer.Capture(_capturer.VirtualDesktop);
            FlashWindow.Show(_capturer.VirtualDesktop);
            SaveAndCopy(shot);
        }
        catch (Exception ex) { Report(ex); }
    }

    // ---- stitch session ----

    /// <summary>Starts a session, or when one is running, lets the user pick a new region and adds it as the next frame.</summary>
    async Task StartStitchAsync()
    {
        try
        {
            if (await PickRegionAsync(stitchMode: true) is not { } pick) return;
            using var frozen = pick.Frozen;
            if (pick.Result is not { } result) return;
            var r = result.Region;
            _stitchRegion = new ScreenRect(pick.Desktop.X + r.X, pick.Desktop.Y + r.Y, r.Width, r.Height);
            if (_stitch is null)
            {
                _stitch = new StitchComposer { TrimOverlap = _settings.TrimOverlap, CropTop = _settings.CropTop };
                _hud = new HudWindow(_settings, StitchCapture, () => _ = StitchScrollCaptureAsync(), StitchUndo,
                    () => _ = StartStitchAsync(), StitchFinish, StitchCancel);
                _hud.Show();
            }
            _stitch.Add(ImageSaver.Crop(frozen, r));
            _hud!.Update(_stitch);
            FlashWindow.Show(_stitchRegion);
        }
        catch (Exception ex) { Report(ex); }
    }

    void StitchCapture()
    {
        if (_stitch is null) { ToastWindow.Show("No stitch session", $"Start one with {_settings.StitchStartHotkey} or from the tray menu."); return; }
        try
        {
            _stitch.Add(_capturer.Capture(_stitchRegion));
            _hud?.Update(_stitch);
            FlashWindow.Show(_stitchRegion);
        }
        catch (Exception ex) { Report(ex); }
    }

    async Task StitchScrollCaptureAsync()
    {
        if (_stitch is null) { StitchCapture(); return; }
        _simulator.SimulateKeyPress(KeyCode.VcPageDown);
        _simulator.SimulateKeyRelease(KeyCode.VcPageDown);
        await Task.Delay(_settings.ScrollDelayMs); // let smooth scrolling settle
        StitchCapture();
    }

    void StitchUndo()
    {
        if (_stitch is null) return;
        _stitch.Undo();
        _hud?.Update(_stitch);
    }

    void StitchFinish()
    {
        if (_stitch is null) return;
        try
        {
            if (_stitch.Count > 0)
            {
                using var image = _stitch.Compose();
                SaveAndCopy(image);
            }
        }
        catch (Exception ex) { Report(ex); }
        finally { StitchCancel(); }
    }

    void StitchCancel()
    {
        _stitch?.Dispose();
        _stitch = null;
        _hud?.Close();
        _hud = null;
    }

    // ---- output ----

    void OpenEditor(SKBitmap image)
    {
        // The editor's save always copies too, whatever the capture setting says: an edited image is about to be pasted somewhere.
        var editor = new EditorWindow(image, img => { SaveAndCopy(img); if (!_settings.CopyToClipboard) _clipboard.SetImage(img); }, CopyOnly);
        editor.Closed += (_, _) => image.Dispose();
        editor.Show();
    }

    void CopyOnly(SKBitmap bitmap)
    {
        _clipboard.SetImage(bitmap);
        ToastWindow.Show("Copied to clipboard", "Paste it with Ctrl+V. Nothing was saved to disk.", bitmap);
    }

    public void SaveAndCopy(SKBitmap bitmap)
    {
        var path = ImageSaver.SavePng(bitmap, _settings.SaveFolder, DateTime.Now);
        if (_settings.CopyToClipboard) _clipboard.SetImage(bitmap);
        ToastWindow.Show($"Saved {Path.GetFileName(path)}",
            (_settings.CopyToClipboard ? "Also copied to the clipboard. " : "") + "Click to show it in the folder.", bitmap, path,
            edit: () => { if (SKBitmap.Decode(path) is { } img) OpenEditor(img); });
    }

    void Report(Exception ex)
    {
        Log.Write(ex.ToString());
        ToastWindow.Show("Something went wrong", $"{ex.Message}\nDetails: {Log.Path}");
    }
}
