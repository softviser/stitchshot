<?php
// The StitchShot product page: what it does, screenshots, downloads and release notes from GitHub, the docs, the project.
require __DIR__ . '/inc.php';

$releases = releases();
$latest = null;
foreach ($releases as $r) { if (empty($r['prerelease'])) { $latest = $r; break; } }
$files = $latest ? assets_by_kind($latest) : [];
$version = $latest ? ltrim($latest['tag_name'], 'v') : null;
$repo = gh_json('');

// Screenshots live next to this file. Only the ones that exist are shown, in this order.
$shots = [
    'overlay.png'    => 'Region selection: the screen is frozen, a crosshair spans it and the loupe shows the pixels, position and colour under the cursor.',
    'stitch-hud.png' => 'A stitch session: the HUD keeps count of the frames and the size of the image so far.',
    'stitched.png'   => 'The result: a page far longer than the screen, in one image, with the overlaps trimmed.',
    'editor.png'     => 'The editor: rectangle, ellipse, arrow, pen, text, pixelate and crop.',
    'settings.png'   => 'Settings: folder, clipboard, start at login, hotkeys and the stitch options.',
    'tray.png'       => 'The tray menu. StitchShot waits there for its hotkeys.',
];
$shots = array_filter($shots, function ($f) { return is_file(__DIR__ . '/screenshots/' . $f); }, ARRAY_FILTER_USE_KEY);

page_head('StitchShot', TAGLINE . ' Free and open source for Windows, Linux and macOS.');
?>
<section class="hero">
  <div class="wrap">
    <img class="mark" src="assets/stitchshot-app-icon.svg" alt="" width="96" height="96">
    <h1>One tall screenshot<br>from many small ones.</h1>
    <p class="lead">StitchShot captures a region, then appends the same region again every time you press a key: scroll a list, page through a document, or pick pieces from different places. The frames join into a single image, overlaps trimmed. The usual region and full-screen captures, a loupe and a small editor are there too.</p>
    <p class="actions">
      <?php if ($latest && isset($files['win-setup'])): ?>
      <a class="btn primary" id="dl" href="<?= h($files['win-setup']['browser_download_url']) ?>" data-win="<?= h($files['win-setup']['browser_download_url']) ?>" data-mac="<?= h($files['mac-arm']['browser_download_url'] ?? '') ?>" data-linux="<?= h($files['linux']['browser_download_url'] ?? '') ?>">Download for <span>Windows</span></a>
      <a class="btn" href="#download">All downloads</a>
      <?php else: ?>
      <a class="btn primary" href="<?= REPO_URL ?>/releases/latest" rel="noopener">Download from GitHub</a>
      <?php endif; ?>
      <a class="btn" href="<?= REPO_URL ?>" rel="noopener">View source</a>
    </p>
    <p class="meta"><?= $latest ? 'Version ' . h($version) . ', released ' . fmt_date($latest['published_at']) . '.' : '' ?> Free, MIT licensed, no account, no telemetry.</p>
  </div>
  <div class="stitch" aria-hidden="true"></div>
</section>

<section id="features" class="wrap">
  <h2>What it does</h2>
  <div class="features">
    <div><h3>Stitch captures</h3><p>Select a region once. Each press of a key grabs the same region from the live screen and appends it below the previous frame. Finish, and the frames become one PNG.</p></div>
    <div><h3>Long pages by themselves</h3><p>Let StitchShot press Page Down for you, wait for the scroll to settle and capture. Overlapping rows between frames are found and dropped, so nothing repeats at the seams.</p></div>
    <div><h3>Pieces of any size</h3><p>Add a new region of a different size in the middle of a session: a toolbar from here, a dialog from there, in one picture.</p></div>
    <div><h3>Frozen screen, precise edges</h3><p>The screen freezes before the overlay appears, so menus and tooltips stay in the shot. A full-screen crosshair and a loupe with coordinates and colour put the edges where you want them.</p></div>
    <div><h3>A small editor</h3><p>Rectangle, ellipse, line, arrow, pen, text, pixelate and crop, with undo. Open it straight from the selection or from the saved notification.</p></div>
    <div><h3>Out of the way</h3><p>Lives in the tray, saves PNGs to a folder, copies every capture to the clipboard. Hotkeys are yours to change. Windows, Linux (X11) and macOS from one code base.</p></div>
  </div>
</section>

