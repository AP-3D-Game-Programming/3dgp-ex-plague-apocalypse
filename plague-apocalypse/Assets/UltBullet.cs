using UnityEngine;

public class UltBullet : MonoBehaviour
{
    [Header("Bullet Stats")]
    public int damage = 30;
    public float lifeTime = 6f;

    [Header("Explosion Settings")]
    public float explosionRadius = 5f; // How big the explosion is
    public GameObject hitEffect;

    void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    void OnTriggerEnter(Collider other)
    {
        // Don't explode if we just hit the Boss or another Enemy
        if (other.CompareTag("Zombie") || other.CompareTag("Boss") || other.CompareTag("Enemy"))
            return;

        Impact();
    }

    void Impact()
    {
        // 1. Play Visual Effect
        if (hitEffect != null)
            Instantiate(hitEffect, transform.position, Quaternion.identity);

        // 2. Find everyone in the radius
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, explosionRadius);

        foreach (var hit in hitColliders)
        {
            // 3. Check for Player (or other destructibles)
            if (hit.CompareTag("Player"))
            {
                PlayerHealth hp = hit.GetComponent<PlayerHealth>();
                if (hp != null)
                {
                    hp.TakeDamage(damage);
                }
            }
        }

        // 4. Destroy Bullet
        Destroy(gameObject);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}