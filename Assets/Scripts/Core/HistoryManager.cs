using System.Collections.Generic;

namespace GridPuzzle.Core
{
    /// <summary>
    /// Command-pattern history stack. Stores lightweight MoveRecord structs only —
    /// NOT full grid deep copies — keeping memory O(moves) instead of O(moves * cells).
    /// </summary>
    public class HistoryManager
    {
        private readonly Stack<MoveRecord> _undoStack = new Stack<MoveRecord>();
        private readonly GridModel _grid;
        private readonly int _maxUndos;

        public int RemainingUndos { get; private set; }
        public bool CanUndo => _undoStack.Count > 0 && RemainingUndos > 0;

        public HistoryManager(GridModel grid, int maxUndos = -1)
        {
            _grid = grid;
            _maxUndos = maxUndos;
            RemainingUndos = maxUndos < 0 ? int.MaxValue : maxUndos;
            _grid.OnTileMoved += Record;
        }

        private void Record(MoveRecord record) => _undoStack.Push(record);

        public bool Undo()
        {
            if (!CanUndo) return false;
            var record = _undoStack.Pop();
            _grid.UndoMove(record);
            if (_maxUndos >= 0) RemainingUndos--;
            return true;
        }

        public void Clear()
        {
            _undoStack.Clear();
            RemainingUndos = _maxUndos < 0 ? int.MaxValue : _maxUndos;
        }

        public int MovesRecorded => _undoStack.Count;

        public void Dispose() => _grid.OnTileMoved -= Record;
    }
}
