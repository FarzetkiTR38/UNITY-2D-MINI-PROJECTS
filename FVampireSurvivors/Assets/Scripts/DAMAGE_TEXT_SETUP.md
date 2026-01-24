# Floating Damage Text System - Kurulum Kılavuzu

## 1. Prefab Oluşturma (DamagePopup)

### Adım 1: Boş GameObject Oluştur
1. **Hierarchy** → Right Click → **Create Empty**
2. İsim: `DamagePopup`

### Adım 2: TextMeshPro Ekle
1. DamagePopup seçili iken → **Add Component** → `TextMeshPro - Text`
2. **NOT:** TextMeshPro (3D) kullan, UI değil!

### Adım 3: TextMeshPro Ayarları
| Ayar | Değer |
|------|-------|
| Font Size | 5 |
| Alignment | Center |
| Sorting Layer | Default (veya UI) |
| Order in Layer | 100 |

### Adım 4: DamagePopup Script Ekle
1. **Add Component** → `DamagePopup`
2. Inspector'da default değerler zaten uygun

### Adım 5: Prefab Kaydet
1. DamagePopup objesi → **Assets/Prefabs/UI** klasörüne sürükle
2. Hierarchy'deki objeyi sil

---

## 2. Manager Kurulumu

### Adım 1: DamageTextManager Oluştur
1. **Hierarchy** → Right Click → **Create Empty**
2. İsim: `DamageTextManager`
3. **Add Component** → `DamageTextManager`

### Adım 2: Inspector Ayarları
| Ayar | Değer |
|------|-------|
| Damage Popup Prefab | DamagePopup prefab'ı sürükle |
| Initial Pool Size | 50 |
| Max Pool Size | 200 |
| Default Y Offset | 0.5 |

---

## 3. Enemy/Boss Ayarları

Her **EnemyHealthController** için:
1. Inspector'da **Damage Text Settings** bölümünü bul
2. `Show Damage Text` = ✓ (aktif)
3. `Damage Text Y Offset` = 0.5 (isteğe göre ayarla)

---

## 4. Kullanım Örnekleri

### Mevcut Skill'ler (Değişiklik Gerekmez!)
```csharp
// Eski kod hala çalışır:
enemyHealth.TakeDamage(25);
```

### Yeni Damage Türleri
```csharp
// Critical hit
enemyHealth.TakeCriticalDamage(50);

// DOT (poison/burn)
enemyHealth.TakeDOTDamage(5);

// Veya DamageInfo ile:
enemyHealth.TakeDamage(DamageInfo.Critical(50));
```

### Direct Manager Çağrısı (İsteğe Bağlı)
```csharp
// Heal gösterimi
DamageTextManager.Instance.ShowHeal(20, playerPosition);
```

---

## 5. Renk/Boyut Özelleştirme

**DamagePopup prefab** Inspector'ında:

| Damage Type | Varsayılan Renk | Font Çarpanı |
|-------------|-----------------|--------------|
| Normal | White | 1.0x |
| Critical | Yellow-Orange | 1.5x |
| DOT | Purple | 0.8x |
| Heal | Green | 1.0x |

---

## Dosya Yapısı

```
Assets/Scripts/
├── Interfaces/
│   └── IDamageable.cs
├── Combat/
│   └── DamageInfo.cs
├── UI/
│   └── DamagePopup.cs
├── Management/
│   └── DamageTextManager.cs
└── Enemy/
    └── EnemyHealthController.cs (modified)
```
