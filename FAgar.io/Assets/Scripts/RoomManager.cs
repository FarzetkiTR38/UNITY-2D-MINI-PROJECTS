using UnityEngine;
using Photon.Pun;
using Photon;
using Photon.Chat;
using Photon.Realtime;

public class RoomManager : MonoBehaviourPunCallbacks
{
    public string roomName = "Room1";

    void Start()
    {
        Debug.Log("Connecting");

        PhotonNetwork.ConnectUsingSettings();
    }

    public override void OnConnectedToMaster()
    {
        base.OnConnectedToMaster();

        Debug.Log("Connected");

        PhotonNetwork.JoinLobby();
    }

    public override void OnJoinedLobby()
    {
        base.OnJoinedLobby();

        PhotonNetwork.JoinOrCreateRoom(roomName, null, null);
    }

    public override void OnJoinedRoom()
    {
        base.OnJoinedRoom();

        Debug.Log("We're connected and in a room!");

        GameManager.instance.SpawnPlayer();
    }


}
