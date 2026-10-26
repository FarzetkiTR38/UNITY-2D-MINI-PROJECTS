# Arrow Swarm — öncelikli geliştirme fikirleri

Güncel durum: **1, 2 ve 3. maddeler Quality Pass 5'te uygulandı ve Unity MCP ile doğrulandı.** [Sonuç ve kanıtlar](PROJECT_QUALITY_PASS_5_OVERVIEW.md). Aşağıdaki dayanak/kapsam metinleri ilk değerlendirmeyi korur; bu üç maddeyi yeniden başlanacak iş saymayın. Kalan 4–7. maddeler henüz uygulanmadı.

Bu liste Quality Pass 4 sırasında gerçek kaynaklar okunarak hazırlandı. Aşağıdaki işler uygulanmış kabul edilmemeli; her biri için yarar, sınır, bağımlılık ve kabul testi belirtiliyor. Yerel ayarların gereksiz skor göndermesi bu listede tekrar iş değildir: `61ca3d5` ile düzeltildi ve MCP doğrulaması geçti. Daha önce istenen leaderboard görsel tasarımları, bir tasarım seçilene kadar yalnızca önizleme kapsamındadır.

“Kaynakta doğrulanan boşluk” mevcut davranışın koddan görülebildiğini belirtir; bu belge yeni bir cihaz/MCP testi yapıldığı anlamına gelmez. “Fikir” mevcut mekanik arızası iddiası değildir. Eforlar süre taahhüdü değil, göreli kapsamdır.

| Öncelik | İş | Tür | Yarar | Kapsam |
| --- | --- | --- | --- | --- |
| 1 | Leaderboard yükleniyor / kayıtlı sonuç / çevrimdışı durumu | Kaynakta doğrulanan UX boşluğu | Oyuncu bağlantı sorununu boş veya bozuk ekran sanmaz | Orta |
| 2 | Boş sıralar ve doğrulanmamış dünya sırası için dürüst gösterim | Kaynakta doğrulanan UX boşluğu | Olmayan oyuncular ve kesin olmayan birincilik gösterilmez | Küçük–orta |
| 3 | Profil kaydının tek skor eşitlemesine dönüşmesi | Kaynakta doğrulanan yinelenen çağrı adayı | Gereksiz istek ve çakışan güncelleme azalır | Orta |
| 4 | Color Shift için renk yanında eşleşme simgeleri | Erişilebilirlik fikri | Renk ayrımı zor olan oyuncular da duvar kuralını okuyabilir | Orta |
| 5 | Dekoratif hareketi azaltma seçeneği | Erişilebilirlik fikri | Fazla hareket istemeyen oyuncuya sakin sunum sağlar | Orta |
| 6 | Tüm pencerelerde tutarlı Geri / Escape davranışı | Kaynakta doğrulanan gezinme tutarsızlığı adayı | Aynı düğme her yerde beklenen tek işi yapar | Orta |
| 7 | Shapes editöründe bağımlılık ve seçim haritası | İçerik üretim fikri | Belirgin şekil ile gerçek puzzle zorluğu birlikte ayarlanır | Orta |

## 1. Leaderboard bağlantı ve yükleme durumu

**Dayanak:** Panel açılışı hemen yerel/kayıtlı satırları gösteriyor ve ardından bir yenileme başlatıyor. Yenileme hatası Console uyarısı üretiyor; panelin abone olduğu olay yalnızca tamamlanma bildiriyor. Panelde yükleniyor, son başarılı güncelleme veya çevrimdışı durum alanı yok. Başka bir yenileme sürerken çağrı mevcut satırları dönüyor. Bu gözlem sağlayıcının sonsuza kadar takıldığını kanıtlamaz.

**Öneri ve sınır:** Mevcut panelde TMP ile küçük bir durum alanı ve Yeniden Dene eylemi. Durumlar: ilk yükleme, güncel sonuç, kayıtlı sonuç + son başarılı yenileme zamanı, bağlantı kurulamadı. Mevcut Console uyarısını gizleme. Açılışı ağ yanıtına bağlama; yerel bilgi anında kalsın. Sağlayıcının kendi zaman aşımı davranışı ayrıca incelensin; sınırlı bekleme eklenirse geç gelen yanıtların yeni isteği ezmesini önleyen istek kimliğiyle birlikte tasarlansın.

