# Kalan beş video · 17 Kasım 2026

1. **Mod karşılaştırması — 20s, 1920×1080:** gerçek Color Shift dönüşü → Defense atışları → Shapes temizliği → seçilen marka görseli.
2. **Başlangıç rehberi — 90s, 1920×1080:** FarzetkiGames splash, açık yol, engellenen okun çarpıp geri dönmesi, canlar/Tekrar Dene, Shapes, renk eşleştirme/dönüş, Defense, İpucu, Dondur ve gerçek kazanma sonucu. Türkçe AhmetNeural seslendirme, bölüm başlıkları ve SRT. Kaynaklar arasında yeni örneklere geçilir; Retry düğmesine basıldığı sahnelenmez. Rewarded reklam/ödül kazanımı gösterilmez veya vaat edilmez.
3. **Savunma — 18s, 1080×1920:** gerçek bölüm3, normal hız, sıralı atışlar ve doğal üç yıldızlı sonuç; Freeze kullanılmaz.
4. **Şekiller — 18s, 1080×1920:** gerçek42oklu penguen bölümü; ekranda belirtilen1,33× oynanış ve gerçek sonuç.
5. **Renk Değişimi — 18s, 1080×1920:** gerçek12oklu bölüm3, renk dönüşü ve gerçek sonuç; ekranda belirtilen0,8× oynanış.

Her kaydın Top/Bottom HUD’u korunur. Başlıklar sonuç popup’larının üzerine gelmeden gizlenir. Tutorial ve mod karşılaştırmasında bütün dikey oyun görüntüsü yatay çerçevenin ortasında kalır. Kaynak Shapes/Color Shift HUD’ları İngilizce, Defense HUD’u Türkçedir; dış metinler ve rehberin sesi Türkçedir. Oynanış AI ile üretilmemiştir. Seçilen mor karakterli ikon ve mevcut TR feature graphic yeniden kullanılır.

Ses, oyundan kaydedilmiş AudioListener çıkışı değildir: mevcut proje GameBGM_alt1 müziği, kaynak olaylarına göre yerleştirilen SFX ve rehber için Microsoft speech/edge-tts üzerinden yalnız kamusal özgün anlatım cümlelerinden oluşan editoryal karışımdır. Seslendirme kaynak MP3’leri `Voice/` altında; PCM ara dosyaları yeniden üretilebilir. Bazı cümleler bölüme sığması için en fazla1,2× tempo ile sığdırılır. Teknik kontroller estetik dinleme puanı değildir.

Önceki25sfragman,15sreklam,5DefenseReels,ColorShiftpilotu ve bütün oyun/sahne/oyuncu dosyaları değiştirilmez. Bu oturumda Unity MCP bağlantısı yoktur; doğrulanmış mevcut native kayıtlar ve olay/provenance kayıtları kullanılır. Yeni kayıt/oyun kontrolü yapıldığı iddia edilmez. Mağazaya/sosyal medyaya yükleme ve Git push yapılmaz.

Yeniden üretim sırası:

```powershell
py -3.13 ArrowSwarm_Marketing/Tools/build_remaining_videos.py --only all
py -3.13 ArrowSwarm_Marketing/Tools/master_remaining_audio.py
py -3.13 ArrowSwarm_Marketing/Tools/verify_remaining_videos.py
```

Önceki onaylı kaynak JPEG dizileri ve yerel media bağımlılıkları gerekir. Kaydedilmiş MP3 anlatım kaynakları varsa speech servisine yeniden ihtiyaç yoktur. Beş filmin sesleri yaklaşık **−18 LUFS** seviyesinde eşleştirilir; video paketleri master sırasında bit olarak aynı kalır. Final hash/süre/kare/ses/kaynak doğrulaması `ENCODED_QA.json`, gerçek tarayıcı/indirme kontrolleri `DELIVERY_QA.json` içindedir. Kurgu zaman çizelgeleri `manifest.json` ve video başına manifestlerde yer alır. Sonraki adım kullanıcı kreatif incelemesidir.

Önizleme: http://127.0.0.1:8200/#%2Fmarketing%2F06_Trailers_Ads_Tutorials%2FRemainingVideos_20261117%2Findex.html
