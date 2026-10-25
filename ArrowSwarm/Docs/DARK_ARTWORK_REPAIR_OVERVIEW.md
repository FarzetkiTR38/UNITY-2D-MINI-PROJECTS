# Koyu tema çizimleri ve lider tablosu seçenekleri

Mod seçim penceresi ile üst göstergelerin koyu tema sorunları giderildi. Açık temadaki mevcut çizimler, TMP metinleri, yerelleştirme ve düğme efektleri korundu.

- Mod kartları artık ayrı gece çizimleri kullanır. Beyaz yol, ok parlamaları ve dönüş simgeleri doygunluk eşiğiyle boyanmaz.
- Kupa/yıldız göstergelerinde eski çizim tamamen kaldırılır ve iki temiz koyu çizim mevcut yazıların altında gösterilir. İkon çevresindeki mavi yamaları koruyan renk sınıflandırması kaldırıldı. Açık temada ek çizimler tamamen saydamdır.
- Üç lider tablosu tasarımı yalnızca önizlemedir. Hiçbiri sahneye, prefaba, materyale veya oyun koduna bağlanmadı.

## Görseller

- [Gerçek Unity açık/koyu karşılaştırması](../Captures/DarkArtworkRepair/Overview.jpg)
- [Koyu mod penceresi](../Captures/DarkArtworkRepair/Cards/Modal-dark.png)
- [Koyu üst göstergeler](../Captures/DarkArtworkRepair/Hud/HUD-dark.png)
- [A — Klasik açık lider tablosu](../Captures/LeaderboardConcepts20261025/A-Classic-Light.png)
- [B — Podyumlu açık lider tablosu](../Captures/LeaderboardConcepts20261025/B-Podium-Light.png)
- [C — Lacivert lider tablosu](../Captures/LeaderboardConcepts20261025/C-Midnight-Dark.png)

Görseller yerleşik imagegen ile üretildi. Tam istemler ve dosya konumları [üretim notlarında](DARK_ARTWORK_GENERATION.md), lider tablosu istemleri [konsept README'sinde](../Captures/LeaderboardConcepts20261025/README.md) bulunur. Örnek oyuncu adları ve puanlar kurmacadır.

## Kontroller

| Kontrol | Sonuç |
| --- | --- |
| Kart çizimlerinin kaynak renkleri, beyaz detayları, alfa ve basılı/kapalı durumları | [42 GPU kontrolü başarılı](../Captures/DarkArtworkRepair/Cards/dark-card-artwork-gpu.txt) |
| Gösterge renkleri, altın/beyaz parlamaları, saydam dış köşeler ve açık temada gizlenme | [22 GPU kontrolü başarılı](../Captures/DarkArtworkRepair/Hud/dark-hud-artwork-gpu.txt) |
| Genel düğmeler, temizlenen eski göstergeler ve korunan dekoratif oklar | [50 GPU kontrolü başarılı](../Captures/DarkArtworkRepair/Hud/dark-menu-gpu.txt) |
| Tema değiştirme, yeniden etkinleştirme ve gerçek geçiş sırasında tema değiştirme | [67 canlı kontrol başarılı](../Captures/DarkArtworkRepair/Final/live-theme.txt) |
| Üç modun seçim callback'leri, doğru görsel ve tek seçili yazısı | [İki temada başarılı](../Captures/DarkArtworkRepair/Final/PhoneModes/mode-artwork.txt) |
| Gerçek fare basılı tutma, düğme dışına sürükleme ve kapalı durum | [Başarılı](../Captures/DarkArtworkRepair/Final/pointer-verification.txt) |
| Açık temaya eksiksiz geri dönüş | Telefonda 1080×1920, tablette 1536×2048; dokuzar çift ekran — [telefon](../Captures/DarkArtworkRepair/Hud/Native/capture-report.txt), [tablet](../Captures/DarkArtworkRepair/Final/Tablet/capture-report.txt) |
| Oyuncu tercihleri ve mevcut yerel dosyalar | [5 tercih / 9 dosya eksiksiz korundu](../Captures/DarkArtworkRepair/Final/restoration.txt) |

Statik karşılaştırmada 1119 orijinal sahne bloğunun hiçbiri silinmedi. Yalnızca üç kartın koyu sprite referansı ve MenuArt'ın iki yeni çocuğu değişti. Mevcut 111 Image, 54 TMP, 29 yerelleştirme, 36 Button, 1 input ve 9 geri bildirim bloğu değişmedi. Lider tablosunun 43 nesnesi/218 bloğu aynen korundu; konseptlere Assets altından hiçbir referans bulunmuyor.

Son konsolda **0 hata**, mevcut bulut Leaderboards servisinden **1 bağlantı uyarısı** vardı. Uyarı menü denetiminde eski lider tablosu açılırken oluştu; arayüz/tema/girdi kontrolleri başarılı. Servis davranışı veya lider tablosu uygulaması bu görevde değiştirilmedi. Unity testlerden sonra güvenli giriş sahnesi BootScene'de, Edit Mode'da bırakıldı.

Plan, konseptler, mod kartları, göstergeler, derleme temizliği ve son doğrulama ayrı commit'ler olarak kaydedildi. Önceki adayların ham kareleri özel arşive alınır; son karşılaştırmalar ve gerekli gerçek Unity ekranları repoda saklanır.
