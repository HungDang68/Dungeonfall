using UnityEngine;

[CreateAssetMenu(menuName = "Weapon/Stick")]

public class StickWeaponSO : WeaponSO
{
    public GameObject stickGameObject;


    public override void Perform(Transform shootingStartPoint, int damage, GameObject player)
    {
        GameObject temp_stick = Instantiate(stickGameObject, shootingStartPoint.position, Quaternion.identity);

        Vector3 shootDir = (HelpfulUtility.GetMousePosition() - shootingStartPoint.position).normalized;

        temp_stick.GetComponent<Projectile>().SetShootDir(shootDir);

        temp_stick.GetComponent<Projectile>().damage = damage;

        temp_stick.GetComponent<Projectile>().player = player;
    }
}
