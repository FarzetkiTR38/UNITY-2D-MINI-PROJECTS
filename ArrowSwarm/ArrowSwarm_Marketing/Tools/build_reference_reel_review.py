"""Package the single approval pilot and its exact reference/source trail."""
from pathlib import Path
import hashlib,json,html
from render_reference_reel import OUT,SOURCE,ROOT,VOICE_STARTS

def stamp(t):
    ms=round(t*1000)
    return f'{ms//3600000:02}:{ms//60000%60:02}:{ms//1000%60:02},{ms%1000:03}'
def main():
    qa=json.loads((OUT/'QA_REPORT.json').read_text(encoding='utf-8'))
    assert qa['passed'] and qa['sourceLanguage']=='tr' and qa['decodedFrames']==840
    voices=json.loads((OUT/'audio/voice_manifest.json').read_text(encoding='utf-8'))['clips']
    subs=[]
    for clip,start in zip(voices,VOICE_STARTS):subs.append((start,start+clip['durationSeconds'],clip['text']))
    subs.append((5.5,8.5,'3… 2… 1…'))
    subs.sort()
    assert all(a[1]<=b[0] for a,b in zip(subs,subs[1:]))
    (OUT/'ArrowSwarm_Shapes2_TR.srt').write_text('\n\n'.join(f'{i+1}\n{stamp(s)} --> {stamp(e)}\n{text}' for i,(s,e,text) in enumerate(subs))+'\n',encoding='utf-8')
    (OUT/'STORYBOARD.md').write_text('''# Shapes 2 / Referans pilotu

Tek örnek, 28 saniye. Diğer seviyeler kullanıcı bu kurguyu onayladıktan sonra üretilecek.

| Zaman | Kurgu |
|---|---|
|0–1,2|Bulanık penguen üzerinde kısa, iri soru başlığı. Türkçe anlatım.|
|1,2–3,1|Ayaklardaki üç gerçek oka yumuşak 1,3× yakınlaşma. A/B/C yolları ve rozetler.|
|3,1–5,5|Üç seçenek tam görünür; soru ve seçenekler seslendirilir.|
|5,5–8,5|Seçenek anlatımı bittikten sonra tam üç saniye düşünme süresi.|
|8,5–11,3|Cevap A. 8,833'te üretim kaydındaki ilk atış. Vurgu çizgisi atış başlamadan kaybolur.|
|11,3–13,3|Tam şekle yumuşak geri açılma ve çözümün devamı.|
|13,3–20|Gerçek atışlar yaklaşık 1,54× hızda; hız etiketi görünür.|
|20–23,5|Son okun çıkışı, gerçek Türkçe kazanma paneli ve üç altın yıldız.|
|23,5–28|Mevcut oyun logosu ve yorum sorusu.|

## Doğrulanan soru

A=üretim ok0; sola çıkışı açık. B=ok3; önünde üç hücre sonra ok1 var. C=ok1; bir hücre sonra ok2 var. Soru **bu üç ok** içindir; tahtadaki diğer açık oklar hakkında tek doğru hamle iddiası yoktur. Unity MCP `PuzzleRules.ObstacleDistance` / `CanRemove` ile kontrol edildi.

## Kaynak ve ses

14 Kasım 2026'da Unity MCP ile kaydedilen Shapes 2, 600 gerçek kare, 42 doğru atış, üç can ve kazanma. Oyunun üretim Fire çağrıları solver sırasıyla çalıştırıldı; insan parmağı kaydı değildir. Kaynak sessizdir. Başlangıç bekletmesi, görüntü kırpması, yakınlaşma, hızlandırma ve işaretler editoryaldir. Türkçe sentetik anlatım, bu pilot için yazılmış metinle üretildi; müzik ve kısa işitsel işaretler örneksiz sayısal sentezdir. Instagram'dan görüntü, müzik veya konuşma kopyalanmadı.

Kaynak ayrıntıları `source_provenance.json`, native kontroller ham kayıt klasöründeki `native_capture_checks.json`; her montaj karesinin kaynağı `timeline.json`, ses eşleşmeleri `audio_timeline.json` içinde.

## Paylaşım metni taslağı

Bu üç oktan hangisi önce çıkar: A, B, C? 🐧 Üç saniyede seç, sonra penguenin adım adım çözülüşünü izle. Sen ilk seferde bildin mi?

#ArrowSwarm #OkBulmacası #Bulmaca #IndieGame

Henüz paylaşılmadı. Mağaza çıkışı veya doğrulanmamış indirme bağlantısı vaat edilmiyor. Bu dosyanın teknik testleri yaratıcı onay değildir.
''',encoding='utf-8')
    page='''<!doctype html><html lang="tr"><meta charset="utf-8"><meta name="viewport" content="width=device-width,initial-scale=1"><title>ArrowSwarm · Shapes 2 Reels</title>
<style>*{box-sizing:border-box}body{margin:0;background:#09152b;color:#eef7ff;font:16px/1.55 system-ui,sans-serif}main{max-width:1120px;margin:auto;padding:30px 24px 48px}small{color:#7dcdfc;letter-spacing:.12em;font-weight:700}h1{font-size:clamp(28px,4vw,42px);margin:6px 0}p{color:#b6cbe4;margin:8px 0 20px}.player{display:flex;justify-content:center}.layout{display:grid;grid-template-columns:minmax(260px,420px) 1fr;gap:42px;align-items:start}video{display:block;width:min(100%,43.875vh);height:auto;aspect-ratio:9/16;background:#050b17;border-radius:22px;box-shadow:0 20px 65px #0007}.card{border:1px solid #2a466a;background:#10233e;border-radius:18px;padding:22px;margin:0 0 18px}h2{font-size:20px;margin:0 0 12px}button,a.link{display:inline-block;color:#f6fbff;border:1px solid #3b638e;background:#193452;border-radius:12px;padding:10px 14px;text-decoration:none;font:inherit;cursor:pointer;margin:0 7px 9px 0}button.primary{background:#08a6dc;color:#061d38;font-weight:800}button:hover,a.link:hover{background:#27507c}ol{padding-left:21px;color:#cbdced}li{padding:4px}code{color:#92d6ff}.muted{font-size:13px;color:#8faac9}output{font-variant-numeric:tabular-nums;color:#8fdbff}.links{margin-top:16px}@media(max-width:720px){main{padding:22px 16px}.layout{grid-template-columns:1fr;gap:24px}.player{max-width:390px;width:100%;margin:auto}video{width:min(100%,42.75vh)}.card{padding:18px}h1{font-size:29px}}</style>
<main><small>ARROW SWARM / TEK PİLOT / ONAY BEKLİYOR</small><h1>Önce hangi ok?</h1><p>Şekiller · Bölüm 2 · 28 saniye · Türkçe · 1080 × 1920</p><div class="layout"><div class="player"><video id="reel" controls playsinline preload="metadata" poster="cover.jpg" src="ArrowSwarm_Shapes2_ReferencePilot_TR.mp4"></video></div><div><section class="card"><h2>Referanslardan oyuna</h2><p>Gerçek penguen bulmacası, üç seçenek, düşünme süresi ve tam çözüm. Tek örnek hazırlandı; diğer bölümler senin değerlendirmenden sonra gelecek.</p><button class="primary" id="play">Sesli oynat</button><button id="pause">Duraklat</button><output id="clock">0,0 / 28,0 s</output><div class="links"><button data-time="2.5">Yakın plan</button><button data-time="5.6">Geri sayım</button><button data-time="8.6">Cevap</button><button data-time="15">Çözüm</button><button data-time="22.5">Kazanma</button><button data-time="25">Kapanış</button></div></section><section class="card"><h2>Kurgu</h2><ol><li>A/B/C arasından açık yolu seç.</li><li>Üç saniye düşün; cevabı gerçek atışta gör.</li><li>42 okun temizlenmesini ve üç yıldızlı bitişi izle.</li></ol><p class="muted">Yeni Türkçe Unity kaydı kullanıldı. Yakınlaşma, işaretler, zamanlama ve sesler montaj katmanıdır. Oyun değiştirilmedi.</p><a class="link" href="ArrowSwarm_Shapes2_ReferencePilot_TR.mp4" download>MP4 indir</a><a class="link" href="ArrowSwarm_Shapes2_TR.srt" download>Altyazı</a><a class="link" href="encoded_contact_sheet.jpg">Kurgu kareleri</a></section><section class="card"><h2>Referanslar ve devam sırası</h2><a class="link" href="https://www.instagram.com/p/De9PdAZAkUI/">Referans 1</a><a class="link" href="https://www.instagram.com/p/DfAPMK4DcrW/">Referans 2</a><p class="muted">Bağlantılar ve zaman kodlu referans kareleri kalıcı arşivde. Özgün Instagram MP4'leri indirilemedi.</p><p>Onay sonrası: <code>12 → 16 → 21 → 5 → 29 → 39</code></p></section></div></div></main>
<script>const v=document.getElementById('reel');document.getElementById('play').onclick=()=>{v.muted=false;v.play()};document.getElementById('pause').onclick=()=>v.pause();document.querySelectorAll('[data-time]').forEach(b=>b.onclick=()=>{v.pause();v.currentTime=Number(b.dataset.time)});v.addEventListener('timeupdate',()=>document.getElementById('clock').textContent=v.currentTime.toFixed(1).replace('.',',')+' / 28,0 s');</script></html>'''
    (OUT/'index.html').write_text(page,encoding='utf-8')
    tracked=[p for p in OUT.rglob('*') if p.is_file() and not p.name.startswith('still_') and p.name!='file_manifest.json']
    manifest=[{'file':p.relative_to(OUT).as_posix(),'bytes':p.stat().st_size,'sha256':hashlib.sha256(p.read_bytes()).hexdigest()} for p in sorted(tracked)]
    (OUT/'file_manifest.json').write_text(json.dumps(manifest,indent=2),encoding='utf-8')
    print(json.dumps({'files':len(manifest),'videoBytes':qa['bytes'],'source':SOURCE.name,'approval':'pending'},indent=2))
if __name__=='__main__':main()
