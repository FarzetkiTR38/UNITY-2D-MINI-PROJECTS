"""Compose Revision4 review sheets exclusively from actual Unity gameplay captures."""
from pathlib import Path
from PIL import Image, ImageDraw, ImageFont, ImageOps
import csv
import html
import json

ROOT = Path(__file__).resolve().parent
SOURCE = ROOT / 'Playthroughs'
OUT = ROOT / 'Overview'
BEFORE = ROOT.parent / 'Revision3' / 'Playthroughs'
FONT_DIR = Path('C:/Windows/Fonts')
COMPARE_LEVELS = (1, 11, 12, 15, 20)


def font(size, bold=False):
    return ImageFont.truetype(str(FONT_DIR / ('arialbd.ttf' if bold else 'arial.ttf')), size)


def read_balance(path):
    with path.open(encoding='utf-8-sig', newline='') as stream:
        return {int(row['level']): row for row in csv.DictReader(stream)}


def mean(rows, name):
    return sum(float(row[name]) for row in rows.values()) / len(rows)


def crop_capture(source, row, dark=False):
    path = source / row['dark_capture' if dark else 'light_capture']
    with Image.open(path) as image:
        w, h = image.size
        return image.convert('RGB').crop((int(w * .015), int(h * .125), int(w * .985), int(h * .875)))


