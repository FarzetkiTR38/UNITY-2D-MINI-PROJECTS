# Özellik matrisi
Kanıt düzeyi: **K** kaynakta / **E** canlı Editor asset sorgusu / **A** tarihli native arşiv / **R** bu oturumda runtime test. R yalnız13KasımShapes2kaydı için doğrulandı; diğer maddeler eski kaynak/asset/arsiv kanıtı olarak kalır.

| Özellik | Mod | Kanıt | Pazarlamada kullanım |
|---|---|---|---|
| Yol boşsa oku fırlatma | Defense | K Arrow.cs, GridManager | Runtime klip sonrası |
| Son okun Rainbow olması | Defense | K ArrowSpawner.PromoteLastArrowToRainbow | Çekim sonrası; tüm modlara mal etme |
| Mob rotası ve hasar | Defense | K Core/Mob/Path | 'Bulmaca + savunma' test hipotezi |
| Siluet şekilli ok bulmacası | Shapes | K/E/A/R |13Kasım600kare,42gerçekatışvewin;diğerşekillerarsiv |
| Çarpıp geri dönme ve can kaybı | Shapes/Color Shift | K PuzzleSession.Actions | Gerçek yanlış atış klibiyle öğret |
| Birden fazla eşzamanlı doğru atış | Shapes | K Ready/Shot | Tatmin edici sekans; otomatik zincir iddia etme |
| İki renk duvar, dönen çerçeve | Color Shift | K PuzzleFrame/Session | Alternatif konumlandırma |
| Ayrı 50 bölüm | Shapes/Color Shift | E Count=50/50 | Sayı güncel, tüm seviyeler tekrar test edilmedi |
| Üç can, yıldızlar, mod bazlı ilerleme | Puzzle modları | K Session/ModeProgress | İlerleme örneği, fake rank kullanma |
| İpucu/roket | Puzzle | K; A Shapes HUD | İşlev için runtime doğrulama bekler |
| Freeze | Defense | K UI/skill | Puzzle modlarına ekleme |
| Light/Dark | UI | K/A | Yeni native karşılaştırma bekler |
| 10 dil | UI | K JSON envanteri | Tam kapsam/taşma kontrolü bekler |
| Tutorial | Defense + ilk puzzle yardımı | K | Baştan yeni oyuncu testi bekler |
| Leaderboard | Ortak | K UGS hizmeti | Online erişim ve gerçek data doğrulanmadı |
| Classic | Ayrı mod bulunmadı | Enum + EN/TR JSON | Dördüncü mod vaat edilmez |
