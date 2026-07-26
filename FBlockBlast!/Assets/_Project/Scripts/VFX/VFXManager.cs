using System.Collections;
using UnityEngine;
using NeonGalaxy.Core;
using NeonGalaxy.Data;

namespace NeonGalaxy.VFX
{
    /// <summary>
    /// Central VFX coordinator. Listens to GameEvents and spawns visual effects.
    /// Manages particle pools, screen flash, camera shake, and hit-stop.
    /// 
    /// Now supports 3-tier effect intensity:
    /// - Standard line clear: per-cell burst + sweep line + subtle flash
    /// - Nova Cross: cross shockwave + zoom punch + strong glow
    /// - Board Full Clear: supernova explosion + mega shockwave + screen distortion
    /// 
    /// Art direction: "satisfying line clears, combo escalation feedback,
    /// Nova Cross premium effect, not visually noisy."
    /// </summary>
    public class VFXManager : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private VFXConfigSO config;

        [Header("References")]
        [SerializeField] private ScreenFlash screenFlash;
        [SerializeField] private Camera mainCamera;

        // Pools
        private VFXPool _placementPool;
        private VFXPool _lineClearPool;
        private VFXPool _comboPool;
        private VFXPool _cellBurstPool;
        private VFXPool _sweepLinePool;

        // Previously one-shot, now pooled for performance
        private VFXPool _novaCrossPool;
        private VFXPool _boardClearPool;

        private Vector3 _cameraOriginalPos;
        private Coroutine _shakeCoroutine;
        private Coroutine _hitStopCoroutine;

        private void Awake()
        {
            if (mainCamera == null)
                mainCamera = Camera.main;

            if (mainCamera != null)
                _cameraOriginalPos = mainCamera.transform.localPosition;
        }

        private void Start()
        {
            InitializePools();
            InitializeProceduralFallbacks();
        }

        private void OnEnable()
        {
            GameEvents.OnPiecePlaced += HandlePiecePlaced;
            GameEvents.OnLinesCleared += HandleLinesCleared;
            GameEvents.OnNovaCross += HandleNovaCross;
            GameEvents.OnComboUpdated += HandleComboUpdated;
            GameEvents.OnGameOver += HandleGameOver;
            GameEvents.OnCellClearing += HandleCellClearing;
            GameEvents.OnBoardCleared += HandleBoardCleared;
        }

        private void OnDisable()
        {
            GameEvents.OnPiecePlaced -= HandlePiecePlaced;
            GameEvents.OnLinesCleared -= HandleLinesCleared;
            GameEvents.OnNovaCross -= HandleNovaCross;
            GameEvents.OnComboUpdated -= HandleComboUpdated;
            GameEvents.OnGameOver -= HandleGameOver;
            GameEvents.OnCellClearing -= HandleCellClearing;
            GameEvents.OnBoardCleared -= HandleBoardCleared;
        }

        private void Update()
        {
            // Auto-return finished particle effects
            _placementPool?.RecycleFinished();
            _lineClearPool?.RecycleFinished();
            _comboPool?.RecycleFinished();
            _cellBurstPool?.RecycleFinished();
            _sweepLinePool?.RecycleFinished();
            _novaCrossPool?.RecycleFinished();
            _boardClearPool?.RecycleFinished();
        }

        // ── Initialization ──────────────────────────────────────

        private void InitializePools()
        {
            if (config == null) return;

            if (config.placementVFXPrefab != null)
                _placementPool = new VFXPool(config.placementVFXPrefab, transform, config.placementPoolSize);

            // If procedural fallback is ON, ignore the placeholder prefab for line clear
            if (config.lineClearVFXPrefab != null && !config.useProceduralFallback)
                _lineClearPool = new VFXPool(config.lineClearVFXPrefab, transform, config.lineClearPoolSize);

            if (config.comboVFXPrefab != null)
                _comboPool = new VFXPool(config.comboVFXPrefab, transform, config.comboPoolSize);

            if (config.novaCrossVFXPrefab != null && !config.useProceduralFallback)
                _novaCrossPool = new VFXPool(config.novaCrossVFXPrefab, transform, 2);

            if (config.boardClearVFXPrefab != null && !config.useProceduralFallback)
                _boardClearPool = new VFXPool(config.boardClearVFXPrefab, transform, 1);
        }