<section class="wrap how">
  <h2>How a stitch session goes</h2>
  <ol>
    <li><strong>Ctrl+Shift+S</strong> and select the region. That is frame 1. A small HUD appears with the frame count.</li>
    <li>Scroll, turn the page or move to the other window. Press <strong>F8</strong> to capture the same region again, or <strong>F10</strong> to have StitchShot press Page Down first.</li>
    <li>Repeat. Undo the last frame from the HUD if a capture went wrong.</li>
    <li><strong>F9</strong> joins the frames, saves the PNG and copies it to the clipboard.</li>
  </ol>
  <p>Every key is a default and can be changed in Settings. The <a href="docs.php?p=help">help page</a> has the rest: sticky headers, the editor, platform notes, troubleshooting.</p>
</section>

<section id="screenshots" class="wrap">
  <h2>Screenshots</h2>
  <?php if ($shots): ?>
  <div class="shots">
    <?php foreach ($shots as $file => $caption): ?>
    <figure><a href="screenshots/<?= h($file) ?>"><img src="screenshots/<?= h($file) ?>" alt="<?= h($caption) ?>" loading="lazy"></a><figcaption><?= h($caption) ?></figcaption></figure>
    <?php endforeach; ?>
  </div>
  <?php else: ?>
  <p class="notice">Screenshots are being prepared. In the meantime the <a href="docs.php?p=help">help page</a> describes every screen.</p>
  <?php endif; ?>
</section>

<section id="download" class="wrap">
  <h2>Download</h2>
  <?php if ($latest): ?>
  <p class="lead">StitchShot <?= h($version) ?>, released <?= fmt_date($latest['published_at']) ?>. Pick the file for your system; every file has a SHA-256 in the checksums list.</p>
  <div class="platforms">
    <div>
      <h3>Windows</h3>
      <?php if (isset($files['win-setup'])): ?><a class="btn primary" href="<?= h($files['win-setup']['browser_download_url']) ?>">Installer, <?= fmt_size((int)$files['win-setup']['size']) ?></a><?php endif; ?>
      <p>Run the installer; it asks whether to start at login. Or take the portable file from the table.</p>
    </div>
    <div>
      <h3>macOS</h3>
      <?php if (isset($files['mac-arm'])): ?><a class="btn primary" href="<?= h($files['mac-arm']['browser_download_url']) ?>">Apple silicon, <?= fmt_size((int)$files['mac-arm']['size']) ?></a><?php endif; ?>
      <?php if (isset($files['mac-intel'])): ?><a class="btn" href="<?= h($files['mac-intel']['browser_download_url']) ?>">Intel, <?= fmt_size((int)$files['mac-intel']['size']) ?></a><?php endif; ?>
      <pre><code>tar xzf StitchShot-*-osx-*.tar.gz
xattr -d com.apple.quarantine StitchShot
./StitchShot</code></pre>
      <p>Grant Screen Recording and Accessibility when asked, then start it again.</p>
    </div>
    <div>
      <h3>Linux</h3>
      <?php if (isset($files['linux'])): ?><a class="btn primary" href="<?= h($files['linux']['browser_download_url']) ?>">x64, <?= fmt_size((int)$files['linux']['size']) ?></a><?php endif; ?>
      <pre><code>tar xzf StitchShot-*-linux-x64.tar.gz
./StitchShot</code></pre>
      <p>X11 session (or XWayland apps only). Needs <code>libX11</code> and <code>xclip</code> or <code>wl-copy</code> for the clipboard; GNOME needs the AppIndicator extension for the tray icon.</p>
    </div>
  </div>
  <table class="downloads">
    <thead><tr><th>Platform</th><th>File</th><th>Size</th><th>Notes</th></tr></thead>
    <tbody>
    <?php foreach (ASSET_KINDS as $key => [$pattern, $platform, $label, $note]): if (!isset($files[$key])) continue; $a = $files[$key]; ?>
      <tr>
        <td><?= h($platform) ?><br><small><?= h($label) ?></small></td>
        <td><a href="<?= h($a['browser_download_url']) ?>"><?= h($a['name']) ?></a></td>
        <td><?= fmt_size((int)$a['size']) ?></td>
        <td><?= h($note) ?></td>
      </tr>
    <?php endforeach; ?>
    </tbody>
  </table>
  <p class="small">On Windows the <a href="https://apps.microsoft.com/" rel="noopener">Microsoft Store version</a> is signed and updates by itself. The files above are not code-signed yet, so SmartScreen warns the first time: "More info", then "Run anyway". macOS: the download is not notarised; run <code>xattr -d com.apple.quarantine StitchShot</code> once, or right-click and Open. Linux needs an X11 session; Wayland capture is on the roadmap.</p>
  <?php else: ?>
  <p class="notice">Release information could not be loaded right now. All files are on the <a href="<?= REPO_URL ?>/releases" rel="noopener">GitHub releases page</a>.</p>
  <?php endif; ?>
