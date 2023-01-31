using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LobbyManager : MonoBehaviourPunCallbacks
{
    
    [SerializeField]
    private Transform roomContent;

    [SerializeField] private TextMeshProUGUI inputUsername;

    [SerializeField] private Transform lobbyPanel;

    IEnumerator AutoRefreshLobby()
    {
        if (PhotonNetwork.IsConnectedAndReady)
        {
            //if is in main lobby
            if (PhotonNetwork.CurrentRoom == null)
            {
                print("refreshing lobby..");
                PhotonNetwork.JoinLobby(TypedLobby.Default);
            }
        }

        yield return new WaitForSeconds(2f);

        StartCoroutine(AutoRefreshLobby());
    }

    public void CreateRoom()
    {
        print(PhotonNetwork.IsConnectedAndReady);
        print(PhotonNetwork.IsConnected);

        RoomOptions roomOptions = new RoomOptions();
        roomOptions.MaxPlayers = 2;
        
        if (PhotonNetwork.IsConnectedAndReady)
        {
            print("creating room..");

            if (inputUsername.text.Length > 1)
            {
                PhotonNetwork.NickName = inputUsername.text;
            }
            
            PhotonNetwork.CreateRoom(PhotonNetwork.LocalPlayer.NickName + "'s Lobby", roomOptions, TypedLobby.Default);
        }
    }
    
    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        Debug.LogErrorFormat("Room creation failed with error code {0} and error message {1}", returnCode, message);
    }

    public override void OnCreatedRoom()
    {
        print("room created!");
    }

    public override void OnJoinedRoom()
    {
        lobbyPanel.gameObject.SetActive(false);
    }
    
    public override void OnLeftRoom()
    {
        lobbyPanel.gameObject.SetActive(true);
    }
    
    // Start is called before the first frame update
    void Start()
    {
        PhotonNetwork.ConnectUsingSettings();
        StartCoroutine(AutoRefreshLobby());
    }

    public override void OnJoinedLobby()
    {
        print("joined lobby");
    }

    public override void OnConnectedToMaster()
    {
        print("joined master server");
        PhotonNetwork.NickName = "Player" + PhotonNetwork.CountOfPlayers;
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {

        foreach (Transform t in roomContent)
        {
            Destroy(t.gameObject);
        }
        
        foreach (RoomInfo roomInfo in roomList)
        {
            if (roomInfo.PlayerCount > 0)
            {
                GameObject roomPanel = (GameObject) Instantiate(Resources.Load("LobbyPanel"), Vector3.zero, Quaternion.identity, roomContent);
                roomPanel.transform.Find("LobbyName").GetComponent<TextMeshProUGUI>().text = roomInfo.Name;
                roomPanel.transform.Find("PlayerText").GetComponent<TextMeshProUGUI>().text = roomInfo.PlayerCount + "/" + roomInfo.MaxPlayers;
            
                roomPanel.transform.Find("Button").GetComponent<Button>().onClick.AddListener(() =>
                {
                    if (inputUsername.text.Length > 1)
                    {
                        PhotonNetwork.NickName = inputUsername.text;
                    }
                    PhotonNetwork.JoinRoom(roomInfo.Name);
                });
            }

        }
    }
}
