# Kayıt planı · doğrulanan ilk üretim

## Tamamlanan

1. Editor-onlyMarketingCapture lifecycle/cleanup düzeltildi; üretim gameplay/sahne/prefab/paketler değiştirilmedi.
2. Unitytamrefresh+derleme hatasız.
3. Arm→Play→Begin(2,"Shapes",1)→PASS→Stop:30gerçekkare.
4. Arm→Play→Begin(2,"Shapes",20)→PASS→Stop:600gerçekkare,42gerçekproductionFire,3can,gerçekwin.
5. Sonuç sceneMainMenuEditclean; Time.captureFramerate0,armedfalse. Oyuncuverisi/sağlayıcılar ve14açıkdosya hashleri korunuyor.

## Yeniden üretim

NativeGameView1080x1920veGame sekmesi odakta olsun. Edit'te sahne kaydedilmemiş değişiklik içermemeli; araç bunları kendiliğinden kaydetmez.

- `ArrowSwarm.Marketing.Editor.MarketingCapture.Arm()`
- UnityPlay,MainMenu/DataManager initialization bekle.
- `ArrowSwarm.Marketing.Editor.MarketingCapture.Begin(2,"Shapes",20)`
- StatusPASS olunca PlayStop. Sonraarmedfalse veplayer_state_restoration.txt kontrol et.
- Kesinti sonrasıEditMode'da `MarketingCapture.Recover()`; özel yedek yoksa araç kurtarma bayrağını silmeden hata verir.

Çıktı:UTCisimli04_Raw_Gameplay/<folder> içinde JPEGkareleri,source.png,events.csv,provenance.json,state_checks.txt. Ham kareler yerelde tutulur ve marketing.gitignore ile Git'e eklenmez. MP4/provenance/keyframe ayrı teslim edilir. Ses ham kayıtta yoktur; ilk editte özgün editoryal ses kullanılır.

## Devam

Captureparametreleri şimdilikShapes/ColorShift authoredpuzzle solver akışıdır. Defense/tutorial/Rainbow/yanlışhamle ve cihaz kayıtları henüz yok. Bunlar için ayrı kayıt senaryosu ve sahne doğrulaması gerekir; destek varmış gibi belgelemeyin. İlk18sReels kalite incelemesi bitmeden seri üretime geçmeyin.
