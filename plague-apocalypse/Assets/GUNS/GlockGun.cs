using UnityEngine;

public class GlockGun : MonoBehaviour
{
    [Header("Projectile")]
    public Transform muzzlePoint;
    public GameObject projectilePrefab;
    public float projectileForce = 200f; // Pistoolkogels zijn vaak iets trager/lichter

    [Header("Effects")]
    public ParticleSystem muzzleFlash;

    bool isReloading = false;
    Animator animator;
    float nextTimeToFire = 0f; 
    WeaponController weaponController;

    void Start()
    {
        animator = GetComponent<Animator>();
        weaponController = GetComponent<WeaponController>();
        
        if (weaponController == null)
        {
            Debug.LogError("GlockGun: WeaponController component niet gevonden!");
        }
    }

    void Update()
    {
        if (weaponController == null || isReloading)
            return;

        if (Input.GetKeyDown(KeyCode.R))
        {
            TryReload();
        }

        // SEMI-AUTOMATISCH: We gebruiken GetButtonDown in plaats van GetButton
        // Hierdoor moet de speler voor elk schot opnieuw klikken.
        if (Input.GetButtonDown("Fire1")) 
        {
            if (Time.time >= nextTimeToFire && weaponController.HasAmmo())
            {
                Shoot();
                nextTimeToFire = Time.time + (1f / weaponController.GetFireRate());
            }
            else if (!weaponController.HasAmmo())
            {
                Debug.Log("Glock: Klik... herlaad met R");
            }
        }
    }

    void Shoot()
    {
        if (projectilePrefab != null && muzzlePoint != null)
        {
            Vector3 shootDir = muzzlePoint.forward;
            GameObject projObj = Instantiate(projectilePrefab, muzzlePoint.position, Quaternion.LookRotation(shootDir));

            Projectile proj = projObj.GetComponent<Projectile>();
            if (proj != null)
            {
                proj.Initialize(
                    weaponController.GetDamage(),
                    weaponController.weaponInstance.data.weaponType,
                    weaponController.GetEffects()
                );
            }

            Rigidbody rb = projObj.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.interpolation = RigidbodyInterpolation.Interpolate;
                rb.useGravity = false;
                rb.AddForce(shootDir * projectileForce, ForceMode.VelocityChange);
            }
        }

        weaponController.ConsumeAmmo();

        if (muzzleFlash != null)
        {
            muzzleFlash.Play();
        }

        if (animator != null)
        {
            animator.Play("Glock_Shoot", 0, 0f); 
        }
    }

    void TryReload()
    {
        if (weaponController.CanReload())
        {
            StartCoroutine(ReloadCoroutine());
        }
    }

    System.Collections.IEnumerator ReloadCoroutine()
    {
        isReloading = true;
        Debug.Log("Glock: Herladen...");

        yield return new WaitForSeconds(weaponController.GetReloadTime());

        weaponController.RefillClip();
        isReloading = false;
    }
}