# 🎮 FVampireSurvivors - Complete Game Documentation

Vampire Survivors tarzı roguelike survivor oyunu. Unity 2D ile geliştirilmiştir.

---

# 📁 PROJE YAPISI

```
Assets/Scripts/
├── Camera/                 # Kamera sistemi
│   ├── CameraController.cs # Ana kamera takip
│
├── Enemy/                  # Düşman sistemi
│   ├── EnemyController.cs      # Hareket, saldırı AI
│   ├── EnemyHealthController.cs # Can, ölüm, XP drop
│   ├── EnemyAnimation.cs       # Animasyon
│   ├── EnemySpawner.cs         # Wave spawn sistemi
│   ├── SpawnTableSO.cs         # Spawn ayarları (ScriptableObject)
│   ├── ArenaBossManager.cs     # Boss arena yönetimi
│
├── Management/             # Oyun yönetimi
│   ├── GameManager.cs      # Ana oyun döngüsü, UI
│
├── Other/                  # Skill'ler ve Projectile'lar
│   ├── [18 Aktif Skill Script]
│   ├── [Projectile Variants]
│   ├── XPOrb.cs           # XP toplama
│   ├── XPOrbGlobalSettings.cs # Magnet ayarları
│
├── Player/                 # Oyuncu sistemi
│   ├── PlayerController.cs     # Hareket
│   ├── PlayerHealthController.cs # Can
│   ├── PlayerExperience.cs     # XP ve Level
│   ├── PlayerAutoAttack.cs     # Fireball skill
│   ├── PlayerSwordSkills.cs    # Sword skill
│
├── Skill/                  # Skill yönetimi
│   ├── SkillType.cs           # Tüm skill enum
│   ├── SkillData.cs           # Skill veri yapısı
│   ├── SkillDatabaseSO.cs     # Skill database (SO)
│   ├── PassiveStats.cs        # Pasif bonus yönetimi
│   ├── PlayerSkillManager.cs  # Skill seçimi, slot limitleri
│   ├── PlayerSkillsController.cs # Aktif skill instance'ları
│
└── UI/                     # Arayüz
    ├── MinimapCamera.cs    # Minimap kamera
```

---

# 🎯 AKTİF SKİLL'LER (SİLAHLAR)

## 1. Fireball 🔥
**Script:** `PlayerAutoAttack.cs`

Otomatik ateş eden mermi. İlk skill olarak oyuna başlangıçta açılır.

| Level | Mermi | Cooldown | Hasar |
|-------|-------|----------|-------|
| 1 | 1 | 2.0s | 10 |
| 2 | 2 | 1.85s | 10 |
| 3 | 3 | 1.70s | 10 |
| 4 | 4 | 1.55s | 10 |
| 5 | 5 | 1.40s | 10 |

**Formüller:**
```csharp
projectileCount = level
cooldown = 2.0 - (level-1) * 0.15  // min 0.5
```

---

## 2. Sword ⚔️
**Script:** `PlayerSwordSkill.cs`

Oyuncunun etrafında dönen kılıç.

| Level | Kılıç Sayısı | Hasar | Dönüş Hızı |
|-------|--------------|-------|------------|
| 1 | 1 | 15 | 1x |
| 2 | 2 | 18 | 1.1x |
| 3 | 3 | 21 | 1.2x |
| 4 | 4 | 24 | 1.3x |
| 5 | 5 | 27 | 1.4x |

---

## 3. HomingMissiles 🚀
**Script:** `HomingMissiles.cs`

Düşmana kilitli füzeler.

| Level | Füze | Hasar | Fire Rate |
|-------|------|-------|-----------|
| 1 | 1 | 20 | 0.8s |
| 2 | 2 | 25 | 0.8s |
| 3 | 3 | 30 | 0.8s |
| 4 | 4 | 35 | 0.8s |
| 5 | 5 | 40 | 0.8s |