**Bağımlılık/kabul:** Manager durum olayı, cache zaman damgası, Inspector üzerinden Light/Dark görünümü ve on dilde metin. Kontrollü gecikme/hata/çevrimdışı/yeniden açma senaryolarında gerçek panel test edilsin; tek aktif yenileme ve eski pending/version/reconnect testleri korunmalı. Gerçek hesaba test skoru gönderilmemeli.

Kaynaklar: [Panel açılışı](C:/Users/Farzetki/Desktop/GitHub/UNITY-2D-MINI-PROJECTS/ArrowSwarm/Assets/_Project/Scripts/UI/LeaderboardUI.cs:160), [yenileme ve hata akışı](C:/Users/Farzetki/Desktop/GitHub/UNITY-2D-MINI-PROJECTS/ArrowSwarm/Assets/_Project/Scripts/Data/LeaderboardManager.Refresh.cs:13), [cache kaydı](C:/Users/Farzetki/Desktop/GitHub/UNITY-2D-MINI-PROJECTS/ArrowSwarm/Assets/_Project/Scripts/Data/LeaderboardManager.cs:66).

## 2. Boş sıra ile gerçek oyuncuyu; yerel sıra ile dünya sırasını ayırma

**Dayanak:** Veri eksik olsa da on satır etkin kalıyor; eksik satırlar boş isim, Lv.1 ve 0 yıldızla dolduruluyor. Yetkili sıra alınamadığında `GetPlayerRank()` mevcut yerel/cache listesindeki konumu döndürüyor. Cache olmayan çevrimdışı ilk açılışta listede yalnızca oyuncu varsa bu değer 1 olur; küresel birincilik doğrulanmış değildir. Ana liste de yerel oyuncuyu cache ile birleştirerek sıralıyor.

**Öneri ve sınır:** Sonuç olmayan satırları gizle veya açık “Henüz sonuç yok” durumu göster. Yetkili dünya sırası bilinmiyorsa “—” / “Sıra çevrimiçi güncellenir” kullan; cache sırası gösterilecekse kayıtlı olduğunu belirt. Gerçek oyuncunun yıldızı ve ilerlemesi görünmeye devam etsin. Bu iş puan formülünü veya mevcut level/yıldız sıralama önceliğini değiştirmemeli. Yeni leaderboard düzeni tasarım seçimine bağlıdır; içerik doğruluğu ayrı bir iş olarak ele alınabilir.

**Bağımlılık/kabul:** 1. işin durum modeliyle ortak veri kaynağı, mevcut satır şablonları, yerelleştirme. Sıfır global satır, bir satır, on satır, yetkili sıra bilinmiyor ve oyuncu ilk on dışında durumları fake sağlayıcıyla test et. Eksik satır gerçek hesap gibi; yerel sıra dünya sırası gibi sunulmamalı.

Kaynaklar: [Boş satır üretimi](C:/Users/Farzetki/Desktop/GitHub/UNITY-2D-MINI-PROJECTS/ArrowSwarm/Assets/_Project/Scripts/UI/LeaderboardUI.cs:243), [sıra geri dönüşü](C:/Users/Farzetki/Desktop/GitHub/UNITY-2D-MINI-PROJECTS/ArrowSwarm/Assets/_Project/Scripts/Data/LeaderboardManager.cs:140), [yerel/cache birleştirme](C:/Users/Farzetki/Desktop/GitHub/UNITY-2D-MINI-PROJECTS/ArrowSwarm/Assets/_Project/Scripts/Data/LeaderboardManager.Entries.cs:10).

## 3. Profil kaydında yinelenen skor isteklerini birleştirme

**Dayanak:** Profil Kaydet önce `SetPlayerProfile()` çağırıyor, sonra `LeaderboardManager.SetPlayerName()` çağırıyor. İkinci yol tekrar `DataManager.SetPlayerName()` üzerinden skor gönderebiliyor ve ayrıca açık eşitleme istiyor. Bunlar aynı profil/puan içeriğini birden fazla kez gönderebilen ayrı yollar. Ayar düzeltmesi gerçek profil eşitlemesini bilerek korudu; bu çağrı zinciri o düzeltmenin kapsamına alınmadı. Kesin istek sayısı, başlangıç ve sağlayıcı gecikmesine göre ayrıca ölçülmeli.

