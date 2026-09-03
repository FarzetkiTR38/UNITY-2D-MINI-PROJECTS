namespace ArrowSwarm.Audio
{
    using UnityEngine;

    /// <summary>
    /// ScriptableObject holding all audio clip assets for the game.
    /// Easily configured and assigned via the Unity Inspector.
    /// </summary>
    [CreateAssetMenu(fileName = "SFXLibrary", menuName = "ArrowSwarm/SFXLibrary")]
    public class SFXLibrary : ScriptableObject
    {
        [Header("--- Arrows ---")]
        [Tooltip("Standard arrow fired sound.")]
        [SerializeField] private AudioClip _arrowFire;

        [Tooltip("Blocked arrow colliding / return sound.")]
        [SerializeField] private AudioClip _arrowWrong;

        [Tooltip("Last arrow or special rainbow arrow fire sound.")]
        [SerializeField] private AudioClip _rainbowArrow;

        [Tooltip("Arrow hits an enemy mob along the track.")]
        [SerializeField] private AudioClip _arrowHitEnemy;

        [Header("--- Mobs & Portals ---")]
        [Tooltip("Mob damaged sound.")]
        [SerializeField] private AudioClip _mobHit;

        [Tooltip("Mob defeated / destroyed sound.")]
        [SerializeField] private AudioClip _mobDie;

        [Tooltip("Mob reached end portal / life lost.")]
        [SerializeField] private AudioClip _mobFinish;

        [Header("--- Level & Health ---")]
        [Tooltip("Level victory fanfare / jingle.")]
        [SerializeField] private AudioClip _levelWin;

        [Tooltip("Level failed / game over sound.")]
        [SerializeField] private AudioClip _levelLose;

        [Tooltip("Heart break / life penalty sound.")]
        [SerializeField] private AudioClip _heartBreak;

        [Tooltip("Star earned chime on victory screen.")]
        [SerializeField] private AudioClip _starEarn;

        [Header("--- UI & Navigation ---")]
        [Tooltip("Generic button click.")]
        [SerializeField] private AudioClip _buttonClick;

        [Tooltip("Popup panel opening swoosh.")]
        [SerializeField] private AudioClip _popupOpen;

        [Tooltip("Popup panel closing swoosh.")]
        [SerializeField] private AudioClip _popupClose;

        [Tooltip("Toggle switch ON/OFF sound.")]
        [SerializeField] private AudioClip _toggleSwitch;

        [Header("--- Skills ---")]
        [Tooltip("Freeze skill activated sound.")]
        [SerializeField] private AudioClip _skillFreeze;

        [Tooltip("Tips skill activated / target highlight sound.")]
        [SerializeField] private AudioClip _skillTips;

        [Header("--- Background Music (BGM) ---")]
        [Tooltip("Main menu ambient music.")]
        [SerializeField] private AudioClip _menuBGM;

        [Tooltip("Active gameplay puzzle music.")]
        [SerializeField] private AudioClip _gameBGM;

        // --- Public Getters ---
        public AudioClip ArrowFire => _arrowFire;
        public AudioClip ArrowWrong => _arrowWrong;
        public AudioClip RainbowArrow => _rainbowArrow;
        public AudioClip ArrowHitEnemy => _arrowHitEnemy;
        public AudioClip MobHit => _mobHit;
        public AudioClip MobDie => _mobDie;
        public AudioClip MobFinish => _mobFinish;
        public AudioClip LevelWin => _levelWin;
        public AudioClip LevelLose => _levelLose;
        public AudioClip HeartBreak => _heartBreak;
        public AudioClip StarEarn => _starEarn;
        public AudioClip ButtonClick => _buttonClick;
        public AudioClip PopupOpen => _popupOpen;
        public AudioClip PopupClose => _popupClose;
        public AudioClip ToggleSwitch => _toggleSwitch;
        public AudioClip SkillFreeze => _skillFreeze;
        public AudioClip SkillTips => _skillTips;
        public AudioClip MenuBGM => _menuBGM;
        public AudioClip GameBGM => _gameBGM;
    }
}
