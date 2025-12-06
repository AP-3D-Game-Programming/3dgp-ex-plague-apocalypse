using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Gun : MonoBehaviour
{
    [Header("Setup")]
    public Transform muzzlePoint;
    
    // Automatisch gevonden componenten
    private Animator gunAnimator;
    private WeaponData data;
    private PlayerEffectManager effectManager;
    
    private float nextFireTime;
    private int currentClip;
    private int currentReserve;
    private bool isReloading = false;

    public void Initialize(WeaponData weaponData, PlayerEffectManager manager)
    {
        this.data = weaponData;
        this.effectManager = manager;
        this.gunAnimator = GetComponent<Animator>();
    
        currentClip = data.magazineSize;
        currentReserve = data.maxAmmo;
    }

    public void AttemptShoot()
    {
        if (data == null || isReloading) return;

        // 1. Fire Rate Check
        if (Time.time >= nextFireTime)
        {
            // 2. Ammo Check
            if (currentClip <= 0)
            {
                StartCoroutine(Reload()); // Leeg? Reload automatisch
                return;
            }

            // Alles OK? Vuur!
            nextFireTime = Time.time + data.fireRate;
            currentClip--; 
            
            Shoot();
        }
    }

    public void AttemptReload()
    {
        // Alleen reloaden als we niet vol zitten EN reserve hebben
        if (!isReloading && currentClip < data.magazineSize && currentReserve > 0)
        {
            StartCoroutine(Reload());
        }
    }
    protected virtual void Shoot()
    {
        // A. Kogel Spawnen
        if (data.projectilePrefab != null && muzzlePoint != null)
        {
            GameObject bulletObj = Instantiate(data.projectilePrefab, muzzlePoint.position, muzzlePoint.rotation);
            Projectile bulletScript = bulletObj.GetComponent<Projectile>();

            if (bulletScript != null)
            {
                // Effecten verzamelen
                List<BulletEffect> effects = new List<BulletEffect>();
                if(effectManager != null) effects = effectManager.GetActiveEffectsForWeapon(data.weaponType);
                if(data.effects != null) effects.AddRange(data.effects);

                // Kogel activeren
                bulletScript.Initialize(data.damage, data.weaponType, effects);
            }
        }

        // B. Muzzle Flash
        if (data.muzzleFlashPrefab != null && muzzlePoint != null)
        {
            GameObject flash = Instantiate(data.muzzleFlashPrefab, muzzlePoint.position, muzzlePoint.rotation);
            // Zorg dat de flash even meebeweegt met de loop
            flash.transform.SetParent(muzzlePoint); 
            Destroy(flash, 0.5f);
        }
        // C. Animatie afspelen
        if (gunAnimator != null)
        {
            gunAnimator.SetTrigger("Shoot");
        }
    }

    IEnumerator Reload()
    {
        isReloading = true;
        Debug.Log("Reloading...");

        if (gunAnimator != null) gunAnimator.SetTrigger("Reload");

        yield return new WaitForSeconds(data.reloadTime);

        int needed = data.magazineSize - currentClip;
        int toLoad = Mathf.Min(needed, currentReserve);

        currentReserve -= toLoad;
        currentClip += toLoad;

        isReloading = false;
        Debug.Log("Reload Klaar!");
    }
}