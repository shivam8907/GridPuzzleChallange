using GridPuzzle.Core;

namespace GridPuzzle.Gameplay
{
   
    public class ComboScoreSystem
    {
        private readonly GridModel _grid;
        private int _lastDistance;

        public int Score { get; private set; }
        public int ComboStreak { get; private set; }
        public int BestCombo { get; private set; }

        private const int BasePoints = 10;
        private const int ComboBonusPerStep = 5;
        private const int ComboCap = 8; // prevents runaway multipliers

        public ComboScoreSystem(GridModel grid)
        {
            _grid = grid;
            _lastDistance = grid.TotalManhattanDistance();
            grid.OnTileMoved += _ => Evaluate();
            grid.OnBoardSolved += () => Score += 100; // solve bonus
        }

        private void Evaluate()
        {
            int currentDistance = _grid.TotalManhattanDistance();

            if (currentDistance < _lastDistance)
            {
                ComboStreak = System.Math.Min(ComboStreak + 1, ComboCap);
                Score += BasePoints + ComboBonusPerStep * ComboStreak;
                if (ComboStreak > BestCombo) BestCombo = ComboStreak;
            }
            else
            {
                ComboStreak = 0; // streak broken by a neutral/backward move
                Score += 1;      // small consolation so score never feels punishing
            }

            _lastDistance = currentDistance;
        }

        public void Reset()
        {
            Score = 0;
            ComboStreak = 0;
            BestCombo = 0;
            _lastDistance = _grid.TotalManhattanDistance();
        }
    }
}
