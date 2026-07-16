using System.Collections;
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
        [SerializeField] private Text label;
        [SerializeField] private float moveDuration = 0.12f;

        private RectTransform _rect;
        private Coroutine _moveRoutine;

        private void Awake() => _rect = GetComponent<RectTransform>();

        public void SetValue(int value) => label.text = value.ToString();

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
