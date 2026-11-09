# StitchShot

Screenshot tool that stitches several captures into one tall image.

Pick a region once, then press a key each time you want the current content of that
region appended below the previous shot. Works for lists that don't fit on screen,
for long web pages and documents (page down, press, repeat), and for pieces taken from
different places and sizes that belong in a single picture. Finish with another key
and the result opens in the editor or lands in your folder.

Also does the usual: region / window / full screen capture with a frozen screen,
full-width crosshair, pixel magnifier with coordinates and color, and a small
annotation editor (rectangle, ellipse, line, dashed line, arrow, pen, text, blur).

Status: early development, nothing to download yet. See [PLAN.md](PLAN.md). Usage: [docs/HELP.md](docs/HELP.md).

## Platforms

| | Capture | Clipboard | Start at login | Notes |
|---|---|---|---|---|
| Windows | GDI BitBlt, all monitors | native | Run key | the reference platform |
| Linux | X11 (XGetImage), all monitors | `xclip` or `wl-copy` | XDG autostart | Wayland capture not yet; tray needs a StatusNotifier host |
| macOS | `screencapture`, main display | AppleScript pasteboard | LaunchAgent | needs Screen Recording + Accessibility permission |

The UI, hotkeys, stitching and the editor are the same code everywhere (Avalonia,
SkiaSharp, SharpHook). Only `src/StitchShot.Platform.*` differs: one small class
each for capture, clipboard and startup registration. Linux and macOS builds are
produced by CI but have had far less testing than Windows; reports welcome.

## Build

Requires the .NET 10 SDK (the app itself targets .NET 8).

```bash
dotnet build
dotnet test
dotnet run --project src/StitchShot.App
```

Linux needs `libX11` at runtime and `xclip` (X11) or `wl-copy` (Wayland) for the
clipboard. macOS asks for Screen Recording and Accessibility permissions on first use.

`StitchShot --fullscreen` captures the whole desktop to the screenshots folder and the
clipboard without starting the tray app; useful for scripts and smoke tests.

## License

MIT, see [LICENSE](LICENSE).