**Öneri ve sınır:** Tek profil-kaydet işlemi için tek eşitleme sorumlusu belirle. Aynı etkin profil/puan isteğine katılmayı veya yinelenen açık çağrıyı kaldırmayı değerlendirmeden önce fake ile gerçek Kaydet düğmesini ölç. Daha yeni isim/ülke/yıldızın gönderilmesini, başarısız işlemin yeniden denenmesini ve pending bayrağını koru; yalnızca aynı içeriğe sahip olmayı başarılı eşitleme sayma.

**Bağımlılık/kabul:** Editor-only fake seam hazır. Gerçek modal Save üzerinden önce/sonra istek sayısı; anında ve gecikmiş yanıt; aynı anda profil ile yıldız değişimi; başarısız eski istek ve başarılı yeni istek; yeniden bağlanma testleri. Değişiklik mutlaka ayrı commit ve bütün eski bulut regresyonlarıyla yapılmalı.

Kaynaklar: [Profil Save çağrıları](C:/Users/Farzetki/Desktop/GitHub/UNITY-2D-MINI-PROJECTS/ArrowSwarm/Assets/_Project/Scripts/UI/ProfileSetupModalUI.cs:331), [isim kaydı ve açık eşitleme](C:/Users/Farzetki/Desktop/GitHub/UNITY-2D-MINI-PROJECTS/ArrowSwarm/Assets/_Project/Scripts/Data/LeaderboardManager.cs:113), [eşitleme anlık görüntüsü](C:/Users/Farzetki/Desktop/GitHub/UNITY-2D-MINI-PROJECTS/ArrowSwarm/Assets/_Project/Scripts/Data/LeaderboardManager.Refresh.cs:51), [sürüm koruması](C:/Users/Farzetki/Desktop/GitHub/UNITY-2D-MINI-PROJECTS/ArrowSwarm/Assets/_Project/Scripts/Data/LeaderboardManager.Pending.cs:10).

## 4. Color Shift eşleşmesini yalnızca renge bağlı bırakmama

**Dayanak:** Mantık zaten görünür renkten ayrı `ColorId` ile duvar eşleşmesini hesaplıyor. Oklar ve duvarlar ortak `ThemeColor()` üzerinden çiziliyor; bu iyi ayrım, kuralları değiştirmeden ek bir görsel ipucu eklemeye uygun. Şu anki sunum ok/duvar kimliğini renk ile iletiyor. Bu bir çözülmezlik hatası iddiası değildir.

**Öneri ve sınır:** İsteğe bağlı erişilebilirlik ayarında iki renge iki farklı küçük simge veya desen ata. Örneğin bir kimlikte nokta, diğerinde kısa çizgi; aynı işaret oka ve eşleşen duvar bölgesine uygulanır. Desen kimliği boya akışıyla birlikte taşınır, köşelerde kopmaz. Varsayılan sanat ve iki renkli MVP kuralı korunur; yeni renk/mekanik eklenmez.

**Bağımlılık/kabul:** Inspector-authored desen/simge materyalleri, kayıtlı seçenek, ayar satırı ve yerelleştirme. Pattern açık/kapalıyken aynı solver sonucu, can, dönüş bütçesi ve collider davranışı; Light/Dark, yakınlaştırma ve hareketli çerçevede okunurluk kontrolü. Gerçek renk görme farklılıkları için kullanıcı testi ayrıca gerekir; gri tonlu görüntü tek başına sertifika sayılmaz.

Kaynaklar: [Kimlik tabanlı duvar kuralı](C:/Users/Farzetki/Desktop/GitHub/UNITY-2D-MINI-PROJECTS/ArrowSwarm/Assets/_Project/Scripts/Modes/PuzzleRules.cs:30), [ok teması](C:/Users/Farzetki/Desktop/GitHub/UNITY-2D-MINI-PROJECTS/ArrowSwarm/Assets/_Project/Scripts/Puzzle/PuzzleArrowView.cs:37), [duvar boyası](C:/Users/Farzetki/Desktop/GitHub/UNITY-2D-MINI-PROJECTS/ArrowSwarm/Assets/_Project/Scripts/Puzzle/PuzzleFrame.Paint.cs:14), [ortak palet dönüşümü](C:/Users/Farzetki/Desktop/GitHub/UNITY-2D-MINI-PROJECTS/ArrowSwarm/Assets/_Project/Scripts/Modes/PuzzleConfig.cs:55).

