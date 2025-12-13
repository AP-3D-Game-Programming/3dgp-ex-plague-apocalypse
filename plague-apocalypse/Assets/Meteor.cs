using UnityEngine;

public class Meteor : MonoBehaviour
{
    [Header("Damage")]
    public int directDamage = 50;
    public float aoeRadius = 3f;
    public int aoeDamage = 35;
    public GameObject impactEffect;
    public float lifetime = 15f;
    private AudioSource audioSource;
    public AudioClip impactSound;
    void Start()
    {
        Destroy(gameObject, lifetime);
    }

    void OnCollisionEnter(Collision collision)
    {
        // Check if we hit the player
        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerHealth playerStats = collision.gameObject.GetComponent<PlayerHealth>();

            if (playerStats != null)
            {

                playerStats.TakeDamage(directDamage);
            }
        }

        // Impact on any surface (ground, wall, player)
        Impact();
    }
    void Impact()
    {
        if (audioSource != null && impactSound != null)
        {

            audioSource.PlayOneShot(impactSound);
        }


        Collider[] hitColliders = Physics.OverlapSphere(transform.position, aoeRadius);

        foreach (var hitCollider in hitColliders)
        {

            if (hitCollider.gameObject == gameObject) continue;

            if (hitCollider.CompareTag("Player") && aoeDamage > 0)
            {

                PlayerHealth playerStats = hitCollider.GetComponent<PlayerHealth>();
                if (playerStats != null)
                {

                    playerStats.TakeDamage(aoeDamage);
                }
            }


        }

        if (impactEffect != null)
        {
            Instantiate(impactEffect, transform.position, Quaternion.identity);
        }


        Destroy(gameObject);
    }
}