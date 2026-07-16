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
        [SerializeField] private SwipeInputController inputController;
        [SerializeField] private UIManager uiManager;

        private GridModel _grid;
        private HistoryManager _history;
        private ComboScoreSystem _combo;

        private void Awake()
        {
            _grid = new GridModel(width, height);
            _history = new HistoryManager(_grid, maxUndos);
            _combo = new ComboScoreSystem(_grid);

            _grid.OnTileMoved += record => uiManager.OnTileMoved(record);
            _grid.OnBoardSolved += HandleSolved;

            inputController.OnSwipe += HandleSwipe;
        }

        private void Start() => NewGame();

        public void NewGame()
        {
            _grid.ShuffleSolvable(seed: (int)System.DateTime.Now.Ticks);
            _history.Clear();
            _combo.Reset();
            uiManager.OnBoardReset(_grid);
            uiManager.UpdateHud(_grid.MoveCount, _combo.Score, _combo.ComboStreak, _history.RemainingUndos);
        }

        private void HandleSwipe(Direction dir)
        {
            if (_grid.IsSolved()) return;
            bool moved = _grid.TryMove(dir);
            if (moved)
                uiManager.UpdateHud(_grid.MoveCount, _combo.Score, _combo.ComboStreak, _history.RemainingUndos);
        }

        public void OnUndoButtonPressed()
        {
            if (_history.Undo())
                uiManager.OnBoardReset(_grid); // simplest correct redraw after undo
            uiManager.UpdateHud(_grid.MoveCount, _combo.Score, _combo.ComboStreak, _history.RemainingUndos);
        }

        private void HandleSolved()
        {
            uiManager.OnSolved();
        }

        private void OnDestroy()
        {
            inputController.OnSwipe -= HandleSwipe;
            _history.Dispose();
        }
    }
}
