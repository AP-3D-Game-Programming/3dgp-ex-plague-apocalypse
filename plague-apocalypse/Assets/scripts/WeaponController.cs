using UnityEngine;
using System.Collections.Generic;
using System;

public class WeaponController : MonoBehaviour
{
    public WeaponInstance weaponInstance;
    private PlayerEffectManager effectManager;

    public Action<int, int> onAmmoChanged;

    public void Initialize(WeaponInstance instance, PlayerEffectManager manager)
    {
        this.weaponInstance = instance;
        this.effectManager = manager;
        UpdateUI();
    }

    public bool HasAmmo()
    {
        return weaponInstance != null && weaponInstance.currentClip > 0;
    }

    public bool CanReload()
    {
         return weaponInstance != null && 
                weaponInstance.currentClip < weaponInstance.data.magazineSize && 
                weaponInstance.currentReserve > 0;
    }

    public void ConsumeAmmo()
    {
        if (weaponInstance != null)
        {
            weaponInstance.currentClip--;
            UpdateUI();
        }
    }

    public void RefillClip()
    {
        if (weaponInstance == null) return;

        int needed = weaponInstance.data.magazineSize - weaponInstance.currentClip;
        int toLoad = Mathf.Min(needed, weaponInstance.currentReserve);

        weaponInstance.currentReserve -= toLoad;
        weaponInstance.currentClip += toLoad;
        UpdateUI();
    }

    public float GetDamage()
    {
        float dmg = weaponInstance.data.damage;
        if (PlayerStats.Instance != null) 
            dmg *= PlayerStats.Instance.GetTotalDamageMult(weaponInstance.data.weaponType);
        return dmg;
    }

    public float GetFireRate()
    {
        float rate = weaponInstance.data.fireRate; 
        if (PlayerStats.Instance != null) 
            rate /= PlayerStats.Instance.GetTotalFireRateMult(weaponInstance.data.weaponType);
        return rate;
    }

    public float GetReloadTime()
    {
        float time = weaponInstance.data.reloadTime;
        if (PlayerStats.Instance != null) 
            time /= PlayerStats.Instance.reloadSpeedMultiplier;
        return time;
    }

    public List<BulletEffect> GetEffects()
    {
        List<BulletEffect> allEffects = new List<BulletEffect>();
        if (weaponInstance.data.effects != null) 
            allEffects.AddRange(weaponInstance.data.effects);
        if (effectManager != null) 
            allEffects.AddRange(effectManager.GetActiveEffectsForWeapon(weaponInstance.data.weaponType));
        return allEffects;
    }

    private void UpdateUI()
    {
        onAmmoChanged?.Invoke(weaponInstance.currentClip, weaponInstance.currentReserve);
    }
}