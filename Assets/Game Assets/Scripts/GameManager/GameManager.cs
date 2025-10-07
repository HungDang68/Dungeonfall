using System;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.SceneManagement;

[DisallowMultipleComponent]
public class GameManager : SingletomMonobehavior<GameManager>
{
    [SerializeField] private PanelSwapper panelSwapper;
    [SerializeField] private List<DungeonLevel> dungeonLevelList;

    [SerializeField] private int currentDungeonLevelListIndex = 0;
    [HideInInspector] public GameState gameState;
    public GameObject SwapUI;
    private Player player;
    private Room currentRoom;
    private Room previousRoom;
    private void Start()
    {
        gameState = GameState.gameStarted;

        player = FindAnyObjectByType<Player>();
    }
    private void Update()
    {
        HandleGameState();

        if (Input.GetKeyDown(KeyCode.R))
        {
            gameState = GameState.gameStarted;
        }
        if (Input.GetKeyDown(KeyCode.T))
        {
            Debug.Log("gameLost");
            gameState = GameState.gameLost;
        }
        if (Input.GetKeyDown(KeyCode.Y))
        {
            Debug.Log("gameWon");
            gameState = GameState.gameWon;
        }
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Debug.Log("gamePaused");
            gameState = GameState.gamePaused;
        }
    }
    void OnEnable()
    {
        StaticEventHandler.OnRoomChanged += StaticEventHandler_OnRoomChanged;

    }
    void OnDisable()
    {
        StaticEventHandler.OnRoomChanged -= StaticEventHandler_OnRoomChanged;
    }

    private void HandleGameState()
    {
        switch (gameState)
        {
            case GameState.gameStarted:
                PlayDungeonLevel(currentDungeonLevelListIndex);

                gameState = GameState.playingLevel;

                break;
            case GameState.levelCompleted:
                LevelCompleteDealer();

                break;
            case GameState.gameWon:
                GameWonDealer();

                break;

            case GameState.gameLost:
                GameLostDealer();

                break;
            case GameState.gamePaused:
                Pause();

                break;
        }
    }

    [Command(requiresAuthority = false)]
    public void Pause()
    {
        PauseRPC();
    }

    [Server]
    private void PauseRPC()
    {
        panelSwapper.SwapPanel("Pause");
        Time.timeScale = 0f;
    }

    [Command(requiresAuthority = false)]
    public void Resume()
    {
        ResumeRPC();
    }


    private void ResumeRPC()
    {
        gameState = GameState.playingLevel;
        Time.timeScale = 1f;
        panelSwapper.SwapPanel("");
    }
    public void Quit()
    {
        Time.timeScale = 1f;
        if (NetworkServer.active)
        {
            // Host is leaving - stop host and disconnect all clients
            Debug.Log("Host is leaving the room - stopping host");
            NetworkManager.singleton.StopHost();
        }
        else if (NetworkClient.isConnected)
        {
            // Client is leaving - stop client
            Debug.Log("Client is leaving the room - stopping client");
            NetworkManager.singleton.StopClient();
        }
        else
        {
            Debug.Log("Not connected to any room");
        }
    }
    [Command(requiresAuthority = false)]
    private void GameWonDealer()
    {
        GameWonDealerRPC();
    }

    [Server]
    private void GameWonDealerRPC()
    {
        panelSwapper.SwapPanel("WinPanel");
    }

    [Command(requiresAuthority = false)]
    private void GameLostDealer()
    {
        GameLostDealerRPC();
    }

    [Server]
    private void GameLostDealerRPC()
    {
        panelSwapper.SwapPanel("LosePanel");
    }

    private void LevelCompleteDealer()
    {
        if (currentDungeonLevelListIndex == dungeonLevelList.Count - 1)
        {
            gameState = GameState.gameWon;
        }
        //Complete level stuff here
        else
        {

        }
    }

    public void SetPlayerDeath()
    {
        List<Player> players = GetAllPlayers();
        int deadPlayerCount = 0;

        foreach (Player player in players)
        {
            if (player.health.GetHealth() <= 0)
            {
                deadPlayerCount++;
            }
            else
            {
                continue;
            }
        }

        if (deadPlayerCount >= players.Count)
        {
            gameState = GameState.gameLost;
        }
    }

    public void SetLevelComplete()
    {
        gameState = GameState.levelCompleted;
    }

    public void SetCurrentRoom(Room room)
    {
        if (room == null)
        {
            Debug.LogError("Attempting to set a null room or a room with a null InstantiatedRoom as the current room.");
        }
        previousRoom = currentRoom;
        currentRoom = room;
    }
    private void StaticEventHandler_OnRoomChanged(RoomChangedEventArgs roomChangedEventArgs)
    {
        SetCurrentRoom(roomChangedEventArgs.room);
    }
    private void PlayDungeonLevel(int dungeonLevelListIndex)
    {
        if (!NetworkServer.active)
        {
            Debug.Log("Only the host can trigger dungeon generation.");
            return;
        }

        bool dungeonBuildSuccessfully = DungeonBuilder.Instance.GenerateDungeon(dungeonLevelList[dungeonLevelListIndex]);

        if (!dungeonBuildSuccessfully)
        {
            Debug.LogError("Couldn't build the dungeon");
            return;
        }

        if (currentRoom == null)
        {
            Debug.LogError("Current room is null after dungeon generation.");
            return;
        }
        StaticEventHandler.CallRoomChangedEvent(currentRoom);
    }

    private new void OnValidate()
    {
        HelpfulUtility.ValidateCheckEnumerableValues(this, nameof(dungeonLevelList), dungeonLevelList);
    }

    public Player GetPlayer()
    {
        player = FindAnyObjectByType<Player>();
        if (player == null)
        {
            Debug.LogError("Player reference is null. Ensure the Player object exists in the scene.");
        }
        return player;
    }
    public List<Player> GetAllPlayers()
    {
        List<Player> players = new List<Player>(FindObjectsByType<Player>(FindObjectsSortMode.None));
        if (players == null)
        {
            Debug.LogError("Player reference is null. Ensure the Player object exists in the scene.");
        }
        return players;
    }

    public Room GetCurrentRoom()
    {
        return currentRoom;
    }
    public Room GetPriviousRoom()
    {
        return previousRoom;
    }
    public DungeonLevel GetCurrentDungeonLevel()
    {
        return dungeonLevelList[currentDungeonLevelListIndex];
    }

    [Command(requiresAuthority = false)]
    public void OnWinButtonClicked()
    {
        HandleGameEnd("Win");
    }

    [Command(requiresAuthority = false)]
    public void OnLoseButtonClicked()
    {
        HandleGameEnd("Lose");
    }


    [Server]
    private void HandleGameEnd(string result)
    {
        Debug.Log($"Game ended with result: {result}");

        // Only the server (host) should stop the host
        if (NetworkServer.active)
        {
            Debug.Log("Stopping the host...");
            NetworkManager.singleton.StopHost();
        }
        else
        {
            Debug.LogWarning("HandleGameEnd called on a client. This should only be called on the server.");
        }
    }
}
