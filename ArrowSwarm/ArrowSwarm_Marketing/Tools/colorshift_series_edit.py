"""Keep the genuine Color Shift actions visible in a compact portrait edit."""
from __future__ import annotations

import csv
from dataclasses import dataclass
from functools import lru_cache
import json

import render_reference_reel as style
import render_promo_outro as promo
from PIL import Image, ImageChops, ImageDraw

REELS = style.MEDIA / '05_Reels'
PACKAGE = REELS / 'ColorShiftSeries_20261118'
RAW = style.MEDIA / '04_Raw_Gameplay'
PILOT = REELS / 'ColorShiftPilot01_20261117' / 'ArrowSwarm_ColorShift_L03_TR.mp4'
EXISTING_L5 = RAW / 'ColorShift_L5_20261117_183440_159'
W, H, FPS = 1080, 1920, 30
FRAMES, INTRO, WIN, OUTRO = 570, 72, 357, 438
HUD = ((0, 0, W, 230), (0, 1690, W, H))
CYAN = (107, 231, 255)

# Each title describes its own actual solution, from the first puzzle to four turns.
BRIEFS = {
    1: ('Son okun rengi', 'İlk bulmacada bir dönüşle son okun çıkışını eşleştir.',
        'SON OK HANGİ RENKTEN?', 'Son okun çıkışı hangi renk?', 'Şimdi çerçeveyi çevir.'),
    4: ('Açık yolları önce kullan', 'Önce açık okları gönder, dönüşten sonra kalan beş oku temizle.',
        'ÖNCE AÇIK YOLLAR', 'Açık yolları önce kullan.', 'Dönüşten sonra beş ok daha.'),
    5: ('Son iki oka bir dönüş', 'İki ok kalınca çerçeveyi çevir ve ikisini de çıkar.',
        'İKİ OK. TEK DÖNÜŞ.', 'İki ok. Tek dönüş.', 'Çevir ve son iki oku gönder.'),
    12: ('27 ok, 3 dönüş', 'Yirmi yedi oku üç dönüşle temizle.',
         '27 OK. 3 DÖNÜŞ.', 'Yirmi yedi ok. Üç dönüş.', 'Yol kapanınca yeni rengi aç.'),
    19: ('38 ok, 4 dönüş', 'Dört dönüşü doğru anlarda kullanarak 38 oku temizle.',
         '38 OK. 4 DÖNÜŞ.', 'Otuz sekiz ok. Dört dönüş.', 'Her dönüşü doğru anda kullan.'),
}


def read(path):
    return json.loads(path.read_text(encoding='utf-8-sig'))


def speech(level):
    brief = BRIEFS[level]
    return [{'key': 'hook', 'text': brief[3]}, {'key': 'turn', 'text': brief[4]},
            {'key': 'result', 'text': 'Hepsi çıktı. Üç yıldız!'}]


@dataclass(frozen=True)
class Segment:
    raw_start: int
    raw_end: int
    start: int
    end: int

    @property
    def speed(self):
        return (self.raw_end - self.raw_start) / (self.end - self.start)


@lru_cache(maxsize=4)
def stable_promo(index):
    return promo.frame(index / FPS)


