using UnityEngine;
using GridPuzzle.Core;
using GridPuzzle.InputSystem;
using GridPuzzle.UI;

namespace GridPuzzle.Gameplay
{
    /// <summary>
    /// Composition root. Owns no gameplay logic itself — only wires
    /// GridModel (state) <-> SwipeInputController (input) <-> UIManager (render).
    /// This is the ONLY class that knows about all three layers at once,
    /// keeping the layers themselves mutually unaware of each other.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        [SerializeField] private int width = 4;
        [SerializeField] private int height = 4;
        [SerializeField] private int maxUndos = 3;
        [Tooltip("0 = unlimited moves. >0 = player loses if board isn't solved within this many moves.")]
        [SerializeField] private int maxMoves = 0;
        [SerializeField] private int maxPeekUses = 2;
        [SerializeField] private SwipeInputController inputController;
        [SerializeField] private UIManager uiManager;

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
            uiManager.UpdateHud(_grid.MoveCount, _combo.Score, _combo.ComboStreak, _history.RemainingUndos, maxMoves);
        }

        private void HandleSolved()
        {
            _roundOver = true;
            uiManager.OnSolved();
        }

        private void OnDestroy()
        {
            inputController.OnSwipe -= HandleSwipe;
            _history.Dispose();
        }
    }
}
