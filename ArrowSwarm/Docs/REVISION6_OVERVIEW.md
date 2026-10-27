# Revision 6 — tamamlandı

27 Ekim 2026. Başlangıç commit'i `c8966057`. İstenen dört iş tamamlandı: örnek sıralama, Defense ölüm efektinin şeffaflık düzeltmesi, Dark Pause butonu ve oyuna uygulanmayan kapsamlı AI tasarım galerisi.

## Tam önce / sonra galerisi

[Galeriyi aç](../Captures/Revision6/index.html). Yerel sunucu açıksa `http://127.0.0.1:8168/index.html` adresi de aynı dosyayı gösterir. Galeri dosyadan çevrimdışı açılır; Before, Concepts ve Checks klasörleri yanında tutulmalıdır.

60 gerçek Unity ekran kaydı, 30 Light ve 30 Dark karşılık; bunlara karşılık gelen 15 incelenmiş AI panosu vardır. Her pano soldan sağa iki ekranın Light / Dark karşılıklarını gösterir. Galeride arama, mod filtresi, toplu açma/kapatma ve tam boy görsel önizlemesi bulunur. Her görselin tam üretim promptu saklıdır.

| Pano | Ekranlar — her biri Light ve Dark |
|---|---|
| 01 | Boot, ana menü |
| 02 | Oyun modu seçimi, ayarlar |
| 03 | Profil, sıralama |
| 04 | Defense ve Shapes bölüm seçimi |
| 05 | Color Shift bölüm seçimi, sahne geçişi |
| 06 | Defense oynanış, duraklatma |
| 07 | Defense kazanma, kaybetme |
| 08 | Defense ipucu, dondurma |
| 09 | Öğretici, öğretici tamamlama |
| 10 | Shapes oynanış, duraklatma |
| 11 | Shapes kazanma, kaybetme |
| 12 | Shapes ipucu, Color Shift oynanış |
| 13 | Color Shift duraklatma, kazanma |
| 14 | Color Shift kaybetme, ipucu |
| 15 | Harita önizleme, duraklatma |

**Hiçbir AI tasarımı oyuna entegre edilmedi.** Tüm PNG/prompt/inceleme kayıtları Assets dışında, `Captures/Revision6/Concepts/` altında. Araç belirli bir DALL·E 3 modeli seçtirmediği için mevcut yerleşik `image_gen` kullanıldı. Küçük yazılar ve çizilen puzzle yolları görsel öneridir; üretim UI'ı, çeviri veya çalışır bölüm verisi değildir. Uygulanacak bir tasarım daha sonra ayrıca istenmelidir.

## Oyunda uygulanan işler

### Örnek sıralama tablosu

BootScene'den yeni Play başlat. Gerçek sıralamayı açmadan **ArrowSwarm → Preview → Leaderboard Samples → Show 10 sample players** seç.

Gerçek mevcut panelde 10 örnek oyuncu görünür. Light/Dark düğmeleriyle temayı, seçenekle yerel oyuncunun örnek 7. sırada veya ilk 10 dışında 27. sırada olmasını inceleyebilirsin. Panelde açıkça ÖRNEK / SAMPLE DATA yazar. Pencere/panel kapatılınca, sahne değişince, Play durunca veya kodlar yeniden yüklenince önizleme temizlenir.

Bu araç yalnızca Editor içindir. Gerçek oyuncu skorları, kalıcı sıralama önbelleği ve kayıtlar değiştirilmez; oyuncu yapıları geçici kopyalardır. Varsayılan oyun gerçek sıralamayı göstermeye devam eder.

[Gerçek Light örneği](../Captures/Revision6/Leaderboard/sample-ten-light.png) · [Dark örneği](../Captures/Revision6/Leaderboard/sample-ten-dark.png)

### Defense ölüm efekti

Ring dokusunun şeffaf piksellerinde kalan mavi RGB, dokuyla uyuşmayan blend ayarından dolayı Dark'ta kare şeklinde görünüyordu. Yalnızca bu efekte ait `Mat_RingGlow` materyalinin blend modu ve kaynak RGB blend çarpanı straight alpha yapıldı. Katmanlar, renkler, doku, parçacık sayısı, süre ve havuz sistemi korunuyor.

İlk keyword çözümü Unity'nin materyal doğrulamasında geri alındı; gerçek oyun görüntüsü bunu yakaladı. Son çözüm yeniden içe aktarma ve gerçek havuzdan oluşturma sonrasında da geçerli. Her iki temada kare iz kayboldu.

[Dark önce](../Captures/Revision6/Death/before/death-dark-180ms.png) · [Dark sonra](../Captures/Revision6/Death/after/death-dark-180ms.png)

### Dark Pause butonu

