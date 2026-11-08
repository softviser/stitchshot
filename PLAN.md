# StitchShot – plan

## Why

Existing tools capture one rectangle. StitchShot captures the same rectangle (or a new
one) as many times as you like and stacks the results, so a list that needs ten
scrolls becomes one image. Everything else in the app exists to make that pleasant.

## Stack

| Piece | Choice | Why |
|---|---|---|
| Language / runtime | C# on .NET 8 | Same as the rest of SVAPPS |
| UI | Avalonia | One UI for Windows, Linux, macOS; has tray icons |
| Images | SkiaSharp | Cross-platform drawing and encoding; System.Drawing is Windows-only |
| Global hotkeys | SharpHook (libuiohook) | Cross-platform |
| Screen capture | per-OS P/Invoke | Windows: BitBlt. Linux: X11 XGetImage, Wayland via portal later. macOS: CGWindowListCreateImage |

## Layout

```
src/StitchShot.Core              pure logic: stitching, overlap detection, annotation model, settings, filename template
src/StitchShot.App               Avalonia: tray, capture overlay, editor, HUD, settings window
src/StitchShot.Platform.Windows  IScreenCapturer, active-window rect, clipboard bitmap, start-with-Windows
src/StitchShot.Platform.Linux    stub in v1
src/StitchShot.Platform.Mac      stub in v1
tests/StitchShot.Core.Tests      xUnit; stitching, overlap, template are the parts worth testing
```

Core must not reference Avalonia or any platform project.

## v1.0 (Windows release)

### Capture
- Hotkey grabs the whole virtual desktop into a bitmap and shows it in a full-screen
  overlay, so menus and tooltips stay put while you select.
- Crosshair across the full width and height of the screen at the cursor.
- Magnifier next to the cursor: zoomed pixels, X,Y, hex color. While dragging, a
  second magnifier stays pinned on the start corner.
- Live W×H readout. Arrows nudge 1 px, Shift+arrows 10 px, Space moves the
  selection, Esc cancels, Enter confirms.
- Modes: region, full screen, current monitor, active window, repeat last region.
- Ctrl+C in the overlay copies the color under the cursor.
- Multi-monitor, per-monitor DPI, coordinates in physical pixels.

### Stitch session
- Start: select a region. A small HUD appears with frame count, total size and the
  active keys.
- Capture key: grab the same rectangle from the live screen (no overlay) and append
  it below the previous frame. Horizontal direction is a setting.
- Re-select region mid-session. Different widths: canvas grows to the widest frame,
  align left or center, gaps filled with the background color.
- Auto-overlap trim (toggle): compare the top rows of the new frame with the bottom
  rows of the previous one, drop the overlap. Needed because Page Down scrolls less
  than a full viewport.
- "Crop top N px on frames 2+" for sticky headers. Automatic detection is v1.1.
- Helper key: send Page Down, wait a moment, capture.
- Undo last frame, Finish (opens editor or saves straight away), Cancel.

### Editor
- Rectangle, ellipse (Shift for circle/square), line, dashed line, arrow, pen, text,
  blur/pixelate, crop.
- Color palette, stroke width, undo/redo. Shapes stay vector until export so they can
  be moved or deleted.
- Ctrl+S save, Ctrl+C copy, Save As, open folder. Filename template
  `{app}_{yyyy-MM-dd_HH-mm-ss}`. PNG, JPG, WebP.
- After-capture action: open editor, save silently, copy only.

### Hotkeys and HUD
- Every key rebindable from Settings with a press-to-record control and conflict
  warning.
- Defaults: PrintScreen region, Ctrl+PrtSc full screen, Alt+PrtSc active window,
  Ctrl+Shift+S start stitch, F8 stitch capture, F9 finish, Ctrl+Shift+R repeat
  last region.
- If PrintScreen can't be registered (Windows Snipping Tool owns it), say so and
  point at the Windows setting.
- HUD: frameless, always on top, draggable, remembers position, optional
  click-through. Hidden outside a session unless "always show" is on.

### Settings and distribution
- Save folder, format and quality, stitch options, hotkeys, start with Windows,
  light/dark follows the OS.
- JSON in the per-user app data folder; a `portable` file next to the exe moves it
  beside the exe.
- Self-contained single-file exe and a small framework-dependent exe, GitHub
  Releases, SignPath signing via the same workflow Port Killer uses, winget after 1.0.

## Later

- Screen recording: full screen, region, window. Pipe frames to ffmpeg so one path
  covers all three OSes; MP4, GIF, WebM. Microphone and webcam corner afterwards.
- Automatic scroll-stitch: scroll and capture until two frames are identical.
  Automatic sticky-header detection.
- Pin screenshot as a floating window, history gallery, OCR, uploads (Imgur, S3,
  custom endpoint), CLI.
- Official Linux (incl. Wayland portal) and macOS builds.

## Milestones

1. Region capture, frozen overlay, crosshair and magnifier, save/copy, tray, hotkeys.
   Runs on Windows.
2. Stitch session, HUD, overlap trim, Page Down helper.
3. Editor tools.
4. Settings window, hotkey editor, packaging, CI, README with screenshots. Tag v1.0.

## Known risks

- Avalonia has no cross-platform "put a bitmap on the clipboard"; Windows gets Win32
  code, the others follow with their capturers.
- Wayland only allows repeated silent capture through the portal with a restore
  token; macOS needs Screen Recording and Accessibility permissions.
- BitBlt returns black for some DRM / hardware-overlay content. Windows.Graphics.Capture
  can replace it later.
