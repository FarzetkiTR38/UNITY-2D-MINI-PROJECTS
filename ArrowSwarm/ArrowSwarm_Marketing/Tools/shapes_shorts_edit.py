"""Frame mapping and annotations for 16-second Shapes episodes."""
from __future__ import annotations

import csv
from functools import lru_cache
import json

import render_reference_reel as style
import render_promo_outro as promo
from PIL import Image, ImageChops, ImageDraw

MEDIA = style.MEDIA
REELS = MEDIA / '05_Reels'
PACKAGE = REELS / 'ShapesShorts_20261118'
SERIES = REELS / 'ShapesSeries_20261114'
CONFIG = {2: ('Penguen', ((405, 1532), (600, 1532))),
          5: ('Roket', ((540, 1651), (350, 1651))),
          12: ('Kaplumbağa', ((123, 1528), (350, 1528))),
          16: ('Ördek', ((1006, 484), (780, 484))),
          29: ('Flamingo', ((1042, 620), (1042, 716))),
          39: ('Ayçiçeği', ((802, 1651), (967, 1651)))}
W, H, FPS = 1080, 1920, 30
FIRE, RUSH, FINAL, OUTRO = 114, 150, 228, 348
FRAMES = 480
HUD = ((0, 0, W, 230), (0, 1690, W, H))
GREEN = (83, 238, 180)

def read(path):
    return json.loads(path.read_text(encoding='utf-8-sig'))


def upper(text):
    return text.replace('i', 'İ').replace('ı', 'I').upper()


@lru_cache(maxsize=4)
def still_promo(index):
    return promo.frame(index / FPS)


