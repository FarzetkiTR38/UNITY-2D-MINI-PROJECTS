# Defense · Panik yok

15 Kasım2026. Kullanıcı incelemesi için tek yeni Defense Reels. Sıradaki Defense serisi ve ColorShift için önce bu örneğin kreatif onayı beklenir.

- **Final:** `ArrowSwarm_Defense_PanikYok_TR.mp4` ·17s ·1080×1920 ·30fps ·7.05MB (decimal).
- **Kurgu:** doğal düşman baskısı → doğru anı bekleme → bir gerçek Dondur kullanımı → karşı saldırı → doğal gökkuşağı oku → üç yıldız → ortak tanıtım.
- **Tam sahne:** native Top/Bottom HUD ve gerçek WinPanel korunur. Oynanışta kırpma, zoom veya arayüz yenilemesi yok.
- **Ses:** özgün kısa Türkçe konuşma; projedeki GameBGM_alt1 ve olaylara yerleştirilmiş mevcut SFX. Düzeltilmiş P sesi olan ortak CTA byte-exact yeniden kullanılır.
- **Mağaza:** yayınlanmış adres henüz yok; açıkça mock. Preview Play Now yalnız mock durumunu gösterir, dışarıya yönlendirmez.

| Süre | Gerçek kaynak ve kurgu |
| --- | --- |
|0–3s|Düşmanlar normal hızda ilerler. “Panik yok. Biraz daha…”|
|3s|Native Dondur kontrolü; gerçek şarj1→0.|
|3.7s|İlk yasal atış; “Şimdi sıra sende.”|
|5–9.133s|0,5× etiketli ağır çekim; son gökkuşağı oku5.2s.|
|9.133–12.3s|Gerçek üretimWin; üç yıldız11.867s'de tamamlanır.|
|12.3–17s|Mevcut ortak logo, düzeltilmiş “piley” CTA ve Play Now basma animasyonu.|

38s kesintisiz sessiz kaynak `raw_capture.mp4` aynı oynatıcıda incelenebilir. Sessiz kaynak Unity kare yakalamasıdır; finaldeki konuşma/müzik/SFX kurguya eklenmiştir. Otomasyon gerçek oyun kurallarından geçen Editor tıklamalarını yapar; insan parmağı kaydı iddiası yoktur.

Kaynakta15yasal atış,78cleanup-dışı hasar olayı,22mermi ölümü ve1normal zafer temizliği ölümü vardır. Son gökkuşağı okunun bütün düşmanları öldürdüğü iddia edilmez. Gerçek oyun koşulları ve15ok/3can sonucu korunur; HP/hız/hasar veya sonuç zorlanmaz.

`QA_REPORT.json`:510kare tam decode, H264/AAC,48kHz stereo,−16.59LUFS /−2.82dBTP. `ENCODED_REVIEW.md`: bağımsız14encodedkare incelemesi. `BROWSER_CHECKS.json`:1280×900/390×844, tam17s ses açık oynatma, ham38s/seek/pause/mute/mockCTA, konsolda0hata. Bu kontroller kullanıcının estetik ve ses tercihi onayının yerini almaz.

## Tekrar üretim

Repo kökünden Python3.13 ve önceki medya bağımlılıklarıyla:

```powershell
py -3.13 ArrowSwarm_Marketing/Tools/check_defense_capture.py --source ArrowSwarm_Marketing/04_Raw_Gameplay/Defense_L10_20261114_214156_530
py -3.13 ArrowSwarm_Marketing/Tools/render_defense_pilot.py
py -3.13 ArrowSwarm_Marketing/Tools/build_defense_pilot_review.py
py -3.13 ArrowSwarm_Marketing/Tools/serve_reel_review.py --revision defense --port 8196
```

Native kaynakJPEG'ler yerelde tutulur; kare manifesti, tam kaynakMP4, olay/konum verileri ve sonuç kanıtları Git'tedir. Finali yeniden render etmek JPEGdizisini gerektirir. Yeni bir native kayıt için mevcut Arm/CaptureScope/Recover izolasyonunu ve `MarketingCapture.BeginDefense()` API'sini kullan; canlı oyuncu verisiyle çalıştırma.

**Kesin devam noktası:** kullanıcı bu tek pilotu inceleyecek. Onay veya revize gelmeden başka Defense/ColorShift videosu üretme. Önceki Shapes8medya ve16s kısa kurgu korunur.
