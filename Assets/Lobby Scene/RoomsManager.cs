using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using ExitGames.Client.Photon;
using System.Linq;
using System;

public class RoomsManager : MonoBehaviourPunCallbacks, IPunObservable
{
    public InputField playerField;
    public InputField createFiled;
    public InputField joinField;
    public Text errorText;
    public Text mainMenuErrorText;
    private string selectedMap;
    [SerializeField] CanvasesManager canvasesManager;

    // Rooms listing
    [Header("Players Listing")]

    // Players Listing
    public Transform playerListParent;
    [SerializeField] private GameObject playerListing;
    public bool isReady;
    [SerializeField] Text readyText;
    public Text roomName;
    public static int roundTime;
    private bool joinedRandom;
    public ToggleGroup maps;
    public ToggleGroup roundTimeSelect;
    private const byte TOGGLE_READY_EVENT = 12;

    void Awake()
    {

        isReady = false;
        Hashtable hash = new Hashtable();
        hash.Add("Ready", isReady);
        PhotonNetwork.LocalPlayer.SetCustomProperties(hash);
    }

    // Update is called once per frame
    void Update()
    {
        foreach (Toggle map in maps.GetComponentsInChildren<Toggle>())
        {
            map.interactable = PhotonNetwork.LocalPlayer.IsMasterClient && !isReady;
        }
        foreach (Toggle time in roundTimeSelect.GetComponentsInChildren<Toggle>())
        {
            time.interactable = PhotonNetwork.LocalPlayer.IsMasterClient && !isReady;
        }
    }
    public void CreateRoom()
    {
        if (createFiled.text == "")
        {
            errorText.text = "Room most have a name.";
            return;
        }
        RoomOptions options = new RoomOptions();
        options.MaxPlayers = 5;
        errorText.text = "Creating Room...";
        PhotonNetwork.JoinOrCreateRoom(createFiled.text, options, TypedLobby.Default);
    }
    public override void OnCreatedRoom()
    {
        PhotonNetwork.CurrentRoom.IsVisible = false;
        roomName.text = PhotonNetwork.CurrentRoom.Name;
        errorText.text = "Room created successfully";
        canvasesManager.ShowCurrentRoomCanvas();
        roomName.text = PhotonNetwork.CurrentRoom.Name;
        for (int i = 0; i < playerListParent.childCount; i++)
        {
            Debug.Log(playerListParent.GetChild(i).name);
            Destroy(playerListParent.GetChild(i).gameObject);
        }
        Playerstats stats = Instantiate(playerListing, playerListParent).GetComponent<Playerstats>();
        stats.SetPlayerInfo(PhotonNetwork.LocalPlayer);
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        errorText.text = "Couldnt create room ," + message;
    }
    public void JoinRoom()
    {
        errorText.text = "joining room..";
        if (joinField.text == "")
        {
            errorText.text = "Enter Room name to Join it.";
            return;
        }
        PhotonNetwork.JoinRoom(joinField.text);
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        base.OnJoinRoomFailed(returnCode, message);
        errorText.text = "Joining Room failed," + message;
        mainMenuErrorText.text = "Joining Room failed," + message;

    }
    public override void OnJoinedRoom()
    {
        base.OnJoinedRoom();
        canvasesManager.ShowCurrentRoomCanvas();
        roomName.text = (joinedRandom ? "Random Room" : PhotonNetwork.CurrentRoom.Name);
        foreach (var player in PhotonNetwork.CurrentRoom.Players)
        {
            Playerstats stats = (Playerstats)Instantiate(playerListing, playerListParent).GetComponent<Playerstats>();
            stats.SetPlayerInfo(player.Value);
        }
    }


    public override void OnLeftRoom()
    {
        base.OnLeftRoom();
        errorText.text = "";
        mainMenuErrorText.text = "";
        canvasesManager.ShowMainMenuCanavs();
        for (int i = 0; i < playerListParent.childCount; i++)
        {
            Destroy(playerListParent.GetChild(i).gameObject);
        }
    }
    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        Playerstats stats = (Playerstats)Instantiate(playerListing, playerListParent).GetComponent<Playerstats>();
        stats.SetPlayerInfo(newPlayer);

