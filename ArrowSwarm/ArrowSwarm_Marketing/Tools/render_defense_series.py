"""Five event-driven Defense edits from genuine, independently checked sources.

No Freeze is allowed. Exactly the configured Tip episode demonstrates the actual
charged hint. Native HUD bands remain pixel-identical before video encoding.
"""
from __future__ import annotations
import argparse
import csv
import functools
import json
import math
import shutil
from pathlib import Path
import render_reference_reel as m
import render_promo_outro as promo
from defense_series_audio import make_audio
from defense_series_verify import verify_episode
from defense_series_encode import encode_episode
from PIL import Image, ImageDraw, ImageChops

SERIES = m.MEDIA/'05_Reels/DefenseSeries01_20261115'
ORDER = [3, 6, 8, 12, 15]
W, H, FPS = 1080, 1920, 30
HUD = ((0, 0, W, 230), (0, 1690, W, H))
CYAN = (93, 228, 255)


def read(path):
    return json.loads(path.read_text(encoding='utf-8-sig'))


class Episode:
    def __init__(self, source, level, seconds=21):
        self.source = source.resolve()
        self.level, self.order = level, ORDER.index(level)
        self.out = SERIES/f'L{level:02d}'
        self.out.mkdir(parents=True, exist_ok=True)
        self.p = read(self.source/'provenance.json')
        proof_path = self.out/'native_capture_checks.json'
        if not proof_path.is_file():
            proof_path = self.source/'native_capture_checks.json'
        self.checks = read(proof_path)
        self.geometry = read(self.source/'geometry.json')
        with (self.source/'events.csv').open(encoding='utf-8-sig', newline='') as stream:
            self.events = list(csv.DictReader(stream))
        assert self.checks['passed'] and self.p['allFramesSaved'] and self.p['level'] == level
        assert self.checks['source'] == self.source.name
        assert self.checks['sourceProvenanceSha256'] == m.sha(self.source/'provenance.json')
        assert self.checks['sourceGeometrySha256'] == m.sha(self.source/'geometry.json')
        assert self.checks['sourceEventsSha256'] == m.sha(self.source/'events.csv')
        assert self.p['won'] and self.p['lives'] == 3 and self.p['remaining'] == 0
        assert self.p['freezeFrame'] == -1 and self.p['cleanupKills'] == 0
        assert self.p['spawnedMobs'] == self.p['projectileKills'] > 0
        assert not any(e['event'] in ('freeze', 'freeze_end', 'cleanup_kill', 'mob_finish') for e in self.events)
        self.library = SERIES/'AudioLibrary'
        manifest = read(self.library/'voice_manifest.json')
        self.voices = manifest['clips']
        self.spec = manifest['episodes'][str(level)]
        self.tip_used = self.spec['tipUsed']
        assert bool(self.p.get('tipUsed', False)) == self.tip_used
        shots = [e for e in self.events if e['event'] in ('fire', 'rainbow_fire')]
        assert len(shots) == self.p['arrows'] and shots[-1]['event'] == 'rainbow_fire'
        self.start = self.checks['openingFrame']
        self.first, self.penultimate, self.rain, self.win = map(int, (
            shots[0]['frame'], shots[-2]['frame'], shots[-1]['frame'], self.p['winFrame']))
        assert self.start < self.first < self.penultimate < self.rain < self.win
        assert self.p['frames'] > self.win+96, 'Keep the genuine settled outcome animation'
        hook = self.voices[self.spec['hook']]['durationSeconds']
        calm = self.voices[self.spec['calm']]['durationSeconds']
        self.tip_at = max(1.8, .1+hook+.22) if self.tip_used else None
        self.calm_start = self.tip_at+.03 if self.tip_used else .1+hook+.18
        intro = (self.first-self.start)/42 if not self.tip_used else 5.0
        self.first_at = max(2.8, intro, self.calm_start+calm+.15)
        ordinary = (self.penultimate-self.first)/30
        flight_seconds = min(4.5, max(1.25, (self.win-self.rain)/22.5))
        hold = max(1.05, min(1.8, (self.rain-self.penultimate)/30))
        budget = (24 if self.tip_used else max(22, seconds))-4.7-3.2-flight_seconds-hold-self.first_at
        rate = max(1, ordinary/max(1, budget))
        assert rate <= 1.5, 'Source needs a longer editorial plan; do not hide actions'
        self.promoted_at = self.first_at+ordinary/rate
        self.rain_at = self.promoted_at+hold
        self.win_at = self.rain_at+flight_seconds
        self.outro = self.win_at+3.2
        self.seconds = max(18, math.ceil(self.outro+4.7))
        self.anchors = [(self.start, 0)]
        if self.tip_used:
            self.anchors.append((int(self.p['tipFrame']), self.tip_at))
        self.anchors += [(self.first, self.first_at), (self.penultimate, self.promoted_at),
                         (self.rain, self.rain_at), (self.win, self.win_at), (self.win+96, self.outro)]
        assert all(a[0] < b[0] and a[1] < b[1] for a, b in zip(self.anchors, self.anchors[1:]))
        self.voice_starts = [(self.spec['hook'], .1), (self.spec['calm'], self.calm_start),
                             ('send', self.first_at+.08), ('rainbow', self.rain_at-.97),
                             ('clear', self.win_at+.4), ('cta', self.outro+.8)]
        assert all(start+self.voices[key]['durationSeconds']+.04 < next_start
                   for (key, start), (_, next_start) in zip(self.voice_starts, self.voice_starts[1:]))
        assert self.voice_starts[-1][1]+2.3 < self.outro+3.15 < self.seconds
        self.final = self.out/f'ArrowSwarm_Defense_L{level:02d}_TR.mp4'
        self.write('edit_plan.json', self.plan())

    def write(self, name, value):
        (self.out/name).write_text(json.dumps(value, ensure_ascii=False, indent=2)+'\n', encoding='utf-8')

    def plan(self):
        return {'level': self.level, 'source': self.source.name, 'durationSeconds': self.seconds,
                'freezeUsed': False, 'tipUsed': self.tip_used, 'tipAt': self.tip_at,
                'firstShotAt': self.first_at, 'rainbowAt': self.rain_at,
                'naturalWinAt': self.win_at, 'promoAt': self.outro,
                'anchors': [{'sourceFrame': f, 'seconds': t} for f, t in self.anchors],
                'voiceStarts': self.voice_starts, 'nativeHudBands': HUD,
                'storeStatus': 'mock', 'publicStoreUrl': None}

    def event_time(self, index):
        for (a, ta), (b, tb) in zip(self.anchors, self.anchors[1:]):
            if index <= b:
                return ta+(index-a)*(tb-ta)/(b-a)
        return self.outro+(index-self.win-96)/30

    def raw_index(self, t):
        for (a, ta), (b, tb) in zip(self.anchors, self.anchors[1:]):
            if t <= tb:
                return round(a+(t-ta)*(b-a)/(tb-ta))
        return min(self.p['frames']-1, self.win+96)

    def speed(self, t):
        for (a, ta), (b, tb) in zip(self.anchors, self.anchors[1:]):
            if t <= tb:
                return (b-a)/(30*(tb-ta))
        return 1

    @functools.lru_cache(maxsize=15)
    def native(self, i):
        return Image.open(self.source/f'frame_{i:05d}.jpg').convert('RGB')

    def frame(self, t):
        if t >= self.outro+.2:
            return promo.frame(t-self.outro)
        source = self.native(self.raw_index(t))
        im = source.convert('RGBA')
        if t < self.first_at:
            key = self.spec['hook']
            if t >= self.calm_start:
                key = self.spec['calm']
            text = self.voices[key]['text'].replace('i', 'İ').replace('ı', 'I').upper()
            m.label(im, text, 260, 44, CYAN if self.tip_used and t >= self.calm_start else m.WHITE, 4, 540)
        if self.first_at <= t < self.rain_at-.97:
            m.label(im, 'OKLARI SIRAYLA GÖNDER.', 1580, 43, m.GOLD, 3, 540)
        elif self.rain_at-.97 <= t < self.rain_at:
            m.label(im, 'SON OK…', 1575, 52, m.GOLD, 4, 540)
            point = self.geometry['arrows'][self.p['reservedLastArrow']]['screenHead']
            x, y = point['x'], point['y']
            r = min(59+3*math.sin(t*8), x-6, 1074-x, y-236, 1684-y)
            draw = ImageDraw.Draw(im)
            if r >= 18:
                for angle in [0, 90, 180, 270]:
                    draw.arc((x-r, y-r, x+r, y+r), angle+8, angle+73, fill=CYAN, width=5)
        elif self.rain_at <= t < self.win_at:
            m.label(im, 'GÖKKUŞAĞI!', 1565, 53, m.GOLD, 4, 540)
        elif self.win_at+.4 <= t < self.win_at+2:
            m.label(im, 'HEPSİ TEMİZLENDİ!', 238, 29, CYAN, 2, 540)
        rate = self.speed(t)
        if t < self.win_at and abs(rate-1) > .035:
            ImageDraw.Draw(im).rounded_rectangle((437, 1632, 643, 1672), radius=20, fill=m.INK)
            m.label(im, (f'{rate:.2f}×').replace('.', ','), 1638, 27, CYAN, 0, 540)
        rgb = im.convert('RGB')
        if t < self.outro:
            for band in HUD:
                assert ImageChops.difference(rgb.crop(band), source.crop(band)).getbbox() is None
        else:
            rgb = Image.blend(rgb, promo.frame(t-self.outro), m.ease((t-self.outro)/.2))
        return rgb

