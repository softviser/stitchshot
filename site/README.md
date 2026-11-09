# StitchShot website

The pages behind https://svsr.net/stitchshot/. Plain PHP, no framework, no build step.

    index.php       product page: features, screenshots, downloads, release notes, docs, project
    docs.php        renders HELP.md, README.md, PLAN.md, the logo guidelines and LICENSE (?p=help|readme|plan|logo|license)
    inc.php         GitHub fetch with a file cache, a small Markdown renderer, header and footer
    style.css       one stylesheet, light and dark
    assets/         logo files and icons, copied from design/logo/dist
    screenshots/    product screenshots, see below
    cache/          GitHub responses, one hour each; must be writable (falls back to the system temp folder)

## Where the content comes from

Nothing about versions or files is typed into the pages. Downloads, sizes and release notes come
from the GitHub Releases API, the documents from the main branch (raw.githubusercontent.com), the
star and issue counts from the repository API. Each response is cached for an hour, and a stale
copy is served if GitHub cannot be reached, so the site keeps working during outages and never
hits the unauthenticated rate limit (60 requests an hour per IP).

Publishing a new release on GitHub is all that is needed to update the download table.

## Deploy

1. Copy the contents of this folder to the `/stitchshot/` directory of the web root.
2. PHP 7.4 or newer, with `allow_url_fopen` on (the default).
3. Make `cache/` writable by the web server, or accept the temp-folder fallback.

All links are relative, so the folder can be moved or served from a subdomain without edits. Only
`SITE_URL` in `inc.php` (used for the social preview image) would then change.

## Screenshots

`index.php` shows the files below from `screenshots/` when they exist, in this order, and skips the
missing ones. PNG, about 1200 px wide, taken with StitchShot itself.

    overlay.png       region selection with crosshair and loupe
    stitch-hud.png    a stitch session with the HUD and a few frames
    stitched.png      a stitched result, a long page in one image
    editor.png        the editor with a few annotations
    settings.png      the settings window
    tray.png          the tray menu

## Try it locally

    php -S localhost:8080 -t site

then open http://localhost:8080/index.php.
