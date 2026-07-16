using GridPuzzle.Core;
using GridPuzzle.InputSystem;
using GridPuzzle.UI;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

namespace GridPuzzle.Gameplay
{
    
    public class GameManager : MonoBehaviour
    {
        [SerializeField] private int width = 4;
        [SerializeField] private int height = 4;
        [SerializeField] private int maxUndos = 3;
        [SerializeField] private int maxMoves = 0;
        [SerializeField] private int maxPeekUses = 2;
        [SerializeField] private SwipeInputController inputController;
        [SerializeField] private UIManager uiManager;
        [SerializeField] private AudioClip undoClip;
        [SerializeField] private AudioClip peekClip;
        private AudioSource audioSource;

        private GridModel _grid;
        private HistoryManager _history;
        private ComboScoreSystem _combo;
        private PeekPowerUp _peek;
        private bool _roundOver;

        private void Awake()
        {
            _grid = new GridModel(width, height);
            _history = new HistoryManager(_grid, maxUndos);
            _combo = new ComboScoreSystem(_grid);
            _peek = new PeekPowerUp(_grid, maxPeekUses);

            _grid.OnTileMoved += record => uiManager.OnTileMoved(record);
            _grid.OnBoardSolved += HandleSolved;
            _peek.OnHint += (tileValue, dir) => uiManager.HighlightHintTile(tileValue);

            inputController.OnSwipe += HandleSwipe;
        }

        private void Start() => NewGame();

        public void NewGame()
        {
            _grid.ShuffleSolvable(seed: (int)System.DateTime.Now.Ticks);
            _history.Clear();
            _combo.Reset();
            _peek.Reset(maxPeekUses);
            _roundOver = false;
            uiManager.OnBoardReset(_grid);
            uiManager.SetMoveLimit(maxMoves);
            uiManager.UpdateHud(_grid.MoveCount, _combo.Score, _combo.ComboStreak, _history.RemainingUndos, maxMoves);
            uiManager.UpdatePeekUses(_peek.RemainingUses);
        }

        public void OnPeekButtonPressed()
        {
            if (_roundOver) return;
            if (!_peek.TryUse()) return;
            PlayPeekSound();
            uiManager.UpdatePeekUses(_peek.RemainingUses);
        }

        private void HandleSwipe(Direction dir)
        {
            if (_roundOver) return; // lock input once won/lost until NewGame()
            bool moved = _grid.TryMove(dir);
            if (!moved) return;

            uiManager.UpdateHud(_grid.MoveCount, _combo.Score, _combo.ComboStreak, _history.RemainingUndos, maxMoves);

            // Step-limit ("rigid step framework") mode: lose if out of moves and not solved.
            if (maxMoves > 0 && _grid.MoveCount >= maxMoves && !_grid.IsSolved())
            {
                _roundOver = true;
                uiManager.OnGameOver();
            }
        }

        public void OnUndoButtonPressed()
        {
            if (_roundOver) return;
            if (_history.Undo())
                uiManager.OnBoardReset(_grid); // simplest correct redraw after undo
            PlayUndoSound();
            uiManager.UpdateHud(_grid.MoveCount, _combo.Score, _combo.ComboStreak, _history.RemainingUndos, maxMoves);
        }

        private void HandleSolved()
        {
            _roundOver = true;
            uiManager.OnSolved();
        }

        public void PlayUndoSound()
        {
            if (undoClip != null)
                audioSource.PlayOneShot(undoClip);
        }

        public void PlayPeekSound()
        {
            if (peekClip != null)
                audioSource.PlayOneShot(peekClip);
        }

        private void OnDestroy()
        {
            inputController.OnSwipe -= HandleSwipe;
            _history.Dispose();
        }
    }
}
