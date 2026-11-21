# ArrowSwarm · Oyuncu deneyimi yol haritası

19 Kasım 2026 · Plan kaydedildi. Uygulama, temel ölçüm ve oyuncu testleri bekliyor.

İlk hedef, yeni oyuncunun ek yardım almadan doğru bir ok göndermesi, bir hata yaptığında sebebini anlaması ve ilk galibiyetten sonra kendi isteğiyle ikinci bölüme geçmesi. Kapalı testte bu akışı ölç. Ardından Savunma’daki zamanlamanın değerini ve üç modun ayrı ayrı tekrar oynanma nedenini doğrula.

## 0 · Kapalı test için başlangıç ölçümü ve yayın adayı

- [ ] Play’den kurulabilen aday derlemenin sürüm kodunu ve kaynak commit’ini kaydet. Portre donanım bildirimi düzeltmesini birleştirilmiş manifestte ve Play Console’da doğrula. Dar bir Android telefon ve uzun ekranlı bir telefonda temiz açılış, duraklatma, arka plana alma ve geri dönme akışlarını dene. Erişilebilir cihaz varsa tablet düzenini de kontrol et. Sorunları cihaz ve derleme bilgisiyle kaydet. [Yayın kontrol listesi](../ArrowSwarm_Marketing/10_Launch/GO_NO_GO.md)
- [ ] İlk testte projeyi bilmeyen 5–8 oyuncuya kurulu oyunu ver. Yardım etmeden ilk oturumu izle. İlk açılış, ilk oynanabilir dokunuş, ilk doğru ok, öğreticiyi bitirme veya atlama, ilk zafer, ikinci bölüm başlangıcı ve oturumdan çıkışı zamanla. Yanlış hamlede oyuncunun dokunmak istediği oku ve hataya verdiği açıklamayı not et. Kayıt varsa oyuncunun iznini al.
- [ ] İlk grupla mümkünse ilk oturumda geçiş reklamsız bir başlangıç ölçümü al. Reklam izin akışını, doldurulamayan reklamı ve ödül dönüşünü ayrı cihaz senaryolarında doğrula. Üretime hazır olma kararı güncel AAB, cihaz kanıtı ve Play Console durumuyla verilir. [Yayın engelleri](../ArrowSwarm_Marketing/10_Launch/GO_NO_GO.md)

**Kabul ölçütü.** Her gözlemde cihaz, dil, derleme, reklam ayarı ve katılımcı sayısı var. Küçük grup sonuçları sorun bulmak için kullanılıyor. D1 ve D7, erişilebilir ve izinli gerçek oyuncu verisi olduğunda olgunlaşmış gruplarda hesaplanıyor. [Ölçüm tanımları](../ArrowSwarm_Marketing/09_Analytics/MEASUREMENT_GUIDE.md)

## 1 · Açılış, ilk galibiyet ve ikinci bölüm

