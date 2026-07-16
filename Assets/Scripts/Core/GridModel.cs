using System;
using System.Collections.Generic;

namespace GridPuzzle.Core
{
    public enum Direction { Up, Down, Left, Right }

    /// <summary>
    /// Immutable-per-move record describing exactly what changed on the board.
    /// Used by HistoryManager to undo in O(1) instead of storing a full grid snapshot.
    /// </summary>
    public readonly struct MoveRecord
    {
        public readonly int BlankIndexBefore;
        public readonly int BlankIndexAfter;
        public readonly int MovedTileValue;
        public readonly Direction Direction;

        public MoveRecord(int blankBefore, int blankAfter, int movedValue, Direction dir)
        {
            BlankIndexBefore = blankBefore;
            BlankIndexAfter = blankAfter;
            MovedTileValue = movedValue;
            Direction = dir;
        }
    }

    /// <summary>
    /// Pure logical representation of an N x M sliding-tile board.
    /// Contains zero rendering / engine dependencies (SRP + testability).
    /// Notifies listeners via events -> UI layer subscribes, never the reverse.
    /// </summary>
    public class GridModel
    {
        public int Width { get; }
        public int Height { get; }
        public int MoveCount { get; private set; }

        private readonly int[] _cells;      // 0 = blank, 1..(W*H-1) = tile value
        public int BlankIndex { get; private set; }

        public event Action<MoveRecord> OnTileMoved;
        public event Action OnBoardSolved;

        public GridModel(int width, int height)
        {
            Width = width;
            Height = height;
            _cells = new int[width * height];
            Reset();
        }

        public int GetCell(int index) => _cells[index];
        public int GetCell(int x, int y) => _cells[y * Width + x];

        private int ToIndex(int x, int y) => y * Width + x;
        private (int x, int y) ToCoord(int index) => (index % Width, index / Width);

        public void Reset()
        {
            for (int i = 0; i < _cells.Length; i++) _cells[i] = i + 1;
            _cells[_cells.Length - 1] = 0; // blank in bottom-right (solved order)
            BlankIndex = _cells.Length - 1;
            MoveCount = 0;
        }

        /// <summary>
        /// Fisher-Yates shuffle followed by a parity fix so the board is ALWAYS solvable.
        /// This is what makes generation deterministic-reliable rather than "hope it's solvable".
        /// </summary>
        public void ShuffleSolvable(int seed)
        {
            var rng = new System.Random(seed);
            for (int i = _cells.Length - 1; i > 0; i--)
            {
                int j = rng.Next(i + 1);
                (_cells[i], _cells[j]) = (_cells[j], _cells[i]);
                if (_cells[i] == 0) BlankIndex = i;
                if (_cells[j] == 0) BlankIndex = j;
            }
            BlankIndex = Array.IndexOf(_cells, 0);

            if (!IsSolvable())
            {
                // Swap any two non-blank tiles to flip parity -> guarantees solvability.
                int a = _cells[0] == 0 ? 1 : 0;
                int b = a + 1 == BlankIndex ? a + 2 : a + 1;
                (_cells[a], _cells[b]) = (_cells[b], _cells[a]);
            }
            MoveCount = 0;
        }

        private bool IsSolvable()
        {
            int inversions = 0;
            for (int i = 0; i < _cells.Length; i++)
            {
                if (_cells[i] == 0) continue;
                for (int j = i + 1; j < _cells.Length; j++)
                {
                    if (_cells[j] == 0) continue;
                    if (_cells[i] > _cells[j]) inversions++;
                }
            }

            if (Width % 2 == 1)
            {
                return inversions % 2 == 0;
            }
            int blankRowFromBottom = Height - (BlankIndex / Width);
            if (blankRowFromBottom % 2 == 0)
                return inversions % 2 == 1;
            return inversions % 2 == 0;
        }

