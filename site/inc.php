<?php
// Shared pieces of the StitchShot site: GitHub data behind a file cache, a small Markdown renderer, page chrome and SEO tags.
// Runs on PHP 7.4+ with allow_url_fopen on. Nothing here needs a database or a build step.
declare(strict_types=1);

const REPO = 'softviser/stitchshot';
const REPO_URL = 'https://github.com/' . REPO;
const SITE_URL = 'https://svsr.net/stitchshot/';
const MAKER = 'Softviser Yazılım';
const MAKER_URL = 'https://softviser.com.tr';
const TAGLINE = 'Screenshot tool that stitches several captures into one tall image.';
const ANALYTICS_ID = '3e6985ed-fef4-4d08-adf3-ec3e81ce9538';

function h(?string $s): string { return htmlspecialchars((string)$s, ENT_QUOTES, 'UTF-8'); }

function http_get(string $url): ?string
{
    $ctx = stream_context_create(['http' => [
        'header' => "User-Agent: stitchshot-site\r\nAccept: application/vnd.github+json, text/plain\r\n",
        'timeout' => 8,
    ]]);
    $body = @file_get_contents($url, false, $ctx);
    return $body === false ? null : $body;
}

/** Fetches a URL through a one-hour file cache. A stale copy beats nothing when GitHub is down or rate-limits us. */
function cached_get(string $url, int $ttl = 3600): ?string
{
    $dir = __DIR__ . '/cache';
    if (!is_dir($dir) || !is_writable($dir)) {
        $dir = sys_get_temp_dir() . '/stitchshot-site';
        if (!is_dir($dir)) @mkdir($dir, 0775, true);
    }
    $file = $dir . '/' . md5($url);
    if (is_file($file) && filemtime($file) > time() - $ttl) return (string)file_get_contents($file);
    $body = http_get($url);
    if ($body === null) return is_file($file) ? (string)file_get_contents($file) : null;
    @file_put_contents($file, $body, LOCK_EX);
    return $body;
}

function gh_json(string $path): ?array
{
    $body = cached_get('https://api.github.com/repos/' . REPO . $path);
    $json = $body === null ? null : json_decode($body, true);
    return is_array($json) ? $json : null;
}

function gh_raw(string $path): ?string { return cached_get('https://raw.githubusercontent.com/' . REPO . '/main/' . $path); }

/** Published releases, newest first. */
function releases(): array
{
    return array_values(array_filter(gh_json('/releases') ?? [], function ($r) { return empty($r['draft']) && isset($r['tag_name']); }));
}

/** The newest release that is not a pre-release, or null. */
function latest_release(array $releases): ?array
{
    foreach ($releases as $r) if (empty($r['prerelease'])) return $r;
    return null;
}

/** The files the release pipeline produces: key => [file-name pattern, platform, label, note]. */
const ASSET_KINDS = [
    'win-setup'    => ['/-win-x64-setup\.exe$/', 'Windows', 'Installer', 'Start Menu entry, optional start at login, uninstaller. Installs per user, no admin rights.'],
    'win-portable' => ['/-win-x64\.exe$/', 'Windows', 'Portable', 'One file, run it from anywhere. .NET is bundled.'],
    'win-small'    => ['/-win-x64-requires-net8\.exe$/', 'Windows', 'Small', 'Needs the .NET 8 Desktop Runtime installed.'],
    'linux'        => ['/-linux-x64\.tar\.gz$/', 'Linux', 'x64', 'Extract and run ./StitchShot. X11 session; needs libX11 and xclip or wl-copy.'],
    'mac-arm'      => ['/-osx-arm64\.tar\.gz$/', 'macOS', 'Apple silicon', 'Extract, remove the quarantine flag, run. Grant Screen Recording and Accessibility.'],
    'mac-intel'    => ['/-osx-x64\.tar\.gz$/', 'macOS', 'Intel', 'Same steps as Apple silicon, built for Intel Macs.'],
    'checksums'    => ['/^SHA256SUMS\.txt$/', 'All', 'Checksums', 'SHA-256 of every file above.'],
];

/** A release's assets keyed by ASSET_KINDS key. */
function assets_by_kind(array $release): array
{
    $out = [];
    foreach ($release['assets'] ?? [] as $a) {
        foreach (ASSET_KINDS as $key => $kind) {
            if (!isset($out[$key]) && preg_match($kind[0], $a['name'])) { $out[$key] = $a; break; }
        }
    }
    return $out;
}

