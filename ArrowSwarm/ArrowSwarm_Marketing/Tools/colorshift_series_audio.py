"""Prepare and mix the original Turkish narration for the ColorShift series."""

from __future__ import annotations

import asyncio
import json
import math
import re
import subprocess
import wave

from reference_pilot_voice import RATE, SR, VOICE, edge_tts, trim_to_wav
from render_reference_reel import sha
from shapes_shorts_audio import CTA, MUSIC, SFX, match_level, np, pcm

from aiohttp import ClientError


def validate_keys(lines):
    keys = [line['key'] for line in lines]
    if len(set(keys)) != len(keys) or any(not re.fullmatch(r'[A-Za-z0-9_-]+', key) or key == 'cta' for key in keys):
        raise ValueError('Use unique narration filename stems and reserve cta for the shared outro.')


async def prepare_voices(entries, package, ffmpeg):
    """Cache MP3s by script and narrator, with bounded retries and atomic replacements."""
    limit = asyncio.Semaphore(4)

    async def prepare_level(level, lines):
        validate_keys(lines)
        folder = package / f'L{level:02d}' / 'voices'
        folder.mkdir(parents=True, exist_ok=True)
        cache = folder / 'cache.json'
        try:
            existing = json.loads(cache.read_text(encoding='utf-8')) if cache.exists() else {}
        except (OSError, ValueError):
            existing = {}

        async def get_clip(line):
            key, text = line['key'], line['text']
            path = folder / f'{key}.mp3'
            identity = {'text': text, 'narrator': VOICE, 'rate': RATE}
            if path.is_file() and path.stat().st_size:
                cached_identity = {**identity, 'sha256': sha(path)}
                if existing.get(key) == cached_identity:
                    return key, cached_identity
            temporary = path.with_suffix('.partial')
            async with limit:
                for attempt in range(3):
                    try:
                        phrase = edge_tts.Communicate(text, VOICE, rate=RATE)
                        await asyncio.wait_for(phrase.save(str(temporary)), timeout=25)
                        if not temporary.stat().st_size:
                            raise RuntimeError('Empty narration response.')
                        subprocess.run([ffmpeg, '-v', 'error', '-xerror', '-i', str(temporary),
                                        '-map', '0:a:0', '-f', 'null', '-'], check=True, capture_output=True)
                        temporary.replace(path)
                        return key, {**identity, 'sha256': sha(path)}
                    except (OSError, RuntimeError, asyncio.TimeoutError, ClientError,
                            edge_tts.exceptions.EdgeTTSException, subprocess.CalledProcessError) as exc:
                        temporary.unlink(missing_ok=True)
                        if attempt == 2:
                            raise RuntimeError(f'L{level:02d} narration {key} failed.') from exc
                        await asyncio.sleep(1 + attempt)

        new_entries = dict(await asyncio.gather(*(get_clip(line) for line in lines)))
        temporary = cache.with_suffix('.partial')
        temporary.write_text(json.dumps({**existing, **new_entries}, ensure_ascii=False, indent=2) + '\n', encoding='utf-8')
        temporary.replace(cache)

    await asyncio.gather(*(prepare_level(level, lines) for level, lines in entries.items()))


def fit_voice(ffmpeg, source, work, key, sample_limit):
    """Fit a new line without cutting speech or ever changing the approved CTA speed."""
    if key == 'cta':
        data = pcm(ffmpeg, source)
        tempo = 1.0
    else:
        trimmed = work / f'{key}.wav'
        trim_to_wav(source, trimmed, ffmpeg)
        data = pcm(ffmpeg, trimmed)
        tempo = 1.0
        if len(data) > sample_limit:
            tempo = min(1.25, len(data) / max(1, sample_limit - round(.025 * SR)))
            shortened = work / f'{key}_fit.wav'
            subprocess.run([ffmpeg, '-v', 'error', '-y', '-i', str(trimmed), '-af', f'atempo={tempo:.8f}',
                            '-ar', str(SR), '-c:a', 'pcm_s16le', str(shortened)], check=True, capture_output=True)
            data = pcm(ffmpeg, shortened)
    if not len(data) or len(data) > sample_limit:
        raise ValueError(f'{key}: {len(data) / SR:.2f}s of narration does not fit {sample_limit / SR:.2f}s.')
    return data, tempo


