namespace ArrowSwarm.Data
{
    using UnityEngine;

    /// <summary>Lets only the newest submitted snapshot determine whether local progress still needs synchronization.</summary>
    public partial class LeaderboardManager
    {
        private int _submissionVersion;

        private void RecordSubmissionResult(int version, bool success)
        {
            if (version != _submissionVersion) return;
            PlayerPrefs.SetInt(PENDING_SYNC_KEY, success ? 0 : 1);
            PlayerPrefs.Save();
        }
    }
}
