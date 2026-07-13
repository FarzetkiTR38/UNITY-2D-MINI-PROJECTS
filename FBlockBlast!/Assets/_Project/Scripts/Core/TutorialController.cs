using UnityEngine;
using NeonGalaxy.Core;
using NeonGalaxy.Data;
using NeonGalaxy.Services;
using NeonGalaxy.Generation;
using System.Collections;

namespace NeonGalaxy.Core
{
    public class TutorialController : MonoBehaviour
    {
        [SerializeField] private GameManager gameManager;
        [SerializeField] private BoardController boardController;
        // Optional reference to an overlay script (will create next)
        public GameObject tutorialOverlayPrefab;
        private GameObject _overlayInstance;

        private SaveService _saveService;
        private bool _tutorialActive = false;
        private int _tutorialStep = 0;

        private void Start()
        {
            _saveService = Boot.ServiceLocator.Get<SaveService>();
            
            if (_saveService != null && !_saveService.Data.hasCompletedTutorial)
            {
                StartCoroutine(InitTutorialRoutine());
            }
        }

        private IEnumerator InitTutorialRoutine()
        {
            // Wait for GameManager to initialize the board
            yield return new WaitForSeconds(0.1f);
            
            _tutorialActive = true;
            SetupTutorialBoard();
            
            GameEvents.OnPiecePlaced += HandlePiecePlaced;
            GameEvents.OnNovaCross += HandleNovaCross;
            
            if (tutorialOverlayPrefab != null)
            {
                _overlayInstance = Instantiate(tutorialOverlayPrefab);
                // Can initialize overlay here
            }
            
            ShowStepInstruction();
        }

        private void SetupTutorialBoard()
        {
            BoardModel board = gameManager.BoardModel;
            board.Reset();
            
            int colorIndex = 1; // Pick a nice color index

            // Fill Row 2 except Col 2
            for (int c = 0; c < 8; c++)
            {
                if (c != 2) board.PlacePiece(new Vector2Int[] { Vector2Int.zero }, 2, c, colorIndex);
            }

            // Fill Col 2 except Row 2
            for (int r = 0; r < 8; r++)
            {
                if (r != 2 && !board.IsOccupied(r, 2)) board.PlacePiece(new Vector2Int[] { Vector2Int.zero }, r, 2, colorIndex);
            }

            // Fill Row 5 except Cols 5,6,7
            for (int c = 0; c < 5; c++)
            {
                if (!board.IsOccupied(5, c)) board.PlacePiece(new Vector2Int[] { Vector2Int.zero }, 5, c, colorIndex);
            }

            // Fill Col 5 except Rows 5,6,7
            for (int r = 0; r < 5; r++)
            {
                if (!board.IsOccupied(r, 5)) board.PlacePiece(new Vector2Int[] { Vector2Int.zero }, r, 5, colorIndex);
            }

            // Refresh visuals
            boardController.RefreshBoard(board);
        }

        private void HandlePiecePlaced(PieceInstance piece, Vector2Int pos)
        {
            if (!_tutorialActive) return;
            
            _tutorialStep++;
            ShowStepInstruction();
        }

        private void HandleNovaCross()
        {
            if (!_tutorialActive) return;

            // Tutorial complete!
            CompleteTutorial();
        }

        private void ShowStepInstruction()
        {
            if (_overlayInstance != null)
            {
                // We will send a message to the UI to update text based on _tutorialStep
                _overlayInstance.SendMessage("SetStep", _tutorialStep, SendMessageOptions.DontRequireReceiver);
            }
            
            switch (_tutorialStep)
            {
                case 0:
                    Debug.Log("Tutorial Step 0: Place the horizontal line to clear a row!");
                    break;
                case 1:
                    Debug.Log("Tutorial Step 1: Place the vertical line to clear a column!");
                    break;
                case 2:
                    Debug.Log("Tutorial Step 2: Place the single block in the corner to trigger a NOVA CROSS!");
                    break;
            }
        }

        private void CompleteTutorial()
        {
            _tutorialActive = false;
            
            GameEvents.OnPiecePlaced -= HandlePiecePlaced;
            GameEvents.OnNovaCross -= HandleNovaCross;

            if (_saveService != null)
            {
                _saveService.Data.hasCompletedTutorial = true;
                _saveService.Save();
            }

            if (_overlayInstance != null)
            {
                _overlayInstance.SendMessage("ShowCompletion", SendMessageOptions.DontRequireReceiver);
                Destroy(_overlayInstance, 3f);
            }

            // Switch back to normal batch generator for the next batches
            gameManager.SetBatchGenerator(new ComboFriendlyBatchGenerator());
            
            Debug.Log("Tutorial Completed! Returning to normal gameplay.");
        }

        private void OnDestroy()
        {
            GameEvents.OnPiecePlaced -= HandlePiecePlaced;
            GameEvents.OnNovaCross -= HandleNovaCross;
        }
    }
}
