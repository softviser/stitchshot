using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using StitchShot.Core;

namespace StitchShot.App;

/// <summary>Small always-on-top strip shown during a stitch session: frame count, size and the session actions.</summary>
public sealed class HudWindow : Window
{
    readonly Settings _settings;
    readonly TextBlock _status = new() { Foreground = Brushes.White, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(8, 0, 6, 0) };

    public HudWindow(Settings settings, Action capture, Action scrollCapture, Action undo, Action reselect, Action finish, Action cancel)
    {
        _settings = settings;
        WindowDecorations = WindowDecorations.None;
        Topmost = true;
        ShowInTaskbar = false;
        ShowActivated = false; // keep the keyboard in the app being captured
        CanResize = false;
        SizeToContent = SizeToContent.WidthAndHeight;
        Background = Brushes.Transparent;
        TransparencyLevelHint = new[] { WindowTransparencyLevel.Transparent };

        var row = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 2 };
        row.Children.Add(new PathIcon { Data = Geometry.Parse(IconButton.Layers), Width = 14, Height = 14, Foreground = Brushes.White, Margin = new Thickness(8, 0, 0, 0) });
        row.Children.Add(_status);
        row.Children.Add(IconButton.Create(IconButton.Camera, $"Capture the region again ({settings.StitchCaptureHotkey})", capture));
        row.Children.Add(IconButton.Create(IconButton.PageDown, $"Send Page Down, then capture ({settings.StitchScrollCaptureHotkey}). Use the hotkey while the page has focus.", scrollCapture));
        row.Children.Add(IconButton.Create(IconButton.Undo, "Remove the last frame", undo));
        row.Children.Add(IconButton.Create(IconButton.Select, $"Select a different region and add it ({settings.StitchStartHotkey})", reselect));
        row.Children.Add(IconButton.Create(IconButton.Check, $"Finish and save ({settings.StitchFinishHotkey})", finish));
        row.Children.Add(IconButton.Create(IconButton.Close, "Discard the session", cancel));

        var panel = new Border
        {
            Background = IconButton.PanelBrush,
            CornerRadius = new CornerRadius(6),
            Padding = new Thickness(2),
            Child = row,
            Cursor = new Cursor(StandardCursorType.SizeAll),
        };
        panel.PointerPressed += (_, e) => { if (e.GetCurrentPoint(panel).Properties.IsLeftButtonPressed) BeginMoveDrag(e); };
        Content = panel;

        if (settings.HudX is { } x && settings.HudY is { } y)
        {
            WindowStartupLocation = WindowStartupLocation.Manual;
            Position = new PixelPoint(x, y);
        }
    }

    public void Update(StitchComposer composer) =>
        _status.Text = composer.Count == 1 ? $"1 frame · {composer.Width} × {composer.Height}" : $"{composer.Count} frames · {composer.Width} × {composer.Height}";

    /// <summary>Where the HUD sits on screen, in physical pixels, so captures can dodge it.</summary>
    public ScreenRect ScreenBounds => new(Position.X, Position.Y, (int)(Bounds.Width * RenderScaling), (int)(Bounds.Height * RenderScaling));

    protected override void OnOpened(EventArgs e)
    {
        base.OnOpened(e);
        if (_settings.HudX is null && Screens.Primary is { } screen)
        {
            var area = screen.WorkingArea;
            Position = new PixelPoint(area.Right - ScreenBounds.Width - 16, area.Bottom - ScreenBounds.Height - 16);
        }
    }

    protected override void OnClosing(WindowClosingEventArgs e)
    {
        (_settings.HudX, _settings.HudY) = (Position.X, Position.Y);
        try { _settings.Save(); } catch (Exception ex) { Log.Write(ex.ToString()); }
        base.OnClosing(e);
    }
}