## 5. Dekoratif hareketi azaltma

**Dayanak:** Butonlarda isteğe bağlı sürekli nefes alma ve basma/bırakma zıplaması; pencerelerde esnek ölçek animasyonu; sahne geçişlerinde shader efektleri mevcut. Mevcut VFX seçeneği ayrı; bu bileşenlerde ortak “hareketi azalt” tercihi okunmuyor.

**Öneri ve sınır:** Ayarlara ayrı bir seçenek: sürekli dekoratif nefes almayı kapat, pencere zıplamasını sade bir fade'e indir, sahne geçişi için sakin Inspector-authored varyant seç. Basılı/etkin/pasif buton geri bildirimi kalsın. Ok uçuşu, çarpışma, enemy hızı, kanat döngüsü, dönüş hakkı veya bulmaca zamanlaması bu ilk işin kapsamına alınmasın; oyunun kuralları aynı kalsın.

**Bağımlılık/kabul:** Eski save'ler için varsayılan kapalı alan, tek ayar olayı ve sahne/prefab üzerinden iki sunum varyantı. Pause sırasında unscaled animasyonların bitmesi, popup tekrar açılışı, basılı düğmenin kapanması ve geçişin input kilidini bırakması iki ayarda da test edilsin. CPU/pil kazancı ancak hedef cihazda ölçülürse raporlansın.

Kaynaklar: [Buton nefes ve ölçek davranışı](C:/Users/Farzetki/Desktop/GitHub/UNITY-2D-MINI-PROJECTS/ArrowSwarm/Assets/_Project/Scripts/UI/UIButtonFeedback.cs:18), [pencere animasyonu](C:/Users/Farzetki/Desktop/GitHub/UNITY-2D-MINI-PROJECTS/ArrowSwarm/Assets/_Project/Scripts/UI/UIPopupAnimator.cs:19), [sahne geçiş stilleri](C:/Users/Farzetki/Desktop/GitHub/UNITY-2D-MINI-PROJECTS/ArrowSwarm/Assets/_Project/Scripts/Core/SceneTransitionManager.cs:15), [mevcut kayıtlı ayarlar](C:/Users/Farzetki/Desktop/GitHub/UNITY-2D-MINI-PROJECTS/ArrowSwarm/Assets/_Project/Scripts/Data/PlayerData.cs:40).

## 6. Tek ve tutarlı Geri davranışı

**Dayanak:** Mode seçici kendi `Keyboard.escapeKey` kontrolünü yapıyor. Leaderboard, Settings ve Pause kendi kapatma düğmelerini bağlıyor; taranan runtime kaynaklarında bu pencereleri kapsayan ortak bir geri yönlendiricisi bulunmadı. Bu, Android cihazındaki sistem Geri tuşunun tüm davranışlarının test edildiği anlamına gelmez.

**Öneri ve sınır:** Önce en üst açık pencereyi kapatan ortak bir geri eylemi. Aktif oyunda önce Pause; modal varken alttaki oyunu etkileme; sahne geçişi sırasında yeni gezinme başlatma. Profile, zorunlu tutorial ve reklam ekranları için izin verilen davranış ayrı tanımlanmalı. Çıkış onayı eklemek ilk kapsamın zorunlu parçası değildir.

**Bağımlılık/kabul:** New Input System action, mevcut Show/Hide/Resume API'lerine bağlanan küçük modal kayıt mekanizması, sahne temizliği. Gerçek Escape input olayıyla üst üste pencereler, art arda hızlı geri, açık/kapalı animasyonu, Time.timeScale=0 ve sahne değişimi; ayrıca Android cihazda sistem Geri testi. Bir input yalnızca tek pencereyi kapatmalı ve görünmez raycast engeli bırakmamalı.

