using System;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class RoomManager : MonoBehaviourPunCallbacks
{

    public GameObject startGameBtn;
    public Transform roomPanel;
    public Transform playerListContent;

    private void Start()
    {
        print("dfd");
    }

    public override void OnJoinedRoom()
    {
        roomPanel.Find("RoomName").GetComponent<TextMeshProUGUI>().text = PhotonNetwork.CurrentRoom.Name;
        roomPanel.gameObject.SetActive(true);

        startGameBtn.SetActive(PhotonNetwork.IsMasterClient);
        
        startGameBtn.GetComponent<Button>().onClick.AddListener(() =>
        {
            PhotonView.Get(this).RPC("JoinGameScene", RpcTarget.All);
        });

        RefreshPlayerList();
    }

    public void LeaveRoom()
    {
        PhotonNetwork.LeaveRoom();
    }

    public override void OnLeftRoom()
    {
        roomPanel.gameObject.SetActive(false);
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        RefreshPlayerList();
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        RefreshPlayerList();
    }

    void RefreshPlayerList()
    {
        foreach (Transform t in playerListContent)
        {
            Destroy(t.gameObject);
        }

        foreach (Player player in PhotonNetwork.PlayerList)
        {
            GameObject playerFrame = (GameObject)Instantiate(Resources.Load("UserPanel"), Vector3.zero,
                Quaternion.identity, playerListContent);

            playerFrame.transform.Find("PlayerName").GetComponent<TextMeshProUGUI>().text = player.NickName;

            playerFrame.transform.Find("PlayerType").GetComponent<TextMeshProUGUI>().text =
                player.IsMasterClient ? "Host" : "Guest";
        }
    }

    [PunRPC]
    public void JoinGameScene()
    {
        SceneManager.LoadScene("MainScene");
    }
}
