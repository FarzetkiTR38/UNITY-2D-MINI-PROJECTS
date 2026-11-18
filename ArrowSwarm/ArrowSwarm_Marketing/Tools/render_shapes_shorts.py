"""Render and verify six portrait shorts from the existing Shapes captures."""
from __future__ import annotations

import argparse
import asyncio
from concurrent.futures import ThreadPoolExecutor
import json
import os
from pathlib import Path
import re
import shutil
import subprocess
import tempfile
import zipfile

import render_reference_reel as style
from shapes_shorts_audio import build_mix, prepare_voices
from shapes_shorts_edit import CONFIG, FIRE, FINAL, FPS, FRAMES, H, HUD, OUTRO, PACKAGE, REELS, RUSH, W, Episode
from PIL import Image, ImageDraw

FFMPEG = style.imageio_ffmpeg.get_ffmpeg_exe()
ZIP_NAME = 'ArrowSwarm_Shapes_Kisa_Surumler_TR.zip'


def run(*args):
    result = subprocess.run([FFMPEG, '-hide_banner', *map(str, args)], capture_output=True, text=True,
                            encoding='utf-8', errors='replace')
    if result.returncode:
        raise RuntimeError(result.stderr[-3000:])
    return result


def relative(path):
    return Path(os.path.relpath(path, PACKAGE)).as_posix()


def metadata(file):
    reader = style.imageio_ffmpeg.read_frames(str(file))
    try:
        return next(reader)
    finally:
        reader.close()


def render(episode):
    with tempfile.TemporaryDirectory(prefix=f'ArrowSwarm-Shapes{episode.level}-') as directory:
        work = Path(directory)
        mix, audio = build_mix(episode, FFMPEG, work)
        staged = work / episode.name
        command = [FFMPEG, '-hide_banner', '-loglevel', 'error', '-y',
                   '-f', 'rawvideo', '-pixel_format', 'rgb24', '-video_size', f'{W}x{H}',
                   '-framerate', str(FPS), '-i', 'pipe:0', '-i', str(mix),
                   '-map', '0:v:0', '-map', '1:a:0', '-vf',
                   'scale=in_range=pc:out_range=tv:out_color_matrix=bt709,format=yuv420p',
                   '-c:v', 'libx264', '-preset', 'veryfast', '-crf', '18', '-threads', '3',
                   '-color_range', 'tv', '-colorspace', 'bt709', '-color_primaries', 'bt709', '-color_trc', 'bt709',
                   '-c:a', 'aac', '-b:a', '192k', '-ar', '48000', '-t', '16', '-movflags', '+faststart', str(staged)]
        with (work / 'encode.log').open('wb') as log:
            with subprocess.Popen(command, stdin=subprocess.PIPE, stderr=log) as encoder:
                try:
                    for index in range(FRAMES):
                        encoder.stdin.write(episode.frame(index).tobytes())
                except Exception:
                    encoder.kill()
                    raise
                finally:
                    encoder.stdin.close()
                if encoder.wait():
                    raise RuntimeError((work / 'encode.log').read_text(encoding='utf-8', errors='replace'))

        report = verify(staged)
        result = episode.output / episode.name
        pending = result.with_suffix('.partial')
        shutil.copyfile(staged, pending)
        pending.replace(result)
        poster = episode.output / 'cover.jpg'
        run('-v', 'error', '-y', '-ss', '2.1', '-i', result, '-frames:v', '1', '-vf', 'scale=432:768', '-q:v', '2', poster)
        timings = {'firstShot': FIRE / FPS, 'firstSettled': episode.event_time(episode.settled),
                   'acceleratedSection': [RUSH / FPS, FINAL / FPS], 'speed': episode.speed,
                   'finalShot': episode.event_time(episode.fires[-1]), 'win': episode.event_time(episode.won),
                   'outro': OUTRO / FPS}
        row = {'level': episode.level, 'title': episode.shape, 'file': relative(result),
               'original': relative(episode.original), 'originalSeconds': metadata(episode.original)['duration'],
               'durationSeconds': FRAMES / FPS, 'poster': relative(poster), 'arrows': episode.arrows,
               'source': episode.source.name, 'choicesAB': episode.choices, 'timings': timings, 'audio': audio, **report}
        row['originalSha256'] = style.sha(episode.original)
        row['nativeHudBandsCheckedBeforeEncode'] = list(HUD)
        (episode.output / 'qa.json').write_text(json.dumps(row, ensure_ascii=False, indent=2) + '\n', encoding='utf-8')
        print(f'PASS L{episode.level:02} | {row["originalSeconds"]} -> 16.0 s | {episode.rate_label} | {row["integratedLUFS"]} LUFS', flush=True)
        return row


