# Referans Reels · Shapes 2 · 14 Kasım 2026

## Aktif kapsam ve onay sınırı

Kullanıcının yeni isteği, iki Instagram örneğine benzer **tek bir Shapes 2 Reels** hazırlamak. Önceki ilk pilot beğenilmedi ve korunuyor. Bu yeni örnek onaylanmadan diğer bölümler üretilmeyecek. Sonraki istenen sıra: **12, 16, 21, 5, 29, 39**; ardından diğer modlar ayrı değerlendirilecek. Genel pazarlama fazlarının tamamlandığı iddia edilmiyor.

## Süren uygulama listesi

- [x] Proje kuralları, devam dosyaları ve Git durumu okundu. Önceden değişmiş 10 dosyanın SHA256 başlangıç kaydı TEMP'te alındı; özel oyuncu durumuna dokunulmadı.
- [x] İki Instagram videosu gerçek tarayıcı oynatımında görsel olarak incelendi; 24,04 ve 33,13 saniyelik süreler ve zaman kodlu kareler arşivlendi.
- [x] Referansların kalıcı bağlantı/analiz arşivi kuruldu. Özgün MP4 indirmesi başarısız; kare arşivi özgün video diye sunulmayacak. Sesleri dinlenmiş sayılmıyor.
- [x] Gerçek Shapes 2 kaydının 600 karesi/hashleri, 42 atış ve kazanma sonu denetlendi. Eski 18 saniyelik kurgunun erken kesilen sonu tespit edildi.
- [x] A/B/C sorusu mevcut Unity MCP bağlantısında üretim `PuzzleRules` koduyla doğrulandı: A=ok0 açık, B=ok3 üç hücrede engelli, C=ok1 bir hücrede engelli. Başka açık oklar da bulunduğundan soru yalnız bu üç adaya uygulanacak.
- [ ] Özgün Türkçe kurgu, yumuşak yakınlaşma, üç saniyelik geri sayım, gerçek ilk atış ve hızlandırılmış tam çözüm oluştur.
- [ ] Gerçek kazanma sonunu, okun çıkışı tamamlandıktan sonra göster; mevcut oyun logosuyla kısa kapanış ekle.
- [ ] Sesleri, altyazıları, kaynak/event eşlemesini ve düzenlenebilir montaj aracını teslim et.
- [ ] MP4'ü tamamen çözerek video/ses/kare/renk/taşma kontrollerini yap; encoded kareleri ve gerçek oynatımı gözden geçir.
- [ ] Önceden değişmiş kaynakların aynı kaldığını doğrula; yalnız bu çalışmanın dosyalarını commit et.
- [ ] Tek pilotu kullanıcıya gönder ve diğer bölümlere başlamadan onay bekle.

## Kurgu

Yaklaşık 28 saniye, 1080×1920, 30fps. İlk saniyede penguen ve kısa soru; ardından ayaklardaki üç turuncu oka yakın plan. Harfler ve yol vurguları montaj katmanıdır, oyunun mekaniği değildir. Üç saniye düşünme süresinden sonra doğru adayın gerçek atışı gösterilir. Kamera tam şekle geri açılır; çözüm kontrollü hızlanır ve gerçek üç yıldızlı kazanma görünür. Son ekran yorum çağrısı içerir; doğrulanmamış mağaza/yayın vaadi yoktur.

Kaynak: `../04_Raw_Gameplay/Shapes_L2_20261113_081526_913/` (bu dosyadan köke göre `../../04_Raw_Gameplay/...`). Üretim `PuzzleSession.Fire` çağrılarıyla alınan gerçek Unity görüntüsü, insan parmağı kaydı değildir. Kaynak sessizdir; ses montajda eklenir. Hız/freeze/zoom/işaretler açıkça editoryal olarak kaydedilecek. Oyun sahneleri, mekanik ve ilerleme değiştirilmeyecek.

## Kesin sonraki adım

`ReferencePilot02_20261114/` içine yeni kurguyu üret; `Tools/render_reference_reel.py` ve ses üretim dosyalarıyla tekrar üretilebilir tut. QA bitince bu listeyi güncelle. Diğer seviyelere **onaydan önce geçme**.
