using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
public class LoadNextScene : MonoBehaviourPun
{
    // Start is called before the first frame update
    void Start()
    {
        string levelName = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameRulesManager>().currentMap;
        PhotonNetwork.LoadLevel(levelName);
    }

}
