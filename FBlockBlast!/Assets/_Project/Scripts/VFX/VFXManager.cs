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
        }

        private void OnEnable()
        {
            GameEvents.OnPiecePlaced += HandlePiecePlaced;
            GameEvents.OnLinesCleared += HandleLinesCleared;
            GameEvents.OnNovaCross += HandleNovaCross;
            GameEvents.OnComboUpdated += HandleComboUpdated;
            GameEvents.OnGameOver += HandleGameOver;
        }

        private void OnDisable()
        {
            GameEvents.OnPiecePlaced -= HandlePiecePlaced;
            GameEvents.OnLinesCleared -= HandleLinesCleared;
            GameEvents.OnNovaCross -= HandleNovaCross;
            GameEvents.OnComboUpdated -= HandleComboUpdated;
            GameEvents.OnGameOver -= HandleGameOver;
        }

        private void Update()
        {
            // Auto-return finished particle effects
            _placementPool?.RecycleFinished();
            _lineClearPool?.RecycleFinished();
            _comboPool?.RecycleFinished();
        }

        // ── Initialization ──────────────────────────────────────

        private void InitializePools()
        {
            if (config == null) return;

            if (config.placementVFXPrefab != null)
                _placementPool = new VFXPool(config.placementVFXPrefab, transform, config.placementPoolSize);

            if (config.lineClearVFXPrefab != null)
                _lineClearPool = new VFXPool(config.lineClearVFXPrefab, transform, config.lineClearPoolSize);

            if (config.comboVFXPrefab != null)
                _comboPool = new VFXPool(config.comboVFXPrefab, transform, config.comboPoolSize);
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

            // Spawn line clear particles along cleared rows/columns
            for (int i = 0; i < rowCount; i++)
            {
                Vector3 worldPos = GetWorldPosFromGrid(new Vector2Int(4, rows[i])); // Center of row
                _lineClearPool?.Get(worldPos);
            }
            for (int i = 0; i < colCount; i++)
            {
                Vector3 worldPos = GetWorldPosFromGrid(new Vector2Int(cols[i], 4)); // Center of column
                _lineClearPool?.Get(worldPos);
            }

            // Screen flash for line clears (subtle)
            if (screenFlash != null)
            {
                screenFlash.Flash(config.lineClearFlashColor, config.flashDuration);
            }

            // Brief hit-stop for impact feel
            TriggerHitStop(config.hitStopDuration, config.hitStopTimeScale);
        }

        private void HandleNovaCross()
        {
            if (config == null) return;

            // Premium Nova Cross effect
            if (config.novaCrossVFXPrefab != null)
            {
                Vector3 center = GetWorldPosFromGrid(new Vector2Int(4, 4));
                var ps = Instantiate(config.novaCrossVFXPrefab, center, Quaternion.identity, transform);
                ps.Play(true);
                Destroy(ps.gameObject, ps.main.duration + ps.main.startLifetime.constantMax);
            }

            // Strong screen flash for Nova Cross (premium)
            if (screenFlash != null)
            {
                screenFlash.Flash(config.novaCrossFlashColor, config.flashDuration * 2f);
            }

            // Strong camera shake
            TriggerShake(config.novaCrossShakeIntensity, config.shakeDuration * 1.5f);
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
    }
}
