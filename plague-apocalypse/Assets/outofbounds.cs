using UnityEngine;

public class OutOfBounds : MonoBehaviour
{
    // This function runs automatically when something touches the Trigger collider
    void OnTriggerEnter(Collider other)
    {
        // Check if the object touching this is the Player
        if (other.CompareTag("Player"))
        {
            // Get the player's health script
            PlayerHealth healthScript = other.GetComponent<PlayerHealth>();

            // If found, kill them instantly
            if (healthScript != null)
            {
                // Deal massive damage (e.g., 999999) to ensure instant death
                // regardless of armor or regen.
                healthScript.TakeDamage(99999999);
                Debug.Log("Player fell out of bounds!");
            }
            else if (other.CompareTag("Zombie"))
            {
                return;
            }
        }
        if (other.GetComponent<FINALLBOSS>() != null)
        {
            return;
        }

    }
}