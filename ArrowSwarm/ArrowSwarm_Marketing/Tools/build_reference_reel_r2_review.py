"""Build the full-scene R2 review without overwriting the previous pilot."""
import hashlib,json
from render_reference_reel_r2 import OUT,SOURCE,MEDIA,VOICE_STARTS,SECONDS,VIDEO,REST_START,event_time,FIRST,SETTLED
from build_reference_reel_review import stamp

def main():
    qa=json.loads((OUT/'QA_REPORT.json').read_text(encoding='utf-8'))
    assert qa['passed'] and qa['fullNativeScene'] and qa['durationSeconds']==37
    clips=json.loads((OUT/'audio/voice_manifest.json').read_text(encoding='utf-8'))['clips']
    subs=[(start,start+c['durationSeconds'],c['text']) for c,start in zip(clips,VOICE_STARTS)]
    subs.append((6.2,9.2,'3… 2… 1…'));subs.sort()
    assert all(a[1]<=b[0] for a,b in zip(subs,subs[1:]))
    (OUT/'ArrowSwarm_Shapes2_R2_TR.srt').write_text('\n\n'.join(f'{i+1}\n{stamp(s)} --> {stamp(e)}\n{text}' for i,(s,e,text) in enumerate(subs))+'\n',encoding='utf-8')
    old=MEDIA/'05_Reels/ReferencePilot02_20261114/index.html'
    page=old.read_text(encoding='utf-8')
    replacements={
      'ArrowSwarm · Shapes 2 Reels':'ArrowSwarm · Shapes 2 Reels R2',
      'TEK PİLOT / ONAY BEKLİYOR':'PİLOT R2 / ONAY BEKLİYOR',
      '28 saniye':'37 saniye','28,0':'37,0',
      'ArrowSwarm_Shapes2_ReferencePilot_TR.mp4':VIDEO,
      'ArrowSwarm_Shapes2_TR.srt':'ArrowSwarm_Shapes2_R2_TR.srt',
      'Gerçek penguen bulmacası, üç seçenek, düşünme süresi ve tam çözüm. Tek örnek hazırlandı; diğer bölümler senin değerlendirmenden sonra gelecek.':'İki HUD da tam oyun sahnesinde duruyor. Önce yalnız A çıkar; kısa bekleme ve anlatımın ardından kalan oklar gönderilir. Açılış ve tebrik cümleleri güncellendi.',
      'Yakın plan':'Tam sahne','data-time="5.6"':'data-time="6.5"','data-time="8.6"':'data-time="10.3"',
      '<button data-time="15">Çözüm</button>':'<button data-time="12.5">A sonrası bekleme</button><button data-time="16">Kalan oklar</button>',
      'data-time="22.5"':'data-time="30.5"','<button data-time="25">Kapanış</button>':'<button data-time="35.5">Son soru</button>',
      'Üç saniye düşün; cevabı gerçek atışta gör.':'Üç saniye düşün; yalnız A’nın çıkışını ve 1/42 sayacını gör.',
      '42 okun temizlenmesini ve üç yıldızlı bitişi izle.':'“Şimdi geriye kalan okları yollayalım” bittikten sonra çözüm devam eder.',
      'Yeni Türkçe Unity kaydı kullanıldı. Yakınlaşma, işaretler, zamanlama ve sesler montaj katmanıdır. Oyun değiştirilmedi.':'Yeni Türkçe Unity kaydı kullanıldı. Üst ve alt HUD korunur; görüntü kırpılmadı veya HUD yeniden çizilmedi. İşaretler, anlatım ve ses montaj katmanıdır.'}
    for a,b in replacements.items():
        assert a in page,a
        page=page.replace(a,b)
    page=page.replace('<h2>Kurgu</h2>','<h2>Revize edilen akış</h2>')
    # Native transport chrome obscures BottomHUD on paused frames. Existing
    # explicit Play/Pause/chapter controls keep the review fully operable.
    page=page.replace('<video id="reel" controls','<video id="reel"')
    (OUT/'index.html').write_text(page,encoding='utf-8')
    storyboard=f'''# Shapes 2 · R2 · Tam sahne

Kullanıcının dört revizesi bu sürümdedir. Önceki MP4 ayrı klasörde korunur; diğer seviyelere henüz geçilmedi.

| Zaman | Olay |
|---|---|
|0–2,4|“Bu penguen 42 oktan oluşuyor.” Tam oyun sahnesi ve iki doğal HUD.|
|2,4–6,2|Üç ok sorusu; A/B/C işaretleri.|
|6,2–9,2|Seçenek anlatımından sonra tam üç saniyelik geri sayım.|
|9,3–10,1|“Cevap A!”|
|{event_time(FIRST):.3f}|Yalnız A ateşlenir.|
|{event_time(SETTLED):.3f}|A’nın uçuşu biter. Gerçek sayaç 1/42, diğer oklar sabit.|
|12,8–14,8|“Şimdi geriye kalan okları yollayalım.”|
|{REST_START:.3f}|Cümle bittikten 0,4 saniye sonra kalan 41 ok başlar.|
|15,2–26,53|Gerçek çözüm 1,5×; iki HUD kendi yerinde ve gerçek sayaç çalışıyor.|
|26,53–30|Son uçuşlar normal hızda ve gerçek Türkçe kazanma paneli.|
|28,8–33,52|“Tebrikler! 42 oktan oluşan penguen bölümünü başarıyla geçtin!”|
|34,1–37|Son soru; native sonuç paneli ve HUD’lar ekranda kalır.|

## Kaynak ve kontroller

`{SOURCE.name}`: 840 gerçek Unity karesi, Türkçe, Shapes2, 42 doğru atış/3 can ve kazanma. Kaynak olayları A=f75, bitiş=f97, sonraki ok=f225. Sahnenin ilk ok sonrası durgunluğu gerçekten kaydedildi; uçan oklar görüntüde silinmedi.

Tüm montaj kareleri tam 1080×1920 kaynağa dayanır. TopHUD0–230 ve BottomHUD1690–1920 piksel bantları her montaj karesinde kaynakla birebir doğrulanır. Tüm kareye yakınlaşma, HUD maskesi, başka HUD grafiği veya değiştirilmiş oyun sahnesi yoktur. Kazanma anındaki kararma, oyunun mevcut popup karartmasıdır.

Kaynak sessizdir. Üç yeni Türkçe anlatım klibi üretildi; beş aynı klip yeniden kullanıldı. Özgün sentez müziği ve olay eşlemeli kısa sesler eklendi. Sesin teknik ölçümü, dinlenerek öznel değerlendirme yapıldığı anlamına gelmez.

`QA_REPORT.json`, `audio_timeline.json`, `timeline.json` ve ham kaydın `native_capture_checks.json` dosyası ayrıntıları tutar. Oyun/sahne/prefab değişmedi; yalnız Editor kayıt aracı seçenek kazandı. Oyuncu tercihleri/sağlayıcıları ve başlangıç Edit durumu geri getirildi.

## Paylaşım metni taslağı

Bu penguen 42 oktan oluşuyor 🐧 Peki bu üç oktan hangisi önce çıkar: A, B, C? Önce seçimini yap, sonra çözümü izle. İlk seferde bildin mi?

#ArrowSwarm #OkBulmacası #Bulmaca #IndieGame

Henüz yayımlanmadı. Yeni R2 kullanıcı değerlendirmesini bekliyor.
'''
    (OUT/'STORYBOARD_R2.md').write_text(storyboard,encoding='utf-8')
    files=sorted(p for p in OUT.rglob('*') if p.is_file() and not p.name.startswith(('still_','playback-')) and p.name!='file_manifest.json')
    manifest=[{'file':p.relative_to(OUT).as_posix(),'bytes':p.stat().st_size,'sha256':hashlib.sha256(p.read_bytes()).hexdigest()} for p in files]
    (OUT/'file_manifest.json').write_text(json.dumps(manifest,indent=2),encoding='utf-8')
    print(json.dumps({'revision':'R2','files':len(files),'duration':SECONDS,'bytes':qa['bytes'],'firstFire':event_time(FIRST),'firstSettled':event_time(SETTLED),'otherShots':REST_START,'approval':'pending'},indent=2))
if __name__=='__main__':main()
