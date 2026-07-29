using System.Reflection;
using UnityEngine;
using UnityEngine.InputSystem;
using NeonGalaxy.Core;
using NeonGalaxy.Services;
using NeonGalaxy.Meta;


namespace NeonGalaxy.Utility
{
    /// <summary>
    /// Test script for debugging and showcasing features.
    /// Provides keyboard shortcuts (1-9, 0) to manipulate game state, score, and currency.
    /// Attach this to the GameManager object in the scene.
    /// </summary>
    public class CheatController : MonoBehaviour
    {
        private GameManager _gameManager;

        private void Awake()
        {
            _gameManager = FindFirstObjectByType<GameManager>();
            if (_gameManager == null)
            {
                Debug.LogWarning("[CheatController] GameManager not found in scene. Cheats won't work optimally.");
            }
        }

        private void Update()
        {
            var keyboard = Keyboard.current;
            if (keyboard == null) return;

            if (keyboard.digit1Key.wasPressedThisFrame)
            {
                AddScoreCheat(1000);
            }
            if (keyboard.digit2Key.wasPressedThisFrame)
            {
                AddCoinsCheat(500);
            }
            if (keyboard.digit3Key.wasPressedThisFrame)
            {
                TriggerGameOverCheat();
            }
            if (keyboard.digit4Key.wasPressedThisFrame)
            {
                TriggerLevelUpCheat();
            }
            if (keyboard.digit5Key.wasPressedThisFrame)
            {
                TriggerAchievementCheat();
            }
            if (keyboard.digit6Key.wasPressedThisFrame)
            {
                TriggerNovaCrossCheat();
            }
            if (keyboard.digit7Key.wasPressedThisFrame)
            {
                TriggerComboCheat(5);
            }
            if (keyboard.digit8Key.wasPressedThisFrame)
            {
                ClearBoardCheat();
            }
            if (keyboard.digit9Key.wasPressedThisFrame)
            {
                ToggleTimeScaleCheat();
            }
            if (keyboard.digit0Key.wasPressedThisFrame)
            {
                // Reset scene
                UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
                Debug.Log("[CheatController] [0] Scene Reloaded");
            }
            if (keyboard.tKey.wasPressedThisFrame)
            {
                ResetTutorialCheat();
            }
            if (keyboard.cKey.wasPressedThisFrame)
            {
                ResetCosmeticsCheat();
            }

            // --- VFX Testing Cheats ---
            if (keyboard.f1Key.wasPressedThisFrame)
            {
                TestCellBurstCheat();
            }
            if (keyboard.f2Key.wasPressedThisFrame)
            {
                TestLineClearRowCheat();
            }
            if (keyboard.f3Key.wasPressedThisFrame)
            {
                TestLineClearColCheat();
            }
            if (keyboard.f4Key.wasPressedThisFrame)
            {
                TestBoardClearCheat();
            }
        }

        private void AddScoreCheat(int amount)
        {
            if (_gameManager != null)
            {
                // Use reflection to get the private ScoreManager instance
                FieldInfo scoreManagerField = typeof(GameManager).GetField("_scoreManager", BindingFlags.NonPublic | BindingFlags.Instance);
                if (scoreManagerField != null)
                {
                    ScoreManager sm = scoreManagerField.GetValue(_gameManager) as ScoreManager;
                    if (sm != null)
                    {
                        // ScoreManager doesn't have a direct AddScore, but we can hack the property via reflection
                        PropertyInfo totalScoreProp = typeof(ScoreManager).GetProperty("TotalScore", BindingFlags.Public | BindingFlags.Instance);
                        if (totalScoreProp != null && totalScoreProp.CanWrite)
                        {
                            int currentScore = (int)totalScoreProp.GetValue(sm);
                            int newScore = currentScore + amount;
                            
                            // It has a private set, so we use reflection to set it
                            totalScoreProp.SetValue(sm, newScore, null);
                            
                            // Trigger UI update
                            GameEvents.InvokeScoreChanged(newScore);
                            GameEvents.InvokeScorePopupRequested(amount, Vector3.zero);
                            Debug.Log($"[CheatController] [1] Added {amount} Score. New Score: {newScore}");
                            return;
                        }
                    }
                }
            }
            Debug.LogWarning("[CheatController] Failed to add score via reflection.");
        }

