"""Turkish narration and audio from the established 16-second Shapes edit."""
from __future__ import annotations

import asyncio
import subprocess
import wave

from reference_pilot_voice import RATE, SR, VOICE, trim_to_wav
import edge_tts
import numpy as np
import render_reference_reel as style

ROOT, MEDIA = style.ROOT, style.MEDIA
REFERENCE = MEDIA / '05_Reels/CreativeShort01_20261114/audio'
CTA = MEDIA / '05_Reels/SharedOutro01/cta_voice.wav'
NUMBERS = {40: 'Kırk', 42: 'Kırk iki', 44: 'Kırk dört', 45: 'Kırk beş'}
MUSIC = ROOT / 'Assets/_Project/Audio/Music/GameBGM_alt2.ogg'
SFX = ROOT / 'Assets/_Project/Audio/SFX'


def narration(episode):
    """Place brief level-specific lines around the unchanged shared voice cues."""
    return [(.10, f'{NUMBERS[episode.arrows]} ok. Hangisi önce çıkar?', 'hook'),
            (3.50, 'Önce A!', 'answer'),
            (9.30, f'Ve {episode.shape.lower()} çözüldü!', 'result'),
            (12.40, 'Oynamak için Play Store’dan indirin!', 'cta')]


async def prepare_voices(episodes):
    """Speak only new editorial lines and cache them beside the new cuts."""
    limit = asyncio.Semaphore(4)

    async def generate(episode, key, text):
        path = episode.output / f'{key}.mp3'
        if path.is_file() and path.stat().st_size:
            return
        temporary = path.with_suffix('.partial')
        async with limit:
            for attempt in range(3):
                try:
                    await asyncio.wait_for(edge_tts.Communicate(text, VOICE, rate=RATE).save(str(temporary)), 25)
                    if not temporary.stat().st_size:
                        raise RuntimeError('Empty voice clip')
                    temporary.replace(path)
                    return
                except (RuntimeError, OSError, asyncio.TimeoutError, edge_tts.exceptions.NoAudioReceived):
                    if attempt == 2:
                        raise
                    await asyncio.sleep(1 + attempt)

    await asyncio.gather(*(generate(episode, key, text)
                           for episode in episodes for _, text, key in narration(episode)
                           if key in ('hook', 'result')))


def pcm(ffmpeg, path, seconds=None, start=0):
    """Decode a project audio asset to stereo float PCM."""
    command = [ffmpeg, '-v', 'error', '-i', str(path)]
    if start:
        command += ['-ss', str(start)]
    if seconds:
        command += ['-t', str(seconds)]
    result = subprocess.run(command + ['-f', 'f32le', '-ar', str(SR), '-ac', '2', 'pipe:1'],
                            capture_output=True, check=True)
    return np.frombuffer(result.stdout, dtype='<f4').reshape(-1, 2).astype(np.float64)


def match_level(data, rms, ceiling):
    data = data * (rms / max(.001, float(np.sqrt(np.mean(data**2)))))
    return data * min(1, ceiling / max(.001, float(np.abs(data).max())))


def build_mix(episode, ffmpeg, work):
    """Keep reference music and effects, with shooting cues tied to real frames."""
    count = 16 * SR
    music = match_level(pcm(ffmpeg, MUSIC, 16, 8)[:count], .045, .28)
    assert len(music) == count
    effects = np.zeros_like(music)
    speech = np.zeros_like(music)
    cache, cues, voices = {}, [], []

    def add(track, time, data):
        start = round(time * SR)
        size = min(len(data), len(track) - start)
        if size > 0:
            track[start:start + size] += data[:size]

    def cue(time, name, rms):
        if name not in cache:
            cache[name] = pcm(ffmpeg, SFX / name)
        add(effects, time, match_level(cache[name], rms, .22))
        cues.append({'time': time, 'asset': name})

    for time in (.05, 1.50, 2.50):
        cue(time, 'ButtonClick_alt2.wav', .035 if time < 1 else .025)
    cue(3.50, 'StarEarn.wav', .06)
    cue(3.80, 'ArrowFire.wav', .045)
    for fire in episode.fires[1:-1:4]:
        cue(episode.event_time(fire), 'ArrowFire.wav', .028)
    cue(episode.event_time(episode.fires[-1]), 'ArrowFire.wav', .045)
    cue(episode.event_time(episode.won) + .12, 'LevelWin.ogg', .045)
    cue(10.25, 'StarEarn.wav', .035)
    cue(14.75, 'ButtonClick_alt2.wav', .05)
    rng = np.random.default_rng(2116)
    noise = rng.normal(0, 1, (round(.32 * SR), 2))
    lift = noise - np.vstack((np.zeros((1, 2)), noise[:-1])) * .82
    add(effects, 4.84, lift * np.sin(np.linspace(0, np.pi, len(lift)))[:, None]**2 * .012)

    limits = {'hook': 3.48, 'answer': 4.5, 'result': 11.48, 'cta': 14.75}
    for start, text, key in narration(episode):
        if key in ('hook', 'result'):
            path = work / f'{key}.wav'
            trim_to_wav(episode.output / f'{key}.mp3', path, ffmpeg)
        else:
            path = CTA if key == 'cta' else REFERENCE / 'voice_01.wav'
        data = match_level(pcm(ffmpeg, path), .13, .57)
        end = start + len(data) / SR
        if end > limits[key]:
            raise ValueError(f'L{episode.level}: {key} voice extends to {end:.2f}s')
        a, b = round(start * SR), round(end * SR)
        fade_in = round(.08 * SR)
        fade_out = round(.12 * SR)
        music[a-fade_in:a] *= np.linspace(1, .22, fade_in)[:, None]
        music[a:b] *= .22
        music[b:b+fade_out] *= np.linspace(.22, 1, fade_out)[:, None]
        add(speech, start, data)
        voices.append({'text': text, 'start': start, 'end': end, 'sourceSha256': style.sha(path)})

    mix = music + effects + speech
    indexes = np.arange(count)
    edge_fade = np.minimum(indexes / (SR * .04), 1) * np.minimum((count - 1 - indexes) / (SR * .18), 1)
    mix *= edge_fade[:, None]
    # Match the reference's roughly -16.4 LUFS with headroom before AAC encoding.
    mix *= min(1.28, .86 / max(.001, float(np.abs(mix).max())))
    mix_path = work / 'mix.wav'
    with wave.open(str(mix_path), 'wb') as output:
        output.setparams((2, 2, SR, 0, 'NONE', 'not compressed'))
        output.writeframes(np.rint(mix * 32767).astype('<i2').tobytes())
    metadata = {'soundtrack': MUSIC.name, 'narrator': VOICE, 'lines': voices, 'effects': cues,
                'approvedCtaSha256': style.sha(CTA)}
    return mix_path, metadata
