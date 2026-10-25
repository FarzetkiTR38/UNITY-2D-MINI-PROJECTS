# Koyu tema çizim düzeltmeleri ve lider tablosu konseptleri

## Kullanıcı talepleri
1. Mod seçim penceresinin koyu temada bozulan çizimlerini düzelt; açık tema kalitesini koru.
2. Koyu üst göstergelerde kupa/yıldız arkasındaki mavi izleri kaldır.
3. Lider tablosu için birkaç görsel alternatif hazırla ve sohbette göster; onaydan önce oyuna entegre etme.

## Aşamalar
- [ ] A. Mod kartları: ayrı, çizilmiş koyu tema sprite'ları; açık sprite'lar ve TMP değişmez. Shader ile beyaz yol/parlaklık boyanmaz. Unity görüntüsü, seçim ve GPU kontrolünden sonra commit.
- [ ] B. Üst göstergeler: eski koyu göstergeleri güvenli yuvarlak alfa maskesiyle kaldır; temiz koyu göstergeleri sahnede mevcut TMP'lerin altına bağla. Açık tema değişmez. Yakın görüntü, tema dönüşü ve GPU kontrolünden sonra commit.
- [x] C. Lider tablosu: açık klasik liste, podyumlu liste ve koyu liste olmak üzere üç ayrı 941×1672 konsept üretildi. Yalnızca `Captures/LeaderboardConcepts20261025` altında bulunur; oyun/sahne/prefab bağlantısı yoktur. Tam istemler README'de kaydedildi. Gerçek uygulamadaki lider tablosu değişmedi. Konseptler son yanıtta gösterilir.
- [ ] D. Birleşik kontrol: telefon/tablet, iki tema, seçili mod, basılı tutma, oyuncu verisi ve yerel dosya koruması; genel görünüm ve rapor commit'i.

## Sorunun nedeni
Kartlarda düşük doygunluktaki her pikseli arka plan sanan koyulaştırma; beyaz yolu, ikon parlamalarını ve dönüş oklarını da boyuyordu. Üst göstergelerde altın rengi koruyan komşu piksel yaklaşımı ise çevredeki camgöbeği piksellerini de tutuyordu. Bu iki sorun eşik ayarı yerine ayrı çizim ve gerçek alfa ile giderilir.

## Koruma
Mevcut yerel dosyalar ve güncel oyuncu tercihleri özel klasörde yedeklendi. Testler geçici oyuncu kopyasını kullanır. Oynanış, açık temanın kaynak bitmapleri ve lider tablosunun mevcut uygulaması değişmez. Yeni çizimler yerleşik imagegen aracından alınır; model adı araçta seçilemediğinden belirli bir model kullanıldığı iddia edilmez.
