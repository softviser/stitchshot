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

Status: early development, nothing to download yet. See [PLAN.md](PLAN.md).

## Platforms

Windows first. The app is built with Avalonia and SkiaSharp so the UI and the
stitching code run on Linux and macOS too; only the thin screen-capture layer is
per-OS and those two are not written yet.

## Build

Requires the .NET 10 SDK (the app itself targets .NET 8).

```bash
dotnet build
dotnet test
dotnet run --project src/StitchShot.App
```

`StitchShot --fullscreen` captures the whole desktop to the screenshots folder and the
clipboard without starting the tray app; useful for scripts and smoke tests.

## License

MIT, see [LICENSE](LICENSE).
