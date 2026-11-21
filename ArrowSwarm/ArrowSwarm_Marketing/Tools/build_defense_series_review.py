"""Build the five-episode local review only after final/native evidence passes."""
from __future__ import annotations
import html
import json
from pathlib import Path

from check_defense_series import CTA_SHA, SERIES, read, sha, write


STYLE = r'''
:root{color-scheme:dark;--paper:#071328;--card:#0e2040;--line:#264367;--ink:#eaf5ff;--muted:#a9bed6;--cyan:#51dcff;--gold:#ffdb4c}
*{box-sizing:border-box}body{margin:0;background:radial-gradient(ellipse at 20% 0,#103761,var(--paper) 65%);color:var(--ink);font:16px/1.5 system-ui,-apple-system,"Segoe UI",sans-serif}main{max-width:1240px;margin:auto;padding:32px 24px 48px}h1{font-size:clamp(29px,4vw,48px);line-height:1.12;letter-spacing:-.035em;margin:10px 0 16px}h2{font-size:21px;margin:0 0 10px}p{color:var(--muted)}.eyebrow{font-size:12px;font-weight:800;letter-spacing:.14em;color:var(--cyan)}header{max-width:850px;margin-bottom:25px}.tags{display:flex;gap:7px;flex-wrap:wrap}.tag{font-size:12px;border:1px solid var(--line);border-radius:99px;padding:5px 10px}.gold{color:var(--gold)}.layout{display:grid;grid-template-columns:minmax(280px,470px) minmax(0,1fr);gap:27px;align-items:start}.player{position:sticky;top:20px;min-width:0}.video-wrap{background:#020813;border:1px solid var(--line);border-radius:21px;overflow:hidden;line-height:0}video{display:block;width:100%;aspect-ratio:9/16;max-height:calc(100vh - 170px);object-fit:contain}.controls,.links,.chapters{display:flex;flex-wrap:wrap;gap:8px;margin-top:12px}button,.button{font:inherit;border:1px solid var(--line);border-radius:12px;padding:10px 13px;background:#142e52;color:var(--ink);cursor:pointer;text-decoration:none;min-height:44px;touch-action:manipulation}button:hover,.button:hover{border-color:var(--cyan)}button:focus-visible,a:focus-visible,input:focus-visible{outline:3px solid var(--gold);outline-offset:3px}.primary{background:var(--cyan);color:#072442;font-weight:800}.transport{display:flex;gap:10px;align-items:center;margin-top:14px}input[type=range]{min-width:0;flex:1;accent-color:var(--cyan)}.clock{font-size:12px;min-width:93px;text-align:right}.status{min-height:1.5em;font-size:13px;margin:8px 0}.card{border:1px solid var(--line);border-radius:20px;padding:20px;background:#0e2040e6;margin-bottom:16px}.episodes{display:grid;gap:10px}.episode{display:grid;grid-template-columns:58px minmax(0,1fr);gap:13px;align-items:center;text-align:left;width:100%;padding:11px;border-radius:16px}.episode[aria-pressed=true]{border-color:var(--cyan);background:#163e62;box-shadow:inset 0 0 0 1px var(--cyan)}.episode img{width:58px;height:89px;object-fit:cover;border-radius:9px}.episode strong{display:block;font-size:16px}.episode small{display:block;color:var(--muted);margin-top:4px;font-size:12px}.chapters button{font-size:12px;flex:1;white-space:nowrap}.small{font-size:13px}.notice{border-left:3px solid var(--gold);padding-left:13px}.links a{font-size:13px}footer{margin-top:30px;color:var(--muted);font-size:12px}.store{background:#096599;font-weight:800;width:100%}.summary{margin:10px 0 0}.hero-actions{display:flex;gap:8px;flex-wrap:wrap;margin:16px 0 22px}
@media(max-width:760px){main{padding:23px 15px 35px}.layout{grid-template-columns:1fr;gap:17px}.player{position:static;max-width:470px;width:100%;margin:auto}video{max-height:none}.card{padding:17px}.episodes{grid-template-columns:repeat(2,minmax(0,1fr))}.episode{grid-template-columns:1fr;gap:8px}.episode img{display:none}.episode strong{font-size:14px}.episode small{font-size:11px}.hero-actions .button{flex:1;text-align:center}.controls button{flex:1;padding-inline:8px;font-size:14px}}@media(prefers-reduced-motion:reduce){*{scroll-behavior:auto!important}}
'''

