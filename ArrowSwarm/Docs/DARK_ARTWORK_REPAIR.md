# Koyu tema çizim düzeltmeleri ve lider tablosu konseptleri

## Kullanıcı talepleri
1. Mod seçim penceresinin koyu temada bozulan çizimlerini düzelt; açık tema kalitesini koru.
2. Koyu üst göstergelerde kupa/yıldız arkasındaki mavi izleri kaldır.
3. Lider tablosu için birkaç görsel alternatif hazırla ve sohbette göster; onaydan önce oyuna entegre etme.

## Aşamalar
- [x] A. Mod kartları: ayrı gece çizimleri bağlandı; açık sprite'lar ve TMP korundu, shader ile renk değiştirme kapatıldı. Unity'de dokuz çift tema ekranında açık görsellerin eksiksiz geri geldiği doğrulandı. 42 gerçek GPU kontrolü; kaynak renkleri, beyaz parlamalar, alfa, basılı/kapalı durumlar ve başlık kontrastını doğruladı. Mod penceresi yakın görselleri `Captures/DarkArtworkRepair/Cards` altında.
- [x] B. Üst göstergeler: eski koyu göstergeler yuvarlak alfa maskesiyle kaldırıldı; iki temiz koyu çizim mevcut TMP'lerin altına bağlandı. Yakın Unity görüntüsünde mavi yamalar bulunmuyor. Açık tema geri dönüşü dokuz çift ekranla, yeni gösterge çizimi 22 GPU kontrolüyle doğrulandı. Genel 50 GPU kontrolü de geçti; yakın dekoratif oklar ve diğer maskot/logo pikselleri korunuyor. GPU koordinat kontrolleri platformun dikey okuma yönünü ve kenar yumuşatmasını dikkate alır.
- [x] C. Lider tablosu: açık klasik liste, podyumlu liste ve koyu liste olmak üzere üç ayrı 941×1672 konsept üretildi. Yalnızca `Captures/LeaderboardConcepts20261025` altında bulunur; oyun/sahne/prefab bağlantısı yoktur. Tam istemler README'de kaydedildi. Gerçek uygulamadaki lider tablosu değişmedi. Konseptler son yanıtta gösterilir.
- [x] D. Birleşik kontrol: telefon/tablette dokuz çift ekran, üç modun seçim callback'leri, gerçek fare basılı tutma/sürükleme, 67 canlı tema kontrolü, 42 kart/22 gösterge/50 genel GPU kontrolü başarılı. 5 oyuncu tercihi ve 9 mevcut yerel dosya eksiksiz korundu. Konsol: 0 hata; mevcut bulut lider tablosu servisinden bir bağlantı uyarısı kaydedildi. Derleme uyarısına neden olan kullanılmayan editör değişkeni ayrı commit'te kaldırıldı. Genel görünüm ve rapor hazır.

## Sorunun nedeni
Kartlarda düşük doygunluktaki her pikseli arka plan sanan koyulaştırma; beyaz yolu, ikon parlamalarını ve dönüş oklarını da boyuyordu. Üst göstergelerde altın rengi koruyan komşu piksel yaklaşımı ise çevredeki camgöbeği piksellerini de tutuyordu. Bu iki sorun eşik ayarı yerine ayrı çizim ve gerçek alfa ile giderilir.

## Koruma
Mevcut yerel dosyalar ve güncel oyuncu tercihleri özel klasörde yedeklendi. Testler geçici oyuncu kopyasını kullanır. Oynanış, açık temanın kaynak bitmapleri ve lider tablosunun mevcut uygulaması değişmez. Yeni çizimler yerleşik imagegen aracından alınır; model adı araçta seçilemediğinden belirli bir model kullanıldığı iddia edilmez.
