using Mirror.BouncyCastle.Bcpg;
using UnityEngine;

[CreateAssetMenu(menuName = "Enemy/Details")]
public class EnemyDetailsSO : ScriptableObject
{
    public string enemyName;
    public GameObject enemyPrefab;
    public float chaseDistance = 50f;
    public float attackDelay = 0.5f;
    public int speed = 10;
    public int strength = 20;
    public int maxHealth = 100;
    public int armor = 0;

    public int exp = 20;

}
