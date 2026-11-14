# Yayın kararı — NO-GO

13 Kasım 2026. Bu karar bir uygulama reddi değildir; gerekli yayın kanıtları henüz yoktur. Pazarlama belgelerinin veya teknik medya dosyalarının hazır olması üretim uygulamasının hazır olduğunu göstermez. Bu turda Unitykontrol araçları sunulmadığından yeni derleme/cihaz/PlayConsole testi yapılmadı.

## En önce kapatılacak kapılar

1. İmzalı releaseAAB alınmalı. AndroidhedefiAutomatic(0); gerçekmanifest ve Play doğrulaması görülmeden APIuygunluğu söylenemez.
2. Kaynakta native/IL2CPP mimari ayarları bulunması sonAAB'nin ARM64/16KBuyumunu kanıtlamaz;binaryvegerçekçalıştırma gerekli.
3. Üç mod, ilk oyuncu akışı, kaydetme/bulut, reklam/izin/yaş akışı, ekran oranları ve nativeyerelleştirme cihazda denenmeli.
4. Console üretim erişimi, gerekli testler, uygulama içeriği ve veri beyanları hesap/uygulama bazında doğrulanmalı.
5. Kullanıcının beğenmediği ikon/Reels ile karma tarihli mağaza taslakları final sürüm olarak gönderilmemeli.

## Güncel resmî kuralların bu proje açısından sonucu

- Standart telefonlarda yeniapp/update için31Ağustos2026'dan beri API36gerekiyor. KaynaktaAutomaticolması bu koşulu karşıladığı anlamına gelmez. [Google Play hedefAPI](https://support.google.com/googleplay/android-developer/answer/11926878?hl=en).
- Android'in erişilen16KBbelgesi, API35+64bituyumluluğunu ve güncellemeler için1Şubat2027engelleme tarihini belirtiyor. Eski2025tarihli özetlere dayanarak bu buildgeçti denmez. Uygulama/SDKnativebinaryleri ve Consoleuyarıları tek tek kontrol edilmeli. [Android sayfa boyutları](https://developer.android.com/guide/practices/page-sizes).
-13Kasım2023sonrası açılan ilgili kişisel hesaplar için12testerin14günkesintisizopt-in koşulu ve sonrası üretim erişimi başvurusu var. Bu hesabın tipi/tarihi bilinmediğinden koşul bu hesaba otomatik atanmadı. [Test gereklilikleri](https://support.google.com/googleplay/android-developer/answer/14151465?hl=en).
- Veri beyanı, kullanılanSDK'ların davranışlarını da kapsamalı; gizlilikpolitikası ve gerçekuygulama davranışı tutarlı olmalı. Kaynak incelemesi veya boşdashboard hukuki/teknikuygunluk belgesi değildir. [DataSafety](https://support.google.com/googleplay/android-developer/answer/10787469?hl=en), [AppContent](https://support.google.com/googleplay/android-developer/answer/9859455?hl=en).

Tamgates ve gerekenkanıtlar LAUNCH_CHECKLIST.csv içinde. Üretim yayını, Console formugönderimi, hesap işlemi veya harcama yapılmadı.