class Episode:
    def __init__(self, level, source):
        self.level, self.source = level, source
        self.title, self.note, self.hook, _, _ = BRIEFS[level]
        self.out = PACKAGE / f'L{level:02}'
        self.name = f'ArrowSwarm_ColorShift_L{level:02}_Series_TR.mp4'
        self.proof = read(source / 'provenance.json')
        geometry = read(source / 'geometry.json')
        with (source / 'events.csv').open(encoding='utf-8-sig', newline='') as stream:
            events = list(csv.DictReader(stream))
        self.events = [(row['event'], int(row['frame']), int(row['remaining'])) for row in events]
        self.turns = [frame for kind, frame, _ in self.events if kind == 'rotate']
        self.fires = [frame for kind, frame, _ in self.events if kind == 'fire']
        self.win = next(frame for kind, frame, _ in self.events if kind == 'won')
        self.last = self.proof['frames'] - 1
        assert self.proof['mode'] == 'ColorShift' and self.proof['level'] == level and self.proof['language'] == 'tr'
        assert self.proof['won'] and self.proof['remaining'] == 0 and all(row['lives'] == '3' for row in events)
        assert (self.proof['width'], self.proof['height'], self.proof['fps']) == (W, H, FPS)
        actions = [int(row['arrow']) for row in events if row['event'] in ('fire', 'rotate')]
        assert actions == geometry['witness'] and len(self.fires) == self.proof['arrows']
        assert self.turns and self.fires[-1] < self.win <= self.last - 15
        self.segments = self.plan()
        self.turn_times = [self.event_time(frame) for frame in self.turns]
        result = speech(level)
        turn_start = max(2.42, self.turn_times[0] - 1.35)
        self.voice = [dict(result[0], start=.12, maxEnd=2.4),
                      dict(result[1], start=turn_start, maxEnd=11.85),
                      dict(result[2], start=12.07, maxEnd=14.52)]

    def plan(self):
        start = self.fires[0] - 9
        protected = [(start, self.fires[0] + 15), (self.fires[-1] - 6, self.win)]
        # The production animation takes .7s. Preserve that much surrounding action for every turn.
        protected.extend((frame - 4, min(frame + 22, self.win)) for frame in self.turns)
        merged = []
        for left, right in sorted(protected):
            if merged and left <= merged[-1][1]:
                merged[-1][1] = max(merged[-1][1], right)
            else:
                merged.append([left, right])
        normal = sum(b - a for a, b in merged)
        gaps = self.win - start - normal
        target_gaps = WIN - INTRO - normal
        assert gaps > 0 and target_gaps > 0
        spans, raw_head, output_head, spent_raw, spent_out = [], start, INTRO, 0, 0
        for left, right in merged:
            if left > raw_head:
                spent_raw += left - raw_head
                cumulative = round(target_gaps * spent_raw / gaps)
                length = cumulative - spent_out
                if length > 0:
                    spans.append(Segment(raw_head, left, output_head, output_head + length))
                    output_head += length
                spent_out = cumulative
            spans.append(Segment(left, right, output_head, output_head + right - left))
            raw_head, output_head = right, output_head + right - left
        assert output_head == WIN and raw_head == self.win
        return spans

    def event_time(self, frame):
        if frame == self.win:
            return WIN / FPS
        for segment in self.segments:
            if segment.raw_start <= frame < segment.raw_end:
                return (segment.start + (frame - segment.raw_start) / segment.speed) / FPS
        raise ValueError(f'L{self.level}: event outside sequence {frame}')

    @lru_cache(maxsize=14)
    def native(self, index):
        with Image.open(self.source / f'frame_{index:05d}.jpg') as frame:
            return frame.convert('RGB')

    def overlay(self, image, index, speed):
        if index >= WIN:
            return
        now = index / FPS
        turning = next((i for i, at in enumerate(self.turn_times) if at - .15 <= now < at + .76), None)
        draw = ImageDraw.Draw(image)
        if turning is not None:
            draw.rounded_rectangle((80, 236, 384, 303), radius=24, fill=style.INK, outline=CYAN, width=3)
            style.label(image, f'DÖNÜŞ {turning + 1} / {len(self.turns)}', 250, 34, CYAN, 0, 232)
            return
        if index < INTRO:
            sub = 'RENK DEĞİŞİMİ'
            label = self.hook
        elif now >= self.event_time(self.fires[-1]) - .16:
            label, sub = 'SON OK…', ''
        elif speed > 1.16:
            label = 'EŞLEŞEN OKLARI GÖNDER'
            sub = f'{speed:.1f}'.replace('.', ',') + '×  HIZLANDIRILDI'
        else:
            label, sub = 'RENKLERİ EŞLEŞTİR', ''
        bottom = 350 if sub else 305
        draw.rounded_rectangle((83, 234, 997, bottom), radius=24, fill=style.INK, outline=CYAN, width=3)
        style.label(image, label, 247, 53 if index < INTRO else 43, style.GOLD, 0, 540)
        if sub:
            style.label(image, sub, 309, 27, style.WHITE, 0, 540)

    def frame(self, index):
        if index >= OUTRO + 6:
            offset = index - OUTRO
            if 20 <= offset <= 63 or offset >= 114:
                return stable_promo(20 if offset <= 63 else 115)
            return promo.frame(offset / FPS)
        speed = 1
        if index < INTRO:
            raw = self.fires[0] - 20
        elif index >= WIN:
            raw = min(self.last, self.win + index - WIN)
        else:
            segment = next(item for item in self.segments if item.start <= index < item.end)
            speed = segment.speed
            raw = min(segment.raw_end - 1, round(segment.raw_start + (index - segment.start) * speed))
        native = self.native(raw)
        image = native.convert('RGBA')
        self.overlay(image, index, speed)
        result = image.convert('RGB')
        for band in HUD:
            assert ImageChops.difference(result.crop(band), native.crop(band)).getbbox() is None, (self.level, index)
        if index >= OUTRO:
            offset = index - OUTRO
            result = Image.blend(result, promo.frame(offset / FPS), style.ease(offset / 6))
        return result
