<?php
// Privacy policy; short because the app collects nothing.
require __DIR__ . '/inc.php';
page_head('Privacy', 'StitchShot collects no data: no accounts, no telemetry, no network connections from the app.', 'Docs');
?>
<section class="wrap narrow doc">
  <h1>Privacy policy</h1>
  <p class="lead">StitchShot does not collect, store or transmit any personal data. There is nothing to opt out of.</p>
  <article>
    <h2 id="what-the-app-does-with-your-data">What the app does with your data</h2>
    <ul>
      <li><strong>Screenshots</strong> are saved as PNG files in the folder you choose (by default <code>Pictures/StitchShot</code>) and, if enabled, placed on your clipboard. They never leave your computer unless you share them yourself.</li>
      <li><strong>Settings</strong> (folder, hotkeys, stitch options) are kept in a small JSON file in your user profile.</li>
      <li><strong>A log file</strong> with error messages is written to your temporary folder for troubleshooting. It contains no screenshots and is never sent anywhere.</li>
    </ul>
    <h2 id="network">Network</h2>
    <p>The app makes no network connections. It has no update check, no analytics, no crash reporting and no advertising. Downloading StitchShot from this site or from GitHub is subject to those sites' own policies.</p>
    <h2 id="keyboard-hook">Keyboard hook</h2>
    <p>To react to its hotkeys while other programs are in front, StitchShot listens to key presses system-wide. Each key press is compared with the configured shortcuts and discarded; nothing is recorded.</p>
    <h2 id="permissions">Permissions</h2>
    <p>On macOS the app asks for Screen Recording (to capture the screen) and Accessibility (for the hotkeys). On Windows and Linux it needs no special permission.</p>
    <h2 id="this-website">This website</h2>
    <p>This site sets no cookies and runs no analytics. The web server keeps ordinary access logs (IP address, requested page, time) for a limited period, as most servers do. Release and download information shown here is fetched from GitHub by the server, not by your browser.</p>
    <h2 id="contact">Contact</h2>
    <p>Questions about this policy: open an issue at <a href="<?= REPO_URL ?>/issues" rel="noopener">github.com/<?= REPO ?>/issues</a> or write to Softviser Yazılım via <a href="https://softviser.com.tr" rel="noopener">softviser.com.tr</a>.</p>
    <p class="small">Last updated 9 November 2026.</p>
  </article>
</section>
<?php page_foot();
