using UnityEngine;

public class TestDamage : MonoBehaviour
{
    public int damageAmount = 10; // How much damage to apply per touch

    private void OnTriggerEnter(Collider other)
    {
        // Check if the object has a Zombie script
        Zombie zombie = other.GetComponent<Zombie>();
        if (zombie != null)
        {
            zombie.TakeDamage(damageAmount);
            Debug.Log($"Dealt {damageAmount} damage to {zombie.name}. Current HP: {zombie.health}");
            return;
        }

        // Check if the object has an EliteToilet script
        EliteToilet elite = other.GetComponent<EliteToilet>();
        if (elite != null)
        {
            elite.TakeDamage(damageAmount);
            Debug.Log($"Dealt {damageAmount} damage to {elite.name}. Current HP: {elite.health}");
            return;
        }

        // Optional: Add other enemy types here
    }
}