**Formüller:**
```csharp
missileCount = level  // + bonusProjectileCount
damage = 15 + (level × 5)
```

---

## 4. IceShards ❄️
**Script:** `IceShards.cs`

Düşmanları yavaşlatan buz parçaları.

| Level | Parça | Hasar | Slow Süresi |
|-------|-------|-------|-------------|
| 1 | 1 | 11 | 2.5s |
| 2 | 2 | 14 | 3.0s |
| 3 | 3 | 17 | 3.5s |
| 4 | 4 | 20 | 4.0s |
| 5 | 5 | 23 | 4.5s |

**Formüller:**
```csharp
shardCount = level  // + bonusProjectileCount
damage = 8 + (level × 3)
slowDuration = 2.0 + (level × 0.5)
slowPercent = 50%
```

---

## 5. PiercingArrows 🏹
**Script:** `PiercingArrows.cs`

Düşmanları delip geçen oklar.

| Level | Ok | Hasar | Pierce Sayısı |
|-------|-----|-------|---------------|
| 1 | 1 | 16 | 4 |
| 2 | 2 | 20 | 5 |
| 3 | 3 | 24 | 6 |
| 4 | 4 | 28 | 7 |
| 5 | 5 | 32 | 8 |

**Formüller:**
```csharp
arrowCount = max(1, level)  // + bonusProjectileCount
damage = 12 + (level × 4)
pierceCount = 3 + level
```

---

## 6. FanOfDaggers 🗡️
**Script:** `FanOfDaggers.cs`

Yelpaze şeklinde hançer fırlatır.

| Level | Hançer | Hasar | Spread Açısı |
|-------|--------|-------|--------------|
| 1 | 5 | 8 | 70° |
| 2 | 7 | 10 | 80° |
| 3 | 9 | 12 | 90° |
| 4 | 11 | 14 | 100° |
| 5 | 13 | 16 | 110° |

**Formüller:**
```csharp
daggerCount = 3 + (level × 2)  // + bonusProjectileCount
damage = 6 + (level × 2)
spreadAngle = 60 + (level × 10)
```

---

## 7. Whirlwind 🌪️
**Script:** `Whirlwind.cs`

Oyuncu etrafında sürekli dönen kasırga.

| Level | Yarıçap | Hasar/Tick | Tick Interval |
|-------|---------|------------|---------------|
| 1 | 1.50 | 8 | 0.3s |
| 2 | 1.75 | 11 | 0.3s |
| 3 | 2.00 | 14 | 0.3s |
| 4 | 2.25 | 17 | 0.3s |
| 5 | 2.50 | 20 | 0.3s |

**Formüller:**
```csharp
radius = 1.5 + (level-1) × 0.25  // × areaSizeMultiplier
damage = 5 + (level × 3)
```

---

## 8. AuraDamage ☀️
**Script:** `AuraDamage.cs`

Oyuncu etrafında hasar veren aura. Fire/Poison/Ice türleri var.

| Level | Yarıçap | Hasar/Tick | Tick Interval |
|-------|---------|------------|---------------|
| 1 | 1.50 | 5 | 0.5s |
| 2 | 1.75 | 7 | 0.5s |
| 3 | 2.00 | 9 | 0.5s |
| 4 | 2.25 | 11 | 0.5s |
| 5 | 2.50 | 13 | 0.5s |

**Formüller:**
```csharp
radius = 1.5 + (level-1) × 0.25  // × areaSizeMultiplier
damage = 3 + (level × 2)
// Ice türü: 30% slow, 1s
```

---

## 9. ShockwavePulse ⭕
**Script:** `ShockwavePulse.cs`

Patlayan dalgalar.

| Level | Yarıçap | Hasar | Cooldown |
|-------|---------|-------|----------|
| 1 | 2.0 | 15 | 2.0s |
| 2 | 2.5 | 20 | 1.8s |
| 3 | 3.0 | 25 | 1.6s |
| 4 | 3.5 | 30 | 1.4s |
| 5 | 4.0 | 35 | 1.2s |

