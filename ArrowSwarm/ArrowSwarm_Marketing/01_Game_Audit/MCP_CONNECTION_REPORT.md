# MCP erişimi ve gerçek kayıt kanıtı

13Kasım2026 devam oturumunda native Unity MCP yeniden doğrulandı: ArrowSwarm@3f240769be4f5e6f,6000.3.18f1,Androidhedefi,1080x1920GameView. Başlangıç ve bitiş MainMenuScene, EditMode, dirty=false. Önceki oturumdaki izin/terminal engelleri bu oturumda tekrarlanmadı; çalışma dalı ve yerel dosya işlemleri gerçekten başarılı.

Doğrulanmış eylemler: instances/custom-tools/editor-state,execute_code,refresh_unity,read_console,Play/Stop. Yeni partial kayıt kaynaklarının ilk scripts-only derlemesi yeni dosyaları import etmedi ve CaptureScope hatası verdi. Tam asset refresh ile yeni dosyalar import edildi; sonraki derleme ve kayıt sonrası Console sorguları sıfır hata döndürdü. Bu kayıt gerçekçidir; ilk hata gizlenmedi.

## Gerçek çıktı

- Smoke:04_Raw_Gameplay/Shapes_L2_20261113_081427_071,30kare/1s/1080x1920.
- Ana çekim:04_Raw_Gameplay/Shapes_L2_20261113_081526_913,600kare/20s/30fps/1080x1920.
- Üretim PuzzleSession.Fire çağrılarıyla yasal42hamle; son provenance remaining=0,won=true. Board yeniden çizilmedi, AI gameplay kullanılmadı. Kontroller solver tarafından otomatik yürütüldü; insan dokunuşu kaydı diye sunulmaz.
- Ad/cloud sağlayıcıları, oyun verisi, dil/tema, launchcontext ve zaman/RNG/background ayarları scope sonunda doğrulandı. PlayStop sonrasında beş özel tercih anahtarı varlık/değerleriyle geri yüklendi; içerikleri teslimata eklenmedi.
- Oyuncunun önceden değişmiş14dosyası, kayıt öncesi/sonrası hash karşılaştırmasında aynı:source_preservation.json.

Kayıt sabit simülasyon adımlarıyla ekran kareleri alır; ağır Editor işinde duvar saati20s'den uzun sürebilir. Render30fps aynı oyun zamanını korur, son montajda oynanış hızlandırılmaz. NativeMCP doğrudan video/sistem sesi kaydetmez; FFmpeg gerçek kareleri MP4'e çevirir. Montajdaki sesler sonradan eklenmiş özgün editoryal sestir. Gerçek cihaz ve PlayConsole testi yapılmadı.


## Sonraki devam turu — erişim ayrımı
Unity kontrol araçları bu devam turunda sunulmuyor. Yukarıdaki bağlantı/kayıt sonuçları önceki gerçek testin kanıtıdır; bu turda yeni canlı test veya kayıt yapıldığı iddia edilmez. Güncel kayıt kuyruğu04_Raw_Gameplay/CAPTURE_QUEUE.csv içindedir.
