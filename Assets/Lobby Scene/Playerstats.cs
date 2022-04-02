using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Realtime;
using Photon.Pun;

public class Playerstats : MonoBehaviour
{
    [SerializeField] private Text playerName;
    private void Awake()
    {

    }

    public void SetPlayerInfo(Player player)
    {
        bool isReady = ((bool)player.CustomProperties["Ready"]);
        if (!player.IsLocal)
        {
            playerName.text = player.NickName + "  " + (isReady ? "Ready" : "Not Ready");
        }
        else
        {
            playerName.text = player.NickName;
        }

    }

}
