using UnityEngine;

public class TestDamage : MonoBehaviour
{
    public int damageAmount = 10;

    private void OnTriggerEnter(Collider other)
    {
        // 1. Check for Zombie
        Zombie zombie = other.GetComponent<Zombie>();
        if (zombie != null)
        {
            zombie.TakeDamage(damageAmount);
            Debug.Log($"Dealt {damageAmount} damage to {zombie.name}.");

            TriggerLifeSteal();
            return;
        }

        // 2. Check for EliteToilet
        EliteToilet elite = other.GetComponent<EliteToilet>();
        if (elite != null)
        {
            elite.TakeDamage(damageAmount);
            Debug.Log($"Dealt {damageAmount} damage to {elite.name}.");

            TriggerLifeSteal();
            return;
        }

        // 3. Check for MechEnemy
        MechEnemy mech = other.GetComponentInParent<MechEnemy>();
        if (mech != null)
        {
            mech.TakeDamage(damageAmount);
            Debug.Log($"Dealt {damageAmount} to MechEnemy.");

            TriggerLifeSteal();
            return;
        }
        RageZombie rageZombie = other.GetComponentInParent<RageZombie>();
        if (rageZombie != null)
        {
            rageZombie.TakeDamage(damageAmount);
            Debug.Log($"Dealt {damageAmount} to rageZombie.");

            TriggerLifeSteal();
            return;
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
            // Heal the player
            int oldHealth = health.currentHealth;
            health.currentHealth += (int)stats.lifeStealPerHit;

            if (health.currentHealth > health.maxHealth)
                health.currentHealth = health.maxHealth;

        }
    }
}