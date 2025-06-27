using UnityEngine;
using Mirror;
using UnityEngine.Rendering;
using System;

[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(PolygonCollider2D))]
[RequireComponent(typeof(Health))]

public class Enemy : NetworkBehaviour, IMoveable
{
    public Stats stats = new Stats();
    public EnemyDetailsSO enemyDetails;
    private EnemyMovementAI enemyMovementAI;
    [HideInInspector] public MovementToPositionEvent movementToPositionEvent;
    private CircleCollider2D circleCollider2D;
    private PolygonCollider2D polygonCollider2D;
    [HideInInspector] public SpriteRenderer[] spriteRenderers;
    private Health health;
    private void Awake()
    {
        enemyMovementAI = GetComponent<EnemyMovementAI>();
        movementToPositionEvent = GetComponent<MovementToPositionEvent>();
        health = GetComponent<Health>();
        if (enemyDetails != null)
        {
            stats.EnemySetter(enemyDetails.maxHealth, enemyDetails.armor, enemyDetails.strength, enemyDetails.speed);
            health.SetStats(stats);
        }
        else
        {
            Debug.LogError("EnemyDetailsSO is null! Stats cannot be initialized.");
        }
        Debug.Log("Damage :" + stats.CalculateDamage());
        circleCollider2D = GetComponent<CircleCollider2D>();
        polygonCollider2D = GetComponent<PolygonCollider2D>();
        spriteRenderers = GetComponentsInChildren<SpriteRenderer>();

    }

    private void OnEnable()
    {
        health.OnDeath.AddListener(OnDeathListener);
    }

    private void OnDisable()
    {
        if (health != null)
        {
            health.OnDeath.RemoveAllListeners();
        }
    }

    private void OnDeathListener()
    {
        Destroy(gameObject);
    }
    public void EnemyInit(EnemyDetailsSO enemyDetailsSO, int enemiesSpawnNumber, DungeonLevel dungeonLevel)
    {
        this.enemyDetails = enemyDetailsSO;
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {

        if (collision.tag == Settings.playerTag)
        {
            Health hittable = collision.GetComponent<Health>();
            hittable.GetHit(stats.CalculateDamage(), gameObject);
            Debug.Log("damage: " + stats.CalculateDamage());
        }
    }
}

