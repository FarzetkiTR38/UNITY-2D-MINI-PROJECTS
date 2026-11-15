"""Verify new no-Freeze Defense captures and the five delivered series episodes.

Trial checks are explicitly labelled and cannot certify a renderable full source.
Only new QA JSONs are written to the caller's new series output directory.
"""
from __future__ import annotations
import argparse
import csv
import hashlib
import json
import os
from pathlib import Path
import re
import sys

sys.path.insert(0, str(Path(os.environ['TEMP']) / 'ArrowSwarm-Marketing-Media-Python'))
from PIL import Image

ROOT = Path(__file__).resolve().parents[2]
MEDIA = ROOT / 'ArrowSwarm_Marketing'
SERIES = MEDIA / '05_Reels/DefenseSeries01_20261115'
CTA_SHA = '3cffc7f86966e7eb7c4c6e7b0524ea7a2ed59c9c013ad2305cfe0adc857e4c23'


def read(path):
    return json.loads(Path(path).read_text(encoding='utf-8-sig'))


def sha(path):
    return hashlib.sha256(Path(path).read_bytes()).hexdigest()


def rows(path):
    with Path(path).open(encoding='utf-8-sig', newline='') as stream:
        return list(csv.DictReader(stream))


def write(path, value):
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text(json.dumps(value, ensure_ascii=False, indent=2) + '\n', encoding='utf-8')


