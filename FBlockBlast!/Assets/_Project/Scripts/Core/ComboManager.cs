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
        /// Accumulates line clear statistics for the current placement.
        /// </summary>
        public void OnPlacementResolved(PlacementResult result)
        {
            if (result.LinesCleared > 0)
            {
                BatchLinesCleared += result.LinesCleared;
            }

            if (result.NovaCross)
            {
                BatchHadNovaCross = true;
            }
        }

        /// <summary>
        /// Evaluates combo growth or reset rules at the end of a 3-piece batch.
        /// </summary>
        public void OnBatchComplete()
        {
            if (_config == null) return;

            if (BatchLinesCleared > 0)
            {
                // Calculate combo growth based on total lines cleared in this batch
                int increment = _config.GetComboIncrement(BatchLinesCleared);

                // Add bonus increment if any placement in the batch triggered a Nova Cross
                if (BatchHadNovaCross)
                {
                    increment += _config.novaCrossBonusComboIncrement;
                }

                CurrentCombo += increment;
            }
            else
            {
                // Reset combo to zero if the entire batch resulted in no lines cleared
                if (_config.resetOnZeroClearBatch)
                {
                    CurrentCombo = 0;
                }
            }

            // Dispatch event to update UI and systems
            GameEvents.InvokeComboUpdated(CurrentCombo);

            // Reset batch counters for the next batch
            BatchLinesCleared = 0;
            BatchHadNovaCross = false;
        }
    }
}