function fmt_size(int $bytes): string { return $bytes >= 1048576 ? round($bytes / 1048576) . ' MB' : round($bytes / 1024) . ' KB'; }
function fmt_date(?string $iso): string { return $iso ? date('j F Y', strtotime($iso)) : ''; }

/** Shell commands, one per row, each with its own copy button. Rows wrap instead of scrolling. */
function cmds(array $lines): string
{
    $out = '<div class="cmds">';
    foreach ($lines as $l) $out .= '<div class="cmd"><code>' . h($l) . '</code><button type="button" class="copy" aria-label="Copy this command">Copy</button></div>';
    return $out . '</div>';
}

// ---- Markdown: enough for this project's own files (headings, lists, tables, fenced and indented code, links, images) ----

/** Links inside the repository's Markdown point at files; send them to our docs pages where we have one, else to GitHub. */
function md_url(string $url, bool $image): string
{
    if (preg_match('#^(https?:|mailto:|\#)#', $url)) return $url;
    $pages = ['docs/HELP.md' => 'docs.php?p=help', 'README.md' => 'docs.php?p=readme', 'LICENSE' => 'docs.php?p=license', 'design/logo/GUIDELINES.md' => 'docs.php?p=logo', 'PLAN.md' => 'docs.php?p=plan', 'CHANGELOG.md' => 'docs.php?p=changelog'];
    if (isset($pages[$url])) return $pages[$url];
    return REPO_URL . ($image ? '/raw/main/' : '/blob/main/') . ltrim($url, '/');
}

function md_inline(string $s): string
{
    $s = h($s);
    $s = preg_replace('/`([^`]+)`/', '<code>$1</code>', $s);
    $s = preg_replace('/\*\*(.+?)\*\*/', '<strong>$1</strong>', $s);
    $s = preg_replace_callback('/!\[([^\]]*)\]\(([^)\s]+)\)/', function ($m) { return '<img src="' . h(md_url($m[2], true)) . '" alt="' . $m[1] . '">'; }, $s);
    $s = preg_replace_callback('/\[([^\]]+)\]\(([^)\s]+)\)/', function ($m) { return '<a href="' . h(md_url($m[2], false)) . '">' . $m[1] . '</a>'; }, $s);
    $s = preg_replace('#(?<![">=])\bhttps?://[^\s<]+[^\s<.,)]#', '<a href="$0">$0</a>', $s);
    return $s;
}

function md_slug(string $s): string { return trim(preg_replace('/[^a-z0-9]+/', '-', strtolower(strip_tags($s))), '-'); }

function md_cells(string $line): array
{
    return array_map('md_inline', array_map('trim', explode('|', trim(trim($line), '|'))));
}

