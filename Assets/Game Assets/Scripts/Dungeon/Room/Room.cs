using System.Collections.Generic;
using UnityEngine;

public class Room
{
    public string id;

    public string templateID;

    public GameObject prefab;
    public RoomNodeTypeSO roomNodeType;
    public Vector2Int lowerBounds;
    public Vector2Int upperBounds;
    public Vector2Int templateLowerBounds;
    public Vector2Int templateUpperBounds;
    public Vector2Int[] spawnPositionArray;
    public List<string> childRoomIDList;
    public string parentRoomID;
    public List<Doorway> doorwayList;
    public bool isPositioned = false;
    public InstantiatedRoom instatiatedRoom;
    public bool isLit = false;
    public bool isClearedOfEnermies = false;
    public bool isPrevouslyVisited = false;
    public List<SpawnableObjectByLevel<EnemyDetailsSO>> enemiesByLevelList;
    public List<RoomEnemySpawnParameter> roomLevelEnemySpawnParameterList;

    public Room()
    {
        childRoomIDList = new List<string>();
        doorwayList = new List<Doorway>();
    }

    public int GetNumberOfEnemiesToSpawn(DungeonLevel dungeonLevel)
    {
        foreach (RoomEnemySpawnParameter roomEnemySpawnParameter in roomLevelEnemySpawnParameterList)
        {
            if (roomEnemySpawnParameter.dungeonLevel == dungeonLevel)
            {
                return Random.Range(roomEnemySpawnParameter.minTotalEnemiesToSpawn, roomEnemySpawnParameter.maxTotalEnemiesToSpawn);
            }
        }
        return 0;
    }
    public RoomEnemySpawnParameter GetRoomEnemySpawnParameters(DungeonLevel dungeonLevel)
    {
        foreach (RoomEnemySpawnParameter roomEnemySpawnParameters in roomLevelEnemySpawnParameterList)
        {
            if (roomEnemySpawnParameters.dungeonLevel == dungeonLevel)
            {
                return roomEnemySpawnParameters;
            }
        }
        return null;
    }
}
