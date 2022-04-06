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
    [SerializeField] Animator linkDoor;

    [Header("Player Information")]
    List<PlayerInfo> playerInfos = new List<PlayerInfo>();
    public GameObject playersUIHolder;
    public static GameRulesManager gameRulesManager;
    [SerializeField] private int startingSeeker;
    private List<int> playersToBeSeekers = new List<int>();
    private int seekersCount = 0;


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
    public Text scoreText;
    public Text timerText;
    float startingServerTime;


    [Header("Score and Rounds")]
    public string currentMap;
    public float score;
    public bool endRound;
    private bool sentScores;
    private bool printScore;
    public int roundNum;
    public List<Transform> spawnPoints;
    public bool gameEnded;
    public bool returnToMenuButton;
    private int updateScoreInServer = 0;



    // EVENTS CODES
    private const byte START_GAME_EVENT = 0;
    private const byte END_GAME_EVENT = 1;
    private const byte UPDATE_SEEKER_EVENT = 2;
    private const byte RELOAD_SCENE_EVENT = 3;
    private const byte RESET_TIMER_EVENT = 4;
    private const byte END_ROUND_EVENT = 5;
    private const byte SEND_SCORE_EVENT = 76;
    private const byte OPEN_DOOR_EVENT = 20;

    private void Start()
    {
        defaultCagePosition = cage.transform.position;
        gameRulesManager = this;
        defaultRoundTime = RoomsManager.roundTime;
        roundTime = defaultRoundTime;
    }

    void Update()
    {
        if (updateScoreInServer < Time.time && PhotonNetwork.IsMasterClient)
        {
            updateScoreInServer = (int)(Time.time) + 2;
            Hashtable hash = new Hashtable();
            hash.Add("Score", playerInfos);
            PhotonNetwork.CurrentRoom.SetCustomProperties(hash);
        }
        if (gameStarted)
        {
            gameTimer = (float)PhotonNetwork.Time - startingServerTime;
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
            timerText.text = "Round " + roundNum;
        }
        else if (!endStartCooldown)
        {
            UpdateSeeker(startingSeeker);
            SpawnPlayers();
            endStartCooldown = true;
        }
        else if (gameTimer < cageCooldown)
        {
            timerText.text = "Round " + roundNum + " Starting in: " + ((int)(cageCooldown - gameTimer)).ToString();
        }
        else if (cageIsUp)
        {
            roundTime = defaultRoundTime + cageCooldown - gameTimer;
            timerText.text = "RUN!";
            cage.Translate(Vector3.up * 3 * Time.deltaTime);
        }
        else
        {
            roundTime = defaultRoundTime + cageCooldown - gameTimer;
            if (!gameEnded && roundTime >= 0)
                timerText.text = ((int)roundTime).ToString();
        }
        if (!endRound) endRound = roundTime < 0;

        cageIsUp = cage.position.y <= 5;


        if (endRound)
        {
            endTimer = gameTimer - defaultRoundTime - cageCooldown;
            if (endTimer > 3 && !gameEnded)
            {
                timerText.text = "Round over";

            }

            else if (0 != playersToBeSeekers.Count && PhotonNetwork.IsMasterClient)
            {
                PhotonNetwork.RaiseEvent(RELOAD_SCENE_EVENT, new object[] { }, RaiseEventOptions.Default, SendOptions.SendReliable);
                ReloadLevel();
            }
            else if (PhotonNetwork.IsMasterClient)
            {
                PhotonNetwork.RaiseEvent(END_GAME_EVENT, new object[] { }, RaiseEventOptions.Default, SendOptions.SendReliable);
                EndGame();
            }
        }

        if (returnToMenuButton)
        {
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
        startingServerTime = (float)PhotonNetwork.Time;
        playersToBeSeekers.Remove(playersToBeSeekers[randomIndex]);
        object[] data = new object[] { startingSeeker, startingServerTime };
        PhotonNetwork.RaiseEvent(START_GAME_EVENT, data, RaiseEventOptions.Default, SendOptions.SendReliable);
        timerText.gameObject.SetActive(true);

    }
    public void SendScore(float score)
    {
        scoreText.text = "Calculating Scores..";
        PhotonNetwork.RaiseEvent(SEND_SCORE_EVENT, new object[] { playerInfos }, RaiseEventOptions.Default, SendOptions.SendReliable);
        PrintScores();
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
            object[] data = (object[])obj.CustomData;
            if (roundNum == 0)
            {
                foreach (var player in PhotonNetwork.CurrentRoom.Players.Values)
                {
                    playersToBeSeekers.Add(player.ActorNumber);
                }
            }
            startingServerTime = (float)data[1];
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
        if (obj.Code == SEND_SCORE_EVENT)
        {
            object[] data = (object[])obj.CustomData;
            playerInfos = (List<PlayerInfo>)data[0];
            PrintScores();
        }
        if (obj.Code == OPEN_DOOR_EVENT)
        {
            object[] data = (object[])obj.CustomData;
            linkDoor.SetTrigger("Trigger");
        }
        if (obj.Code == END_ROUND_EVENT)
        {
            object[] data = (object[])obj.CustomData;
            endRound = true;
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
            if (playerInfos[i].isSeeker) seekersCount++;
            playersUIHolder.transform.GetChild(i).GetComponent<Text>().text = (playerInfos[i].name + "  " + (playerInfos[i].isSeeker ? "SEEKER" : ""));
        }
    }
    void PrintScores()
    {
        scoreText.text = "Press 'Q' to quit";

        playerInfos = playerInfos.OrderBy(p => p.score).ToList<PlayerInfo>();

        for (int i = 0; i < playerInfos.Count; i++)
        {
            playersUIHolder.transform.GetChild(i).GetComponent<Text>().text = (playerInfos[i].name + "  Score:" + (int)(playerInfos[i].score));
            Debug.Log((playerInfos[i].name + "  Score:" + (int)(playerInfos[i].score)));
        }
        timerText.text = playerInfos[0].name + " Wins";

    }
    void EndGame()
    {
        if (!sentScores && PhotonNetwork.IsMasterClient)
        {
            SendScore(score);
            sentScores = true;
        }
        timerText.text = "Game Over,Press 'Q' to return to main menu";
        gameEnded = true;
        returnToMenuButton = true;
        foreach (var player in GameObject.FindGameObjectsWithTag("Player"))
        {


            player.GetComponent<PlayerStatus>().isSeeker = false;


            player.GetComponentInChildren<Sensor>().Other = new List<Collider>();
            player.GetComponent<PlayerStatus>().isTargeted = false;
        }
    }
    public void ReturnToMenu()
    {
        Hashtable hash = new Hashtable();
        hash.Add("Ready", false);
        PhotonNetwork.LocalPlayer.SetCustomProperties(hash);
        PhotonNetwork.LeaveRoom();
        returnToMenuButton = false;
    }
    public override void OnLeftRoom()
    {
        base.OnLeftRoom();
        SceneManager.LoadScene("Lobby");
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
        if (PhotonNetwork.IsMasterClient)
        {
            foreach (var player in playerInfos)
            {
                if (player.isSeeker) return;
            }
        }
    }
    void OnApplicationQuit()
    {
        PhotonNetwork.LeaveRoom();
    }
    public void ReloadLevel()
    {
        cage.transform.position = defaultCagePosition;
        gameStarted = false;
        endStartCooldown = false;
        cageIsUp = false;
        roundTime = defaultRoundTime;
        endTimer = 0;
        endRound = false;
        foreach (var player in GameObject.FindGameObjectsWithTag("Player"))
        {
            player.GetComponent<PlayerStatus>().isSeeker = false;
        }
    }
    void ScoreCalculator()
    {
        bool allSeekers = true;
        bool allNotSeekers = true;
        foreach (PlayerInfo player in playerInfos)
        {
            if (player.isSeeker)
            {
                player.score += Time.deltaTime;
                allNotSeekers = false;
            }
            else
            {
                allSeekers = false;
            }
        }
        if (!gameEnded)
            gameEnded = allSeekers || allNotSeekers;
    }
    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        base.OnMasterClientSwitched(newMasterClient);
        if (newMasterClient.IsLocal)
        {
            playerInfos = (List<PlayerInfo>)PhotonNetwork.CurrentRoom.CustomProperties["Score"];
        }
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