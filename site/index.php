<?php
// The StitchShot front page. Everything about versions and files comes from GitHub (see inc.php); the rest is written here.
require __DIR__ . '/inc.php';

$releases = releases();
$latest = latest_release($releases);
$files = $latest ? assets_by_kind($latest) : [];
$version = $latest ? ltrim($latest['tag_name'], 'v') : null;
$repo = gh_json('');

// Screenshots live next to this file. Only the ones that exist are shown, in this order.
$shots = [
    'overlay.png'    => 'Region selection: the screen is frozen, a crosshair spans it and the loupe shows the pixels, position and colour under the cursor.',
    'stitch-hud.png' => 'A stitch session: the HUD keeps count of the frames and the size of the image so far.',
    'stitched.png'   => 'The result: a page far longer than the screen, in one image, with the overlaps trimmed.',
    'editor.png'     => 'The editor: rectangle, ellipse, arrow, pen, text, pixelate and crop.',
    'settings.png'   => 'Settings: folder, clipboard, start at login, hotkeys, loupe, notification corner and the stitch options.',
    'tray.png'       => 'The tray menu. StitchShot waits there for its hotkeys.',
];
$shots = array_filter($shots, function ($f) { return is_file(__DIR__ . '/screenshots/' . $f); }, ARRAY_FILTER_USE_KEY);

// The frames of the demonstration in the hero: a long page captured screen by screen. The text is the manual's own.
$frames = [
    ['Capturing a region', 'Press the region hotkey. The screen freezes and dims; whatever was on screen, including open menus and tooltips, stays exactly as it was. A crosshair spans the whole screen so edges are easy to line up.'],
    ['Stitching', 'Select a region once. Each press of a key grabs the same region from the live screen and appends it below the previous frame. Finish, and the frames become one PNG.'],
    ['Long pages', 'With "Trim the overlap" on, StitchShot compares the bottom of the previous frame with the top of the new one, finds the band of identical rows and drops it, so nothing repeats at the seams.'],
    ['Pieces of different sizes', 'Press the start hotkey again during a session to select a new region. A toolbar from one window and a dialog from another end up in one picture, each at its own width.'],
    ['Fixing mistakes', 'Undo on the HUD removes the last frame. Pressing the start hotkey again lets you select a new region without losing what was captured so far. Escape cancels the whole session.'],
    ['Editor', 'Rectangle, ellipse, line, arrow, pen, text, pixelate and crop, with undo. Open it straight from the selection toolbar or from the "Saved" notification afterwards.'],
    ['Notifications', 'When a file is saved, a small notification appears in a corner of the monitor the capture was on, with a thumbnail, a folder icon and a pencil that opens the editor.'],
    ['Settings', 'Save folder, clipboard, start at login, every hotkey, the loupe, the notification corner, overlap trimming, sticky-header crop and the Page Down delay.'],
];

