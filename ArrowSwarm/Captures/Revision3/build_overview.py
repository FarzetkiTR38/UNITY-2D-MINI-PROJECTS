"""Compose Revision3 review sheets from actual Unity gameplay screenshots."""
from pathlib import Path
from PIL import Image, ImageDraw, ImageFont, ImageOps
import html
import json

ROOT = Path(__file__).resolve().parent
SOURCE = ROOT / 'Playthroughs'
OUT = ROOT / 'Overview'
OUT.mkdir(exist_ok=True)
ROWS = json.loads((SOURCE / 'level-metrics.json').read_text(encoding='utf-8'))
FONT_DIR = Path('C:/Windows/Fonts')


def font(size, bold=False):
    return ImageFont.truetype(str(FONT_DIR / ('arialbd.ttf' if bold else 'arial.ttf')), size)


def sheet(rows, name, dark=False):
    cols, tile_w, tile_h, gap, top = 5, 430, 650, 18, 100
    count_rows = (len(rows) + cols - 1) // cols
    background = '#111d32' if dark else '#e8e5da'
    foreground = '#e8f4ff' if dark else '#193353'
    canvas = Image.new('RGB', (cols * (tile_w + gap) + gap, top + count_rows * (tile_h + gap)), background)
    draw = ImageDraw.Draw(canvas)
    draw.text((gap, 18), 'ARROW SWARM / SHAPES / REVISION 3', font=font(30, True), fill=foreground)
    draw.text((gap, 58), 'Actual Unity gameplay captures / 4 directions / filled silhouettes / rapid shots', font=font(20), fill=foreground)
    for i, row in enumerate(rows):
        x = gap + (i % cols) * (tile_w + gap)
        y = top + (i // cols) * (tile_h + gap)
        capture = row['dark_capture'] if dark else row['light_capture']
        im = Image.open(SOURCE / capture).convert('RGB')
        # Keep the complete playable viewport between the original top/bottom HUD.
        w, h = im.size
        im = im.crop((int(w * .015), int(h * .125), int(w * .985), int(h * .875)))
        im = ImageOps.contain(im, (tile_w, tile_h - 85), Image.Resampling.LANCZOS)
        canvas.paste(im, (x + (tile_w - im.width) // 2, y + 80 + (tile_h - 85 - im.height) // 2))
        draw.text((x + 4, y + 2), f"{row['level']:02d}. {row['title']}", font=font(23, True), fill=foreground)
        draw.text((x + 4, y + 35), f"{row['grid_width']} x {row['grid_height']} / {row['arrows']} arrows / depth {row['dependency_depth']}", font=font(19), fill=foreground)
        draw.text((x + 4, y + 59), f"{row['colors']} colors / {row['stroke_pixels']:.1f}px strokes", font=font(17), fill=foreground)
    canvas.save(OUT / name, quality=95, subsampling=0)


for start in range(0, len(ROWS), 10):
    sheet(ROWS[start:start + 10], f'shapes-{start + 1:02d}-{min(start + 10, len(ROWS)):02d}.jpg')
sheet([r for r in ROWS if r['dark_capture']], 'shapes-dark.jpg', True)

cards = []
for row in ROWS:
    title = html.escape(row['title'])
    cards.append(f'''<article><h2>{row['level']:02d}. {title}</h2>
    <p>{row['grid_width']} × {row['grid_height']} · {row['arrows']} ok · {row['colors']} renk · bağımlılık {row['dependency_depth']}</p>
    <a href="Playthroughs/{row['light_capture']}"><img loading="lazy" src="Playthroughs/{row['light_capture']}" alt="{title}"></a>
    <p><a href="Playthroughs/{row['light_capture']}">Tam boyut</a>{f' · <a href="Playthroughs/{row["dark_capture"]}">Koyu tema</a>' if row['dark_capture'] else ''}</p></article>''')

arrows = [r['arrows'] for r in ROWS]
sizes = {(r['grid_width'], r['grid_height']) for r in ROWS}
overview_links = ''.join(f'<a href="Overview/shapes-{i + 1:02d}-{i + 10:02d}.jpg">{i + 1}–{i + 10}</a> ' for i in range(0, 50, 10))
page = f'''<!doctype html><html lang="tr"><meta charset="utf-8"><title>Shapes Revision3 Overview</title>
<style>body{{background:#eeeae1;color:#183350;font:16px system-ui;margin:28px}}h1{{margin-bottom:8px}}a{{color:#0065b5}}nav a{{display:inline-block;padding:10px 20px;background:white;border-radius:12px;margin:4px}}main{{display:grid;grid-template-columns:repeat(auto-fit,minmax(270px,1fr));gap:18px}}article{{background:white;border-radius:16px;padding:16px}}h2{{font-size:19px}}p{{font-size:14px}}img{{width:100%;height:auto;border-radius:12px}}header{{max-width:1000px;margin-bottom:24px}}</style>
<header><h1>Shapes — Revision3 Overview</h1><p>50 bölümün gerçek Unity sahnesi. {len(sizes)} farklı en/boy; {min(arrows)}–{max(arrows)} ok (ortalama {sum(arrows)/len(arrows):.1f}). Görsellere basarak tam boyut açabilirsin.</p>
<p>Gövde kalınlığı ve renk bölgeleri Shapes'e özel. 4 yönlü atış, tam silüet kapsamı ve hızlı çoklu atış korunur.</p>
<nav>{overview_links}<a href="Overview/shapes-dark.jpg">Koyu tema</a></nav>
<p><a href="Playthroughs/runtime-verification.txt">50 bölüm çalışma testi</a> · <a href="Playthroughs/level-metrics.csv">Bölüm ölçümleri</a></p></header><main>{''.join(cards)}</main></html>'''
(ROOT / 'index.html').write_text(page, encoding='utf-8')
readme = ['# Shapes Revision3 Overview', '',
          f'50 gerçek Unity sahnesi · {len(sizes)} farklı grid ölçüsü · {min(arrows)}–{max(arrows)} ok · ortalama {sum(arrows)/len(arrows):.1f} ok.', '',
          '[Bölüm bölüm tam boyut galeri](index.html) · [50 bölüm oynanış testi](Playthroughs/runtime-verification.txt) · [Ölçümler](Playthroughs/level-metrics.csv)', '']
for start in range(0, 50, 10):
    filename = f'shapes-{start + 1:02d}-{start + 10:02d}.jpg'
    readme += [f'## Bölüm {start + 1}–{start + 10}', '', f'![Bölüm {start + 1}–{start + 10}](Overview/{filename})', '']
readme += ['## Koyu tema', '', '![Koyu tema](Overview/shapes-dark.jpg)', '',
           'Tüm bölümler 4 yönde, tam silüet kapsamıyla ve eşzamanlı atışlarla test edildi. Görseller mockup değil, gerçek oyun sahnesinden alındı.', '']
(ROOT / 'README.md').write_text('\n'.join(readme), encoding='utf-8')
print(f'Built {len(ROWS)}-level Revision3 overview: {OUT}')
