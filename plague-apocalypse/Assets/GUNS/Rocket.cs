using UnityEngine;
using System.Collections.Generic;

public class Rocket : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float _speed = 30.0f;
    [SerializeField] private float _lifeTimeSeconds = 10f;

    [Header("Explosion Settings")]
    [SerializeField] private GameObject _explosionPrefab;
    [SerializeField] private float _explosionForce = 20f;
    [SerializeField] private float _radius = 5f;
    [SerializeField] private float _upwardModifier = 3.0F;

    // Integrated from Projectile Logic
    [HideInInspector] public float damage;
    [HideInInspector] public WeaponType sourceWeaponType;

    private Rigidbody _rb;
    private bool _initialized;
    private MeshRenderer _meshRenderer;
    private BoxCollider _boxCollider;

    void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _boxCollider = GetComponent<BoxCollider>();
        _meshRenderer = GetComponent<MeshRenderer>();
    }

    // This matches the Projectile.cs Initialize call from your gun
    public void Initialize(float weaponDamage, WeaponType type, List<BulletEffect> effects)
    {
        this.damage = weaponDamage;
        this.sourceWeaponType = type;

        // Handle effects if you have them (like fire/poison)
        if (effects != null)
        {
            foreach (var effect in effects) effect.Apply(this.gameObject);
        }

        _initialized = true;
        if (_rb != null)
        {
            _rb.useGravity = false;
            _rb.linearVelocity = transform.forward * _speed;
        }

        Destroy(gameObject, _lifeTimeSeconds);
    }

    private void OnCollisionEnter(Collision collision)
    {
        Explode();
    }

    void Explode()
    {
        Vector3 explosionPos = transform.position;

        // 1. Visual Effect
        if (_explosionPrefab != null)
        {
            GameObject explosionInstance = Instantiate(_explosionPrefab, explosionPos, Quaternion.identity);
            Destroy(explosionInstance, 3f);
        }

        // 2. Physics & DAMAGE logic
        Collider[] colliders = Physics.OverlapSphere(explosionPos, _radius);

        foreach (Collider hit in colliders)
        {
            // Apply Physics Push
            Rigidbody rb = hit.GetComponent<Rigidbody>();
            if (rb != null)
                rb.AddExplosionForce(_explosionForce, explosionPos, _radius, _upwardModifier, ForceMode.Impulse);

            // APPLY DAMAGE TO ENEMIES IN RADIUS
            ApplyDamageToTarget(hit.gameObject);
        }

        // Cleanup rocket
        _speed = 0;
        if (_rb != null) _rb.linearVelocity = Vector3.zero;
        if (_meshRenderer != null) _meshRenderer.enabled = false;
        if (_boxCollider != null) _boxCollider.enabled = false;
        Destroy(gameObject, 0.2f);
    }

    // This is the "Damageshit" logic you needed from Projectile.cs
    void ApplyDamageToTarget(GameObject target)
    {
        // Try to find any enemy component (checking parents too)
        Zombie zombie = target.GetComponentInParent<Zombie>();
        GunRobot robot = target.GetComponentInParent<GunRobot>();
        EliteToilet elite = target.GetComponentInParent<EliteToilet>();
        MechEnemy mech = target.GetComponentInParent<MechEnemy>();
        RageZombie rage = target.GetComponentInParent<RageZombie>();
        FINALLBOSS boss = target.GetComponentInParent<FINALLBOSS>();

        if (zombie != null) zombie.TakeDamage((int)damage);
        else if (robot != null) robot.TakeDamage((int)damage);
        else if (elite != null) elite.TakeDamage((int)damage);
        else if (mech != null) mech.TakeDamage((int)damage);
        else if (rage != null) rage.TakeDamage((int)damage);
        else if (boss != null) boss.TakeDamage((int)damage);

        // If we hit any of them, trigger the Lifesteal
        if (zombie || robot || elite || mech || rage || boss)
        {
            TriggerLifeSteal();
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
            health.currentHealth = Mathf.Min(health.maxHealth, health.currentHealth + (int)stats.lifeStealPerHit);
        }
    }
}