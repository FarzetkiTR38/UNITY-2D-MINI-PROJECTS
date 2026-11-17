# Fragman, reklam ve öğretici — kaynak bağımlılıkları

**17 Kasım · Kalan beş video tamamlandı:** 20s mod karşılaştırması, Türkçe seslendirmeli90s başlangıç rehberi ve üç ayrı18s Defense/Shapes/Color Shift tanıtımı hazır. [Beşini izle/indir](http://127.0.0.1:8200/#%2Fmarketing%2F06_Trailers_Ads_Tutorials%2FRemainingVideos_20261117%2Findex.html), [tikli uygulama ve kanıtlar](RemainingVideos_20261117/IMPLEMENTATION.md). Gerçek native kaynaklar, iki HUD, görünür hız etiketleri; tam decode/ses/hash, mobil/masaüstü oynatma ve ZIP indirme doğrulandı. Bu tablodaki her briefin artık bir gerçek çıktısı vardır; yaratıcı değerlendirme kullanıcıdadır. Önceki teslimler korunur.

**16 Kasım güncellemesi:** 25 saniyelik üç modlu mağaza fragmanı ve 15 saniyelik yanlış hamle reklamı tamamlandı. [İzleme/indirme](http://127.0.0.1:8200/#%2Fmarketing%2F06_Trailers_Ads_Tutorials%2FStoreLaunch_20261115%2Findex.html), [tikli uygulama kaydı](StoreLaunch_20261115/IMPLEMENTATION.md). Seçilen ikonla uyumlu TR/EN özellik grafikleri de aynı sayfadadır. Aşağıdaki diğer briefler plan olarak kalır; tutorial veya diğer reklamların üretildiği anlamına gelmez.

| İş | Akış | Doğruluk koşulu |
|---|---|---|
| Puzzle reklamı15s |0–2sgerçekkarar,2–10sdoğru/yanlışhamle,10–15sgerçeksonuç+CTA|Çarpıpdönüşcananimasyonutamgörünmeli; yayınöncesiCTAindirmevaat etmez|
| Mod karşılaştırma reklamı20s |Gerçek çerçeve dönüşü → gerçekDefense → kısa mod isimleri|Kayıtlar ayrı modlar olarak etiketli; tekboarddagörünmeyenözellikbileştirilmez|
| Fragman25s |Shapes3s,ColorShift7s,Defense7s,gerçek ilerleme+kimlik|Yanıltıcı sıralama/puan/oyuncuyorumu yok|
| Tutorial90s |Seçim/yolblokajı → can/geri dönüş → mod farkları → yardım araçları|Herbeceri ve etkileşim gerçektestten; rewardedreklam sonucu sahnelenirse testbağlamı açık|
| Üç mod klibi |Her biri18s; tek modun gerçek hamle ve sonucu|Mod dışıFreeze/Rainbowvaadi yok|

Üretim notu: bu teslim mevcut doğrulanmış native kayıtlar ve olay zaman kodlarıyla tamamlandı. Yeni Unity kaydı veya oyun değişikliği yapılmadı. Video metinleri/SRT’ler Türkçedir; kaynak HUD dilleri ilgili manifestte açıklanır. Sonraki adım kullanıcı yaratıcı incelemesidir.
