using TMPro;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class ZombieType
{
    public string typeName;
    public GameObject prefab;

    [Header("Stats")]
    public int baseHealth = 100;
    public int maxHealth = 500;
    public float baseSpeed = 2f;
    public float maxSpeed = 6f;

    [Header("Spawn Settings")]
    public int unlockRound = 1;             // Earliest round it can appear
    [Range(0f, 1f)]
    public float initialSpawnChance = 0.1f; // Spawn chance at unlockRound
    public float spawnIncreasePerRound = 0.05f; // Spawn chance increases per round
    [Range(0f, 1f)]
    public float maxSpawnChance = 1f;
    public bool isFlying = false;
    public float flyingHeight = 5f; // only used if isFlying = true

    [Header("Robot Combat Stats")]
    public float baseFireRate = 1f;
    public float maxFireRate = 5f;
    [HideInInspector]
    public float currentFireRate;

    [HideInInspector]
    public int currentHealth;
    [HideInInspector]
    public float currentSpeed;


    public float GetSpawnChance(int currentRound)
    {
        if (currentRound < unlockRound) return 0f;

        float chance = initialSpawnChance + (currentRound - unlockRound) * spawnIncreasePerRound;
        return Mathf.Clamp(chance, 0f, maxSpawnChance); //  capped at maxSpawnChance
    }

    public void ScaleStats(int round, float healthMultiplier, float speedMultiplier, float fireRateMultiplier)
    {
        if (round < unlockRound)
        {
            currentHealth = baseHealth;
            currentSpeed = baseSpeed;
            currentFireRate = baseFireRate;
            return;
        }

        int roundsPassed = round - unlockRound;

        currentHealth = Mathf.Min(
            baseHealth + Mathf.RoundToInt(roundsPassed * healthMultiplier),
            maxHealth
        );

        currentSpeed = Mathf.Min(
            baseSpeed + roundsPassed * speedMultiplier,
            maxSpeed
        );

        currentFireRate = Mathf.Min(
            baseFireRate + roundsPassed * fireRateMultiplier,
            maxFireRate
        );
    }

}

[System.Serializable]
public class EliteType
{
    public string typeName;
    public GameObject prefab;

    [Header("Unlock & Spawn")]
    public int unlockRound = 12;
    [Range(0f, 1f)]
    public float initialSpawnChance = 0.2f; // spawn chance at unlockRound
    public float spawnIncreasePerRound = 0.05f; // chance increases per round
    [Range(0f, 1f)]
    public float maxSpawnChance = 1f;
    public int[] guaranteedRounds;
    public int maxPerRound = 3; // Default limit

    [Header("Stats")]
    public int baseHealth = 500;
    public int maxHealth = 2000;
    public float baseSpeed = 2f;
    public float maxSpeed = 8f;

    [HideInInspector] public int currentHealth;
    [HideInInspector] public float currentSpeed;

    // Same as zombies
    public float GetSpawnChance(int currentRound)
    {
        if (currentRound < unlockRound) return 0f;
        float chance = initialSpawnChance + (currentRound - unlockRound) * spawnIncreasePerRound;
        return Mathf.Clamp(chance, 0f, maxSpawnChance);
    }

    // Scale stats like zombies
    public void ScaleStats(int round, float healthMultiplier, float speedMultiplier)
    {
        if (round < unlockRound)
        {
            currentHealth = baseHealth;
            currentSpeed = baseSpeed;
            return;
        }

        int roundsPassed = round - unlockRound;

        currentHealth = Mathf.Min(
            baseHealth + Mathf.RoundToInt(roundsPassed * healthMultiplier),
            maxHealth
        );

        currentSpeed = Mathf.Min(
            baseSpeed + roundsPassed * speedMultiplier,
            maxSpeed
        );
    }
}

public struct CurrentEnemyStats
{
    public float totalHealthMultiplier;
    public float totalSpeedMultiplier;
    public float totalFireRateMultiplier;
    public float totalDamageMultiplier;
    public float currentHealthIncrement;
}

public class RoundManager : MonoBehaviour
{
    [Header("Game Settings")]
    public int currentRound = 1;
    public int baseEnemies = 5;
    public float playerLuck = 1.0f;
    [Header("Spawning Settings")]
    public List<ZombieType> zombieTypes;
    [Header("Elite zombies")]
    public List<EliteType> eliteTypes;

