# ArrowSwarm · Oyuncu deneyimi yol haritası

20 Kasım 2026 · Açılış, öğretici ve geri bildirim Unity’de doğrulandı. Fiziksel Android ve oyuncu testleri bekliyor.

İlk hedef, yeni oyuncunun ek yardım almadan doğru bir ok göndermesi, bir hata yaptığında sebebini anlaması ve ilk galibiyetten sonra kendi isteğiyle ikinci bölüme geçmesi. Kapalı testte bu akışı ölç. Ardından Savunma’daki zamanlamanın değerini ve üç modun ayrı ayrı tekrar oynanma nedenini doğrula.

## 0 · Kapalı test için başlangıç ölçümü ve yayın adayı

- [ ] Play’den kurulabilen aday derlemenin sürüm kodunu ve kaynak commit’ini kaydet. Portre donanım bildirimi düzeltmesini birleştirilmiş manifestte ve Play Console’da doğrula. Dar bir Android telefon ve uzun ekranlı bir telefonda temiz açılış, duraklatma, arka plana alma ve geri dönme akışlarını dene. Erişilebilir cihaz varsa tablet düzenini de kontrol et. Sorunları cihaz ve derleme bilgisiyle kaydet. [Yayın kontrol listesi](../ArrowSwarm_Marketing/10_Launch/GO_NO_GO.md)
  - [x] Yerel imzalı aday `Builds/ArrowSwarm 1.0.0 (2) Boot check.aab`. Paket `com.farzetkigames.arrowswarm`, kod 2, kaynak `6954e74f` ve mevcut yerel Android Player Settings. SHA-256 `D3C724AA341C463ED12E3D6E1D81EBA2A244E04A9BD134D1040A83D3F16AD3BA`. Unity IL2CPP derlemesi ve bundletool geçerli. Bir etkin ve dışa açık GameActivity giriş noktası, portre yönü, isteğe bağlı portre donanımı ve AD_ID bildirimi doğrulandı. İmza önceki yükleme anahtarıyla eşleşiyor. Play Console ve cihaz kurulum doğrulaması açık.
- [ ] İlk testte projeyi bilmeyen 5–8 oyuncuya kurulu oyunu ver. Yardım etmeden ilk oturumu izle. İlk açılış, ilk oynanabilir dokunuş, ilk doğru ok, öğreticiyi bitirme veya atlama, ilk zafer, ikinci bölüm başlangıcı ve oturumdan çıkışı zamanla. Yanlış hamlede oyuncunun dokunmak istediği oku ve hataya verdiği açıklamayı not et. Kayıt varsa oyuncunun iznini al.
- [ ] İlk grupla mümkünse ilk oturumda geçiş reklamsız bir başlangıç ölçümü al. Reklam izin akışını, doldurulamayan reklamı ve ödül dönüşünü ayrı cihaz senaryolarında doğrula. Üretime hazır olma kararı güncel AAB, cihaz kanıtı ve Play Console durumuyla verilir. [Yayın engelleri](../ArrowSwarm_Marketing/10_Launch/GO_NO_GO.md)

**Kabul ölçütü.** Her gözlemde cihaz, dil, derleme, reklam ayarı ve katılımcı sayısı var. Küçük grup sonuçları sorun bulmak için kullanılıyor. D1 ve D7, erişilebilir ve izinli gerçek oyuncu verisi olduğunda olgunlaşmış gruplarda hesaplanıyor. [Ölçüm tanımları](../ArrowSwarm_Marketing/09_Analytics/MEASUREMENT_GUIDE.md)

## 1 · Açılış, ilk galibiyet ve ikinci bölüm

