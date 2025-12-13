using UnityEngine;

public class UltBullet : MonoBehaviour
{
    public int damage = 30;
    public GameObject hitEffect;
    public float lifeTime = 6f;

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
            PlayerHealth hp = other.GetComponent<PlayerHealth>();
            if (hp != null)
            {
                hp.TakeDamage(damage);
            }
        }

        Impact();
    }

    void Impact()
    {
        if (hitEffect != null)
            Instantiate(hitEffect, transform.position, Quaternion.identity);

        Destroy(gameObject);
    }
}
