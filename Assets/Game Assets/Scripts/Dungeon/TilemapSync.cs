using UnityEngine;
using Mirror;
using UnityEngine.Tilemaps;
public class TilemapSync : NetworkBehaviour
{
    [SerializeField] private Tilemap collisionTileMap;

    [Server]
    public void DisableCollisionTilemapRenderer()
    {
        // Disable the TilemapRenderer on the server
        TilemapRenderer renderer = collisionTileMap.gameObject.GetComponent<TilemapRenderer>();
        if (renderer != null)
        {
            renderer.enabled = false;
        }

        // Call the ClientRpc to disable it on all clients
        RpcDisableCollisionTilemapRenderer();
    }

    [ClientRpc]
    private void RpcDisableCollisionTilemapRenderer()
    {
        // Disable the TilemapRenderer on the client
        TilemapRenderer renderer = collisionTileMap.gameObject.GetComponent<TilemapRenderer>();
        if (renderer != null)
        {
            renderer.enabled = false;
        }
    }
}
