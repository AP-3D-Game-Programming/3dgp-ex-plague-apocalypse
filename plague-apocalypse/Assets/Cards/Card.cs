using UnityEngine;

public enum CardTarget
{
    Player,
    Zombies,
    Elites,
    Both
}

[CreateAssetMenu(fileName = "NewCard", menuName = "Card")]
public class Card : ScriptableObject
{
    public string cardName;
    [TextArea]
    public string description;

    public CardTarget target;

    [Header("Player Stats")]
    public int playerHealthBonus = 0;
    public float playerFireRateMultiplier = 1f;
    public bool playerBouncingBullets = false;

    [Header("Zombie Stats")]
    public int zombieHealthBonus = 0;
    public float zombieHealthScaleMultiplier = 1f;
    public float zombieSpeedBonus = 0f;
    public float zombieSpeedScaleMultiplier = 1f;
    public float zombieFireRateBonus = 0f;   // for GunRobots

    [Header("Elite Stats")]
    public int eliteHealthBonus = 0;
    public float eliteHealthScaleMultiplier = 1f;
    public float eliteSpeedBonus = 0f;

    [Header("Other Effects")]
    public int forceElitesNextRound = 0;

    public void Apply(RoundManager roundManager)
    {
        // Player effects
        if (roundManager.playerTransform != null)
        {
            PlayerStats playerStats = roundManager.playerTransform.GetComponent<PlayerStats>();
            PlayerHealth playerHealth = roundManager.playerTransform.GetComponent<PlayerHealth>();

            if (playerStats != null && playerHealth != null)
            {
                // Apply max health increase
                playerHealth.maxHealth += playerHealthBonus;

                // Heal player instantly
                playerHealth.currentHealth += playerHealthBonus;

                // Apply other stats
                playerStats.fireRate *= playerFireRateMultiplier;
                playerStats.bouncingBullets |= playerBouncingBullets;

                Debug.Log($"Applied card: +{playerHealthBonus} HP, fire rate x{playerFireRateMultiplier}, bouncing bullets: {playerBouncingBullets}");
            }
        }


        // Zombies effects
        if (target == CardTarget.Zombies || target == CardTarget.Both)
        {
            foreach (var z in roundManager.zombieTypes)
            {
                z.maxHealth = Mathf.RoundToInt(z.maxHealth * zombieHealthScaleMultiplier) + zombieHealthBonus;
                z.baseHealth = Mathf.RoundToInt(z.baseHealth * zombieHealthScaleMultiplier) + zombieHealthBonus;
                z.maxSpeed = Mathf.Min(z.maxSpeed * zombieSpeedScaleMultiplier + zombieSpeedBonus, z.maxSpeed + zombieSpeedBonus);
                z.baseSpeed = Mathf.Min(z.baseSpeed * zombieSpeedScaleMultiplier + zombieSpeedBonus, z.maxSpeed);
                z.maxFireRate += zombieFireRateBonus;
                z.baseFireRate += zombieFireRateBonus;
            }

            foreach (var e in roundManager.eliteTypes)
            {
                e.maxHealth = Mathf.RoundToInt(e.maxHealth * eliteHealthScaleMultiplier) + eliteHealthBonus;
                e.baseHealth = Mathf.RoundToInt(e.baseHealth * eliteHealthScaleMultiplier) + eliteHealthBonus;
                e.maxSpeed = Mathf.Min(e.maxSpeed + eliteSpeedBonus, e.maxSpeed + eliteSpeedBonus);
                e.baseSpeed = Mathf.Min(e.baseSpeed + eliteSpeedBonus, e.maxSpeed);
            }
        }

        // Force elite spawn
        if (forceElitesNextRound > 0)
            roundManager.ForceSpawnEliteNextRound(forceElitesNextRound);
    }
}