---

## 10. ChainLightning ⚡
**Script:** `ChainLightning.cs`

Düşmanlar arasında zıplayan yıldırım.

| Level | Zincir Sayısı | Hasar | Strike Interval |
|-------|---------------|-------|-----------------|
| 1 | 3 | 23 | 1.0s |
| 2 | 4 | 31 | 1.0s |
| 3 | 5 | 39 | 1.0s |
| 4 | 6 | 47 | 1.0s |
| 5 | 7 | 55 | 1.0s |

**Formüller:**
```csharp
chainCount = 2 + level
damage = 15 + (level × 8)  // Her zincirde -2 hasar
```

---

## 11. Boomerang 🪃
**Script:** `BoomerangWeapon.cs`

360° çevresine bumerang fırlatır.

| Level | Bumerang | Hasar | Throw Interval |
|-------|----------|-------|----------------|
| 1 | 1 | 17 | 1.5s |
| 2 | 2 | 22 | 1.5s |
| 3 | 3 | 27 | 1.5s |
| 4 | 4 | 32 | 1.5s |
| 5 | 5 | 37 | 1.5s |

**Formüller:**
```csharp
boomerangCount = level
damage = 12 + (level × 5)
```

---

## 12. SpinningShuriken ⭐
**Script:** `SpinningShuriken.cs`

Oyuncu etrafında dönen shuriken'lar.

| Level | Shuriken | Hasar | Orbit Hızı |
|-------|----------|-------|------------|
| 1 | 2 | 10 | 1x |
| 2 | 3 | 12 | 1.1x |
| 3 | 4 | 14 | 1.2x |
| 4 | 5 | 16 | 1.3x |
| 5 | 6 | 18 | 1.4x |

---

## 13. ConeAttack 🔥
**Script:** `ConeAttack.cs`

Koni şeklinde alev püskürtme.

| Level | Açı | Hasar/Tick | Menzil |
|-------|-----|------------|--------|
| 1 | 45° | 5 | 3.0 |
| 2 | 55° | 7 | 3.5 |
| 3 | 65° | 9 | 4.0 |
| 4 | 75° | 11 | 4.5 |
| 5 | 85° | 13 | 5.0 |

---

## 14. MeteorShower ☄️
**Script:** `MeteorShower.cs`

Rastgele meteor düşürür.

| Level | Meteor | Hasar | Etki Yarıçapı | Interval |
|-------|--------|-------|---------------|----------|
| 1 | 1 | 40 | 1.50 | 2.5s |
| 2 | 2 | 55 | 1.75 | 2.5s |
| 3 | 3 | 70 | 2.00 | 2.5s |
| 4 | 4 | 85 | 2.25 | 2.5s |
| 5 | 5 | 100 | 2.50 | 2.5s |

**Formüller:**
```csharp
meteorCount = level  // + bonusProjectileCount
damage = 25 + (level × 15)
impactRadius = 1.5 + (level-1) × 0.25  // × areaSizeMultiplier
```

---

## 15. ExplodingProjectiles 💥
**Script:** `ExplodingProjectiles.cs`

Düşmana çarpınca patlayan mermiler.

| Level | Mermi | Patlama Hasarı | Patlama Yarıçapı |
|-------|-------|----------------|------------------|
| 1 | 1 | 20 | 1.5 |
| 2 | 2 | 28 | 1.7 |
| 3 | 3 | 36 | 1.9 |
| 4 | 4 | 44 | 2.1 |
| 5 | 5 | 52 | 2.3 |

---

## 16. LaserBeam 🔴
**Script:** `LaserBeam.cs`

Sürekli hasar veren lazer.

