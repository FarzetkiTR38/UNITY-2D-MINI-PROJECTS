# 🎰 Boss Chest & Spin Wheel - Unity Setup Pathmap

Bu rehberi adım adım takip ederek sistemi oyununa ekleyebilirsin.

---

## PHASE 1: Segment Prefab Oluştur

### Adım 1.1: Segment UI Prefab
1. **Hierarchy** → Sağ tık → **UI** → **Image** (geçici canvas oluşturur)
2. Image'ı şu şekilde düzenle:
   - **Name**: `WheelSegment`
   - **Width**: 150, **Height**: 80
   - **Pivot**: (0.5, 0) - alta ortala
3. Image'a çocuk olarak ekle:
   - **UI → Text** (Name: `SkillNameText`)
     - Font Size: 14
     - Alignment: Center
     - Color: White
4. `WheelSegment`'e **SpinWheelSegment.cs** script'i ekle
5. Inspector'da ata:
   - `Background Image` → WheelSegment'in kendisi
   - `Skill Name Text` → SkillNameText objesi
6. **Assets/Prefabs/UI/** klasörüne sürükle → Prefab yap
7. Hierarchy'deki geçici objeyi sil

---

## PHASE 2: Spin Wheel UI Oluştur

### Adım 2.1: SpinWheel Panel
1. **Canvas** altında → **UI → Panel** oluştur
   - **Name**: `SpinWheelPanel`
   - **Color**: (0, 0, 0, 0.8) - yarı saydam siyah
2. Panel'i **inactive** yap (Inspector'da checkbox kapat)

### Adım 2.2: Wheel Container
1. SpinWheelPanel altına → **Create Empty** 
   - **Name**: `WheelContainer`
   - **RectTransform**: 
     - Anchor: Middle Center
     - Width: 300, Height: 300
     - Pivot: (0.5, 0.5)
2. WheelContainer altına → **Create Empty**
   - **Name**: `SegmentHolder`

### Adım 2.3: Pointer (Ok)
1. SpinWheelPanel altına → **UI → Image**
   - **Name**: `Pointer`
   - En üste konumla (wheel'in üstünde)
   - Ok/üçgen sprite ata
   - Width: 40, Height: 60

### Adım 2.4: Spin Button
1. SpinWheelPanel altına → **UI → Button**
   - **Name**: `SpinButton`
   - Text: "DÖNDÜR!"
   - Wheel'in altına konumla

### Adım 2.5: Reward Text
1. SpinWheelPanel altına → **UI → Text**
   - **Name**: `RewardText`
   - Font Size: 18
   - Alignment: Center
   - Wheel'in yanına veya altına konumla

### Adım 2.6: Script Ata
1. `SpinWheelPanel`'i seç
2. **Add Component** → `SpinWheelManager`
3. Inspector'da ata:

| Field | Değer |
|-------|-------|
| Wheel Panel | SpinWheelPanel (kendisi) |
| Wheel Transform | WheelContainer |
| Spin Button | SpinButton |
| Segment Prefab | WheelSegment prefab |
| Segment Holder | SegmentHolder |
| Reward Text | RewardText |
| Segment Count | 8 (veya istediğin) |
| Reward Count | 3 (veya istediğin) |

---

## PHASE 3: Chest Prefab Oluştur

### Adım 3.1: Chest GameObject
1. **Hierarchy** → **2D Object → Sprite**
   - **Name**: `ChestDrop`
   - **Sprite**: Chest görseli (pack'inden veya kendi çizimin)
   - **Sorting Layer**: Items (veya uygun layer)
   - **Order in Layer**: 5

### Adım 3.2: Collider Ekle
1. **Add Component** → **BoxCollider2D**
   - **Is Trigger**: ✅ checked
   - Size'ı sprite'a göre ayarla

### Adım 3.3: Script Ekle
1. **Add Component** → `ChestDrop`
2. Inspector'da ayarla:

| Field | Değer |
|-------|-------|
| Interaction Range | 2 |
| Highlight Effect | (opsiyonel) |
| Open Effect | (opsiyonel) |

### Adım 3.4: Prefab Yap
1. **Assets/Prefabs/** klasörüne sürükle
2. Hierarchy'deki objeyi sil

---

## PHASE 4: Boss Prefab'larını Güncelle

### Her boss prefab için:
1. Prefab'ı aç (çift tıkla)
2. `EnemyHealthController` component'ini bul
3. Ayarla:

| Field | Değer |
|-------|-------|
| Is Boss | ✅ checked |
| Chest Prefab | ChestDrop prefab'ı sürükle |

4. Prefab'ı kaydet (Ctrl+S)

---

## PHASE 5: Test Et

### Hızlı Test Yöntemi:
1. Play Mode'a gir
2. Normal bir enemy'yi yakala (prefab'ını aç)
3. Geçici olarak `isBoss = true` yap
4. Play Mode'da öldür
5. Chest düşmeli!
6. Chest'e yaklaş ve tıkla
7. Spin Wheel açılmalı!

### Veya GameManager'a test tuşu ekle:
```csharp
// GameManager.cs - TestKeys() içine ekle
if (Input.GetKeyDown(KeyCode.C))
{
    SpinWheelManager.instance?.Show();
    Debug.Log("Test: Spin Wheel opened!");
}
```

---

## 🎮 Inspector Ayar Özeti

### SpinWheelManager Ayarları:
| Parametre | Açıklama | Önerilen |
|-----------|----------|----------|
| `segmentCount` | Çarktaki dilim sayısı | 6-12 |
| `rewardCount` | Kazanılacak skill sayısı | 1-4 |
| `spinDuration` | Dönüş süresi (sn) | 2-4 |
| `maxSpinSpeed` | Max hız (derece/sn) | 720 |
| `minExtraRotations` | Min ekstra tur | 3-5 |
| `fixedSkillPool` | Sabit skill havuzu | Boş = random |

---

## ✅ Checklist

- [ ] Segment prefab oluşturuldu
- [ ] SpinWheelPanel UI hazır
- [ ] SpinWheelManager bağlantıları yapıldı
- [ ] ChestDrop prefab oluşturuldu
- [ ] Boss prefab'ları güncellendi
- [ ] Test edildi ve çalışıyor!

---

## 🎨 Bonus: Görsel İyileştirmeler

### Çark Dönüş Animasyonu
- `SpinWheelManager.cs`'de `spinDuration` ve `maxSpinSpeed` değerlerini ayarla
- Daha dramatik: `minExtraRotations = 5`

### Segment Renkleri
- `SpinWheelSegment.cs`'de `activeSkillColor` ve `passiveSkillColor` değiştir
- Aktif = Mavi, Pasif = Yeşil varsayılan

### Chest Highlight
- Chest etrafına glow effect ekle
- `highlightEffect` field'ına ata
