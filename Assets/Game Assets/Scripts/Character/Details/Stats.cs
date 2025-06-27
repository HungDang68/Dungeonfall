using UnityEngine;
using Mirror;
using UnityEngine.Events;
using Mirror.BouncyCastle.Asn1.Misc;
using Unity.Cinemachine;

[System.Serializable]
public class Stats
{
    private int level = 0;
    private int totalEXP = 0;
    private int nextLevelsEXP = 100;



    private int maxHealth = 100;
    private int armor = 0;
    private int strength = 100;
    private int critRate = 10;
    private int critDamage = 100;
    private int moveSpeed = 2000;



    public Stats() { }
    public Stats(int maxHealth, int armor, int strength, int critRate, int critDamage, int moveSpeed)
    {
        this.maxHealth = maxHealth;
        this.armor = armor;
        this.strength = strength;
        this.critRate = critRate;
        this.critDamage = critDamage;
        this.moveSpeed = moveSpeed;
    }
    public Stats(Stats stats)
    {
        this.maxHealth = stats.maxHealth;
        this.armor = stats.armor;
        this.strength = stats.strength;
        this.critRate = stats.critRate;
        this.critDamage = stats.critDamage;
        this.moveSpeed = stats.moveSpeed;
    }

    public int Getstrength()
    {
        return this.strength;
    }
    public int GetCritDamage()
    {
        return this.critDamage;
    }
    public int GetHealth()
    {
        return this.maxHealth;
    }
    public int GetSpeed()
    {
        return this.moveSpeed;
    }
    public int GetLevel()
    {
        return this.level;
    }
    public int GetTotalExp()
    {
        return this.totalEXP;
    }
    public int GetNextLevelExp()
    {
        return this.nextLevelsEXP;
    }
    public void SetSpeed(int speed)
    {
        this.moveSpeed = speed;
    }
    public void EnemySetter(int maxHealth, int armor, int strength, int speed)
    {
        this.maxHealth = maxHealth;
        this.armor = armor;
        this.strength = strength;
        this.moveSpeed = speed;
    }
    public void Setter(Stats stats)
    {
        if (stats != null)
        {
            this.level = stats.level;
            this.totalEXP = stats.totalEXP;


            this.maxHealth = stats.maxHealth;
            this.armor = stats.armor;
            this.strength = stats.strength;
            this.critRate = stats.critRate;
            this.critDamage = stats.critDamage;
            this.moveSpeed = stats.moveSpeed;
        }
    }
    public void AddStat(Stats stat1, Stats stat2)
    {
        this.maxHealth = stat1.maxHealth + stat2.maxHealth;
        this.armor = stat1.armor + stat2.armor;
        this.strength = stat1.strength + stat2.strength;
        this.critRate = stat1.critRate + stat2.critRate;
        this.critDamage = stat1.critDamage + stat2.critDamage;
        this.moveSpeed = stat1.moveSpeed + stat2.moveSpeed;
    }
    public int CalculateDamage()
    {
        int damage = strength;

        if (Random.Range(0, 100) < critRate)
        {
            damage = Mathf.RoundToInt(damage * ((critDamage + 100) / 100f));
            Debug.Log("Critical hit!");
        }


        return damage;
    }
    public float CalculateDamageReduction()
    {
        float reduction = armor / (armor + 100f);

        return reduction;
    }
    public bool AddExperience(int amount)
    {
        this.totalEXP += amount;
        Debug.Log("totalEXP: " + totalEXP);
        return CheckForLevelUp();
    }

    private bool CheckForLevelUp()
    {
        if (totalEXP >= nextLevelsEXP)
        {
            int levelsToLevel = (int)(totalEXP / nextLevelsEXP);
            this.level += levelsToLevel;
            this.maxHealth += levelsToLevel;
            this.totalEXP -= nextLevelsEXP * levelsToLevel;
            Debug.Log("LV: " + level);
            Debug.Log("MaxHealth: " + maxHealth);
            return true;
        }
        else
        {
            return false;
        }
    }
}

