using UnityEditor;
using UnityEngine;
using ArrowSwarm.Core;

public static class TestRetry
{
    [MenuItem("Tools/Test Retry Level")]
    public static void Execute()
    {
        if (Application.isPlaying)
        {
            Debug.Log("[Test] Forcing RetryLevel...");
            LevelManager.Instance.RetryLevel();
        }
        else
        {
            Debug.Log("[Test] Must be in Play Mode to test RetryLevel.");
        }
    }
}
