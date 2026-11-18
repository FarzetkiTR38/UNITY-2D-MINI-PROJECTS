"""Compose eight English store screenshots from verified native game captures.

Only the area below the complete native screenshot receives marketing artwork.
No gameplay, UI words, numbers, stars, arrows or ranks are fabricated/repainted.
Run with Python3.13 and the existing cached marketing-media dependencies.
"""
from __future__ import annotations

import argparse
import functools
import hashlib
import json
import os
from pathlib import Path
import sys

sys.path.insert(0, str(Path(os.environ['TEMP']) / 'ArrowSwarm-Marketing-Media-Python'))
from PIL import Image, ImageChops, ImageDraw, ImageFont
import numpy as np

ROOT = Path(__file__).resolve().parents[2]
PACKAGE = ROOT / 'ArrowSwarm_Marketing/03_GooglePlay/StoreSet_EN_20261115'
FONT = ROOT / 'Assets/_Project/Fonts/Farzetki_Title_Font_H1.ttf'
W, H = 1080, 1920
INK = (8, 30, 67)
WHITE = (255, 255, 255)
GOLD = (255, 229, 70)
ITEMS = [
    ('01_Home_EN.png', 'Home', ('THREE MODES', 'ONE BIG CHALLENGE'), '#20d9ff', '#0063d9'),
    ('02_Defense_EN.png', 'Defense', ('TAP TO', 'ATTACK!'), '#ffb739', '#f16a08'),
    ('03_Shapes_EN.png', 'Shapes', ('SHAPES TO', 'SOLVE'), '#ff75c6', '#ce2e91'),
    ('04_ColorShift_EN.png', 'Color Shift', ('MATCH THE', 'COLORS'), '#b269ff', '#6426c8'),
    ('05_Defense_Freeze_EN.png', 'Defense', ('FREEZE THE', 'SWARM!'), '#54eadb', '#008caf'),
    ('06_Shapes_Advanced_EN.png', 'Shapes', ('FIND THE', 'WAY OUT'), '#87dc3c', '#159b4c'),
    ('07_ColorShift_Rotate_EN.png', 'Color Shift', ('ROTATE TO', 'ESCAPE'), '#f65bcd', '#9a20d7'),
    ('08_Leaderboard_EN.png', 'Leaderboard', ('EARN STARS', 'CLIMB THE RANKS'), '#337fe9', '#172f9b'),
]

def sha(path: Path) -> str:
    return hashlib.sha256(path.read_bytes()).hexdigest()

def write_json(path: Path, value) -> None:
    path.write_text(json.dumps(value, ensure_ascii=False, indent=2) + '\n', encoding='utf-8')

def rgb(hex_color: str) -> tuple[int, int, int]:
    return tuple(int(hex_color[i:i + 2], 16) for i in (1, 3, 5))

@functools.lru_cache(maxsize=80)
def font(size: int):
    return ImageFont.truetype(str(FONT), size)

def fitted(lines: tuple[str, ...], scale: int, height: int) -> tuple[ImageFont.FreeTypeFont, int]:
    for n in range(min(128, round(128 * height / 300)), 69, -1):
        f = font(n * scale)
        boxes = [f.getbbox(line, stroke_width=6 * scale) for line in lines]
        block_height = sum(box[3] - box[1] for box in boxes) + 9 * scale * (len(lines) - 1)
        if max(f.getlength(s) for s in lines) <= 940 * scale and block_height <= (height - 78) * scale:
            return f, n
    raise ValueError('Headline cannot fit safely without becoming too small')

