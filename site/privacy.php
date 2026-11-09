<?php
// Privacy policy; short because the app collects nothing. The website runs a visit counter, which is described honestly.
require __DIR__ . '/inc.php';
page_head('Privacy', 'StitchShot collects no data: no accounts, no telemetry, no network connections from the app. What the website records.', '', 'privacy.php', ['keywords' => 'StitchShot privacy policy, no telemetry, screenshot tool privacy']);
?>
<section class="wrap narrow doc">
  <h1>Privacy</h1>
  <p class="intro">The StitchShot application does not collect, store or transmit any personal data. There is nothing in it to opt out of. This website keeps anonymous visit statistics; the details are below.</p>
  <article>
    <h2 id="the-app">The application</h2>
    <ul>
      <li><strong>Screenshots</strong> are saved as PNG files in the folder you choose (by default <code>Pictures/StitchShot</code>) and, if enabled, placed on your clipboard. They never leave your computer unless you share them yourself.</li>
      <li><strong>Settings</strong> (folder, hotkeys, stitch options) are kept in a small JSON file in your user profile.</li>
      <li><strong>A log file</strong> with error messages is written to your temporary folder for troubleshooting. It contains no screenshots and is never sent anywhere.</li>
    </ul>
    <p>The app makes no network connections. It has no update check, no analytics, no crash reporting and no advertising.</p>
    <h2 id="keyboard-hook">Keyboard hook</h2>
    <p>To react to its hotkeys while other programs are in front, StitchShot listens to key presses system-wide. Each key press is compared with the configured shortcuts and discarded; nothing is recorded.</p>
    <h2 id="permissions">Permissions</h2>
    <p>On macOS the app asks for Screen Recording (to capture the screen) and Accessibility (for the hotkeys). On Windows and Linux it needs no special permission.</p>
    <h2 id="this-website">This website</h2>
    <p>This site uses a self-hosted, privacy-focused analytics service at <code>analytics.softviser.net</code> to count visits and see which pages and downloads are used, and to record anonymised sessions (page views, scrolling and clicks) so the site can be improved. It sets no advertising cookies and the data is not shared with third parties. Text typed into form fields is not recorded; the site has no forms in any case. If you block scripts, the site works without them.</p>
    <p>The web server keeps ordinary access logs (IP address, requested page, time) for a limited period, as most servers do. Release and download information shown here is fetched from GitHub by the server, not by your browser. Downloading StitchShot from GitHub is subject to GitHub's own policies.</p>
    <h2 id="contact">Contact</h2>
    <p>Questions about this policy: open an issue at <a href="<?= REPO_URL ?>/issues" rel="noopener">github.com/<?= REPO ?>/issues</a> or write to <?= h(MAKER) ?> via <a href="<?= MAKER_URL ?>" rel="noopener">softviser.com.tr</a>.</p>
    <p>Last updated 10 November 2026.</p>
  </article>
</section>
<?php page_foot();
