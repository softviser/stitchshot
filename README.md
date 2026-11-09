<p align="center">
  <picture>
    <source media="(prefers-color-scheme: dark)" srcset="design/logo/dist/stitchshot-horizontal-on-dark.svg">
    <img src="design/logo/dist/stitchshot-horizontal.svg" alt="StitchShot" width="440">
  </picture>
</p>

Screenshot tool that stitches several captures into one tall image.

Pick a region once, then press a key each time you want the current content of that
region appended below the previous shot. Works for lists that don't fit on screen,
for long web pages and documents (page down, press, repeat), and for pieces taken from
different places and sizes that belong in a single picture. Finish with another key
and the result opens in the editor or lands in your folder.

Also does the usual: region / window / full screen capture with a frozen screen,
full-width crosshair, pixel magnifier with coordinates and color, and a small
annotation editor (rectangle, ellipse, line, dashed line, arrow, pen, text, blur).

Website: [svsr.net/stitchshot](https://svsr.net/stitchshot). Usage: [docs/HELP.md](docs/HELP.md). Changes: [CHANGELOG.md](CHANGELOG.md). Roadmap: [PLAN.md](PLAN.md).

## Install

Downloads are on the [Releases](https://github.com/softviser/stitchshot/releases) page.

| Platform | File | Notes |
|---|---|---|
| Windows | `StitchShot-x.y.z-win-x64-setup.exe` | installer: Start Menu entry, optional start at login, uninstaller; installs per user, no admin rights |
| Windows, portable | `StitchShot-x.y.z-win-x64.exe` | one file, run it from anywhere; .NET is bundled |
| Windows, small | `StitchShot-x.y.z-win-x64-requires-net8.exe` | needs the [.NET 8 Desktop Runtime](https://dotnet.microsoft.com/download/dotnet/8.0) |
| Linux | `StitchShot-x.y.z-linux-x64.tar.gz` | extract and run `./StitchShot`; needs `libX11` and `xclip` or `wl-copy` |
| macOS | `StitchShot-x.y.z-osx-arm64.tar.gz` (Apple silicon), `-osx-x64` (Intel) | extract, `xattr -dr com.apple.quarantine StitchShot.app`, open the app; grant Screen Recording and Accessibility when asked, then start it again |

The Windows files are not code-signed yet, so SmartScreen shows a warning the first
time; "More info" > "Run anyway". Every file has a SHA-256 next to it in
`SHA256SUMS.txt` or a `.sha256` file.

### macOS

Pick the file for your chip: `osx-arm64` for Apple silicon (M1 and later), `osx-x64`
for Intel. `uname -m` prints `arm64` or `x86_64` if you are not sure.

```bash
tar xzf StitchShot-x.y.z-osx-arm64.tar.gz
xattr -dr com.apple.quarantine StitchShot.app
mv StitchShot.app /Applications/
open /Applications/StitchShot.app
```

The `xattr` line is needed because the app is not notarised; without it macOS says the
developer cannot be verified. Alternatively open the app once, let macOS refuse, then
use "Open Anyway" under System Settings > Privacy & Security.

First run: the first capture asks for Screen Recording, and the hotkeys need
Accessibility. Both are under System Settings > Privacy & Security; tick StitchShot in
each list, then quit (menu bar icon > Exit) and open it again. Until Accessibility is
granted the app shows "Hotkeys are off" at start; the menu bar menu still works.
StitchShot has no Dock icon; look for it in the menu bar.

Start at login is the checkbox in Settings; it writes a LaunchAgent that points at the
app's current location, so if you move the app later, untick and tick it again.

Update: quit StitchShot, then repeat the four lines above with the new file. The
`mv` replaces the old bundle (answer yes if asked). Settings live in
`~/.config/StitchShot/settings.json` and survive updates. Permissions stay granted as
long as the app keeps its path.

Uninstall: untick start at login, quit, delete `/Applications/StitchShot.app`. To
remove every trace: `rm -rf ~/.config/StitchShot ~/Library/LaunchAgents/com.softviser.stitchshot.plist`.

### Linux

Needs an X11 session (on Wayland, only XWayland windows are captured for now),
`libX11`, and `xclip` (X11) or `wl-copy` (Wayland) for the clipboard. The tray icon
needs a StatusNotifier host: KDE, XFCE, Cinnamon and MATE have one; GNOME needs the
AppIndicator extension.

```bash
# Debian / Ubuntu: sudo apt install libx11-6 xclip
# Fedora:          sudo dnf install libX11 xclip
# Arch:            sudo pacman -S libx11 xclip
mkdir -p ~/.local/bin
tar xzf StitchShot-x.y.z-linux-x64.tar.gz -C ~/.local/bin
~/.local/bin/StitchShot
```

The archive holds the executable and `stitchshot.png`. For an entry in the
application menu, save this as `~/.local/share/applications/stitchshot.desktop`
(adjust the paths if you extracted elsewhere):

```ini
[Desktop Entry]
Type=Application
Name=StitchShot
Comment=Screenshot tool that stitches several captures into one tall image
Exec=/home/you/.local/bin/StitchShot
Icon=/home/you/.local/bin/stitchshot.png
Categories=Utility;Graphics;
StartupNotify=false
```

Start at login is the checkbox in Settings; it writes
`~/.config/autostart/stitchshot.desktop` with the executable's current path, so after
moving the file untick and tick it again.

Update: quit StitchShot (tray icon > Exit), extract the new archive over the old
files with the same `tar` line, start it again. Settings live in
`~/.config/StitchShot/settings.json` and survive updates.

Uninstall: untick start at login, quit, then
`rm ~/.local/bin/StitchShot ~/.local/bin/stitchshot.png ~/.local/share/applications/stitchshot.desktop`
and, to remove the settings, `rm -rf ~/.config/StitchShot`.

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

## Logo

Master files, colours and usage notes are in [design/logo](design/logo/GUIDELINES.md).

## License

MIT, see [LICENSE](LICENSE).
