# Ortak tanıtım kapanışı · R3

**Küçük telaffuz düzeltmesi:** Kullanıcı diğer her şeyi beğendi; yalnız “Play Store’dan” okunuşu revize edildi. Görünen metin aynı, sentez girdisi `Oynamak için, piley sıtordan indirin!`. Yeni CTA2,30s;34,5–36,8s aralığında. Her iki MP4'ün video akışı yeniden kodlanmadan kopyalandı ve SHA256 birebir aynı kaldı. Tam1200kare decode/ses metrikleri ve tarayıcıda güncel sesle sona kadar oynatım geçti. Önceki ses/video dosyaları değiştirme öncesinde TEMP yedeğine alındı; Git geçmişinde de korunuyor.

Kullanıcı yalnız son bölümü değiştirdi: uzun tebrik cümlesi bittikten sonra eski logolu kompozisyonla tanıtım/Play Now/indirme çağrısı. Mağaza henüz yok; kullanıcı açıkça **mock bağlantı** istedi. Uydurulmuş uygulama adresi kullanılmayacak.

- [x] R2 ve referans görsel okundu; 33,52 s'de tebrik tamamlanıyor.
- [x] Önceki sürümleri koru; oyun dosyalarına dokunma. Yeni çıktı `ReferencePilot02_R3_20261114/`.
- [x] Mevcut oyun logosu ve Unity blank primary plate/play icon atlasıyla6,3s yeniden kullanılabilir kapanış üretildi. Yeni oyun asset'i yok; tanıtım başlığı,Play Now,buton basma/ripple animasyonu var.
- [x] İlk33,7s kurgu korundu;33,7–34,1s fade,toplam40s. Tebrik33,52s'de tamamlanıyor. Eski son soru sesi çıkarıldı; yeni indirme anlatımı34,5–36,54s,basma36,85s.
- [x] Mock bağlantı `SharedOutro01/store_config.json` içinde; store_url=null. Önizlemedeki tıklama gerçek bir mock-durum mesajı gösterir; mağazaya veya uydurma uygulama adresine yönlendirme yok.
- [x] MP4 tam decode1200kare,1080×1920/30fps,3.551.380bayt;-17,98LUFS/-2,02dBTP. Bağımsız gerçek encoded35,4/37s kareleri temiz. İlk1011kare R2 karşılaştırması PSNR52,526dB ortalama: yalnız küçük yeniden kodlama farkı.
- [x] Tarayıcıda30,5–40s oynatım,sonuna erişme,mockbuton,390×844telefon/taşma ve sona atlama kontrolleri geçti. Ekran kanıtı kaydedildi.
- [x] Ortak kapanış,finalvideo ve devam notları ayrı aşama commitlerine alındı; teslim hazır.
- [ ] Kullanıcı yeni tanıtım kapanışını değerlendirdi/onayladı. Henüz başka seviye üretilmedi.

Sonraki aksiyon: `ReferencePilot02_R3_20261114/ArrowSwarm_Shapes2_R3_Promo_TR.mp4` / `http://127.0.0.1:8193/index.html` için kullanıcı değerlendirmesini bekle. Sunucu yeniden gerekirse `py ArrowSwarm_Marketing/Tools/serve_reel_review.py --revision r3 --port 8193`. Ortak `SharedOutro01/ArrowSwarm_SharedOutro01_TR.mp4` sonraki onaylı videolara eklenir. Oynanışta HUD koruma/tek A sonrası bekleme kuralları devam eder; promosyon ekranı, kullanıcının bu isteğiyle bitişe eklenen istisnadır. Gerçek PlayStore yayını açıldığında ortak config'te store_url/status güncellenir. Sonraki seviye sırası12/16/21/5/29/39; mevcut onay sınırını aşma.
