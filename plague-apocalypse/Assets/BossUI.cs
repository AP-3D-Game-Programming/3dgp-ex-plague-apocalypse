using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;
public class BossUI : MonoBehaviour
{
    public static BossUI Instance { get; private set; }

    [Header("UI Components")]
    [SerializeField] private GameObject uiContainer;
    [SerializeField] private Image ghostHealthFillImage;
    [SerializeField] private Image shieldHealthFillImage;
    [SerializeField] private Image currentHealthFillImage;
    [Header("Doomsday UI")]
    public GameObject doomsdayContainer;
    public GameObject message_Start;     // Drag "Text_Start" here
    public GameObject message_Warning;   // Drag "Text_Warning" here
    public GameObject message_Wipe;      // Drag "Text_Wipe" here
    public TMP_Text timerText;
    [Header("Phase 2 Effects")]
    public bool usePhase2Effects = true;
    public Color phase1Color = Color.red;
    [Tooltip("Define a gradient of colors for Phase 2")]
    public Gradient phase2Gradient;
    public float colorCycleSpeed = 1.0f;

    [Header("Shield Effects")]
    public float shieldPulseSpeed = 4f; // How fast it glows
    public float shieldMinAlpha = 0.6f; // How transparent it gets
    public float shieldMaxAlpha = 1.0f; // How opaque it gets

    [Header("Scene Lighting")]
    public Light directionalLight;
    private Color originalLightColor;

    private bool isPhase2 = false;
    private bool hasShield = false; // Track if we currently have shields

    [Header("Transition Settings")]
    public float damageDisplaySpeed = 0.5f;

    private void Awake()
    {
        if (doomsdayContainer != null) doomsdayContainer.SetActive(false);
        if (timerText != null) timerText.gameObject.SetActive(false);
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            if (uiContainer != null) uiContainer.SetActive(false);

            if (directionalLight != null)
            {
                originalLightColor = directionalLight.color;
            }
        }
        if (doomsdayContainer != null) doomsdayContainer.SetActive(false);
        if (message_Start != null) message_Start.SetActive(false);
        if (message_Warning != null) message_Warning.SetActive(false);
        if (message_Wipe != null) message_Wipe.SetActive(false);
    }

    public void ToggleMessage(int index, bool isActive)
    {
        // Always turn on the main panel if we are showing a message
        if (isActive && doomsdayContainer != null) doomsdayContainer.SetActive(true);

        if (index == 0 && message_Start != null) message_Start.SetActive(isActive);
        if (index == 1 && message_Warning != null) message_Warning.SetActive(isActive);
        if (index == 2 && message_Wipe != null) message_Wipe.SetActive(isActive);
    }
    public void SetBarVisibility(bool isVisible)
    {
        if (uiContainer != null) uiContainer.SetActive(isVisible);
    }

    private void Update()
    {
        // --- 1. PHASE 2 RAINBOW LOGIC ---
        if (isPhase2 && usePhase2Effects)
        {
            // Use unscaledTime so visuals don't freeze during Time Stop
            float t = Mathf.PingPong(Time.unscaledTime * colorCycleSpeed, 1f);
            Color newColor = phase2Gradient.Evaluate(t);

            if (currentHealthFillImage != null)
                currentHealthFillImage.color = newColor;

            if (directionalLight != null)
                directionalLight.color = newColor;
        }

        // --- 2. SHIELD PULSE LOGIC (NEW) ---
        if (hasShield && shieldHealthFillImage != null)
        {
            // Calculate alpha between Min and Max
            float alpha = Mathf.Lerp(shieldMinAlpha, shieldMaxAlpha, Mathf.PingPong(Time.unscaledTime * shieldPulseSpeed, 1f));

            // Apply alpha while keeping the original blue color
            Color c = shieldHealthFillImage.color;
            c.a = alpha;
            shieldHealthFillImage.color = c;
        }
    }

    public void TriggerPhase2Visuals()
    {
        isPhase2 = true;
    }

    public void ShowHealthBar()
    {
        if (uiContainer != null) uiContainer.SetActive(true);

        isPhase2 = false;
        hasShield = false; // Reset shield status

        if (currentHealthFillImage != null)
            currentHealthFillImage.color = phase1Color;

        if (directionalLight != null)
            directionalLight.color = originalLightColor;

        if (ghostHealthFillImage != null) ghostHealthFillImage.fillAmount = 1f;
        if (currentHealthFillImage != null) currentHealthFillImage.fillAmount = 1f;

        if (shieldHealthFillImage != null)
        {
            shieldHealthFillImage.fillAmount = 0f;
            // Reset alpha to full just in case
            Color c = shieldHealthFillImage.color;
            c.a = 1f;
            shieldHealthFillImage.color = c;
        }
    }

    public void HideHealthBar()
    {
        if (doomsdayContainer != null) doomsdayContainer.SetActive(false);
        if (uiContainer != null) uiContainer.SetActive(false);
        if (directionalLight != null) directionalLight.color = originalLightColor;
    }

    public void UpdateHealthBar(float currentHealth, float maxHealth, float currentShield)
    {
        float targetHealthRatio = currentHealth / maxHealth;
        // Logic: Shield Bar represents (HP + Shield). 
        // Since Health Bar is ON TOP of Shield Bar in hierarchy, the extra length is the shield.
        float totalEffectiveHealthRatio = (currentHealth + currentShield) / maxHealth;

        if (shieldHealthFillImage != null)
        {
            shieldHealthFillImage.fillAmount = totalEffectiveHealthRatio;

            // Check if we actually have active shield to trigger the pulse effect
            hasShield = currentShield > 0;

            // If shield is gone, hide the bar completely or reset alpha
            if (!hasShield)
            {
                Color c = shieldHealthFillImage.color;
                c.a = 0f; // Make invisible instantly if shield breaks
                shieldHealthFillImage.color = c;
            }
        }

        if (ghostHealthFillImage != null) ghostHealthFillImage.fillAmount = targetHealthRatio;

        StartCoroutine(AnimateHealthBar(targetHealthRatio));
    }


    public void SetTimer(string timeText, Color color)
    {

        if (doomsdayContainer != null) doomsdayContainer.SetActive(true);

        if (timerText != null)
        {
            timerText.text = timeText;
            timerText.color = color;
            timerText.gameObject.SetActive(true);
        }
    }

    public void HideTimer()
    {
        if (doomsdayContainer != null) doomsdayContainer.SetActive(false);
    }


    IEnumerator AnimateHealthBar(float targetHealthRatio)
    {
        if (currentHealthFillImage == null) yield break;

        float startValue = currentHealthFillImage.fillAmount;
        float timer = 0f;

        // Use unscaledDeltaTime so the health bar animates even if time is stopped
        while (timer < damageDisplaySpeed)
        {
            timer += Time.unscaledDeltaTime;
            float t = timer / damageDisplaySpeed;
            t = t * t * (3f - 2f * t);

            currentHealthFillImage.fillAmount = Mathf.Lerp(startValue, targetHealthRatio, t);
            yield return null;
        }

        currentHealthFillImage.fillAmount = targetHealthRatio;
    }

    public void UpdateTimerShake(float panicLevel)
    {
        // Check if the timer text has the DramaticTXT script
        if (timerText != null)
        {
            DramaticTXT shaker = timerText.GetComponent<DramaticTXT>();
            if (shaker != null)
            {
                shaker.SetShakeIntensity(panicLevel);
            }
        }
    }
}