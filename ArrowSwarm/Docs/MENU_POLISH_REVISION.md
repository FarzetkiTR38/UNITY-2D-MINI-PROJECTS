# Menü görsel revizyonu

## Kapsam
Üç kullanıcı talebi: koyu temada üst istatistik göstergelerinin açık renkli alt izlerini temizlemek; ana menünün mod düğmesinde seçili moda ait görseli göstermek; sahne geçişini oyunun parlak mavi/altın görsel diline uygun yenilemek.

## Aşamalar
- [x] 1. İstatistik göstergeleri: iç yüzeydeki beyaz/camgöbeği alt yansıma tamamen kaldırıldı; kupa/yıldızın altın renkleri ve parlak noktaları ile mavi dış çerçeve korundu. Unity'de 9 çift tema ekranı ve 50 gerçek GPU kontrolü geçti. `Captures/MenuPolishRevision/hud-after.png` yakın görünümü içerir.
- [x] 2. Mod düğmesi: üç modun açık/koyu tema görselleri Inspector'da bağlandı; mevcut Button, TMP etiketleri, yerelleştirme, tıklama hedefi ve basılma efekti korundu. Unity MCP üzerinden altı görünüm, üretim seçim callback'leri, tek selected etiketi, yeniden etkinleştirme ve oyuncu verisi geri yükleme kontrolleri geçti. Gerçek fare basılı tutma/sürükleme testi de başarılı. `mode-buttons.jpg` altı gerçek Unity görünümünü karşılaştırır.
- [x] 3. Geçiş: mevcut logo/maskotlar merkezde, yumuşak mavi ışık, ince bulmaca yolları, camgöbeği/altın yörüngeler ve korunmuş parlak dairesel kenar. Kenar süsleri yumuşatıldı. Son aday her iki temada 10 gerçek animasyon karesiyle incelendi; toplam 12 gerçek mod giriş/dönüş yüklemesi, duraklatılmış geçiş, tek callback, tam örtme, giriş kilidinin kalkması, materyal ve veri korunması başarılı. 111 GPU kontrolü de geçti. `Transitions/*-sequence.jpg` ve `*-transition.gif` Unity görüntülerinden üretildi.
- [x] 4. Birleşik kontrol: 1080×1920 telefonda ve 1536×2048 tablette üç mod/iki tema doğrulandı. Zorunlu duruşta yarıda kesilen son test yeniden çalıştırıldı: 65 canlı tema, 50 koyu tema GPU ve 111 düğme/geçiş GPU kontrolü başarılı. Son konsol: 0 hata, 0 uyarı. Güncel oyuncu tercihleri (5/5), mevcut yerel dosyalar (9/9), editör sahnesi, çözünürlük ve arka plan ayarı korundu. Sonuç: `Docs/MENU_POLISH_OVERVIEW.md` ve `Captures/MenuPolishRevision/Overview.jpg`.

## Koruma
Oynanış kuralları değişmez. Çalışma öncesi mevcut yerel dosya farkları ve oyuncu tercihleri özel geçici klasörde yedeklendi. Testler oyuncu verisinin geçici kopyasını kullanır. Açık sahnenin dosyasını dışarıdan değiştirmeden önce Boot sahnesine geçilir. Commitler yalnızca bu revizyona ait dosyaları içerir.
