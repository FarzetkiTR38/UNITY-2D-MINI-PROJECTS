namespace ArrowSwarm.Core
{
    using ArrowSwarm.Modes;
    using UnityEngine;

    /// <summary>Adds isolated campaign preview selection to the existing map workflow.</summary>
    public partial class MapSceneController
    {
        [Header("Game Mode Preview")]
        [SerializeField] private GameModeId _previewMode = GameModeId.Defense;
        [SerializeField, Range(1,5)] private int _puzzleLevel = 1;
        public GameModeId PreviewMode => _previewMode;
        public int PuzzlePreviewLevel => _puzzleLevel;

        /// <summary>Starts the selected puzzle as a non-saving preview.</summary>
        public void StartPuzzlePreview()
        {
            if (_previewMode == GameModeId.Defense) return;
            PuzzleLaunch.Preview=true;PuzzleLaunch.PreviewMode=_previewMode;PuzzleLaunch.PreviewLevel=_puzzleLevel;
            GameModeSelection.Load(_previewMode==GameModeId.Shapes?"ShapesScene":"ColorShiftScene");
        }
    }
}
