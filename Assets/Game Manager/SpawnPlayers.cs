using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
public class SpawnPlayers : MonoBehaviourPunCallbacks, IInRoomCallbacks
{
    private GameRulesManager gameManager;
    public GameObject playerPrefab;
    public GameObject cam;
    public float minX, maxX, minZ, maxZ;
    public float spawnPositionHight;
    // Start is called before the first frame update

    private void Awake()
    {
        gameManager = GetComponent<GameRulesManager>();
    }
    void Start()
    {
        Vector3 randomPosition = new Vector3(Random.Range(minX, maxX), spawnPositionHight, Random.Range(minZ, maxZ));
        PhotonNetwork.Instantiate(playerPrefab.name, randomPosition, Quaternion.identity);

        var players = GameObject.FindGameObjectsWithTag("Player");
        foreach (var player in players)
        {
            if (player.GetPhotonView().IsMine)
            {
                gameManager.UpdatePlayersInfos();
                cam.GetComponent<UnityStandardAssets.Cameras.FreeLookCam>().SetTarget(player.transform);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {

    }



}