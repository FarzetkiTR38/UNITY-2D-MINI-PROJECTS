"""Encode five Color Shift clips, check the real actions, and package the gallery."""
from __future__ import annotations

import argparse
import asyncio
from concurrent.futures import ThreadPoolExecutor
import csv
import json
import os
from pathlib import Path
import re
import shutil
import subprocess
import tempfile
import zipfile

import render_reference_reel as style
from colorshift_series_audio import build_mix, prepare_voices
from colorshift_series_edit import BRIEFS, EXISTING_L5, FPS, FRAMES, H, HUD, OUTRO, PACKAGE, PILOT, W, Episode, read, speech
from PIL import Image, ImageDraw

FFMPEG = style.imageio_ffmpeg.get_ffmpeg_exe()
ZIP = 'ArrowSwarm_ColorShift_Serisi_TR.zip'


def write(path, value):
    path.write_text(json.dumps(value, ensure_ascii=False, indent=2) + '\n', encoding='utf-8')


def run(*args):
    return subprocess.run([FFMPEG, '-hide_banner', *map(str, args)], check=True, capture_output=True, text=True,
                          encoding='utf-8', errors='replace')


def relative(path):
    return Path(os.path.relpath(path, PACKAGE)).as_posix()


def available_sources():
    sources = {5: EXISTING_L5}
    batch = PACKAGE / 'capture_batch.csv'
    if batch.exists():
        with batch.open(encoding='utf-8-sig', newline='') as stream:
            for row in csv.DictReader(stream):
                if row['status'] != 'PASS':
                    continue
                path = style.ROOT / row['source']
                if read(path / 'provenance.json')['won']:
                    sources[int(row['level'])] = path
    return sources


def decode_verify(path):
    reader = style.imageio_ffmpeg.read_frames(str(path))
    try:
        info = next(reader)
    finally:
        reader.close()
    assert (tuple(info['size']), info['fps'], info['duration']) == ((W, H), FPS, FRAMES / FPS), info
    result = run('-xerror', '-i', path, '-af', 'loudnorm=I=-17:TP=-2:LRA=8:print_format=json',
                 '-progress', 'pipe:1', '-f', 'null', '-')
    assert int(re.findall(r'^frame=(\d+)', result.stdout, re.M)[-1]) == FRAMES
    assert all(word in result.stderr for word in ('Video: h264', 'yuv420p', 'Audio: aac', '48000 Hz, stereo'))
    loud = json.JSONDecoder().raw_decode(result.stderr[result.stderr.rfind('{'):])[0]
    assert -18.5 < float(loud['input_i']) < -15.4 and float(loud['input_tp']) < -1, loud
    return {'frames': FRAMES, 'integratedLUFS': float(loud['input_i']), 'truePeakDbTP': float(loud['input_tp']),
            'sha256': style.sha(path)}


def encode(episode, audio, destination, work):
    command = [FFMPEG, '-hide_banner', '-loglevel', 'error', '-y', '-f', 'rawvideo', '-pixel_format', 'rgb24',
               '-video_size', f'{W}x{H}', '-framerate', str(FPS), '-i', 'pipe:0', '-i', str(audio),
               '-map', '0:v:0', '-map', '1:a:0', '-vf',
               'scale=in_range=pc:out_range=tv:out_color_matrix=bt709,format=yuv420p', '-c:v', 'libx264',
               '-preset', 'veryfast', '-crf', '18', '-threads', '3', '-color_range', 'tv', '-colorspace', 'bt709',
               '-color_primaries', 'bt709', '-color_trc', 'bt709', '-c:a', 'aac', '-b:a', '192k', '-ar', '48000',
               '-t', str(FRAMES / FPS), '-movflags', '+faststart', str(destination)]
    log = work / 'encode.log'
    with log.open('wb') as stream, subprocess.Popen(command, stdin=subprocess.PIPE, stderr=stream) as process:
        try:
            for index in range(FRAMES):
                process.stdin.write(episode.frame(index).tobytes())
        except BaseException:
            process.kill()
            raise
        finally:
            process.stdin.close()
        if process.wait():
            raise RuntimeError(log.read_text(encoding='utf-8', errors='replace'))


