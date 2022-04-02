using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Pun.Demo.PunBasics;
using UnityEngine;

public class PlayerStatus : MonoBehaviourPun
{
    public bool isSeeker;
    public bool isTargeted;
    public GameObject sensorParent;
    Sensor sensor;
    private bool sentScore;
    void Start()
    {
        sensor = sensorParent.GetComponent<Sensor>();

    }

    void Update()
    {
        if (isSeeker)
        {
            GetComponentInChildren<Renderer>().material.SetColor("_Color", Color.blue);
            if (gameObject.GetPhotonView().IsMine)
            {
                if (!GameRulesManager.gameRulesManager.endRound && GameRulesManager.gameRulesManager.gameTimer > GameRulesManager.gameRulesManager.cageCooldown)
                {
                    GameRulesManager.gameRulesManager.score += Time.deltaTime;
                    GameRulesManager.gameRulesManager.timerText.text = "Score: " + ((int)GameRulesManager.gameRulesManager.score).ToString();
                }


            }
        }
        else if (isTargeted)
        {
            GetComponentInChildren<Renderer>().material.SetColor("_Color", Color.red);
        }
        else
        {
            GetComponentInChildren<Renderer>().material.SetColor("_Color", Color.gray);
        }
    }
}