        /// <summary>
        /// Creates procedural particle systems when config prefab slots are empty.
        /// This ensures effects always work even without imported assets.
        /// </summary>
        private void InitializeProceduralFallbacks()
        {
            if (config == null) return;
            Debug.Log($"[VFXManager] InitializeProceduralFallbacks: useProceduralFallback = {config.useProceduralFallback}");

            // Cell Burst pool (always procedural — these are new)
            var cellBurstPrefab = LineClearVFXFactory.CreateCellBurstPS(transform, config);
            _cellBurstPool = new VFXPool(cellBurstPrefab, transform, config.cellBurstPoolSize);
            Debug.Log($"[VFXManager] Cell Burst Pool Created: Size={config.cellBurstPoolSize}, PrefabValid={cellBurstPrefab != null}");

            // Sweep Line pool (always procedural — these are new)
            var sweepLinePrefab = LineClearVFXFactory.CreateSweepLinePS(transform, config);
            _sweepLinePool = new VFXPool(sweepLinePrefab, transform, config.sweepLinePoolSize);
            Debug.Log($"[VFXManager] Sweep Line Pool Created: Size={config.sweepLinePoolSize}, PrefabValid={sweepLinePrefab != null}");

            // Enhanced Line Clear (force if fallback enabled and we skipped prefab loading)
            if (_lineClearPool == null)
            {
                var lineClearPrefab = LineClearVFXFactory.CreateEnhancedLineClearPS(transform);
                _lineClearPool = new VFXPool(lineClearPrefab, transform, config.lineClearPoolSize);
            }

            // Nova Cross procedural prefab (always generate if fallback enabled)
            var novaCrossPrefab = LineClearVFXFactory.CreateNovaCrossPS(transform);
            _novaCrossPool = new VFXPool(novaCrossPrefab, transform, 2);

            // Board Clear procedural prefab (always generate if fallback enabled)
            var boardClearPrefab = LineClearVFXFactory.CreateBoardClearPS(transform, config);
            _boardClearPool = new VFXPool(boardClearPrefab, transform, 1);
        }

        // ── Event Handlers ──────────────────────────────────────

        private void HandlePiecePlaced(PieceInstance piece, Vector2Int gridPos)
        {
            if (config == null) return;

            // Subtle placement particle burst
            _placementPool?.Get(GetWorldPosFromGrid(gridPos));

            // Minimal camera shake for tactile feedback
            TriggerShake(config.placementShakeIntensity, config.shakeDuration * 0.5f);
        }

        private void HandleLinesCleared(int[] rows, int rowCount, int[] cols, int colCount)
        {
            if (config == null) return;

            // ── Spawn line clear particles along cleared rows/columns ──
            for (int i = 0; i < rowCount; i++)
            {
                Vector3 worldPos = GetWorldPosFromGrid(new Vector2Int(4, rows[i])); // Center of row
                _lineClearPool?.Get(worldPos);

                // Sweep line effect along the row
                SpawnSweepLine(rows[i], true);
            }
            for (int i = 0; i < colCount; i++)
            {
                Vector3 worldPos = GetWorldPosFromGrid(new Vector2Int(cols[i], 4)); // Center of column
                _lineClearPool?.Get(worldPos);

                // Sweep line effect along the column
                SpawnSweepLine(cols[i], false);
            }

            // ── Screen Flash (Removed per user request) ──
            // if (screenFlash != null)
            // {
            //     screenFlash.Flash(config.lineClearFlashColor, config.flashDuration);
            // }

            // ── Brief hit-stop for impact feel ──
            TriggerHitStop(config.hitStopDuration, config.hitStopTimeScale);
        }

