using System.Collections.Generic;
using UnityEngine;
[System.Serializable]
public class SpawnableObjectByLevel<T>
{
    public DungeonLevel dungeonLevel;
    public List<SpawnableObjectRatio<T>> spawnableObjectRatioList;
}