def footer(height: int, background: tuple[int, int, int], lines: tuple[str, ...], top: str, bottom: str):
    """Render reusable vector-like ribbon artwork at2x, entirely outside native pixels."""
    scale = 2
    width, hh = W * scale, height * scale
    yy, xx = np.mgrid[0:hh, 0:width]
    t = yy.astype(np.float32) / max(1, hh - 1)
    left, right = np.array(rgb(top), dtype=np.float32), np.array(rgb(bottom), dtype=np.float32)
    pixels = left[None, None, :] * (1 - t[:, :, None]) + right[None, None, :] * t[:, :, None]
    glow = .11 * np.exp(-(((xx / width - .26) / .6) ** 2 + ((yy / hh + .03) / .58) ** 2))
    pixels = np.clip(pixels + (255 - pixels) * glow[:, :, None], 0, 255).astype(np.uint8)
    gradient = Image.fromarray(pixels, 'RGB')
    ribbon = Image.new('RGB', (width, hh), background)
    mask = Image.new('L', (width, hh), 0)
    ImageDraw.Draw(mask).polygon([(0, 31 * scale), (width, 0), (width, hh), (0, hh)], fill=255)
    ribbon.paste(gradient, (0, 0), mask)
    sheen = Image.new('RGBA', ribbon.size)
    draw = ImageDraw.Draw(sheen)
    draw.line([(0, 31 * scale), (width, 0)], fill=(255, 255, 255, 190), width=4 * scale)
    draw.line([(0, 37 * scale), (width, 6 * scale)], fill=(255, 255, 255, 60), width=2 * scale)
    draw.ellipse((-100 * scale, -120 * scale, 1180 * scale, 69 * scale), fill=(255, 255, 255, 15))
    draw.line([(0, hh - 5 * scale), (width, hh - 5 * scale)], fill=(0, 25, 85, 42), width=9 * scale)
    # Restrict the gloss to the actual colored ribbon, including its slanted top.
    clipped = Image.new('RGBA', ribbon.size)
    clipped.paste(sheen, (0, 0), mask)
    ribbon = Image.alpha_composite(ribbon.convert('RGBA'), clipped)
    f, n = fitted(lines, scale, height)
    draw = ImageDraw.Draw(ribbon)
    stroke = 6 * scale
    bounds = [draw.textbbox((0, 0), s, font=f, stroke_width=stroke) for s in lines]
    heights = [b[3] - b[1] for b in bounds]
    gap = 9 * scale
    block_h = sum(heights) + gap * (len(lines) - 1)
    y = round((hh - block_h) / 2 + 10 * scale)
    boxes = []
    for i, (text, bound, line_h) in enumerate(zip(lines, bounds, heights)):
        x = round((width - f.getlength(text)) / 2)
        baseline_y = y - bound[1]
        draw.text((x, baseline_y + 8 * scale), text, font=f, fill=INK, stroke_fill=INK, stroke_width=stroke + scale)
        draw.text((x, baseline_y + 3 * scale), text, font=f, fill=INK, stroke_fill=INK, stroke_width=stroke)
        draw.text((x, baseline_y), text, font=f, fill=WHITE if i == 0 else GOLD, stroke_fill=INK, stroke_width=stroke)
        box = draw.textbbox((x, baseline_y), text, font=f, stroke_width=stroke + scale)
        assert box[0] >= 24 * scale and box[2] <= width - 24 * scale
        assert box[1] >= 33 * scale and box[3] + 8 * scale <= hh - 19 * scale
        boxes.append([round(v / scale, 2) for v in box])
        y += line_h + gap
    return ribbon.convert('RGB').resize((W, height), Image.Resampling.LANCZOS), n, boxes

def compose(source: Image.Image, lines, top, bottom):
    source = source.convert('RGB')
    # A fixed-size native body is never resized, cropped, filtered or overpainted.
    if source.size == (1080, 1620):
        body, x, y, footer_top, treatment = source.copy(), 0, 0, 1620, 'native_exact'
    elif source.size == (1080, 1920):
        body = source.resize((900, 1600), Image.Resampling.LANCZOS)
        x, y, footer_top, treatment = 90, 16, 1632, 'uniform_full_frame_fit'
        assert body.width / body.height == source.width / source.height
    else:
        raise ValueError(f'Unexpected native dimensions{source.size}; expected1080x1620 or1080x1920')
    background = tuple(np.median(np.asarray(source)[-1], axis=0).astype(np.uint8).tolist())
    im = Image.new('RGB', (W, H), background)
    im.paste(body, (x, y))
    ribbon, n, boxes = footer(H - footer_top, background, lines, top, bottom)
    im.paste(ribbon, (0, footer_top))
    rect = (x, y, x + body.width, y + body.height)
    assert footer_top >= rect[3], 'Marketing ribbon must not cover any native screenshot pixel'
    assert ImageChops.difference(body, im.crop(rect)).getbbox() is None
    return im, body, rect, footer_top, treatment, n, boxes