def paste_board(canvas, source, row, box, dark=False):
    image = ImageOps.contain(crop_capture(source, row, dark), box[2:], Image.Resampling.LANCZOS)
    canvas.paste(image, (box[0] + (box[2] - image.width) // 2, box[1] + (box[3] - image.height) // 2))


def balance_text(metric):
    return f"Açılış %{float(metric['initial_legal_ratio']) * 100:.0f} / alan ağırlıklı dönüş {float(metric['cell_weighted_bends']):.1f}"


def sheet(rows, name, balance, dark=False):
    cols, tile_w, tile_h, gap, top = 5, 430, 675, 18, 100
    count_rows = (len(rows) + cols - 1) // cols
    background, foreground = ('#111d32', '#e8f4ff') if dark else ('#e8e5da', '#193353')
    canvas = Image.new('RGB', (cols * (tile_w + gap) + gap, top + count_rows * (tile_h + gap)), background)
    draw = ImageDraw.Draw(canvas)
    draw.text((gap, 18), 'ARROW SWARM / SHAPES / REVISION 4', font=font(30, True), fill=foreground)
    draw.text((gap, 58), 'Gerçek Unity sahneleri / 4 yön / dolu silüet / birbirini açan oklar', font=font(20), fill=foreground)
    for i, row in enumerate(rows):
        x, y = gap + (i % cols) * (tile_w + gap), top + (i // cols) * (tile_h + gap)
        paste_board(canvas, SOURCE, row, (x, y + 105, tile_w, tile_h - 110), dark)
        draw.text((x + 4, y + 2), f"{row['level']:02d}. {row['title']}", font=font(23, True), fill=foreground)
        draw.text((x + 4, y + 34), f"{row['grid_width']} × {row['grid_height']} / {row['arrows']} ok / {row['colors']} renk", font=font(19), fill=foreground)
        draw.text((x + 4, y + 59), balance_text(balance[row['level']]), font=font(17, True), fill=foreground)
        draw.text((x + 4, y + 82), f"Bağımlılık {row['dependency_depth']} / çizgi {row['stroke_pixels']:.1f}px", font=font(17), fill=foreground)
    canvas.save(OUT / name, quality=95, subsampling=0)


def comparison(rows, balance, baseline):
    old = {row['level']: row for row in json.loads((BEFORE / 'level-metrics.json').read_text(encoding='utf-8'))}
    new = {row['level']: row for row in rows}
    gap, tile_w, tile_h, top = 22, 520, 700, 105
    canvas = Image.new('RGB', (2 * tile_w + 3 * gap, top + len(COMPARE_LEVELS) * (tile_h + gap)), '#e8e5da')
    draw = ImageDraw.Draw(canvas)
    draw.text((gap, 16), 'SHAPES / REVISION 3 → REVISION 4', font=font(29, True), fill='#193353')
    draw.text((gap, 57), 'Aynı silüet, renk alanları ve kalınlık; yeniden tasarlanan ok yolları', font=font(21), fill='#193353')
    for index, level in enumerate(COMPARE_LEVELS):
        y = top + index * (tile_h + gap)
        for column, (source, row, metrics, label) in enumerate(((BEFORE, old[level], baseline[level], 'REVISION 3'), (SOURCE, new[level], balance[level], 'REVISION 4'))):
            x = gap + column * (tile_w + gap)
            draw.text((x + 4, y + 2), f"{level:02d}. {row['title']} / {label}", font=font(23, True), fill='#193353')
            draw.text((x + 4, y + 36), balance_text(metrics), font=font(19), fill='#193353')
            paste_board(canvas, source, row, (x, y + 72, tile_w, tile_h - 80))
    canvas.save(OUT / 'shapes-before-after.jpg', quality=95, subsampling=0)


def build():
    rows = json.loads((SOURCE / 'level-metrics.json').read_text(encoding='utf-8'))
    balance = read_balance(ROOT / 'balance-candidates.csv')
    baseline = read_balance(ROOT / 'balance-baseline.csv')
    assert len(rows) == 50 and set(balance) == set(range(1, 51)), '50 yeni bölüm ve denge ölçümü gerekli.'
    rows.sort(key=lambda row: row['level'])
    for row in rows:
        metric = balance[row['level']]
        assert row['arrows'] == int(metric['arrows']), f"{row['level']}: sahne görüntüsü yeni ok sayısıyla eşleşmiyor."
        assert row['covered_cells'] == int(metric['cells']), f"{row['level']}: sahne hücre sayısı denge raporuyla eşleşmiyor."
    OUT.mkdir(exist_ok=True)
    for start in range(0, len(rows), 10):
        sheet(rows[start:start + 10], f'shapes-{start + 1:02d}-{min(start + 10, len(rows)):02d}.jpg', balance)
    sheet([row for row in rows if row['dark_capture']], 'shapes-dark.jpg', balance, True)
    comparison(rows, balance, baseline)
    summary = (f"Açılışta seçilebilen okların ortalaması %{mean(baseline, 'initial_legal_ratio') * 100:.1f} → %{mean(balance, 'initial_legal_ratio') * 100:.1f}; "
               f"çözümün orta kısmındaki serbest seçenek oranı %{mean(baseline, 'mid_choice_area') * 100:.1f} → %{mean(balance, 'mid_choice_area') * 100:.1f}. "
               'Daha fazla ok, önce başka okların çıkarılmasını gerektiriyor; tüm bölümler çözülebilir kalıyor.')
    path_summary = (f"Alan ağırlıklı dönüş sayısı {mean(baseline, 'cell_weighted_bends'):.2f} → {mean(balance, 'cell_weighted_bends'):.2f}. "
                    'Bu ölçüm uzun yolları daha fazla ağırlıklandırır; küçük kıvrımlı oklar sonucu şişirmez. '
                    'R2’nin çok sık kıvrımları ile R3’ün uzun paralel şeritleri arasında daha okunaklı yollar hedeflendi.')
    early, late = {k: v for k, v in balance.items() if k <= 10}, {k: v for k, v in balance.items() if k > 40}
    progression = f"İlk 10 bölümde açılış seçenekleri ortalama %{mean(early, 'initial_legal_ratio') * 100:.1f}; son 10 bölümde %{mean(late, 'initial_legal_ratio') * 100:.1f}. Daha az serbest seçenek bırakan kademeli bir ilerleme hedeflendi."
    overview_links = ''.join(f'<a href="Overview/shapes-{i + 1:02d}-{i + 10:02d}.jpg">{i + 1}–{i + 10}</a> ' for i in range(0, 50, 10))
    reports = [('balance-comparison.txt', 'Denge doğrulaması'), ('balance-candidates.csv', 'Yeni denge ölçümleri'), ('balance-baseline.csv', 'R3 başlangıç ölçümleri'),
               ('balance-legacy-summary.json', 'R2 karşılaştırması'), ('policy-comparison.txt', '12 farklı çözüm sırası'),
               ('Playthroughs/runtime-verification.txt', '50 bölüm oynanış testi'), ('Playthroughs/level-metrics.csv', 'Sahne ölçümleri'),
               ('interaction-verification.txt', 'Hızlı atış ve etkileşim testleri'), ('ui-verification.txt', 'HUD ve yerelleştirme testleri')]
    report_links = ' · '.join(f'<a href="{path}">{label}</a>' for path, label in reports)
    cards = []
    for row in rows:
        title = html.escape(row['title'])
        light, dark = row['light_capture'], row['dark_capture']
        dark_link = f' · <a href="Playthroughs/{dark}">Koyu tema</a>' if dark else ''
        cards.append(f'''<article><h2>{row['level']:02d}. {title}</h2>
<p>{row['grid_width']} × {row['grid_height']} · {row['arrows']} ok · {row['colors']} renk · bağımlılık {row['dependency_depth']}</p>
<p class="metric">{balance_text(balance[row['level']])}</p>
<a href="Playthroughs/{light}"><img loading="lazy" src="Playthroughs/{light}" alt="{title}"></a>
<p><a href="Playthroughs/{light}">Tam boyut</a>{dark_link}</p></article>''')
    arrows = [row['arrows'] for row in rows]
    sizes = {(row['grid_width'], row['grid_height']) for row in rows}
    page = f'''<!doctype html><html lang="tr"><meta charset="utf-8"><meta name="viewport" content="width=device-width,initial-scale=1"><title>Shapes Revision4 Overview</title>
<style>body{{background:#eeeae1;color:#183350;font:16px system-ui;margin:28px}}h1{{margin-bottom:8px}}a{{color:#0065b5}}nav a{{display:inline-block;padding:10px 20px;background:white;border-radius:12px;margin:4px}}main{{display:grid;grid-template-columns:repeat(auto-fit,minmax(280px,1fr));gap:18px}}article{{background:white;border-radius:16px;padding:16px}}h2{{font-size:19px}}p{{font-size:15px;line-height:1.55}}.metric{{font-weight:700;color:#075c8c}}img{{width:100%;height:auto;border-radius:12px}}header{{max-width:1100px;margin-bottom:24px}}</style>
<header><h1>Shapes — Revision4 Overview</h1><p>50 bölümün gerçek Unity sahnesi. {len(sizes)} farklı en/boy; {min(arrows)}–{max(arrows)} ok (ortalama {sum(arrows)/len(arrows):.1f}). Görsellere basarak tam boyut açabilirsin.</p>
<p>{summary}</p><p>{path_summary}</p><p>{progression} Bu oranlar yapısal ölçümlerdir; hissedilen zorluk oyuncunun çözüm sırasına göre değişir.</p>
<p>R3’ün silüetleri, her hücrenin rengi, harita boyutları ve Shapes’e özel çizgi kalınlığı korunur. Dört yönlü ve eşzamanlı atış devam eder.</p>
<nav><a href="Overview/shapes-before-after.jpg">R3 / R4 karşılaştırma</a>{overview_links}<a href="Overview/shapes-dark.jpg">Koyu tema</a></nav>
<p>{report_links}</p></header><main>{''.join(cards)}</main></html>'''
    (ROOT / 'index.html').write_text(page, encoding='utf-8')
    readme = ['# Shapes Revision4 Overview', '', f'50 gerçek Unity sahnesi · {len(sizes)} farklı grid ölçüsü · {min(arrows)}–{max(arrows)} ok · ortalama {sum(arrows)/len(arrows):.1f} ok.', '',
              '[Tam boyut galeri](index.html) · [R3 / R4 karşılaştırma](Overview/shapes-before-after.jpg)', '', summary, '', path_summary, '', progression, '',
              'R3 silüeti, hücre renkleri, boyutları ve ok kalınlığı korunur. Açılış yüzdesi ilk hamlede çıkabilen okların oranıdır. Bu oranlar yapısal ölçümlerdir; hissedilen zorluk oyuncunun çözüm sırasına göre değişir.', '',
              '## R3 → R4 karşılaştırması', '', '1, 11, 12, 15 ve 20. bölümlerin aynı şekilde kırpılmış gerçek Unity görüntüleri.', '',
              '![R3 ve R4 karşılaştırması](Overview/shapes-before-after.jpg)', '']
    for start in range(0, 50, 10):
        name = f'shapes-{start + 1:02d}-{start + 10:02d}.jpg'
        readme += [f'## Bölüm {start + 1}–{start + 10}', '', f'![Bölüm {start + 1}–{start + 10}](Overview/{name})', '']
    readme += ['## Koyu tema', '', '![Koyu tema](Overview/shapes-dark.jpg)', '', '## Doğrulama raporları', '']
    readme += [f'- [{label}]({path})' for path, label in reports]
    readme += ['', 'Görseller mockup değil, gerçek oyun sahnesinden alınmıştır. 50 bölüm tam silüet kapsamı, çözülebilirlik ve hızlı çoklu atışlarla doğrulanır.', '']
    (ROOT / 'README.md').write_text('\n'.join(readme), encoding='utf-8')
    print(f'Built {len(rows)}-level Revision4 overview: {OUT}')


if __name__ == '__main__':
    build()
