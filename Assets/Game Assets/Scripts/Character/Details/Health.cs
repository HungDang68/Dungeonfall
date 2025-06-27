using UnityEngine;
using UnityEngine.Events;
using Mirror;
using Unity.Multiplayer.Playmode;
using Unity.VisualScripting;

public class Health : NetworkBehaviour, IHitable
{
    [SyncVar(hook = nameof(OnHealthChanged))]
    private int currentHealth;
    private Stats stats;

    public UnityEvent OnDeath, OnHit;

    public void SetHealth(int health)
    {
        Debug.Log($"Setting health to {health}");
        currentHealth = health;
    }

    public int GetHealth()
    {
        return currentHealth;
    }

    public void SetStats(Stats stat)
    {
        if (stat == null)
        {
            Debug.LogError("Stats is null! Cannot set stats.");
            return;
        }
        stats = stat;

        if (stats.GetHealth() <= 0)
        {
            OnDeath?.Invoke();
        }
        currentHealth = stats.GetHealth();
        Debug.Log($"Stats set. Current health: {currentHealth}");
    }

    public void SetFullHealth()
    {
        if (stats.GetHealth() <= 0)
        {
            OnDeath?.Invoke();
        }
        currentHealth = stats.GetHealth();
    }


    public void GetHit(int damage, GameObject sender)
    {
        if (stats == null)
        {
            currentHealth -= damage;
        }
        else
        {
            currentHealth -= (int)(damage * (1 - stats.CalculateDamageReduction()));
        }

        Debug.Log("Current Health: " + currentHealth);

        if (currentHealth <= 0)
        {
            OnDeath?.Invoke();
            if (sender.GetComponent<Player>() != null)
            {
                sender.GetComponent<Player>().OnGetEXP();
            }
        }
        else
        {
            OnHit?.Invoke();
        }
    }


    private void OnHealthChanged(int oldHealth, int newHealth)
    {
        Debug.Log($"Health changed from {oldHealth} to {newHealth}");
        if (newHealth <= 0)
        {
            OnDeath?.Invoke();
        }
        else if (newHealth < oldHealth)
        {
            OnHit?.Invoke();
        }
    }
}
