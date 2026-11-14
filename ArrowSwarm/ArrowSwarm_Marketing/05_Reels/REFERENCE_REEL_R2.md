# Shapes 2 Reels · R2 · tam oyun sahnesi

Kullanıcı önceki kurgunun genel yönünü beğendi; dört revize istedi. Diğer seviyelere geçiş onayı verilmiş sayılmıyor.

- [x] Mevcut kayıt/montaj denetlendi: HUD'lar montajda maskelenmiş ve ilk atıştan 11 kare sonra başka oklar ateşlenmiş.
- [x] Koruma başlangıcı: önceden değişmiş 10 dosyanın yeni SHA256 kaydı TEMP'te alındı. Eski MP4 ve sesler korunuyor.
- [x] Yeni Türkçe açılış, kalan okları gönderme ve uzun tebrik cümlesi üretildi; diğer beş ses klibi aynen kullanıldı.
- [x] Editor-only kayıt aracı derlendi ve gerçek kayıtta doğrulandı. `Idle` kontrolü, en az bir saniye durgun bekleme ve `first_settled` olayı var; varsayılan eski kayıt akışı korunuyor.
- [x] Yeni 28 saniyelik Türkçe kayıt: `Shapes_L2_20261114_105432_122`. A f75'te atıldı, f97'de bitti; sonraki ok f225'e kadar bekledi. 42 atış/3 can/gerçek kazanma; 840 kare, hashli kaynak ve ham MP4. İki HUD sahnenin kendi düzeninde, sahne/prefab değiştirilmedi.
- [x] Stop/Recover sonrası başlangıç GameScene Edit durumu, tercihler/sağlayıcılar/tema/dil/zamanlama geri getirildi. Önceden değişmiş 10 dosya bayt olarak aynı.
- [ ] R2 kurguyu `ReferencePilot02_R2_20261114/` altında üret. Tam 1080×1920 kaynak; HUD maskesi, tüm kareyi kırpan yakınlaşma veya HUD yerine çizilen grafik olmasın.
- [ ] Açılış: “Bu penguen 42 oktan oluşuyor.” Cevap A'dan sonra yalnız A çıksın. “Şimdi geriye kalan okları yollayalım” tamamlanınca diğer atışlar başlasın.
- [ ] Kazanma: “Tebrikler! 42 oktan oluşan penguen bölümünü başarıyla geçtin!” Native sonuç ve HUD'lar ekranda kalsın.
- [ ] Kaynak olayları, A'nın bitişi, bekleme, ses eşleşmesi, tam decode, HUD piksel alanları ve gerçek tarayıcı oynatımını doğrula. Özel oyuncu durumunu geri getir.
- [ ] Tamamlanan aşamaları ayrı commitlere al; yeni MP4/önizlemeyi kullanıcı değerlendirmesine gönder.

## Kesin sonraki adım

Kayıt aracı değişikliğini Unity MCP ile derle; temiz Edit durumunda `Arm()` → MainMenu Play → `Begin(2,"Shapes",28,150,"tr")` çalıştır. Bittiğinde Stop/Recover ve özgün Edit sahnesine dön. Sonra olay dosyasından gerçek zamanlarla tam sahne R2 montajını kur. Diğer seviyelere bu revizyon onaylanmadan geçme.
