using UnityEngine;

public class MP5Gun : MonoBehaviour
{
    [Header("Bullet")]
    public Transform muzzlePoint;        // Waar de kogel uitkomt (voor op de loop)
    public GameObject bulletPrefab;      // Je bullet prefab
    public float bulletForce = 800f;     // Snelheid van de kogel

    [Header("Effects")]
    public ParticleSystem muzzleFlash;   // Wijs hier je "muzzle_flash" ParticleSystem toe

    [Header("Ammo Settings")]
    public int magazineSize = 20;         // Grootte van magazijn (MP5)
    public int reserveAmmo = 60;         // Reserve kogels (MP5)
    public float reloadTime = 2.5f;      // Tijd om te herladen (MP5)
    public float fireRate = 0.1f;        // Tijd tussen schoten (houd M1 ingedrukt voor automatisch vuur)

    [HideInInspector]
    public int currentAmmo;              // Kogels momenteel in magazijn

    bool isReloading = false;
    Animator animator;
    float shotFired = 0f;

    void Start()
    {
        // Begin met een vol magazijn
        currentAmmo = magazineSize;
        animator = GetComponent<Animator>();
        Debug.Log(animator);
    }

    void Update()
    {
        if (isReloading)
            return;

        // R om te herladen (indien niet vol en er is reserve)
        if (Input.GetKeyDown(KeyCode.R))
        {
            TryReload();
        }

        // Hou Linkermuisknop ingedrukt voor automatisch vuur (SMG)
        if (Input.GetButton("Fire1"))
        {
            // Zet animatie aan zolang we fire houden en er ammo is
            if (animator != null)
                animator.SetBool("isFiring", currentAmmo > 0);

            // Schiet volgens fireRate
            if (Time.time - shotFired >= fireRate)
            {
                Shoot();
                shotFired = Time.time;
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
        // Geen ammo in magazijn
        if (currentAmmo <= 0)
        {
            Debug.Log("Klik... Geen kogels in magazijn! Reload met R.");
            return;
        }



        // Spawn bullet
        if (bulletPrefab != null && muzzlePoint != null)
        {
            // bepaal richting vóór animatie-aanroep zodat we altijd dezelfde richting gebruiken
            Vector3 shootDir = muzzlePoint.forward;

            GameObject bullet = Instantiate(bulletPrefab, muzzlePoint.position, Quaternion.LookRotation(shootDir));

            // Zorg dat de bullet meteen een vaste wereld-snelheid krijgt (onafhankelijk van latere beweging van de wapen-animatie)
            Rigidbody rb = bullet.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.interpolation = RigidbodyInterpolation.Interpolate; // smoother movement
                rb.AddForce(shootDir * bulletForce, ForceMode.VelocityChange);
            }
            else
            {
                Debug.LogWarning("MP5: Bullet prefab has no Rigidbody — it won't move.");
            }

            Destroy(bullet, 5f); // verwijder na 5 seconden
        }

        // Play muzzle flash at the muzzlePoint.
        // If `muzzleFlash` is a scene object, parent it to the muzzlePoint and play.
        // If it's a prefab/asset, instantiate it as a child of the muzzlePoint with local position/rotation zero.
        if (muzzleFlash != null && muzzlePoint != null)
        {
            if (muzzleFlash.gameObject.scene.IsValid())
            {
                // Parent the scene ParticleSystem to the muzzlePoint so it aligns properly.
                muzzleFlash.transform.SetParent(muzzlePoint, false);
                muzzleFlash.transform.localPosition = Vector3.zero;
                muzzleFlash.transform.localRotation = Quaternion.identity;
                muzzleFlash.Play();
            }
            else
            {
                ParticleSystem spawned = Instantiate(muzzleFlash, muzzlePoint);
                spawned.transform.localPosition = Vector3.zero;
                spawned.transform.localRotation = Quaternion.identity;
                spawned.Play();

                // Destroy the instantiated particle after its lifetime (duration + startLifetime)
                var main = spawned.main;
                float startLifetime = 0f;
                // handle different startLifetime modes
                if (main.startLifetime.mode == ParticleSystemCurveMode.TwoConstants)
                    startLifetime = main.startLifetime.constantMax;
                else
                    startLifetime = main.startLifetime.constant;

                float lifetime = main.duration + startLifetime;
                Destroy(spawned.gameObject, lifetime);
            }
        }

        // Speel kickback animatie (naam van je animatieclip: "1911").
        // Dit roept de animator state direct aan; als die state bestaat zal de kickback afgespeeld worden.
        if (animator != null)
        {
            animator.Play("MP5", 0, 0f);
        }
        
        // 1 kogel kwijt
        currentAmmo--;

        Debug.Log($"Ammo: {currentAmmo} / {reserveAmmo}");
        shotFired = Time.time;
    }

    void TryReload()
    {
        // Als magazijn al vol is → niets doen
        if (currentAmmo >= magazineSize)
        {
            Debug.Log("Magazijn is al vol.");
            return;
        }

        // Geen reserve kogels
        if (reserveAmmo <= 0)
        {
            Debug.Log("Geen reserve ammo!");
            return;
        }

        // Start reload
        StartCoroutine(ReloadCoroutine());
    }

    System.Collections.IEnumerator ReloadCoroutine()
    {
        isReloading = true;
        Debug.Log("Reloading...");

        yield return new WaitForSeconds(reloadTime);

        // Hoeveel kogels hebben we nodig om magazijn te vullen?
        int needed = magazineSize - currentAmmo;

        // Hoeveel kunnen we effectief bijvullen?
        int toLoad = Mathf.Min(needed, reserveAmmo);

        currentAmmo += toLoad;
        reserveAmmo -= toLoad;

        Debug.Log($"Reload klaar: {currentAmmo} / {reserveAmmo}");

        isReloading = false;
    }
}
