# Arrow Swarm · Şekiller Reels serisi

14Kasım2026. Onaylanan pilotun devamı; istenen sıra korunmuştur. [Galeriyi aç](index.html). Her video1080×1920/30fps, Türkçe anlatımlı, yaklaşık41s; altı finalin toplamı yaklaşık20MB'dır.

Son küçük revize: "piley" başlangıcının45ms'lik P sesi netleştirildi. Yedi güncel Reels ve ortak kapanışta uygulandı; görüntü paketleri, süreler ve diğer konuşma korundu. Seri `P_PRONUNCIATION_QA.json` sekiz çıktıdaki hash/decode ve dar ses düzenlemesini kaydeder. ZIP güncel dosyaları içerir.

|Sıra|Bölüm / şekil|Ok|MP4|Altyazı|
|---|---|---:|---|---|
|1|12 · Kaplumbağa|45|[İndir](L12/ArrowSwarm_Shapes12_TR.mp4)|[SRT](L12/captions_TR.srt)|
|2|16 · Ördek|42|[İndir](L16/ArrowSwarm_Shapes16_TR.mp4)|[SRT](L16/captions_TR.srt)|
|3|21 · Ahtapot|44|[İndir](L21/ArrowSwarm_Shapes21_TR.mp4)|[SRT](L21/captions_TR.srt)|
|4|5 · Roket|40|[İndir](L05/ArrowSwarm_Shapes5_TR.mp4)|[SRT](L05/captions_TR.srt)|
|5|29 · Flamingo|40|[İndir](L29/ArrowSwarm_Shapes29_TR.mp4)|[SRT](L29/captions_TR.srt)|
|6|39 · Ayçiçeği|44|[İndir](L39/ArrowSwarm_Shapes39_TR.mp4)|[SRT](L39/captions_TR.srt)|

Toplu yerel ZIP: `../../Final_Deliverables/ArrowSwarm_Shapes_Series_TR_20261114.zip`. Mağaza bağlantısı kullanıcının isteğiyle mock; yayınlanmış uygulama adresi eklenmedi. Videodaki PlayNow basma hareketi bir tanıtım animasyonudur.

Gerçek ShapesScene kayıtlarında iki HUD korunur. İlk doğru A çıkar, tamamen biter, bekleme/anlatım sonrası kalan çözüm1,5× işaretli olarak ilerler; son uçuş/nativeWinPanel normal hızdadır. Her bölüm üç canla kazanılır. Tebrik cümlesi tamamlandıktan sonra aynı onaylı6,3s kapanış gelir. Uzun roket/ayçiçeği ve flamingoda açıklamalar/işaretler gerçek okları örtmeyecek boşluklara uyarlanmıştır. Oynanışa veya oyun sahnelerine değişiklik yoktur.

Anlatım sentetiktir; müzik/ses vurguları özgün kurgu sesleridir, Unity AudioListener kaydı değildir. Teknik kontroller ve kare incelemesi sesin öznel olarak beğenildiği iddiası değildir. Henüz bu altı bölümün yaratıcı değerlendirmesi kullanıcıdadır.

## Kanıt ve tekrar üretme

- `FINAL_CHECKS.json`: SHA256, codec/frame/decode, ses seviyesi,255atış ve5400nativekare toplamı.
- `EDITORIAL_REVIEW.md`: gerçekten incelenen encoded kareler; kapanan görünür sorunlar ve inceleme sınırları.
- `native_session_checks.json`: Unity MCP sonuçları, temiz GameScene/Edit geri dönüşü, özel durum ve önceden açık10dosyanın korunması.
- `browser_checks.json` ve `browser_desktop.png`/`browser_mobile.png`: gerçek altı video seçimi/oynatma/seek/mute/mock CTA,1280×900 ve390×844; yatay taşma yok.
- Her `Lxx/`: finalMP4, SRT, finalencodedkapak/contactsheet, editplan, kaynakprovenance, nativekontrol, sesmanifesti, zaman eşlemesi ve FFmpeg logları.
- NativeJPEGserileri yerelde ve Gitignore içindedir. Anahtar PNG, geometri, üretim-kuralı sonuçları, eventCSV, provenance ve900karelik hashmanifesti her kaynak klasöründe versionlanır. Özel tercih değerleri versionlanmaz.

Repo kökünden: `py ArrowSwarm_Marketing/Tools/render_shapes_series.py --level 12 --stills`. Gerçek yerel kareler mevcutken `--stills` olmadan yeniden render eder. `build_series_gallery.py` galeriyi üretir; `check_shapes_series.py` son dosyaları ve ZIP'i doğrular. Sunucu: `py ArrowSwarm_Marketing/Tools/serve_reel_review.py --revision series --port 8194`; sadece yerel galeri kökü sunulur.
