using System.Collections;
using UnityEngine;

[DisallowMultipleComponent]
public class EnemySpawner : SingletomMonobehavior<EnemySpawner>
{
    private int enemiesToSpawn;
    private int currentEnemyCount;
    private int enemiesSpawnedSoFar;
    private int enemyMaxConcurrentSpawnNumber;
    private Room currentRoom;
    private RoomEnemySpawnParameter roomEnemySpawnParameters;

    private void OnEnable()
    {
        StaticEventHandler.OnRoomChanged += StaticEventHandler_OnRoomChanged;
    }

    private void OnDisable()
    {
        StaticEventHandler.OnRoomChanged -= StaticEventHandler_OnRoomChanged;
    }
    private void StaticEventHandler_OnRoomChanged(RoomChangedEventArgs roomChangedEventArgs)
    {
        enemiesSpawnedSoFar = 0;
        currentEnemyCount = 0;

        currentRoom = roomChangedEventArgs.room;



        if (currentRoom.roomNodeType.isCorridorEW || currentRoom.roomNodeType.isCorridorNS || currentRoom.roomNodeType.isEntrance)
            return;

        if (currentRoom.isClearedOfEnermies) return;

        enemiesToSpawn = currentRoom.GetNumberOfEnemiesToSpawn(GameManager.Instance.GetCurrentDungeonLevel());

        roomEnemySpawnParameters = currentRoom.GetRoomEnemySpawnParameters(GameManager.Instance.GetCurrentDungeonLevel());


        if (enemiesToSpawn == 0)
        {
            currentRoom.isClearedOfEnermies = true;

            return;
        }


        enemyMaxConcurrentSpawnNumber = GetConcurrentEnemies();


        SpawnEnemies();
    }
    private void SpawnEnemies()
    {
        if (GameManager.Instance.gameState == GameState.playingLevel)
        {
            GameManager.Instance.gameState = GameState.engaingEnemies;
        }

        StartCoroutine(SpawnEnemiesRoutine());
    }
    private IEnumerator SpawnEnemiesRoutine()
    {
        Grid grid = currentRoom.instatiatedRoom.grid;

        RandomSpawnableObject<EnemyDetailsSO> randomEnemyHelperClass = new RandomSpawnableObject<EnemyDetailsSO>(currentRoom.enemiesByLevelList);


        if (currentRoom.spawnPositionArray.Length > 0)
        {
            for (int i = 0; i < enemiesToSpawn; i++)
            {
                while (currentEnemyCount >= enemyMaxConcurrentSpawnNumber)
                {
                    yield return null;
                }

                Vector3Int cellPosition = (Vector3Int)currentRoom.spawnPositionArray[Random.Range(0, currentRoom.spawnPositionArray.Length)];

                CreateEnemy(randomEnemyHelperClass.GetItem(), grid.CellToWorld(cellPosition));

                yield return new WaitForSeconds(GetEnemySpawnInterval());
            }
        }
    }
    private float GetEnemySpawnInterval()
    {
        return (Random.Range(roomEnemySpawnParameters.minSpawnInterval, roomEnemySpawnParameters.maxSpawnInterval));
    }

    private int GetConcurrentEnemies()
    {
        return (Random.Range(roomEnemySpawnParameters.minConcurrentEnemies, roomEnemySpawnParameters.maxConcurrentEnemies));
    }
    private void CreateEnemy(EnemyDetailsSO enemyDetails, Vector3 position)
    {
        enemiesSpawnedSoFar++;

        currentEnemyCount++;

        DungeonLevel dungeonLevel = GameManager.Instance.GetCurrentDungeonLevel();


        GameObject enemy = Instantiate(enemyDetails.enemyPrefab, position, Quaternion.identity, transform);

        enemy.GetComponent<Enemy>().EnemyInit(enemyDetails, enemiesSpawnedSoFar, dungeonLevel);


    }
}
