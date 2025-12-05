using System.Collections.Generic;
using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    [SerializeField] private List<WeaponData> weapons = new List<WeaponData>();
    [SerializeField] private Transform weaponHolder;
    
    // Welk wapen hebben we nu vast? (0 of 1)
    private int currentWeaponIndex = 0;
    
    // Maximaal aantal wapens
    private int maxWeapons = 2;
    private GameObject currentWeapon;

    void Update()
    {
        // Wapen wisselena met scrollwiel of toetsen
        if (Input.GetKeyDown(KeyCode.Alpha1)) EquipWeapon(0);
        if (Input.GetKeyDown(KeyCode.Alpha2)) EquipWeapon(1);
        
        // Simpele scrollwiel logica
        if (Input.GetAxis("Mouse ScrollWheel") != 0)
        {
            // Wissel tussen 0 en 1
            int newIndex = currentWeaponIndex == 0 ? 1 : 0;
            EquipWeapon(newIndex);
        }
    }
    public void PickupWeapon(WeaponData newWeapon)
    {
        if (weapons.Count < maxWeapons)
        {
            weapons.Add(newWeapon);
            EquipWeapon(weapons.Count - 1); // Pak het nieuwe wapen vast
        }
        else
        {
            weapons[currentWeaponIndex] = newWeapon;
            EquipWeapon(currentWeaponIndex); // Herlaad het model
        }
    }

    void EquipWeapon(int index)
    {
        if (index >= weapons.Count) return;
        if (currentWeapon != null) Destroy(currentWeapon);

        currentWeapon = Instantiate(weapons[index].weaponPrefab, weaponHolder);
        currentWeapon.transform.localPosition = new Vector3(0.25f, 1f, 1f);
        currentWeapon.transform.localRotation = Quaternion.identity;
        currentWeaponIndex = index;
    }
}