"""Compose verified Turkish store artwork and pack the release marketing files."""
from __future__ import annotations

import json
import re
import zipfile
from hashlib import sha256
from pathlib import Path
from tempfile import TemporaryDirectory

from compose_store_set_en import ITEMS, Image, ImageChops, ImageDraw, compose, font

MEDIA = Path(__file__).resolve().parents[1]
PLAY = MEDIA / '03_GooglePlay'
EN = PLAY / 'StoreSet_EN_20261115'
TR = PLAY / 'StoreSet_TR_20261118'
OUT = MEDIA / '10_Launch/ReleaseKit_20261118'
VIDEO = MEDIA / '06_Trailers_Ads_Tutorials/StoreLaunch_20261115'
ASO = MEDIA / '07_ASO_SEO_GEO'
SOCIAL = MEDIA / '08_Social_Media/LAUNCH_POST_PACK_TR_EN.md'
ZIP_NAME = 'ArrowSwarm_Yayin_Marketing_TR_EN.zip'

TR_COPY = [
    (('ÜÇ MOD', 'TEK OYUN'), 'Ana menü, Oyna düğmesi ve üç oyun modu.'),
    (('OKLARI GÖNDER', 'SÜRÜYÜ DURDUR'), 'Savunma bölümünde oklarla ilerleyen düşmanları durdur.'),
    (('ŞEKİLLERİ', 'ÇÖZ'), 'Kaplumbağa biçimindeki bulmacada okların çıkış sırasını bul.'),
    (('RENKLERİ', 'EŞLEŞTİR'), 'Renk Değişimi bulmacasında okları doğru renkteki kenarlardan çıkar.'),
    (('DÜŞMANLARI', 'DONDUR'), 'Savunmada Dondur becerisi ilerleyen düşmanları buzla kaplıyor.'),
    (('ÇIKIŞ YOLUNU', 'BUL'), 'Mor ahtapot biçiminde ileri seviye Şekiller bulmacası.'),
    (('ÇERÇEVEYİ', 'DÖNDÜR'), 'Renk Değişimi’nde çerçeve dönünce oklar yeni renklerle eşleşiyor.'),
    (('ÖRNEK SIRALAMA', 'YILDIZ TOPLA'), 'Temsili oyuncular ve örnek sıralar gösteren liderlik tablosu.'),
]
EN_ALT = [
    'Arrow Swarm home screen with Play and three game modes.',
    'Defense gameplay with advancing enemies and an arrow puzzle.',
    'A turtle-shaped arrow puzzle in Shapes mode.',
    'Match arrows with the purple and gold exits in Color Shift.',
    'The Freeze skill covers advancing Defense enemies in ice.',
    'An advanced purple octopus-shaped arrow puzzle.',
    'The frame rotates to match new exit colors in Color Shift.',
    'A demo leaderboard with fictional players and example ranks.',
]


def write(path, data):
    path.write_text(json.dumps(data, ensure_ascii=False, indent=2) + '\n', encoding='utf-8')


def digest(path):
    return sha256(path.read_bytes()).hexdigest()


def make_image(native, target, lines, colors):
    with Image.open(native) as source:
        image, body, rect, footer, treatment, size, _ = compose(source, lines, *colors)
    target.parent.mkdir(parents=True, exist_ok=True)
    image.save(target, optimize=True)
    with Image.open(target) as check:
        assert check.mode == 'RGB' and check.size == (1080, 1920)
        assert ImageChops.difference(check.crop(rect), body).getbbox() is None
    return {'file': target.name, 'sha256': digest(target), 'nativeSha256': digest(native),
            'nativeBody': rect, 'footerTop': footer, 'treatment': treatment, 'fontPx': size}


