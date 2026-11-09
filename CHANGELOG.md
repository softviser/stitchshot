# Changelog

All notable changes to StitchShot. Dates are release dates; downloads are on the
[releases page](https://github.com/softviser/stitchshot/releases).

## 1.0.2 – 2026-11-09

### Fixed
- macOS: when the system refuses the keyboard hook (no Accessibility permission),
  a notification now says so at start instead of the hotkeys silently doing nothing.
- The stitch HUD and notifications ignore a remembered position that is no longer on
  any screen, and the HUD logs where it was placed.

### Changed
- macOS downloads are now an app bundle, `StitchShot.app`, with an icon and no Dock
  icon. Permissions attach to StitchShot itself rather than to the terminal it was
  started from.
- The "Saved" notification appears in the top-right corner of the monitor the capture
  was taken on. Dragging it still moves later notifications there.

## 1.0.1 – 2026-11-09

### Fixed
- macOS: the app crashed at start while adding the menu bar icon (the tooltip was
  set after the icon was attached; the macOS backend rejects an empty one).

### Added
- About window (tray menu > About StitchShot, and an About button in Settings) with
  version, links to the website, the source and the issue tracker, and the log path.
- New logo and app icon ("Seam"); tray, window icons and the installer use it.
- Website: svsr.net/stitchshot, with downloads, release notes and the manual.

### Changed
- Start at login: when running from an MSIX package, the Settings window links to
  Windows' own Startup apps page instead of showing a checkbox, since packaged apps
  cannot register themselves in the Run key.

## 1.0.0 – 2026-11-09

First release.

- Stitching: select a region once, then press a key each time you want the current
  content of that region appended below the previous shot. Page Down + capture in one
  key for long pages, overlap trimming, sticky-header crop, regions of different sizes
  in one session, undo the last frame.
- Region capture on a frozen screen with a full-width crosshair and a magnifier showing
  coordinates and the pixel colour. Full-screen capture of all monitors, also from the
  command line with `--fullscreen`.
- Editor: rectangle, ellipse, line, dashed line, arrow, pen, text, pixelate, crop;
  undo/redo; save writes a PNG and copies it to the clipboard.
- Every hotkey is user-definable. Captures flash on screen and a small notification
  shows the saved file with buttons to reveal it or open it in the editor.
- Built-in help (tray menu > Help) and an option to start at login.
- Windows installer (per user, no admin), portable executable, and a smaller build for
  machines with .NET 8. Linux (X11) and macOS builds from the same code; both have
  had far less testing than Windows.
