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

    [Header("Scene Lighting (NEW)")]
    public Light directionalLight; // DRAG YOUR DIRECTIONAL LIGHT HERE
    private Color originalLightColor; // To remember what the sun looked like before

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

            // Save the original sun color so we can reset it later
            if (directionalLight != null)
            {
                originalLightColor = directionalLight.color;
            }
        }
    }

    private void Update()
    {
        // Only run this logic if we are in Phase 2
        if (isPhase2 && usePhase2Effects)
        {
            float t = Mathf.PingPong(Time.time * colorCycleSpeed, 1f);
            Color newColor = phase2Gradient.Evaluate(t);

            // 1. Update the UI Bar
            if (currentHealthFillImage != null)
            {
                currentHealthFillImage.color = newColor;
            }

            // 2. Update the Directional Light
            if (directionalLight != null)
            {
                directionalLight.color = newColor;
            }
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

        // Reset UI Color
        if (currentHealthFillImage != null)
            currentHealthFillImage.color = phase1Color;

        // Reset Light Color
        if (directionalLight != null)
            directionalLight.color = originalLightColor;

        ghostHealthFillImage.fillAmount = 1f;
        currentHealthFillImage.fillAmount = 1f;

        if (shieldHealthFillImage != null)
            shieldHealthFillImage.fillAmount = 0f;
    }

    public void HideHealthBar()
    {
        uiContainer.SetActive(false);

        // Ensure light resets if we hide the bar (boss dies)
        if (directionalLight != null)
            directionalLight.color = originalLightColor;
    }

    public void UpdateHealthBar(int currentHealth, int maxHealth, float currentShield)
    {
        float targetHealthRatio = (float)currentHealth / maxHealth;
        float totalEffectiveHealthRatio = (currentHealth + currentShield) / maxHealth;

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
            t = t * t * (3f - 2f * t);

            currentHealthFillImage.fillAmount = Mathf.Lerp(startValue, targetHealthRatio, t);
            yield return null;
        }

        currentHealthFillImage.fillAmount = targetHealthRatio;
    }
}