        /// <summary>
        /// Spawns a per-cell particle burst at the clearing cell's position.
        /// Color matches the block being destroyed for visual coherence.
        /// </summary>
        private void HandleCellClearing(Vector3 worldPos, Color cellColor)
        {
            if (config == null) return;

            // Neon-enhanced color: increase brightness for additive glow
            Color neonColor = new Color(
                Mathf.Min(cellColor.r * 1.3f, 1f),
                Mathf.Min(cellColor.g * 1.3f, 1f),
                Mathf.Min(cellColor.b * 1.3f, 1f),
                1f
            );

            var ps = _cellBurstPool?.Get(worldPos, neonColor);
            Debug.Log($"[VFXManager] HandleCellClearing at {worldPos}, Color {neonColor}. ParticleSystem retrieved: {ps != null}");
        }

        private void HandleNovaCross()
        {
            if (config == null) return;

            Vector3 center = GetWorldPosFromGrid(new Vector2Int(4, 4));

            // ── Premium Nova Cross: now pooled for performance ──
            _novaCrossPool?.Get(center);

            // ── Screen flash for Nova Cross (Removed per user request) ──
            // if (screenFlash != null)
            // {
            //     screenFlash.Flash(config.novaCrossFlashColor, config.flashDuration * 2f);
            // }

            // ── Strong camera shake ──
            TriggerShake(config.novaCrossShakeIntensity, config.shakeDuration * 1.5f);
        }

        /// <summary>
        /// Handles the MEGA board clear celebration.
        /// Supernova explosion + mega shockwave + sparkle rain + screen distortion.
        /// </summary>
        private void HandleBoardCleared()
        {
            if (config == null) return;

            // 8x8 tahtanın tam merkezi (3,3) ile (4,4) hücrelerinin tam ortasıdır.
            Vector3 p1 = GetWorldPosFromGrid(new Vector2Int(3, 3));
            Vector3 p2 = GetWorldPosFromGrid(new Vector2Int(4, 4));
            Vector3 center = (p1 + p2) * 0.5f;

            // ── Supernova particle explosion (Pooled) ──
            _boardClearPool?.Get(center);

            // ── MEGA screen flash (Removed per user request) ──
            // if (screenFlash != null)
            // {
            //     screenFlash.MegaFlash(config.boardClearFlashColor, 0.6f);
            // }

            // ── Strong camera shake ──
            TriggerShake(config.boardClearShakeIntensity, config.shakeDuration * 2.5f);

            // ── Extended hit-stop for dramatic impact ──
            TriggerHitStop(config.hitStopDuration * 2f, config.hitStopTimeScale);
        }

        private void HandleComboUpdated(int comboLevel)
        {
            if (config == null || comboLevel < 2) return;

            // Combo escalation VFX at combo milestones (2, 4, 6, 8, 10...)
            if (comboLevel % 2 == 0)
            {
                Vector3 center = GetWorldPosFromGrid(new Vector2Int(4, 4));
                _comboPool?.Get(center);
            }
        }

        private void HandleGameOver(int finalScore)
        {
            if (config == null) return;

            // Game over shatter effect
            if (config.gameOverVFXPrefab != null)
            {
                Vector3 center = GetWorldPosFromGrid(new Vector2Int(4, 4));
                var ps = Instantiate(config.gameOverVFXPrefab, center, Quaternion.identity, transform);
                ps.Play(true);
                Destroy(ps.gameObject, ps.main.duration + ps.main.startLifetime.constantMax + 1f);
            }

            // Strong camera shake
            TriggerShake(config.gameOverShakeIntensity, config.shakeDuration * 2f);
        }

        // ── Sweep Line ──────────────────────────────────────────

