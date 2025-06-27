using System;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

[DisallowMultipleComponent]
public class GameManager : SingletomMonobehavior<GameManager>
{
    [SerializeField] private List<DungeonLevel> dungeonLevelList;

    [SerializeField] private int currentDungeonLevelListIndex = 0;
    [HideInInspector] public GameState gameState;
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
        }
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
        // if (!NetworkServer.active)
        // {
        //     Debug.Log("Only the host can trigger dungeon generation.");
        //     return;
        // }

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

    public Room GetCurrentRoom()
    {
        return currentRoom;
    }
    public DungeonLevel GetCurrentDungeonLevel()
    {
        return dungeonLevelList[currentDungeonLevelListIndex];
    }
}
