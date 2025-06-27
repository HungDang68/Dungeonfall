using Mirror;
using Unity.Cinemachine;
using UnityEngine;

public class PlayerSetter : NetworkBehaviour
{
    [SerializeField] private GameObject canvas;
    [SerializeField] private GameObject weaponGO;
    [SerializeField] private GameObject playerCamera;
    public override void OnStartLocalPlayer()
    {
        if (playerCamera != null)
        {
            playerCamera.SetActive(true);
        }
        if (canvas != null)
        {
            canvas.SetActive(true);
        }
        if (weaponGO != null)
        {
            weaponGO.SetActive(true);
        }
    }

}