function md_html(string $md): string
{
    $lines = preg_split('/\r?\n/', $md);
    $n = count($lines);
    $out = '';
    $block = '/^(#{1,6}\s|```|\s*[-*]\s+\S|\s*\d+\.\s+\S|\s*\||>|\s{2,}\S|<)/';
    for ($i = 0; $i < $n;) {
        $l = $lines[$i];
        if (trim($l) === '') { $i++; continue; }
        if (preg_match('/^```/', $l)) {
            $buf = [];
            for ($i++; $i < $n && !preg_match('/^```/', $lines[$i]); $i++) $buf[] = $lines[$i];
            $i++;
            $out .= '<pre><code>' . h(implode("\n", $buf)) . "</code></pre>\n";
        } elseif (preg_match('/^(#{1,6})\s+(.*?)\s*#*$/', $l, $m)) {
            $lvl = strlen($m[1]);
            $out .= "<h$lvl id=\"" . md_slug($m[2]) . '">' . md_inline($m[2]) . "</h$lvl>\n";
            $i++;
        } elseif (preg_match('/^\s*\|/', $l) && $i + 1 < $n && preg_match('/^\s*\|?\s*:?-{2,}/', $lines[$i + 1])) {
            $out .= '<table><thead><tr><th>' . implode('</th><th>', md_cells($l)) . '</th></tr></thead><tbody>';
            for ($i += 2; $i < $n && preg_match('/^\s*\|/', $lines[$i]); $i++) $out .= '<tr><td>' . implode('</td><td>', md_cells($lines[$i])) . '</td></tr>';
            $out .= "</tbody></table>\n";
        } elseif (preg_match('/^\s*([-*]|\d+\.)\s+\S/', $l, $m)) {
            $ordered = $m[1] !== '-' && $m[1] !== '*';
            $re = $ordered ? '/^\s*\d+\.\s+(.*)$/' : '/^\s*[-*]\s+(.*)$/';
            $items = [];
            while ($i < $n && preg_match($re, $lines[$i], $m)) {
                $item = $m[1];
                for ($i++; $i < $n && preg_match('/^\s{2,}(\S.*)$/', $lines[$i], $c) && !preg_match($re, $lines[$i]); $i++) $item .= ' ' . $c[1];
                $items[] = md_inline($item);
            }
            $tag = $ordered ? 'ol' : 'ul';
            $out .= "<$tag><li>" . implode('</li><li>', $items) . "</li></$tag>\n";
        } elseif (preg_match('/^>\s?(.*)$/', $l, $m)) {
            $buf = [];
            for (; $i < $n && preg_match('/^>\s?(.*)$/', $lines[$i], $m); $i++) $buf[] = $m[1];
            $out .= '<blockquote>' . md_inline(implode(' ', $buf)) . "</blockquote>\n";
        } elseif (preg_match('/^\s{2,}\S/', $l)) {
            // Indented text: the key tables in HELP.md. Kept as written, in monospace.
            $buf = [];
            for (; $i < $n && preg_match('/^\s{2,}\S/', $lines[$i]); $i++) $buf[] = $lines[$i];
            $out .= '<pre>' . h(implode("\n", $buf)) . "</pre>\n";
        } elseif ($l[0] === '<') {
            // Raw HTML (the README's logo header): the site has its own header, so skip the block.
            for (; $i < $n && trim($lines[$i]) !== ''; $i++);
        } else {
            $buf = [];
            for (; $i < $n && trim($lines[$i]) !== '' && !preg_match($block, $lines[$i]); $i++) $buf[] = trim($lines[$i]);
            $out .= '<p>' . md_inline(implode(' ', $buf)) . "</p>\n";
        }
    }
    return $out;
}

// ---- Page chrome ----

/**
 * $page is the path of this page relative to the site root ('' for the front page) and becomes the canonical URL.
 * $extra may hold 'jsonld' (an array to emit as application/ld+json), 'keywords', 'image' and 'type'.
 */
