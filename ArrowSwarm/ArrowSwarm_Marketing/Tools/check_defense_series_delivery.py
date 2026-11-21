"""Independent complete-decode, timing, asset and sound checks for five new reels."""
from __future__ import annotations
import argparse
import json
from pathlib import Path
import re
import subprocess
import tempfile

from check_defense_series import CTA_SHA, MEDIA, SERIES, read, sha, write
import imageio_ffmpeg
from PIL import Image, ImageDraw, ImageFont


def command(args, log):
    result = subprocess.run(args, capture_output=True, text=True, encoding='utf-8', errors='replace')
    log.parent.mkdir(parents=True, exist_ok=True)
    log.write_text(result.stdout + result.stderr, encoding='utf-8')
    assert result.returncode == 0, f'FFmpeg failed: {log.name}'
    return result


def extract_visual_review(video, qa, package, ff):
    """Extract actual encoded beats independently; never substitute composed stills."""
    level = qa['level']; folder = Path(tempfile.gettempdir()) / f'ArrowSwarm-DefenseSeries-QA-L{level:02d}'
    folder.mkdir(parents=True, exist_ok=True)
    times = [.35, qa['firstShotAt'] - .15, qa['firstShotAt'] + .45,
             qa['rainbowAt'] - .25, qa['rainbowAt'] + .7,
             qa['naturalWinAt'] + .35, qa['naturalWinAt'] + 1.8,
             qa['promoAt'] - .15, qa['promoAt'] + 1.2, qa['buttonPressAt'] + .1]
    if qa['tipUsed']:
        times[1] = qa['tipAt'] + .5
    sheet = Image.new('RGB', (1080, 824), '#071328'); draw = ImageDraw.Draw(sheet)
    from check_defense_series import ROOT
    font = ImageFont.truetype(str(ROOT / 'Assets/_Project/Fonts/Farzetki_Title_Font_H1.ttf'), 15)
    metadata = []
    for i, time in enumerate(times):
        assert 0 <= time < qa['durationSeconds']
        target = folder / f'encoded_{i:02d}.jpg'
        command([ff, '-hide_banner', '-y', '-ss', str(time), '-i', str(video),
                 '-frames:v', '1', '-q:v', '2', '-update', '1', str(target)], folder / f'extract_{i:02d}.log')
        with Image.open(target) as frame:
            frame.load(); assert frame.size == (1080, 1920)
            sheet.paste(frame.convert('RGB').resize((216, 384), Image.Resampling.LANCZOS),
                        ((i % 5) * 216, 25 + (i // 5) * 412))
        draw.text(((i % 5) * 216 + 6, 5 + (i // 5) * 412), f'{time:.2f}s', font=font, fill='white')
        metadata.append({'seconds': time, 'sha256': sha(target), 'bytes': target.stat().st_size})
    out = package / 'IndependentQA'; out.mkdir(parents=True, exist_ok=True)
    sheet.save(out / f'L{level:02d}_encoded_beats.jpg', quality=92, subsampling=0)
    # Keep the full-size settled native outcome as the most important visual proof.
    (out / f'L{level:02d}_settled_result.jpg').write_bytes((folder / 'encoded_07.jpg').read_bytes())
    if qa['tipUsed']:
        (out / f'L{level:02d}_tip_active.jpg').write_bytes((folder / 'encoded_01.jpg').read_bytes())
    return metadata


def verify(package=SERIES):
    package = package.resolve()
    assert package == SERIES.resolve(), 'Restrict checks to the new series delivery'
    items = read(package / 'series_manifest.json')['items']
    assert len(items) == 5 and len({x['level'] for x in items}) == 5
    ff = imageio_ffmpeg.get_ffmpeg_exe(); results = []
    for item in items:
        level = item['level']; folder = package / f'L{level:02d}'
        video = (package / item['file']).resolve()
        assert video.is_relative_to(package) and video.is_file()
        qa = read(folder / 'QA_REPORT.json'); native = read(folder / 'native_capture_checks.json')
        provenance = read(folder / 'source_provenance.json'); timeline = read(folder / 'timeline.json')
        audio = read(folder / 'audio_timeline.json')
        assert qa['passed'] and native['passed'] and native['allFramesSaved'] and not native['trialOnly']
        assert qa['sha256'] == sha(video) and qa['bytes'] == video.stat().st_size
        assert qa['source'] == native['source'] and provenance['level'] == level == native['level']
        assert native['sourceProvenanceSha256'] == sha(folder / 'source_provenance.json')
        source = MEDIA / '04_Raw_Gameplay' / qa['source']
        assert source.resolve().is_relative_to((MEDIA / '04_Raw_Gameplay').resolve())
        assert sha(source / 'provenance.json') == native['sourceProvenanceSha256']
        state = (source / 'state_checks.txt').read_text(encoding='utf-8-sig')
        assert 'PASS' in state and 'FAIL' not in state
        assert abs(native['captureCameraZoomOut'] - 1.12) < .00001
        assert qa['freezeUsed'] is False and native['freezeUsed'] is False
        assert qa['tipUsed'] == native['tipUsed'] == item['tipUsed']
        assert qa['victoryCleanupKills'] == qa['mobFinishes'] == native['victoryCleanupKills'] == 0
        assert qa['spawnedMobs'] == qa['projectileKills'] == native['spawnedMobs'] == native['projectileKills'] > 0
        assert qa['lives'] == 3 and qa['arrows'] == native['arrows']
        seconds = float(qa['durationSeconds']); expected = round(seconds * 30)
        assert 18 <= seconds <= 25 and len(timeline) == expected == qa['frames']
        assert (qa['width'], qa['height'], qa['fps']) == (1080, 1920, 30)
        assert qa['firstShotAt'] < qa['rainbowAt'] < qa['naturalWinAt'] < qa['promoAt'] < seconds
        if qa['tipUsed']:
            assert 0 <= qa['tipAt'] < qa['firstShotAt']
        else:
            assert qa['tipAt'] is None or qa['tipAt'] < 0
        voices = audio['voices']
        assert voices and voices[-1]['sha256'] == CTA_SHA
        assert all(sha(MEDIA / x['file']) == x['sha256'] for x in voices)
        assert all(0 <= x['start'] < x['end'] < seconds for x in voices)
        assert all(a['end'] <= b['start'] for a, b in zip(voices, voices[1:]))
        texts = ' '.join(x['text'] for x in voices).casefold().replace('\u0307', '')
        assert 'dondur' not in texts and 'freeze' not in texts
        assert ('ipucu' in texts) == bool(qa['tipUsed'])
        assert voices[-1]['start'] >= qa['promoAt'] and voices[-1]['end'] < qa['buttonPressAt']
        assert qa['buttonPressAt'] < seconds
        subtitle = (folder / 'captions_TR.srt').read_text(encoding='utf-8')
        assert 'Play Store’dan' in subtitle and 'piley' not in subtitle and 'biley' not in subtitle
        assert len(re.findall(' --> ', subtitle)) == len(voices)
        assert qa['storeStatus'] == 'mock' and qa['publicStoreUrl'] is None
        assert qa['nativeHudBandsIdenticalBeforeEncode'] == [[0, 0, 1080, 230], [0, 1690, 1080, 1920]]
        assert qa['speedChangesLabelled'] is True
        result = command([ff, '-hide_banner', '-i', str(video), '-f', 'null', '-', '-progress', 'pipe:1'],
                         package / 'IndependentQA' / f'L{level:02d}_decode.log')
        frames = int(re.findall(r'^frame=(\d+)$', result.stdout, re.M)[-1])
        assert frames == expected
        assert all(x in result.stderr for x in ('1080x1920', 'yuv420p', '48000 Hz, stereo', 'h264', 'aac'))
        sound = command([ff, '-hide_banner', '-i', str(video), '-vn', '-af',
                         'loudnorm=I=-16:TP=-1.5:LRA=8:print_format=json', '-f', 'null', '-'],
                        package / 'IndependentQA' / f'L{level:02d}_loudness.log')
        levels = json.loads(re.findall(r'\{\s*"input_i".*?\}', sound.stderr, re.S)[-1])
        assert -18 <= float(levels['input_i']) <= -14 and float(levels['input_tp']) < -1
        extracted = extract_visual_review(video, qa, package, ff)
        results.append({'level': level, 'tipUsed': qa['tipUsed'], 'freezeUsed': False,
            'file': item['file'], 'durationSeconds': seconds, 'decodedFrames': frames,
            'bytes': video.stat().st_size, 'sha256': sha(video), 'source': qa['source'],
            'legalShots': native['legalShotReplay'], 'spawnedMobs': native['spawnedMobs'],
            'projectileKills': native['projectileKills'], 'victoryCleanupKills': 0, 'mobFinishes': 0,
            'loudnessLUFS': float(levels['input_i']), 'truePeakDbTP': float(levels['input_tp']),
            'voiceClips': len(voices), 'ctaByteExact': True,
            'independentlyExtractedEncodedFrames': extracted, 'passed': True})
    assert sum(x['tipUsed'] for x in results) == 1
    result = {'passed': True, 'episodes': results, 'count': 5, 'tipEpisodes': 1, 'freezeEpisodes': 0,
        'totalDecodedFrames': sum(x['decodedFrames'] for x in results),
        'totalFinalBytes': sum(x['bytes'] for x in results), 'correctedPCtaSha256': CTA_SHA,
        'scope': 'Independent full MP4 decode/loudness; source/geometry/event/hash/timing cross-checks. Visual and browser checks are separate.',
        'creativeApprovalClaimed': False, 'subjectiveAudioListeningClaimed': False, 'publicUpload': False}
    write(package / 'INDEPENDENT_DELIVERY_CHECKS.json', result)
    return result


if __name__ == '__main__':
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--package', type=Path, default=SERIES)
    args = parser.parse_args()
    print(json.dumps(verify(args.package), ensure_ascii=False, indent=2), flush=True)