| Level | Genişlik | Hasar/Tick | Tick Interval |
|-------|----------|------------|---------------|
| 1 | 0.2 | 5 | 0.1s |
| 2 | 0.3 | 7 | 0.1s |
| 3 | 0.4 | 9 | 0.1s |
| 4 | 0.5 | 11 | 0.1s |
| 5 | 0.6 | 13 | 0.1s |

**Formüller:**
```csharp
width = 0.2 + (level-1) × 0.1  // × areaSizeMultiplier
damage = 3 + (level × 2)
```
**DPS (Saniyede Hasar):** Level 5 = 130 DPS (13 × 10)

---

## 17. Turret 🗼
**Script:** `Turret.cs` + `TurretBehavior.cs`

Sabit kalan taret spawn eder.

| Level | Taret | Hasar | Fire Interval |
|-------|-------|-------|---------------|
| 1 | 1 | 12 | 1.5s |
| 2 | 2 | 16 | 1.3s |
| 3 | 3 | 20 | 1.1s |
| 4 | 4 | 24 | 0.9s |
| 5 | 5 | 28 | 0.7s |

**Özellikleri:**
- Taretler arasında minimum 1.5 birim mesafe
- Otomatik en yakın düşmana hedefleme
- Hedefe dönme animasyonu

---

## 18. BlackHole 🕳️
**Script:** `BlackHole.cs` + `BlackHoleBehavior.cs`

Düşman çeken kara delik.

| Level | Yarıçap | Süre | Hasar/Tick | Çekme Gücü |
|-------|---------|------|------------|------------|
| 1 | 2.0 | 3.0s | 8 | 7 |
| 2 | 2.5 | 3.125s | 11 | 9 |
| 3 | 3.0 | 3.25s | 14 | 11 |
| 4 | 3.5 | 3.375s | 17 | 13 |
| 5 | 4.0 | 3.5s | 20 | 15 |

**Formüller:**
```csharp
radius = 2 + (level-1) × 0.5
duration = 3 + (level-1) × 0.125
damage = 5 + (level × 3)
pullForce = 5 + (level × 2)
damageInterval = 0.3s
spawnInterval = 4s
```

---

# 📊 PASİF SKİLL'LER

Tüm pasif skill'ler `PassiveStats.cs` tarafından yönetilir.

## 1. Damage (Hasar) ⚔️
Tüm skill'lere bonus hasar ekler.

| Level | Bonus Hasar | Çarpan |
|-------|-------------|--------|
| 1 | +5 | 1.1x |
| 2 | +10 | 1.2x |
| 3 | +15 | 1.3x |
| 4 | +20 | 1.4x |
| 5 | +25 | 1.5x |

**Formül:**
```csharp
bonusDamage = level × 5
damageMultiplier = 1 + (level × 0.1)
finalDamage = (baseDamage + bonusDamage) × damageMultiplier
```

---

## 2. AttackSpeed (Saldırı Hızı) ⚡
Tüm skill'lerin cooldown'unu azaltır.

| Level | Çarpan | Etki |
|-------|--------|------|
| 1 | 1.1x | %10 hızlı |
| 2 | 1.2x | %20 hızlı |
| 3 | 1.3x | %30 hızlı |
| 4 | 1.4x | %40 hızlı |
| 5 | 1.5x | %50 hızlı |

**Formül:**
```csharp
attackSpeedMultiplier = 1 + (level × 0.1)
effectiveCooldown = baseCooldown / attackSpeedMultiplier
```

---

## 3. ProjectileCount (Mermi Sayısı) 🎯
Tüm çoklu mermi skill'lerine bonus mermi.

| Level | Bonus |
|-------|-------|
| 1 | +1 |
| 2 | +2 |
| 3 | +3 |
| 4 | +4 |
| 5 | +5 |

**Formül:**
```csharp
bonusProjectileCount = level
totalProjectiles = baseCount + bonusProjectileCount
```

---

## 4. AreaSize (Alan Büyüklüğü) 📐
AoE skill'lerin yarıçapını artırır.

