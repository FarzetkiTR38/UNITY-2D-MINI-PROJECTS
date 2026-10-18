"""Compare committed R3 artwork with current Shapes using only Python's standard library."""
import collections
import hashlib
import json
from pathlib import Path
import random
import re
import statistics
import subprocess

ROOT = Path(__file__).resolve().parents[2]
BASELINE = '1bdf74e'
GIT_PREFIX = subprocess.check_output(['git', 'rev-parse', '--show-prefix'], cwd=ROOT, text=True).strip()
ASSETS = Path('Assets/_Project/ScriptableObjects/Puzzles')


def read_asset(number, revision=None):
    relative = ASSETS / f'Shapes_{number}.asset'
    path = ROOT / relative
    stamp = None if revision else path.stat().st_mtime_ns
    if revision:
        content = subprocess.check_output(['git', 'show', f'{revision}:{GIT_PREFIX}{relative.as_posix()}'],
                                          cwd=ROOT, encoding='utf-8-sig')
    else:
        content = path.read_text(encoding='utf-8-sig')
        assert stamp == path.stat().st_mtime_ns, 'asset changed while reading'
    level = {'level': number, 'width': int(re.search(r'  _width: (\d+)', content)[1]),
             'height': int(re.search(r'  _height: (\d+)', content)[1]), 'arrows': []}
    source = content.split('  _arrows:\n', 1)[1].split('  _wallColors:', 1)[0]
    for chunk in source.split('  - _points:\n')[1:]:
        points = [[int(x), int(y)] for x, y in re.findall(r'    - \{x: (-?\d+), y: (-?\d+)\}', chunk)]
        direction = [int(v) for v in re.search(r'_direction: \{x: (-?\d+), y: (-?\d+)\}', chunk).groups()]
        color = [float(v) for v in re.search(r'_color: \{r: ([^,]+), g: ([^,]+), b: ([^,]+), a: ([^}]+)\}', chunk).groups()]
        level['arrows'].append({'points': points, 'direction': direction, 'color': color})
    return level, stamp


def graph(level):
    owner, colors = {}, {}
    for i, arrow in enumerate(level['arrows']):
        assert len(arrow['points']) >= 2
        assert [a-b for a, b in zip(arrow['points'][0], arrow['points'][1])] == arrow['direction']
        assert sum(map(abs, arrow['direction'])) == 1
        for j, point in enumerate(arrow['points']):
            p = tuple(point)
            assert p not in owner
            assert 0 <= p[0] < level['width'] and 0 <= p[1] < level['height']
            if j:
                assert sum(abs(a-b) for a, b in zip(point, arrow['points'][j-1])) == 1
            owner[p] = i
            colors[p] = tuple(int(v*255+.5) for v in arrow['color'])
    dependencies = []
    for i, arrow in enumerate(level['arrows']):
        dx, dy = arrow['direction']
        x, y = arrow['points'][0]
        x += dx
        y += dy
        blockers = set()
        while 0 <= x < level['width'] and 0 <= y < level['height']:
            if (x, y) in owner:
                blockers.add(owner[x, y])
            x += dx
            y += dy
        assert i not in blockers
        dependencies.append(blockers)
    dependents = [[] for _ in dependencies]
    for i, blockers in enumerate(dependencies):
        for other in blockers:
            dependents[other].append(i)
    return dependencies, dependents, colors


def choices(dependencies, dependents, policy, seed=0):
    count = len(dependencies)
    degrees = list(map(len, dependencies))
    removed = [False] * count
    rng = random.Random(seed)
    first, end = int(count*.2), max(int(count*.2)+1, int(count*.8))
    middle, initial = [], None
    for step in range(count):
        legal = [i for i in range(count) if not removed[i] and degrees[i] == 0]
        assert legal, 'dependency cycle'
        ratio = len(legal)/(count-step)
        if initial is None:
            initial = ratio
        if first <= step < end:
            middle.append(ratio)
        if policy == 'index':
            selected = legal[0]
        elif policy == 'reverse_index':
            selected = legal[-1]
        elif policy == 'random':
            selected = rng.choice(legal)
        else:
            def unlocks(i):
                return sum(degrees[d] == 1 for d in dependents[i])
            selected = min(legal, key=lambda i: (unlocks(i) if policy == 'lowest_unlock' else -unlocks(i), i))
        removed[selected] = True
        for dependent in dependents[selected]:
            degrees[dependent] -= 1
    return {'initial': initial, 'mid': statistics.mean(middle), 'peak_mid': max(middle)}


