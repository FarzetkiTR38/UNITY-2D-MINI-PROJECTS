# Menü görsel revizyonu

## Kapsam
Üç kullanıcı talebi: koyu temada üst istatistik göstergelerinin açık renkli alt izlerini temizlemek; ana menünün mod düğmesinde seçili moda ait görseli göstermek; sahne geçişini oyunun parlak mavi/altın görsel diline uygun yenilemek.

## Aşamalar
- [ ] 1. İstatistik göstergeleri: mevcut ikonları ve dış çerçeveyi koruyarak iç yüzeyi temizle; gerçek Unity görüntüsü ve GPU doğrulaması; ayrı commit.
- [ ] 2. Mod düğmesi: Defense, Shapes, Color Shift için Inspector bağlantılı görseller; TMP ve yerelleştirmeyi koru; seçim, tekrar açma, tema ve basılı tutma kontrolleri; ayrı commit.
- [ ] 3. Geçiş: daha dengeli desen, derinlik ve tema uyumlu çizim; tam örtme, açılma ve sahne yükleme kontrolleri; ayrı commit.
- [ ] 4. Birleşik kontrol: iki tema, üç mod, telefon/tablet görünümü; veri ve kaynak materyal korunması; sonuç görselleri ve rapor.

## Koruma
Oynanış kuralları değişmez. Çalışma öncesi mevcut yerel dosya farkları ve oyuncu tercihleri özel geçici klasörde yedeklendi. Testler oyuncu verisinin geçici kopyasını kullanır. Açık sahnenin dosyasını dışarıdan değiştirmeden önce Boot sahnesine geçilir. Commitler yalnızca bu revizyona ait dosyaları içerir.
