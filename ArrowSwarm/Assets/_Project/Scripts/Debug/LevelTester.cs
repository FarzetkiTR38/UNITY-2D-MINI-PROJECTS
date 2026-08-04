using UnityEngine;
using ArrowSwarm.Debug;
using System.Collections;

public class LevelTester : MonoBehaviour
{
    private static bool _hasJumped = false;

    private IEnumerator Start()
    {
        if (_hasJumped) yield break;
        _hasJumped = true;
        
        yield return new WaitForSeconds(1f);
        if (DebugManager.Instance != null)
        {
            DebugManager.Instance.JumpToLevel(100);
            Debug.Log("[ArrowSwarm] LevelTester: Jumped to Level 100");
        }
    }
}
