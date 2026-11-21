# ArrowSwarm Google Play Console rehberi

18 Kasım 2026. Bu rehber mağazaya hangi dosyaların yükleneceğini ve Console beyanlarından önce hangi kanıtların gerektiğini gösterir.

| Alan | Mevcut bilgi |
| --- | --- |
| Android paket kimliği | `com.farzetkiGames.ArrowSwarm`. Play Console ve bağlantılarda büyük ve küçük harfleri AAB manifestindeki gibi kullan. |
| İncelenen yerel AAB | Sürüm `1.0.0`, versionCode `1`, min API 25, hedef API 36, `arm64-v8a`. Console’da versionCode 1 kullanıldıysa yeni derleme için daha yüksek bir kod seç. |
| Mağaza türü ve kategori | Oyun, Bulmaca. Etiketleri Console’da mevcut oyun içeriğiyle eşleşen seçeneklerden seç. |
| Geliştirici sitesi | [FarzetkiGames](https://farzetkigames.com.tr) |
| Oyun ve gizlilik | [ArrowSwarm sayfası](https://farzetkigames.com.tr/arrowswarm), [gizlilik politikası](https://farzetkigames.com.tr/arrowswarm/privacy-policy) |
| Destek | İzlenen geliştirici destek e-postasını Console’a gir ve bir test mesajıyla teslimatı doğrula. |
| Herkese açık mağaza | Kontrolde [AAB kimliğiyle oluşturulan Play sayfası](https://play.google.com/store/apps/details?id=com.farzetkiGames.ArrowSwarm) HTTP 404 verdi. Duyuruda kullanmadan önce Console’daki resmi bağlantıyı hedef ülkeden ve oturum açmadan aç. |

## 1 · Ana mağaza girişi

Console’da **Mağaza varlığı → Ana mağaza girişi** bölümünden varsayılan dili ve yayına alacağın çevirileri seç. [Kopyalama ekranında](index.html) TR, EN, DE, FR, ES, PT, IT, RU, JA ve KO için metinler var. Başlık sınırı 30, kısa açıklama sınırı 80, uzun açıklama sınırı 4.000 karakter. Metin değişirse [karakter kontrolünü](validate_metadata.py) yeniden çalıştır. İlk dil setinde uygulamanın gerçek yerelleştirmesiyle ekran görüntüsü, başlık ve mağaza metnini eşleştir. [Google Play uygulama ve mağaza girişi rehberi](https://support.google.com/googleplay/android-developer/answer/9859152?hl=en).

| Varlık | Yüklenecek dosya |
| --- | --- |
| 512×512 mağaza ikonu | [app_icon_512x512.png](../03_GooglePlay/app_icon_512x512.png) |
| 1024×500 Türkçe özellik grafiği | [feature_graphic_final_TR_1024x500.png](../03_GooglePlay/feature_graphic_final_TR_1024x500.png) |
| 1024×500 İngilizce özellik grafiği | [feature_graphic_final_EN_1024x500.png](../03_GooglePlay/feature_graphic_final_EN_1024x500.png) |
| Türkçe telefon görüntüleri | [StoreSet_TR_20261118/Final](../03_GooglePlay/StoreSet_TR_20261118/Final/) dizinindeki son doğrulanmış görüntüler |
| İngilizce telefon görüntüleri | [Yayın ZIP’inde](../10_Launch/ReleaseKit_20261118/ArrowSwarm_Yayin_Marketing_TR_EN.zip) `03_GooglePlay/EN/Screenshots` altında sekiz görüntü. Sekizinci görselde sıralama örnek olarak etiketli. |
| İsteğe bağlı oynanış fragmanı | [ArrowSwarm_StoreTrailer_EN_25s.mp4](../06_Trailers_Ads_Tutorials/StoreLaunch_20261115/ArrowSwarm_StoreTrailer_EN_25s.mp4) |

Önerilen sıralama `01_Home` → `02_Defense` → `03_Shapes` → `04_ColorShift` → `05_Defense_Freeze` → `06_Shapes_Advanced` → `07_ColorShift_Rotate` → `08_Leaderboard`. Telefon için iki görüntü yayın asgarisidir. İlk üç görsele oyunu tanıtan okunur ekranlar koy. Görselleri Console önizlemesinde cihaz boyutunda kontrol et. Sıralama görüntüsünde gerçek kullanıcı verisi, geliştirici işareti ve üst sıra ya da indirme sayısı iddiası bulunmadığını kontrol et. Görsel betimlemelerini ilgili dilde ekle. [Google Play önizleme varlıkları](https://support.google.com/googleplay/android-developer/answer/9866151?hl=en).

Fragmanı seçersen YouTube’a reklam gösterimi kapalı, herkese açık veya liste dışı ve yerleştirilebilir olarak yükle. Yaş kısıtı olmayan tek video bağlantısını grafikler bölümündeki video alanına ekle. EN fragmanın dış yazıları İngilizce, Savunma HUD metni Türkçe. Bir dil için uygun fragman hazır değilse o dilde alanı boş bırakabilirsin. [Google Play önizleme videosu](https://support.google.com/googleplay/android-developer/answer/9866151?hl=en).

## 2 · Uygulama içeriği ve Veri Güvenliği

**Gizlilik ve destek.** Gizlilik politikası URL’sini Console’a ekle. Final uygulamada erişilebilir bir bağlantı veya metin bulunmalı. [Canlı gizlilik sayfasını](https://farzetkigames.com.tr/arrowswarm/privacy-policy) açıp uygulama adı, geliştirici, destek, gerçek veri kullanımı, saklama ve silme süreçlerini final AAB ile karşılaştır. Proje taramasında gizlilik sayfasına açılan bir uygulama içi bağlantı doğrulanmadı. [Google Play kullanıcı verileri politikası](https://support.google.com/googleplay/android-developer/answer/10144311?hl=en).

**Reklamlar.** Oyunda ödüllü ve geçiş reklamları için AdMob akışları bulunuyor. [Yönetici prefabında](../../Assets/_Project/Prefabs/Core/CoreManagers.prefab) `_useTestAds` açık. [Reklam servisi](../../Assets/_Project/Scripts/Ads/GoogleMobileAdsService.cs), test modu açıkken veya ilgili üretim birimi kimliği boşken test reklamını seçiyor. Final sürümde reklam gösterimi ve SDK’nın etkinliği üzerinden Console’da **Reklamlar** alanını yanıtla. Reklamla yayına çıkacaksan üretim birimlerini ve kullanıcı seçimlerine bağlı reklam isteği akışını doğrula. Proje taramasında UMP onay akışı bulunmadı. UMP kullanıldığında açılışta durumu güncelle, gerekiyorsa onay formunu ve gizlilik seçeneklerini göster, reklam isteğinden önce `CanRequestAds()` durumunu kontrol et. Bölge, onay/red, çevrimdışı açılış, gösterim yokken geri dönüş ve ödülün bir kez verilmesi senaryolarını mağazadan kurulan yapıda dene. [AdMob Unity gizlilik rehberi](https://developers.google.com/admob/unity/privacy), [AEA için reklam onayı](https://developers.google.com/admob/unity/privacy/gdpr).

**Hedef kitle.** Yaş gruplarını oyunun gerçekten tasarlandığı kitleye, mağaza görsellerine ve hedef ülkelere göre belirle. Çocukları kapsayan bir seçim yaptığında çocuklara ve yaşı bilinmeyen kullanıcılara yönelik tasarım ve reklam gerekliliklerini ayrıca kontrol et. [Hedef kitle ve içerik rehberi](https://support.google.com/googleplay/android-developer/answer/9867159?hl=en), [Google Play Aileler Politikası](https://support.google.com/googleplay/android-developer/answer/9893335?hl=en).

**IARC derecelendirmesi.** Savunma’da renkli oklar stilize düşmanlara atılıyor. Soru formuna gerçek görüntüleri esas alarak bu çatışmayı ve uygulamanın diğer içeriklerini bildir. Oyuncunun takma ad girebildiği ve skor listesindeki adları görebildiği etkileşimi de ilgili soru çıkarsa değerlendir. Yaş derecesi formun sonucu. [Google Play içerik derecelendirmesi](https://support.google.com/googleplay/android-developer/answer/9859655?hl=en).

| Veri yolu | Play formuna girmeden önce doğrula |
| --- | --- |
| Unity Authentication | [Bulut servisi](../../Assets/_Project/Scripts/Data/UnityCloudService.cs) anonim oturum açıyor ve oyuncu kimliği kullanıyor. Temiz kurulum ve çevrimdışı/çevrimiçi geçişte bu hesabın ne zaman oluştuğunu ve oyuncunun bu işlemi hangi akışta seçebildiğini belirle. [Unity anonim giriş](https://docs.unity.com/en-us/authentication/use-anon-sign-in). |
| Profil ve liderlik | Aynı servis görünen adı güncelliyor ve seviye, yıldız, ad ve ülke kodunu skor metadatası olarak gönderiyor. Ülke kodu alanı tek başına cihaz konumu kanıtı sunmaz. Oyunculara hangi verilerin göründüğünü, gönderimin nasıl tetiklendiğini ve saklama sürecini test et. |
| Google Mobile Ads ve diğer SDK’lar | AAB’deki SDK sürümlerini, varsa reklam arabuluculuğu sağlayıcılarını ve etkin yapılandırmayı çıkar. IP tabanlı konum, reklam tanımlayıcıları, etkileşimler ve tanılama gibi kategorileri yüklenen sürümlerin beyanları ve cihaz akışıyla eşleştir. Amaç, işleme süresi, toplama ve paylaşım alanlarını bu kanıtla birbirinden ayır. [AdMob veri beyanı kılavuzu](https://developers.google.com/admob/android/privacy/play-data-disclosure). |
| Güvenlik ve kullanıcı seçimi | Aktarım şifrelemesini tüm etkin servisler için doğrula. Toplanması zorunlu ve isteğe bağlı verileri gerçek kullanıcı akışına göre belirle. Bir profil ekranını atlama seçeneği otomatik SDK verilerinin de isteğe bağlı olduğunu göstermez. [Google Play Veri Güvenliği](https://support.google.com/googleplay/android-developer/answer/10787469?hl=en). |

**Hesap ve veri silme.** UGS anonim kimliği oyuncu kaydı oluşturabildiği için bu akışa Google Play’in hesap silme kuralının uygulanıp uygulanmadığını final davranışı ve Console formuyla netleştir. Kural uygulanıyorsa uygulamada kolay bulunur bir talep yolu, dışarıda da ArrowSwarm hesabı için açık adımlar içeren erişilebilir bir adres sağla. Talep testi, UGS kimliğinin ve ilgili liderlik verisinin silinmesini kapsamalı. Genel iletişim sayfasının ulaşılabilir olması tek başına silme işleminin çalıştığını doğrulamaz. `DeleteAccountAsync()` Unity Authentication hesabını siler. Diğer UGS kayıtlarını da kapsayan işlem gerekir. [Google Play hesap silme gereklilikleri](https://support.google.com/googleplay/android-developer/answer/13327111?hl=en), [Unity hesap silme](https://docs.unity.com/en-us/authentication/delete-accounts).

## 3 · Testten üretime yükleme sırası

1. **Uygulama ve ülke ayarları.** Paket kimliğini ve sürümü ilk AAB ile eşleştir. Play App Signing ve yükleme sertifikasını Console’daki kayda göre doğrula. Varsayılan dil, dağıtım ülkeleri ve ücretsiz/ücretli seçimini hesabında belirle. [Google Play sürüm hazırlama rehberi](https://support.google.com/googleplay/android-developer/answer/9859348?hl=en).
2. **Dahili test.** İmzalı aday AAB’yi test kanalına yükle. App Bundle Explorer’da manifest, izinler, hedef API, arm64 ve native kütüphanelerin 16 KB sayfa desteğini kontrol et. Pre-launch raporundaki çökme ve uyumluluk bulgularını değerlendir. Gerçek cihazlarda ilk kurulum, üç mod, kaydetme, geri yükleme, bağlantı kaybı ve sürüm güncellemesi dene. [Hedef API gereklilikleri](https://support.google.com/googleplay/android-developer/answer/11926878?hl=en), [Android 16 KB uyumluluk rehberi](https://developer.android.com/guide/practices/page-sizes).
3. **Kapalı test ve üretim erişimi.** Console panosundan hesabının üretim erişimini kontrol et. 13 Kasım 2023’ten sonra açılan kişisel geliştirici hesapları için Google’ın kapalı test kuralı, en az 12 testçinin kesintisiz en az 14 gün katılımı ve ardından üretim erişimi başvurusudur. Hesabın gerektirdiği kanal ve test geçmişini Console’dan doğrula. [Yeni kişisel hesapların test gereklilikleri](https://support.google.com/googleplay/android-developer/answer/14151465?hl=en).
4. **Yayın adayı.** Formları final AAB ve cihaz kanıtıyla doldur, mağaza önizlemesini kontrol et, ardından onaylanan paketi ve sürüm notlarını üretim incelemesine gönder. Canlı sayfada ilk kurulum doğrulanınca bağlantıyı siteye, sosyal profil ve [yayın haftası metinlerine](../08_Social_Media/LAUNCH_POST_PACK_TR_EN.md) ekle. AdMob kullanıyorsan geliştirici sitesi alanını ve [app-ads.txt adresini](https://farzetkigames.com.tr/app-ads.txt) doğrula. Tarama ve yetkilendirme sonucunu AdMob’da kontrol et. [AdMob app-ads.txt adımları](https://support.google.com/admob/answer/9363762?hl=en).

## 4 · Yayın sonrası ASO deneyleri

1. **İlk ekran.** `01_Home` ile ilk sıraya `03_Shapes` konan dizilimi aynı dil ve diğer görsellerle karşılaştır.
2. **İkon.** Seçili mağaza ikonuyla tek renk veya kontrast değişikliği içeren bir alternatifi karşılaştır.
3. **Kısa açıklama.** Üç mod özetini, tek hamle ya da şekil sorusuyla başlayan bir metinle karşılaştır.

Bir deneyde bir öğeyi değiştir. Her dil için görünürlük, mağaza sayfasına geçiş ve kurulum dönüşümünü aynı dönem içinde izle. Örneklem kararı desteklemiyorsa deneyi açık tut.