def normalize_mix(ffmpeg, mix):
    """Use a fixed gain to match the series and leave space for final AAC encoding."""
    command = [ffmpeg, '-hide_banner', '-f', 'f32le', '-ar', str(SR), '-ac', '2', '-i', 'pipe:0',
               '-af', 'loudnorm=I=-17:TP=-2.2:LRA=8:print_format=json', '-f', 'null', '-']
    analysis = subprocess.run(command, input=mix.astype('<f4').tobytes(), check=True, capture_output=True)
    log = analysis.stderr.decode('utf-8', errors='replace')
    loudness = json.JSONDecoder().raw_decode(log[log.rfind('{'):])[0]
    integrated, peak = float(loudness['input_i']), float(loudness['input_tp'])
    if not (math.isfinite(integrated) and math.isfinite(peak)):
        raise ValueError('The ColorShift mix has no usable audio signal.')
    gain = min(-17.0 - integrated, -2.2 - peak)
    mix *= 10 ** (gain / 20)
    return {'gainDb': gain, 'estimatedWavLUFS': integrated + gain, 'estimatedWavPeakDbTP': peak + gain}


def build_mix(ffmpeg, episode_dir, work, duration, outro, voice, cues):
    """Mix the established music and CTA with short voice lines and frame-accurate sound cues."""
    validate_keys(voice)
    work.mkdir(parents=True, exist_ok=True)
    count = round(duration * SR)
    music = match_level(pcm(ffmpeg, MUSIC, duration, 8)[:count], .045, .28)
    if len(music) != count:
        raise ValueError('The music excerpt is shorter than the clip.')
    effects, speech = np.zeros_like(music), np.zeros_like(music)
    assets, used_cues, narration = {}, [], []

    def add(track, start, data):
        end = min(len(track), start + len(data))
        if not 0 <= start < len(track):
            raise ValueError('Audio cue is outside the edit.')
        track[start:end] += data[:end-start]

    effect_map = {'fire': ('ArrowFire.wav', .032), 'rotate': ('ToggleSwitch.wav', .054),
                  'won': ('LevelWin.ogg', .046), 'promo_click': ('ButtonClick_alt2.wav', .050)}
    for cue in [*cues, {'event': 'promo_click', 'time': outro + 3.15}]:
        event, time = cue['event'], float(cue['time'])
        if event not in effect_map:
            continue
        filename, volume = effect_map[event]
        if event not in assets:
            assets[event] = match_level(pcm(ffmpeg, SFX / filename), volume, .22)
        add(effects, round(time * SR), assets[event])
        used_cues.append({'event': event, 'time': time, 'asset': filename})

    lines = [*voice, {'key': 'cta', 'text': 'Oynamak için Play Store’dan indirin!',
                      'start': outro + .8, 'maxEnd': outro + 3.15}]
    duck = np.ones(count)
    previous_end = 0
    for line in sorted(lines, key=lambda line: line['start']):
        key = line['key']
        source = CTA if key == 'cta' else episode_dir / 'voices' / f'{key}.mp3'
        start = round(line['start'] * SR)
        slot_end = round(line['maxEnd'] * SR)
        if not previous_end <= start < slot_end <= count or (key != 'cta' and slot_end > round(outro * SR)):
            raise ValueError(f'Invalid, overlapping or out-of-bounds narration slot: {key}.')
        data, tempo = fit_voice(ffmpeg, source, work, key, slot_end - start)
        end = start + len(data)
        add(speech, start, match_level(data, .13, .57))
        before, after = max(0, start - round(.08 * SR)), min(count, end + round(.12 * SR))
        duck[before:start] = np.minimum(duck[before:start], np.linspace(1, .24, start-before))
        duck[start:end] = .24
        duck[end:after] = np.minimum(duck[end:after], np.linspace(.24, 1, after-end))
        narration.append({'key': key, 'text': line['text'], 'start': start / SR, 'end': end / SR,
                          'maxEnd': line['maxEnd'], 'tempo': tempo, 'sourceSha256': sha(source)})
        previous_end = end

    mix = music * duck[:, None] + effects + speech
    index = np.arange(count)
    fade = np.minimum(index / (SR * .04), 1) * np.minimum((count - 1 - index) / (SR * .18), 1)
    mix *= fade[:, None]
    levels = normalize_mix(ffmpeg, mix)
    destination = work / 'mix.wav'
    with wave.open(str(destination), 'wb') as output:
        output.setparams((2, 2, SR, 0, 'NONE', 'not compressed'))
        output.writeframes(np.rint(mix * 32767).astype('<i2').tobytes())
    metadata = {'durationSeconds': duration, 'soundtrack': MUSIC.name, 'narrator': VOICE,
                'lines': narration, 'effects': used_cues, 'approvedCtaSha256': sha(CTA), **levels}
    return destination, metadata
