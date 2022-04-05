using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;
using System.Linq;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using UnityEngine.SceneManagement;
public class GameRulesManager : MonoBehaviourPunCallbacks, IPunObservable, IInRoomCallbacks
{
    [Header("Player Information")]
    List<PlayerInfo> playerInfos = new List<PlayerInfo>();
    public GameObject playersUIHolder;
    public static GameRulesManager gameRulesManager;
    [SerializeField] private int startingSeeker;
    private List<int> playersToBeSeekers = new List<int>();


    [Header("Timers")]
    public float gameTimer;
    [SerializeField] private bool gameStarted;
    [SerializeField] float startCooldown = 5;
    public bool endStartCooldown;
    [SerializeField] public float cageCooldown = 10;
    public bool cageIsUp;
    [SerializeField] Transform cage;
    Vector3 defaultCagePosition;
    private float roundTime;
    private int defaultRoundTime;
    private float endTimer;
    public Text timerText;


    [Header("Score and Rounds")]
    public string currentMap;
    public float score;
    public bool endRound;
    private bool sentScores;
    private bool printScore;
    public int roundNum;
    public List<Transform> spawnPoints;
    public bool returnToMenuButton;



    // EVENTS CODES
    private const byte START_GAME_EVENT = 0;
    private const byte END_GAME_EVENT = 1;
    private const byte UPDATE_SEEKER_EVENT = 2;
    private const byte RELOAD_SCENE_EVENT = 3;
    private const byte RESET_TIMER_EVENT = 4;

    private void Start()
    {
        defaultCagePosition = cage.transform.position;
        gameRulesManager = this;
        defaultRoundTime = (int)PhotonNetwork.MasterClient.CustomProperties["time"];
        roundTime = defaultRoundTime;
        Hashtable hash = new Hashtable();
        hash.Add("Ready", false);
        PhotonNetwork.LocalPlayer.SetCustomProperties(hash);
    }

    void Update()
    {
        if (gameStarted)
        {
            gameTimer += Time.deltaTime;
        }

        if (!gameStarted)
        {
            if (PhotonNetwork.LocalPlayer.IsMasterClient)
            {
                if (roundNum == 0)
                {
                    foreach (var player in PhotonNetwork.CurrentRoom.Players.Values)
                    {
                        playersToBeSeekers.Add(player.ActorNumber);
                    }
                }
                StartGame();
                gameStarted = true;
            }
        }
        else if (gameTimer < startCooldown)
        {
            timerText.text = "Starting in: " + ((int)(startCooldown - gameTimer)).ToString();
        }
        else if (!endStartCooldown)
        {
            if (PhotonNetwork.IsMasterClient)
            {
                PhotonNetwork.RaiseEvent(RESET_TIMER_EVENT, new object[] { gameTimer }, RaiseEventOptions.Default, SendOptions.SendReliable);
            }
            UpdateSeeker(startingSeeker);
            SpawnPlayers();
            endStartCooldown = true;
        }
        else if (gameTimer < cageCooldown)
        {
            timerText.text = ((int)(cageCooldown - gameTimer)).ToString();
        }
        else if (cageIsUp)
        {
            cage.Translate(Vector3.up * 0.05f);
        }
        else
        {
            roundTime -= Time.deltaTime;
        }
        endRound = roundTime <= 0;
        cageIsUp = cage.position.y <= 5;


        if (endRound)
        {
            if (!sentScores)
            {
                SendScore(score);

                sentScores = true;
            }
            endTimer += Time.deltaTime;
            if (endTimer > 3 && !printScore)
            {
                PrintScores();
                printScore = true;
            }
            else if (endTimer > 3)
            {
                timerText.text = "Game Over !";
            }
            else if (0 != playersToBeSeekers.Count && PhotonNetwork.LocalPlayer.IsMasterClient)
            {
                PhotonNetwork.RaiseEvent(RELOAD_SCENE_EVENT, new object[] { }, RaiseEventOptions.Default, SendOptions.SendReliable);
                ReloadLevel();
            }
            else if (PhotonNetwork.LocalPlayer.IsMasterClient)
            {
                PhotonNetwork.RaiseEvent(END_GAME_EVENT, new object[] { }, RaiseEventOptions.Default, SendOptions.SendReliable);
                EndGame();
            }
        }
        if (returnToMenuButton)
        {
            timerText.text = "Press 'Q' to return to main menu";
            if (Input.GetKeyDown(KeyCode.Q))
            {
                ReturnToMenu();
            }
        }
    }
    public void StartGame()
    {

        roundNum++;
        int randomIndex = Random.Range(0, playersToBeSeekers.Count - 1);
        startingSeeker = playersToBeSeekers[randomIndex];
        playersToBeSeekers.Remove(playersToBeSeekers[randomIndex]);
        object[] data = new object[] { startingSeeker };
        PhotonNetwork.RaiseEvent(START_GAME_EVENT, data, RaiseEventOptions.Default, SendOptions.SendReliable);
        timerText.gameObject.SetActive(true);

    }
    public void SendScore(float score)
    {
        Hashtable hash = new Hashtable();
        hash.Add("Score", score);
        PhotonNetwork.LocalPlayer.SetCustomProperties(hash);
    }
    private void OnEnable()
    {
        PhotonNetwork.NetworkingClient.EventReceived += NetworkingClient_EventReceived;
    }
    private void OnDisable()
    {
        PhotonNetwork.NetworkingClient.EventReceived -= NetworkingClient_EventReceived;
    }
    private void NetworkingClient_EventReceived(EventData obj)
    {
        if (obj.Code == START_GAME_EVENT)
        {
            if (roundNum == 0)
            {
                foreach (var player in PhotonNetwork.CurrentRoom.Players.Values)
                {
                    playersToBeSeekers.Add(player.ActorNumber);
                }
            }
            object[] data = (object[])obj.CustomData;
            startingSeeker = (int)data[0];
            timerText.gameObject.SetActive(true);
            gameStarted = true;
            roundNum++;

        }
        if (obj.Code == RESET_TIMER_EVENT)
        {
            object[] data = (object[])obj.CustomData;
            float hostTimer = (float)data[0];
            if (gameTimer < hostTimer) gameTimer = hostTimer;
        }
        if (obj.Code == UPDATE_SEEKER_EVENT)
        {
            object[] data = (object[])obj.CustomData;
            int seekerId = (int)data[0];
            UpdateSeeker(seekerId);

        }
        if (obj.Code == RELOAD_SCENE_EVENT)
        {
            object[] data = (object[])obj.CustomData;
            ReloadLevel();

        }
        if (obj.Code == END_GAME_EVENT)
        {
            object[] data = (object[])obj.CustomData;
            EndGame();

        }
    }
    void SpawnPlayers()
    {
        int i = 0;
        foreach (var player in GameObject.FindGameObjectsWithTag("Player").OrderBy(p => p.GetPhotonView().OwnerActorNr))
        {
            if (player.GetComponent<PlayerStatus>().isSeeker)
            {
                player.transform.position = spawnPoints[0].position;
            }
            else
            {
                player.transform.position = spawnPoints[++i].position;
            }
        }
    }

