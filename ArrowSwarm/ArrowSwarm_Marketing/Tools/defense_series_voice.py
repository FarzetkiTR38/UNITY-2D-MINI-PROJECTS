"""Reusable public narration for five no-Freeze Defense videos.

Existing R2 generic lines and the corrected-P CTA are copied byte for byte.
Only the short original public lines below are sent to the existing speech service.
"""
from __future__ import annotations
import asyncio
import json
import shutil
import wave
from pathlib import Path

from reference_pilot_voice import SR, VOICE, RATE, trim_to_wav
import edge_tts
import imageio_ffmpeg
import numpy as np

MEDIA = Path(__file__).resolve().parents[1]
SERIES = MEDIA / '05_Reels/DefenseSeries01_20261115'
OUT = SERIES / 'AudioLibrary'
OLD = MEDIA / '05_Reels/DefensePilot02_20261115/audio'
CTA_HASH = '3cffc7f86966e7eb7c4c6e7b0524ea7a2ed59c9c013ad2305cfe0adc857e4c23'
REUSE = {'approach': 0, 'calm': 1, 'send': 3, 'rainbow': 4, 'clear': 5, 'cta': 6}
LINES = {
    'rush_hook': 'Düşmanlar yaklaşıyor.',
    'steady_hook': 'Bu savunma hattı dayanır mı?',
    'corners_hook': 'Köşeyi dönmeden durdur.',
    'last_hook': 'Yeni bölüm, yeni bir savunma.',
    'open_path': 'Önce yolu açık oklar.',
    'steady': 'Sırayı bozma.',
    'tip_hook': 'İlk oku bulamadın mı?',
    'tip_explain': 'İpucu, yolu açık bir oku gösterir.',
}
EPISODES = {
    3: {'hook': 'rush_hook', 'calm': 'calm', 'tipUsed': False},
    6: {'hook': 'steady_hook', 'calm': 'open_path', 'tipUsed': False},
    8: {'hook': 'tip_hook', 'calm': 'tip_explain', 'tipUsed': True},
    12: {'hook': 'corners_hook', 'calm': 'open_path', 'tipUsed': False},
    15: {'hook': 'last_hook', 'calm': 'steady', 'tipUsed': False},
}


def sha(path):
    import hashlib
    return hashlib.sha256(path.read_bytes()).hexdigest()


def write(path, value):
    path.write_text(json.dumps(value, ensure_ascii=False, indent=2) + '\n', encoding='utf-8')


async def main():
    OUT.mkdir(parents=True, exist_ok=True)
    old_manifest = json.loads((OLD / 'voice_manifest.json').read_text(encoding='utf-8'))['clips']
    clips = {}
    for key, index in REUSE.items():
        row = old_manifest[index]
        source, target = OLD / row['file'], OUT / f'{key}.wav'
        assert sha(source) == row['sha256'], f'Protected R2 narration changed: {key}'
        shutil.copy2(source, target)
        clips[key] = {'text': row['text'], 'file': target.name, 'durationSeconds': row['durationSeconds'],
                      'sha256': sha(target), 'generatedThisSeries': False,
                      'reusedFrom': str(source.relative_to(MEDIA)).replace('\\', '/')}
    assert clips['cta']['sha256'] == CTA_HASH
    semaphore = asyncio.Semaphore(3)
    ffmpeg = imageio_ffmpeg.get_ffmpeg_exe()

    async def generate(key, text):
        raw, target = OUT / f'{key}_raw.mp3', OUT / f'{key}.wav'
        async with semaphore:
            if not raw.is_file() or not raw.stat().st_size:
                await asyncio.wait_for(edge_tts.Communicate(text, VOICE, rate=RATE).save(str(raw)), 45)
            metrics = trim_to_wav(raw, target, ffmpeg)
        return key, {'text': text, 'file': target.name, 'rawFile': raw.name, 'rawSha256': sha(raw),
                     'generatedThisSeries': True, **metrics}

    clips.update(await asyncio.gather(*(generate(k, v) for k, v in LINES.items())))
    checks = []
    for key, row in clips.items():
        path = OUT / row['file']
        with wave.open(str(path), 'rb') as wav:
            assert (wav.getframerate(), wav.getnchannels(), wav.getsampwidth()) == (SR, 1, 2)
            pcm = np.frombuffer(wav.readframes(wav.getnframes()), dtype='<i2').astype(np.int32)
        assert sha(path) == row['sha256'] and len(pcm) > 0
        assert abs(len(pcm) / SR - row['durationSeconds']) < 1 / SR
        clipped = int(np.count_nonzero(np.abs(pcm) >= 32767))
        assert clipped == 0
        assert 'dondur' not in row['text'].lower() and 'freeze' not in row['text'].lower()
        checks.append({'key': key, 'durationSeconds': len(pcm) / SR, 'clippedSamples': clipped,
                       'sha256': sha(path)})
    manifest = {'provider': 'Microsoft online speech via edge-tts', 'voice': VOICE, 'rate': RATE,
                'audioIsEditorial': True, 'sampleRate': SR, 'channels': 1, 'sampleFormat': 'PCM16',
                'onlyPublicCreativeSentencesSent': True, 'approvedCtaSha256': CTA_HASH,
                'freezeNarrationPresent': False, 'clips': clips,
                'episodes': {str(k): v for k, v in EPISODES.items()}}
    write(OUT / 'voice_manifest.json', manifest)
    write(OUT / 'voice_QA.json', {'passed': True, 'checks': checks, 'clipCount': len(clips),
                                 'approvedCtaPOnsetByteIdentical': True, 'subjectiveListeningClaimed': False})
    print(json.dumps({'passed': True, 'clips': {k: r['durationSeconds'] for k, r in clips.items()}},
                     ensure_ascii=False), flush=True)


if __name__ == '__main__':
    asyncio.run(main())
