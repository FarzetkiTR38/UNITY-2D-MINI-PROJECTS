# Fragman, reklam ve öğretici — kaynak bağımlılıkları

**16 Kasım güncellemesi:** 25 saniyelik üç modlu mağaza fragmanı ve 15 saniyelik yanlış hamle reklamı tamamlandı. [İzleme/indirme](http://127.0.0.1:8200/#%2Fmarketing%2F06_Trailers_Ads_Tutorials%2FStoreLaunch_20261115%2Findex.html), [tikli uygulama kaydı](StoreLaunch_20261115/IMPLEMENTATION.md). Seçilen ikonla uyumlu TR/EN özellik grafikleri de aynı sayfadadır. Aşağıdaki diğer briefler plan olarak kalır; tutorial veya diğer reklamların üretildiği anlamına gelmez.

| İş | Akış | Doğruluk koşulu |
|---|---|---|
| Puzzle reklamı15s |0–2sgerçekkarar,2–10sdoğru/yanlışhamle,10–15sgerçeksonuç+CTA|Çarpıpdönüşcananimasyonutamgörünmeli; yayınöncesiCTAindirmevaat etmez|
| Mod karşılaştırma reklamı20s |Gerçek çerçeve dönüşü → gerçekDefense → kısa mod isimleri|Kayıtlar ayrı modlar olarak etiketli; tekboarddagörünmeyenözellikbileştirilmez|
| Fragman25s |Shapes3s,ColorShift7s,Defense7s,gerçek ilerleme+kimlik|Yanıltıcı sıralama/puan/oyuncuyorumu yok|
| Tutorial90s |Seçim/yolblokajı → can/geri dönüş → mod farkları → yardım araçları|Herbeceri ve etkileşim gerçektestten; rewardedreklam sonucu sahnelenirse testbağlamı açık|
| Üç mod klibi |Her biri18s; tek modun gerçek hamle ve sonucu|Mod dışıFreeze/Rainbowvaadi yok|

Erişim sağlandığında CAPTURE_QUEUE senaryolarını üret, hamvideo zaman kodlarını seç,fonetik olmayanTR/ENaltyazı ve platform safearea kontrolü ekle. İlk taslakların reddi devamı durdurmuyor; nihai estetik revizyon kuyruğu ayrı korunuyor.
