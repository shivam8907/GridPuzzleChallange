using GridPuzzle.Core;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

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
        [SerializeField] private TextMeshProUGUI peekUsesText;
        [SerializeField] private GameObject winPanel;
        [SerializeField] private GameObject gameOverPanel;
        [SerializeField] private float cellSize = 150f;
        [SerializeField] private AudioClip tileSwipeClip;
       
        private AudioSource audioSource;
      
        [SerializeField] private Button undoButton;
        [SerializeField] private Button peekButton;

        private readonly Dictionary<int, TileView> _tilesByValue = new Dictionary<int, TileView>();
        private int _width, _height;
        private int _moveLimit;
        private void Awake()
        {
            audioSource = GetComponent<AudioSource>();

            if (audioSource == null)
                audioSource = gameObject.AddComponent<AudioSource>();
        }

        public void OnBoardReset(GridModel model)
        {
            foreach (var tile in _tilesByValue.Values)
                if (tile) Destroy(tile.gameObject);
            _tilesByValue.Clear();

            _width = model.Width;
            _height = model.Height;
            winPanel.SetActive(false);
            if (gameOverPanel) gameOverPanel.SetActive(false);

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
            PlayTileSwipeSound();
            }
        }
        private void PlayTileSwipeSound()
        {
            if (audioSource != null && tileSwipeClip != null)
            {
                audioSource.PlayOneShot(tileSwipeClip);
            }
        }

        public void OnSolved() => winPanel.SetActive(true);

        public void OnGameOver()
        {
            if (gameOverPanel) gameOverPanel.SetActive(true);
        }

        public void SetMoveLimit(int limit) => _moveLimit = limit;

        public void HighlightHintTile(int tileValue)
        {
            if (_tilesByValue.TryGetValue(tileValue, out var view))
                view.PulseHint();
        }

        public void UpdatePeekUses(int remaining)
        {
            if (peekUsesText) peekUsesText.text = $"Peek: {remaining}";
            SetPeekButtonState(remaining > 0);
        }

        public void UpdateHud(int moves, int score, int combo, int undosLeft, int moveLimit = 0)
        {
            moveCounterText.text = moveLimit > 0 ? $"Moves: {moves}/{moveLimit}" : $"Moves: {moves}";
            scoreText.text = $"Score: {score}";
            comboText.text = combo > 1 ? $"Combo x{combo}!" : "";
            undosLeftText.text = undosLeft == int.MaxValue ? "Undo: ∞" : $"Undo: {undosLeft}";
            SetUndoButtonState(undosLeft > 0 || undosLeft == int.MaxValue);
        }

        private Vector2 IndexToPos(int index)
        {
            int x = index % _width;
            int y = index / _width;

            // Offset so the whole grid is centered on BoardRoot's origin,
            // instead of anchoring tile (0,0) directly on BoardRoot's center.
            float offsetX = -(_width - 1) * cellSize * 0.5f;
            float offsetY = (_height - 1) * cellSize * 0.5f;

            return new Vector2(offsetX + x * cellSize, offsetY - y * cellSize);
        }

        public void SetUndoButtonState(bool enabled)
        {
            if (undoButton != null)
                undoButton.interactable = enabled;
        }

        public void SetPeekButtonState(bool enabled)
        {
            if (peekButton != null)
                peekButton.interactable = enabled;
        }
       
    }
}