def render(episode, reuse):
    result = episode.out / episode.name
    poster = episode.out / 'cover.jpg'
    qa = episode.out / 'qa.json'
    if reuse and result.is_file() and poster.is_file() and qa.is_file():
        old = read(qa)
        if old['sha256'] == style.sha(result) and old['nativeSource'] == episode.source.name:
            print(f'Reuse verified L{episode.level:02}', flush=True)
            return old
    cues = [{'event': event, 'time': episode.event_time(frame)} for event, frame, _ in episode.events]
    with tempfile.TemporaryDirectory(prefix=f'ArrowSwarm-ColorShift{episode.level}-') as directory:
        work = Path(directory)
        mix, audio = build_mix(FFMPEG, episode.out, work, FRAMES / FPS, OUTRO / FPS, episode.voice, cues)
        draft = work / episode.name
        encode(episode, mix, draft, work)
        stats = decode_verify(draft)
        pending = result.with_suffix('.partial')
        shutil.copyfile(draft, pending)
        pending.replace(result)
    run('-v', 'error', '-y', '-ss', '1', '-i', result, '-frames:v', 1, '-vf', 'scale=432:768', '-q:v', 2, poster)
    for file in ('events.csv', 'provenance.json', 'state_checks.txt'):
        shutil.copy2(episode.source / file, episode.out / file)
    row = {'level': episode.level, 'title': episode.title, 'note': episode.note, 'file': relative(result),
           'poster': relative(poster), 'durationSeconds': FRAMES / FPS, 'existing': False,
           'nativeSource': episode.source.name, 'arrows': len(episode.fires), 'turns': len(episode.turns),
           'turnTimes': episode.turn_times, 'finalShot': episode.event_time(episode.fires[-1]),
           'winTime': episode.event_time(episode.win), 'outroTime': OUTRO / FPS,
           'hudBands': HUD, 'segments': [vars(segment) for segment in episode.segments], 'audio': audio, **stats}
    write(qa, row)
    print(f'PASS L{episode.level:02}: {row["arrows"]} arrows, {row["turns"]} turns, {stats["integratedLUFS"]} LUFS', flush=True)
    return row


def contact_sheet(rows):
    width, height = 162, 288
    sheet = Image.new('RGB', (width * 6, (height + 23) * len(rows)), '#0c1830')
    draw = ImageDraw.Draw(sheet)
    with tempfile.TemporaryDirectory(prefix='ArrowSwarm-ColorShift-QA-') as directory:
        frame = Path(directory) / 'sample.jpg'
        for row_index, row in enumerate(rows):
            turn = row['turnTimes'][0]
            times = (.85, min(4.3, turn - .35), turn + .28, row['finalShot'] + .15, 13.2, 17.85)
            y = row_index * (height + 23)
            for i, time in enumerate(times):
                run('-v', 'error', '-y', '-ss', time, '-i', PACKAGE / row['file'], '-frames:v', 1,
                    '-vf', f'scale={width}:{height}', '-q:v', 2, frame)
                with Image.open(frame) as still:
                    sheet.paste(still, (i * width, y + 23))
                draw.text((i * width + 6, y + 5), f'L{row["level"]:02}  {time:.2f}s', fill='white')
    sheet.save(PACKAGE / 'encoded_contact_sheet.jpg', quality=90)


def package(rows, pilot_sha):
    assert style.sha(PILOT) == pilot_sha
    original = {'level': 3, 'title': 'Çıkış rengini eşleştir', 'note': 'Serinin ilk ColorShift videosu.',
                'file': relative(PILOT), 'poster': relative(PILOT.with_name('cover.jpg')),
                'durationSeconds': 21, 'existing': True, 'sha256': pilot_sha}
    series = sorted(rows + [original], key=lambda row: row['level'])
    write(PACKAGE / 'manifest.json', series)
    keys = ('level', 'title', 'note', 'file', 'poster', 'durationSeconds', 'existing')
    gallery = [{key: row[key] for key in keys} for row in series]
    (PACKAGE / 'manifest.js').write_text('const COLORSHIFT_SERIES = '
        + json.dumps(gallery, ensure_ascii=False, indent=2) + ';\n', encoding='utf-8')
    contact_sheet(rows)
    path = PACKAGE / ZIP
    with zipfile.ZipFile(path, 'w', compression=zipfile.ZIP_STORED) as archive:
        for row in rows:
            file = PACKAGE / row['file']
            archive.write(file, file.name)
    with zipfile.ZipFile(path) as archive:
        assert len(archive.namelist()) == 5 and archive.testzip() is None
    write(PACKAGE / 'QA_REPORT.json', {'new': [row['level'] for row in rows], 'pilotSha256': pilot_sha,
        'decodedFrames': sum(row['frames'] for row in rows), 'zip': {'file': ZIP, 'sha256': style.sha(path)}})


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--ready-only', action='store_true')
    parser.add_argument('--reuse', action='store_true')
    args = parser.parse_args()
    sources = available_sources()
    if not args.ready_only and set(sources) != set(BRIEFS):
        raise RuntimeError(f'Missing verified native source levels: {set(BRIEFS) - set(sources)}')
    pilot_sha = style.sha(PILOT)
    asyncio.run(prepare_voices({level: speech(level) for level in BRIEFS}, PACKAGE, FFMPEG))
    episodes = [Episode(level, sources[level]) for level in sorted(sources)]
    with ThreadPoolExecutor(max_workers=2) as pool:
        rows = list(pool.map(lambda entry: render(entry, args.reuse), episodes))
    if len(rows) == len(BRIEFS):
        package(rows, pilot_sha)


if __name__ == '__main__':
    main()
