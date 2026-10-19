# Shapes baş–boyun hizası

Gönderilen 20 kural incelendi. İlgili kurallar **7: doğal boyun** ve **14: baş–boyun hizası**. Bir unit/weight düz bağlantı zorunlu; baş bu bağlantının devamına bakar.

![Önce / sonra: normal ve ipucu başı](review.png)

Her tarafta soldaki normal, sağdaki ipucu görünümüdür. Aynı yol, kalınlık ve baş boyutu kullanılır. Görüntüler gerçek Unity kamera renderlarından alınmıştır.

Verideki 3.906 ok zaten doğru yöndeydi. Önceki Shapes çizimi üçgenin ucunu kafa hücresine koyduğu için, ilk dönüşten önceki düz parçanın çoğu üçgenin altında kalıyordu. Baş artık Defense'teki gibi hücreye merkezlenir. İpucu büyütmesinde de dönüşten sonra görünür düz boyun bırakılır. Geçersiz baş/boyun verisi hem bölüm doğrulamasında hem çizime bağlanmadan önce reddedilir.

[Dört yönün tüm karşılaştırması](comparison.png) · [Gerçek Shapes Bölüm 1](Playthroughs/shapes-01.png) · [İpucu görünümü](highlight-gameplay.png)

## MCP sonucu

- 50 Shapes bölümündeki 3.906 ok normal ve ipucu halinde kontrol edildi. Baş merkezi, yön, görünen düz boyun ve üçgenin uç/taban/iç bölgelerinden seçim geçti: **90.098 kontrol, sıfır hata**.
- Görünen düz boynun minimumu normalde **.43 hücre**, ipucunda **.176 hücre**. Bunlar üçgenin altında kalan çizgiyi saymayan görsel ölçülerdir; mantıksal bağlantı en az **bir tam unit** olarak korunur.
- Kalınlık, dört yön, hareket, kısa/virajlı/merdiven yolları ve havuz sıfırlama: merkezli başta **7.020**, eski ankrajda **6.764** geometri kontrolü geçti.
- **50 Shapes + 50 Color Shift** bölümü gerçek runtime çözümleriyle üç canla tamamlandı; kayıt yazılmadı. Testler geçici hızlandırılmış ayarlarla yapıldı; baş/kalınlık oranları korundu.
- **450 etkileşim kontrolü** hızlı tıklama, paralel atış, ipucu, roket, geri sekme, hasar, pause, retry ve çerçeve hareketlerini doğruladı.
- Gerçek oturumda büyütülmüş başın ileri ucuna ayrıca tıklandı: eski yolun .62208 hücre uzağındaki nokta, yeni baş seçimiyle doğru oku ateşledi; envanter/ilerleme değişmedi.
- Son konsolda hata yok. 352 korunan dosya aynı; sahneler, 50 Shapes bölümünün hücreleri/renkleri, kataloglar, prefablar ve Defense kaynakları korundu. Yalnız Shapes config'ine baş merkezleme anahtarı eklendi.

[Boyun ve seçim raporu](neck-verification.txt) · [Bölüm özeti](catalog-necks.csv) · [Merkezli geometri](GeometryCentered/geometry-verification.txt) · [Eski ankraj geometrisi](GeometryLegacy/geometry-verification.txt) · [100 bölüm](Playthroughs/runtime-verification.txt) · [Etkileşimler](interaction-verification.txt) · [Gerçek baş ucu tıklaması](real-tip-tap-verification.txt) · [Dosyaların korunması](isolation-verification.txt)
