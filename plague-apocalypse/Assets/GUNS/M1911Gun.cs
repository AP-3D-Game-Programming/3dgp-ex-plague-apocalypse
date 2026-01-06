// using UnityEngine;

// [RequireComponent(typeof(WeaponController))]
// public class M1911Gun : MonoBehaviour
// {
//     [Header("Visuals")]
//     public Transform muzzlePoint;
//     public ParticleSystem muzzleFlash;

//     private WeaponController controller;
//     bool isReloading = false;
//     Animator animator;
//     float shotFired = 0f;

//     void Start()
//     {
//         controller = GetComponent<WeaponController>();
//         animator = GetComponent<Animator>();
//     }

//     void Update()
//     {
//         if (isReloading) return;

//         if (Input.GetKeyDown(KeyCode.R))
//         {
//             TryReload();
//         }

//         if (Input.GetButtonDown("Fire1"))
//         {
//             animator.SetTrigger("shots_fired");
//             Shoot();
//             if (Time.time - shotFired > 0.05f) animator.ResetTrigger("shots_fired");
//         }
//     }

//     void Shoot()
//     {
//         if (!controller.HasAmmo()) return;

//         GameObject prefab = controller.weaponInstance.data.projectilePrefab;
//         if (prefab != null && muzzlePoint != null)
//         {
//             GameObject bullet = Instantiate(prefab, muzzlePoint.position, muzzlePoint.rotation);
//             Projectile p = bullet.GetComponent<Projectile>();
//             if (p != null)
//             {
//                 p.Initialize(controller.GetDamage(), controller.weaponInstance.data.weaponType, controller.GetEffects());
//             }
//         }

//         if (muzzleFlash != null)
//         {
//             if (muzzleFlash.gameObject.scene.IsValid()) muzzleFlash.Play();
//             else { Instantiate(muzzleFlash, muzzlePoint).Play(); }
//         }

//         controller.ConsumeAmmo();
//         shotFired = Time.time;
//     }

//     void TryReload()
//     {
//         if (controller.CanReload()) StartCoroutine(ReloadCoroutine());
//     }

//     System.Collections.IEnumerator ReloadCoroutine()
//     {
//         isReloading = true;
//         yield return new WaitForSeconds(controller.GetReloadTime());
//         controller.RefillClip();
//         isReloading = false;
//     }
// }