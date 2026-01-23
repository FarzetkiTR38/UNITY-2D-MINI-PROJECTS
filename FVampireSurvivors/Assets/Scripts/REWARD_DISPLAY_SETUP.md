# 🎁 Reward Display Panel - Setup Guide

Çark döndükten sonra kazanılan skill/pasif'leri ikonlarıyla gösteren panel.

---

## 1. Reward Item Prefab Oluştur

1. **Hierarchy** → Sağ tık → **UI → Image**
   - **Name**: `RewardItem`
   - **Width**: 100, **Height**: 120

2. Altına ekle:
   - **UI → Image** (Name: `Icon`, Size: 80x80)
   - **UI → Text - TextMeshPro** (Name: `NameText`, Font Size: 14)

3. **Assets/Prefabs/UI/** klasörüne sürükle → Prefab yap

---

## 2. Reward Panel Oluştur

1. **Canvas** altında → **UI → Panel**
   - **Name**: `RewardPanel`
   - **Color**: (0, 0, 0, 0.85)

2. Altına ekle:
   ```
   RewardPanel
   ├── TitleText (TextMeshPro: "🎉 Skill Kazandın!")
   ├── RewardContainer (Empty - ödül itemları buraya spawn olur)
   └── CloseButton (Button: "Tamam")
   ```

3. Panel'i **inactive** yap

---

## 3. Script Ekle ve Bağla

1. `RewardPanel`'e **Add Component** → `SpinWheelRewardDisplay`

2. Inspector'da ata:
   | Field | Değer |
   |-------|-------|
   | Reward Panel | RewardPanel (kendisi) |
   | Reward Container | RewardContainer |
   | Reward Item Prefab | RewardItem prefab |
   | Title Text | TitleText |
   | Close Button | CloseButton |
   | Display Duration | 3 (veya 0 = manual) |
   | Skill Database | SkillDatabase asset |

---

## 4. SkillDatabase'i Bağla

**ÖNEMLİ:** İkonların görünmesi için:
- `Skill Database` field'ına projendeki **SkillDatabaseSO** asset'ini sürükle
- Bu asset'te her skill'in **icon** field'ı dolu olmalı

---

## Test

1. Play Mode → **C tuşu** (çark aç)
2. **"DÖNDÜR"** butonuna bas
3. Çark durur → 1.5 saniye bekler
4. Çark kapanır → **Reward Panel** açılır (ikonlarla birlikte!)
5. 3 saniye sonra otomatik kapanır veya "Tamam" butonuyla
