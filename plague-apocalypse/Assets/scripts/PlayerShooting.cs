using UnityEngine;

public class PlayerShooting : MonoBehaviour
{
    private Gun currentGun;
    private PlayerInventory inventory;

    void Awake()
    {
        inventory = GetComponent<PlayerInventory>();
    }

    void Update()
    {
        if (currentGun == null)
        {
            currentGun = GetComponentInChildren<Gun>();
        }

        WeaponData currentData = inventory.GetCurrentWeapon();

        if (currentGun != null && currentData != null)
        {
            HandleShooting(currentData);
            HandleReload();
        }
    }

    void HandleShooting(WeaponData data)
    {
        bool isFiring;

        if (data.isAutomatic)
        {
            isFiring = Input.GetButton("Fire1"); 
        }
        else
        {
            isFiring = Input.GetButtonDown("Fire1"); 
        }

        if (isFiring)
        {
            currentGun.AttemptShoot();
        }
    }

    void HandleReload()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            currentGun.AttemptReload();
        }
    }
    public void UpdateCurrentGun()
    {
        currentGun = GetComponentInChildren<Gun>();
    }
}