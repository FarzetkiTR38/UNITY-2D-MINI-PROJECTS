using UnityEngine;
using FishNet.Managing;

public class NetworkStarter : MonoBehaviour
{
    private NetworkManager _manager;

    void Awake()
    {
        _manager = FindAnyObjectByType<NetworkManager>();

        if (_manager == null)
            Debug.LogError("NetworkManager sahnede bulunamadı!");
    }

    void OnGUI()
    {
        GUILayout.BeginArea(new Rect(10, 10, 200, 150));

        if (GUILayout.Button("Start Host"))
            StartHost();

        if (GUILayout.Button("Start Server"))
            StartServer();

        if (GUILayout.Button("Start Client"))
            StartClient();

        GUILayout.EndArea();
    }

    void StartHost()
    {
        _manager.ServerManager.StartConnection();
        _manager.ClientManager.StartConnection();
        Debug.Log("HOST başlatıldı (Server + Client)");
    }

    void StartServer()
    {
        _manager.ServerManager.StartConnection();
        Debug.Log("Server başlatıldı");
    }

    void StartClient()
    {
        _manager.ClientManager.StartConnection();
        Debug.Log("Client başlatıldı");
    }
}
