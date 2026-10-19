"""Encode real GameScene captures using their measured production-speed timing."""
from pathlib import Path
from PIL import Image, ImageOps
import json
import shutil

ROOT = Path(__file__).resolve().parent
PROJECT = ROOT.parents[1]
manifest = dict(line.split('=', 1) for line in (ROOT / 'capture-manifest.txt').read_text().splitlines() if '=' in line)
source = PROJECT / manifest['directory']
paths = sorted(source.glob('frame-*.png'))
assert len(paths) == int(manifest['frames']) == 32
assert manifest['scene'] == 'GameScene' and manifest['timeScale'] == '1'
images = []
for path in paths:
    with Image.open(path) as image:
        assert image.size == (1080, 1920)
        images.append(image.convert('RGB'))
elapsed = float(manifest['elapsedSeconds']) * 1000
durations = [round((index + 1) * elapsed / len(images) / 10) * 10 - round(index * elapsed / len(images) / 10) * 10 for index in range(len(images))]


def encode(filename, frames):
    samples = Image.new('RGB', (540 * 4, 960 * 4))
    for index, frame in enumerate(frames[::2]):
        thumbnail = ImageOps.contain(frame, (540, 960), Image.Resampling.LANCZOS)
        samples.paste(thumbnail, ((index % 4) * 540, (index // 4) * 960))
    palette = samples.quantize(colors=256, dither=Image.Dither.NONE)
    converted = [frame.quantize(palette=palette, dither=Image.Dither.NONE) for frame in frames]
    converted[0].save(ROOT / filename, save_all=True, append_images=converted[1:], duration=durations, loop=0, optimize=False, disposal=2)
    with Image.open(ROOT / filename) as result:
        assert result.is_animated and result.n_frames == 32 and result.info.get('loop') == 0
        for frame in range(result.n_frames):
            result.seek(frame)
            result.load()


encode('gameplay.gif', [image.resize((540, 960), Image.Resampling.LANCZOS) for image in images])
encode('enemy-closeup.gif', [image.crop((80, 230, 1000, 650)) for image in images])
shutil.copyfile(paths[16], ROOT / 'gameplay.png')
(ROOT / 'capture-verification.json').write_text(json.dumps({'status': 'PASS', 'scene': 'GameScene', 'level': 101, 'timeScale': 1, 'source': manifest['directory'], 'decoded_pngs': 32, 'capture_seconds': elapsed / 1000, 'gif_frames': 32, 'preview_timing': 'Measured capture duration; sampled gameplay recording, not the runtime render frame rate'}, indent=2), encoding='utf-8')
print('PASS: 32 real GameScene PNGs decoded; full gameplay and close-up GIFs preserve measured capture timing.')