        private void AddCoinsCheat(int amount)
        {
            if (_gameManager != null)
            {
                // Use reflection to get the private SaveService instance
                FieldInfo saveServiceField = typeof(GameManager).GetField("_saveService", BindingFlags.NonPublic | BindingFlags.Instance);
                if (saveServiceField != null)
                {
                    SaveService ss = saveServiceField.GetValue(_gameManager) as SaveService;
                    if (ss != null && ss.Data != null)
                    {
                        ss.Data.coins += amount;
                        ss.MarkDirty();
                        GameEvents.InvokeCoinBalanceChanged(ss.Data.coins);
                        Debug.Log($"[CheatController] [2] Added {amount} Coins. New Balance: {ss.Data.coins}");
                        return;
                    }
                }
            }
            Debug.LogWarning("[CheatController] Failed to add coins via reflection.");
        }

        private void TriggerGameOverCheat()
        {
            Debug.Log("[CheatController] [3] Triggering Game Over (via Reviving)");
            
            if (_gameManager != null)
            {
                // Goes through Reviving state first (shows continue popup), then game over
                _gameManager.TransitionState(NeonGalaxy.Data.GameState.Reviving);
            }
            else
            {
                Debug.LogWarning("[CheatController] GameManager not found. Cannot trigger Game Over.");
            }
        }

        private void TriggerLevelUpCheat()
        {
            Debug.Log("[CheatController] [4] Triggering Level Up UI Event");
            // Just triggers the UI/VFX event
            GameEvents.InvokeLevelUp(UnityEngine.Random.Range(2, 50));
        }

        private void TriggerAchievementCheat()
        {
            Debug.Log("[CheatController] [5] Triggering Achievement Unlocked UI Event");
            // Just triggers the UI/VFX event
            GameEvents.InvokeAchievementUnlocked("ach_cheat_test");
        }

        private void TriggerNovaCrossCheat()
        {
            Debug.Log("[CheatController] [6] Triggering Nova Cross Event");
            // Fires Nova Cross to test screen shake / VFX
            GameEvents.InvokeNovaCross();
            GameEvents.InvokeScorePopupRequested(500, Vector3.zero);
        }

        private void TriggerComboCheat(int comboValue)
        {
            Debug.Log($"[CheatController] [7] Triggering Combo UI Event ({comboValue}x)");
            // Fires Combo event to test HUD punch/glow
            GameEvents.InvokeComboUpdated(comboValue);
        }

        private void ClearBoardCheat()
        {
            Debug.Log("[CheatController] [8] Clearing Board (Visual Only Hack)");
            BoardController board = FindFirstObjectByType<BoardController>();
            if (board != null)
            {
                for (int r = 0; r < 8; r++)
                {
                    for (int c = 0; c < 8; c++)
                    {
                        var cell = board.GetCellView(r, c);
                        if (cell != null) cell.SetEmpty();
                    }
                }
            }
        }

