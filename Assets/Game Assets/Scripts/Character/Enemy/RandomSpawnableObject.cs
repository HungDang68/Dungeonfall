using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class RandomSpawnableObject<T>
{
    private struct chanceBoundaries
    {
        public T spawnableObject;
        public int lowBoundaryValue;
        public int highBoundaryValue;
    }

    private int ratioValueTotal = 0;
    private List<chanceBoundaries> chanceBoundariesList = new List<chanceBoundaries>();
    private List<SpawnableObjectByLevel<T>> spawnableObjectsByLevelList;

    public RandomSpawnableObject(List<SpawnableObjectByLevel<T>> spawnableObjectsByLevelList)
    {
        this.spawnableObjectsByLevelList = spawnableObjectsByLevelList;
    }

    public T GetItem()
    {
        int upperBoundary = -1;
        ratioValueTotal = 0;
        chanceBoundariesList.Clear();
        T spawnableObject = default(T);

        foreach (SpawnableObjectByLevel<T> spawnableObjectsByLevel in spawnableObjectsByLevelList)
        {
            if (spawnableObjectsByLevel.dungeonLevel == GameManager.Instance.GetCurrentDungeonLevel())
            {
                foreach (SpawnableObjectRatio<T> spawnableObjectRatio in spawnableObjectsByLevel.spawnableObjectRatioList)
                {
                    int lowerBoundary = upperBoundary + 1;

                    upperBoundary = lowerBoundary + spawnableObjectRatio.ratio - 1;

                    ratioValueTotal += spawnableObjectRatio.ratio;

                    chanceBoundariesList.Add(new chanceBoundaries() { spawnableObject = spawnableObjectRatio.dungeonObject, lowBoundaryValue = lowerBoundary, highBoundaryValue = upperBoundary });

                }
            }
        }

        if (chanceBoundariesList.Count == 0) return default(T);

        int lookUpValue = Random.Range(0, ratioValueTotal);

        foreach (chanceBoundaries spawnChance in chanceBoundariesList)
        {
            if (lookUpValue >= spawnChance.lowBoundaryValue && lookUpValue <= spawnChance.highBoundaryValue)
            {
                spawnableObject = spawnChance.spawnableObject;
                break;
            }
        }


        return spawnableObject;
    }
}
