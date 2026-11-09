# StitchShot help

StitchShot is a screenshot tool with one unusual trick: it can take several
captures and stack them into a single tall image. It lives in the system tray,
waits for its hotkeys, and stays out of the way.

Every key named below is the default. All of them can be changed in Settings.

  Default keys               Windows                 Linux / macOS
  Capture a region           PrintScreen             Ctrl+Shift+4
  Capture the full screen    Ctrl+PrintScreen        Ctrl+Shift+3
  Start stitch / add region  Ctrl+Shift+S            Ctrl+Shift+S
  Stitch: capture frame      F8                      F8
  Stitch: Page Down + frame  F10                     F10
  Stitch: finish and save    F9                      F9


## 1. Getting started

The first time StitchShot runs it asks whether to start at login. Say no if you
are only trying it; the option is in Settings whenever you change your mind.

The tray icon (blue tile with three bars) is the home base:

  left click          capture a region
  right click         menu: capture, stitch, open the screenshots folder,
                      Settings, Help, About, Exit

Hover over the icon to see the current hotkeys. Screenshots go to
Pictures/StitchShot as PNG files named StitchShot_2026-03-14_09-41-07.png, and
every capture is also placed on the clipboard, so Ctrl+V pastes it straight into
a chat, document or mail.


## 2. Capturing a region

Press the region hotkey. The screen freezes and dims; whatever was on screen,
including open menus and tooltips, stays exactly as it was.

While you move the mouse:

  - a crosshair spans the whole screen so edges are easy to line up,
  - a loupe next to the cursor shows the pixels under it, magnified, with the
    X,Y position and the colour as #RRGGBB. The loupe can be turned off in
    Settings; the position and colour label stays.

Drag to select. While dragging, a second loupe stays pinned on the starting
corner so both ends can be placed precisely, and the size in pixels is shown
above the selection. When the mouse button is released:

  Enter or the save icon     save a PNG and copy it to the clipboard
  Ctrl+C or the copy icon    copy only, nothing written to disk
  E or the pencil icon       open the selection in the editor (section 5)
  Arrow keys                 move the selection by 1 px
  Shift + arrow keys         move it by 10 px
  Double-click inside        same as Enter
  Esc or the X icon          cancel

The icons appear just below the selection, or above or inside it when there is
no room. Hover over any icon for its name and key.

Capturing a menu or a tooltip: open it first, then press the hotkey. The screen
is frozen before the overlay appears, so the menu is in the picture.


## 3. Capturing the full screen

Press the full-screen hotkey. Every monitor is saved as one image, no selection
step. Useful as a quick "save everything" and for scripts:

  StitchShot --fullscreen

saves the whole desktop to the screenshots folder, copies it, prints the file
name and exits without starting the tray app.


## 4. Stitching

Stitching builds one tall image from several captures. Three typical uses:

  A list longer than the screen       capture, scroll, capture, scroll ...
  A long web page or document         let StitchShot press Page Down for you
  Pieces from different places        a toolbar here, a dialog there, one image

### 4.1 The session

  1. Press Ctrl+Shift+S. Select the region as in section 2 and press Enter.
     That is frame 1. A small dark bar, the HUD, appears in the bottom-right
     corner showing the frame count and the total size so far.
  2. Change what is on screen: scroll the list, open the next page, move to the
     other window. Press F8. The same region is captured from the live screen
     (no overlay this time) and appended below the previous frame. The region
     flashes white to confirm.
  3. Repeat as often as needed. The HUD counter goes up each time.
  4. Press F9. The frames are joined, saved as one PNG and copied.

The HUD icons, left to right:

  camera              capture the same region again (F8)
  double chevron      send Page Down, wait, capture (F10)
  curved arrow        remove the last frame
  dotted rectangle    select a different region and add it (Ctrl+Shift+S)
  check mark          finish and save (F9)
  X                   discard the whole session

Drag the HUD by its body to any place; it remembers the position. Keep it
outside the region you capture, or it will appear in the frames.

### 4.2 Long pages: F10 and overlap trimming

Scrolling by hand is fine, but for a long page let StitchShot do it: click into
the page so it has keyboard focus, then press F10 repeatedly. Each press sends
Page Down, waits a moment (400 ms by default, adjustable in Settings) and
captures.

Page Down scrolls slightly less than one screen, so consecutive frames share a
band of identical rows. With "Trim the overlap" on (the default) StitchShot
finds that band and drops it, so text is not duplicated at the seams.

Sticky headers and toolbars that stay on screen while the page scrolls confuse
overlap detection. Measure the header height (the loupe shows Y) and enter it
under "Crop this many pixels from the top of frames 2+". The first frame keeps
the header, the following frames start below it.

Use the F10 key, not the HUD button: clicking the HUD moves keyboard focus away
from the page, so the Page Down would go nowhere.

### 4.3 Pieces of different sizes

While a session is running, press Ctrl+Shift+S again (or the dotted-rectangle
icon). Select a new region anywhere, any size, and the camera icon or Enter adds
it as the next frame. The result is as wide as the widest frame; narrower frames
are left-aligned and the rest of the row is filled white.

### 4.4 Fixing mistakes

The curved-arrow icon removes the last frame; press it again to go further back.
The X icon throws the session away. After saving, the pencil icon on the
notification opens the result in the editor for cropping or annotating.


## 5. Editor

Open it with E in the overlay, the pencil icon there, or the pencil icon on a
"Saved" notification. The toolbar, left to right:

  Tools (key)         R rectangle   O ellipse    L line    D dashed line
                      A arrow       P pen        T text    X pixelate
                      C crop
  Colours             eight swatches; the ring marks the current one
  Stroke              three dots: thin 2 px, medium 4 px, thick 8 px
  Undo / redo         Ctrl+Z / Ctrl+Y
  Save                write a PNG and copy it to the clipboard (Ctrl+S)
  Copy                clipboard only (Ctrl+C)