    public Transform[] spawnPoints;
    public float spawnRange = 20f;
    public int maxZombiesOnScreen = 10;
    public float spawnInterval = 1f;

    [Header("UI")]
    public TextMeshProUGUI roundText;
    public TextMeshProUGUI zombiesRemainingText;
    public Gameoverscript gameOverUI;
    public int totalZombiesKilled = 0;
    public int totalPointsEarned = 0;
    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip roundCompleteSound;
    public AudioClip roundStartSound;
    [Header("Enemy Scaling Settings")]
    public float healthIncrement = 20f;
    public float speedIncrement = 0.2f;
    public float fireRateIncrement = 0.15f;
    public float globalEnemyHealthMultiplier = 1f;
    public float globalEnemySpeedMultiplier = 1f;

    public float globalEliteHealthMultiplier = 1f;
    public float globalEliteSpeedMultiplier = 1f;
    public float eliteHealthIncrement = 50f;
    public float eliteSpeedIncrement = 0.3f;
    public float globalEliteFireRateMultiplier = 1f;
    public float globalEliteDamageMultiplier = 1f;
    public float globalElitePhase2HealthTriggerMultiplier = 1f;
    public float globalElitePhase2SpeedMultiplier = 1f;
    private int enemiesRemaining;
    private int zombiesAlive = 0;
    public Transform playerTransform;
    private Coroutine spawnRoutine;

    private Dictionary<EliteType, int> eliteRoundQuota = new Dictionary<EliteType, int>();

    private const string DifficultyKey = "Difficulty";
    public List<GameObject> queuedSpecialUnits = new List<GameObject>();

    void Start()
    {

        ApplyDifficulty();
        GameObject playerObj = GameObject.FindWithTag("Player");
        if (playerObj != null)
            playerTransform = playerObj.transform;
        else
            Debug.LogError("RoundManager: Could not find object with tag 'Player'!");

        UpdateRoundUI();
        UpdateZombiesUI();

        spawnRoutine = StartCoroutine(StartRound());
    }
    void ApplyDifficulty()
    {
        int difficultyIndex = PlayerPrefs.GetInt(DifficultyKey, 1);

        float statMultiplier = 1f;

        switch (difficultyIndex)
        {
            case 0: // Easy
                statMultiplier = 0.7f;
                break;
            case 1: // Normal
                statMultiplier = 1.0f;
                break;
            case 2: // Hard
                statMultiplier = 1.5f;
                break;
            case 3: // Nightmare
                statMultiplier = 3.0f;
                break;
        }

        Debug.Log($"Applying Difficulty: Index {difficultyIndex}, Multiplier {statMultiplier}x");

        globalEnemyHealthMultiplier *= statMultiplier;
        globalEliteHealthMultiplier *= statMultiplier;

        healthIncrement *= statMultiplier;
        eliteHealthIncrement *= statMultiplier;
        globalEliteDamageMultiplier *= statMultiplier;


        // Easy = 0.9x, Normal = 1.0x, Hard = 1.1x, Nightmare = 1.3x
        float speedMod = 1.0f;
        if (difficultyIndex == 0) speedMod = 0.9f;
        if (difficultyIndex == 2) speedMod = 1.1f;
        if (difficultyIndex == 3) speedMod = 1.3f;

        globalEnemySpeedMultiplier *= speedMod;
        globalEliteSpeedMultiplier *= speedMod;
    }
    IEnumerator StartRound()
    {
        CalculateEliteQuota();
        enemiesRemaining = baseEnemies + (currentRound - 1) * 2;
        UpdateZombiesUI();
        if (audioSource != null && roundStartSound != null)
        {
            audioSource.PlayOneShot(roundStartSound);
        }
        SpawnQueuedUnits();
        while (enemiesRemaining > 0 || zombiesAlive > 0)
        {
            if (enemiesRemaining > 0 && zombiesAlive < maxZombiesOnScreen)
            {
                SpawnEnemy();
                enemiesRemaining--;
            }

            SpawnForcedElites();
            foreach (var elite in eliteTypes)
            {
                if (AttemptSpawnEliteFromQuota(elite))
                {
                    SpawnElite(elite);
                }
            }

            yield return new WaitForSeconds(spawnInterval);
        }
    }



