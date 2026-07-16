using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GridPuzzle.UI
{
    /// <summary>
    /// Purely presentational. Knows nothing about GridModel, Direction, or game rules —
    /// it just draws a number and can be told "move to this position".
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public class TileView : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI label;
        [SerializeField] private Image background;
        [SerializeField] private float moveDuration = 0.12f;
        [SerializeField] private Color hintColor = new Color(1f, 0.84f, 0.2f); // warm gold pulse
        [SerializeField] private float hintPulseDuration = 0.9f;

        private RectTransform _rect;
        private Coroutine _moveRoutine;
        private Coroutine _hintRoutine;
        private Color _originalColor;
        private bool _hasOriginalColor;

        private void Awake() => _rect = GetComponent<RectTransform>();

        public void SetValue(int value) => label.text = value.ToString();

        /// <summary>Briefly flashes the tile a highlight color — used by the Peek power-up. Purely cosmetic, no gameplay effect.</summary>
        public void PulseHint()
        {
            if (!background) return;
            if (!_hasOriginalColor)
            {
                _originalColor = background.color;
                _hasOriginalColor = true;
            }
            if (_hintRoutine != null) StopCoroutine(_hintRoutine);
            _hintRoutine = StartCoroutine(HintPulseRoutine());
        }

        private IEnumerator HintPulseRoutine()
        {
            float t = 0f;
            while (t < hintPulseDuration)
            {
                t += Time.deltaTime;
                float wave = Mathf.Sin((t / hintPulseDuration) * Mathf.PI * 3f) * 0.5f + 0.5f;
                background.color = Color.Lerp(_originalColor, hintColor, wave);
                yield return null;
            }
            background.color = _originalColor;
        }

        public void SetGridPosition(Vector2 anchoredPos, float cellSize, bool animate)
        {
            if (!animate)
            {
                _rect.anchoredPosition = anchoredPos;
                return;
            }
            if (_moveRoutine != null) StopCoroutine(_moveRoutine);
            _moveRoutine = StartCoroutine(AnimateTo(anchoredPos));
        }

        private IEnumerator AnimateTo(Vector2 target)
        {
            Vector2 start = _rect.anchoredPosition;
            float t = 0f;
            while (t < moveDuration)
            {
                t += Time.deltaTime;
                _rect.anchoredPosition = Vector2.Lerp(start, target, t / moveDuration);
                yield return null;
            }
            _rect.anchoredPosition = target;
        }
    }
}