| Level | Çarpan | Etki |
|-------|--------|------|
| 1 | 1.15x | %15 büyük |
| 2 | 1.30x | %30 büyük |
| 3 | 1.45x | %45 büyük |
| 4 | 1.60x | %60 büyük |
| 5 | 1.75x | %75 büyük |

**Formül:**
```csharp
areaSizeMultiplier = 1 + (level × 0.15)
finalRadius = baseRadius × areaSizeMultiplier
```

---

## 5. XPGain (XP Kazanımı) 📈
XP orb'larından daha fazla XP.

| Level | Çarpan | Etki |
|-------|--------|------|
| 1 | 1.1x | %10 fazla |
| 2 | 1.2x | %20 fazla |
| 3 | 1.3x | %30 fazla |
| 4 | 1.4x | %40 fazla |
| 5 | 1.5x | %50 fazla |

**Formül:**
```csharp
xpGainMultiplier = 1 + (level × 0.1)
finalXP = baseXP × xpGainMultiplier
```

---

## 6. CriticalChance (Kritik Şansı) 🎲
Kritik vuruş şansı.

| Level | Şans |
|-------|------|
| 1 | 5% |
| 2 | 10% |
| 3 | 15% |
| 4 | 20% |
| 5 | 25% |

**Formül:**
```csharp
criticalChance = level × 0.05
```

---

## 7. CriticalDamage (Kritik Hasarı) 💥
Kritik vuruş çarpanı.

| Level | Çarpan |
|-------|--------|
| 1 | 2.25x |
| 2 | 2.50x |
| 3 | 2.75x |
| 4 | 3.00x |
| 5 | 3.25x |

**Formül:**
```csharp
criticalDamageMultiplier = 2 + (level × 0.25)
```

---

## 8. Lifesteal (Can Çalma) 🩸
Verilen hasarın bir kısmını can olarak al.

| Level | Yüzde |
|-------|-------|
| 1 | 3% |
| 2 | 6% |
| 3 | 9% |
| 4 | 12% |
| 5 | 15% |

**Formül:**
```csharp
lifestealPercent = level × 0.03
healAmount = damageDealt × lifestealPercent
```

---

## 9. HealthRegen (Can Yenileme) 💚
Saniyede can yenileme.

| Level | HP/Sn |
|-------|-------|
| 1 | 1 |
| 2 | 2 |
| 3 | 3 |
| 4 | 4 |
| 5 | 5 |

**Formül:**
```csharp
healthRegenPerSecond = level × 1
```

---

## 10. Armor (Zırh) 🛡️
Alınan hasarı azaltır.

| Level | Azaltma |
|-------|---------|
| 1 | 5% |
| 2 | 10% |
| 3 | 15% |
| 4 | 20% |
| 5 | 25% |

**Formül:**
```csharp
damageReduction = level × 0.05
finalDamageTaken = incomingDamage × (1 - damageReduction)
```

---

## 11. MoveSpeed (Hareket Hızı) 🏃
Oyuncu hareket hızını artırır.

**Yönetim:** `PlayerController.UpgradeSpeed()`

---

## 12. MaxHealth (Maksimum Can) ❤️
Maksimum can artırır.

**Yönetim:** `PlayerHealthController.UpgradeHealth()`

---

## 13. Magnet (Mıknatıs) 🧲
XP orb toplama menzilini artırır.

**Yönetim:** `XPOrbGlobalSettings.UpgradeMagnet()`

---

# 🔄 EVOLVED (EVRİMLEŞMİŞ) SKİLL'LER

İki skill max level olunca birleşerek güçlü evolved skill'e dönüşür.

