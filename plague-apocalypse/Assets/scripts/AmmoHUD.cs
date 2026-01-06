using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class AmmoHUD : MonoBehaviour
{
    [Header("Main Stats")]
    [SerializeField] private TextMeshProUGUI ammoText;
    [SerializeField] private TextMeshProUGUI weaponNameText;

    [Header("Weapon Slots")]
    // Assign your two Icon images here in the Inspector
    [SerializeField] private Image[] slotIcons;
    // Assign the background/frame images for the slots here to show selection
    [SerializeField] private Image[] slotFrames;

    [Header("Visual Settings")]
    [SerializeField] private Color activeColor = Color.white;
    [SerializeField] private Color inactiveColor = new Color(0.3f, 0.3f, 0.3f, 0.5f);
    [SerializeField] private float activeScale = 1.1f;
    [SerializeField] private float inactiveScale = 0.9f;

    /// <summary>
    /// Updates the entire inventory UI (Icons and Highlights)
    /// </summary>
    public void RefreshInventory(List<WeaponInstance> weapons, int activeIndex)
    {
        for (int i = 0; i < slotIcons.Length; i++)
        {
            // Check if we actually have a weapon for this UI slot
            if (i < weapons.Count)
            {
                slotIcons[i].enabled = true;
                slotIcons[i].sprite = weapons[i].data.weaponIcon;

                // Highlight the active slot
                bool isActive = (i == activeIndex);
                ApplySlotVisuals(i, isActive);

                // Update the Name text if this is the active one
                if (isActive)
                {
                    weaponNameText.text = weapons[i].data.weaponName;
                }
            }
            else
            {
                // No weapon in this slot
                slotIcons[i].enabled = false;
                slotFrames[i].color = inactiveColor;
                slotFrames[i].transform.localScale = Vector3.one * inactiveScale;
            }
        }
    }

    private void ApplySlotVisuals(int index, bool isActive)
    {
        slotFrames[index].color = isActive ? activeColor : inactiveColor;
        slotIcons[index].color = isActive ? Color.white : new Color(1, 1, 1, 0.4f);
        slotFrames[index].transform.localScale = Vector3.one * (isActive ? activeScale : inactiveScale);
    }

    public void UpdateAmmoDisplay(int clip, int reserve)
    {
        if (ammoText != null)
            ammoText.text = $"{clip} / {reserve}";
    }
}