    void SpawnEnemy()
    {
        if (playerTransform == null || zombieTypes.Count == 0 || spawnPoints.Length == 0)
            return;

        // Weighted random selection
        float totalWeight = 0f;
        foreach (var type in zombieTypes)
            totalWeight += type.GetSpawnChance(currentRound);

        if (totalWeight == 0f)
            return; // no valid zombies this round

        float rand = Random.value * totalWeight;
        float sum = 0f;
        ZombieType chosenType = null;
        foreach (var type in zombieTypes)
        {
            sum += type.GetSpawnChance(currentRound);
            if (rand <= sum)
            {
                chosenType = type;
                break;
            }
        }

        if (chosenType == null)
            return;

        // Scale stats
        float finalHealthMult = healthIncrement * globalEnemyHealthMultiplier;
        float finalSpeedMult = speedIncrement * globalEnemySpeedMultiplier;

        chosenType.ScaleStats(
            currentRound,
            healthMultiplier: finalHealthMult,
            speedMultiplier: finalSpeedMult,
            fireRateMultiplier: fireRateIncrement
        );

        // Pick spawn point
        List<Transform> validSpawns = new List<Transform>();
        foreach (Transform sp in spawnPoints)
        {
            float distance = Vector3.Distance(sp.position, playerTransform.position);
            if (distance <= spawnRange)
                validSpawns.Add(sp);
        }

        Transform spawnPoint = validSpawns.Count > 0 ? validSpawns[Random.Range(0, validSpawns.Count)] :
                                                      spawnPoints[Random.Range(0, spawnPoints.Length)];

        // Determine spawn position
        Vector3 spawnPos = spawnPoint.position;

        if (chosenType.isFlying)
            spawnPos += Vector3.up * chosenType.flyingHeight; // spawn in air
                                                              // Instantiate enemy
        GameObject enemy = Instantiate(chosenType.prefab, spawnPos, spawnPoint.rotation);
        zombiesAlive++;
        UpdateZombiesUI();

        // Assign stats to the correct script
        if (chosenType.isFlying)
        {
            GunRobot robotScript = enemy.GetComponent<GunRobot>();
            if (robotScript != null)
            {
                robotScript.ApplyScaledStats(
                    chosenType.currentHealth,
                    chosenType.currentSpeed,
                    chosenType.currentFireRate
                );

                robotScript.roundManager = this;
            }
        }

        else
        {
            Zombie zombieScript = enemy.GetComponent<Zombie>();
            if (zombieScript != null)
            {
                zombieScript.health = chosenType.currentHealth;
                zombieScript.moveSpeed = chosenType.currentSpeed;

                UnityEngine.AI.NavMeshAgent agent = enemy.GetComponent<UnityEngine.AI.NavMeshAgent>();
                if (agent != null)
                    agent.speed = chosenType.currentSpeed;

                zombieScript.roundManager = this;
            }
        }

    }

    void SpawnElite(EliteType elite)
    {
        float finalHealthMult = eliteHealthIncrement * globalEliteHealthMultiplier;
        float finalSpeedMult = eliteSpeedIncrement * globalEliteSpeedMultiplier;
        elite.ScaleStats(currentRound, healthMultiplier: finalHealthMult, speedMultiplier: finalSpeedMult);

        Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
        GameObject enemy = Instantiate(elite.prefab, spawnPoint.position, spawnPoint.rotation);

        IElite eliteScript = enemy.GetComponent<IElite>();
        if (eliteScript != null)
        {
            // --- UPDATED CALL TO PASS ALL MULTIPLIERS ---
            eliteScript.ApplyStats(
                elite.currentHealth,
                elite.currentSpeed,
                this,
                fireRateMult: globalEliteFireRateMultiplier,
                damageMult: globalEliteDamageMultiplier,
                phase2HealthMult: globalElitePhase2HealthTriggerMultiplier,
                phase2SpeedMult: globalElitePhase2SpeedMultiplier
            );
        }

        zombiesAlive++;
        UpdateZombiesUI();
    }

    // Instead of a single bool, track how many elites to force spawn next round
    private int forcedElitesNextRound = 0;

    public void ForceSpawnEliteNextRound(int count = 1)
    {
        forcedElitesNextRound += count;
    }
    private void SpawnForcedElites()
    {
        while (forcedElitesNextRound > 0)
        {
            // Pick a random elite type
            EliteType elite = eliteTypes[Random.Range(0, eliteTypes.Count)];
            SpawnElite(elite);
            forcedElitesNextRound--;
        }
    }



