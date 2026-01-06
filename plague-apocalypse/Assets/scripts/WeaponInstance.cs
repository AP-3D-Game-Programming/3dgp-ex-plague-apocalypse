using UnityEngine;

[System.Serializable]
public class WeaponInstance
{
    public WeaponData data;       // De statische stats
    public int currentClip;       // Kogels in magazijn
    public int currentReserve;    // Kogels in zak

    public WeaponInstance(WeaponData weaponData)
    {
        this.data = weaponData;
        this.currentClip = weaponData.magazineSize;
        this.currentReserve = weaponData.maxAmmo;
    }

    public void RefillFull()
    {
        currentClip = data.magazineSize;
        currentReserve = data.maxAmmo;
    }
}