- [x] Boot göstergesini gerçek sahne yüklemesine bağla ve hazır sahneyi bekleten zamanlayıcıyı kaldır. `700cc1c7` ile Unity MCP Play Mode karşılaştırmasında ana menü 3,967 → 2,079 sn, öğretici 5,239 → 1,975 sn. İki rotada da tek sahne aktivasyonu, gerilemeden %100’e ulaşan gösterge ve sıfır konsol hatası kaydedildi. [Unity öncesi/sonrası](http://127.0.0.1:8200/#%2Fcaptures%2FPlayerExperience%2Findex.html)
- [ ] Android cihazda soğuk açılışı ve menüden bölüme dönüşü ayrı ölç.
- [x] Öğretici zaferinde **Sonraki Bölüm** ve **Ana Menü** seçenekleri var. Unity’de gerçek giriş olaylarıyla tamamlanan öğretici üç yıldız verdi ve Sonraki Bölüm doğrudan ikinci Savunma bölümünü açtı. Atla temiz kayıtta ikinci bölümü sıfır yıldızla açtı ve eski kayıttaki iki yıldızı korudu. Ana Menü seçeneği kazanılan yıldızları ve ikinci bölüm erişimini korudu. `d3008c9f` · [Unity öncesi/sonrası ve test kayıtları](http://127.0.0.1:8200/#%2Fcaptures%2FPlayerExperience%2Findex.html)
- [x] İlk ana menü açılışı profil formu göstermiyor. İlk Lider Tablosu tıklaması formu açıyor ve kayıt sonrasında tabloya devam ediyor. Profil simgesi de formu açmaya devam ediyor. Unity’de ülkeyi ABD’den Türkiye’ye değiştirmek ve kaydetmek İngilizce dilini korudu. Dil kaydını değiştirmek de ülkeyi korudu. `aa13ade0`
- [ ] Anonim çevrimiçi kimlik akışını gerçek test ortamında doğrula. Editör gezinme testi yalıtılmış çevrimdışı sağlayıcı ile yapıldı.
- [x] Savunma öğreticisinde açık yol, ikinci bölümde engellenmiş ok ve üçüncü bölümde düşmana göre atış zamanlaması anlatılıyor. Şekiller ve Renk Değişimi ilk tahtalarında kendi kurallarını gösteriyor. Unity’de ikinci bölümde engelli ve doğru atış, üçüncü bölümde hasar bildirimi, Şekiller’de doğru atış ve Renk Değişimi’nde dönüş kontrol edildi. Tamamlama kartı 10 dil ve iki temada 20/20 geçti. Gerçek kullanıcı ölçümü açık. `f03dbf39` · [Sahne görüntüleri ve Unity testleri](http://127.0.0.1:8200/#%2Fcaptures%2FPlayerExperience%2Findex.html)

**Kabul ölçütü.** Temiz kurulumda gerçek dokunuşla oynayıp bitirme ve atlama ayrı ayrı geçer. İkinci bölüm tek eylemle açılır ve profil formu araya girmez. Yerelleştirme on dilde, iki temada ve dar/uzun ekranda okunur. Aynı protokolle yeni oyuncularda ilk hamle süresini ve ikinci bölüme geçişi başlangıçla karşılaştır.

## 2 · Oyun tahtasının okunurluğu, dokunma ve hata geri bildirimi

- [ ] Şekiller’de başlangıç ve yoğun tahtalarda ok ucu, gövde ve arka plan kontrastını iyileştir. HUD ile tahta arasındaki görsel ağırlığı ve Savunma düşman sayılarının küçük ekrandaki okunurluğunu düzenle. Kaydedilen örneklerde küçük hedefleri gerçek parmakla dene. [Unity mağaza çekimleri](../ArrowSwarm_Marketing/03_GooglePlay/StoreSet_TR_20261118/overview_TR.jpg)
  - [x] Şekiller’in açık tema ok paletinde en yüksek ölçülen doğrusal parlaklık 0,462 → 0,281. 720×1280 Unity Game View’da 1, 2 ve 50. bölümlerde baş ve gövde birlikte kontrol edildi. Savunma 35’te düşman sayıları 3 → 3,45 TMP boyutuna yükseltildi, beyaz yüz ve koyu kontur eklendi. [Önce ve sonra görüntüleri](http://127.0.0.1:8200/#%2Fcaptures%2FPlayerExperience%2Findex.html)
- [ ] Yakın iki ok üzerinde amaçlanan ve hareket eden oku karşılaştır. Dokunma önceliğini ölçülen yanlış seçimlere göre düzelt. Gerekirse yakınlaştırma, yenilenen ok hedeflerinden sonra ayrı bir deney olarak değerlendirilir. [PuzzleSession.Actions.cs](../Assets/_Project/Scripts/Puzzle/PuzzleSession.Actions.cs#L13)
- [ ] Yanlış hamlede çarpılan oku, yanlış renk kenarını ve Savunma’da düşmanın bitişe ulaşmasını birbirinden ayırt edilebilir göster. Renk Değişimi’nde eşleşen ok ve duvar için ortak simge/desen dene. İlk tahtada dönüşün hangi okun yolunu açtığını göster. Sağa ve sola dönüş aynı mantıksal duruma gidiyor ve bir hak tüketiyor. Oyuncu bu kuralı kontrolü gördüğünde anlayabilmeli. [PuzzleRules.cs](../Assets/_Project/Scripts/Modes/PuzzleRules.cs#L28)
  - [x] Ok teması, yanlış renkli duvar ve düşmanın çıkışa ulaşması farklı metinlerle gösteriliyor. Renk Değişimi’nin dönüş kontrolü iki yönün de tek hak kullandığını belirtiyor. Unity’de üç modun gerçek oyun akışlarında ve iki dönüş yönünde doğrulandı. Düşman çıkış bildirimi testte doğrudan oyun yöneticisine gönderildi. [Sahne çekimleri ve test kayıtları](http://127.0.0.1:8200/#%2Fcaptures%2FPlayerExperience%2Findex.html)
  - [x] Renk Değişimi okları ve eşleşen duvarlarında aynı halka veya elmas işareti var. İlk tahtada açık çıkış kalmadığında dönüş öneriliyor ve dönüşle açılan ok vurgulanıyor. 1080×2400 ve 720×1280 Unity Game View’da birinci tahta ve 114 oklu 50. tahta kontrol edildi. 50/50 bölüm çözücüden geçti. Gri ton önizlemesi mevcut, dar fiziksel telefonda oyuncu testi açık.
- [x] Renk Değişimi 3’te dönüşler tükenince sonuç paneli nedeni ve ücretsiz Tekrar Dene seçeneğini gösteriyor. Renk Değişimi 1’de kullanılabilir roket tahta açık kalırken yönlendirmesiyle çalışıyor. Son canın kaybında ok geri dönene kadar sonuç paneli açılmıyor. [Unity test kayıtları](http://127.0.0.1:8200/#%2Fcaptures%2FPlayerExperience%2Findex.html)

**Kabul ölçütü.** Oyuncu yardım almadan canı niye kaybettiğini ve sonraki geçerli eylemi açıklayabiliyor. Dar telefonda yan yana ok testlerinde yanlış seçimler başlangıç ölçümünden düşük. Renk eşleşmesi gri tonlu görüntüde de okunuyor. Mevcut ipucu, roket, dondurma, son can, tekrar dene ve duraklatma senaryoları geçiyor. Oynanış kuralı veya girdi değişikliklerinde etkilenen kampanyaların mevcut çözülebilirlik ve Unity oynanış kontrollerini tekrar çalıştır.

## 3 · Savunma dengesi, zorluk ve reklam ritmi

- [x] 2. ve 35. Savunma bölümlerinde aynı tahta tohumuyla iki atış stratejisi denendi. Dört koşu da üç canla kazandı. Konuma göre seçilen atışlarda normal ok isabeti 2. bölümde 4 → 7, 35. bölümde 33 → 76 oldu. Toplam süre sırasıyla 2,41 → 5,28 ve 9,28 → 17,86 oyun saniyesiydi. Dondur her iki yöntemde kapalıydı, bütün bitişlerde kalan düşman sayısı sıfırdı. Unity hasar kaydıyla otomatik final temizliği gerçek ok temasından ayrıldı. Oyuncu tercihleri henüz ölçülmedi. [Koşu tablosu](http://127.0.0.1:8200/captures/PlayerExperience/DefenseTiming/attempts.csv)
- [ ] Test sonuçlarına göre en küçük denge değişikliğini seç. İyi zamanlama zaten fark yaratıyorsa bunu sahada daha görünür kıl. Hızlı boşaltma üstün geliyorsa önce zamanlamaya bağlı görünür ustalık hedefini dene. Ana kazanma kuralı için prototip gerekirse eski ve yeni kuralla aynı tahtaları karşılaştır. Gökkuşağı finalini, son can ile son ok yarışını ve kayıt ilerlemesini doğrula.
  - [x] İsabet farkı sonuç kartında **Normal ok isabeti** olarak görünüyor. Gerçek atış temasları sayılıyor. Gökkuşağı teması ve otomatik final temizliği bu sayıya dahil edilmiyor. Bölüm kazanma ve yıldız koşulları korunuyor. Unity’de gerçek atışla kazanılan 2. bölümde sayaç Türkçe açık ve İngilizce koyu temada doğrulandı. Ustalık hedefi için sayısal eşik oyuncu gözlemine bağlı.
- [ ] Üç modun ilk beş bölümünde süre, tekrar ve çıkış nedenlerine bak. Tek bir seviyedeki büyük zorluk sıçramasını önce düzenle. Şekiller’de tıkanmanın çizgi yoğunluğundan mı, gerçek planlama zorluğundan mı geldiğini ayırt et. Renk Değişimi’nde ilk dönüş kullanımını ve sıfır hakla kaybı gözle.
- [ ] İlk birkaç galibiyette geçiş reklamı ertelemesini dene. Mevcut ayar 4. bölüm ve 90 saniye. Reklam koşulları değiştiğinde galibiyetten sonraki bölüme geçişi, oturumdan çıkışı ve yeniden denemeyi karşılaştır. Ödüllü akışta ret, başarısız yükleme, çevrimdışı durum ve tek ödül senaryolarını test et. [CoreManagers.prefab](../Assets/_Project/Prefabs/Core/CoreManagers.prefab#L375)

**Kabul ölçütü.** Oyuncu ne zaman ateş ettiğinin sonucunu görebiliyor. Planlı atış ile hızlı boşaltma karşılaştırmasının sonucu kaydedilmiş. Yeni reklam ritmi seçilirse seçim, sonraki bölüme geçiş ve gerçek cihazdaki izin/ödül akışıyla gerekçelendiriliyor.

## 4 · Sıralama ve mağaza vaadi

- [ ] Canlı sıralamadaki `Savunma en yüksek bölüm × 10.000 + toplam yıldız` ağırlığını kullanıcıya açıkla. İlk seçenek listeyi **Savunma Sıralaması** diye adlandırmak. Ayrı mod tablolarına, her modda anlamlı bir rekabet talebi ve skor yönetimi varsa geç. Yardımlı/yardımsız sonuçların kapsamını tanımla. Formül değişikliği seçilirse eski skoru koruyan sürümleme, geçiş ve geri dönüş planını ayrı kaydet. [UnityCloudService.cs](../Assets/_Project/Scripts/Data/UnityCloudService.cs#L129)
- [ ] Aynı mağaza metniyle ilk görseldeki iki adayı karşılaştır. İlk görseli gören kişiye oyunda ne yapacağını sor. Mağazada yeterli trafik oluşursa aynı dil ve dönemle Play Console dönüşümünü karşılaştır. [Mağaza stratejisi](../ArrowSwarm_Marketing/07_ASO_SEO_GEO/ASO_STRATEGY.md)
  - [x] Savunma’da gerçek isabet karesi ve Şekiller silueti aynı Türkçe başlıkla hazır. Ana menü görseli karşılaştırmada duruyor. [Mağaza görseli karşılaştırması](http://127.0.0.1:8200/#%2Fcaptures%2FPlayerExperience%2Fstore.html)

**Kabul ölçütü.** Oyuncu yıldızın ve bölümün sırasına etkisini anlıyor. Sıralamanın boş, gecikmeli, çevrimdışı ve yeniden bağlanma durumları önceki testleri geçiyor. Yeni mağaza görseli, oyunun kuralını ilk bakışta daha iyi anlatıyor ve gerçek oynanışla eşleşiyor.

## 5 · İlk içeriğe dönme nedeni

- [ ] İlk oturum akışı iyileştikten sonra **Günün Bulmacası** prototipini mevcut seviyelerden gün ve mod bazında seçilen tek bir tahta ile dene. Herkese aynı tahta ve oyuncunun kendi yıldız kaydı ilk deneme için yeterli. Mevcut kampanya kilidi, kaydı ve yıldız hesabıyla etkileşimi doğrula.
- [ ] Günlük tahtaya girenlerin tamamlama oranını, D1/D7 dönüşünü ve kampanyaya devamını ölç. Oyuncu verisi talebi gösterirse özel günlük tahtaları değerlendir. Dördüncü mod, seri ödülleri, bildirime dayalı hatırlatmalar ve geniş seviye üretimi bu ölçümden sonra değerlendirilir.

## Devam noktası

**Sıradaki ilk iş.** Savunma’da zamanlı atışın etkisini sonuçta görünür kıl ve sıralama puanının hangi ilerlemeyi saydığını anlat. Küçük ok hedeflerini ve HUD ile tahta dengesini Android cihazda kendi ölçümleriyle doğrula.

Her aşama için bulgu, önce/sonra kanıtı, doğrulama sonucu ve commit kaydını bu dosyaya ekle. Unity MCP ile gerçek kontrolleri ve önceki regresyonları dene. Cihaz ve insan gözlemlerini Editör ve çözücü sonuçlarından ayrı kaydet. Oyuncu kayıtlarını, tercihleri ve açık yerel değişiklikleri koru. Onaylı UI tasarım sırası kendi [UI yol haritasından](UI_REDESIGN_ROLLOUT.md) izlenir.
