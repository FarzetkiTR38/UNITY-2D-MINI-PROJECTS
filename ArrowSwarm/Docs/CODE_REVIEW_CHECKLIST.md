# Arrow Swarm — Kapsamlı Kod İncelemesi (Code Review), Mimari Denetim ve QA Test Kontrol Listesi

> **Çalışma Prensibi & Handoff Kuralı:**
> - Her adımda yalnızca **1–2 madde** ele alınır ve Unity MCP / statik kod analizi ile fiilen doğrulanır.
> - Tamamlanan maddeler `- [x]` olarak işaretlenir ve bulgular/kanıtlar hemen altına kaydedilir.
> - Dosyanın en altındaki **"📌 AKTİF DURUM VE SIRADAKİ ADIM"** bölümü her adımda güncellenir; böylece bağlam yenilense veya model değişse bile süreç kaldığı yerden kesintisiz devam eder.

---

## 1. Unity MCP & Derleme Sağlığı

- [x] **1.1. Konsol Hata, Uyarı ve Eksik Referans (Missing Component / Broken Reference) Denetimi (`read_console`, `manage_scene`, Asset Taraması)**
  - **Durum:** GEÇTİ (Kritik derleme/referans hatası yok; 1 adet log standardı bulgusu Kategori 2 için not edildi).
  - **Bulgular:**
    - `read_console` (`error`, `warning`): **0 Error**, **0 Warning** (`isPlaying=False, isCompiling=False`).
    - `manage_scene(action="validate")` (`GameScene`): **0 sorun** (`missingScripts: 0`, `brokenPrefabs: 0`).
    - **Prefab Taraması (`Assets/_Project` altındaki 15 Prefab):** `MissingScripts: 0`, `BrokenRefs: 0`.
    - **ScriptableObject Taraması (`Assets/_Project` altındaki 154 SO):** `BrokenRefs: 0`.
    - **Sahne YAML & GUID Taraması (Build Settings'deki 6 sahne):** `BootScene` (72 MonoBehaviour), `MainMenuScene` (1329 MonoBehaviour), `GameScene` (145 MonoBehaviour), `MapTestScene` (22 MonoBehaviour), `ShapesScene` (380 MonoBehaviour), `ColorShiftScene` (380 MonoBehaviour) — toplam **2.328** bileşen kaydında `NullScript: 0`, `UnresolvedGuid: 0`.
    - **Log Hijyeni Notu (Kategori 2'de ele alınacak):** `Assets/_Project/Scripts/Core/LevelGenerator.cs:352` satırında `[Conditional("UNITY_EDITOR")]` olmadan doğrudan `Debug.Log("[ArrowSwarm DIAG] ...")` çağrısı bulunuyor (hem `[ArrowSwarm]` standart önekine hem de Editor-only log kuralına aykırı). Ayrıca `GoogleMobileAdsService.cs:51` üzerinden SDK kaynaklı `"Placeholder .ctor"` konsol çıktısı mevcut.

- [x] **1.2. Unity Test Runner Testlerinin Çalıştırılması (`run_tests` — EditMode & PlayMode)**
  - **Durum:** TAMAMLANDI (Test Runner sorunsuz çalışıyor ancak kayıtlı NUnit testi bulunmuyor).
  - **Bulgular:**
    - **EditMode Test Koşumu (`job_id: 99e0c30107314b328cb285dd30b57861`):** `resultState: Passed` (`total: 0, passed: 0, failed: 0, skipped: 0`, süre: `0.0079s`).
    - **PlayMode Test Koşumu (`job_id: 479f4db7251a416f8ed0b7be1af7af62`):** `resultState: Passed` (`total: 0, passed: 0, failed: 0, skipped: 0`, süre: `0.00018s`).
    - **Mimari/QA Tespit:** Projede Unity Test Framework (`[Test]` / `[UnityTest]`) altında tanımlı birim testi (unit test) bulunmuyor. Bunun yerine doğrulama araçları `Assets/_Project/Scripts/Modes/Editor/` (`Quality*Verification.cs`, `PuzzleVerification.cs`, `ShapesBalanceVerification.cs` vb.), `Assets/_Project/Scripts/UI/Editor/` (`UiRollout*Verification.cs`, `AudioRevisionVerification.cs`) altında özel Editor doğrulama scriptleri ve runtime `SolvabilityTester.cs` olarak kurgulanmış.

---

## 2. Mimari & Kod Kalitesi (`.agents/AGENTS.md` Kurallarına Uygunluk)

- [x] **2.1. Klasör Yapısı, Namespace Uyumu ve XML Dokümantasyon Denetimi**
  - **Durum:** ÇOĞUNLUKLA UYGUN (Runtime namespace ve encapsulation %100 uyumlu; az sayıda XML yorum eksikliği ve 2 dosya yerleşim/namespace sapması mevcut).
  - **Bulgular:**
    - **Klasör Yapısı (`Assets/_Project/Scripts/`):** Üçüncü parti paketler (`Assets/GoogleMobileAds/`, `Assets/TextMesh Pro/`) haricinde projeye ait **517 C# dosyasının tamamı** (**182 Runtime**, **335 Editor**) `Assets/_Project/Scripts/` altında yer alıyor (`0` klasör dışı script). Küçük not: `Assets/_Project/Scripts/UI/UiRollout01NativeFrameRunner.cs` `#if UNITY_EDITOR` ile sarılı bir Editor doğrulama yardımcısı olmasına rağmen `UI/Editor/` yerine `UI/` altında duruyor.
    - **Namespace Uyumu (`ArrowSwarm.<Subfolder>`):** **182 Runtime scriptinin tamamı (%100)** klasör yapısıyla birebir uyumlu `ArrowSwarm.<Subfolder>` namespace'ine sahip. Editor tarafında yalnızca `Assets/_Project/Scripts/Editor/BootSceneBuilder.cs` `ArrowSwarm.Editor` yerine `ArrowSwarm.Core.Editor` namespace'ini kullanıyor.
    - **Encapsulation (`[SerializeField] private` Kuralı):** Reflection taramasında hiçbir `MonoBehaviour` veya `ScriptableObject` sınıfında `public` instance field tespit edilmedi (**0 ihlal**). `public` alanlar yalnızca DTO / serileştirilebilir veri yapılarında (`PlayerData`, `LevelStarData`, `LanguageDefinition`, `LeaderboardEntry`, `WaveData`, `LevelParams`, `ArrowPlacement` vb.) kullanılmış.
    - **XML Dokümantasyon (`/// <summary>`):**
      - Birincil runtime tiplerinden yalnızca **1 sınıfta** eksik: `Assets/_Project/Scripts/Data/PlayerData.cs:123` (`LevelStarData`).
      - Public metotlarda **13 metotta** `/// <summary>` eksik: `MapSceneController.cs` (`StartTutorialTest`, `StopTutorialTest`), `DataManager.cs` (`GetLevelStars`, `SetLevelStars`), `UnityCloudService.cs` (`SavePlayerData`, `LoadLeaderboard`), `GameModeSelection.cs` (`DisplayName`), `PuzzleCatalog.cs` (`Get`), `PuzzleRules.cs` (`Inside`, `WallId`, `MatchesWall`), `UiRollout01NativeFrameRunner.cs` (`Begin`, `End`).

- [x] **2.2. Manager Sınıflarında `ArrowSwarm.Utils.Singleton<T>` Kullanımı**
  - **Durum:** KISMİ UYGUNLUK / İYİLEŞTİRME GEREKLİ (24 Manager `Singleton<T>` kullanıyor; 1 adet `OnDestroy` method-hiding hatası ve 2 manuel `Instance` istisnası tespit edildi).
  - **Bulgular:**
    - **`Singleton<T>` Kullanan Sınıflar (24 Sınıf):** `GameManager`, `LevelManager`, `SceneTransitionManager`, `ThemeManager`, `InputManager`, `HapticManager`, `DataManager`, `LeaderboardManager`, `LocalizationManager`, `AudioManager`, `AdManager`, `MockAdService`, `GridManager`, `PathManager`, `ArrowSpawner`, `MobSpawner`, `TipManager`, `FreezeManager`, `DebugManager`, `ParticleManager`, `ScreenEffects`, `ArrowEffects`, `FloatingTextManager`, `TouchEffectManager`. Hiçbiri `Awake()` metodunu override edip `base.Awake()` akışını bozmamış; tamamı `OnSingletonAwake()` kullanıyor.
    - **Bulgu 2.2.A (`FreezeManager.OnDestroy()` Method Hiding — Orta/Yüksek Önem):** `Assets/_Project/Scripts/Skills/FreezeManager.cs:52` satırında `protected override void OnDestroy()` yerine `private void OnDestroy()` yazıldığı için `Singleton<FreezeManager>.OnDestroy()` gizlenmiş ve `base.OnDestroy()` çağrılmamış (`_instance = null` temizliği çalışmıyor). Buna karşın `MobSpawner`, `HapticManager`, `LevelManager`, `SceneTransitionManager` (`Materials.cs`), `ArrowSpawner` ve `AdManager` (`Requests.cs`) doğru şekilde `protected override void OnDestroy()` + `base.OnDestroy()` kullanıyor.
    - **Bulgu 2.2.B (Koşulsuz `DontDestroyOnLoad` & `SetParent(null)`):** `Assets/_Project/Scripts/Utils/Singleton.cs:61-65` içinde `ShouldPersist()` desteği olmadığı için tüm `Singleton<T>` sınıfları koşulsuz olarak sahne hiyerarşisinden koparılıp (`SetParent(null)`) `DontDestroyOnLoad` yapılıyor. Bu durum `GameScene` içi yöneticilerin (`GridManager`, `PathManager`, `ArrowSpawner`, `MobSpawner`, `ParticleManager`, `ScreenEffects`, `FreezeManager`, `TipManager`) diğer sahnelere geçildiğinde de bellekte kalıcı olmasına yol açıyor.
    - **Bulgu 2.2.C (`Singleton<T>` Dışı Manager'lar):** `TutorialManager` (`Assets/_Project/Scripts/Tutorial/TutorialManager.cs:17`) ve `AdminManager` (`Assets/_Project/Scripts/Debug/AdminManager.cs:15`) `MonoBehaviour`'dan türeyip manuel `public static ... Instance` tanımlıyor (`AdminManager` içinde duplicate guard ve `OnDestroy` temizliği yok). `OverlayManager` (`Assets/_Project/Scripts/UI/OverlayManager.cs:12`) statik `Instance` olmadan `Canvas_Overlay` altında event dinleyerek sahne-içi çalışıyor (uygun). Ek olarak `CameraController` (`HasInstance` içinde fallback sahne taraması var) ve `ProfileSetupModalUI` de Canvas/sahne hiyerarşisi nedeniyle manuel `Instance` kullanıyor.
- [x] **2.3. Sistemler Arası Bağımsızlık (Tight Coupling vs. C# Static Event Mimarisi)**
  - **Durum:** KISMİ UYGUNLUK / İYİLEŞTİRME GEREKLİ (33 static event mevcut, ancak runtime'da 90+ doğrudan `.Instance` çağrısı ve çift tetikleme mevcut).
  - **Bulgular:**
    - **C# Static Event Altyapısı (33 Static Event):** `GameManager`, `LevelManager`, `Arrow`, `ArrowSpawner`, `Mob`, `MobSpawner`, `GridManager`, `PathManager`, `FreezeManager`, `TipManager`, `ThemeManager`, `DataManager`, `LocalizationManager`, `LeaderboardManager`, `CameraController` ve `PuzzleSession` üzerinde toplam **33 adet `public static event`** tanımlanmış. `AudioManager`, `HapticManager`, `ScreenEffects`, `GameHUD`, `OverlayManager`, `GridVisualizer` ve `PathVisualizer` bu event'leri dinleyerek gevşek bağlı çalışıyor.
    - **Bulgu 2.3.A (Oyun Varlıklarından Efekt Singleton'larına Doğrudan Bağ):** `Arrow.cs:206` (`ArrowEffects.Instance.PlayLaunchBurst`), `ArrowMovement.cs:289` (`ArrowEffects.Instance.PlayClashSparks`), `MobHealth.cs:56` (`FloatingTextManager.Instance.ShowDamage`) ve `MobVisuals.cs:91` (`ParticleManager.Instance.SpawnMobDeathEffect`) satırlarında oyun nesneleri doğrudan efekt yöneticilerini çağırıyor (ilgili static event'ler zaten mevcut olmasına rağmen).
    - **Bulgu 2.3.B (Çift Kanallı Bildirim — Event + Doğrudan `GameManager` Çağrısı):** `Arrow.cs:169-170` (`OnArrowClicked` + `game.HandleWrongClick()`), `Arrow.cs:209-210` (`OnArrowFiredEvent` + `GameManager.Instance.HandleArrowFired()`), `ArrowSpawner.cs:197-198` (`OnAllArrowsFired` + `GameManager.Instance.HandleAllArrowsFired()`) ve `Mob.cs:128-129` (`OnMobFinished` + `GameManager.Instance?.HandleMobReachedFinish()`) satırlarında hem statik event yayınlanıyor hem `GameManager` metodu doğrudan tetikleniyor.
    - **Bulgu 2.3.C (`FindFirstObjectByType` & Çift Tutorial Başlatma Hatası):** `LevelManager.cs:168-181` içinde `FindFirstObjectByType<TutorialManager>()` ile `tutorial.StartTutorial()` doğrudan çağrılıyor; ancak `TutorialManager.cs:45-67` zaten `LevelManager.OnLevelReady` event'ine abone olup `StartTutorial()` çalıştırdığı için **Level 1 açılışında `StartTutorial()` aynı karede iki kez tetikleniyor**. Ayrıca `TutorialManager.cs:85, 227, 262, 306, 333` satırlarında `FindFirstObjectByType<GameHUD>()` ile UI doğrudan kontrol ediliyor.

- [x] **2.4. `ObjectPool<T>` Kullanımı ve Runtime `Instantiate`/`Destroy` Denetimi**
  - **Durum:** KISMİ UYGUNLUK / PERFORMANS BULGUSU (Havuz sınıfları mevcut ancak her seviye yüklemesinde sıfırlanıp yeniden Instantiate ediliyor).
  - **Bulgular:**
    - **Mevcut Havuzlar:** `ArrowSpawner` (`ObjectPool<Arrow>`, initial: 20, max: 100), `MobSpawner` (`ObjectPool<Mob>`, initial: 20, max: 150) ve `PuzzleSession` (`ObjectPool<PuzzleArrowView>`) jenerik `ArrowSwarm.Utils.ObjectPool<T>` kullanıyor. `FloatingTextManager` (`Queue<FloatingTextItem>`, initial: 12), `ParticleManager` (`Dictionary<string, Queue<ParticleSystem>>`), `LevelSelectUI` (`_buttonPool`, 20 buton) ve `LeaderboardUI` (`_entryRows`) ise kendi havuz/önbellek yapılarını kullanıyor.
    - **Bulgu 2.4.A (Her Seviyede Havuzların Yıkılıp Yeniden `Instantiate` Edilmesi — Yüksek Önem):** `LevelManager.cs:131, 254-255` akışında önce `CleanupLevel()` ile oklar/moblar havuza geri bırakılıyor (`Release`), hemen ardından `InitializeSystems()` içinde `ArrowSpawner.InitializePool()` ve `MobSpawner.InitializePool()` çağrılıyor. `ArrowSpawner.cs:65-83` ve `MobSpawner.cs:80-86` içindeki `InitializePool()` metotları `_arrowPool.Clear()` / `_mobPool.Clear()` çağırarak **havuzdaki tüm (20+20 = 40 adet) hazır GameObject'i `Destroy()` ediyor** ve `new ObjectPool<...>()` ile **baştan 40 adet `Instantiate()` yapıyor**.
    - **Bulgu 2.4.B (`GridVisualizer` & `PathVisualizer` Seviye Başı `Destroy`/`new GameObject`):** `GridVisualizer.cs:70-91, 115` her seviye başı ve tema değişiminde tüm grid noktalarını (80–144 adet `Dot_x_y`) `Destroy()` edip `new GameObject` + `AddComponent<SpriteRenderer>` ile yeniden oluşturuyor; `PathVisualizer.cs:84-90, 114` de her seviyede portal objelerini `Destroy()` + `new GameObject` ile baştan yaratıyor.
- [x] **2.5. 200 Satır Sınırı, Single Responsibility Principle (SRP) ve Loglama Kuralları Denetimi**
  - **Durum:** İHLALLER TESPİT EDİLDİ (182 runtime dosyasının 28'i 200 satır sınırını aşıyor; 12 dosyada `[Conditional("UNITY_EDITOR")]` korumasız runtime `Debug.Log` mevcut).
  - **Bulgular:**
    - **Bulgu 2.5.A (200 Satır Sınırı & SRP İhlalleri — 28/182 Runtime Script):** `.agents/AGENTS.md` içindeki *"Max 200 lines per script — split if larger"* kuralını aşan 28 runtime dosyası tespit edildi:
      1. `Assets/_Project/Scripts/Core/LevelGenerator.cs` (**2.803 satır** — tersine söküm/reverse disassembly, yol büyütme, bağımlılık derinleştirme, kilitlenme çözümü, renk atama ve tanılama mantıklarını tek sınıfta toplayan devasa monolit),
      2. `Assets/_Project/Scripts/Arrow/ArrowVisuals.cs` (**900 satır** — gövde çizimi, ok başı, büyüme/küçülme animasyonları, gökkuşağı efekti ve renk geçişleri tek sınıfta),
      3. `Assets/_Project/Scripts/Data/DataManager.cs` (**503 satır**),
      4. `Assets/_Project/Scripts/Arrow/ArrowMovement.cs` (**459 satır**), `Assets/_Project/Scripts/UI/MainMenuUI.cs` (**457 satır**), `Assets/_Project/Scripts/Camera/CameraController.cs` (**446 satır**), `Assets/_Project/Scripts/UI/GameHUD.cs` (**431 satır**), `Assets/_Project/Scripts/UI/SettingsUI.cs` (**413 satır**), `Assets/_Project/Scripts/UI/ProfileSetupModalUI.cs` (**409 satır**), `Assets/_Project/Scripts/Core/SceneTransitionManager.cs` (**407 satır**), `Assets/_Project/Scripts/Core/DifficultyCalculator.cs` (**407 satır**),
      5. 201–400 satır aralığındaki diğer 17 dosya: `TutorialHandUI.cs` (357), `MobSpawner.cs` (355), `TutorialManager.cs` (351), `PauseMenuUI.cs` (336), `MapSceneController.cs` (328), `TouchEffectManager.cs` (320), `UnityCloudService.cs` (312), `GridManager.cs` (302), `LevelManager.cs` (289), `GameManager.cs` (287), `HapticManager.cs` (276), `Arrow.cs` (269), `GameOverUI.cs` (255), `ArrowSpawner.cs` (254), `SolvabilityTester.cs` (231), `LevelCompleteUI.cs` (219), `PathManager.cs` (205).
      - *Olumlu Not:* Yeni eklenen `Puzzle*`, `AudioManager.*`, `LeaderboardManager.*`, `LeaderboardUI.*`, `LevelSelectUI.*` ve `AdManager.*` modülleri `partial class` ve küçük yardımcı sınıflarla 200 satır sınırına tam uyumlu yazılmış.
    - **Bulgu 2.5.B (Loglama Kuralları — `[Conditional("UNITY_EDITOR")]` ve `[ArrowSwarm]` Öneki):** `.agents/AGENTS.md` kuralına göre debug logları `[Conditional("UNITY_EDITOR")]` ile sarılmalı ve `[ArrowSwarm]` öneki taşımalıdır. Ancak aşağıdaki runtime dosyalarında doğrudan (korumasız) `Debug.Log` çağrıları mevcuttur ve Android/Release derlemelerinde çalışıp string allocation üretecektir:
      - `Assets/_Project/Scripts/Core/LevelGenerator.cs:352` (`Debug.Log($"[ArrowSwarm DIAG] {LastDiag}")` — hem `[Conditional("UNITY_EDITOR")]` dışında hem de standart `[ArrowSwarm]` yerine `[ArrowSwarm DIAG]` öneki kullanıyor),
      - `Assets/_Project/Scripts/Ads/GoogleMobileAdsService.cs:54, 84`, `Assets/_Project/Scripts/Core/AutoBootstrapper.cs:42`, `Assets/_Project/Scripts/Core/SceneTransitionManager.cs:288, 326, 399`, `Assets/_Project/Scripts/Debug/AdminManager.cs:122, 172`, `Assets/_Project/Scripts/Debug/SolvabilityTester.cs:51-58, 88, 93, 113-117, 131, 136, 140`, `Assets/_Project/Scripts/Localization/LocalizationManager.cs:82`, `Assets/_Project/Scripts/Tutorial/TutorialOverlayPresentation.cs:89`, `Assets/_Project/Scripts/UI/MainMenuUI.cs:431`, `Assets/_Project/Scripts/UI/ProfileSetupModalUI.cs:355, 387`.

- [x] **2.6. `Update()` / `LateUpdate()` / `FixedUpdate()` İçinde GC Allocation Denetimi**
  - **Durum:** ÇOĞUNLUKLA UYGUN (`Update`/`LateUpdate` sıcak döngüleri temiz; `OnGUI`, 1 LINQ kullanımı ve Coroutine'lerde 20 adet cache'lenmemiş `new WaitForSeconds` tespit edildi).
  - **Bulgular:**
    - **Sıcak Döngüler (`Update` / `LateUpdate` / `FixedUpdate`):** 182 runtime sınıfının yalnızca 20'sinde `Update`/`LateUpdate` tanımlı (`FixedUpdate` kullanan sınıf yok: `0`). Çekirdek oynanış döngüleri (`ArrowMovement.Update`, `PathFollower.Update`, `PortalVisualEffect.Update`, `UIButtonFeedback.Update`, `CameraController.LateUpdate`) önceden ayrılmış (pre-allocated) tamponlar (`_trajectory`, `_bodyPointsBuffer`) kullanıyor; kare başına `GetComponent`, LINQ veya `new` referans tipi tahsisi yapmıyor.
    - **Bulgu 2.6.A (`OnGUI` String Allocation):** `Assets/_Project/Scripts/Debug/SolvabilityTester.cs:167` satırındaki `GUI.Button(new Rect(20, 120, 220, 50), $"⚡ Test Sweep (Pass #{_passNumber})")` ifadesi `_showOnGUIButton` aktifken her IMGUI geçişinde (kare başına birden fazla kez) yeni string tahsis ediyor.
    - **Bulgu 2.6.B (Runtime LINQ Kullanımı):** `Assets/_Project/Scripts/Data/LeaderboardManager.Entries.cs` içinde `System.Linq` kullanılıyor (UI yenileme/sıralama esnasında geçici enumerator/liste tahsisi).
    - **Bulgu 2.6.C (Cache'lenmemiş `new WaitForSeconds` / `new WaitForSecondsRealtime` — 20 Nokta):**
      - `Assets/_Project/Scripts/Mob/MobSpawner.cs:201, 210, 321`: `InfiniteContinuousSpawnRoutine` içindeki duraklatma bekleme döngüsünde her 0.5 saniyede bir `yield return new WaitForSeconds(0.5f)` çağrılıyor; ayrıca başlangıçta `new WaitForSeconds(1.0f)` ve `GapCloseRoutine` içinde `new WaitForSeconds(duration)` üretiliyor.
      - `Assets/_Project/Scripts/Arrow/ArrowVisuals.cs:246, 890`: Her ok doğuşunda (`GrowBirthRoutine`) ve tepki animasyonunda `new WaitForSeconds` tahsis ediliyor.
      - `Assets/_Project/Scripts/Effects/ParticleManager.cs:104, 108, 112, 155`: Havuzdan çağrılan her partikül efektinde (`ReturnToPoolAfterDuration`) yeni `WaitForSeconds` nesnesi oluşturuluyor.
      - Diğer noktalar: `TutorialHandUI.cs:296, 337`, `TutorialManager.cs:247`, `LevelCompleteUI.cs:135, 157`, `PuzzlePopups.Animation.cs:89, 106`.

---

## 3. Oynanış & Procedural Seviye Mantığı

- [x] **3.1. `DifficultyCalculator` ve `LevelGenerator` Algoritma Tutarlılığı**
  - **Durum:** ALGORİTMA ÇALIŞIYOR ANCAK DOKÜMANTASYON/FORMÜL SAPMALARI VE KULLANILMAYAN PARAMETRELER MEVCUT.
  - **Bulgular:**
    - **Saf Fonksiyon & MonoBehaviour Bağımsızlığı:** `DifficultyCalculator` (`Assets/_Project/Scripts/Core/DifficultyCalculator.cs`) tamamen `static` ve `MonoBehaviour` bağımlılığı olmayan saf matematiksel fonksiyonlardan oluşuyor (`GetDifficultyTier`, `GetArrowCount`, `GetOutwardChance`, `GetMobHP`, `GetMobSpeed`, `GetSpawnInterval`, `GetTotalMobs`, `GetMinWeight`, `GetMaxWeight`, `GetMapIndex`, `GetMapScaleFactor`, `GetMobSpeedForTransit`, `GetWaveConfig`, `CalculateAll`). `LevelGenerator` (`Assets/_Project/Scripts/Core/LevelGenerator.cs`) da `static` bir sınıf olup `MonoBehaviour` referansı barındırmıyor.
    - **Bulgu 3.1.A (`LevelGenerator` Determinizm Eksikliği):** `LevelGenerator` içerisindeki rastgele yürüyüş ve aday seçimleri (`UnityEngine.Random.Range` / `Random.value`) seviye numarasına bağlı bir seed (`Random.InitState`) ile başlatılmadığı için aynı seviye numarası her çağrıda farklı bir ok dizilimi üretiyor.
    - **Bulgu 3.1.B (Mimari Doküman `difficulty_formulas.md` / Yorumlar ile Kod Arasındaki Sapmalar):**
      - **Harita Sayısı (12 vs. 5):** `difficulty_formulas.md:14`, `GameConfig.cs:123`, `MapData.cs:9, 41` ve `DifficultyCalculator.cs:361` (`LevelParams.MapIndex`) yorumlarında 5 harita (`0..4`, cyclic) yazmasına rağmen `GameConfig.asset` içinde **12 harita** (`Map 1`..`Map 12`) tanımlı ve `DifficultyCalculator.GetMapIndex()` (satır `182-209`) `0..11` aralığında indeks döndürüyor.
      - **`GetArrowCount` Formülü:** `difficulty_formulas.md:19` içinde `min(gridWidth * gridHeight * 0.8, 15 + difficultyTier * 1.5)` yazarken, `DifficultyCalculator.GetArrowCount()` (satır `27-39`) `fillRatio = Mathf.Min(0.95f, 0.75f + (level * 0.02f))` ve `avgPointsPerArrow` üzerinden `Mathf.Max(5, maxArrows)` hesaplıyor.
      - **`GetMinWeight` / `GetMaxWeight` ve `GetOutwardChance`:** `difficulty_formulas.md:29, 75-78` içinde tier tabanlı formüller (`0.65 -> 0.20` ve `min(5, 1 + floor(tier/20))`) tanımlıyken, kodda `GetOutwardChance` (satır `45-51`) `0.70f -> 0.25f` aralığını, `GetMinWeight`/`GetMaxWeight` (satır `112-167`) ise harita indeksine göre (`0..11`) sabit `switch` tablolarını (`MinWeight: 2..3`, `MaxWeight: 5..18`) kullanıyor.
      - **Hesaplanıp Runtime'da Kullanılmayan Parametreler (`WaveConfig`, `MobSpeed`, `SpawnInterval`, `DifficultyReductionOnFail`):** `DifficultyCalculator.CalculateAll()` içinde `GetWaveConfig()` (satır `234-278`), `GetMobSpeed()` ve `GetSpawnInterval()` hesaplanıp `LevelParams` içine yazılmasına rağmen `MobSpawner.StartSpawning()` (`MobSpawner.cs:113-147, 199-225`) bu üç alanı hiç kullanmıyor; hızı `GetMobSpeedForTransit(totalPathLength, transitSeconds)` ile, spawn aralığını `_desiredSpacing / _baseMobSpeed` ile yeniden hesaplıyor ve mobları dalga yerine sonsuz akışta (`InfiniteContinuousSpawnRoutine`) üretiyor. Ayrıca `LevelGenerator.cs:66` satırında `float difficultyReduction = config.DifficultyReductionOnFail;` yerel değişkene okunup metodun hiçbir yerinde kullanılmıyor.

- [x] **3.2. Çözülebilirlik Kontrolü (`SolvabilityChecker` & `PuzzleSolver`) Doğrulaması**
  - **Durum:** GEÇTİ (%100 Çözülebilirlik Doğrulandı — Unity MCP üzerinden Canlı Simülasyon Koşuldu).
  - **Bulgular:**
    - **Defense Modu (`LevelGenerator.Generate` + `SolvabilityChecker.Check`):** Unity MCP (`execute_code`) üzerinden `GameConfig.asset` kullanılarak örnek seviyeler (`Level 0, 1, 2, 5, 10, 20, 35, 50, 75, 100`) canlı olarak üretildi ve denetlendi:
      - Tüm normal seviyelerde (`1..100`) **%100 Grid Nokta Kapsaması** (`coveredPts == gridCap`: Lvl 1 `48/48`, Lvl 10 `80/80`, Lvl 20 `180/180`, Lvl 35 `375/375`, Lvl 50 `800/800`, Lvl 100 `1000/1000`),
      - **0 Çakışma (`ValidateNoOverlaps == true`)**,
      - **%100 Çözülebilirlik ve Kazanılabilirlik (`IsSolvable == true`, `IsWinnable == true`, `IsValid == true`)**,
      - Üretim deneme sayısı `1` ile `8` arasında (`MaxRegenerateAttempts = 100` sınırının çok altında).
    - **Puzzle Modları (`ShapesCatalog` & `ColorShiftCatalog` + `PuzzleSolver.Solve`):**
      - `Assets/_Project/ScriptableObjects/Puzzles/ShapesCatalog.asset`: **50/50 seviye** `PuzzleSolver.Solve` testinden hatasız geçti (`failed = 0`).
      - `Assets/_Project/ScriptableObjects/Puzzles/ColorShiftCatalog.asset`: **50/50 seviye** döndürme bütçesi dahilinde `PuzzleSolver.Solve` testinden hatasız geçti (`failed = 0`).
    - **Bulgu 3.2.A (`GenerateSimpleGridPlacements` Son Çare Fallback Riski — Düşük Önem):** `LevelGenerator.cs:136-140` satırlarında 100 normal deneme + 5 fallback denemesi başarısız olursa çağrılan `GenerateSimpleGridPlacements(width, height)` metodu `x += 2` adımıyla ilerlediği için genişliği tek sayı olan haritalarda (örn. `15x25`, `25x40`) son sütunu boş bırakıyor ve `SolvabilityChecker.Check` çağrılmadan `result.IsValid = true` döndürüyor.

- [x] **3.3. Magic Number Denetimi (`GameConfig`, `MapData` ve `PuzzleConfig` Uyumu)**
  - **Durum:** İYİLEŞTİRME GEREKLİ (`GameConfig` / `PuzzleConfig` dışına taşmış hardcoded oynanış sabitleri tespit edildi).
  - **Bulgular:**
    - **Bulgu 3.3.A (`DifficultyCalculator.CalculateAll` İçinde Hardcoded Sabitler):** `Assets/_Project/Scripts/Core/DifficultyCalculator.cs:292-293` satırlarında `0.82f` (maksimum teorik hasar oranı) ve `0.60f` (`GameConfig.WinabilityRatio` yerine) sabit yazılmış; ayrıca `GetWaveConfig` (satır `276`) `WavePauseDuration = 1.5f` sabitini kullanıyor.
    - **Bulgu 3.3.B (`LevelGenerator.cs` İçindeki Sabit Katsayılar):**
      - Satır `39`: `public static float TargetUpperWeightRatio = 0.50f;`,
      - Satır `70`: `Mathf.FloorToInt(maxGridCapacity * 0.82f)`,
      - Satır `98`: `attempt < 4 ? (placements.Count > 60 ? 0.22f : 0.18f) : (attempt < 8 ? 0.28f : 0.35f)` (başlangıçta açık ok oranı eşikleri).
    - **Bulgu 3.3.C (`MobSpawner.cs` İçindeki Sabitler ve `GameConfig` Uyumsuzlukları):**
      - `Assets/_Project/Scripts/Mob/MobSpawner.cs:201`: `GameConfig.MobSpawnDelay` (`2.5f`) yerine sabit `yield return new WaitForSeconds(1.0f)` kullanılıyor.
      - `Assets/_Project/Scripts/Mob/MobSpawner.cs:131-136`: Fallback yol uzunluğu `26f` ve mob dünya genişliği `0.8f * _mobScaleFactor` sabit.
      - `Assets/_Project/Scripts/Mob/MobSpawner.cs:174`: `DestroyAllMobs()` içinde `TakeDamage(9999)` magic number kullanılıyor (oysa `GameConfig.RainbowArrowDamage = 999`).
      - `Assets/_Project/Scripts/Mob/MobSpawner.cs:219-220`: Sonsuz mob döngüsünde `int groupIndex = _spawnedCount / 4; int mobHP = baseHP + (groupIndex * 2);` sabitleri hardcoded.
    - **Bulgu 3.3.D (`ArrowSpawner` & `LevelManager` Sabitleri):**
      - `Assets/_Project/Scripts/Arrow/ArrowSpawner.cs:83` ve `MobSpawner.cs:86`: Havuz kapasiteleri (`20, 100` ve `20, 150`) ve `ArrowSpawner.cs:124` dalga gecikmesi (`0.22f`) hardcoded (`PuzzleConfig` içinde `_poolSize = 96` ScriptableObject'ten okunurken Defense modunda sabit yazılmış).
      - `Assets/_Project/Scripts/Core/LevelManager.cs:164` (`BlockInput(0.4f)`) ve `MainMenuUI.cs:432` (`BlockInput(0.5f)`) giriş kilitleme süreleri sabit.

---

## 4. UI & Görsel Bütünlük (Scene-First Rule)

- [x] **4.1. TextMeshPro Zorunluluğu ve Legacy UI Text Kontrolü**
  - **Durum:** GEÇTİ (%100 Uyumlu — 0 Legacy Text).
  - **Bulgular:**
    - Unity MCP (`execute_code`) ile yapılan tam kapsamlı taramada:
      - **15 Prefab:** `0` adet `UnityEngine.UI.Text`, **139** adet `TMP_Text`.
      - **6 Sahne (`BootScene`: 5 TMP, `MainMenuScene`: 195 TMP, `GameScene`: 22 TMP, `MapTestScene`: 0 TMP, `ShapesScene`: 74 TMP, `ColorShiftScene`: 74 TMP):** Toplam **370** `TextMeshProUGUI` bileşeni, **`0` adet Legacy `UnityEngine.UI.Text`**.
      - **182 Runtime Script:** `UnityEngine.UI.Text` referansı barındıran script sayısı **`0`**.

- [x] **4.2. İki Canvas Mimarisi (`Canvas_HUD` / `Canvas_Overlay`) ve `CanvasGroup` Geçişleri**
  - **Durum:** ÇOĞUNLUKLA UYGUN (Oynanış sahnelerinde iki Canvas ayrımı tam; geçişlerde `CanvasGroup` + animasyon sonrası `SetActive(false)` hibrit deseni kullanılıyor).
  - **Bulgular:**
    - **İki Canvas Mimarisi (`Canvas_HUD` + `Canvas_Overlay`):**
      - `GameScene`, `ShapesScene` ve `ColorShiftScene` sahnelerinin tamamında `Canvas_HUD` (`sortingOrder = 0`, `ScreenSpaceOverlay`, `1080x1920`) ve `Canvas_Overlay` (`sortingOrder = 10`, `ScreenSpaceOverlay`, `1080x1920`) + `Canvas_SceneTransition` (`sortingOrder = 9999`) ayrımı eksiksiz uygulanmış.
      - `BootScene` (`Canvas_Boot`) ve `MainMenuScene` (`Canvas_MainMenu`) tekil sahne oldukları için kendi ana Canvas'larını kullanıyor.
    - **`CanvasGroup` vs. `SetActive` Kullanımı:**
      - `GameHUD.SetHUDVisible()` (`GameHUD.cs:370-380`), `BootLoadingUI`, `MenuPanelTransition` (`MenuPanelTransition.cs:58-90`), `UIPopupAnimator`, `PauseMenuUI`, `GameOverUI`, `LevelCompleteUI`, `TipPopupUI`, `FreezePopupUI`, `PuzzlePopups` ve `PuzzleRocketPopupUI` görünürlük ve etkileşim geçişlerini `CanvasGroup` (`alpha`, `interactable`, `blocksRaycasts`) üzerinden yönetiyor.
      - **Bulgu 4.2.A (`SetActive` ile Hibrit Kullanım Notu):** Popup'lar kapalıyken hiyerarşide deaktif (`activeSelf = false`) tutulduğu için `OverlayManager.cs:62, 73, 84, 95, 106` ve `MainMenuUI` panelleri açarken önce `gameObject.SetActive(true)` çağırıp ardından `CanvasGroup` animasyonunu başlatıyor; kapanış animasyonu bittiğinde de `MenuPanelTransition.cs:89` üzerinden `gameObject.SetActive(false)` çağrılıyor. Ayrıca `GameHUD.SetBarsVisible()` (`GameHUD.cs:309, 318`) içinde `CanvasGroup` değerleri ayarlandıktan hemen sonra `_topPanelRect.gameObject.SetActive(showTop)` çağrılması `CanvasGroup` yaklaşımını gereksiz yere bypass ediyor.

- [x] **4.3. Scene-First Visuals Kuralı Denetimi (`Awake`/`Start`/`Setup` Görsel Override Kontrolü)**
  - **Durum:** BÜYÜK ÖLÇÜDE UYGUN (Yeni UI bileşenleri Scene-First kuralına tam uyumlu; birkaç eski fallback/efekt kodunda hardcoded renk/stil ataması mevcut).
  - **Bulgular:**
    - **Scene-First Kuralına Uyumlu Modern Tasarım:**
      - `ThemedUIElement.cs`, `ThemedInputField.cs`, `DarkThemeChoiceVisual.cs`, `ModeButtonVisual.cs` ve `LevelButtonUI.States.cs` (`ApplyAuthoredStates`) tüm renk, materyal ve sprite varyantlarını `[SerializeField]` üzerinden Inspector/Scene'den alıyor; `Awake`/`Start` içinde hardcoded renk veya font ataması yapmıyor (`FontAssign = 0`).
      - `LevelButtonUI.States.cs:17-36` içinde `_unlockedState` ve `_lockedState` atanmışsa hiçbir rengi veya sprite'ı koddan ezmeden sadece ilgili state GameObject'lerini aktif/pasif yapıyor.
    - **Bulgu 4.3.A (Eski Fallback ve Efekt Kodlarındaki Hardcoded Görsel Atamalar — Düşük/Orta Önem):**
      - `Assets/_Project/Scripts/UI/HUDFeedbackEffects.cs:122-123`: Kalp kırılma animasyonunda `Color flashGold = new Color(1f, 0.95f, 0.5f, 1f)` ve `Color amberEnd = new Color(1f, 0.62f, 0.12f, 0f)` renkleri `[SerializeField]` yerine metodun içinde sabit tanımlanmış.
      - `Assets/_Project/Scripts/UI/SettingsUI.cs:374-375`: `UpdateThemeDisplay` içinde `_themeSelectorImage.sprite = targetSprite; _themeSelectorImage.color = Color.white;` ile Inspector'da verilmiş olabilecek tint rengi `Color.white` ile eziliyor (aynı durum `SettingsToggleUI.cs:110` içinde `_useColorTint == false` iken mevcut).
      - `Assets/_Project/Scripts/Tutorial/TutorialManager.cs:115-162`: Tutorial metinlerinde `<color=#FF66B2>`, `<color=#FFE066>` ve `<color=#66E0FF>` rich-text renk kodları C# string'leri içine gömülmüş.

---

## 5. Performans & Mobil (Android Portrait)

- [x] **5.1. Event Abonelikleri ve Bellek Sızıntısı (Event Leak) Denetimi**
  - **Durum:** ÇOĞUNLUKLA GÜVENLİ (182 runtime scriptinin tamamında `+=` ve `-=` eşleşmeleri incelendi; `MainMenuUI` üzerinde 1 adet lambda `RemoveListener` uyumsuzluğu tespit edildi).
  - **Bulgular:**
    - **Genel Event Hijyeni:** `AudioManager`, `HapticManager`, `GameHUD`, `PuzzleHUD`, `OverlayManager`, `SettingsUI`, `LeaderboardUI`, `LevelSelectUI`, `TipManager`, `TipHighlighter`, `MobSpawner`, `ArrowSpawner` (`ClearAllArrows` + `OnDestroy`), `LevelManager` (`CleanupLevel` + `OnDestroy`), `ThemedUIElement`, `LocalizedText`, `SafeAreaPanelFit` ve `ReferenceCanvasFit` sınıflarının tamamı abone oldukları statik event'lerden (`+=`) `OnDisable` veya `OnDestroy` içinde eksiksiz olarak çıkıyor (`-=`).
    - **Bulgu 5.1.A (`MainMenuUI.SetupButtons()` Anonim Lambda vs. `OnDestroy()` `RemoveListener` Uyumsuzluğu — Orta Önem):**
      - `Assets/_Project/Scripts/UI/MainMenuUI.cs:221, 227, 233, 239` satırlarında `_playButton`, `_leaderboardButton`, `_levelsButton` ve `_settingsButton` için `onClick.AddListener(() => { AudioManager.Instance?.PlayButtonClick(); OnPlayClicked(); })` şeklinde **anonim lambda** ifadeleri kaydediliyor; ancak `OnDestroy()` (`MainMenuUI.cs:445-448`) içinde `_playButton?.onClick.RemoveListener(OnPlayClicked)` gibi adlandırılmış metot referansları çağrılıyor. C#/UnityEvent'te `RemoveListener(OnPlayClicked)` anonim lambda delegate'ini **kaldırmaz**. Ayrıca `WireExitButtons()` (`MainMenuUI.cs:272-282`) alt panellerdeki child butonlara `BackToMain` eklerken `OnDestroy()` (satır `450-454`) yalnızca `_exitButtons` dizisini temizliyor.
    - **Bulgu 5.1.B (`ArrowSpawner` ve `LevelManager` Yaşam Döngüsü Notu — Düşük Önem):** `ArrowSpawner.SpawnArrows()` (`ArrowSpawner.cs:145-146`) ve `LevelManager.LoadLevel()` (`LevelManager.cs:159-160`) statik event aboneliklerini `OnEnable` yerine seviye başlatma metotlarında yapıyor (`OnDestroy` içinde `ClearAllArrows()` / `CleanupLevel()` ile temizleniyor, ancak bileşen `Destroy` edilmeden `SetActive(false)` yapılırsa abonelik aktif kalır).

- [x] **5.2. Mobil Güvenli Alan (Safe Area) ve Çözünürlük Ölçekleme (Android Portrait 1080x1920) Denetimi**
  - **Durum:** KISMİ UYGUNLUK / İYİLEŞTİRME GEREKLİ (`SafeAreaPanelFit` ve `ReferenceCanvasFit` mevcut, ancak `CanvasScaler.matchWidthOrHeight = 0` ve oynanış sahnelerindeki `Canvas_HUD` üst/alt barlarında Safe Area bileşeni eksik).
  - **Bulgular:**
    - **Referans Çözünürlük (`1080x1920` Portrait):** Tüm sahnelerdeki (`BootScene`, `MainMenuScene`, `GameScene`, `ShapesScene`, `ColorShiftScene`) `CanvasScaler` bileşenleri `ScaleWithScreenSize` (`m_UiScaleMode: 1`) ve `1080x1920` referans çözünürlüğüne ayarlı (`Canvas_SceneTransition` hariç tümünde `matchWidthOrHeight = 0`).
    - **Sahne Bazlı Safe Area Kapsaması:**
      - `BootScene`: `ReferenceCanvasFit` (`BootDesign`) ile güvenli alana oturtuluyor.
      - `MainMenuScene`: `ReferenceCanvasFit` (`MenuArt`) + **6 adet** `SafeAreaPanelFit` (`SafeAreaFit` x3, `LevelsSafeArea`, `ProfileSafeArea`, `ModeSafeFit`) ile tüm menü/alt paneller güvenli alan içinde ölçekleniyor.
      - `GameScene`: **2 adet** `SafeAreaPanelFit` yalnızca `Canvas_Overlay` altındaki `TipPopupPanel/Ui08SafeArea` ve `FreezePopupPanel/Ui08SafeArea` üzerinde tanımlı.
      - `ShapesScene` ve `ColorShiftScene`: **1'er adet** `SafeAreaPanelFit` (`Ui08SafeArea`) popup katmanında tanımlı.
    - **Bulgu 5.2.A (`Canvas_HUD` Üst ve Alt Barlarında Safe Area Eksikliği — Orta/Yüksek Önem):** `GameScene` (`TopBar`, `BottomBar`), `ShapesScene` ve `ColorShiftScene` içerisindeki `Canvas_HUD` elemanlarında `SafeAreaPanelFit` veya `Screen.safeArea` tabanlı bir üst/alt inset bileşeni bulunmuyor (ayrıca `GameScene` içindeki `PausePanel`, `WinPanel`, `LosePanel` üzerinde de `SafeAreaPanelFit` yok). Çentikli (notch/punch-hole) modern Android telefonlarda (`1080x2400`, `20:9`) üst HUD barının kamera deliğiyle çakışma riski vardır.
    - **Bulgu 5.2.B (`CanvasScaler` `matchWidthOrHeight = 0` Ayarı):** Tüm ana Canvas'larda `matchWidthOrHeight = 0` (sadece genişlik eşleştirme) kullanılmış. Geniş/kısa ekran oranlarında dikey taşmayı önlemek için `SafeAreaPanelFit` olmayan HUD/popup elemanlarında Safe Area koruması veya dengeli `matchWidthOrHeight` (`0.5f`) önerilir.

---

## 📊 GENEL DENETİM ÖZETİ VE ÖNCELİKLİ AKSİYON PLANI

- **Genel Durum:** 5 kategorinin tamamındaki **16/16 madde** Unity MCP araçları (`read_console`, `run_tests`, `manage_scene`, `execute_code`) ve kaynak kod incelemesiyle tamamlandı.
- **Derleme & Oynanabilirlik Sağlığı:** **0** derleme hatası, **0** Missing Script / Broken Reference, Defense örnek seviyeleri (`1..100`), `ShapesCatalog` (50/50) ve `ColorShiftCatalog` (50/50) **%100 çözülebilir**, tüm UI metinleri **%100 TextMeshPro** (`0` Legacy Text).
- **Öncelikli İyileştirme Önerileri (Önerilen Sıra):**
  1. **[Yüksek Öncelik — Yaşam Döngüsü / Performans / Doğru Çalışma]:**
     - `LevelManager.InitializeSystems()` + `ArrowSpawner`/`MobSpawner.InitializePool()`: Her seviye yüklemesinde mevcut `ObjectPool<T>` yapısını `Clear()` ile yok edip 40 nesneyi yeniden `Instantiate` etmek yerine havuzu bir kez kurup seviyeler arası `Release` ile yeniden kullanmak (`2.4.A`); `GridVisualizer` ve `PathVisualizer` nokta/portal objelerini havuzlamak (`2.4.B`).
     - `LevelManager.LoadLevel()` / `TutorialManager.HandleLevelReady()`: Level 1 açılışında `StartTutorial()` çağrısının iki kez tetiklenmesini önlemek (`2.3.C`).
     - `FreezeManager.OnDestroy()`: `private void OnDestroy()` yerine `protected override void OnDestroy()` + `base.OnDestroy()` kullanmak (`2.2.A`).
     - `GameScene`, `ShapesScene` ve `ColorShiftScene` `Canvas_HUD` (`TopBar`/`BottomBar`) ve `GameScene` overlay panellerine (`PausePanel`/`WinPanel`/`LosePanel`) Safe Area koruması eklemek (`5.2.A`).
  2. **[Orta Öncelik — Release Hijyeni / GC / Mimari Tutarlılık]:**
     - 12 runtime dosyasındaki korumasız `Debug.Log` çağrılarını (`LevelGenerator.cs:352` dahil) `[Conditional("UNITY_EDITOR")]` + `[ArrowSwarm]` standardına çekmek (`2.5.B`).
     - `MobSpawner`, `ArrowVisuals`, `ParticleManager` gibi coroutine'lerdeki 20 adet cache'lenmemiş `WaitForSeconds` nesnesini ve `LeaderboardManager.Entries` / `SolvabilityTester.OnGUI` GC tahsislerini optimize etmek (`2.6.A-C`).
     - `DifficultyCalculator.CalculateAll` (`0.60f` -> `GameConfig.WinabilityRatio`), `MobSpawner` (`1.0f` -> `GameConfig.MobSpawnDelay`, `9999`) ve `ArrowSpawner` (`20, 100`, `0.22f`) magic number'larını `GameConfig` ile hizalamak (`3.3`) ve atıl `WaveConfig` / `DifficultyReductionOnFail` kararını netleştirmek (`3.1.B`).
     - `MainMenuUI.SetupButtons()` içindeki anonim lambda dinleyicilerini adlandırılmış metotlara dönüştürerek `OnDestroy` `RemoveListener` uyumsuzluğunu gidermek (`5.1.A`).
  3. **[Düşük / Refaktör Önceliği — Kod Organizasyonu & Testler]:**
     - 200 satırı aşan 28 runtime dosyasını (özellikle `LevelGenerator.cs` 2.803 satır, `ArrowVisuals.cs` 900 satır, `DataManager.cs` 503 satır) `partial class` / yardımcı sınıflara bölmek (`2.5.A`).
     - Oyun nesnelerinden efekt singleton'larına / `GameManager`'a giden doğrudan `.Instance` çağrılarını C# static event dinleyicilerine taşımak (`2.3.A-B`) ve `Singleton<T>.ShouldPersist()` ayrımını eklemek (`2.2.B`).
     - `HUDFeedbackEffects`, `SettingsUI`, `SettingsToggleUI` ve `TutorialManager` içindeki hardcoded UI renk atamalarını/tint ezmelerini Scene-First kuralına göre Inspector/stil yapılandırmasına çekmek (`4.3.A`).
     - Mevcut Editor doğrulama akışlarına ek olarak çekirdek mantık (`DifficultyCalculator`, `SolvabilityChecker`, `PuzzleSolver`) için EditMode NUnit testleri eklemek (`1.2`).

---

## 📌 AKTİF DURUM VE SIRADAKİ ADIM

- **Tamamlanma Durumu:** **TÜM KATEGORİLER TAMAMLANDI (16/16 Madde `%100` Denetlendi)** — Kategori 1 (`1.1`–`1.2`), Kategori 2 (`2.1`–`2.6`), Kategori 3 (`3.1`–`3.3`), Kategori 4 (`4.1`–`4.3`), Kategori 5 (`5.1`–`5.2`).
- **Son Tamamlanan Adım:** **Kategori 5 — Madde 5.1 ve Madde 5.2** tamamlandı; genel denetim özeti ve öncelikli aksiyon planı eklendi.
  - **5.1:** Statik event abonelik hijyeni güçlü (`ArrowSpawner` ve `LevelManager` `OnDestroy` temizliği mevcut); ancak `MainMenuUI.SetupButtons()` anonim lambda kayıtları `OnDestroy()` içindeki `RemoveListener(...)` ile eşleşmiyor.
  - **5.2:** Tüm sahneler `1080x1920` `ScaleWithScreenSize` ayarlı ve `BootScene`/`MainMenuScene` üzerinde `ReferenceCanvasFit`/`SafeAreaPanelFit` mevcut; ancak `GameScene`, `ShapesScene` ve `ColorShiftScene` sahnelerindeki `Canvas_HUD` (`TopBar`/`BottomBar`) elemanlarında Safe Area koruması eksik.
- **Sıradaki Adım:** Kod değişikliği fazına geçilmesi istendiğinde yukarıdaki **Öncelikli Aksiyon Planı** maddeleri (1. aşamadan başlayarak) sırayla uygulanacak ve her düzeltme Unity MCP ile doğrulanacaktır.
