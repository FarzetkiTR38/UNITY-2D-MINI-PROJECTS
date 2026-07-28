using NeonGalaxy.Data;

namespace NeonGalaxy.Core
{
    /// <summary>
    /// Tracks batch-level combo state and quality.
    /// Accumulates rows, columns, and Nova Cross events during a 3-piece batch,
    /// then updates the total combo multiplier at the end of the batch.
    /// This is a pure C# class.
    /// </summary>
    public class ComboManager
    {
        private readonly ComboConfigSO _config;

        public int CurrentCombo { get; private set; }
        public int BatchLinesCleared { get; private set; }
        public bool BatchHadNovaCross { get; private set; }

        public ComboManager(ComboConfigSO config)
        {
            _config = config;
            Reset();
        }

        /// <summary>
        /// Resets the combo level and batch statistics.
        /// </summary>
        public void Reset()
        {
            CurrentCombo = 0;
            BatchLinesCleared = 0;
            BatchHadNovaCross = false;
        }

        /// <summary>
        /// Restores combo state from a saved run snapshot.
        /// </summary>
        public void RestoreState(int combo, int batchLines, bool batchNova)
        {
            CurrentCombo = combo;
            BatchLinesCleared = batchLines;
            BatchHadNovaCross = batchNova;
        }

        /// <summary>
        /// Accumulates line clear statistics for the current placement.
        /// </summary>
        public void OnPlacementResolved(PlacementResult result)
        {
            if (result.LinesCleared > 0)
            {
                BatchLinesCleared += result.LinesCleared;

                // Increment combo instantly upon clearing lines with this placement
                CurrentCombo += 1;

                if (result.NovaCross)
                {
                    CurrentCombo += _config.novaCrossBonusComboIncrement;
                }

                GameEvents.InvokeComboUpdated(CurrentCombo);
            }

            if (result.NovaCross)
            {
                BatchHadNovaCross = true;
            }
        }

        /// <summary>
        /// Evaluates combo reset rules at the end of a 3-piece batch.
        /// </summary>
        public void OnBatchComplete()
        {
            if (_config == null) return;

            if (BatchLinesCleared == 0)
            {
                // Reset combo to zero if the entire batch resulted in no lines cleared
                if (_config.resetOnZeroClearBatch)
                {
                    CurrentCombo = 0;
                    GameEvents.InvokeComboUpdated(CurrentCombo);
                }
            }

            // Reset batch counters for the next batch
            BatchLinesCleared = 0;
            BatchHadNovaCross = false;
        }
    }
}
