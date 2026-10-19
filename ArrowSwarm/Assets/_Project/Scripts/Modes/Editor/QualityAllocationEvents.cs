namespace ArrowSwarm.Modes.Editor
{
    using System;
    using Unity.Profiling;

    /// <summary>Counts native Unity GC.Alloc events in one synchronous, main-thread test bracket.</summary>
    internal static class QualityAllocationEvents
    {
        private const int Capacity = 65536;

        /// <summary>Counts events rather than bytes because this Editor's Mono byte counter returns zero for known allocations.</summary>
        internal static int Measure(Action action)
        {
            var recorder = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "GC.Alloc", Capacity,
                ProfilerRecorderOptions.StartImmediately | ProfilerRecorderOptions.CollectOnlyOnCurrentThread);
            try
            {
                action();
                recorder.Stop();
                if (!recorder.Valid) throw new InvalidOperationException("Unity GC.Alloc recorder is unavailable.");
                int count = recorder.Count;
                if (count >= Capacity) throw new InvalidOperationException("Unity GC.Alloc recorder capacity was exhausted.");
                return count;
            }
            finally { recorder.Dispose(); }
        }
    }
}