$description = 'StitchShot is a free, open-source screenshot tool for Windows, Linux and macOS. Capture a region, press a key as you scroll, and the frames are stitched into one tall image with the overlaps trimmed. Region and full-screen capture, a loupe and a small editor included.';
$jsonld = ['@context' => 'https://schema.org', '@graph' => [
    org_jsonld(),
    ['@type' => 'WebSite', '@id' => SITE_URL . '#site', 'url' => SITE_URL, 'name' => 'StitchShot', 'publisher' => ['@id' => MAKER_URL . '/#org'], 'inLanguage' => 'en'],
    array_filter([
        '@type' => 'SoftwareApplication', '@id' => SITE_URL . '#app', 'name' => 'StitchShot', 'url' => SITE_URL,
        'description' => TAGLINE, 'applicationCategory' => 'UtilitiesApplication', 'applicationSubCategory' => 'Screenshot tool',
        'operatingSystem' => 'Windows 10, Windows 11, macOS 12+, Linux (X11)', 'softwareVersion' => $version,
        'datePublished' => $latest ? substr($latest['published_at'], 0, 10) : null,
        'downloadUrl' => $latest ? $latest['html_url'] : REPO_URL . '/releases',
        'releaseNotes' => SITE_URL . 'docs.php?p=changelog', 'softwareHelp' => ['@type' => 'CreativeWork', 'url' => SITE_URL . 'docs.php?p=help'],
        'license' => 'https://opensource.org/license/mit', 'isAccessibleForFree' => true,
        'offers' => ['@type' => 'Offer', 'price' => '0', 'priceCurrency' => 'USD'],
        'author' => ['@id' => MAKER_URL . '/#org'], 'image' => SITE_URL . 'assets/icon-512.png',
        'screenshot' => $shots ? array_map(function ($f) { return SITE_URL . 'screenshots/' . $f; }, array_keys($shots)) : null,
        'sameAs' => [REPO_URL], 'featureList' => 'Stitched captures, automatic Page Down capture, overlap trimming, region and full-screen capture, loupe, annotation editor, clipboard, tray application, configurable hotkeys',
    ], function ($v) { return $v !== null; }),
]];

page_head('StitchShot', $description, '', '', ['jsonld' => $jsonld]);
?>
<section class="hero" id="top">
  <div class="card">
    <div class="band" aria-hidden="true"><?= str_repeat('<i></i>', 24) ?></div>
    <h1>Long pages,<br>one screenshot.</h1>
    <p class="lead">Select a region once. Press a key each time you scroll, and StitchShot appends the same region below the last one, overlaps trimmed, until a whole page is one PNG. Free and open source for Windows, Linux and macOS.</p>
    <p class="actions">
      <?php if ($latest && isset($files['win-setup'])): ?>
      <a class="btn primary" id="dl" href="<?= h($files['win-setup']['browser_download_url']) ?>" data-win="<?= h($files['win-setup']['browser_download_url']) ?>" data-mac="<?= h($files['mac-arm']['browser_download_url'] ?? '') ?>" data-linux="<?= h($files['linux']['browser_download_url'] ?? '') ?>">Download for <span>Windows</span></a>
      <?php else: ?>
      <a class="btn primary" href="<?= REPO_URL ?>/releases/latest" rel="noopener">Download from GitHub</a>
      <?php endif; ?>
    </p>
    <p class="links"><a href="#download">All downloads</a><a href="install.php">Install and update</a><a href="<?= REPO_URL ?>" rel="noopener">Source on GitHub</a></p>
    <p class="meta"><?= $latest ? 'Version ' . h($version) . ', ' . fmt_date($latest['published_at']) . '. ' : '' ?>MIT licensed. No account, no telemetry, no network connections.</p>
  </div>
  <div class="stage" aria-label="A demonstration of a stitch session">
    <div class="stitched" id="stitched" data-ui>
      <?php foreach (array_slice($frames, 0, 3) as $i => [$title, $text]): ?>
      <div class="frame"><strong><?= h($title) ?></strong><p><?= h($text) ?></p></div>
      <?php endforeach; ?>
    </div>
    <div class="hud" id="hud" data-ui role="group" aria-label="Stitch session controls">
      <span class="count"><b id="hud-n">3</b> frames · <span id="hud-size">1280 × 1584</span></span>
      <button type="button" data-act="add" title="F8: capture the region again"><kbd>F8</kbd> Capture</button>
      <button type="button" data-act="page" title="F10: Page Down, wait, capture"><kbd>F10</kbd> Page Down + capture</button>
      <button type="button" data-act="undo" title="Remove the last frame">Undo</button>
      <button type="button" class="finish" data-act="finish" title="F9: join the frames and save"><kbd>F9</kbd> Finish</button>
    </div>
    <div class="toast" id="toast" data-ui hidden>
      <span class="thumb" aria-hidden="true"></span>
      <span><strong>Saved</strong><br><span id="toast-file">StitchShot 2026-11-10 09.41.07.png</span> · <span id="toast-size">1280 × 2610</span></span>
    </div>
    <p class="hint">Press the keys on your keyboard, or the buttons.</p>
  </div>
  <script type="application/json" id="frames"><?= json_encode($frames, JSON_UNESCAPED_UNICODE | JSON_HEX_TAG) ?></script>
