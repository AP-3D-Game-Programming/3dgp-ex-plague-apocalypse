using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class BossUI : MonoBehaviour
{
    public static BossUI Instance { get; private set; }

    [Header("UI Components")]
    [SerializeField] private GameObject uiContainer;
    [SerializeField] private Image ghostHealthFillImage;
    [SerializeField] private Image shieldHealthFillImage;
    [SerializeField] private Image currentHealthFillImage;

    [Header("Phase 2 Effects")]
    public bool usePhase2Effects = true;
    public Color phase1Color = Color.red;
    [Tooltip("Define a gradient of colors for Phase 2")]
    public Gradient phase2Gradient;
    public float colorCycleSpeed = 1.0f;

    private bool isPhase2 = false;

    [Header("Transition Settings")]
    public float damageDisplaySpeed = 0.5f;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            uiContainer.SetActive(false);
        }
    }

    private void Update()
    {
        if (isPhase2 && usePhase2Effects && currentHealthFillImage != null)
        {
            float t = Mathf.PingPong(Time.time * colorCycleSpeed, 1f);
            currentHealthFillImage.color = phase2Gradient.Evaluate(t);
        }
    }

    public void TriggerPhase2Visuals()
    {
        isPhase2 = true;
    }


    public void ShowHealthBar()
    {
        uiContainer.SetActive(true);

        // Reset Phase 2 flags
        isPhase2 = false;

        // --- ADDED: Reset Color to Phase 1 Red ---
        if (currentHealthFillImage != null)
            currentHealthFillImage.color = phase1Color;

        ghostHealthFillImage.fillAmount = 1f;
        currentHealthFillImage.fillAmount = 1f;

        // --- ADDED: Reset Shield to 0 ---
        if (shieldHealthFillImage != null)
            shieldHealthFillImage.fillAmount = 0f;
    }

    public void HideHealthBar()
    {
        uiContainer.SetActive(false);
    }

    public void UpdateHealthBar(int currentHealth, int maxHealth, float currentShield)
    {
        float targetHealthRatio = (float)currentHealth / maxHealth;

        // Shield represents (HP + Shield)
        float totalEffectiveHealthRatio = (currentHealth + currentShield) / maxHealth;

        // Update shield immediately
        if (shieldHealthFillImage)
        {
            shieldHealthFillImage.fillAmount = totalEffectiveHealthRatio;
        }

        ghostHealthFillImage.fillAmount = targetHealthRatio;
        StartCoroutine(AnimateHealthBar(targetHealthRatio));
    }

    IEnumerator AnimateHealthBar(float targetHealthRatio)
    {
        float startValue = currentHealthFillImage.fillAmount;
        float timer = 0f;

        while (timer < damageDisplaySpeed)
        {
            timer += Time.deltaTime;
            float t = timer / damageDisplaySpeed;

            // Use smooth step for a more satisfying deceleration
            t = t * t * (3f - 2f * t);

            currentHealthFillImage.fillAmount = Mathf.Lerp(startValue, targetHealthRatio, t);

            yield return null;
        }

        currentHealthFillImage.fillAmount = targetHealthRatio;
    }
}