        base.OnPlayerEnteredRoom(newPlayer);
    }
    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        base.OnPlayerLeftRoom(otherPlayer);
        UpdatePlayersList();
    }
    public void JoinRandomRoom()
    {
        errorText.text = "";
        mainMenuErrorText.text = "";
        joinedRandom = true;
        if (playerField.text == "")
        {
            mainMenuErrorText.text = "PLayer most have a Nickname.";
            return;
        }
        PhotonNetwork.NickName = playerField.text;
        mainMenuErrorText.text = "joinig Room..";
        PhotonNetwork.JoinRandomOrCreateRoom();
    }
    public void LeaveRoom()
    {
        PhotonNetwork.LeaveRoom();
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        PhotonNetwork.LoadLevel("Loading");
        base.OnDisconnected(cause);
    }

    public void ToggleReady()
    {
        isReady = !isReady;
        Hashtable hash = new Hashtable();
        hash.Add("Ready", isReady);
        if (PhotonNetwork.LocalPlayer.IsMasterClient)
        {
            selectedMap = maps.GetFirstActiveToggle().name;
            roundTime = int.Parse(roundTimeSelect.GetFirstActiveToggle().name);
            hash.Add("Map", selectedMap);
        }

        PhotonNetwork.LocalPlayer.SetCustomProperties(hash);
        PhotonNetwork.RaiseEvent(TOGGLE_READY_EVENT, new object[] { selectedMap, roundTime }, RaiseEventOptions.Default, SendOptions.SendReliable);
        readyText.text = (isReady ? "Ready" : "Not Ready");
        foreach (var player in PhotonNetwork.CurrentRoom.Players)
        {
            if (player.Value != PhotonNetwork.LocalPlayer && (!(bool)player.Value.CustomProperties["Ready"])) return;
        }
        if (!isReady) return;
        PhotonNetwork.CurrentRoom.IsOpen = false;
        PhotonNetwork.LoadLevel(selectedMap);
    }
    public override void OnEnable()
    {
        base.OnEnable();
        PhotonNetwork.NetworkingClient.EventReceived += NetworkingClient_EventReceived;
    }

    private void NetworkingClient_EventReceived(EventData obj)
    {

        if (obj.Code == TOGGLE_READY_EVENT)
        {
            object[] data = (object[])obj.CustomData;
            UpdatePlayersList();
            if (data[0] != null)
            {
                selectedMap = (string)data[0];
                roundTime = (int)data[1];
                foreach (var map in maps.GetComponentsInChildren<Toggle>())
                {
                    if (map.name == selectedMap)
                    {
                        map.isOn = true;
                    }
                    else
                    {
                        map.isOn = false;
                    }
                }
                foreach (var time in roundTimeSelect.GetComponentsInChildren<Toggle>())
                {
                    if (time.name == roundTime.ToString())
                    {
                        time.isOn = true;
                    }
                    else
                    {
                        time.isOn = false;
                    }
                }
            }

            foreach (var player in PhotonNetwork.CurrentRoom.Players)
            {
                if (!(bool)player.Value.CustomProperties["Ready"]) return;
            }

            if (!isReady) return;
            PhotonNetwork.CurrentRoom.IsOpen = false;
            PhotonNetwork.LoadLevel(selectedMap);
        }
    }
    void UpdatePlayersList()
    {
        for (int i = 0; i < playerListParent.childCount; i++)
        {
            Debug.Log(playerListParent.GetChild(i).name);
            Destroy(playerListParent.GetChild(i).gameObject);
        }
        foreach (var newPlayer in PhotonNetwork.CurrentRoom.Players)
        {
            Playerstats stats = (Playerstats)Instantiate(playerListing, playerListParent).GetComponent<Playerstats>();
            stats.SetPlayerInfo(newPlayer.Value);
        }
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {

    }
}