def verify_native(source, output, allow_trial=False):
    source, output = source.resolve(), output.resolve()
    assert source.is_relative_to((MEDIA / '04_Raw_Gameplay').resolve())
    assert output.is_relative_to(SERIES.resolve()), 'Write proof only inside the new series package'
    p, g = read(source / 'provenance.json'), read(source / 'geometry.json')
    events, telemetry = rows(source / 'events.csv'), rows(source / 'telemetry.csv')
    full = p.get('allFramesSaved') is True
    assert full or allow_trial, 'Sparse trial is not a full video source'
    assert p['won'] and p['lives'] == 3 and p['remaining'] == 0
    assert p['mode'] == 'Defense' and p['language'] == 'tr' and p['level'] > 1
    assert (p['width'], p['height'], p['fps']) == (1080, 1920, 30)
    count, total = p['arrows'], p['frames']
    assert len(telemetry) == total and total > 0 and count >= 2
    assert [int(t['frame']) for t in telemetry] == list(range(total))
    assert all(abs(float(t['time_seconds']) - i / 30) < .00002 for i, t in enumerate(telemetry))
    freeze_charges = {t['freeze_charge'] for t in telemetry}
    assert len(freeze_charges) == 1 and freeze_charges <= {'0', '1'}
    assert all(t['frozen'] == 'False' and t['lives'] == '3' for t in telemetry)
    assert p['freezeFrame'] == -1 and not any(e['event'] in ('freeze', 'freeze_end') for e in events)
    assert p['freezeStartedEvents'] == 0
    first, win = p['firstFireFrame'], p['winFrame']
    assert 0 < first < win < total
    shots = [e for e in events if e['event'] in ('fire', 'rainbow_fire')]
    flights = [e for e in events if e['event'] == 'flight_complete']
    assert len(shots) == len(flights) == count and g['totalArrows'] == count and g['level'] == p['level']
    assert len({e['arrow'] for e in shots}) == len({e['arrow'] for e in flights}) == count
    assert sorted(e['arrow'] for e in shots) == sorted(e['arrow'] for e in flights)
    assert int(shots[0]['frame']) == first and max(int(e['frame']) for e in flights) <= win
    frames = [int(e['frame']) for e in shots]
    assert all(b - a >= p['cadenceFrames'] >= 1 for a, b in zip(frames, frames[1:]))
    assert [int(e['remaining']) for e in shots] == list(range(count - 1, -1, -1))
    assert all(e['event'] == 'fire' for e in shots[:-1]) and shots[-1]['event'] == 'rainbow_fire'
    reserved = p['reservedLastArrow']
    assert int(shots[-1]['arrow']) == reserved and frames[-1] - frames[-2] >= p['lastHoldFrames']
    normal = [e for e in flights if int(e['arrow']) != reserved]
    last = [e for e in flights if int(e['arrow']) == reserved]
    assert len(last) == 1 and int(last[0]['frame']) == win
    assert max(int(e['frame']) for e in normal) <= frames[-1]
    map_path = ROOT / g['mapAsset']
    map_text = map_path.read_text(encoding='utf-8-sig')
    width = int(re.search(r'_gridWidth: (\d+)', map_text)[1])
    height = int(re.search(r'_gridHeight: (\d+)', map_text)[1])
    remaining = {a['index']: a for a in g['arrows']}
    assert len(remaining) == count and {int(e['arrow']) for e in shots} == set(remaining)
    step = {'Up': (0, 1), 'Right': (1, 0), 'Down': (0, -1), 'Left': (-1, 0)}
    for event in shots:
        index = int(event['arrow']); arrow = remaining[index]
        assert event['detail'] == 'weight=' + str(arrow['weight'])
        dx, dy = step[arrow['direction']]; head = arrow['points'][0]
        x, y = head['x'] + dx, head['y'] + dy
        occupied = {(point['x'], point['y']) for a in remaining.values() for point in a['points']}
        while 0 <= x < width and 0 <= y < height:
            assert (x, y) not in occupied, f'Illegal native shot {index}'
            x += dx; y += dy
        del remaining[index]
    damage = [e for e in events if e['event'] == 'projectile_damage']
    kills = [e for e in events if e['event'] == 'projectile_kill']
    assert not any(e['event'] in ('cleanup_kill', 'cleanup_damage', 'mob_finish') for e in events)
    assert p['cleanupKills'] == 0 and len(damage) == p['projectileHits'] > 0
    assert len(kills) == p['projectileKills'] == p['spawnedMobs'] > 0
    spawns = [e for e in events if e['event'] == 'spawn']
    assert len(spawns) == p['spawnedMobs'] and {int(e['arrow']) for e in spawns} == set(range(p['spawnedMobs']))
    assert all(e['detail'] in ('initial_alive', 'natural_spawn') for e in spawns)
    assert {int(e['arrow']) for e in kills} == set(range(p['spawnedMobs']))
    assert len({e['arrow'] for e in kills}) == len(kills)
    paired = {(e['frame'], e['arrow']) for e in damage}
    assert all((e['frame'], e['arrow']) in paired for e in kills)
    assert not any('damage=9999;' in e['detail'] for e in damage)
    rainbow_hits = [e for e in damage if e['detail'].startswith('damage=999;') and int(e['frame']) >= frames[-1]]
    assert rainbow_hits, 'Shown final rainbow must actually hit enemies'
    wins = [e for e in events if e['event'] == 'won']
    assert len(wins) == 1 and int(wins[0]['frame']) == win and wins[0]['detail'] == 'production'
    assert telemetry[win]['alive_mobs'] == '0' and max(int(e['frame']) for e in kills) <= win
    tips = [e for e in events if e['event'] == 'tip']
    tip_used = p['tipUsed'] is True
    assert p['tipUsedEvents'] == int(tip_used)
    if tip_used:
        tip, target = p['tipFrame'], p['tipTargetArrow']
        assert len(tips) == 1 and int(tips[0]['frame']) == tip and int(tips[0]['arrow']) == target
        assert tips[0]['detail'] == 'real_button' and 0 < tip < first
        assert p['tipCountBefore'] == 1 and p['tipCountAfter'] == 0
        assert all(t['tip_charge'] == '1' for t in telemetry[:tip])
        assert all(t['tip_charge'] == '0' for t in telemetry[tip:])
        assert first - tip >= p['tipHoldFrames'] >= 18 and int(shots[0]['arrow']) == target
        assert all(int(t['tip_target']) == target for t in telemetry[tip:first])
        assert all(int(t['tip_target']) == -1 for t in telemetry[first:])
    else:
        assert not tips and p['tipFrame'] == p['tipTargetArrow'] == -1
        assert p['tipCountBefore'] == p['tipCountAfter']
        assert all(int(t['tip_charge']) == p['tipCountBefore'] and int(t['tip_target']) == -1 for t in telemetry)
    approach = [float(t['leading_progress']) for t in telemetry[:p['tipFrame'] if tip_used else first]]
    assert max(approach) > min(approach) and all(b >= a - .00002 for a, b in zip(approach, approach[1:]))
    halfway = next((i for i, t in enumerate(telemetry[:first]) if float(t['leading_progress']) >= .5), None)
    opening_threshold = .30 if tip_used else .32
    action = p['tipFrame'] if tip_used else first
    opening = next((i for i, t in enumerate(telemetry[:action]) if float(t['leading_progress']) >= opening_threshold), 0)
    files = []
    paths = sorted(source.glob('frame_*.jpg'))
    if full:
        assert len(paths) == total and [p.name for p in paths] == [f'frame_{i:05d}.jpg' for i in range(total)]
    for path in paths:
        with Image.open(path) as image:
            image.load(); assert image.size == (1080, 1920) and image.format == 'JPEG'
        files.append({'file': path.name, 'bytes': path.stat().st_size, 'sha256': sha(path)})
    proof = {'passed': True, 'trialOnly': not full, 'allFramesSaved': full, 'source': source.name,
        'level': p['level'], 'frames': total, 'width': 1080, 'height': 1920, 'fps': 30,
        'arrows': count, 'legalShotReplay': count, 'shotOrder': [int(e['arrow']) for e in shots],
        'grid': [width, height], 'freezeUsed': False, 'freezeChargeUnchanged': int(next(iter(freeze_charges))),
        'tipUsed': tip_used, 'tipFrame': p['tipFrame'],
        'tipTargetArrow': p['tipTargetArrow'], 'tipCharge': [p['tipCountBefore'], p['tipCountAfter']],
        'tipHighlightHoldFrames': first - p['tipFrame'] if tip_used else 0,
        'firstShotFrame': first, 'rainbowFrame': frames[-1], 'winFrame': win,
        'minimumShotSpacingFrames': min(b - a for a, b in zip(frames, frames[1:])),
        'rainbowHoldFrames': frames[-1] - frames[-2], 'rainbowNaturalPromotion': True,
        'reservedRainbowArrow': reserved, 'rainbowDamageEvents': len(rainbow_hits),
        'ordinaryFlightsCompletedBeforeRainbow': True, 'rainbowIsFinalCompletedFlight': True,
        'completedFlights': len(flights), 'spawnedMobs': p['spawnedMobs'], 'projectileKills': len(kills),
        'victoryCleanupKills': 0, 'mobFinishes': 0, 'lives': 3, 'remaining': 0,
        'openingFrame': opening, 'openingRequestedThreshold': opening_threshold,
        'openingLeadingProgress': float(telemetry[opening]['leading_progress']),
        'halfwayOpeningFrame': halfway, 'sourceAudioRecorded': False, 'generationSeed': p.get('generationSeed'),
        'sourceMapSha256': sha(map_path), 'sourceGeometrySha256': sha(source / 'geometry.json'),
        'sourceProvenanceSha256': sha(source / 'provenance.json'), 'sourceEventsSha256': sha(source / 'events.csv'),
        'sourceTelemetrySha256': sha(source / 'telemetry.csv'),
        'claimLimit': 'Event/geometry replay proves this captured attempt; no human-finger, recorded native audio or live-service claim.'}
    write(output / 'native_capture_checks.json', proof)
    write(output / 'frame_manifest.json', files)
    return proof


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--source', required=True, type=Path)
    parser.add_argument('--output', required=True, type=Path)
    parser.add_argument('--allow-trial', action='store_true')
    args = parser.parse_args()
    proof = verify_native(args.source, args.output, args.allow_trial)
    print(json.dumps(proof, ensure_ascii=False, indent=2), flush=True)


if __name__ == '__main__':
    main()
