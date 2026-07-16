namespace GridPuzzle.Core
{
    /// <summary>
    /// Any renderer implements this to react to model changes.
    /// The model never references this interface's implementers directly —
    /// it only fires C# events. This keeps Core fully engine-agnostic and unit-testable.
    /// </summary>
    public interface IGridObserver
    {
        void OnTileMoved(MoveRecord record);
        void OnBoardReset(GridModel model);
        void OnSolved();
    }
}
