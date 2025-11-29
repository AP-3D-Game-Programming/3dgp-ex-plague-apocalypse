using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class PlayerHealth : MonoBehaviour
{
    [Header("UI Reference")]
    public HealthDisplayManager healthDisplayManager;

    [Header("Health")]
    public int maxHealth = 100;
    public int currentHealth;

    [Header("Regeneration")]
    public float regenRate = 5f;
    public float regenDelay = 5f;

    private float lastDamageTime;
    private float regenBuffer = 0f;

    void Awake()
    {
        currentHealth = maxHealth;
        lastDamageTime = -regenDelay;

        UpdateHealthDisplay();
    }

    void Update()
    {
        if (currentHealth < maxHealth && Time.time >= lastDamageTime + regenDelay)
        {
            Regenerate();
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
        Debug.Log("Player died!");

        StartCoroutine(RestartLevel());
    }

    IEnumerator RestartLevel()
    {
        yield return new WaitForSeconds(2f);
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}