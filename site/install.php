<?php
// Install, first run, permissions, start at login, update and uninstall, for each system. Every command is one copyable row.
require __DIR__ . '/inc.php';

$releases = releases();
$latest = latest_release($releases);
$files = $latest ? assets_by_kind($latest) : [];
$v = $latest ? ltrim($latest['tag_name'], 'v') : '1.0.3';

function dl_btn(array $files, string $key, string $label, string $cls = 'btn primary'): string
{
    if (!isset($files[$key])) return '';
    return '<a class="' . $cls . '" href="' . h($files[$key]['browser_download_url']) . '">' . h($label) . ', ' . fmt_size((int)$files[$key]['size']) . '</a>';
}

$description = 'How to install, update and remove StitchShot on Windows, macOS and Linux: downloads, the quarantine flag on macOS, Screen Recording and Accessibility permissions, start at login, the application-menu entry on Linux.';
$jsonld = ['@context' => 'https://schema.org', '@graph' => [
    org_jsonld(),
    ['@type' => 'BreadcrumbList', 'itemListElement' => [
        ['@type' => 'ListItem', 'position' => 1, 'name' => 'StitchShot', 'item' => SITE_URL],
        ['@type' => 'ListItem', 'position' => 2, 'name' => 'Install and update', 'item' => SITE_URL . 'install.php'],
    ]],
    ['@type' => 'HowTo', 'name' => 'Install StitchShot on macOS', 'totalTime' => 'PT3M', 'step' => [
        ['@type' => 'HowToStep', 'name' => 'Download', 'text' => 'Download the archive for your chip, Apple silicon or Intel.'],
        ['@type' => 'HowToStep', 'name' => 'Extract and clear the quarantine flag', 'text' => 'tar xzf the archive, then xattr -dr com.apple.quarantine StitchShot.app, then move it to /Applications.'],
        ['@type' => 'HowToStep', 'name' => 'Grant permissions', 'text' => 'Allow Screen Recording and Accessibility for StitchShot under System Settings > Privacy & Security, then start it again.'],
    ]],
    ['@type' => 'HowTo', 'name' => 'Install StitchShot on Linux', 'totalTime' => 'PT3M', 'step' => [
        ['@type' => 'HowToStep', 'name' => 'Packages', 'text' => 'Install libX11 and xclip with your package manager.'],
        ['@type' => 'HowToStep', 'name' => 'Extract', 'text' => 'Extract the archive into ~/.local/bin and run StitchShot from an X11 session.'],
        ['@type' => 'HowToStep', 'name' => 'Menu entry', 'text' => 'Create ~/.local/share/applications/stitchshot.desktop pointing at the binary and the icon.'],
    ]],
]];
page_head('Install and update', $description, 'Install', 'install.php', ['jsonld' => $jsonld, 'keywords' => 'install StitchShot, update StitchShot, macOS quarantine xattr, Screen Recording permission, Accessibility permission, Linux desktop entry, autostart, uninstall']);
?>
<section class="wrap guide">
  <h1>Install, update, remove</h1>
  <p class="intro">StitchShot <?= h($v) ?> is one download per system and no installer on macOS and Linux. Each step below is a command you can copy, in the order you run them. Settings survive every update; they live in a small JSON file in your profile.</p>
  <p class="jump"><a href="#windows">Windows</a><a href="#macos">macOS</a><a href="#linux">Linux</a><a href="#settings">Where things are kept</a></p>

  <div class="os-block" id="windows">
    <h2>Windows</h2>
    <div class="panes">
      <div class="step"><span>Which file</span><div>
        <?= dl_btn($files, 'win-setup', 'Installer') ?> <?= dl_btn($files, 'win-portable', 'Portable', 'btn') ?> <?= dl_btn($files, 'win-small', 'Small, needs .NET 8', 'btn') ?>
        <p>The installer adds a Start Menu entry and an uninstaller, installs for your user only (no admin rights) and asks whether to start at login. The portable file is one exe with .NET inside; the small file is the same without .NET and needs the <a href="https://dotnet.microsoft.com/download/dotnet/8.0" rel="noopener">.NET 8 Desktop Runtime</a>.</p></div></div>
      <div class="step"><span>SmartScreen</span><div><p>The files are not code-signed yet, so Windows shows "Windows protected your PC" the first time. Click <strong>More info</strong>, then <strong>Run anyway</strong>. To check the file first, compare its hash with the one in <code>SHA256SUMS.txt</code> from the same release:</p>
        <?= cmds(['certutil -hashfile "%USERPROFILE%\\Downloads\\StitchShot-' . h($v) . '-win-x64-setup.exe" SHA256']) ?></div></div>
      <div class="step"><span>First run</span><div><p>A tray icon appears (look under the arrow next to the clock if it is hidden). <strong>PrintScreen</strong> captures a region, <strong>Ctrl+PrintScreen</strong> the full screen, <strong>Ctrl+Shift+S</strong> starts a stitch session. Right-click the icon for Settings and the manual.</p></div></div>
      <div class="step"><span>Start at login</span><div><p>Tick "Start StitchShot when I log in" in Settings; it writes a Run key for your user. The installer offers the same box.</p></div></div>
      <div class="step"><span>Update</span><div><p>Quit StitchShot from the tray menu, run the new installer; it replaces the files in place. Portable: replace the exe. Settings and screenshots stay where they are.</p></div></div>
      <div class="step"><span>Uninstall</span><div><p>Settings &gt; Apps &gt; Installed apps &gt; StitchShot &gt; Uninstall, or delete the portable exe. To remove the settings too:</p>
        <?= cmds(['rmdir /s /q "%AppData%\\StitchShot"']) ?></div></div>
    </div>
  </div>

  <div class="os-block" id="macos">
    <h2>macOS</h2>
    <div class="panes">
      <div class="step"><span>Which file</span><div>
        <?= dl_btn($files, 'mac-arm', 'Apple silicon') ?> <?= dl_btn($files, 'mac-intel', 'Intel', 'btn') ?>
        <p>Apple menu &gt; About This Mac shows the chip. Or in Terminal:</p>
        <?= cmds(['uname -m']) ?>
        <p><code>arm64</code> means Apple silicon (M1 and later), <code>x86_64</code> means Intel. macOS 12 or newer.</p></div></div>
      <div class="step"><span>Install</span><div><p>In Terminal. The <code>xattr</code> line removes the quarantine flag Safari puts on downloads; without it macOS says the developer cannot be verified, because the app is not notarised yet.</p>
        <?= cmds(['cd ~/Downloads', 'tar xzf StitchShot-' . h($v) . '-osx-arm64.tar.gz', 'xattr -dr com.apple.quarantine StitchShot.app', 'mv StitchShot.app /Applications/', 'open /Applications/StitchShot.app']) ?>
        <p>On an Intel Mac the archive is <code>StitchShot-<?= h($v) ?>-osx-x64.tar.gz</code>. If you prefer not to use Terminal: double-click the archive, right-click StitchShot.app &gt; Open, and confirm "Open Anyway" under System Settings &gt; Privacy &amp; Security.</p></div></div>
      <div class="step"><span>Permissions</span><div><p>Two permissions, both under <strong>System Settings &gt; Privacy &amp; Security</strong>:</p>
        <ul><li><strong>Screen Recording</strong>, to capture the screen at all. macOS asks the first time you press a hotkey or use the menu.</li>
          <li><strong>Accessibility</strong>, for the system-wide hotkeys. Without it a notification says "Hotkeys are off" at start; the menu bar icon still works for everything.</li></ul>
        <p>Allow both for StitchShot, then quit it (menu bar icon &gt; Quit) and open it again: macOS applies the permissions to the next start. The app has no Dock icon; it lives in the menu bar.</p></div></div>
      <div class="step"><span>Start at login</span><div><p>Tick the box in Settings. It writes <code>~/Library/LaunchAgents/com.softviser.stitchshot.plist</code> pointing at the app's current location, so if you move the bundle later, untick and tick again.</p></div></div>
      <div class="step"><span>Update</span><div><p>Quit StitchShot, download the new archive and run the same lines; <code>mv</code> replaces the old bundle. Permissions and settings stay.</p>
        <?= cmds(['cd ~/Downloads', 'tar xzf StitchShot-' . h($v) . '-osx-arm64.tar.gz', 'xattr -dr com.apple.quarantine StitchShot.app', 'rm -rf /Applications/StitchShot.app && mv StitchShot.app /Applications/', 'open /Applications/StitchShot.app']) ?></div></div>
      <div class="step"><span>Uninstall</span><div>
        <?= cmds(['rm -rf /Applications/StitchShot.app', 'rm -f ~/Library/LaunchAgents/com.softviser.stitchshot.plist', 'rm -rf ~/.config/StitchShot']) ?>
        <p>The last line removes the settings; leave it out to keep them for a later install. Your screenshots are in the folder you chose and are not touched.</p></div></div>
    </div>
  </div>

  <div class="os-block" id="linux">
    <h2>Linux</h2>
    <div class="panes">
      <div class="step"><span>Which file</span><div>
        <?= dl_btn($files, 'linux', 'Linux x64') ?>
        <p>One self-contained binary for x86-64 with .NET inside. Capture uses X11: log in to an X11 (Xorg) session; on Wayland only XWayland windows are captured.</p></div></div>
      <div class="step"><span>Packages</span><div><p>The clipboard goes through <code>xclip</code> (or <code>wl-copy</code> if present). Debian and Ubuntu, Fedora, Arch:</p>
        <?= cmds(['sudo apt install libx11-6 xclip', 'sudo dnf install libX11 xclip', 'sudo pacman -S libx11 xclip']) ?>
        <p>GNOME shows no tray icons by itself; install the <a href="https://extensions.gnome.org/extension/615/appindicator-support/" rel="noopener">AppIndicator extension</a>. KDE, XFCE, MATE and Cinnamon need nothing.</p></div></div>
      <div class="step"><span>Install</span><div>
        <?= cmds(['mkdir -p ~/.local/bin', 'tar xzf ~/Downloads/StitchShot-' . h($v) . '-linux-x64.tar.gz -C ~/.local/bin', '~/.local/bin/StitchShot']) ?>
        <p>A tray icon appears. <strong>Ctrl+Shift+4</strong> captures a region, <strong>Ctrl+Shift+3</strong> the full screen, <strong>Ctrl+Shift+S</strong> starts a stitch session. Change them in Settings if your desktop uses them.</p></div></div>
      <div class="step"><span>Menu entry</span><div><p>So it appears in the application menu, with its icon (the archive includes <code>stitchshot.png</code> from version 1.0.3):</p>
        <?= cmds(['mkdir -p ~/.local/share/applications', 'printf \'[Desktop Entry]\nType=Application\nName=StitchShot\nComment=Screenshots and stitched captures\nExec=%s/.local/bin/StitchShot\nIcon=%s/.local/bin/stitchshot.png\nCategories=Graphics;Utility;\nStartupNotify=false\n\' "$HOME" "$HOME" > ~/.local/share/applications/stitchshot.desktop']) ?></div></div>
      <div class="step"><span>Start at login</span><div><p>Tick the box in Settings; it writes <code>~/.config/autostart/stitchshot.desktop</code> with the binary's current path. If you move the binary, untick and tick again.</p></div></div>
      <div class="step"><span>Update</span><div><p>Quit StitchShot (tray menu &gt; Quit), extract the new archive over the old binary, start it again.</p>
        <?= cmds(['tar xzf ~/Downloads/StitchShot-' . h($v) . '-linux-x64.tar.gz -C ~/.local/bin', '~/.local/bin/StitchShot']) ?></div></div>
      <div class="step"><span>Uninstall</span><div>
        <?= cmds(['rm -f ~/.local/bin/StitchShot ~/.local/bin/stitchshot.png', 'rm -f ~/.local/share/applications/stitchshot.desktop ~/.config/autostart/stitchshot.desktop', 'rm -rf ~/.config/StitchShot']) ?></div></div>
    </div>
  </div>

  <div class="os-block" id="settings">
    <h2>Where things are kept</h2>
    <table>
      <thead><tr><th>What</th><th>Windows</th><th>macOS and Linux</th></tr></thead>
      <tbody>
        <tr><td>Screenshots</td><td colspan="2">The folder in Settings; by default <code>Pictures/StitchShot</code>. Files are named by date and time.</td></tr>
        <tr><td>Settings</td><td><code>%AppData%\StitchShot\settings.json</code></td><td><code>~/.config/StitchShot/settings.json</code></td></tr>
        <tr><td>Log</td><td><code>%Temp%\StitchShot.log</code></td><td><code>$TMPDIR/StitchShot.log</code> on macOS, <code>/tmp/StitchShot.log</code> on Linux</td></tr>
        <tr><td>Start at login</td><td>Run key under <code>HKCU</code></td><td><code>~/Library/LaunchAgents/com.softviser.stitchshot.plist</code>, <code>~/.config/autostart/stitchshot.desktop</code></td></tr>
      </tbody>
    </table>
    <p>Deleting the settings file restores every default and brings back the first-run question. The <a href="docs.php?p=help">manual</a> has the troubleshooting table; the About window in the app shows the log path.</p>
  </div>
</section>
<?php page_foot();