        /// <summary>
        /// Spawns a sweep line particle that travels across the cleared line.
        /// For rows: starts at left, moves right.
        /// For columns: starts at bottom, moves up.
        /// </summary>
        private void SpawnSweepLine(int lineIndex, bool isRow)
        {
            if (_sweepLinePool == null || config == null) return;

            Vector3 startPos, endPos;

            if (isRow)
            {
                startPos = GetWorldPosFromGrid(new Vector2Int(0, lineIndex));
                endPos = GetWorldPosFromGrid(new Vector2Int(7, lineIndex));
            }
            else
            {
                startPos = GetWorldPosFromGrid(new Vector2Int(lineIndex, 0));
                endPos = GetWorldPosFromGrid(new Vector2Int(lineIndex, 7));
            }

            var ps = _sweepLinePool.Get(startPos);
            if (ps != null)
            {
                StartCoroutine(SweepLineRoutine(ps, startPos, endPos, config.sweepLineDuration));
            }
        }

        private IEnumerator SweepLineRoutine(ParticleSystem ps, Vector3 start, Vector3 end, float duration)
        {
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                // Ease-out for accelerating start
                t = 1f - (1f - t) * (1f - t);
                ps.transform.position = Vector3.Lerp(start, end, t);
                yield return null;
            }

            ps.transform.position = end;
            // Let the pool's RecycleFinished handle returning when emission stops
        }

        // ── Camera Shake ────────────────────────────────────────

        private void TriggerShake(float intensity, float duration)
        {
            if (mainCamera == null || intensity <= 0f) return;

            if (_shakeCoroutine != null)
            {
                StopCoroutine(_shakeCoroutine);
                mainCamera.transform.localPosition = _cameraOriginalPos;
            }

            _shakeCoroutine = StartCoroutine(ShakeRoutine(intensity, duration));
        }

        private IEnumerator ShakeRoutine(float intensity, float duration)
        {
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float dampening = 1f - (elapsed / duration);
                float x = Random.Range(-intensity, intensity) * dampening;
                float y = Random.Range(-intensity, intensity) * dampening;

                mainCamera.transform.localPosition = _cameraOriginalPos + new Vector3(x, y, 0f);
                yield return null;
            }

            mainCamera.transform.localPosition = _cameraOriginalPos;
            _shakeCoroutine = null;
        }

        // ── Hit Stop ────────────────────────────────────────────

        private void TriggerHitStop(float duration, float timeScale)
        {
            if (duration <= 0f) return;

            if (_hitStopCoroutine != null)
            {
                StopCoroutine(_hitStopCoroutine);
                Time.timeScale = 1f;
            }

            _hitStopCoroutine = StartCoroutine(HitStopRoutine(duration, timeScale));
        }

        private IEnumerator HitStopRoutine(float duration, float timeScale)
        {
            Time.timeScale = timeScale;
            yield return new WaitForSecondsRealtime(duration);
            Time.timeScale = 1f;
            _hitStopCoroutine = null;
        }

        // ── Utility ─────────────────────────────────────────────

        private Vector3 GetWorldPosFromGrid(Vector2Int gridPos)
        {
            // Approximate world position from grid coordinates
            // This uses the same layout math as BoardController
            float cellSize = 1.0f;
            float cellSpacing = 0.05f;
            float totalCell = cellSize + cellSpacing;
            float halfWidth = (8 * cellSize + 7 * cellSpacing) / 2f;
            float halfHeight = halfWidth;

            float x = -halfWidth + gridPos.x * totalCell + cellSize / 2f;
            float y = -halfHeight + gridPos.y * totalCell + cellSize / 2f;

            return new Vector3(x, y, 0f);
        }

        /// <summary>
        /// Gets the maximum duration across a ParticleSystem and all its children.
        /// Used to calculate safe Destroy timing for instantiated effects.
        /// </summary>
        private float GetMaxDuration(ParticleSystem root)
        {
            float max = 0f;
            foreach (var ps in root.GetComponentsInChildren<ParticleSystem>())
            {
                float dur = ps.main.duration + ps.main.startLifetime.constantMax;
                if (dur > max) max = dur;
            }
            return max;
        }
    }
}
