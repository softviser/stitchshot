# StitchShot — logo guidelines

One page, for anyone who needs to put the logo somewhere. Master files live in `design/logo/dist/`.

## 1. The logo

**Idea.** One tall frame whose middle bar is a running stitch. The dashed line is both the selection
marquee of a screenshot tool and the stitch that joins two captures, so the mark says "capture" and
"join" in a single shape. The tall proportion hints at the tall images StitchShot produces.

| Version | File | Use |
|---|---|---|
| Horizontal (primary) | `stitchshot-horizontal.svg` | README header, website, documents |
| Horizontal on dark | `stitchshot-horizontal-on-dark.svg` | dark backgrounds, GitHub dark mode |
| Stacked | `stitchshot-stacked.svg` / `-on-dark.svg` | square spaces, social posts, stickers |
| Symbol | `stitchshot-symbol.svg` (blue), `-black`, `-white`, `-on-dark` | anywhere the name is already visible |
| Symbol, small sizes | `stitchshot-symbol-small.svg` | 16–32 px: favicon, tray, lists |
| App icon | `stitchshot-app-icon.svg`, `stitchshot.ico`, `web/` | OS icons, favicon.ico, PWA set |
| Wordmark only | `stitchshot-wordmark.svg` | text-only contexts |
| Social preview | `stitchshot-social-preview.png` (1280×640) | GitHub repository social preview |

## 2. Clear space

Keep a clear zone of **¼ of the symbol's height** around every version, on all sides. Nothing else
(text, other logos, edges) enters it. The zone scales with the logo.

## 3. Minimum size

| Version | Screen | Print |
|---|---|---|
| Horizontal | 120 px wide | 30 mm wide |
| Stacked | 80 px wide | 20 mm wide |
| Symbol | 24 px; below that use the small-size file (down to 16 px) | 6 mm |

## 4. Colour

| Name | HEX | RGB | Role |
|---|---|---|---|
| Seam Blue | `#2457C5` | 36 87 197 | symbol, tiles, primary accent |
| Thread Yellow | `#FFC43D` | 255 196 61 | the stitches, only on Seam Blue or Ink |
| Ink | `#0F172A` | 15 23 42 | wordmark, text |
| White | `#FFFFFF` | 255 255 255 | reversed versions |

Approved pairs: blue symbol + ink wordmark on white or very light grounds · white symbol with yellow
stitches on Seam Blue or Ink · all-white or all-black one-colour versions anywhere else. Thread Yellow
never appears on white (contrast 1.6:1); on Seam Blue it is 4.1:1, on Ink 11:1. White on Seam Blue is 6.5:1.
No CMYK or Pantone values are specified yet; match to the HEX with a swatch book if the logo goes to print.

## 5. Typography

The wordmark is **Inter SemiBold**, outlined to paths with −1.2 % tracking; never retype it. Inter is
under the SIL Open Font License and is also the UI font of the app. For text next to the logo use
Inter or the platform's system sans (Segoe UI, SF, Cantarell).

## 6. Construction notes

256-unit canvas, 8-unit grid. Frame 200 × 216, outer radius 28, stroke 24, inner radius 4. Five stitches
20 × 20 with 13-unit gaps, running edge to edge, centred 4 units above the geometric centre (optical
centring). Small-size cut: stroke 32, three stitches 32 × 28. Reversed versions use stroke 22 to
counter irradiation. App icon: tile radius 56, mark at 70 % (75 % for the small cut).

## 7. Don'ts

Don't stretch, rotate or add effects · don't change the number or spacing of stitches · don't put
yellow stitches on white · don't recolour outside the palette · don't place the colour version on busy
images (use a tile or the one-colour version) · don't rebuild the wordmark from a font.

## 8. Contact

Softviser Yazılım · https://softviser.com.tr · https://github.com/softviser/stitchshot
