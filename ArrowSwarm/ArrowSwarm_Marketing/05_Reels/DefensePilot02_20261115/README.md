# Defense R2 · Düşmanlar ilerliyor, ama panik yok

15 Kasım2026. Tek pilot revizesi; yeni Defense serisi veya ColorShift için kreatif onay beklenir. Çalışma dalı **ArrowSwarm**.

**Final:** `ArrowSwarm_Defense_R2_Sirayla_TR.mp4` ·21s ·1080×1920 ·30fps ·7,219,938bytes. Önizleme `http://127.0.0.1:8197/index.html`; aynı oynatıcıda yeni21s, önceki17s ve kesintisiz sessiz38s kaynak seçilir. Önceki MP4 byte-exact korunur.

| Revize | Doğrulanmış sonuç |
| --- | --- |
|Daha sakin başlangıç|Öncü yolun tam yarısındayken açılır; açılışta20yerine13düşman. En yüksek eşzamanlı sayı23yerine20.|
|Erken Dondur|Gerçek şarj1→0; öncü.90132yerine.77066. Doğal yol süresinde3.2665s erken; tam2s iddiası yok.|
|Sıralı oklar|15yasal atış, normal oklar10kare/.333s arayla ve videoda1× hızda.|
|Ayrı gökkuşağı|Son normal atıştan32kare sonra. Normal uçuşların tamamı son atıştan önce biter; tek kalan ok önce işaretlenir, gerçek gökkuşağı uçuşu0,75× etiketiyle gösterilir.|
|Tam temizlik|24spawn=24benzersizmermiölümü;0cleanuphasarı/ölümü,0yolu bitiren düşman. Sonuç3can/3yıldız.|
|Yeni anlatım|“Düşmanlar ilerliyor. Ama panik yok.” → “Önce dondur.” → “Şimdi okları sırayla gönder.” → “Son ok… Gökkuşağı!” → “Hepsi temizlendi!”|

## Kurgu akışı

- 0–3.4s: gerçek yarım-yol yaklaşması;2× etiketiyle kısa giriş.
- 3.4s: gerçek Dondur.
- 4.1s: sıralı normal atışların başlangıcı, hız1×.
- 8.43s: tek kalan okun doğal gökkuşağına yükselmesi;8.53s “Son ok…” anlatımı.
- 8.93s: normal uçuşların hepsi tamamlanır.
- 9.5s: “Gökkuşağı!” kelimesiyle gerçek son atış;0,75× uçuş.
- 12.522s: oyunun kendi Win sonucu; üçüncü altın yıldız tanıtımdan önce tamamlanır.
- 15.733–21s: mevcut ortak tanıtım kartı. DüzeltilmişP CTA16.533–18.833s; buton basışı18.883s. Mağaza hâlâmock, gerçek yayın adresi yok.

Tam sahne ve Top/Bottom HUD korunur. Oynanışta kırpma/zoom/yeni HUD yok; HUD bantları encoding öncesinde kaynakla birebir karşılaştırılır. Konuşma, proje müzik/SFX'i ve yazılar kurguya eklenir; ham kaynak kare yakalaması ses içermez. Editor tıklamaları gerçek açık-yol ve şarj kurallarını kullanır; insan parmağı kaydı iddiası yoktur.

Native procedural seviyenin tekrarlanabilmesi için yalnız izole Editor kaydında seed20261115 kullanılır. Üretim GameConfig/HP/hız/hasar/spawn/sonuç kuralları değiştirilmez; özel veriler ve random durum geri yüklenir. İlk başarısız kısmi denemeler final yerine sunulmaz; kısa sonuçları `TRIAL_OUTCOMES.json` içindedir.

## Kontroller

`native_capture_checks.json`:1140nativekare/38s,15atış için occupancy replay, şarj, hareketin durması,24benzersizmermiölümü ve0cleanup doğrulanır. `QA_REPORT.json`:630encodedkare/21s, H264/AAC,48kHz stereo,−17.24LUFS/−4.09dBTP. `ENCODED_REVIEW.md`: bağımsız17encodedkare, ayrıgökkuşağı, gerçek3yıldız ve CTA dalga eşleşmesi incelemesi.

`BROWSER_CHECKS.json`:1280×900 ve390×844 taşmasız görünüm;21s ses açık oynatmanın sona ulaşması,17sönceki/38sham seçimleri, seek/pause/mute/mockCTA ve0consolehatası. Geçici viewport ayarı kaldırılır, önizleme teslim sekmesi olarak kalır. Teknik kontroller subjektif dinleme veya kullanıcı kreatif onayı iddiası değildir.

`audio/voice_QA.json` sentez/hazırlık aşamasını; `audio/FINAL_ALIGNMENT_CHECKS.json` gerçek final olaylarıyla son zamanlama ve temiz-sonuç eşleşmesini kaydeder. Yeni “Son ok… Gökkuşağı!” klibinde yalnız0.75–1.45s sessizliği çıkarılır; konuşulan örnekler değişmez. Ortak düzeltilmişP CTA byte-exact korunur.

`NATIVE_SESSION_CHECKS.json`:5özel tercih, hizmetler, debug/random/timing ve13öncedenaçıkdosyanın hashleri geri yüklenmiş/doğrulanmıştır. Unsaved Boot sahnesi kaydedilmeden/atılmadan korunur; geçici boş additive sahne kapatılır. Son durumEditBoot, recorderkapalı,FPS0,0hata. Önceki10medya hashleri aynıdır (`PRESERVATION_CHECKS.json`).

## Yeniden üretim / devam

Repo kökünden, mevcutPython3.13medya ortamıyla:

```powershell
py -3.13 ArrowSwarm_Marketing/Tools/check_defense_r2_capture.py --source ArrowSwarm_Marketing/04_Raw_Gameplay/DefenseR2_L10_20261115_002652_928 --output ArrowSwarm_Marketing/05_Reels/DefensePilot02_20261115
py -3.13 ArrowSwarm_Marketing/Tools/render_defense_r2.py --source ArrowSwarm_Marketing/04_Raw_Gameplay/DefenseR2_L10_20261115_002652_928
py -3.13 ArrowSwarm_Marketing/Tools/build_defense_r2_review.py
py -3.13 ArrowSwarm_Marketing/Tools/serve_reel_review.py --revision defense-r2 --port 8197
```

SourceJPEG dizisi yereldir; manifest/proof/tamMP4 Git'te kalır. Yeniden render için yerelJPEG'ler gerekir. Tam kontrol/devam listesi: `../DEFENSE_PILOT_01.md` üstteki R2 aşamaları. **Sonraki adım kullanıcının bu tek revizeyi incelemesidir; onay gelmeden başka Defense/ColorShift videosuna geçme.**