| Evolved Skill | Gereksinimler | Açıklama |
|---------------|---------------|----------|
| **BeastMode** | Fireball + HealthRegen | Güçlendirilmiş ateş topu |
| **BladeStorm** | Sword + AttackSpeed | Ultra hızlı kılıç |
| **VampiricField** | AuraDamage + Lifesteal | Can çalan aura |
| **FrozenWorld** | IceShards + AreaSize | Büyük dondurucu alan |
| **MeteorFire** | MeteorShower + CriticalDamage | Kritik meteor |
| **GreedyOverlord** | XPGain + Damage | Güçlü XP farmı |
| **ImmortalForm** | HealthRegen + MaxHealth | Ölümsüz form |

---

# 👾 DÜŞMAN SİSTEMİ

## EnemyController.cs
Düşman hareketi ve saldırısı.

**Özellikler:**
- Oyuncuya doğru hareket
- Collision ile hasar verme
- Slow efekti desteği
- Knockback efekti desteği

```csharp
// Slow uygulama
enemy.ApplySlow(0.5f, 2f);  // %50 slow, 2 saniye

// Knockback uygulama
enemy.ApplyKnockback(direction, 10f, 0.2f);
```

---

## EnemyHealthController.cs
Düşman canı ve ölümü.

**Özellikler:**
- `TakeDamage(float)` - Hasar al
- Ölünce XP orb spawn
- DamageFlash efekti

---

## EnemySpawner.cs
Wave bazlı düşman spawn sistemi.

**Özellikler:**
- Level bracket sistemi (SpawnTableSO)
- Rush wave modu (hızlı spawn)
- Arena boss tetikleme
- Weighted random spawn

---

## SpawnTableSO.cs (ScriptableObject)
Spawn ayarlarını tutan veri yapısı.

**Yapı:**
```
SpawnTableSO
├── brackets[] (LevelBracket)
│   ├── minLevelInclusive
│   ├── maxLevelExclusive
│   ├── baseSpawnInterval
│   ├── rushSpeedMultiplier
│   ├── enemies[] (WeightedPrefab)
│   │   ├── prefab
│   │   └── weight
│   └── bosses[] (WeightedPrefab)
│
└── arenaBossTriggers[] (ArenaBossTrigger)
    ├── triggerLevel
    ├── arenaBossPrefab
    └── triggerOnce
```

**Örnek Bracket:**
```
Level 1-5:  interval=1.5s, enemies=[Skeleton:50, Zombie:50]
Level 6-10: interval=1.2s, enemies=[Skeleton:30, Zombie:40, Orc:30]
Level 11+:  interval=1.0s, enemies=[Zombie:30, Orc:40], bosses=[Giant:10]
```

---

# 🎥 KAMERA SİSTEMİ

## CameraController.cs
Ana kamera takip.

```csharp
// LateUpdate ile takip (player hareketten sonra)
transform.position = new Vector3(target.x, target.y, transform.z);
```

---

## MinimapCamera.cs
Minimap için ayrı kamera.

**Özellikler:**
- Oyuncuyu takip
- Farklı layer culling
- RenderTexture'a render

---

# 📦 SLOT SİSTEMİ

`PlayerSkillManager.cs` ile yönetilir.

| Tür | Maksimum Slot |
|-----|---------------|
| Aktif Skill | 8 |
| Pasif Skill | 8 |

**Kurallar:**
- Slot doluysa sadece seçili skill'ler level up olabilir
- Yeni skill seçilemez
- Level-up seçenekleri otomatik filtrelenir

---

# 🔧 PassiveStats API

`PassiveStats.cs` singleton - tüm bonus hesaplamaları.

```csharp
// Hasar hesaplama (bonus + multiplier + crit)
int damage = PassiveStats.instance.CalculateDamage(baseDamage);

// Cooldown hesaplama
float cd = PassiveStats.instance.GetAttackInterval(baseCooldown);

// Mermi sayısı
int count = PassiveStats.instance.GetTotalProjectileCount(baseCount);

// Alan büyüklüğü
float radius = PassiveStats.instance.GetScaledArea(baseRadius);

// XP hesaplama
int xp = PassiveStats.instance.CalculateXP(baseXP);

// Lifesteal uygula
PassiveStats.instance.ApplyLifesteal(damageDealt);

// Alınan hasar hesaplama
float taken = PassiveStats.instance.CalculateDamageTaken(incomingDamage);
```