</section>

<section class="how" id="how" aria-labelledby="how-h">
  <div class="field f1">
    <h2 id="how-h">How a session goes</h2>
    <div class="mini" data-ui><span class="k">Ctrl+Shift+S</span><span>Select the region</span><em>Frame 1 · 1280 × 720</em></div>
    <p>First you select the region. That is frame one, and a small HUD appears with the count.</p>
  </div>
  <div class="field f2">
    <div class="mini" data-ui><span class="k">F8</span><span>Capture again</span><em>Frame 2 · 1280 × 1392</em></div>
    <p>Then you scroll and press F8. The same region is captured and joined below; the 48 rows both frames shared are dropped.</p>
  </div>
  <div class="field f3">
    <div class="mini" data-ui><span class="k">F10</span><span>Page Down, then capture</span><em>Frame 3 · 1280 × 2064</em></div>
    <p>Or press F10 and let StitchShot press Page Down, wait for the page to settle and capture, as many times as the page is long.</p>
  </div>
  <div class="field f4">
    <div class="mini saved" data-ui><span class="k">F9</span><span>Finish</span><em>Saved · 1280 × 2064 · on the clipboard</em></div>
    <p>Finally F9 writes one PNG to your folder, puts it on the clipboard and shows a notification that opens the file or the editor.</p>
  </div>
</section>

<section class="switches" id="features" aria-label="Settings you can switch">
  <?php
  // Each sentence is a real setting; "on" shows what it does in figures. Two are on at load, like a fresh install.
  $sw = [
      ['Trim the overlap between scrolled frames.', 'The bottom of the previous frame and the top of the new one are compared row by row; the identical band is dropped once. A 400-pixel scroll adds 400 pixels, not 720.', true],
      ['Press Page Down for me, then capture.', 'F10 sends Page Down to the window, waits 400 ms (you set it), then captures. A page twenty screens long is twenty presses and no mouse.', true],
      ['Copy every capture to the clipboard.', 'The PNG is saved and on the clipboard at the same time; paste it into a chat, a mail or a document straight away.', false],
      ['Show the loupe next to the cursor.', 'A 15 by 15 pixel patch magnified eight times, with X, Y and the colour as #RRGGBB, so an edge lands on the exact pixel. New in 1.0.3: it can be turned off.', false],
      ['Start StitchShot when I log in.', 'A Run key on Windows, an autostart entry on Linux, a LaunchAgent on macOS. The tray icon waits for PrintScreen; nothing else runs.', false],
  ];
  foreach ($sw as $i => [$sentence, $result, $on]): ?>
  <div class="sw<?= $on ? ' on' : '' ?>">
    <p class="s"><?= h($sentence) ?></p>
    <button type="button" class="toggle" role="switch" aria-checked="<?= $on ? 'true' : 'false' ?>" aria-label="<?= h($sentence) ?>"><span></span></button>
    <p class="r"><?= h($result) ?></p>
  </div>
  <?php endforeach; ?>
</section>

