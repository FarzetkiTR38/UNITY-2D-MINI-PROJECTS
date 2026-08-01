namespace ArrowSwarm.Audio
{
    using UnityEngine;

    /// <summary>
    /// ScriptableObject that holds references to all sound effect clips.
    /// Assigned in the Inspector for easy management.
    /// </summary>
    [CreateAssetMenu(fileName = "SFXLibrary", menuName = "ArrowSwarm/SFXLibrary")]
    public class SFXLibrary : ScriptableObject
    {
        [Header("Arrow")]
        [SerializeField] private AudioClip _arrowFire;
        [SerializeField] private AudioClip _arrowWrong;
        [SerializeField] private AudioClip _rainbowArrow;

        [Header("Mob")]
        [SerializeField] private AudioClip _mobHit;
        [SerializeField] private AudioClip _mobDie;
        [SerializeField] private AudioClip _mobFinish;

        [Header("Level")]
        [SerializeField] private AudioClip _levelWin;
        [SerializeField] private AudioClip _levelLose;

        [Header("UI")]
        [SerializeField] private AudioClip _buttonClick;
        [SerializeField] private AudioClip _heartBreak;

        [Header("Music")]
        [SerializeField] private AudioClip _menuBGM;
        [SerializeField] private AudioClip _gameBGM;

        // --- Properties ---
        public AudioClip ArrowFire => _arrowFire;
        public AudioClip ArrowWrong => _arrowWrong;
        public AudioClip RainbowArrow => _rainbowArrow;
        public AudioClip MobHit => _mobHit;
        public AudioClip MobDie => _mobDie;
        public AudioClip MobFinish => _mobFinish;
        public AudioClip LevelWin => _levelWin;
        public AudioClip LevelLose => _levelLose;
        public AudioClip ButtonClick => _buttonClick;
        public AudioClip HeartBreak => _heartBreak;
        public AudioClip MenuBGM => _menuBGM;
        public AudioClip GameBGM => _gameBGM;
    }
}