---

# 🚀 HIZLI BAŞLANGIÇ

## 1. Sahne Kurulumu
1. **Player** objesi oluştur
   - `PlayerController`
   - `PlayerHealthController`
   - `PlayerExperience`
   - `PlayerSkillsController`
   - Tag: "Player"

2. **Main Camera** objesi
   - `CameraController`

3. **GameManager** objesi
   - `GameManager`

4. **EnemySpawner** objesi
   - `EnemySpawner`
   - SpawnTableSO ata

## 2. Prefablar
- Enemy prefablarına: `EnemyController`, `EnemyHealthController`
- Projectile prefablarına: `Projectile` veya türevleri

## 3. ScriptableObject'ler
- `SkillDatabaseSO` - Skills → PlayerSkillManager
- `SpawnTableSO` - Spawn ayarları → EnemySpawner

---

# 🧪 TEST TUŞLARI (GameManager.cs)

| Tuş | İşlev |
|-----|-------|
| Alpha 1-5 | PassiveStats upgrade test |
| X | Manuel level up |

---

# 📐 MİMARİ DİYAGRAMI

```
┌─────────────────────────────────────────────────────────────────────┐
│                           GameManager                                │
│        (Oyun döngüsü, UI güncellemesi, level-up tetikleme)          │
└───────────────────────────────┬─────────────────────────────────────┘
                                │
┌───────────────┬───────────────┼───────────────┬─────────────────────┐
│               │               │               │                     │
▼               ▼               ▼               ▼                     ▼
┌─────────┐ ┌────────────┐ ┌──────────────┐ ┌─────────────┐ ┌─────────────┐
│ Player  │ │EnemySpawner│ │PlayerSkill   │ │CameraControl│ │   XPOrb     │
│Controller│ │            │ │   Manager    │ │             │ │             │
└────┬────┘ └─────┬──────┘ └──────┬───────┘ └─────────────┘ └──────┬──────┘
     │            │               │                                 │
     ▼            ▼               ▼                                 ▼
┌─────────┐ ┌──────────────┐ ┌──────────────────┐           ┌─────────────┐
│ Health  │ │EnemyController│ │PlayerSkillsCtrl │           │PlayerExper. │
│Controller│ │EnemyHealth   │ │(Skill Instances)│           │             │
└─────────┘ └──────────────┘ └────────┬─────────┘           └─────────────┘
                                      │
                                      ▼
                            ┌─────────────────┐
                            │  PassiveStats   │
                            │   (Singleton)   │
                            │                 │
                            │ • damageMulti   │
                            │ • attackSpeed   │
                            │ • projectiles   │
                            │ • areaSize      │
                            │ • xpGain        │
                            │ • critChance    │
                            │ • critDamage    │
                            │ • lifesteal     │
                            │ • healthRegen   │
                            │ • armor         │
                            └─────────────────┘
```

---

# 📝 NOTLAR

1. **Singleton'lar:** `.instance` ile erişilir
   - `PassiveStats.instance`
   - `PlayerHealthController.instance`
   - `PlayerSkillManager.instance`
   - `PlayerSkillsController.instance`
   - `XPOrbGlobalSettings.instance`

2. **Tag'ler:**
   - "Player" - Oyuncu
   - "Enemy" - Düşmanlar

3. **Layer'lar:**
   - Default - Normal objeler
   - UI - Arayüz
   - Minimap - Minimap görünürlük

4. **Debug Logları:**
   - Console'da `[Skill Name]` prefixli loglar
   - Damage, cooldown hesaplamaları izlenebilir

---

Bu döküman FVampireSurvivors projesinin tüm mekaniklerini ve sistemlerini kapsar. Güncellemeler için ilgili scriptleri inceleyin.
