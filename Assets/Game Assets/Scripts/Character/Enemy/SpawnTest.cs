using UnityEngine;
using Mirror;
using System.Collections.Generic;
using System; 
public class SpawnTest : NetworkBehaviour
{
    private List<SpawnableObjectByLevel<EnemyDetailsSO>> testLevelSpawnList;

    private RandomSpawnableObject<EnemyDetailsSO> randomEnemyHelperClass;

    private List<GameObject> instantiatedEnemyList = new List<GameObject>();

    void OnEnable()
    {
        StaticEventHandler.OnRoomChanged += StaticEventHandler_OnRoomChanged;
    }
    void OnDisable()
    {
        StaticEventHandler.OnRoomChanged -= StaticEventHandler_OnRoomChanged;
    }

    private void StaticEventHandler_OnRoomChanged(RoomChangedEventArgs roomChangedEventArgs)
    {
        if (instantiatedEnemyList != null && instantiatedEnemyList.Count > 0)
        {
            foreach (GameObject enemy in instantiatedEnemyList)
            {
                Destroy(enemy);
            }
        }

        RoomTemplateSO roomTemplate = DungeonBuilder.Instance.GetRoomTemplate(roomChangedEventArgs.room.templateID);

        if (roomTemplate != null)
        {
            testLevelSpawnList = roomTemplate.enemysByLevelList;

            randomEnemyHelperClass = new RandomSpawnableObject<EnemyDetailsSO>(testLevelSpawnList);
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            EnemyDetailsSO enemyDetails = randomEnemyHelperClass.GetItem();

            if (enemyDetails != null)
            {
                instantiatedEnemyList.Add(Instantiate(enemyDetails.enemyPrefab, HelpfulUtility.GetMousePosition(), Quaternion.identity));
            }
        }
    }
}
