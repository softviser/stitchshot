// StitchShot site: copy buttons, the system tabs, the stitch demonstration in the hero, the feature switches, entrances.
// Everything on the page reads without this file; it only adds behaviour.
(function () {
  'use strict';

  // ---- copy: every .copy button copies the text of the code next to it; every <pre> gets one ----
  document.querySelectorAll('pre > code').forEach(function (code) {
    var pre = code.parentElement; // fenced code only; the manual's indented key tables are reading matter, not commands
    if (pre.querySelector('.copy')) return;
    var b = document.createElement('button');
    b.type = 'button'; b.className = 'copy'; b.textContent = 'Copy'; b.setAttribute('aria-label', 'Copy this block');
    pre.appendChild(b);
  });
  function copyText(text, button) {
    var done = function () {
      button.classList.add('ok'); button.textContent = 'Copied';
      setTimeout(function () { button.classList.remove('ok'); button.textContent = 'Copy'; }, 1600);
    };
    if (navigator.clipboard && window.isSecureContext) navigator.clipboard.writeText(text).then(done, function () { fallback(text); done(); });
    else { fallback(text); done(); }
  }
  function fallback(text) {
    var ta = document.createElement('textarea');
    ta.value = text; ta.setAttribute('readonly', ''); ta.style.position = 'fixed'; ta.style.top = '-1000px';
    document.body.appendChild(ta); ta.select();
    try { document.execCommand('copy'); } catch (e) { /* nothing to do */ }
    document.body.removeChild(ta);
  }
  document.addEventListener('click', function (e) {
    var b = e.target.closest('.copy'); if (!b) return;
    var host = b.parentElement;
    var code = host.querySelector('code');
    var text;
    if (code) text = code.textContent;
    else { var tmp = host.cloneNode(true); tmp.querySelectorAll('.copy').forEach(function (x) { x.remove(); }); text = tmp.textContent; }
    copyText(text.replace(/\s+$/, ''), b);
  });

  // ---- the visitor's system: the big button and the open tab ----
  var p = navigator.platform || '', ua = navigator.userAgent;
  var os = /Mac/.test(p) ? 'mac' : (/Linux/.test(p) && !/Android/.test(ua)) ? 'linux' : 'win';
  var names = { mac: 'macOS', linux: 'Linux', win: 'Windows' };
  var dl = document.getElementById('dl');
  if (dl) {
    var url = dl.getAttribute('data-' + os);
    if (url) {
      document.querySelectorAll('#dl, .dl2').forEach(function (a) { a.href = url; var s = a.querySelector('span'); if (s) s.textContent = names[os]; });
    }
  }
  document.querySelectorAll('[data-tabs]').forEach(function (box) {
    var tabs = Array.prototype.slice.call(box.querySelectorAll('[role=tab]'));
    function select(tab) {
      tabs.forEach(function (t) {
        var on = t === tab;
        t.setAttribute('aria-selected', on ? 'true' : 'false');
        var pane = document.getElementById(t.getAttribute('aria-controls'));
        if (pane) pane.hidden = !on;
      });
    }
    tabs.forEach(function (t, i) {
      t.addEventListener('click', function () { select(t); });
      t.addEventListener('keydown', function (e) {
        var j = e.key === 'ArrowRight' || e.key === 'ArrowDown' ? i + 1 : e.key === 'ArrowLeft' || e.key === 'ArrowUp' ? i - 1 : -1;
        if (j < 0) return;
        e.preventDefault(); var n = tabs[(j + tabs.length) % tabs.length]; n.focus(); select(n);
      });
    });
    var mine = tabs.filter(function (t) { return t.getAttribute('data-os') === os; })[0];
    if (mine) select(mine);
  });

  // ---- the stitch demonstration ----
  var stitched = document.getElementById('stitched'), hud = document.getElementById('hud');
  if (stitched && hud) {
    var frames = JSON.parse(document.getElementById('frames').textContent);
    var toast = document.getElementById('toast'), n = document.getElementById('hud-n'), size = document.getElementById('hud-size');
    var width = 1280, first = 720, step = 432; // frame 1 in full, then each frame minus the 288 rows it shares with the previous
    var count = stitched.children.length, timer;
    function update() {
      n.textContent = count;
      size.textContent = width + ' × ' + (first + (count - 1) * step);
    }
    function add() {
      if (count >= frames.length) { count = 0; while (stitched.firstChild) stitched.removeChild(stitched.firstChild); }
      var f = frames[count], el = document.createElement('div');
      el.className = 'frame';
      var t = document.createElement('strong'); t.textContent = f[0];
      var d = document.createElement('p'); d.textContent = f[1];
      el.appendChild(t); el.appendChild(d);
      el.style.animationDelay = '0ms';
      stitched.appendChild(el);
      count++; update(); hide();
    }
    function undo() { if (count > 1) { stitched.lastElementChild.remove(); count--; update(); } hide(); }
    function finish() {
      var now = new Date(), two = function (x) { return (x < 10 ? '0' : '') + x; };
      document.getElementById('toast-file').textContent = 'StitchShot ' + now.getFullYear() + '-' + two(now.getMonth() + 1) + '-' + two(now.getDate()) + ' ' + two(now.getHours()) + '.' + two(now.getMinutes()) + '.' + two(now.getSeconds()) + '.png';
      document.getElementById('toast-size').textContent = size.textContent;
      toast.hidden = false;
      clearTimeout(timer); timer = setTimeout(hide, 6000);
    }
    function hide() { toast.hidden = true; }
    var acts = { add: add, page: add, undo: undo, finish: finish };
    hud.addEventListener('click', function (e) { var b = e.target.closest('button'); if (b) acts[b.getAttribute('data-act')](); });
    document.addEventListener('keydown', function (e) {
      if (!(e.target instanceof Element) || e.target.closest('input, textarea, select') || e.ctrlKey || e.metaKey || e.altKey) return;
      var k = { F8: add, F10: add, F9: finish }[e.key];
      if (k) { e.preventDefault(); k(); }
    });
    update();
  }

  // ---- feature switches ----
  document.querySelectorAll('.sw .toggle').forEach(function (t) {
    t.addEventListener('click', function () {
      var on = t.getAttribute('aria-checked') !== 'true';
      t.setAttribute('aria-checked', on ? 'true' : 'false');
      t.parentElement.classList.toggle('on', on);
    });
  });

  // ---- entrances: parts of each section arrive in reading order; nothing stays hidden if this never runs ----
  if (!('IntersectionObserver' in window) || matchMedia('(prefers-reduced-motion: reduce)').matches) return;
  var parts = 'h2, h3, p, .step, .sw, .release, .q, .mini, figure, .btn, table, .cmds, .firstrun, .tabs, .stats';
  document.querySelectorAll('main > section:not(.hero)').forEach(function (sec) {
    var items = Array.prototype.slice.call(sec.querySelectorAll(parts)).filter(function (el) {
      if (el.matches('.mini, .firstrun')) return true;
      return !el.parentElement.closest('.rv') && !el.closest('[data-ui]');
    });
    items.slice(0, 40).forEach(function (el, i) { el.classList.add('rv'); el.style.setProperty('--d', Math.min(i, 8) * 70 + 'ms'); });
  });
  var io = new IntersectionObserver(function (entries) {
    entries.forEach(function (en) { if (en.isIntersecting) { en.target.classList.add('in'); io.unobserve(en.target); } });
  }, { rootMargin: '0px 0px -8% 0px', threshold: 0.05 });
  document.querySelectorAll('.rv').forEach(function (el) { io.observe(el); });
})();