Kaynaklar: [Mode seçicinin özel Escape kontrolü](C:/Users/Farzetki/Desktop/GitHub/UNITY-2D-MINI-PROJECTS/ArrowSwarm/Assets/_Project/Scripts/UI/GameModeSelectUI.cs:48), [leaderboard düğme bağlantıları](C:/Users/Farzetki/Desktop/GitHub/UNITY-2D-MINI-PROJECTS/ArrowSwarm/Assets/_Project/Scripts/UI/LeaderboardUI.cs:59), [Pause sunumu](C:/Users/Farzetki/Desktop/GitHub/UNITY-2D-MINI-PROJECTS/ArrowSwarm/Assets/_Project/Scripts/UI/PauseMenuUI.cs:199).

## 7. Shapes editöründe görsel zorluk teşhisi

**Dayanak:** Mevcut ölçümler ilk çıkabilen ok oranını, bağımlılık katmanlarını, başlangıç/orta seçim alanını ve bölgeler arası bağımlılığı hesaplıyor. Draft editörünün doğrulama mesajı ise solvable, ok sayısı, çözüm adımı ve maskenin tamamlığını gösteriyor; önizleme esas olarak yol rengini ve seçilen oku çiziyor. Üretimde zaten kullanılan ölçümleri editöre taşımak, yeni bir zorluk sistemi icat etmekten daha somut bir geliştirme.

**Öneri ve sınır:** Draft için üç isteğe bağlı görünüm: normal renkler; ilk legal oklar; seçili oku engelleyen/ona bağlı oklar. Yan panelde başlangıç/orta seçim oranı ve katman genişlikleri. Bir ok seçildiğinde engelleyici zinciri görünür; “neden kolay/zor?” sorusu harita üstünden anlaşılır. Maske kapsamını, doğal boynu ve solvability kontrolünü atlayarak kaydetmeye izin verilmez. Varsayılan kampanya asset'leri veya oyuncuya gösterilen zorluk otomatik değiştirilmez.

**Bağımlılık/kabul:** Mevcut Editor-only `ShapesDifficultyMetrics`, draft/Undo akışı ve solver; ek runtime sistem gerekmez. Kolay ve zor iki bilinen fixture'da vurgular `PuzzleRules.CanRemove()` ile karşılaştırılsın; Undo/Redo ve iptal kaynak asset'i değiştirmesin. İlk okunabilirlik kontrolü gerçek gameplay ekran görüntüsüyle yapılsın. Bu metrikler insanın algıladığı zorluğun kesin ölçüsü olarak sunulmamalı.

Kaynaklar: [Zorluk ölçümleri](C:/Users/Farzetki/Desktop/GitHub/UNITY-2D-MINI-PROJECTS/ArrowSwarm/Assets/_Project/Scripts/Modes/Editor/ShapesDifficultyMetrics.cs:12), [draft doğrulaması](C:/Users/Farzetki/Desktop/GitHub/UNITY-2D-MINI-PROJECTS/ArrowSwarm/Assets/_Project/Scripts/Modes/Editor/ShapeLevelEditorWindow.Validation.cs:32), [mevcut önizleme](C:/Users/Farzetki/Desktop/GitHub/UNITY-2D-MINI-PROJECTS/ArrowSwarm/Assets/_Project/Scripts/Modes/Editor/ShapeLevelEditorWindow.Preview.cs:9), [çözüm tanığı](C:/Users/Farzetki/Desktop/GitHub/UNITY-2D-MINI-PROJECTS/ArrowSwarm/Assets/_Project/Scripts/Modes/PuzzleSolver.cs:21).

## Uygulama sırası ve devam kuralı

Önce mevcut Quality Pass 4 doğrulanmış hata düzeltmeleri ve entegrasyon testleri bitirilmeli. Ardından 1–2 birlikte tasarlanabilir; 3 bağımsız bir istek sayısı fixture'ıyla ele alınmalı. 4 ve 5 ortak erişilebilirlik ayarı altyapısını paylaşabilir, ancak ayrı kabul testleri ve commit'leri olmalı. 6 bağımsız gezinme işi; 7 oyuncu kayıtlarına dokunmayan içerik üretim işi olarak yürütülebilir.

Bu belge yeni bir işin tamamlandığına veya canlı servislerin doğrulandığına dair kayıt değildir. Seçilen her iş için önce yeniden üretim/ölçüm, sonra küçük uygulama, Unity MCP doğrulaması, exact state restoration ve commit kaydı ana yol haritasına eklenmeli.
