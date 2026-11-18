# Shapes kısa sürümleri

18 Kasım 2026. Penguen, Roket, Kaplumbağa, Ördek, Flamingo ve Ayçiçeği için Ahtapot kısa videosunun akışından altı ayrı 16 saniyelik MP4 hazırlandı. Her bölüm kendi Unity kaydını, arayüzünü ve üç yıldızlı sonucunu gösteriyor.

| Bölüm | Şekil | Uzun sürüm | Kısa sürüm |
| --- | --- | ---: | ---: |
| 2 | Penguen | 40,0 sn | 16 sn |
| 5 | Roket | 40,6 sn | 16 sn |
| 12 | Kaplumbağa | 41,7 sn | 16 sn |
| 16 | Ördek | 41,4 sn | 16 sn |
| 29 | Flamingo | 40,6 sn | 16 sn |
| 39 | Ayçiçeği | 41,4 sn | 16 sn |

İlk ok 3,8. saniyede normal hızda çıkıyor. 5–7,6. saniyeler arasında gerçek çözüm hızlanıyor ve hız ekranda belirtiliyor. Ardından son ok normal hızda çıkıyor, üç yıldız görünüyor ve 11,6. saniyede ortak Play Now kapanışı başlıyor. Roket ve Ayçiçeği’nde yazılar oyun alanının üstündeki dar boşluğa sığması için tek satır.

[Kısa ve uzun kurguları karşılaştır](http://127.0.0.1:8200/marketing/05_Reels/ShapesShorts_20261118/index.html). Penguen’in uzun karşılaştırması projedeki son ses düzeltmeli sürümü kullanıyor. Gönderilen Penguen dosyası aynı görüntüye sahip ve o ses düzeltmesinden önceki sürüm.

- [x] İki Ahtapot referansını karşılaştır ve altı gerçek Unity kaydını eşleştir.
- [x] Altı kısa kurguyu işle. Kapanış sesini ve referansın müziğini kullan. Bölüm adlarını ve ok sayılarını seslendir.
- [x] 2.880 kareyi çöz, görüntü ve ses kodlamasını doğrula. Sonuç ekranlarını ve kısa başlıkları gerçek MP4 karelerinden incele.
- [x] Altı kısa sürümü uygulama tarayıcısında oynat. Uzun sürüme geçişi, 12 akış bağlantısını ve ZIP içeriğini doğrula.
- [x] Altı MP4’ü kaynakların yanındaki `Kısa sürümler` klasörüne ayrı dosyalar olarak kopyala. Gönderilen sekiz kaynağın ve kopyaların SHA-256 değerlerini doğrula.

`manifest.json` bölüm başına kurgu zamanlarını ve ses doğrulamasını, `QA_REPORT.json` kaynak ve ZIP özetini kaydeder. Yeniden üretmek için proje kökünde `py -3.13 ArrowSwarm_Marketing/Tools/render_shapes_shorts.py` çalıştır. Sonraki inceleme noktası altı videonun kısa ve uzun sürüm karşılaştırmasıdır.
