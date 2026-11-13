# İlk ikon onay paketi — 13 Kasım 2026

Üç yeni konsept gerçekten üretildi, yerel dosyaya kopyalandı ve 48/72/128/512px PNG olarak açılıp doğrulandı. Önceki ChatGPT görselleri yerel dosya bulunamadığından bu pakette yeniden kullanılmış gibi gösterilmedi. Yeni üretimin tam promptları generation_prompts.json içindedir.

## Görsel değerlendirme

Bu puanlar yaratıcı değerlendirmedir; A/B testi, mağaza dönüşüm oranı veya kullanıcı araştırması sonucu değildir. Her aday aynı dört gerçek piksel boyutunda ve dairesel kırpma önizlemesinde incelendi.

| Kriter / 10 | A minimal | B canlı | C premium |
|---|---:|---:|---:|
| 48px ok/yol okunabilirliği | 9 | 8 | 9 |
| Gerçek dik açılı ok mekaniğiyle bağ | 9 | 9 | 9 |
| Mevcut mavi/altın UI ile tutarlılık | 8 | 8 | 9 |
| Renkle dikkat çekme | 7 | 9 | 8 |
| Sadelik / gereksiz detaydan kaçınma | 9 | 8 | 9 |
| Ayırt edilebilir sembol potansiyeli | 7 | 8 | 8 |

- A: en sakin ve düz okunabilir aday. Üç yolun yönleri net; krem zemin Light temasına yakın. Genel yönlendirme simgesine benzemesi risk.
- B: üç renk enerjisi yüksek, yollar birbirine değmiyor. Yoğun mavi ve parlak pembe diğer ikisinden daha agresif; uzun süreli marka tercihi kullanıcıya ait.
- C: iki yol ile güçlü sade siluet; cyan/altın oyunun mevcut HUD renkleriyle en yakın akraba. Karşı karşıya gelen başlar bulmaca gerilimini anlatıyor, fakat resim gerçek çözülebilir seviye iddiası taşımaz.

**İlk öneri C; okunabilirlik kontrol adayı A; yüksek enerji test adayı B.** Nihai seçim yapılmadı. app_icon_512.png adında final dosyası, Android adaptive katmanları ve üretim entegrasyonu seçime kadar oluşturulmadı.

## Teknik QA

- Kaynaklar: üç adet 1254×1254 RGB PNG (yerel imagegen çıktısı).
- Her aday: 512px için32bit RGBA PNG (alpha255);128/72/48 için RGB PNG; toplam12 export; dosyalar Pillow13 ile yeniden açıldı, boyut/biçim ve SHA256 kaydedildi.
- icon_comparison.png: 1760×1040 inceleme panosu; büyük örnekler512px, küçük örnekler etiketindeki gerçek piksel boyutunda.
- icon_X_circle128.png: yalnız önizleme; daire içinde ok başları okunur. Bunlar Android adaptive mask testi veya launcher cihaz testi sayılmaz.
- PNG exportları toplam720.899 bayt; kaynak görseller ve karşılaştırma panosu ayrıca saklandı. Dosyalar Assets altında değil; oyun build maliyetine eklenmedi.
- Oyunun Player Settings/ikonu/sahneleri/oynanışı değiştirilmedi.
- export_icon_sizes.py ile aynı kaynaklardan yeniden üretilebilir. Gereken bağımlılık Pillow13; geçici medya ortamı kullanıldı, Unity paketi eklenmedi.

## Tamamlanmayan işler

- Rakip ikonlarıyla gerçek görsel yan yana test ve hedef kullanıcı tercihi ölçümü.
- Google Play Store Listing Experiment sonucu.
- Kullanıcı seçimi sonrası final/adaptive katman, fiziksel launcher mask testi ve logo/sosyal profil türevleri.

Bu eksikler tamamlanmış gibi işaretlenmez. İlk kreatif paket onayı için üç ayrı, incelenebilir ikon adayı hazırdır.

## Google Play teknik export düzeltmesi

13Kasım2026 resmî Play Console Help gereksinimi:512×512,32bitPNG (alpha), azami1024KB. A/B/C512px adayları RGBA olarak yeniden export edildi. Alpha kanalı tamamen255 (opak); çizim ve RGB renkler değişmedi. Üç aday da boyut/biçim/RGBA/alpha ve1024KB sınırı assert kontrollerini geçti. Diğer önizleme boyutları RGB olarak kalır. icon_files.json SHA256 değerleri yeni dosyalar için güncellendi.

Kaynak: https://support.google.com/googleplay/android-developer/answer/9866151?hl=en . Bu teknik uygunluk ikonun seçildiği, mağazaya yüklendiği veya Android adaptive/launcher testinden geçtiği anlamına gelmez.
