# Referans Reels · Shapes 2 · 14 Kasım 2026

**Güncel revizyon R2:** Kullanıcı genel yönü beğendi; HUD'ların korunması, yalnız A sonrası bekleme ve iki anlatım cümlesi revize edildi. Güncel teslim/devam listesi [REFERENCE_REEL_R2.md](REFERENCE_REEL_R2.md). Aşağıdaki28s ilk referans pilotu tarihsel sürümdür.

## Aktif kapsam ve onay sınırı

Kullanıcının yeni isteği, iki Instagram örneğine benzer **tek bir Shapes 2 Reels** hazırlamak. Önceki ilk pilot beğenilmedi ve korunuyor. Bu yeni örnek onaylanmadan diğer bölümler üretilmeyecek. Sonraki istenen sıra: **12, 16, 21, 5, 29, 39**; ardından diğer modlar ayrı değerlendirilecek. Genel pazarlama fazlarının tamamlandığı iddia edilmiyor.

## Süren uygulama listesi

- [x] Proje kuralları, devam dosyaları ve Git durumu okundu. Önceden değişmiş 10 dosyanın SHA256 başlangıç kaydı TEMP'te alındı; özel oyuncu durumuna dokunulmadı.
- [x] İki Instagram videosu gerçek tarayıcı oynatımında görsel olarak incelendi; 24,04 ve 33,13 saniyelik süreler ve zaman kodlu kareler arşivlendi.
- [x] Referansların kalıcı bağlantı/analiz arşivi kuruldu. Özgün MP4 indirmesi başarısız; kare arşivi özgün video diye sunulmayacak. Sesleri dinlenmiş sayılmıyor.
- [x] Gerçek Shapes 2 kaydının 600 karesi/hashleri, 42 atış ve kazanma sonu denetlendi. Eski 18 saniyelik kurgunun erken kesilen sonu tespit edildi.
- [x] A/B/C sorusu mevcut Unity MCP bağlantısında üretim `PuzzleRules` koduyla doğrulandı: A=ok0 açık, B=ok3 üç hücrede engelli, C=ok1 bir hücrede engelli. Başka açık oklar da bulunduğundan soru yalnız bu üç adaya uygulanacak.
- [x] Unity MCP yeniden bağlandıktan sonra Türkçe yeni Shapes 2 kaydı alındı: `Shapes_L2_20261114_101115_693`, 600 kare / 42 doğru atış / 3 can / gerçek kazanma. Başlangıç tahtası önceki kayıtla piksel olarak aynı; kazanma paneli de artık Türkçe.
- [x] Kayıt sonrasında özel tercihler, reklam/bulut sağlayıcıları, dil, tema, zamanlama ve başlangıç GameScene Edit durumu geri getirildi. Önceden değişmiş 10 dosyanın hashleri aynı kaldı.
- [x] Özgün Türkçe kurgu, yumuşak yakınlaşma, üç saniyelik geri sayım, gerçek ilk atış ve hızlandırılmış tam çözüm oluşturuldu.
- [x] Gerçek kazanma sonu, okun çıkışı tamamlandıktan sonra gösterildi; mevcut oyun logosuyla kısa kapanış eklendi.
- [x] Özgün metinden Türkçe sentetik anlatım, örneksiz özgün müzik/işaret sentezi, altyazı, kaynak/event eşlemesi ve düzenlenebilir montaj araçları hazır.
- [x] MP4 tamamen çözüldü: 840 kare, 1080×1920/30fps, H264/yuv420p/BT709 limited, 48kHz stereo AAC; -17,51 LUFS / -1,50 dBTP, hata yok. 42 kaynak atışının zaman eşlemesi doğrulandı.
- [x] Gerçek tarayıcıda sona kadar oynatım, byte-range ile 5,6/22,5/25 s atlamaları, masaüstü ve 390×844 telefon görünümü/taşma kontrolü geçti. Görsel incelemede vurgunun hayalet kalması, düşünme süresi ve kazanma arka planı düzeltildi. Sesin öznel dinleme değerlendirmesi yapılmış sayılmıyor; ses kontrolleri teknik ölçümdür.
- [x] Önceden değişmiş kaynakların aynı kaldığı doğrulandı; yalnız bu çalışmanın dosyaları aşama commitlerine alındı.
- [x] Tek pilot ve inceleme önizlemesi teslim için hazır.
- [ ] Kullanıcı yeni pilotu onayladı; diğer seviyelere başlanabilir. **Henüz onay gelmedi.**

## Kurgu

Yaklaşık 28 saniye, 1080×1920, 30fps. İlk saniyede penguen ve kısa soru; ardından ayaklardaki üç turuncu oka yakın plan. Harfler ve yol vurguları montaj katmanıdır, oyunun mekaniği değildir. Üç saniye düşünme süresinden sonra doğru adayın gerçek atışı gösterilir. Kamera tam şekle geri açılır; çözüm kontrollü hızlanır ve gerçek üç yıldızlı kazanma görünür. Son ekran yorum çağrısı içerir; doğrulanmamış mağaza/yayın vaadi yoktur.

Kaynak: `../04_Raw_Gameplay/Shapes_L2_20261114_101115_693/`. Üretim `PuzzleSession.Fire` çağrılarıyla alınan gerçek Unity görüntüsü, insan parmağı kaydı değildir. Kaynak sessizdir; ses montajda eklenir. Hız/freeze/zoom/işaretler açıkça editoryal olarak kaydedilecek. Oyun sahneleri, mekanik ve ilerleme değiştirilmeyecek. Eski İngilizce kaynak ve reddedilen ilk pilot korunuyor.

## Kesin sonraki adım

`ReferencePilot02_20261114/index.html` veya `ArrowSwarm_Shapes2_ReferencePilot_TR.mp4` üzerinden kullanıcı değerlendirmesi bekleniyor. Onay gelirse önce Shapes 12 için aynı editoryal dili o bölümün gerçek bağımlılıklarına uyarlayan senaryoyu kur, ardından 16,21,5,29,39 sırasını takip et. Revize istenirse yalnız bu yeni pilot üzerinde çalış. Önizlemeyi yeniden açmak için `py ArrowSwarm_Marketing/Tools/serve_reel_review.py` kullanılır. Diğer seviyelere **onaydan önce geçme**.