- [ ] Boot’taki 2,4 saniyelik dolum ve ardından gelen 0,25 saniyelik beklemeyi gerçek sahne yüklemesine bağla. Hazır olan sahneyi yapay beklemeye sokan döngüyü kaldır. Soğuk açılış ve menüden bölüme dönüşü ayrı ölç. [BootLoadingUI.cs](../Assets/_Project/Scripts/UI/BootLoadingUI.cs#L147) · [BootLoader.cs](../Assets/_Project/Scripts/Core/BootLoader.cs#L47)
- [ ] Öğretici zaferinde birincil eylemi **Sonraki Bölüm** yap, **Ana Menü** seçeneğini erişilebilir tut. Öğreticiyi atlamak ikinci bölümü açabilsin. Üç yıldız yalnız oynanmış bir başarıdan gelsin. Eski kayıtlar için kazanılmış ilerleme veya yıldızı geriye dönük değiştirme. [TutorialManager.cs](../Assets/_Project/Scripts/Tutorial/TutorialManager.cs#L254)
- [ ] İlk menü girişindeki profil penceresini, oyuncu sıralamaya katılmak ya da profilini düzenlemek istediği ana taşı. Ülke ve dil etkileşimini ayrı değerlendir. Ülke kaydı, oyuncunun açıkça seçtiği dili değiştirmesin. Dil tercihi ve anonim çevrimiçi kimlik akışını ayrı test et. [MainMenuUI.cs](../Assets/_Project/Scripts/UI/MainMenuUI.cs#L95) · [ProfileSetupModalUI.cs](../Assets/_Project/Scripts/UI/ProfileSetupModalUI.cs#L357)
- [ ] Savunma öğreticisini ortak TMP yerelleştirmesine taşı. İlk üç bölümde açık yol, engellenmiş ok ve düşmana göre atış zamanlamasını kısa, oynanabilir örneklerle öğret. Diğer modların özel kurallarını ilk girişlerinde öğret. [TutorialManager.cs](../Assets/_Project/Scripts/Tutorial/TutorialManager.cs#L337)

**Kabul ölçütü.** Temiz kurulumda gerçek dokunuşla oynayıp bitirme ve atlama ayrı ayrı geçer. İkinci bölüm tek eylemle açılır ve profil formu araya girmez. Yerelleştirme on dilde, iki temada ve dar/uzun ekranda okunur. Aynı protokolle yeni oyuncularda ilk hamle süresini ve ikinci bölüme geçişi başlangıçla karşılaştır.

## 2 · Oyun tahtasının okunurluğu, dokunma ve hata geri bildirimi

- [ ] Şekiller’de başlangıç ve yoğun tahtalarda ok ucu, gövde ve arka plan kontrastını iyileştir. HUD ile tahta arasındaki görsel ağırlığı ve Savunma düşman sayılarının küçük ekrandaki okunurluğunu düzenle. Kaydedilen örneklerde küçük hedefleri gerçek parmakla dene. [Unity mağaza çekimleri](../ArrowSwarm_Marketing/03_GooglePlay/StoreSet_TR_20261118/overview_TR.jpg)
- [ ] Yakın iki ok üzerinde amaçlanan ve hareket eden oku karşılaştır. Dokunma önceliğini ölçülen yanlış seçimlere göre düzelt. Gerekirse yakınlaştırma, yenilenen ok hedeflerinden sonra ayrı bir deney olarak değerlendirilir. [PuzzleSession.Actions.cs](../Assets/_Project/Scripts/Puzzle/PuzzleSession.Actions.cs#L13)
- [ ] Yanlış hamlede çarpılan oku, yanlış renk kenarını ve Savunma’da düşmanın bitişe ulaşmasını birbirinden ayırt edilebilir göster. Renk Değişimi’nde eşleşen ok ve duvar için ortak simge/desen dene. İlk tahtada dönüşün hangi okun yolunu açtığını göster. Sağa ve sola dönüş aynı mantıksal duruma gidiyor ve bir hak tüketiyor. Oyuncu bu kuralı kontrolü gördüğünde anlayabilmeli. [PuzzleRules.cs](../Assets/_Project/Scripts/Modes/PuzzleRules.cs#L28)
- [ ] Dönüş hakkı tükenince panelin sebebi ve mevcut seçenekleri açıkça anlattığını doğrula. Roket yeterliyse hangi hamlenin mümkün olduğunu, bölüm tıkandıysa ücretsiz tekrar seçeneğini göster. Son canın kaybında çarpma ve geri dönüş bitince sonuç açılmalı.

**Kabul ölçütü.** Oyuncu yardım almadan canı niye kaybettiğini ve sonraki geçerli eylemi açıklayabiliyor. Dar telefonda yan yana ok testlerinde yanlış seçimler başlangıç ölçümünden düşük. Renk eşleşmesi gri tonlu görüntüde de okunuyor. Mevcut ipucu, roket, dondurma, son can, tekrar dene ve duraklatma senaryoları geçiyor. Oynanış kuralı veya girdi değişikliklerinde etkilenen kampanyaların mevcut çözülebilirlik ve Unity oynanış kontrollerini tekrar çalıştır.

## 3 · Savunma dengesi, zorluk ve reklam ritmi

- [ ] Aynı erken ve orta Savunma tahtalarını iki yöntemle karşılaştır. İlk yöntemde her açık oku hemen gönder, ikincide düşman konumuna ve atış zamanına göre oyna. Zaferi, kalan canı, gerçek mermi isabetini, bitişe ulaşan düşmanı, final temizliğini ve Dondur kullanımını kaydet. [LevelManager.cs](../Assets/_Project/Scripts/Core/LevelManager.cs#L270)
- [ ] Test sonuçlarına göre en küçük denge değişikliğini seç. İyi zamanlama zaten fark yaratıyorsa bunu sahada daha görünür kıl. Hızlı boşaltma üstün geliyorsa önce zamanlamaya bağlı görünür ustalık hedefini dene. Ana kazanma kuralı için prototip gerekirse eski ve yeni kuralla aynı tahtaları karşılaştır. Gökkuşağı finalini, son can ile son ok yarışını ve kayıt ilerlemesini doğrula.
- [ ] Üç modun ilk beş bölümünde süre, tekrar ve çıkış nedenlerine bak. Tek bir seviyedeki büyük zorluk sıçramasını önce düzenle. Şekiller’de tıkanmanın çizgi yoğunluğundan mı, gerçek planlama zorluğundan mı geldiğini ayırt et. Renk Değişimi’nde ilk dönüş kullanımını ve sıfır hakla kaybı gözle.
- [ ] İlk birkaç galibiyette geçiş reklamı ertelemesini dene. Mevcut ayar 4. bölüm ve 90 saniye. Reklam koşulları değiştiğinde galibiyetten sonraki bölüme geçişi, oturumdan çıkışı ve yeniden denemeyi karşılaştır. Ödüllü akışta ret, başarısız yükleme, çevrimdışı durum ve tek ödül senaryolarını test et. [CoreManagers.prefab](../Assets/_Project/Prefabs/Core/CoreManagers.prefab#L375)

**Kabul ölçütü.** Oyuncu ne zaman ateş ettiğinin sonucunu görebiliyor. Planlı atış ile hızlı boşaltma karşılaştırmasının sonucu kaydedilmiş. Yeni reklam ritmi seçilirse seçim, sonraki bölüme geçiş ve gerçek cihazdaki izin/ödül akışıyla gerekçelendiriliyor.

## 4 · Sıralama ve mağaza vaadi

- [ ] Canlı sıralamadaki `Savunma en yüksek bölüm × 10.000 + toplam yıldız` ağırlığını kullanıcıya açıkla. İlk seçenek listeyi **Savunma Sıralaması** diye adlandırmak. Ayrı mod tablolarına, her modda anlamlı bir rekabet talebi ve skor yönetimi varsa geç. Yardımlı/yardımsız sonuçların kapsamını tanımla. Formül değişikliği seçilirse eski skoru koruyan sürümleme, geçiş ve geri dönüş planını ayrı kaydet. [UnityCloudService.cs](../Assets/_Project/Scripts/Data/UnityCloudService.cs#L129)
- [ ] Aynı mağaza metniyle ilk görseldeki iki adayı karşılaştır. Bir aday Savunma’da okun sürüyü vurduğu anı, diğeri tanınan bir Şekiller siluetini gösterir. Mevcut menü görselini karşılaştırma için sakla. İlk görseli gören kişiye oyunda ne yapacağını sor. Mağazada yeterli trafik oluşursa aynı dil ve dönemle Play Console dönüşümünü karşılaştır. [Mağaza stratejisi](../ArrowSwarm_Marketing/07_ASO_SEO_GEO/ASO_STRATEGY.md)

**Kabul ölçütü.** Oyuncu yıldızın ve bölümün sırasına etkisini anlıyor. Sıralamanın boş, gecikmeli, çevrimdışı ve yeniden bağlanma durumları önceki testleri geçiyor. Yeni mağaza görseli, oyunun kuralını ilk bakışta daha iyi anlatıyor ve gerçek oynanışla eşleşiyor.

## 5 · İlk içeriğe dönme nedeni

- [ ] İlk oturum akışı iyileştikten sonra **Günün Bulmacası** prototipini mevcut seviyelerden gün ve mod bazında seçilen tek bir tahta ile dene. Herkese aynı tahta ve oyuncunun kendi yıldız kaydı ilk deneme için yeterli. Mevcut kampanya kilidi, kaydı ve yıldız hesabıyla etkileşimi doğrula.
- [ ] Günlük tahtaya girenlerin tamamlama oranını, D1/D7 dönüşünü ve kampanyaya devamını ölç. Oyuncu verisi talebi gösterirse özel günlük tahtaları değerlendir. Dördüncü mod, seri ödülleri, bildirime dayalı hatırlatmalar ve geniş seviye üretimi bu ölçümden sonra değerlendirilir.

## Devam noktası

**Sıradaki ilk iş.** Faz 0’da mevcut aday AAB’nin derleme kimliğini kaydet ve temiz Android kurulumunda açılıştan öğretici zaferi, profil penceresi ve ikinci bölüme kadar başlangıç videosu al. Kayıtta süreleri, doğrudan ilerlemeyi engelleyen adımları ve cihaz bilgisini işaretle. Ardından Faz 1’in ilk küçük değişikliğiyle karşılaştır.

Her aşama için bulgu, önce/sonra kanıtı, doğrulama sonucu ve commit kaydını bu dosyaya ekle. Unity MCP ile gerçek kontrolleri ve önceki regresyonları dene. Cihaz ve insan gözlemlerini Editör ve çözücü sonuçlarından ayrı kaydet. Oyuncu kayıtlarını, tercihleri ve açık yerel değişiklikleri koru. Onaylı UI tasarım sırası kendi [UI yol haritasından](UI_REDESIGN_ROLLOUT.md) izlenir.
