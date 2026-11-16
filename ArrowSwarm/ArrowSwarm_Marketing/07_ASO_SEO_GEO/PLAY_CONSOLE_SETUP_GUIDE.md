# ArrowSwarm — Google Play Console Kurulum ve ASO/SEO/GEO Rehberi

**Paket Adı:** `com.farzetkigames.arrowswarm`  
**Geliştirici:** FarzetkiGames  
**Canlı Web Sitesi:** https://farzetkigames.com.tr  
**Oyun Sayfası:** https://farzetkigames.com.tr/arrowswarm  
**Gizlilik Politikası:** https://farzetkigames.com.tr/arrowswarm/privacy-policy  
**Veri Silme / İletişim:** https://farzetkigames.com.tr/contact  
**AdMob app-ads.txt:** https://farzetkigames.com.tr/app-ads.txt  

---

## 1. Mağaza Ayarları (Store Settings)
- **Uygulama Türü:** Oyun (Game)
- **Kategori:** Bulmaca (Puzzle)
- **Etiketler (Tags — en fazla 5 adet seç, Console listesinde uygun olanlardan):**
  - `Puzzle` (Bulmaca)
  - `Logic` (Mantık)
  - `Single player` (Tek oyunculu)
  - `Casual` (Günlük)
  - `Abstract` veya `Minimalist` / `2D` (listede mevcutsa)
- **Mağaza Girişi İletişim Bilgileri:**
  - **Web Sitesi:** `https://farzetkigames.com.tr` *(Kök domain yazılmalı ki AdMob `app-ads.txt` doğrulaması otomatik geçsin)*
  - **E-posta:** Kendi resmi geliştirici destek e-postan
  - **Telefon:** İsteğe bağlı

---

## 2. Ana Mağaza Girişi ve 10 Dil (ASO)
Oyunun içinde tam 10 dil desteği var (`TR, EN, DE, FR, ES, PT, IT, RU, JA, KO`). Google Play Console → **Mağaza varlığı → Ana mağaza girişi → Çevirileri yönet** bölümünden bu 10 dili ekleyip `07_ASO_SEO_GEO/index.html` sayfasındaki tek tıkla "Kopyala" butonlarıyla her dilin:
- **Uygulama adı (≤ 30 karakter)**
- **Kısa açıklama (≤ 80 karakter)**
- **Uzun açıklama (≤ 4000 karakter)**
alanlarını birebir yapıştır.

### Mağaza Görselleri Sıralaması (8 Ekran Görüntüsü + Özellik Grafiği)
1. **İkon (512×512 PNG):** [app_icon_512x512.png](../03_GooglePlay/app_icon_512x512.png)
2. **Özellik Grafiği (1024×500 PNG):** [feature_graphic_final_EN_1024x500.png](../02_Branding/Selected_20261115/feature_graphic_final_EN_1024x500.png) / [feature_graphic_final_TR_1024x500.png](../02_Branding/Selected_20261115/feature_graphic_final_TR_1024x500.png)
3. **Tanıtım Videosu (YouTube linki olarak eklenir):** [ArrowSwarm_StoreTrailer_EN_25s.mp4](../06_Trailers_Ads_Tutorials/StoreLaunch_20261115/ArrowSwarm_StoreTrailer_EN_25s.mp4) (önce YouTube'a yükleyip linkini Play Console'a yapıştır)
4. **8 Ekran Görüntüsü (`StoreSet_EN_20261115/Final/`):** `01_Home` → `02_Defense` → `03_Shapes` → `04_ColorShift` → `05_Defense_Freeze` → `06_Shapes_Advanced` → `07_ColorShift_Rotate` → `08_Leaderboard`

---

## 3. Uygulama İçeriği (App Content) Form Cevapları

### A. Gizlilik Politikası (Privacy Policy)
- **URL:** `https://farzetkigames.com.tr/arrowswarm/privacy-policy`

### B. Reklamlar (Ads)
- **"Uygulamamda reklamlar var mı?"** → **Evet, uygulamam reklam içeriyor.** *(Google AdMob — Ödüllü ve Geçiş Reklamları)*

### C. Hedef Kitle ve İçerik (Target Audience & Content)
- **Hedef Yaş Grupları:** `13-15`, `16-17`, `18 ve üzeri` seç; `13 yaş altı` grupları **seçme** *(Google Aileler Politikası / COPPA gereksinimleri devreye girmemesi için; gizlilik politikamız da 13+ genel kitleye göre düzenlendi)*.
- **Çocuklara hitap ediyor mu?** → **Hayır** (Genel kitle bulmaca arayüzü).

