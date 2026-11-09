# Screenshots

Produces the pictures in `docs/screenshots/` (and the website's `screenshots/` folder) from the real
windows of the app, so they never drift from the UI. Windows only; it uses the app's own GDI capturer.

1. Put two PNGs in `in/`: `desktop.png` (1920 x 1080, shown behind the windows as the "screen") and
   `longpage.png` (1280 wide, tall; sliced into overlapping frames for the stitched example). Any
   rendering of a web page works, for instance with headless Chrome:

       chrome --headless=new --hide-scrollbars --virtual-time-budget=6000 --window-size=1920,1080 --screenshot=in/desktop.png  http://localhost:8085/docs.php?p=readme
       chrome --headless=new --hide-scrollbars --virtual-time-budget=6000 --window-size=1280,4200 --screenshot=in/longpage.png http://localhost:8085/docs.php?p=help

2. Run it on a machine whose primary monitor is 1920 x 1080 at 100 % scaling. The windows appear on
   screen for a few seconds.

       dotnet run --project tools/Screenshots -- tools/Screenshots/in docs/screenshots

Two pictures (the overlay mid-drag and the editor with marks) are set up through reflection on private
fields; if those fields are renamed, the tool throws and the field names here need updating.
