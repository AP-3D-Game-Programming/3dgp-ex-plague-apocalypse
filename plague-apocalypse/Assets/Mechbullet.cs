using UnityEngine;

public class MechBullet : MonoBehaviour
{
    [Header("Settings")]
    public int damage = 10;
    public GameObject hitEffect;
    public float lifeTime = 5f;

    void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    void OnTriggerEnter(Collider other)
    {

        if (other.CompareTag("Zombie"))
            return;
        if (other.CompareTag("Player"))
        {
            PlayerHealth playerStats = other.GetComponent<PlayerHealth>();

            if (playerStats != null)
            {
                playerStats.TakeDamage(damage);
            }

            Impact();
        }
        else
        {
            Impact();
        }
    }

    void Impact()
    {
        if (hitEffect != null)
        {
            Instantiate(hitEffect, transform.position, Quaternion.identity);
        }

        Destroy(gameObject);
    }
}