PAGE = r'''<!doctype html><html lang="tr"><head><meta charset="utf-8"><meta name="viewport" content="width=device-width,initial-scale=1"><meta name="color-scheme" content="dark"><title>Arrow Swarm · Beş Yeni Defense Reels</title><style>__STYLE__</style></head><body><main>
<header><div class="eyebrow">ARROW SWARM / DEFENSE · YENİ SERİ</div><h1>Beş bölüm.<br>Aynı heyecan.</h1><p>Onaylanan R2 kurgusunun devamı: gerçek oyun sahnesi, sırayla giden oklar ve son gökkuşağı anı. Bu seride Dondur kullanılmaz; yalnız bir bölümde gerçek İpucu gösterilir ve anlatılır.</p><div class="tags"><span class="tag gold">5 yeni video</span><span class="tag">1080 × 1920 · 30 fps</span><span class="tag">4 normal · 1 İpucu</span><span class="tag">Gerçek Unity görüntüsü</span></div><div class="hero-actions"><a class="button primary" href="ArrowSwarm_Defense_5_Reels_TR_20261115.zip" download>↓ Beş videoyu indir · ZIP</a><a class="button" href="FINAL_CHECKS.json">Seri kontrolü</a></div></header>
<div class="layout"><section class="player" aria-label="Defense serisi oynatıcısı"><div class="video-wrap"><video id="reel" playsinline preload="metadata" aria-label="Defense bölüm videosu"></video></div><div class="controls"><button id="play" class="primary">▶ Sesli oynat</button><button id="pause">Duraklat</button><button id="mute" aria-pressed="false">Sesi kapat</button></div><div class="transport"><input type="range" id="seek" min="0" max="21" step="0.1" value="0" aria-label="Videoda zaman seç"><output class="clock" id="clock">0,0 / 21,0 s</output></div><p id="player-status" class="status" role="status" aria-live="polite"></p></section>
<div><section class="card"><h2>İzlemek istediğin bölümü seç</h2><div class="episodes" role="group" aria-label="Beş Defense videosu">__EPISODES__</div></section><section class="card"><div id="selected-label" class="eyebrow"></div><h2 id="selected-title"></h2><p id="selected-copy"></p><div id="chapters" class="chapters" role="group" aria-label="Video bölümleri"></div><p id="selected-proof" class="small summary"></p><div class="links"><a id="download" class="button" download>↓ Seçili MP4</a><a id="captions" class="button" download>Altyazı · SRT</a><a id="frames" class="button">Kurgu kareleri</a><a id="technical" class="button">Teknik kontrol</a><a id="native" class="button">Oyun kanıtı</a></div></section><section class="card"><h2>Gerçek oyun. Korunan HUD.</h2><p class="small">Top ve Bottom HUD görüntüde kalır. Düşman canı/hızı, hasar, canlar veya bölüm sonucu değiştirilmez. Yasal Editor tıklamaları kullanılır; insan parmağı kaydı iddiası yoktur. Kaynakta ses kaydedilmez; konuşma, müzik, ses vurguları ve yazılar kurguda eklenir. Hız değişimleri videoda belirtilir.</p><p class="small notice">Her kaynakta bütün düşmanlar gerçek atışlarla ölür: zafer temizliği ölümü ve yolu bitiren düşman yoktur. Beş videoda Dondur yoktur; İpucu bölümünde gerçek hak tüketimi ve önerilen okun seçimi kullanılır.</p></section><section class="card"><h2>Sıra sende.</h2><button id="store" class="button store">▶ PLAY NOW</button><p class="small">MOCK · Oyun henüz mağazada yayınlanmadı. Videodaki basma hareketi tanıtım animasyonudur.</p><p id="store-status" class="status" role="status" hidden>Mock düğme çalıştı; yayınlanmış mağaza bağlantısı olmadığı için dış sayfa açılmadı.</p></section></div></div><footer>Farzetki Games · Yerel kreatif inceleme · Otomatik oynatma veya dışarıya yayın yok.</footer></main>
<script>'use strict';const items=__CONFIG__;let selected=0;const v=document.getElementById('reel'),seek=document.getElementById('seek'),clock=document.getElementById('clock'),status=document.getElementById('player-status'),mute=document.getElementById('mute');const fmt=n=>Number(n).toFixed(1).replace('.',',');function duration(){return Number.isFinite(v.duration)&&v.duration>0?v.duration:items[selected].duration}function update(){clock.textContent=fmt(v.currentTime||0)+' / '+fmt(duration())+' s';seek.max=duration();seek.value=v.currentTime||0;seek.setAttribute('aria-valuetext',fmt(v.currentTime||0)+' saniye / '+fmt(duration())+' saniye')}function syncMute(){mute.textContent=v.muted?'Sesi aç':'Sesi kapat';mute.setAttribute('aria-pressed',String(v.muted))}function jump(t){v.pause();const target=Math.max(0,Math.min(t,duration()-.05));if(v.readyState>0){v.currentTime=target;update()}else{v.addEventListener('loadedmetadata',()=>{v.currentTime=target;update()},{once:true});v.load()}}function select(i){selected=i;const c=items[i];v.pause();v.src=c.file;v.poster=c.cover;v.setAttribute('aria-label',c.title+' · '+c.duration+' saniye');v.load();status.textContent='';document.querySelectorAll('.episode').forEach((b,j)=>b.setAttribute('aria-pressed',String(j===i)));document.getElementById('selected-label').textContent=c.tip?'İPUCU KULLANILAN BÖLÜM':'NORMAL OYNANIŞ';document.getElementById('selected-title').textContent=c.title;document.getElementById('selected-copy').textContent=c.copy;document.getElementById('selected-proof').textContent=c.proof;for(const [id,key] of [['download','file'],['captions','captions'],['frames','frames'],['technical','qa'],['native','native']])document.getElementById(id).href=c[key];const chapters=document.getElementById('chapters');chapters.replaceChildren();c.chapters.forEach(([label,time])=>{const b=document.createElement('button');b.type='button';b.textContent=label;b.setAttribute('aria-label',label+' · '+fmt(time)+' saniye');b.onclick=()=>jump(time);chapters.append(b)});syncMute();update()}document.querySelectorAll('.episode').forEach((b,i)=>b.onclick=()=>select(i));document.getElementById('play').onclick=async()=>{try{v.muted=false;syncMute();await v.play();status.textContent=''}catch(e){status.textContent='Video açılamadı; MP4 indir bağlantısını kullanabilirsin.'}};document.getElementById('pause').onclick=()=>v.pause();mute.onclick=()=>{v.muted=!v.muted;syncMute()};seek.oninput=()=>{v.currentTime=Number(seek.value);update()};for(const e of ['loadedmetadata','durationchange','timeupdate'])v.addEventListener(e,update);v.addEventListener('volumechange',syncMute);v.addEventListener('error',()=>status.textContent='Video dosyası yüklenemedi.');document.getElementById('store').onclick=()=>document.getElementById('store-status').hidden=false;select(0);</script></body></html>'''