        private void ToggleTimeScaleCheat()
        {
            if (Time.timeScale > 1f)
            {
                Time.timeScale = 1f;
                Debug.Log("[CheatController] [9] Normal Speed (1x)");
            }
            else
            {
                Time.timeScale = 3f;
                Debug.Log("[CheatController] [9] Fast Forward (3x)");
            }
        }
        private void ResetTutorialCheat()
        {
            SaveService ss = null;

            // Try via ServiceLocator first
            if (Boot.ServiceLocator.Has<SaveService>())
            {
                ss = Boot.ServiceLocator.Get<SaveService>();
            }
            else if (_gameManager != null)
            {
                // Fallback: reflection from GameManager
                FieldInfo saveServiceField = typeof(GameManager).GetField("_saveService", BindingFlags.NonPublic | BindingFlags.Instance);
                if (saveServiceField != null)
                    ss = saveServiceField.GetValue(_gameManager) as SaveService;
            }

            if (ss != null && ss.Data != null)
            {
                ss.Data.hasCompletedTutorial = false;
                ss.Save();
                Debug.Log("[CheatController] [T] Tutorial sıfırlandı! Sahne yeniden yükleniyor...");

                // Reload scene so TutorialController picks it up fresh
                UnityEngine.SceneManagement.SceneManager.LoadScene(
                    UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
            }
            else
            {
                Debug.LogWarning("[CheatController] [T] SaveService bulunamadı. Tutorial sıfırlanamadı.");
            }
        }

        private void ResetCosmeticsCheat()
        {
            SaveService ss = null;

            if (Boot.ServiceLocator.Has<SaveService>())
            {
                ss = Boot.ServiceLocator.Get<SaveService>();
            }
            else if (_gameManager != null)
            {
                FieldInfo saveServiceField = typeof(GameManager).GetField("_saveService", BindingFlags.NonPublic | BindingFlags.Instance);
                if (saveServiceField != null)
                    ss = saveServiceField.GetValue(_gameManager) as SaveService;
            }

            if (ss != null && ss.Data != null)
            {
                ss.Data.unlockedCosmeticIds.Clear();
                ss.Data.purchasedProductIds.Clear();
                ss.Data.equippedBoardSkin = "default";
                ss.Data.equippedBlockSkin = "default";
                ss.Data.equippedFrame = "default";
                ss.Data.equippedTitle = "default";

                if (Boot.ServiceLocator.Has<CosmeticManager>())
                {
                    var cm = Boot.ServiceLocator.Get<CosmeticManager>();
                    MethodInfo ensureMethod = typeof(CosmeticManager).GetMethod("EnsureDefaultItemsUnlocked", BindingFlags.NonPublic | BindingFlags.Instance);
                    if (ensureMethod != null)
                    {
                        ensureMethod.Invoke(cm, null);
                    }
                }

                ss.Save();

                if (Boot.ServiceLocator.Has<CosmeticManager>())
                {
                    // Optionally call refresh, but simplest is to reload scene
                    Debug.Log("[CheatController] [C] Tüm kozmetikler ve satın alımlar sıfırlandı! Sahne yenileniyor...");
                    UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
                }
            }
            else
            {
                Debug.LogWarning("[CheatController] [C] SaveService bulunamadı. Kozmetikler sıfırlanamadı.");
            }
        }

        // ── VFX Test Methods ────────────────────────────────────

        private void TestCellBurstCheat()
        {
            Debug.Log("[CheatController] [F1] Testing Cell Burst VFX");
            // Random position near center
            Vector3 pos = new Vector3(UnityEngine.Random.Range(2f, 5f), UnityEngine.Random.Range(2f, 5f), 0f);
            Color rndColor = new Color(UnityEngine.Random.value, UnityEngine.Random.value, UnityEngine.Random.value, 1f);
            GameEvents.InvokeCellClearing(pos, rndColor);
        }

        private void TestLineClearRowCheat()
        {
            Debug.Log("[CheatController] [F2] Testing Row Line Clear VFX");
            int rndRow = UnityEngine.Random.Range(0, 8);
            GameEvents.InvokeLinesCleared(new int[] { rndRow }, 1, new int[0], 0);
        }

        private void TestLineClearColCheat()
        {
            Debug.Log("[CheatController] [F3] Testing Column Line Clear VFX");
            int rndCol = UnityEngine.Random.Range(0, 8);
            GameEvents.InvokeLinesCleared(new int[0], 0, new int[] { rndCol }, 1);
        }

        private void TestBoardClearCheat()
        {
            Debug.Log("[CheatController] [F4] Testing Board Clear (Supernova) VFX");
            GameEvents.InvokeBoardCleared();
        }
    }
}