def audit(level):
    dependencies, dependents, colors = graph(level)
    result = {policy: choices(dependencies, dependents, policy) for policy in
              ['index', 'reverse_index', 'lowest_unlock', 'highest_unlock']}
    for seed in range(8):
        result[f'random_{seed}'] = choices(dependencies, dependents, 'random', 81017 + seed*997 + level['level']*31)
    result['random_mean'] = {key: statistics.mean(result[f'random_{seed}'][key] for seed in range(8))
                             for key in ['initial', 'mid', 'peak_mid']}
    direction_counts = collections.Counter(tuple(a['direction']) for a in level['arrows'])
    result['largest_direction_ratio'] = max(direction_counts.values())/len(level['arrows'])
    assert len(direction_counts) == 4
    result['arrows'] = len(level['arrows'])
    return result, colors


original = [read_asset(number, BASELINE)[0] for number in range(1, 51)]
current, stamps = [], []
for number in range(1, 51):
    level, stamp = read_asset(number)
    current.append(level)
    stamps.append(stamp)
rows = []
for old, new in zip(original, current):
    before, old_colors = audit(old)
    after, new_colors = audit(new)
    assert old['width'] == new['width'] and old['height'] == new['height']
    assert old_colors == new_colors, f'artwork differs in {old["level"]}'
    rows.append({'level': old['level'], 'before': before, 'after': after})
for number, stamp in enumerate(stamps, 1):
    assert read_asset(number)[1] == stamp, 'assets changed during audit'
policies = ['index', 'reverse_index', 'lowest_unlock', 'highest_unlock', 'random_mean'] + [f'random_{i}' for i in range(8)]
summary = {}
for policy in policies:
    data = []
    for start, end in [(0, 50), (0, 10), (10, 30), (30, 50)]:
        subset = rows[start:end]
        before = statistics.mean(row['before'][policy]['mid'] for row in subset)
        after = statistics.mean(row['after'][policy]['mid'] for row in subset)
        data.append({'group': f'{start+1}-{end}', 'old_mid': before, 'new_mid': after,
                     'relative_drop': (before-after)/before,
                     'improved_levels': sum(row['after'][policy]['mid'] < row['before'][policy]['mid'] for row in subset)})
    worst = sorted(rows, key=lambda row: row['after'][policy]['mid'] - row['before'][policy]['mid'], reverse=True)[:5]
    summary[policy] = {'groups': data, 'largest_deltas': [{'level': r['level'], 'old': r['before'][policy]['mid'],
                        'new': r['after'][policy]['mid']} for r in worst]}
out = {'summary': summary, 'rows': rows, 'largest_direction_ratios': sorted(
       [(r['level'], r['after']['largest_direction_ratio']) for r in rows], key=lambda x: -x[1])[:10]}
out['method'] = {
    'original': f'git:{BASELINE}:{GIT_PREFIX}{ASSETS.as_posix()}/Shapes_*.asset',
    'original_sha256': hashlib.sha256(json.dumps(original, sort_keys=True).encode('utf-8')).hexdigest(),
    'candidate': 'Actual persisted Assets/_Project/ScriptableObjects/Puzzles/Shapes_1.asset ... Shapes_50.asset',
    'midgame': 'Mean legal/remaining ratio before each removal at floor(N*.2) <= step < floor(N*.8)',
    'policies': ['lowest legal index', 'highest legal index', 'fewest immediate unlocks (tie: index)',
                 'most immediate unlocks (tie: index)', '8 random seeds: 81017 + seed*997 + level*31'],
    'runs': 1200,
    'validation': 'Each complete removal policy requires a nonempty legal frontier until all arrows are removed; exact Color32 cell colors and dimensions match R3; all paths unique, orthogonal, contiguous, in bounds and direction-aligned; all four head directions present; no asset changed during reading.',
    'limitation': 'Choice pressure is a structural proxy, not a substitute for human visual playtesting. Every individual level need not improve under every policy.'
}
out['asset_sha256'] = {str(n): hashlib.sha256((ROOT/ASSETS/f'Shapes_{n}.asset').read_bytes()).hexdigest() for n in range(1, 51)}
output = Path(__file__).resolve().with_name('policy-comparison.json')
output.write_text(json.dumps(out, indent=2), encoding='utf-8')
for policy in policies:
    s = summary[policy]['groups'][0]
    print(f'{policy:16}: {s["old_mid"]:.4f} -> {s["new_mid"]:.4f}; drop={s["relative_drop"]:.1%}; improved={s["improved_levels"]}/50')
print('RESULT:', output)