</section>

<section id="releases" class="wrap">
  <h2>Releases</h2>
  <?php if ($releases): foreach ($releases as $i => $r): ?>
  <details class="release"<?= $i === 0 ? ' open' : '' ?>>
    <summary><strong><?= h($r['name'] ?: $r['tag_name']) ?></strong> <span><?= fmt_date($r['published_at']) ?><?= !empty($r['prerelease']) ? ' · pre-release' : '' ?></span></summary>
    <div class="notes"><?= $r['body'] ? md_html($r['body']) : '<p>No notes for this release.</p>' ?>
      <p class="small"><a href="<?= h($r['html_url']) ?>" rel="noopener">This release on GitHub</a><?= isset($r['assets']) ? ', ' . count($r['assets']) . ' files' : '' ?></p>
    </div>
  </details>
  <?php endforeach; else: ?>
  <p class="notice">See the <a href="<?= REPO_URL ?>/releases" rel="noopener">releases on GitHub</a>.</p>
  <?php endif; ?>
</section>

<section id="docs" class="wrap">
  <h2>Documentation</h2>
  <div class="cards">
    <a class="card" href="docs.php?p=help"><h3>Help</h3><p>The manual: capturing, stitching, the editor, settings, platform notes, troubleshooting.</p></a>
    <a class="card" href="docs.php?p=readme"><h3>README</h3><p>Overview, install table, platform matrix and building from source.</p></a>
    <a class="card" href="docs.php?p=plan"><h3>Roadmap</h3><p>What comes next and in which order.</p></a>
    <a class="card" href="docs.php?p=logo"><h3>Logo</h3><p>Files, colours and usage rules for the StitchShot mark.</p></a>
    <a class="card" href="docs.php?p=license"><h3>License</h3><p>MIT. Use it, change it, ship it; keep the notice.</p></a>
  </div>
</section>

<section id="github" class="wrap project">
  <h2>Open source</h2>
  <div class="project-grid">
    <div>
      <p>StitchShot is developed in the open at <a href="<?= REPO_URL ?>" rel="noopener">github.com/<?= REPO ?></a>. C# on .NET 8 with Avalonia, SkiaSharp and SharpHook; one code base for all three platforms, with a small platform layer each for capture, clipboard and start-at-login. Every release is built by GitHub Actions from a tag.</p>
      <p>Found a bug, or something you miss? <a href="<?= REPO_URL ?>/issues" rel="noopener">Open an issue</a>. Pull requests are welcome; the README explains the build.</p>
      <?php if ($repo): ?>
      <p class="stats">
        <span><strong><?= (int)$repo['stargazers_count'] ?></strong> stars</span>
        <span><strong><?= (int)$repo['forks_count'] ?></strong> forks</span>
        <span><strong><?= (int)$repo['open_issues_count'] ?></strong> open issues</span>
        <span><strong><?= count($releases) ?></strong> releases</span>
      </p>
      <?php endif; ?>
    </div>
    <div>
      <pre><code>git clone <?= REPO_URL ?>.git
cd stitchshot
dotnet build
dotnet test
dotnet run --project src/StitchShot.App</code></pre>
      <p class="small">Needs the .NET 10 SDK; the app itself targets .NET 8.</p>
    </div>
  </div>
</section>

<script>
// Point the big button at the visitor's own system. The table below has every file anyway.
(function () {
  var b = document.getElementById('dl'); if (!b) return;
  var p = navigator.platform || '', ua = navigator.userAgent;
  var os = /Mac/.test(p) ? 'mac' : (/Linux/.test(p) && !/Android/.test(ua)) ? 'linux' : 'win';
  var url = b.getAttribute('data-' + os);
  if (!url) return;
  b.href = url;
  b.querySelector('span').textContent = os === 'mac' ? 'macOS' : os === 'linux' ? 'Linux' : 'Windows';
})();
</script>
<?php page_foot();
