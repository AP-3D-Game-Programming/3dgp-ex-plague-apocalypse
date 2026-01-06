using UnityEngine;
using System.Collections;

public class GalilGun : MonoBehaviour
{
    [Header("Projectile Settings")]
    public Transform muzzlePoint;
    public GameObject projectilePrefab;
    public float projectileForce = 350f; // Galil kogels hebben vaak veel kracht

    [Header("Effects")]
    public ParticleSystem muzzleFlash;

    private bool isReloading = false;
    private Animator animator;
    private float nextTimeToFire = 0f;
    private WeaponController weaponController;

    void Start()
    {
        animator = GetComponent<Animator>();
        weaponController = GetComponent<WeaponController>();
        
        if (weaponController == null)
        {
            Debug.LogError("GalilGun: WeaponController component niet gevonden!");
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

        // Volautomatisch schieten (GetButton houdt de muisknop ingedrukt)
        if (Input.GetButton("Fire1"))
        {
            if (Time.time >= nextTimeToFire && weaponController.HasAmmo())
            {
                Shoot();
                // Gebruik de Fire Rate uit je WeaponData
                nextTimeToFire = Time.time + (1f / weaponController.GetFireRate());
            }
        }
    }

    void Shoot()
    {
        if (projectilePrefab != null && muzzlePoint != null)
        {
            Vector3 shootDir = muzzlePoint.forward;
            GameObject projObj = Instantiate(projectilePrefab, muzzlePoint.position, Quaternion.LookRotation(shootDir));

            // Initialiseer de kogel met data uit de WeaponController
            Projectile proj = projObj.GetComponent<Projectile>();
            if (proj != null)
            {
                proj.Initialize(
                    weaponController.GetDamage(),
                    weaponController.weaponInstance.data.weaponType,
                    weaponController.GetEffects()
                );
            }

            // Physics instellingen voor de kogel
            Rigidbody rb = projObj.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.linearVelocity = shootDir * projectileForce;
            }
        }

        // Munitie verbruiken
        weaponController.ConsumeAmmo();

        // Effecten
        if (muzzleFlash != null) muzzleFlash.Play();

        if (animator != null)
        {
            // Zorg dat je een animatie hebt genaamd "Galil_Shoot"
            animator.Play("Galil_Shoot", 0, 0f);
        }
    }

    void TryReload()
    {
        if (weaponController.CanReload())
        {
            StartCoroutine(ReloadCoroutine());
        }
    }

    IEnumerator ReloadCoroutine()
    {
        isReloading = true;
        Debug.Log("Galil: Herladen...");

        // Wacht op de reloadTime uit je WeaponData
        yield return new WaitForSeconds(weaponController.GetReloadTime());

        weaponController.RefillClip();
        isReloading = false;
    }
}