def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--package', type=Path, default=PACKAGE)
    args = parser.parse_args()
    package = args.package.resolve()
    native_dir, final_dir = package / 'Native', package / 'Final'
    manifest_path = native_dir / 'capture_manifest.json'
    if not manifest_path.is_file():
        manifest_path = package / 'capture_manifest.json'
    assert manifest_path.is_file(), 'Native capture_manifest.json must exist before composing final screenshots'
    capture_manifest = json.loads(manifest_path.read_text(encoding='utf-8-sig'))
    assert isinstance(capture_manifest, (dict, list)) and capture_manifest, 'Empty native capture manifest'
    assert not isinstance(capture_manifest, dict) or capture_manifest.get('passed') is not False, 'Native capture verification failed'
    assert FONT.is_file(), 'Existing licensed title font is required'
    sources = [native_dir / item[0] for item in ITEMS]
    missing = [str(p) for p in sources if not p.is_file()]
    assert not missing, 'No final images generated: missing actual native captures: ' + ', '.join(missing)
    prepared = []
    for item, path in zip(ITEMS, sources):
        with Image.open(path) as source:
            source.load()
            output = compose(source, *item[2:])
        prepared.append((item, path, output))
    final_dir.mkdir(parents=True, exist_ok=True)
    results = []
    for item, source_path, (im, body, rect, footer_top, treatment, n, boxes) in prepared:
        name, mode, lines, top, bottom = item
        path = final_dir / name
        im.save(path, format='PNG', optimize=True)
        with Image.open(path) as check:
            check.load()
            assert check.size == (W, H) and check.mode == 'RGB' and 'A' not in check.getbands()
            assert ImageChops.difference(check.crop(rect), body).getbbox() is None
        results.append({'file': 'Final/' + name, 'order': len(results) + 1, 'mode': mode, 'language': 'en',
            'source': 'Native/' + name, 'sourceSha256': sha(source_path), 'sha256': sha(path), 'bytes': path.stat().st_size,
            'width': W, 'height': H, 'colorMode': 'RGB', 'nativeBodyRect': list(rect), 'nativeBodyPreserved': True,
            'sourceTreatment': treatment, 'footerTop': footer_top, 'nativeUiTextRepainted': False,
            'headline': ' '.join(lines), 'headlineLines': list(lines), 'headlineFontPixels': n,
            'footerTextBoxes': boxes, 'footerGradient': [top, bottom]})
    # Overview is a review aid; only the eight full-resolution PNGs are store screenshots.
    overview = Image.new('RGB', (1540, 1420), '#f1f3f6')
    d = ImageDraw.Draw(overview)
    d.text((38, 21), 'ARROW SWARM / ENGLISH STORE SCREENSHOTS', font=font(32), fill=INK)
    for i, row in enumerate(results):
        with Image.open(package / row['file']) as image:
            thumb = image.resize((342, 608), Image.Resampling.LANCZOS)
        x, y = 38 + (i % 4) * 377, 80 + (i // 4) * 664
        overview.paste(thumb, (x, y))
        d.text((x + 1, y + 617), f'{i + 1:02d}  {row["mode"]}', font=font(22), fill=INK)
    overview.save(package / 'overview_EN.jpg', quality=93, subsampling=0)
    ledger = {'passed': True, 'status': 'READY_FOR_CREATIVE_REVIEW', 'count': 8, 'language': 'en',
        'dimensions': [W, H], 'allPngs24BitRgb': True, 'nativeGameplayAndUiPixelsFabricated': False,
        'nativeFullFrameRetained': True, 'captionArtworkOutsideNativeBody': True,
        'captureManifest': str(manifest_path.relative_to(package)).replace('\\', '/'),
        'captureManifestSha256': sha(manifest_path), 'font': str(FONT.relative_to(ROOT)).replace('\\', '/'),
        'fontSha256': sha(FONT), 'uploaded': False, 'items': results}
    write_json(package / 'store_set_manifest.json', ledger)
    write_json(package / 'image_QA.json', {'passed': True, 'count': 8, 'size': [W, H], 'format': 'PNG RGB',
        'nativeBodiesPixelIdenticalAfterExport': True, 'nativeExactCount': sum(r['sourceTreatment'] == 'native_exact' for r in results),
        'uniformFitCount': sum(r['sourceTreatment'] == 'uniform_full_frame_fit' for r in results),
        'noNativePixelsCovered': True, 'allHeadlinesInsideFooter': True,
        'overviewSha256': sha(package / 'overview_EN.jpg'), 'items': results})
    print(json.dumps({'passed': True, 'images': 8, 'nativeExact': ledger['items'][0]['sourceTreatment'],
        'overview': str(package / 'overview_EN.jpg'), 'output': str(final_dir)}, ensure_ascii=False), flush=True)

if __name__ == '__main__':
    main()
