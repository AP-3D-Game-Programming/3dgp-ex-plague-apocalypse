// using UnityEngine;
// using System.Collections.Generic;
// using System.Collections;

// public class FamasGun : MonoBehaviour
// {
//     [Header("Projectile Settings")]
//     public Transform muzzlePoint;
//     public GameObject projectilePrefab;
//     public float projectileForce = 300f;

//     [Header("Burst Settings")]
//     public int shotsPerBurst = 3;       // 3 kogels per klik
//     public float timeBetweenBursts = 0.1f; // Tijd tussen de 3 kogels in de burst

//     [Header("Effects")]
//     public ParticleSystem muzzleFlash;

//     private bool isReloading = false;
//     private bool isShootingBurst = false;
//     private Animator animator;
//     private float nextTimeToFire = 0f;
//     private WeaponController weaponController;

//     void Start()
//     {
//         animator = GetComponent<Animator>();
//         weaponController = GetComponent<WeaponController>();

//         if (weaponController == null)
//         {
//             Debug.LogError("FamasGun: WeaponController niet gevonden!");
//         }
//     }

//     void Update()
//     {
//         if (weaponController == null || isReloading || isShootingBurst)
//             return;

//         if (Input.GetKeyDown(KeyCode.R))
//         {
//             TryReload();
//         }

//         // Burst fire werkt het lekkerst met GetButtonDown
//         if (Input.GetButtonDown("Fire1")) 
//         {
//             if (Time.time >= nextTimeToFire && weaponController.HasAmmo())
//             {
//                 StartCoroutine(ShootBurst());
//                 // De fireRate uit de WeaponData bepaalt de tijd tussen de bursts
//                 nextTimeToFire = Time.time + (1f / weaponController.GetFireRate());
//             }
//         }
//     }

//     IEnumerator ShootBurst()
//     {
//         isShootingBurst = true;

//         for (int i = 0; i < shotsPerBurst; i++)
//         {
//             // Check voor elk schot in de burst of er nog ammo is
//             if (weaponController.HasAmmo())
//             {
//                 Shoot();
//                 yield return new WaitForSeconds(timeBetweenBursts);
//             }
//             else
//             {
//                 break; // Stop de burst als het magazijn leeg is
//             }
//         }

//         isShootingBurst = false;
//     }

//     void Shoot()
//     {
//         if (projectilePrefab != null && muzzlePoint != null)
//         {
//             Vector3 shootDir = muzzlePoint.forward;
//             GameObject projObj = Instantiate(projectilePrefab, muzzlePoint.position, Quaternion.LookRotation(shootDir));

//             Projectile proj = projObj.GetComponent<Projectile>();
//             if (proj != null)
//             {
//                 proj.Initialize(
//                     weaponController.GetDamage(),
//                     weaponController.weaponInstance.data.weaponType,
//                     weaponController.GetEffects()
//                 );
//             }

//             Rigidbody rb = projObj.GetComponent<Rigidbody>();
//             if (rb != null)
//             {
//                 rb.linearVelocity = shootDir * projectileForce;
//             }
//         }

//         weaponController.ConsumeAmmo();

//         if (muzzleFlash != null) muzzleFlash.Play();

//         if (animator != null)
//         {
//             animator.Play("Famas_Shoot", 0, 0f);
//         }
//     }

//     void TryReload()
//     {
//         if (weaponController.CanReload())
//         {
//             StartCoroutine(ReloadCoroutine());
//         }
//     }

//     IEnumerator ReloadCoroutine()
//     {
//         isReloading = true;
//         Debug.Log("FAMAS: Herladen...");

//         yield return new WaitForSeconds(weaponController.GetReloadTime());

//         weaponController.RefillClip();
//         isReloading = false;
//     }
// }