Drawing:

  - Drag to draw a shape. Hold Shift for a square or circle, or to snap a line
    to 45-degree steps.
  - Text: click where the text should start, type, press Enter to place it.
    Shift+Enter starts a new line, Esc discards. Text size is six times the
    stroke width (12, 24 or 48 px).
  - Pixelate hides what is under the rectangle, for passwords, names and faces.
    The blocks are 10 px; nothing of the original survives inside the box.
  - Crop: drag the part to keep. The rest is dimmed and removed on save. Undo
    brings the full image back.

Shapes are kept as data until you save, so Undo removes them cleanly. Moving or
editing an existing shape is not possible yet; undo and redraw instead.

Esc closes the editor without saving.


## 6. Notifications

Every capture flashes the captured area white for a moment, so you know the key
was received even when nothing else changes on screen.

When a file is saved, a small dark notification appears in a corner of the
monitor the capture was on (top-right by default; Settings has the four
corners), with a thumbnail and the file name. It has a folder icon that opens
the file's folder with the file selected, a pencil icon that opens the image in
the editor, and an X. It closes by itself after six seconds, but stays while the
mouse is over it, and it can be dragged anywhere; later notifications then
appear where you left it, until a corner is chosen again in Settings.

Errors use the same notification and name the log file.


## 7. Settings

Right-click the tray icon and choose Settings.

  Save screenshots to        folder for PNGs; Browse opens a folder picker
  Copy to clipboard          put every capture on the clipboard as well
  Start at login             register StitchShot to start with the OS
                             (Run key on Windows, autostart entry on Linux,
                             LaunchAgent on macOS). A packaged (MSIX)
                             install shows a link instead: Windows owns the
                             setting, under Settings > Apps > Startup
  Show the loupe             the magnified pixels next to the cursor while
                             selecting; off leaves the crosshair and the
                             position/colour label
  Notification position      top right (default), top left, bottom right,
                             bottom left, or where you last dragged it
  Hotkeys                    click a box, press the combination you want.
                             Duplicates are refused and the box keeps focus
                             until it is changed
  Trim the overlap           see 4.2
  Crop pixels from the top   sticky header height, frames 2+, see 4.2
  Wait after Page Down       milliseconds between Page Down and the capture;
                             raise it on pages with slow or smooth scrolling

Settings live in a JSON file:

  Windows   %AppData%\StitchShot\settings.json
  Linux     ~/.config/StitchShot/settings.json
  macOS     ~/.config/StitchShot/settings.json

Deleting the file restores every default and brings back the first-run question.


## 8. Platform notes

### Windows

If PrintScreen does nothing, Windows' own Snipping Tool probably owns the key:
Settings > Accessibility > Keyboard > "Use the Print screen key to open screen
capture", turn it off. Or pick another key in StitchShot's Settings.

Video players and some protected windows capture as black. That is a limit of
the capture method used for now.

### Linux

  - Install: extract the archive somewhere on your PATH, for example
    ~/.local/bin, and run StitchShot. The README has a desktop entry for the
    application menu. To update, quit (tray > Exit) and extract the new archive
    over the old files; settings are kept.
  - Capture works on X11 sessions. On Wayland only X11 (XWayland) windows can
    be seen; log in with an X11 session for now.
  - Copying needs xclip (X11) or wl-copy (Wayland) installed; the error
    message says which one is missing.
  - The tray icon needs a StatusNotifier host. GNOME users need the AppIndicator
    extension; KDE, XFCE and most others work out of the box.
  - Hotkeys are read through libuiohook. The key still reaches the desktop,
    so avoid combinations your desktop already uses.

### macOS

  - Install: extract the archive, run `xattr -dr com.apple.quarantine
    StitchShot.app`, move the app to /Applications and open it. To update, quit
    (menu bar icon > Exit) and replace the app with the new one; settings and
    permissions are kept as long as the path stays the same.
  - The first capture triggers the Screen Recording permission dialog, and the
    hotkeys need Accessibility permission. Both are under System Settings >
    Privacy & Security. Restart StitchShot after granting. Without Accessibility
    a notification says "Hotkeys are off" at start; the tray menu still works.
  - Run StitchShot.app from the Finder. Started from a terminal, macOS asks the
    permissions for the terminal instead of for StitchShot.
  - StitchShot has no Dock icon; it lives in the menu bar.
  - Captures cover the main display. Secondary displays come later.
  - The download is not notarised. If macOS refuses to open it, run
    `xattr -dr com.apple.quarantine StitchShot.app` once, or use "Open Anyway"
    under System Settings > Privacy & Security right after the refusal.


## 9. Troubleshooting

  Nothing happens on the hotkey      another app owns the key; change it in
                                     Settings. On macOS grant Accessibility.
  Capture is black or wrong screen   see the platform notes above.
  Frames overlap or repeat           turn "Trim the overlap" on; raise the
                                     Page Down delay; set the header crop.
  Header appears in every frame      set "Crop pixels from the top".
  HUD is in the picture              drag it outside the region.
  Something else went wrong          the notification shows the message;
                                     details are in the log file:
                                       Windows  %TEMP%\StitchShot.log
                                       Linux    /tmp/StitchShot.log
                                       macOS    $TMPDIR/StitchShot.log

"About StitchShot" in the tray menu (or the About button in Settings) shows the
version, the log file location, and links to the website (svsr.net/stitchshot)
and the GitHub project, where issues can be reported.