class Episode:
    def __init__(self, level):
        self.level = level
        self.shape, self.badges = CONFIG[level]
        self.output = PACKAGE / f'L{self.level:02}'
        self.name = f'ArrowSwarm_Shapes{level}_Short_TR.mp4'
        self.output.mkdir(parents=True, exist_ok=True)
        self.compact = level in (5, 39)
        if level == 2:
            folder = 'Shapes_L2_20261114_105432_122'
            self.original = REELS / 'ReferencePilot02_R3_20261114/ArrowSwarm_Shapes2_R3_Promo_TR.mp4'
            self.choices = [0, 3]
            self.paths = [[style.grid_to_source(point) for point in style.PATHS[i]] for i in self.choices]
        else:
            series = SERIES / f'L{level:02}'
            plan = read(series / 'edit_plan.json')
            folder = plan['source']
            self.original = series / f'ArrowSwarm_Shapes{level}_TR.mp4'
            self.choices = [choice['arrow_id'] for choice in plan['choices'][:2]]

        self.source = MEDIA / '04_Raw_Gameplay' / folder
        self.proof = read(self.source / 'provenance.json')
        self.arrows = self.proof['arrows']
        with (self.source / 'events.csv').open(encoding='utf-8-sig', newline='') as stream:
            events = list(csv.DictReader(stream))
        actions = [event for event in events if event['event'] == 'fire']
        self.fires = [int(event['frame']) for event in actions]
        self.settled = next(int(event['frame']) for event in events if event['event'] == 'first_settled')
        # This earlier penguin capture predates the win event. Frame 766 shows the win dimmer begin.
        self.won = next((int(event['frame']) for event in events if event['event'] == 'won'), 766 if level == 2 else -1)
        assert self.proof['won'] and self.proof['remaining'] == 0 and self.won > self.fires[-1]
        assert (self.proof['width'], self.proof['height'], self.proof['fps']) == (W, H, FPS)
        assert all(event['lives'] == '3' for event in events)
        assert int(actions[0]['arrow']) == self.choices[0] and len(self.fires) == self.arrows
        assert len({event['arrow'] for event in actions}) == self.arrows
        if level != 2:
            geometry = read(self.source / 'geometry.json')
            assert [int(event['arrow']) for event in actions] == geometry['witness']
            assert [geometry['arrows'][i]['initiallyRemovable'] for i in self.choices] == [True, False]
            zero, right, up = (geometry[key] for key in ('gridZero', 'gridX', 'gridY'))

            def point(grid):
                return tuple(zero[axis] + (right[axis] - zero[axis]) * grid['x']
                             + (up[axis] - zero[axis]) * grid['y'] for axis in ('x', 'y'))

            self.paths = [[point(grid) for grid in geometry['arrows'][i]['points']] for i in self.choices]
        self.rush_stop = self.fires[-1] - 10
        self.speed = (self.rush_stop - self.fires[1]) / (FINAL - RUSH)
        self.rate_label = f'{self.speed:.1f}'.replace('.', ',') + '×'
        assert self.source_frame(FIRE) == self.fires[0] and self.source_frame(RUSH) == self.fires[1]
        assert self.source_frame(RUSH - 1) >= self.settled and self.event_time(self.won) < 9.3

    def source_frame(self, output):
        first, second = self.fires[:2]
        if output < FIRE - 10:
            return first - 15
        if output < RUSH:
            return first + output - FIRE
        if output < FINAL:
            return round(second + (output - RUSH) * self.speed)
        return min(self.proof['frames'] - 1, self.rush_stop + output - FINAL)

    def event_time(self, index):
        if index < self.fires[1]:
            return (FIRE + index - self.fires[0]) / FPS
        if index < self.rush_stop:
            return (RUSH + (index - self.fires[1]) / self.speed) / FPS
        return (FINAL + index - self.rush_stop) / FPS

    @lru_cache(maxsize=12)
    def native(self, index):
        with Image.open(self.source / f'frame_{index:05d}.jpg') as frame:
            return frame.convert('RGB')

    def draw_choices(self, image, t):
        if not .60 <= t < 3.76:
            return
        overlay = Image.new('RGBA', image.size)
        draw = ImageDraw.Draw(overlay)
        for order, (points, (x, y)) in enumerate(zip(self.paths, self.badges)):
            alpha = style.ease((t - .60 - .10 * order) / .18) * (1 - style.ease((t - 3.58) / .15))
            if alpha <= 0:
                continue
            color = GREEN if order == 0 and t >= 3.5 else style.GOLD
            radius = round((28 if self.compact or self.level == 29 else 34) * alpha)
            draw.line(points, fill=(*color, round(100 * alpha)), width=20, joint='curve')
            draw.line((points[0], (x, y)), fill=(*style.INK, round(215 * alpha)), width=3)
            draw.ellipse((x-radius, y-radius, x+radius, y+radius), fill=(*color, round(255 * alpha)),
                         outline=(*style.INK, round(255 * alpha)), width=4)
            draw.text((x, y), 'AB'[order], font=style.font(35), anchor='mm', fill=(*style.INK, round(255 * alpha)))
        image.alpha_composite(overlay)

    def draw_caption(self, image, t):
        def label(text, y, size, color=style.GOLD, stroke=4):
            style.label(image, text, y, size, color, stroke, cx=540)

        if t >= 9.3:
            text = f'{upper(self.shape)} ÇÖZÜLDÜ!' if t < 10.71 else f'{self.arrows} OK. 3 YILDIZ.'
            label(text, 1644, 32, stroke=2)
        elif t >= self.event_time(self.won):
            return
        elif self.compact:
            lines = [(1.35, f'{self.arrows} OK.  İLK HAMLEN?'), (3.5, 'HANGİSİ ÖNCE ÇIKAR?  A MI, B Mİ?'),
                     (4.65, 'ÖNCE A!  ÖNÜ AÇIK.'), (5, 'İLK OK ÇIKTI.'),
                     (7.6, f'HIZLI SARALIM.  {self.rate_label}'), (16, 'SON OK…')]
            caption = next(text for end, text in lines if t < end)
            if 1.5 <= t < 3.5:
                caption += f'  ·  {2 if t < 2.5 else 1}'
            label(caption, 238, 44 if t < 1.35 else 40, stroke=3)
        elif t < 1.35:
            size = round(94 * (.86 + .14 * style.ease(t / .22)))
            label(f'{self.arrows} OK.', 258, size)
            label('İLK HAMLEN?', 354, 48, style.WHITE)
        elif t < 3.5:
            label('HANGİSİ ÖNCE ÇIKAR?', 259, 51, style.WHITE)
            label('A MI, B Mİ?', 322, 44)
            if t >= 1.5:
                draw = ImageDraw.Draw(image)
                draw.ellipse((519, 376, 561, 418), fill=style.INK, outline=style.GOLD, width=2)
                label('2' if t < 2.5 else '1', 383, 30, style.WHITE, 0)
        elif t < 4.65:
            label('ÖNCE A!', 266, 80)
            label('ÖNÜ AÇIK.', 365, 34, GREEN, 2)
        elif t < 5:
            label('İLK OK ÇIKTI.', 302, 53)
        elif t < 7.6:
            label('HIZLI SARALIM.', 268, 43, style.WHITE)
            ImageDraw.Draw(image).rounded_rectangle((425, 325, 655, 395), radius=34, fill=style.INK)
            label(self.rate_label, 340, 43, stroke=0)
        else:
            label('SON OK…', 301, 55)

    def frame(self, index):
        if index >= OUTRO + 6:
            offset = index - OUTRO
            if 20 <= offset <= 63:
                return still_promo(20)
            if offset >= 114:
                return still_promo(115)
            return promo.frame(offset / FPS)
        t = index / FPS
        native = self.native(self.source_frame(index))
        image = native.convert('RGBA')
        self.draw_choices(image, t)
        self.draw_caption(image, t)
        if 3.72 <= t < 4.10:
            x, y = self.paths[0][0]
            u = (t - 3.72) / .38
            radius = 12 + 48 * u
            overlay = Image.new('RGBA', image.size)
            ImageDraw.Draw(overlay).ellipse((x-radius, y-radius, x+radius, y+radius),
                                            outline=(255, 224, 75, round(240 * (1-u))), width=5)
            image.alpha_composite(overlay)
        result = image.convert('RGB')
        for band in HUD:
            assert ImageChops.difference(result.crop(band), native.crop(band)).getbbox() is None, (self.level, index)
        if index >= OUTRO:
            offset = index - OUTRO
            result = Image.blend(result, promo.frame(offset / FPS), style.ease(offset / 6))
        return result
