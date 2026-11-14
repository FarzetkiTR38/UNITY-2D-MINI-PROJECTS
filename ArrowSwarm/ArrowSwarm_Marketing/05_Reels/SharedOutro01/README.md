# Ortak tanıtım kapanışı01

Son dar revize: "Play" kelimesinin başı daha net P için düzeltildi. CTA içindeki yalnız45ms değişti;2,3s süre ve diğer konuşma korundu. Ortak kapanış, penguen pilotu ve altı seri videosunun görüntü paketleri bit olarak aynı kaldı. `P_PRONUNCIATION_QA.json` ve `cta_p_patch.json` kanıtı içerir. AAC ses yeniden kodlandığından sıkıştırılmış ses paketlerinin bit olarak aynı kaldığı iddia edilmez.

Kullanıcının isteğiyle sonraki onaylı videoların sonunda kullanılacak6,3s ortak kapanış. Oyun logosu ve blank primary plate/play icon mevcut Unity atlasından tekrar kullanılır. Arka plan, yazı, hareket/ripple/pointer kod ile düzenlenebilir. Yeni oyun asset'i/import yoktur.

`store_config.json`: gerçek mağaza adresi için tek kaynak. Şu an `status=mock`, `store_url=null`; kullanıcı oyunun henüz yayınlanmadığını doğruladı. Uydurma packageID veya uygulama sayfası yoktur. Görseldeki play.google.com yalnız mağazanın genel alan adıdır; önizlemedeki PlayNow mock durum mesajı gösterir. Video içindeki basma animasyonu bir kurgu öğesidir; MP4 içine tıklanabilir mağaza düğmesi eklenmiş değildir.

Mağaza açılınca `store_url` ve `status` güncellenir, inceleme sayfası yeniden üretilir. Yayın gönderisinin gerçek linki de ayrıca eklenmelidir.

`ArrowSwarm_SharedOutro01_TR.mp4` bağımsız kapanış,`poster.jpg` still,`press.jpg` basma karesi. Render: `py ArrowSwarm_Marketing/Tools/render_promo_outro.py`. R3 tebrik33,52s'de biter;33,7s sonrası0,4s fade ile kapanışa geçer. Önceki ilk33,7s kurgu değişmez; final40s. Yeni CTA anlatımı34,5–36,54s, basma36,85s.
