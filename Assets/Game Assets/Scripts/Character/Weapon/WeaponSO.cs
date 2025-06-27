using UnityEngine;

public abstract class WeaponSO : ScriptableObject
{
    public float attackDelay = 0.5f;
    public int speed = 0;

    public int strength = 0;
    public int critRate = 0;
    public int critDamage = 0;

    [SerializeField]
    protected int health = 0;
    [SerializeField]
    protected int armor = 0;

    public int GetHealth()
    {
        return health;
    }
    public int GetArmor()
    {
        return armor;
    }
    public abstract void Perform(Transform shootingStartPoint, int damage, GameObject player);
}
