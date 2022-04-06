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
    [SerializeField] Color seeker_color;
    [SerializeField] Color hider_color;
    [SerializeField] Color Target_color;
    private bool sentScore;
    void Start()
    {
        sensor = sensorParent.GetComponent<Sensor>();

    }

    void Update()
    {
        if(!GameRulesManager.gameRulesManager.endRound && !GameRulesManager.gameRulesManager.gameEnded)
        GameRulesManager.gameRulesManager.scoreText.text = "Score: " + ((int)GameRulesManager.gameRulesManager.score).ToString();

        if (isSeeker)
        {
            GetComponentInChildren<SkinnedMeshRenderer>().materials[0].color = seeker_color;
            if (gameObject.GetPhotonView().IsMine)
            {
                if (!GameRulesManager.gameRulesManager.endRound && GameRulesManager.gameRulesManager.gameTimer > GameRulesManager.gameRulesManager.cageCooldown)
                {
                    GameRulesManager.gameRulesManager.score += Time.deltaTime;
                }
            }
        }
        else if (isTargeted)
        {
            GetComponentInChildren<SkinnedMeshRenderer>().materials[0].color = Target_color;
        }
        else
        {
            GetComponentInChildren<SkinnedMeshRenderer>().materials[0].color = hider_color;
        }
    }
}
