// using UnityEngine;

// public class dubbel_barrel : MonoBehaviour
// {
//     [Header("Bullet")]
//     public Transform muzzlePoint1;       // Waar de kogels uitkomen
//     public GameObject bulletPrefab;      // Je bullet prefab
//     public float bulletForce = 800f;     // Snelheid van de kogels

//     [Header("Effects")]
//     public ParticleSystem muzzleFlash;   // Wijs hier je "muzzle_flash" ParticleSystem toe

//     [Header("Ammo Settings")]
//     public int magazineSize = 10;         // Capaciteit van het shotgun (10 kogels)
//     public int reserveAmmo = 40;         // Reserve kogels
//     public float reloadTime = 5f;        // Tijd om te herladen (5 seconden)
//     public float fireRate = 0.5f;        // Tijd tussen schoten
//     public int bulletsPerShot = 10;      // 10 kogels per schot
//     public float spreadRadius = 3f;      // Spreiding radius in graden (klein = dicht bij elkaar)

//     [HideInInspector]
//     public int currentAmmo;              // Kogels momenteel in magazijn

//     bool isReloading = false;
//     Animator animator;
//     float shotFired = 0f;

//     void Start()
//     {
//         // Begin met een vol magazijn
//         currentAmmo = magazineSize;
//         animator = GetComponent<Animator>();
//         Debug.Log(animator);

//         // Zorg ervoor dat muzzle_flash uitgeschakeld is aan het begin
//         if (muzzleFlash != null)
//         {
//             muzzleFlash.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
//             muzzleFlash.gameObject.SetActive(false);
//             Debug.Log("Muzzle flash disabled at start");
//         }
//     }

//     void Update()
//     {
//         if (isReloading)
//             return;

//         // R om te herladen (indien niet vol en er is reserve)
//         if (Input.GetKeyDown(KeyCode.R))
//         {
//             TryReload();
//         }

//         // Hou Linkermuisknop ingedrukt voor schoten
//         if (Input.GetButton("Fire1"))
//         {
//             // Zet animatie aan zolang we fire houden en er ammo is
//             if (animator != null)
//                 animator.SetBool("isFiring", currentAmmo > 0);

//             // Schiet volgens fireRate
//             if (Time.time - shotFired >= fireRate)
//             {
//                 Shoot();
//                 shotFired = Time.time;
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
//         // Geen ammo in magazijn
//         if (currentAmmo <= 0)
//         {
//             Debug.Log("Klik... Geen kogels in magazijn! Reload met R.");
//             return;
//         }

//         Debug.Log($"Shooting {bulletsPerShot} bullets!");

//         // Spawn 10 bullets uit muzzlePoint1 met random spreiding
//         for (int i = 0; i < bulletsPerShot; i++)
//         {
//             if (bulletPrefab != null && muzzlePoint1 != null)
//             {
//                 // bepaal richting met random spreiding in een cone
//                 Vector3 shootDir = muzzlePoint1.forward;

//                 // Random spreiding in cone shape (met bias naar links)
//                 float randomX = Random.Range(-spreadRadius, spreadRadius);
//                 float randomY = Random.Range(-spreadRadius * 3f, spreadRadius * 0.5f);  // Veel meer naar links

//                 shootDir = Quaternion.Euler(randomX, randomY, 0) * shootDir;

//                 GameObject bullet = Instantiate(bulletPrefab, muzzlePoint1.position, Quaternion.LookRotation(shootDir));

//                 // Zorg dat de bullet meteen een vaste wereld-snelheid krijgt
//                 Rigidbody rb = bullet.GetComponent<Rigidbody>();
//                 if (rb != null)
//                 {
//                     rb.interpolation = RigidbodyInterpolation.Interpolate; // smoother movement
//                     rb.AddForce(shootDir * bulletForce, ForceMode.VelocityChange);
//                 }


//                 Destroy(bullet, 5f); // verwijder na 5 seconden
//             }
//             else
//             {
//                 Debug.LogWarning($"bulletPrefab is null: {bulletPrefab == null}, muzzlePoint1 is null: {muzzlePoint1 == null}");
//             }
//         }

//         // Play muzzle flash at muzzlePoint1
//         if (muzzleFlash != null && muzzlePoint1 != null)
//         {
//             if (muzzleFlash.gameObject.scene.IsValid())
//             {
//                 // Enable en Setup de particle system
//                 muzzleFlash.gameObject.SetActive(true);
//                 muzzleFlash.transform.SetParent(muzzlePoint1, false);
//                 muzzleFlash.transform.localPosition = Vector3.zero;
//                 muzzleFlash.transform.localRotation = Quaternion.identity;

//                 // Clear en play
//                 muzzleFlash.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
//                 muzzleFlash.Play();

//                 Debug.Log("Muzzle flash played");

//                 // Disable het na de duration
//                 var main = muzzleFlash.main;
//                 StartCoroutine(DisableMuzzleFlashAfter(main.duration + 0.5f));
//             }
//             else
//             {
//                 ParticleSystem spawned = Instantiate(muzzleFlash, muzzlePoint1);
//                 spawned.gameObject.SetActive(true);
//                 spawned.transform.localPosition = Vector3.zero;
//                 spawned.transform.localRotation = Quaternion.identity;
//                 spawned.Play();

//                 // Destroy the instantiated particle after its lifetime (duration + startLifetime)
//                 var main = spawned.main;
//                 float startLifetime = 0f;
//                 // handle different startLifetime modes
//                 if (main.startLifetime.mode == ParticleSystemCurveMode.TwoConstants)
//                     startLifetime = main.startLifetime.constantMax;
//                 else
//                     startLifetime = main.startLifetime.constant;

//                 float lifetime = main.duration + startLifetime;
//                 Destroy(spawned.gameObject, lifetime);
//             }
//         }

//         // Speel kickback animatie (naam van je animatieclip: "DubbelBarrel").
//         if (animator != null)
//         {
//             animator.Play("DubbelBarrel", 0, 0f);
//         }

//         // 1 schot kwijt (= 10 kogels)
//         currentAmmo--;

//         Debug.Log($"Ammo: {currentAmmo} / {reserveAmmo}");
//         shotFired = Time.time;
//     }

//     void TryReload()
//     {
//         // Als magazijn al vol is → niets doen
//         if (currentAmmo >= magazineSize)
//         {
//             Debug.Log("Magazijn is al vol.");
//             return;
//         }

//         // Geen reserve kogels
//         if (reserveAmmo <= 0)
//         {
//             Debug.Log("Geen reserve ammo!");
//             return;
//         }

//         // Start reload
//         StartCoroutine(ReloadCoroutine());
//     }

//     System.Collections.IEnumerator ReloadCoroutine()
//     {
//         isReloading = true;
//         Debug.Log("Reloading...");

//         yield return new WaitForSeconds(reloadTime);

//         // Hoeveel kogels hebben we nodig om magazijn te vullen?
//         int needed = magazineSize - currentAmmo;

//         // Hoeveel kunnen we effectief bijvullen?
//         int toLoad = Mathf.Min(needed, reserveAmmo);

//         currentAmmo += toLoad;
//         reserveAmmo -= toLoad;

//         Debug.Log($"Reload klaar: {currentAmmo} / {reserveAmmo}");

//         isReloading = false;
//     }

//     System.Collections.IEnumerator DisableMuzzleFlashAfter(float delay)
//     {
//         yield return new WaitForSeconds(delay);

//         if (muzzleFlash != null)
//         {
//             muzzleFlash.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
//             muzzleFlash.gameObject.SetActive(false);
//             Debug.Log("Muzzle flash disabled after duration");
//         }
//     }
// }
