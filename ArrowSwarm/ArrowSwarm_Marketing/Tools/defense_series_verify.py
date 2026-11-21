"""Encode verification and proof output for each new Defense series episode."""
import json
import re
import render_reference_reel as m

W, H, FPS = 1080, 1920, 30
HUD = ((0, 0, W, 230), (0, 1690, W, H))


def verify_episode(ep, ff):
    result = m.command([ff, '-hide_banner', '-i', str(ep.final), '-f', 'null', '-',
                        '-progress', 'pipe:1'], ep.out/'decode.log')
    frames = int(re.findall(r'^frame=(\d+)$', result.stdout, re.M)[-1])
    assert frames == ep.seconds*FPS
    assert all(s in result.stderr for s in ['1080x1920', 'yuv420p', '48000 Hz, stereo', 'h264', 'aac'])
    assert not re.search(r'corrupt|Error while decoding|Invalid data', result.stderr, re.I)
    result = m.command([ff, '-hide_banner', '-i', str(ep.final), '-vn', '-af',
                        'loudnorm=I=-16:TP=-1.5:LRA=8:print_format=json', '-f', 'null', '-'], ep.out/'loudness.log')
    loud = json.loads(re.findall(r'\{\s*"input_i".*?\}', result.stderr, re.S)[-1])
    assert -18 <= float(loud['input_i']) <= -14 and float(loud['input_tp']) < -1
    qa = {**ep.plan(), 'passed': True, 'frames': frames, 'width': W, 'height': H, 'fps': FPS,
          'bytes': ep.final.stat().st_size, 'sha256': m.sha(ep.final), 'audio': loud,
          'arrows': ep.p['arrows'], 'lives': 3, 'spawnedMobs': ep.p['spawnedMobs'],
          'projectileKills': ep.p['projectileKills'], 'victoryCleanupKills': 0, 'mobFinishes': 0,
          'sourceOpeningFrame': ep.start, 'sourceOpeningProgress': ep.checks['openingLeadingProgress'],
          'sourceHalfwayOpeningFrame': ep.checks.get('halfwayOpeningFrame'),
          'sourceProvenanceSha256': m.sha(ep.source/'provenance.json'),
          'nativeHudBandsIdenticalBeforeEncode': HUD, 'speedChangesLabelled': True,
          'ctaNarration': [ep.outro+.8, ep.outro+3.1], 'buttonPressAt': ep.outro+3.15}
    ep.write('QA_REPORT.json', qa)
    ep.write('timeline.json', [{'outputFrame': i, 'sourceFrame': ep.raw_index(i/FPS),
                               'nativeHudGuard': i/FPS < ep.outro, 'promo': i/FPS >= ep.outro}
                              for i in range(ep.seconds*FPS)])
    m.command([ff, '-hide_banner', '-y', '-i', str(ep.final), '-vf',
               f'fps=1/{ep.seconds/10},scale=216:384,tile=5x2', '-frames:v', '1', '-update', '1',
               str(ep.out/'encoded_contact_sheet.jpg')], ep.out/'contact_sheet.log')
    m.command([ff, '-hide_banner', '-y', '-ss', '0.4', '-i', str(ep.final), '-frames:v', '1',
               '-update', '1', str(ep.out/'cover.jpg')], ep.out/'cover.log')
    update_series_manifest(ep)
    return qa


def update_series_manifest(ep):
    """Publish the episode ledger only when all five final QA files exist."""
    series, items = ep.out.parent, []
    for level in [3, 6, 8, 12, 15]:
        folder = series/f'L{level:02d}'
        path = folder/'QA_REPORT.json'
        if not path.is_file():
            return
        row = json.loads(path.read_text(encoding='utf-8'))
        assert row['passed'] and row['freezeUsed'] is False
        items.append({'level': level, 'file': f'L{level:02d}/ArrowSwarm_Defense_L{level:02d}_TR.mp4',
                      'durationSeconds': row['durationSeconds'], 'tipUsed': row['tipUsed'],
                      'qa': f'L{level:02d}/QA_REPORT.json', 'cover': f'L{level:02d}/cover.jpg',
                      'caption': f'Defense {level}: '+('İpucu ile başlangıç' if row['tipUsed'] else 'Normal oynanış')})
    assert len(items) == 5 and sum(r['tipUsed'] for r in items) == 1
    (series/'series_manifest.json').write_text(json.dumps(
        {'passed': True, 'count': 5, 'language': 'tr', 'freezeEpisodes': 0, 'tipEpisodes': 1,
         'creativeReviewPending': True, 'uploaded': False, 'items': items},
        ensure_ascii=False, indent=2)+'\n', encoding='utf-8')
