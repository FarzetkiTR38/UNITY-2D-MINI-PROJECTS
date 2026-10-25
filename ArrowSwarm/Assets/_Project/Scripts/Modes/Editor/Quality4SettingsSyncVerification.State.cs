namespace ArrowSwarm.Modes.Editor
{
    using System;
    using System.Reflection;
    using System.Threading.Tasks;
    using ArrowSwarm.Data;
    using UnityEngine;

    /// <summary>Owns a temporary fake service and restores exact data, preference existence and runtime presentation.</summary>
    public static partial class Quality4SettingsSyncVerification
    {
        private const BindingFlags InstanceFlags = BindingFlags.Instance | BindingFlags.NonPublic;
        private const BindingFlags StaticFlags = BindingFlags.Static | BindingFlags.NonPublic;

        /// <summary>Prevents synthetic fixture progress from reaching a real network client.</summary>
        private sealed class Fixture
        {
            internal readonly SettingsCloud Cloud = new SettingsCloud();
            internal readonly QualityDataSnapshot Data;
            private readonly FieldInfo _factory;
            private readonly object _originalFactory;
            private readonly bool _background;

            internal Fixture()
            {
                if (!Application.isPlaying || !DataManager.HasInstance || LeaderboardManager.HasInstance)
                    throw new InvalidOperationException("Use a fresh main-menu Play session before opening the leaderboard.");
                _factory = typeof(LeaderboardManager).GetField("_editorCloudFactory", StaticFlags);
                if (_factory == null) throw new InvalidOperationException("The Editor-only fake provider seam is required.");
                _originalFactory = _factory.GetValue(null);
                Data = new QualityDataSnapshot(DataManager.Instance);
                _factory.SetValue(null, new Func<ILeaderboardCloudService>(() => { Cloud.FactoryCalls++; return Cloud; }));
                _background = Application.runInBackground; Application.runInBackground = true;
            }

            internal void SetAutoSave(bool enabled) => typeof(DataManager).GetField("_autoSaveOnChange", InstanceFlags)
                .SetValue(DataManager.Instance, enabled);

            internal async Task Restore()
            {
                try
                {
                    for (int i = 0; i < 2; i++) await Task.Yield();
                    if (LeaderboardManager.HasInstance)
                    {
                        var manager = LeaderboardManager.Instance;
                        if (!ReferenceEquals(typeof(LeaderboardManager).GetField("_cloudService", InstanceFlags).GetValue(manager), Cloud))
                            throw new InvalidOperationException("The fixture does not own the active leaderboard provider.");
                        UnityEngine.Object.DestroyImmediate(manager.gameObject);
                    }
                }
                finally
                {
                    _factory.SetValue(null, _originalFactory); Data.Dispose();
                    Application.runInBackground = _background;
                    // Refresh existing subscribers directly, without saving or scheduling another score submission.
                    var changed = (Action<PlayerData>)typeof(DataManager).GetField("OnPlayerDataChanged", StaticFlags).GetValue(null);
                    changed?.Invoke(DataManager.Instance.PlayerData);
                    Data.VerifyRestored(Results);
                }
                Check(!LeaderboardManager.HasInstance && ReferenceEquals(_factory.GetValue(null), _originalFactory),
                    "temporary fake singleton removed and exact original provider factory restored");
            }
        }
    }
}
