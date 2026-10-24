# Menü görsel revizyonu

## Kapsam
Üç kullanıcı talebi: koyu temada üst istatistik göstergelerinin açık renkli alt izlerini temizlemek; ana menünün mod düğmesinde seçili moda ait görseli göstermek; sahne geçişini oyunun parlak mavi/altın görsel diline uygun yenilemek.

## Aşamalar
- [x] 1. İstatistik göstergeleri: iç yüzeydeki beyaz/camgöbeği alt yansıma tamamen kaldırıldı; kupa/yıldızın altın renkleri ve parlak noktaları ile mavi dış çerçeve korundu. Unity'de 9 çift tema ekranı ve 50 gerçek GPU kontrolü geçti. `Captures/MenuPolishRevision/hud-after.png` yakın görünümü içerir.
- [x] 2. Mod düğmesi: üç modun açık/koyu tema görselleri Inspector'da bağlandı; mevcut Button, TMP etiketleri, yerelleştirme, tıklama hedefi ve basılma efekti korundu. Unity MCP üzerinden altı görünüm, üretim seçim callback'leri, tek selected etiketi, yeniden etkinleştirme ve oyuncu verisi geri yükleme kontrolleri geçti. Gerçek fare basılı tutma/sürükleme testi de başarılı. `mode-buttons.jpg` altı gerçek Unity görünümünü karşılaştırır.
- [ ] 3. Geçiş: daha dengeli desen, derinlik ve tema uyumlu çizim; tam örtme, açılma ve sahne yükleme kontrolleri; ayrı commit.
- [ ] 4. Birleşik kontrol: iki tema, üç mod, telefon/tablet görünümü; veri ve kaynak materyal korunması; sonuç görselleri ve rapor.

## Koruma
Oynanış kuralları değişmez. Çalışma öncesi mevcut yerel dosya farkları ve oyuncu tercihleri özel geçici klasörde yedeklendi. Testler oyuncu verisinin geçici kopyasını kullanır. Açık sahnenin dosyasını dışarıdan değiştirmeden önce Boot sahnesine geçilir. Commitler yalnızca bu revizyona ait dosyaları içerir.
