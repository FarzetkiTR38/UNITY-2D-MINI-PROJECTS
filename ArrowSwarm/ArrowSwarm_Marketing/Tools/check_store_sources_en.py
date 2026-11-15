"""Check the eight native English store sources without changing game/source pixels.

Checks Unity's visible-TMP inventory as evidence, not OCR. A separate human-style
visual review remains necessary for layout, legibility and non-TMP artwork.
Writes only native_source_checks.json beside the Native directory.
"""
from __future__ import annotations

import argparse
import hashlib
import json
import os
from pathlib import Path
import re
import sys

sys.path.insert(0, str(Path(os.environ['TEMP']) / 'ArrowSwarm-Marketing-Media-Python'))
from PIL import Image

ROOT = Path(__file__).resolve().parents[2]
PACKAGE = ROOT / 'ArrowSwarm_Marketing/03_GooglePlay/StoreSet_EN_20261115'
EXPECTED = (
    ('01_Home_EN.png', 'Home', 'MainMenuScene', 10),
    ('02_Defense_EN.png', 'Defense', 'GameScene', 10),
    ('03_Shapes_EN.png', 'Shapes', 'ShapesScene', 12),
    ('04_ColorShift_EN.png', 'ColorShift', 'ColorShiftScene', 3),
    ('05_Defense_Freeze_EN.png', 'Defense', 'GameScene', 15),
    ('06_Shapes_Advanced_EN.png', 'Shapes', 'ShapesScene', 21),
    ('07_ColorShift_Rotate_EN.png', 'ColorShift', 'ColorShiftScene', 15),
    ('08_Leaderboard_EN.png', 'Leaderboard', 'MainMenuScene', 20),
)
TURKISH_WORDS = re.compile(
    r'\b(?:oyna|ayarlar|bölümler|bölüm|lider|tablosu|şekiller|savunma|'
    r'ipucu|dondur|yenile|yıldız|oklar|açık|koyu|döndür|başarılı|başarısız)\b',
    re.IGNORECASE,
)
TURKISH_GLYPHS = re.compile('[çğıİöşüÇĞÖŞÜ]')
FORBIDDEN_PRIVATE_NAMES = ('SwarmStriker_83', 'HyperArcher_15')
DEMO_NAMES = (
    'SkyRunner', 'PixelNova', 'LunaArrow', 'NeonWing', 'MapleFox', 'Nova',
    'BlueComet', 'CoralCloud', 'SunnyOrbit', 'JadeSpark',
)


def sha(path: Path) -> str:
    return hashlib.sha256(path.read_bytes()).hexdigest()


def verify(package: Path) -> dict:
    native = package / 'Native'
    manifest_path = native / 'capture_manifest.json'
    manifest = json.loads(manifest_path.read_text(encoding='utf-8-sig'))
    assert manifest['language'] == 'en', 'Global language must be English'
    assert manifest['theme'] == 'Light', 'Expected native Light store set'
    assert manifest['nativeWidth'] == 1080 and manifest['nativeHeight'] == 1620
    assert manifest['externalServiceCalls'] == 0, 'No external requests permitted in isolated capture'
    assert manifest['demoProviderCalls'] > 0, 'Native board must have queried the local demo provider'
    provenance = manifest['leaderboardData'].lower()
    assert 'fictional' in provenance and 'not verified live ranks' in provenance
    shots = manifest['shots']
    assert len(shots) == len(EXPECTED), 'Exactly eight complete shots required'
    assert sorted(p.name for p in native.glob('*.png')) == sorted(x[0] for x in EXPECTED)
    checks = []
    for shot, (filename, mode, scene, level) in zip(shots, EXPECTED):
        assert shot['file'] == filename and shot['mode'] == mode, filename
        assert Path(shot['scene']).stem == scene and shot['level'] == level, filename
        assert shot['language'] == 'en', filename
        assert (shot['width'], shot['height']) == (1080, 1620), filename
        path = native / filename
        with Image.open(path) as image:
            assert image.format == 'PNG' and image.size == (1080, 1620), filename
            assert image.mode in ('RGB', 'RGBA'), filename
            image.verify()
        alpha_range = [255, 255]
        translucent_pixels = 0
        with Image.open(path) as image:
            image.load()
            if image.mode == 'RGBA':
                alpha = image.getchannel('A')
                alpha_range = list(alpha.getextrema())
                assert alpha_range[0] > 0, f'{filename}: fully transparent native pixels'
                translucent_pixels = sum(alpha.histogram()[:255])
        texts = shot['visibleTexts']
        assert texts and all(isinstance(t, str) and t.strip() for t in texts), filename
        joined = '\n'.join(texts)
        assert not TURKISH_GLYPHS.search(joined), f'{filename}: Turkish glyph in TMP inventory'
        assert not TURKISH_WORDS.search(joined), f'{filename}: Turkish UI word'
        assert not any(name in joined for name in FORBIDDEN_PRIVATE_NAMES), filename
        assert 'EDITOR / SAMPLE' not in joined and 'ÖRNEK' not in joined, filename
        if mode == 'Home':
            assert {'PLAY', 'GAME MODE', 'DEFENSE', 'LEVELS', 'LEADERBOARD', 'SETTINGS'} <= set(texts)
        elif mode in ('Defense', 'Shapes', 'ColorShift'):
            assert f'Lv.{level}' in texts and '||' in texts, filename
            assert any(t.startswith('Arrows: ') for t in texts), filename
            if mode == 'ColorShift':
                assert 'Tap or swipe left / right to rotate' in texts, filename
        else:
            assert {'LEADERBOARD', 'REFRESH', 'Global results updated'} <= set(texts)
            assert set(DEMO_NAMES) <= set(texts), 'Only the documented fictional roster expected'
            assert any('[US] Nova' in t and '108 stars' in t and 'World rank: #6' in t for t in texts)
        checks.append({
            'file': filename, 'width': 1080, 'height': 1620,
            'bytes': path.stat().st_size, 'sha256': sha(path),
            'mode': mode, 'level': level, 'language': 'en',
            'visibleTmpTexts': len(texts), 'turkishUiWordsFound': [],
            'nativeAlphaRange': alpha_range,
            'translucentNativeUiEffectPixels': translucent_pixels,
        })
    state_path = native / 'state_checks.txt'
    state_text = state_path.read_text(encoding='utf-8-sig')
    assert 'FAIL' not in state_text.upper() and 'PASS' in state_text.upper(), 'Native state checks failed/missing'
    return {
        'passed': True,
        'scope': 'PNG decode/dimensions/nonempty alpha, manifest order and English visible-TMP inventory; not OCR or pixel layout certification',
        'alphaHandling': 'Unity may preserve partial UI-effect alpha; final composer uses rendered RGB values without repainting game pixels',
        'nativeCount': len(checks), 'externalServiceCallsReportedByManifest': 0,
        'externalServiceEvidenceLimit': 'Manifest counters plus inspected in-process provider source; not network packet capture',
        'leaderboard': 'Authentic native UI; fictional demonstration roster, not verified live/global ranks',
        'manifestSha256': sha(manifest_path), 'stateChecksSha256': sha(state_path),
        'shots': checks,
    }


def main() -> None:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--package', type=Path, default=PACKAGE)
    args = parser.parse_args()
    package = args.package.resolve()
    proof = verify(package)
    output = package / 'native_source_checks.json'
    output.write_text(json.dumps(proof, ensure_ascii=False, indent=2) + '\n', encoding='utf-8')
    print(f'PASS: {proof["nativeCount"]} native English PNGs; {output}')


if __name__ == '__main__':
    main()
