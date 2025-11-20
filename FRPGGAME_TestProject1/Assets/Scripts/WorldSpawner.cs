using UnityEngine;
using FishNet;
using FishNet.Managing;
using FishNet.Object;
using FishNet.Transporting;   // ServerConnectionStateArgs ve LocalConnectionState buradan gelir

public class WorldSpawner : MonoBehaviour
{
    [Header("Spawn edilecek NetworkObject prefab")]
    public NetworkObject prefab;

    private void OnEnable()
    {
        // ServerManager hazırsa event'e abone ol
        if (InstanceFinder.ServerManager != null)
        {
            InstanceFinder.ServerManager.OnServerConnectionState += OnServerConnectionState;
        }
        else
        {
            Debug.LogWarning("WorldSpawner: ServerManager henüz hazır değil.");
        }
    }

    private void OnDisable()
    {
        if (InstanceFinder.ServerManager != null)
            InstanceFinder.ServerManager.OnServerConnectionState -= OnServerConnectionState;
    }

    private void OnServerConnectionState(ServerConnectionStateArgs args)
    {
        // Sadece server BAŞLADIĞINDA çalışsın
        if (args.ConnectionState == LocalConnectionState.Started)
        {
            Debug.Log("WorldSpawner: Server başladı, objeyi spawn ediyorum...");
            //SpawnWorldObject();
            SpawnObjectAtY(-4f);
        }
    }

    private void SpawnWorldObject()
    {
        if (prefab == null)
        {
            Debug.LogError("WorldSpawner: Prefab atanmadı!");
            return;
        }

        // Spawn pozisyonu
        Vector3 pos = new Vector3(0f, 1f, 0f);
        Quaternion rot = Quaternion.identity;

        // Normal Instantiate
        NetworkObject nob = Instantiate(prefab, pos, rot);

        // FishNet'e "bunu network objesi olarak dağıt" diyoruz
        InstanceFinder.ServerManager.Spawn(nob);

        Debug.Log("WorldSpawner: NetworkObject spawn edildi.");
    }

    public void SpawnObjectAtY(float y)
    {
        Vector3 pos = new Vector3(0f, y, 0f);
        Quaternion rot = Quaternion.identity;

        NetworkObject nob = Instantiate(prefab, pos, rot);

        InstanceFinder.ServerManager.Spawn(nob);
    }

}
