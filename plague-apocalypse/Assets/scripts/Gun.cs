using UnityEngine;
using System.Collections.Generic;

public class Gun : MonoBehaviour
{
    [Header("Setup")]
    public Transform muzzlePoint;
    private WeaponData currentWeaponData;
    private float nextFireTime = 0f;
    private PlayerEffectManager effectManager;

    public void Initialize(WeaponData data)
    {
        currentWeaponData = data;
        effectManager = GetComponentInParent<PlayerEffectManager>();
    }
    public void AttemptShoot()
    {
        if (currentWeaponData == null) return;

        if (Time.time >= nextFireTime)
        {
            nextFireTime = Time.time + currentWeaponData.fireRate;
            if (currentWeaponData.projectilePrefab != null && muzzlePoint != null)
            {
                Shoot();
            }
        }
    }

    protected virtual void Shoot()
    {
        GameObject bullet = Instantiate(currentWeaponData.projectilePrefab, muzzlePoint.position, muzzlePoint.rotation);
        Projectile bulletScript = bullet.GetComponent<Projectile>();

        if (bulletScript != null)
        {
            bulletScript.Initialize(
                currentWeaponData.damage,
                currentWeaponData.weaponType,
                currentWeaponData.effects
            );
        }
    }
}