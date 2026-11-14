# Ahtapot · Creative Short01

**16 saniye**,1080×1920,30fps; önceki41,3 saniyelik sürümden **%61,3 daha kısa**. Yeni kurgu ayrı bir alternatiftir; önceki videolar korunur.

[MP4](ArrowSwarm_Ahtapot_CreativeShort_TR.mp4) · [Öncekiyle karşılaştır](index.html) · [Altyazı](captions_TR.srt) · [Encoded kareler](encoded_contact_sheet.jpg)

Açılış doğrudan "44 OK. İLK HAMLEN?" sorusuyla başlar. A/B seçimi ve iki saniyelik geri sayımdan sonra gerçek A atışı normal hızda biter. Yarım saniyelik bekleme,7,4× ile2,6 saniyeye sıkıştırılmış kalan çözüm, normal hızdaki son atış ve gerçek WinPanel gelir. Kısa sonuç cümlesinden sonra4,4 saniyelik native-art tanıtım kapanışı kullanılır. Tam düzeltilmiş P-CTA duyulur; son buton basması korunur.

Müzik ve efektler projenin mevcut seslerinden yeniden mikslenmiştir: GameBGM_alt2 kesiti kesintisiz devam eder, konuşmada kısılır; dokunma/ok/kazanma vurguları kurguya eşlenir. Üç yeni kısa AhmetNeural cümlesi editoryal anlatımdır; CTA önceki düzeltilmiş WAV'ın byte-exact kopyasıdır. Unity AudioListener kaydı değildir. Telifli dış sosyal medya müziği alınmadı.

Oyun ve iki HUD tüm oynanış boyunca gerçek native karelerdir; kamera zoom/crop veya HUD yeniden çizimi yoktur. Etiketler, dokunma halkası ve hız değişimi kurgu unsurlarıdır. Kaynak44 gerçek ve doğru atış içerir; tüm atışların ayrı ayrı izlendiği iddia edilmez. A çıktıktan sonra B'nin tüm yolu açılmaz; bu yönde bir iddia kullanılmaz. Mağaza URL'si hâlâ kullanıcı isteğiyle mock.

`NATIVE_PREFLIGHT.json` güncel Unity MCP üretim kurallarını; `QA_REPORT.json` tam480kare decode/ses/hash kontrollerini; `timeline.json` her output karedeki native kaynağı; `audio_timeline.json` ses/kaynak zamanlarını kaydeder. Bağımsız görsel inceleme ve browser kanıtı yanına eklenir. Sesin zevke göre beğenilmesi ve kreatif onay kullanıcıdadır.

Yeniden render: `py ArrowSwarm_Marketing/Tools/render_creative_short.py --stills`, ardından `--stills` olmadan. Önizleme: `py ArrowSwarm_Marketing/Tools/serve_reel_review.py --revision short --port 8195`. Ham900JPEG kaynak kare yerelde gereklidir ve Gitignore içindedir.
