# ColorShift kısa video serisi

18 Kasım 2026. Bölüm 3 pilotuna beş Türkçe kısa video eklendi. Yeni filmler 19 saniye, 1080×1920 ve 30 fps. Her biri oyunun kendi arayüzünü ve üç yıldızlı sonucunu gösteriyor. Dönüşler ve son atış normal hızda. Hızlanan atışlarda hız oranı ekranda. Ortak Play Now kapanışı 14,6. saniyede başlıyor.

| Bölüm | Ok | Dönüş | Video konusu |
| --- | ---: | ---: | --- |
| 1 | 11 | 1 | Son okun çıkış rengini eşleştir |
| 4 | 16 | 1 | Açık yolları önce kullan, sonra kalan beş oku çıkar |
| 5 | 14 | 1 | Tek dönüşle son iki oku çıkar |
| 12 | 27 | 3 | Üç dönüşte yeni çıkışları sırayla aç |
| 19 | 38 | 4 | Dört dönüşü doğru anlarda kullan |

- [x] Bölüm 1, 4, 12 ve 19’un gerçek Türkçe Unity görüntülerini al. Bölüm 5’in doğrulanmış kaydını kullan. 12 ve 19’un ilk kayıtları sonuçtan önce bittiği için daha uzun denemeler alındı. `capture_batch.csv` kaynakları gösteriyor.
- [x] Unity’yi önceki BootScene düzenleme durumuna döndür. Dört kayıt tercihi ve bekleyen eşitleme ayarı geri yüklendi. Konsol hata vermedi.
- [x] Beş videoyu işle. 2.850 kareyi çöz. Altı önizlemeyi uygulama tarayıcısında oynat. Video akışlarını ve beş MP4 içeren ZIP’i doğrula.
- [x] Beş yeni MP4’ü Masaüstü `reels videoları` klasörüne kopyala. Kopyaların ve pilotun SHA-256 değerlerini doğrula.

[Seriyi izle](http://127.0.0.1:8200/#%2Fmarketing%2F05_Reels%2FColorShiftSeries_20261118%2Findex.html). `manifest.json` bölüm başına kurgu zamanlarını, ok ve dönüş sayısını ve ses ölçümlerini içeriyor. `QA_REPORT.json` toplu kontrolü gösteriyor.

Yerel kaynak kareleriyle yeniden üretmek için proje kökünde `py -3.13 ArrowSwarm_Marketing/Tools/render_colorshift_series.py` çalıştır. Tekrar kayıtta mevcut `MarketingCapture` aracı Bölüm 1, 4, 12 ve 19 için sırasıyla 14, 17, 27 ve 36 saniyelik çekimle kullanıldı. Sonraki inceleme noktası altı videonun galerisidir.