<section class="detail wrap" id="detail" aria-labelledby="detail-h">
  <h2 id="detail-h">What else is in it</h2>
  <div class="two">
    <div>
      <h3>Region and full-screen capture</h3>
      <p>PrintScreen freezes the screen and dims it; menus and tooltips stay open in the shot. Drag a region, or press Ctrl+PrintScreen for the whole screen. The size in pixels is shown while you drag, and a second loupe stays pinned on the starting corner so both ends land precisely.</p>
      <h3>The selection toolbar</h3>
      <p>After the drag: save, copy only, open in the editor, add to a stitch session, or cancel. Enter saves, Escape cancels, and the same keys work in the editor.</p>
      <h3>Pieces of any size</h3>
      <p>A stitch session is not limited to one region. Press the start hotkey again and select something else; frames of different widths line up on the left in the final image.</p>
      <h3>Sticky headers</h3>
      <p>A header that stays fixed while the page scrolls would repeat in every frame and defeat the overlap detection. Enter its height once in Settings and it is cropped from frames two onwards.</p>
    </div>
    <div>
      <h3>A small editor</h3>
      <p>Rectangle, ellipse, line, arrow, freehand pen, text, pixelate and crop, each in the colour and width you pick, with undo. It opens from the toolbar or later from the notification, so nothing has to be decided at capture time.</p>
      <h3>Out of the way</h3>
      <p>StitchShot is a tray icon. It saves PNGs named by date and time into a folder you choose, optionally copies each to the clipboard, and shows a notification in the corner you choose. Every hotkey can be changed.</p>
      <h3>One code base, three systems</h3>
      <p>C# on .NET 8 with Avalonia, SkiaSharp and SharpHook. Windows, Linux (X11) and macOS share everything except a small layer each for screen capture, the clipboard and start at login.</p>
      <h3>Nothing leaves your computer</h3>
      <p>No account, no update check, no analytics in the app. The only files it writes are your screenshots, a settings file and a log with error messages. <a href="privacy.php">Privacy</a> has the detail.</p>
    </div>
  </div>
</section>

<?php if ($shots): ?>
<section class="shots wrap" id="screenshots" aria-labelledby="shots-h">
  <h2 id="shots-h">Screenshots</h2>
  <?php foreach ($shots as $file => $caption): ?>
  <figure><a href="screenshots/<?= h($file) ?>"><img src="screenshots/<?= h($file) ?>" alt="<?= h($caption) ?>" loading="lazy" width="1200" height="750"></a><figcaption><?= h($caption) ?></figcaption></figure>
  <?php endforeach; ?>
</section>
<?php endif; ?>

