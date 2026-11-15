"""Create the R2 review only from passed final/native/raw evidence.

One player switches among the new cut, unchanged previous17s cut and continuous
silent source. Waiting for incomplete proofs never writes a pretend final page.
"""
from __future__ import annotations

import csv
import hashlib
import json
import shutil
from pathlib import Path

from build_defense_pilot_review import PAGE as PREVIOUS_PAGE

MEDIA = Path(__file__).resolve().parents[1]
OUTPUT = MEDIA / "05_Reels/DefensePilot02_20261115"
PREVIOUS = MEDIA / "05_Reels/DefensePilot01_20261115"
FINAL = "ArrowSwarm_Defense_R2_Sirayla_TR.mp4"
PREVIOUS_HASH = "f38f6e881372f7444062d02e232825d002f464a1af3549850e1c638db1da15de"
STYLE = PREVIOUS_PAGE.split("<style>",1)[1].split("</style>",1)[0]

PAGE = r'''<!doctype html>
<html lang="tr"><head><meta charset="utf-8"><meta name="viewport" content="width=device-width,initial-scale=1">
<meta name="color-scheme" content="dark"><title>Arrow Swarm · Defense R2</title><style>__STYLE__
.versions{display:grid;grid-template-columns:repeat(3,minmax(0,1fr));gap:7px}.versions button{min-width:0;font-size:12px;padding:10px 7px;min-height:61px}
.fact{font-size:14px;border-top:1px solid var(--edge);padding-top:12px}.comparison{color:var(--gold)}
</style></head><body><main>
<header class="intro"><div class="eyebrow">ARROW SWARM / DEFENSE · REVİZE 2</div><h1>Düşmanlar ilerliyor.<br>Ama panik yok.</h1>
<p>Daha erken dondur. Okları sırayla gönder. Son gökkuşağı okunu ayrı bir anda izle; gerçek bölüm sonucuna ulaş.</p>
<div class="tags"><span class="tag gold">__DURATION__s</span><span class="tag">__ARROWS__ ok</span><span class="tag">__LIVES__ can</span><span class="tag">1080 × 1920 · 30 fps</span><span class="tag">Gerçek Unity görüntüsü</span></div></header>
<div class="layout"><section class="player-column" aria-label="Defense revizyon videosu oynatıcısı">
<div class="versions" role="group" aria-label="Yeni önceki ve ham video seçimi"><button id="final" type="button" aria-pressed="true">Yeni R2 · __DURATION__s</button><button id="previous" type="button" aria-pressed="false">Önceki · 17s</button><button id="raw" type="button" aria-pressed="false">Kesintisiz ham · __RAW_DURATION__s<br>(sessiz)</button></div>
<div class="video-wrap"><video id="reel" playsinline preload="metadata" poster="cover.jpg" aria-label="Yeni Defense R2 videosu"></video></div>
<div class="controls"><button class="primary" id="play" type="button">▶ Sesli oynat</button><button id="pause" type="button">Duraklat</button><button id="mute" type="button" aria-pressed="false">Sesi kapat</button></div>
<div class="transport"><input type="range" id="seek" min="0" max="__DURATION__" step="0.1" value="0" aria-label="Videoda zaman seç"><output for="seek" id="clock" class="clock">0,0 / __DURATION__ s</output></div>
<p id="player-status" role="status" aria-live="polite" class="status"></p></section>
<div><section class="card"><div class="eyebrow" id="version-label">YENİ R2 KURGU</div><h2 id="version-title">Dondur. Sırayla gönder. Temizle.</h2>
<p id="version-description">Yeni kayıttan hazırlanan, ayrı gökkuşağı anı olan revize kurgu.</p>
<div class="chapters" id="chapters" role="group" aria-label="Video bölümleri"></div>
<div class="links"><a class="button" href="__FINAL__" download>↓ Yeni R2 MP4 indir</a><a class="button" href="previous.mp4" download>Önceki MP4 · 17s</a><a class="button" href="raw_capture.mp4" download>Ham MP4 · sessiz</a><a class="button" href="captions_TR.srt" download>Altyazı · SRT</a><a class="button" href="encoded_contact_sheet.jpg">Kurgu kareleri</a><a class="button" href="QA_REPORT.json">Teknik kontrol</a><a class="button" href="native_capture_checks.json">Oyun kanıtı</a></div></section>
<section class="card"><h2>Bu revizyonda ne değişti?</h2><ol class="steps">
<li>Düşmanlar <strong>yolun yarısındayken</strong> başlayan giriş; açılışta 20 yerine 13 düşman.</li>
<li>Bölüm akışında önceki kayıttan <strong>__EARLIER__s önce</strong> kullanılan gerçek Dondur.</li>
<li>Aynı anda bir seri yerine <strong>sırayla __ARROWS__ yasal atış</strong>.</li>
<li>Son ok için ayrı hazırlık ve belirgin native gökkuşağı uçuşu.</li>
<li><strong>__KILLS__ gerçek mermi ölümü · 0 zafer temizliği ölümü</strong>; ardından gerçek bölüm sonucu.</li></ol>
<p class="small fact">Top ve Bottom HUD ile oyun sahnesi korunur. Düşman canı/hızı/hasarı veya sonuç değiştirilmez. Editor kontrollü yasal tıklamalar kullanılır; insan parmağı kaydı iddiası yoktur.</p>
<p class="small comparison">Önceki 17 saniyelik video karşılaştırma için aynen korunur. Eski kayıtta kalan bir düşmanı normal zafer temizliği kaldırıyordu; yeni kaynakta bu sayı sıfırdır.</p></section>
<section class="card"><h2>Sıra sende.</h2><button class="button store" id="store" type="button">▶ PLAY NOW</button>
<p class="small" style="margin-top:14px">MOCK · Mağaza henüz yayınlanmadı. Videodaki basma hareketi tanıtım animasyonudur.</p>
<p role="status" id="store-status" class="status" hidden>Mock düğme çalıştı. Yayınlanmış mağaza adresi bulunmadığı için dış bağlantı açılmadı.</p></section>
<p class="small notice">Kesintisiz ham kayıt __RAW_DURATION__s ve sessizdir; kare yakalama Unity sesini kaydetmez. Konuşma, müzik, ses vurguları ve yazılar kurguya eklenmiştir. Oyun arayüzü gerçek kayıttan gelir.</p>
</div></div><footer>Farzetki Games · Yerel inceleme · Otomatik oynatma veya dışarıya yayın yok.</footer></main>
<script>
'use strict';
const configs=__CONFIGS__;
const v=document.getElementById('reel'),seek=document.getElementById('seek'),clock=document.getElementById('clock'),status=document.getElementById('player-status'),mute=document.getElementById('mute'),play=document.getElementById('play');let selected='final';const fmt=n=>Number(n).toFixed(1).replace('.',',');
function duration(){return v.readyState>0&&Number.isFinite(v.duration)&&v.duration>0?v.duration:configs[selected].duration}
function update(){const current=Number.isFinite(v.currentTime)?v.currentTime:0;clock.textContent=fmt(current)+' / '+fmt(duration())+' s';seek.max=duration();seek.value=current;seek.setAttribute('aria-valuetext',fmt(current)+' saniye / '+fmt(duration())+' saniye')}
function syncMute(){const silent=selected==='raw';mute.disabled=silent;mute.textContent=silent?'Ham kayıt sessiz':v.muted?'Sesi aç':'Sesi kapat';mute.setAttribute('aria-pressed',String(silent||v.muted));play.textContent=silent?'▶ Sessiz oynat':'▶ Sesli oynat'}
function jump(time){v.pause();const target=Math.max(0,Math.min(time,duration()-.05));if(v.readyState>0){v.currentTime=target;update()}else{v.addEventListener('loadedmetadata',()=>{v.currentTime=target;update()},{once:true});v.load()}}
function select(key){selected=key;const c=configs[key];v.pause();status.textContent=key==='raw'?'Ham kayıt sessizdir. Konuşma ve müzik yalnız kurgularda bulunur.':'';v.src=c.file;v.poster=c.poster;v.muted=key==='raw';v.setAttribute('aria-label',c.aria);v.load();for(const name of ['final','previous','raw'])document.getElementById(name).setAttribute('aria-pressed',String(name===key));for(const [id,value] of [['version-label',c.label],['version-title',c.title],['version-description',c.description]])document.getElementById(id).textContent=value;const chapters=document.getElementById('chapters');chapters.replaceChildren();c.chapters.forEach(([text,time])=>{const b=document.createElement('button');b.type='button';b.textContent=text;b.dataset.time=time;b.setAttribute('aria-label',text+' · '+fmt(time)+' saniye');b.onclick=()=>jump(time);chapters.append(b)});seek.value=0;syncMute();update()}
for(const key of ['final','previous','raw'])document.getElementById(key).onclick=()=>select(key);play.onclick=async()=>{try{v.muted=selected==='raw';syncMute();await v.play();status.textContent=selected==='raw'?'Ham kayıt sessiz oynatılıyor.':''}catch(error){status.textContent='Video oynatılamadı. MP4 indir bağlantısını kullanabilirsin.'}};document.getElementById('pause').onclick=()=>v.pause();mute.onclick=()=>{v.muted=!v.muted;syncMute()};seek.addEventListener('input',()=>{v.currentTime=Number(seek.value);update()});for(const event of ['timeupdate','loadedmetadata','durationchange'])v.addEventListener(event,update);v.addEventListener('volumechange',syncMute);v.addEventListener('error',()=>{status.textContent='Video dosyası yüklenemedi. MP4 indir bağlantısını kontrol edebilirsin.'});document.getElementById('store').onclick=()=>{document.getElementById('store-status').hidden=false};select('final');
</script></body></html>'''


