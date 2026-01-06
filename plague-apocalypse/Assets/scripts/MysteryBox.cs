using System.Collections.Generic;
using UnityEngine;

public class MysteryBox : Interactable
{
    [Header("Mystery Box Settings")]
    [SerializeField] private List<WeaponData> weapons;
    public int boxCost = 950;

    [Header("Visuals")]
    [SerializeField] private GameObject skyBeamObj; // Drag your Cylinder here
    [Header("Audio")]
    [SerializeField] private AudioSource boxAudioSource; // The speaker
    [SerializeField] private AudioClip buySound;         // The jingle/sound file
    void Start()
    {
        promptMessage = $"Press E for Mystery Box [{boxCost}]";

        // Turn the beam ON when the game starts
        if (skyBeamObj != null)
        {
            skyBeamObj.SetActive(true);
        }
    }

    public override void OnInteract(PlayerInventory inventory)
    {
        if (PlayerStats.Instance.points >= boxCost)
        {
            PlayerStats.Instance.RemovePoints(boxCost);
            if (boxAudioSource != null && buySound != null)
            {
                boxAudioSource.PlayOneShot(buySound, 2.0f);
            }
            if (weapons.Count > 0)
            {
                int randomIndex = Random.Range(0, weapons.Count);
                WeaponData wonWeapon = weapons[randomIndex];

                inventory.PickupWeapon(wonWeapon);
                Debug.Log($"Mystery Box bought: {wonWeapon.weaponName}");
            }
        }
        else
        {
            Debug.Log("Not enough points!");
        }
    }
    public void HideBox()
    {
        if (skyBeamObj != null) skyBeamObj.SetActive(false);

    }
}