    public void SetFallingPlayerToSeeker(int id)
    {
        PhotonNetwork.RaiseEvent(UPDATE_SEEKER_EVENT, new object[] { id }, RaiseEventOptions.Default, SendOptions.SendReliable);

        UpdateSeeker(id);
    }


    public void UpdateSeeker(int id)
    {
        foreach (var player in GameObject.FindGameObjectsWithTag("Player"))
        {

            if (player.GetPhotonView().OwnerActorNr == id)
            {
                player.GetComponent<PlayerStatus>().isSeeker = true;
            }
            else
            {
                player.GetComponent<PlayerStatus>().isSeeker = false;
            }

            player.GetComponentInChildren<Sensor>().Other = new List<Collider>();
            player.GetComponent<PlayerStatus>().isTargeted = false;
        }

        UpdatePlayersInfos();
    }
    public void UpdatePlayersInfos()
    {
        playerInfos = new List<PlayerInfo>();

        foreach (var player in GameObject.FindGameObjectsWithTag("Player"))
        {
            playerInfos.Add(new PlayerInfo(player.GetPhotonView().OwnerActorNr, player.GetPhotonView().Owner.NickName, player.GetComponent<PlayerStatus>().isSeeker, 0));
        }

        for (int i = 0; i < playerInfos.Count; i++)
        {
            playersUIHolder.transform.GetChild(i).GetComponent<Text>().text = (playerInfos[i].name + "  " + (playerInfos[i].isSeeker ? "SEEKER" : ""));
        }
    }
    void PrintScores()
    {
        foreach (var playerInf in playerInfos)
        {
            foreach (var player in PhotonNetwork.CurrentRoom.Players)
            {
                if (playerInf.id == player.Value.ActorNumber)
                {
                   // Debug.Log((float)player.Value.CustomProperties["Score"]);
                    playerInf.score = (float)player.Value.CustomProperties["Score"];
                }
            }
        }
        playerInfos = playerInfos.OrderBy(p => p.score).ToList<PlayerInfo>();


        for (int i = 0; i < playerInfos.Count; i++)
        {
            playersUIHolder.transform.GetChild(i).GetComponent<Text>().text = (playerInfos[i].name + "  Score:" + (int)(playerInfos[i].score));

        }

    }
    void EndGame()
    {
        timerText.text = "Game Over,Press 'Q' to return to main menu";
        PrintScores();
        UpdateSeeker(999);
        returnToMenuButton = true;
    }
    public void ReturnToMenu()
    {
        PhotonNetwork.LeaveRoom();
        PhotonNetwork.LoadLevel("Lobby");
        returnToMenuButton = false;
    }
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {

    }
    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        UpdatePlayersInfos();
    }
    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        base.OnPlayerLeftRoom(otherPlayer);
        playersToBeSeekers.Remove(otherPlayer.ActorNumber);
        UpdatePlayersInfos();
    }

    public void ReloadLevel()
    {
        cage.transform.position = defaultCagePosition;
        gameStarted = false;
        gameTimer = 0;
        endStartCooldown = false;
        cageIsUp = false;
        roundTime = defaultRoundTime;
        endTimer = 0;
        endRound = false;
        sentScores = false;
        printScore = false;
        UpdateSeeker(999);
    }
}

public class PlayerInfo
{
    public int id;
    public string name;
    public bool isSeeker;
    public float score;
    public PlayerInfo(int _id, string _name, bool _isSeeker, float _score)
    {
        id = _id;
        name = _name;
        isSeeker = _isSeeker;
        score = _score;
    }
}