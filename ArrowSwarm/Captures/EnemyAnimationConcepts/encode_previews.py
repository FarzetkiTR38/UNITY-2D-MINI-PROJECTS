"""Decode generated sprite atlases into looping GIF previews; original art remains unchanged."""
from pathlib import Path
from PIL import Image, ImageDraw, ImageFont, ImageOps
import json
import hashlib

ROOT = Path(__file__).resolve().parent
ITEMS = [
    ('01-kristal-hayalet', 'Kristal Hayalet', 2000),
    ('02-yaramaz-vizildayan', 'Yaramaz Vızıldayan', 1000),
    ('03-jole-kristal', 'Jöle Kristal', 1350),
]
CREAM = '#f3f0e6'
INK = '#272348'
FONT = 'C:/Windows/Fonts/arial.ttf'
BOLD = 'C:/Windows/Fonts/arialbd.ttf'
RECTANGLES = json.loads((ROOT / 'frame-rects.json').read_text(encoding='utf-8'))
(ROOT / 'frame-rects.js').write_text('window.ENEMY_FRAMES = ' + json.dumps(RECTANGLES) + ';\n', encoding='utf-8')


def frames_for(name):
    image = Image.open(ROOT / (name + '.png')).convert('RGBA')
    info = RECTANGLES[name + '.png']
    assert hashlib.sha256((ROOT / (name + '.png')).read_bytes()).hexdigest() == info['source_sha256']
    assert image.getchannel('A').getextrema() == (0, 255), 'Expected actual transparent art'
    frames = []
    for rectangle in info['rects']:
        x, y, w, h = (rectangle[key] for key in ('x', 'y', 'w', 'h'))
        trimmed = image.crop((x, y, x + w, y + h))
        frame = Image.new('RGBA', (info['max_width'] + 8, info['max_height'] + 8))
        frame.paste(trimmed, ((frame.width - w) // 2, (frame.height - h) // 2))
        assert frame.getchannel('A').getbbox() is not None
        frames.append(frame)
    return frames


def draw_sprite(canvas, frame, box):
    im = ImageOps.contain(frame, box[2:], Image.Resampling.LANCZOS)
    canvas.paste(im, (box[0] + (box[2] - im.width) // 2, box[1] + (box[3] - im.height) // 2), im)


def encode(path, frames, duration):
    # A shared palette avoids per-frame color shimmer during GIF playback.
    palette_source = Image.new('RGB', (360 * 4, 360 * 4), CREAM)
    for i, frame in enumerate(frames[:16]):
        tile = ImageOps.contain(frame, (360, 360), Image.Resampling.LANCZOS)
        palette_source.paste(tile, ((i % 4) * 360, (i // 4) * 360))
    palette = palette_source.quantize(colors=256, dither=Image.Dither.NONE)
    indexed = [frame.quantize(palette=palette, dither=Image.Dither.NONE) for frame in frames]
    durations = [round((i + 1) * duration / len(frames) / 10) * 10 - round(i * duration / len(frames) / 10) * 10 for i in range(len(frames))]
    indexed[0].save(path, save_all=True, append_images=indexed[1:], duration=durations, loop=0, optimize=False, disposal=2)
    with Image.open(path) as decoded:
        assert decoded.is_animated and decoded.n_frames == len(frames)
        for i in range(decoded.n_frames):
            decoded.seek(i)
            decoded.load()
        assert decoded.info.get('loop') == 0


all_frames = []
report = []
for name, title, cycle in ITEMS:
    source = frames_for(name)
    all_frames.append(source)
    rendered = []
    for frame in source:
        canvas = Image.new('RGB', (360, 360), CREAM)
        draw_sprite(canvas, frame, (16, 16, 328, 328))
        rendered.append(canvas)
    encode(ROOT / (name + '.gif'), rendered, cycle)
    report.append({'name': name, 'frames': 16, 'cycle_ms': cycle, 'gif_size': [360, 360]})

comparison = []
for tick in range(50):
    canvas = Image.new('RGB', (960, 470), CREAM)
    draw = ImageDraw.Draw(canvas)
    draw.text((22, 14), 'ARROW SWARM / ENEMY ANIMATION CONCEPTS', font=ImageFont.truetype(BOLD, 20), fill=INK)
    for column, (name, title, cycle) in enumerate(ITEMS):
        x = column * 320
        period = [2000, 1000, 4000 / 3][column]
        index = int((tick * 80 % period) / period * 16) % 16
        draw.text((x + 16, 58), f'{column + 1:02d}. {title}', font=ImageFont.truetype(BOLD, 19), fill=INK)
        draw_sprite(canvas, all_frames[column][index], (x + 20, 86, 280, 280))
        draw.text((x + 24, 385), '64 px', font=ImageFont.truetype(FONT, 12), fill='#736d87')
        for small in range(3):
            draw_sprite(canvas, all_frames[column][(index + small * 5) % 16], (x + 85 + small * 70, 376, 64, 64))
    comparison.append(canvas)
encode(ROOT / 'enemy-animation-comparison.gif', comparison, 4000)
(ROOT / 'preview-verification.json').write_text(json.dumps({'status': 'PASS', 'concepts': report, 'comparison_frames': 50, 'comparison_cycle_ms': 4000, 'source_atlases_unmodified': True}, indent=2), encoding='utf-8')
print('PASS: 3 sixteen-frame looping GIFs and 1 animated comparison decoded successfully.')
