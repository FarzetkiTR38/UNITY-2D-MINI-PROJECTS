# Shapes 2 / Referans pilotu

Tek örnek, 28 saniye. Diğer seviyeler kullanıcı bu kurguyu onayladıktan sonra üretilecek.

| Zaman | Kurgu |
|---|---|
|0–1,2|Bulanık penguen üzerinde kısa, iri soru başlığı. Türkçe anlatım.|
|1,2–3,1|Ayaklardaki üç gerçek oka yumuşak 1,3× yakınlaşma. A/B/C yolları ve rozetler.|
|3,1–5,5|Üç seçenek tam görünür; soru ve seçenekler seslendirilir.|
|5,5–8,5|Seçenek anlatımı bittikten sonra tam üç saniye düşünme süresi.|
|8,5–11,3|Cevap A. 8,833'te üretim kaydındaki ilk atış. Vurgu çizgisi atış başlamadan kaybolur.|
|11,3–13,3|Tam şekle yumuşak geri açılma ve çözümün devamı.|
|13,3–20|Gerçek atışlar yaklaşık 1,54× hızda; hız etiketi görünür.|
|20–23,5|Son okun çıkışı, gerçek Türkçe kazanma paneli ve üç altın yıldız.|
|23,5–28|Mevcut oyun logosu ve yorum sorusu.|

## Doğrulanan soru

A=üretim ok0; sola çıkışı açık. B=ok3; önünde üç hücre sonra ok1 var. C=ok1; bir hücre sonra ok2 var. Soru **bu üç ok** içindir; tahtadaki diğer açık oklar hakkında tek doğru hamle iddiası yoktur. Unity MCP `PuzzleRules.ObstacleDistance` / `CanRemove` ile kontrol edildi.

## Kaynak ve ses

14 Kasım 2026'da Unity MCP ile kaydedilen Shapes 2, 600 gerçek kare, 42 doğru atış, üç can ve kazanma. Oyunun üretim Fire çağrıları solver sırasıyla çalıştırıldı; insan parmağı kaydı değildir. Kaynak sessizdir. Başlangıç bekletmesi, görüntü kırpması, yakınlaşma, hızlandırma ve işaretler editoryaldir. Türkçe sentetik anlatım, bu pilot için yazılmış metinle üretildi; müzik ve kısa işitsel işaretler örneksiz sayısal sentezdir. Instagram'dan görüntü, müzik veya konuşma kopyalanmadı.

Kaynak ayrıntıları `source_provenance.json`, native kontroller ham kayıt klasöründeki `native_capture_checks.json`; her montaj karesinin kaynağı `timeline.json`, ses eşleşmeleri `audio_timeline.json` içinde.

## Paylaşım metni taslağı

Bu üç oktan hangisi önce çıkar: A, B, C? 🐧 Üç saniyede seç, sonra penguenin adım adım çözülüşünü izle. Sen ilk seferde bildin mi?

#ArrowSwarm #OkBulmacası #Bulmaca #IndieGame

Henüz paylaşılmadı. Mağaza çıkışı veya doğrulanmamış indirme bağlantısı vaat edilmiyor. Bu dosyanın teknik testleri yaratıcı onay değildir.
