# Geçiş logosu ve ses incelemesi · 2026-11-11

Bu istek, geçişte kesilen görseli düzeltmek ve mevcut sesleri alternatifleriyle birlikte değerlendirmekle sınırlıdır. Ses değiştirme/yeniden üretme, kullanıcı değerlendirmesinden sonraki ayrı bir iştir. Sonraki numaralı UI board'u başlatılmadı.

## Aşamalar

- [x] Başlangıçtaki 16 değişmiş dosyayı ve özel oyuncu tercihlerini proje dışında yedekle.
- [x] Geçişte kullanılan atlas alanını ve kesilmenin kaynağını Unity MCP ile incele.
- [x] Mevcut tam bağımsız logoyu malzemeye bağla; yeni görsel veya runtime kodu ekleme.
- [x] Açık/koyu temada 1080×1920 ve 1080×2400 gerçek geçişleri denetle; önce/sonra kanıtını hazırla.
- [x] Geçiş düzeltmesini ve teslim sayfasını ilk tamamlanmış aşama olarak hazırla.
- [x] Gerçek aktif SFXLibrary referansından 19 rolü ve 58 kaynak dosyayı çıkar.
- [x] 58 klibin Unity importunu denetle; Unity'nin çözdüğü PCM üzerinden teknik ölçüm ve puanları hesapla.
- [x] Bağımsız ajanlarla ölçüm yöntemi, kodda kullanım ve editoryal yorum sınırlarını incele.
- [x] Ses seviyesi, mute, pause yenilemesi ve can kaybı tetiklenmesini Unity MCP ile sınayıp raporla.
- [x] Aktif ve alternatif klipler için kaynaklardan çalma kontrolleri, çok kategorili puanlar ve A/B karşılaştırma sayfasını hazırla.
- [ ] Kaynak bütünlüğü, rapor bağlantıları ve önceden değişmiş dosyaların korunmasını son kez doğrula; ses raporunu commit et.
- [ ] İşitsel estetik değerlendirme: bu oturum ses girdisini işitemediğinden tamamlanamadı. Tını, melodi, yoruculuk ve oyuna estetik uyum için puan uydurulmadı.

## Geçişte uygulanan değişiklik

Tek oyun dosyası: `Assets/_Project/Art/Materials/Mat_SwarmIrisWipe.mat`.

Eski kaynak `MainMenuNightCutout.png` menü atlası ve dar `_BrandRect` alanıydı. Bu alan genişletildiğinde atlasın ilgisiz üst HUD parçaları da görünebildiğinden, projede zaten bulunan tam bağımsız Boot logosu seçildi: `Art/_FarzetkiArts/Logos/ChatGPT Image 9 Eyl 2026 18_42_19 (3)-Photoroom.png`.

`_BrandRect` tam 0–1 alanını kullanır; kenar feather değeri 0,04'ten 0,001'e indirilmiştir. Logo genişliği 0,82 olarak korunmuştur. Boot/Home sahneleri veya kaynak logolar yeniden yazılmadı. Atlas dışındaki küçük üçgenleri taklit etmek yerine, tam karakterleri, yazıyı ve çevresindeki çizgileri içeren bağımsız sanat kullanıldı.

Native kanıt: `Captures/TransitionCrop/index.html`. İlk önce/sonra çifti gerçek Unity material render'ıdır; canlı oyun ekranı diye etiketlenmemiştir. Diğer dört görüntü üretim `SceneTransitionManager.PlayTransition` akışının gerçek kapalı ekran anıdır. Testte duraklatılmış zaman kullanılmış; devam çağrısının bir kez çalışması, tam texture alanı, overlay'in kaybolması ve input engelinin kalkması denetlenmiştir. Sonuçlar `live-checks.txt` içindedir. Bunlar fiziksel Android cihaz sınaması değildir.

## Ses raporunun kapsamı

Aktif kaynak `Assets/_Project/Data/SFXLibrary.asset` ve onu kullanan CoreManagers prefabıdır. `ScriptableObjects/SFXLibrary.asset` kopyası aktif seçim kabul edilmedi. 17 efekt + 2 müzik rolünde toplam 58 dosya vardır. Sesler, atamalar, import ayarları ve AudioManager kodu değiştirilmedi.

Teknik puan = %40 sinyal bütünlüğü + %25 seviye payı + %35 dosya sınır düzenlemesi. Oyun içi kullanım puanı ayrıca kod yollarına göre verilmiştir. Bunlar estetik puan değildir. Alternatifte yüksek teknik puan otomatik değiştirme kararı oluşturmaz.

Teslim sayfası: `Captures/AudioReview/index.html`. Tam ölçümler, Unity import envanteri, kod incelemesi, yöntem, dinleme planı ve native çalışma gözlemleri aynı klasördedir.

Unity'de gözlenenler:

- Sıfır verilen müzik/efekt seviyesi 0,7/1 varsayılana dönüyor.
- Pause müziği 0,18 iken veri/ayar yenilemesiyle 0,6 oluyor.
- Mute sırasında ButtonClick duruyor; ToggleSwitch kaynak isteği devam ediyor.
- Doğrudan yanlış-hamle can kaybı akışı iki SFX isteği oluşturuyor; kaynak incelemesi iki HeartBreak yolunu doğruluyor.

Bu kontrol, ses duyumu yerine gerçek AudioSource kazançları ve çalma isteklerini gözlemler. Cihaz miksinin veya kulağa hoş gelmenin sınandığı iddia edilmez.

## Sınırlar ve devam noktası

Yerel raporu tarayıcıda açma isteği, otomatik güvenlik denetimi tarafından `file://` adresi nedeniyle reddedildi. Engel başka sunucu/tarayıcıyla aşılmadı. HTML, gömülü veriler, filtreler, kaynak yolları ve RMS kazançları programatik kontrol edildi; tarayıcı ses oynatımı doğrulanmadı. Model bu oturumda ses girdisini işitemediğinden eleştirmen gibi dinlenmiş estetik puanlar üretilemedi.

**Sonraki adım:** ses sayfasında önce 19 rolün özetini incele; özellikle MobDie, PopupOpen, GameBGM, HeartBreak ve SkillTips için mevcut klibi alternatifleriyle dinle. Kullanıcı seçimi veya gerçek işitsel değerlendirme sonrasında sadece seçilen sesleri revize et. Kodda bulunan kullanım hatalarını düzeltmek de sonraki ayrı aşamadır. Yeni bir UI board'una geçme.
