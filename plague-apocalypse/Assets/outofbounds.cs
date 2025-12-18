using UnityEngine;

public class OutOfBounds : MonoBehaviour
{
    // This function runs automatically when something touches the Trigger collider
    void OnTriggerEnter(Collider other)
    {
        // Check if the object touching this is the Player
        if (other.CompareTag("Player"))
        {

            PlayerHealth healthScript = other.GetComponent<PlayerHealth>();


            if (healthScript != null)
            {

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