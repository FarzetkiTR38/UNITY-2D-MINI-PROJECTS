using UnityEngine;

namespace NeonGalaxy.Data
{
    /// <summary>
    /// Audio configuration: clip references, volume, and pitch settings.
    /// Audio direction: "ambient cosmic + soft synth arcade",
    /// "long-session friendly", "satisfying but not harsh SFX".
    /// Create instances via: Create → NeonGalaxy → Audio Config.
    /// </summary>
    [CreateAssetMenu(fileName = "AudioConfig", menuName = "NeonGalaxy/Audio Config", order = 21)]
    public class AudioConfigSO : ScriptableObject
    {
        [Header("UI Sounds")]
        public AudioClip uiClick;
        public AudioClip uiBack;
        public AudioClip uiNavigate;

        [Header("Gameplay — Piece Interaction")]
        public AudioClip piecePickup;
        public AudioClip piecePlace;
        public AudioClip pieceReturn;

        [Header("Gameplay — Line Clear")]
        public AudioClip lineClear;
        public AudioClip novaCross;

        [Header("Gameplay — Combo")]
        public AudioClip comboIncrement;
        public AudioClip comboBreak;

        [Header("Gameplay — Game Flow")]
        public AudioClip gameOver;
        public AudioClip newBestScore;
        public AudioClip reviveSuccess;
        public AudioClip batchReady;

        [Header("Meta")]
        public AudioClip levelUp;
        public AudioClip achievementUnlock;
        public AudioClip coinEarned;
        public AudioClip purchaseSuccess;

        [Header("Music")]
        [Tooltip("Main gameplay background music loop. Should be ambient cosmic synth.")]
        public AudioClip gameplayMusic;
        [Tooltip("Home screen background music loop.")]
        public AudioClip homeMusic;

        [Header("Pitch Variation")]
        [Tooltip("Random pitch variation for gameplay SFX (e.g., 0.05 = ±5%).")]
        [Range(0f, 0.2f)]
        public float pitchVariation = 0.05f;

        [Tooltip("Pitch increase per combo level for escalation feel.")]
        [Range(0f, 0.1f)]
        public float comboPitchEscalation = 0.02f;

        [Tooltip("Maximum pitch multiplier cap for combo escalation.")]
        public float maxComboPitch = 1.4f;

        [Header("Volume Defaults")]
        [Range(0f, 1f)]
        public float defaultSFXVolume = 0.8f;
        [Range(0f, 1f)]
        public float defaultMusicVolume = 0.5f;
    }
}
