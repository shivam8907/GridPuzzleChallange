using System;
using UnityEngine;
using GridPuzzle.Core;

namespace GridPuzzle.InputSystem
{
    /// <summary>
    /// Tracks pointer/touch down->up vector and resolves it into a discrete Direction.
    /// Works with mouse (editor/testing) and touch (device) via Unity's Input class.
    /// Emits a plain C# event so GameManager (not this class) decides what happens next.
    /// </summary>
    public class SwipeInputController : MonoBehaviour
    {
        [SerializeField] private float minSwipeDistancePixels = 50f;
        [SerializeField] private float maxSwipeTimeSeconds = 0.75f;

        public event Action<Direction> OnSwipe;

        private Vector2 _startPos;
        private float _startTime;
        private bool _tracking;

        private void Update()
        {
#if UNITY_EDITOR || UNITY_STANDALONE
            HandleMouse();
#else
            HandleTouch();
#endif
        }

        private void HandleMouse()
        {
            if (Input.GetMouseButtonDown(0)) BeginTrack(Input.mousePosition);
            else if (Input.GetMouseButtonUp(0) && _tracking) EndTrack(Input.mousePosition);
        }

        private void HandleTouch()
        {
            if (Input.touchCount == 0) return;
            Touch t = Input.GetTouch(0);
            switch (t.phase)
            {
                case TouchPhase.Began: BeginTrack(t.position); break;
                case TouchPhase.Ended:
                case TouchPhase.Canceled:
                    if (_tracking) EndTrack(t.position);
                    break;
            }
        }

        private void BeginTrack(Vector2 pos)
        {
            _startPos = pos;
            _startTime = Time.time;
            _tracking = true;
        }

        private void EndTrack(Vector2 endPos)
        {
            _tracking = false;
            float elapsed = Time.time - _startTime;
            Vector2 delta = endPos - _startPos;

            if (delta.magnitude < minSwipeDistancePixels) return;
            if (elapsed > maxSwipeTimeSeconds) return; // too slow -> treat as drag, not swipe

            Direction dir = Mathf.Abs(delta.x) > Mathf.Abs(delta.y)
                ? (delta.x > 0 ? Direction.Right : Direction.Left)
                : (delta.y > 0 ? Direction.Up : Direction.Down);

            OnSwipe?.Invoke(dir);
        }

        /// <summary>Allows on-screen arrow buttons to feed the same pipeline as swipes.</summary>
        public void InjectDirection(Direction dir) => OnSwipe?.Invoke(dir);
    }
}