        /// <summary>
        /// Attempts to slide the tile adjacent to the blank, in the given swipe direction.
        /// Returns false (no-op) if the move is illegal, e.g. blank on edge.
        /// </summary>
        public bool TryMove(Direction dir)
        {
            var (bx, by) = ToCoord(BlankIndex);
            int tx = bx, ty = by;

            // Swiping "Up" pulls the tile BELOW the blank upward into it, etc.
            switch (dir)
            {
                case Direction.Up: ty = by + 1; break;
                case Direction.Down: ty = by - 1; break;
                case Direction.Left: tx = bx + 1; break;
                case Direction.Right: tx = bx - 1; break;
            }

            if (tx < 0 || tx >= Width || ty < 0 || ty >= Height) return false;

            int targetIndex = ToIndex(tx, ty);
            int movedValue = _cells[targetIndex];

            _cells[BlankIndex] = movedValue;
            _cells[targetIndex] = 0;

            var record = new MoveRecord(BlankIndex, targetIndex, movedValue, dir);
            BlankIndex = targetIndex;
            MoveCount++;

            OnTileMoved?.Invoke(record);

            if (IsSolved()) OnBoardSolved?.Invoke();
            return true;
        }

        /// <summary>Reverses a previously applied move. O(1), no snapshot needed.</summary>
        public void UndoMove(MoveRecord record)
        {
            // Reverse of TryMove: put the tile back where it originally sat
            // (BlankIndexAfter), and restore the blank to its original slot
            // (BlankIndexBefore). The forward move did the opposite assignment —
            // mirroring it here (not repeating it) is what makes this an undo.
            _cells[record.BlankIndexAfter] = record.MovedTileValue;
            _cells[record.BlankIndexBefore] = 0;
            BlankIndex = record.BlankIndexBefore;
            MoveCount = Math.Max(0, MoveCount - 1);
        }

        public bool IsSolved()
        {
            for (int i = 0; i < _cells.Length - 1; i++)
                if (_cells[i] != i + 1) return false;
            return _cells[_cells.Length - 1] == 0;
        }

        /// <summary>Sum of Manhattan distances of every tile from its solved position — used by the combo/hint system, not by core solve logic.</summary>
        public int TotalManhattanDistance()
        {
            int total = 0;
            for (int i = 0; i < _cells.Length; i++)
            {
                int value = _cells[i];
                if (value == 0) continue;
                int targetIndex = value - 1;
                var (cx, cy) = ToCoord(i);
                var (tx, ty) = ToCoord(targetIndex);
                total += Math.Abs(cx - tx) + Math.Abs(cy - ty);
            }
            return total;
        }

        private int ManhattanDistanceOf(int value, int atIndex)
        {
            int targetIndex = value - 1;
            var (cx, cy) = ToCoord(atIndex);
            var (tx, ty) = ToCoord(targetIndex);
            return Math.Abs(cx - tx) + Math.Abs(cy - ty);
        }

        /// <summary>
        /// Pure, non-mutating preview of what a move WOULD do — no state change, no events fired.
        /// Returns the change in that single tile's distance-to-solved (negative = improvement).
        /// Used by the Peek power-up to suggest a move without touching real game state.
        /// </summary>
        public bool TryPreviewMove(Direction dir, out int resultingDistanceDelta, out int tileValue)
        {
            var (bx, by) = ToCoord(BlankIndex);
            int tx = bx, ty = by;
            switch (dir)
            {
                case Direction.Up: ty = by + 1; break;
                case Direction.Down: ty = by - 1; break;
                case Direction.Left: tx = bx + 1; break;
                case Direction.Right: tx = bx - 1; break;
            }

            if (tx < 0 || tx >= Width || ty < 0 || ty >= Height)
            {
                resultingDistanceDelta = 0;
                tileValue = 0;
                return false;
            }

            int targetIndex = ToIndex(tx, ty);
            tileValue = _cells[targetIndex];
            int oldDist = ManhattanDistanceOf(tileValue, targetIndex);
            int newDist = ManhattanDistanceOf(tileValue, BlankIndex);
            resultingDistanceDelta = newDist - oldDist;
            return true;
        }

        /// <summary>
        /// Scans all legal moves and returns the one that most improves distance-to-solved.
        /// Ties broken by enum order. Returns null only if somehow no move is legal (never happens on a valid board).
        /// </summary>
        public Direction? FindBestHintMove(out int tileValue)
        {
            Direction? best = null;
            int bestDelta = int.MaxValue;
            int bestValue = 0;

            foreach (Direction dir in (Direction[])Enum.GetValues(typeof(Direction)))
            {
                if (TryPreviewMove(dir, out int delta, out int val) && delta < bestDelta)
                {
                    bestDelta = delta;
                    best = dir;
                    bestValue = val;
                }
            }

            tileValue = bestValue;
            return best;
        }

        public IReadOnlyList<int> Snapshot() => Array.AsReadOnly(_cells);
    }
}
