using System.Collections.Generic;
using UnityEngine;

public class WeaponUpgrade : Interactable
{
    [Header("Upgrade Box Settings")]
    [SerializeField] private List<WeaponData> weapons;
    public int boxCost = 950;
    private PlayerInventory playerInventory;


    void Start()
    {

    }

    public override void OnInteract(PlayerInventory inventory)
    {
        if (weapons.Count > 0)
        {
            if (PlayerStats.Instance.points >= boxCost)
            {
                PlayerStats.Instance.RemovePoints(boxCost);
                {
                    WeaponData currentWeapondata = playerInventory.GetCurrentWeapon();
                    if (currentWeapondata != null)
                    {
                        WeaponData wonWeapon = weapons[1];
                        inventory.PickupWeapon(wonWeapon);
                        Debug.Log("Upgrading weapon: " + currentWeapondata.name);
                        currentWeapondata.damage = currentWeapondata.damage * 10;
                    }
                    else
                    {
                        Debug.Log("No current weapon to upgrade!");
                    }
                }
            }
        }
    }
}