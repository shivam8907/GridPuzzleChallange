using System;
using GridPuzzle.Core;

namespace GridPuzzle.Gameplay
{
   
    public class PeekPowerUp
    {
        private readonly GridModel _grid;

        public int RemainingUses { get; private set; }
        public event Action<int, Direction> OnHint; // (tileValue, suggestedDirection)

        public PeekPowerUp(GridModel grid, int maxUses)
        {
            _grid = grid;
            RemainingUses = maxUses;
        }

        public bool TryUse()
        {
            if (RemainingUses <= 0) return false;

            Direction? dir = _grid.FindBestHintMove(out int tileValue);
            if (dir == null) return false; // no legal move exists (shouldn't happen mid-game)

            RemainingUses--;
            OnHint?.Invoke(tileValue, dir.Value);
            return true;
        }

        public void Reset(int maxUses) => RemainingUses = maxUses;
    }
}
