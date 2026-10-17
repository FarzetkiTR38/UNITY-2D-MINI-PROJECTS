namespace ArrowSwarm.Modes
{
    using System.Collections.Generic;

    /// <summary>Deterministic solution witness; -1 means a clockwise quarter turn.</summary>
    public sealed class PuzzleSolution
    {
        public bool Solvable { get; internal set; }
        public int RequiredTurns { get; internal set; }
        public List<int> Steps { get; } = new List<int>();
        public string Error { get; internal set; }
    }

    /// <summary>
    /// Exact monotonic solver for two-color opposite walls. Removing any legal arrow cannot
    /// block another, so saturating an orientation before changing it minimizes turns.
    /// </summary>
    public static class PuzzleSolver
    {
        /// <summary>Proves a remaining state within the available turns and returns safe hint actions.</summary>
        public static PuzzleSolution Solve(PuzzleLevel level, bool[] removed = null, int orientation = -1, int turns = -1)
        {
            var result = new PuzzleSolution { Error = PuzzleRules.Validate(level) };
            if (result.Error != null) return result;
            if (removed != null && removed.Length != level.Arrows.Count) { result.Error = "State length mismatch."; return result; }
            var cleared = removed != null ? (bool[])removed.Clone() : new bool[level.Arrows.Count];
            int remaining = 0;
            foreach (bool item in cleared) if (!item) remaining++;
            if (orientation < 0) orientation = level.InitialOrientation;
            if (turns < 0) turns = level.TurnBudget;
            int emptyOrientations = 0;
            while (remaining > 0)
            {
                bool progress = false, pass;
                do
                {
                    pass = false;
                    for (int i = 0; i < cleared.Length; i++)
                        if (PuzzleRules.CanRemove(level, cleared, i, orientation))
                        { cleared[i] = true; remaining--; result.Steps.Add(i); pass = progress = true; }
                } while (pass);
                if (remaining == 0) break;
                emptyOrientations = progress ? 0 : emptyOrientations + 1;
                if (level.Mode != GameModeId.ColorShift || result.RequiredTurns >= turns || emptyOrientations >= 2)
                { result.Error = "No clearing sequence within the rotation budget."; return result; }
                result.RequiredTurns++; orientation ^= 1; result.Steps.Add(-1);
            }
            result.Solvable = true;
            return result;
        }
    }
}