def read(path: Path) -> dict:
    """Read existing evidence without inventing a passed marker."""
    return json.loads(path.read_text(encoding="utf-8-sig"))


def sha(path: Path) -> str:
    """Hash complete local media before and after copying."""
    return hashlib.sha256(path.read_bytes()).hexdigest()


def verified_copy(source: Path,target: Path,expected: str) -> None:
    """Atomically copy verified media without touching its original."""
    if sha(source)!=expected:raise RuntimeError(f"Source hash mismatch: {source.name}")
    if target.is_file() and sha(target)==expected:return
    temporary=target.with_suffix(target.suffix+".copying");shutil.copy2(source,temporary)
    if sha(temporary)!=expected:raise RuntimeError(f"Copied hash mismatch: {source.name}")
    temporary.replace(target)


def build() -> dict:
    """Gate final review on final/native/raw proof, then build one player."""
    required=[OUTPUT/name for name in [FINAL,"QA_REPORT.json","native_capture_checks.json","source_provenance.json","cover.jpg","captions_TR.srt"]]
    missing=[p.name for p in required if not p.is_file()]
    if missing:return {"passed":False,"waitingFor":missing,"indexWritten":False}
    final=read(OUTPUT/"QA_REPORT.json");native=read(OUTPUT/"native_capture_checks.json");prov=read(OUTPUT/"source_provenance.json")
    assert final["passed"] and native["passed"] and prov["won"],"Final/native source proof has not passed"
    assert native["victoryCleanupKills"]==0 and native["mobFinishes"]==0 and native["projectileKills"]>0,"All-enemy projectile proof missing"
    assert final["sha256"]==sha(OUTPUT/FINAL),"Final MP4 differs from passed QA"
    duration=float(final["durationSeconds"]);assert duration>0
    source=MEDIA/"04_Raw_Gameplay"/final["source"]
    assert source.resolve().is_relative_to((MEDIA/"04_Raw_Gameplay").resolve()),"Unexpected source location"
    marker=source/"raw_video_QA.json"
    if not marker.is_file():return {"passed":False,"waitingFor":[str(marker)],"indexWritten":False}
    raw=read(marker);assert raw["passed"]
    assert sha(source/"provenance.json")==sha(OUTPUT/"source_provenance.json"),"Source provenance differs"
    assert int(raw["frames"])==int(prov["frames"]) and int(raw["fps"])==int(prov["fps"])
    raw_source=source/raw["file"];assert raw_source.stat().st_size==raw["bytes"] and sha(raw_source)==raw["sha256"]
    with (source/"events.csv").open(encoding="utf-8-sig",newline="") as stream:events=list(csv.DictReader(stream))
    assert not any(e["event"] in ["cleanup_kill","mob_finish"] for e in events),"Source contains uncleared path/cleanup outcome"
    kills=sum(e["event"]=="projectile_kill" for e in events);assert kills==native["projectileKills"]
    rainbow=[e for e in events if e["event"]=="rainbow_fire"];assert len(rainbow)==1
    # Different capture warm-up offsets make raw frame clocks incomparable.
    # The native check measures progress delta / configured natural path speed.
    earlier=float(native["freezeEarlierInNaturalTraversalSeconds"]);assert earlier>0
    assert native["halfwayLeadingProgress"]==.5 and native["halfwayOpeningAliveMobs"]==13 and native["previousOpeningAliveMobs"]==20
    chapters=[["Yaklaşma",0],["Dondur",final["freezeAt"]],["İlk atış",final["firstShotAt"]],["Gökkuşağı",final["rainbowAt"]],["Sonuç",final["naturalWinAt"]],["Tanıtım",final["promoAt"]]]
    assert all(0<=float(t)<duration for _,t in chapters) and all(chapters[i][1]<=chapters[i+1][1] for i in range(len(chapters)-1))
    old_video=PREVIOUS/"ArrowSwarm_Defense_PanikYok_TR.mp4";verified_copy(old_video,OUTPUT/"previous.mp4",PREVIOUS_HASH)
    verified_copy(PREVIOUS/"cover.jpg",OUTPUT/"previous_cover.jpg",sha(PREVIOUS/"cover.jpg"))
    verified_copy(raw_source,OUTPUT/"raw_capture.mp4",raw["sha256"])
    raw_duration=float(raw["durationSeconds"]);fps=prov["fps"]
    configs={
        "final":{"file":FINAL,"duration":duration,"poster":"cover.jpg","label":f"YENİ R2 KURGU · {duration:g}s","title":"Dondur. Sırayla gönder. Temizle.","description":f"Dondur, doğal bölüm akışında {earlier:.2f}s daha erken. Atışlar 1× normal hızda ve sırayla; son gökkuşağı 0,75× vurgulu. Giriş 2×; hız değişimleri videoda etiketlidir.","aria":f"{duration:g} saniyelik yeni Defense R2 videosu","chapters":chapters},
        "previous":{"file":"previous.mp4","duration":17,"poster":"previous_cover.jpg","label":"ÖNCEKİ 17s KURGU · KARŞILAŞTIRMA","title":"Önceki Panik yok kurgusu.","description":"Önceki çıktı aynen korunur. Daha geç dondurma ve hızlı ardışık atışlar vardı; kalan bir düşman normal zafer temizliğiyle kaldırılıyordu.","aria":"Önceki 17s Defense videosu","chapters":[["Yaklaşma",0],["Dondur",3],["Karşı saldırı",3.7],["Gökkuşağı",5.2],["Sonuç",9.7],["Tanıtım",13.1]]},
        "raw":{"file":"raw_capture.mp4","duration":raw_duration,"poster":"","label":"KESİNTİSİZ HAM KAYIT · SESSİZ","title":"Yeni native kaydın tamamı.","description":f"{prov['frames']} kare sırayla korunur. Gerçek erken Dondur ve {prov['arrows']} yasal atış; {kills} mermi ölümü, sıfır zafer temizliği. Kaynakta ses ve kurgu yazıları yoktur.","aria":f"{raw_duration:g} saniyelik kesintisiz sessiz Defense ham kaydı","chapters":[["Başlangıç",0],["Dondur",prov["freezeFrame"]/fps],["İlk atış",prov["firstFireFrame"]/fps],["Gökkuşağı",int(rainbow[0]["frame"])/fps],["Sonuç",prov["winFrame"]/fps]]}}
    page=PAGE
    for token,value in {"__STYLE__":STYLE,"__DURATION__":f"{duration:g}","__RAW_DURATION__":f"{raw_duration:g}","__ARROWS__":str(prov["arrows"]),"__LIVES__":str(prov["lives"]),"__KILLS__":str(kills),"__EARLIER__":f"{earlier:.2f}".replace(".",","),"__FINAL__":FINAL,"__CONFIGS__":json.dumps(configs,ensure_ascii=False).replace("</","<\\/")}.items():page=page.replace(token,value)
    assert page.count("<video ")==1 and "<video controls" not in page and " autoplay" not in page and 'href="http' not in page
    (OUTPUT/"index.html").write_text(page,encoding="utf-8")
    proof={"passed":True,"gallery":str(OUTPUT/"index.html"),"onePlayer":True,"versions":3,"default":FINAL,"durationSeconds":duration,"rawDurationSeconds":raw_duration,"previousByteIdentical":sha(OUTPUT/"previous.mp4")==PREVIOUS_HASH,"rawCopyByteIdentical":sha(OUTPUT/"raw_capture.mp4")==raw["sha256"],"nativeCleanupKills":0,"mockStoreNoNavigation":True,"chapters":chapters,"browserVerification":"Pending parent native-browser check","serve":"py -3.13 ArrowSwarm_Marketing/Tools/serve_reel_review.py --revision defense-r2 --port 8197"}
    (OUTPUT/"REVIEW_BUILD_CHECKS.json").write_text(json.dumps(proof,ensure_ascii=False,indent=2),encoding="utf-8")
    return proof


if __name__=="__main__":
    result=build();print(json.dumps(result,ensure_ascii=True,indent=2),flush=True)
    raise SystemExit(0 if result["passed"] else 2)