    public void EnemyKilled()
    {
        zombiesAlive--;
        totalZombiesKilled++;
        UpdateZombiesUI();

        if (enemiesRemaining <= 0 && zombiesAlive <= 0)
        {
            if (spawnRoutine != null)
                StopCoroutine(spawnRoutine);

            currentRound++;
            UpdateRoundUI();
            if (audioSource != null && roundCompleteSound != null)
            {
                audioSource.PlayOneShot(roundCompleteSound);
            }
            StartCoroutine(RoundFlash());
            StartCoroutine(ShowCardOptions());
        }
    }

    IEnumerator StartNextRoundWithDelay()
    {
        yield return new WaitForSeconds(5f); // rest time for player
        spawnRoutine = StartCoroutine(StartRound());
    }

    void UpdateRoundUI()
    {
        if (roundText != null)
            roundText.text = $"{currentRound}";
    }

    void UpdateZombiesUI()
    {
        if (zombiesRemainingText != null)
            zombiesRemainingText.text = $"Zombies alive: {zombiesAlive}";
    }

    IEnumerator RoundFlash()
    {
        if (roundText == null) yield break;

        Vector3 originalScale = roundText.transform.localScale;
        for (int i = 0; i < 5; i++)
        {
            roundText.transform.localScale = originalScale * 1.5f;
            roundText.color = Color.yellow;
            yield return new WaitForSeconds(0.25f);
            roundText.transform.localScale = originalScale;
            roundText.color = Color.white;
            yield return new WaitForSeconds(0.25f);
        }
    }

