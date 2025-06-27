using UnityEngine;
using static UnityEngine.Time;
using Mirror;
using UnityEngine.InputSystem;
using System.Collections;


public class Player : NetworkBehaviour, IMoveable
{
    private bool isSaving = false;
    private Weapon weapon;
    private PlayerCanvas playerCanvas;
    [SerializeField] private Stats personalStats = new Stats();
    private Stats totalStats;
    private Health health;
    private Rigidbody2D myBody;
    private BoxCollider2D boxCollider2D;
    private Vector2 myPosition;
    private void Start()
    {
        if (!isLocalPlayer) { return; }
        myBody = GetComponent<Rigidbody2D>();
        // LoadPlayer();
        playerCanvas = GetComponent<PlayerCanvas>();
        health = GetComponent<Health>();
        weapon = GetComponentInChildren<Weapon>();
        weapon.SetupStats();
        totalStats = new Stats();
        if (personalStats != null)
        {
            totalStats.Setter(personalStats);
        }
        else
        {
            Debug.LogWarning("personalStats is null. totalStats may not be initialized correctly.");
        }
        health.SetStats(totalStats);
        playerCanvas.SetMaxHealth(totalStats.GetHealth());
        playerCanvas.SetLevel(personalStats.GetLevel());
        playerCanvas.SetEXP(personalStats.GetTotalExp(), personalStats.GetNextLevelExp());
        Debug.Log("Player total damage :" + totalStats.CalculateDamage());
        totalStats.AddStat(totalStats, weapon.stats);
        StartCoroutine(SaveStatsAfterTime());
        health.OnHit.AddListener(OnHit);
        health.OnDeath.AddListener(OnHit);
        Debug.Log("Player total damage :" + totalStats.CalculateDamage());


    }

    // void OnEnable()
    // {
    //     if (!isLocalPlayer) { return; }


    // }
    void OnDisable()
    {
        if (!isLocalPlayer) { return; }
        health.OnHit.RemoveAllListeners();
        health.OnDeath.RemoveAllListeners();
    }
    private void Update()
    {
        if (!isLocalPlayer) { return; }

        Move();
    }

    private void Move()
    {
        if (myBody == null)
        {
            Debug.LogError("Rigidbody2D (myBody) is null! Ensure the Player GameObject has a Rigidbody2D component.");
            return;
        }
        if (totalStats == null)
        {
            Debug.LogError("totalStats is null! Ensure Stats are properly initialized in Start().");
            return;
        }
        if (myPosition == null)
        {
            Debug.LogError("myPosition is null! Ensure OnMove() is being called to update myPosition.");
            return;
        }
        myBody.linearVelocity = myPosition * totalStats.GetSpeed() * deltaTime;

    }

    private void OnMove(InputValue inputValue)
    {
        if (isLocalPlayer)
        {
            myPosition = inputValue.Get<Vector2>();
        }
    }
    private IEnumerator SaveStatsAfterTime()
    {
        while (true && myBody != null)
        {
            SavePlayer();
            yield return new WaitForSeconds(10);
        }
    }
    public void SavePlayer()
    {
        if (!isLocalPlayer || isSaving) { return; }

        isSaving = true;
        SaveSystem.SaveStats(personalStats);
        isSaving = false;
    }
    private void LoadPlayer()
    {
        if (!isLocalPlayer || isSaving) { return; }

        isSaving = true;
        personalStats.Setter(SaveSystem.LoadStats());
        Debug.Log("Player Loaded!");
        isSaving = false;
    }
    private void OnHit()
    {
        playerCanvas.SetHealth(health.GetHealth());
    }
    public void OnAttack()
    {
        weapon.PerformAttack(totalStats.CalculateDamage(), gameObject);
    }
    public void OnGetEXP()
    {
        if (!isLocalPlayer) { return; }

        if (personalStats.AddExperience(20))
        {
            playerCanvas.SetLevel(personalStats.GetLevel());
        }

        playerCanvas.SetEXP(personalStats.GetTotalExp(), personalStats.GetNextLevelExp());
    }

    public Vector3 GetPlayerPosition()
    {
        return transform.position;
    }
}
