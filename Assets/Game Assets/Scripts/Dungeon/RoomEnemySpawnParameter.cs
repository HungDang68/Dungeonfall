using UnityEngine;

[System.Serializable]
public class RoomEnemySpawnParameter
{
    public DungeonLevel dungeonLevel;
    public int minTotalEnemiesToSpawn;
    public int maxTotalEnemiesToSpawn;
    public int minConcurrentEnemies;
    public int maxConcurrentEnemies;
    public int minSpawnInterval;
    public int maxSpawnInterval;
}
