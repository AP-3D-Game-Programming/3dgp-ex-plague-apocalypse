using UnityEngine;

public class BossRocket : MonoBehaviour
{
    [Header("Damage")]
    public int directDamage = 50;
    public float aoeRadius = 15f;
    public int aoeDamage = 35;
    public GameObject impactEffect;
    public float lifetime = 15f;

    private AudioSource audioSource;
    public AudioClip impactSound;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        Destroy(gameObject, lifetime);
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Zombie"))
            return;
        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerHealth hp = collision.gameObject.GetComponentInParent<PlayerHealth>();
            if (hp != null)
                hp.TakeDamage(directDamage);
        }

        Impact();
    }

    void Impact()
    {
        if (audioSource != null && impactSound != null)
            audioSource.PlayOneShot(impactSound);

        Collider[] hitColliders = Physics.OverlapSphere(transform.position, aoeRadius);

        foreach (var hit in hitColliders)
        {

            if (hit.CompareTag("Player"))
            {
                PlayerHealth hp = hit.GetComponentInParent<PlayerHealth>();
                if (hp != null)
                    hp.TakeDamage(aoeDamage);
            }
        }

        if (impactEffect != null)
            Instantiate(impactEffect, transform.position, Quaternion.identity);

        Destroy(gameObject);
    }
}
