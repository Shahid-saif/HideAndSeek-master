using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;

public class ConnectToServer : MonoBehaviourPunCallbacks
{
    public Text loadingText;
    // Start is called before the first frame update
    void Start()
    {

        PhotonNetwork.ConnectUsingSettings();

    }
    public override void OnConnectedToMaster()
    {
        PhotonNetwork.JoinLobby();
    }

    public override void OnJoinedLobby()
    {
        SceneManager.LoadScene("Lobby");
    }
    public override void OnDisconnected(DisconnectCause cause)
    {
        loadingText.text = "Server disconnected for reason:" + cause.ToString() + "\n Please restart the game.";
    }
}
