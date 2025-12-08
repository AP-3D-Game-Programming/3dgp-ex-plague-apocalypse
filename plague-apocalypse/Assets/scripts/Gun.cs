using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class Gun : MonoBehaviour
{
    [Header("Setup")]
    public Transform muzzlePoint;
    private Animator gunAnimator;
    private WeaponData data;
    private PlayerEffectManager effectManager;

    private float nextFireTime;
    private int currentClip;
    private int currentReserve;
    private bool isReloading = false;

    public Action<int, int> onAmmoChanged; // Event: (Huidige clip, huidige reserve)

    public void Initialize(WeaponData weaponData, PlayerEffectManager manager)
    {
        this.data = weaponData;
        this.effectManager = manager;
        this.gunAnimator = GetComponent<Animator>();

        currentClip = data.magazineSize;
        currentReserve = data.maxAmmo;

        // Update de UI direct bij het spawnen
        onAmmoChanged?.Invoke(currentClip, currentReserve);
    }

    public void AttemptShoot()
    {
        if (data == null)
        {
            Debug.LogError("FOUT: Data is NULL! Initialize is niet aangeroepen.");
            return;
        }

        // Als we herladen, mogen we niet schieten
        if (isReloading) return;

        // 1. Fire Rate Check
        if (Time.time >= nextFireTime)
        {
            // 2. Ammo Check (Is het magazijn leeg?)
            if (currentClip <= 0)
            {
                AttemptReload(); // Probeer te herladen als we leeg zijn
                return;
            }

            // Alles OK? Vuur!
            nextFireTime = Time.time + data.fireRate;
            currentClip--;

            // Update UI
            onAmmoChanged?.Invoke(currentClip, currentReserve);

            Shoot();
        }
    }

    public void AttemptReload()
    {
        // Alleen reloaden als we niet vol zitten EN reserve hebben EN niet al bezig zijn
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
                if (effectManager != null) effects = effectManager.GetActiveEffectsForWeapon(data.weaponType);
                if (data.effects != null) effects.AddRange(data.effects);

                // Kogel activeren met de juiste damage
                bulletScript.Initialize(data.damage, data.weaponType, effects);
            }
        }

        // B. Muzzle Flash
        if (data.muzzleFlashPrefab != null && muzzlePoint != null)
        {
            GameObject flash = Instantiate(data.muzzleFlashPrefab, muzzlePoint.position, muzzlePoint.rotation);
            flash.transform.SetParent(muzzlePoint);

            // Zorg dat de flash zichzelf opruimt
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

        // Bereken hoeveel kogels we nodig hebben
        int needed = data.magazineSize - currentClip;

        // Pak wat we nodig hebben, of alles wat over is als de reserve bijna op is
        int toLoad = Mathf.Min(needed, currentReserve);

        currentReserve -= toLoad;
        currentClip += toLoad;

        // Update UI
        onAmmoChanged?.Invoke(currentClip, currentReserve);

        isReloading = false;
        Debug.Log("Reload Klaar!");
    }


    public void RefillAmmo()
    {
        if (data == null) return;

        // Vul alles weer tot het maximum
        currentClip = data.magazineSize;
        currentReserve = data.maxAmmo;

        // Stop herladen als we dat aan het doen waren (instant fill)
        isReloading = false;
        StopAllCoroutines();

        if (onAmmoChanged != null) onAmmoChanged(currentClip, currentReserve);

        Debug.Log("Ammo Refilled!");
    }
}