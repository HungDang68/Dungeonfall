using System.Collections;
using UnityEngine;
using Mirror;
public class Weapon : NetworkBehaviour
{
    [SerializeField] private WeaponSO weaponSO;
    [SerializeField] private Transform shottingStartPoint;
    private bool shootingDelayed;
    public Stats stats;

    private void Start()
    {
        if (!isLocalPlayer) { return; }

        if (shootingDelayed)
        {
            StartCoroutine(DelayShooting());
        }
    }
    public void SetupStats()
    {
        stats = new Stats(weaponSO.GetHealth(), weaponSO.GetArmor(), weaponSO.strength, weaponSO.critRate, weaponSO.critDamage, 0);
    }
    public void PerformAttack(int damage, GameObject player)
    {
        if (shootingDelayed == false)
        {
            shootingDelayed = true;

            weaponSO.Perform(shottingStartPoint, damage, player);

            StartCoroutine(DelayShooting());
        }
    }
    private IEnumerator DelayShooting()
    {
        yield return new WaitForSeconds(weaponSO.attackDelay);
        shootingDelayed = false;
    }

}
