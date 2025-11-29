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

    private Dictionary<EliteType, bool> eliteSpawnedThisRound = new Dictionary<EliteType, bool>();

    void Start()
    {
        GameObject playerObj = GameObject.FindWithTag("Player");
        if (playerObj != null)
            playerTransform = playerObj.transform;
        else
            Debug.LogError("RoundManager: Could not find object with tag 'Player'!");

        UpdateRoundUI();
        UpdateZombiesUI();

        spawnRoutine = StartCoroutine(StartRound());
    }

    IEnumerator StartRound()
    {
        eliteSpawnedThisRound.Clear();
        enemiesRemaining = baseEnemies + (currentRound - 1) * 2;
        UpdateZombiesUI();

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
                if (ShouldSpawnElite(elite, currentRound))
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

    bool ShouldSpawnElite(EliteType elite, int currentRound)
    {
        if (currentRound < elite.unlockRound)
            return false;


        // Guaranteed rounds
        if (elite.guaranteedRounds != null && elite.guaranteedRounds.Length > 0)
        {
            if (System.Array.Exists(elite.guaranteedRounds, r => r == currentRound))
            {
                if (eliteSpawnedThisRound.ContainsKey(elite) && eliteSpawnedThisRound[elite])
                    return false;
                eliteSpawnedThisRound[elite] = true;
                return true;
            }
        }

        // Weighted chance
        return Random.value <= elite.GetSpawnChance(currentRound);
    }



    public void EnemyKilled()
    {
        zombiesAlive--;
        UpdateZombiesUI();

        if (enemiesRemaining <= 0 && zombiesAlive <= 0)
        {
            if (spawnRoutine != null)
                StopCoroutine(spawnRoutine);

            currentRound++;
            UpdateRoundUI();
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

        List<Card> availableCards = new List<Card>(allCards);
        List<Card> options = new List<Card>();

        for (int i = 0; i < 3 && availableCards.Count > 0; i++)
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
                if (rand <= sum)
                {
                    cardToAdd = card;
                    break;
                }
            }

            if (cardToAdd == null) cardToAdd = availableCards[0];

            options.Add(cardToAdd);
            availableCards.Remove(cardToAdd);
        }

        CardSelectionUI.Instance.ShowOptions(options);
        yield return new WaitUntil(() => CardSelectionUI.Instance.cardChosen);
        Card chosenCard = CardSelectionUI.Instance.GetChosenCard();
        chosenCard.Apply(this);

        foreach (var obj in hudObjects) obj.SetActive(true);
        spawnRoutine = StartCoroutine(StartNextRoundWithDelay());
    }

    // This method expects TWO arguments (rarity, currentLuck)
    private float GetRarityWeight(CardRarity rarity, float currentLuck)
    {
        float baseWeight = 0f;
        switch (rarity)
        {
            case CardRarity.Common: baseWeight = 50f; break;
            case CardRarity.Uncommon: baseWeight = 25f; break;
            case CardRarity.Rare: baseWeight = 13f; break;
            case CardRarity.Epic: baseWeight = 7f; break;
            case CardRarity.Legendary: baseWeight = 4f; break;
            case CardRarity.Mythical: baseWeight = 0.5f; break;
            case CardRarity.Exotic: baseWeight = 0.15f; break;
            default: return 1f;
        }

        if (rarity >= CardRarity.Rare)
        {
            return baseWeight * currentLuck;
        }
        return baseWeight;
    }
}

