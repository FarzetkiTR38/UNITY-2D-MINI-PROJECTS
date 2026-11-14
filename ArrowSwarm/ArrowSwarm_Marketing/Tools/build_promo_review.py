"""R3 review and a real operable mock CTA, configured by the shared outro."""
import json,hashlib,html,shutil
from render_promo_outro import OUT,SHARED,PREVIOUS,CONFIG,NAME,MEDIA

def main():
    qa=json.loads((OUT/'QA_REPORT.json').read_text(encoding='utf-8'));assert qa['passed']
    page=(PREVIOUS/'index.html').read_text(encoding='utf-8')
    for a,b in [('Reels R2','Reels R3'),('PİLOT R2','PİLOT R3'),('37 saniye','40 saniye'),('37,0','40,0'),('ArrowSwarm_Shapes2_R2_FullScene_TR.mp4',NAME),('ArrowSwarm_Shapes2_R2_TR.srt','ArrowSwarm_Shapes2_R3_TR.srt'),('<button data-time="35.5">Son soru</button>','<button data-time="36.5">Tanıtım kapanışı</button>')]:page=page.replace(a,b)
    page=page.replace('İki HUD da tam oyun sahnesinde duruyor. Önce yalnız A çıkar; kısa bekleme ve anlatımın ardından kalan oklar gönderilir. Açılış ve tebrik cümleleri güncellendi.','Oynanış ve tebrik cümlesi korundu. 33,7 saniyeden sonra ortak tanıtım kapanışı, Play Now basma animasyonu ve indirme çağrısı geliyor.')
    page=page.replace('Yeni Türkçe Unity kaydı kullanıldı. Üst ve alt HUD korunur; görüntü kırpılmadı veya HUD yeniden çizilmedi. İşaretler, anlatım ve ses montaj katmanıdır.','Oynanış bölümü R2 ile aynı. Tebrik cümlesi bittikten sonra tanıtım ekranına geçilir. Mağaza henüz yayınlanmadığı için bağlantı mock.')
    url=CONFIG.get('store_url');mock=not url
    href=CONFIG['preview_mock_href'] if mock else url
    card='<section class="card"><h2>Oyunu oyna</h2><a class="link" style="background:#08a6dc;color:#061d38;font-weight:800" id="store-cta" href="'+html.escape(href,quote=True)+'">▶ PLAY NOW</a><p class="muted">'+('MOCK · Mağaza bağlantısı yayınlanınca eklenecek.' if mock else 'Google Play mağaza sayfası')+'</p><p id="store-status" role="status" hidden>Mock bağlantı test edildi. Mağaza henüz yayında değil; gerçek adres ortak config üzerinden eklenecek.</p></section>'
    page=page.replace('<h2>Referanslar ve devam sırası</h2>','<h2>Referanslar ve devam sırası</h2>')
    page=page.replace('<section class="card"><h2>Referanslar ve devam sırası',card+'<section class="card"><h2>Referanslar ve devam sırası')
    if mock:page=page.replace('</script>',"document.getElementById('store-cta').onclick=e=>{e.preventDefault();document.getElementById('store-status').hidden=false};</script>")
    (OUT/'index.html').write_text(page,encoding='utf-8')
    srt=(PREVIOUS/'ArrowSwarm_Shapes2_R2_TR.srt').read_text(encoding='utf-8')
    blocks=[b for b in srt.strip().split('\n\n') if 'Sen ilk seferde' not in b]
    blocks.append(f'{len(blocks)+1}\n00:00:34,500 --> 00:00:36,540\nOynamak için Play Store’dan indirin!')
    (OUT/'ArrowSwarm_Shapes2_R3_TR.srt').write_text('\n\n'.join(blocks)+'\n',encoding='utf-8')
    (SHARED/'README.md').write_text('''# Ortak tanıtım kapanışı01

Kullanıcının isteğiyle sonraki onaylı videoların sonunda kullanılacak6,3s ortak kapanış. Oyun logosu ve blank primary plate/play icon mevcut Unity atlasından tekrar kullanılır. Arka plan, yazı, hareket/ripple/pointer kod ile düzenlenebilir. Yeni oyun asset'i/import yoktur.

`store_config.json`: gerçek mağaza adresi için tek kaynak. Şu an `status=mock`, `store_url=null`; kullanıcı oyunun henüz yayınlanmadığını doğruladı. Uydurma packageID veya uygulama sayfası yoktur. Görseldeki play.google.com yalnız mağazanın genel alan adıdır; önizlemedeki PlayNow mock durum mesajı gösterir. Video içindeki basma animasyonu bir kurgu öğesidir; MP4 içine tıklanabilir mağaza düğmesi eklenmiş değildir.

Mağaza açılınca `store_url` ve `status` güncellenir, inceleme sayfası yeniden üretilir. Yayın gönderisinin gerçek linki de ayrıca eklenmelidir.

`ArrowSwarm_SharedOutro01_TR.mp4` bağımsız kapanış,`poster.jpg` still,`press.jpg` basma karesi. Render: `py ArrowSwarm_Marketing/Tools/render_promo_outro.py`. R3 tebrik33,52s'de biter;33,7s sonrası0,4s fade ile kapanışa geçer. Önceki ilk33,7s kurgu değişmez; final40s. Yeni CTA anlatımı34,5–36,54s, basma36,85s.
''',encoding='utf-8')
    (OUT/'DELIVERY.md').write_text('''# R3 · yalnız tanıtım kapanışı

R2'nin oynanışı/sorusu/A sonrası beklemesi/HUD'ları ve tam tebrik cümlesi korundu. Yalnız33,7s sonrası ortak tanıtımla değiştirildi. Önceki son soru sesi çıkarıldı; yeni indirme anlatımı, mevcut oyun logosu/native buton görseli, cursor press/ripple ve kısa özgün ses eklendi.

Kullanıcının açık yönlendirmesiyle mağaza **mock**. Gerçek URL yok. Ortak config: `../SharedOutro01/store_config.json`. Özgün oyun/sahne dosyaları değiştirilmedi. Önceki R2/ilk pilot dosyaları korunuyor. Diğer seviyeler henüz üretilmedi; bu yeni kapanış sonraki onaylı videolarda tekrar kullanılacak.
''',encoding='utf-8')
    for directory in [OUT,SHARED]:
        items=[{'file':p.relative_to(directory).as_posix(),'bytes':p.stat().st_size,'sha256':hashlib.sha256(p.read_bytes()).hexdigest()} for p in sorted(directory.rglob('*')) if p.is_file() and p.name!='file_manifest.json']
        (directory/'file_manifest.json').write_text(json.dumps(items,indent=2),encoding='utf-8')
    print(json.dumps({'duration':40,'store':'mock','video':str(OUT/NAME),'bytes':qa['bytes']}))
if __name__=='__main__':main()
