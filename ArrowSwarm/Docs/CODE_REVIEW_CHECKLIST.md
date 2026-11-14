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
- [ ] **2.3. Sistemler Arası Bağımsızlık (Tight Coupling vs. C# Static Event Mimarisi)**
  - Bağımsız sistemler/manager'lar arasında doğrudan çapraz referanslar olup olmadığının ve iletişimin C# static event'ler üzerinden yürütülüp yürütülmediğinin denetimi.
- [ ] **2.4. `ObjectPool<T>` Kullanımı ve Runtime `Instantiate`/`Destroy` Denetimi**
  - Mob, Arrow ve sık üretilen efekt/UI öğelerinde `ObjectPool<T>` kullanımının; çalışma zamanında (runtime) ham `Instantiate` / `Destroy` çağrıları olup olmadığının incelenmesi.
- [ ] **2.5. 200 Satır Sınırı ve Single Responsibility Principle (SRP) Denetimi**
  - 200 satırı aşan sınıfların/dosyaların tespiti, sorumluluk dağılımı (SRP) ve kod kalitesi/loglama (`[Conditional("UNITY_EDITOR")]`, `[ArrowSwarm]` öneki) kurallarının kontrolü.
- [ ] **2.6. `Update()` / `LateUpdate()` / `FixedUpdate()` İçinde GC Allocation Denetimi**
  - Döngüsel metotlarda `new`, `WaitForSeconds` (cache'lenmemiş), string birleştirme (`+`, `$""`), LINQ, `GetComponent` veya boxing kaynaklı çöp üretimi (GC spike) olup olmadığının incelenmesi.

---

## 3. Oynanış & Procedural Seviye Mantığı

- [ ] **3.1. `DifficultyCalculator` ve `LevelGenerator` Algoritma Tutarlılığı**
  - `DifficultyCalculator`'ın saf (pure) statik fonksiyonlardan oluşup oluşmadığı, `LevelGenerator`'ın MonoBehaviour bağımlılığı olmadan seviye numarasından deterministik/tutarlı parametre ürettiğinin kontrolü.
- [ ] **3.2. Çözülebilirlik Kontrolü (`SolvabilityChecker` & `PuzzleSolver`) Doğrulaması**
  - Üretilen her seviyenin oynanabilir olmadan önce çözülebilirlik (solvability) ve kazanılabilirlik (winnability) kontrolünden eksiksiz geçtiğinin incelenmesi.
- [ ] **3.3. Magic Number Denetimi (`GameConfig` ve `MapData` Uyumu)**
  - Kod tabanında `GameConfig`, `MapData` veya `PuzzleConfig` ScriptableObject'leri dışına taşmış sabitlenmiş (hardcoded) magic number'ların tespiti.

---

## 4. UI & Görsel Bütünlük (Scene-First Rule)

- [ ] **4.1. TextMeshPro Zorunluluğu ve Legacy UI Text Kontrolü**
  - Sahne, prefab ve scriptlerde Legacy `UnityEngine.UI.Text` kullanımı olup olmadığının, tüm metinlerin `TextMeshPro` (`TMP_Text` / `TextMeshProUGUI`) kullandığının doğrulanması.
- [ ] **4.2. İki Canvas Mimarisi (`Canvas_HUD` / `Canvas_Overlay`) ve `CanvasGroup` Geçişleri**
  - HUD ve Overlay ayrımının incelenmesi; menü/popup gösterim-gizleme geçişlerinde `SetActive` yerine `CanvasGroup` kullanılıp kullanılmadığının kontrolü.
- [ ] **4.3. Scene-First Visuals Kuralı Denetimi (`Awake`/`Start`/`Setup` Görsel Override Kontrolü)**
  - Runtime kodunda (`Awake`, `Start`, `Setup` vb.) sahne/prefab/Inspector'da tanımlanması gereken statik UI renk, font veya sprite değerlerinin hardcoded olarak ezilip ezilmediğinin denetimi.

---

## 5. Performans & Mobil (Android Portrait)

- [ ] **5.1. Event Abonelikleri ve Bellek Sızıntısı (Event Leak) Denetimi**
  - `OnEnable` / `Awake` / `Start` içinde abone olunan (`+=`) C# static event ve UnityEvent'lerin `OnDisable` / `OnDestroy` içinde eksiksiz olarak ayrıldığının (`-=`) kontrolü.
- [ ] **5.2. Mobil Güvenli Alan (Safe Area) ve Çözünürlük Ölçekleme (Android Portrait 1080x1920) Denetimi**
  - `SafeAreaPanelFit`, `ReferenceCanvasFit`, `CanvasScaler` ayarları ve farklı dikey mobil en-boy oranlarında (1080x1920, 1080x2400 vb.) UI yerleşim/ölçekleme uyumluluğunun incelenmesi.

---

## 📌 AKTİF DURUM VE SIRADAKİ ADIM

- **Son Tamamlanan Adım:** **Kategori 2 — Madde 2.1 ve Madde 2.2** tamamlandı.
  - **2.1:** 517 scriptin tamamı `Assets/_Project/Scripts/` altında; 182 runtime scriptin tamamı `%100` doğru `ArrowSwarm.<Subfolder>` namespace'i ve `[SerializeField] private` kapsüllemesine sahip. 1 tip (`LevelStarData`) ve 13 public metotta `/// <summary>` eksikliği; 2 Editor dosyasında (`BootSceneBuilder.cs`, `UiRollout01NativeFrameRunner.cs`) küçük yerleşim/namespace sapması not edildi.
  - **2.2:** 24 Manager `Singleton<T>` ve `OnSingletonAwake()` kullanıyor. `FreezeManager.OnDestroy()` method-hiding hatası (`private void OnDestroy()` → `base.OnDestroy()` çağrılmıyor), `Singleton<T>` içinde koşulsuz `DontDestroyOnLoad` kullanımı ve `TutorialManager` / `AdminManager` manuel `Instance` istisnaları tespit edildi.
- **Sıradaki Adım (Bir Sonraki İnceleme):** **Kategori 2 — Mimari & Kod Kalitesi** altında **Madde 2.3 (Sistemler Arası Bağımsızlık: Tight Coupling vs. C# Static Event Mimarisi)** ve **Madde 2.4 (`ObjectPool<T>` Kullanımı ve Runtime `Instantiate`/`Destroy` Denetimi)** incelenecektir.
- **Kural Hatırlatması:** Kullanıcı onayı/talimatı olmadan hiçbir kod dosyası değiştirilmeyecek; her adımda yalnızca 1–2 madde incelenip bu dosya güncellenecektir.