def make_store_images():
    capture = json.loads((TR / 'Native/capture_manifest.json').read_text(encoding='utf-8-sig'))
    assert capture['language'] == 'tr' and capture['externalServiceCalls'] == 0 and len(capture['shots']) == 8
    shots = []
    for (en, _, _, light, dark), (lines, alt), record in zip(ITEMS, TR_COPY, capture['shots'], strict=True):
        name = en.replace('_EN.png', '_TR.png')
        assert name == record['file'] and record['language'] == 'tr'
        shots.append({**make_image(TR / 'Native' / name, TR / 'Final' / name, lines, (light, dark)),
                      'caption': ' / '.join(lines), 'alt': alt})
    write(TR / 'store_set_manifest.json', {'pass': True, 'language': 'tr', 'items': shots, 'size': [1080, 1920]})
    overview = Image.new('RGB', (1120, 1058), '#0a1931')
    draw = ImageDraw.Draw(overview)
    draw.text((32, 16), 'ARROWSWARM  /  GOOGLE PLAY  /  TÜRKÇE', font=font(26), fill='white')
    for index, row in enumerate(shots):
        x, y = 32 + (index % 4) * 272, 62 + (index // 4) * 498
        with Image.open(TR / 'Final' / row['file']) as shot:
            overview.paste(shot.resize((240, 427), Image.Resampling.LANCZOS), (x, y))
        draw.text((x, y + 441), f'{index + 1:02d}  {row["caption"].split(" / ")[0]}', font=font(17), fill='white')
    overview.save(TR / 'overview_TR.jpg', quality=91)
    original, _, _, top, bottom = ITEMS[-1]
    demo = OUT / '08_Leaderboard_Demo_EN.png'
    make_image(EN / 'Native' / original, demo, ('DEMO RANKINGS', 'EARN STARS'), (top, bottom))
    return shots, demo


def add_assets(archive, shots, demo):
    icon = PLAY / 'app_icon_512x512.png'
    with Image.open(icon) as image:
        assert image.mode == 'RGBA' and image.size == (512, 512) and icon.stat().st_size < 1024 * 1024
    archive.write(icon, '03_GooglePlay/' + icon.name)
    for lang in ('EN', 'TR'):
        graphic = PLAY / f'feature_graphic_final_{lang}_1024x500.png'
        with Image.open(graphic) as image:
            assert image.mode == 'RGB' and image.size == (1024, 500)
        archive.write(graphic, f'03_GooglePlay/{lang}/{graphic.name}')
        names = []
        alts = EN_ALT if lang == 'EN' else [shot['alt'] for shot in shots]
        for index, (item, alt) in enumerate(zip(ITEMS, alts, strict=True)):
            assert len(alt) <= 140
            if lang == 'EN':
                path = demo if index == 7 else EN / 'Final' / item[0]
            else:
                path = TR / 'Final' / shots[index]['file']
            with Image.open(path) as image:
                assert image.mode == 'RGB' and image.size == (1080, 1920)
            archive.write(path, f'03_GooglePlay/{lang}/Screenshots/{path.name}')
            names.append((path.name, alt))
        archive.writestr(f'03_GooglePlay/{lang}/alt_text.txt', '\n'.join(f'{n}\t{a}' for n, a in names) + '\n')

    metadata = json.loads((ASO / 'store_metadata.json').read_text(encoding='utf-8'))['languages']
    assert set(metadata) == set('TR EN DE FR ES PT IT RU JA KO'.split())
    limits = {'title': 30, 'short': 80, 'long': 4000}
    for lang, row in metadata.items():
        for field, limit in limits.items():
            source = ASO / f'play_{lang.lower()}_{field}.txt'
            value = source.read_text(encoding='utf-8')
            assert value == row[field] and len(value.encode('utf-16-le')) // 2 <= limit
            archive.write(source, f'07_ASO_SEO_GEO/{lang}/{source.name}')
    trailer = VIDEO / 'ArrowSwarm_StoreTrailer_EN_25s.mp4'
    archive.write(trailer, trailer.relative_to(MEDIA).as_posix())
    for name in ('ArrowSwarm_FailAd_EN_15s.mp4', 'fail_captions_EN.srt'):
        clip = VIDEO / name
        archive.write(clip, clip.relative_to(MEDIA).as_posix())
    archive.write(SOCIAL, SOCIAL.relative_to(MEDIA).as_posix())
    for relative in sorted(set(re.findall(r'\]\(\.\./([^\)]+\.(?:mp4|srt))\)', SOCIAL.read_text(encoding='utf-8')))):
        if relative != trailer.relative_to(MEDIA).as_posix():
            archive.write(MEDIA / relative, relative)


def main():
    OUT.mkdir(parents=True, exist_ok=True)
    shots, demo = make_store_images()
    path = OUT / ZIP_NAME
    with TemporaryDirectory(dir=OUT) as staging:
        candidate = Path(staging) / ZIP_NAME
        with zipfile.ZipFile(candidate, 'w', compression=zipfile.ZIP_DEFLATED) as archive:
            add_assets(archive, shots, demo)
            archive.write(OUT / 'BASLA.md', 'BASLA.md')
        with zipfile.ZipFile(candidate) as archive:
            assert archive.testzip() is None and len(archive.namelist()) == len(set(archive.namelist()))
            evidence = [{'file': name, 'sha256': sha256(archive.read(name)).hexdigest()}
                        for name in archive.namelist()]
        candidate.replace(path)
    write(OUT / 'package_checks.json', {'passed': True, 'zip': ZIP_NAME, 'sha256': digest(path),
          'bytes': path.stat().st_size, 'screenshots': {'en': 8, 'tr': 8}, 'storeTextLocales': 10, 'files': evidence})
    print(f'PASS: {len(evidence)} files, 16 store screenshots, 10 listing locales, ZIP integrity')


if __name__ == '__main__':
    main()