def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--source', required=True, type=Path)
    parser.add_argument('--level', required=True, type=int, choices=ORDER)
    parser.add_argument('--seconds', type=int, default=21)
    parser.add_argument('--stills', action='store_true')
    parser.add_argument('--finish-only', action='store_true')
    args = parser.parse_args()
    ep = Episode(args.source, args.level, args.seconds)
    for t in [.4, ep.calm_start+.25, ep.first_at+.4, ep.rain_at-.2, ep.rain_at+.7,
              ep.win_at+.9, ep.win_at+3.05, ep.outro+1.3, ep.outro+3.3]:
        ep.frame(t).save(ep.out/f'still_{t:05.2f}.jpg', quality=94)
    if args.stills:
        print(json.dumps(ep.plan(), ensure_ascii=False), flush=True)
        return
    ff = m.imageio_ffmpeg.get_ffmpeg_exe()
    if args.finish_only: assert read(ep.out/'QA_REPORT.json')['sourceProvenanceSha256'] == m.sha(ep.source/'provenance.json')
    if not args.finish_only:
        make_audio(ep, ff)
        encode_episode(ep, ff)
    qa = verify_episode(ep, ff)
    shutil.copy2(ep.source/'provenance.json', ep.out/'source_provenance.json')
    shutil.copy2(m.ROOT/'Assets/_Project/Fonts/Farzetki_Title_Font_H1-OFL.txt', ep.out/'FONT_LICENSE.txt')
    print(json.dumps(qa, ensure_ascii=False, indent=2), flush=True)


if __name__ == '__main__':
    main()