    private void OnDrawGizmosSelected()
    {
        GameObject player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            Gizmos.color = new Color(1, 0, 0, 0.5f);
            Gizmos.DrawWireSphere(player.transform.position, spawnRange);
        }
    }
    [SerializeField] private List<Card> allCards;

    IEnumerator ShowCardOptions()
    {
        yield return new WaitForSeconds(3f);
        GameObject[] hudObjects = GameObject.FindGameObjectsWithTag("HUD");
        foreach (var obj in hudObjects) obj.SetActive(false);

        // 1. Generate the initial cards
        List<Card> options = GenerateRandomCards();

        // 2. Show the UI
        CardSelectionUI.Instance.ShowOptions(options);

        // 3. Wait until a choice is made
        yield return new WaitUntil(() => CardSelectionUI.Instance.cardChosen);

        // 4. Apply logic
        Card chosenCard = CardSelectionUI.Instance.GetChosenCard();
        chosenCard.Apply(this);

        foreach (var obj in hudObjects) obj.SetActive(true);
        spawnRoutine = StartCoroutine(StartNextRoundWithDelay());
    }


    public List<Card> GenerateRandomCards()
    {
        List<Card> availableCards = new List<Card>(allCards);
        List<Card> selectedCards = new List<Card>();

        int cardsToPick = Mathf.Min(3, availableCards.Count);

        for (int i = 0; i < cardsToPick; i++)
        {
            float totalWeight = 0f;
            foreach (var card in availableCards)
                totalWeight += GetRarityWeight(card.rarity, playerLuck);

            float rand = Random.value * totalWeight;
            float sum = 0f;
            Card cardToAdd = null;

            foreach (var card in availableCards)
            {
                sum += GetRarityWeight(card.rarity, playerLuck);
                if (sum >= rand)
                {
                    cardToAdd = card;
                    break;
                }
            }

            if (cardToAdd == null && availableCards.Count > 0)
                cardToAdd = availableCards[availableCards.Count - 1];

            selectedCards.Add(cardToAdd);
            availableCards.Remove(cardToAdd);
        }

        return selectedCards;
    }

    public void RerollCards()
    {
        List<Card> newCards = GenerateRandomCards();
        CardSelectionUI.Instance.UpdateCardOptions(newCards);
    }
    private float GetRarityWeight(CardRarity rarity, float currentLuck)
    {
        float luck = Mathf.Max(currentLuck, 1f);

        switch (rarity)
        {
            case CardRarity.Common:
                return 100f / luck;

            case CardRarity.Uncommon:
                return 50f / (luck * 0.5f);

            case CardRarity.Rare:
                return 20f * Mathf.Clamp(luck, 1f, 50f);

            case CardRarity.Epic:
                return 10f * (luck * 0.5f);

            case CardRarity.Legendary:
                return 4f * luck;

            case CardRarity.Mythical:
                return 1f * Mathf.Pow(luck, 1.2f);

            case CardRarity.Exotic:
                return 0.15f * Mathf.Pow(luck, 1.5f);

            default: return 1f;
        }
    }
    public CurrentEnemyStats GetCurrentEnemyMultipliers()
    {

        float currentHealthInc = healthIncrement * globalEnemyHealthMultiplier;

        return new CurrentEnemyStats
        {
            totalHealthMultiplier = globalEnemyHealthMultiplier,
            totalSpeedMultiplier = globalEnemySpeedMultiplier,
            totalFireRateMultiplier = globalEliteFireRateMultiplier,
            totalDamageMultiplier = globalEliteDamageMultiplier,
            currentHealthIncrement = currentHealthInc
        };
    }
    public void QueueSpecialUnit(GameObject prefab, int count)
    {
        for (int i = 0; i < count; i++)
        {
            queuedSpecialUnits.Add(prefab);
        }
    }
    public void SpawnSpecialUnitImmediate(GameObject prefab, int count)
    {
        for (int i = 0; i < count; i++)
        {
            if (spawnPoints.Length == 0) return;

            Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];

            GameObject boss = Instantiate(prefab, spawnPoint.position, spawnPoint.rotation);

            zombiesAlive++;
            UpdateZombiesUI();
        }
    }
    void CalculateEliteQuota()
    {
        eliteRoundQuota.Clear();

        foreach (var elite in eliteTypes)
        {
            int countToSpawn = 0;

            // 1. Check Unlock
            if (currentRound < elite.unlockRound)
            {
                eliteRoundQuota[elite] = 0;
                continue;
            }

            // 2. Check Guaranteed Rounds (Force spawn)
            bool isGuaranteed = false;
            if (elite.guaranteedRounds != null)
            {
                if (System.Array.Exists(elite.guaranteedRounds, r => r == currentRound))
                {
                    countToSpawn = 1; // At least one
                    isGuaranteed = true;
                }
            }

            // 3. Roll Dice for EXTRA spawns (only up to maxPerRound)
            // We run the probability check 'maxPerRound' times.
            // If chance is 20%, and max is 3, we flip a coin 3 times.
            // You might get 0, 1, 2, or 3.

            int attempts = elite.maxPerRound;
            if (isGuaranteed) attempts--; // We already used one slot

            float chance = elite.GetSpawnChance(currentRound);

            for (int i = 0; i < attempts; i++)
            {
                if (Random.value <= chance)
                {
                    countToSpawn++;
                }
            }

            eliteRoundQuota[elite] = countToSpawn;

            if (countToSpawn > 0)
                Debug.Log($"Round {currentRound}: Quota for {elite.typeName} is {countToSpawn}");
        }
    }

    // --- NEW FUNCTION: TRY TO SPAWN ONE FROM QUOTA ---
    bool AttemptSpawnEliteFromQuota(EliteType elite)
    {
        // 1. Do we have any left in the budget?
        if (!eliteRoundQuota.ContainsKey(elite) || eliteRoundQuota[elite] <= 0)
            return false;

        // 2. Is screen full?
        if (zombiesAlive >= maxZombiesOnScreen)
            return false;

        // 3. Stagger them! (Don't spawn all instantly at second 0)
        // 5% chance per second to release a queued elite.
        // This makes them appear randomly DURING the round.
        if (Random.value > 0.05f)
            return false;

        // Success: Decrease quota and spawn
        eliteRoundQuota[elite]--;
        return true;
    }
    private void SpawnQueuedUnits()
    {
        if (queuedSpecialUnits.Count == 0) return;

        foreach (GameObject prefab in queuedSpecialUnits)
        {
            SpawnSpecialUnitImmediate(prefab, 1);
        }
        queuedSpecialUnits.Clear();
    }
    public void AddPointsToTotal(int amount)
    {
        totalPointsEarned += amount;
    }
    public void TriggerGameOver()
    {
        if (gameOverUI != null)
        {
            gameOverUI.Setup(currentRound, totalZombiesKilled, totalPointsEarned);
        }
        else
        {
            Debug.LogError("GameOverUI reference not set in RoundManager!");
        }
    }
}

