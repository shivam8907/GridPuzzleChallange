using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using GridPuzzle.Core;
using TMPro;

namespace GridPuzzle.UI
{
   
    public class UIManager : MonoBehaviour, IGridObserver
    {
        [SerializeField] private RectTransform boardRoot;
        [SerializeField] private TileView tilePrefab;
        [SerializeField] private TextMeshProUGUI moveCounterText;
        [SerializeField] private TextMeshProUGUI scoreText;
        [SerializeField] private TextMeshProUGUI comboText;
        [SerializeField] private TextMeshProUGUI undosLeftText;
        [SerializeField] private GameObject winPanel;
        [SerializeField] private float cellSize = 150f;

        private readonly Dictionary<int, TileView> _tilesByValue = new Dictionary<int, TileView>();
        private int _width, _height;

        public void OnBoardReset(GridModel model)
        {
            foreach (var tile in _tilesByValue.Values)
                if (tile) Destroy(tile.gameObject);
            _tilesByValue.Clear();

            _width = model.Width;
            _height = model.Height;
            winPanel.SetActive(false);

            for (int i = 0; i < _width * _height; i++)
            {
                int value = model.GetCell(i);
                if (value == 0) continue;

                var view = Instantiate(tilePrefab, boardRoot);
                view.SetValue(value);
                view.SetGridPosition(IndexToPos(i), cellSize, animate: false);
                _tilesByValue[value] = view;
            }
        }

        public void OnTileMoved(MoveRecord record)
        {
            if (_tilesByValue.TryGetValue(record.MovedTileValue, out var view))
            {
                Vector2 target = IndexToPos(record.BlankIndexBefore); // moved INTO the old blank slot
                view.SetGridPosition(target, cellSize, animate: true);
            }
        }

        public void OnSolved() => winPanel.SetActive(true);

        public void UpdateHud(int moves, int score, int combo, int undosLeft)
        {
            moveCounterText.text = $"Moves: {moves}";
            scoreText.text = $"Score: {score}";
            comboText.text = combo > 1 ? $"Combo x{combo}!" : "";
            undosLeftText.text = undosLeft == int.MaxValue ? "Undo: ∞" : $"Undo: {undosLeft}";
        }

        private Vector2 IndexToPos(int index)
        {
            int x = index % _width;
            int y = index / _width;
            return new Vector2(x * cellSize, -y * cellSize);
        }
    }
}
