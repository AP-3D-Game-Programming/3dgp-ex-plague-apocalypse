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


    public void UpdateDisplay(int currentHealth, int maxHealth)
    {

        if (healthText != null)
        {
            healthText.text = $"{currentHealth}";
        }

        if (healthSpriteImage == null || healthPanelBackgroundImage == null) return;

        float healthPercent = (float)currentHealth / maxHealth;
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

        // 4. Apply the chosen sprites
        healthSpriteImage.sprite = foregroundIconSprite;
        healthPanelBackgroundImage.sprite = backgroundPanelSprite;
    }
}