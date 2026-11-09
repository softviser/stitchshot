# StitchShot website

The pages behind https://svsr.net/stitchshot/. Plain PHP, no framework, no build step.

    index.php       front page: the stitch demonstration, how a session goes, features, downloads per system, releases, questions
    install.php     install, permissions, start at login, update and uninstall for Windows, macOS and Linux
    docs.php        renders HELP.md, README.md, CHANGELOG.md, PLAN.md, the logo guidelines and LICENSE (?p=help|readme|changelog|plan|logo|license)
    privacy.php     privacy policy
    inc.php         GitHub fetch with a file cache, a small Markdown renderer, header, footer, SEO tags
    style.css       one stylesheet, light and dark
    site.js         copy buttons, system tabs, the demonstration in the hero, feature switches, entrances
    sitemap.xml     the pages above, absolute URLs
    robots.txt      see Deploy
    assets/         logo files and icons, copied from design/logo/dist
    screenshots/    product screenshots, see below
    cache/          GitHub responses, one hour each; must be writable (falls back to the system temp folder)

## Where the content comes from

Nothing about versions or files is typed into the pages. Downloads, sizes and release notes come
from the GitHub Releases API, the documents from the main branch (raw.githubusercontent.com), the
star and issue counts from the repository API. Each response is cached for an hour, and a stale
copy is served if GitHub cannot be reached, so the site keeps working during outages and never
hits the unauthenticated rate limit (60 requests an hour per IP).

Publishing a new release on GitHub is all that is needed to update the download table, the version
in the hero and the commands on the install page.

## SEO

Every page sets a canonical URL, description, keywords, Open Graph and Twitter tags, and JSON-LD:
`SoftwareApplication` and `WebSite` on the front page, `HowTo` for the macOS and Linux steps on the
install page, `TechArticle` with breadcrumbs on the docs pages. `sitemap.xml` lists the pages.

## Analytics

`inc.php` loads the two scripts from analytics.softviser.net (site id in `ANALYTICS_ID`) on every
page. The privacy page describes this; keep the two in step.

## Deploy

1. Copy the contents of this folder to the `/stitchshot/` directory of the web root.
2. PHP 7.4 or newer, with `allow_url_fopen` on (the default).
3. Make `cache/` writable by the web server, or accept the temp-folder fallback.
4. `robots.txt` is only read from the domain root. Merge its lines into `https://svsr.net/robots.txt`
   (the `Sitemap:` line matters most); the copy here is a reminder.

All links are relative, so the folder can be moved or served from a subdomain without edits. Only
`SITE_URL` in `inc.php` (canonical URLs, social preview, JSON-LD) and `sitemap.xml` would then change.

## Screenshots

`index.php` shows the files below from `screenshots/` when they exist, in this order, and skips the
missing ones. PNG, about 1200 px wide, taken with StitchShot itself. Until they exist the page has
no photographs; the hero and "How a session goes" use interface fragments built in HTML.

    overlay.png       region selection with crosshair and loupe
    stitch-hud.png    a stitch session with the HUD and a few frames
    stitched.png      a stitched result, a long page in one image
    editor.png        the editor with a few annotations
    settings.png      the settings window
    tray.png          the tray menu

## Try it locally

    php -S localhost:8085 -t site

then open http://localhost:8085/index.php. The first load fetches from GitHub and takes a second.
