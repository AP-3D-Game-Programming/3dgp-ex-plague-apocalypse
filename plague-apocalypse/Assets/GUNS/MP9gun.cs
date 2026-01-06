// using UnityEngine;
// using System.Collections;

// public class MP9gun : MonoBehaviour
// {
//     [Header("Projectile Settings")]
//     public Transform muzzlePoint;
//     public GameObject projectilePrefab;
//     public float projectileForce = 300f; 

//     [Header("Effects")]
//     public ParticleSystem muzzleFlash;

//     private bool isReloading = false;
//     private Animator animator;
//     private float nextTimeToFire = 0f;
//     private WeaponController weaponController;

//     void Start()
//     {
//         animator = GetComponent<Animator>();
//         weaponController = GetComponent<WeaponController>();

//         if (weaponController == null)
//         {
//             Debug.LogError("MP9gun: WeaponController component niet gevonden op dit object!");
//         }
//     }

//     void Update()
//     {
//         if (weaponController == null || isReloading)
//             return;

//         // Herladen
//         if (Input.GetKeyDown(KeyCode.R))
//         {
//             TryReload();
//         }

//         // Volautomatisch schieten (GetButton houdt de muisknop ingedrukt)
//         if (Input.GetButton("Fire1"))
//         {
//             if (Time.time >= nextTimeToFire && weaponController.HasAmmo())
//             {
//                 Shoot();
//                 // MP9 vuursnelheid is erg hoog
//                 nextTimeToFire = Time.time + (1f / weaponController.GetFireRate());
//             }
//         }
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
//                 // Gebruik linearVelocity voor consistente snelheid zonder bouncen
//                 rb.linearVelocity = shootDir * projectileForce;
//             }
//         }

//         weaponController.ConsumeAmmo();

//         if (muzzleFlash != null) muzzleFlash.Play();

//         if (animator != null)
//         {
//             animator.Play("MP9_Shoot", 0, 0f);
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
//         Debug.Log("MP9: Herladen...");

//         yield return new WaitForSeconds(weaponController.GetReloadTime());

//         weaponController.RefillClip();
//         isReloading = false;
//     }
// }