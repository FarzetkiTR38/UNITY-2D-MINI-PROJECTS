"""Event-aligned editorial audio for the no-Freeze Defense series."""
from __future__ import annotations
import math
import wave
import render_reference_reel as m
from render_creative_short import pcm
import numpy as np

SR = 48000


def make_audio(ep, ff):
    n = round(ep.seconds * SR)
    music_asset = ['GameBGM_alt1.ogg', 'GameBGM_alt2.ogg', 'GameBGM.mp3'][ep.order % 3]
    music = pcm(ff, m.ROOT / 'Assets/_Project/Audio/Music' / music_asset, ep.seconds, 8)[:n]
    if len(music) < n:
        music = np.pad(music, ((0, n-len(music)), (0, 0)))
    music *= .048 / max(.001, float(np.sqrt(np.mean(music ** 2))))
    mix = music.copy()
    cues = []

    def add(t, data):
        a = round(t * SR)
        b = min(n, a + len(data))
        if 0 <= a < b:
            mix[a:b] += data[:b-a]

    def effect(t, name, rms):
        path = m.ROOT / 'Assets/_Project/Audio/SFX' / name
        data = pcm(ff, path)
        data *= rms / max(.001, float(np.sqrt(np.mean(data ** 2))))
        data *= min(1, .25 / max(.001, float(abs(data).max())))
        add(t, data)
        cues.append({'seconds': t, 'asset': str(path.relative_to(m.ROOT)).replace('\\', '/'),
                     'sha256': m.sha(path)})

    if ep.tip_used:
        effect(ep.tip_at, 'SkillTips.ogg', .057)
    for event in ep.events:
        if event['event'] in ('fire', 'rainbow_fire'):
            rainbow = event['event'] == 'rainbow_fire'
            effect(ep.event_time(int(event['frame'])),
                   'RainbowArrow.ogg' if rainbow else 'ArrowFire.wav', .05 if rainbow else .029)
    last = -10
    for event in ep.events:
        if event['event'] == 'projectile_damage':
            t = ep.event_time(int(event['frame']))
            if t - last >= .18:
                effect(t, 'ArrowHitEnemy.ogg', .022)
                last = t
    effect(ep.win_at + .1, 'LevelWin.ogg', .043)
    effect(ep.win_at + 2.4, 'StarEarn.wav', .035)
    effect(ep.outro + 3.15, 'ButtonClick_alt2.wav', .05)
    timeline = []
    for key, start in ep.voice_starts:
        row = ep.voices[key]
        path = ep.library / row['file']
        assert m.sha(path) == row['sha256']
        data = pcm(ff, path)
        data *= .13 / max(.001, float(np.sqrt(np.mean(data ** 2))))
        data *= min(1, .57 / max(.001, float(abs(data).max())))
        a = round(start * SR)
        b = a + len(data)
        assert b <= n
        da, db = max(0, a-round(.08*SR)), min(n, b+round(.12*SR))
        env = np.ones(db-da)*.22
        if a > da:
            env[:a-da] = np.linspace(1, .22, a-da)
        if db > b:
            env[-(db-b):] = np.linspace(.22, 1, db-b)
        mix[da:db] *= env[:, None]
        add(start, data)
        timeline.append({'key': key, 'text': row['text'], 'start': start, 'end': b/SR,
                         'file': str(path.relative_to(m.MEDIA)).replace('\\', '/'), 'sha256': m.sha(path)})
    fade = np.minimum(np.arange(n)/(SR*.04), 1)*np.minimum((n-1-np.arange(n))/(SR*.18), 1)
    mix *= fade[:, None]
    mix *= min(1, .88/max(.001, float(abs(mix).max())))
    with wave.open(str(ep.out/'editorial_mix.wav'), 'wb') as wav:
        wav.setnchannels(2)
        wav.setsampwidth(2)
        wav.setframerate(SR)
        wav.writeframes(np.rint(mix*32767).astype('<i2').tobytes())
    ep.write('audio_timeline.json', {'voices': timeline, 'cues': cues, 'music': music_asset,
                                    'nativeAudioListenerRecorded': False, 'editorialReconstruction': True,
                                    'correctedPCtaCopiedByteExact': True, 'freezeSfxPresent': False})

    def tc(t):
        ms = round(t*1000)
        return f'{ms//3600000:02d}:{ms//60000%60:02d}:{ms//1000%60:02d},{ms%1000:03d}'

    (ep.out/'captions_TR.srt').write_text('\n\n'.join(
        f'{i+1}\n{tc(r["start"])} --> {tc(r["end"])}\n{r["text"]}'
        for i, r in enumerate(timeline))+'\n', encoding='utf-8')
