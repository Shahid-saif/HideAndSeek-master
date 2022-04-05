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
    [SerializeField]
    ThirdPersonCharacter personCharacter;

    [Space]
    [Header("Level player states")]
    [SerializeField] float m_MovingTurnSpeed = 360;
    [SerializeField] float m_StationaryTurnSpeed = 180;
    [SerializeField] float m_JumpPower = 12f;
    [SerializeField] float m_WallJumpPower = 12f;
    [Range(1f, 16f)] [SerializeField] float m_GravityMultiplier = 2f;
    [SerializeField] float m_RunCycleLegOffset = 0.2f; //specific to the character in sample assets, will need to be modified to work with others
    [SerializeField] float m_MoveSpeedMultiplier = 1f;
    [SerializeField] float m_AnimSpeedMultiplier = 1f;
    [SerializeField] float m_GroundCheckDistance = 0.1f;

    private void Awake()
    {
        gameManager = GetComponent<GameRulesManager>();
    }

    public void setPlayerVal(ThirdPersonCharacter personCharacter)
    {
        if (personCharacter != null)
        {
            personCharacter.m_MovingTurnSpeed = m_MovingTurnSpeed;
            personCharacter.m_StationaryTurnSpeed = m_StationaryTurnSpeed;
            personCharacter.m_JumpPower = m_JumpPower;
            personCharacter.m_WallJumpPower = m_WallJumpPower;
            personCharacter.m_GravityMultiplier = m_GravityMultiplier;
            personCharacter.m_RunCycleLegOffset = m_RunCycleLegOffset;
            personCharacter.m_MoveSpeedMultiplier = m_MoveSpeedMultiplier;
            personCharacter.m_AnimSpeedMultiplier = m_AnimSpeedMultiplier;
            personCharacter.m_GroundCheckDistance = m_GroundCheckDistance;

        }
    }
    void Start()
    {
       
        Vector3 randomPosition = new Vector3(Random.Range(minX, maxX), spawnPositionHight, Random.Range(minZ, maxZ));
     GameObject playerObj=   PhotonNetwork.Instantiate(playerPrefab.name, randomPosition, Quaternion.identity);
        personCharacter = playerObj.GetComponentInChildren<ThirdPersonCharacter>();

        setPlayerVal(personCharacter);
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