<?php
// Renders the project's own Markdown files, fetched from the main branch, so the docs here are never older than GitHub.
require __DIR__ . '/inc.php';

$pages = [
    'help'      => ['docs/HELP.md', 'Help', 'The StitchShot manual: capturing, stitching, the editor, settings, platform notes and troubleshooting.'],
    'readme'    => ['README.md', 'README', 'Overview, installation, platforms and how to build StitchShot from source.'],
    'changelog' => ['CHANGELOG.md', 'Changelog', 'What changed in each StitchShot release.'],
    'plan'      => ['PLAN.md', 'Roadmap', 'What is planned for StitchShot and in which order.'],
    'logo'      => ['design/logo/GUIDELINES.md', 'Logo guidelines', 'Files, colours, clear space and minimum sizes of the StitchShot logo.'],
    'license'   => ['LICENSE', 'License', 'The MIT license StitchShot is released under.'],
];
$key = $_GET['p'] ?? '';

if (!isset($pages[$key])) {
    page_head('Documentation', 'Documentation for StitchShot: the manual, README, changelog, roadmap, logo guidelines and license.', 'Help', 'docs.php');
    ?>
<section class="wrap narrow doc">
  <h1>Documentation</h1>
  <p class="intro">Everything is kept in the <a href="<?= REPO_URL ?>" rel="noopener">repository</a> and shown here from the main branch. The <a href="install.php">install guide</a> is on this site only.</p>
  <div class="cards">
    <?php foreach ($pages as $k => [$path, $title, $desc]): ?>
    <a class="cardlink" href="docs.php?p=<?= $k ?>"><h3><?= h($title) ?></h3><p><?= h($desc) ?></p><span class="path"><?= h($path) ?></span></a>
    <?php endforeach; ?>
  </div>
</section>
<?php
    page_foot();
    exit;
}

[$path, $title, $desc] = $pages[$key];
$md = gh_raw($path);
$jsonld = ['@context' => 'https://schema.org', '@graph' => [
    org_jsonld(),
    ['@type' => 'BreadcrumbList', 'itemListElement' => [
        ['@type' => 'ListItem', 'position' => 1, 'name' => 'StitchShot', 'item' => SITE_URL],
        ['@type' => 'ListItem', 'position' => 2, 'name' => 'Documentation', 'item' => SITE_URL . 'docs.php'],
        ['@type' => 'ListItem', 'position' => 3, 'name' => $title, 'item' => SITE_URL . 'docs.php?p=' . $key],
    ]],
    ['@type' => 'TechArticle', 'headline' => $title . ' · StitchShot', 'description' => $desc, 'url' => SITE_URL . 'docs.php?p=' . $key,
     'author' => ['@id' => MAKER_URL . '/#org'], 'publisher' => ['@id' => MAKER_URL . '/#org'], 'about' => ['@id' => SITE_URL . '#app'], 'inLanguage' => 'en'],
]];
page_head($title, $desc, $title === 'Help' ? 'Help' : '', 'docs.php?p=' . $key, ['jsonld' => $jsonld, 'type' => 'article']);
?>
<section class="wrap doc">
  <p class="crumbs"><a href="docs.php">Documentation</a> <span>›</span> <?= h($title) ?> <span>·</span> <a href="<?= REPO_URL ?>/blob/main/<?= h($path) ?>" rel="noopener"><?= h($path) ?> on GitHub</a></p>
<?php if ($md === null): ?>
  <h1><?= h($title) ?></h1>
  <p class="notice">Could not load <code><?= h($path) ?></code> right now. Read it <a href="<?= REPO_URL ?>/blob/main/<?= h($path) ?>" rel="noopener">on GitHub</a>.</p>
<?php elseif ($key === 'license'): ?>
  <h1><?= h($title) ?></h1>
  <pre class="license"><?= h($md) ?></pre>
<?php else:
    $html = md_html($md);
    preg_match_all('/<h2 id="([^"]+)">(.*?)<\/h2>/', $html, $heads, PREG_SET_ORDER);
    if (count($heads) > 2): ?>
  <nav class="toc" aria-label="Contents"><strong>Contents</strong>
    <?php foreach ($heads as $hd): ?><a href="#<?= $hd[1] ?>"><?= strip_tags($hd[2]) ?></a><?php endforeach; ?>
  </nav>
<?php endif; ?>
  <article><?= $html ?></article>
<?php endif; ?>
</section>
<?php page_foot();
