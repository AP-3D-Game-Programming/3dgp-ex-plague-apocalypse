using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    public int baseHealth = 100;  // optional reference for max health
    public float fireRate = 1f;
    public bool bouncingBullets = false;

    private PlayerHealth playerHealth;

    void Awake()
    {
        playerHealth = GetComponent<PlayerHealth>();
        if (playerHealth != null)
            playerHealth.maxHealth = baseHealth;
    }

    public void ApplyHealth(int amount)
    {
        if (playerHealth != null)
        {
            playerHealth.maxHealth += amount;
            playerHealth.currentHealth += amount; // also heal the player
            Debug.Log($"Applied {amount} health. Current: {playerHealth.currentHealth}");
        }
    }

    public void MultiplyFireRate(float multiplier)
    {
        fireRate *= multiplier;
    }
}
