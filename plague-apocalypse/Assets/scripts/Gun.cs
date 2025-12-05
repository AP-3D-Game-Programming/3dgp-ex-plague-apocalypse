using UnityEngine;

public class Gun : MonoBehaviour
{
    [Header("Setup")]
    public Transform muzzlePoint; // HIER komt de kogel uit (leeg puntje voor de loop)
    
    private WeaponData data;
    private float nextFireTime;

    // Deze functie wordt aangeroepen zodra het wapen spawnt
    public void Initialize(WeaponData weaponData)
    {
        this.data = weaponData;
    }

    public void AttemptShoot()
    {
        if (Time.time >= nextFireTime)
        {
            nextFireTime = Time.time + data.fireRate;
            Shoot();
        }
    }

    // VIRTUAL: Zodat je bv. een Shotgun script kunt maken dat 5 kogels tegelijk doet
    protected virtual void Shoot()
    {
        if (data.projectilePrefab == null || muzzlePoint == null) return;

        // 1. Maak de kogel
        GameObject bullet = Instantiate(data.projectilePrefab, muzzlePoint.position, muzzlePoint.rotation);
        
        // Optioneel: Geef de bullet extra data mee als dat nodig is
        // bullet.GetComponent<Projectile>().damage = data.damage;
    }
}