using Mirror;
using Unity.Cinemachine;
using UnityEngine;

public class PlayerSetter : NetworkBehaviour
{
    [SerializeField] private GameObject weaponGO;
    [SerializeField] private GameObject sprite;
    [SerializeField] private GameObject playerCamera;
    public override void OnStartLocalPlayer()
    {
        SetActive();
    }
    public void SetActive()
    {
        if (!isLocalPlayer) { return; }

        if (playerCamera != null)
        {
            playerCamera.SetActive(true);
        }
        if (weaponGO != null)
        {
            weaponGO.SetActive(true);
        }
        if (sprite != null)
        {
            sprite.SetActive(true);
        }
    }
    public void SetNotActive()
    {
        if (!isLocalPlayer) { return; }
        
        if (playerCamera != null)
        {
            playerCamera.SetActive(false);
        }
        if (weaponGO != null)
        {
            weaponGO.SetActive(false);
        }
        if (sprite != null)
        {
            sprite.SetActive(false);
        }
    }
}
