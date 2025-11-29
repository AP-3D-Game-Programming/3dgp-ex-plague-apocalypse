using UnityEngine;

public enum CardTarget
{
    Player,
    Zombies,
    Elites,
    Both
}
public enum CardRarity
{
    Common,
    Uncommon,
    Rare,
    Epic,
    Legendary,
    Mythical,
    Exotic
}

[CreateAssetMenu(fileName = "NewCard", menuName = "Card")]
public class Card : ScriptableObject
{
    public string cardName;
    [TextArea]
    public string description;

    public CardTarget target;
    [Header("Card Rarity")]
    public CardRarity rarity = CardRarity.Common;

    [Header("Player Stats")]
    public int playerHealthBonus = 0;
    public float playerRegenBonus = 0f;
    public float playerFireRateMultiplier = 1f;
    [Header("Abilities")]
    public float lifeStealAmount = 0f;
    public bool playerBouncingBullets = false;
    [Header("Luck")]
    public float playerLuckBonus = 0f;

    [Header("Zombie Stats")]
    public float zombieHealthPercentIncrease = 0f;
    public float zombieSpeedPercentIncrease = 0f;

    public float zombieSpeedFlatBonus = 0f;
    public float zombieFireRateBonus = 0f;

    [Header("Elite Stats")]
    public int forceElitesNextRound = 0;
    public float eliteHealthPercentIncrease = 0f;
    public float eliteSpeedPercentIncrease = 0f;

    [Header("Elite Ability Buffs")]
    public float eliteFireRateMultiplier = 1f;
    public float elitePhase2HealthTriggerMultiplier = 1f;
    public float elitePhase2SpeedMultiplier = 1f;
    public float eliteDamageMultiplier = 1f;

    [Header("Points Buffs")]
    public float shotPointsMultiplier = 1f;
    public float deathPointsMultiplier = 1f;
    public int maxShootPointsIncrease = 0;

    public Color GetRarityColor()
    {
        switch (rarity)
        {
            case CardRarity.Common: return Color.grey;
            case CardRarity.Uncommon: return Color.green;
            case CardRarity.Rare: return Color.blue;
            case CardRarity.Epic: return new Color(0.64f, 0.21f, 0.93f);
            case CardRarity.Legendary: return Color.Lerp(new Color(1f, 0.5f, 0f), Color.yellow, 0.5f);
            case CardRarity.Mythical: return Color.Lerp(Color.yellow, Color.cyan, 0.5f);
            case CardRarity.Exotic: return Color.magenta;
            default: return Color.white;
        }
    }

    public void Apply(RoundManager roundManager)
    {
        // --- 1. APPLY PLAYER STATS ---
        if (playerLuckBonus != 0f)
        {
            roundManager.playerLuck += playerLuckBonus;
        }


        if (roundManager.playerTransform != null)
        {
            PlayerStats playerStats = roundManager.playerTransform.GetComponent<PlayerStats>();
            PlayerHealth playerHealth = roundManager.playerTransform.GetComponent<PlayerHealth>();

            if (playerStats != null && playerHealth != null)
            {
                playerHealth.maxHealth += playerHealthBonus;
                playerHealth.currentHealth += playerHealthBonus;
                playerStats.lifeStealPerHit += lifeStealAmount;

                if (lifeStealAmount > 0)
                    if (playerRegenBonus != 0f) playerHealth.regenRate += playerRegenBonus;

                // Handle multipliers carefully. If the inspector says 0, we assume no change (1).
                if (playerFireRateMultiplier > 0) playerStats.fireRate *= playerFireRateMultiplier;

                playerStats.bouncingBullets |= playerBouncingBullets;

                if (shotPointsMultiplier > 0) playerStats.shotPointsMultiplier *= shotPointsMultiplier;
                if (deathPointsMultiplier > 0) playerStats.deathPointsMultiplier *= deathPointsMultiplier;

                playerStats.maxShootPointsPerEnemy += maxShootPointsIncrease;
            }
        }

        // --- 2. APPLY ZOMBIE STATS (SAFELY) ---
        if (target == CardTarget.Zombies || target == CardTarget.Both)
        {
            // Instead of looping through types, we modify the RoundManager's global multipliers.

            // Example: If zombieHealthPercentIncrease is 0.1 (10%), we add that to the global multiplier
            if (zombieHealthPercentIncrease != 0)
            {
                // If it was 1.0, it becomes 1.1 (10% harder)
                roundManager.globalEnemyHealthMultiplier += zombieHealthPercentIncrease;
            }

            if (zombieSpeedPercentIncrease != 0)
            {
                roundManager.globalEnemySpeedMultiplier += zombieSpeedPercentIncrease;
            }

            if (zombieSpeedFlatBonus != 0)
            {
                roundManager.speedIncrement += zombieSpeedFlatBonus;
            }

            if (zombieFireRateBonus != 0)
            {
                roundManager.fireRateIncrement += zombieFireRateBonus;
            }
        }
        // --- 3. APPLY ELITE STATS (SAFELY) ---
        if (target == CardTarget.Elites || target == CardTarget.Both)
        {
            if (eliteHealthPercentIncrease != 0)
            {
                roundManager.globalEliteHealthMultiplier += eliteHealthPercentIncrease;
            }

            if (eliteSpeedPercentIncrease != 0)
            {
                roundManager.globalEliteSpeedMultiplier += eliteSpeedPercentIncrease;
            }

            // --- APPLY NEW ABILITY MULTIPLIERS ---
            if (eliteFireRateMultiplier > 0)
            {
                // Note: We use multiplication for rate/damage, not addition
                roundManager.globalEliteFireRateMultiplier *= eliteFireRateMultiplier;
            }
            if (eliteDamageMultiplier > 0)
            {
                roundManager.globalEliteDamageMultiplier *= eliteDamageMultiplier;
            }
            if (elitePhase2HealthTriggerMultiplier > 0)
            {
                // This is a special case: a 1.0 multiplier is 1/3 health. 
                // A card giving a "stronger" Elite might decrease this number (e.g., 0.5 for 50% health trigger).
                // Assuming cards *decrease* this value to make elites harder (phase 2 earlier).
                roundManager.globalElitePhase2HealthTriggerMultiplier *= elitePhase2HealthTriggerMultiplier;
            }
            if (elitePhase2SpeedMultiplier > 0)
            {
                roundManager.globalElitePhase2SpeedMultiplier *= elitePhase2SpeedMultiplier;
            }
        }
        if (forceElitesNextRound > 0)
            roundManager.ForceSpawnEliteNextRound(forceElitesNextRound);

        Debug.Log($"Applied Card: {cardName}");
    }
}