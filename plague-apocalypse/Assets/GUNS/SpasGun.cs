// using UnityEngine;

// public class SpasGun : MonoBehaviour
// {
//     [Header("Projectile Settings")]
//     public Transform muzzlePoint;
//     public GameObject projectilePrefab;
//     public float projectileForce = 400f;

//     [Header("Shotgun Settings")]
//     public int pelletsPerShot = 8;
//     public float spreadAngle = 5f;
//     [Range(-10f, 10f)]
//     public float horizontalOffset = -2f; // PAS DIT AAN: Negatief is meer naar links!

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
//             Debug.LogError("SpasGun: WeaponController niet gevonden!");
//         }
//     }

//     void Update()
//     {
//         if (weaponController == null || isReloading)
//             return;

//         if (Input.GetKeyDown(KeyCode.R))
//         {
//             TryReload();
//         }

//         if (Input.GetButtonDown("Fire1")) 
//         {
//             if (Time.time >= nextTimeToFire && weaponController.HasAmmo())
//             {
//                 Shoot();
//                 nextTimeToFire = Time.time + (1f / weaponController.GetFireRate());
//             }
//         }
//     }

//     void Shoot()
//     {
//         if (!weaponController.HasAmmo()) return;

//         for (int i = 0; i < pelletsPerShot; i++)
//         {
//             SpawnPellet();
//         }

//         weaponController.ConsumeAmmo();

//         if (muzzleFlash != null) muzzleFlash.Play();

//         if (animator != null)
//         {
//             animator.Play("Spas_Shoot", 0, 0f);
//         }
//     }

//     void SpawnPellet()
//     {
//         if (projectilePrefab != null && muzzlePoint != null)
//         {
//             // Bereken de spreiding EN voeg de horizontale offset toe op de Y-as
//             float randomX = Random.Range(-spreadAngle, spreadAngle);
//             float randomY = Random.Range(-spreadAngle, spreadAngle) + horizontalOffset;

//             Quaternion spreadRotation = Quaternion.Euler(randomX, randomY, 0);

//             Vector3 shootDir = spreadRotation * muzzlePoint.forward;

//             GameObject pellet = Instantiate(projectilePrefab, muzzlePoint.position, Quaternion.LookRotation(shootDir));

//             Projectile proj = pellet.GetComponent<Projectile>();
//             if (proj != null)
//             {
//                 proj.Initialize(
//                     weaponController.GetDamage(),
//                     weaponController.weaponInstance.data.weaponType,
//                     weaponController.GetEffects()
//                 );
//             }

//             Rigidbody rb = pellet.GetComponent<Rigidbody>();
//             if (rb != null)
//             {
//                 rb.AddForce(shootDir * projectileForce, ForceMode.VelocityChange);
//             }
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
//         if (animator != null) animator.SetTrigger("Reload");

//         yield return new WaitForSeconds(weaponController.GetReloadTime());

//         weaponController.RefillClip();
//         isReloading = false;
//     }
// }