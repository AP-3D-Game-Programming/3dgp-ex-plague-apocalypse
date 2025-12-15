using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HealthDisplayManager : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI healthText;
    public Image healthSpriteImage;
    public Image healthPanelBackgroundImage;

    [Header("Foreground Sprites (Icon)")]
    public Sprite fullHealthSprite;   // 66.7%
    public Sprite damagedSprite;      // 33.4%
    public Sprite criticalSprite;     // 33.4%

    [Header("Background Sprites")]
    public Sprite normalBackgroundSprite;
    public Sprite criticalBackgroundSprite;

    [Header("Heartbeat Settings")]
    public float minPulseSpeed = 2f;  // Speed at full health
    public float maxPulseSpeed = 10f; // Speed at near death
    public float pulseScaleAmount = 0.1f; // How much it grows (0.1 = 10% bigger)

    private Vector3 _originalScale;
    private float _currentPulseSpeed;

    private void Start()
    {
        // Store the starting size of the heart so we can scale relative to it
        if (healthSpriteImage != null)
        {
            _originalScale = healthSpriteImage.transform.localScale;
        }
    }

    private void Update()
    {
        if (healthSpriteImage == null) return;

        // Calculate the scale factor using a Sine wave
        // Time.time * _currentPulseSpeed determines how fast the wave moves
        float scaleOffset = Mathf.Sin(Time.time * _currentPulseSpeed) * pulseScaleAmount;

        // Apply the scale (Original Size + The Sine Wave offset)
        healthSpriteImage.transform.localScale = _originalScale + (_originalScale * scaleOffset);
    }

    public void UpdateDisplay(int currentHealth, int maxHealth)
    {
        if (healthText != null)
        {
            healthText.text = $"{currentHealth}";
        }

        if (healthSpriteImage == null || healthPanelBackgroundImage == null) return;

        float healthPercent = (float)currentHealth / maxHealth;

        _currentPulseSpeed = Mathf.Lerp(maxPulseSpeed, minPulseSpeed, healthPercent);

        Sprite foregroundIconSprite;
        Sprite backgroundPanelSprite;

        if (healthPercent > 0.667f)
        {
            // HP 3/3
            foregroundIconSprite = fullHealthSprite;
            backgroundPanelSprite = normalBackgroundSprite;
        }
        else if (healthPercent > 0.334f)
        {
            //HP 2/3
            foregroundIconSprite = damagedSprite;
            backgroundPanelSprite = normalBackgroundSprite;
        }
        else
        {
            // HP 1/3
            foregroundIconSprite = criticalSprite;
            backgroundPanelSprite = criticalBackgroundSprite;
        }

        healthSpriteImage.sprite = foregroundIconSprite;
        healthPanelBackgroundImage.sprite = backgroundPanelSprite;
    }
}