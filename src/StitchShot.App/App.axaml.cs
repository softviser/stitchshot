using System;
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
using SkiaSharp;
using StitchShot.Core;
using StitchShot.Platform.Windows;

namespace StitchShot.App;

public partial class App : Application
{
    readonly Settings _settings = Settings.Load();
    HotkeyBinding _regionKey = null!, _fullScreenKey = null!;

    IScreenCapturer _capturer = null!;
    IImageClipboard _clipboard = null!;
    SimpleGlobalHook? _hook;
    TrayIcon? _tray;
    OverlayWindow? _overlay;
    SettingsWindow? _settingsWindow;

    public override void Initialize() => AvaloniaXamlLoader.Load(this);

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime desktop) return;
        Log.Write($"starting, shutdown mode {desktop.ShutdownMode}");
        desktop.Exit += (_, e) => Log.Write($"exit code {e.ApplicationExitCode}");
        (_capturer, _clipboard) = CreatePlatform();
        ApplyHotkeys();

        var menu = new NativeMenu();
        menu.Items.Add(MenuItem("Capture region", () => _ = CaptureRegionAsync()));
        menu.Items.Add(MenuItem("Capture full screen", CaptureFullScreen));
        menu.Items.Add(new NativeMenuItemSeparator());
        menu.Items.Add(MenuItem("Open screenshots folder", () =>
        {
            Directory.CreateDirectory(_settings.SaveFolder);
            Process.Start(new ProcessStartInfo(_settings.SaveFolder) { UseShellExecute = true });
        }));
        menu.Items.Add(MenuItem("Settings…", OpenSettings));
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
        base.OnFrameworkInitializationCompleted();
    }

    public static (IScreenCapturer, IImageClipboard) CreatePlatform()
    {
        if (OperatingSystem.IsWindows()) return (new GdiScreenCapturer(), new WindowsImageClipboard());
        throw new PlatformNotSupportedException("StitchShot currently captures on Windows only; Linux and macOS are planned.");
    }

    void ApplyHotkeys()
    {
        _regionKey = HotkeyBinding.Parse(_settings.RegionHotkey);
        _fullScreenKey = HotkeyBinding.Parse(_settings.FullScreenHotkey);
    }

    void UpdateTooltip(string? status = null)
    {
        if (_tray is null) return;
        _tray.ToolTipText = $"StitchShot\n{_regionKey}: capture region\n{_fullScreenKey}: full screen" + (status is null ? "" : $"\n{status}");
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
        _settingsWindow = new SettingsWindow(_settings);
        _settingsWindow.Saved += () => { ApplyHotkeys(); UpdateTooltip(); };
        _settingsWindow.Closed += (_, _) => _settingsWindow = null;
        _settingsWindow.Show();
    }

    void OnKeyPressed(object? sender, KeyboardHookEventArgs e)
    {
        Action? action = Matches(_regionKey, e) ? () => _ = CaptureRegionAsync()
                       : Matches(_fullScreenKey, e) ? CaptureFullScreen
                       : null;
        if (action is null) return;
        e.SuppressEvent = true;
        // Typing a new hotkey into the settings window must not trigger a capture.
        Dispatcher.UIThread.Post(() => { if (_settingsWindow?.IsActive != true) action(); });
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

    async Task CaptureRegionAsync()
    {
        if (_overlay is not null) return;
        try
        {
            var desktop = _capturer.VirtualDesktop;
            using var frozen = _capturer.Capture(desktop);
            _overlay = new OverlayWindow(frozen, desktop);
            _overlay.Show();
            var result = await _overlay.Result;
            _overlay = null;
            if (result is null) return;
            using var crop = ImageSaver.Crop(frozen, result.Region);
            if (result.Save) SaveAndCopy(crop);
            else { _clipboard.SetImage(crop); UpdateTooltip("Copied to clipboard"); }
        }
        catch (Exception ex) { Report(ex); }
    }

    void CaptureFullScreen()
    {
        try
        {
            using var shot = _capturer.Capture(_capturer.VirtualDesktop);
            SaveAndCopy(shot);
        }
        catch (Exception ex) { Report(ex); }
    }

    public void SaveAndCopy(SKBitmap bitmap)
    {
        var path = ImageSaver.SavePng(bitmap, _settings.SaveFolder, DateTime.Now);
        if (_settings.CopyToClipboard) _clipboard.SetImage(bitmap);
        UpdateTooltip($"Saved {Path.GetFileName(path)}");
    }

    // shortcut: no notification UI yet, so errors land in the tray tooltip and the log; replace when the HUD exists.
    void Report(Exception ex)
    {
        _overlay = null;
        Log.Write(ex.ToString());
        UpdateTooltip($"Error: {ex.Message}");
    }
}
