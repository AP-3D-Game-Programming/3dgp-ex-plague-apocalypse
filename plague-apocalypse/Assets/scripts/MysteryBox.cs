using System.Collections.Generic;
using UnityEngine;

public class MysteryBox : Interactable
{
    [Header("Mystery Box Settings")]
    [SerializeField] private List<WeaponData> weapons;
    public int boxCost = 950;

    void Start()
    {
        promptMessage = $"Press E for Mystery Box [{boxCost}]";
    }

    public override void OnInteract(PlayerInventory inventory)
    {
        if (weapons.Count > 0)
        {
            int randomIndex = Random.Range(0, weapons.Count);
            WeaponData wonWeapon = weapons[randomIndex];
            
            inventory.PickupWeapon(wonWeapon);
            Debug.Log("Mystery Box gaf: " + wonWeapon.weaponName);
        }
    }
}