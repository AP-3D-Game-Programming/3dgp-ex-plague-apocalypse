using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI; // Import for Image
using System.Collections;

public class PlayerHealth : MonoBehaviour
{
    [Header("UI Reference")]
    public HealthDisplayManager healthDisplayManager;
    public Image damageOverlay;

    [Header("Damage Overlay Settings")]
    public float damageFlashSpeed = 2f;
    public float maxOverlayAlpha = 0.7f;
    [Header("Health")]
    public int maxHealth = 100;
    public int currentHealth;

    [Header("Regeneration")]
    public float regenRate = 5f;
    public float regenDelay = 5f;

    private float lastDamageTime;
    private float regenBuffer = 0f;
    private bool damaged = false;

    [Header("Game Over Settings")]
    public Gameoverscript gameOverUI;
    public RoundManager roundManager;
    public GameObject mapCamera;
    public GameObject mainCamera;
    public float deathDelay = 2.0f;
    private bool isDead = false;

    void Awake()
    {
        currentHealth = maxHealth;
        lastDamageTime = -regenDelay;
        if (mainCamera != null) mainCamera.SetActive(true);
        if (mapCamera != null) mapCamera.SetActive(false);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        UpdateHealthDisplay();
    }

    void Update()
    {
        if (isDead) return;

        // --- Handle Regeneration ---
        if (currentHealth < maxHealth && Time.time >= lastDamageTime + regenDelay)
        {
            Regenerate();
        }

        // --- Handle Damage Overlay ---
        if (damageOverlay != null)
        {
            // Calculate the target alpha based on missing health
            float healthPercent = (float)currentHealth / maxHealth;
            float targetAlpha = maxOverlayAlpha * (1f - healthPercent);

            // If we just got hit, temporarily flash to a higher alpha
            if (damaged)
            {
                damageOverlay.color = new Color(damageOverlay.color.r, damageOverlay.color.g, damageOverlay.color.b, 0.8f); // Instant flash
                damaged = false;
            }

            // Smoothly lerp the current alpha towards the target alpha
            Color newColor = damageOverlay.color;
            newColor.a = Mathf.Lerp(damageOverlay.color.a, targetAlpha, damageFlashSpeed * Time.deltaTime);
            damageOverlay.color = newColor;
        }
    }

    void Regenerate()
    {
        regenBuffer += regenRate * Time.deltaTime;

        if (regenBuffer >= 1f)
        {
            int amountToHeal = Mathf.FloorToInt(regenBuffer);
            int oldHealth = currentHealth;

            currentHealth += amountToHeal;
            regenBuffer -= amountToHeal;

            if (currentHealth > maxHealth)
            {
                currentHealth = maxHealth;
                regenBuffer = 0f;
            }

            if (currentHealth != oldHealth)
            {
                UpdateHealthDisplay();
            }
        }
    }

    public void TakeDamage(int damage)
    {
        if (isDead) return;

        damaged = true;
        currentHealth -= damage;
        lastDamageTime = Time.time;
        regenBuffer = 0f;

        UpdateHealthDisplay();

        Debug.Log($"Player took {damage} damage, remaining health: {currentHealth}");

        if (currentHealth <= 0)
            Die();
    }

    public void Heal(int amount)
    {
        int oldHealth = currentHealth;
        currentHealth += amount;

        if (currentHealth > maxHealth) currentHealth = maxHealth;

        if (currentHealth != oldHealth)
        {
            UpdateHealthDisplay();
        }
    }


    public void SetMaxHealth(int newMaxHealth)
    {
        int healthDifference = newMaxHealth - maxHealth;
        maxHealth = newMaxHealth;


        currentHealth += healthDifference;
        currentHealth = Mathf.Min(currentHealth, maxHealth);

        UpdateHealthDisplay();
    }
    public void UpdateHealthDisplay()
    {
        if (healthDisplayManager != null)
        {
            healthDisplayManager.UpdateDisplay(currentHealth, maxHealth);
        }
    }

    void Die()
    {
        if (isDead) return;
        isDead = true;
        if (damageOverlay != null)
        {
            Color deadColor = damageOverlay.color;
            deadColor.a = 1f;
            damageOverlay.color = deadColor;
        }

        Debug.Log("Player died!");

        var movement = GetComponent<FirstPersonMovement>();
        if (movement != null) movement.enabled = false;

        StartCoroutine(DeathSequence());
    }

    IEnumerator DeathSequence()
    {
        yield return new WaitForSeconds(deathDelay);

        if (mainCamera != null) mainCamera.SetActive(false);
        if (mapCamera != null) mapCamera.SetActive(true);

        int kills = 0;
        int round = 0;
        int score = 0;
        if (roundManager != null)
        {
            kills = roundManager.totalZombiesKilled;
            round = roundManager.currentRound;
            score = roundManager.totalPointsEarned;
        }

        if (gameOverUI != null)
        {
            gameOverUI.Setup(round, kills, score);
        }
        else
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }
}