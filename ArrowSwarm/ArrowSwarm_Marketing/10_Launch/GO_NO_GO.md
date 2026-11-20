# ArrowSwarm · Yayına kalan işler

**18 Kasım 2026 · Google Play üretim yayını beklemeli.** Mağaza ve ilk hafta pazarlama paketi hazır. Optimize AAB yerelde imza ve paket kontrollerini geçti. Üretimden önce reklam izni, veri beyanı, gerçek cihaz testi ve Play Console erişimi kapanmalı.

[Mağaza ve yayın haftası paketini indir](ReleaseKit_20261118/ArrowSwarm_Yayin_Marketing_TR_EN.zip) · [Sıralı kontrol listesi](LAUNCH_CHECKLIST.csv) · [Önizleme](ReleaseKit_20261118/index.html)

## Yayına geçmeden önce

| Öncelik | Kalan iş | Tamam sayılacağı kanıt |
| --- | --- | --- |
| P0 | **Reklam izni ve reklam modu** | `CoreManagers.prefab` içinde `_useTestAds` açık. 20 Kasım kaynak kodunda Google UMP açılış kontrolü, gerekli form, reklam isteği izni ve Ayarlar’da gizlilik seçenekleri var. Unity ve yalıtılmış yaşam döngüsü kontrolleri geçti. Bu kodu içeren imzalı AAB, AdMob mesajının ilgili bölgelerdeki davranışı, gerçek cihazda izin değişikliği ve test reklamıyla ödül/geçiş senaryoları bekliyor. [UMP Unity](https://developers.google.com/admob/unity/privacy) · [AdMob bölgesel koşulları](https://support.google.com/admob/answer/13554116?hl=en) |
| P0 | **Veri Güvenliği, reklam beyanı ve gizlilik** | UGS anonim oturumu ve sıralamaya gönderilen takma ad, seçilen ülke, skor/ilerleme bilgisi ile AdMob verisi gerçek SDK davranışıyla eşleştirilir. Anonim profil akışına hesap silme şartının uygulanırlığı değerlendirilir. Uygulanıyorsa uygulama içi yol ve çalışan web talebi denenir. Mevcut web politikasındaki silme süresi, iletişim ve izin beyanlarını gerçek süreçle eşleştir. Hedef yaş grubunu gerçek tasarım ve içeriğe göre seç, IARC anketini tamamla. [Veri Güvenliği](https://support.google.com/googleplay/android-developer/answer/10787469?hl=en) · [Hesap silme](https://support.google.com/googleplay/android-developer/answer/13327111?hl=en) · [Uygulama içeriği](https://support.google.com/googleplay/android-developer/answer/9859455?hl=en) |
| P0 | **Yükleme anahtarı, sürüm kodu, cihaz testi** | Play Console bu paketi ve imzayı kabul eder, `versionCode` kullanılabilir. Son AAB internal testten yüklenir. Aşağıdaki cihaz kontrolü ve Play pre-launch raporu geçer. Reklam veya gizlilik kodu değişirse yeniden derle ve yeni AAB’yi doğrula. |
| P0 | **Üretim erişimi ve mağaza incelemesi** | Play Dashboard uygulama bazında Production erişimini gösterir. 13 Kasım 2023’ten sonra açılmış kişisel hesaplara uygulanıyorsa en az 12 testçi 14 gün kesintisiz kapalı testte kalır, ardından üretim erişimi için başvuru yapılır. Console ülke, dağıtım ve destek e-postası bilgileri son kez doğrulanır. [Google Play test şartı](https://support.google.com/googleplay/android-developer/answer/14151465?hl=en) |
| Yayın günü | **Mağaza bağlantısı, web ve ilk gönderiler** | AAB ile birebir eşleşen `com.farzetkiGames.ArrowSwarm` bağlantısı kontrolde HTTP 404 döndü. Hedef ülkede sayfayı açıp indirme kurulumu görünce site ve gönderilerde doğrulanmış bağlantıya geç. Yerel web paket adı düzeltmesi `farzetkigames` commit `bb00d8e` içinde. Canlıya alınca gizlilik ve mağaza bağlantısını tekrar kontrol et. |
| Kullanımdan önce | **Video ve ses kullanım hakları** | Ticari paylaşım veya reklam öncesi kullanılan müzik ve ses kayıtlarının lisansını ve seslendirme sağlayıcısının kullanım koşullarını doğrula. Mağaza fragmanı isteğe bağlı. Kullanacaksan herkese açık veya liste dışı, yerleştirmeye açık, yaş kısıtı olmayan, reklamsız YouTube videosunun URL’sini gir. [Play önizleme varlıkları](https://support.google.com/googleplay/android-developer/answer/9866151?hl=en) |

## Yerelde doğrulanan Android paketi

İlk teknik kontrol için `Builds/ArrowSwarm v1.0.0 build optimize hali.aab` kullan. Tam boy paket yaklaşık 161,7 MB. Optimize paket **80,2 MB** ve imza doğrulaması geçti. Gerçek manifestte `com.farzetkiGames.ArrowSwarm`, `1.0.0 (1)`, minimum API 25, hedef ve derleme API 36, ARM64 var. Paket yapılandırması ve yedi native kitaplık statik 16 KB kontrolünü geçti. Bu AAB’nin güncel Git revizyonundan üretildiği kayda bağlanmamış. Play anahtar eşleşmesi ve cihaz sonucu ayrı olarak alınmalı. [Hedef API şartı](https://support.google.com/googleplay/android-developer/answer/11926878?hl=en) · [16 KB doğrulama](https://developer.android.com/guide/practices/page-sizes)

Yerel ADB listesi boştu. Android cihaz koşusu yerine mevcut Unity Editor ve medya kanıtları değerlendirildi. Build Settings içinde Boot, Ana Menü ve üç oyun sahnesi etkin. MapTest devre dışı. Mağaza çekiminden sonra BootScene düzenleme moduna dönüldü, oyuncu tercihleri geri yüklendi ve Unity Console’da hata sayısı sıfırdı.

### Internal testte oynatılacak akışlar

- [ ] Temiz kurulumda soğuk açılış, profil ve ilk öğretici. TR/EN, açık/koyu tema, dar ve uzun telefon ekranı. Aynı açılışı düşük kapasiteli bir cihazda ve 16 KB Android ortamında dene.
- [ ] Savunma, Şekiller ve Renk Değişimi. İlk seviyeler, bir ileri seviye, yanlış ok ve son can animasyonu, dönme hakkı, duraklatma, kazanma, yeniden dene ve ana menüye dönüş.
- [ ] Oynayıp kapat ve tekrar aç. İlerleme, dil ve tema korunuyor mu? İnternet kapalıyken açılış, sonra yeniden bağlanınca UGS sıralaması, oyuncu adı/ülke ve güncel skor doğru mu?
- [ ] Uygulanabilir bölge için reklam izni kabul/ret/hata. Test reklamında ödül bir kez geliyor mu? Reklam yüklenmezse ekran kullanılabilir mi? Gizlilik ve varsa silme yolundan çalışan hedefe varılıyor mu?
- [ ] Final AAB kimliği/sürümü, Play App Signing, cihaz raporları ve politika uyarıları. Bilinen P0 sorunlar açıkken üretim yüklemesini başlatma.

## Pazarlama teslimi

- **Mağaza:** 512×512 seçilen ikon, TR/EN 1024×500 özellik grafikleri, 8 TR ve 8 EN ekran görüntüsü, iki dilde alt metin. TR çekimleri gerçek Unity ekranlarından. Sıralama kartı her iki dilde örnek olarak etiketli.
- **Metin:** 10 dilde 30 başlık/açıklama alanı paketlendi, her alan karakter sınırına karşı doğrulandı. Grafikler TR/EN. [Kopyalama merkezi](../07_ASO_SEO_GEO/index.html) · [Play Console doldurma rehberi](../07_ASO_SEO_GEO/PLAY_CONSOLE_SETUP_GUIDE.md)
- **Video:** Mevcut mağaza fragmanı, dikey reklam, Reels serileri ve eğitim videosu hazır. Yayın haftası paketi gerçek dosyalara bağlı. [Tüm video arşivi](../Final_Deliverables/VideoLibrary/index.html) · [TR/EN ilk hafta gönderileri](../08_Social_Media/LAUNCH_POST_PACK_TR_EN.md)
- **Teslim kontrolü:** Önizlemedeki 11 bağlantı ve video arşivindeki 50 MP4’ün yerel sunucudan erişimi doğrulandı. İndirilen ZIP, 63 iç dosyası ve masaüstü kopyası beklenen SHA-256 değerleriyle eşleşti. [Paket doğrulaması](ReleaseKit_20261118/package_checks.json)
- **Web:** Oyun, gizlilik, iletişim ve `app-ads.txt` rotaları HTTP 200 döndü. AdMob doğrulama durumu, destek formunun uçtan uca teslimi ve arama motoru indeksleme durumu ayrı kontrol. [Oyun sayfası](https://farzetkigames.com.tr/arrowswarm) · [Gizlilik](https://farzetkigames.com.tr/arrowswarm/privacy-policy)
- **Yayın sonrası:** Organik başlangıç ve KPI şablonu hazır. Console’daki gerçek mağaza/kurulum verisiyle başla. D1/D7 ve bölüm terki ancak ölçüm gerçekten bağlı, izinler güncel ve veri geldiğinde raporlanır. [Ölçüm rehberi](../09_Analytics/MEASUREMENT_GUIDE.md)

**İlk işlem:** Play Console Dashboard’da üretim erişimini kontrol et ve optimize imzalı AAB’yi internal test adayı olarak değerlendir. Paralelde reklam izni ve veri silme kararını kapat. Düzeltilmiş son AAB cihazdan geçince mağaza paketini uygulama kaydına bağla.