RAW_SCRIPT = r'''
let rawMode=false;
const editedButton=document.getElementById('edited-version'),rawButton=document.getElementById('raw-version');
const baseSelect=select;
duration=function(){return Number.isFinite(v.duration)&&v.duration>0?v.duration:(rawMode?items[selected].rawDuration:items[selected].duration)};
syncMute=function(){if(rawMode)v.muted=true;mute.disabled=rawMode;mute.textContent=rawMode?'Ham kayıt sessiz':v.muted?'Sesi aç':'Sesi kapat';mute.setAttribute('aria-pressed',String(v.muted));document.getElementById('play').textContent=rawMode?'▶ Sessiz oynat':'▶ Sesli oynat'};
function versionTabs(){editedButton.setAttribute('aria-pressed',String(!rawMode));rawButton.setAttribute('aria-pressed',String(rawMode))}
select=function(i){rawMode=false;baseSelect(i);versionTabs();document.getElementById('raw-download').href=items[i].rawFile};
function switchVersion(raw){rawMode=raw;const c=items[selected];v.pause();v.src=raw?c.rawFile:c.file;v.poster=raw?'':c.cover;v.setAttribute('aria-label',c.title+(raw?' · Kesintisiz sessiz ham kayıt':' · Kurgu'));v.load();status.textContent=raw?'Ham kaynak sessizdir; bütün native kareler sırayla gösterilir.':'';document.getElementById('selected-label').textContent=raw?'KESİNTİSİZ HAM KAYIT · SESSİZ':c.tip?'İPUCU KULLANILAN BÖLÜM':'NORMAL OYNANIŞ';document.getElementById('selected-title').textContent=c.title+(raw?' · Ham kaynak':'');document.getElementById('selected-copy').textContent=raw?'Tam doğal bölüm akışı; kurgu yazısı, ses, kırpma veya hız değişimi yoktur.':c.copy;document.getElementById('frames').href=raw?c.rawFrames:c.frames;const chapters=document.getElementById('chapters');chapters.replaceChildren();(raw?c.rawChapters:c.chapters).forEach(([label,time])=>{const b=document.createElement('button');b.type='button';b.textContent=label;b.setAttribute('aria-label',label+' · '+fmt(time)+' saniye');b.onclick=()=>jump(time);chapters.append(b)});versionTabs();syncMute();update()}
editedButton.onclick=()=>switchVersion(false);rawButton.onclick=()=>switchVersion(true);
document.getElementById('play').onclick=async()=>{try{v.muted=rawMode;syncMute();await v.play();status.textContent=rawMode?'Kesintisiz ham kayıt sessiz oynatılıyor.':''}catch(e){status.textContent='Video açılamadı; MP4 indir bağlantısını kullanabilirsin.'}};
select(0);
'''