<section class="download wrap" id="download" aria-labelledby="dl-h">
  <div class="dl-head">
    <h2 id="dl-h"><span>Download</span><br>StitchShot <?= h($version ?? '') ?></h2>
    <?php if ($latest): ?><p><?= fmt_date($latest['published_at']) ?>. Every file is built by GitHub Actions from the tagged source and listed with its SHA-256 in <code>SHA256SUMS.txt</code>.</p><?php endif; ?>
  </div>
  <?php if ($latest): ?>
  <div class="os" data-tabs>
    <div class="tabs" role="tablist" aria-label="Operating system">
      <button type="button" role="tab" id="tab-win" aria-selected="true" aria-controls="pane-win" data-os="win">Windows<small>10 and 11, x64</small></button>
      <button type="button" role="tab" id="tab-mac" aria-selected="false" aria-controls="pane-mac" data-os="mac">macOS<small>12 or newer</small></button>
      <button type="button" role="tab" id="tab-linux" aria-selected="false" aria-controls="pane-linux" data-os="linux">Linux<small>x64, X11 session</small></button>
    </div>
    <div class="panes">
      <div class="pane" role="tabpanel" id="pane-win" aria-labelledby="tab-win">
        <div class="step"><span>Download</span><div>
          <?php if (isset($files['win-setup'])): ?><a class="btn primary" href="<?= h($files['win-setup']['browser_download_url']) ?>">Installer, <?= fmt_size((int)$files['win-setup']['size']) ?></a><?php endif; ?>
          <?php if (isset($files['win-portable'])): ?><a class="btn" href="<?= h($files['win-portable']['browser_download_url']) ?>">Portable, <?= fmt_size((int)$files['win-portable']['size']) ?></a><?php endif; ?>
          <p>The installer puts StitchShot in your Start Menu, no admin rights needed, and asks whether to start at login. The portable file is one exe, nothing to install.</p></div></div>
        <div class="step"><span>SmartScreen</span><div><p>The files are not code-signed yet, so Windows warns the first time. Click <strong>More info</strong>, then <strong>Run anyway</strong>. The SHA-256 in the checksums file lets you verify the download.</p></div></div>
        <div class="step"><span>First run</span><div><p>A tray icon appears. Press <strong>PrintScreen</strong> for a region, <strong>Ctrl+Shift+S</strong> to start a stitch. Right-click the icon for Settings.</p></div></div>
        <div class="step"><span>Update</span><div><p>Run the new installer over the old one, or replace the portable exe. Settings and screenshots stay. <a href="install.php#windows">Details, uninstall</a>.</p></div></div>
      </div>
      <div class="pane" role="tabpanel" id="pane-mac" aria-labelledby="tab-mac" hidden>
        <div class="step"><span>Download</span><div>
          <?php if (isset($files['mac-arm'])): ?><a class="btn primary" href="<?= h($files['mac-arm']['browser_download_url']) ?>">Apple silicon, <?= fmt_size((int)$files['mac-arm']['size']) ?></a><?php endif; ?>
          <?php if (isset($files['mac-intel'])): ?><a class="btn" href="<?= h($files['mac-intel']['browser_download_url']) ?>">Intel, <?= fmt_size((int)$files['mac-intel']['size']) ?></a><?php endif; ?>
          <p>Apple menu &gt; About This Mac tells you which chip you have. Or run <code>uname -m</code>: <code>arm64</code> is Apple silicon, <code>x86_64</code> is Intel.</p></div></div>
        <div class="step"><span>Install</span><div><p>In Terminal, in the folder you downloaded to. The second line removes the quarantine flag; the app is not notarised, and macOS would otherwise refuse to open it.</p>
          <?= cmds(['cd ~/Downloads', 'tar xzf StitchShot-' . h($version) . '-osx-arm64.tar.gz', 'xattr -dr com.apple.quarantine StitchShot.app', 'mv StitchShot.app /Applications/', 'open /Applications/StitchShot.app']) ?>
          <p>Intel Macs: use the <code>osx-x64</code> file name in the second line.</p></div></div>
        <div class="step"><span>Permissions</span><div><p>macOS asks for <strong>Screen Recording</strong> (to capture) and <strong>Accessibility</strong> (for the hotkeys) under System Settings &gt; Privacy &amp; Security. Allow both for StitchShot, then quit it from the menu bar icon and open it again. Without Accessibility a notification says "Hotkeys are off"; the menu bar icon still works.</p></div></div>
        <div class="step"><span>Update</span><div><p>Quit StitchShot, download the new archive and repeat the install lines; the new bundle replaces the old one. Settings stay in <code>~/.config/StitchShot</code>. <a href="install.php#macos">Details, start at login, uninstall</a>.</p></div></div>
      </div>
      <div class="pane" role="tabpanel" id="pane-linux" aria-labelledby="tab-linux" hidden>
        <div class="step"><span>Download</span><div>
          <?php if (isset($files['linux'])): ?><a class="btn primary" href="<?= h($files['linux']['browser_download_url']) ?>">Linux x64, <?= fmt_size((int)$files['linux']['size']) ?></a><?php endif; ?>
          <p>One self-contained binary; .NET is inside. Needs an X11 session (on Wayland, only XWayland windows are captured).</p></div></div>
        <div class="step"><span>Packages</span><div><p>The clipboard goes through <code>xclip</code> (or <code>wl-copy</code>); the tray icon on GNOME needs the AppIndicator extension.</p>
          <?= cmds(['sudo apt install libx11-6 xclip', 'sudo dnf install libX11 xclip', 'sudo pacman -S libx11 xclip']) ?>
          <p>Debian and Ubuntu, Fedora, Arch, in that order; pick the one for your distribution.</p></div></div>
        <div class="step"><span>Install</span><div>
          <?= cmds(['mkdir -p ~/.local/bin', 'tar xzf ~/Downloads/StitchShot-' . h($version) . '-linux-x64.tar.gz -C ~/.local/bin', '~/.local/bin/StitchShot']) ?>
          <p>A tray icon appears; <strong>Ctrl+Shift+4</strong> captures a region, <strong>Ctrl+Shift+S</strong> starts a stitch. <a href="install.php#linux">Application-menu entry, autostart, update, uninstall</a>.</p></div></div>
        <div class="step"><span>Update</span><div><p>Quit StitchShot (tray menu &gt; Quit), extract the new archive over the old binary with the same command, start it again.</p></div></div>
      </div>
    </div>
  </div>

  <h3 class="all">All files in this release</h3>
  <table class="downloads">
    <thead><tr><th>Platform</th><th>File</th><th>Size</th><th>Notes</th></tr></thead>
    <tbody>
    <?php foreach (ASSET_KINDS as $key => [$pattern, $platform, $label, $note]): if (!isset($files[$key])) continue; $a = $files[$key]; ?>
      <tr>
        <td><?= h($platform) ?> <small><?= h($label) ?></small></td>
        <td><a href="<?= h($a['browser_download_url']) ?>"><?= h($a['name']) ?></a></td>
        <td><?= fmt_size((int)$a['size']) ?></td>
        <td><?= h($note) ?></td>
      </tr>
    <?php endforeach; ?>
    </tbody>
  </table>
  <?php else: ?>
  <p class="notice">Release information could not be loaded right now. All files are on the <a href="<?= REPO_URL ?>/releases" rel="noopener">GitHub releases page</a>.</p>
  <?php endif; ?>
