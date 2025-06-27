using System;
using UnityEngine;
using UnityEngine.Tilemaps;

[RequireComponent(typeof(BoxCollider2D))]
public class InstantiatedRoom : MonoBehaviour
{
    [HideInInspector] public Room room;
    [HideInInspector] public Grid grid;
    [HideInInspector] public Tilemap groundTileMap;
    [HideInInspector] public Tilemap decoration1TileMap;
    [HideInInspector] public Tilemap decoration2TileMap;
    [HideInInspector] public Tilemap frontTileMap;
    [HideInInspector] public Tilemap collisionTileMap;
    [HideInInspector] public Tilemap minimapTileMap;
    [HideInInspector] public int[,] aStarMovementPenalty;
    [HideInInspector] public Bounds roomColliderBounds;

    private BoxCollider2D boxCollider2D;

    private void Awake()
    {
        boxCollider2D = GetComponent<BoxCollider2D>();

        roomColliderBounds = boxCollider2D.bounds;
    }

    public void Initialise(GameObject roomGameObject)
    {
        PopulateTilemapMemberVariables(roomGameObject);
        BlockoffUnsedDoorWay();
        AddObstaclesAndPreferedPath();
        DisableCollisionTilemapRenderer();
    }

    private void AddObstaclesAndPreferedPath()
    {
        aStarMovementPenalty = new int[room.templateUpperBounds.x - room.templateLowerBounds.x + 1,
        room.templateUpperBounds.y - room.templateLowerBounds.y + 1];

        for (int x = 0; x < (room.templateUpperBounds.x - room.templateLowerBounds.x + 1); x++)
        {
            for (int y = 0; y < room.templateUpperBounds.y - room.templateLowerBounds.y + 1; y++)
            {
                aStarMovementPenalty[x, y] = Settings.defaultAStarMovementPenalty;

                TileBase tile = collisionTileMap.GetTile(new Vector3Int(x + room.templateLowerBounds.x, y + room.templateLowerBounds.y, 0));

                foreach (TileBase collisionTile in GameResources.Instance.enemyUnwalkableCollissionTilesArray)
                {
                    if (tile == collisionTile)
                    {
                        aStarMovementPenalty[x, y] = 0;
                        break;
                    }
                }

                if (tile == GameResources.Instance.preferredEnemyPathTile)
                {
                    aStarMovementPenalty[x, y] = Settings.preferedPathAStarMovementPenalty;
                }
            }
        }
        ;
    }

    private void BlockoffUnsedDoorWay()
    {
        foreach (Doorway doorway in room.doorwayList)
        {
            if (doorway.isConnected)
            {
                continue;
            }
            if (collisionTileMap != null)
            {
                BlockADoorwayOnTilemapLayer(collisionTileMap, doorway);
            }
            if (collisionTileMap != null)
            {
                BlockADoorwayOnTilemapLayer(minimapTileMap, doorway);
            }
            if (collisionTileMap != null)
            {
                BlockADoorwayOnTilemapLayer(groundTileMap, doorway);
            }
            if (collisionTileMap != null)
            {
                BlockADoorwayOnTilemapLayer(decoration1TileMap, doorway);
            }
            if (collisionTileMap != null)
            {
                BlockADoorwayOnTilemapLayer(decoration2TileMap, doorway);
            }
            if (collisionTileMap != null)
            {
                BlockADoorwayOnTilemapLayer(frontTileMap, doorway);
            }
        }
    }

    private void BlockADoorwayOnTilemapLayer(Tilemap tileMap, Doorway doorway)
    {
        switch (doorway.orientation)
        {
            case Orientation.north:
            case Orientation.south:
                BlockDoorwayHorizontally(tileMap, doorway);
                break;
            case Orientation.west:
            case Orientation.east:
                BlockDoorwayVertically(tileMap, doorway);
                break;
            case Orientation.none:
                break;
        }
    }

    private void BlockDoorwayVertically(Tilemap tileMap, Doorway doorway)
    {
        Vector2Int startPosition = doorway.doorwayStartCopyPosition;

        for (int yPos = 0; yPos < doorway.doorwayCopyTileHeight; yPos++)
        {
            for (int xPos = 0; xPos < doorway.doorwayCopyTileWidth; xPos++)
            {
                Matrix4x4 transformMatrix = tileMap.GetTransformMatrix(new Vector3Int(startPosition.x + xPos, startPosition.y - yPos, 0));

                tileMap.SetTile(new Vector3Int(startPosition.x + xPos, startPosition.y - 1 - yPos, 0), tileMap.GetTile(new Vector3Int(startPosition.x +
                xPos, startPosition.y - yPos, 0)));

                tileMap.SetTransformMatrix(new Vector3Int(startPosition.x + xPos, startPosition.y - 1 - yPos, 0), transformMatrix);

            }
        }
    }

    private void BlockDoorwayHorizontally(Tilemap tileMap, Doorway doorway)
    {
        Vector2Int startPosition = doorway.doorwayStartCopyPosition;

        for (int xPos = 0; xPos < doorway.doorwayCopyTileWidth; xPos++)
        {
            for (int yPos = 0; yPos < doorway.doorwayCopyTileHeight; yPos++)
            {
                Matrix4x4 transformMatrix = tileMap.GetTransformMatrix(new Vector3Int(startPosition.x + xPos, startPosition.y - yPos, 0));

                tileMap.SetTile(new Vector3Int(startPosition.x + 1 + xPos, startPosition.y - yPos, 0), tileMap.GetTile(new Vector3Int(startPosition.x +
                xPos, startPosition.y - yPos, 0)));

                tileMap.SetTransformMatrix(new Vector3Int(startPosition.x + 1 + xPos, startPosition.y - yPos, 0), transformMatrix);

            }
        }
    }

    private void DisableCollisionTilemapRenderer()
    {
        collisionTileMap.gameObject.GetComponent<TilemapRenderer>().enabled = false;
    }

    private void PopulateTilemapMemberVariables(GameObject roomGameObject)
    {
        grid = roomGameObject.GetComponentInChildren<Grid>();

        Tilemap[] tilemaps = roomGameObject.GetComponentsInChildren<Tilemap>();

        foreach (Tilemap tilemap in tilemaps)
        {
            if (tilemap.gameObject.tag == "groundTilemap")
            {
                groundTileMap = tilemap;
            }
            else if (tilemap.gameObject.tag == "decoration1Tilemap")
            {
                decoration1TileMap = tilemap;
            }
            else if (tilemap.gameObject.tag == "decoration2Tilemap")
            {
                decoration2TileMap = tilemap;
            }
            else if (tilemap.gameObject.tag == "frontTilemap")
            {
                frontTileMap = tilemap;
            }
            else if (tilemap.gameObject.tag == "collisionTilemap")
            {
                collisionTileMap = tilemap;
            }
            else if (tilemap.gameObject.tag == "minimapTilemap")
            {
                minimapTileMap = tilemap;
            }
        }
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // If the player triggered the collider
        if (collision.tag == Settings.playerTag && room != GameManager.Instance.GetCurrentRoom())
        {
            Debug.Log("Player entered room");
            // Set room as visited
            this.room.isPrevouslyVisited = true;

            // Call room changed event
            StaticEventHandler.CallRoomChangedEvent(room);
        }
    }

}
