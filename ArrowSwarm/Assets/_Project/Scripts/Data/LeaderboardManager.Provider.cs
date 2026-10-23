namespace ArrowSwarm.Data
{
    /// <summary>Resolves the production provider while allowing isolated Editor verification before Awake.</summary>
    public partial class LeaderboardManager
    {
#if UNITY_EDITOR
        private static System.Func<ILeaderboardCloudService> _editorCloudFactory = null;
#endif

        private static ILeaderboardCloudService CreateCloudService()
        {
#if UNITY_EDITOR
            if (_editorCloudFactory != null) return _editorCloudFactory();
#endif
            return new UnityCloudService();
        }
    }
}