function page_head(string $title, string $description, string $active = '', string $page = '', array $extra = []): void
{
    $front = $page === '';
    $full = $front ? 'StitchShot: free screenshot tool that stitches long pages into one image' : $title . ' · StitchShot';
    $canonical = SITE_URL . $page;
    $image = $extra['image'] ?? SITE_URL . 'assets/stitchshot-social-preview.png';
    $keywords = $extra['keywords'] ?? 'screenshot tool, scrolling screenshot, stitch screenshots, long screenshot, screen capture, open source, Windows, Linux, macOS, free';
    $nav = ['index.php#features' => 'Features', 'index.php#download' => 'Download', 'install.php' => 'Install', 'docs.php?p=help' => 'Help', REPO_URL => 'GitHub'];
    ?><!doctype html>
<html lang="en">
<head>
<meta charset="utf-8">
<meta name="viewport" content="width=device-width, initial-scale=1">
<title><?= h($full) ?></title>
<meta name="description" content="<?= h($description) ?>">
<meta name="keywords" content="<?= h($keywords) ?>">
<meta name="author" content="<?= h(MAKER) ?>">
<meta name="robots" content="index, follow, max-image-preview:large">
<link rel="canonical" href="<?= h($canonical) ?>">
<meta property="og:site_name" content="StitchShot">
<meta property="og:locale" content="en_US">
<meta property="og:type" content="<?= h($extra['type'] ?? 'website') ?>">
<meta property="og:url" content="<?= h($canonical) ?>">
<meta property="og:title" content="<?= h($full) ?>">
<meta property="og:description" content="<?= h($description) ?>">
<meta property="og:image" content="<?= h($image) ?>">
<meta property="og:image:width" content="1280">
<meta property="og:image:height" content="640">
<meta property="og:image:alt" content="StitchShot logo on blue">
<meta name="twitter:card" content="summary_large_image">
<meta name="twitter:title" content="<?= h($full) ?>">
<meta name="twitter:description" content="<?= h($description) ?>">
<meta name="twitter:image" content="<?= h($image) ?>">
<meta name="theme-color" content="#2457C5">
<meta name="color-scheme" content="light dark">
<link rel="icon" href="assets/favicon.ico" sizes="48x48">
<link rel="icon" href="assets/favicon.svg" type="image/svg+xml">
<link rel="apple-touch-icon" href="assets/apple-touch-icon.png">
<link rel="manifest" href="assets/site.webmanifest">
<link rel="sitemap" type="application/xml" href="sitemap.xml">
<link rel="preconnect" href="https://fonts.googleapis.com">
<link rel="preconnect" href="https://fonts.gstatic.com" crossorigin>
<link rel="stylesheet" href="https://fonts.googleapis.com/css2?family=Inter:wght@400;500;600;700;800&display=swap">
<link rel="stylesheet" href="style.css?v=3">
<script>document.documentElement.classList.add('js')</script>
<script defer src="https://analytics.softviser.net/script.js" data-website-id="<?= ANALYTICS_ID ?>"></script>
<script defer src="https://analytics.softviser.net/recorder.js" data-website-id="<?= ANALYTICS_ID ?>"></script>
<?php if (!empty($extra['jsonld'])): ?>
<script type="application/ld+json"><?= json_encode($extra['jsonld'], JSON_UNESCAPED_SLASHES | JSON_UNESCAPED_UNICODE) ?></script>
<?php endif; ?>
</head>
<body>
<header class="top">
  <div class="wrap">
    <a class="brand" href="index.php" aria-label="StitchShot home">
      <picture>
        <source media="(prefers-color-scheme: dark)" srcset="assets/stitchshot-horizontal-on-dark.svg">
        <img src="assets/stitchshot-horizontal.svg" alt="StitchShot" height="32" width="178">
      </picture>
    </a>
    <nav aria-label="Main">
      <?php foreach ($nav as $href => $label): ?>
      <a href="<?= h($href) ?>"<?= $label === $active ? ' aria-current="page"' : '' ?><?= $label === 'GitHub' ? ' rel="noopener"' : '' ?>><?= h($label) ?></a>
      <?php endforeach; ?>
    </nav>
  </div>
</header>
<main>
<?php
}

/** The organisation entry other JSON-LD graphs refer to. */
function org_jsonld(): array
{
    return ['@type' => 'Organization', '@id' => MAKER_URL . '/#org', 'name' => MAKER, 'url' => MAKER_URL, 'logo' => SITE_URL . 'assets/icon-512.png'];
}

function page_foot(): void
{
    ?></main>
<footer class="foot">
  <div class="wrap">
    <img class="mark" src="assets/stitchshot-symbol-on-dark.svg" alt="" width="256" height="256" aria-hidden="true">
    <div class="cols">
      <div><strong>StitchShot</strong><a href="index.php#features">Features</a><a href="index.php#download">Download</a><a href="index.php#releases">Releases</a><a href="install.php">Install and update</a></div>
      <div><strong>Documentation</strong><a href="docs.php?p=help">Help</a><a href="docs.php?p=readme">README</a><a href="docs.php?p=changelog">Changelog</a><a href="docs.php?p=plan">Roadmap</a><a href="docs.php?p=logo">Logo</a></div>
      <div><strong>Project</strong><a href="<?= REPO_URL ?>" rel="noopener">Source on GitHub</a><a href="<?= REPO_URL ?>/issues" rel="noopener">Report an issue</a><a href="<?= REPO_URL ?>/releases" rel="noopener">All releases</a><a href="docs.php?p=license">MIT license</a></div>
      <div><strong>Made by</strong><a href="<?= MAKER_URL ?>" rel="noopener"><?= h(MAKER) ?></a><a href="privacy.php">Privacy</a></div>
    </div>
    <p class="legal">© <?= date('Y') ?> <?= h(MAKER) ?>. StitchShot is free software under the MIT license. Windows, macOS and Linux are trademarks of their owners.</p>
  </div>
</footer>
<script src="site.js?v=3" defer></script>
</body>
</html>
<?php
}
