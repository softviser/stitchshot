# StitchShot help

StitchShot lives in the tray. Every key below can be changed in Settings;
these are the defaults.

## Capture a region            PrintScreen

The screen freezes and dims. Move the mouse: a crosshair spans the screen and a
loupe shows the pixels under the cursor with X,Y and the hex color.
Drag to select. Then:

  Enter or the save icon     save a PNG to the screenshots folder (and the clipboard)
  Ctrl+C or the copy icon    copy only
  Arrow keys                 move the selection by 1 px, Shift+arrows by 10 px
  Double-click inside        same as Enter
  Esc                        cancel

## Capture the full screen     Ctrl+PrintScreen

Saves every monitor as one image, no selection step.

## Stitch session              Ctrl+Shift+S

One tall image from several captures: lists that need scrolling, long pages,
or pieces from different places.

  1. Press Ctrl+Shift+S, select the region, press Enter. That is frame 1.
     A small bar (the HUD) appears with the frame count and the total size.
  2. Scroll or change what is on screen, then press F8. The same region is
     captured again and appended below the previous frame.
     F10 sends Page Down to the focused window first, waits a moment, then
     captures. Use the key, not the HUD button: clicking the HUD moves focus.
  3. Need a different region or size? Press Ctrl+Shift+S again (or the
     dotted-rectangle icon), select, then the camera icon or Enter adds it as
     the next frame. Narrower frames are left-aligned on a white background.
  4. Press F9 (or the check icon) to save the result and copy it.

Every capture flashes white briefly; a saved file shows a small notification in
the bottom-right corner with a thumbnail. The folder icon opens the folder with the file
selected. Drag the notification anywhere and later ones appear there; it stays while
the pointer is over it.

HUD icons, left to right: capture the same region again, Page Down + capture,
remove the last frame, select a new region, finish and save, discard the session.
Drag the HUD anywhere; it remembers its place. Keep it outside the region
you capture or it ends up in the picture.

### Scrolled pages

Page Down scrolls a little less than a screen, so frames overlap. With
"Trim the overlap" on (default), the overlapping rows are detected and removed.
Sticky headers break this: set "Crop this many pixels from the top of frames 2+"
to the header height so frames 2+ start below it.
"Wait after Page Down" is for pages with smooth scrolling; raise it if frames
come out mid-scroll.

## Editor

Press E in the overlay (or the pencil icon) instead of Enter, or click the pencil
on a "Saved" notification, to annotate before saving.

  R rectangle   O ellipse   L line   D dashed line   A arrow   P pen
  T text        X pixelate  C crop
  Shift while dragging keeps squares and circles and snaps lines to 45 degrees.
  Text: click where it starts, type, Enter places it (Shift+Enter = new line).
  Crop: drag the part to keep; everything outside is dropped on export.
  Ctrl+Z / Ctrl+Y undo and redo, Ctrl+S saves and closes, Ctrl+C copies, Esc closes.

The colour swatches and the stroke width apply to the next mark; text size is six
times the stroke width.

## Settings

  Start with Windows       registers StitchShot in the per-user startup list.
                           Asked once on the first run, changeable here any time.
  Save screenshots to      folder for PNGs. Files are named StitchShot_date_time.png.
  Copy to clipboard        also put every capture on the clipboard.
  Hotkeys                  click a box, press the combination. Duplicates are refused
                           and the box stays focused until you change it.
  Stitching                overlap trim, top crop, Page Down delay (see above).

Settings are stored in %AppData%\StitchShot\settings.json.
If something goes wrong, the tray tooltip shows the error and details are in
%TEMP%\StitchShot.log.

## Tips

  - PrintScreen does nothing? Windows Snipping Tool may own it:
    Settings > Accessibility > Keyboard > "Use the Print screen key to open
    screen capture", turn it off, or pick another key in StitchShot settings.
  - Capturing a menu or tooltip: open it, then press the hotkey. The screen
    is frozen before the overlay appears, so it stays in the picture.
  - Content that comes out black (video, protected windows) is a limit of the
    capture method used now.
