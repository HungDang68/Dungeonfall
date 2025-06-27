using UnityEngine;
using UnityEngine.Tilemaps;


public class GameResources : MonoBehaviour
{
    private static GameResources instance;

    public static GameResources Instance
    {
        get
        {
            if (instance == null)
            {
                instance = Resources.Load<GameResources>("GameResources");
            }
            return instance;
        }
    }

    public RoomNodeTypeListSO roomNodeTypeList;

    public TileBase[] enemyUnwalkableCollissionTilesArray;

    public TileBase preferredEnemyPathTile;


}
