using UnityEngine;
using FishNet;
using FishNet.Managing;
using FishNet.Object;

public class SimpleSpawner : MonoBehaviour
{
    [Header("Spawn edilecek prefab (NetworkObject şart)")]
    public NetworkObject prefab;

    private void Start()
    {
        // Sadece SERVER tarafında spawn edeceğiz
        if (!InstanceFinder.IsServer)
        {
            Debug.Log("SimpleSpawner: Ben server değilim, spawn etmeyeceğim.");
            return;
        }

        if (prefab == null)
        {
            Debug.LogError("SimpleSpawner: Prefab atanmadı!");
            return;
        }

        // 0,0,0 konumunda spawn edelim
        Vector3 spawnPos = Vector3.zero;
        Quaternion spawnRot = Quaternion.identity;

        NetworkObject nob = Instantiate(prefab, spawnPos, spawnRot);

        // Burası çok önemli: FishNet'e "bunu network objesi olarak dağıt" diyoruz
        InstanceFinder.ServerManager.Spawn(nob);

        Debug.Log("SimpleSpawner: Objeyi server tarafında spawn ettim.");
    }
}
