using UnityEditor;
using Mirror;
using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Health))]

public class Projectile : NetworkBehaviour
{
    [ShowInInspector]
    private float deathDelay = 4;
    private Rigidbody2D rbd2D;
    private Health health;
    private int projectileHealth = 1;
    [HideInInspector]
    public int damage = 0;
    private float speed = 10f;
    private Vector3 shootDirection;
    public GameObject player;

    void Awake()
    {
        rbd2D = GetComponent<Rigidbody2D>();
        health = GetComponent<Health>();
        health.SetHealth(projectileHealth);

        StartCoroutine(DeathAfterDelay(deathDelay));
    }
    void FixedUpdate()
    {
        transform.position += shootDirection * speed * Time.deltaTime;
    }
    public void SetShootDir(Vector3 shootDir)
    {
        this.shootDirection = shootDir;
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        Health hittable = collision.GetComponent<Health>();
        if (hittable != null)
        {
            hittable.GetHit(damage, player);
            health.GetHit(1, gameObject);
            Destroy(gameObject);
        }
    }
    private IEnumerator DeathAfterDelay(float deathDelay)
    {
        yield return new WaitForSeconds(deathDelay);
        health.GetHit(1, gameObject);
        Destroy(gameObject);
    }

}
