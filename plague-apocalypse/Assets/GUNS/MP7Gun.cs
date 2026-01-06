// using UnityEngine;

// public class MP7Gun : MonoBehaviour
// {
//     [Header("Projectile")]
//     public Transform muzzlePoint;
//     public GameObject projectilePrefab;
//     public float projectileForce = 250f; // MP7 heeft vaak iets minder kracht per kogel dan M4

//     [Header("Effects")]
//     public ParticleSystem muzzleFlash;

//     bool isReloading = false;
//     Animator animator;
//     float nextTimeToFire = 0f; 
//     WeaponController weaponController;

//     void Start()
//     {
//         animator = GetComponent<Animator>();
//         weaponController = GetComponent<WeaponController>();

//         if (weaponController == null)
//         {
//             Debug.LogError("MP7Gun: WeaponController component niet gevonden op dit GameObject!");
//         }
//     }

//     void Update()
//     {
//         if (weaponController == null || isReloading)
//             return;

//         // Herladen met R
//         if (Input.GetKeyDown(KeyCode.R))
//         {
//             TryReload();
//         }

//         // Schieten (MP7 is meestal volledig automatisch)
//         if (Input.GetButton("Fire1"))
//         {
//             if (animator != null)
//                 animator.SetBool("isFiring", weaponController.HasAmmo());

//             // Gebruik de verbeterde Fire Rate berekening
//             if (Time.time >= nextTimeToFire && weaponController.HasAmmo())
//             {
//                 Shoot();
//                 nextTimeToFire = Time.time + (1f / weaponController.GetFireRate());
//             }
//         }
//         else
//         {
//             if (animator != null)
//                 animator.SetBool("isFiring", false);
//         }
//     }

//     void Shoot()
//     {
//         if (!weaponController.HasAmmo())
//         {
//             Debug.Log("MP7: Leeg! Herlaad.");
//             return;
//         }

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
//                 rb.interpolation = RigidbodyInterpolation.Interpolate;
//                 rb.useGravity = false;
//                 rb.AddForce(shootDir * projectileForce, ForceMode.VelocityChange);
//             }
//         }

//         weaponController.ConsumeAmmo();

//         // Muzzle Flash
//         if (muzzleFlash != null)
//         {
//             muzzleFlash.Play();
//         }

//         // Speel MP7 specifieke animatie af
//         if (animator != null)
//         {
//             animator.Play("MP7_Shoot", 0, 0f); 
//         }
//     }

//     void TryReload()
//     {
//         if (weaponController.CanReload())
//         {
//             StartCoroutine(ReloadCoroutine());
//         }
//     }

//     System.Collections.IEnumerator ReloadCoroutine()
//     {
//         isReloading = true;
//         Debug.Log("MP7: Herladen...");

//         yield return new WaitForSeconds(weaponController.GetReloadTime());

//         weaponController.RefillClip();
//         isReloading = false;
//     }
// }