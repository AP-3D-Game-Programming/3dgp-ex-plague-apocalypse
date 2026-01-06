using UnityEngine;

public class M4Gun : MonoBehaviour
{
    [Header("Projectile")]
    public Transform muzzlePoint;
    public GameObject projectilePrefab;
    public float projectileForce = 300f;

    [Header("Effects")]
    public ParticleSystem muzzleFlash;

    bool isReloading = false;
    Animator animator;
    float nextTimeToFire = 0f; // Verbeterde timing variabele
    WeaponController weaponController;

    void Start()
    {
        animator = GetComponent<Animator>();
        weaponController = GetComponent<WeaponController>();
        
        if (weaponController == null)
        {
            Debug.LogError("M4Gun: WeaponController component niet gevonden op dit GameObject! Voeg het toe.");
        }
    }

    void Update()
    {
        if (weaponController == null || isReloading)
            return;

        // Herladen met R
        if (Input.GetKeyDown(KeyCode.R))
        {
            TryReload();
        }

        // Schieten
        if (Input.GetButton("Fire1"))
        {
            if (animator != null)
                animator.SetBool("isFiring", weaponController.HasAmmo());

            // VERBETERING: 1f gedeeld door de fire rate zorgt voor de juiste snelheid
            if (Time.time >= nextTimeToFire && weaponController.HasAmmo())
            {
                Shoot();
                // Bereken wanneer het volgende schot mag plaatsvinden
                nextTimeToFire = Time.time + (1f / weaponController.GetFireRate());
            }
        }
        else
        {
            if (animator != null)
                animator.SetBool("isFiring", false);
        }
    }

    void Shoot()
    {
        if (!weaponController.HasAmmo())
        {
            Debug.Log("M4: Klik... Geen kogels in magazijn! Reload met R.");
            return;
        }

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
            else
            {
                Debug.LogWarning("M4: Projectile prefab heeft geen Projectile script!");
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

        // Muzzle Flash afspelen
        if (muzzleFlash != null && muzzlePoint != null)
        {
            // We spelen de flash simpelweg af op de huidige plek
            muzzleFlash.Play();
        }

        if (animator != null)
        {
            animator.Play("M4", 0, 0f);
        }
    }

    void TryReload()
    {
        if (!weaponController.CanReload())
        {
            Debug.Log("M4: Kan niet herladen.");
            return;
        }

        StartCoroutine(ReloadCoroutine());
    }

    System.Collections.IEnumerator ReloadCoroutine()
    {
        isReloading = true;
        Debug.Log("M4: Reloading...");

        // De reload tijd komt nu dynamisch uit de weaponController
        yield return new WaitForSeconds(weaponController.GetReloadTime());

        weaponController.RefillClip();

        Debug.Log("M4: Reload klaar!");
        isReloading = false;
    }
}