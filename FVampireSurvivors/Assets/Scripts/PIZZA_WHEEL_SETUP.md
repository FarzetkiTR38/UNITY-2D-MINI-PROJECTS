# 🍕 Pizza-Style Spin Wheel - Yeni Setup Guide

## Segment Prefab'ı YENİDEN Oluştur

Eski segment prefab'ı sil ve bu adımları takip et:

### 1. Yeni Segment Prefab
1. **Hierarchy** → Sağ tık → **Create Empty**
   - **Name**: `PieSegment`
2. **Add Component** → `PieSlice` (yeni script!)
3. **Add Component** → `SpinWheelSegment`

### 2. PieSlice Ayarları
Inspector'da PieSlice component:
| Parametre | Değer |
|-----------|-------|
| Color | Beyaz (script otomatik değiştirir) |
| Fill Angle | 45 (8 segment için) |
| Rotation Angle | 0 (script ayarlar) |
| Segments | 20 (kenar yumuşaklığı) |
| Inner Radius | 0.15 (ortada boşluk için) |

### 3. Text Ekle (Opsiyonel)
1. PieSegment altına → **UI → Text - TextMeshPro**
   - **Name**: `SkillNameText`
   - Font Size: 12
   - Alignment: Center
   - Color: White

### 4. SpinWheelSegment Bağlantıları
Inspector'da SpinWheelSegment:
| Field | Değer |
|-------|-------|
| Pie Slice | PieSlice component (aynı objede) |
| Skill Name Text | SkillNameText (alt obje) |
| Icon Image | Boş bırak |

### 5. Prefab Kaydet
- `PieSegment`'i **Assets/Prefabs/UI/** klasörüne sürükle
- Hierarchy'deki objeyi sil

### 6. SpinWheelManager'a Ata
- `Segment Prefab` field'ına yeni `PieSegment` prefab'ı ata
- `Wheel Radius` = 150 (veya istediğin boyut)

---

## Test Et
1. Play Mode → **C tuşu**
2. Renkli pizza dilimleri göreceksin! 🍕

---

## Renk Paleti
Script otomatik renk atıyor:
- **Aktif Skill**: Kırmızı, Mavi, Yeşil, Turuncu, Mor, Sarı, Cyan, Pembe
- **Pasif Skill**: Açık Yeşil, Açık Mor, Gri, Açık Mavi