### D. İçerik Derecelendirmesi (IARC Content Rating)
- **Kategori:** Oyun (Game)
- **Şiddet:** Gerçekçi kan, vahşet veya insan/hayvanlara yönelik şiddet **YOK**. *(Defense modunda soyut/çizgi film karakterli botlara ok atışı var; soru gelirse "Fantezi / Çizgi Film Şiddeti — Hafif" işaretleyebilirsin, diğer tüm korku/kumar/dil/cinsellik soruları "Hayır")*.

### E. Veri Güvenliği (Data Safety — Unity Gaming Services + Google AdMob)
> **Dikkat:** Kendi sunucun olmasa bile oyunda **Unity Gaming Services (Authentication + Leaderboards)** ve **Google Mobile Ads (AdMob)** SDK'ları bulunduğu için "Veri toplamıyor" **DEĞİL**, "Evet, veri topluyor / paylaşıyor" seçmelisin!

1. **Veri Toplama ve Güvenlik:**
   - Uygulamanız gerekli kullanıcı verilerini topluyor veya paylaşıyor mu? → **Evet**
   - Aktarım sırasında tüm kullanıcı verileri şifreleniyor mu (HTTPS/TLS)? → **Evet**
   - Kullanıcıların verilerinin silinmesini istemeleri için bir yöntem sağlıyor musunuz? → **Evet**
   - **Veri Silme URL'si:** `https://farzetkigames.com.tr/arrowswarm/privacy-policy` *(veya doğrudan form: `https://farzetkigames.com.tr/contact`)*
2. **Toplanan Veri Türleri (Data Types):**
   - **Konum → Yaklaşık konum (Approximate location):**
     - *Neden:* Google AdMob SDK (IP tabanlı bölge/reklam eşleştirme) & UGS ülke kodu.
     - *Toplanıyor mu?* Evet (veya AdMob üzerinden Paylaşılıyor). *Kullanıcı isteğe bağlı mı?* Reklam/SDK için zorunlu veya bölgeye göre. *Amaçlar:* Reklamcılık veya pazarlama, Analiz, Uygulama işlevselliği.
   - **Kişisel bilgiler → Ad (Name) veya Kullanıcı Kimlikleri (User IDs):**
     - *Neden:* Liderlik tablosunda oyuncunun seçtiği takma ad (Display Name) + Unity Authentication Anonymous ID (`UnityCloudService`). *(Ad/Kullanıcı kimlikleri altında "Kullanıcı Kimlikleri / User IDs" kesin işaretle; oyuncu takma adı serbest metin olduğu için "Ad" veya "Diğer kullanıcı tarafından oluşturulan içerik" seçmek güvenlidir).*
     - *Kullanıcı seçimi:* Profil oluşturma adımında **Atla (Skip)** seçeneği varsa "İsteğe bağlı" işaretleyebilirsin.
     - *Amaç:* Uygulama işlevselliği (Liderlik tablosu).
   - **Uygulama etkinliği → Uygulama etkileşimleri (App interactions):**
     - *Neden:* AdMob SDK reklam gösterim/tıklama ve UGS liderlik puanı.
     - *Amaç:* Analiz, Reklamcılık veya pazarlama, Dolandırıcılık önleme.
   - **Uygulama bilgileri ve performansı → Kilitlenme günlükleri / Tanılama (Crash logs / Diagnostics):**
     - *Neden:* Google Play & AdMob SDK standart kararlılık/performans telemetrisi.
     - *Amaç:* Analiz, Uygulama işlevselliği.
   - **Cihaz veya diğer kimlikler (Device or other IDs):**
     - *Neden:* Google Advertising ID (AdMob SDK) + UGS anonim cihaz/oyuncu ID'si.
     - *Paylaşılıyor mu?* Evet (AdMob reklam ağı).
     - *Amaç:* Reklamcılık veya pazarlama, Analiz, Dolandırıcılık önleme ve güvenlik.

---

## 4. Yayın Sonrası İlk 3 ASO Deneyi (Store Listing Experiments)
1. **ASO-01 (İlk Ekran Görüntüsü):** Mevcut `01_Home` vs doğrudan `03_Shapes` (okunabilir büyük bulmaca tahtası) 1. sırada gösterilerek kurulum dönüşümünü karşılaştır.
2. **ASO-02 (İkon Testi):** Mevcut `ArrowSwarm App Icon` vs farklı renk/kontrast odaklı varyasyon (yayın oturduktan sonra).
3. **ASO-03 (Kısa Açıklama):** "Üç mod özeti" vs "Doğrudan hamle/şekil kancası" karşılaştırması.
