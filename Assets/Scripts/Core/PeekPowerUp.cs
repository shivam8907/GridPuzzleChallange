using System;
using GridPuzzle.Core;

namespace GridPuzzle.Gameplay
{
    /// <summary>
    /// CANDIDATE INITIATIVE FEATURE — "Peek" power-up.
    ///
    /// A limited-use hint: on activation, finds whichever legal move most
    /// reduces total distance-to-solved (reusing GridModel.FindBestHintMove,
    /// the same Manhattan-distance math ComboScoreSystem already relies on)
    /// and tells the UI which tile to highlight. Does NOT move the tile itself —
    /// the player still has to make the move — so it can't be spammed to
    /// auto-solve the board, only to break decision paralysis.
    /// </summary>
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