</section>

<section class="releases wrap" id="releases" aria-labelledby="rel-h">
  <h2 id="rel-h">Releases</h2>
  <?php if ($releases): foreach ($releases as $i => $r): ?>
  <details class="release"<?= $i === 0 ? ' open' : '' ?>>
    <summary><strong><?= h($r['name'] ?: $r['tag_name']) ?></strong><span><?= fmt_date($r['published_at']) ?><?= !empty($r['prerelease']) ? ' · pre-release' : '' ?></span></summary>
    <div class="notes"><?= $r['body'] ? md_html($r['body']) : '<p>No notes for this release.</p>' ?>
      <p><a href="<?= h($r['html_url']) ?>" rel="noopener">This release on GitHub</a><?= isset($r['assets']) ? ', ' . count($r['assets']) . ' files' : '' ?></p>
    </div>
  </details>
  <?php endforeach; else: ?>
  <p class="notice">See the <a href="<?= REPO_URL ?>/releases" rel="noopener">releases on GitHub</a>.</p>
  <?php endif; ?>
  <p>The full history is in the <a href="docs.php?p=changelog">changelog</a>.</p>
</section>

<section class="faq wrap" id="faq" aria-labelledby="faq-h">
  <h2 id="faq-h">Questions</h2>
  <?php
  $faq = [
      ['Price', 'Is StitchShot free?', 'Yes, for any use. It is open source under the MIT license: use it, change it, ship it with your own software; keep the notice. There is no paid version and nothing is locked.'],
      ['Privacy', 'Does it send anything anywhere?', 'No. The app has no update check, no analytics, no crash reporting and makes no network connections. Screenshots go to the folder you chose and, if you want, to the clipboard. The keyboard hook compares each key press with your hotkeys and discards it.'],
      ['Wayland', 'Does it work on Wayland?', 'Partly. Capture uses X11, so on a Wayland desktop only windows running through XWayland are captured; a native Wayland window shows up black. Log in to an X11 session for now. Wayland capture through the portal is on the roadmap.'],
      ['Warnings', 'Why does Windows or macOS warn me?', 'The builds are not code-signed or notarised yet; a certificate is on the list. On Windows click More info, then Run anyway. On macOS remove the quarantine flag once with xattr, as shown under Download. Compare the SHA-256 with SHA256SUMS.txt if you want to be sure of the file.'],
      ['Hotkeys', 'Can I change the hotkeys?', 'All of them, in Settings: click a box and press the combination. Duplicates are refused. On macOS and Linux the defaults are Ctrl+Shift+4 and Ctrl+Shift+3, since those keyboards have no PrintScreen or the desktop owns it.'],
      ['Headers', 'The page has a sticky header and it repeats in every frame.', 'Measure its height (the loupe shows Y) and enter it under "Crop this many pixels from the top of frames 2+". The first frame keeps the header, the others lose it, and the overlap detection works again.'],
  ];
  foreach ($faq as $i => [$tab, $q, $a]): ?>
  <details class="q c<?= $i % 3 ?>"<?= $i === 0 ? ' open' : '' ?>>
    <summary><span class="tab"><?= h($tab) ?></span><span class="qq"><?= h($q) ?></span></summary>
    <p><?= h($a) ?></p>
  </details>
  <?php endforeach; ?>
  <p>More in the <a href="docs.php?p=help">manual</a> and its troubleshooting table, or <a href="<?= REPO_URL ?>/issues" rel="noopener">ask on GitHub</a>.</p>
