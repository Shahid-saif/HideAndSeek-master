using System.Collections;
using System.Collections.Generic;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.UI;
public class RoomStats : MonoBehaviour
{
    [SerializeField] private Text roomName;


    public void SetRoomInfo(RoomInfo roomInfo)
    {
        roomName.text = roomInfo.PlayerCount + "/" + roomInfo.MaxPlayers + " , " + roomInfo.Name;
    }

}
