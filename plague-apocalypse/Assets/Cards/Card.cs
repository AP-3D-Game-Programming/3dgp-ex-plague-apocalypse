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
    [Header("Weapon Specifics")]
    public bool applyToSpecificType = false;
    public WeaponType targetWeaponType;
    [Header("Weapon Buffs")]

    public float weaponDamageMultiplier = 1f;
    public float weaponFireRateMultiplier = 1f;
    public float weaponReloadTimeMultiplier = 1f;
    public int weaponMagSizeBonus = 0;

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
    [Header("Boss/Unit Spawning")]
    public GameObject specialUnitPrefab;
    public int specialUnitCount = 0;
    public bool spawnImmediately = false;
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
        // --- 1. APPLY PLAYER & WEAPON STATS ---
        // This wrapper ensures "Zombie Only" cards don't touch your guns.
        if (target == CardTarget.Player || target == CardTarget.Both)
        {
            if (playerLuckBonus != 0f) roundManager.playerLuck += playerLuckBonus;

            if (roundManager.playerTransform != null)
            {
                PlayerStats playerStats = roundManager.playerTransform.GetComponent<PlayerStats>();
                PlayerHealth playerHealth = roundManager.playerTransform.GetComponent<PlayerHealth>();

                if (playerStats != null && playerHealth != null)
                {
                    // Basic Health/Regen
                    playerHealth.maxHealth += playerHealthBonus;
                    playerHealth.currentHealth += playerHealthBonus;
                    playerStats.lifeStealPerHit += lifeStealAmount;
                    if (lifeStealAmount > 0 && playerRegenBonus != 0f) playerHealth.regenRate += playerRegenBonus;

                    // Player specific fire rate (separate from weapon fire rate)
                    if (playerFireRateMultiplier > 0 && playerFireRateMultiplier != 1f)
                        playerStats.fireRate *= playerFireRateMultiplier;

                    playerStats.bouncingBullets |= playerBouncingBullets;

                    // Points logic
                    if (shotPointsMultiplier > 0 && shotPointsMultiplier != 1f) playerStats.shotPointsMultiplier *= shotPointsMultiplier;
                    if (deathPointsMultiplier > 0 && deathPointsMultiplier != 1f) playerStats.deathPointsMultiplier *= deathPointsMultiplier;

                    // --- THE SNIPER VS PISTOL FIX ---
                    if (applyToSpecificType)
                    {
                        // PATH A: Only update the dictionary for the specific type (e.g., Sniper).
                        // The Pistol will check this dictionary, find no entry for "Pistol", and ignore it.
                        if (playerStats.typeDamageMults.ContainsKey(targetWeaponType))
                            playerStats.typeDamageMults[targetWeaponType] *= weaponDamageMultiplier;

                        if (playerStats.typeFireRateMults.ContainsKey(targetWeaponType))
                            playerStats.typeFireRateMults[targetWeaponType] *= weaponFireRateMultiplier;
                    }
                    else
                    {
                        // PATH B: Only runs if 'applyToSpecificType' is FALSE.
                        // This updates the GLOBAL multiplier which affects ALL guns.
                        if (weaponDamageMultiplier != 1f)
                            playerStats.damageMultiplier *= weaponDamageMultiplier;

                        if (weaponFireRateMultiplier != 1f)
                            playerStats.fireRateMultiplier *= weaponFireRateMultiplier;
                    }

                    // Global weapon stats (Reload and Mag size usually apply to all weapons)
                    if (weaponReloadTimeMultiplier != 1f) playerStats.reloadSpeedMultiplier *= weaponReloadTimeMultiplier;
                    if (weaponMagSizeBonus != 0) playerStats.magazineSizeBonus += weaponMagSizeBonus;
                }
            }
        }

        // --- 2. APPLY ZOMBIE STATS ---
        if (target == CardTarget.Zombies || target == CardTarget.Both)
        {
            if (zombieHealthPercentIncrease != 0) roundManager.globalEnemyHealthMultiplier += zombieHealthPercentIncrease;
            if (zombieSpeedPercentIncrease != 0) roundManager.globalEnemySpeedMultiplier += zombieSpeedPercentIncrease;
            if (zombieSpeedFlatBonus != 0) roundManager.speedIncrement += zombieSpeedFlatBonus;
            if (zombieFireRateBonus != 0) roundManager.fireRateIncrement += zombieFireRateBonus;
        }

        // --- 3. APPLY ELITE STATS ---
        if (target == CardTarget.Elites || target == CardTarget.Both)
        {
            if (eliteHealthPercentIncrease != 0) roundManager.globalEliteHealthMultiplier += eliteHealthPercentIncrease;
            if (eliteSpeedPercentIncrease != 0) roundManager.globalEliteSpeedMultiplier += eliteSpeedPercentIncrease;
            if (eliteFireRateMultiplier > 0 && eliteFireRateMultiplier != 1f) roundManager.globalEliteFireRateMultiplier *= eliteFireRateMultiplier;
            if (eliteDamageMultiplier > 0 && eliteDamageMultiplier != 1f) roundManager.globalEliteDamageMultiplier *= eliteDamageMultiplier;
            if (elitePhase2HealthTriggerMultiplier > 0 && elitePhase2HealthTriggerMultiplier != 1f) roundManager.globalElitePhase2HealthTriggerMultiplier *= elitePhase2HealthTriggerMultiplier;
            if (elitePhase2SpeedMultiplier > 0 && elitePhase2SpeedMultiplier != 1f) roundManager.globalElitePhase2SpeedMultiplier *= elitePhase2SpeedMultiplier;
        }

        // --- 4. SPAWNING LOGIC ---
        if (forceElitesNextRound > 0) roundManager.ForceSpawnEliteNextRound(forceElitesNextRound);
        if (specialUnitPrefab != null && specialUnitCount > 0)
        {
            if (spawnImmediately) roundManager.SpawnSpecialUnitImmediate(specialUnitPrefab, specialUnitCount);
            else roundManager.QueueSpecialUnit(specialUnitPrefab, specialUnitCount);
        }

        Debug.Log($"Applied Card: {cardName} | Target: {target} | Specific Type: {(applyToSpecificType ? targetWeaponType.ToString() : "None")}");
    }


}