def verify(file):
    info = metadata(file)
    assert (tuple(info['size']), info['fps'], info['duration']) == ((W, H), FPS, 16.0), info
    result = run('-xerror', '-i', file, '-af', 'loudnorm=I=-16:TP=-1.5:LRA=8:print_format=json',
                 '-progress', 'pipe:1', '-f', 'null', '-')
    frames = int(re.findall(r'^frame=(\d+)$', result.stdout, re.M)[-1])
    assert frames == FRAMES and all(text in result.stderr for text in ('Video: h264', 'yuv420p', 'Audio: aac', '48000 Hz, stereo'))
    levels = json.JSONDecoder().raw_decode(result.stderr[result.stderr.rfind('{'):])[0]
    lufs, peak = float(levels['input_i']), float(levels['input_tp'])
    assert -17.5 < lufs < -15.0 and peak < -1, (file, lufs, peak)
    return {'framesDecoded': frames, 'integratedLUFS': lufs, 'truePeakDbTP': peak, 'sha256': style.sha(file)}


def make_contact_sheet(rows):
    width, height = 162, 288
    moments = [.5, 2.1, 3.56, 4.52, 6.35, 8.1, 10.2, 14.8]
    sheet = Image.new('RGB', (width * len(moments), (height + 23) * len(rows)), '#0c1830')
    draw = ImageDraw.Draw(sheet)
    for row_number, row in enumerate(rows):
        top = row_number * (height + 23)
        with tempfile.TemporaryDirectory(prefix='ArrowSwarm-short-QA-') as directory:
            still = Path(directory) / 'frame.jpg'
            for column, seconds in enumerate(moments):
                run('-v', 'error', '-y', '-ss', seconds, '-i', PACKAGE / row['file'], '-frames:v', 1,
                    '-vf', f'scale={width}:{height}', '-q:v', 2, still)
                left = column * width
                with Image.open(still) as frame:
                    sheet.paste(frame, (left, top + 23))
                draw.text((left + 6, top + 5), f'L{row["level"]:02}  {seconds:.2f}s', fill='white')
    sheet.save(PACKAGE / 'encoded_contact_sheet.jpg', quality=91)


def package(rows, originals, before):
    manifest = json.dumps(rows, ensure_ascii=False, indent=2)
    (PACKAGE / 'manifest.json').write_text(manifest + '\n', encoding='utf-8')
    gallery = [{key: row[key] for key in ('level', 'title', 'file', 'original', 'originalSeconds',
                                          'durationSeconds', 'poster')} for row in rows]
    (PACKAGE / 'manifest.js').write_text('const SHAPES_SHORTS = ' + json.dumps(gallery, ensure_ascii=False, indent=2)
                                         + ';\n', encoding='utf-8')
    make_contact_sheet(rows)
    path = PACKAGE / ZIP_NAME
    with zipfile.ZipFile(path, 'w', compression=zipfile.ZIP_STORED) as archive:
        for row in rows:
            file = PACKAGE / row['file']
            archive.write(file, file.name)
    with zipfile.ZipFile(path) as archive:
        assert len(archive.namelist()) == 6 and archive.testzip() is None
    assert all(style.sha(file) == before[file] for file in originals)
    qa = {'levels': [row['level'] for row in rows], 'totalDecodedFrames': sum(row['framesDecoded'] for row in rows),
          'zip': {'name': path.name, 'files': 6, 'sha256': style.sha(path), 'bytes': path.stat().st_size},
          'preservedInputHashes': {file.name: before[file] for file in originals}}
    (PACKAGE / 'QA_REPORT.json').write_text(json.dumps(qa, indent=2) + '\n', encoding='utf-8')


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--level', type=int, action='append', choices=CONFIG)
    parser.add_argument('--input-dir', type=Path, help='Optionally also check the eight supplied files for preservation')
    parser.add_argument('--workers', type=int, default=2)
    args = parser.parse_args()
    episodes = [Episode(level) for level in (args.level or CONFIG)]
    reference = REELS / 'CreativeShort01_20261114/ArrowSwarm_Ahtapot_CreativeShort_TR.mp4'
    octopus = REELS / 'ShapesSeries_20261114/L21/ArrowSwarm_Shapes21_TR.mp4'
    originals = [episode.original for episode in episodes] + [reference, octopus]
    if args.input_dir:
        originals += [args.input_dir / path.name for path in originals]
    before = {file: style.sha(file) for file in originals}
    asyncio.run(prepare_voices(episodes))
    with ThreadPoolExecutor(max_workers=args.workers) as pool:
        rows = list(pool.map(render, episodes))
    if len(rows) == len(CONFIG) and set(row['level'] for row in rows) == set(CONFIG):
        package(rows, originals, before)


if __name__ == '__main__':
    main()