def build(package=SERIES):
    package = package.resolve()
    assert package == SERIES.resolve(), 'Review is limited to the new series package'
    path = package / 'series_manifest.json'
    if not path.is_file():
        return {'passed': False, 'waitingFor': ['series_manifest.json'], 'indexWritten': False}
    manifest = read(path); items = manifest['items']
    assert len(items) == 5 and len({x['level'] for x in items}) == 5
    configs, buttons = [], []
    for i, item in enumerate(items):
        level = item['level']; folder = package / f'L{level:02d}'
        video = package / item['file']; qa = read(folder / 'QA_REPORT.json')
        native = read(folder / 'native_capture_checks.json'); audio = read(folder / 'audio_timeline.json')
        raw = read(folder / 'native_source_silent_QA.json')
        assert raw['passed'] and raw['sha256'] == sha(folder / raw['file'])
        assert raw['sourceProvenanceSha256'] == native['sourceProvenanceSha256']
        assert raw['frames'] == raw['decodedFrames'] == native['frames']
        assert video.resolve().is_relative_to(package) and video.is_file()
        assert qa['passed'] and native['passed'] and native['allFramesSaved'] and not native['trialOnly']
        assert qa['sha256'] == sha(video) and qa['bytes'] == video.stat().st_size
        assert (qa['width'], qa['height'], qa['fps']) == (1080, 1920, 30)
        assert qa['frames'] == round(qa['durationSeconds'] * 30) and 18 <= qa['durationSeconds'] <= 25
        assert qa['freezeUsed'] is False and native['freezeUsed'] is False
        tip = qa['tipUsed']; assert tip == native['tipUsed'] == item['tipUsed']
        assert native['projectileKills'] == native['spawnedMobs'] > 0
        assert native['victoryCleanupKills'] == native['mobFinishes'] == 0 and native['lives'] == 3
        assert audio['voices'][-1]['sha256'] == CTA_SHA
        duration = float(qa['durationSeconds']); promo = float(qa['promoAt'])
        assert qa['naturalWinAt'] < promo < duration
        assert all(0 <= x['start'] < x['end'] <= duration for x in audio['voices'])
        assert all(a['end'] <= b['start'] for a, b in zip(audio['voices'], audio['voices'][1:]))
        folder_name = f'L{level:02d}'
        for required in ['cover.jpg', 'captions_TR.srt', 'encoded_contact_sheet.jpg', 'source_provenance.json']:
            assert (folder / required).is_file(), f'Missing {folder_name}/{required}'
        chapters = [('Başlangıç', 0)]
        if tip:
            assert 0 <= qa['tipAt'] < qa['firstShotAt']
            chapters.append(('İpucu', qa['tipAt']))
        chapters.extend([('İlk atış', qa['firstShotAt']), ('Gökkuşağı', qa['rainbowAt']),
                         ('Sonuç', qa['naturalWinAt']), ('Tanıtım', promo)])
        assert all(0 <= t < duration for _, t in chapters)
        title = f'Bölüm {level} · ' + ('İpucu' if tip else 'Normal oynanış')
        copy = ('Gerçek İpucu hakkını kullan, önerilen oku gör ve sırayla gönder.' if tip else
                'Dondur kullanmadan, okları sırayla gönder ve son gökkuşağı anını izle.')
        configs.append({'level': level, 'tip': tip, 'title': title, 'copy': copy,
            'duration': duration, 'file': item['file'], 'cover': f'{folder_name}/cover.jpg',
            'rawFile': f'{folder_name}/{raw["file"]}', 'rawDuration': raw['durationSeconds'],
            'rawFrames': f'{folder_name}/native_contact_sheet.jpg',
            'rawChapters': [('Başlangıç', 0)] + ([('İpucu', native['tipFrame'] / 30)] if tip else []) +
                [('İlk atış', native['firstShotFrame'] / 30), ('Gökkuşağı', native['rainbowFrame'] / 30),
                 ('Sonuç', native['winFrame'] / 30)],
            'captions': f'{folder_name}/captions_TR.srt', 'frames': f'{folder_name}/encoded_contact_sheet.jpg',
            'qa': f'{folder_name}/QA_REPORT.json', 'native': f'{folder_name}/native_capture_checks.json',
            'chapters': chapters, 'proof': f"{native['arrows']} yasal atış · {native['projectileKills']} gerçek mermi ölümü · 0 zafer temizliği · 3 can"})
        buttons.append(f'<button class="episode" type="button" aria-pressed="{str(i == 0).lower()}" aria-label="{html.escape(title)}"><img src="{folder_name}/cover.jpg" alt="Bölüm {level} videosundan gerçek oyun karesi" width="58" height="89"><span><strong>{html.escape(title)}</strong><small>{duration:g} saniye · {native["arrows"]} ok</small></span></button>')
    assert sum(x['tip'] for x in configs) == 1
    page = PAGE.replace('__STYLE__', STYLE).replace('__EPISODES__', ''.join(buttons)).replace(
        '__CONFIG__', json.dumps(configs, ensure_ascii=False).replace('</', '<\\/'))
    page = page.replace('<div class="video-wrap">', '<div class="controls" role="group" aria-label="Kurgu veya ham kaynak"><button id="edited-version" aria-pressed="true">Kurgu</button><button id="raw-version" aria-pressed="false">Ham kaynak · sessiz</button></div><div class="video-wrap">')
    page = page.replace('<a id="native"', '<a id="raw-download" class="button" download>Ham kaynak · MP4</a><a id="native"')
    page = page.replace('select(0);', RAW_SCRIPT)
    assert page.count('<video ') == 1 and ' autoplay' not in page and 'href="http' not in page
    (package / 'index.html').write_text(page, encoding='utf-8')
    proof = {'passed': True, 'episodes': 5, 'distinctLevels': [x['level'] for x in configs],
        'tipEpisodes': 1, 'freezeEpisodes': 0, 'onePlayer': True, 'autoPlay': False,
        'mockStoreNoNavigation': True, 'publicUpload': False, 'allVideoHashesMatched': True,
        'nativeRawVersionPerEpisode': True, 'allRawSourcesDecodedAndHashed': True,
        'browserVerification': 'Pending root native-browser verification'}
    write(package / 'REVIEW_BUILD_CHECKS.json', proof)
    return proof


if __name__ == '__main__':
    result = build()
    print(json.dumps(result, ensure_ascii=False, indent=2), flush=True)
    raise SystemExit(0 if result['passed'] else 2)
