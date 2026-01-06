using UnityEngine;
using System;
using System.Collections;

public class WeaponController : MonoBehaviour
{
    private WeaponInstance instance;
    private PlayerStats playerStats;
    public Transform muzzlePoint;
    public Action<int, int> onAmmoChanged;

    private float nextFireTime;
    private bool isReloading;

    public void Initialize(WeaponInstance weaponInstance, PlayerEffectManager effects)
    {
        instance = weaponInstance;
        // Search in parent to find the PlayerStats component
        playerStats = GetComponentInParent<PlayerStats>();
    }

    void Update()
    {
        if (isReloading || instance == null) return;

        if (Input.GetButton("Fire1") && Time.time >= nextFireTime)
        {
            if (instance.currentClip > 0) Shoot();
            else StartCoroutine(Reload());
        }

        if (Input.GetKeyDown(KeyCode.R) && instance.currentClip < GetMaxMagSize())
            StartCoroutine(Reload());
    }

    void Shoot()
    {
        // 1. Calculate Fire Rate (includes Global and Type-Specific)
        nextFireTime = Time.time + (1f / GetFinalFireRate());

        GameObject bulletObj = Instantiate(instance.data.projectilePrefab, muzzlePoint.position, muzzlePoint.rotation);
        Projectile projectile = bulletObj.GetComponent<Projectile>();

        if (projectile != null)
        {
            // 2. Calculate Damage (includes Global and Type-Specific)
            float finalDamage = GetFinalDamage();
            projectile.Initialize(finalDamage, instance.data.weaponType, instance.data.effects);
        }

        instance.currentClip--;
        onAmmoChanged?.Invoke(instance.currentClip, instance.currentReserve);
    }

    IEnumerator Reload()
    {
        isReloading = true;

        // 3. Calculate Reload Time (Standard time * Multiplier)
        // Note: Usually reload multipliers are < 1 (e.g., 0.8) to make it faster.
        float reloadDuration = instance.data.reloadTime * (playerStats != null ? playerStats.reloadSpeedMultiplier : 1f);

        yield return new WaitForSeconds(reloadDuration);

        int amountNeeded = GetMaxMagSize() - instance.currentClip;
        int amountToTake = Mathf.Min(amountNeeded, instance.currentReserve);

        instance.currentClip += amountToTake;
        instance.currentReserve -= amountToTake;

        onAmmoChanged?.Invoke(instance.currentClip, instance.currentReserve);
        isReloading = false;
    }

    // --- Calculation Helpers ---

    private float GetFinalDamage()
    {
        float damage = instance.data.damage;
        if (playerStats == null) return damage;

        // Apply Global Multiplier
        damage *= playerStats.damageMultiplier;

        // Apply Weapon-Specific Multiplier (e.g., from a "Sniper Buff" card)
        if (playerStats.typeDamageMults.TryGetValue(instance.data.weaponType, out float typeMult))
        {
            damage *= typeMult;
        }

        return damage;
    }

    private float GetFinalFireRate()
    {
        float rate = instance.data.fireRate;
        if (playerStats == null) return rate;

        // Apply Global Multiplier
        rate *= playerStats.fireRateMultiplier;

        // Apply Weapon-Specific Multiplier
        if (playerStats.typeFireRateMults.TryGetValue(instance.data.weaponType, out float typeMult))
        {
            rate *= typeMult;
        }

        return rate;
    }

    private int GetMaxMagSize()
    {
        return instance.data.magazineSize + (playerStats != null ? playerStats.magazineSizeBonus : 0);
    }
}