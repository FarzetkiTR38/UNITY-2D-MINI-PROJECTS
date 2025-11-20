using UnityEngine;
using FishNet.Managing;

public class AutoHostStarter : MonoBehaviour
{
    private NetworkManager _manager;

    void Awake()
    {
        _manager = FindAnyObjectByType<NetworkManager>();

        if (_manager == null)
        {
            Debug.LogError("AutoHostStarter: NetworkManager bulunamadı!");
            return;
        }

        // Host = Server + Client
        _manager.ServerManager.StartConnection();
        _manager.ClientManager.StartConnection();

        Debug.Log("AutoHostStarter: Host başlatıldı (Server + Client).");
    }
}