Yalnızca özel `DarkPause` materyali, Light'taki orijinal UI shader'ı ve beyaz tint ile tanımlandı. Yeniden materyal üreten Editor tarifi de aynı ayarı korur. Ortak shader, sahne/prefab düzeni ve basma efekti değiştirilmedi.

Normal, basılı ve yarı saydam durumların GPU karşılaştırmasında Light'a göre RGB/alpha farkı **0**. Defense, Shapes, Color Shift ve MapTest'te mevcut basma küçülmesi, gerçek duraklatma ve Devam Et kontrol edildi.

[Düzeltilmiş gerçek Dark HUD](../Captures/Revision6/Checks/Pause/GameScene-dark.png)

## Doğrulama ve koruma

- Unity 6000.3.18f1, gerçek Boot akışı ve Unity MCP üzerinden 60 Before kaydı; tüm ekranlarda tam Light materyaline dönüş doğrulandı.
- Örnek tablo: 10 gerçek satır, satır metinlerinin görünürlüğü, 7/27 sıra seçenekleri, Retry, dil/tema geçişleri ve tekrar açma/kapatma geçti. Gerçek pencere kapatma, Stop Play ve gerçek assembly reload temizliği geçti. Beş tercih, oyuncu nesnesi, sağlayıcı, seçili UI ve rastgelelik durumu geri döndü; sıfır auth/fetch/skor/profil çağrısı.
- Ölüm efekti: GPU pozitif/negatif kontrolleri; gerçek Mob.TakeDamage → MobVisuals → ParticleManager akışı; iki temada 180/350 ms kareleri; beş katmanın erken emisyonu; yaklaşık bir saniyede havuza dönüş; aynı örneğin yeniden kullanımı; VFX kapalı kontrolü geçti.
- Pause: dört sahne × iki tema; gerçek geçici Input System mouse olayları, küçülme, pause/resume, timescale ve puzzle duraklatma bayrağı geçti. Orijinal mouse/InputSettings geri yüklendi.
- Galeri: 15/15 incelenmiş pano, 60 benzersiz orijinal, 117 yerel HTTP bağlantısı başarılı. Manifest eşleşiyor; dış script veya fetch gerekmiyor. Gerçek IAB'de filtre, arama/boş sonuç, toplu aç/kapat, modal/zoom/Escape ve 1440×960 / 390×844 düzenleri kontrol edildi; yatay taşma yok. Geçici boyut değişikliği geri alındı.
- Son materyal yeniden içe aktarması geçti. Son native Console kontrolü: **0 error / warning**.
- Başlangıçtaki 11 değiştirilmiş dosya byte düzeyinde aynı hale getirildi. Önceden bulunan recovery ve `.codex` dosyaları korundu; testin değiştirdiği temiz font önbellekleri geri yüklendi.
- Beş orijinal PlayerPrefs kaydı geri yüklendi; Editor BootScene'de, Play kapalı ve arka planda çalışma ayarı başlangıçtaki gibi kapalı bırakıldı. Özel oyuncu JSON'u/restore snapshot yayımlanmadı.
- Commit kapsamı kontrolü: üretimde yalnızca iki materyal değişti; diğer kaynak değişiklikleri Editor altında. Oyun sahneleri, prefablar, kampanyalar, runtime mekanikleri ve fontlar bu görevde değiştirilmedi.

Kanıtlar `Captures/Revision6/Before/`, `Leaderboard/`, `Death/`, `Checks/Pause/` ve `Checks/Gallery/` altında. Son koruma raporları `Checks/final-native-state.json`, `final-file-preservation.json` ve `committed-scope.json` dosyalarında. Bunlar Editor doğrulamalarıdır; canlı UGS, gerçek reklam gösterimi veya Android cihaz sertifikasyonu iddiası içermez.

## Commitler ve devam noktası

| Commit | İş |
|---|---|
| `747260a4` | Uygulama planı ve koruma kapsamı |
| `d1053ab5` | 60 native Before ve ilk tasarım panosu |
| `824f06e1` | Örnek sıralama aracı ve yaşam döngüsü testleri |
| `c7490239` | Gerçek ölüm efekti blend düzeltmesi |
| `b5d15982` | Menü/bölüm seçimi konseptleri |
| `f6816044` | Oynanış/öğretici/popup konseptleri |
| `c33c81df` | Puzzle sonuç/harita konseptleri |
| `f31b84a1` | Dark Pause düzeltmesi ve sekiz native kontrol |
| `a0f2f113` | Tam önce/sonra galerisi ve tarayıcı kanıtları |
| Bu belgeyi ekleyen son commit | Kapanış, kapsam ve kayıt koruma raporları |

Revision 6'da açık aşama kalmadı. Devam istendiğinde önce bu özet ve `REVISION6_PLAN.md` okunmalı; tamamlanmış testler veya konsept üretimi baştan tekrarlanmamalı. AI tasarımlarını uygulamama sınırı geçerlidir.
