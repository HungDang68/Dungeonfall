using Mirror;
using UnityEngine;

[CreateAssetMenu(menuName = "Weapon/Stick")]

public class StickWeaponSO : WeaponSO
{
    public GameObject stickGameObject;

    [Command]
    public override void Perform(Transform shootingStartPoint, int damage, GameObject player)
    {
        PerformRpc(shootingStartPoint, damage, player);
    }

    [ClientRpc]
    private void PerformRpc(Transform shootingStartPoint, int damage, GameObject player)
    {
        GameObject temp_stick = Instantiate(stickGameObject, shootingStartPoint.position, Quaternion.identity);

        NetworkServer.Spawn(temp_stick);

        Vector3 shootDir = (HelpfulUtility.GetMousePosition() - shootingStartPoint.position).normalized;

        temp_stick.GetComponent<Projectile>().SetShootDir(shootDir);

        temp_stick.GetComponent<Projectile>().damage = damage;

        temp_stick.GetComponent<Projectile>().player = player;
    }

}
