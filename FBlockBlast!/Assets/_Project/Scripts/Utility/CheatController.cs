using System.Reflection;
using UnityEngine;
using UnityEngine.InputSystem;
using NeonGalaxy.Core;
using NeonGalaxy.Services;

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
            Debug.Log("[CheatController] [3] Triggering Game Over");
            
            int currentScore = 0;
            if (_gameManager != null)
            {
                FieldInfo scoreManagerField = typeof(GameManager).GetField("_scoreManager", BindingFlags.NonPublic | BindingFlags.Instance);
                if (scoreManagerField != null)
                {
                    ScoreManager sm = scoreManagerField.GetValue(_gameManager) as ScoreManager;
                    if (sm != null) currentScore = sm.TotalScore;
                }
            }

            // Force state to game over logic
            GameEvents.InvokeGameOver(currentScore);
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
    }
}
