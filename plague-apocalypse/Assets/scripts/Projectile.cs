using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(Rigidbody))]
public class Projectile : MonoBehaviour
{
    [Header("Base Stats")]
    public float speed = 50f;
    public float lifetime = 5f;

    [HideInInspector] public float damage;
    [HideInInspector] public WeaponType sourceWeaponType;
    private List<BulletLogic> activeLogics = new List<BulletLogic>();

    private Rigidbody rb;

    public void Initialize(float weaponDamage, WeaponType type, List<BulletEffect> effects)
    {
        this.damage = weaponDamage;
        this.sourceWeaponType = type;

        if (effects != null)
        {
            foreach (var effect in effects)
            {
                effect.Apply(this.gameObject);
            }
        }
        activeLogics.AddRange(GetComponents<BulletLogic>());
    }

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.linearVelocity = transform.forward * speed;

        Destroy(gameObject, lifetime);
    }

    void OnCollisionEnter(Collision collision)
    {

        bool enemyHit = false; // Track if we hit something so we don't hit 2 things at once

        // Check for Zombie
        if (!enemyHit)
        {
            Zombie zombie = collision.gameObject.GetComponent<Zombie>();
            if (zombie != null)
            {
                zombie.TakeDamage((int)damage); // Cast float damage to int
                TriggerLifeSteal();
                enemyHit = true;
            }
        }

        // Check for EliteToilet
        if (!enemyHit)
        {
            EliteToilet elite = collision.gameObject.GetComponent<EliteToilet>();
            if (elite != null)
            {
                elite.TakeDamage((int)damage);
                TriggerLifeSteal();
                enemyHit = true;
            }
        }

        // Check for MechEnemy 
        if (!enemyHit)
        {
            MechEnemy mech = collision.gameObject.GetComponentInParent<MechEnemy>();
            if (mech != null)
            {
                mech.TakeDamage((int)damage);
                TriggerLifeSteal();
                enemyHit = true;
            }
        }

        // Check for RageZombie
        if (!enemyHit)
        {
            RageZombie rageZombie = collision.gameObject.GetComponentInParent<RageZombie>();
            if (rageZombie != null)
            {
                rageZombie.TakeDamage((int)damage);
                TriggerLifeSteal();
                enemyHit = true;
            }
        }

        // --- 2. BULLET EFFECTS LOGIC ---

        BulletAction finalAction = BulletAction.Destroy;

        foreach (var logic in activeLogics)
        {
            // Pass the collision data to any effects (explosions, poison, etc.)
            finalAction = logic.OnHit(collision, finalAction);
        }

        ExecuteAction(finalAction, collision);
    }

    void ExecuteAction(BulletAction action, Collision collision)
    {
        switch (action)
        {
            case BulletAction.Destroy:
                Destroy(gameObject);
                break;

            case BulletAction.PassThrough:
                Physics.IgnoreCollision(GetComponent<Collider>(), collision.collider);
                break;

            case BulletAction.Bounce:
                // Bouncing logic here if needed
                break;
        }
    }
    void TriggerLifeSteal()
    {

        GameObject player = GameObject.FindWithTag("Player");
        if (player == null) return;

        PlayerStats stats = player.GetComponent<PlayerStats>();
        PlayerHealth health = player.GetComponent<PlayerHealth>();

        if (stats != null && health != null && stats.lifeStealPerHit > 0)
        {
            // Heal the player
            int oldHealth = health.currentHealth;
            health.currentHealth += (int)stats.lifeStealPerHit;

            if (health.currentHealth > health.maxHealth)
                health.currentHealth = health.maxHealth;

        }
    }
}