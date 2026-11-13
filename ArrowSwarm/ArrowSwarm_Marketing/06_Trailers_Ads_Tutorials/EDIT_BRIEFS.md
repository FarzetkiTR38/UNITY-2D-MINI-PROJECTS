# Fragman, reklam ve öğretici — kaynak bağımlılıkları

Bunlar üretim briefidir; bu klasörde henüz tamamlanmışMP4yoktur. Gerçek kaynak yokken bir Storyboard'u video diye teslim etmiyoruz.

| İş | Akış | Doğruluk koşulu |
|---|---|---|
| Puzzle reklamı15s |0–2sgerçekkarar,2–10sdoğru/yanlışhamle,10–15sgerçeksonuç+CTA|Çarpıpdönüşcananimasyonutamgörünmeli; yayınöncesiCTAindirmevaat etmez|
| Mod karşılaştırma reklamı20s |Gerçek çerçeve dönüşü → gerçekDefense → kısa mod isimleri|Kayıtlar ayrı modlar olarak etiketli; tekboarddagörünmeyenözellikbileştirilmez|
| Fragman25s |Shapes3s,ColorShift7s,Defense7s,gerçek ilerleme+kimlik|Yanıltıcı sıralama/puan/oyuncuyorumu yok|
| Tutorial90s |Seçim/yolblokajı → can/geri dönüş → mod farkları → yardım araçları|Herbeceri ve etkileşim gerçektestten; rewardedreklam sonucu sahnelenirse testbağlamı açık|
| Üç mod klibi |Her biri18s; tek modun gerçek hamle ve sonucu|Mod dışıFreeze/Rainbowvaadi yok|

Erişim sağlandığında CAPTURE_QUEUE senaryolarını üret, hamvideo zaman kodlarını seç,fonetik olmayanTR/ENaltyazı ve platform safearea kontrolü ekle. İlk taslakların reddi devamı durdurmuyor; nihai estetik revizyon kuyruğu ayrı korunuyor.
