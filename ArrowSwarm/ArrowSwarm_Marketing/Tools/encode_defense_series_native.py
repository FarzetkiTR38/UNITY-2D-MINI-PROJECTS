"""Encode a complete new Defense source into its scoped episode review folder."""
from __future__ import annotations
import argparse
from pathlib import Path
import re

from check_defense_series import MEDIA, ROOT, SERIES, read, sha, write
from check_defense_series_delivery import command
import imageio_ffmpeg
from PIL import Image, ImageDraw, ImageFont


def encode(source, output):
    source, output = source.resolve(), output.resolve()
    assert source.is_relative_to((MEDIA / '04_Raw_Gameplay').resolve())
    assert output.is_relative_to(SERIES.resolve())
    p, proof = read(source / 'provenance.json'), read(output / 'native_capture_checks.json')
    assert p['allFramesSaved'] and proof['passed'] and not proof['trialOnly']
    assert proof['source'] == source.name and proof['sourceProvenanceSha256'] == sha(source / 'provenance.json')
    count, fps = p['frames'], p['fps']
    assert (p['width'], p['height'], fps) == (1080, 1920, 30)
    assert all((source / f'frame_{i:05d}.jpg').is_file() for i in range(count))
    target = output / 'native_source_silent.mp4'; ff = imageio_ffmpeg.get_ffmpeg_exe()
    temporary = output / 'native_source_silent.encoding.mp4'
    args = [ff, '-hide_banner', '-y', '-framerate', str(fps), '-start_number', '0',
        '-i', str(source / 'frame_%05d.jpg'), '-frames:v', str(count),
        '-vf', 'scale=iw:ih:out_color_matrix=bt709:out_range=tv,format=yuv420p',
        '-c:v', 'libx264', '-preset', 'fast', '-crf', '20', '-threads', '2',
        '-pix_fmt', 'yuv420p', '-color_range', 'tv', '-colorspace', 'bt709',
        '-color_primaries', 'bt709', '-color_trc', 'bt709', '-an', '-movflags', '+faststart',
        '-metadata', 'comment=Complete genuine Unity Defense frames in order; no crop, overlay or recorded audio.', str(temporary)]
    command(args, output / 'native_source_encode.log')
    decoded = command([ff, '-hide_banner', '-i', str(temporary), '-map', '0:v:0',
        '-f', 'null', '-', '-progress', 'pipe:1'], output / 'native_source_decode.log')
    assert int(re.findall(r'^frame=(\d+)$', decoded.stdout, re.M)[-1]) == count
    assert all(x in decoded.stderr for x in ('1080x1920', 'h264', 'yuv420p'))
    assert 'Audio:' not in decoded.stderr
    temporary.replace(target)
    beats = [('Opening', proof['openingFrame']), ('First shot', proof['firstShotFrame']),
             ('Rainbow', proof['rainbowFrame'] + 5), ('3-star result', proof['winFrame'] + 96)]
    assert all(0 <= frame < count for _, frame in beats)
    sheet = Image.new('RGB', (1080, 530), '#09172c'); draw = ImageDraw.Draw(sheet)
    font = ImageFont.truetype(str(ROOT / 'Assets/_Project/Fonts/Farzetki_Title_Font_H1.ttf'), 19)
    for i, (name, frame) in enumerate(beats):
        with Image.open(source / f'frame_{frame:05d}.jpg') as image:
            thumb = image.convert('RGB').resize((270, 480), Image.Resampling.LANCZOS)
        sheet.paste(thumb, (i * 270, 40))
        draw.text((i * 270 + 7, 9), f'{name} / f{frame}', fill='white', font=font)
    sheet.save(output / 'native_contact_sheet.jpg', quality=92, subsampling=0)
    report = {'passed': True, 'file': target.name, 'source': source.name, 'level': p['level'],
        'frames': count, 'decodedFrames': count, 'fps': fps, 'durationSeconds': count / fps,
        'width': 1080, 'height': 1920, 'videoCodec': 'h264', 'pixelFormat': 'yuv420p',
        'audio': 'None: native frame capture did not record Unity audio', 'editorialOverlay': False,
        'allSourceFramesEncodedInOrder': True, 'croppedOrScaled': False, 'lossyCodec': True,
        'threads': 2, 'crf': 20, 'bytes': target.stat().st_size, 'sha256': sha(target),
        'sourceProvenanceSha256': sha(source / 'provenance.json'),
        'nativeContactSheet': 'native_contact_sheet.jpg', 'contactSheetSourceFrames': [x[1] for x in beats]}
    write(output / 'native_source_silent_QA.json', report)
    return report


if __name__ == '__main__':
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--source', type=Path, required=True)
    parser.add_argument('--output', type=Path, required=True)
    args = parser.parse_args()
    import json
    print(json.dumps(encode(args.source, args.output), indent=2), flush=True)