</section>

<section class="cta" id="get" aria-labelledby="cta-h">
  <div class="close">
    <h2 id="cta-h">Take the<br>long shot.</h2>
    <p>Download, run, press PrintScreen. The first screen you see is the one on the right.</p>
  </div>
  <div class="firstrun" data-ui role="group" aria-label="The first-run question, as the app shows it">
    <strong>Welcome to StitchShot</strong>
    <p>PrintScreen captures a region, Ctrl+Shift+S starts a stitch. Right-click the tray icon for the rest.</p>
    <label><input type="checkbox" checked> Start StitchShot when I log in</label>
    <div class="row">
      <?php if ($latest && isset($files['win-setup'])): ?>
      <a class="btn primary dl2" href="<?= h($files['win-setup']['browser_download_url']) ?>">Download for <span>Windows</span></a>
      <?php else: ?>
      <a class="btn primary" href="<?= REPO_URL ?>/releases/latest" rel="noopener">Download from GitHub</a>
      <?php endif; ?>
      <a class="btn ghost" href="docs.php?p=help">Read the manual first</a>
    </div>
  </div>
</section>

<section class="project wrap" id="github" aria-labelledby="gh-h">
  <h2 id="gh-h">Built in the open</h2>
  <div class="two">
    <div>
      <p>StitchShot is developed at <a href="<?= REPO_URL ?>" rel="noopener">github.com/<?= REPO ?></a>. Every release is built by GitHub Actions from a tag, and the documents on this site are read from the main branch, so nothing here is older than the code.</p>
      <p>Found a bug, or something you miss? <a href="<?= REPO_URL ?>/issues" rel="noopener">Open an issue</a>. Pull requests are welcome; the <a href="docs.php?p=readme">README</a> explains the build.</p>
      <?php if ($repo): ?>
      <p class="stats"><span><b><?= (int)$repo['stargazers_count'] ?></b> stars</span><span><b><?= (int)$repo['forks_count'] ?></b> forks</span><span><b><?= (int)$repo['open_issues_count'] ?></b> open issues</span><span><b><?= count($releases) ?></b> releases</span></p>
      <?php endif; ?>
    </div>
    <div>
      <?= cmds(['git clone ' . REPO_URL . '.git', 'cd stitchshot', 'dotnet build', 'dotnet test', 'dotnet run --project src/StitchShot.App']) ?>
      <p>Needs the .NET 10 SDK; the app itself targets .NET 8.</p>
    </div>
  </div>
</section>
<?php page_foot();
